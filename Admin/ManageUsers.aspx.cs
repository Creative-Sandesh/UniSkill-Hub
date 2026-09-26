using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class ManageUsers : Page
    {
        private const int PageSize = 10;

        // Messages shown after an action (the address only carries a short code, never text).
        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "created",     "The user has been created." },
            { "updated",     "The user has been updated." },
            { "deleted",     "The user has been deleted." },
            { "activated",   "The user has been activated." },
            { "deactivated", "The user has been deactivated and can no longer log in." }
        };

        // The user being edited (0 = adding a new user). Kept in ViewState between postbacks.
        private int EditingUserId
        {
            get { return ViewState["EditUserId"] is int ? (int)ViewState["EditUserId"] : 0; }
            set { ViewState["EditUserId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            if (IsPostBack) return;

            // Search, filters and page number are in the address, so results can be bookmarked.
            txtSearch.Text = Limit((Request.QueryString["q"] ?? "").Trim(), 100);
            SelectIfExists(ddlRoleFilter, Request.QueryString["role"]);
            SelectIfExists(ddlStatusFilter, Request.QueryString["status"]);

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);

            BindGrid(page);
        }

        // ---------- READ: the users list (SELECT with search, filters and paging) ----------

        private void BindGrid(int page)
        {
            string search = txtSearch.Text;
            object searchParam = null;
            if (search.Length > 0)
            {
                // make % _ [ harmless inside LIKE
                searchParam = "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
            }
            object roleParam = string.IsNullOrEmpty(ddlRoleFilter.SelectedValue) ? null : ddlRoleFilter.SelectedValue;
            object statusParam = string.IsNullOrEmpty(ddlStatusFilter.SelectedValue) ? null : ddlStatusFilter.SelectedValue;

            const string where =
                "FROM Users " +
                "WHERE (@Search IS NULL OR FullName LIKE @Search OR Username LIKE @Search OR Email LIKE @Search) " +
                "AND (@Role IS NULL OR Role = @Role) AND (@Status IS NULL OR Status = @Status) ";

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where,
                DBHelper.Param("@Search", searchParam), DBHelper.Param("@Role", roleParam), DBHelper.Param("@Status", statusParam));

            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            gvUsers.DataSource = DBHelper.GetDataTable(
                "SELECT UserID, FullName, Username, Email, Role, Status, DateRegistered " + where +
                "ORDER BY DateRegistered DESC, UserID DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY",
                DBHelper.Param("@Search", searchParam), DBHelper.Param("@Role", roleParam), DBHelper.Param("@Status", statusParam),
                DBHelper.Param("@Skip", (page - 1) * PageSize), DBHelper.Param("@Take", PageSize));
            gvUsers.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " user" : " users");
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "User pages");
        }

        // Proper <thead> so the table header gets its style. Done just before the page is drawn
        // (not in DataBound) so it also works after a postback that does not re-bind the grid.
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvUsers.HeaderRow != null) gvUsers.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(ListUrl(1, null));
        }

        // The list address for the current search/filters (used by the pager and after every action).
        private string ListUrl(int page, string message)
        {
            StringBuilder url = new StringBuilder("~/Admin/ManageUsers?page=" + page);
            if (txtSearch.Text.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(txtSearch.Text));
            if (ddlRoleFilter.SelectedValue.Length > 0) url.Append("&role=" + ddlRoleFilter.SelectedValue);
            if (ddlStatusFilter.SelectedValue.Length > 0) url.Append("&status=" + ddlStatusFilter.SelectedValue);
            if (message != null) url.Append("&msg=" + message);
            return url.ToString();
        }

        private int CurrentPage()
        {
            int page;
            return (int.TryParse(Request.QueryString["page"], out page) && page > 0) ? page : 1;
        }

        // ---------- row buttons: Edit / Activate-Deactivate / Delete ----------

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out userId)) return;

            switch (e.CommandName)
            {
                case "EditUser":
                    ShowForm(userId);
                    break;
                case "ToggleStatus":
                    ToggleStatus(userId);
                    break;
                case "DeleteUser":
                    DeleteUser(userId);
                    break;
            }
        }

        // UPDATE: switch between Active and Inactive. You can never change your own account here.
        private void ToggleStatus(int userId)
        {
            if (userId == Utility.CurrentUserId)
            {
                ShowMessage("You cannot deactivate your own account.", false);
                return;
            }

            DataTable dt = DBHelper.GetDataTable("SELECT Status FROM Users WHERE UserID = @UserID", DBHelper.Param("@UserID", userId));
            if (dt.Rows.Count == 0) { ShowMessage("That user no longer exists.", false); return; }

            bool wasActive = (string)dt.Rows[0]["Status"] == "Active";

            DBHelper.ExecuteNonQuery(
                "UPDATE Users SET Status = @NewStatus WHERE UserID = @UserID AND UserID <> @Me",
                DBHelper.Param("@NewStatus", wasActive ? "Inactive" : "Active"),
                DBHelper.Param("@UserID", userId),
                DBHelper.Param("@Me", Utility.CurrentUserId));

            AccountCheck.Forget(userId);   // so a deactivated user is signed out on their very next request

            Response.Redirect(ListUrl(CurrentPage(), wasActive ? "deactivated" : "activated"));
        }

        // DELETE: only possible for a user without any records. Otherwise the database refuses
        // (foreign keys protect submissions, quiz attempts, posts...) and we suggest deactivating.
        private void DeleteUser(int userId)
        {
            if (userId == Utility.CurrentUserId)
            {
                ShowMessage("You cannot delete your own account.", false);
                return;
            }

            try
            {
                int rows = DBHelper.ExecuteNonQuery(
                    "DELETE FROM Users WHERE UserID = @UserID AND UserID <> @Me",
                    DBHelper.Param("@UserID", userId),
                    DBHelper.Param("@Me", Utility.CurrentUserId));

                if (rows == 0) { ShowMessage("That user no longer exists.", false); return; }

                AccountCheck.Forget(userId);
            }
            catch (SqlException ex)
            {
                if (ex.Number != 547) Logger.Error("ManageUsers: could not delete user " + userId, ex);
                // 547 = foreign key conflict: this user has related records
                ShowMessage(ex.Number == 547
                    ? "This user has activity (submissions, quiz attempts, posts or content), so it cannot be deleted. Deactivate the account instead."
                    : "Sorry, the user could not be deleted right now.", false);
                return;
            }

            Response.Redirect(ListUrl(CurrentPage(), "deleted"));
        }

        // ---------- the Add / Edit form ----------

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            ShowForm(0);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            EditingUserId = 0;
        }

        private void ShowForm(int userId)
        {
            EditingUserId = userId;
            pnlForm.Visible = true;
            lblSelfNote.Visible = false;
            ddlRole.Enabled = true;
            ddlStatus.Enabled = true;
            txtPassword.Text = "";

            if (userId == 0)
            {
                lblFormTitle.Text = "Add user";
                txtFullName.Text = txtUsername.Text = txtEmail.Text = "";
                txtUsername.ReadOnly = false;
                lblUsernameHelp.Text = "3-30 letters, numbers or underscores.";
                lblPasswordHelp.Text = "At least 8 characters.";
                ddlRole.SelectedValue = "Student";
                ddlStatus.SelectedValue = "Active";
                return;
            }

            DataTable dt = DBHelper.GetDataTable(
                "SELECT FullName, Username, Email, Role, Status FROM Users WHERE UserID = @UserID",
                DBHelper.Param("@UserID", userId));

            if (dt.Rows.Count == 0)
            {
                pnlForm.Visible = false;
                EditingUserId = 0;
                ShowMessage("That user no longer exists.", false);
                return;
            }

            DataRow user = dt.Rows[0];
            lblFormTitle.Text = "Edit user";
            txtFullName.Text = (string)user["FullName"];
            txtUsername.Text = (string)user["Username"];
            txtUsername.ReadOnly = true;
            lblUsernameHelp.Text = "A username cannot be changed.";
            txtEmail.Text = (string)user["Email"];
            ddlRole.SelectedValue = (string)user["Role"];
            ddlStatus.SelectedValue = (string)user["Status"];
            lblPasswordHelp.Text = "Leave blank to keep the current password, or type a new one to reset it.";

            if (userId == Utility.CurrentUserId)
            {
                ddlRole.Enabled = false;
                ddlStatus.Enabled = false;
                lblSelfNote.Visible = true;
            }
        }

        // ---------- server-side validation ----------

        protected void cvUsername_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // An existing user's username is read-only, so only a NEW username needs the check.
            args.IsValid = (EditingUserId != 0) || !Exists("Username = @Value", args.Value.Trim(), 0);
        }

        protected void cvEmail_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !Exists("Email = @Value", args.Value.Trim(), EditingUserId);
        }

        // A new user needs a password; when editing, a blank password means "keep the old one".
        protected void cvPassword_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string password = args.Value ?? "";
            if (password.Length == 0)
            {
                args.IsValid = (EditingUserId != 0);
            }
            else
            {
                args.IsValid = password.Length >= 8 && password.Length <= 64;
            }
        }

        // "condition" is always a fixed fragment from this class; the value is a parameter.
        private static bool Exists(string condition, string value, int exceptUserId)
        {
            return (int)DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE " + condition + " AND UserID <> @Except",
                DBHelper.Param("@Value", value), DBHelper.Param("@Except", exceptUserId)) > 0;
        }

        // ---------- INSERT / UPDATE ----------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("UserForm");
            if (!Page.IsValid) return;

            int userId = EditingUserId;
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();

            try
            {
                if (userId == 0)
                {
                    string salt = PasswordHelper.GenerateSalt();
                    DBHelper.ExecuteNonQuery(
                        "INSERT INTO Users (FullName, Username, Email, PasswordHash, PasswordSalt, Role, Status) " +
                        "VALUES (@FullName, @Username, @Email, @Hash, @Salt, @Role, @Status)",
                        DBHelper.Param("@FullName", fullName),
                        DBHelper.Param("@Username", txtUsername.Text.Trim()),
                        DBHelper.Param("@Email", email),
                        DBHelper.Param("@Hash", PasswordHelper.HashPassword(txtPassword.Text, salt)),
                        DBHelper.Param("@Salt", salt),
                        DBHelper.Param("@Role", ddlRole.SelectedValue),
                        DBHelper.Param("@Status", ddlStatus.SelectedValue));

                    Response.Redirect(ListUrl(1, "created"));
                    return;
                }

                UpdateUser(userId, fullName, email);
                Response.Redirect(ListUrl(CurrentPage(), "updated"));
            }
            catch (SqlException ex)
            {
                bool duplicate = (ex.Number == 2627 || ex.Number == 2601);
                if (!duplicate) Logger.Error("ManageUsers: could not save the user", ex);
                ShowMessage(duplicate ? "That username or email is already in use."
                                      : "Sorry, the user could not be saved right now.", false);
            }
        }

        private void UpdateUser(int userId, string fullName, string email)
        {
            List<string> sets = new List<string> { "FullName = @FullName", "Email = @Email" };
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                DBHelper.Param("@FullName", fullName),
                DBHelper.Param("@Email", email),
                DBHelper.Param("@UserID", userId)
            };

            // Role and status can be changed for other people, never for your own account.
            if (userId != Utility.CurrentUserId)
            {
                sets.Add("Role = @Role");
                sets.Add("Status = @Status");
                parameters.Add(DBHelper.Param("@Role", ddlRole.SelectedValue));
                parameters.Add(DBHelper.Param("@Status", ddlStatus.SelectedValue));
            }

            // Only when a new password was typed: new salt + new hash.
            if (txtPassword.Text.Length > 0)
            {
                string salt = PasswordHelper.GenerateSalt();
                sets.Add("PasswordHash = @Hash");
                sets.Add("PasswordSalt = @Salt");
                parameters.Add(DBHelper.Param("@Hash", PasswordHelper.HashPassword(txtPassword.Text, salt)));
                parameters.Add(DBHelper.Param("@Salt", salt));
            }

            // the column names come from this method only, never from the user
            DBHelper.ExecuteNonQuery("UPDATE Users SET " + string.Join(", ", sets) + " WHERE UserID = @UserID", parameters.ToArray());

            AccountCheck.Forget(userId);   // a changed role or status applies on the user's very next request
        }

        // ---------- helpers used by the page markup ----------

        protected bool IsAnotherUser(object userId)
        {
            return (int)userId != Utility.CurrentUserId;
        }

        protected string StatusClass(object status)
        {
            return "status-badge status-" + Convert.ToString(status).ToLowerInvariant();   // status-active / status-inactive
        }

        protected string RoleClass(object role)
        {
            return "status-badge role-" + Convert.ToString(role).ToLowerInvariant();       // role-admin / role-student
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }

        private static string Limit(string text, int max)
        {
            return text.Length > max ? text.Substring(0, max) : text;
        }

        private static void SelectIfExists(DropDownList list, string value)
        {
            ListItem item = list.Items.FindByValue(value ?? "");
            if (item != null) list.SelectedValue = item.Value;
        }
    }
}
