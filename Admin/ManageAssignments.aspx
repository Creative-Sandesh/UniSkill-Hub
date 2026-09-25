<%@ Page Title="Manage Assignments" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageAssignments.aspx.cs" Inherits="UniSkillHub.Admin.ManageAssignments" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3">
                <div>
                    <h1>Manage assignments</h1>
                    <p>Create assignments, set deadlines and marks, and attach instruction files.</p>
                </div>
                <asp:Button ID="btnAdd" runat="server" Text="Add new assignment" CssClass="btn btn-primary btn-lg" CausesValidation="false" OnClick="btnAdd_Click" />
            </div>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

        <%-- ADD / EDIT FORM --%>
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="surface-card mb-4">
            <h2 class="h4 mb-4"><asp:Label ID="lblFormTitle" runat="server" Text="Add assignment"></asp:Label></h2>

            <asp:ValidationSummary ID="vsAssignment" runat="server" ValidationGroup="AssignmentForm" CssClass="alert alert-danger"
                HeaderText="Please fix the following:" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtTitle" CssClass="form-label" Text="Assignment title" />
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="150" ValidateRequestMode="Disabled" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ValidationGroup="AssignmentForm"
                        ErrorMessage="Title is required." Text="Title is required." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtDescription" CssClass="form-label" Text="Description" />
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                    <div class="form-text">A short summary, up to 1000 characters.</div>
                    <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription" ValidationGroup="AssignmentForm"
                        ErrorMessage="Description is required." Text="Description is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RegularExpressionValidator ID="revDescription" runat="server" ControlToValidate="txtDescription" ValidationGroup="AssignmentForm"
                        ValidationExpression="^[\s\S]{1,1000}$"
                        ErrorMessage="Description must be 1000 characters or fewer." Text="Description must be 1000 characters or fewer." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtInstructions" CssClass="form-label" Text="Instructions (optional)" />
                    <asp:TextBox ID="txtInstructions" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" ValidateRequestMode="Disabled" />
                    <div class="form-text">Detailed steps for students, up to 5000 characters. Blank lines start a new paragraph.</div>
                    <asp:RegularExpressionValidator ID="revInstructions" runat="server" ControlToValidate="txtInstructions" ValidationGroup="AssignmentForm"
                        ValidationExpression="^[\s\S]{0,5000}$"
                        ErrorMessage="Instructions must be 5000 characters or fewer." Text="Instructions must be 5000 characters or fewer." Display="Dynamic" CssClass="field-error" />
                </div>

                <div class="col-md-5">
                    <asp:Label runat="server" AssociatedControlID="txtDueDate" CssClass="form-label" Text="Due date and time" />
                    <asp:TextBox ID="txtDueDate" runat="server" CssClass="form-control" TextMode="DateTimeLocal" />
                    <asp:RequiredFieldValidator ID="rfvDueDate" runat="server" ControlToValidate="txtDueDate" ValidationGroup="AssignmentForm"
                        ErrorMessage="Please choose a due date and time." Text="Please choose a due date and time." Display="Dynamic" CssClass="field-error" />
                    <asp:CustomValidator ID="cvDueDate" runat="server" ControlToValidate="txtDueDate" ValidationGroup="AssignmentForm"
                        OnServerValidate="cvDueDate_ServerValidate" ErrorMessage="Invalid due date." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-3">
                    <asp:Label runat="server" AssociatedControlID="txtMaxMarks" CssClass="form-label" Text="Maximum marks" />
                    <asp:TextBox ID="txtMaxMarks" runat="server" CssClass="form-control" TextMode="Number" Text="100" />
                    <asp:RequiredFieldValidator ID="rfvMaxMarks" runat="server" ControlToValidate="txtMaxMarks" ValidationGroup="AssignmentForm"
                        ErrorMessage="Maximum marks is required." Text="Maximum marks is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RangeValidator ID="rngMaxMarks" runat="server" ControlToValidate="txtMaxMarks" ValidationGroup="AssignmentForm"
                        Type="Integer" MinimumValue="1" MaximumValue="1000"
                        ErrorMessage="Maximum marks must be a whole number from 1 to 1000." Text="Enter a whole number from 1 to 1000." Display="Dynamic" CssClass="field-error" />
                    <asp:CustomValidator ID="cvMaxMarks" runat="server" ControlToValidate="txtMaxMarks" ValidationGroup="AssignmentForm"
                        OnServerValidate="cvMaxMarks_ServerValidate" ErrorMessage="Invalid maximum marks." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Published" Text="Published (visible to students)"></asp:ListItem>
                        <asp:ListItem Value="Draft" Text="Draft (hidden)"></asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="fuInstructions" CssClass="form-label" Text="Instruction file (optional)" />
                    <asp:FileUpload ID="fuInstructions" runat="server" CssClass="form-control" accept=".pdf,.doc,.docx,.ppt,.pptx,.zip,.txt" />
                    <div class="form-text">PDF, DOC, DOCX, PPT, PPTX, ZIP or TXT &middot; maximum 10 MB.</div>
                    <asp:Label ID="lblCurrentFile" runat="server" CssClass="form-text d-block fw-semibold"></asp:Label>
                    <asp:CheckBox ID="chkRemoveFile" runat="server" Visible="false" Text="&nbsp;Remove the current instruction file" CssClass="form-text d-block" />
                    <asp:CustomValidator ID="cvFile" runat="server" ControlToValidate="fuInstructions" ValidationGroup="AssignmentForm"
                        ClientValidationFunction="validateSubmissionFile" OnServerValidate="cvFile_ServerValidate"
                        ErrorMessage="Invalid file." Display="Dynamic" CssClass="field-error" />
                </div>
            </div>

            <div class="d-flex flex-wrap gap-2 mt-4">
                <asp:Button ID="btnSave" runat="server" Text="Save assignment" CssClass="btn btn-primary" ValidationGroup="AssignmentForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>

        <%-- SEARCH + FILTERS --%>
        <section class="filter-bar" aria-label="Search and filter assignments">
            <div class="row g-3 align-items-end">
                <div class="col-lg-5">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Title or description..." />
                </div>
                <div class="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlStatusFilter" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All statuses"></asp:ListItem>
                        <asp:ListItem Value="Published" Text="Published"></asp:ListItem>
                        <asp:ListItem Value="Draft" Text="Draft"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlDueFilter" CssClass="form-label" Text="Deadline" />
                    <asp:DropDownList ID="ddlDueFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="Any"></asp:ListItem>
                        <asp:ListItem Value="Open" Text="Still open"></asp:ListItem>
                        <asp:ListItem Value="Past" Text="Past due"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" CausesValidation="false" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Admin/ManageAssignments">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <%-- ASSIGNMENTS GRID --%>
        <div class="table-responsive data-table-wrap">
            <asp:GridView ID="gvAssignments" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="AssignmentID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvAssignments_RowCommand" EmptyDataText="No assignments match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:BoundField DataField="AssignmentID" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Assignment">
                        <ItemTemplate>
                            <strong><%#: Eval("Title") %></strong>
                            <div class="dash-meta"><%#: Eval("MaxMarks") %> marks</div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Due">
                        <ItemTemplate>
                            <%#: Eval("DueDate", "{0:dd MMM yyyy, h:mm tt}") %>
                            <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("IsPastDue") == 1 %>'>
                                <div><span class="status-badge status-overdue">Past due</span></div>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Submissions">
                        <ItemTemplate><%# SubmissionText(Eval("SubmissionCount"), Eval("ToGrade")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# StatusClass(Eval("Status")) %>'><%#: Eval("Status") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <a class="btn btn-sm btn-outline-primary" href='<%# SubmissionsUrl(Eval("AssignmentID")) %>'>Submissions</a>
                            <asp:LinkButton runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-primary"
                                CommandName="EditAssignment" CommandArgument='<%# Eval("AssignmentID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Published" ? "Unpublish" : "Publish" %>' CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="TogglePublish" CommandArgument='<%# Eval("AssignmentID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="DeleteAssignment" CommandArgument='<%# Eval("AssignmentID") %>' CausesValidation="false"
                                OnClientClick="return confirm('Delete this assignment AND all student submissions for it (including their files)? This cannot be undone.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>

    <script src="<%= ResolveUrl("~/Scripts/validation.js") %>"></script>
</asp:Content>
