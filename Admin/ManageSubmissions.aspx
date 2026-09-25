<%@ Page Title="Manage Submissions" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageSubmissions.aspx.cs" Inherits="UniSkillHub.Admin.ManageSubmissions" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Student submissions</h1>
            <p>Review submitted work, enter marks and give feedback.</p>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

        <%-- GRADE FORM --%>
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="surface-card mb-4">
            <h2 class="h4 mb-3">Grade submission</h2>

            <dl class="grade-facts">
                <div><dt>Student</dt><dd><asp:Literal ID="litStudent" runat="server"></asp:Literal></dd></div>
                <div><dt>Assignment</dt><dd><asp:Literal ID="litAssignment" runat="server"></asp:Literal></dd></div>
                <div><dt>Submitted</dt><dd><asp:Literal ID="litSubmitted" runat="server"></asp:Literal></dd></div>
                <div><dt>Submitted file</dt><dd><a id="lnkFile" runat="server" class="btn btn-sm btn-outline-primary" href="~/Assignments/Download.ashx">Download file</a></dd></div>
            </dl>
            <asp:Panel ID="pnlStudentComment" runat="server" Visible="false" CssClass="student-comment">
                <strong>Student's comment:</strong> <asp:Literal ID="litStudentComment" runat="server"></asp:Literal>
            </asp:Panel>

            <asp:ValidationSummary ID="vsGrade" runat="server" ValidationGroup="GradeForm" CssClass="alert alert-danger"
                HeaderText="Please fix the following:" DisplayMode="BulletList" />

            <div class="row g-3 mt-1">
                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select js-grade-status">
                        <asp:ListItem Value="Graded" Text="Graded"></asp:ListItem>
                        <asp:ListItem Value="Submitted" Text="Submitted (return for resubmission)"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="form-text">"Submitted" lets the student upload a new file (before the deadline) and clears the marks.</div>
                </div>
                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="txtMarks" CssClass="form-label" Text="Marks" />
                    <asp:TextBox ID="txtMarks" runat="server" CssClass="form-control js-grade-marks" MaxLength="6" autocomplete="off" />
                    <asp:Label ID="lblMaxHint" runat="server" CssClass="form-text d-block"></asp:Label>
                    <asp:CustomValidator ID="cvMarks" runat="server" ControlToValidate="txtMarks" ValidationGroup="GradeForm"
                        ValidateEmptyText="true" ClientValidationFunction="validateGradeMarks" OnServerValidate="cvMarks_ServerValidate"
                        ErrorMessage="Invalid marks." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtFeedback" CssClass="form-label" Text="Feedback for the student" />
                    <asp:TextBox ID="txtFeedback" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled" />
                    <div class="form-text">Optional, up to 1000 characters.</div>
                    <asp:RegularExpressionValidator ID="revFeedback" runat="server" ControlToValidate="txtFeedback" ValidationGroup="GradeForm"
                        ValidationExpression="^[\s\S]{0,1000}$"
                        ErrorMessage="Feedback must be 1000 characters or fewer." Text="Feedback must be 1000 characters or fewer." Display="Dynamic" CssClass="field-error" />
                </div>
            </div>

            <div class="d-flex flex-wrap gap-2 mt-4">
                <asp:Button ID="btnSave" runat="server" Text="Save grade" CssClass="btn btn-primary" ValidationGroup="GradeForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>

        <%-- SEARCH + FILTERS --%>
        <section class="filter-bar" aria-label="Search and filter submissions">
            <div class="row g-3 align-items-end">
                <div class="col-lg-3">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Student" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Name, username or email..." />
                </div>
                <div class="col-lg-4">
                    <asp:Label runat="server" AssociatedControlID="ddlAssignment" CssClass="form-label" Text="Assignment" />
                    <asp:DropDownList ID="ddlAssignment" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlStatusFilter" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All statuses"></asp:ListItem>
                        <asp:ListItem Value="Submitted" Text="To grade"></asp:ListItem>
                        <asp:ListItem Value="Graded" Text="Graded"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" CausesValidation="false" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Admin/ManageSubmissions">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <%-- SUBMISSIONS GRID --%>
        <div class="table-responsive data-table-wrap">
            <asp:GridView ID="gvSubmissions" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="SubmissionID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvSubmissions_RowCommand" EmptyDataText="No submissions match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:TemplateField HeaderText="Student">
                        <ItemTemplate>
                            <strong><%#: Eval("FullName") %></strong>
                            <div class="dash-meta">@<%#: Eval("Username") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Assignment">
                        <ItemTemplate>
                            <%#: Eval("Title") %>
                            <div class="dash-meta">Max <%#: Eval("MaxMarks") %> marks</div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Submitted">
                        <ItemTemplate>
                            <%#: Eval("SubmittedDate", "{0:dd MMM yyyy, h:mm tt}") %>
                            <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("IsLate") == 1 %>'>
                                <div><span class="status-badge status-overdue">Late</span></div>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# StatusClass(Eval("Status")) %>'><%# StatusText(Eval("Status")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Marks">
                        <ItemTemplate><%# MarksText(Eval("Marks"), Eval("MaxMarks")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <a class="btn btn-sm btn-outline-primary" href='<%# FileUrl(Eval("SubmissionID")) %>'>Download</a>
                            <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Graded" ? "Edit grade" : "Grade" %>'
                                CssClass="btn btn-sm btn-primary" CommandName="GradeSubmission" CommandArgument='<%# Eval("SubmissionID") %>' CausesValidation="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>

    <script src="<%= ResolveUrl("~/Scripts/validation.js") %>"></script>
</asp:Content>
