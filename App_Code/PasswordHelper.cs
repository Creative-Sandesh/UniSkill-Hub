using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace UniSkillHub
{
    /// <summary>
    /// Password hashing with a random salt (PBKDF2 using SHA-256).
    /// Plaintext passwords are never stored: the database keeps only
    /// PasswordHash and PasswordSalt.
    ///
    /// New hashes are stored as text "pbkdf2$600000$&lt;Base64 hash&gt;", so the number of iterations
    /// travels with the hash and can be raised later without breaking existing accounts.
    /// Older accounts have a plain Base64 hash made with 100,000 iterations; those still verify,
    /// and Login re-hashes them with the current settings after the next successful login
    /// (see NeedsRehash).
    ///
    /// Register : string salt = PasswordHelper.GenerateSalt();
    ///            string hash = PasswordHelper.HashPassword(password, salt);
    /// Login    : bool ok = PasswordHelper.VerifyPassword(password, storedHash, storedSalt);
    ///            if (ok &amp;&amp; PasswordHelper.NeedsRehash(storedHash)) { ...store a new salt + hash... }
    /// </summary>
    public static class PasswordHelper
    {
        private const int SaltSize = 16;                 // bytes
        private const int HashSize = 32;                 // bytes
        private const int CurrentIterations = 600000;    // slows down brute-force guessing (OWASP guidance for PBKDF2-SHA256)
        private const int LegacyIterations = 100000;     // hashes without the "pbkdf2$" prefix were made with this
        private const string Prefix = "pbkdf2$";

        /// <summary>
        /// Creates a new random salt, returned as Base64 text.
        /// </summary>
        public static string GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// Hashes the password with the given Base64 salt using the current settings.
        /// Returns the text to store in Users.PasswordHash.
        /// </summary>
        public static string HashPassword(string password, string base64Salt)
        {
            return Prefix + CurrentIterations + "$" + Convert.ToBase64String(Derive(password, base64Salt, CurrentIterations));
        }

        /// <summary>
        /// Checks a login password against the stored hash and salt.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(storedHash) ||
                string.IsNullOrEmpty(storedSalt))
            {
                return false;
            }

            int iterations;
            byte[] expected;
            if (!TryParse(storedHash, out iterations, out expected)) return false;

            byte[] actual = Derive(password, storedSalt, iterations);
            return ConstantTimeEquals(expected, actual);
        }

        /// <summary>
        /// True when the stored hash was made with older (weaker) settings than the current ones.
        /// Only call this after VerifyPassword succeeded - that is the only moment the plain password is known.
        /// </summary>
        public static bool NeedsRehash(string storedHash)
        {
            int iterations;
            byte[] ignored;
            return TryParse(storedHash, out iterations, out ignored) && iterations < CurrentIterations;
        }

        // Reads "pbkdf2$<iterations>$<Base64>" or a legacy plain Base64 hash.
        private static bool TryParse(string storedHash, out int iterations, out byte[] hash)
        {
            iterations = LegacyIterations;
            hash = null;

            try
            {
                string base64 = storedHash;
                if (storedHash.StartsWith(Prefix, StringComparison.Ordinal))
                {
                    string[] parts = storedHash.Split('$');
                    if (parts.Length != 3 || !int.TryParse(parts[1], out iterations) || iterations < 1) return false;
                    base64 = parts[2];
                }

                hash = Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static byte[] Derive(string password, string base64Salt, int iterations)
        {
            byte[] salt = Convert.FromBase64String(base64Salt);

            // .NET Framework's own PBKDF2 is slow (over a second at 600,000 iterations), so the same
            // calculation is done by Windows' built-in crypto library when it is available.
            // Both give exactly the same result, so hashes made either way verify either way.
            byte[] fast = TryDeriveWithWindows(password, salt, iterations);
            if (fast != null) return fast;

            using (Rfc2898DeriveBytes pbkdf2 =
                new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        // ---------- Windows CNG (bcrypt.dll) ----------

        [DllImport("bcrypt.dll", CharSet = CharSet.Unicode)]
        private static extern uint BCryptOpenAlgorithmProvider(out IntPtr hAlgorithm, string algorithmId, string implementation, uint flags);

        [DllImport("bcrypt.dll")]
        private static extern uint BCryptDeriveKeyPBKDF2(IntPtr hPrf, byte[] password, uint passwordLength,
            byte[] salt, uint saltLength, ulong iterations, byte[] derivedKey, uint derivedKeyLength, uint flags);

        [DllImport("bcrypt.dll")]
        private static extern uint BCryptCloseAlgorithmProvider(IntPtr hAlgorithm, uint flags);

        private const uint BCRYPT_ALG_HANDLE_HMAC_FLAG = 0x00000008;

        // Returns null when the Windows routine cannot be used (the caller then uses the .NET one).
        private static byte[] TryDeriveWithWindows(string password, byte[] salt, int iterations)
        {
            byte[] passwordBytes = new UTF8Encoding(false).GetBytes(password);   // the same bytes Rfc2898DeriveBytes uses
            if (passwordBytes.Length == 0) return null;

            IntPtr algorithm = IntPtr.Zero;
            try
            {
                if (BCryptOpenAlgorithmProvider(out algorithm, "SHA256", null, BCRYPT_ALG_HANDLE_HMAC_FLAG) != 0) return null;

                byte[] key = new byte[HashSize];
                uint status = BCryptDeriveKeyPBKDF2(algorithm, passwordBytes, (uint)passwordBytes.Length,
                    salt, (uint)salt.Length, (ulong)iterations, key, (uint)key.Length, 0);
                return status == 0 ? key : null;
            }
            catch (DllNotFoundException) { return null; }
            catch (EntryPointNotFoundException) { return null; }
            finally
            {
                if (algorithm != IntPtr.Zero) BCryptCloseAlgorithmProvider(algorithm, 0);
            }
        }

        // Compares every byte (no early exit) so timing does not leak
        // how much of the hash matched.
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            int difference = 0;
            for (int i = 0; i < a.Length; i++)
            {
                difference |= a[i] ^ b[i];
            }
            return difference == 0;
        }
    }
}
