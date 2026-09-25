using System;
using System.Security.Cryptography;

namespace UniSkillHub
{
    /// <summary>
    /// Password hashing with a random salt (PBKDF2 using SHA-256).
    /// Plaintext passwords are never stored: the database keeps only
    /// PasswordHash and PasswordSalt (both Base64 text).
    ///
    /// Register : string salt = PasswordHelper.GenerateSalt();
    ///            string hash = PasswordHelper.HashPassword(password, salt);
    /// Login    : bool ok = PasswordHelper.VerifyPassword(password, storedHash, storedSalt);
    /// </summary>
    public static class PasswordHelper
    {
        private const int SaltSize = 16;        // bytes
        private const int HashSize = 32;        // bytes
        private const int Iterations = 100000;  // slows down brute-force guessing

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
        /// Hashes the password with the given Base64 salt. Returns Base64 text.
        /// </summary>
        public static string HashPassword(string password, string base64Salt)
        {
            byte[] salt = Convert.FromBase64String(base64Salt);
            using (Rfc2898DeriveBytes pbkdf2 =
                new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
            }
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

            byte[] expected = Convert.FromBase64String(storedHash);
            byte[] actual = Convert.FromBase64String(HashPassword(password, storedSalt));
            return ConstantTimeEquals(expected, actual);
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
