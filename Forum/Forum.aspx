<%@ Page Title="Discussion Forum" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Forum.aspx.cs" Inherits="UniSkillHub.Forum.ForumHome" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3">
                <div>
                    <h1>Discussion forum</h1>
                    <p>Ask questions, share answers and learn together.</p>
                </div>
                <a id="lnkNewPost" runat="server" class="btn btn-primary btn-lg" href="~/Forum/CreatePost">Create new discussion</a>
            </div>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Panel ID="pnlDeleted" runat="server" Visible="false" CssClass="alert alert-success" role="status">
            The discussion has been deleted.
        </asp:Panel>

        <%-- Search + category filter --%>
        <section class="filter-bar" aria-label="Search the forum">
            <div class="row g-3 align-items-end">
                <div class="col-lg-6">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search discussions" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Search titles and posts..." />
                </div>
                <div class="col-sm-6 col-lg-3">
                    <asp:Label runat="server" AssociatedControlID="ddlCategory" CssClass="form-label" Text="Category" />
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Forum/Forum">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <asp:Repeater ID="rptPosts" runat="server">
            <ItemTemplate>
                <article class="forum-item">
                    <div class="forum-item-main">
                        <h2><a href='<%# PostUrl(Eval("PostID")) %>'><%#: Eval("Title") %></a></h2>
                        <p class="forum-snippet"><%#: Eval("Snippet") %></p>
                        <p class="forum-meta">
                            <span class="type-badge type-link"><%#: Eval("Category") %></span>
                            <span>Posted by <strong><%#: Eval("FullName") %></strong></span>
                            <asp:PlaceHolder runat="server" Visible='<%# Eval("Role").ToString() == "Admin" %>'>
                                <span class="author-badge">Lecturer</span>
                            </asp:PlaceHolder>
                            <span><%#: Eval("DatePosted", "{0:dd MMM yyyy}") %></span>
                        </p>
                    </div>
                    <div class="forum-item-stat">
                        <strong><%#: Eval("Replies") %></strong>
                        <span><%# (int)Eval("Replies") == 1 ? "reply" : "replies" %></span>
                    </div>
                </article>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty-state">
            <h2>No discussions found</h2>
            <p>Try a different search, or start the first discussion yourself.</p>
        </asp:Panel>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>
</asp:Content>
