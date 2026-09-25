using System;
using System.Web.Security;
using System.Web.UI;

namespace UniSkillHub.Account
{
    public partial class Logout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Remove the login cookie and any session data, then go to the login page.
            SessionRevocation.RevokeCurrent(Context);   // copies of this login cookie stop working too
            FormsAuthentication.SignOut();
            Session.Abandon();

            Response.Redirect("~/Account/Login?loggedout=1");
        }
    }
}
