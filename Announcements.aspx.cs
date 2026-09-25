using System;
using System.Data;
using System.Web.UI;

namespace UniSkillHub
{
    public partial class Announcements : Page
    {
        private const int PageSize = 6;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            // Visible = published, already started and not yet expired.
            const string visible =
                "FROM Announcements a INNER JOIN Users u ON u.UserID = a.PostedBy " +
                "WHERE a.Status = 'Published' AND a.PublishDate <= GETDATE() " +
                "AND (a.ExpiryDate IS NULL OR a.ExpiryDate >= GETDATE()) ";

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + visible);
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            DataTable dt = DBHelper.GetDataTable(
                "SELECT a.AnnouncementID, a.Title, a.Content, a.PublishDate, a.ExpiryDate, u.FullName " + visible +
                "ORDER BY a.PublishDate DESC, a.AnnouncementID DESC " +
                "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY",
                DBHelper.Param("@Skip", (page - 1) * PageSize),
                DBHelper.Param("@Take", PageSize));

            rptAnnouncements.DataSource = dt;
            rptAnnouncements.DataBind();
            pnlEmpty.Visible = (total == 0);

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " announcement" : " announcements");
            litPager.Text = Utility.BuildPagerHtml(
                n => ResolveUrl("~/Announcements?page=" + n), page, totalPages, "Announcement pages");
        }

        // ---------- helpers used by the page markup ----------

        protected string ContentHtml(object content)
        {
            return Utility.TextToHtml((string)content);   // encoded first, then paragraphs / line breaks
        }

        protected bool IsNew(object publishDate)
        {
            return (DateTime.Now - (DateTime)publishDate).TotalDays <= 7;
        }

        protected string ExpiryText(object expiryDate)
        {
            // Called even when there is no expiry date (a database NULL), so it must cope with that.
            return (expiryDate is DBNull) ? "" : ((DateTime)expiryDate).ToString("dd MMMM yyyy");
        }
    }
}
