<%@ Page Title="Something went wrong" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="UniSkillHub.ErrorPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <div class="empty-state" role="alert">
            <h1 class="h3"><asp:Literal ID="litHeading" runat="server"></asp:Literal></h1>
            <p><asp:Literal ID="litMessage" runat="server"></asp:Literal></p>
            <div class="d-flex flex-wrap gap-2 justify-content-center">
                <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-outline-primary" NavigateUrl="javascript:history.back();" Visible="false">Go back</asp:HyperLink>
                <a class="btn btn-primary" runat="server" href="~/">Go to the home page</a>
            </div>
        </div>
    </div>
</asp:Content>
