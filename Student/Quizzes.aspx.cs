using System;
using System.Data;
using System.Web.UI;

namespace UniSkillHub.Student
{
    public partial class Quizzes : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            // Published quizzes that really have questions, plus THIS student's progress on each.
            // TotalMarks is added up from the questions so it can never be out of date.
            DataTable dt = DBHelper.GetDataTable(
                "SELECT q.QuizID, q.Title, q.Description, q.TimeLimit, " +
                "  (SELECT COUNT(*) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS QuestionCount, " +
                "  (SELECT ISNULL(SUM(x.Marks), 0) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS TotalMarks, " +
                "  (SELECT COUNT(*) FROM QuizAttempts a WHERE a.QuizID = q.QuizID AND a.StudentID = @UserID) AS Attempts, " +
                "  (SELECT MAX(a.Percentage) FROM QuizAttempts a WHERE a.QuizID = q.QuizID AND a.StudentID = @UserID) AS BestPercentage, " +
                "  (SELECT TOP 1 a.AttemptID FROM QuizAttempts a WHERE a.QuizID = q.QuizID AND a.StudentID = @UserID " +
                "     ORDER BY a.AttemptDate DESC, a.AttemptID DESC) AS LastAttemptID " +
                "FROM Quizzes q " +
                "WHERE q.Status = 'Published' AND EXISTS (SELECT 1 FROM QuizQuestions x WHERE x.QuizID = q.QuizID) " +
                "ORDER BY q.DateCreated DESC, q.QuizID DESC",
                DBHelper.Param("@UserID", Utility.CurrentUserId));

            rptQuizzes.DataSource = dt;
            rptQuizzes.DataBind();
            pnlEmpty.Visible = dt.Rows.Count == 0;

            lblSummary.Text = dt.Rows.Count == 0 ? "" :
                dt.Rows.Count + (dt.Rows.Count == 1 ? " quiz" : " quizzes") + " available";
        }

        protected string AttemptsText(object attempts)
        {
            int count = (int)attempts;
            return count + (count == 1 ? " attempt" : " attempts");
        }

        protected string StartUrl(object quizId)
        {
            return ResolveUrl("~/Student/TakeQuiz?id=" + quizId);
        }

        protected string ResultUrl(object attemptId)
        {
            return ResolveUrl("~/Student/QuizResult?attemptId=" + attemptId);
        }
    }
}
