<%@ Page Title="Manage Quizzes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageQuizzes.aspx.cs" Inherits="UniSkillHub.Admin.ManageQuizzes" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3">
                <div>
                    <h1>Manage quizzes</h1>
                    <p>Create quizzes, add their questions and choose when students can take them.</p>
                </div>
                <asp:Button ID="btnAdd" runat="server" Text="Add new quiz" CssClass="btn btn-primary btn-lg" CausesValidation="false" OnClick="btnAdd_Click" />
            </div>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

        <%-- ADD / EDIT FORM --%>
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="surface-card mb-4">
            <h2 class="h4 mb-4"><asp:Label ID="lblFormTitle" runat="server" Text="Add quiz"></asp:Label></h2>

            <asp:ValidationSummary ID="vsQuiz" runat="server" ValidationGroup="QuizForm" CssClass="alert alert-danger"
                HeaderText="Please fix the following:" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtTitle" CssClass="form-label" Text="Quiz title" />
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="150" ValidateRequestMode="Disabled" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ValidationGroup="QuizForm"
                        ErrorMessage="Title is required." Text="Title is required." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtDescription" CssClass="form-label" Text="Description (optional)" />
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                    <div class="form-text">Shown to students on the quiz list, up to 500 characters.</div>
                    <asp:RegularExpressionValidator ID="revDescription" runat="server" ControlToValidate="txtDescription" ValidationGroup="QuizForm"
                        ValidationExpression="^[\s\S]{0,500}$"
                        ErrorMessage="Description must be 500 characters or fewer." Text="Description must be 500 characters or fewer." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="txtTimeLimit" CssClass="form-label" Text="Time limit (minutes)" />
                    <asp:TextBox ID="txtTimeLimit" runat="server" CssClass="form-control" TextMode="Number" Text="10" />
                    <asp:RequiredFieldValidator ID="rfvTimeLimit" runat="server" ControlToValidate="txtTimeLimit" ValidationGroup="QuizForm"
                        ErrorMessage="Time limit is required." Text="Time limit is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RangeValidator ID="rngTimeLimit" runat="server" ControlToValidate="txtTimeLimit" ValidationGroup="QuizForm"
                        Type="Integer" MinimumValue="1" MaximumValue="300"
                        ErrorMessage="Time limit must be a whole number of minutes from 1 to 300." Text="Enter a whole number from 1 to 300." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-8">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Draft" Text="Draft (hidden from students)"></asp:ListItem>
                        <asp:ListItem Value="Published" Text="Published (students can take it)"></asp:ListItem>
                    </asp:DropDownList>
                    <div class="form-text">A quiz needs at least one question before it can be published.</div>
                    <asp:CustomValidator ID="cvStatus" runat="server" ValidationGroup="QuizForm" OnServerValidate="cvStatus_ServerValidate"
                        ErrorMessage="Add at least one question before publishing this quiz." Display="Dynamic" CssClass="field-error" />
                </div>
            </div>

            <div class="d-flex flex-wrap gap-2 mt-4">
                <asp:Button ID="btnSave" runat="server" Text="Save quiz" CssClass="btn btn-primary" ValidationGroup="QuizForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>

        <%-- SEARCH + FILTER --%>
        <section class="filter-bar" aria-label="Search and filter quizzes">
            <div class="row g-3 align-items-end">
                <div class="col-lg-6">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Title or description..." />
                </div>
                <div class="col-sm-6 col-lg-3">
                    <asp:Label runat="server" AssociatedControlID="ddlStatusFilter" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All statuses"></asp:ListItem>
                        <asp:ListItem Value="Published" Text="Published"></asp:ListItem>
                        <asp:ListItem Value="Draft" Text="Draft"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-sm-6 col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" CausesValidation="false" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Admin/ManageQuizzes">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <%-- QUIZZES GRID --%>
        <div class="table-responsive data-table-wrap">
            <asp:GridView ID="gvQuizzes" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="QuizID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvQuizzes_RowCommand" EmptyDataText="No quizzes match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:BoundField DataField="QuizID" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Quiz">
                        <ItemTemplate>
                            <strong><%#: Eval("Title") %></strong>
                            <div class="dash-meta"><%#: Eval("TimeLimit") %> min</div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Questions">
                        <ItemTemplate><%# QuestionsText(Eval("QuestionCount"), Eval("TotalMarks")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Attempts" HeaderText="Attempts" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# StatusClass(Eval("Status")) %>'><%#: Eval("Status") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <a class="btn btn-sm btn-primary" href='<%# QuestionsUrl(Eval("QuizID")) %>'>Questions</a>
                            <asp:LinkButton runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-primary"
                                CommandName="EditQuiz" CommandArgument='<%# Eval("QuizID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Published" ? "Unpublish" : "Publish" %>' CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="TogglePublish" CommandArgument='<%# Eval("QuizID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="DeleteQuiz" CommandArgument='<%# Eval("QuizID") %>' CausesValidation="false"
                                OnClientClick="return confirm('Delete this quiz AND all its questions and every student attempt? This cannot be undone.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>
</asp:Content>
