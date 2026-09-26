using System;
using System.Data;
using System.Web.UI;

namespace UniSkillHub
{
    public partial class _Default : Page
    {
        // Where the "dashboard" buttons on the home page should point,
        // depending on who is logged in.
        protected string DashboardUrl
        {
            get
            {
                return ResolveUrl(User.IsInRole("Admin") ? "~/Admin/Dashboard" : "~/Student/Dashboard");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindCategories();
        }

        // One tile per category, with the number of PUBLISHED resources in it.
        // If the database is not reachable the section is simply left out, so the home page still opens.
        private void BindCategories()
        {
            try
            {
                DataTable dt = DBHelper.GetDataTable(
                    "SELECT c.CategoryID, c.CategoryName, c.Description, COUNT(r.ResourceID) AS ResourceCount " +
                    "FROM Categories c " +
                    "LEFT JOIN Resources r ON r.CategoryID = c.CategoryID AND r.Status = 'Published' " +
                    "GROUP BY c.CategoryID, c.CategoryName, c.Description " +
                    "ORDER BY c.CategoryID");

                rptCategories.DataSource = dt;
                rptCategories.DataBind();
                pnlCategories.Visible = dt.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                Logger.Error("Home page: could not load the categories", ex);
                pnlCategories.Visible = false;
            }
        }

        // The markup uses these small methods (it cannot name the App_Code helper class directly).
        protected string CategoryImage(object categoryName) { return Utility.CategoryImageUrl(categoryName); }

        protected string CategoryLink(object categoryId) { return Utility.CategoryLinkUrl(categoryId); }

        protected string CountText(object count)
        {
            int n = Convert.ToInt32(count);
            return n + (n == 1 ? " resource" : " resources");
        }
    }
}
