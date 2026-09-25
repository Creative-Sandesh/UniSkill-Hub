<%@ Page Title="Assignments" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Assignments.aspx.cs" Inherits="UniSkillHub.Student.Assignments" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Assignments</h1>
            <p>See what is due, hand in your work and follow your progress.</p>
        </div>
    </header>

    <div class="container mt-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <p class="result-count mb-0" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>
            <a class="btn btn-outline-primary" runat="server" href="~/Student/MySubmissions">My submissions &amp; marks</a>
        </div>

        <asp:Repeater ID="rptAssignments" runat="server">
            <HeaderTemplate>
                <div class="table-responsive data-table-wrap">
                    <table class="table table-hover align-middle data-table mb-0">
                        <thead>
                            <tr>
                                <th scope="col">Assignment</th>
                                <th scope="col">Due date</th>
                                <th scope="col">Marks</th>
                                <th scope="col">Status</th>
                                <th scope="col"><span class="visually-hidden">Action</span></th>
                            </tr>
                        </thead>
                        <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><a class="fw-semibold" href='<%# DetailsUrl(Eval("AssignmentID")) %>'><%#: Eval("Title") %></a></td>
                    <td>
                        <%#: Eval("DueDate", "{0:dd MMM yyyy, h:mm tt}") %>
                        <asp:PlaceHolder runat="server" Visible='<%# Eval("MyStatus").ToString() == "Pending" %>'>
                            <%-- Inline CSS example: highlights the countdown --%>
                            <div style="font-size: 0.8125rem; font-weight: 600; color: var(--color-warning);"><%#: DueText(Eval("DueDate")) %></div>
                        </asp:PlaceHolder>
                    </td>
                    <td><%#: Eval("MaxMarks") %></td>
                    <td><span class='<%# StatusClass(Eval("MyStatus")) %>'><%#: Eval("MyStatus") %></span></td>
                    <td class="text-end">
                        <asp:PlaceHolder runat="server" Visible='<%# Eval("MyStatus").ToString() == "Pending" %>'>
                            <a class="btn btn-sm btn-primary" href='<%# SubmitUrl(Eval("AssignmentID")) %>'>Submit</a>
                        </asp:PlaceHolder>
                        <a class="btn btn-sm btn-outline-primary" href='<%# DetailsUrl(Eval("AssignmentID")) %>'>View</a>
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
            <h2>No assignments yet</h2>
            <p>Your lecturers have not published any assignments. Check back soon.</p>
        </asp:Panel>
    </div>
</asp:Content>
