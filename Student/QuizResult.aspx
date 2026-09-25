<%@ Page Title="Quiz Result" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QuizResult.aspx.cs" Inherits="UniSkillHub.Student.QuizResult" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
        <div class="container py-5">
            <div class="empty-state">
                <h1 class="h3">Result not found</h1>
                <p>This quiz result does not exist or is not yours to view.</p>
                <a class="btn btn-primary" runat="server" href="~/Student/Quizzes">Back to quizzes</a>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlResult" runat="server">
        <header class="page-header">
            <div class="container">
                <nav aria-label="Breadcrumb">
                    <ol class="breadcrumb mb-2">
                        <li class="breadcrumb-item"><a runat="server" href="~/Student/Quizzes">Quizzes</a></li>
                        <li class="breadcrumb-item active" aria-current="page">Result</li>
                    </ol>
                </nav>
                <h1>Quiz completed</h1>
                <p><asp:Literal ID="litQuizTitle" runat="server"></asp:Literal> &middot; <asp:Literal ID="litDate" runat="server"></asp:Literal></p>
            </div>
        </header>

        <div class="container mt-5">

            <%-- Score summary --%>
            <section class="surface-card result-summary" aria-labelledby="score-heading">
                <div id="divRing" runat="server" class="score-ring" role="img">
                    <div class="score-ring-inner">
                        <strong><asp:Literal ID="litPercentRing" runat="server"></asp:Literal>%</strong>
                    </div>
                </div>

                <div class="result-details">
                    <h2 id="score-heading" class="h5 text-muted mb-1">Your score</h2>
                    <p class="result-score"><asp:Literal ID="litScore" runat="server"></asp:Literal> / <asp:Literal ID="litTotal" runat="server"></asp:Literal></p>

                    <dl class="result-counts">
                        <div><dt>Percentage</dt><dd><asp:Literal ID="litPercentage" runat="server"></asp:Literal>%</dd></div>
                        <asp:PlaceHolder ID="phCounts" runat="server">
                            <div><dt>Correct answers</dt><dd class="text-success"><asp:Literal ID="litCorrect" runat="server"></asp:Literal></dd></div>
                            <div><dt>Wrong answers</dt><dd class="text-danger"><asp:Literal ID="litWrong" runat="server"></asp:Literal></dd></div>
                            <div><dt>Left blank</dt><dd><asp:Literal ID="litBlank" runat="server"></asp:Literal></dd></div>
                        </asp:PlaceHolder>
                    </dl>

                    <div class="d-flex flex-wrap gap-2">
                        <asp:PlaceHolder ID="phReviewButton" runat="server">
                            <a id="lnkReview" runat="server" class="btn btn-primary" href="~/Student/QuizResult">Review answers</a>
                        </asp:PlaceHolder>
                        <a id="lnkRetake" runat="server" class="btn btn-outline-primary" href="~/Student/Quizzes">Try again</a>
                        <a class="btn btn-outline-primary" runat="server" href="~/Student/Quizzes">Back to quizzes</a>
                    </div>
                </div>
            </section>

            <%-- Answer review (shown when "Review answers" is clicked) --%>
            <asp:Panel ID="pnlReview" runat="server" Visible="false" CssClass="mt-5">
                <h2 id="review" class="h4 mb-3">Review your answers</h2>

                <asp:Repeater ID="rptReview" runat="server" OnItemDataBound="rptReview_ItemDataBound">
                    <ItemTemplate>
                        <article class="quiz-question review-question">
                            <div class="review-head">
                                <span class="q-number">Question <%# Container.ItemIndex + 1 %></span>
                                <span class='<%# ResultBadgeClass(Eval("IsCorrect"), Eval("ChosenOption")) %>'><%# ResultBadgeText(Eval("IsCorrect"), Eval("ChosenOption")) %></span>
                                <span class="q-marks"><%#: Eval("MarksAwarded") %> / <%#: Eval("Marks") %></span>
                            </div>
                            <p class="q-text"><%#: Eval("QuestionText") %></p>

                            <ul class="quiz-options review-options">
                                <asp:Repeater ID="rptReviewOptions" runat="server">
                                    <ItemTemplate>
                                        <li class='<%# OptionClass(Eval("OptionLetter")) %>'>
                                            <span class="option-letter"><%#: Eval("OptionLetter") %></span>
                                            <span class="option-text"><%#: Eval("OptionText") %></span>
                                            <%# OptionTags(Eval("OptionLetter")) %>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
