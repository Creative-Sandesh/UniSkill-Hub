using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Student
{
    public partial class TakeQuiz : Page
    {
        // A little extra time after the countdown ends, for slow connections / auto-submit.
        private const int GraceSeconds = 60;

        private int _quizId;
        private DataRow _quiz;
        private DataTable _options;                 // all options of this quiz (A-D of every question)

        private string SessionKey { get { return "QuizStart_" + _quizId; } }
        private int TimeLimitSeconds { get { return (int)_quiz["TimeLimit"] * 60; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks anyone who is not a logged-in Student.
            if (!int.TryParse(Request.QueryString["id"], out _quizId) || !LoadQuiz())
            {
                pnlQuiz.Visible = false;
                pnlNotFound.Visible = true;
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return;
            }

            if (IsPostBack) return;

            StartOrResumeTimer();
            ShowHeader();
            BindQuestions();
        }

        // ---------- loading ----------

        // Only published quizzes that have questions can be taken.
        private bool LoadQuiz()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT q.QuizID, q.Title, q.TimeLimit, " +
                "  (SELECT COUNT(*) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS QuestionCount, " +
                "  (SELECT ISNULL(SUM(x.Marks), 0) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS TotalMarks " +
                "FROM Quizzes q WHERE q.QuizID = @QuizID AND q.Status = 'Published'",
                DBHelper.Param("@QuizID", _quizId));

            if (dt.Rows.Count == 0 || (int)dt.Rows[0]["QuestionCount"] == 0) return false;

            _quiz = dt.Rows[0];
            return true;
        }

        private DataTable LoadOptions()
        {
            return DBHelper.GetDataTable(
                "SELECT o.QuestionID, o.OptionLetter, o.OptionText " +
                "FROM QuizOptions o INNER JOIN QuizQuestions q ON q.QuestionID = o.QuestionID " +
                "WHERE q.QuizID = @QuizID ORDER BY o.QuestionID, o.OptionLetter",
                DBHelper.Param("@QuizID", _quizId));
        }

        // ---------- timer (kept on the SERVER so it cannot be reset by editing the page) ----------

        private void StartOrResumeTimer()
        {
            DateTime? started = Session[SessionKey] as DateTime?;

            // Refreshing the page resumes the same countdown; an old, finished one starts fresh.
            if (started == null || (DateTime.Now - started.Value).TotalSeconds >= TimeLimitSeconds)
            {
                started = DateTime.Now;
                Session[SessionKey] = started.Value;
            }

            int remaining = TimeLimitSeconds - (int)(DateTime.Now - started.Value).TotalSeconds;
            spanTimer.Attributes["data-remaining"] = Math.Max(0, remaining).ToString();
        }

        private void ShowHeader()
        {
            string title = (string)_quiz["Title"];
            Page.Title = title;
            litCrumb.Text = Server.HtmlEncode(title);
            litTitle.Text = Server.HtmlEncode(title);
            litQuestionCount.Text = _quiz["QuestionCount"].ToString();
            litTotalQuestions.Text = _quiz["QuestionCount"].ToString();
            litTotalMarks.Text = _quiz["TotalMarks"].ToString();
            litTimeLimit.Text = _quiz["TimeLimit"].ToString();
            lnkRestart.HRef = "~/Student/TakeQuiz?id=" + _quizId;
        }

        // Only the question text and options are sent to the browser - never the correct answer.
        private void BindQuestions()
        {
            _options = LoadOptions();

            rptQuestions.DataSource = DBHelper.GetDataTable(
                "SELECT QuestionID, QuestionText, Marks FROM QuizQuestions WHERE QuizID = @QuizID ORDER BY QuestionID",
                DBHelper.Param("@QuizID", _quizId));
            rptQuestions.DataBind();
        }

        protected string MarksText(object marks)
        {
            int count = (int)marks;
            return count + (count == 1 ? " mark" : " marks");
        }

        protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            DataRowView question = (DataRowView)e.Item.DataItem;
            RadioButtonList list = (RadioButtonList)e.Item.FindControl("rblOptions");

            foreach (DataRow option in _options.Select("QuestionID = " + (int)question["QuestionID"]))
            {
                string letter = (string)option["OptionLetter"];

                // RadioButtonList does NOT encode item text, so we must: otherwise an option
                // such as "<script>..." would be run by the browser instead of shown as text.
                list.Items.Add(new ListItem(Server.HtmlEncode(letter + ".  " + (string)option["OptionText"]), letter));
            }
        }

        // ---------- submit + scoring ----------

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // 1. Did the student start this quiz, and is the time up?
            DateTime? started = Session[SessionKey] as DateTime?;
            if (started == null)
            {
                ShowExpired("Your quiz session has expired, so this attempt could not be saved. Please start again.");
                return;
            }
            if ((DateTime.Now - started.Value).TotalSeconds > TimeLimitSeconds + GraceSeconds)
            {
                Session.Remove(SessionKey);
                ShowExpired("Time is up! The " + _quiz["TimeLimit"] + "-minute limit has passed, so this attempt was not saved. You can try again.");
                return;
            }

            // 2. What the student picked. Anything that is not one of this quiz's own
            //    questions/options is ignored, so a tampered form cannot change the score.
            Dictionary<int, string> picked = new Dictionary<int, string>();
            foreach (RepeaterItem item in rptQuestions.Items)
            {
                int questionId;
                string chosen = ((RadioButtonList)item.FindControl("rblOptions")).SelectedValue;
                if (int.TryParse(((HiddenField)item.FindControl("hfQuestionID")).Value, out questionId) &&
                    chosen.Length == 1 && "ABCD".IndexOf(chosen, StringComparison.Ordinal) >= 0)
                {
                    picked[questionId] = chosen;
                }
            }

            // 3. Score against the questions in the DATABASE.
            DataTable questions = DBHelper.GetDataTable(
                "SELECT QuestionID, Marks, CorrectOption FROM QuizQuestions WHERE QuizID = @QuizID ORDER BY QuestionID",
                DBHelper.Param("@QuizID", _quizId));
            DataTable options = LoadOptions();

            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>();
            StringBuilder answerRows = new StringBuilder();
            int score = 0, total = 0, i = 0;

            foreach (DataRow q in questions.Rows)
            {
                int questionId = (int)q["QuestionID"];
                int marks = (int)q["Marks"];
                string correct = ((string)q["CorrectOption"]).Trim();

                string chosen = null;
                string pickedLetter;
                if (picked.TryGetValue(questionId, out pickedLetter) &&
                    options.Select("QuestionID = " + questionId + " AND OptionLetter = '" + pickedLetter.Replace("'", "") + "'").Length == 1)
                {
                    chosen = pickedLetter;
                }

                bool isCorrect = (chosen == correct);
                int awarded = isCorrect ? marks : 0;
                total += marks;
                score += awarded;

                if (i > 0) answerRows.Append(", ");
                answerRows.Append("(@AttemptID, @Q" + i + ", @Ch" + i + ", @Co" + i + ", @Ic" + i + ", @Ma" + i + ")");
                parameters.Add(DBHelper.Param("@Q" + i, questionId));
                parameters.Add(DBHelper.Param("@Ch" + i, chosen));
                parameters.Add(DBHelper.Param("@Co" + i, correct));
                parameters.Add(DBHelper.Param("@Ic" + i, isCorrect));
                parameters.Add(DBHelper.Param("@Ma" + i, awarded));
                i++;
            }

            decimal percentage = total > 0 ? Math.Round(score * 100m / total, 2) : 0m;

            // 4. INSERT the attempt and all its answers together (all or nothing).
            string sql =
                "SET XACT_ABORT ON; BEGIN TRANSACTION; " +
                "INSERT INTO QuizAttempts (QuizID, StudentID, Score, TotalMarks, Percentage) " +
                "VALUES (@QuizID, @StudentID, @Score, @Total, @Percentage); " +
                "DECLARE @AttemptID INT = SCOPE_IDENTITY(); " +
                "INSERT INTO QuizAnswers (AttemptID, QuestionID, ChosenOption, CorrectOption, IsCorrect, MarksAwarded) VALUES " + answerRows + "; " +
                "COMMIT TRANSACTION; " +
                "SELECT @AttemptID;";

            parameters.Add(DBHelper.Param("@QuizID", _quizId));
            parameters.Add(DBHelper.Param("@StudentID", Utility.CurrentUserId));
            parameters.Add(DBHelper.Param("@Score", score));
            parameters.Add(DBHelper.Param("@Total", total));
            parameters.Add(DBHelper.Param("@Percentage", percentage));

            int attemptId;
            try
            {
                attemptId = (int)DBHelper.ExecuteScalar(sql, parameters.ToArray());
            }
            catch (Exception)
            {
                lblMessage.Text = "Sorry, we could not save your quiz right now. Your answers are still on this page - please try submitting again.";
                lblMessage.Visible = true;
                return;
            }

            Session.Remove(SessionKey);
            Response.Redirect("~/Student/QuizResult?attemptId=" + attemptId);
        }

        private void ShowExpired(string message)
        {
            pnlTake.Visible = false;
            pnlExpired.Visible = true;
            litExpired.Text = Server.HtmlEncode(message);
            ShowHeader();
        }
    }
}
