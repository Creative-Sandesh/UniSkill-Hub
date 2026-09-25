<%@ Page Title="Assignment" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AssignmentDetails.aspx.cs" Inherits="UniSkillHub.Student.AssignmentDetails" %>

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
                        <li class="breadcrumb-item active" aria-current="page"><asp:Literal ID="litCrumb" runat="server"></asp:Literal></li>
                    </ol>
                </nav>
                <h1><asp:Literal ID="litTitle" runat="server"></asp:Literal></h1>
                <p class="resource-meta">
                    <asp:Literal ID="litStatusBadge" runat="server"></asp:Literal>
                    <span>Due <asp:Literal ID="litDue" runat="server"></asp:Literal></span>
                    <span><asp:Literal ID="litMaxMarks" runat="server"></asp:Literal> marks</span>
                </p>
            </div>
        </header>

        <div class="container mt-5">
            <div class="row g-4">

                <%-- Description + instructions --%>
                <div class="col-lg-7">
                    <article class="surface-card">
                        <h2 class="h5">Description</h2>
                        <div class="resource-description"><asp:Literal ID="litDescription" runat="server"></asp:Literal></div>

                        <asp:Panel ID="pnlInstructions" runat="server" Visible="false">
                            <h2 class="h5 mt-4">Instructions</h2>
                            <div class="resource-description"><asp:Literal ID="litInstructions" runat="server"></asp:Literal></div>
                        </asp:Panel>

                        <asp:Panel ID="pnlAttachment" runat="server" Visible="false" CssClass="mt-4">
                            <a id="lnkInstructions" runat="server" class="btn btn-outline-primary" href="~/Assignments/Download.ashx">Download instructions</a>
                        </asp:Panel>
                    </article>
                </div>

                <%-- My submission --%>
                <div class="col-lg-5">
                    <aside class="surface-card" aria-labelledby="mysub-heading">
                        <h2 id="mysub-heading" class="h5">My submission</h2>

                        <asp:Panel ID="pnlNoSubmission" runat="server" Visible="false">
                            <p class="text-muted">You have not submitted this assignment yet.</p>
                        </asp:Panel>

                        <asp:Panel ID="pnlSubmission" runat="server" Visible="false">
                            <dl class="detail-list">
                                <dt>Submitted</dt>
                                <dd><asp:Literal ID="litSubmittedDate" runat="server"></asp:Literal></dd>
                                <asp:Panel ID="pnlComment" runat="server" Visible="false">
                                    <dt>Your comment</dt>
                                    <dd><asp:Literal ID="litComment" runat="server"></asp:Literal></dd>
                                </asp:Panel>
                                <%-- Work the lecturer returned for changes: show their message --%>
                                <asp:Panel ID="pnlReturned" runat="server" Visible="false">
                                    <dt>Message from your lecturer</dt>
                                    <dd><asp:Literal ID="litReturnedNote" runat="server"></asp:Literal></dd>
                                </asp:Panel>
                                <asp:Panel ID="pnlGrade" runat="server" Visible="false">
                                    <dt>Marks</dt>
                                    <dd><strong><asp:Literal ID="litMarks" runat="server"></asp:Literal></strong></dd>
                                    <dt>Lecturer feedback</dt>
                                    <dd><asp:Literal ID="litFeedback" runat="server"></asp:Literal></dd>
                                </asp:Panel>
                            </dl>
                            <a id="lnkMySubmission" runat="server" class="btn btn-outline-primary btn-sm mb-3" href="~/Assignments/Download.ashx">Download my submitted file</a>
                        </asp:Panel>

                        <%-- Only one of these is shown, depending on the deadline / grading state --%>
                        <asp:Panel ID="pnlCanSubmit" runat="server" Visible="false">
                            <a id="lnkSubmit" runat="server" class="btn btn-primary w-100" href="~/Student/SubmitAssignment">Submit assignment</a>
                        </asp:Panel>
                        <asp:Panel ID="pnlLocked" runat="server" Visible="false">
                            <div class="alert alert-secondary mb-0" role="status"><asp:Literal ID="litLockedReason" runat="server"></asp:Literal></div>
                        </asp:Panel>
                    </aside>
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
