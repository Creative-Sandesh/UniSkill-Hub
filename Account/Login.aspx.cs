using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace UniSkillHub.Account
{
    public partial class Login : Page
    {
        // Checked when the username does not exist, so "unknown user" takes as long as
        // "wrong password" and response time cannot be used to find out which usernames exist.
        private static readonly string DummySalt = PasswordHelper.GenerateSalt();
        private static readonly string DummyHash = PasswordHelper.HashPassword("not-a-real-password", DummySalt);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            if (Request.IsAuthenticated)
            {
                string role = User.IsInRole("Admin") ? "Admin" : "Student";

                if (Request.QueryString["ReturnUrl"] != null)
                {
                    // Forms Authentication sent a logged-in user here because their role
                    // is not allowed to open the requested page.
                    pnlLoginForm.Visible = false;
                    pnlAccessDenied.Visible = true;
                    lnkMyDashboard.HRef = Utility.DashboardUrlForRole(role);
                }
                else
                {
                    Response.Redirect(Utility.DashboardUrlForRole(role));
                }
                return;
            }

            // Friendly notices coming from other pages
            if (Request.QueryString["registered"] == "1")
            {
                ShowNotice("Registration successful! Please login with your new account.");
            }
            else if (Request.QueryString["loggedout"] == "1")
            {
                ShowNotice("You have been logged out.");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Never trust client-side validation alone.
            if (!Page.IsValid) return;

            string identifier = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string address = Request.UserHostAddress ?? "";

            int minutesLeft;
            if (LoginThrottle.IsLocked(address, identifier, out minutesLeft))
            {
                ShowError("Too many failed login attempts. Please wait " + minutesLeft +
                          (minutesLeft == 1 ? " minute" : " minutes") + " and try again.");
                return;
            }

            DataTable users;
            try
            {
                // Usernames cannot contain "@", so a value can only match one user.
                users = DBHelper.GetDataTable(
                    "SELECT UserID, Username, PasswordHash, PasswordSalt, Role, Status " +
                    "FROM Users WHERE Username = @Identifier OR Email = @Identifier",
                    DBHelper.Param("@Identifier", identifier));
            }
            catch (Exception ex)
            {
                Logger.Error("Login: could not read the user from the database", ex);
                ShowError("Sorry, we could not log you in right now. Please try again later.");
                return;
            }

            // Same message for "no such user" and "wrong password" so attackers
            // cannot discover which usernames exist.
            if (users.Rows.Count == 0)
            {
                PasswordHelper.VerifyPassword(password, DummyHash, DummySalt);
                LoginThrottle.RecordFailure(address, identifier);
                ShowError("Invalid username/email or password.");
                return;
            }

            DataRow user = users.Rows[0];

            if (!PasswordHelper.VerifyPassword(password, (string)user["PasswordHash"], (string)user["PasswordSalt"]))
            {
                LoginThrottle.RecordFailure(address, identifier);
                ShowError("Invalid username/email or password.");
                return;
            }

            LoginThrottle.Reset(address, identifier);

            if ((string)user["Status"] != "Active")
            {
                ShowError("Your account is inactive. Please contact the administrator.");
                return;
            }

            string username = (string)user["Username"];
            string role = (string)user["Role"];
            int userId = (int)user["UserID"];

            UpgradeOldHash(userId, password, (string)user["PasswordHash"]);

            SignIn(username, role, userId, chkRemember.Checked);

            // Go back to the page the user originally asked for (if it is safe),
            // otherwise to the dashboard of their role.
            string target = Utility.GetSafeReturnUrl(Request.QueryString["ReturnUrl"], role);
            Response.Redirect(target ?? Utility.DashboardUrlForRole(role));
        }

        // The password was just checked, so this is the one moment we can re-hash it with the current,
        // stronger settings. A failure here must not stop the login, so it is only logged.
        private static void UpgradeOldHash(int userId, string password, string storedHash)
        {
            if (!PasswordHelper.NeedsRehash(storedHash)) return;

            try
            {
                string salt = PasswordHelper.GenerateSalt();
                DBHelper.ExecuteNonQuery(
                    "UPDATE Users SET PasswordHash = @Hash, PasswordSalt = @Salt WHERE UserID = @UserID",
                    DBHelper.Param("@Hash", PasswordHelper.HashPassword(password, salt)),
                    DBHelper.Param("@Salt", salt),
                    DBHelper.Param("@UserID", userId));
            }
            catch (Exception ex)
            {
                Logger.Error("Login: could not upgrade the password hash of user " + userId, ex);
            }
        }

        // Creates the Forms Authentication cookie. The role and UserID are stored
        // (encrypted) inside the ticket as "Role|UserID|SessionId"; Global.asax reads them back.
        private void SignIn(string username, string role, int userId, bool persistent)
        {
            DateTime now = DateTime.Now;
            DateTime expires = now.AddMinutes(persistent ? 60 * 24 * 7 : 30);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, username, now, expires, persistent,
                role + "|" + userId + "|" + SessionRevocation.NewSessionId(), FormsAuthentication.FormsCookiePath);

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.HttpOnly = true;
            cookie.SameSite = SameSiteMode.Lax;   // not sent on cross-site form posts
            cookie.Secure = FormsAuthentication.RequireSSL;   // true once requireSSL is switched on (Web.Release.config)
            if (persistent) cookie.Expires = expires;

            Response.Cookies.Add(cookie);
        }

        private void ShowError(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.Visible = true;
        }

        private void ShowNotice(string message)
        {
            lblNotice.Text = Server.HtmlEncode(message);
            lblNotice.Visible = true;
        }
    }
}
