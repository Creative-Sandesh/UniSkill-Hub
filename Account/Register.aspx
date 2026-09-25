<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="UniSkillHub.Account.Register" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="auth-section" aria-labelledby="register-heading">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-md-9 col-lg-6">
                    <div class="auth-card">
                        <h1 id="register-heading">Create your account</h1>
                        <p class="auth-subtitle">Join UniSkill Hub to access resources, assignments, quizzes and the forum.</p>

                        <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert"></asp:Label>

                        <asp:ValidationSummary ID="vsRegister" runat="server" CssClass="alert alert-danger"
                            HeaderText="Please fix the following:" DisplayMode="BulletList" />

                        <div class="mb-3">
                            <asp:Label runat="server" AssociatedControlID="txtFullName" CssClass="form-label" Text="Full name" />
                            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="100" autocomplete="name" />
                            <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName"
                                ErrorMessage="Full name is required." Text="Full name is required." Display="Dynamic" CssClass="field-error" />
                        </div>

                        <div class="mb-3">
                            <asp:Label runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username" />
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" MaxLength="30" autocomplete="username" />
                            <div class="form-text">3&ndash;30 characters: letters, numbers and underscore only.</div>
                            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername"
                                ErrorMessage="Username is required." Text="Username is required." Display="Dynamic" CssClass="field-error" />
                            <asp:RegularExpressionValidator ID="revUsername" runat="server" ControlToValidate="txtUsername"
                                ValidationExpression="^[A-Za-z0-9_]{3,30}$"
                                ErrorMessage="Username must be 3-30 letters, numbers or underscores."
                                Text="Username must be 3-30 letters, numbers or underscores." Display="Dynamic" CssClass="field-error" />
                            <asp:CustomValidator ID="cvUsername" runat="server" ControlToValidate="txtUsername"
                                OnServerValidate="cvUsername_ServerValidate"
                                ErrorMessage="That username is already taken." Text="That username is already taken."
                                Display="Dynamic" CssClass="field-error" />
                        </div>

                        <div class="mb-3">
                            <asp:Label runat="server" AssociatedControlID="txtEmail" CssClass="form-label" Text="Email" />
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" autocomplete="email" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                ErrorMessage="Email is required." Text="Email is required." Display="Dynamic" CssClass="field-error" />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                                ValidationExpression="^\s*[^@\s]+@[^@\s]+\.[^@\s]+\s*$"
                                ErrorMessage="Enter a valid email address." Text="Enter a valid email address." Display="Dynamic" CssClass="field-error" />
                            <asp:CustomValidator ID="cvEmail" runat="server" ControlToValidate="txtEmail"
                                OnServerValidate="cvEmail_ServerValidate"
                                ErrorMessage="An account with that email already exists." Text="An account with that email already exists."
                                Display="Dynamic" CssClass="field-error" />
                        </div>

                        <div class="row">
                            <div class="col-sm-6 mb-3">
                                <asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" />
                                <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                                    ErrorMessage="Password is required." Text="Password is required." Display="Dynamic" CssClass="field-error" />
                                <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword"
                                    ValidationExpression="^.{8,64}$"
                                    ErrorMessage="Password must be at least 8 characters." Text="At least 8 characters." Display="Dynamic" CssClass="field-error" />
                            </div>
                            <div class="col-sm-6 mb-3">
                                <asp:Label runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label" Text="Confirm password" />
                                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                                <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                    ErrorMessage="Please confirm your password." Text="Please confirm your password." Display="Dynamic" CssClass="field-error" />
                                <asp:CompareValidator ID="cmpPassword" runat="server" ControlToValidate="txtConfirmPassword"
                                    ControlToCompare="txtPassword" Operator="Equal" Type="String"
                                    ErrorMessage="Passwords do not match." Text="Passwords do not match." Display="Dynamic" CssClass="field-error" />
                            </div>
                        </div>

                        <asp:Button ID="btnRegister" runat="server" Text="Create account" CssClass="btn btn-primary btn-lg w-100 mt-2" OnClick="btnRegister_Click" />

                        <p class="auth-footer">Already have an account? <a runat="server" href="~/Account/Login">Login</a></p>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
