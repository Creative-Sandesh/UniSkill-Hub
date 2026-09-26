<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="UniSkillHub._Default" %>

<asp:Content ID="HeadCss" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Internal CSS: styles used only by this page (the hero preview card) --%>
    <style>
        .hero-preview {
            background: var(--color-surface);
            border: 1px solid var(--color-border);
            border-radius: var(--radius-xl);
            box-shadow: var(--shadow-lg);
            padding: var(--space-6);
            animation: hero-float 7s ease-in-out infinite;
        }
        .hero-preview-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--space-4); }
        .hero-preview-head strong { font-size: var(--font-size-lg); }
        .preview-item { padding: var(--space-3) 0; border-top: 1px solid var(--color-border); }
        .preview-item:first-of-type { border-top: 0; }
        .preview-row { display: flex; justify-content: space-between; font-size: var(--font-size-sm); font-weight: 600; margin-bottom: var(--space-2); }
        .preview-row span:last-child { color: var(--color-text-subtle); font-weight: 500; }
        .preview-bar { height: 0.5rem; border-radius: var(--radius-full); background: var(--color-primary-100); overflow: hidden; }
        .preview-bar > span { display: block; height: 100%; border-radius: inherit; background: linear-gradient(90deg, var(--color-primary-600), var(--color-accent-500)); }
        .preview-score { display: flex; align-items: center; gap: var(--space-3); margin-top: var(--space-4); padding: var(--space-3) var(--space-4); border-radius: var(--radius-md); background: var(--color-primary-50); }
        .preview-score b { font-size: var(--font-size-2xl); color: var(--color-primary-700); }
        @keyframes hero-float {
            0%, 100% { transform: translateY(0); }
            50%      { transform: translateY(-8px); }
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- HERO --%>
    <section class="hero" aria-labelledby="hero-heading">
        <div class="container">
            <div class="row align-items-center g-5">
                <div class="col-lg-6">
                    <span class="eyebrow"><span class="eyebrow-dot" aria-hidden="true"></span>Built for university students</span>
                    <h1 id="hero-heading" class="hero-title">Learn, submit and grow &mdash; all in <span class="text-gradient">one place</span>.</h1>
                    <p class="hero-lead">UniSkill Hub brings your lecture notes, assignments, quizzes, announcements and academic discussions into a single, easy-to-use portal.</p>

                    <div class="d-flex flex-wrap gap-3 mt-4">
                        <% if (!Request.IsAuthenticated) { %>
                            <a class="btn btn-primary btn-lg" href="<%: ResolveUrl("~/Account/Register") %>">Get started &mdash; Register</a>
                            <a class="btn btn-outline-primary btn-lg" href="<%: ResolveUrl("~/Account/Login") %>">Login</a>
                        <% } else { %>
                            <a class="btn btn-primary btn-lg" href="<%: DashboardUrl %>">Go to my dashboard</a>
                        <% } %>
                    </div>
                    <p class="hero-note mt-3 mb-0">Free for registered students of the university.</p>
                </div>

                <div class="col-lg-6">
                    <%-- Decorative preview of the student dashboard --%>
                    <div class="hero-preview" aria-hidden="true">
                        <div class="hero-preview-head">
                            <strong>My learning</strong>
                            <span class="badge text-bg-light border">This week</span>
                        </div>

                        <div class="preview-item">
                            <div class="preview-row"><span>Database Fundamentals</span><span>72%</span></div>
                            <div class="preview-bar"><span style="width: 72%;"></span></div>
                        </div>
                        <div class="preview-item">
                            <div class="preview-row"><span>Web Application Assignment</span><span>Due in 3 days</span></div>
                            <div class="preview-bar"><span style="width: 45%;"></span></div>
                        </div>
                        <div class="preview-item">
                            <div class="preview-row"><span>Python Basics Quiz</span><span>Completed</span></div>
                            <div class="preview-bar"><span style="width: 100%;"></span></div>
                        </div>

                        <div class="preview-score">
                            <b>8 / 10</b>
                            <span>Latest quiz score &middot; 80%</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <%-- FEATURES --%>
    <section class="section" aria-labelledby="features-heading">
        <div class="container">
            <header class="text-center mb-5 reveal">
                <h2 id="features-heading" class="section-title">Everything your studies need</h2>
                <p class="section-lead mt-2">One portal for the whole learning cycle &mdash; from reading the notes to getting your marks back.</p>
            </header>

            <div class="row g-4">
                <div class="col-md-6 col-lg-4 reveal">
                    <article class="feature-card">
                        <span class="feature-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20" /><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z" /></svg></span>
                        <h3>Learning resources</h3>
                        <p>Browse, search and download lecture notes and study materials, organised by category.</p>
                    </article>
                </div>
                <div class="col-md-6 col-lg-4 reveal">
                    <article class="feature-card">
                        <span class="feature-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" /><rect x="8" y="2" width="8" height="4" rx="1" /><path d="m9 14 2 2 4-4" /></svg></span>
                        <h3>Assignments &amp; grades</h3>
                        <p>See deadlines, upload your work and track your submissions, marks and lecturer feedback.</p>
                    </article>
                </div>
                <div class="col-md-6 col-lg-4 reveal">
                    <article class="feature-card">
                        <span class="feature-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" /><path d="M9.1 9a3 3 0 0 1 5.8 1c0 2-3 3-3 3" /><path d="M12 17h.01" /></svg></span>
                        <h3>Interactive quizzes</h3>
                        <p>Test your understanding with online quizzes and get your score the moment you finish.</p>
                    </article>
                </div>
                <div class="col-md-6 col-lg-4 reveal">
                    <article class="feature-card">
                        <span class="feature-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9" /><path d="M13.7 21a2 2 0 0 1-3.4 0" /></svg></span>
                        <h3>Announcements</h3>
                        <p>Never miss an exam schedule or class update &mdash; the latest news is always on the front page.</p>
                    </article>
                </div>
                <div class="col-md-6 col-lg-4 reveal">
                    <article class="feature-card">
                        <span class="feature-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" /></svg></span>
                        <h3>Discussion forum</h3>
                        <p>Ask questions, share answers and learn together with classmates in a moderated space.</p>
                    </article>
                </div>
                <div class="col-md-6 col-lg-4 reveal">
                    <article class="feature-card">
                        <span class="feature-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" /><polygon points="10 8 16 12 10 16 10 8" /></svg></span>
                        <h3>Video &amp; audio lessons</h3>
                        <p>Watch and listen to lessons right in your browser, alongside the written notes.</p>
                    </article>
                </div>
            </div>
        </div>
    </section>

    <%-- CATEGORIES (from the database: each tile shows how many published resources it holds) --%>
    <asp:Panel ID="pnlCategories" runat="server" CssClass="section">
        <div class="container" role="region" aria-labelledby="categories-heading">
            <header class="text-center mb-5 reveal">
                <h2 id="categories-heading" class="section-title">Explore by category</h2>
                <p class="section-lead mt-2">Notes, videos and audio lessons, organised by subject.</p>
            </header>

            <div class="row g-4 justify-content-center">
                <asp:Repeater ID="rptCategories" runat="server">
                    <ItemTemplate>
                        <div class="col-sm-6 col-lg-4 reveal">
                            <a class="category-tile" href='<%# CategoryLink(Eval("CategoryID")) %>'>
                                <figure class="category-figure">
                                    <img src='<%# CategoryImage(Eval("CategoryName")) %>' alt='<%#: Eval("CategoryName") %> illustration' width="640" height="360" loading="lazy" />
                                    <figcaption>
                                        <strong><%#: Eval("CategoryName") %></strong>
                                        <span><%#: Eval("Description") %></span>
                                        <span class="category-count"><%# CountText(Eval("ResourceCount")) %></span>
                                    </figcaption>
                                </figure>
                            </a>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </asp:Panel>

    <%-- HOW IT WORKS --%>
    <section class="section" aria-labelledby="steps-heading">
        <div class="container">
            <header class="text-center mb-4 reveal">
                <h2 id="steps-heading" class="section-title">How it works</h2>
                <p class="section-lead mt-2">Three simple steps to get started.</p>
            </header>

            <div class="row g-4">
                <div class="col-md-4 reveal">
                    <div class="step">
                        <span class="step-number">1</span>
                        <h3>Create your account</h3>
                        <p>Register with your name, username and email &mdash; it only takes a minute.</p>
                    </div>
                </div>
                <div class="col-md-4 reveal">
                    <div class="step">
                        <span class="step-number">2</span>
                        <h3>Learn and practise</h3>
                        <p>Read the notes, watch the lessons and take quizzes at your own pace.</p>
                    </div>
                </div>
                <div class="col-md-4 reveal">
                    <div class="step">
                        <span class="step-number">3</span>
                        <h3>Submit and track</h3>
                        <p>Hand in assignments online and follow your marks and feedback.</p>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <%-- CALL TO ACTION --%>
    <section class="section" aria-labelledby="cta-heading">
        <div class="container">
            <div class="cta-band reveal">
                <h2 id="cta-heading">Ready to start learning?</h2>
                <p>Join UniSkill Hub today and keep everything for your course in one organised place.</p>
                <% if (!Request.IsAuthenticated) { %>
                    <a class="btn btn-light btn-lg" href="<%: ResolveUrl("~/Account/Register") %>">Create a free account</a>
                <% } else { %>
                    <a class="btn btn-light btn-lg" href="<%: DashboardUrl %>">Open my dashboard</a>
                <% } %>
            </div>
        </div>
    </section>

</asp:Content>
