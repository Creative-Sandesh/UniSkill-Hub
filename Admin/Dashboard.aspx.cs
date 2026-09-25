using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks anyone who is not an Admin from reaching this page.
            if (IsPostBack) return;

            LoadName();
            LoadStatistics();
            LoadRecentRegistrations();
            LoadRecentSubmissions();
            LoadRecentForumActivity();
        }

        private void LoadName()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT FullName FROM Users WHERE UserID = @UserID",
                DBHelper.Param("@UserID", Utility.CurrentUserId));

            if (dt.Rows.Count > 0)
            {
                lblFullName.Text = Server.HtmlEncode((string)dt.Rows[0]["FullName"]);
            }
        }

        // All the numbers come back from ONE query (one database round trip).
        private void LoadStatistics()
        {
            DataRow s = DBHelper.GetDataTable(
                "SELECT " +
                "  (SELECT COUNT(*) FROM Users WHERE Role = 'Student') AS Students, " +
                "  (SELECT COUNT(*) FROM Users WHERE Role = 'Student' AND Status = 'Active') AS ActiveStudents, " +
                "  (SELECT COUNT(*) FROM Resources) AS Resources, " +
                "  (SELECT COUNT(*) FROM Resources WHERE Status = 'Published') AS PublishedResources, " +
                "  (SELECT COUNT(*) FROM Assignments) AS Assignments, " +
                "  (SELECT COUNT(*) FROM Assignments WHERE Status = 'Published' AND DueDate >= GETDATE()) AS OpenAssignments, " +
                "  (SELECT COUNT(*) FROM Submissions) AS Submissions, " +
                "  (SELECT COUNT(*) FROM Submissions WHERE Status = 'Submitted') AS AwaitingGrading, " +
                "  (SELECT COUNT(*) FROM Quizzes) AS Quizzes, " +
                "  (SELECT COUNT(*) FROM Quizzes WHERE Status = 'Published') AS PublishedQuizzes, " +
                "  (SELECT COUNT(*) FROM Announcements) AS Announcements, " +
                "  (SELECT COUNT(*) FROM Announcements WHERE Status = 'Published' AND PublishDate <= GETDATE() " +
                "      AND (ExpiryDate IS NULL OR ExpiryDate >= GETDATE())) AS LiveAnnouncements, " +
                "  (SELECT COUNT(*) FROM ForumPosts) AS ForumPosts, " +
                "  (SELECT COUNT(*) FROM ForumReplies) AS ForumReplies").Rows[0];

            lblStudents.Text = s["Students"].ToString();
            lblStudentsSub.Text = s["ActiveStudents"] + " active";
            lblResources.Text = s["Resources"].ToString();
            lblResourcesSub.Text = s["PublishedResources"] + " published";
            lblAssignments.Text = s["Assignments"].ToString();
            lblAssignmentsSub.Text = s["OpenAssignments"] + " open now";
            lblSubmissions.Text = s["Submissions"].ToString();
            lblSubmissionsSub.Text = s["AwaitingGrading"] + " awaiting grading";
            lblQuizzes.Text = s["Quizzes"].ToString();
            lblQuizzesSub.Text = s["PublishedQuizzes"] + " published";
            lblAnnouncements.Text = s["Announcements"].ToString();
            lblAnnouncementsSub.Text = s["LiveAnnouncements"] + " visible now";
            lblForumPosts.Text = s["ForumPosts"].ToString();
            lblForumPostsSub.Text = s["ForumReplies"] + " replies";
        }

        private void LoadRecentRegistrations()
        {
            Bind(rptRegistrations, pnlNoRegistrations, DBHelper.GetDataTable(
                "SELECT TOP 5 FullName, Username, Role, DateRegistered FROM Users ORDER BY DateRegistered DESC, UserID DESC"));
        }

        private void LoadRecentSubmissions()
        {
            Bind(rptSubmissions, pnlNoSubmissions, DBHelper.GetDataTable(
                "SELECT TOP 5 a.Title, u.FullName, s.SubmittedDate, s.Status " +
                "FROM Submissions s " +
                "INNER JOIN Assignments a ON a.AssignmentID = s.AssignmentID " +
                "INNER JOIN Users u ON u.UserID = s.StudentID " +
                "ORDER BY s.SubmittedDate DESC, s.SubmissionID DESC"));
        }

        // New posts and new replies merged into one "latest first" list.
        private void LoadRecentForumActivity()
        {
            Bind(rptForum, pnlNoForum, DBHelper.GetDataTable(
                "SELECT TOP 5 * FROM ( " +
                "  SELECT p.PostID, u.FullName, p.Title, 'started' AS Action, p.DatePosted AS ActivityDate " +
                "  FROM ForumPosts p INNER JOIN Users u ON u.UserID = p.UserID " +
                "  UNION ALL " +
                "  SELECT r.PostID, u.FullName, p.Title, 'replied to' AS Action, r.DatePosted AS ActivityDate " +
                "  FROM ForumReplies r INNER JOIN Users u ON u.UserID = r.UserID " +
                "  INNER JOIN ForumPosts p ON p.PostID = r.PostID " +
                ") x ORDER BY ActivityDate DESC"));
        }

        private static void Bind(Repeater repeater, Panel emptyMessage, DataTable data)
        {
            repeater.DataSource = data;
            repeater.DataBind();
            repeater.Visible = data.Rows.Count > 0;
            emptyMessage.Visible = data.Rows.Count == 0;
        }

        protected string StatusClass(object status)
        {
            return Utility.StatusBadgeClass(status);
        }

        protected string PostUrl(object postId)
        {
            return ResolveUrl("~/Forum/PostDetails?id=" + postId);
        }
    }
}
