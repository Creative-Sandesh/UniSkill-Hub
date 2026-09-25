<%@ Page Title="Submit Assignment" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SubmitAssignment.aspx.cs" Inherits="UniSkillHub.Student.SubmitAssignment" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="container py-5">
            <div class="empty-state">
                <h1 class="h3">Assignment not found</h1>
                <p>This assignment does not exist or is not available.</p>
                <a class="btn btn-primary" runat="server" href="~/Student/Assignments">Back to assignments</a>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlAssignment" runat="server">
        <header class="page-header">
            <div class="container">
                <nav aria-label="Breadcrumb">
                    <ol class="breadcrumb mb-2">
                        <li class="breadcrumb-item"><a runat="server" href="~/Student/Assignments">Assignments</a></li>
                        <li class="breadcrumb-item"><a id="lnkBack" runat="server" href="~/Student/AssignmentDetails"><asp:Literal ID="litCrumb" runat="server"></asp:Literal></a></li>
                        <li class="breadcrumb-item active" aria-current="page">Submit</li>
                    </ol>
                </nav>
                <h1>Submit assignment</h1>
                <p class="resource-meta">
                    <strong><asp:Literal ID="litTitle" runat="server"></asp:Literal></strong>
                    <span>Due <asp:Literal ID="litDue" runat="server"></asp:Literal></span>
                    <span><asp:Literal ID="litMaxMarks" runat="server"></asp:Literal> marks</span>
                </p>
            </div>
        </header>

        <div class="container mt-5">
            <div class="row justify-content-center">
                <div class="col-lg-8">

                    <%-- Deadline passed / already graded --%>
                    <asp:Panel ID="pnlClosed" runat="server" Visible="false">
                        <div class="alert alert-warning" role="alert"><asp:Literal ID="litClosedReason" runat="server"></asp:Literal></div>
                        <a id="lnkBackClosed" runat="server" class="btn btn-outline-primary" href="~/Student/AssignmentDetails">Back to the assignment</a>
                    </asp:Panel>

                    <asp:Panel ID="pnlForm" runat="server">
                        <div class="surface-card">
                            <asp:Panel ID="pnlExisting" runat="server" Visible="false" CssClass="alert alert-info" role="status">
                                You already submitted this assignment on <strong><asp:Literal ID="litExistingDate" runat="server"></asp:Literal></strong>.
                                Uploading a new file will <strong>replace</strong> it (allowed until the deadline).
                            </asp:Panel>

                            <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert"></asp:Label>
                            <asp:ValidationSummary ID="vsSubmit" runat="server" CssClass="alert alert-danger"
                                HeaderText="Please fix the following:" DisplayMode="BulletList" />

                            <div class="mb-3">
                                <asp:Label runat="server" AssociatedControlID="fuAssignment" CssClass="form-label" Text="Your file" />
                                <asp:FileUpload ID="fuAssignment" runat="server" CssClass="form-control" accept=".pdf,.doc,.docx,.ppt,.pptx,.zip,.txt" />
                                <div class="form-text">PDF, DOC, DOCX, PPT, PPTX, ZIP or TXT &middot; maximum 10 MB.</div>
                                <asp:RequiredFieldValidator ID="rfvFile" runat="server" ControlToValidate="fuAssignment"
                                    ErrorMessage="Please choose a file to upload." Text="Please choose a file to upload." Display="Dynamic" CssClass="field-error" />
                                <%-- Client-side check lives in Scripts/validation.js; the server checks again --%>
                                <asp:CustomValidator ID="cvFile" runat="server" ControlToValidate="fuAssignment"
                                    ClientValidationFunction="validateSubmissionFile" OnServerValidate="cvFile_ServerValidate"
                                    ErrorMessage="Invalid file." Display="Dynamic" CssClass="field-error" />
                            </div>

                            <div class="mb-4">
                                <asp:Label runat="server" AssociatedControlID="txtComment" CssClass="form-label" Text="Comment (optional)" />
                                <asp:TextBox ID="txtComment" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4"
                                    placeholder="Anything you would like your lecturer to know..." />
                                <asp:RegularExpressionValidator ID="revComment" runat="server" ControlToValidate="txtComment"
                                    ValidationExpression="^[\s\S]{0,500}$"
                                    ErrorMessage="Comment must be 500 characters or fewer." Text="Comment must be 500 characters or fewer." Display="Dynamic" CssClass="field-error" />
                            </div>

                            <div class="d-flex flex-wrap gap-2">
                                <asp:Button ID="btnSubmit" runat="server" Text="Submit assignment" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
                                <a id="lnkCancel" runat="server" class="btn btn-outline-primary" href="~/Student/AssignmentDetails">Cancel</a>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>

        <script src="<%= ResolveUrl("~/Scripts/validation.js") %>"></script>
    </asp:Panel>
</asp:Content>
