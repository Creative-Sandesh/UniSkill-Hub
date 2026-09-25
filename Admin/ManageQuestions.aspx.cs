using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class ManageQuestions : Page
    {
        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "created",   "The question has been added." },
            { "updated",   "The question has been updated." },
            { "deleted",   "The question and the students' answers to it have been deleted." },
            { "lastgone",  "The question was deleted. The quiz has no questions left, so it was changed back to a draft." }
        };

        private int _quizId;
        private DataRow _quiz;
        private DataTable _options;      // the options of every question of this quiz (for the grid)

        private int EditingQuestionId
        {
            get { return ViewState["EditQuestionId"] is int ? (int)ViewState["EditQuestionId"] : 0; }
            set { ViewState["EditQuestionId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            if (!int.TryParse(Request.QueryString["quiz"], out _quizId) || !LoadQuiz())
            {
                pnlQuiz.Visible = false;
                pnlNotFound.Visible = true;
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
                return;
            }

            if (IsPostBack) return;

            ShowHeader();

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);

            BindGrid();
        }

        private bool LoadQuiz()
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT q.QuizID, q.Title, q.Status, q.TimeLimit, " +
                "  (SELECT COUNT(*) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS QuestionCount, " +
                "  (SELECT ISNULL(SUM(x.Marks), 0) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS TotalMarks, " +
                "  (SELECT COUNT(*) FROM QuizAttempts a WHERE a.QuizID = q.QuizID) AS Attempts " +
                "FROM Quizzes q WHERE q.QuizID = @Id", DBHelper.Param("@Id", _quizId));

            if (dt.Rows.Count == 0) return false;
            _quiz = dt.Rows[0];
            return true;
        }

        private void ShowHeader()
        {
            string title = (string)_quiz["Title"];
            Page.Title = "Questions - " + title;
            litCrumb.Text = Server.HtmlEncode(title);
            litTitle.Text = Server.HtmlEncode(title);
            litStatusBadge.Text = "<span class=\"" + Utility.StatusBadgeClass(_quiz["Status"]) + "\">" + Server.HtmlEncode((string)_quiz["Status"]) + "</span>";

            int n = (int)_quiz["QuestionCount"];
            litSummary.Text = n + (n == 1 ? " question" : " questions") + " · " + _quiz["TotalMarks"] + " marks · " + _quiz["TimeLimit"] + " min · " +
                              _quiz["Attempts"] + ((int)_quiz["Attempts"] == 1 ? " attempt" : " attempts");
        }

        // ---------- READ: the questions of this quiz ----------

        private void BindGrid()
        {
            _options = DBHelper.GetDataTable(
                "SELECT o.QuestionID, o.OptionLetter, o.OptionText FROM QuizOptions o " +
                "INNER JOIN QuizQuestions q ON q.QuestionID = o.QuestionID WHERE q.QuizID = @Id ORDER BY o.QuestionID, o.OptionLetter",
                DBHelper.Param("@Id", _quizId));

            gvQuestions.DataSource = DBHelper.GetDataTable(
                "SELECT QuestionID, QuestionText, Marks, CorrectOption FROM QuizQuestions WHERE QuizID = @Id ORDER BY QuestionID",
                DBHelper.Param("@Id", _quizId));
            gvQuestions.DataBind();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvQuestions.HeaderRow != null) gvQuestions.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        // The four options of a row, correct one marked (all text HTML-encoded).
        protected string OptionsHtml(object questionId, object correct)
        {
            StringBuilder html = new StringBuilder();
            string right = Convert.ToString(correct).Trim();
            foreach (DataRow o in _options.Select("QuestionID = " + (int)questionId))
            {
                string letter = (string)o["OptionLetter"];
                bool isRight = (letter == right);
                html.Append("<li" + (isRight ? " class=\"is-correct-option\"" : "") + "><span class=\"option-letter\">" + letter + "</span> " +
                            HttpUtility.HtmlEncode((string)o["OptionText"]) + (isRight ? " <span class=\"option-tag option-tag-correct\">Correct</span>" : "") + "</li>");
            }
            return html.ToString();
        }

        // ---------- row buttons ----------

        protected void gvQuestions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (_quiz == null) return;      // no valid quiz in the address: nothing to act on

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            if (e.CommandName == "EditQuestion") ShowForm(id);
            else if (e.CommandName == "DeleteQuestion") DeleteQuestion(id);
        }

        // DELETE: the question's stored answers must go first (that table has no cascade from questions),
        // then the question itself (its options cascade). The quiz total is updated, and a quiz that
        // is left without questions goes back to Draft. All in ONE transaction.
        private void DeleteQuestion(int questionId)
        {
            object result = DBHelper.ExecuteScalar(
                "SET XACT_ABORT ON; BEGIN TRANSACTION; " +
                "DECLARE @Deleted INT = 0, @Unpublished INT = 0; " +
                "IF EXISTS (SELECT 1 FROM QuizQuestions WHERE QuestionID = @QuestionID AND QuizID = @QuizID) " +
                "BEGIN " +
                "  DELETE FROM QuizAnswers WHERE QuestionID = @QuestionID; " +
                "  DELETE FROM QuizQuestions WHERE QuestionID = @QuestionID AND QuizID = @QuizID; " +
                "  SET @Deleted = 1; " +
                "  UPDATE Quizzes SET TotalMarks = (SELECT ISNULL(SUM(Marks), 0) FROM QuizQuestions WHERE QuizID = @QuizID) WHERE QuizID = @QuizID; " +
                "  IF NOT EXISTS (SELECT 1 FROM QuizQuestions WHERE QuizID = @QuizID) AND EXISTS (SELECT 1 FROM Quizzes WHERE QuizID = @QuizID AND Status = 'Published') " +
                "  BEGIN UPDATE Quizzes SET Status = 'Draft' WHERE QuizID = @QuizID; SET @Unpublished = 1; END " +
                "END " +
                "COMMIT TRANSACTION; " +
                "SELECT @Deleted * 10 + @Unpublished;",
                DBHelper.Param("@QuestionID", questionId), DBHelper.Param("@QuizID", _quizId));

            int code = (int)result;      // 0 = not found, 10 = deleted, 11 = deleted and quiz reverted to draft
            if (code == 0) { ShowMessage("That question no longer exists.", false); return; }

            Response.Redirect("~/Admin/ManageQuestions?quiz=" + _quizId + "&msg=" + (code == 11 ? "lastgone" : "deleted"));
        }

        // ---------- the Add / Edit form ----------

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (_quiz == null) return;
            ShowForm(0);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            EditingQuestionId = 0;
        }

        private void ShowForm(int questionId)
        {
            EditingQuestionId = questionId;
            pnlForm.Visible = true;

            int attempts = (int)_quiz["Attempts"];
            lblAttemptsWarning.Visible = attempts > 0;
            lblAttemptsWarning.Text = Server.HtmlEncode(attempts + (attempts == 1 ? " student has" : " students have") +
                " already attempted this quiz. Changing questions or the correct answer does not change the scores they already received.");

            if (questionId == 0)
            {
                lblFormTitle.Text = "Add question";
                txtQuestion.Text = txtOptionA.Text = txtOptionB.Text = txtOptionC.Text = txtOptionD.Text = "";
                rblCorrect.ClearSelection();
                txtMarks.Text = "1";
                return;
            }

            DataTable q = DBHelper.GetDataTable(
                "SELECT QuestionText, Marks, CorrectOption FROM QuizQuestions WHERE QuestionID = @Id AND QuizID = @QuizID",
                DBHelper.Param("@Id", questionId), DBHelper.Param("@QuizID", _quizId));

            if (q.Rows.Count == 0)
            {
                pnlForm.Visible = false;
                EditingQuestionId = 0;
                ShowMessage("That question no longer exists.", false);
                return;
            }

            lblFormTitle.Text = "Edit question";
            txtQuestion.Text = (string)q.Rows[0]["QuestionText"];
            txtMarks.Text = q.Rows[0]["Marks"].ToString();
            rblCorrect.SelectedValue = ((string)q.Rows[0]["CorrectOption"]).Trim();

            foreach (DataRow o in DBHelper.GetDataTable("SELECT OptionLetter, OptionText FROM QuizOptions WHERE QuestionID = @Id", DBHelper.Param("@Id", questionId)).Rows)
            {
                string text = (string)o["OptionText"];
                switch ((string)o["OptionLetter"])
                {
                    case "A": txtOptionA.Text = text; break;
                    case "B": txtOptionB.Text = text; break;
                    case "C": txtOptionC.Text = text; break;
                    case "D": txtOptionD.Text = text; break;
                }
            }
        }

        // The four options must be different from each other (ignoring case and spaces at the ends).
        protected void cvDistinct_ServerValidate(object source, ServerValidateEventArgs args)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (TextBox box in new[] { txtOptionA, txtOptionB, txtOptionC, txtOptionD })
            {
                string text = box.Text.Trim();
                if (text.Length > 0 && !seen.Add(text)) { args.IsValid = false; return; }
            }
            args.IsValid = true;
        }

        // ---------- INSERT / UPDATE: the question, its four options and the quiz total, all together ----------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (_quiz == null) return;

            Page.Validate("QuestionForm");
            if (!Page.IsValid) return;

            int questionId = EditingQuestionId;

            List<SqlParameter> p = new List<SqlParameter>
            {
                DBHelper.Param("@QuizID", _quizId),
                DBHelper.Param("@Text", txtQuestion.Text.Trim()),
                DBHelper.Param("@Marks", int.Parse(txtMarks.Text)),
                DBHelper.Param("@Correct", rblCorrect.SelectedValue),
                DBHelper.Param("@A", txtOptionA.Text.Trim()),
                DBHelper.Param("@B", txtOptionB.Text.Trim()),
                DBHelper.Param("@C", txtOptionC.Text.Trim()),
                DBHelper.Param("@D", txtOptionD.Text.Trim())
            };

            const string syncTotal =
                "UPDATE Quizzes SET TotalMarks = (SELECT ISNULL(SUM(Marks), 0) FROM QuizQuestions WHERE QuizID = @QuizID) WHERE QuizID = @QuizID; ";

            try
            {
                if (questionId == 0)
                {
                    DBHelper.ExecuteNonQuery(
                        "SET XACT_ABORT ON; BEGIN TRANSACTION; " +
                        "INSERT INTO QuizQuestions (QuizID, QuestionText, Marks, CorrectOption) VALUES (@QuizID, @Text, @Marks, @Correct); " +
                        "DECLARE @Q INT = SCOPE_IDENTITY(); " +
                        "INSERT INTO QuizOptions (QuestionID, OptionText, OptionLetter) VALUES (@Q, @A, 'A'), (@Q, @B, 'B'), (@Q, @C, 'C'), (@Q, @D, 'D'); " +
                        syncTotal + "COMMIT TRANSACTION;", p.ToArray());
                }
                else
                {
                    p.Add(DBHelper.Param("@QuestionID", questionId));
                    int rows = (int)DBHelper.ExecuteScalar(
                        "SET XACT_ABORT ON; BEGIN TRANSACTION; " +
                        "UPDATE QuizQuestions SET QuestionText = @Text, Marks = @Marks, CorrectOption = @Correct WHERE QuestionID = @QuestionID AND QuizID = @QuizID; " +
                        "DECLARE @Changed INT = @@ROWCOUNT; " +
                        "IF @Changed = 1 BEGIN " +
                        "  UPDATE QuizOptions SET OptionText = @A WHERE QuestionID = @QuestionID AND OptionLetter = 'A'; " +
                        "  UPDATE QuizOptions SET OptionText = @B WHERE QuestionID = @QuestionID AND OptionLetter = 'B'; " +
                        "  UPDATE QuizOptions SET OptionText = @C WHERE QuestionID = @QuestionID AND OptionLetter = 'C'; " +
                        "  UPDATE QuizOptions SET OptionText = @D WHERE QuestionID = @QuestionID AND OptionLetter = 'D'; " +
                        "  " + syncTotal + "END " +
                        "COMMIT TRANSACTION; SELECT @Changed;", p.ToArray());

                    if (rows == 0) { ShowMessage("That question no longer exists.", false); pnlForm.Visible = false; return; }
                }
            }
            catch (SqlException)
            {
                ShowMessage("Sorry, the question could not be saved right now. Please try again.", false);
                return;
            }

            Response.Redirect("~/Admin/ManageQuestions?quiz=" + _quizId + "&msg=" + (questionId == 0 ? "created" : "updated"));
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }
    }
}
