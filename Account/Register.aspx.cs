using System;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Account
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // A logged-in user has no reason to register again.
            if (!IsPostBack && Request.IsAuthenticated)
            {
                Response.Redirect(Utility.DashboardUrlForRole(User.IsInRole("Admin") ? "Admin" : "Student"));
            }
        }

        // Server-side check: is the username already used?
        protected void cvUsername_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !ValueExists("Username", args.Value.Trim());
        }

        // Server-side check: is the email already used?
        protected void cvEmail_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !ValueExists("Email", args.Value.Trim());
        }

        // "column" is always a fixed name from this class (never user input);
        // the value itself is passed as a parameter.
        private bool ValueExists(string column, string value)
        {
            try
            {
                object count = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Users WHERE " + column + " = @Value",
                    DBHelper.Param("@Value", value));
                return (int)count > 0;
            }
            catch (Exception ex)
            {
                Logger.Error("Register: could not check whether the " + column + " is already used", ex);
                // If the database cannot be reached, skip this check: the INSERT in
                // btnRegister_Click will fail too and show a friendly message.
                return false;
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // Never trust client-side validation alone: this re-runs all validators on the server.
            if (!Page.IsValid) return;

            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();

            try
            {
                // Salt + hash the password. The plain password is never stored.
                string salt = PasswordHelper.GenerateSalt();
                string hash = PasswordHelper.HashPassword(txtPassword.Text, salt);

                // INSERT: the role is fixed to "Student" here - it can never come from the form.
                DBHelper.ExecuteNonQuery(
                    "INSERT INTO Users (FullName, Username, Email, PasswordHash, PasswordSalt, Role) " +
                    "VALUES (@FullName, @Username, @Email, @PasswordHash, @PasswordSalt, 'Student')",
                    DBHelper.Param("@FullName", fullName),
                    DBHelper.Param("@Username", username),
                    DBHelper.Param("@Email", email),
                    DBHelper.Param("@PasswordHash", hash),
                    DBHelper.Param("@PasswordSalt", salt));
            }
            catch (SqlException ex)
            {
                // 2627 / 2601 = unique constraint violated (someone registered the same
                // username/email between our check and the insert).
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    ShowError("That username or email is already registered.");
                }
                else
                {
                    Logger.Error("Register: could not create the account", ex);
                    ShowError("Sorry, we could not create your account right now. Please try again later.");
                }
                return;
            }
            catch (Exception ex)
            {
                Logger.Error("Register: could not create the account", ex);
                ShowError("Sorry, we could not create your account right now. Please try again later.");
                return;
            }

            // Registration successful -> go to the login page.
            Response.Redirect("~/Account/Login?registered=1");
        }

        private void ShowError(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.Visible = true;
        }
    }
}
