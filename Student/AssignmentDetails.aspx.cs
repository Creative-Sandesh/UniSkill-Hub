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
            ShowSummarizeButton();
        }

        // ---------- optional AI explanation ----------

        // The button is always shown so students can find it; without an API key it is disabled and says why.
        private void ShowSummarizeButton()
        {
            pnlSummarize.Visible = true;

            if (GeminiHelper.IsConfigured)
            {
                litAiNote.Text = "The text of this assignment (and its attached brief) is sent to Google Gemini. This can take up to a minute.";
            }
            else
            {
                btnSummarize.Enabled = false;
                btnSummarize.Text = "Summarize assignment with AI (not set up yet)";
                litAiNote.Text = "AI summaries are not switched on yet: the administrator has to add a Gemini API key.";
            }
        }

        protected void btnSummarize_Click(object sender, EventArgs e)
        {
            // Page_Load does not run its checks on a postback, so everything is read and checked again here.
            int assignmentId;
            if (!int.TryParse(Request.QueryString["id"], out assignmentId)) return;

            DataTable dt = DBHelper.GetDataTable(
                "SELECT Title, Description, Instructions, DueDate, MaxMarks, FilePath FROM Assignments " +
                "WHERE AssignmentID = @AssignmentID AND Status = 'Published'",
                DBHelper.Param("@AssignmentID", assignmentId));

            if (dt.Rows.Count == 0)
            {
                ShowSummaryError("A summary is not available for this assignment.");
                return;
            }

            DataRow row = dt.Rows[0];
            string text =
                "Title: " + (string)row["Title"] + "\n" +
                "Due: " + ((DateTime)row["DueDate"]).ToString("dd MMM yyyy, h:mm tt") + "\n" +
                "Maximum marks: " + row["MaxMarks"] + "\n\n" +
                "Description:\n" + (string)row["Description"] + "\n\n" +
                "Instructions:\n" + (row["Instructions"] as string ?? "(none)");

            // The attached brief (if any) is found through the database, and must lie inside /Uploads.
            string filePath = row["FilePath"] as string;
            string fullPath = filePath == null ? null : FileHelper.ResolveInsideUploads(Context, filePath);

            try
            {
                string summary = GeminiHelper.GetAssignmentSummary(Utility.CurrentUserId, assignmentId, text, fullPath);

                // The AI's text is untrusted: encode it first, then turn line breaks into HTML.
                litSummary.Text = "<p>" + Server.HtmlEncode(summary).Replace("**", "")
                    .Replace("\r\n", "\n").Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";
                pnlSummary.Visible = true;
            }
            catch (GeminiException ex)
            {
                ShowSummaryError(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error("AssignmentDetails: could not summarize assignment " + assignmentId, ex);
                ShowSummaryError("Sorry, the summary could not be made right now. Please try again later.");
            }
        }

        private void ShowSummaryError(string message)
        {
            lblSummaryError.Text = Server.HtmlEncode(message);
            lblSummaryError.Visible = true;
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
