using System;
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

        }
    }
}
