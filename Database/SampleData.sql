 /* =====================================================================
   UniSkill Hub - SAMPLE DATA (for development and demos only)
   Run AFTER UniSkillHubDB.sql. Safe to re-run: a resource is only
   inserted if one with the same title does not exist yet.
   Delete these rows from the Admin > Resources page once real content
   exists. The three PDF files live in /Uploads/Notes.
   ===================================================================== */
USE UniSkillHubDB;
GO

DECLARE @Admin INT = (SELECT TOP 1 UserID FROM dbo.Users WHERE Role = N'Admin' ORDER BY UserID);

IF @Admin IS NULL
BEGIN
    RAISERROR (N'No Admin user found - create the admin account first.', 16, 1);
    RETURN;
END

DECLARE @Programming INT = (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = N'Programming');
DECLARE @Database    INT = (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = N'Database');
DECLARE @Web         INT = (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = N'Web Development');
DECLARE @Networking  INT = (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = N'Networking');
DECLARE @HCI         INT = (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = N'HCI');

DECLARE @Sample TABLE (
    CategoryID INT, Title NVARCHAR(150), Description NVARCHAR(1000),
    ResourceType NVARCHAR(20), FilePath NVARCHAR(260), ExternalURL NVARCHAR(500), Status NVARCHAR(20));

INSERT INTO @Sample VALUES
 (@Programming, N'Python Basics - Lecture Notes',
  N'Introductory notes on Python: variables, data types, control flow, loops and functions. A good starting point for first-year programming.',
  N'PDF', N'Uploads/Notes/python-basics.pdf', NULL, N'Published'),
 (@Database, N'Database Fundamentals - Lecture Notes',
  N'Core database concepts: tables, primary and foreign keys, normalisation and the four basic SQL statements (SELECT, INSERT, UPDATE, DELETE).',
  N'PDF', N'Uploads/Notes/database-fundamentals.pdf', NULL, N'Published'),
 (@Web, N'HTML5 and CSS3 Cheat Sheet',
  N'A one-page reference of semantic HTML5 elements and common CSS3 properties for building responsive pages.',
  N'PDF', N'Uploads/Notes/html-css-cheatsheet.pdf', NULL, N'Published'),
 (@Web, N'MDN Web Docs - Learn Web Development',
  N'Free, high-quality tutorials from Mozilla covering HTML, CSS and JavaScript from beginner to advanced level.',
  N'Link', NULL, N'https://developer.mozilla.org/en-US/docs/Learn', N'Published'),
 (@Database, N'What is Database Normalisation?',
  N'Normalisation organises data into separate related tables to reduce duplication and prevent update anomalies.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
  N'First Normal Form (1NF): every column holds a single value and every row is unique.' + CHAR(13) + CHAR(10) +
  N'Second Normal Form (2NF): 1NF plus every non-key column depends on the whole primary key.' + CHAR(13) + CHAR(10) +
  N'Third Normal Form (3NF): 2NF plus no non-key column depends on another non-key column.',
  N'Article', NULL, NULL, N'Published'),
 (@Networking, N'Introduction to Computer Networks',
  N'A network connects computers so they can share data. Key ideas: the OSI and TCP/IP layers, IP addressing, routers and switches, and common protocols such as HTTP, DNS and TCP.',
  N'Article', NULL, NULL, N'Published'),
 (@HCI, N'Usability Heuristics Overview',
  N'Nielsen''s ten usability heuristics, including visibility of system status, consistency and standards, error prevention and recognition rather than recall.',
  N'Article', NULL, NULL, N'Published'),
 (@Networking, N'Network Security Basics (draft)',
  N'Work in progress - this draft is not visible to students.',
  N'Article', NULL, NULL, N'Draft');

INSERT INTO dbo.Resources (CategoryID, Title, Description, ResourceType, FilePath, ExternalURL, CreatedBy, Status)
SELECT s.CategoryID, s.Title, s.Description, s.ResourceType, s.FilePath, s.ExternalURL, @Admin, s.Status
FROM @Sample s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Resources r WHERE r.Title = s.Title);

SELECT COUNT(*) AS TotalResources FROM dbo.Resources;
GO

/* =====================================================================
   SAMPLE ASSIGNMENTS (dates are relative to the moment you run this script)
   Re-runnable: an assignment is only inserted if its title does not exist yet.
   ===================================================================== */
USE UniSkillHubDB;
GO

DECLARE @Admin INT = (SELECT TOP 1 UserID FROM dbo.Users WHERE Role = N'Admin' ORDER BY UserID);

IF @Admin IS NULL
BEGIN
    RAISERROR (N'No Admin user found - create the admin account first.', 16, 1);
    RETURN;
END

DECLARE @Assignments TABLE (
    Title NVARCHAR(150), Description NVARCHAR(1000), Instructions NVARCHAR(MAX),
    DueDate DATETIME, MaxMarks INT, FilePath NVARCHAR(260), Status NVARCHAR(20));

INSERT INTO @Assignments VALUES
 (N'Web Application Assignment',
  N'Design and build a small database-driven web application using ASP.NET Web Forms.',
  N'1. Your application must have a registration page and a login page.' + CHAR(13) + CHAR(10) +
  N'2. Show at least one INSERT, SELECT, UPDATE and DELETE using ADO.NET.' + CHAR(13) + CHAR(10) +
  N'3. Use parameterized SQL and hash all passwords.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
  N'Submit a single ZIP file containing your project and a short report (PDF).',
  DATEADD(DAY, 10, GETDATE()), 100, N'Uploads/Assignments/web-application-brief.pdf', N'Published'),
 (N'Database Design Task',
  N'Draw an ERD for a small library system and write the SQL to create its tables.',
  N'Submit one PDF with your ERD and your CREATE TABLE statements.',
  DATEADD(DAY, 5, GETDATE()), 50, NULL, N'Published'),
 (N'HTML Layout Exercise',
  N'Recreate a given page layout using semantic HTML5 and CSS3.',
  NULL,
  DATEADD(DAY, -2, GETDATE()), 30, NULL, N'Published'),
 (N'Networking Lab (draft)',
  N'Work in progress - not visible to students.',
  NULL,
  DATEADD(DAY, 20, GETDATE()), 40, NULL, N'Draft');

INSERT INTO dbo.Assignments (Title, Description, Instructions, DueDate, MaxMarks, FilePath, CreatedBy, Status)
SELECT a.Title, a.Description, a.Instructions, a.DueDate, a.MaxMarks, a.FilePath, @Admin, a.Status
FROM @Assignments a
WHERE NOT EXISTS (SELECT 1 FROM dbo.Assignments x WHERE x.Title = a.Title);

SELECT COUNT(*) AS TotalAssignments FROM dbo.Assignments;
GO

/* =====================================================================
   SAMPLE QUIZZES
   Re-runnable: a quiz (and its questions) is only created if no quiz with
   that title exists yet. Correct answers are stored in QuizQuestions.CorrectOption.
   ===================================================================== */
USE UniSkillHubDB;
GO

-- Temporary helper (exists only for this script run): adds one question with options A-D
IF OBJECT_ID('tempdb..#AddQuestion') IS NOT NULL DROP PROCEDURE #AddQuestion;
GO
CREATE PROCEDURE #AddQuestion
    @QuizID INT, @Text NVARCHAR(500), @A NVARCHAR(300), @B NVARCHAR(300), @C NVARCHAR(300), @D NVARCHAR(300),
    @Correct CHAR(1), @Marks INT
AS
BEGIN
    INSERT INTO dbo.QuizQuestions (QuizID, QuestionText, Marks, CorrectOption) VALUES (@QuizID, @Text, @Marks, @Correct);
    DECLARE @QuestionID INT = SCOPE_IDENTITY();
    INSERT INTO dbo.QuizOptions (QuestionID, OptionText, OptionLetter) VALUES
        (@QuestionID, @A, 'A'), (@QuestionID, @B, 'B'), (@QuestionID, @C, 'C'), (@QuestionID, @D, 'D');
END
GO

DECLARE @Admin INT = (SELECT TOP 1 UserID FROM dbo.Users WHERE Role = N'Admin' ORDER BY UserID);
DECLARE @QuizID INT;

IF @Admin IS NULL
BEGIN
    RAISERROR (N'No Admin user found - create the admin account first.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE Title = N'Python Basics Quiz')
BEGIN
    INSERT INTO dbo.Quizzes (Title, Description, TimeLimit, TotalMarks, CreatedBy, Status)
    VALUES (N'Python Basics Quiz', N'Check your understanding of variables, data types, loops and built-in functions.', 10, 10, @Admin, N'Published');
    SET @QuizID = SCOPE_IDENTITY();
    EXEC #AddQuestion @QuizID, N'Which function prints text to the screen in Python?', N'print()', N'echo()', N'console.log()', N'printf()', 'A', 2;
    EXEC #AddQuestion @QuizID, N'Which of these is a valid Python variable name?', N'2nd_value', N'my-value', N'my_value', N'my value', 'C', 2;
    EXEC #AddQuestion @QuizID, N'What does len("UniSkill") return?', N'7', N'8', N'9', N'It causes an error', 'B', 2;
    EXEC #AddQuestion @QuizID, N'Which keyword starts a loop that repeats while a condition is true?', N'for', N'loop', N'repeat', N'while', 'D', 2;
    EXEC #AddQuestion @QuizID, N'Which data type stores only True or False?', N'str', N'int', N'bool', N'float', 'C', 2;
END

IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE Title = N'Web Development Basics Quiz')
BEGIN
    INSERT INTO dbo.Quizzes (Title, Description, TimeLimit, TotalMarks, CreatedBy, Status)
    VALUES (N'Web Development Basics Quiz', N'A short quiz on HTML, CSS and how the web works.', 5, 4, @Admin, N'Published');
    SET @QuizID = SCOPE_IDENTITY();
    EXEC #AddQuestion @QuizID, N'What does HTML stand for?', N'Hyper Text Markup Language', N'High Text Machine Language', N'Hyperlink Text Management Language', N'Home Tool Markup Language', 'A', 1;
    EXEC #AddQuestion @QuizID, N'Which HTML5 element is meant for the main navigation links of a page?', N'<menu>', N'<nav>', N'<links>', N'<navigate>', 'B', 1;
    EXEC #AddQuestion @QuizID, N'Which CSS property changes the colour of text?', N'font-color', N'text-style', N'color', N'foreground', 'C', 1;
    EXEC #AddQuestion @QuizID, N'Which protocol does a browser use to request web pages?', N'FTP', N'SMTP', N'HTTP', N'SSH', 'C', 1;
END

-- A draft quiz WITH a question (must stay hidden) and a published quiz WITHOUT questions (must stay hidden)
IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE Title = N'Networking Quiz (draft)')
BEGIN
    INSERT INTO dbo.Quizzes (Title, Description, TimeLimit, TotalMarks, CreatedBy, Status)
    VALUES (N'Networking Quiz (draft)', N'Work in progress.', 10, 1, @Admin, N'Draft');
    SET @QuizID = SCOPE_IDENTITY();
    EXEC #AddQuestion @QuizID, N'What does IP stand for?', N'Internet Protocol', N'Internal Port', N'Interface Program', N'Input Packet', 'A', 1;
END

IF NOT EXISTS (SELECT 1 FROM dbo.Quizzes WHERE Title = N'Empty Quiz (no questions yet)')
    INSERT INTO dbo.Quizzes (Title, Description, TimeLimit, TotalMarks, CreatedBy, Status)
    VALUES (N'Empty Quiz (no questions yet)', N'Has no questions, so students cannot see it.', 10, 0, @Admin, N'Published');

SELECT COUNT(*) AS TotalQuizzes FROM dbo.Quizzes;
GO

/* =====================================================================
   SAMPLE ANNOUNCEMENTS AND FORUM DISCUSSIONS
   Re-runnable: rows are only inserted if the title does not exist yet.
   The Announcements table shows every visibility rule:
     current -> visible | expired -> hidden | scheduled (future) -> hidden | draft -> hidden
   Forum sample threads are written by the Admin account (shown with a "Lecturer" badge).
   ===================================================================== */
USE UniSkillHubDB;
GO

DECLARE @Admin INT = (SELECT TOP 1 UserID FROM dbo.Users WHERE Role = N'Admin' ORDER BY UserID);

IF @Admin IS NULL
BEGIN
    RAISERROR (N'No Admin user found - create the admin account first.', 16, 1);
    RETURN;
END

-- ---------- announcements ----------
DECLARE @Ann TABLE (Title NVARCHAR(150), Content NVARCHAR(MAX), PublishDate DATETIME, ExpiryDate DATETIME, Status NVARCHAR(20));

INSERT INTO @Ann VALUES
 (N'Mid-Semester Examination Schedule',
  N'The mid-semester examination schedule has been published.' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
  N'Please check the timetable on the notice board and arrive at least 15 minutes before your paper starts. Bring your student ID.',
  DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, 14, GETDATE()), N'Published'),
 (N'Library Extended Opening Hours',
  N'The library will stay open until 10 PM on weekdays during the examination period.',
  DATEADD(DAY, -3, GETDATE()), NULL, N'Published'),
 (N'Welcome to UniSkill Hub',
  N'UniSkill Hub is your new home for lecture notes, assignments, quizzes and discussions. Explore the menu to get started, and do not hesitate to ask questions in the forum.',
  DATEADD(DAY, -10, GETDATE()), NULL, N'Published'),
 (N'Old notice (expired - hidden)',
  N'This notice has expired, so students cannot see it.',
  DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -5, GETDATE()), N'Published'),
 (N'Upcoming workshop (scheduled - hidden)',
  N'This announcement starts next week, so it is not visible yet.',
  DATEADD(DAY, 7, GETDATE()), NULL, N'Published'),
 (N'Draft announcement (hidden)',
  N'A draft that has not been published.',
  GETDATE(), NULL, N'Draft');

INSERT INTO dbo.Announcements (Title, Content, PostedBy, PublishDate, ExpiryDate, Status)
SELECT a.Title, a.Content, @Admin, a.PublishDate, a.ExpiryDate, a.Status
FROM @Ann a
WHERE NOT EXISTS (SELECT 1 FROM dbo.Announcements x WHERE x.Title = a.Title);

-- ---------- forum ----------
DECLARE @Posts TABLE (Title NVARCHAR(150), Content NVARCHAR(MAX), Category NVARCHAR(50), Status NVARCHAR(20));

INSERT INTO @Posts VALUES
 (N'How do I understand recursion?',
  N'I keep getting confused when a function calls itself. Can someone explain how the calls "unwind"?' + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) +
  N'A small example with factorial would help a lot.',
  N'Programming', N'Active'),
 (N'Database normalization question',
  N'What is the practical difference between 2NF and 3NF? I understand the definitions but not when to apply each one.',
  N'Database', N'Active'),
 (N'Tips for the Web Application assignment',
  N'Share your tips here: how are you structuring your folders, and how are you testing the login page?',
  N'Web Development', N'Active'),
 (N'Moderated discussion (hidden)',
  N'This thread was hidden by a moderator, so it does not appear in the forum.',
  N'General', N'Hidden');

INSERT INTO dbo.ForumPosts (UserID, Title, Content, Category, Status)
SELECT @Admin, p.Title, p.Content, p.Category, p.Status
FROM @Posts p
WHERE NOT EXISTS (SELECT 1 FROM dbo.ForumPosts x WHERE x.Title = p.Title);

DECLARE @Recursion INT = (SELECT PostID FROM dbo.ForumPosts WHERE Title = N'How do I understand recursion?');
DECLARE @Normal    INT = (SELECT PostID FROM dbo.ForumPosts WHERE Title = N'Database normalization question');

IF NOT EXISTS (SELECT 1 FROM dbo.ForumReplies WHERE PostID = @Recursion)
    INSERT INTO dbo.ForumReplies (PostID, UserID, ReplyText, DatePosted) VALUES
     (@Recursion, @Admin, N'Think of it as a stack of unfinished jobs. factorial(3) waits for factorial(2), which waits for factorial(1). When the last call returns, each waiting call finishes in reverse order.', DATEADD(HOUR, -5, GETDATE())),
     (@Recursion, @Admin, N'Tip: always write the base case first, otherwise the function never stops calling itself.', DATEADD(HOUR, -2, GETDATE()));

IF NOT EXISTS (SELECT 1 FROM dbo.ForumReplies WHERE PostID = @Normal)
    INSERT INTO dbo.ForumReplies (PostID, UserID, ReplyText, DatePosted) VALUES
     (@Normal, @Admin, N'2NF removes partial dependencies on part of a composite key. 3NF removes dependencies between non-key columns. Apply 2NF first, then 3NF.', DATEADD(HOUR, -1, GETDATE()));

SELECT (SELECT COUNT(*) FROM dbo.Announcements) AS Announcements, (SELECT COUNT(*) FROM dbo.ForumPosts) AS ForumPosts, (SELECT COUNT(*) FROM dbo.ForumReplies) AS ForumReplies;
GO

/* =====================================================================
   SAMPLE MULTIMEDIA (a short video and an audio clip for the HTML5 players)
   Files: /Uploads/Videos/getting-started-demo.webm and
          /Uploads/Audio/listening-demo.wav
   Re-runnable: a resource is only inserted if its title does not exist yet.
   ===================================================================== */
USE UniSkillHubDB;
GO

DECLARE @Admin INT = (SELECT TOP 1 UserID FROM dbo.Users WHERE Role = N'Admin' ORDER BY UserID);
DECLARE @Web   INT = (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = N'Web Development');
DECLARE @HCI   INT = (SELECT CategoryID FROM dbo.Categories WHERE CategoryName = N'HCI');

IF @Admin IS NULL
BEGIN
    RAISERROR (N'No Admin user found - create the admin account first.', 16, 1);
    RETURN;
END

INSERT INTO dbo.Resources (CategoryID, Title, Description, ResourceType, VideoPath, CreatedBy, Status)
SELECT @Web, N'Getting Started with UniSkill Hub (video)',
       N'A short animated walkthrough of how to use the portal: browse resources, learn, practise with assignments and quizzes, and join the forum.',
       N'Video', N'Uploads/Videos/getting-started-demo.webm', @Admin, N'Published'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Resources WHERE Title = N'Getting Started with UniSkill Hub (video)');

INSERT INTO dbo.Resources (CategoryID, Title, Description, ResourceType, AudioPath, CreatedBy, Status)
SELECT @HCI, N'Listening Practice: Audio Player Demo',
       N'A short audio clip that demonstrates the built-in audio player: play, pause, seek and adjust the volume.',
       N'Audio', N'Uploads/Audio/listening-demo.wav', @Admin, N'Published'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Resources WHERE Title = N'Listening Practice: Audio Player Demo');
GO
