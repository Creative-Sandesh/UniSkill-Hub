# UniSkill Hub — Web Application Development Implementation Requirements

**Module:** CT050-3-2-WAPP – Web Applications  
**Project:** UniSkill Hub: A Web-Based Learning Portal for University Students  
**Development Approach:** ASP.NET Web Forms  
**Language:** C#  
**Database:** Microsoft SQL Server  
**Database Connectivity:** ADO.NET  
**IDE:** Visual Studio 2022  

---

## 1. Project Overview

UniSkill Hub is a web-based university learning portal designed to provide students with a centralized place to access learning materials, submit assignments, take quizzes, view announcements, and participate in discussion forums.

The system has two main user roles:

- **Student**
- **Administrator / Lecturer**

The proposal identifies lecture notes, assignments, quizzes, announcements, and discussion forums as the main learning functions. It also describes AI-based note summarization as an optional future/enhancement feature.

The mandatory implementation should therefore focus on making the core Web Forms system fully functional before considering the optional AI feature.

---

# 2. Assignment Requirements

The final website must:

- Consist of interlinked webpages.
- Demonstrate appropriate HTML5 elements.
- Demonstrate CSS usage.
- Provide good quality content.
- Connect to a database.
- Perform:
  - Insert
  - Display
  - Update
  - Delete
- Contain a registration page.
- Contain registered-member modules requiring login.
- Contain an administrator module requiring login.
- Perform form validation.
- Provide appropriate navigation.
- Use proper file organization and naming conventions.
- Demonstrate good usage of multimedia.

---

# 3. Assumptions and Technology Decisions

## 3.1 Development Environment

Recommended environment:

- Visual Studio 2022 Community or higher
- ASP.NET Web Application (.NET Framework)
- Web Forms template
- Microsoft SQL Server Express / LocalDB
- SQL Server Management Studio (SSMS)
- IIS Express for development/testing

## 3.2 Required Technologies

| Area | Technology |
|---|---|
| Frontend | HTML5 |
| Styling | CSS3 |
| Client-side scripting | JavaScript |
| UI framework | Bootstrap 5 |
| Server-side | ASP.NET Web Forms |
| Backend language | C# |
| Database | Microsoft SQL Server |
| Database access | ADO.NET |
| Authentication | Forms Authentication |
| Development | Visual Studio 2022 |
| Local server | IIS Express / IIS |

## 3.3 Technologies Not Recommended for This Assignment

The main implementation should not be changed to:

- ASP.NET MVC
- ASP.NET Core / .NET 5+
- Razor Pages
- React
- Angular
- Node.js backend
- Entity Framework as the primary database-access method

The proposal specifically defines ASP.NET Web Forms with C# and ADO.NET.

---

# 4. Main User Roles

## 4.1 Student

A registered student should be able to:

- Register
- Login
- Logout
- Manage profile
- Browse learning resources
- Search resources
- Filter resources
- View learning resources
- Download lecture notes/files
- View assignments
- Submit assignments
- Track assignment submissions
- View submission marks and feedback
- Take quizzes
- View quiz results
- Read announcements
- Create discussion posts
- Reply to forum discussions
- Manage their own posts/replies where appropriate

## 4.2 Administrator / Lecturer

The administrator should be able to:

- Login
- Logout
- View dashboard
- Manage users
- Manage resources
- Upload learning materials
- Manage assignments
- View student submissions
- Grade assignments
- Manage quizzes
- Manage quiz questions
- Manage announcements
- Manage forum content
- Moderate forum activity
- Monitor system activity

---

# 5. Public Website Pages

Recommended public pages:

```text
Default.aspx
About.aspx
Resources/BrowseResources.aspx
Resources/ResourceDetails.aspx
Announcements.aspx
Forum/Forum.aspx
Forum/PostDetails.aspx
Contact.aspx
Account/Login.aspx
Account/Register.aspx
Account/Logout.aspx
```

## 5.1 Default.aspx

The home page should contain:

- UniSkill Hub logo
- Navigation bar
- Hero section
- Short system introduction
- Featured resources
- Latest announcements
- Popular learning resources
- Register button
- Login button
- Footer

Use semantic HTML5 structure:

```html
<header>
<nav>
<main>
<section>
<article>
<footer>
```

---

# 6. Student Module

Create the following folder:

```text
/Student/
```

Recommended pages:

```text
Student/
├── Dashboard.aspx
├── Profile.aspx
├── Resources.aspx
├── ResourceDetails.aspx
├── Assignments.aspx
├── AssignmentDetails.aspx
├── SubmitAssignment.aspx
├── MySubmissions.aspx
├── Quizzes.aspx
├── TakeQuiz.aspx
├── QuizResult.aspx
├── Announcements.aspx
├── Forum.aspx
├── CreatePost.aspx
├── PostDetails.aspx
└── Web.config
```

## 6.1 Student Dashboard

Display a summary such as:

```text
Welcome, Student Name

--------------------------------
My Learning
- Resources viewed
- Pending assignments
- Submitted assignments
- Quiz attempts

Recent Announcements

Upcoming Assignments

Recent Forum Discussions
```

The dashboard should be the default destination after successful student login.

---

# 7. Profile Management

## Profile.aspx

Students should be able to:

- View full name
- View username
- View email
- Edit full name
- Edit email
- Change password

Suggested controls:

```text
txtFullName
txtEmail
txtCurrentPassword
txtNewPassword
txtConfirmPassword
btnUpdate
```

The update must be performed using a parameterized ADO.NET query.

---

# 8. Learning Resource Module

Learning resources are one of the main parts of UniSkill Hub.

Possible resource types:

- Lecture notes
- PDF documents
- Video lessons
- Audio lessons
- Articles
- External learning links
- Interactive learning content

## 8.1 Resource Browsing

Students should be able to:

- Search
- Filter by category
- Filter by resource type
- View resource details
- Download resource files

Example interface:

```text
Search: [ __________________ ]

Category: [ Programming ▼ ]
Type:     [ PDF ▼ ]

-----------------------------------------
Python Basics
Lecture Notes
PDF

[View] [Download]
-----------------------------------------
Database Fundamentals
Lecture Notes
PDF

[View] [Download]
-----------------------------------------
```

---

# 9. Multimedia Requirements

The website should clearly demonstrate multimedia.

## 9.1 Video

Use the HTML5 video element:

```html
<video controls width="700">
    <source src="Videos/python-introduction.mp4" type="video/mp4">
</video>
```

## 9.2 Audio

```html
<audio controls>
    <source src="Audio/database-introduction.mp3" type="audio/mpeg">
</audio>
```

## 9.3 Images

```html
<img src="Images/learning-resource.jpg" alt="Learning material">
```

## 9.4 Interactive Content

The online quiz provides an interactive learning experience.

The project can therefore demonstrate:

- Video
- Audio
- Images
- Interactive quiz

---

# 10. Assignment Module

The assignment system should support the complete student-to-admin workflow.

## 10.1 Assignment List

Example:

| Assignment | Due Date | Status |
|---|---|---|
| Web Application Assignment | 30 Sept | Pending |
| Database Assignment | 5 Oct | Submitted |

## 10.2 Assignment Details

Display:

- Assignment title
- Description
- Instructions
- Due date
- Maximum marks
- Attached instruction file
- Current submission status

Buttons:

```text
[Download Instructions]
[Submit Assignment]
```

---

# 11. Assignment Submission

## SubmitAssignment.aspx

Suggested fields:

```text
Assignment
File Upload
Comment
Submit Button
```

Example:

```text
Assignment:
[Web Application Assignment]

File:
[Choose File]

Comment:
[________________________]

[Submit Assignment]
```

Validate:

- File selected
- File extension
- Maximum file size
- Assignment exists
- User is authenticated
- Submission deadline

Recommended storage:

```text
/Uploads/Assignments/
```

Store the file path in the database.

---

# 12. Submission Tracking

## MySubmissions.aspx

Example:

| Assignment | Submitted Date | Status | Marks |
|---|---|---|---|
| Web Application | 20 Sept | Submitted | 85 |
| Database | — | Pending | — |

Students must only see their own submissions.

---

# 13. Quiz Module

The quiz module provides online self-assessment.

## 13.1 Quiz List

Example:

```text
Python Basics Quiz
10 Questions
Time: 10 Minutes

[Start Quiz]
```

## 13.2 TakeQuiz.aspx

Example:

```text
Question 1

Which language is mainly used in this module?

○ Java
○ Python
○ C++
○ PHP

Question 2
...

[Submit Quiz]
```

Use RadioButton or RadioButtonList controls for single-choice questions.

---

# 14. Quiz Result

After submission, show:

```text
Quiz Completed

Score: 8 / 10

Percentage: 80%

Correct Answers: 8
Wrong Answers: 2

[Review Answers]
[Back to Quizzes]
```

Store quiz attempts in the database.

---

# 15. Announcement Module

## Student View

Example:

```text
Latest Announcements

----------------------------------
Mid-Semester Examination

Date: 15 September 2026

The examination schedule has been
published.
----------------------------------
```

## Admin Functions

Admin can:

- Add announcement
- Display announcements
- Edit announcement
- Delete announcement
- Publish/unpublish announcement
- Set expiry date

---

# 16. Discussion Forum

The forum should provide a simple structured space for academic discussion.

## 16.1 Forum Page

Example:

```text
Discussion Forum

[Create New Discussion]

---------------------------------
How do I understand recursion?
Posted by Student
Replies: 5
---------------------------------

Database normalization question
Posted by Student
Replies: 3
---------------------------------
```

## 16.2 CreatePost.aspx

Fields:

```text
Title
Content
Category
[Post Discussion]
```

## 16.3 PostDetails.aspx

Display:

```text
Question Title

Posted by Student

Question content

------------------------
Reply 1
Reply 2
Reply 3

[Write Reply]
```

Students can:

- Create posts
- Read posts
- Reply
- Delete their own posts/replies

Admins can moderate or delete inappropriate content.

---

# 17. Administrator Module

Create:

```text
/Admin/
```

Recommended pages:

```text
Admin/
├── Dashboard.aspx
├── ManageUsers.aspx
├── ManageResources.aspx
├── ManageAssignments.aspx
├── ManageSubmissions.aspx
├── ManageQuizzes.aspx
├── ManageQuestions.aspx
├── ManageAnnouncements.aspx
├── ManageForum.aspx
└── Web.config
```

---

# 18. Admin Dashboard

The dashboard should provide an overview:

```text
ADMIN DASHBOARD

Students          125
Resources          48
Assignments        12
Quizzes             8
Announcements       6
Forum Posts        102

--------------------------------

Recent Registrations

Recent Submissions

Recent Forum Activity
```

Suggested dashboard indicators:

- Total students
- Total resources
- Total assignments
- Total submissions
- Total quizzes
- Total announcements
- Total forum posts

---

# 19. Manage Users

## ManageUsers.aspx

Admin functions:

- View users
- Search users
- Add user
- Edit user
- Delete/deactivate user
- Manage account status

Example GridView:

| ID | Name | Email | Role | Date | Action |
|---|---|---|---|---|---|
| 1 | Student | student@email.com | Student | ... | Edit / Delete |
| 2 | Admin | admin@email.com | Admin | ... | Edit / Delete |

---

# 20. Manage Resources

## ManageResources.aspx

Admin should be able to:

- Add resource
- View resource
- Edit resource
- Delete resource
- Upload files

Suggested fields:

```text
Title
Description
Category
Resource Type
File
Video URL/Path
Audio URL/Path
External URL
Created By
Status
```

---

# 21. Manage Assignments

Admin functions:

- Add assignment
- Edit assignment
- Delete assignment
- View assignment
- Set due date
- Set maximum marks
- Upload instructions

Suggested fields:

```text
Assignment Title
Description
Instructions
Due Date
Maximum Marks
Attachment
Status
```

---

# 22. Manage Submissions

Admin can view:

| Student | Assignment | Submitted | Status | Marks |
|---|---|---|---|---|
| Student | WAPP | 20 Sept | Submitted | 85 |
| Student 2 | WAPP | 21 Sept | Submitted | 90 |

Admin functions:

- View submission
- Download submitted file
- Enter marks
- Add feedback
- Update status

---

# 23. Manage Quizzes

Admin functions:

- Create quiz
- Edit quiz
- Delete quiz
- Publish quiz
- Unpublish quiz

Quiz fields:

```text
Quiz Title
Description
Time Limit
Total Marks
Status
```

---

# 24. Manage Quiz Questions

Admin can add:

```text
Question
Option A
Option B
Option C
Option D
Correct Answer
Marks
```

Example:

```text
Question:
What does HTML stand for?

A. Hyper Text Markup Language
B. High Text Machine Language
C. Hyperlink Text Management Language
D. Home Tool Markup Language

Correct Answer:
A
```

---

# 25. Database Design

Database name:

```text
UniSkillHubDB
```

Recommended tables:

1. Users
2. Categories
3. Resources
4. Assignments
5. Submissions
6. Quizzes
7. QuizQuestions
8. QuizOptions
9. QuizAttempts
10. Announcements
11. ForumPosts
12. ForumReplies

---

# 26. Users Table

```text
Users
---------
UserID PK
FullName
Username
Email
PasswordHash
PasswordSalt
Role
Status
DateRegistered
```

Allowed roles:

```text
Student
Admin
```

---

# 27. Categories Table

```text
Categories
------------
CategoryID PK
CategoryName
Description
```

Possible categories:

```text
Programming
Database
Web Development
Networking
HCI
```

Categories are used to organize resources without introducing a full multi-course management system.

---

# 28. Resources Table

```text
Resources
------------
ResourceID PK
CategoryID FK
Title
Description
ResourceType
FilePath
VideoPath
AudioPath
ExternalURL
CreatedBy FK
DateCreated
Status
```

Possible resource types:

```text
PDF
Video
Audio
Article
Link
```

---

# 29. Assignments Table

```text
Assignments
------------
AssignmentID PK
Title
Description
Instructions
DueDate
MaxMarks
FilePath
CreatedBy FK
DateCreated
Status
```

---

# 30. Submissions Table

```text
Submissions
------------
SubmissionID PK
AssignmentID FK
StudentID FK
FilePath
Comment
SubmittedDate
Status
Marks
Feedback
```

---

# 31. Quizzes Table

```text
Quizzes
------------
QuizID PK
Title
Description
TimeLimit
TotalMarks
CreatedBy FK
DateCreated
Status
```

---

# 32. QuizQuestions Table

```text
QuizQuestions
-------------
QuestionID PK
QuizID FK
QuestionText
Marks
CorrectOption
```

---

# 33. QuizOptions Table

```text
QuizOptions
-----------
OptionID PK
QuestionID FK
OptionText
OptionLetter
```

Example:

```text
QuestionID = 1

A = Python
B = Java
C = HTML
D = CSS
```

---

# 34. QuizAttempts Table

```text
QuizAttempts
------------
AttemptID PK
QuizID FK
StudentID FK
Score
TotalMarks
Percentage
AttemptDate
```

---

# 35. Announcements Table

```text
Announcements
-------------
AnnouncementID PK
Title
Content
PostedBy FK
PublishDate
ExpiryDate
Status
```

---

# 36. ForumPosts Table

```text
ForumPosts
----------
PostID PK
UserID FK
Title
Content
Category
DatePosted
Status
```

---

# 37. ForumReplies Table

```text
ForumReplies
------------
ReplyID PK
PostID FK
UserID FK
ReplyText
DatePosted
Status
```

---

# 38. ERD Relationships

The main relationship structure should be:

```text
Users
  │
  ├──────────────< Resources
  │                   │
  │                   └──── Categories
  │
  ├──────────────< Assignments
  │                   │
  │                   └────< Submissions
  │
  ├──────────────< Quizzes
  │                   │
  │                   └────< QuizQuestions
  │                               │
  │                               └────< QuizOptions
  │
  ├──────────────< QuizAttempts
  │
  ├──────────────< Announcements
  │
  ├──────────────< ForumPosts
  │                   │
  │                   └────< ForumReplies
```

More specifically:

```text
Users 1 ──── * Resources
Categories 1 ──── * Resources

Users 1 ──── * Assignments
Assignments 1 ──── * Submissions
Users 1 ──── * Submissions

Users 1 ──── * Quizzes
Quizzes 1 ──── * QuizQuestions
QuizQuestions 1 ──── * QuizOptions

Users 1 ──── * QuizAttempts
Quizzes 1 ──── * QuizAttempts

Users 1 ──── * Announcements

Users 1 ──── * ForumPosts
ForumPosts 1 ──── * ForumReplies
Users 1 ──── * ForumReplies
```

---

# 39. CRUD Requirement Mapping

The system must visibly demonstrate all four database operations.

| Operation | Example |
|---|---|
| INSERT | Register a new student |
| INSERT | Add resource |
| INSERT | Add assignment |
| INSERT | Submit assignment |
| INSERT | Create forum post |
| SELECT | Browse resources |
| SELECT | View assignments |
| SELECT | View announcements |
| SELECT | View submissions |
| UPDATE | Edit student profile |
| UPDATE | Edit resource |
| UPDATE | Edit assignment |
| UPDATE | Grade submission |
| UPDATE | Edit announcement |
| DELETE | Delete resource |
| DELETE | Delete assignment |
| DELETE | Delete announcement |
| DELETE | Delete forum post |

For the final demonstration, clearly show at least:

- One INSERT
- One SELECT
- One UPDATE
- One DELETE

---

# 40. Authentication

Use Forms Authentication.

Login flow:

```text
Login Page
    ↓
Validate Input
    ↓
Find User in Database
    ↓
Verify Password Hash
    ↓
Check Account Status
    ↓
Create Authentication Session
    ↓
Check Role
    ↓
Student Dashboard / Admin Dashboard
```

---

# 41. Registration

## Register.aspx

Required fields:

```text
Full Name
Username
Email
Password
Confirm Password
```

Validation:

```text
Full Name → Required
Username → Required + Unique
Email → Required + Valid Email
Password → Required + Minimum Length
Confirm Password → Must Match Password
```

Recommended ASP.NET validation controls:

```text
RequiredFieldValidator
RegularExpressionValidator
CompareValidator
CustomValidator
```

---

# 42. Password Security

Do not store passwords as plain text.

Incorrect:

```text
Password = 123456
```

Use:

```text
PasswordHash
PasswordSalt
```

Passwords should be salted and securely hashed before storage.

---

# 43. ADO.NET Architecture

Recommended simple architecture:

```text
Web Forms Page
       ↓
Code-Behind
       ↓
DBHelper.cs
       ↓
SQL Server
```

Example:

```text
ManageResources.aspx
       ↓
ManageResources.aspx.cs
       ↓
DBHelper.cs
       ↓
SQL Server
```

This keeps database logic centralized and easier to maintain.

---

# 44. DBHelper.cs

Create:

```text
App_Code/DBHelper.cs
```

Suggested methods:

```text
ExecuteNonQuery()
ExecuteScalar()
ExecuteReader()
GetDataTable()
```

Always use parameterized SQL.

Correct:

```csharp
SqlCommand cmd = new SqlCommand(
    "SELECT * FROM Users WHERE Email=@Email",
    con);

cmd.Parameters.AddWithValue("@Email", email);
```

Avoid SQL string concatenation:

```csharp
"SELECT * FROM Users WHERE Email='" + email + "'"
```

---

# 45. PasswordHelper.cs

Create:

```text
App_Code/PasswordHelper.cs
```

Responsibilities:

- Generate password salt
- Hash passwords
- Verify passwords

Do not implement password storage using plain text.

---

# 46. File and Folder Structure

Recommended complete structure:

```text
UniSkillHub/
│
├── Account/
│   ├── Login.aspx
│   ├── Login.aspx.cs
│   ├── Register.aspx
│   ├── Register.aspx.cs
│   ├── Logout.aspx
│   └── Logout.aspx.cs
│
├── Admin/
│   ├── Dashboard.aspx
│   ├── ManageUsers.aspx
│   ├── ManageResources.aspx
│   ├── ManageAssignments.aspx
│   ├── ManageSubmissions.aspx
│   ├── ManageQuizzes.aspx
│   ├── ManageQuestions.aspx
│   ├── ManageAnnouncements.aspx
│   ├── ManageForum.aspx
│   └── Web.config
│
├── Student/
│   ├── Dashboard.aspx
│   ├── Profile.aspx
│   ├── Resources.aspx
│   ├── ResourceDetails.aspx
│   ├── Assignments.aspx
│   ├── AssignmentDetails.aspx
│   ├── SubmitAssignment.aspx
│   ├── MySubmissions.aspx
│   ├── Quizzes.aspx
│   ├── TakeQuiz.aspx
│   ├── QuizResult.aspx
│   ├── Announcements.aspx
│   ├── Forum.aspx
│   ├── CreatePost.aspx
│   ├── PostDetails.aspx
│   └── Web.config
│
├── Resources/
│   ├── BrowseResources.aspx
│   └── ResourceDetails.aspx
│
├── Uploads/
│   ├── Notes/
│   ├── Assignments/
│   ├── Videos/
│   └── Audio/
│
├── Css/
│   └── site.css
│
├── Scripts/
│   ├── site.js
│   └── validation.js
│
├── Images/
│
├── App_Code/
│   ├── DBHelper.cs
│   ├── PasswordHelper.cs
│   └── Utility.cs
│
├── Site.Master
├── Site.Master.cs
├── Web.config
├── Web.sitemap
├── Default.aspx
├── About.aspx
├── Announcements.aspx
├── Contact.aspx
└── Global.asax
```

---

# 47. Naming Convention

Use consistent naming conventions.

## TextBox

```text
txtFullName
txtEmail
txtTitle
txtDescription
```

## Button

```text
btnLogin
btnRegister
btnSave
btnUpdate
btnDelete
btnSubmit
```

## GridView

```text
gvUsers
gvResources
gvAssignments
gvSubmissions
```

## DropDownList

```text
ddlCategory
ddlResourceType
ddlRole
```

## FileUpload

```text
fuResource
fuAssignment
```

## Labels

```text
lblMessage
lblUsername
lblScore
```

## Repeaters

```text
rptResources
rptAnnouncements
rptForumPosts
```

Use PascalCase for classes, pages, and files. Keep server-control prefixes consistent.

---

# 48. Master Page

Use:

```text
Site.Master
```

It should contain:

- Logo
- Main navigation
- Login/register links
- User menu
- Footer

## Guest Navigation

```text
Home
Resources
Announcements
Forum
About
Contact
Login
Register
```

## Student Navigation

```text
Home
Resources
Assignments
Quizzes
Announcements
Forum
My Profile
Logout
```

## Admin Navigation

```text
Dashboard
Users
Resources
Assignments
Submissions
Quizzes
Announcements
Forum
Logout
```

---

# 49. Web.config Authorization

Use Forms Authentication in the root Web.config.

The Student folder must prevent anonymous users.

Conceptually:

```text
Guest
  ↓
Public Pages Only

Student
  ↓
Student Pages

Admin
  ↓
Admin Pages
```

A student must not be able to access:

```text
/Admin/ManageUsers.aspx
/Admin/ManageResources.aspx
```

An unauthenticated user must not be able to access student pages.

---

# 50. Form Validation

Validation should be applied to every important form.

## Registration

- RequiredFieldValidator
- RegularExpressionValidator
- CompareValidator
- CustomValidator

## Login

- RequiredFieldValidator

## Resource Form

- Required title
- Required description
- Required type
- File extension validation
- File-size validation

## Assignment Form

- Required title
- Required description
- Due-date validation
- File validation

## Forum

- Required title
- Required content

## Quiz

- Required question
- Required options
- Required correct answer

Use both:

```text
Client-side validation
+
Server-side validation
```

Always re-check important values on the server.

---

# 51. HTML5 Requirements

Use semantic elements intentionally:

```html
<header>
<nav>
<main>
<section>
<article>
<aside>
<footer>
<figure>
<figcaption>
<video>
<audio>
```

Use suitable HTML5 input types:

```html
<input type="email">
<input type="date">
<input type="number">
<input type="search">
```

This makes the HTML5 requirement easy to demonstrate during the presentation.

---

# 52. CSS Requirements

Use all three CSS approaches required by the assignment.

## External CSS

```text
Css/site.css
```

This should be the main stylesheet.

## Internal CSS

Example:

```html
<style>
    .quiz-header {
        margin-bottom: 20px;
    }
</style>
```

## Inline CSS

Example:

```html
<span style="font-weight:bold;">
    Due Soon
</span>
```

The project should mainly use external CSS while demonstrating internal and inline CSS for the assignment requirement.

---

# 53. Bootstrap

Bootstrap can be used for:

- Navbar
- Cards
- Buttons
- Forms
- Tables
- Alerts
- Modal dialogs
- Grid layout
- Responsive design

Do not allow Bootstrap to replace the demonstration of your own CSS. Keep a project stylesheet such as:

```text
Css/site.css
```

---

# 54. Search Functionality

Resource browsing should contain:

```text
[ Search lecture notes... ] [Search]
```

Search by:

- Resource title
- Description

Optional filters:

- Category
- Resource type
- Date

Parameterized query concept:

```sql
WHERE Title LIKE @Search
   OR Description LIKE @Search
```

---

# 55. File Management

Recommended file storage:

```text
Uploads/
    Notes/
    Assignments/
    Videos/
    Audio/
```

Store file paths in SQL Server.

Example:

```text
Uploads/Notes/web-development.pdf
```

Do not unnecessarily store large multimedia files directly in relational database fields.

---

# 56. Security Requirements

The project should include:

- Forms Authentication
- Role-based authorization
- Password hashing and salt
- Parameterized SQL
- Server-side validation
- File validation
- Authentication checks
- Authorization checks
- Safe error handling
- Session protection
- Access restriction between users

Never trust only browser/client-side validation.

---

# 57. Activity Monitoring

Admin dashboard should provide simple statistics:

```text
Total Students
Total Resources
Total Assignments
Total Submissions
Total Quizzes
Total Announcements
Total Forum Posts
```

Also display:

```text
Recent Registrations
Recent Submissions
Recent Forum Activity
```

This provides clear evidence of monitoring functionality.

---

# 58. Optional Gemini AI Feature

AI note summarization is an optional enhancement.

Recommended workflow:

```text
Lecture Note
     ↓
Extract Text
     ↓
Gemini API
     ↓
Generate Summary
     ↓
Display Summary
```

Important:

- Do not make the mandatory system dependent on Gemini.
- First complete all core Web Forms functions.
- Add the AI feature only if time and resources allow.

The AI feature should remain optional because it is outside the essential CRUD/authentication/admin requirements.

---

# 59. Functions That Should Remain Out of Scope

Do not spend project time on the following unless the project is already complete:

```text
Real-time chat
Live video classes
Dedicated mobile application
AI assignment grading
Advanced AI marking
Push notifications
Full multi-course management
```

These should remain future enhancements or limitations rather than mandatory implementation.

---

# 60. Navigation Structure

Recommended navigation structure:

```text
                         UNI SKILL HUB
                              │
          ┌───────────────────┼───────────────────┐
          │                   │                   │
        Guest              Student             Admin
          │                   │                   │
    ┌─────┼─────┐      ┌──────┼──────┐     ┌─────┼──────────┐
    │     │     │      │      │      │     │     │          │
  Home  About Login  Dashboard Resources Assignments Users Resources
    │          │        │         │         │      │       │
Resources   Register   Profile    Notes    Submit   ...    ...
    │                            │         │
Announcements                 Download    Quiz
    │                                      │
  Forum                                    Result
```

---

# 61. Complete Page List

## Public

```text
1.  Default.aspx
2.  About.aspx
3.  BrowseResources.aspx
4.  ResourceDetails.aspx
5.  Announcements.aspx
6.  Forum.aspx
7.  PostDetails.aspx
8.  Contact.aspx
9.  Login.aspx
10. Register.aspx
```

## Student

```text
11. Dashboard.aspx
12. Profile.aspx
13. Resources.aspx
14. ResourceDetails.aspx
15. Assignments.aspx
16. AssignmentDetails.aspx
17. SubmitAssignment.aspx
18. MySubmissions.aspx
19. Quizzes.aspx
20. TakeQuiz.aspx
21. QuizResult.aspx
22. Announcements.aspx
23. Forum.aspx
24. CreatePost.aspx
25. PostDetails.aspx
```

## Admin

```text
26. Dashboard.aspx
27. ManageUsers.aspx
28. ManageResources.aspx
29. ManageAssignments.aspx
30. ManageSubmissions.aspx
31. ManageQuizzes.aspx
32. ManageQuestions.aspx
33. ManageAnnouncements.aspx
34. ManageForum.aspx
```

Some public/student/admin pages can be redesigned as shared components if that reduces duplication.

---

# 62. Important Web Forms Controls

Use Web Forms controls deliberately:

```text
TextBox
Label
Button
LinkButton
HyperLink
DropDownList
RadioButton
RadioButtonList
CheckBox
CheckBoxList
FileUpload
GridView
Repeater
DataList
Panel
MultiView
```

Validation controls:

```text
RequiredFieldValidator
RegularExpressionValidator
CompareValidator
CustomValidator
ValidationSummary
```

Especially use GridView for administrative CRUD pages.

---

# 63. Main Workflows

## 63.1 Student Registration

```text
Register
 ↓
Validate Input
 ↓
Check Duplicate Username / Email
 ↓
Hash Password
 ↓
INSERT Users
 ↓
Registration Successful
 ↓
Login
```

## 63.2 Student Login

```text
Login
 ↓
Validate Input
 ↓
SELECT User
 ↓
Verify Password
 ↓
Check Role
 ↓
Create Authentication
 ↓
Redirect to Dashboard
```

## 63.3 Add Resource

```text
Admin
 ↓
Manage Resources
 ↓
Add
 ↓
Validate
 ↓
Upload File
 ↓
INSERT Resources
 ↓
Display New Resource
```

## 63.4 Assignment Submission

```text
Student
 ↓
Assignments
 ↓
Assignment Details
 ↓
Submit
 ↓
Upload File
 ↓
INSERT Submission
 ↓
Admin Views Submission
 ↓
Admin Grades
 ↓
Student Views Result
```

## 63.5 Quiz

```text
Student
 ↓
Select Quiz
 ↓
Load Questions
 ↓
Answer Questions
 ↓
Submit
 ↓
Calculate Score
 ↓
INSERT QuizAttempt
 ↓
Display Result
```

---

# 64. Non-Functional Requirements

## 64.1 Usability

- Simple navigation
- Consistent layout
- Clear forms
- Clear validation messages
- Breadcrumbs where useful
- Responsive design
- Consistent button placement
- Readable typography

## 64.2 Performance

- Use efficient queries
- Avoid unnecessary database calls
- Use paging in large GridViews
- Optimize image sizes
- Avoid loading large datasets unnecessarily

## 64.3 Security

- Password hashing
- Parameterized SQL
- Authentication
- Role-based authorization
- Server-side validation
- Upload restrictions

## 64.4 Reliability

- Error handling
- Database constraints
- Validation
- Avoid duplicate records
- Safe file uploads
- Prevent unauthorized operations

## 64.5 Maintainability

- Clear folder structure
- Consistent naming
- Master Page
- Central database helper
- Reusable CSS
- Reusable utility functions
- Clear comments for important logic

---

# 65. Team Division for Four Members

## Member 1 — Authentication + Student

Responsible for:

```text
Registration
Login
Logout
Profile
Student Dashboard
Resources
```

## Member 2 — Assignments

Responsible for:

```text
Admin Assignments
Student Assignments
Assignment Details
Submission
File Upload
Admin Grading
Submission Tracking
```

## Member 3 — Quizzes + Announcements

Responsible for:

```text
Quiz Management
Question Management
Quiz Attempt
Quiz Result
Announcements
```

## Member 4 — Forum + UI + Integration

Responsible for:

```text
Forum
Master Page
CSS
Bootstrap
Navigation
Responsive Design
Integration Testing
```

One team member should coordinate final SQL database integration so that all modules use the same database structure.

---

# 66. Final Demonstration Plan

During the final presentation, demonstrate a complete workflow rather than only showing pages.

## Demonstration 1 — Registration

```text
Register
→ New user
→ Database record created
```

## Demonstration 2 — Login

```text
Login
→ Student Dashboard
```

## Demonstration 3 — Resource

```text
Admin
→ Add Resource
→ Resource appears in Student Resource page
```

## Demonstration 4 — Assignment

```text
Admin
→ Create Assignment

Student
→ Open Assignment
→ Upload Submission

Admin
→ View Submission
→ Give Marks

Student
→ View Marks/Feedback
```

## Demonstration 5 — Quiz

```text
Student
→ Start Quiz
→ Answer
→ Submit
→ View Result
```

## Demonstration 6 — Forum

```text
Student
→ Create Discussion
→ Another Student Replies
→ Admin Moderates
```

## Demonstration 7 — CRUD

Explicitly demonstrate:

```text
INSERT
SELECT
UPDATE
DELETE
```

---

# 67. Final Implementation Checklist

```text
[ ] ASP.NET Web Forms project created
[ ] C# code-behind used
[ ] SQL Server database created
[ ] ADO.NET connectivity implemented

[ ] INSERT working
[ ] SELECT working
[ ] UPDATE working
[ ] DELETE working

[ ] Registration working
[ ] Login working
[ ] Logout working
[ ] Student role working
[ ] Admin role working
[ ] Unauthorized access blocked

[ ] Student profile update working

[ ] Resource upload working
[ ] Resource display working
[ ] Resource download working
[ ] Resource CRUD working

[ ] Assignment creation working
[ ] Assignment display working
[ ] Assignment submission working
[ ] Submission viewing working
[ ] Assignment grading working

[ ] Quiz creation working
[ ] Question management working
[ ] Quiz attempt working
[ ] Quiz scoring working
[ ] Quiz result working

[ ] Announcement management working

[ ] Forum post working
[ ] Forum reply working
[ ] Forum moderation working

[ ] Search working
[ ] Form validation working
[ ] Server-side validation working

[ ] HTML5 semantic elements demonstrated
[ ] External CSS demonstrated
[ ] Internal CSS demonstrated
[ ] Inline CSS demonstrated

[ ] Video demonstrated
[ ] Audio demonstrated
[ ] Images demonstrated
[ ] Interactive quiz demonstrated

[ ] Responsive layout
[ ] Consistent navigation
[ ] Master Page
[ ] Web.config authorization
[ ] Parameterized SQL
[ ] Password hashing
[ ] File validation
[ ] Error handling

[ ] Proper folder structure
[ ] Proper naming convention
[ ] ERD completed
[ ] Wireframes completed
[ ] Navigation diagram completed
[ ] Source-code screenshots/snippets prepared
[ ] User guidance screenshots prepared
[ ] Final testing completed
```

---

# 68. Recommended Build Order

Do not build all pages at the same time.

```text
1. Create Web Forms project
        ↓
2. Create SQL Server database
        ↓
3. Create all tables and relationships
        ↓
4. Create DBHelper.cs
        ↓
5. Create PasswordHelper.cs
        ↓
6. Build Master Page and CSS
        ↓
7. Build Registration
        ↓
8. Build Login + role authentication
        ↓
9. Build Student Dashboard
        ↓
10. Build Resource module
        ↓
11. Build Assignment + Submission module
        ↓
12. Build Quiz module
        ↓
13. Build Announcement module
        ↓
14. Build Forum module
        ↓
15. Build Admin Dashboard
        ↓
16. Build Admin CRUD pages
        ↓
17. Add multimedia
        ↓
18. Add validation and security
        ↓
19. Test responsive design
        ↓
20. Final integration testing
```

---

# 69. Documentation Requirements

The final report should include the required sections.

## Recommended report structure

```text
1. Cover Page
2. Table of Contents
3. Introduction / Project Plan
4. Requirement Specification
5. Design and Modeling
6. Implementation
7. User Guidance
8. Conclusion
9. References
10. Appendix
```

## Requirement Specification

Include:

- Audience modeling
- Use cases
- Flowcharts
- Major functions

## Design and Modeling

Include:

- ERD
- Wireframes for major layouts
- Website navigation structure
- Descriptions of components

## Implementation

Include explanations of:

- HTML/ASPX
- CSS
- Form validation
- ADO.NET database connectivity
- SQL queries
- Major Web Forms features
- Authentication
- File upload

Do not include the entire source code in the report.

## User Guidance

Include:

- Screenshots
- Page names
- Explanation of each screenshot
- Instructions for common tasks

## Conclusion

Include:

- Project summary
- Lessons learned
- Limitations
- Future enhancements

## Appendix

Include:

- Proposal report

---

# 70. Corrections to Proposal Wording / Scope

The proposal is generally aligned with the assignment, but these points should be cleaned up in the final project documentation.

## 70.1 Use ASP.NET Web Forms Consistently

Where the proposal says:

```text
ASP-based web-based system
```

use:

```text
ASP.NET Web Forms-based web application
```

## 70.2 Forum Administrator Role

The proposal contains wording about the administrator working to "postpone the forum."

For the actual system, use:

```text
Admin can manage and moderate forum content.
```

## 70.3 Subjects vs Categories

The proposal mentions management of subjects in the administrator scope but also states that this version does not support multiple courses or subjects.

For the implementation, use:

```text
Resource Categories
```

such as:

```text
Programming
Database
Web Development
Networking
HCI
```

rather than implementing a full multi-course architecture.

## 70.4 AI Summarization

Keep Gemini note summarization optional.

Core functionality must work without it.

---

# 71. Final System Architecture

The final architecture can be represented as:

```text
                    UNI SKILL HUB
                         │
                 ASP.NET Web Forms
                         │
          ┌──────────────┴──────────────┐
          │                             │
       Student                        Admin
          │                             │
          ├─ Dashboard                  ├─ Dashboard
          ├─ Resources                  ├─ Users
          ├─ Assignments                ├─ Resources
          ├─ Submissions                ├─ Assignments
          ├─ Quizzes                    ├─ Submissions
          ├─ Announcements              ├─ Quizzes
          └─ Forum                      ├─ Announcements
                                        └─ Forum
                         │
                      ADO.NET
                         │
                      DBHelper
                         │
                  Microsoft SQL Server
                         │
        ┌────────────────┼─────────────────┐
        │                │                 │
      Users          Resources       Assignments
        │                                  │
     Quizzes                         Submissions
        │
    QuizAttempts
        │
  Announcements
        │
     ForumPosts
        │
    ForumReplies
```

---

# 72. Minimum Complete System

At minimum, the completed UniSkill Hub must successfully support this end-to-end flow:

```text
Visitor
   ↓
Register
   ↓
Login
   ↓
Student Dashboard
   ↓
Browse Resources
   ↓
View / Download Notes
   ↓
View Assignment
   ↓
Submit Assignment
   ↓
Take Quiz
   ↓
View Quiz Result
   ↓
Read Announcement
   ↓
Participate in Forum
   ↓
Logout
```

And the administrator flow:

```text
Admin Login
   ↓
Admin Dashboard
   ↓
Manage Users
   ↓
Manage Resources
   ↓
Manage Assignments
   ↓
View Submissions
   ↓
Grade Assignments
   ↓
Manage Quizzes
   ↓
Manage Announcements
   ↓
Manage Forum
   ↓
Logout
```

This provides a complete Web Forms learning-management style system while keeping the project within the scope defined in the UniSkill Hub proposal.
