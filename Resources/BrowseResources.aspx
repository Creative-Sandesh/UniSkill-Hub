<%@ Page Title="Resources" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BrowseResources.aspx.cs" Inherits="UniSkillHub.Resources.BrowseResources" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Learning resources</h1>
            <p>Search lecture notes, videos and study materials, and filter them by category or type.</p>
        </div>
    </header>

    <div class="container mt-4">

        <%-- Search + filters --%>
        <section class="filter-bar" aria-label="Search and filter resources">
            <div class="row g-3 align-items-end">
                <div class="col-lg-4">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control"
                        MaxLength="100" placeholder="Search lecture notes..." />
                </div>
                <div class="col-sm-6 col-lg-3">
                    <asp:Label runat="server" AssociatedControlID="ddlCategory" CssClass="form-label" Text="Category" />
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlResourceType" CssClass="form-label" Text="Type" />
                    <asp:DropDownList ID="ddlResourceType" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All types"></asp:ListItem>
                        <asp:ListItem Value="PDF" Text="PDF"></asp:ListItem>
                        <asp:ListItem Value="Video" Text="Video"></asp:ListItem>
                        <asp:ListItem Value="Audio" Text="Audio"></asp:ListItem>
                        <asp:ListItem Value="Article" Text="Article"></asp:ListItem>
                        <asp:ListItem Value="Link" Text="Link"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Resources/BrowseResources" title="Clear filters">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status">
            <asp:Label ID="lblResultCount" runat="server"></asp:Label>
        </p>

        <%-- Results --%>
        <div class="row g-4">
            <asp:Repeater ID="rptResources" runat="server">
                <ItemTemplate>
                    <div class="col-md-6 col-lg-4">
                        <article class="resource-card">
                            <div class="resource-card-top">
                                <span class='type-badge type-<%# Eval("ResourceType").ToString().ToLower() %>'><%#: Eval("ResourceType") %></span>
                                <span class="resource-category"><%#: Eval("CategoryName") %></span>
                            </div>
                            <h2 class="resource-title"><%#: Eval("Title") %></h2>
                            <p class="resource-snippet"><%#: Eval("Snippet") %></p>
                            <div class="resource-card-foot">
                                <span class="resource-date"><%#: Eval("DateCreated", "{0:dd MMM yyyy}") %></span>
                                <span class="d-flex gap-2">
                                    <a class="btn btn-sm btn-outline-primary" href='<%# DetailsUrl(Eval("ResourceID")) %>'>View</a>
                                    <asp:PlaceHolder runat="server" Visible='<%# IsLoggedIn && (int)Eval("HasFile") == 1 %>'>
                                        <a class="btn btn-sm btn-primary" href='<%# DownloadUrl(Eval("ResourceID")) %>'>Download</a>
                                    </asp:PlaceHolder>
                                </span>
                            </div>
                        </article>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty-state">
            <h2>No resources found</h2>
            <p>Try a different search word, or clear the filters.</p>
            <a class="btn btn-primary" runat="server" href="~/Resources/BrowseResources">Show all resources</a>
        </asp:Panel>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>

        <asp:Panel ID="pnlGuestNote" runat="server" Visible="false" CssClass="guest-note">
            <strong>Want to download files?</strong>
            <a runat="server" href="~/Account/Login">Login</a> or <a runat="server" href="~/Account/Register">create a free account</a>.
        </asp:Panel>
    </div>
</asp:Content>
