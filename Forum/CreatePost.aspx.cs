using System;
using System.Data;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Forum
{
    public partial class CreatePost : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Forum/Web.config already sends guests to the login page; this is a second safety check.
            if (!Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
                return;
            }

            if (!IsPostBack)
            {
                ddlCategory.Items.Add(new ListItem("General", "General"));
                foreach (DataRow row in DBHelper.GetDataTable("SELECT CategoryName FROM Categories ORDER BY CategoryName").Rows)
                {
                    string name = (string)row["CategoryName"];
                    ddlCategory.Items.Add(new ListItem(name, name));
                }
            }
        }

        // INSERT: a new discussion. The author is always the logged-in user (from the login ticket).
        protected void btnPost_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int newPostId = (int)(decimal)DBHelper.ExecuteScalar(
                    "INSERT INTO ForumPosts (UserID, Title, Content, Category) " +
                    "VALUES (@UserID, @Title, @Content, @Category); " +
                    "SELECT SCOPE_IDENTITY();",
                    DBHelper.Param("@UserID", Utility.CurrentUserId),
                    DBHelper.Param("@Title", txtTitle.Text.Trim()),
                    DBHelper.Param("@Content", txtContent.Text.Trim()),
                    DBHelper.Param("@Category", ddlCategory.SelectedValue));

                Response.Redirect("~/Forum/PostDetails?id=" + newPostId);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;   // normal behaviour of Response.Redirect
            }
            catch (Exception ex)
            {
                Logger.Error("CreatePost: could not save the discussion", ex);
                lblMessage.Text = "Sorry, we could not post your discussion right now. Please try again later.";
                lblMessage.Visible = true;
            }
        }
    }
}
