using System;
using System.Data;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Forum
{
    public partial class PostDetails : Page
    {
        private int _postId;
        private int _postOwnerId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["id"], out _postId) || !LoadPost())
            {
                pnlPost.Visible = false;
                pnlNotFound.Visible = true;
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return;
            }

            // Everyone can read; only logged-in users can reply; only the author (or an Admin) can delete.
            pnlReplyForm.Visible = Request.IsAuthenticated;
            pnlLoginToReply.Visible = !Request.IsAuthenticated;
            pnlPostActions.Visible = CanDelete(_postOwnerId);

            if (!IsPostBack)
            {
                BindReplies();
            }
        }

        // ---------- loading ----------

        // Only Active posts are visible (moderated posts are "Hidden") - except that an Admin
        // can open a hidden post, to review it while moderating.
        private bool LoadPost()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT p.UserID, p.Title, p.Content, p.Category, p.DatePosted, p.Status, u.FullName, u.Role " +
                "FROM ForumPosts p INNER JOIN Users u ON u.UserID = p.UserID " +
                "WHERE p.PostID = @PostID AND (p.Status = 'Active' OR @IsAdmin = 1)",
                DBHelper.Param("@PostID", _postId),
                DBHelper.Param("@IsAdmin", User.IsInRole("Admin") ? 1 : 0));

            if (dt.Rows.Count == 0) return false;

            DataRow post = dt.Rows[0];
            _postOwnerId = (int)post["UserID"];

            if (!IsPostBack)
            {
                pnlHiddenNote.Visible = ((string)post["Status"] != "Active");
                string title = (string)post["Title"];
                Page.Title = title;
                litCrumb.Text = Server.HtmlEncode(title);
                litTitle.Text = Server.HtmlEncode(title);
                litCategory.Text = "<span class=\"type-badge type-link\">" + Server.HtmlEncode((string)post["Category"]) + "</span>";
                litAuthor.Text = Server.HtmlEncode((string)post["FullName"]);
                litAuthorBadge.Text = (string)post["Role"] == "Admin" ? "<span class=\"author-badge\">Lecturer</span>" : "";
                litDate.Text = ((DateTime)post["DatePosted"]).ToString("dd MMM yyyy, h:mm tt");
                litContent.Text = Utility.TextToHtml((string)post["Content"]);
            }
            return true;
        }

        private void BindReplies()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT r.ReplyID, r.UserID, r.ReplyText, r.DatePosted, u.FullName, u.Role " +
                "FROM ForumReplies r INNER JOIN Users u ON u.UserID = r.UserID " +
                "WHERE r.PostID = @PostID AND r.Status = 'Active' " +
                "ORDER BY r.DatePosted, r.ReplyID",
                DBHelper.Param("@PostID", _postId));

            rptReplies.DataSource = dt;
            rptReplies.DataBind();
            pnlNoReplies.Visible = dt.Rows.Count == 0;
            litReplyCount.Text = dt.Rows.Count == 0 ? "Replies" :
                dt.Rows.Count + (dt.Rows.Count == 1 ? " reply" : " replies");
        }

        // ---------- INSERT: a reply ----------

        protected void btnReply_Click(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
                return;
            }
            Page.Validate("ReplyGroup");
            if (!Page.IsValid) return;

            // The reply is only added if the discussion still exists and is visible.
            int added = DBHelper.ExecuteNonQuery(
                "INSERT INTO ForumReplies (PostID, UserID, ReplyText) " +
                "SELECT @PostID, @UserID, @ReplyText " +
                "WHERE EXISTS (SELECT 1 FROM ForumPosts WHERE PostID = @PostID AND Status = 'Active')",
                DBHelper.Param("@PostID", _postId),
                DBHelper.Param("@UserID", Utility.CurrentUserId),
                DBHelper.Param("@ReplyText", txtReply.Text.Trim()));

            if (added == 0)
            {
                ShowError("Sorry, this discussion is no longer available.");
                return;
            }

            Response.Redirect("~/Forum/PostDetails?id=" + _postId + "#replies");
        }

        // ---------- DELETE: a reply / the whole discussion ----------

        // The owner check is part of the SQL itself, so a crafted request cannot delete someone else's reply.
        protected void rptReplies_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "DeleteReply") return;

            if (!Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
                return;
            }

            int replyId;
            if (!int.TryParse((string)e.CommandArgument, out replyId)) return;

            DBHelper.ExecuteNonQuery(
                "DELETE FROM ForumReplies WHERE ReplyID = @ReplyID AND PostID = @PostID AND (UserID = @UserID OR @IsAdmin = 1)",
                DBHelper.Param("@ReplyID", replyId),
                DBHelper.Param("@PostID", _postId),
                DBHelper.Param("@UserID", Utility.CurrentUserId),
                DBHelper.Param("@IsAdmin", User.IsInRole("Admin") ? 1 : 0));

            Response.Redirect("~/Forum/PostDetails?id=" + _postId + "#replies");
        }

        protected void btnDeletePost_Click(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
                return;
            }

            // Deleting the post also deletes its replies (ON DELETE CASCADE in the database).
            int deleted = DBHelper.ExecuteNonQuery(
                "DELETE FROM ForumPosts WHERE PostID = @PostID AND (UserID = @UserID OR @IsAdmin = 1)",
                DBHelper.Param("@PostID", _postId),
                DBHelper.Param("@UserID", Utility.CurrentUserId),
                DBHelper.Param("@IsAdmin", User.IsInRole("Admin") ? 1 : 0));

            if (deleted == 1)
            {
                Response.Redirect("~/Forum/Forum?deleted=1");
            }
            else
            {
                ShowError("You are not allowed to delete this discussion.");
            }
        }

        // ---------- helpers used by the page markup ----------

        protected bool CanDelete(object ownerId)
        {
            return Request.IsAuthenticated && ((int)ownerId == Utility.CurrentUserId || User.IsInRole("Admin"));
        }

        protected string ReplyHtml(object text)
        {
            return Utility.TextToHtml((string)text);
        }

        private void ShowError(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.Visible = true;
        }
    }
}
