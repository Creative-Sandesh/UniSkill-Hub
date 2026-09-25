/* ============================================================
   UniSkill Hub - first Admin account (development only)
   Run AFTER UniSkillHubDB.sql and BEFORE SampleData.sql.

   Login : admin   /   Admin@12345
   Change this password after the first login. It is only
   meant to get a fresh database started.

   No plaintext password is stored: PasswordHash is PBKDF2
   (SHA-256, 100,000 iterations) of the password above and
   PasswordSalt is its random salt - the same format that
   App_Code/PasswordHelper.cs produces and checks.
   Safe to run more than once: nothing happens if an Admin exists.
   ============================================================ */
USE UniSkillHubDB;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Role = N'Admin')
BEGIN
    INSERT INTO dbo.Users (FullName, Username, Email, PasswordHash, PasswordSalt, Role, Status)
    VALUES (N'System Administrator', N'admin', N'admin@uniskillhub.local',
            N'AStlHRGU+fgHzT4W4C2Xj1R10xYZlUgiVIuJPqwlwfM=', N'RanfDREyUNHqHtaUuKkIRA==',
            N'Admin', N'Active');
    PRINT 'Admin account created (admin / Admin@12345). Change the password after logging in.';
END
ELSE
    PRINT 'An Admin account already exists - nothing changed.';
GO
