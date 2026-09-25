using System;
using System.Data;
using System.Web;
using System.Web.Security;

namespace UniSkillHub.Assignments
{
    /// <summary>
    /// Sends assignment files to logged-in users.
    ///
    ///   Download.ashx?type=instructions&amp;id=AssignmentID  - the lecturer's instruction file
    ///   Download.ashx?type=submission&amp;id=SubmissionID    - a student's submitted work
    ///
    /// A submission can only be downloaded by the student who made it or by an Admin.
    /// Anyone else gets "404 Not Found" (so the existence of other people's work is not revealed).
    /// </summary>
    public class Download : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!context.Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
                return;
            }

            int id;
            if (!int.TryParse(context.Request.QueryString["id"], out id))
            {
                FileHelper.SendNotFound(context);
                return;
            }

            bool isAdmin = context.User.IsInRole("Admin");
            string relativePath = null;

            switch (context.Request.QueryString["type"])
            {
                case "instructions":
                    relativePath = FindInstructionFile(id, isAdmin);
                    break;
                case "submission":
                    relativePath = FindSubmissionFile(id, isAdmin);
                    break;
            }

            if (relativePath == null || !FileHelper.SendDownload(context, relativePath))
            {
                FileHelper.SendNotFound(context);
            }
        }

        // Students may only see instruction files of PUBLISHED assignments; admins see all.
        private static string FindInstructionFile(int assignmentId, bool isAdmin)
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT FilePath FROM Assignments " +
                "WHERE AssignmentID = @Id AND FilePath IS NOT NULL AND (Status = 'Published' OR @IsAdmin = 1)",
                DBHelper.Param("@Id", assignmentId),
                DBHelper.Param("@IsAdmin", isAdmin ? 1 : 0));

            return dt.Rows.Count == 0 ? null : (string)dt.Rows[0]["FilePath"];
        }

        // The owner check is part of the query itself: a row belonging to somebody
        // else is simply "not found".
        private static string FindSubmissionFile(int submissionId, bool isAdmin)
        {
            DataTable dt = DBHelper.GetDataTable(
                "SELECT FilePath FROM Submissions " +
                "WHERE SubmissionID = @Id AND (StudentID = @UserID OR @IsAdmin = 1)",
                DBHelper.Param("@Id", submissionId),
                DBHelper.Param("@UserID", Utility.CurrentUserId),
                DBHelper.Param("@IsAdmin", isAdmin ? 1 : 0));

            return dt.Rows.Count == 0 ? null : (string)dt.Rows[0]["FilePath"];
        }
    }
}
