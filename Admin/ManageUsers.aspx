<%@ Page Title="Manage Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="UniSkillHub.Admin.ManageUsers" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3">
                <div>
                    <h1>Manage users</h1>
                    <p>Add, edit, deactivate or remove student and administrator accounts.</p>
                </div>
                <asp:Button ID="btnAdd" runat="server" Text="Add new user" CssClass="btn btn-primary btn-lg" CausesValidation="false" OnClick="btnAdd_Click" />
            </div>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

        <%-- ADD / EDIT FORM --%>
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="surface-card mb-4">
            <h2 class="h4 mb-4"><asp:Label ID="lblFormTitle" runat="server" Text="Add user"></asp:Label></h2>

            <asp:ValidationSummary ID="vsUser" runat="server" ValidationGroup="UserForm" CssClass="alert alert-danger"
                HeaderText="Please fix the following:" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="txtFullName" CssClass="form-label" Text="Full name" />
                    <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" ValidationGroup="UserForm"
                        ErrorMessage="Full name is required." Text="Full name is required." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username" />
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="30" />
                    <asp:Label ID="lblUsernameHelp" runat="server" CssClass="form-text d-block" Text="3-30 letters, numbers or underscores."></asp:Label>
                    <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" ValidationGroup="UserForm"
                        ErrorMessage="Username is required." Text="Username is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RegularExpressionValidator ID="revUsername" runat="server" ControlToValidate="txtUsername" ValidationGroup="UserForm"
                        ValidationExpression="^[A-Za-z0-9_]{3,30}$"
                        ErrorMessage="Username must be 3-30 letters, numbers or underscores." Text="Username must be 3-30 letters, numbers or underscores." Display="Dynamic" CssClass="field-error" />
                    <asp:CustomValidator ID="cvUsername" runat="server" ControlToValidate="txtUsername" ValidationGroup="UserForm"
                        OnServerValidate="cvUsername_ServerValidate"
                        ErrorMessage="That username is already taken." Text="That username is already taken." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="txtEmail" CssClass="form-label" Text="Email" />
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="UserForm"
                        ErrorMessage="Email is required." Text="Email is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="UserForm"
                        ValidationExpression="^\s*[^@\s]+@[^@\s]+\.[^@\s]+\s*$"
                        ErrorMessage="Enter a valid email address." Text="Enter a valid email address." Display="Dynamic" CssClass="field-error" />
                    <asp:CustomValidator ID="cvEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="UserForm"
                        OnServerValidate="cvEmail_ServerValidate"
                        ErrorMessage="Another account already uses that email." Text="Another account already uses that email." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" />
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                    <asp:Label ID="lblPasswordHelp" runat="server" CssClass="form-text d-block" Text="At least 8 characters."></asp:Label>
                    <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword" ValidationGroup="UserForm"
                        ValidationExpression="^.{8,64}$"
                        ErrorMessage="Password must be at least 8 characters." Text="Password must be at least 8 characters." Display="Dynamic" CssClass="field-error" />
                    <asp:CustomValidator ID="cvPassword" runat="server" ControlToValidate="txtPassword" ValidationGroup="UserForm"
                        ValidateEmptyText="true" OnServerValidate="cvPassword_ServerValidate"
                        ErrorMessage="A password is required for a new user." Text="A password is required for a new user." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="ddlRole" CssClass="form-label" Text="Role" />
                    <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Student" Text="Student"></asp:ListItem>
                        <asp:ListItem Value="Admin" Text="Administrator / Lecturer"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Active" Text="Active"></asp:ListItem>
                        <asp:ListItem Value="Inactive" Text="Inactive (cannot log in)"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>

            <asp:Label ID="lblSelfNote" runat="server" Visible="false" CssClass="alert alert-secondary d-block mt-3 mb-0"
                Text="This is your own account, so its role and status cannot be changed here."></asp:Label>

            <div class="d-flex flex-wrap gap-2 mt-4">
                <asp:Button ID="btnSave" runat="server" Text="Save user" CssClass="btn btn-primary" ValidationGroup="UserForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>

        <%-- SEARCH + FILTERS --%>
        <section class="filter-bar" aria-label="Search and filter users">
            <div class="row g-3 align-items-end">
                <div class="col-lg-5">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Name, username or email..." />
                </div>
                <div class="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlRoleFilter" CssClass="form-label" Text="Role" />
                    <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All roles"></asp:ListItem>
                        <asp:ListItem Value="Student" Text="Student"></asp:ListItem>
                        <asp:ListItem Value="Admin" Text="Admin"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlStatusFilter" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All statuses"></asp:ListItem>
                        <asp:ListItem Value="Active" Text="Active"></asp:ListItem>
                        <asp:ListItem Value="Inactive" Text="Inactive"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" CausesValidation="false" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Admin/ManageUsers">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <%-- USERS GRID --%>
        <div class="table-responsive data-table-wrap">
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="UserID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvUsers_RowCommand"
                EmptyDataText="No users match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:BoundField DataField="UserID" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <strong><%#: Eval("FullName") %></strong>
                            <div class="dash-meta">@<%#: Eval("Username") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:TemplateField HeaderText="Role">
                        <ItemTemplate><span class='<%# RoleClass(Eval("Role")) %>'><%#: Eval("Role") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# StatusClass(Eval("Status")) %>'><%#: Eval("Status") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="DateRegistered" HeaderText="Registered" DataFormatString="{0:dd MMM yyyy}" HtmlEncode="false" />
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <asp:LinkButton runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-primary"
                                CommandName="EditUser" CommandArgument='<%# Eval("UserID") %>' CausesValidation="false" />
                            <asp:PlaceHolder runat="server" Visible='<%# IsAnotherUser(Eval("UserID")) %>'>
                                <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Active" ? "Deactivate" : "Activate" %>' CssClass="btn btn-sm btn-outline-secondary"
                                    CommandName="ToggleStatus" CommandArgument='<%# Eval("UserID") %>' CausesValidation="false" />
                                <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                    CommandName="DeleteUser" CommandArgument='<%# Eval("UserID") %>' CausesValidation="false"
                                    OnClientClick="return confirm('Delete this user permanently? This cannot be undone.');" />
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>
</asp:Content>
