# UniSkill Hub – Setup guide for a new computer

This guide shows how to get UniSkill Hub running on another Windows computer, from nothing to a working site.
It is written for someone who has never seen the project. For a description of the features, see `README.md`.

> **Checked so far:** a clean copy of the project (cloned from git) restores its packages, builds, and starts, and its
> public pages open. **Not checked:** running the SQL scripts on a brand-new SQL Server, LocalDB, opening the site from
> another device, and hosting on a real IIS server. Those parts are marked *(not tested)* below.

---

## 1. What you need to install (once)

| Software | Notes |
|---|---|
| **Windows 10 or 11** | The project uses .NET Framework 4.8, which only runs on Windows. |
| **Visual Studio 2022** | While installing, tick the **"ASP.NET and web development"** workload. It brings .NET Framework 4.8 and IIS Express. |
| **SQL Server Express** | Use the default instance name `SQLEXPRESS`, so the server is called `.\SQLEXPRESS`. Make sure the Windows service **SQL Server (SQLEXPRESS)** is running. |
| **SSMS** (optional) | SQL Server Management Studio, for running and looking at scripts. `sqlcmd` also works. |
| **Git** | To download the project. |

## 2. Get the project

```
git clone <your-repository-url>
```

If you copy the folder instead (for example on a USB stick), **leave out** the `.vs`, `bin` and `obj` folders.
Visual Studio creates them again by itself.

## 3. Create the database

Open a terminal in the project folder and run these three scripts **in this order**:

```
sqlcmd -S .\SQLEXPRESS -E -i Database\UniSkillHubDB.sql
sqlcmd -S .\SQLEXPRESS -E -i Database\CreateAdmin.sql
sqlcmd -S .\SQLEXPRESS -E -i Database\SampleData.sql
```

| Script | What it does |
|---|---|
| `UniSkillHubDB.sql` | Creates the database `UniSkillHubDB`, its tables and the resource categories. |
| `CreateAdmin.sql` | Creates the first administrator account. |
| `SampleData.sql` | *Optional.* Adds demo resources, assignments, quizzes, announcements and forum posts. |

You can also open each file in SSMS and press **F5**. All three scripts are safe to run again.
The Windows account you use needs permission to create a database (a local administrator is enough).

**About the demo deadlines.** `SampleData.sql` sets assignment deadlines relative to the day you run it (10 days
ahead, 5 days ahead and 2 days *ago*). So one sample assignment is already closed from the start, and the others
close after a few days. Run the script again on the day of a demo, or change a due date under
**Admin → Assignments**.

## 4. Check the connection string

Open `Web.config` and look at:

```xml
<add name="UniSkillHubDB"
     connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=UniSkillHubDB;Integrated Security=True;" />
```

- If your SQL Server instance has another name, change `.\SQLEXPRESS`.
- If the computer only has **LocalDB**, use `(LocalDB)\MSSQLLocalDB` here, and in the `sqlcmd -S` commands too.
  *(not tested)*
- `Integrated Security=True` means the site connects with the Windows account that runs it. In Visual Studio that is
  your own login, so no password is needed.

## 5. Run the site

1. Open `UniSkillHub.sln` in Visual Studio.
2. Press **F5** (or **Ctrl+F5** to run without the debugger). Visual Studio downloads the NuGet packages by itself.
   If it does not, right-click the solution and choose **Restore NuGet Packages**.
3. If Visual Studio asks you to trust the IIS Express development certificate, click **Yes**.
4. The site opens at `https://localhost:44314/`. The plain HTTP address is `http://localhost:63169/`.

## 6. First login

| Account | Username | Password |
|---|---|---|
| Administrator | `admin` | `Admin@12345` |

**Change the admin password straight away:**
**Admin → Users → Edit** on the `admin` row, type a new password, and save. Leaving the password box empty keeps the
old password.

Students create their own accounts on the **Register** page.

## 7. Optional: AI summaries (Google Gemini)

The site works completely without this. Without a key, the "Summarize with AI" buttons are shown **disabled** with the
text "not set up yet".

1. Copy `Secrets.config.example` to a new file named `Secrets.config` in the same folder as `Web.config`.
2. Get a free key at <https://aistudio.google.com/apikey> and paste it into the file, so that the line reads
   `<add key="GeminiApiKey" value="your-key-here" />`.
3. Save and reload the page. No restart is needed.

Good to know:

- `Secrets.config` is listed in `.gitignore`, so your key is never uploaded to git. Never put the key in `Web.config`.
- If `Secrets.config` is empty or broken, the site still works. The button stays disabled and a note is written to
  `App_Data\Logs`.
- In a fresh clone the project file lists `Secrets.config` although git does not contain it. Visual Studio may show a
  small warning icon for the missing file; it is harmless. Copying the example file, even with a blank key, removes it.
- If Google stops accepting the model name, change `GeminiModel` in `Web.config`. Current names are listed at
  <https://ai.google.dev/gemini-api/docs/models>.
- **Privacy:** when someone clicks the button, the text of the assignment or resource file is sent to Google.

## 8. Quick check that everything works

1. Open the home page. You should see the **Explore by category** tiles.
2. Click **Register**, create a student account, and log in.
3. Open **Assignments**, pick one that is still open, click **Submit assignment**, and upload a small PDF. You should
   see *"Success! Your assignment has been submitted."*
4. Log in as `admin`, open **Submissions**, and grade that submission.
5. Log in as the student again. The marks and feedback should now be shown.

## 9. Opening the site from another device *(not tested)*

IIS Express only answers requests from the same computer (`localhost`). The easiest options are to demonstrate from
the same laptop, or to host the site on IIS (section 10). To reach it from a phone on the same network you would need
all of these:

- a binding for the computer's IP address in the site's IIS Express configuration (`.vs\...\config\applicationhost.config`),
- a URL reservation, or Visual Studio started as administrator,
- a Windows Firewall rule for the port.

## 10. Hosting on a real IIS server *(not tested)*

- Publish with the **Release** configuration. That switches on secure-only cookies and the HSTS header, so the site
  needs an **HTTPS certificate**. Without one, nobody can log in.
- Give the application-pool identity:
  - access to the `UniSkillHubDB` database, and
  - **write** permission on the `Uploads` and `App_Data` folders.
- Change the admin password before letting anyone else in.

## 11. Troubleshooting

| Problem | Likely cause and fix |
|---|---|
| Every page shows a 500 error | `Web.config` is not valid XML, or the database does not exist. Look at `App_Data\Logs\error-<date>.log` for the reason. |
| "Cannot open database" or pages fail to load data | The scripts in section 3 have not been run, or the SQL Server service is stopped. |
| Build errors about missing packages | Restore NuGet packages (section 5). |
| An assignment has no submit button | Its deadline has passed. Check the due date (section 3, "About the demo deadlines"). |
| A file upload fails on a server | The application-pool identity needs write permission on the `Uploads` folder. |
| The AI button is disabled | There is no key in `Secrets.config`, or the file does not look like `Secrets.config.example`. |
| Login works locally but not on a server | The Release build needs HTTPS. Open the site through `https://`. |
