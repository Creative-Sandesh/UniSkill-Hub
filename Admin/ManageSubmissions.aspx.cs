using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class ManageSubmissions : Page
    {
        private const int PageSize = 10;

        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "graded",   "The grade has been saved. The student can now see the marks and feedback." },
            { "returned", "The submission has been returned to the student for resubmission." }
        };

        // The submission being graded, and its SubmittedDate at the moment the form was opened.
        private int GradingId
        {
            get { return ViewState["GradingId"] is int ? (int)ViewState["GradingId"] : 0; }
            set { ViewState["GradingId"] = value; }
        }
        private DateTime LoadedSubmittedDate
        {
            get { return ViewState["LoadedSubmittedDate"] is DateTime ? (DateTime)ViewState["LoadedSubmittedDate"] : DateTime.MinValue; }
            set { ViewState["LoadedSubmittedDate"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            if (IsPostBack) return;

            LoadAssignmentFilter();

            txtSearch.Text = Limit((Request.QueryString["q"] ?? "").Trim(), 100);
            SelectIfExists(ddlAssignment, Request.QueryString["assignment"]);
            SelectIfExists(ddlStatusFilter, Request.QueryString["status"]);

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);

            BindGrid(page);
        }

        private void LoadAssignmentFilter()
        {
            ddlAssignment.DataSource = DBHelper.GetDataTable("SELECT AssignmentID, Title FROM Assignments ORDER BY DueDate DESC, AssignmentID DESC");
            ddlAssignment.DataValueField = "AssignmentID";
            ddlAssignment.DataTextField = "Title";
            ddlAssignment.DataBind();
            ddlAssignment.Items.Insert(0, new ListItem("All assignments", ""));
        }

        // ---------- READ: the submissions list ----------

        private void BindGrid(int page)
        {
            string search = txtSearch.Text;
            int assignmentId;
            bool byAssignment = int.TryParse(ddlAssignment.SelectedValue, out assignmentId);
            string status = ddlStatusFilter.SelectedValue;

            const string where =
                "FROM Submissions s " +
                "INNER JOIN Users u ON u.UserID = s.StudentID " +
                "INNER JOIN Assignments a ON a.AssignmentID = s.AssignmentID " +
                "WHERE (@Search IS NULL OR u.FullName LIKE @Search OR u.Username LIKE @Search OR u.Email LIKE @Search) " +
                "AND (@AssignmentID IS NULL OR s.AssignmentID = @AssignmentID) " +
                "AND (@Status IS NULL OR s.Status = @Status) ";

            Func<SqlParameter[]> filters = () => new[] {
                DBHelper.Param("@Search", search.Length == 0 ? null : "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%"),
                DBHelper.Param("@AssignmentID", byAssignment ? (object)assignmentId : null),
                DBHelper.Param("@Status", status.Length == 0 ? null : status) };

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where, filters());
            int toGrade = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where + "AND s.Status = 'Submitted' ", filters());
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            List<SqlParameter> paging = new List<SqlParameter>(filters());
            paging.Add(DBHelper.Param("@Skip", (page - 1) * PageSize));
            paging.Add(DBHelper.Param("@Take", PageSize));

            // Work that still needs grading comes first (the longest-waiting at the top).
            gvSubmissions.DataSource = DBHelper.GetDataTable(
                "SELECT s.SubmissionID, s.SubmittedDate, s.Status, s.Marks, u.FullName, u.Username, a.Title, a.MaxMarks, " +
                "  CASE WHEN s.SubmittedDate > a.DueDate THEN 1 ELSE 0 END AS IsLate " + where +
                "ORDER BY CASE WHEN s.Status = 'Submitted' THEN 0 ELSE 1 END, " +
                "         CASE WHEN s.Status = 'Submitted' THEN s.SubmittedDate END ASC, s.SubmittedDate DESC, s.SubmissionID DESC " +
                "OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", paging.ToArray());
            gvSubmissions.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " submission" : " submissions") + " · " + toGrade + " to grade";
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "Submission pages");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvSubmissions.HeaderRow != null) gvSubmissions.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(ListUrl(1, null));
        }

        private string ListUrl(int page, string message)
        {
            StringBuilder url = new StringBuilder("~/Admin/ManageSubmissions?page=" + page);
            if (txtSearch.Text.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(txtSearch.Text));
            if (ddlAssignment.SelectedValue.Length > 0) url.Append("&assignment=" + ddlAssignment.SelectedValue);
            if (ddlStatusFilter.SelectedValue.Length > 0) url.Append("&status=" + ddlStatusFilter.SelectedValue);
            if (message != null) url.Append("&msg=" + message);
            return url.ToString();
        }

        private int CurrentPage()
        {
            int page;
            return (int.TryParse(Request.QueryString["page"], out page) && page > 0) ? page : 1;
        }

        // ---------- the grade form ----------

        protected void gvSubmissions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (e.CommandName == "GradeSubmission" && int.TryParse(Convert.ToString(e.CommandArgument), out id))
            {
                ShowForm(id);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            GradingId = 0;
        }

        private void ShowForm(int id)
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT s.SubmissionID, s.Comment, s.SubmittedDate, s.Status, s.Marks, s.Feedback, " +
                "       u.FullName, u.Username, a.Title, a.MaxMarks " +
                "FROM Submissions s " +
                "INNER JOIN Users u ON u.UserID = s.StudentID " +
                "INNER JOIN Assignments a ON a.AssignmentID = s.AssignmentID " +
                "WHERE s.SubmissionID = @Id", DBHelper.Param("@Id", id));

            if (dt.Rows.Count == 0)
            {
                pnlForm.Visible = false;
                GradingId = 0;
                ShowMessage("That submission no longer exists (the assignment may have been deleted).", false);
                return;
            }

            DataRow s = dt.Rows[0];
            GradingId = id;
            LoadedSubmittedDate = (DateTime)s["SubmittedDate"];     // used to notice if the student replaces the file meanwhile

            pnlForm.Visible = true;
            litStudent.Text = Server.HtmlEncode((string)s["FullName"]) + " (@" + Server.HtmlEncode((string)s["Username"]) + ")";
            litAssignment.Text = Server.HtmlEncode((string)s["Title"]);
            litSubmitted.Text = ((DateTime)s["SubmittedDate"]).ToString("dd MMM yyyy, h:mm tt");
            lnkFile.HRef = "~/Assignments/Download.ashx?type=submission&id=" + id;

            string comment = s["Comment"] as string;
            pnlStudentComment.Visible = !string.IsNullOrWhiteSpace(comment);
            litStudentComment.Text = Server.HtmlEncode(comment ?? "");

            int max = (int)s["MaxMarks"];
            lblMaxHint.Text = "Out of " + max + " (up to 2 decimals).";
            txtMarks.Attributes["data-max"] = max.ToString(CultureInfo.InvariantCulture);

            bool graded = (string)s["Status"] == "Graded";
            ddlStatus.SelectedValue = "Graded";       // the usual next step is to grade it
            txtMarks.Text = graded ? Convert.ToDecimal(s["Marks"]).ToString("0.##", CultureInfo.InvariantCulture) : "";
            txtFeedback.Text = (s["Feedback"] as string) ?? "";
        }

        // ---------- server-side validation ----------

        // "Graded" needs marks between 0 and the assignment's maximum; "Submitted" ignores the marks box.
        protected void cvMarks_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (ddlStatus.SelectedValue != "Graded") return;

            string text = (args.Value ?? "").Trim();
            if (text.Length == 0)
            {
                cvMarks.ErrorMessage = "Enter the marks to save a grade.";
                args.IsValid = false;
                return;
            }
            if (!Regex.IsMatch(text, @"^\d{1,3}(\.\d{1,2})?$"))
            {
                cvMarks.ErrorMessage = "Marks must be a number such as 85 or 85.5 (up to 2 decimals).";
                args.IsValid = false;
                return;
            }

            int max = MaxMarksOfCurrent();
            if (decimal.Parse(text, CultureInfo.InvariantCulture) > max)
            {
                cvMarks.ErrorMessage = "Marks cannot be more than the maximum of " + max + ".";
                args.IsValid = false;
            }
        }

        private int MaxMarksOfCurrent()
        {
            object max = DBHelper.ExecuteScalar(
                "SELECT a.MaxMarks FROM Submissions s INNER JOIN Assignments a ON a.AssignmentID = s.AssignmentID WHERE s.SubmissionID = @Id",
                DBHelper.Param("@Id", GradingId));
            return max == null ? 0 : (int)max;
        }

        // ---------- UPDATE: save the grade ----------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("GradeForm");
            if (!Page.IsValid) return;
            if (GradingId == 0) return;

            bool graded = ddlStatus.SelectedValue == "Graded";
            object marks = graded ? (object)decimal.Parse(txtMarks.Text.Trim(), CultureInfo.InvariantCulture) : null;   // "returned" work has no marks
            string feedback = txtFeedback.Text.Trim();

            // The submission is only updated if it is STILL the same one we opened: if the student
            // replaced their file meanwhile, SubmittedDate changed and we refuse to grade the wrong work.
            int rows = DBHelper.ExecuteNonQuery(
                "UPDATE Submissions SET Marks = @Marks, Feedback = @Feedback, Status = @Status " +
                "WHERE SubmissionID = @Id AND SubmittedDate = @Loaded",
                DBHelper.Param("@Marks", marks),
                DBHelper.Param("@Feedback", feedback.Length == 0 ? null : feedback),
                DBHelper.Param("@Status", ddlStatus.SelectedValue),
                DBHelper.Param("@Id", GradingId),
                DBHelper.Param("@Loaded", LoadedSubmittedDate));

            if (rows == 0)
            {
                pnlForm.Visible = false;
                GradingId = 0;
                ShowMessage("Nothing was saved: the student has submitted a new file (or the submission was removed) since you opened it. Please open it again.", false);
                BindGrid(CurrentPage());
                return;
            }

            Response.Redirect(ListUrl(CurrentPage(), graded ? "graded" : "returned"));
        }

        // ---------- helpers used by the page markup ----------

        protected string StatusClass(object status)
        {
            return Convert.ToString(status) == "Graded" ? "status-badge status-graded" : "status-badge status-pending";
        }

        protected string StatusText(object status)
        {
            return Convert.ToString(status) == "Graded" ? "Graded" : "To grade";
        }

        protected string MarksText(object marks, object maxMarks)
        {
            return (marks is DBNull) ? "&mdash;" : HttpUtility.HtmlEncode(string.Format("{0:0.##} / {1}", marks, maxMarks));
        }

        protected string FileUrl(object submissionId)
        {
            return ResolveUrl("~/Assignments/Download.ashx?type=submission&id=" + submissionId);
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }

        private static string Limit(string text, int max) { return text.Length > max ? text.Substring(0, max) : text; }

        private static void SelectIfExists(DropDownList list, string value)
        {
            ListItem item = list.Items.FindByValue(value ?? "");
            if (item != null) list.SelectedValue = item.Value;
        }
    }
}
