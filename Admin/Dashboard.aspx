<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="UniSkillHub.Admin.Dashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Admin dashboard</h1>
            <p>Welcome, <asp:Label ID="lblFullName" runat="server"></asp:Label>. Here is an overview of UniSkill Hub.</p>
        </div>
    </header>

    <div class="container mt-4">

        <%-- System statistics --%>
        <section aria-label="System statistics">
            <div class="row g-3">
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblStudents" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Students</span>
                        <span class="stat-sub"><asp:Label ID="lblStudentsSub" runat="server"></asp:Label></span>
                    </div>
                </div>
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblResources" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Resources</span>
                        <span class="stat-sub"><asp:Label ID="lblResourcesSub" runat="server"></asp:Label></span>
                    </div>
                </div>
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblAssignments" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Assignments</span>
                        <span class="stat-sub"><asp:Label ID="lblAssignmentsSub" runat="server"></asp:Label></span>
                    </div>
                </div>
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblSubmissions" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Submissions</span>
                        <span class="stat-sub"><asp:Label ID="lblSubmissionsSub" runat="server"></asp:Label></span>
                    </div>
                </div>
                <div class="col-6 col-lg-4">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblQuizzes" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Quizzes</span>
                        <span class="stat-sub"><asp:Label ID="lblQuizzesSub" runat="server"></asp:Label></span>
                    </div>
                </div>
                <div class="col-6 col-lg-4">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblAnnouncements" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Announcements</span>
                        <span class="stat-sub"><asp:Label ID="lblAnnouncementsSub" runat="server"></asp:Label></span>
                    </div>
                </div>
                <div class="col-12 col-lg-4">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblForumPosts" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Forum posts</span>
                        <span class="stat-sub"><asp:Label ID="lblForumPostsSub" runat="server"></asp:Label></span>
                    </div>
                </div>
            </div>
        </section>

        <div class="row g-4 mt-1">
            <div class="col-lg-6">

                <%-- Recent registrations --%>
                <section class="dash-panel" aria-labelledby="reg-heading">
                    <div class="dash-panel-head">
                        <h2 id="reg-heading">Recent registrations</h2>
                        <a runat="server" href="~/Admin/ManageUsers">Manage users</a>
                    </div>
                    <asp:Repeater ID="rptRegistrations" runat="server">
                        <HeaderTemplate><ul class="dash-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <strong><%#: Eval("FullName") %></strong>
                                <span class="dash-meta">@<%#: Eval("Username") %> &middot; <%#: Eval("Role") %></span>
                                <span class="dash-text"><%#: Eval("DateRegistered", "{0:dd MMM yyyy, h:mm tt}") %></span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoRegistrations" runat="server" Visible="false" CssClass="dash-empty">No users yet.</asp:Panel>
                </section>

                <%-- Recent submissions --%>
                <section class="dash-panel" aria-labelledby="sub-heading">
                    <div class="dash-panel-head">
                        <h2 id="sub-heading">Recent submissions</h2>
                        <a runat="server" href="~/Admin/ManageSubmissions">View all</a>
                    </div>
                    <asp:Repeater ID="rptSubmissions" runat="server">
                        <HeaderTemplate><ul class="dash-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <strong><%#: Eval("Title") %></strong>
                                <span class='<%# StatusClass(Eval("Status")) %>'><%#: Eval("Status") %></span>
                                <span class="dash-text">by <%#: Eval("FullName") %> &middot; <%#: Eval("SubmittedDate", "{0:dd MMM yyyy, h:mm tt}") %></span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoSubmissions" runat="server" Visible="false" CssClass="dash-empty">No submissions yet.</asp:Panel>
                </section>
            </div>

            <div class="col-lg-6">

                <%-- Recent forum activity --%>
                <section class="dash-panel" aria-labelledby="forum-heading">
                    <div class="dash-panel-head">
                        <h2 id="forum-heading">Recent forum activity</h2>
                        <a runat="server" href="~/Admin/ManageForum">Moderate</a>
                    </div>
                    <asp:Repeater ID="rptForum" runat="server">
                        <HeaderTemplate><ul class="dash-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <strong><%#: Eval("FullName") %></strong>
                                <span class="dash-meta"><%#: Eval("Action") %></span>
                                <a href='<%# PostUrl(Eval("PostID")) %>'><%#: Eval("Title") %></a>
                                <span class="dash-text"><%#: Eval("ActivityDate", "{0:dd MMM yyyy, h:mm tt}") %></span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoForum" runat="server" Visible="false" CssClass="dash-empty">No forum activity yet.</asp:Panel>
                </section>
            </div>
        </div>
    </div>
</asp:Content>
