using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class ManageResources : Page
    {
        private const int PageSize = 10;

        // What each type stores, where, and how big it may be.
        // (Keep the extensions and sizes in sync with Scripts/validation.js.)
        private class UploadRule
        {
            public string[] Extensions;
            public int MaxBytes;
            public string Folder;       // under /Uploads
            public string Column;       // database column that holds the path
            public string Label;        // for messages
        }

        private static UploadRule RuleFor(string type)
        {
            switch (type)
            {
                case "PDF":
                    return new UploadRule { Extensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".zip", ".txt" },
                                            MaxBytes = 20 * 1024 * 1024, Folder = "Uploads/Notes/", Column = "FilePath", Label = "document" };
                case "Video":
                    return new UploadRule { Extensions = new[] { ".mp4", ".webm", ".ogv" },
                                            MaxBytes = 40 * 1024 * 1024, Folder = "Uploads/Videos/", Column = "VideoPath", Label = "video" };
                case "Audio":
                    return new UploadRule { Extensions = new[] { ".mp3", ".ogg", ".wav" },
                                            MaxBytes = 20 * 1024 * 1024, Folder = "Uploads/Audio/", Column = "AudioPath", Label = "audio" };
                default:
                    return null;
            }
        }

        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "created",     "The resource has been created." },
            { "updated",     "The resource has been updated." },
            { "deleted",     "The resource and its file have been deleted." },
            { "published",   "The resource is now published and visible to students." },
            { "unpublished", "The resource is now a draft and hidden from students." }
        };

        private int EditingResourceId
        {
            get { return ViewState["EditResourceId"] is int ? (int)ViewState["EditResourceId"] : 0; }
            set { ViewState["EditResourceId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            if (IsPostBack) return;

            LoadCategories();

            txtSearch.Text = Limit((Request.QueryString["q"] ?? "").Trim(), 100);
            SelectIfExists(ddlCategoryFilter, Request.QueryString["category"]);
            SelectIfExists(ddlTypeFilter, Request.QueryString["type"]);
            SelectIfExists(ddlStatusFilter, Request.QueryString["status"]);

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);
            if (Request.QueryString["toolarge"] == "1") ShowMessage("That upload was too large. Videos may be up to 40 MB, other files up to 20 MB.", false);

            BindGrid(page);
        }

        private void LoadCategories()
        {
            DataTable categories = DBHelper.GetDataTable("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName");

            ddlCategory.DataSource = categories;
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataBind();

            ddlCategoryFilter.DataSource = categories;
            ddlCategoryFilter.DataValueField = "CategoryID";
            ddlCategoryFilter.DataTextField = "CategoryName";
            ddlCategoryFilter.DataBind();
            ddlCategoryFilter.Items.Insert(0, new ListItem("All categories", ""));
        }

        // ---------- READ: the resources list ----------

        private void BindGrid(int page)
        {
            object searchParam = null;
            if (txtSearch.Text.Length > 0)
            {
                searchParam = "%" + txtSearch.Text.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            }
            int categoryId;
            object categoryParam = int.TryParse(ddlCategoryFilter.SelectedValue, out categoryId) ? (object)categoryId : null;
            object typeParam = string.IsNullOrEmpty(ddlTypeFilter.SelectedValue) ? null : ddlTypeFilter.SelectedValue;
            object statusParam = string.IsNullOrEmpty(ddlStatusFilter.SelectedValue) ? null : ddlStatusFilter.SelectedValue;

            const string where =
                "FROM Resources r INNER JOIN Categories c ON c.CategoryID = r.CategoryID " +
                "WHERE (@Search IS NULL OR r.Title LIKE @Search OR r.Description LIKE @Search) " +
                "AND (@CategoryID IS NULL OR r.CategoryID = @CategoryID) " +
                "AND (@Type IS NULL OR r.ResourceType = @Type) AND (@Status IS NULL OR r.Status = @Status) ";

            SqlParameter[] filters = {
                DBHelper.Param("@Search", searchParam), DBHelper.Param("@CategoryID", categoryParam),
                DBHelper.Param("@Type", typeParam), DBHelper.Param("@Status", statusParam) };

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where, CloneParams(filters));
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            List<SqlParameter> paging = new List<SqlParameter>(CloneParams(filters));
            paging.Add(DBHelper.Param("@Skip", (page - 1) * PageSize));
            paging.Add(DBHelper.Param("@Take", PageSize));

            gvResources.DataSource = DBHelper.GetDataTable(
                "SELECT r.ResourceID, r.Title, r.ResourceType, r.Status, r.DateCreated, c.CategoryName " + where +
                "ORDER BY r.DateCreated DESC, r.ResourceID DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", paging.ToArray());
            gvResources.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " resource" : " resources");
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "Resource pages");
        }

        // A SqlParameter can belong to only one command, so each query gets its own copies.
        private static SqlParameter[] CloneParams(SqlParameter[] source)
        {
            SqlParameter[] copy = new SqlParameter[source.Length];
            for (int i = 0; i < source.Length; i++) copy[i] = DBHelper.Param(source[i].ParameterName, source[i].Value);
            return copy;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvResources.HeaderRow != null) gvResources.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(ListUrl(1, null));
        }

        private string ListUrl(int page, string message)
        {
            StringBuilder url = new StringBuilder("~/Admin/ManageResources?page=" + page);
            if (txtSearch.Text.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(txtSearch.Text));
            if (ddlCategoryFilter.SelectedValue.Length > 0) url.Append("&category=" + ddlCategoryFilter.SelectedValue);
            if (ddlTypeFilter.SelectedValue.Length > 0) url.Append("&type=" + ddlTypeFilter.SelectedValue);
            if (ddlStatusFilter.SelectedValue.Length > 0) url.Append("&status=" + ddlStatusFilter.SelectedValue);
            if (message != null) url.Append("&msg=" + message);
            return url.ToString();
        }

        private int CurrentPage()
        {
            int page;
            return (int.TryParse(Request.QueryString["page"], out page) && page > 0) ? page : 1;
        }

        // ---------- row buttons ----------

        protected void gvResources_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            switch (e.CommandName)
            {
                case "EditResource": ShowForm(id); break;
                case "TogglePublish": TogglePublish(id); break;
                case "DeleteResource": DeleteResource(id); break;
            }
        }

        // UPDATE: Published <-> Draft
        private void TogglePublish(int id)
        {
            DataTable dt = DBHelper.GetDataTable("SELECT Status FROM Resources WHERE ResourceID = @Id", DBHelper.Param("@Id", id));
            if (dt.Rows.Count == 0) { ShowMessage("That resource no longer exists.", false); return; }

            bool wasPublished = (string)dt.Rows[0]["Status"] == "Published";
            DBHelper.ExecuteNonQuery("UPDATE Resources SET Status = @Status WHERE ResourceID = @Id",
                DBHelper.Param("@Status", wasPublished ? "Draft" : "Published"), DBHelper.Param("@Id", id));

            Response.Redirect(ListUrl(CurrentPage(), wasPublished ? "unpublished" : "published"));
        }

        // DELETE: the database row first, then the uploaded files it pointed to.
        private void DeleteResource(int id)
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT FilePath, VideoPath, AudioPath FROM Resources WHERE ResourceID = @Id", DBHelper.Param("@Id", id));
            if (dt.Rows.Count == 0) { ShowMessage("That resource no longer exists.", false); return; }

            DataRow files = dt.Rows[0];
            DBHelper.ExecuteNonQuery("DELETE FROM Resources WHERE ResourceID = @Id", DBHelper.Param("@Id", id));

            foreach (string column in new[] { "FilePath", "VideoPath", "AudioPath" })
            {
                string path = files[column] as string;
                if (!string.IsNullOrEmpty(path)) FileHelper.DeleteUploadedFile(Context, path);
            }

            Response.Redirect(ListUrl(CurrentPage(), "deleted"));
        }

        // ---------- the Add / Edit form ----------

        protected void btnAdd_Click(object sender, EventArgs e) { ShowForm(0); }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            EditingResourceId = 0;
        }

        private void ShowForm(int id)
        {
            EditingResourceId = id;
            pnlForm.Visible = true;
            lblCurrentDocument.Text = lblCurrentVideo.Text = lblCurrentAudio.Text = "";

            if (id == 0)
            {
                lblFormTitle.Text = "Add resource";
                txtTitle.Text = txtDescription.Text = txtExternalUrl.Text = "";
                ddlResourceType.SelectedValue = "PDF";
                ddlStatus.SelectedValue = "Published";
                return;
            }

            DataTable dt = DBHelper.GetDataTable(
                "SELECT CategoryID, Title, Description, ResourceType, FilePath, VideoPath, AudioPath, ExternalURL, Status " +
                "FROM Resources WHERE ResourceID = @Id", DBHelper.Param("@Id", id));

            if (dt.Rows.Count == 0)
            {
                pnlForm.Visible = false;
                EditingResourceId = 0;
                ShowMessage("That resource no longer exists.", false);
                return;
            }

            DataRow r = dt.Rows[0];
            lblFormTitle.Text = "Edit resource";
            txtTitle.Text = (string)r["Title"];
            txtDescription.Text = (string)r["Description"];
            SelectIfExists(ddlCategory, r["CategoryID"].ToString());
            ddlResourceType.SelectedValue = (string)r["ResourceType"];
            ddlStatus.SelectedValue = (string)r["Status"];
            txtExternalUrl.Text = (r["ExternalURL"] as string) ?? "";

            lblCurrentDocument.Text = CurrentFileText(r["FilePath"] as string);
            lblCurrentVideo.Text = CurrentFileText(r["VideoPath"] as string);
            lblCurrentAudio.Text = CurrentFileText(r["AudioPath"] as string);
        }

        private static string CurrentFileText(string path)
        {
            return string.IsNullOrEmpty(path) ? "" : "Current file: " + Path.GetFileName(path) + " (choose a new file to replace it)";
        }

        // ---------- server-side validation ----------

        protected void cvDocument_ServerValidate(object source, ServerValidateEventArgs args) { args.IsValid = CheckUpload("PDF", fuDocument, cvDocument); }
        protected void cvVideo_ServerValidate(object source, ServerValidateEventArgs args) { args.IsValid = CheckUpload("Video", fuVideo, cvVideo); }
        protected void cvAudio_ServerValidate(object source, ServerValidateEventArgs args) { args.IsValid = CheckUpload("Audio", fuAudio, cvAudio); }

        // Checks the upload that belongs to the selected type (other types' validators pass).
        // A new resource needs a file; when editing, "no new file" keeps the existing one.
        private bool CheckUpload(string type, FileUpload upload, CustomValidator validator)
        {
            if (ddlResourceType.SelectedValue != type) return true;

            UploadRule rule = RuleFor(type);

            if (!upload.HasFile)
            {
                if (EditingResourceId != 0 && ExistingPath(rule.Column) != null) return true;
                validator.ErrorMessage = "Please choose a " + rule.Label + " file to upload.";
                return false;
            }

            HttpPostedFile file = upload.PostedFile;
            if (file.ContentLength == 0) { validator.ErrorMessage = "That file is empty."; return false; }
            if (file.ContentLength > rule.MaxBytes) { validator.ErrorMessage = "That file is larger than " + (rule.MaxBytes / (1024 * 1024)) + " MB."; return false; }
            if (!FileHelper.IsAllowedExtension(file.FileName, rule.Extensions))
            {
                validator.ErrorMessage = "That file type is not allowed. Allowed: " + string.Join(", ", rule.Extensions).ToUpperInvariant() + ".";
                return false;
            }
            if (!FileHelper.ContentMatchesExtension(file))
            {
                validator.ErrorMessage = "The contents of that file do not match its type.";
                return false;
            }
            return true;
        }

        protected void cvUrl_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (ddlResourceType.SelectedValue != "Link") { args.IsValid = true; return; }
            args.IsValid = IsSafeWebUrl(txtExternalUrl.Text.Trim());
        }

        // Only http/https addresses, so a value such as "javascript:..." can never be stored as a link.
        private static bool IsSafeWebUrl(string url)
        {
            Uri uri;
            return url.Length > 0 && url.Length <= 500 && Uri.TryCreate(url, UriKind.Absolute, out uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private string ExistingPath(string column)
        {
            // "column" is always one of three fixed names from RuleFor() - never user input.
            DataTable dt = DBHelper.GetDataTable("SELECT " + column + " FROM Resources WHERE ResourceID = @Id",
                DBHelper.Param("@Id", EditingResourceId));
            return dt.Rows.Count == 0 ? null : dt.Rows[0][0] as string;
        }

        // ---------- INSERT / UPDATE ----------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("ResourceForm");
            if (!Page.IsValid) return;

            int id = EditingResourceId;
            string type = ddlResourceType.SelectedValue;
            UploadRule rule = RuleFor(type);

            // What the resource has today (empty for a new one).
            Dictionary<string, string> old = new Dictionary<string, string> { { "FilePath", null }, { "VideoPath", null }, { "AudioPath", null }, { "ExternalURL", null } };
            if (id != 0)
            {
                DataTable dt = DBHelper.GetDataTable("SELECT FilePath, VideoPath, AudioPath, ExternalURL FROM Resources WHERE ResourceID = @Id", DBHelper.Param("@Id", id));
                if (dt.Rows.Count == 0) { ShowMessage("That resource no longer exists.", false); pnlForm.Visible = false; return; }
                foreach (string key in new List<string>(old.Keys)) old[key] = dt.Rows[0][key] as string;
            }

            // What it will have: only the field that belongs to the chosen type is kept.
            Dictionary<string, string> now = new Dictionary<string, string> { { "FilePath", null }, { "VideoPath", null }, { "AudioPath", null }, { "ExternalURL", null } };
            string newFile = null;

            try
            {
                if (rule != null)
                {
                    FileUpload upload = type == "PDF" ? fuDocument : (type == "Video" ? fuVideo : fuAudio);
                    if (upload.HasFile)
                    {
                        newFile = SaveUpload(upload, rule);
                        now[rule.Column] = newFile;
                    }
                    else
                    {
                        now[rule.Column] = old[rule.Column];     // keep the existing file
                    }
                }
                else if (type == "Link")
                {
                    now["ExternalURL"] = txtExternalUrl.Text.Trim();
                }

                SaveRow(id, now);
            }
            catch (Exception)
            {
                if (newFile != null) FileHelper.DeleteUploadedFile(Context, newFile);   // no orphan file
                ShowMessage("Sorry, the resource could not be saved right now. Please try again.", false);
                return;
            }

            // The database now points to the new files, so files that are no longer used can go.
            foreach (string column in new[] { "FilePath", "VideoPath", "AudioPath" })
            {
                if (old[column] != null && old[column] != now[column]) FileHelper.DeleteUploadedFile(Context, old[column]);
            }

            Response.Redirect(ListUrl(id == 0 ? 1 : CurrentPage(), id == 0 ? "created" : "updated"));
        }

        // Saves the file under a safe, unique name and returns its relative path.
        private string SaveUpload(FileUpload upload, UploadRule rule)
        {
            string extension = Path.GetExtension(upload.PostedFile.FileName).ToLowerInvariant();
            string name = FileHelper.MakeSafeFileName(Path.GetFileNameWithoutExtension(upload.PostedFile.FileName), 60) +
                          "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
            string relative = rule.Folder + name;

            Directory.CreateDirectory(Server.MapPath("~/" + rule.Folder));
            upload.SaveAs(Server.MapPath("~/" + relative));
            return relative;
        }

        private void SaveRow(int id, Dictionary<string, string> paths)
        {
            List<SqlParameter> p = new List<SqlParameter>
            {
                DBHelper.Param("@CategoryID", int.Parse(ddlCategory.SelectedValue)),
                DBHelper.Param("@Title", txtTitle.Text.Trim()),
                DBHelper.Param("@Description", txtDescription.Text.Trim()),
                DBHelper.Param("@Type", ddlResourceType.SelectedValue),
                DBHelper.Param("@FilePath", paths["FilePath"]),
                DBHelper.Param("@VideoPath", paths["VideoPath"]),
                DBHelper.Param("@AudioPath", paths["AudioPath"]),
                DBHelper.Param("@ExternalURL", paths["ExternalURL"]),
                DBHelper.Param("@Status", ddlStatus.SelectedValue)
            };

            if (id == 0)
            {
                p.Add(DBHelper.Param("@CreatedBy", Utility.CurrentUserId));
                DBHelper.ExecuteNonQuery(
                    "INSERT INTO Resources (CategoryID, Title, Description, ResourceType, FilePath, VideoPath, AudioPath, ExternalURL, CreatedBy, Status) " +
                    "VALUES (@CategoryID, @Title, @Description, @Type, @FilePath, @VideoPath, @AudioPath, @ExternalURL, @CreatedBy, @Status)", p.ToArray());
            }
            else
            {
                p.Add(DBHelper.Param("@Id", id));
                DBHelper.ExecuteNonQuery(
                    "UPDATE Resources SET CategoryID = @CategoryID, Title = @Title, Description = @Description, ResourceType = @Type, " +
                    "FilePath = @FilePath, VideoPath = @VideoPath, AudioPath = @AudioPath, ExternalURL = @ExternalURL, Status = @Status " +
                    "WHERE ResourceID = @Id", p.ToArray());
            }
        }

        // ---------- helpers used by the page markup ----------

        protected string ViewUrl(object id) { return ResolveUrl("~/Resources/ResourceDetails?id=" + id); }

        protected string TypeClass(object type) { return "type-badge type-" + Convert.ToString(type).ToLowerInvariant(); }

        protected string StatusClass(object status) { return Utility.StatusBadgeClass(status); }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }

        private static string Limit(string text, int max) { return text.Length > max ? text.Substring(0, max) : text; }

        private static void SelectIfExists(DropDownList list, string value)
        {
            ListItem item = list.Items.FindByValue(value ?? "");
            if (item != null) list.SelectedValue = item.Value;
        }
    }
}
