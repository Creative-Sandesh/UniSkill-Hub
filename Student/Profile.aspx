    <%@ Page Title="My Profile" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="UniSkillHub.Student.Profile" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>My profile</h1>
            <p>Update your details or change your password.</p>
        </div>
    </header>

    <div class="container mt-5">
        <div class="row g-4">

            <%-- ACCOUNT DETAILS --%>
            <div class="col-lg-6">
                <section class="surface-card" aria-labelledby="details-heading">
                    <h2 id="details-heading" class="h4 mb-4">Account details</h2>

                    <asp:Label ID="lblProfileMessage" runat="server" Visible="false" CssClass="alert d-block" role="status"></asp:Label>
                    <asp:ValidationSummary ID="vsProfile" runat="server" ValidationGroup="ProfileGroup" CssClass="alert alert-danger"
                        HeaderText="Please fix the following:" DisplayMode="BulletList" />

                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtUsername" CssClass="form-label" Text="Username" />
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" ReadOnly="true" />
                        <div class="form-text">Your username cannot be changed.</div>
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtFullName" CssClass="form-label" Text="Full name" />
                        <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="100" autocomplete="name" />
                        <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" ValidationGroup="ProfileGroup"
                            ErrorMessage="Full name is required." Text="Full name is required." Display="Dynamic" CssClass="field-error" />
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtEmail" CssClass="form-label" Text="Email" />
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" autocomplete="email" />
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="ProfileGroup"
                            ErrorMessage="Email is required." Text="Email is required." Display="Dynamic" CssClass="field-error" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="ProfileGroup"
                            ValidationExpression="^\s*[^@\s]+@[^@\s]+\.[^@\s]+\s*$"
                            ErrorMessage="Enter a valid email address." Text="Enter a valid email address." Display="Dynamic" CssClass="field-error" />
                        <asp:CustomValidator ID="cvEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="ProfileGroup"
                            OnServerValidate="cvEmail_ServerValidate"
                            ErrorMessage="Another account already uses that email." Text="Another account already uses that email."
                            Display="Dynamic" CssClass="field-error" />
                    </div>

                    <p class="form-text mb-4">Member since <asp:Label ID="lblMemberSince" runat="server"></asp:Label></p>

                    <asp:Button ID="btnUpdate" runat="server" Text="Save changes" CssClass="btn btn-primary"
                        ValidationGroup="ProfileGroup" OnClick="btnUpdate_Click" />
                </section>
            </div>

            <%-- CHANGE PASSWORD --%>
            <div class="col-lg-6">
                <section class="surface-card" aria-labelledby="password-heading">
                    <h2 id="password-heading" class="h4 mb-4">Change password</h2>

                    <asp:Label ID="lblPasswordMessage" runat="server" Visible="false" CssClass="alert d-block" role="status"></asp:Label>
                    <asp:ValidationSummary ID="vsPassword" runat="server" ValidationGroup="PasswordGroup" CssClass="alert alert-danger"
                        HeaderText="Please fix the following:" DisplayMode="BulletList" />

                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtCurrentPassword" CssClass="form-label" Text="Current password" />
                        <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="current-password" />
                        <asp:RequiredFieldValidator ID="rfvCurrentPassword" runat="server" ControlToValidate="txtCurrentPassword" ValidationGroup="PasswordGroup"
                            ErrorMessage="Enter your current password." Text="Enter your current password." Display="Dynamic" CssClass="field-error" />
                    </div>

                    <div class="mb-3">
                        <asp:Label runat="server" AssociatedControlID="txtNewPassword" CssClass="form-label" Text="New password" />
                        <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                        <div class="form-text">At least 8 characters.</div>
                        <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server" ControlToValidate="txtNewPassword" ValidationGroup="PasswordGroup"
                            ErrorMessage="Enter a new password." Text="Enter a new password." Display="Dynamic" CssClass="field-error" />
                        <asp:RegularExpressionValidator ID="revNewPassword" runat="server" ControlToValidate="txtNewPassword" ValidationGroup="PasswordGroup"
                            ValidationExpression="^.{8,64}$"
                            ErrorMessage="New password must be at least 8 characters." Text="New password must be at least 8 characters." Display="Dynamic" CssClass="field-error" />
                    </div>

                    <div class="mb-4">
                        <asp:Label runat="server" AssociatedControlID="txtConfirmPassword" CssClass="form-label" Text="Confirm new password" />
                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" MaxLength="64" autocomplete="new-password" />
                        <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ValidationGroup="PasswordGroup"
                            ErrorMessage="Please confirm the new password." Text="Please confirm the new password." Display="Dynamic" CssClass="field-error" />
                        <asp:CompareValidator ID="cmpPassword" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtNewPassword"
                            Operator="Equal" Type="String" ValidationGroup="PasswordGroup"
                            ErrorMessage="Passwords do not match." Text="Passwords do not match." Display="Dynamic" CssClass="field-error" />
                    </div>

                    <asp:Button ID="btnChangePassword" runat="server" Text="Change password" CssClass="btn btn-outline-primary"
                        ValidationGroup="PasswordGroup" OnClick="btnChangePassword_Click" />
                </section>
            </div>
        </div>
    </div>
</asp:Content>
