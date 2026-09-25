<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="UniSkillHub.Student.Dashboard" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Welcome, <asp:Label ID="lblFullName" runat="server"></asp:Label></h1>
            <p>Here is a summary of your learning.</p>
        </div>
    </header>

    <div class="container mt-4">

        <%-- My learning: four quick numbers --%>
        <section aria-label="My learning summary">
            <div class="row g-3">
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblResources" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Available resources</span>
                    </div>
                </div>
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblPending" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Pending assignments</span>
                    </div>
                </div>
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblSubmitted" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Submitted assignments</span>
                    </div>
                </div>
                <div class="col-6 col-lg-3">
                    <div class="stat-card">
                        <span class="stat-value"><asp:Label ID="lblAttempts" runat="server" Text="0"></asp:Label></span>
                        <span class="stat-label">Quiz attempts</span>
                    </div>
                </div>
            </div>
        </section>

        <div class="row g-4 mt-1">
            <div class="col-lg-7">

                <%-- Recent announcements --%>
                <section class="dash-panel" aria-labelledby="ann-heading">
                    <div class="dash-panel-head">
                        <h2 id="ann-heading">Recent announcements</h2>
                        <a runat="server" href="~/Announcements">View all</a>
                    </div>
                    <asp:Repeater ID="rptAnnouncements" runat="server">
                        <HeaderTemplate><ul class="dash-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <strong><%#: Eval("Title") %></strong>
                                <span class="dash-meta"><%#: Eval("PublishDate", "{0:dd MMM yyyy}") %></span>
                                <span class="dash-text"><%#: Eval("Preview") %></span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoAnnouncements" runat="server" Visible="false" CssClass="dash-empty">No announcements right now.</asp:Panel>
                </section>

                <%-- Upcoming assignments --%>
                <section class="dash-panel" aria-labelledby="asg-heading">
                    <div class="dash-panel-head">
                        <h2 id="asg-heading">Upcoming assignments</h2>
                        <a runat="server" href="~/Student/Assignments">View all</a>
                    </div>
                    <asp:Repeater ID="rptAssignments" runat="server">
                        <HeaderTemplate><ul class="dash-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <strong><%#: Eval("Title") %></strong>
                                <span class="dash-meta">Due <%#: Eval("DueDate", "{0:dd MMM yyyy, h:mm tt}") %></span>
                                <span class="dash-text"><%#: Eval("MaxMarks") %> marks</span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoAssignments" runat="server" Visible="false" CssClass="dash-empty">You have no pending assignments. Nice work!</asp:Panel>
                </section>
            </div>

            <div class="col-lg-5">

                <%-- Recent forum discussions --%>
                <section class="dash-panel" aria-labelledby="forum-heading">
                    <div class="dash-panel-head">
                        <h2 id="forum-heading">Recent discussions</h2>
                        <a runat="server" href="~/Forum/Forum">View all</a>
                    </div>
                    <asp:Repeater ID="rptForumPosts" runat="server">
                        <HeaderTemplate><ul class="dash-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li>
                                <strong><%#: Eval("Title") %></strong>
                                <span class="dash-meta">by <%#: Eval("FullName") %> &middot; <%#: Eval("Replies") %> replies</span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoPosts" runat="server" Visible="false" CssClass="dash-empty">No discussions yet.</asp:Panel>
                </section>

                <%-- Shortcuts --%>
                <section class="dash-panel" aria-labelledby="quick-heading">
                    <div class="dash-panel-head"><h2 id="quick-heading">Quick links</h2></div>
                    <div class="d-grid gap-2">
                        <a class="btn btn-primary" runat="server" href="~/Resources/BrowseResources">Browse learning resources</a>
                        <a class="btn btn-outline-primary" runat="server" href="~/Student/MySubmissions">My submissions &amp; marks</a>
                        <a class="btn btn-outline-primary" runat="server" href="~/Student/Profile">Update my profile</a>
                    </div>
                </section>
            </div>
        </div>
    </div>
</asp:Content>
