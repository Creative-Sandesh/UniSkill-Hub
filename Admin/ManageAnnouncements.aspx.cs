using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class ManageAnnouncements : Page
    {
        private const int PageSize = 10;
        private const string DateFormat = "yyyy-MM-dd'T'HH:mm";

        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "created",     "The announcement has been saved." },
            { "updated",     "The announcement has been updated." },
            { "deleted",     "The announcement has been deleted." },
            { "published",   "The announcement is now published (it shows once its publish date has arrived)." },
            { "unpublished", "The announcement is now a draft and hidden from students." }
        };

        private int EditingId
        {
            get { return ViewState["EditAnnouncementId"] is int ? (int)ViewState["EditAnnouncementId"] : 0; }
            set { ViewState["EditAnnouncementId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            if (IsPostBack) return;

            txtSearch.Text = Limit((Request.QueryString["q"] ?? "").Trim(), 100);
            SelectIfExists(ddlStateFilter, Request.QueryString["state"]);

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);

            BindGrid(page);
        }

        // ---------- READ: the announcements list ----------

        // The "State" is worked out from the dates and status, so the admin sees what students see:
        //   Draft (hidden) | Scheduled (starts later) | Expired (ended) | Live (visible now)
        private const string StateColumn =
            "CASE WHEN a.Status = 'Draft' THEN 'Draft' " +
            "     WHEN a.PublishDate > GETDATE() THEN 'Scheduled' " +
            "     WHEN a.ExpiryDate IS NOT NULL AND a.ExpiryDate < GETDATE() THEN 'Expired' " +
            "     ELSE 'Live' END";

        private void BindGrid(int page)
        {
            string search = txtSearch.Text;
            string state = ddlStateFilter.SelectedValue;

            const string inner =
                "SELECT a.AnnouncementID, a.Title, a.PublishDate, a.ExpiryDate, a.Status, u.FullName, ";
            const string from =
                " AS State FROM Announcements a INNER JOIN Users u ON u.UserID = a.PostedBy " +
                "WHERE (@Search IS NULL OR a.Title LIKE @Search OR a.Content LIKE @Search)";

            Func<SqlParameter[]> filters = () => new[] {
                DBHelper.Param("@Search", search.Length == 0 ? null : "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%"),
                DBHelper.Param("@State", state.Length == 0 ? null : state) };

            string derived = "(" + inner + StateColumn + from + ") x WHERE (@State IS NULL OR x.State = @State) ";

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) FROM " + derived, filters());
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            List<SqlParameter> paging = new List<SqlParameter>(filters());
            paging.Add(DBHelper.Param("@Skip", (page - 1) * PageSize));
            paging.Add(DBHelper.Param("@Take", PageSize));

            gvAnnouncements.DataSource = DBHelper.GetDataTable(
                "SELECT x.* FROM " + derived + "ORDER BY x.PublishDate DESC, x.AnnouncementID DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", paging.ToArray());
            gvAnnouncements.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " announcement" : " announcements");
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "Announcement pages");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvAnnouncements.HeaderRow != null) gvAnnouncements.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(ListUrl(1, null));
        }

        private string ListUrl(int page, string message)
        {
            StringBuilder url = new StringBuilder("~/Admin/ManageAnnouncements?page=" + page);
            if (txtSearch.Text.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(txtSearch.Text));
            if (ddlStateFilter.SelectedValue.Length > 0) url.Append("&state=" + ddlStateFilter.SelectedValue);
            if (message != null) url.Append("&msg=" + message);
            return url.ToString();
        }

        private int CurrentPage()
        {
            int page;
            return (int.TryParse(Request.QueryString["page"], out page) && page > 0) ? page : 1;
        }

        // ---------- row buttons ----------

        protected void gvAnnouncements_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            switch (e.CommandName)
            {
                case "EditAnnouncement": ShowForm(id); break;
                case "TogglePublish": TogglePublish(id); break;
                case "DeleteAnnouncement": Delete(id); break;
            }
        }

        // UPDATE: Published <-> Draft
        private void TogglePublish(int id)
        {
            DataTable dt = DBHelper.GetDataTable("SELECT Status FROM Announcements WHERE AnnouncementID = @Id", DBHelper.Param("@Id", id));
            if (dt.Rows.Count == 0) { ShowMessage("That announcement no longer exists.", false); return; }

            bool wasPublished = (string)dt.Rows[0]["Status"] == "Published";
            DBHelper.ExecuteNonQuery("UPDATE Announcements SET Status = @Status WHERE AnnouncementID = @Id",
                DBHelper.Param("@Status", wasPublished ? "Draft" : "Published"), DBHelper.Param("@Id", id));

            Response.Redirect(ListUrl(CurrentPage(), wasPublished ? "unpublished" : "published"));
        }

        // DELETE
        private void Delete(int id)
        {
            int rows = DBHelper.ExecuteNonQuery("DELETE FROM Announcements WHERE AnnouncementID = @Id", DBHelper.Param("@Id", id));
            if (rows == 0) { ShowMessage("That announcement no longer exists.", false); return; }

            Response.Redirect(ListUrl(CurrentPage(), "deleted"));
        }

        // ---------- the Add / Edit form ----------

        protected void btnAdd_Click(object sender, EventArgs e) { ShowForm(0); }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            EditingId = 0;
        }

        private void ShowForm(int id)
        {
            EditingId = id;
            pnlForm.Visible = true;

            if (id == 0)
            {
                lblFormTitle.Text = "Add announcement";
                txtTitle.Text = txtContent.Text = txtExpiryDate.Text = "";
                txtPublishDate.Text = DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture);
                ddlStatus.SelectedValue = "Published";
                return;
            }

            DataTable dt = DBHelper.GetDataTable(
                "SELECT Title, Content, PublishDate, ExpiryDate, Status FROM Announcements WHERE AnnouncementID = @Id", DBHelper.Param("@Id", id));

            if (dt.Rows.Count == 0)
            {
                pnlForm.Visible = false;
                EditingId = 0;
                ShowMessage("That announcement no longer exists.", false);
                return;
            }

            DataRow a = dt.Rows[0];
            lblFormTitle.Text = "Edit announcement";
            txtTitle.Text = (string)a["Title"];
            txtContent.Text = (string)a["Content"];
            txtPublishDate.Text = ((DateTime)a["PublishDate"]).ToString(DateFormat, CultureInfo.InvariantCulture);
            txtExpiryDate.Text = (a["ExpiryDate"] is DBNull) ? "" : ((DateTime)a["ExpiryDate"]).ToString(DateFormat, CultureInfo.InvariantCulture);
            ddlStatus.SelectedValue = (string)a["Status"];
        }

        // ---------- server-side validation ----------

        private static bool TryParseDate(string text, out DateTime value)
        {
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value) && value.Year >= 2000 && value.Year <= 2100;
        }

        protected void cvPublish_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime d;
            args.IsValid = TryParseDate(args.Value, out d);
            if (!args.IsValid) cvPublish.ErrorMessage = "Please enter a valid publish date and time.";
        }

        // The expiry is optional, but if given it must be after the publish date.
        protected void cvExpiry_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string text = (args.Value ?? "").Trim();
            if (text.Length == 0) return;

            DateTime expiry, publish;
            if (!TryParseDate(text, out expiry))
            {
                cvExpiry.ErrorMessage = "Please enter a valid expiry date and time (or leave it empty).";
                args.IsValid = false;
            }
            else if (TryParseDate(txtPublishDate.Text, out publish) && expiry <= publish)
            {
                cvExpiry.ErrorMessage = "The expiry date must be after the publish date.";
                args.IsValid = false;
            }
        }

        // ---------- INSERT / UPDATE ----------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("AnnouncementForm");
            if (!Page.IsValid) return;

            int id = EditingId;
            DateTime publish;
            TryParseDate(txtPublishDate.Text, out publish);

            DateTime expiry = DateTime.MinValue;
            bool hasExpiry = txtExpiryDate.Text.Trim().Length > 0 && TryParseDate(txtExpiryDate.Text, out expiry);

            List<SqlParameter> p = new List<SqlParameter>
            {
                DBHelper.Param("@Title", txtTitle.Text.Trim()),
                DBHelper.Param("@Content", txtContent.Text.Trim()),
                DBHelper.Param("@PublishDate", publish),
                DBHelper.Param("@ExpiryDate", hasExpiry ? (object)expiry : null),
                DBHelper.Param("@Status", ddlStatus.SelectedValue)
            };

            try
            {
                if (id == 0)
                {
                    p.Add(DBHelper.Param("@PostedBy", Utility.CurrentUserId));
                    DBHelper.ExecuteNonQuery(
                        "INSERT INTO Announcements (Title, Content, PostedBy, PublishDate, ExpiryDate, Status) " +
                        "VALUES (@Title, @Content, @PostedBy, @PublishDate, @ExpiryDate, @Status)", p.ToArray());
                }
                else
                {
                    p.Add(DBHelper.Param("@Id", id));
                    int rows = DBHelper.ExecuteNonQuery(
                        "UPDATE Announcements SET Title = @Title, Content = @Content, PublishDate = @PublishDate, " +
                        "ExpiryDate = @ExpiryDate, Status = @Status WHERE AnnouncementID = @Id", p.ToArray());
                    if (rows == 0) { ShowMessage("That announcement no longer exists.", false); pnlForm.Visible = false; return; }
                }
            }
            catch (SqlException)
            {
                ShowMessage("Sorry, the announcement could not be saved right now. Please try again.", false);
                return;
            }

            Response.Redirect(ListUrl(id == 0 ? 1 : CurrentPage(), id == 0 ? "created" : "updated"));
        }

        // ---------- helpers used by the page markup ----------

        protected string StateClass(object state) { return "status-badge status-" + Convert.ToString(state).ToLowerInvariant(); }

        protected string ExpiryText(object expiry)
        {
            return (expiry is DBNull) ? "<span class=\"text-muted\">never</span>"
                                      : HttpUtility.HtmlEncode(((DateTime)expiry).ToString("dd MMM yyyy, h:mm tt"));
        }

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
