using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Student
{
    public partial class Dashboard : Page
    {
        // Assignments the student still has to hand in: published, not past due,
        // and no row for this student in Submissions.
        private const string PendingAssignmentsWhere =
            "FROM Assignments a " +
            "WHERE a.Status = 'Published' AND a.DueDate >= GETDATE() " +
            "AND NOT EXISTS (SELECT 1 FROM Submissions s WHERE s.AssignmentID = a.AssignmentID AND s.StudentID = @UserID) ";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks anyone who is not a Student from reaching this page.
            if (IsPostBack) return;

            int userId = Utility.CurrentUserId;

            LoadName(userId);
            LoadCounts(userId);
            LoadAnnouncements();
            LoadUpcomingAssignments(userId);
            LoadForumPosts();
        }

        private void LoadName(int userId)
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT FullName FROM Users WHERE UserID = @UserID",
                DBHelper.Param("@UserID", userId));

            if (dt.Rows.Count > 0)
            {
                lblFullName.Text = Server.HtmlEncode((string)dt.Rows[0]["FullName"]);
            }
        }

        private void LoadCounts(int userId)
        {
            lblResources.Text = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Resources WHERE Status = 'Published'").ToString();

            lblPending.Text = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) " + PendingAssignmentsWhere,
                DBHelper.Param("@UserID", userId)).ToString();

            lblSubmitted.Text = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Submissions WHERE StudentID = @UserID",
                DBHelper.Param("@UserID", userId)).ToString();

            lblAttempts.Text = DBHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM QuizAttempts WHERE StudentID = @UserID",
                DBHelper.Param("@UserID", userId)).ToString();
        }

        private void LoadAnnouncements()
        {
            // Published, already started and not expired.
            DataTable dt = DBHelper.GetDataTable(
                "SELECT TOP 3 Title, PublishDate, " +
                "       LEFT(Content, 120) + CASE WHEN LEN(Content) > 120 THEN '...' ELSE '' END AS Preview " +
                "FROM Announcements " +
                "WHERE Status = 'Published' AND PublishDate <= GETDATE() " +
                "AND (ExpiryDate IS NULL OR ExpiryDate >= GETDATE()) " +
                "ORDER BY PublishDate DESC");

            Bind(rptAnnouncements, pnlNoAnnouncements, dt);
        }

        private void LoadUpcomingAssignments(int userId)
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT TOP 4 a.Title, a.DueDate, a.MaxMarks " + PendingAssignmentsWhere +
                "ORDER BY a.DueDate",
                DBHelper.Param("@UserID", userId));

            Bind(rptAssignments, pnlNoAssignments, dt);
        }

        private void LoadForumPosts()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT TOP 4 p.Title, u.FullName, " +
                "       (SELECT COUNT(*) FROM ForumReplies r WHERE r.PostID = p.PostID AND r.Status = 'Active') AS Replies " +
                "FROM ForumPosts p INNER JOIN Users u ON u.UserID = p.UserID " +
                "WHERE p.Status = 'Active' " +
                "ORDER BY p.DatePosted DESC");

            Bind(rptForumPosts, pnlNoPosts, dt);
        }

        // Shows the list, or a friendly "nothing here" message when it is empty.
        private static void Bind(Repeater repeater, Panel emptyMessage, DataTable data)
        {
            repeater.DataSource = data;
            repeater.DataBind();
            repeater.Visible = data.Rows.Count > 0;
            emptyMessage.Visible = data.Rows.Count == 0;
        }

        protected void rptForumPosts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {

        }
    }
}
