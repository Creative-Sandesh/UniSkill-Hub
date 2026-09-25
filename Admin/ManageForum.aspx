<%@ Page Title="Moderate Forum" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageForum.aspx.cs" Inherits="UniSkillHub.Admin.ManageForum" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Moderate forum</h1>
            <p>Hide or remove posts and replies that break the rules. Hidden items disappear for students but are kept.</p>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

        <%-- Posts / Replies switch --%>
        <ul class="nav nav-pills mod-tabs mb-3" aria-label="Moderation views">
            <li class="nav-item"><a id="lnkPostsTab" runat="server" class="nav-link" href="~/Admin/ManageForum?view=posts">Posts</a></li>
            <li class="nav-item"><a id="lnkRepliesTab" runat="server" class="nav-link" href="~/Admin/ManageForum?view=replies">Replies</a></li>
        </ul>

        <%-- SEARCH + FILTERS --%>
        <section class="filter-bar" aria-label="Search and filter">
            <div class="row g-3 align-items-end">
                <div class="col-lg-5">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Text, title or author..." />
                </div>
                <div class="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlStatusFilter" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All statuses"></asp:ListItem>
                        <asp:ListItem Value="Active" Text="Visible"></asp:ListItem>
                        <asp:ListItem Value="Hidden" Text="Hidden"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <asp:Panel ID="pnlCategoryFilter" runat="server" CssClass="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlCategoryFilter" CssClass="form-label" Text="Category" />
                    <asp:DropDownList ID="ddlCategoryFilter" runat="server" CssClass="form-select"></asp:DropDownList>
                </asp:Panel>
                <div class="col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" CausesValidation="false" OnClick="btnSearch_Click" />
                    <a id="lnkReset" runat="server" class="btn btn-outline-primary" href="~/Admin/ManageForum">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <%-- POSTS --%>
        <asp:Panel ID="pnlPosts" runat="server" CssClass="table-responsive data-table-wrap">
            <asp:GridView ID="gvPosts" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="PostID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvPosts_RowCommand" EmptyDataText="No posts match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:BoundField DataField="PostID" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Post">
                        <ItemTemplate>
                            <strong><%#: Eval("Title") %></strong>
                            <div class="dash-meta"><%#: Eval("Category") %> &middot; by <%#: Eval("FullName") %> (@<%#: Eval("Username") %>)</div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Replies">
                        <ItemTemplate><%# RepliesText(Eval("Replies"), Eval("HiddenReplies")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Posted">
                        <ItemTemplate><%#: Eval("DatePosted", "{0:dd MMM yyyy, h:mm tt}") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# StatusClass(Eval("Status")) %>'><%# StatusText(Eval("Status")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <a class="btn btn-sm btn-outline-primary" href='<%# PostUrl(Eval("PostID")) %>' target="_blank" rel="noopener">View</a>
                            <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Active" ? "Hide" : "Unhide" %>' CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="TogglePost" CommandArgument='<%# Eval("PostID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="DeletePost" CommandArgument='<%# Eval("PostID") %>' CausesValidation="false"
                                OnClientClick="return confirm('Delete this post AND all its replies? This cannot be undone.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <%-- REPLIES --%>
        <asp:Panel ID="pnlReplies" runat="server" Visible="false" CssClass="table-responsive data-table-wrap">
            <asp:GridView ID="gvReplies" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="ReplyID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvReplies_RowCommand" EmptyDataText="No replies match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:BoundField DataField="ReplyID" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Reply">
                        <ItemTemplate>
                            <%#: Eval("Snippet") %>
                            <div class="dash-meta">by <%#: Eval("FullName") %> (@<%#: Eval("Username") %>) &middot; on <a href='<%# PostUrl(Eval("PostID")) %>' target="_blank" rel="noopener"><%#: Eval("PostTitle") %></a></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Posted">
                        <ItemTemplate><%#: Eval("DatePosted", "{0:dd MMM yyyy, h:mm tt}") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# StatusClass(Eval("Status")) %>'><%# StatusText(Eval("Status")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Active" ? "Hide" : "Unhide" %>' CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="ToggleReply" CommandArgument='<%# Eval("ReplyID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="DeleteReply" CommandArgument='<%# Eval("ReplyID") %>' CausesValidation="false"
                                OnClientClick="return confirm('Delete this reply? This cannot be undone.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>
</asp:Content>
