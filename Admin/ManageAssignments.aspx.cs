using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UniSkillHub.Admin
{
    public partial class ManageAssignments : Page
    {
        private const int PageSize = 10;

        private static readonly Dictionary<string, string> FlashMessages = new Dictionary<string, string>
        {
            { "created",     "The assignment has been created." },
            { "updated",     "The assignment has been updated." },
            { "deleted",     "The assignment and all its submissions have been deleted." },
            { "published",   "The assignment is now published and visible to students." },
            { "unpublished", "The assignment is now a draft and hidden from students." }
        };

        private int EditingAssignmentId
        {
            get { return ViewState["EditAssignmentId"] is int ? (int)ViewState["EditAssignmentId"] : 0; }
            set { ViewState["EditAssignmentId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Web.config already blocks everyone who is not an Admin from reaching this page.
            if (IsPostBack) return;

            txtSearch.Text = Limit((Request.QueryString["q"] ?? "").Trim(), 100);
            SelectIfExists(ddlStatusFilter, Request.QueryString["status"]);
            SelectIfExists(ddlDueFilter, Request.QueryString["due"]);

            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1) page = 1;

            string flash;
            if (FlashMessages.TryGetValue(Request.QueryString["msg"] ?? "", out flash)) ShowMessage(flash, true);
            if (Request.QueryString["toolarge"] == "1") ShowMessage("That upload was too large. Instruction files may be up to 10 MB.", false);

            BindGrid(page);
        }

        // ---------- READ: the assignments list ----------

        private void BindGrid(int page)
        {
            string search = txtSearch.Text;
            string status = ddlStatusFilter.SelectedValue;
            string due = ddlDueFilter.SelectedValue;

            const string where =
                "FROM Assignments a " +
                "WHERE (@Search IS NULL OR a.Title LIKE @Search OR a.Description LIKE @Search) " +
                "AND (@Status IS NULL OR a.Status = @Status) " +
                "AND (@Due IS NULL OR (@Due = 'Open' AND a.DueDate >= GETDATE()) OR (@Due = 'Past' AND a.DueDate < GETDATE())) ";

            Func<SqlParameter[]> filters = () => new[] {
                DBHelper.Param("@Search", search.Length == 0 ? null : "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%"),
                DBHelper.Param("@Status", status.Length == 0 ? null : status),
                DBHelper.Param("@Due", due.Length == 0 ? null : due) };

            int total = (int)DBHelper.ExecuteScalar("SELECT COUNT(*) " + where, filters());
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            List<SqlParameter> paging = new List<SqlParameter>(filters());
            paging.Add(DBHelper.Param("@Skip", (page - 1) * PageSize));
            paging.Add(DBHelper.Param("@Take", PageSize));

            gvAssignments.DataSource = DBHelper.GetDataTable(
                "SELECT a.AssignmentID, a.Title, a.DueDate, a.MaxMarks, a.Status, " +
                "  CASE WHEN a.DueDate < GETDATE() THEN 1 ELSE 0 END AS IsPastDue, " +
                "  (SELECT COUNT(*) FROM Submissions s WHERE s.AssignmentID = a.AssignmentID) AS SubmissionCount, " +
                "  (SELECT COUNT(*) FROM Submissions s WHERE s.AssignmentID = a.AssignmentID AND s.Status = 'Submitted') AS ToGrade " +
                where + "ORDER BY a.DueDate DESC, a.AssignmentID DESC OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", paging.ToArray());
            gvAssignments.DataBind();

            lblSummary.Text = total == 0 ? "" : total + (total == 1 ? " assignment" : " assignments");
            litPager.Text = Utility.BuildPagerHtml(n => ResolveUrl(ListUrl(n, null)), page, totalPages, "Assignment pages");
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (gvAssignments.HeaderRow != null) gvAssignments.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect(ListUrl(1, null));
        }

        private string ListUrl(int page, string message)
        {
            StringBuilder url = new StringBuilder("~/Admin/ManageAssignments?page=" + page);
            if (txtSearch.Text.Length > 0) url.Append("&q=" + HttpUtility.UrlEncode(txtSearch.Text));
            if (ddlStatusFilter.SelectedValue.Length > 0) url.Append("&status=" + ddlStatusFilter.SelectedValue);
            if (ddlDueFilter.SelectedValue.Length > 0) url.Append("&due=" + ddlDueFilter.SelectedValue);
            if (message != null) url.Append("&msg=" + message);
            return url.ToString();
        }

        private int CurrentPage()
        {
            int page;
            return (int.TryParse(Request.QueryString["page"], out page) && page > 0) ? page : 1;
        }

        // ---------- row buttons ----------

        protected void gvAssignments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            switch (e.CommandName)
            {
                case "EditAssignment": ShowForm(id); break;
                case "TogglePublish": TogglePublish(id); break;
                case "DeleteAssignment": DeleteAssignment(id); break;
            }
        }

        // UPDATE: Published <-> Draft
        private void TogglePublish(int id)
        {
            DataTable dt = DBHelper.GetDataTable("SELECT Status FROM Assignments WHERE AssignmentID = @Id", DBHelper.Param("@Id", id));
            if (dt.Rows.Count == 0) { ShowMessage("That assignment no longer exists.", false); return; }

            bool wasPublished = (string)dt.Rows[0]["Status"] == "Published";
            DBHelper.ExecuteNonQuery("UPDATE Assignments SET Status = @Status WHERE AssignmentID = @Id",
                DBHelper.Param("@Status", wasPublished ? "Draft" : "Published"), DBHelper.Param("@Id", id));

            Response.Redirect(ListUrl(CurrentPage(), wasPublished ? "unpublished" : "published"));
        }

        // DELETE: the database deletes the assignment's submissions with it (ON DELETE CASCADE),
        // so we first collect every file path, then remove the files from disk afterwards.
        private void DeleteAssignment(int id)
        {
            DataTable exists = DBHelper.GetDataTable("SELECT FilePath FROM Assignments WHERE AssignmentID = @Id", DBHelper.Param("@Id", id));
            if (exists.Rows.Count == 0) { ShowMessage("That assignment no longer exists.", false); return; }

            List<string> files = new List<string>();
            if (exists.Rows[0]["FilePath"] is string) files.Add((string)exists.Rows[0]["FilePath"]);
            foreach (DataRow row in DBHelper.GetDataTable("SELECT FilePath FROM Submissions WHERE AssignmentID = @Id", DBHelper.Param("@Id", id)).Rows)
            {
                files.Add((string)row["FilePath"]);
            }

            DBHelper.ExecuteNonQuery("DELETE FROM Assignments WHERE AssignmentID = @Id", DBHelper.Param("@Id", id));

            foreach (string path in files) FileHelper.DeleteUploadedFile(Context, path);

            Response.Redirect(ListUrl(CurrentPage(), "deleted"));
        }

        // ---------- the Add / Edit form ----------

        protected void btnAdd_Click(object sender, EventArgs e) { ShowForm(0); }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            EditingAssignmentId = 0;
        }

        private void ShowForm(int id)
        {
            EditingAssignmentId = id;
            pnlForm.Visible = true;
            lblCurrentFile.Text = "";
            chkRemoveFile.Visible = false;
            chkRemoveFile.Checked = false;

            if (id == 0)
            {
                lblFormTitle.Text = "Add assignment";
                txtTitle.Text = txtDescription.Text = txtInstructions.Text = "";
                txtDueDate.Text = DateTime.Now.AddDays(7).Date.AddHours(23).AddMinutes(59).ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture);
                txtMaxMarks.Text = "100";
                ddlStatus.SelectedValue = "Published";
                return;
            }

            DataTable dt = DBHelper.GetDataTable(
                "SELECT Title, Description, Instructions, DueDate, MaxMarks, FilePath, Status FROM Assignments WHERE AssignmentID = @Id",
                DBHelper.Param("@Id", id));

            if (dt.Rows.Count == 0)
            {
                pnlForm.Visible = false;
                EditingAssignmentId = 0;
                ShowMessage("That assignment no longer exists.", false);
                return;
            }

            DataRow a = dt.Rows[0];
            lblFormTitle.Text = "Edit assignment";
            txtTitle.Text = (string)a["Title"];
            txtDescription.Text = (string)a["Description"];
            txtInstructions.Text = (a["Instructions"] as string) ?? "";
            txtDueDate.Text = ((DateTime)a["DueDate"]).ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture);
            txtMaxMarks.Text = a["MaxMarks"].ToString();
            ddlStatus.SelectedValue = (string)a["Status"];

            string file = a["FilePath"] as string;
            if (!string.IsNullOrEmpty(file))
            {
                lblCurrentFile.Text = "Current file: " + Path.GetFileName(file) + " (choose a new file to replace it)";
                chkRemoveFile.Visible = true;
            }
        }

        // ---------- server-side validation ----------

        private static bool TryParseDue(string text, out DateTime value)
        {
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
        }

        // A NEW assignment needs a deadline in the future. When editing, any valid date is allowed
        // (so an admin can extend a deadline or correct an old assignment).
        protected void cvDueDate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime due;
            if (!TryParseDue(args.Value, out due))
            {
                cvDueDate.ErrorMessage = "Please enter a valid date and time.";
                args.IsValid = false;
            }
            else if (EditingAssignmentId == 0 && due <= DateTime.Now)
            {
                cvDueDate.ErrorMessage = "The due date of a new assignment must be in the future.";
                args.IsValid = false;
            }
            else if (due.Year < 2000 || due.Year > 2100)
            {
                cvDueDate.ErrorMessage = "Please enter a realistic due date.";
                args.IsValid = false;
            }
        }

        // Marks that were already given must still fit inside the new maximum.
        protected void cvMaxMarks_ServerValidate(object source, ServerValidateEventArgs args)
        {
            int newMax;
            if (EditingAssignmentId == 0 || !int.TryParse(args.Value, out newMax)) return;

            object highest = DBHelper.ExecuteScalar(
                "SELECT MAX(Marks) FROM Submissions WHERE AssignmentID = @Id", DBHelper.Param("@Id", EditingAssignmentId));

            if (highest != null && Convert.ToDecimal(highest) > newMax)
            {
                cvMaxMarks.ErrorMessage = "Students already have up to " + Convert.ToDecimal(highest).ToString("0.##") +
                                          " marks, so the maximum cannot be lower than that.";
                args.IsValid = false;
            }
        }

        protected void cvFile_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!fuInstructions.HasFile) return;    // optional

            HttpPostedFile file = fuInstructions.PostedFile;
            string problem = null;

            if (file.ContentLength == 0) problem = "That file is empty.";
            else if (file.ContentLength > FileHelper.MaxSubmissionBytes) problem = "That file is larger than 10 MB.";
            else if (!FileHelper.IsAllowedExtension(file.FileName, FileHelper.SubmissionExtensions))
                problem = "That file type is not allowed. Use PDF, DOC, DOCX, PPT, PPTX, ZIP or TXT.";
            else if (!FileHelper.ContentMatchesExtension(file)) problem = "The contents of that file do not match its type.";

            if (problem != null) { cvFile.ErrorMessage = problem; args.IsValid = false; }
        }

        // ---------- INSERT / UPDATE ----------

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate("AssignmentForm");
            if (!Page.IsValid) return;

            int id = EditingAssignmentId;
            DateTime due;
            TryParseDue(txtDueDate.Text, out due);

            string oldFile = null;
            if (id != 0)
            {
                DataTable dt = DBHelper.GetDataTable("SELECT FilePath FROM Assignments WHERE AssignmentID = @Id", DBHelper.Param("@Id", id));
                if (dt.Rows.Count == 0) { ShowMessage("That assignment no longer exists.", false); pnlForm.Visible = false; return; }
                oldFile = dt.Rows[0]["FilePath"] as string;
            }

            // Which file will the assignment have afterwards?
            string newFile = null;
            string finalFile = oldFile;
            try
            {
                if (fuInstructions.HasFile)
                {
                    newFile = SaveInstructionFile();
                    finalFile = newFile;
                }
                else if (chkRemoveFile.Checked)
                {
                    finalFile = null;
                }

                List<SqlParameter> p = new List<SqlParameter>
                {
                    DBHelper.Param("@Title", txtTitle.Text.Trim()),
                    DBHelper.Param("@Description", txtDescription.Text.Trim()),
                    DBHelper.Param("@Instructions", txtInstructions.Text.Trim().Length == 0 ? null : txtInstructions.Text.Trim()),
                    DBHelper.Param("@DueDate", due),
                    DBHelper.Param("@MaxMarks", int.Parse(txtMaxMarks.Text)),
                    DBHelper.Param("@FilePath", finalFile),
                    DBHelper.Param("@Status", ddlStatus.SelectedValue)
                };

                if (id == 0)
                {
                    p.Add(DBHelper.Param("@CreatedBy", Utility.CurrentUserId));
                    DBHelper.ExecuteNonQuery(
                        "INSERT INTO Assignments (Title, Description, Instructions, DueDate, MaxMarks, FilePath, CreatedBy, Status) " +
                        "VALUES (@Title, @Description, @Instructions, @DueDate, @MaxMarks, @FilePath, @CreatedBy, @Status)", p.ToArray());
                }
                else
                {
                    p.Add(DBHelper.Param("@Id", id));
                    DBHelper.ExecuteNonQuery(
                        "UPDATE Assignments SET Title = @Title, Description = @Description, Instructions = @Instructions, DueDate = @DueDate, " +
                        "MaxMarks = @MaxMarks, FilePath = @FilePath, Status = @Status WHERE AssignmentID = @Id", p.ToArray());
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ManageAssignments: could not save the assignment", ex);
                if (newFile != null) FileHelper.DeleteUploadedFile(Context, newFile);   // no orphan file
                ShowMessage("Sorry, the assignment could not be saved right now. Please try again.", false);
                return;
            }

            // The old instruction file is no longer used once it was replaced or removed.
            if (oldFile != null && oldFile != finalFile) FileHelper.DeleteUploadedFile(Context, oldFile);

            Response.Redirect(ListUrl(id == 0 ? 1 : CurrentPage(), id == 0 ? "created" : "updated"));
        }

        // Instruction files live in the private Uploads/Assignments folder ("brief_..." names).
        private string SaveInstructionFile()
        {
            string extension = Path.GetExtension(fuInstructions.PostedFile.FileName).ToLowerInvariant();
            string name = "brief_" + FileHelper.MakeSafeFileName(Path.GetFileNameWithoutExtension(fuInstructions.PostedFile.FileName), 50) +
                          "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
            string relative = "Uploads/Assignments/" + name;

            Directory.CreateDirectory(Server.MapPath("~/Uploads/Assignments"));
            fuInstructions.SaveAs(Server.MapPath("~/" + relative));
            return relative;
        }

        // ---------- helpers used by the page markup ----------

        protected string StatusClass(object status) { return Utility.StatusBadgeClass(status); }

        protected string SubmissionsUrl(object assignmentId)
        {
            return ResolveUrl("~/Admin/ManageSubmissions?assignment=" + assignmentId);
        }

        protected string SubmissionText(object total, object toGrade)
        {
            int all = (int)total, pending = (int)toGrade;
            if (all == 0) return "<span class=\"text-muted\">none yet</span>";
            return all + (all == 1 ? " submission" : " submissions") +
                   (pending > 0 ? "<div class=\"dash-meta\">" + pending + " to grade</div>" : "<div class=\"dash-meta\">all graded</div>");
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
