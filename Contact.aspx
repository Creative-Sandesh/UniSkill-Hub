<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="UniSkillHub.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Contact us</h1>
            <p>Questions about your account, resources or assignments? We are happy to help.</p>
        </div>
    </header>

    <div class="container mt-5">
        <div class="row g-4">
            <div class="col-md-6">
                <address class="surface-card h-100 mb-0">
                    <h2 class="h5">Student support</h2>
                    <p class="mb-1"><a href="mailto:support@uniskillhub.example">support@uniskillhub.example</a></p>
                    <p class="mb-0">Account, login and registration help.</p>
                </address>
            </div>
            <div class="col-md-6">
                <address class="surface-card h-100 mb-0">
                    <h2 class="h5">Lecturers &amp; administrators</h2>
                    <p class="mb-1"><a href="mailto:admin@uniskillhub.example">admin@uniskillhub.example</a></p>
                    <p class="mb-0">Course content, assignments and grading.</p>
                </address>
            </div>
        </div>
        <%-- Inline CSS example: small muted note --%>
        <p class="mt-4" style="color: var(--color-text-subtle); font-size: 0.875rem;">Replace these sample addresses with your faculty's real contact details before the final submission.</p>
    </div>
</asp:Content>
