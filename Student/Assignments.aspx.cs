using System;
using System.Data;
using System.Web.UI;

namespace UniSkillHub.Student
{
    public partial class Assignments : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            // One row per published assignment, joined with THIS student's submission (if any).
            // MyStatus: Graded / Submitted come from the submission; otherwise Overdue or Pending.
            // Pending work is listed first, soonest deadline first.
            DataTable dt = DBHelper.GetDataTable(
                "SELECT * FROM ( " +
                "  SELECT a.AssignmentID, a.Title, a.DueDate, a.MaxMarks, " +
                "         CASE WHEN s.SubmissionID IS NOT NULL THEN s.Status " +
                "              WHEN a.DueDate < GETDATE() THEN 'Overdue' ELSE 'Pending' END AS MyStatus " +
                "  FROM Assignments a " +
                "  LEFT JOIN Submissions s ON s.AssignmentID = a.AssignmentID AND s.StudentID = @UserID " +
                "  WHERE a.Status = 'Published' " +
                ") x " +
                "ORDER BY CASE WHEN MyStatus = 'Pending' THEN 0 ELSE 1 END, " +
                "         CASE WHEN MyStatus = 'Pending' THEN DueDate END ASC, DueDate DESC",
                DBHelper.Param("@UserID", Utility.CurrentUserId));

            rptAssignments.DataSource = dt;
            rptAssignments.DataBind();
            rptAssignments.Visible = dt.Rows.Count > 0;
            pnlEmpty.Visible = dt.Rows.Count == 0;

            int pending = dt.Select("MyStatus = 'Pending'").Length;
            lblSummary.Text = dt.Rows.Count == 0 ? "" :
                dt.Rows.Count + (dt.Rows.Count == 1 ? " assignment" : " assignments") + " · " + pending + " pending";
        }

        protected string DetailsUrl(object assignmentId)
        {
            return ResolveUrl("~/Student/AssignmentDetails?id=" + assignmentId);
        }

        protected string SubmitUrl(object assignmentId)
        {
            return ResolveUrl("~/Student/SubmitAssignment?id=" + assignmentId);
        }

        protected string StatusClass(object status)
        {
            return Utility.StatusBadgeClass(status);
        }

        protected string DueText(object dueDate)
        {
            return Utility.FriendlyDueText((DateTime)dueDate);
        }
    }
}
