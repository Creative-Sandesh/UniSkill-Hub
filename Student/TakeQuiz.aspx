<%@ Page Title="Take Quiz" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TakeQuiz.aspx.cs" Inherits="UniSkillHub.Student.TakeQuiz" %>

<asp:Content ID="HeadCss" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Internal CSS: only needed on this page (the sticky timer bar) --%>
    <style>
        .quiz-bar { position: sticky; top: 4.5rem; z-index: 1020; display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: var(--space-3);
                    padding: var(--space-3) var(--space-6); margin-bottom: var(--space-6); background: var(--color-surface);
                    border: 1px solid var(--color-border); border-radius: var(--radius-lg); box-shadow: var(--shadow-md); }
        .quiz-timer { font-variant-numeric: tabular-nums; font-size: var(--font-size-xl); font-weight: 800; color: var(--color-primary-700); }
        .quiz-timer.is-low { color: var(--color-error); }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="container py-5">
            <div class="empty-state">
                <h1 class="h3">Quiz not found</h1>
                <p>This quiz does not exist or is not available.</p>
                <a class="btn btn-primary" runat="server" href="~/Student/Quizzes">Back to quizzes</a>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlQuiz" runat="server">
        <header class="page-header">
            <div class="container">
                <nav aria-label="Breadcrumb">
                    <ol class="breadcrumb mb-2">
                        <li class="breadcrumb-item"><a runat="server" href="~/Student/Quizzes">Quizzes</a></li>
                        <li class="breadcrumb-item active" aria-current="page"><asp:Literal ID="litCrumb" runat="server"></asp:Literal></li>
                    </ol>
                </nav>
                <h1><asp:Literal ID="litTitle" runat="server"></asp:Literal></h1>
                <p class="resource-meta">
                    <span><asp:Literal ID="litQuestionCount" runat="server"></asp:Literal> questions</span>
                    <span><asp:Literal ID="litTotalMarks" runat="server"></asp:Literal> marks</span>
                    <span><asp:Literal ID="litTimeLimit" runat="server"></asp:Literal> minutes</span>
                </p>
            </div>
        </header>

        <div class="container mt-4">
            <div class="row justify-content-center">
                <div class="col-lg-9">

                    <%-- Time ran out / session lost --%>
                    <asp:Panel ID="pnlExpired" runat="server" Visible="false">
                        <div class="alert alert-warning" role="alert"><asp:Literal ID="litExpired" runat="server"></asp:Literal></div>
                        <a id="lnkRestart" runat="server" class="btn btn-primary" href="~/Student/Quizzes">Start again</a>
                    </asp:Panel>

                    <asp:Panel ID="pnlTake" runat="server">
                        <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="alert alert-danger d-block" role="alert"></asp:Label>

                        <div class="quiz-bar" data-quiz-root="1">
                            <span>Time left: <strong id="spanTimer" runat="server" class="quiz-timer" data-quiz-timer="1">--:--</strong></span>
                            <span>Answered: <strong><span data-quiz-answered="1">0</span> / <asp:Literal ID="litTotalQuestions" runat="server"></asp:Literal></strong></span>
                        </div>

                        <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound">
                            <ItemTemplate>
                                <fieldset class="quiz-question">
                                    <legend>
                                        <span class="q-number">Question <%# Container.ItemIndex + 1 %></span>
                                        <span class="q-marks"><%# MarksText(Eval("Marks")) %></span>
                                    </legend>
                                    <p class="q-text"><%#: Eval("QuestionText") %></p>
                                    <asp:HiddenField ID="hfQuestionID" runat="server" Value='<%# Eval("QuestionID") %>' />
                                    <asp:RadioButtonList ID="rblOptions" runat="server" CssClass="quiz-options" RepeatLayout="UnorderedList"></asp:RadioButtonList>
                                </fieldset>
                            </ItemTemplate>
                        </asp:Repeater>

                        <div class="d-flex flex-wrap gap-2 mt-4">
                            <asp:Button ID="btnSubmit" runat="server" Text="Submit quiz" CssClass="btn btn-primary btn-lg" data-quiz-submit="1"
                                OnClientClick="return confirmQuizSubmit();" OnClick="btnSubmit_Click" />
                            <a class="btn btn-outline-primary btn-lg" runat="server" href="~/Student/Quizzes">Leave quiz</a>
                        </div>
                        <p class="form-text mt-2">You can leave any question blank. Your answers are only saved when you submit.</p>
                    </asp:Panel>
                </div>
            </div>
        </div>

        <script src="<%= ResolveUrl("~/Scripts/quiz.js") %>"></script>
    </asp:Panel>
</asp:Content>
