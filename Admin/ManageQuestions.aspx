<%@ Page Title="Manage Questions" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageQuestions.aspx.cs" Inherits="UniSkillHub.Admin.ManageQuestions" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="container py-5">
            <div class="empty-state">
                <h1 class="h3">Quiz not found</h1>
                <p>This quiz does not exist. Choose a quiz from the quiz list.</p>
                <a class="btn btn-primary" runat="server" href="~/Admin/ManageQuizzes">Back to quizzes</a>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlQuiz" runat="server">
        <header class="page-header">
            <div class="container">
                <nav aria-label="Breadcrumb">
                    <ol class="breadcrumb mb-2">
                        <li class="breadcrumb-item"><a runat="server" href="~/Admin/ManageQuizzes">Quizzes</a></li>
                        <li class="breadcrumb-item active" aria-current="page"><asp:Literal ID="litCrumb" runat="server"></asp:Literal></li>
                    </ol>
                </nav>
                <div class="d-flex flex-wrap justify-content-between align-items-end gap-3">
                    <div>
                        <h1>Questions</h1>
                        <p class="resource-meta">
                            <strong><asp:Literal ID="litTitle" runat="server"></asp:Literal></strong>
                            <asp:Literal ID="litStatusBadge" runat="server"></asp:Literal>
                            <span><asp:Literal ID="litSummary" runat="server"></asp:Literal></span>
                        </p>
                    </div>
                    <asp:Button ID="btnAdd" runat="server" Text="Add question" CssClass="btn btn-primary btn-lg" CausesValidation="false" OnClick="btnAdd_Click" />
                </div>
            </div>
        </header>

        <div class="container mt-4">

            <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

            <%-- ADD / EDIT QUESTION FORM --%>
            <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="surface-card mb-4">
                <h2 class="h4 mb-3"><asp:Label ID="lblFormTitle" runat="server" Text="Add question"></asp:Label></h2>

                <asp:Label ID="lblAttemptsWarning" runat="server" Visible="false" CssClass="alert alert-warning d-block" role="note"></asp:Label>

                <asp:ValidationSummary ID="vsQuestion" runat="server" ValidationGroup="QuestionForm" CssClass="alert alert-danger"
                    HeaderText="Please fix the following:" DisplayMode="BulletList" />

                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="txtQuestion" CssClass="form-label" Text="Question" />
                    <asp:TextBox ID="txtQuestion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                    <asp:RequiredFieldValidator ID="rfvQuestion" runat="server" ControlToValidate="txtQuestion" ValidationGroup="QuestionForm"
                        ErrorMessage="The question text is required." Text="The question text is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RegularExpressionValidator ID="revQuestion" runat="server" ControlToValidate="txtQuestion" ValidationGroup="QuestionForm"
                        ValidationExpression="^[\s\S]{1,500}$"
                        ErrorMessage="The question must be 500 characters or fewer." Text="The question must be 500 characters or fewer." Display="Dynamic" CssClass="field-error" />
                </div>

                <div class="row g-3">
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="txtOptionA" CssClass="form-label" Text="Option A" />
                        <asp:TextBox ID="txtOptionA" runat="server" CssClass="form-control" MaxLength="300" ValidateRequestMode="Disabled" />
                        <asp:RequiredFieldValidator ID="rfvA" runat="server" ControlToValidate="txtOptionA" ValidationGroup="QuestionForm"
                            ErrorMessage="Option A is required." Text="Option A is required." Display="Dynamic" CssClass="field-error" />
                    </div>
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="txtOptionB" CssClass="form-label" Text="Option B" />
                        <asp:TextBox ID="txtOptionB" runat="server" CssClass="form-control" MaxLength="300" ValidateRequestMode="Disabled" />
                        <asp:RequiredFieldValidator ID="rfvB" runat="server" ControlToValidate="txtOptionB" ValidationGroup="QuestionForm"
                            ErrorMessage="Option B is required." Text="Option B is required." Display="Dynamic" CssClass="field-error" />
                    </div>
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="txtOptionC" CssClass="form-label" Text="Option C" />
                        <asp:TextBox ID="txtOptionC" runat="server" CssClass="form-control" MaxLength="300" ValidateRequestMode="Disabled" />
                        <asp:RequiredFieldValidator ID="rfvC" runat="server" ControlToValidate="txtOptionC" ValidationGroup="QuestionForm"
                            ErrorMessage="Option C is required." Text="Option C is required." Display="Dynamic" CssClass="field-error" />
                    </div>
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="txtOptionD" CssClass="form-label" Text="Option D" />
                        <asp:TextBox ID="txtOptionD" runat="server" CssClass="form-control" MaxLength="300" ValidateRequestMode="Disabled" />
                        <asp:RequiredFieldValidator ID="rfvD" runat="server" ControlToValidate="txtOptionD" ValidationGroup="QuestionForm"
                            ErrorMessage="Option D is required." Text="Option D is required." Display="Dynamic" CssClass="field-error" />
                        <asp:CustomValidator ID="cvDistinct" runat="server" ValidationGroup="QuestionForm" OnServerValidate="cvDistinct_ServerValidate"
                            ErrorMessage="The four options must all be different." Display="Dynamic" CssClass="field-error" />
                    </div>

                    <div class="col-md-6">
                        <span class="form-label d-block" id="lblCorrect">Correct answer</span>
                        <asp:RadioButtonList ID="rblCorrect" runat="server" RepeatDirection="Horizontal" CssClass="correct-choice" aria-labelledby="lblCorrect">
                            <asp:ListItem Value="A" Text="A"></asp:ListItem>
                            <asp:ListItem Value="B" Text="B"></asp:ListItem>
                            <asp:ListItem Value="C" Text="C"></asp:ListItem>
                            <asp:ListItem Value="D" Text="D"></asp:ListItem>
                        </asp:RadioButtonList>
                        <asp:RequiredFieldValidator ID="rfvCorrect" runat="server" ControlToValidate="rblCorrect" ValidationGroup="QuestionForm"
                            ErrorMessage="Choose which option is correct." Text="Choose which option is correct." Display="Dynamic" CssClass="field-error" />
                    </div>
                    <div class="col-md-3">
                        <asp:Label runat="server" AssociatedControlID="txtMarks" CssClass="form-label" Text="Marks" />
                        <asp:TextBox ID="txtMarks" runat="server" CssClass="form-control" TextMode="Number" Text="1" />
                        <asp:RequiredFieldValidator ID="rfvMarks" runat="server" ControlToValidate="txtMarks" ValidationGroup="QuestionForm"
                            ErrorMessage="Marks are required." Text="Marks are required." Display="Dynamic" CssClass="field-error" />
                        <asp:RangeValidator ID="rngMarks" runat="server" ControlToValidate="txtMarks" ValidationGroup="QuestionForm"
                            Type="Integer" MinimumValue="1" MaximumValue="100"
                            ErrorMessage="Marks must be a whole number from 1 to 100." Text="Enter a whole number from 1 to 100." Display="Dynamic" CssClass="field-error" />
                    </div>
                </div>

                <div class="d-flex flex-wrap gap-2 mt-4">
                    <asp:Button ID="btnSave" runat="server" Text="Save question" CssClass="btn btn-primary" ValidationGroup="QuestionForm" OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </asp:Panel>

            <%-- QUESTIONS GRID --%>
            <div class="table-responsive data-table-wrap">
                <asp:GridView ID="gvQuestions" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="QuestionID"
                    CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                    OnRowCommand="gvQuestions_RowCommand" EmptyDataText="This quiz has no questions yet. Click &quot;Add question&quot; to create the first one.">
                    <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                    <Columns>
                        <asp:TemplateField HeaderText="#">
                            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Question and options">
                            <ItemTemplate>
                                <strong><%#: Eval("QuestionText") %></strong>
                                <ul class="option-preview"><%# OptionsHtml(Eval("QuestionID"), Eval("CorrectOption")) %></ul>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Marks">
                            <ItemTemplate><%#: Eval("Marks") %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemStyle CssClass="text-end text-nowrap" />
                            <ItemTemplate>
                                <asp:LinkButton runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-primary"
                                    CommandName="EditQuestion" CommandArgument='<%# Eval("QuestionID") %>' CausesValidation="false" />
                                <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                    CommandName="DeleteQuestion" CommandArgument='<%# Eval("QuestionID") %>' CausesValidation="false"
                                    OnClientClick="return confirm('Delete this question and the students\' answers to it? This cannot be undone.');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
