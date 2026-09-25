<%@ Page Title="Announcements" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Announcements.aspx.cs" Inherits="UniSkillHub.Announcements" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Announcements</h1>
            <p>The latest news, schedules and notices from your lecturers.</p>
        </div>
    </header>

    <div class="container mt-4">
        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <div class="row justify-content-center">
            <div class="col-lg-9">
                <asp:Repeater ID="rptAnnouncements" runat="server">
                    <ItemTemplate>
                        <article class="announcement-card">
                            <div class="announcement-head">
                                <h2><%#: Eval("Title") %></h2>
                                <asp:PlaceHolder runat="server" Visible='<%# IsNew(Eval("PublishDate")) %>'>
                                    <span class="status-badge status-submitted">New</span>
                                </asp:PlaceHolder>
                            </div>
                            <p class="announcement-meta">
                                <time datetime='<%# ((DateTime)Eval("PublishDate")).ToString("yyyy-MM-dd") %>'><%#: Eval("PublishDate", "{0:dd MMMM yyyy}") %></time>
                                &middot; Posted by <%#: Eval("FullName") %>
                            </p>
                            <div class="announcement-body"><%# ContentHtml(Eval("Content")) %></div>
                            <asp:PlaceHolder runat="server" Visible='<%# !(Eval("ExpiryDate") is DBNull) %>'>
                                <p class="announcement-expiry">Valid until <%#: ExpiryText(Eval("ExpiryDate")) %></p>
                            </asp:PlaceHolder>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty-state">
                    <h2>No announcements right now</h2>
                    <p>There is nothing new to show. Please check back later.</p>
                </asp:Panel>

                <asp:Literal ID="litPager" runat="server"></asp:Literal>
            </div>
        </div>
    </div>
</asp:Content>
