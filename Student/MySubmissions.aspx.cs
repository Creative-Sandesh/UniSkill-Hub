using System;
using System.Data;
using System.Web;
using System.Web.UI;

namespace UniSkillHub.Student
{
    public partial class MySubmissions : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            pnlSubmitted.Visible = (Request.QueryString["submitted"] == "1");

            // Every published assignment with THIS student's submission (if any).
            // The join is limited to s.StudentID = the logged-in user, so nobody else's
            // work, marks or feedback can ever appear here.
            DataTable dt = DBHelper.GetDataTable(
                "SELECT a.AssignmentID, a.Title, a.DueDate, a.MaxMarks, " +
                "       s.SubmissionID, s.SubmittedDate, s.Marks, s.Feedback, " +
                "       CASE WHEN s.SubmissionID IS NOT NULL THEN s.Status " +
                "            WHEN a.DueDate < GETDATE() THEN 'Overdue' ELSE 'Pending' END AS MyStatus " +
                "FROM Assignments a " +
                "LEFT JOIN Submissions s ON s.AssignmentID = a.AssignmentID AND s.StudentID = @UserID " +
                "WHERE a.Status = 'Published' " +
                "ORDER BY CASE WHEN s.SubmissionID IS NULL THEN 1 ELSE 0 END, s.SubmittedDate DESC, a.DueDate DESC",
                DBHelper.Param("@UserID", Utility.CurrentUserId));

            rptSubmissions.DataSource = dt;
            rptSubmissions.DataBind();
            rptSubmissions.Visible = dt.Rows.Count > 0;
            pnlEmpty.Visible = dt.Rows.Count == 0;

            int submitted = dt.Select("SubmissionID IS NOT NULL").Length;
            int graded = dt.Select("MyStatus = 'Graded'").Length;
            lblSummary.Text = dt.Rows.Count == 0 ? "" :
                submitted + " of " + dt.Rows.Count + " submitted · " + graded + " graded";
        }

        // ---------- helpers used by the page markup ----------

        protected string DetailsUrl(object assignmentId)
        {
            return ResolveUrl("~/Student/AssignmentDetails?id=" + assignmentId);
        }

        protected string SubmitUrl(object assignmentId)
        {
            return ResolveUrl("~/Student/SubmitAssignment?id=" + assignmentId);
        }

        protected string FileUrl(object submissionId)
        {
            return ResolveUrl("~/Assignments/Download.ashx?type=submission&id=" + submissionId);
        }

        protected string StatusClass(object status)
        {
            return Utility.StatusBadgeClass(status);
        }

        protected string SubmittedText(object date)
        {
            return (date is DBNull) ? "&mdash;" : HttpUtility.HtmlEncode(((DateTime)date).ToString("dd MMM yyyy, h:mm tt"));
        }

        protected string MarksText(object marks, object maxMarks)
        {
            return (marks is DBNull) ? "&mdash;" : HttpUtility.HtmlEncode(string.Format("{0:0.##} / {1}", marks, maxMarks));
        }

        protected string FeedbackText(object feedback)
        {
            string text = feedback as string;
            if (string.IsNullOrWhiteSpace(text)) return "&mdash;";

            string shortText = text.Length > 90 ? text.Substring(0, 90) + "..." : text;
            return HttpUtility.HtmlEncode(shortText);
        }
    }
}
