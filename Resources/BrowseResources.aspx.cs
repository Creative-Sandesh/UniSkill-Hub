using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Resources
{
    public partial class BrowseResources : Page
    {
        // Used by the card markup (it cannot name the App_Code helper class directly).
        protected string CategoryImage(object categoryName) { return Utility.CategoryImageUrl(categoryName); }

        private const int PageSize = 6;
        private static readonly string[] ResourceTypes = { "PDF", "Video", "Audio", "Article", "Link" };

        protected bool IsLoggedIn
        {
            get { return Request.IsAuthenticated; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            // The search, filters and page number live in the query string, so the
            // results can be bookmarked / shared and the Back button works.
            string search = (Request.QueryString["q"] ?? "").Trim();
            if (search.Length > 100) search = search.Substring(0, 100);

            int categoryId = ParseInt(Request.QueryString["category"]);

            string type = Request.QueryString["type"];
            if (Array.IndexOf(ResourceTypes, type) < 0) type = null;

            int page = Math.Max(1, ParseInt(Request.QueryString["page"]));

            LoadCategories();
            txtSearch.Text = search;
            SelectIfExists(ddlCategory, categoryId > 0 ? categoryId.ToString() : "");
            SelectIfExists(ddlResourceType, type ?? "");

            pnlGuestNote.Visible = !IsLoggedIn;
            BindResources(search, categoryId, type, page);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // Redirect to the same page with the chosen filters in the URL.
            Response.Redirect(BuildUrl(txtSearch.Text.Trim(), ParseInt(ddlCategory.SelectedValue), ddlResourceType.SelectedValue, 1));
        }

        // ---------- data ----------

        private void LoadCategories()
        {
            DataTable categories = DBHelper.GetDataTable("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName");
            ddlCategory.DataSource = categories;
            ddlCategory.DataValueField = "CategoryID";
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("All categories", ""));
        }

        private void BindResources(string search, int categoryId, string type, int page)
        {
            // Every filter is optional: a NULL parameter means "do not filter on this".
            object searchParam = null;
            if (search.Length > 0)
            {
                searchParam = "%" + EscapeLike(search) + "%";
            }
            object categoryParam = categoryId > 0 ? (object)categoryId : null;

            const string where =
                "FROM Resources r INNER JOIN Categories c ON c.CategoryID = r.CategoryID " +
                "WHERE r.Status = 'Published' " +
                "AND (@Search IS NULL OR r.Title LIKE @Search OR r.Description LIKE @Search) " +
                "AND (@CategoryID IS NULL OR r.CategoryID = @CategoryID) " +
                "AND (@Type IS NULL OR r.ResourceType = @Type) ";

            int total = (int)DBHelper.ExecuteScalar(
                "SELECT COUNT(*) " + where,
                DBHelper.Param("@Search", searchParam),
                DBHelper.Param("@CategoryID", categoryParam),
                DBHelper.Param("@Type", type));

            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            DataTable rows = DBHelper.GetDataTable(
                "SELECT r.ResourceID, r.Title, r.ResourceType, r.DateCreated, c.CategoryName, " +
                "       LEFT(r.Description, 130) + CASE WHEN LEN(r.Description) > 130 THEN '...' ELSE '' END AS Snippet, " +
                "       CASE WHEN r.FilePath IS NULL THEN 0 ELSE 1 END AS HasFile " +
                where +
                "ORDER BY r.DateCreated DESC, r.ResourceID DESC " +
                "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY",
                DBHelper.Param("@Search", searchParam),
                DBHelper.Param("@CategoryID", categoryParam),
                DBHelper.Param("@Type", type),
                DBHelper.Param("@Skip", (page - 1) * PageSize),
                DBHelper.Param("@Take", PageSize));

            rptResources.DataSource = rows;
            rptResources.DataBind();

            pnlEmpty.Visible = (total == 0);
            lblResultCount.Text = (total == 0)
                ? ""
                : "Showing " + total + (total == 1 ? " resource" : " resources");
            litPager.Text = BuildPager(search, categoryId, type, page, totalPages);
        }

        // ---------- helpers used by the page markup ----------

        protected string DetailsUrl(object resourceId)
        {
            return ResolveUrl("~/Resources/ResourceDetails?id=" + resourceId);
        }

        protected string DownloadUrl(object resourceId)
        {
            return ResolveUrl("~/Resources/Download.ashx?id=" + resourceId);
        }

        private string BuildUrl(string search, int categoryId, string type, int page)
        {
            StringBuilder url = new StringBuilder("~/Resources/BrowseResources?page=" + page);
            if (search.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(search));
            if (categoryId > 0) url.Append("&category=" + categoryId);
            if (!string.IsNullOrEmpty(type)) url.Append("&type=" + HttpUtility.UrlEncode(type));
            return url.ToString();
        }

        private string BuildPager(string search, int categoryId, string type, int page, int totalPages)
        {
            if (totalPages <= 1) return "";

            StringBuilder html = new StringBuilder();
            html.Append("<nav aria-label=\"Resource pages\"><ul class=\"pagination justify-content-center mt-5\">");

            html.Append(PagerItem("Previous", page - 1, page > 1, false, search, categoryId, type));
            for (int i = 1; i <= totalPages; i++)
            {
                html.Append(PagerItem(i.ToString(), i, true, i == page, search, categoryId, type));
            }
            html.Append(PagerItem("Next", page + 1, page < totalPages, false, search, categoryId, type));

            html.Append("</ul></nav>");
            return html.ToString();
        }

        private string PagerItem(string text, int targetPage, bool enabled, bool current, string search, int categoryId, string type)
        {
            if (!enabled)
            {
                return "<li class=\"page-item disabled\"><span class=\"page-link\">" + text + "</span></li>";
            }

            string href = HttpUtility.HtmlAttributeEncode(ResolveUrl(BuildUrl(search, categoryId, type, targetPage)));
            return "<li class=\"page-item" + (current ? " active\" aria-current=\"page" : "") + "\">" +
                   "<a class=\"page-link\" href=\"" + href + "\">" + text + "</a></li>";
        }

        // Makes the characters % _ [ harmless inside a SQL LIKE pattern,
        // so a search for "50%" finds the text "50%" instead of everything.
        private static string EscapeLike(string text)
        {
            return text.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        private static int ParseInt(string value)
        {
            int result;
            return int.TryParse(value, out result) ? result : 0;
        }

        private static void SelectIfExists(DropDownList list, string value)
        {
            ListItem item = list.Items.FindByValue(value);
            if (item != null) list.SelectedValue = value;
        }
    }
}
