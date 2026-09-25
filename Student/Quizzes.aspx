<%@ Page Title="Quizzes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Quizzes.aspx.cs" Inherits="UniSkillHub.Student.Quizzes" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <h1>Quizzes</h1>
            <p>Test what you have learned. You can take each quiz as many times as you like.</p>
        </div>
    </header>

    <div class="container mt-4">
        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <div class="row g-4">
            <asp:Repeater ID="rptQuizzes" runat="server">
                <ItemTemplate>
                    <div class="col-md-6 col-lg-4">
                        <article class="resource-card">
                            <div class="resource-card-top">
                                <span class="type-badge type-article">Quiz</span>
                                <span class="resource-category"><%#: Eval("TimeLimit") %> min</span>
                            </div>
                            <h2 class="resource-title"><%#: Eval("Title") %></h2>
                            <p class="resource-snippet"><%#: Eval("Description") %></p>

                            <ul class="quiz-facts">
                                <li><%#: Eval("QuestionCount") %> questions</li>
                                <li><%#: Eval("TotalMarks") %> marks</li>
                            </ul>

                            <%-- My progress on this quiz --%>
                            <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("Attempts") > 0 %>'>
                                <p class="quiz-progress-note">
                                    Best score: <strong><%#: Eval("BestPercentage", "{0:0.#}") %>%</strong>
                                    &middot; <%# AttemptsText(Eval("Attempts")) %>
                                </p>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("Attempts") == 0 %>'>
                                <p class="quiz-progress-note">Not attempted yet</p>
                            </asp:PlaceHolder>

                            <div class="resource-card-foot">
                                <asp:PlaceHolder runat="server" Visible='<%# (int)Eval("Attempts") > 0 %>'>
                                    <a class="btn btn-sm btn-outline-primary" href='<%# ResultUrl(Eval("LastAttemptID")) %>'>Last result</a>
                                </asp:PlaceHolder>
                                <a class="btn btn-sm btn-primary ms-auto" href='<%# StartUrl(Eval("QuizID")) %>'><%# (int)Eval("Attempts") > 0 ? "Retake quiz" : "Start quiz" %></a>
                            </div>
                        </article>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="empty-state">
            <h2>No quizzes available</h2>
            <p>Your lecturers have not published any quizzes yet. Check back soon.</p>
        </asp:Panel>
    </div>
</asp:Content>
