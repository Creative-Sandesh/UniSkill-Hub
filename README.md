# UniSkill Hub

A university learning portal built for the *Web Applications* course. Lecturers (Admins) publish study
resources, assignments, quizzes and announcements. Students download material, submit work, take timed
quizzes and discuss in a forum. Guests can browse public content.

## Tech stack

| Layer      | Choice                                                                 |
|------------|------------------------------------------------------------------------|
| Framework  | ASP.NET **Web Forms**, .NET Framework 4.8, C# (Web Application project) |
| Data       | **ADO.NET** through one helper class (`App_Code/DBHelper.cs`)          |
| Database   | SQL Server Express (`.\SQLEXPRESS`), database `UniSkillHubDB`          |
| Security   | Forms Authentication, PBKDF2 password hashing with a per-user salt      |
| UI         | Bootstrap 5 + `Css/site.css` (design tokens, accessible, responsive)    |
| Scripting  | Vanilla JavaScript (no frameworks)                                      |

Not used, on purpose: MVC, .NET Core, Entity Framework, React, Tailwind.

## Features by role

**Guest** – landing page, About, Contact, browse resources, read announcements, read the forum,
register and log in. Downloading, posting and replying need an account.

**Student**
- Dashboard with live counts (open assignments, submissions, quiz attempts) and recent activity
- Browse/search/filter resources and download files, watch video, listen to audio
- View assignments, download the brief, upload a submission (own files only), track status, read the
  lecturer's grade and feedback
- Timed quizzes with a server-side timer, automatic scoring, result page and answer review
- Forum: start discussions, reply, delete own posts and replies
- Profile page

**Admin / Lecturer**
- Dashboard with system totals
- **Users** – search, activate/deactivate, promote/demote, delete users that have no records
- **Resources** – add/edit/delete documents, video, audio and links; draft or published
- **Assignments** and **Submissions** – create assignments, review submissions, grade with feedback or
  return for resubmission
- **Quizzes** and **Questions** – multiple-choice quizzes with a time limit and attempt limit
- **Announcements** – publish now or schedule ahead, optional expiry date, draft/publish toggle
- **Forum moderation** – hide/unhide or delete posts and individual replies; hidden items disappear for
  students but stay visible (with a warning banner) to the Admin

## Folder structure

```
UniSkillHub/
├─ Account/          Login, Register, Logout
├─ Admin/            Dashboard + Manage* pages (Admin role only)
├─ Student/          Dashboard, assignments, quizzes, profile (Student role only)
├─ Resources/        Browse, details, Download.ashx (shared public pages)
├─ Assignments/      Download.ashx for assignment briefs and submissions
├─ Forum/            Forum list, post details, create post
├─ App_Code/         DBHelper, PasswordHelper, Utility, FileHelper
├─ Css/  Scripts/    site.css, site.js, validation.js, quiz.js, admin.js
├─ Uploads/          Notes, Assignments (private), Videos, Audio
├─ Database/         UniSkillHubDB.sql, CreateAdmin.sql, SampleData.sql
├─ Site.Master       Shared layout; navigation changes with the user's role
├─ Error.aspx        Friendly page for 400 / 404 / 500 errors
└─ Web.config        Connection string, Forms Authentication, upload limits
```

## Database

13 tables and 16 foreign keys: `Users`, `Categories`, `Resources`, `Assignments`, `Submissions`,
`Quizzes`, `QuizQuestions`, `QuizOptions`, `QuizAttempts`, `QuizAnswers`, `Announcements`, `ForumPosts`,
`ForumReplies`. (`sysdiagrams` may also appear if you draw a diagram in SSMS.)

Rules worth knowing:
- Deleting an assignment removes its submissions; deleting a quiz removes its questions, options and
  attempts; deleting a forum post removes its replies (`ON DELETE CASCADE`).
- Users with activity are **deactivated**, not deleted.
- Status values: resources/assignments/quizzes/announcements `Published | Draft`, forum
  `Active | Hidden`, submissions `Submitted | Graded`, users `Active | Inactive`.
- `QuizAnswers` stores each chosen option so students can review their answers.

## Setup

**Requirements:** Visual Studio 2022 (ASP.NET workload), .NET Framework 4.8, SQL Server Express with an
instance named `SQLEXPRESS`.

1. **Create the database.** In SSMS (or `sqlcmd`) run, in this order:
   1. `Database/UniSkillHubDB.sql` – tables, constraints and categories
   2. `Database/CreateAdmin.sql` – the first Admin account
   3. `Database/SampleData.sql` – optional demo resources, assignments, quizzes, announcements and posts

   All three scripts can be run again safely.
   ```
   sqlcmd -S .\SQLEXPRESS -E -i Database\UniSkillHubDB.sql
   sqlcmd -S .\SQLEXPRESS -E -i Database\CreateAdmin.sql
   sqlcmd -S .\SQLEXPRESS -E -i Database\SampleData.sql
   ```
2. **Check the connection string** in `Web.config` (change it if your instance has another name):
   ```xml
   <add name="UniSkillHubDB"
        connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=UniSkillHubDB;Integrated Security=True;" />
   ```
3. Open `UniSkillHub.sln` in Visual Studio, restore NuGet packages, and press **F5** (IIS Express).
   If files were changed outside Visual Studio, use *Reload Project* first.

**First login:** username `admin`, password `Admin@12345`. This is a development default –
change it (or create your own Admin and deactivate this one) before showing the site to anyone.
Students create their own accounts on the Register page.

## Security

- **Passwords** – PBKDF2 (SHA-256, 100,000 iterations) with a random salt per user; compared in constant
  time; plaintext is never stored or logged.
- **SQL injection** – every query is parameterized; `LIKE` searches escape `[`, `%` and `_`.
- **Authorization is enforced on the server** – folder rules in `Web.config`, plus `Global.asax` checks the
  account in the database on every page request, so a deactivated or demoted user loses access immediately.
  Ownership (e.g. "my submission", "my reply") is checked inside the SQL statement.
- **Forms Authentication** – HttpOnly, SameSite=Lax cookie, 30-minute sliding expiry; the return URL is
  validated to prevent open redirects; login errors are generic.
- **Logout really logs out** – every login carries a random session id inside its encrypted ticket; Logout
  blacklists that id, so even a copied cookie stops working. Other browsers of the same user are unaffected.
- **Brute-force protection** – 5 wrong passwords for one account from one computer lock that pair for
  15 minutes (`App_Code/LoginThrottle.cs`); a wider per-computer limit stops password spraying. An unknown
  username costs the same time as a wrong password, so timing does not reveal which accounts exist.
- **CSRF protection** – `Site.Master` gives each browser a random token cookie and ties it (with the user name)
  to the page's ViewState, so a form posted by another website, or copied from another browser or account, is
  refused with a friendly page and nothing is changed.
- **Hardening headers** – `X-Content-Type-Options: nosniff`, `X-Frame-Options: SAMEORIGIN`, a
  `Content-Security-Policy` (`frame-ancestors`, `base-uri`, `form-action`, `object-src`), a Referrer-Policy;
  `X-Powered-By` and the ASP.NET version header are removed. IIS also refuses to serve development folders and
  files (`obj`, `Database`, `packages`, `.sql`, `.md`, `.csproj` ...), and the public `Uploads/Videos` and
  `Uploads/Audio` folders answer only for media file types.
- **Friendly errors** – visitors never see a stack trace: `customErrors` and `httpErrors` send every failure to
  `Error.aspx`. Input that looks like an HTML tag in a short field (name, username, search box, address bar)
  is still refused by ASP.NET request validation, but now with a clear message. Long free-text fields
  (forum posts, descriptions, feedback) accept tags, which are always encoded on output.
- **XSS** – all user text is HTML-encoded on output (`<%#: %>`, `Server.HtmlEncode`, `TextToHtml`);
  quiz options and forum text are encoded too.
- **File uploads** – extension whitelist, file-signature check (not just the name), size limits, random
  safe stored names, private folders blocked from direct URLs and served only through handlers that
  check who is asking; paths are confined to `/Uploads`.
  Limits: Students 10 MB; Admin documents/audio 20 MB, video 40 MB.
- **Quizzes** – the timer and the scoring run on the server; correct answers never reach the browser.
- **Concurrency** – grading uses optimistic concurrency so two lecturers cannot silently overwrite each other.

## How it was tested

Each milestone was checked by scripted HTTP tests (PowerShell `HttpClient` with real cookies) against the
running site and the real database: access control per role, valid and invalid input, SQL-injection and
XSS strings, upload rules, paging/filtering, and the effect of admin actions on what students see.
Test data uses a `ZZ` prefix and is removed afterwards. Pages were also reviewed visually with headless
Edge screenshots at desktop width. Recent suite results:

| Area                                  | Checks |
|---------------------------------------|--------|
| Admin resources & assignments CRUD    | 114/114 |
| Grading submissions                   | 56/56  |
| Quiz & question admin                 | 82/82  |
| Student quizzes                       | 58/58  |
| Student assignments & submissions     | 75/75  |
| Announcement CRUD + forum moderation  | 84/84  |
| Multimedia (video/audio) pages         | 21/21  |
| Form validation & error handling       | 72/72  |
| Security (CSRF, logout, brute force, access matrix, cross-user access, exposure, headers) | 62/62 |

Some older suites hard-code sample-data assumptions (e.g. "exactly 3 forum posts") and now report
mismatches because real data (a forum post created by a registered student) and newer sample assignments exist;
these are stale expectations, not site errors.

## Known limitations / not done yet

- The multimedia demo is a short generated video (WebM) and audio clip (WAV); replace them with real lectures via Admin > Resources.
- The login throttle and the logout blacklist live in memory, so they reset when the site restarts (a copied cookie
  would then work again until its normal expiry). A production system would keep them in the database.
- Before publishing behind HTTPS: set `requireSSL="true"` on `<httpCookies>` and `<forms>`, set
  `<compilation debug="false">`, add `Strict-Transport-Security`, and change the default admin password.
- There is no CAPTCHA or e-mail verification on registration, and no rate limit on forum posting.
- IIS Express answers HTTP range requests with 200; real IIS returns 206 so video seeking is smoother.
- Landing page does not yet pull featured content from the database.
- The optional AI note-summarization idea is intentionally **not** implemented.
- Remaining planned work: responsive testing across devices, integration testing and bug fixing.
