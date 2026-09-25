/* =====================================================================
   UniSkill Hub - database creation script
   Target : Microsoft SQL Server (Express) - run in SSMS or with sqlcmd
   Safe to re-run: each object is only created if it does not exist yet,
   so existing data is never dropped.
   ===================================================================== */

IF DB_ID(N'UniSkillHubDB') IS NULL
    CREATE DATABASE UniSkillHubDB;
GO

USE UniSkillHubDB;
GO

/* ---------------------------------------------------------------------
   1. Users
   Role   : Student | Admin
   Status : Active  | Inactive   (users are deactivated, not deleted,
                                  so their submissions/posts are kept)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    UserID         INT IDENTITY(1,1) NOT NULL,
    FullName       NVARCHAR(100)     NOT NULL,
    Username       NVARCHAR(50)      NOT NULL,
    Email          NVARCHAR(100)     NOT NULL,
    PasswordHash   NVARCHAR(256)     NOT NULL,
    PasswordSalt   NVARCHAR(128)     NOT NULL,
    Role           NVARCHAR(20)      NOT NULL CONSTRAINT DF_Users_Role   DEFAULT (N'Student'),
    Status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Users_Status DEFAULT (N'Active'),
    DateRegistered DATETIME          NOT NULL CONSTRAINT DF_Users_DateRegistered DEFAULT (GETDATE()),
    CONSTRAINT PK_Users          PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email),
    CONSTRAINT CK_Users_Role     CHECK (Role   IN (N'Student', N'Admin')),
    CONSTRAINT CK_Users_Status   CHECK (Status IN (N'Active', N'Inactive'))
);
GO

/* ---------------------------------------------------------------------
   2. Categories (used to organise resources - no full course system)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
CREATE TABLE dbo.Categories (
    CategoryID   INT IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(100)     NOT NULL,
    Description  NVARCHAR(300)     NULL,
    CONSTRAINT PK_Categories               PRIMARY KEY (CategoryID),
    CONSTRAINT UQ_Categories_CategoryName  UNIQUE (CategoryName)
);
GO

/* ---------------------------------------------------------------------
   3. Resources
   ResourceType : PDF | Video | Audio | Article | Link
   Status       : Published | Draft
   File columns hold relative paths (e.g. Uploads/Notes/web.pdf),
   never the file itself.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Resources', N'U') IS NULL
CREATE TABLE dbo.Resources (
    ResourceID   INT IDENTITY(1,1) NOT NULL,
    CategoryID   INT               NOT NULL,
    Title        NVARCHAR(150)     NOT NULL,
    Description  NVARCHAR(1000)    NOT NULL,
    ResourceType NVARCHAR(20)      NOT NULL,
    FilePath     NVARCHAR(260)     NULL,
    VideoPath    NVARCHAR(260)     NULL,
    AudioPath    NVARCHAR(260)     NULL,
    ExternalURL  NVARCHAR(500)     NULL,
    CreatedBy    INT               NOT NULL,
    DateCreated  DATETIME          NOT NULL CONSTRAINT DF_Resources_DateCreated DEFAULT (GETDATE()),
    Status       NVARCHAR(20)      NOT NULL CONSTRAINT DF_Resources_Status      DEFAULT (N'Published'),
    CONSTRAINT PK_Resources            PRIMARY KEY (ResourceID),
    CONSTRAINT FK_Resources_Categories FOREIGN KEY (CategoryID) REFERENCES dbo.Categories (CategoryID),
    CONSTRAINT FK_Resources_Users      FOREIGN KEY (CreatedBy)  REFERENCES dbo.Users (UserID),
    CONSTRAINT CK_Resources_Type       CHECK (ResourceType IN (N'PDF', N'Video', N'Audio', N'Article', N'Link')),
    CONSTRAINT CK_Resources_Status     CHECK (Status       IN (N'Published', N'Draft'))
);
GO

/* ---------------------------------------------------------------------
   4. Assignments
   Status : Published | Draft
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Assignments', N'U') IS NULL
CREATE TABLE dbo.Assignments (
    AssignmentID INT IDENTITY(1,1) NOT NULL,
    Title        NVARCHAR(150)     NOT NULL,
    Description  NVARCHAR(1000)    NOT NULL,
    Instructions NVARCHAR(MAX)     NULL,
    DueDate      DATETIME          NOT NULL,
    MaxMarks     INT               NOT NULL CONSTRAINT DF_Assignments_MaxMarks    DEFAULT (100),
    FilePath     NVARCHAR(260)     NULL,
    CreatedBy    INT               NOT NULL,
    DateCreated  DATETIME          NOT NULL CONSTRAINT DF_Assignments_DateCreated DEFAULT (GETDATE()),
    Status       NVARCHAR(20)      NOT NULL CONSTRAINT DF_Assignments_Status      DEFAULT (N'Published'),
    CONSTRAINT PK_Assignments        PRIMARY KEY (AssignmentID),
    CONSTRAINT FK_Assignments_Users  FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserID),
    CONSTRAINT CK_Assignments_Marks  CHECK (MaxMarks > 0),
    CONSTRAINT CK_Assignments_Status CHECK (Status IN (N'Published', N'Draft'))
);
GO

/* ---------------------------------------------------------------------
   5. Submissions
   One submission per student per assignment (a re-submit updates the row).
   Status : Submitted | Graded
   Deleting an assignment removes its submissions (the page must also
   delete the uploaded files).
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Submissions', N'U') IS NULL
CREATE TABLE dbo.Submissions (
    SubmissionID  INT IDENTITY(1,1) NOT NULL,
    AssignmentID  INT               NOT NULL,
    StudentID     INT               NOT NULL,
    FilePath      NVARCHAR(260)     NOT NULL,
    Comment       NVARCHAR(500)     NULL,
    SubmittedDate DATETIME          NOT NULL CONSTRAINT DF_Submissions_SubmittedDate DEFAULT (GETDATE()),
    Status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_Submissions_Status        DEFAULT (N'Submitted'),
    Marks         DECIMAL(5,2)      NULL,
    Feedback      NVARCHAR(1000)    NULL,
    CONSTRAINT PK_Submissions             PRIMARY KEY (SubmissionID),
    CONSTRAINT FK_Submissions_Assignments FOREIGN KEY (AssignmentID) REFERENCES dbo.Assignments (AssignmentID) ON DELETE CASCADE,
    CONSTRAINT FK_Submissions_Users       FOREIGN KEY (StudentID)    REFERENCES dbo.Users (UserID),
    CONSTRAINT UQ_Submissions_Student     UNIQUE (AssignmentID, StudentID),
    CONSTRAINT CK_Submissions_Status      CHECK (Status IN (N'Submitted', N'Graded')),
    CONSTRAINT CK_Submissions_Marks       CHECK (Marks IS NULL OR Marks >= 0)
);
GO

/* ---------------------------------------------------------------------
   6. Quizzes
   TimeLimit : minutes
   Status    : Published | Draft
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Quizzes', N'U') IS NULL
CREATE TABLE dbo.Quizzes (
    QuizID      INT IDENTITY(1,1) NOT NULL,
    Title       NVARCHAR(150)     NOT NULL,
    Description NVARCHAR(500)     NULL,
    TimeLimit   INT               NOT NULL CONSTRAINT DF_Quizzes_TimeLimit   DEFAULT (10),
    TotalMarks  INT               NOT NULL CONSTRAINT DF_Quizzes_TotalMarks  DEFAULT (0),
    CreatedBy   INT               NOT NULL,
    DateCreated DATETIME          NOT NULL CONSTRAINT DF_Quizzes_DateCreated DEFAULT (GETDATE()),
    Status      NVARCHAR(20)      NOT NULL CONSTRAINT DF_Quizzes_Status      DEFAULT (N'Draft'),
    CONSTRAINT PK_Quizzes           PRIMARY KEY (QuizID),
    CONSTRAINT FK_Quizzes_Users     FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserID),
    CONSTRAINT CK_Quizzes_TimeLimit CHECK (TimeLimit > 0),
    CONSTRAINT CK_Quizzes_Status    CHECK (Status IN (N'Published', N'Draft'))
);
GO

/* ---------------------------------------------------------------------
   7. QuizQuestions
   CorrectOption : A | B | C | D
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.QuizQuestions', N'U') IS NULL
CREATE TABLE dbo.QuizQuestions (
    QuestionID    INT IDENTITY(1,1) NOT NULL,
    QuizID        INT               NOT NULL,
    QuestionText  NVARCHAR(500)     NOT NULL,
    Marks         INT               NOT NULL CONSTRAINT DF_QuizQuestions_Marks DEFAULT (1),
    CorrectOption CHAR(1)           NOT NULL,
    CONSTRAINT PK_QuizQuestions         PRIMARY KEY (QuestionID),
    CONSTRAINT FK_QuizQuestions_Quizzes FOREIGN KEY (QuizID) REFERENCES dbo.Quizzes (QuizID) ON DELETE CASCADE,
    CONSTRAINT CK_QuizQuestions_Marks   CHECK (Marks > 0),
    CONSTRAINT CK_QuizQuestions_Correct CHECK (CorrectOption IN ('A', 'B', 'C', 'D'))
);
GO

/* ---------------------------------------------------------------------
   8. QuizOptions (options A-D of each question)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.QuizOptions', N'U') IS NULL
CREATE TABLE dbo.QuizOptions (
    OptionID     INT IDENTITY(1,1) NOT NULL,
    QuestionID   INT               NOT NULL,
    OptionText   NVARCHAR(300)     NOT NULL,
    OptionLetter CHAR(1)           NOT NULL,
    CONSTRAINT PK_QuizOptions           PRIMARY KEY (OptionID),
    CONSTRAINT FK_QuizOptions_Questions FOREIGN KEY (QuestionID) REFERENCES dbo.QuizQuestions (QuestionID) ON DELETE CASCADE,
    CONSTRAINT UQ_QuizOptions_Letter    UNIQUE (QuestionID, OptionLetter),
    CONSTRAINT CK_QuizOptions_Letter    CHECK (OptionLetter IN ('A', 'B', 'C', 'D'))
);
GO

/* ---------------------------------------------------------------------
   9. QuizAttempts
   A student may attempt a quiz more than once; every attempt is kept.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.QuizAttempts', N'U') IS NULL
CREATE TABLE dbo.QuizAttempts (
    AttemptID   INT IDENTITY(1,1) NOT NULL,
    QuizID      INT               NOT NULL,
    StudentID   INT               NOT NULL,
    Score       INT               NOT NULL,
    TotalMarks  INT               NOT NULL,
    Percentage  DECIMAL(5,2)      NOT NULL,
    AttemptDate DATETIME          NOT NULL CONSTRAINT DF_QuizAttempts_AttemptDate DEFAULT (GETDATE()),
    CONSTRAINT PK_QuizAttempts         PRIMARY KEY (AttemptID),
    CONSTRAINT FK_QuizAttempts_Quizzes FOREIGN KEY (QuizID)    REFERENCES dbo.Quizzes (QuizID) ON DELETE CASCADE,
    CONSTRAINT FK_QuizAttempts_Users   FOREIGN KEY (StudentID) REFERENCES dbo.Users (UserID),
    CONSTRAINT CK_QuizAttempts_Score   CHECK (Score >= 0 AND TotalMarks >= 0 AND Score <= TotalMarks)
);
GO

/* ---------------------------------------------------------------------
   9b. QuizAnswers  (added in the quiz milestone - supports "Review Answers")
   One row per question of an attempt: what the student chose (NULL = left
   blank), the correct option at that moment and whether it was right.
   Deleting an attempt (or its quiz) deletes its answers.
   QuestionID has no cascade: a quiz delete already reaches this table through
   QuizAttempts, and SQL Server does not allow two cascade paths. The Admin
   "delete question" code must delete that question's answers first.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.QuizAnswers', N'U') IS NULL
CREATE TABLE dbo.QuizAnswers (
    AnswerID      INT IDENTITY(1,1) NOT NULL,
    AttemptID     INT               NOT NULL,
    QuestionID    INT               NOT NULL,
    ChosenOption  CHAR(1)           NULL,
    CorrectOption CHAR(1)           NOT NULL,
    IsCorrect     BIT               NOT NULL,
    MarksAwarded  INT               NOT NULL CONSTRAINT DF_QuizAnswers_MarksAwarded DEFAULT (0),
    CONSTRAINT PK_QuizAnswers           PRIMARY KEY (AnswerID),
    CONSTRAINT FK_QuizAnswers_Attempts  FOREIGN KEY (AttemptID)  REFERENCES dbo.QuizAttempts (AttemptID) ON DELETE CASCADE,
    CONSTRAINT FK_QuizAnswers_Questions FOREIGN KEY (QuestionID) REFERENCES dbo.QuizQuestions (QuestionID),
    CONSTRAINT UQ_QuizAnswers_Question  UNIQUE (AttemptID, QuestionID),
    CONSTRAINT CK_QuizAnswers_Chosen    CHECK (ChosenOption  IS NULL OR ChosenOption IN ('A', 'B', 'C', 'D')),
    CONSTRAINT CK_QuizAnswers_Correct   CHECK (CorrectOption IN ('A', 'B', 'C', 'D'))
);
GO

/* ---------------------------------------------------------------------
   10. Announcements
   ExpiryDate NULL = never expires
   Status     : Published | Draft
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Announcements', N'U') IS NULL
CREATE TABLE dbo.Announcements (
    AnnouncementID INT IDENTITY(1,1) NOT NULL,
    Title          NVARCHAR(150)     NOT NULL,
    Content        NVARCHAR(MAX)     NOT NULL,
    PostedBy       INT               NOT NULL,
    PublishDate    DATETIME          NOT NULL CONSTRAINT DF_Announcements_PublishDate DEFAULT (GETDATE()),
    ExpiryDate     DATETIME          NULL,
    Status         NVARCHAR(20)      NOT NULL CONSTRAINT DF_Announcements_Status      DEFAULT (N'Published'),
    CONSTRAINT PK_Announcements         PRIMARY KEY (AnnouncementID),
    CONSTRAINT FK_Announcements_Users   FOREIGN KEY (PostedBy) REFERENCES dbo.Users (UserID),
    CONSTRAINT CK_Announcements_Status  CHECK (Status IN (N'Published', N'Draft')),
    CONSTRAINT CK_Announcements_Expiry  CHECK (ExpiryDate IS NULL OR ExpiryDate >= PublishDate)
);
GO

/* ---------------------------------------------------------------------
   11. ForumPosts
   Category : free text label such as General, Programming, Database
   Status   : Active | Hidden  (Hidden = moderated by an admin)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.ForumPosts', N'U') IS NULL
CREATE TABLE dbo.ForumPosts (
    PostID     INT IDENTITY(1,1) NOT NULL,
    UserID     INT               NOT NULL,
    Title      NVARCHAR(150)     NOT NULL,
    Content    NVARCHAR(MAX)     NOT NULL,
    Category   NVARCHAR(50)      NOT NULL CONSTRAINT DF_ForumPosts_Category   DEFAULT (N'General'),
    DatePosted DATETIME          NOT NULL CONSTRAINT DF_ForumPosts_DatePosted DEFAULT (GETDATE()),
    Status     NVARCHAR(20)      NOT NULL CONSTRAINT DF_ForumPosts_Status     DEFAULT (N'Active'),
    CONSTRAINT PK_ForumPosts        PRIMARY KEY (PostID),
    CONSTRAINT FK_ForumPosts_Users  FOREIGN KEY (UserID) REFERENCES dbo.Users (UserID),
    CONSTRAINT CK_ForumPosts_Status CHECK (Status IN (N'Active', N'Hidden'))
);
GO

/* ---------------------------------------------------------------------
   12. ForumReplies
   Deleting a post removes its replies.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.ForumReplies', N'U') IS NULL
CREATE TABLE dbo.ForumReplies (
    ReplyID    INT IDENTITY(1,1) NOT NULL,
    PostID     INT               NOT NULL,
    UserID     INT               NOT NULL,
    ReplyText  NVARCHAR(2000)    NOT NULL,
    DatePosted DATETIME          NOT NULL CONSTRAINT DF_ForumReplies_DatePosted DEFAULT (GETDATE()),
    Status     NVARCHAR(20)      NOT NULL CONSTRAINT DF_ForumReplies_Status     DEFAULT (N'Active'),
    CONSTRAINT PK_ForumReplies          PRIMARY KEY (ReplyID),
    CONSTRAINT FK_ForumReplies_Posts    FOREIGN KEY (PostID) REFERENCES dbo.ForumPosts (PostID) ON DELETE CASCADE,
    CONSTRAINT FK_ForumReplies_Users    FOREIGN KEY (UserID) REFERENCES dbo.Users (UserID),
    CONSTRAINT CK_ForumReplies_Status   CHECK (Status IN (N'Active', N'Hidden'))
);
GO

/* ---------------------------------------------------------------------
   Seed data: resource categories
   (The first Admin account is created after PasswordHelper exists,
    because its password must be salted + hashed - never stored plain.)
   --------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
INSERT INTO dbo.Categories (CategoryName, Description) VALUES
    (N'Programming',     N'Programming fundamentals, languages and problem solving'),
    (N'Database',        N'Database design, SQL and data management'),
    (N'Web Development', N'HTML, CSS, JavaScript and server-side web technologies'),
    (N'Networking',      N'Computer networks, protocols and security basics'),
    (N'HCI',             N'Human-computer interaction and interface design');
GO
