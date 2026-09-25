<%@ Page Title="Resource" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ResourceDetails.aspx.cs" Inherits="UniSkillHub.Resources.ResourceDetails" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- Shown when the ID is missing, invalid or the resource is not published --%>
    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="container py-5">
            <div class="empty-state">
                <h1 class="h3">Resource not found</h1>
                <p>This resource does not exist or is no longer available.</p>
                <a class="btn btn-primary" runat="server" href="~/Resources/BrowseResources">Back to resources</a>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlResource" runat="server">
        <header class="page-header">
            <div class="container">
                <nav aria-label="Breadcrumb">
                    <ol class="breadcrumb mb-2">
                        <li class="breadcrumb-item"><a runat="server" href="~/Resources/BrowseResources">Resources</a></li>
                        <li class="breadcrumb-item active" aria-current="page"><asp:Literal ID="litCrumb" runat="server"></asp:Literal></li>
                    </ol>
                </nav>
                <h1><asp:Literal ID="litTitle" runat="server"></asp:Literal></h1>
                <p class="resource-meta">
                    <asp:Literal ID="litTypeBadge" runat="server"></asp:Literal>
                    <span><asp:Literal ID="litCategory" runat="server"></asp:Literal></span>
                    <span>Added <asp:Literal ID="litDate" runat="server"></asp:Literal> by <asp:Literal ID="litAuthor" runat="server"></asp:Literal></span>
                </p>
            </div>
        </header>

        <div class="container mt-5">
            <%-- Only an Admin ever sees a draft --%>
            <asp:Panel ID="pnlDraftNote" runat="server" Visible="false" CssClass="alert alert-warning" role="status">
                <strong>Draft preview.</strong> This resource is not published, so students cannot see it.
            </asp:Panel>
            <div class="row g-4">
                <div class="col-lg-8">
                    <article class="surface-card">
                        <h2 class="h5">About this resource</h2>
                        <div class="resource-description"><asp:Literal ID="litDescription" runat="server"></asp:Literal></div>

                        <%-- Video / audio player (HTML5) --%>
                        <asp:Literal ID="litMedia" runat="server"></asp:Literal>
                    </article>
                </div>

                <div class="col-lg-4">
                    <aside class="surface-card">
                        <h2 class="h5">Get this resource</h2>

                        <%-- File download --%>
                        <asp:Panel ID="pnlDownload" runat="server" Visible="false">
                            <p class="mb-3">File: <strong><asp:Literal ID="litFileName" runat="server"></asp:Literal></strong></p>
                            <a id="lnkDownload" runat="server" class="btn btn-primary w-100" href="~/Resources/Download.ashx">Download file</a>
                        </asp:Panel>

                        <%-- External link --%>
                        <asp:Panel ID="pnlLink" runat="server" Visible="false">
                            <a id="lnkExternal" runat="server" class="btn btn-primary w-100" target="_blank" rel="noopener noreferrer" href="#">Open link</a>
                            <p class="form-text mt-2 mb-0">Opens an external website in a new tab.</p>
                        </asp:Panel>

                        <%-- Guests must log in before downloading --%>
                        <asp:Panel ID="pnlLoginToDownload" runat="server" Visible="false">
                            <p class="mb-3">Please login to download this file.</p>
                            <a class="btn btn-primary w-100 mb-2" runat="server" href="~/Account/Login">Login</a>
                            <a class="btn btn-outline-primary w-100" runat="server" href="~/Account/Register">Create an account</a>
                        </asp:Panel>

                        <asp:Panel ID="pnlNothing" runat="server" Visible="false">
                            <p class="mb-0 text-muted">Nothing to download &mdash; everything you need is on this page.</p>
                        </asp:Panel>

                        <hr />
                        <a class="btn btn-outline-primary w-100" runat="server" href="~/Resources/BrowseResources">&larr; Back to all resources</a>
                    </aside>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
