<%@ Page Title="My Submissions" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MySubmissions.aspx.cs" Inherits="UniSkillHub.Student.MySubmissions" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>My submissions</h1>
            <p>Track what you have handed in, your marks and your lecturer's feedback.</p>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Panel ID="pnlSubmitted" runat="server" Visible="false" CssClass="alert alert-success" role="status">
            <strong>Success!</strong> Your assignment has been submitted.
        </asp:Panel>

        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <p class="result-count mb-0" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>
            <a class="btn btn-outline-primary" runat="server" href="~/Student/Assignments">All assignments</a>
        </div>

        <asp:Repeater ID="rptSubmissions" runat="server">
            <HeaderTemplate>
                <div class="table-responsive data-table-wrap">
                    <table class="table table-hover align-middle data-table mb-0">
                        <thead>
                            <tr>
                                <th scope="col">Assignment</th>
                                <th scope="col">Submitted</th>
                                <th scope="col">Status</th>
                                <th scope="col">Marks</th>
                                <th scope="col">Feedback</th>
                                <th scope="col"><span class="visually-hidden">File</span></th>
                            </tr>
                        </thead>
                        <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <a class="fw-semibold" href='<%# DetailsUrl(Eval("AssignmentID")) %>'><%#: Eval("Title") %></a>
                        <div class="dash-meta">Due <%#: Eval("DueDate", "{0:dd MMM yyyy}") %></div>
                    </td>
                    <td><%# SubmittedText(Eval("SubmittedDate")) %></td>
                    <td><span class='<%# StatusClass(Eval("MyStatus")) %>'><%#: Eval("MyStatus") %></span></td>
                    <td><%# MarksText(Eval("Marks"), Eval("MaxMarks")) %></td>
                    <td class="feedback-cell"><%# FeedbackText(Eval("Feedback")) %></td>
                    <td class="text-end">
                        <asp:PlaceHolder runat="server" Visible='<%# !(Eval("SubmissionID") is DBNull) %>'>
                            <a class="btn btn-sm btn-outline-primary" href='<%# FileUrl(Eval("SubmissionID")) %>'>Download</a>
                        </asp:PlaceHolder>
                        <asp:PlaceHolder runat="server" Visible='<%# Eval("MyStatus").ToString() == "Pending" %>'>
                            <a class="btn btn-sm btn-primary" href='<%# SubmitUrl(Eval("AssignmentID")) %>'>Submit</a>
                        </asp:PlaceHolder>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                        </tbody>
                    </table>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty-state">
            <h2>Nothing to show yet</h2>
            <p>There are no published assignments right now.</p>
        </asp:Panel>
    </div>
</asp:Content>
