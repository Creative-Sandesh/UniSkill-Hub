using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Student
{
    public partial class QuizResult : Page
    {
        private int _attemptId;
        private DataTable _reviewOptions;
        private string _chosen;      // the student's answer to the question currently being rendered
        private string _correct;     // its correct answer

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks anyone who is not a logged-in Student.
            if (IsPostBack) return;

            if (!int.TryParse(Request.QueryString["attemptId"], out _attemptId) || !ShowResult())
            {
                pnlResult.Visible = false;
                pnlNotFound.Visible = true;
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
            }
        }

        // The attempt is only returned if it belongs to the logged-in student,
        // so nobody can open another student's result by changing the number in the URL.
        private bool ShowResult()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT a.AttemptID, a.QuizID, a.Score, a.TotalMarks, a.Percentage, a.AttemptDate, q.Title, " +
                "  CASE WHEN q.Status = 'Published' AND EXISTS (SELECT 1 FROM QuizQuestions x WHERE x.QuizID = q.QuizID) THEN 1 ELSE 0 END AS CanRetake " +
                "FROM QuizAttempts a INNER JOIN Quizzes q ON q.QuizID = a.QuizID " +
                "WHERE a.AttemptID = @AttemptID AND a.StudentID = @UserID",
                DBHelper.Param("@AttemptID", _attemptId),
                DBHelper.Param("@UserID", Utility.CurrentUserId));

            if (dt.Rows.Count == 0) return false;

            DataRow attempt = dt.Rows[0];
            string title = (string)attempt["Title"];
            decimal percentage = (decimal)attempt["Percentage"];

            Page.Title = "Result - " + title;
            litQuizTitle.Text = Server.HtmlEncode(title);
            litDate.Text = ((DateTime)attempt["AttemptDate"]).ToString("dd MMM yyyy, h:mm tt");
            litScore.Text = attempt["Score"].ToString();
            litTotal.Text = attempt["TotalMarks"].ToString();
            litPercentage.Text = percentage.ToString("0.##", CultureInfo.InvariantCulture);
            litPercentRing.Text = Math.Round(percentage, 0).ToString(CultureInfo.InvariantCulture);

            // The ring is drawn by CSS from this value (inline CSS variable).
            divRing.Attributes["style"] = "--pct: " + percentage.ToString("0.##", CultureInfo.InvariantCulture);
            divRing.Attributes["aria-label"] = litPercentage.Text + " percent";

            lnkRetake.HRef = "~/Student/TakeQuiz?id=" + attempt["QuizID"];
            lnkRetake.Visible = (int)attempt["CanRetake"] == 1;

            ShowAnswerCounts();
            return true;
        }

        private void ShowAnswerCounts()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT COUNT(*) AS Total, " +
                "  ISNULL(SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END), 0) AS Correct, " +
                "  ISNULL(SUM(CASE WHEN ChosenOption IS NULL THEN 1 ELSE 0 END), 0) AS Blank " +
                "FROM QuizAnswers WHERE AttemptID = @AttemptID",
                DBHelper.Param("@AttemptID", _attemptId));

            int total = (int)dt.Rows[0]["Total"];
            int correct = (int)dt.Rows[0]["Correct"];
            int blank = (int)dt.Rows[0]["Blank"];

            // An attempt without saved answers has nothing to count or review.
            phCounts.Visible = total > 0;
            phReviewButton.Visible = total > 0;
            if (total == 0) return;

            litCorrect.Text = correct.ToString();
            litBlank.Text = blank.ToString();
            litWrong.Text = (total - correct - blank).ToString();

            lnkReview.HRef = "~/Student/QuizResult?attemptId=" + _attemptId + "&review=1#review";

            if (Request.QueryString["review"] == "1")
            {
                LoadReview();
            }
        }

        // ---------- review of the answers ----------

        private void LoadReview()
        {
            _reviewOptions = DBHelper.GetDataTable(
                "SELECT QuestionID, OptionLetter, OptionText FROM QuizOptions " +
                "WHERE QuestionID IN (SELECT QuestionID FROM QuizAnswers WHERE AttemptID = @AttemptID) " +
                "ORDER BY QuestionID, OptionLetter",
                DBHelper.Param("@AttemptID", _attemptId));

            rptReview.DataSource = DBHelper.GetDataTable(
                "SELECT qq.QuestionID, qq.QuestionText, qq.Marks, ans.ChosenOption, ans.CorrectOption, ans.IsCorrect, ans.MarksAwarded " +
                "FROM QuizAnswers ans INNER JOIN QuizQuestions qq ON qq.QuestionID = ans.QuestionID " +
                "WHERE ans.AttemptID = @AttemptID ORDER BY qq.QuestionID",
                DBHelper.Param("@AttemptID", _attemptId));
            rptReview.DataBind();

            pnlReview.Visible = true;
            phReviewButton.Visible = false;
        }

        protected void rptReview_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            DataRowView question = (DataRowView)e.Item.DataItem;
            _chosen = question["ChosenOption"] as string;
            _correct = ((string)question["CorrectOption"]).Trim();

            Repeater options = (Repeater)e.Item.FindControl("rptReviewOptions");
            DataView view = new DataView(_reviewOptions, "QuestionID = " + (int)question["QuestionID"], "OptionLetter", DataViewRowState.CurrentRows);
            options.DataSource = view;
            options.DataBind();
        }

        // ---------- helpers used by the page markup ----------

        protected string ResultBadgeText(object isCorrect, object chosen)
        {
            if (chosen is DBNull) return "Not answered";
            return (bool)isCorrect ? "Correct" : "Incorrect";
        }

        protected string ResultBadgeClass(object isCorrect, object chosen)
        {
            if (chosen is DBNull) return "status-badge status-pending";
            return (bool)isCorrect ? "status-badge status-graded" : "status-badge status-overdue";
        }

        protected string OptionClass(object letter)
        {
            string l = (string)letter;
            string css = "review-option";
            if (l == _correct) css += " is-correct";
            if (l == _chosen) css += (l == _correct) ? " is-chosen" : " is-chosen is-wrong";
            return css;
        }

        // Words as well as colours, so the review does not rely on colour alone.
        protected string OptionTags(object letter)
        {
            string l = (string)letter;
            string html = "";
            if (l == _chosen) html += "<span class=\"option-tag\">Your answer</span>";
            if (l == _correct) html += "<span class=\"option-tag option-tag-correct\">Correct answer</span>";
            return html;
        }
    }
}
