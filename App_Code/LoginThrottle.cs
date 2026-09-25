using System;
using System.Collections.Generic;

namespace UniSkillHub
{
    /// <summary>
    /// Slows down password guessing. After 5 wrong passwords for the same account from the same
    /// computer within 15 minutes, further attempts from that computer are refused for 15 minutes
    /// (even with the right password). A second, wider limit stops one computer from trying
    /// many different accounts. The counters live in memory, so they reset when the site restarts.
    /// Keying on account + address (not the account alone) means a stranger cannot lock a real
    /// student out of their own account from somewhere else.
    /// </summary>
    public static class LoginThrottle
    {
        private const int MaxFailuresPerAccount = 5;
        private const int MaxFailuresPerAddress = 100;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan Lockout = TimeSpan.FromMinutes(15);

        private class Entry
        {
            public int Failures;
            public DateTime WindowStart;
            public DateTime LockedUntil;
        }

        private static readonly object Sync = new object();
        private static readonly Dictionary<string, Entry> Entries = new Dictionary<string, Entry>();

        private static string AccountKey(string address, string identifier)
        {
            string id = (identifier ?? "").Trim().ToLowerInvariant();
            if (id.Length > 100) id = id.Substring(0, 100);
            return "acct|" + address + "|" + id;
        }

        private static string AddressKey(string address)
        {
            return "addr|" + address;
        }

        /// <summary>True (and the minutes left) when this account/address pair may not try again yet.</summary>
        public static bool IsLocked(string address, string identifier, out int minutesLeft)
        {
            DateTime now = DateTime.UtcNow;
            DateTime until = DateTime.MinValue;

            lock (Sync)
            {
                foreach (string key in new[] { AccountKey(address, identifier), AddressKey(address) })
                {
                    Entry e;
                    if (Entries.TryGetValue(key, out e) && e.LockedUntil > now && e.LockedUntil > until)
                    {
                        until = e.LockedUntil;
                    }
                }
            }

            minutesLeft = until > now ? Math.Max(1, (int)Math.Ceiling((until - now).TotalMinutes)) : 0;
            return minutesLeft > 0;
        }

        public static void RecordFailure(string address, string identifier)
        {
            DateTime now = DateTime.UtcNow;

            lock (Sync)
            {
                Count(AccountKey(address, identifier), MaxFailuresPerAccount, now);
                Count(AddressKey(address), MaxFailuresPerAddress, now);
                RemoveOldEntries(now);
            }
        }

        /// <summary>A successful login clears the counter for that account/address pair.</summary>
        public static void Reset(string address, string identifier)
        {
            lock (Sync)
            {
                Entries.Remove(AccountKey(address, identifier));
            }
        }

        private static void Count(string key, int limit, DateTime now)
        {
            Entry e;
            if (!Entries.TryGetValue(key, out e) || now - e.WindowStart > Window)
            {
                e = new Entry { WindowStart = now };
                Entries[key] = e;
            }

            e.Failures++;
            if (e.Failures >= limit)
            {
                e.LockedUntil = now + Lockout;
                e.Failures = 0;
                e.WindowStart = now;
            }
        }

        // Keeps the dictionary small: forget entries that are neither locked nor inside their window.
        private static void RemoveOldEntries(DateTime now)
        {
            if (Entries.Count < 2000) return;

            List<string> old = new List<string>();
            foreach (KeyValuePair<string, Entry> pair in Entries)
            {
                if (pair.Value.LockedUntil <= now && now - pair.Value.WindowStart > Window) old.Add(pair.Key);
            }
            foreach (string key in old) Entries.Remove(key);
        }
    }
}
