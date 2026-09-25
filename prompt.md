You are helping me develop my university Web Applications group assignment.

PROJECT NAME
UniSkill Hub — A Web-Based Learning Portal for University Students

IMPORTANT:
Before modifying anything, inspect the existing repository/project structure and understand what has already been created. Do not overwrite working code unnecessarily.

==================================================
1. PROJECT PURPOSE
==================================================

UniSkill Hub is a university learning portal that centralizes:

- Learning resources / lecture notes
- Assignments
- Assignment submissions
- Quizzes
- Quiz results
- Announcements
- Academic discussion forum

There are three access states:

1. Guest / Public visitor
2. Registered Student
3. Administrator / Lecturer

The application must demonstrate database connectivity, CRUD operations, authentication, authorization, form validation, multimedia, responsive UI, and proper navigation.

==================================================
2. REQUIRED TECHNOLOGY STACK
==================================================

This project MUST use:

- ASP.NET Web Forms (.NET Framework)
- C#
- HTML5
- CSS3
- JavaScript
- Bootstrap 5
- Microsoft SQL Server
- ADO.NET
- Forms Authentication
- Visual Studio 2022
- IIS Express for local development

DO NOT migrate or convert this project to:

- ASP.NET Core
- ASP.NET MVC
- Razor Pages
- Blazor
- React
- Next.js
- Angular
- Node.js
- Entity Framework as the main database layer

Use normal ASP.NET Web Forms:
.aspx
.aspx.cs
.master
Web.config

Use ADO.NET with SqlConnection, SqlCommand, SqlDataReader, DataTable, etc.

Always use parameterized SQL queries.

==================================================
3. DEVELOPMENT PRINCIPLES
==================================================

This is a university assignment, so code should be:

- understandable by undergraduate students
- clean and logically structured
- reasonably simple
- properly named
- secure enough for an academic web application
- easy to explain during a presentation

Do not introduce unnecessary enterprise architecture, dependency injection frameworks, ORMs, SPA frameworks, or complex abstractions.

Before implementing a major change:

1. Inspect the existing code.
2. Explain briefly what you intend to change.
3. Preserve existing working functionality.
4. Implement the smallest sensible solution.
5. Check for compilation/runtime issues.
6. Tell me which files were created or modified.
7. Tell me how I can manually test the feature.

Do not generate dozens of placeholder pages at once.

Build the application incrementally.

==================================================
4. DESIGN DIRECTION
==================================================

I want UniSkill Hub to have a modern premium educational SaaS-style interface.

Aceternity UI is my visual inspiration:
https://ui.aceternity.com/components

IMPORTANT:
Aceternity UI uses technologies such as React/Tailwind that are NOT part of this project.

Therefore:

- Do NOT install React.
- Do NOT install Next.js.
- Do NOT convert the application to Tailwind.
- Do NOT copy React components directly.

Instead, recreate similar visual ideas using:

- ASPX / HTML5
- CSS
- Bootstrap 5
- lightweight vanilla JavaScript

Visual direction:

- modern
- clean
- academic
- professional
- spacious
- responsive
- subtle animations
- subtle gradients
- modern cards
- polished hover states
- good typography
- soft shadows
- consistent border radius
- accessible contrast

Avoid excessive animations or effects that distract from usability.

The public landing page can be visually impressive.

Student and Admin areas should prioritize usability and dashboard-style layouts.

Use one consistent design system throughout the website.

==================================================
5. SHARED LAYOUT
==================================================

Use Site.Master for shared layout.

It should support dynamic navigation based on authentication/role.

Guest navigation:

Home
Resources
Announcements
Forum
About
Contact
Login
Register

Student navigation:

Dashboard
Resources
Assignments
Quizzes
Announcements
Forum
Profile
Logout

Admin navigation:

Dashboard
Users
Resources
Assignments
Submissions
Quizzes
Announcements
Forum
Logout

Use responsive navigation.

==================================================
6. STUDENT FUNCTIONALITY
==================================================

Registered students should eventually be able to:

- Register
- Login
- Logout
- View dashboard
- Manage profile
- Browse learning resources
- Search resources
- Filter resources by category/type
- View resource details
- Download learning materials
- View assignments
- View assignment details
- Submit assignment files
- Track their own submissions
- View marks and feedback
- View available quizzes
- Take quizzes
- View quiz results
- Read announcements
- Browse forum discussions
- Create forum posts
- Reply to posts
- Manage their own posts/replies where appropriate

Students must NEVER be able to view another student's private submissions/results.

==================================================
7. ADMIN FUNCTIONALITY
==================================================

Administrator/Lecturer should eventually be able to:

- Login
- Logout
- View admin dashboard
- Manage users
- Manage learning resources
- Upload learning materials
- Manage assignments
- View student submissions
- Grade submissions
- Add feedback
- Manage quizzes
- Manage quiz questions
- Manage announcements
- Manage/moderate forum content
- View basic system statistics/activity

Admin CRUD pages can use ASP.NET GridView where appropriate.

==================================================
8. DATABASE
==================================================

Database name:

UniSkillHubDB

Planned tables:

Users
Categories
Resources
Assignments
Submissions
Quizzes
QuizQuestions
QuizOptions
QuizAttempts
Announcements
ForumPosts
ForumReplies

Expected relationships include:

Users -> Resources
Categories -> Resources

Users -> Assignments
Assignments -> Submissions
Users -> Submissions

Users -> Quizzes
Quizzes -> QuizQuestions
QuizQuestions -> QuizOptions

Users -> QuizAttempts
Quizzes -> QuizAttempts

Users -> Announcements

Users -> ForumPosts
ForumPosts -> ForumReplies
Users -> ForumReplies

Use:

- primary keys
- foreign keys
- appropriate data types
- NOT NULL where appropriate
- unique constraints where appropriate
- sensible default values
- referential integrity

Do not casually change the database schema once modules depend on it.

If a schema change is necessary, explain why first.

==================================================
9. USERS TABLE / SECURITY
==================================================

Users should support approximately:

UserID
FullName
Username
Email
PasswordHash
PasswordSalt
Role
Status
DateRegistered

Roles:

Student
Admin

NEVER store plaintext passwords.

Implement password hashing + salt.

Create a reusable PasswordHelper class.

Authentication should use Forms Authentication.

Authorization must be enforced SERVER-SIDE.

Do not merely hide admin navigation links.

A Student manually navigating to:

/Admin/ManageUsers.aspx

must be denied.

An unauthenticated visitor manually navigating to Student pages must be redirected to Login.

==================================================
10. DATABASE ACCESS
==================================================

Create/reuse:

App_Code/DBHelper.cs

Use it to centralize common ADO.NET operations.

Potential helper methods:

ExecuteNonQuery()
ExecuteScalar()
ExecuteReader() or suitable alternative
GetDataTable()

Use connection strings from Web.config.

NEVER hard-code database credentials throughout pages.

NEVER build SQL like:

"SELECT * FROM Users WHERE Email='" + email + "'"

Use parameterized queries:

SELECT * FROM Users WHERE Email=@Email

and add SqlParameter values.

==================================================
11. VALIDATION
==================================================

Important forms need both client-side and server-side validation.

Use Web Forms validation controls where appropriate:

RequiredFieldValidator
RegularExpressionValidator
CompareValidator
CustomValidator
ValidationSummary

Registration should validate:

Full Name -> required
Username -> required + unique
Email -> required + valid + unique
Password -> required + minimum length
Confirm Password -> matches password

File uploads should validate:

- extension
- size
- authentication
- related database record
- appropriate destination

Never trust client-side validation alone.

==================================================
12. FILE STRUCTURE
==================================================

Aim toward a structure similar to:

UniSkillHub/
|
|-- Account/
|   |-- Login.aspx
|   |-- Register.aspx
|   |-- Logout.aspx
|
|-- Student/
|   |-- Dashboard.aspx
|   |-- Profile.aspx
|   |-- Resources.aspx
|   |-- ResourceDetails.aspx
|   |-- Assignments.aspx
|   |-- AssignmentDetails.aspx
|   |-- SubmitAssignment.aspx
|   |-- MySubmissions.aspx
|   |-- Quizzes.aspx
|   |-- TakeQuiz.aspx
|   |-- QuizResult.aspx
|   |-- Announcements.aspx
|   |-- Forum.aspx
|   |-- CreatePost.aspx
|   |-- PostDetails.aspx
|   |-- Web.config
|
|-- Admin/
|   |-- Dashboard.aspx
|   |-- ManageUsers.aspx
|   |-- ManageResources.aspx
|   |-- ManageAssignments.aspx
|   |-- ManageSubmissions.aspx
|   |-- ManageQuizzes.aspx
|   |-- ManageQuestions.aspx
|   |-- ManageAnnouncements.aspx
|   |-- ManageForum.aspx
|   |-- Web.config
|
|-- Uploads/
|   |-- Notes/
|   |-- Assignments/
|   |-- Videos/
|   |-- Audio/
|
|-- Css/
|   |-- site.css
|
|-- Scripts/
|   |-- site.js
|   |-- validation.js
|
|-- Images/
|
|-- App_Code/
|   |-- DBHelper.cs
|   |-- PasswordHelper.cs
|   |-- Utility.cs
|
|-- Site.Master
|-- Site.Master.cs
|-- Default.aspx
|-- About.aspx
|-- Contact.aspx
|-- Announcements.aspx
|-- Web.config
|-- Global.asax

Do not create all of these just because they are listed.
Create them when their module is implemented.

==================================================
13. NAMING CONVENTIONS
==================================================

Use consistent ASP.NET Web Forms control naming.

Examples:

TextBox:
txtFullName
txtEmail
txtTitle

Button:
btnLogin
btnRegister
btnSave
btnUpdate
btnDelete

GridView:
gvUsers
gvResources
gvAssignments

DropDownList:
ddlCategory
ddlResourceType
ddlRole

FileUpload:
fuResource
fuAssignment

Label:
lblMessage
lblScore

Repeater:
rptResources
rptAnnouncements
rptForumPosts

Use PascalCase for C# classes and meaningful method names.

==================================================
14. CRUD REQUIREMENT
==================================================

The final system MUST visibly demonstrate:

CREATE / INSERT
READ / SELECT
UPDATE
DELETE

Examples:

INSERT:
student registration
resource creation
assignment creation
submission
forum post

SELECT:
resources
assignments
announcements
submissions

UPDATE:
student profile
resource
assignment
submission grade
announcement

DELETE:
resource
assignment
announcement
forum post

CRUD should be real SQL Server database operations through ADO.NET.

==================================================
15. MULTIMEDIA
==================================================

The assignment requires multimedia.

Eventually demonstrate:

- Images
- HTML5 video
- HTML5 audio
- Interactive quiz

Store uploaded file paths in SQL Server rather than unnecessarily storing large files as database binary data.

==================================================
16. OPTIONAL AI FEATURE
==================================================

AI note summarization is ONLY an optional future enhancement.

DO NOT implement Gemini/API integration until the complete core system works.

The project must function fully without AI.

==================================================
17. OUT OF SCOPE
==================================================

Do NOT implement unless I explicitly request it later:

- real-time chat
- live video classes
- dedicated mobile app
- AI grading
- advanced AI marking
- push notifications
- full multi-course LMS architecture
- complex notification infrastructure

Use Resource Categories rather than building a full subject/course management architecture.

==================================================
18. DEVELOPMENT ORDER
==================================================

Follow this order:

Phase 1 — Foundation
1. Inspect existing project
2. Confirm Web Forms project works
3. Set up project folders only as needed
4. Create SQL Server database
5. Create tables/relationships
6. Configure connection string
7. Implement DBHelper
8. Implement PasswordHelper
9. Build/refine Site.Master
10. Establish global CSS/design system

Phase 2 — Authentication
11. Registration
12. Login
13. Logout
14. Forms Authentication
15. Student/Admin authorization

Phase 3 — Student core
16. Student Dashboard
17. Profile
18. Resources

Phase 4 — Learning functionality
19. Assignments
20. Submission
21. Submission tracking
22. Quizzes
23. Quiz scoring/results
24. Announcements
25. Forum

Phase 5 — Admin
26. Admin Dashboard
27. User management
28. Resource CRUD
29. Assignment CRUD
30. Submission grading
31. Quiz/question CRUD
32. Announcement CRUD
33. Forum moderation

Phase 6 — Completion
34. Multimedia
35. Validation review
36. Security review
37. Responsive design testing
38. Integration testing
39. Bug fixing

DO NOT jump ahead unless I ask.

==================================================
19. CURRENT WORKING STYLE
==================================================

Work with me one milestone at a time.

When I request a feature:

1. Inspect relevant existing files.
2. Identify dependencies.
3. Tell me the implementation plan briefly.
4. Make the required changes.
5. Avoid unrelated refactoring.
6. Check for obvious errors.
7. Summarize changed files.
8. Give me exact manual testing instructions.
9. Stop and wait for my next instruction.

If something depends on information you do not know, such as my SQL Server instance name, existing connection string, installed .NET Framework version, or current project structure, inspect the project first. If it cannot be determined from the project, ask me instead of inventing it.

==================================================
20. FIRST TASK
==================================================

For now, DO NOT implement the whole application.

Inspect the current repository and report:

1. What type of Visual Studio/.NET project this is.
2. Whether it is correctly configured as ASP.NET Web Forms (.NET Framework).
3. What .NET Framework version it targets.
4. Current files/folders.
5. Whether Site.Master exists.
6. Whether Bootstrap is currently configured.
7. Whether Web.config exists and what relevant configuration is already present.
8. Whether any database connection already exists.
9. Whether App_Code exists.
10. Any obvious setup problems.

Then recommend ONLY the next development step.

Do not modify files until you have finished this inspection and reported your findings.