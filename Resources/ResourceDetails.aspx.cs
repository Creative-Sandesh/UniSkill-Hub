using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace UniSkillHub.Resources
{
    public partial class ResourceDetails : Page
    {
        // Used by the picture in the side card (see the .aspx).
        protected string CategoryImage { get; private set; }
        protected string CategoryLabel { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            int resourceId;
            if (!int.TryParse(Request.QueryString["id"], out resourceId))
            {
                ShowNotFound();
                return;
            }

            // Only published resources are visible - except that an Admin can preview drafts.
            bool isAdmin = User.IsInRole("Admin");
            DataTable dt = DBHelper.GetDataTable(
                "SELECT r.ResourceID, r.Title, r.Description, r.ResourceType, r.FilePath, r.VideoPath, " +
                "       r.AudioPath, r.ExternalURL, r.DateCreated, r.Status, c.CategoryName, u.FullName AS Author " +
                "FROM Resources r " +
                "INNER JOIN Categories c ON c.CategoryID = r.CategoryID " +
                "INNER JOIN Users u ON u.UserID = r.CreatedBy " +
                "WHERE r.ResourceID = @ResourceID AND (r.Status = 'Published' OR @IsAdmin = 1)",
                DBHelper.Param("@ResourceID", resourceId),
                DBHelper.Param("@IsAdmin", isAdmin ? 1 : 0));

            if (dt.Rows.Count == 0)
            {
                ShowNotFound();
                return;
            }

            DataRow row = dt.Rows[0];
            string title = (string)row["Title"];
            string type = (string)row["ResourceType"];

            pnlDraftNote.Visible = ((string)row["Status"] != "Published");

            Page.Title = title;
            litCrumb.Text = Server.HtmlEncode(title);
            litTitle.Text = Server.HtmlEncode(title);
            litTypeBadge.Text = "<span class=\"type-badge type-" + type.ToLowerInvariant() + "\">" + Server.HtmlEncode(type) + "</span>";
            litCategory.Text = Server.HtmlEncode((string)row["CategoryName"]);
            CategoryLabel = (string)row["CategoryName"];
            CategoryImage = Utility.CategoryImageUrl(CategoryLabel);
            litDate.Text = ((DateTime)row["DateCreated"]).ToString("dd MMM yyyy");
            litAuthor.Text = Server.HtmlEncode((string)row["Author"]);

            // Encode first, then keep the author's line breaks.
            litDescription.Text = "<p>" + Server.HtmlEncode((string)row["Description"])
                .Replace("\r\n", "\n").Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";

            ShowMedia(row);
            ShowActions(row, resourceId);
        }

        // ---------- video / audio ----------

        private void ShowMedia(DataRow row)
        {
            string videoPath = row["VideoPath"] as string;
            string audioPath = row["AudioPath"] as string;

            // Only play files that really are inside the public media folders.
            if (IsInFolder(videoPath, "Uploads/Videos/"))
            {
                litMedia.Text =
                    "<figure class=\"media-figure\"><video controls preload=\"metadata\">" +
                    "<source src=\"" + MediaUrl(videoPath) + "\" type=\"" + VideoMimeType(videoPath) + "\" />" +
                    "Your browser does not support the video element.</video>" +
                    "<figcaption>Video lesson</figcaption></figure>";
            }
            else if (IsInFolder(audioPath, "Uploads/Audio/"))
            {
                litMedia.Text =
                    "<figure class=\"media-figure\"><audio controls preload=\"metadata\">" +
                    "<source src=\"" + MediaUrl(audioPath) + "\" type=\"" + AudioMimeType(audioPath) + "\" />" +
                    "Your browser does not support the audio element.</audio>" +
                    "<figcaption>Audio lesson</figcaption></figure>";
            }
        }

        // ---------- download / external link ----------

        private void ShowActions(DataRow row, int resourceId)
        {
            string filePath = row["FilePath"] as string;
            string externalUrl = row["ExternalURL"] as string;

            if (!string.IsNullOrEmpty(filePath))
            {
                if (Request.IsAuthenticated)
                {
                    pnlDownload.Visible = true;
                    litFileName.Text = Server.HtmlEncode(Path.GetFileName(filePath));
                    lnkDownload.HRef = "~/Resources/Download.ashx?id=" + resourceId;

                    // Optional AI summary: the button only exists when a Gemini key is configured.
                    pnlSummarize.Visible = GeminiHelper.IsConfigured && GeminiHelper.CanSummarize(filePath);
                }
                else
                {
                    pnlLoginToDownload.Visible = true;
                }
            }
            else if (IsSafeWebUrl(externalUrl))
            {
                pnlLink.Visible = true;
                lnkExternal.HRef = externalUrl;
            }
            else
            {
                pnlNothing.Visible = true;
            }
        }

        // ---------- optional AI summary ----------

        protected void btnSummarize_Click(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
                return;
            }

            // Page_Load does not run its checks on a postback, so everything is checked again here
            // and the file path comes from the database - never from the form.
            int resourceId;
            if (!int.TryParse(Request.QueryString["id"], out resourceId)) return;

            DataTable dt = DBHelper.GetDataTable(
                "SELECT FilePath FROM Resources " +
                "WHERE ResourceID = @ResourceID AND (Status = 'Published' OR @IsAdmin = 1) AND FilePath IS NOT NULL",
                DBHelper.Param("@ResourceID", resourceId),
                DBHelper.Param("@IsAdmin", User.IsInRole("Admin") ? 1 : 0));

            string filePath = dt.Rows.Count == 0 ? null : (string)dt.Rows[0]["FilePath"];
            string fullPath = FileHelper.ResolveInsideUploads(Context, filePath);

            if (fullPath == null || !GeminiHelper.IsConfigured || !GeminiHelper.CanSummarize(filePath))
            {
                ShowSummaryError("A summary is not available for this resource.");
                return;
            }

            try
            {
                string summary = GeminiHelper.GetSummary(Utility.CurrentUserId, fullPath);

                // The AI's text is untrusted: encode it first, then turn line breaks into HTML.
                litSummary.Text = "<p>" + Server.HtmlEncode(summary).Replace("**", "")
                    .Replace("\r\n", "\n").Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";
                pnlSummary.Visible = true;
            }
            catch (GeminiException ex)
            {
                ShowSummaryError(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error("ResourceDetails: could not summarize resource " + resourceId, ex);
                ShowSummaryError("Sorry, the summary could not be made right now. Please try again later.");
            }
        }

        private void ShowSummaryError(string message)
        {
            lblSummaryError.Text = Server.HtmlEncode(message);
            lblSummaryError.Visible = true;
        }

        // ---------- helpers ----------

        private void ShowNotFound()
        {
            pnlResource.Visible = false;
            pnlNotFound.Visible = true;
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
        }

        // Only http/https links are allowed, so a value such as "javascript:..." is never rendered as a link.
        private static bool IsSafeWebUrl(string url)
        {
            Uri uri;
            return Uri.TryCreate(url, UriKind.Absolute, out uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static bool IsInFolder(string path, string folder)
        {
            return !string.IsNullOrEmpty(path) &&
                   path.StartsWith(folder, StringComparison.OrdinalIgnoreCase) &&
                   path.IndexOf("..", StringComparison.Ordinal) < 0;
        }

        private string MediaUrl(string path)
        {
            return HttpUtility.HtmlAttributeEncode(ResolveUrl("~/" + path));
        }

        private static string VideoMimeType(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".webm": return "video/webm";
                case ".ogv": return "video/ogg";
                default: return "video/mp4";
            }
        }

        private static string AudioMimeType(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".ogg": return "audio/ogg";
                case ".wav": return "audio/wav";
                default: return "audio/mpeg";
            }
        }
    }
}
