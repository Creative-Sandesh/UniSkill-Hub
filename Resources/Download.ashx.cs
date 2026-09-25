using System;
using System.Data;
using System.Web;
using System.Web.Security;

namespace UniSkillHub.Resources
{
    /// <summary>
    /// Sends a resource file to a logged-in user.
    ///
    /// The URL only contains the resource ID (Download.ashx?id=3). The file path is
    /// read from the database, so a visitor can never ask for an arbitrary file, and
    /// FileHelper checks that the path is inside the /Uploads folder before sending it.
    /// </summary>
    public class Download : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // 1. Authentication: only logged-in users may download.
            if (!context.Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage();
                return;
            }

            // 2. The resource must exist, be published and have a file.
            int resourceId;
            if (!int.TryParse(context.Request.QueryString["id"], out resourceId))
            {
                FileHelper.SendNotFound(context);
                return;
            }

            // (An Admin may also download the file of a draft, to check it before publishing.)
            DataTable dt = DBHelper.GetDataTable(
                "SELECT FilePath FROM Resources " +
                "WHERE ResourceID = @ResourceID AND (Status = 'Published' OR @IsAdmin = 1) AND FilePath IS NOT NULL",
                DBHelper.Param("@ResourceID", resourceId),
                DBHelper.Param("@IsAdmin", context.User.IsInRole("Admin") ? 1 : 0));

            if (dt.Rows.Count == 0)
            {
                FileHelper.SendNotFound(context);
                return;
            }

            // 3. Send it (refused if the path escapes /Uploads or the file is missing).
            if (!FileHelper.SendDownload(context, (string)dt.Rows[0]["FilePath"]))
            {
                FileHelper.SendNotFound(context);
            }
        }
    }
}
