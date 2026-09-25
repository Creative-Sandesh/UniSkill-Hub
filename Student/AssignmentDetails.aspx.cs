using System;
using System.Data;
using System.Web.UI;

namespace UniSkillHub.Student
{
    public partial class AssignmentDetails : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            int assignmentId;
            if (!int.TryParse(Request.QueryString["id"], out assignmentId))
            {
                ShowNotFound();
                return;
            }

            // The assignment (published only) plus THIS student's submission, if any.
            DataTable dt = DBHelper.GetDataTable(
                "SELECT a.AssignmentID, a.Title, a.Description, a.Instructions, a.DueDate, a.MaxMarks, a.FilePath, " +
                "       CASE WHEN a.DueDate < GETDATE() THEN 1 ELSE 0 END AS IsPastDue, " +
                "       s.SubmissionID, s.SubmittedDate, s.Status AS SubStatus, s.Marks, s.Feedback, s.Comment " +
                "FROM Assignments a " +
                "LEFT JOIN Submissions s ON s.AssignmentID = a.AssignmentID AND s.StudentID = @UserID " +
                "WHERE a.AssignmentID = @AssignmentID AND a.Status = 'Published'",
                DBHelper.Param("@UserID", Utility.CurrentUserId),
                DBHelper.Param("@AssignmentID", assignmentId));

            if (dt.Rows.Count == 0)
            {
                ShowNotFound();
                return;
            }

            DataRow row = dt.Rows[0];
            string title = (string)row["Title"];
            bool hasSubmission = !(row["SubmissionID"] is DBNull);
            bool isPastDue = (int)row["IsPastDue"] == 1;
            string subStatus = hasSubmission ? (string)row["SubStatus"] : null;
            string myStatus = hasSubmission ? subStatus : (isPastDue ? "Overdue" : "Pending");

            Page.Title = title;
            litCrumb.Text = Server.HtmlEncode(title);
            litTitle.Text = Server.HtmlEncode(title);
            litStatusBadge.Text = "<span class=\"" + Utility.StatusBadgeClass(myStatus) + "\">" + myStatus + "</span>";
            litDue.Text = ((DateTime)row["DueDate"]).ToString("dd MMM yyyy, h:mm tt") +
                          (myStatus == "Pending" ? " (" + Utility.FriendlyDueText((DateTime)row["DueDate"]) + ")" : "");
            litMaxMarks.Text = row["MaxMarks"].ToString();
            litDescription.Text = Utility.TextToHtml((string)row["Description"]);

            string instructions = row["Instructions"] as string;
            if (!string.IsNullOrWhiteSpace(instructions))
            {
                pnlInstructions.Visible = true;
                litInstructions.Text = Utility.TextToHtml(instructions);
            }

            if (!(row["FilePath"] is DBNull))
            {
                pnlAttachment.Visible = true;
                lnkInstructions.HRef = "~/Assignments/Download.ashx?type=instructions&id=" + assignmentId;
            }

            ShowSubmission(row, hasSubmission, subStatus);
            ShowSubmitButton(assignmentId, hasSubmission, subStatus, isPastDue);
        }

        private void ShowSubmission(DataRow row, bool hasSubmission, string subStatus)
        {
            if (!hasSubmission)
            {
                pnlNoSubmission.Visible = true;
                return;
            }

            pnlSubmission.Visible = true;
            litSubmittedDate.Text = ((DateTime)row["SubmittedDate"]).ToString("dd MMM yyyy, h:mm tt");
            lnkMySubmission.HRef = "~/Assignments/Download.ashx?type=submission&id=" + row["SubmissionID"];

            string comment = row["Comment"] as string;
            if (!string.IsNullOrWhiteSpace(comment))
            {
                pnlComment.Visible = true;
                litComment.Text = Server.HtmlEncode(comment);
            }

            // "Submitted" work that carries feedback was returned by the lecturer for changes.
            string returnedNote = row["Feedback"] as string;
            if (subStatus == "Submitted" && !string.IsNullOrWhiteSpace(returnedNote))
            {
                pnlReturned.Visible = true;
                litReturnedNote.Text = Utility.TextToHtml(returnedNote);
            }

            if (subStatus == "Graded")
            {
                pnlGrade.Visible = true;
                litMarks.Text = string.Format("{0:0.##} / {1}", row["Marks"], row["MaxMarks"]);

                string feedback = row["Feedback"] as string;
                litFeedback.Text = string.IsNullOrWhiteSpace(feedback)
                    ? "<span class=\"text-muted\">No feedback was given.</span>"
                    : Utility.TextToHtml(feedback);
            }
        }

        private void ShowSubmitButton(int assignmentId, bool hasSubmission, string subStatus, bool isPastDue)
        {
            if (subStatus == "Graded")
            {
                pnlLocked.Visible = true;
                litLockedReason.Text = "This submission has been graded, so it can no longer be changed.";
            }
            else if (isPastDue)
            {
                pnlLocked.Visible = true;
                litLockedReason.Text = hasSubmission
                    ? "The deadline has passed, so your submission can no longer be changed."
                    : "The deadline for this assignment has passed.";
            }
            else
            {
                pnlCanSubmit.Visible = true;
                lnkSubmit.HRef = "~/Student/SubmitAssignment?id=" + assignmentId;
                lnkSubmit.InnerText = hasSubmission ? "Replace my submission" : "Submit assignment";
            }
        }

        private void ShowNotFound()
        {
            pnlAssignment.Visible = false;
            pnlNotFound.Visible = true;
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
