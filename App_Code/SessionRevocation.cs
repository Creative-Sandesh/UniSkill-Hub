using System;
using System.Collections.Concurrent;
using System.Web;
using System.Web.Security;

namespace UniSkillHub
{
    /// <summary>
    /// Makes "Logout" really end that login. A Forms Authentication cookie is self-contained, so
    /// without this a copy of the cookie would keep working until it expired.
    /// Every login ticket carries a random session id (its UserData is "Role|UserID|SessionId").
    /// On logout the id is remembered here, and Global.asax refuses any ticket that carries it -
    /// including a copy of the cookie. Other logins of the same user (another browser or device)
    /// have their own id and are not affected.
    /// The list is kept in memory, so it is forgotten if the site restarts; the ticket's own
    /// expiry (30 minutes, or 7 days for "Keep me logged in") is the final limit.
    /// </summary>
    public static class SessionRevocation
    {
        // session id -> when the note can be forgotten (longer than any ticket can live)
        private static readonly ConcurrentDictionary<string, DateTime> Revoked = new ConcurrentDictionary<string, DateTime>();
        private static DateTime _lastSweep = DateTime.UtcNow;

        public static string NewSessionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>Called by the Logout page: refuse the login the current request belongs to.</summary>
        public static void RevokeCurrent(HttpContext context)
        {
            FormsIdentity identity = context.User != null ? context.User.Identity as FormsIdentity : null;
            string id = SessionIdOf(identity != null ? identity.Ticket : null);
            if (id == null) return;

            Revoked[id] = DateTime.UtcNow.AddDays(8);
            Sweep();
        }

        public static bool IsRevoked(string sessionId)
        {
            return sessionId != null && Revoked.ContainsKey(sessionId);
        }

        /// <summary>The session id stored in the ticket, or null for a ticket without one.</summary>
        public static string SessionIdOf(FormsAuthenticationTicket ticket)
        {
            if (ticket == null || ticket.UserData == null) return null;
            string[] parts = ticket.UserData.Split('|');
            return parts.Length >= 3 ? parts[2] : null;
        }

        private static void Sweep()
        {
            DateTime now = DateTime.UtcNow;
            if (now - _lastSweep < TimeSpan.FromHours(1)) return;
            _lastSweep = now;

            foreach (var pair in Revoked)
            {
                DateTime ignored;
                if (pair.Value < now) Revoked.TryRemove(pair.Key, out ignored);
            }
        }
    }
}
