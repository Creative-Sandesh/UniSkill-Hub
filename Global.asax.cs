using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.Management;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace UniSkillHub
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // Runs on every request after Forms Authentication has read the login cookie.
        // The login ticket stores "Role|UserID|SessionId" in its UserData; here we turn that
        // into the user's role so Web.config <authorization roles="..."> and
        // User.IsInRole("Admin") work. The ticket is encrypted, so it cannot be edited.
        void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;

            FormsIdentity identity = context.User != null ? context.User.Identity as FormsIdentity : null;
            if (identity == null || !identity.IsAuthenticated) return;

            string[] data = identity.Ticket.UserData.Split('|');
            int userId;
            if (data.Length >= 2 && int.TryParse(data[1], out userId))
            {
                string role = data[0];

                // This login was ended by Logout (so a copied cookie stops working too).
                if (SessionRevocation.IsRevoked(SessionRevocation.SessionIdOf(identity.Ticket)))
                {
                    FormsAuthentication.SignOut();
                    context.User = new GenericPrincipal(new GenericIdentity(""), new string[0]);
                    return;
                }

                // The login cookie can live for days, but an admin may deactivate or demote a
                // user at any time. So for every page request we re-check the account in the
                // database: an inactive/deleted user is signed out on the spot, and the role
                // always comes from the database (a demotion applies immediately).
                if (ShouldCheckAccount(context.Request))
                {
                    string currentRole;
                    if (!AccountCheck.TryGetActiveRole(userId, out currentRole))
                    {
                        FormsAuthentication.SignOut();
                        context.User = new GenericPrincipal(new GenericIdentity(""), new string[0]);
                        return;
                    }
                    role = currentRole ?? role;
                }

                context.User = new GenericPrincipal(identity, new string[] { role });
                context.Items["UserID"] = userId;
            }
        }

        // Only pages and handlers are checked - not css, js, images or downloads of static files.
        private static bool ShouldCheckAccount(HttpRequest request)
        {
            string extension = VirtualPathUtility.GetExtension(request.AppRelativeCurrentExecutionFilePath) ?? "";
            extension = extension.ToLowerInvariant();
            return extension == "" || extension == ".aspx" || extension == ".ashx";
        }

        // ASP.NET refuses uploads bigger than maxRequestLength before any page code runs
        // and would show its technical error page. For the assignment upload page we
        // send the student back to the form with a friendly "file too large" message.
        void Application_Error(object sender, EventArgs e)
        {
            // Write every real error to App_Data/Logs first (a plain 404 is not worth logging).
            Exception lastError = Server.GetLastError();
            HttpException lastHttp = lastError as HttpException;
            if (lastError != null && !(lastHttp != null && lastHttp.GetHttpCode() == 404))
            {
                Logger.Error("Unhandled error", lastError);
            }

            // The "too large" error is usually wrapped inside another exception
            // (HttpUnhandledException), so look through the whole chain.
            bool tooLarge = false;
            for (Exception ex = Server.GetLastError(); ex != null; ex = ex.InnerException)
            {
                HttpException http = ex as HttpException;
                if (http != null && http.WebEventCode == WebEventCodes.RuntimeErrorPostTooLarge)
                {
                    tooLarge = true;
                    break;
                }
            }

            // ASP.NET refuses input that looks like an HTML tag (request validation). Keep that
            // protection, but show a clear message instead of the technical error page.
            for (Exception ex = Server.GetLastError(); ex != null; ex = ex.InnerException)
            {
                if (ex is HttpRequestValidationException)
                {
                    Server.ClearError();
                    Response.Redirect("~/Error?code=input", false);
                    CompleteRequest();
                    return;
                }

                // A form that was not loaded by this browser (see the anti-CSRF check in Site.Master.cs).
                // (a form built for another browser also fails the ViewState check, which is tied to the same token)
                if (ex is AntiForgeryException || ex is ViewStateException)
                {
                    Server.ClearError();
                    Response.Redirect("~/Error?code=csrf", false);
                    CompleteRequest();
                    return;
                }
            }

            string path = Request.Url.AbsolutePath;
            bool isSubmitPage = path.IndexOf("SubmitAssignment", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isAdminUploadPage = path.IndexOf("/Admin/Manage", StringComparison.OrdinalIgnoreCase) >= 0;

            if (tooLarge && (isSubmitPage || isAdminUploadPage))
            {
                Server.ClearError();

                // Back to the same page, with a flag that makes it show the "too large" message.
                string target = isSubmitPage
                    ? path + "?id=" + HttpUtility.UrlEncode(Request.QueryString["id"] ?? "") + "&toolarge=1"
                    : path + "?toolarge=1";
                Response.Redirect(target, false);
                CompleteRequest();
            }
        }
    }
}
