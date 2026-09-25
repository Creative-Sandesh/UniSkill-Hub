using System;
using System.Web;
using System.Web.UI;

namespace UniSkillHub
{
    // Thrown when a form is posted by a browser that did not load it (see the anti-CSRF check below).
    // Global.asax turns it into a friendly page.
    public class AntiForgeryException : HttpException
    {
        public AntiForgeryException() : base(400, "The form could not be verified.") { }
    }

    public partial class SiteMaster : MasterPage
    {
        private const string TokenKey = "__AntiXsrfToken";
        private const string UserKey = "__AntiXsrfUserName";
        private string _token;

        // ---------- anti-CSRF (cross-site request forgery) ----------
        // A cookie holds a random token that only this browser has. The token is also mixed into the
        // page's ViewState (ViewStateUserKey), so a form that another website submits on the user's
        // behalf - which cannot know the token - is refused instead of being carried out.
        protected void Page_Init(object sender, EventArgs e)
        {
            HttpCookie cookie = Request.Cookies[TokenKey];
            Guid parsed;

            if (cookie != null && Guid.TryParse(cookie.Value, out parsed))
            {
                _token = cookie.Value;
            }
            else
            {
                _token = Guid.NewGuid().ToString("N");
                HttpCookie fresh = new HttpCookie(TokenKey, _token) { HttpOnly = true, SameSite = SameSiteMode.Lax };
                if (Request.IsSecureConnection) fresh.Secure = true;
                Response.Cookies.Set(fresh);
            }

            Page.ViewStateUserKey = _token;
            Page.PreLoad += VerifyToken;
        }

        private void VerifyToken(object sender, EventArgs e)
        {
            string user = Context.User.Identity.Name ?? "";

            if (!IsPostBack)
            {
                ViewState[TokenKey] = _token;
                ViewState[UserKey] = user;
            }
            else if ((string)ViewState[TokenKey] != _token || (string)ViewState[UserKey] != user)
            {
                throw new AntiForgeryException();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Choose which navigation to show. This only changes what the menu
            // displays - pages are protected separately by Web.config / server checks.
            // (The Admin role is attached to the login ticket in Phase 2.)
            bool isLoggedIn = Request.IsAuthenticated;
            bool isAdmin = isLoggedIn && Page.User.IsInRole("Admin");

            phGuestNav.Visible = !isLoggedIn;
            phGuestActions.Visible = !isLoggedIn;

            phStudentNav.Visible = isLoggedIn && !isAdmin;
            phAdminNav.Visible = isAdmin;
            phMemberActions.Visible = isLoggedIn;

            if (isLoggedIn)
            {
                string username = Page.User.Identity.Name;
                lblUserName.Text = Server.HtmlEncode(username);
                litUserInitial.Text = Server.HtmlEncode(username.Length > 0 ? username.Substring(0, 1) : "?");
            }
        }
    }
}
