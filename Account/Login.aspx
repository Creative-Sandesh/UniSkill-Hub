<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="UniSkillHub.Account.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-section" aria-labelledby="login-heading">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-md-8 col-lg-5">
                    <div class="auth-card">
                        <h1 id="login-heading">Welcome back</h1>
                        <p class="auth-subtitle">Login to continue to your UniSkill Hub account.</p>

                        <%-- Shown when a logged-in user opens a page their role may not view --%>
                        <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false">
                            <div class="alert alert-warning" role="alert">
                                <strong>Access denied.</strong> Your account does not have permission to view that page.
                            </div>
                            <div class="d-flex flex-wrap gap-2">
                                <a class="btn btn-primary" id="lnkMyDashboard" runat="server" href="~/">Go to my dashboard</a>
                                <a class="btn btn-outline-primary" runat="server" href="~/Account/Logout">Login as a different user</a>
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlLoginForm" runat="server">
                            <asp:Label ID="lblNotice" runat="server" Visible="false" CssClass="alert alert-success d-block" role="status"></asp:Label>
                            <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert"></asp:Label>

                            <asp:ValidationSummary ID="vsLogin" runat="server" CssClass="alert alert-danger"
                                HeaderText="Please fix the following:" DisplayMode="BulletList" />

                            <div class="mb-3">
                                <asp:Label runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username or email" />
                                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="100" autocomplete="username" />
                                <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername"
                                    ErrorMessage="Enter your username or email." Text="Enter your username or email." Display="Dynamic" CssClass="field-error" />
                            </div>

                            <div class="mb-3">
                                <asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" />
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="current-password" />
                                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                    ErrorMessage="Enter your password." Text="Enter your password." Display="Dynamic" CssClass="field-error" />
                            </div>

                            <div class="form-check mb-3">
                                <input type="checkbox" id="chkRemember" runat="server" class="form-check-input" />
                                <label for="<%= chkRemember.ClientID %>" class="form-check-label">Keep me logged in on this computer</label>
                            </div>

                            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary btn-lg w-100" OnClick="btnLogin_Click" />

                            <p class="auth-footer">New to UniSkill Hub? <a runat="server" href="~/Account/Register">Create an account</a></p>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
