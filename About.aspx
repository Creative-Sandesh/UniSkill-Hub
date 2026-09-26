<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="UniSkillHub.About" %>

<asp:Content ID="HeadCss" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Internal CSS: styles used only by this page --%>
    <style>
        .about-list { list-style: none; padding: 0; margin: 0; }
        .about-list li { display: flex; gap: var(--space-3); padding-block: var(--space-2); }
        .about-list li::before { content: "\2713"; color: var(--color-success); font-weight: 700; }
        .tech-badge { display: inline-block; margin: 0 var(--space-2) var(--space-2) 0; padding: 0.3rem 0.8rem; border-radius: var(--radius-full); background: var(--color-primary-50); border: 1px solid var(--color-primary-100); color: var(--color-primary-700); font-size: var(--font-size-sm); font-weight: 600; }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>About UniSkill Hub</h1>
            <p>A web-based learning portal that centralises the everyday tools university students need.</p>
        </div>
    </header>

    <div class="container mt-5">
        <div class="row g-4">
            <div class="col-lg-7">
                <article class="surface-card h-100">
                    <h2 class="h4">Our purpose</h2>
                    <p>University materials are often scattered across email, chat groups and shared drives. UniSkill Hub replaces that with one organised portal, so students always know where to find notes, what is due and how they are doing.</p>
                    <h2 class="h4 mt-4">What you can do</h2>
                    <ul class="about-list">
                        <li>Browse and download learning resources by category</li>
                        <li>View assignments, submit your work and read your feedback</li>
                        <li>Take online quizzes and see your results instantly</li>
                        <li>Read the latest announcements from your lecturers</li>
                        <li>Discuss topics with classmates in the forum</li>
                    </ul>
                </article>
            </div>

            <div class="col-lg-5">
                <aside class="surface-card h-100">
                    <figure class="side-figure">
                        <img src="<%: ResolveUrl("~/Images/hero-learning.svg") %>" alt="A laptop showing a student dashboard, with a stack of books, a quiz score of 8 out of 10 and a completed-task tick" width="800" height="520" loading="lazy" />
                        <figcaption>Notes, assignments and quizzes together in one place.</figcaption>
                    </figure>
                    <h2 class="h4">Who uses it</h2>
                    <p><strong>Students</strong> register to access resources, assignments, quizzes and the forum.</p>
                    <p><strong>Lecturers / administrators</strong> manage content, grade submissions and moderate discussions.</p>
                    <h2 class="h4 mt-4">Built with</h2>
                    <div>
                        <span class="tech-badge">ASP.NET Web Forms</span>
                        <span class="tech-badge">C#</span>
                        <span class="tech-badge">SQL Server</span>
                        <span class="tech-badge">ADO.NET</span>
                        <span class="tech-badge">Bootstrap 5</span>
                        <span class="tech-badge">HTML5 &amp; CSS3</span>
                    </div>
                </aside>
            </div>
        </div>
    </div>
</asp:Content>
