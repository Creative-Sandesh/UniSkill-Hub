using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Student
{
    public partial class SubmitAssignment : Page
    {
        private int _assignmentId;
        private DataRow _assignment;        // the assignment + this student's existing submission (if any)
        private string _closedReason;       // set when the student may not submit (deadline / graded)

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not a logged-in Student.
            if (!int.TryParse(Request.QueryString["id"], out _assignmentId) || !LoadAssignment())
            {
                pnlAssignment.Visible = false;
                pnlNotFound.Visible = true;
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return;
            }

            // Runs on the first load AND on every postback, so the rules below are
            // re-checked when the form is submitted (a page left open past the deadline is refused).
            bool hasSubmission = !(_assignment["SubmissionID"] is DBNull);
            string subStatus = hasSubmission ? (string)_assignment["SubStatus"] : null;

            if (subStatus == "Graded")
            {
                _closedReason = "This submission has already been graded, so it can no longer be changed.";
            }
            else if ((int)_assignment["IsPastDue"] == 1)
            {
                _closedReason = "The deadline for this assignment has passed, so submissions are closed.";
            }

            if (!IsPostBack)
            {
                ShowHeader(hasSubmission);
            }

            pnlClosed.Visible = _closedReason != null;
            pnlForm.Visible = _closedReason == null;
            litClosedReason.Text = Server.HtmlEncode(_closedReason ?? "");
        }

        // ---------- page content ----------

        private bool LoadAssignment()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT a.AssignmentID, a.Title, a.DueDate, a.MaxMarks, " +
                "       CASE WHEN a.DueDate < GETDATE() THEN 1 ELSE 0 END AS IsPastDue, " +
                "       s.SubmissionID, s.SubmittedDate, s.Status AS SubStatus, s.FilePath AS OldFilePath " +
                "FROM Assignments a " +
                "LEFT JOIN Submissions s ON s.AssignmentID = a.AssignmentID AND s.StudentID = @UserID " +
                "WHERE a.AssignmentID = @AssignmentID AND a.Status = 'Published'",
                DBHelper.Param("@UserID", Utility.CurrentUserId),
                DBHelper.Param("@AssignmentID", _assignmentId));

            if (dt.Rows.Count == 0) return false;

            _assignment = dt.Rows[0];
            return true;
        }

        private void ShowHeader(bool hasSubmission)
        {
            string title = (string)_assignment["Title"];
            string detailsUrl = "~/Student/AssignmentDetails?id=" + _assignmentId;

            Page.Title = "Submit - " + title;
            litCrumb.Text = Server.HtmlEncode(title);
            litTitle.Text = Server.HtmlEncode(title);
            litDue.Text = ((DateTime)_assignment["DueDate"]).ToString("dd MMM yyyy, h:mm tt");
            litMaxMarks.Text = _assignment["MaxMarks"].ToString();
            lnkBack.HRef = detailsUrl;
            lnkBackClosed.HRef = detailsUrl;
            lnkCancel.HRef = detailsUrl;

            if (hasSubmission)
            {
                pnlExisting.Visible = true;
                litExistingDate.Text = ((DateTime)_assignment["SubmittedDate"]).ToString("dd MMM yyyy, h:mm tt");
            }

            if (Request.QueryString["toolarge"] == "1")
            {
                ShowError("That file is too large. The maximum size is 10 MB.");
            }
        }

        // ---------- validation ----------

        // Server-side file checks (the browser script in validation.js is only a convenience).
        protected void cvFile_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string problem = CheckFile(fuAssignment);
            args.IsValid = (problem == null);
            if (problem != null) cvFile.ErrorMessage = problem;
        }

        // Returns a message describing what is wrong with the chosen file, or null if it is fine.
        private static string CheckFile(FileUpload upload)
        {
            if (!upload.HasFile) return "Please choose a file to upload.";

            HttpPostedFile file = upload.PostedFile;

            if (file.ContentLength == 0)
                return "That file is empty.";

            if (file.ContentLength > FileHelper.MaxSubmissionBytes)
                return "That file is larger than 10 MB. Please upload a smaller file.";

            if (!FileHelper.IsAllowedExtension(file.FileName, FileHelper.SubmissionExtensions))
                return "That file type is not allowed. Please upload a PDF, DOC, DOCX, PPT, PPTX, ZIP or TXT file.";

            if (!FileHelper.ContentMatchesExtension(file))
                return "The contents of that file do not match its type. Please upload a genuine " +
                       Path.GetExtension(file.FileName).ToUpperInvariant().TrimStart('.') + " file.";

            return null;
        }

        // ---------- INSERT / UPDATE the submission ----------

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // The form is hidden when submissions are closed, but a crafted request could still arrive.
            if (_closedReason != null)
            {
                ShowError(_closedReason);
                return;
            }

            int userId = Utility.CurrentUserId;
            string comment = txtComment.Text.Trim();
            string extension = Path.GetExtension(fuAssignment.PostedFile.FileName).ToLowerInvariant();

            // A unique, safe name: assignment id + student id + time + cleaned original name.
            string fileName = string.Format("a{0}_s{1}_{2:yyyyMMddHHmmss}_{3}{4}",
                _assignmentId, userId, DateTime.Now,
                FileHelper.MakeSafeFileName(Path.GetFileNameWithoutExtension(fuAssignment.PostedFile.FileName), 50),
                extension);
            string relativePath = "Uploads/Assignments/" + fileName;

            bool isReplacement = !(_assignment["SubmissionID"] is DBNull);
            string oldPath = isReplacement ? _assignment["OldFilePath"] as string : null;

            try
            {
                Directory.CreateDirectory(Server.MapPath("~/Uploads/Assignments"));
                fuAssignment.SaveAs(Server.MapPath("~/" + relativePath));

                bool saved;
                try
                {
                    saved = isReplacement
                        ? ReplaceSubmission(userId, relativePath, comment)
                        : AddSubmission(userId, relativePath, comment);
                }
                catch
                {
                    FileHelper.DeleteUploadedFile(Context, relativePath);   // do not leave an orphan file
                    throw;
                }

                if (!saved)
                {
                    FileHelper.DeleteUploadedFile(Context, relativePath);
                    ShowError("Your submission could not be saved because it has just been graded or closed. Please reload the page.");
                    return;
                }
            }
            catch (SqlException ex)
            {
                // 2627 / 2601: this student already has a submission (e.g. a double click)
                ShowError((ex.Number == 2627 || ex.Number == 2601)
                    ? "You have already submitted this assignment. Please reload the page."
                    : "Sorry, we could not save your submission right now. Please try again later.");
                return;
            }
            catch (Exception)
            {
                ShowError("Sorry, we could not save your submission right now. Please try again later.");
                return;
            }

            // The old file is no longer needed once the new one is safely recorded.
            if (oldPath != null) FileHelper.DeleteUploadedFile(Context, oldPath);

            Response.Redirect("~/Student/MySubmissions?submitted=1");
        }

        // INSERT: first submission
        private bool AddSubmission(int userId, string relativePath, string comment)
        {
            DBHelper.ExecuteNonQuery(
                "INSERT INTO Submissions (AssignmentID, StudentID, FilePath, Comment) " +
                "VALUES (@AssignmentID, @StudentID, @FilePath, @Comment)",
                DBHelper.Param("@AssignmentID", _assignmentId),
                DBHelper.Param("@StudentID", userId),
                DBHelper.Param("@FilePath", relativePath),
                DBHelper.Param("@Comment", comment.Length == 0 ? null : comment));
            return true;
        }

        // UPDATE: replace an earlier submission (only while it is still "Submitted", i.e. not graded)
        private bool ReplaceSubmission(int userId, string relativePath, string comment)
        {
            int rows = DBHelper.ExecuteNonQuery(
                "UPDATE Submissions SET FilePath = @FilePath, Comment = @Comment, SubmittedDate = GETDATE(), Status = 'Submitted' " +
                "WHERE SubmissionID = @SubmissionID AND StudentID = @StudentID AND Status = 'Submitted'",
                DBHelper.Param("@FilePath", relativePath),
                DBHelper.Param("@Comment", comment.Length == 0 ? null : comment),
                DBHelper.Param("@SubmissionID", _assignment["SubmissionID"]),
                DBHelper.Param("@StudentID", userId));
            return rows == 1;
        }

        private void ShowError(string message)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.Visible = true;
        }
    }
}
