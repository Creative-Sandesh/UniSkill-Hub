using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class ManageForum : Page
    {
        private const int PageSize = 10;

        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "hidden",        "The post is now hidden from students." },
            { "unhidden",      "The post is visible again." },
            { "deleted",       "The post and all its replies have been deleted." },
            { "replyhidden",   "The reply is now hidden from students." },
            { "replyunhidden", "The reply is visible again." },
            { "replydeleted",  "The reply has been deleted." }
        };

        // Which list is shown: "posts" (default) or "replies". Kept in the address.
        private bool ShowingReplies
        {
            get { return (Request.QueryString["view"] ?? "").ToLowerInvariant() == "replies"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            pnlPosts.Visible = !ShowingReplies;
            pnlReplies.Visible = ShowingReplies;
            pnlCategoryFilter.Visible = !ShowingReplies;

            lnkPostsTab.Attributes["class"] = "nav-link" + (ShowingReplies ? "" : " active");
            lnkRepliesTab.Attributes["class"] = "nav-link" + (ShowingReplies ? " active" : "");
            lnkReset.HRef = ShowingReplies ? "~/Admin/ManageForum?view=replies" : "~/Admin/ManageForum";

            if (IsPostBack) return;

            LoadCategories();

            txtSearch.Text = Limit((Request.QueryString["q"] ?? "").Trim(), 100);
            SelectIfExists(ddlStatusFilter, Request.QueryString["status"]);
            SelectIfExists(ddlCategoryFilter, Request.QueryString["category"]);

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);

            if (ShowingReplies) BindReplies(page); else BindPosts(page);
        }

        private void LoadCategories()
        {
            ddlCategoryFilter.Items.Add(new ListItem("All categories", ""));
            ddlCategoryFilter.Items.Add(new ListItem("General", "General"));
            foreach (DataRow row in DBHelper.GetDataTable("SELECT CategoryName FROM Categories ORDER BY CategoryName").Rows)
            {
                ddlCategoryFilter.Items.Add(new ListItem((string)row["CategoryName"], (string)row["CategoryName"]));
            }
        }

        private static string LikePattern(string text)
        {
            return text.Length == 0 ? null : "%" + text.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
        }

        // ---------- READ: posts ----------

        private void BindPosts(int page)
        {
            const string where =
                "FROM ForumPosts p INNER JOIN Users u ON u.UserID = p.UserID " +
                "WHERE (@Search IS NULL OR p.Title LIKE @Search OR p.Content LIKE @Search OR u.FullName LIKE @Search OR u.Username LIKE @Search) " +
                "AND (@Status IS NULL OR p.Status = @Status) AND (@Category IS NULL OR p.Category = @Category) ";

            Func<SqlParameter[]> filters = () => new[] {
                DBHelper.Param("@Search", LikePattern(txtSearch.Text)),
                DBHelper.Param("@Status", ddlStatusFilter.SelectedValue.Length == 0 ? null : ddlStatusFilter.SelectedValue),
                DBHelper.Param("@Category", ddlCategoryFilter.SelectedValue.Length == 0 ? null : ddlCategoryFilter.SelectedValue) };

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where, filters());
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            List<SqlParameter> paging = new List<SqlParameter>(filters());
            paging.Add(DBHelper.Param("@Skip", (page - 1) * PageSize));
            paging.Add(DBHelper.Param("@Take", PageSize));

            gvPosts.DataSource = DBHelper.GetDataTable(
                "SELECT p.PostID, p.Title, p.Category, p.Status, p.DatePosted, u.FullName, u.Username, " +
                "  (SELECT COUNT(*) FROM ForumReplies r WHERE r.PostID = p.PostID) AS Replies, " +
                "  (SELECT COUNT(*) FROM ForumReplies r WHERE r.PostID = p.PostID AND r.Status = 'Hidden') AS HiddenReplies " + where +
                "ORDER BY p.DatePosted DESC, p.PostID DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", paging.ToArray());
            gvPosts.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " post" : " posts");
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "Post pages");
        }

        // ---------- READ: replies ----------

        private void BindReplies(int page)
        {
            const string where =
                "FROM ForumReplies r INNER JOIN Users u ON u.UserID = r.UserID INNER JOIN ForumPosts p ON p.PostID = r.PostID " +
                "WHERE (@Search IS NULL OR r.ReplyText LIKE @Search OR u.FullName LIKE @Search OR u.Username LIKE @Search OR p.Title LIKE @Search) " +
                "AND (@Status IS NULL OR r.Status = @Status) ";

            Func<SqlParameter[]> filters = () => new[] {
                DBHelper.Param("@Search", LikePattern(txtSearch.Text)),
                DBHelper.Param("@Status", ddlStatusFilter.SelectedValue.Length == 0 ? null : ddlStatusFilter.SelectedValue) };

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where, filters());
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            List<SqlParameter> paging = new List<SqlParameter>(filters());
            paging.Add(DBHelper.Param("@Skip", (page - 1) * PageSize));
            paging.Add(DBHelper.Param("@Take", PageSize));

            gvReplies.DataSource = DBHelper.GetDataTable(
                "SELECT r.ReplyID, r.Status, r.DatePosted, p.PostID, p.Title AS PostTitle, u.FullName, u.Username, " +
                "  LEFT(r.ReplyText, 160) + CASE WHEN LEN(r.ReplyText) > 160 THEN '...' ELSE '' END AS Snippet " + where +
                "ORDER BY r.DatePosted DESC, r.ReplyID DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", paging.ToArray());
            gvReplies.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " reply" : " replies");
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "Reply pages");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvPosts.HeaderRow != null) gvPosts.HeaderRow.TableSection = TableRowSection.TableHeader;
            if (gvReplies.HeaderRow != null) gvReplies.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(ListUrl(1, null));
        }

        private string ListUrl(int page, string message)
        {
            StringBuilder url = new StringBuilder("~/Admin/ManageForum?view=" + (ShowingReplies ? "replies" : "posts") + "&page=" + page);
            if (txtSearch.Text.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(txtSearch.Text));
            if (ddlStatusFilter.SelectedValue.Length > 0) url.Append("&status=" + ddlStatusFilter.SelectedValue);
            if (!ShowingReplies && ddlCategoryFilter.SelectedValue.Length > 0) url.Append("&category=" + HttpUtility.UrlEncode(ddlCategoryFilter.SelectedValue));
            if (message != null) url.Append("&msg=" + message);
            return url.ToString();
        }

        private int CurrentPage()
        {
            int page;
            return (int.TryParse(Request.QueryString["page"], out page) && page > 0) ? page : 1;
        }

        // ---------- moderation: posts ----------

        protected void gvPosts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            if (e.CommandName == "TogglePost")
            {
                // UPDATE: Active <-> Hidden
                DataTable dt = DBHelper.GetDataTable("SELECT Status FROM ForumPosts WHERE PostID = @Id", DBHelper.Param("@Id", id));
                if (dt.Rows.Count == 0) { ShowMessage("That post no longer exists.", false); return; }

                bool wasActive = (string)dt.Rows[0]["Status"] == "Active";
                DBHelper.ExecuteNonQuery("UPDATE ForumPosts SET Status = @Status WHERE PostID = @Id",
                    DBHelper.Param("@Status", wasActive ? "Hidden" : "Active"), DBHelper.Param("@Id", id));
                Response.Redirect(ListUrl(CurrentPage(), wasActive ? "hidden" : "unhidden"));
            }
            else if (e.CommandName == "DeletePost")
            {
                // DELETE: the post's replies are removed with it (ON DELETE CASCADE)
                int rows = DBHelper.ExecuteNonQuery("DELETE FROM ForumPosts WHERE PostID = @Id", DBHelper.Param("@Id", id));
                if (rows == 0) { ShowMessage("That post no longer exists.", false); return; }
                Response.Redirect(ListUrl(CurrentPage(), "deleted"));
            }
        }

        // ---------- moderation: replies ----------

        protected void gvReplies_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            if (e.CommandName == "ToggleReply")
            {
                DataTable dt = DBHelper.GetDataTable("SELECT Status FROM ForumReplies WHERE ReplyID = @Id", DBHelper.Param("@Id", id));
                if (dt.Rows.Count == 0) { ShowMessage("That reply no longer exists.", false); return; }

                bool wasActive = (string)dt.Rows[0]["Status"] == "Active";
                DBHelper.ExecuteNonQuery("UPDATE ForumReplies SET Status = @Status WHERE ReplyID = @Id",
                    DBHelper.Param("@Status", wasActive ? "Hidden" : "Active"), DBHelper.Param("@Id", id));
                Response.Redirect(ListUrl(CurrentPage(), wasActive ? "replyhidden" : "replyunhidden"));
            }
            else if (e.CommandName == "DeleteReply")
            {
                int rows = DBHelper.ExecuteNonQuery("DELETE FROM ForumReplies WHERE ReplyID = @Id", DBHelper.Param("@Id", id));
                if (rows == 0) { ShowMessage("That reply no longer exists.", false); return; }
                Response.Redirect(ListUrl(CurrentPage(), "replydeleted"));
            }
        }

        // ---------- helpers used by the page markup ----------

        protected string PostUrl(object postId) { return ResolveUrl("~/Forum/PostDetails?id=" + postId); }

        protected string StatusClass(object status)
        {
            return Convert.ToString(status) == "Active" ? "status-badge status-active" : "status-badge status-inactive";
        }

        protected string StatusText(object status)
        {
            return Convert.ToString(status) == "Active" ? "Visible" : "Hidden";
        }

        protected string RepliesText(object total, object hidden)
        {
            int all = (int)total, hid = (int)hidden;
            return all + (hid > 0 ? "<div class=\"dash-meta\">" + hid + " hidden</div>" : "");
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
