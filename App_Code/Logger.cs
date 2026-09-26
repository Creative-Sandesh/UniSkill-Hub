using System;
using System.IO;
using System.Text;
using System.Web;

namespace UniSkillHub
{
    /// <summary>
    /// Writes errors to a text file, one file per day: App_Data/Logs/error-yyyyMMdd.log.
    /// App_Data is never served by IIS, so visitors cannot read the log. Visitors still only see
    /// the friendly messages; this file is where the developer finds out what really went wrong.
    /// Logging must never break a request, so any problem while writing the log is ignored.
    /// Only the message and the technical exception are written - never passwords or form values.
    /// </summary>
    public static class Logger
    {
        private static readonly object Sync = new object();

        /// <summary>Records what the page was doing and the exception that stopped it.</summary>
        public static void Error(string what, Exception ex)
        {
            Write("ERROR", what, ex);
        }

        public static void Warning(string what)
        {
            Write("WARN ", what, null);
        }

        private static void Write(string level, string what, Exception ex)
        {
            try
            {
                StringBuilder line = new StringBuilder();
                line.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append(' ').Append(level).Append(' ').Append(what);

                HttpContext context = HttpContext.Current;
                if (context != null && context.Request != null)
                {
                    line.Append(" | ").Append(context.Request.HttpMethod).Append(' ').Append(context.Request.Url.AbsolutePath);
                    if (context.Items["UserID"] != null) line.Append(" | user ").Append(context.Items["UserID"]);
                }

                for (Exception e = ex; e != null; e = e.InnerException)
                {
                    line.AppendLine().Append("    ").Append(e.GetType().Name).Append(": ").Append(e.Message);
                    if (e.InnerException == null && e.StackTrace != null) line.AppendLine().Append(e.StackTrace);
                }

                string folder = AppDomain.CurrentDomain.BaseDirectory;
                folder = Path.Combine(folder, "App_Data", "Logs");
                Directory.CreateDirectory(folder);

                string file = Path.Combine(folder, "error-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                lock (Sync)
                {
                    File.AppendAllText(file, line.AppendLine().ToString(), Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                // A log that cannot be written is not worth failing the request.
            }
        }
    }
}
