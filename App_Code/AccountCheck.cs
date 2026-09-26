using System;
using System.Data;
using System.Web;

namespace UniSkillHub
{
    /// <summary>
    /// Answers "is this login still allowed, and what is its role?" for Global.asax, which asks on
    /// every page request. Asking the database every time is wasteful, so a definite answer is
    /// remembered for a short time (CacheSeconds). Whenever an admin changes a user's role or status
    /// (or deletes the user) the page calls Forget(userId), so on this server the change still applies
    /// at once; the short timeout is only the fallback for changes made outside the site.
    /// </summary>
    public static class AccountCheck
    {
        private const int CacheSeconds = 30;
        private const string InactiveMarker = "!inactive";

        private static string Key(int userId)
        {
            return "account|" + userId;
        }

        /// <summary>Call after a user's role/status is changed or the user is deleted.</summary>
        public static void Forget(int userId)
        {
            HttpRuntime.Cache.Remove(Key(userId));
        }

        /// <summary>
        /// Returns false when the account no longer exists or is not Active.
        /// If the database cannot be reached the check is skipped (the page itself would fail anyway),
        /// but the problem is written to the log.
        /// </summary>
        public static bool TryGetActiveRole(int userId, out string role)
        {
            role = null;

            string cached = HttpRuntime.Cache[Key(userId)] as string;
            if (cached != null)
            {
                if (cached == InactiveMarker) return false;
                role = cached;
                return true;
            }

            try
            {
                DataTable dt = DBHelper.GetDataTable(
                    "SELECT Role, Status FROM Users WHERE UserID = @UserID",
                    DBHelper.Param("@UserID", userId));

                bool active = dt.Rows.Count > 0 && (string)dt.Rows[0]["Status"] == "Active";
                if (active) role = (string)dt.Rows[0]["Role"];

                HttpRuntime.Cache.Insert(Key(userId), active ? role : InactiveMarker, null,
                    DateTime.UtcNow.AddSeconds(CacheSeconds), System.Web.Caching.Cache.NoSlidingExpiration);
                return active;
            }
            catch (Exception ex)
            {
                Logger.Error("Could not check the account status of user " + userId + "; allowing the request", ex);
                return true;
            }
        }
    }
}
