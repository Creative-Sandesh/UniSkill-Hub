using System;
using System.Web;

namespace UniSkillHub
{
    /// <summary>
    /// Small helpers shared by several pages.
    /// </summary>
    public static class Utility
    {
        /// <summary>Link to the resource list filtered to one category (the id is converted to a number first).</summary>
        public static string CategoryLinkUrl(object categoryId)
        {
            return VirtualPathUtility.ToAbsolute("~/Resources/BrowseResources") + "?category=" + Convert.ToInt32(categoryId);
        }

        /// <summary>
        /// The picture that goes with a resource category (Images/categories/*.svg).
        /// Only a fixed list of file names is ever used - the category name from the database is
        /// never put into a path - and any other category gets the general "default" picture.
        /// </summary>
        public static string CategoryImageUrl(object categoryName)
        {
            string slug = (Convert.ToString(categoryName) ?? "").Trim().ToLowerInvariant().Replace(' ', '-');
            switch (slug)
            {
                case "programming":
                case "database":
                case "web-development":
                case "networking":
                case "hci":
                    break;
                default:
                    slug = "default";
                    break;
            }
            return VirtualPathUtility.ToAbsolute("~/Images/categories/" + slug + ".svg");
        }

        /// <summary>
        /// The UserID of the logged-in user, or 0 if nobody is logged in.
        /// (Global.asax reads it from the login ticket on every request.)
        /// </summary>
        public static int CurrentUserId
        {
            get
            {
                object id = HttpContext.Current.Items["UserID"];
                return (id is int) ? (int)id : 0;
            }
        }

        /// <summary>
        /// Home page of a role after login.
        /// </summary>
        public static string DashboardUrlForRole(string role)
        {
            return (role == "Admin") ? "~/Admin/Dashboard" : "~/Student/Dashboard";
        }

        /// <summary>
        /// Short human text for a deadline: "Due in 3 days", "Due in 5 hours", "Overdue".
        /// </summary>
        public static string FriendlyDueText(DateTime dueDate)
        {
            TimeSpan left = dueDate - DateTime.Now;

            if (left.TotalSeconds <= 0) return "Overdue";
            if (left.TotalHours < 1) return "Due in " + Math.Max(1, (int)left.TotalMinutes) + " min";
            if (left.TotalDays < 1) return "Due in " + (int)left.TotalHours + (((int)left.TotalHours == 1) ? " hour" : " hours");

            int days = (int)Math.Ceiling(left.TotalDays);
            return "Due in " + days + (days == 1 ? " day" : " days");
        }

        /// <summary>
        /// Bootstrap "Previous 1 2 3 Next" pager. pageUrl(n) must return the ready-to-use
        /// link of page n. Returns "" when there is only one page.
        /// </summary>
        public static string BuildPagerHtml(Func<int, string> pageUrl, int currentPage, int totalPages, string ariaLabel)
        {
            if (totalPages <= 1) return "";

            System.Text.StringBuilder html = new System.Text.StringBuilder();
            html.Append("<nav aria-label=\"" + HttpUtility.HtmlAttributeEncode(ariaLabel) + "\"><ul class=\"pagination justify-content-center mt-5\">");

            html.Append(PagerItem("Previous", pageUrl, currentPage - 1, currentPage > 1, false));
            for (int i = 1; i <= totalPages; i++)
            {
                html.Append(PagerItem(i.ToString(), pageUrl, i, true, i == currentPage));
            }
            html.Append(PagerItem("Next", pageUrl, currentPage + 1, currentPage < totalPages, false));

            html.Append("</ul></nav>");
            return html.ToString();
        }

        private static string PagerItem(string text, Func<int, string> pageUrl, int targetPage, bool enabled, bool current)
        {
            if (!enabled)
            {
                return "<li class=\"page-item disabled\"><span class=\"page-link\">" + text + "</span></li>";
            }

            return "<li class=\"page-item" + (current ? " active\" aria-current=\"page" : "") + "\">" +
                   "<a class=\"page-link\" href=\"" + HttpUtility.HtmlAttributeEncode(pageUrl(targetPage)) + "\">" + text + "</a></li>";
        }

        /// <summary>
        /// Safely shows plain text in a page: HTML-encodes it first, then turns
        /// blank lines into paragraphs and single line breaks into &lt;br&gt;.
        /// </summary>
        public static string TextToHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            string encoded = HttpUtility.HtmlEncode(text).Replace("\r\n", "\n");
            return "<p>" + encoded.Replace("\n\n", "</p><p>").Replace("\n", "<br />") + "</p>";
        }

        /// <summary>
        /// CSS class for a submission status badge (Pending, Overdue, Submitted, Graded).
        /// </summary>
        public static string StatusBadgeClass(object status)
        {
            return "status-badge status-" + Convert.ToString(status).ToLowerInvariant();
        }

        /// <summary>
        /// Returns the ReturnUrl only if it is safe to redirect to after login:
        /// a local path inside the role's own folder. Otherwise returns null.
        /// This stops "open redirect" tricks such as ReturnUrl=http://evil.com.
        /// </summary>
        public static string GetSafeReturnUrl(string returnUrl, string role)
        {
            if (string.IsNullOrEmpty(returnUrl)) return null;

            // must be a single-slash local path
            if (!returnUrl.StartsWith("/") || returnUrl.StartsWith("//") || returnUrl.StartsWith("/\\"))
            {
                return null;
            }

            string folder = (role == "Admin") ? "Admin/" : "Student/";
            string allowedPrefix = VirtualPathUtility.ToAbsolute("~/") + folder;

            return returnUrl.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase) ? returnUrl : null;
        }
    }
}
