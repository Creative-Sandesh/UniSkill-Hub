<%@ Page Title="Manage Announcements" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageAnnouncements.aspx.cs" Inherits="UniSkillHub.Admin.ManageAnnouncements" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3">
                <div>
                    <h1>Manage announcements</h1>
                    <p>Post news for students, schedule it ahead of time and set when it should disappear.</p>
                </div>
                <asp:Button ID="btnAdd" runat="server" Text="Add new announcement" CssClass="btn btn-primary btn-lg" CausesValidation="false" OnClick="btnAdd_Click" />
            </div>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

        <%-- ADD / EDIT FORM --%>
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="surface-card mb-4">
            <h2 class="h4 mb-4"><asp:Label ID="lblFormTitle" runat="server" Text="Add announcement"></asp:Label></h2>

            <asp:ValidationSummary ID="vsAnnouncement" runat="server" ValidationGroup="AnnouncementForm" CssClass="alert alert-danger"
                HeaderText="Please fix the following:" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtTitle" CssClass="form-label" Text="Title" />
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="150" ValidateRequestMode="Disabled" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ValidationGroup="AnnouncementForm"
                        ErrorMessage="Title is required." Text="Title is required." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtContent" CssClass="form-label" Text="Content" />
                    <asp:TextBox ID="txtContent" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6" ValidateRequestMode="Disabled" />
                    <div class="form-text">Up to 5000 characters. Blank lines start a new paragraph.</div>
                    <asp:RequiredFieldValidator ID="rfvContent" runat="server" ControlToValidate="txtContent" ValidationGroup="AnnouncementForm"
                        ErrorMessage="Content is required." Text="Content is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RegularExpressionValidator ID="revContent" runat="server" ControlToValidate="txtContent" ValidationGroup="AnnouncementForm"
                        ValidationExpression="^[\s\S]{1,5000}$"
                        ErrorMessage="Content must be 5000 characters or fewer." Text="Content must be 5000 characters or fewer." Display="Dynamic" CssClass="field-error" />
                </div>

                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="txtPublishDate" CssClass="form-label" Text="Publish on" />
                    <asp:TextBox ID="txtPublishDate" runat="server" CssClass="form-control" TextMode="DateTimeLocal" />
                    <div class="form-text">A future date schedules it.</div>
                    <asp:RequiredFieldValidator ID="rfvPublish" runat="server" ControlToValidate="txtPublishDate" ValidationGroup="AnnouncementForm"
                        ErrorMessage="Please choose the publish date and time." Text="Please choose the publish date and time." Display="Dynamic" CssClass="field-error" />
                    <asp:CustomValidator ID="cvPublish" runat="server" ControlToValidate="txtPublishDate" ValidationGroup="AnnouncementForm"
                        OnServerValidate="cvPublish_ServerValidate" ErrorMessage="Invalid publish date." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="txtExpiryDate" CssClass="form-label" Text="Expires on (optional)" />
                    <asp:TextBox ID="txtExpiryDate" runat="server" CssClass="form-control" TextMode="DateTimeLocal" />
                    <div class="form-text">Leave empty to keep it visible.</div>
                    <asp:CustomValidator ID="cvExpiry" runat="server" ControlToValidate="txtExpiryDate" ValidationGroup="AnnouncementForm"
                        OnServerValidate="cvExpiry_ServerValidate" ErrorMessage="Invalid expiry date." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Published" Text="Published"></asp:ListItem>
                        <asp:ListItem Value="Draft" Text="Draft (hidden)"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <div class="d-flex flex-wrap gap-2 mt-4">
                <asp:Button ID="btnSave" runat="server" Text="Save announcement" CssClass="btn btn-primary" ValidationGroup="AnnouncementForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>

        <%-- SEARCH + FILTERS --%>
        <section class="filter-bar" aria-label="Search and filter announcements">
            <div class="row g-3 align-items-end">
                <div class="col-lg-6">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Title or content..." />
                </div>
                <div class="col-sm-6 col-lg-3">
                    <asp:Label runat="server" AssociatedControlID="ddlStateFilter" CssClass="form-label" Text="State" />
                    <asp:DropDownList ID="ddlStateFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All"></asp:ListItem>
                        <asp:ListItem Value="Live" Text="Live now"></asp:ListItem>
                        <asp:ListItem Value="Scheduled" Text="Scheduled"></asp:ListItem>
                        <asp:ListItem Value="Expired" Text="Expired"></asp:ListItem>
                        <asp:ListItem Value="Draft" Text="Draft"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" CausesValidation="false" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Admin/ManageAnnouncements">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <%-- ANNOUNCEMENTS GRID --%>
        <div class="table-responsive data-table-wrap">
            <asp:GridView ID="gvAnnouncements" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="AnnouncementID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvAnnouncements_RowCommand" EmptyDataText="No announcements match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:BoundField DataField="AnnouncementID" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Announcement">
                        <ItemTemplate>
                            <strong><%#: Eval("Title") %></strong>
                            <div class="dash-meta">by <%#: Eval("FullName") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Publish on">
                        <ItemTemplate><%#: Eval("PublishDate", "{0:dd MMM yyyy, h:mm tt}") %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Expires">
                        <ItemTemplate><%# ExpiryText(Eval("ExpiryDate")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="State">
                        <ItemTemplate><span class='<%# StateClass(Eval("State")) %>'><%#: Eval("State") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <asp:LinkButton runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-primary"
                                CommandName="EditAnnouncement" CommandArgument='<%# Eval("AnnouncementID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Published" ? "Unpublish" : "Publish" %>' CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="TogglePublish" CommandArgument='<%# Eval("AnnouncementID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="DeleteAnnouncement" CommandArgument='<%# Eval("AnnouncementID") %>' CausesValidation="false"
                                OnClientClick="return confirm('Delete this announcement? This cannot be undone.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>
</asp:Content>
