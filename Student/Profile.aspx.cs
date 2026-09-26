using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Student
{
    public partial class Profile : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks anyone who is not a Student from reaching this page.
            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        // SELECT the logged-in user's own row (the UserID comes from the login ticket,
        // never from the URL or the form, so a student can only see/edit their own profile).
        private void LoadProfile()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT Username, FullName, Email, DateRegistered FROM Users WHERE UserID = @UserID",
                DBHelper.Param("@UserID", Utility.CurrentUserId));

            if (dt.Rows.Count == 0) return;

            DataRow user = dt.Rows[0];
            txtUsername.Text = (string)user["Username"];
            txtFullName.Text = (string)user["FullName"];
            txtEmail.Text = (string)user["Email"];
            lblMemberSince.Text = ((DateTime)user["DateRegistered"]).ToString("dd MMMM yyyy");
        }

        // Server-side check: the email must not belong to a DIFFERENT user.
        protected void cvEmail_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {
                object count = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserID <> @UserID",
                    DBHelper.Param("@Email", args.Value.Trim()),
                    DBHelper.Param("@UserID", Utility.CurrentUserId));
                args.IsValid = ((int)count == 0);
            }
            catch (Exception ex)
            {
                Logger.Error("Profile: could not check whether the email is already used", ex);
                args.IsValid = true;   // the UPDATE below will fail and show a friendly message
            }
        }

        // UPDATE: full name and email
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            Page.Validate("ProfileGroup");
            if (!Page.IsValid) return;

            try
            {
                DBHelper.ExecuteNonQuery(
                    "UPDATE Users SET FullName = @FullName, Email = @Email WHERE UserID = @UserID",
                    DBHelper.Param("@FullName", txtFullName.Text.Trim()),
                    DBHelper.Param("@Email", txtEmail.Text.Trim()),
                    DBHelper.Param("@UserID", Utility.CurrentUserId));

                ShowMessage(lblProfileMessage, "Your profile has been updated.", true);
            }
            catch (SqlException ex)
            {
                bool duplicate = (ex.Number == 2627 || ex.Number == 2601);
                if (!duplicate) Logger.Error("Profile: could not save the profile", ex);
                ShowMessage(lblProfileMessage,
                    duplicate ? "Another account already uses that email."
                              : "Sorry, we could not save your changes right now. Please try again later.", false);
            }
            catch (Exception ex)
            {
                Logger.Error("Profile: could not save the profile", ex);
                ShowMessage(lblProfileMessage, "Sorry, we could not save your changes right now. Please try again later.", false);
            }
        }

        // UPDATE: password (needs the current password, then a NEW salt and hash)
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            Page.Validate("PasswordGroup");
            if (!Page.IsValid) return;

            try
            {
                DataTable dt = DBHelper.GetDataTable(
                    "SELECT PasswordHash, PasswordSalt FROM Users WHERE UserID = @UserID",
                    DBHelper.Param("@UserID", Utility.CurrentUserId));

                if (dt.Rows.Count == 0) return;

                bool currentIsCorrect = PasswordHelper.VerifyPassword(
                    txtCurrentPassword.Text, (string)dt.Rows[0]["PasswordHash"], (string)dt.Rows[0]["PasswordSalt"]);

                if (!currentIsCorrect)
                {
                    ShowMessage(lblPasswordMessage, "Your current password is incorrect.", false);
                    return;
                }

                string newSalt = PasswordHelper.GenerateSalt();
                string newHash = PasswordHelper.HashPassword(txtNewPassword.Text, newSalt);

                DBHelper.ExecuteNonQuery(
                    "UPDATE Users SET PasswordHash = @Hash, PasswordSalt = @Salt WHERE UserID = @UserID",
                    DBHelper.Param("@Hash", newHash),
                    DBHelper.Param("@Salt", newSalt),
                    DBHelper.Param("@UserID", Utility.CurrentUserId));

                ShowMessage(lblPasswordMessage, "Your password has been changed.", true);
            }
            catch (Exception ex)
            {
                Logger.Error("Profile: could not change the password", ex);
                ShowMessage(lblPasswordMessage, "Sorry, we could not change your password right now. Please try again later.", false);
            }
        }

        private void ShowMessage(Label label, string message, bool success)
        {
            label.Text = Server.HtmlEncode(message);
            label.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            label.Visible = true;
        }
    }
}
