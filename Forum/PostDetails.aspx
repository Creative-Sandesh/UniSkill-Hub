<%@ Page Title="Discussion" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PostDetails.aspx.cs" Inherits="UniSkillHub.Forum.PostDetails" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="container py-5">
            <div class="empty-state">
                <h1 class="h3">Discussion not found</h1>
                <p>This discussion does not exist or has been removed.</p>
                <a class="btn btn-primary" runat="server" href="~/Forum/Forum">Back to the forum</a>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlPost" runat="server">
        <header class="page-header">
            <div class="container">
                <nav aria-label="Breadcrumb">
                    <ol class="breadcrumb mb-2">
                        <li class="breadcrumb-item"><a runat="server" href="~/Forum/Forum">Forum</a></li>
                        <li class="breadcrumb-item active" aria-current="page"><asp:Literal ID="litCrumb" runat="server"></asp:Literal></li>
                    </ol>
                </nav>
                <h1><asp:Literal ID="litTitle" runat="server"></asp:Literal></h1>
                <p class="forum-meta">
                    <asp:Literal ID="litCategory" runat="server"></asp:Literal>
                    <span>Posted by <strong><asp:Literal ID="litAuthor" runat="server"></asp:Literal></strong></span>
                    <asp:Literal ID="litAuthorBadge" runat="server"></asp:Literal>
                    <span><asp:Literal ID="litDate" runat="server"></asp:Literal></span>
                </p>
            </div>
        </header>

        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-lg-9">

                    <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert"></asp:Label>

                    <%-- Only an Admin ever sees a hidden post --%>
                    <asp:Panel ID="pnlHiddenNote" runat="server" Visible="false" CssClass="alert alert-warning" role="status">
                        <strong>Hidden post.</strong> A moderator hid this discussion, so students cannot see it.
                    </asp:Panel>

                    <%-- The original question --%>
                    <article class="surface-card post-body">
                        <div class="resource-description"><asp:Literal ID="litContent" runat="server"></asp:Literal></div>
                        <asp:Panel ID="pnlPostActions" runat="server" Visible="false" CssClass="post-actions">
                            <asp:Button ID="btnDeletePost" runat="server" Text="Delete this discussion" CssClass="btn btn-sm btn-outline-danger"
                                CausesValidation="false" OnClientClick="return confirm('Delete this discussion and all its replies?');" OnClick="btnDeletePost_Click" />
                        </asp:Panel>
                    </article>

                    <%-- Replies --%>
                    <h2 id="replies" class="h5 mt-5 mb-3"><asp:Literal ID="litReplyCount" runat="server"></asp:Literal></h2>

                    <asp:Repeater ID="rptReplies" runat="server" OnItemCommand="rptReplies_ItemCommand">
                        <ItemTemplate>
                            <article class="reply-card" id='reply-<%# Eval("ReplyID") %>'>
                                <div class="reply-head">
                                    <strong><%#: Eval("FullName") %></strong>
                                    <asp:PlaceHolder runat="server" Visible='<%# Eval("Role").ToString() == "Admin" %>'>
                                        <span class="author-badge">Lecturer</span>
                                    </asp:PlaceHolder>
                                    <span class="dash-meta"><%#: Eval("DatePosted", "{0:dd MMM yyyy, h:mm tt}") %></span>
                                    <asp:PlaceHolder runat="server" Visible='<%# CanDelete(Eval("UserID")) %>'>
                                        <asp:Button runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger ms-auto"
                                            CommandName="DeleteReply" CommandArgument='<%# Eval("ReplyID") %>' CausesValidation="false"
                                            OnClientClick="return confirm('Delete this reply?');" />
                                    </asp:PlaceHolder>
                                </div>
                                <div class="reply-body"><%# ReplyHtml(Eval("ReplyText")) %></div>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlNoReplies" runat="server" Visible="false" CssClass="dash-empty mb-3">No replies yet &mdash; be the first to answer.</asp:Panel>

                    <%-- Write a reply (logged-in users only) --%>
                    <asp:Panel ID="pnlReplyForm" runat="server" Visible="false" CssClass="surface-card mt-4">
                        <h2 class="h5">Write a reply</h2>
                        <asp:ValidationSummary ID="vsReply" runat="server" CssClass="alert alert-danger" ValidationGroup="ReplyGroup"
                            HeaderText="Please fix the following:" DisplayMode="BulletList" />
                        <asp:TextBox ID="txtReply" runat="server" CssClass="form-control mb-2" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled"
                            placeholder="Share your answer or thoughts..." aria-label="Your reply" />
                        <asp:RequiredFieldValidator ID="rfvReply" runat="server" ControlToValidate="txtReply" ValidationGroup="ReplyGroup"
                            ErrorMessage="Please write your reply." Text="Please write your reply." Display="Dynamic" CssClass="field-error" />
                        <asp:RegularExpressionValidator ID="revReply" runat="server" ControlToValidate="txtReply" ValidationGroup="ReplyGroup"
                            ValidationExpression="^[\s\S]{1,2000}$"
                            ErrorMessage="A reply must be 2000 characters or fewer." Text="A reply must be 2000 characters or fewer." Display="Dynamic" CssClass="field-error" />
                        <div class="mt-3">
                            <asp:Button ID="btnReply" runat="server" Text="Post reply" CssClass="btn btn-primary" ValidationGroup="ReplyGroup" OnClick="btnReply_Click" />
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlLoginToReply" runat="server" Visible="false" CssClass="guest-note">
                        <strong>Want to join the discussion?</strong>
                        <a runat="server" href="~/Account/Login">Login</a> or <a runat="server" href="~/Account/Register">create a free account</a> to reply.
                    </asp:Panel>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
