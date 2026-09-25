<%@ Page Title="New Discussion" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreatePost.aspx.cs" Inherits="UniSkillHub.Forum.CreatePost" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <nav aria-label="Breadcrumb">
                <ol class="breadcrumb mb-2">
                    <li class="breadcrumb-item"><a runat="server" href="~/Forum/Forum">Forum</a></li>
                    <li class="breadcrumb-item active" aria-current="page">New discussion</li>
                </ol>
            </nav>
            <h1>Start a discussion</h1>
            <p>Ask a question or share something useful with your classmates.</p>
        </div>
    </header>

    <div class="container mt-5">
        <div class="row justify-content-center">
            <div class="col-lg-8">
                <div class="surface-card">
                    <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert"></asp:Label>
                    <asp:ValidationSummary ID="vsPost" runat="server" CssClass="alert alert-danger"
                        HeaderText="Please fix the following:" DisplayMode="BulletList" />

                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtTitle" CssClass="form-label" Text="Title" />
                        <%-- Request validation is switched off for the discussion text boxes so students can
                             write code such as <nav> in a question. Everything is HTML-encoded when displayed. --%>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="150" ValidateRequestMode="Disabled" placeholder="e.g. How do I understand recursion?" />
                        <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle"
                            ErrorMessage="Please enter a title." Text="Please enter a title." Display="Dynamic" CssClass="field-error" />
                        <asp:RegularExpressionValidator ID="revTitle" runat="server" ControlToValidate="txtTitle"
                            ValidationExpression="^[\s\S]{1,150}$"
                            ErrorMessage="The title must be 150 characters or fewer." Text="The title must be 150 characters or fewer." Display="Dynamic" CssClass="field-error" />
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="ddlCategory" CssClass="form-label" Text="Category" />
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="mb-4">
                        <asp:Label runat="server" AssociatedControlID="txtContent" CssClass="form-label" Text="Your question or message" />
                        <asp:TextBox ID="txtContent" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8" ValidateRequestMode="Disabled"
                            placeholder="Describe your question with as much detail as you can..." />
                        <div class="form-text">Up to 5000 characters.</div>
                        <asp:RequiredFieldValidator ID="rfvContent" runat="server" ControlToValidate="txtContent"
                            ErrorMessage="Please write your message." Text="Please write your message." Display="Dynamic" CssClass="field-error" />
                        <asp:RegularExpressionValidator ID="revContent" runat="server" ControlToValidate="txtContent"
                            ValidationExpression="^[\s\S]{1,5000}$"
                            ErrorMessage="The message must be 5000 characters or fewer." Text="The message must be 5000 characters or fewer." Display="Dynamic" CssClass="field-error" />
                    </div>

                    <div class="d-flex flex-wrap gap-2">
                        <asp:Button ID="btnPost" runat="server" Text="Post discussion" CssClass="btn btn-primary" OnClick="btnPost_Click" />
                        <a class="btn btn-outline-primary" runat="server" href="~/Forum/Forum">Cancel</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
