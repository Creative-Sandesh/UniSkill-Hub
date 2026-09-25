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
    public partial class ManageQuizzes : Page
    {
        private const int PageSize = 10;

        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "created",     "The quiz has been created. Now add its questions." },
            { "updated",     "The quiz has been updated." },
            { "deleted",     "The quiz, its questions and all attempts have been deleted." },
            { "published",   "The quiz is now published and students can take it." },
            { "unpublished", "The quiz is now a draft and hidden from students." }
        };

        private int EditingQuizId
        {
            get { return ViewState["EditQuizId"] is int ? (int)ViewState["EditQuizId"] : 0; }
            set { ViewState["EditQuizId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            if (IsPostBack) return;

            txtSearch.Text = Limit((Request.QueryString["q"] ?? "").Trim(), 100);
            SelectIfExists(ddlStatusFilter, Request.QueryString["status"]);

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);

            BindGrid(page);
        }

        // ---------- READ: the quizzes list ----------

        private void BindGrid(int page)
        {
            string search = txtSearch.Text;
            string status = ddlStatusFilter.SelectedValue;

            const string where =
                "FROM Quizzes q WHERE (@Search IS NULL OR q.Title LIKE @Search OR q.Description LIKE @Search) " +
                "AND (@Status IS NULL OR q.Status = @Status) ";

            Func<SqlParameter[]> filters = () => new[] {
                DBHelper.Param("@Search", search.Length == 0 ? null : "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%"),
                DBHelper.Param("@Status", status.Length == 0 ? null : status) };

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where, filters());
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            List<SqlParameter> paging = new List<SqlParameter>(filters());
            paging.Add(DBHelper.Param("@Skip", (page - 1) * PageSize));
            paging.Add(DBHelper.Param("@Take", PageSize));

            gvQuizzes.DataSource = DBHelper.GetDataTable(
                "SELECT q.QuizID, q.Title, q.TimeLimit, q.Status, " +
                "  (SELECT COUNT(*) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS QuestionCount, " +
                "  (SELECT ISNULL(SUM(x.Marks), 0) FROM QuizQuestions x WHERE x.QuizID = q.QuizID) AS TotalMarks, " +
                "  (SELECT COUNT(*) FROM QuizAttempts a WHERE a.QuizID = q.QuizID) AS Attempts " + where +
                "ORDER BY q.DateCreated DESC, q.QuizID DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", paging.ToArray());
            gvQuizzes.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " quiz" : " quizzes");
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "Quiz pages");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvQuizzes.HeaderRow != null) gvQuizzes.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(ListUrl(1, null));
        }

        private string ListUrl(int page, string message)
        {
            StringBuilder url = new StringBuilder("~/Admin/ManageQuizzes?page=" + page);
            if (txtSearch.Text.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(txtSearch.Text));
            if (ddlStatusFilter.SelectedValue.Length > 0) url.Append("&status=" + ddlStatusFilter.SelectedValue);
            if (message != null) url.Append("&msg=" + message);
            return url.ToString();
        }

        private int CurrentPage()
        {
            int page;
            return (int.TryParse(Request.QueryString["page"], out page) && page > 0) ? page : 1;
        }

        // ---------- row buttons ----------

        protected void gvQuizzes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            switch (e.CommandName)
            {
                case "EditQuiz": ShowForm(id); break;
                case "TogglePublish": TogglePublish(id); break;
                case "DeleteQuiz": DeleteQuiz(id); break;
            }
        }

        private static int QuestionCount(int quizId)
        {
            return (int)DBHelper.ExecuteScalar("SELECT COUNT(*) FROM QuizQuestions WHERE QuizID = @Id", DBHelper.Param("@Id", quizId));
        }

        // UPDATE: Published <-> Draft. Publishing needs at least one question.
        private void TogglePublish(int id)
        {
            DataTable dt = DBHelper.GetDataTable("SELECT Status FROM Quizzes WHERE QuizID = @Id", DBHelper.Param("@Id", id));
            if (dt.Rows.Count == 0) { ShowMessage("That quiz no longer exists.", false); return; }

            bool wasPublished = (string)dt.Rows[0]["Status"] == "Published";
            if (!wasPublished && QuestionCount(id) == 0)
            {
                ShowMessage("This quiz has no questions yet, so it cannot be published. Add questions first.", false);
                return;
            }

            DBHelper.ExecuteNonQuery("UPDATE Quizzes SET Status = @Status WHERE QuizID = @Id",
                DBHelper.Param("@Status", wasPublished ? "Draft" : "Published"), DBHelper.Param("@Id", id));

            Response.Redirect(ListUrl(CurrentPage(), wasPublished ? "unpublished" : "published"));
        }

        // DELETE: the database removes the quiz's questions, options, attempts and answers with it (ON DELETE CASCADE).
        private void DeleteQuiz(int id)
        {
            int rows = DBHelper.ExecuteNonQuery("DELETE FROM Quizzes WHERE QuizID = @Id", DBHelper.Param("@Id", id));
            if (rows == 0) { ShowMessage("That quiz no longer exists.", false); return; }

            Response.Redirect(ListUrl(CurrentPage(), "deleted"));
        }

        // ---------- the Add / Edit form ----------

        protected void btnAdd_Click(object sender, EventArgs e) { ShowForm(0); }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            EditingQuizId = 0;
        }

        private void ShowForm(int id)
        {
            EditingQuizId = id;
            pnlForm.Visible = true;

            if (id == 0)
            {
                lblFormTitle.Text = "Add quiz";
                txtTitle.Text = txtDescription.Text = "";
                txtTimeLimit.Text = "10";
                ddlStatus.SelectedValue = "Draft";
                return;
            }

            DataTable dt = DBHelper.GetDataTable("SELECT Title, Description, TimeLimit, Status FROM Quizzes WHERE QuizID = @Id", DBHelper.Param("@Id", id));
            if (dt.Rows.Count == 0)
            {
                pnlForm.Visible = false;
                EditingQuizId = 0;
                ShowMessage("That quiz no longer exists.", false);
                return;
            }

            DataRow q = dt.Rows[0];
            lblFormTitle.Text = "Edit quiz";
            txtTitle.Text = (string)q["Title"];
            txtDescription.Text = (q["Description"] as string) ?? "";
            txtTimeLimit.Text = q["TimeLimit"].ToString();
            ddlStatus.SelectedValue = (string)q["Status"];
        }

        // A quiz can only be Published if it has questions (a new quiz has none yet).
        protected void cvStatus_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (ddlStatus.SelectedValue != "Published") { args.IsValid = true; return; }
            args.IsValid = EditingQuizId != 0 && QuestionCount(EditingQuizId) > 0;
        }

        // ---------- INSERT / UPDATE ----------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("QuizForm");
            if (!Page.IsValid) return;

            int id = EditingQuizId;
            string description = txtDescription.Text.Trim();

            try
            {
                if (id == 0)
                {
                    DBHelper.ExecuteNonQuery(
                        "INSERT INTO Quizzes (Title, Description, TimeLimit, TotalMarks, CreatedBy, Status) " +
                        "VALUES (@Title, @Description, @TimeLimit, 0, @CreatedBy, @Status)",
                        DBHelper.Param("@Title", txtTitle.Text.Trim()),
                        DBHelper.Param("@Description", description.Length == 0 ? null : description),
                        DBHelper.Param("@TimeLimit", int.Parse(txtTimeLimit.Text)),
                        DBHelper.Param("@CreatedBy", Utility.CurrentUserId),
                        DBHelper.Param("@Status", ddlStatus.SelectedValue));
                }
                else
                {
                    int rows = DBHelper.ExecuteNonQuery(
                        "UPDATE Quizzes SET Title = @Title, Description = @Description, TimeLimit = @TimeLimit, Status = @Status WHERE QuizID = @Id",
                        DBHelper.Param("@Title", txtTitle.Text.Trim()),
                        DBHelper.Param("@Description", description.Length == 0 ? null : description),
                        DBHelper.Param("@TimeLimit", int.Parse(txtTimeLimit.Text)),
                        DBHelper.Param("@Status", ddlStatus.SelectedValue),
                        DBHelper.Param("@Id", id));

                    if (rows == 0) { ShowMessage("That quiz no longer exists.", false); pnlForm.Visible = false; return; }
                }
            }
            catch (SqlException)
            {
                ShowMessage("Sorry, the quiz could not be saved right now. Please try again.", false);
                return;
            }

            Response.Redirect(ListUrl(id == 0 ? 1 : CurrentPage(), id == 0 ? "created" : "updated"));
        }

        // ---------- helpers used by the page markup ----------

        protected string QuestionsUrl(object quizId) { return ResolveUrl("~/Admin/ManageQuestions?quiz=" + quizId); }

        protected string StatusClass(object status) { return Utility.StatusBadgeClass(status); }

        protected string QuestionsText(object count, object marks)
        {
            int n = (int)count;
            if (n == 0) return "<span class=\"text-muted\">none yet</span>";
            return n + (n == 1 ? " question" : " questions") + "<div class=\"dash-meta\">" + marks + " marks</div>";
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
