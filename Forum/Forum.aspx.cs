using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Forum
{
    public partial class ForumHome : Page
    {
        private const int PageSize = 8;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            pnlDeleted.Visible = (Request.QueryString["deleted"] == "1");

            // Anyone may read the forum, but creating a discussion needs a login
            // (CreatePost is protected by Forum/Web.config; a guest is sent to the login page).

            string search = (Request.QueryString["q"] ?? "").Trim();
            if (search.Length > 100) search = search.Substring(0, 100);

            LoadCategories();
            string category = Request.QueryString["category"];
            if (ddlCategory.Items.FindByValue(category ?? "") == null) category = "";

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            txtSearch.Text = search;
            ddlCategory.SelectedValue = category;

            BindPosts(search, category, page);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(BuildUrl(txtSearch.Text.Trim(), ddlCategory.SelectedValue, 1));
        }

        // ---------- data ----------

        // The forum categories are "General" plus the resource categories.
        private void LoadCategories()
        {
            ddlCategory.Items.Add(new ListItem("All categories", ""));
            ddlCategory.Items.Add(new ListItem("General", "General"));
            foreach (DataRow row in DBHelper.GetDataTable("SELECT CategoryName FROM Categories ORDER BY CategoryName").Rows)
            {
                string name = (string)row["CategoryName"];
                ddlCategory.Items.Add(new ListItem(name, name));
            }
        }

        private void BindPosts(string search, string category, int page)
        {
            object searchParam = null;
            if (search.Length > 0)
            {
                // make % _ [ harmless inside LIKE
                searchParam = "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            }
            object categoryParam = string.IsNullOrEmpty(category) ? null : category;

            // Only Active posts are visible (moderated posts are "Hidden").
            const string where =
                "FROM ForumPosts p INNER JOIN Users u ON u.UserID = p.UserID " +
                "WHERE p.Status = 'Active' " +
                "AND (@Search IS NULL OR p.Title LIKE @Search OR p.Content LIKE @Search) " +
                "AND (@Category IS NULL OR p.Category = @Category) ";

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where,
                DBHelper.Param("@Search", searchParam), DBHelper.Param("@Category", categoryParam));

            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            // Newest activity first: the latest reply, or the post itself if it has none.
            DataTable dt = DBHelper.GetDataTable(
                "SELECT p.PostID, p.Title, p.Category, p.DatePosted, u.FullName, u.Role, " +
                "  LEFT(p.Content, 150) + CASE WHEN LEN(p.Content) > 150 THEN '...' ELSE '' END AS Snippet, " +
                "  (SELECT COUNT(*) FROM ForumReplies r WHERE r.PostID = p.PostID AND r.Status = 'Active') AS Replies " +
                where +
                "ORDER BY ISNULL((SELECT MAX(r.DatePosted) FROM ForumReplies r WHERE r.PostID = p.PostID AND r.Status = 'Active'), p.DatePosted) DESC, p.PostID DESC " +
                "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY",
                DBHelper.Param("@Search", searchParam),
                DBHelper.Param("@Category", categoryParam),
                DBHelper.Param("@Skip", (page - 1) * PageSize),
                DBHelper.Param("@Take", PageSize));

            rptPosts.DataSource = dt;
            rptPosts.DataBind();
            pnlEmpty.Visible = (total == 0);
            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " discussion" : " discussions");
            litPager.Text = Utility.BuildPagerHtml(
                n => ResolveUrl(BuildUrl(search, category, n)), page, totalPages, "Forum pages");
        }

        // ---------- helpers ----------

        protected string PostUrl(object postId)
        {
            return ResolveUrl("~/Forum/PostDetails?id=" + postId);
        }

        private static string BuildUrl(string search, string category, int page)
        {
            StringBuilder url = new StringBuilder("~/Forum/Forum?page=" + page);
            if (!string.IsNullOrEmpty(search)) url.Append("&q=" + HttpUtility.UrlEncode(search));
            if (!string.IsNullOrEmpty(category)) url.Append("&category=" + HttpUtility.UrlEncode(category));
            return url.ToString();
        }
    }
}
