<%@ Page Title="Manage Resources" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageResources.aspx.cs" Inherits="UniSkillHub.Admin.ManageResources" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header class="page-header">
        <div class="container">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3">
                <div>
                    <h1>Manage resources</h1>
                    <p>Upload and organise the notes, videos, audio and links students can study from.</p>
                </div>
                <asp:Button ID="btnAdd" runat="server" Text="Add new resource" CssClass="btn btn-primary btn-lg" CausesValidation="false" OnClick="btnAdd_Click" />
            </div>
        </div>
    </header>

    <div class="container mt-4">

        <asp:Label ID="lblMessage" runat="server" Visible="false" role="status"></asp:Label>

        <%-- ADD / EDIT FORM --%>
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="surface-card mb-4">
            <h2 class="h4 mb-4"><asp:Label ID="lblFormTitle" runat="server" Text="Add resource"></asp:Label></h2>

            <asp:ValidationSummary ID="vsResource" runat="server" ValidationGroup="ResourceForm" CssClass="alert alert-danger"
                HeaderText="Please fix the following:" DisplayMode="BulletList" />

            <div class="row g-3">
                <div class="col-md-8">
                    <asp:Label runat="server" AssociatedControlID="txtTitle" CssClass="form-label" Text="Title" />
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="150" ValidateRequestMode="Disabled" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" ValidationGroup="ResourceForm"
                        ErrorMessage="Title is required." Text="Title is required." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-md-4">
                    <asp:Label runat="server" AssociatedControlID="ddlCategory" CssClass="form-label" Text="Category" />
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>

                <div class="col-12">
                    <asp:Label runat="server" AssociatedControlID="txtDescription" CssClass="form-label" Text="Description" />
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled" />
                    <div class="form-text">Up to 1000 characters. For an Article, this text is the article itself.</div>
                    <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription" ValidationGroup="ResourceForm"
                        ErrorMessage="Description is required." Text="Description is required." Display="Dynamic" CssClass="field-error" />
                    <asp:RegularExpressionValidator ID="revDescription" runat="server" ControlToValidate="txtDescription" ValidationGroup="ResourceForm"
                        ValidationExpression="^[\s\S]{1,1000}$"
                        ErrorMessage="Description must be 1000 characters or fewer." Text="Description must be 1000 characters or fewer." Display="Dynamic" CssClass="field-error" />
                </div>

                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="ddlResourceType" CssClass="form-label" Text="Resource type" />
                    <asp:DropDownList ID="ddlResourceType" runat="server" CssClass="form-select js-resource-type">
                        <asp:ListItem Value="PDF" Text="PDF / document"></asp:ListItem>
                        <asp:ListItem Value="Video" Text="Video"></asp:ListItem>
                        <asp:ListItem Value="Audio" Text="Audio"></asp:ListItem>
                        <asp:ListItem Value="Article" Text="Article (text only)"></asp:ListItem>
                        <asp:ListItem Value="Link" Text="External link"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-6">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Value="Published" Text="Published (visible to students)"></asp:ListItem>
                        <asp:ListItem Value="Draft" Text="Draft (hidden)"></asp:ListItem>
                    </asp:DropDownList>
                </div>

                <%-- The fields below are shown/hidden by Scripts/admin.js depending on the type --%>
                <div class="col-12" data-for-types="PDF">
                    <asp:Label runat="server" AssociatedControlID="fuDocument" CssClass="form-label" Text="Document file" />
                    <asp:FileUpload ID="fuDocument" runat="server" CssClass="form-control" data-file-kind="document"
                        accept=".pdf,.doc,.docx,.ppt,.pptx,.xls,.xlsx,.zip,.txt" />
                    <div class="form-text">PDF, Word, PowerPoint, Excel, ZIP or TXT &middot; maximum 20 MB.</div>
                    <asp:Label ID="lblCurrentDocument" runat="server" CssClass="form-text d-block fw-semibold"></asp:Label>
                    <asp:CustomValidator ID="cvDocument" runat="server" ValidationGroup="ResourceForm" ClientValidationFunction="validateDocumentFile"
                        OnServerValidate="cvDocument_ServerValidate" ErrorMessage="Invalid document file." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12" data-for-types="Video">
                    <asp:Label runat="server" AssociatedControlID="fuVideo" CssClass="form-label" Text="Video file" />
                    <asp:FileUpload ID="fuVideo" runat="server" CssClass="form-control" data-file-kind="video" accept=".mp4,.webm,.ogv" />
                    <div class="form-text">MP4, WebM or OGV &middot; maximum 40 MB.</div>
                    <asp:Label ID="lblCurrentVideo" runat="server" CssClass="form-text d-block fw-semibold"></asp:Label>
                    <asp:CustomValidator ID="cvVideo" runat="server" ValidationGroup="ResourceForm" ClientValidationFunction="validateVideoFile"
                        OnServerValidate="cvVideo_ServerValidate" ErrorMessage="Invalid video file." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12" data-for-types="Audio">
                    <asp:Label runat="server" AssociatedControlID="fuAudio" CssClass="form-label" Text="Audio file" />
                    <asp:FileUpload ID="fuAudio" runat="server" CssClass="form-control" data-file-kind="audio" accept=".mp3,.ogg,.wav" />
                    <div class="form-text">MP3, OGG or WAV &middot; maximum 20 MB.</div>
                    <asp:Label ID="lblCurrentAudio" runat="server" CssClass="form-text d-block fw-semibold"></asp:Label>
                    <asp:CustomValidator ID="cvAudio" runat="server" ValidationGroup="ResourceForm" ClientValidationFunction="validateAudioFile"
                        OnServerValidate="cvAudio_ServerValidate" ErrorMessage="Invalid audio file." Display="Dynamic" CssClass="field-error" />
                </div>
                <div class="col-12" data-for-types="Link">
                    <asp:Label runat="server" AssociatedControlID="txtExternalUrl" CssClass="form-label" Text="Link address" />
                    <asp:TextBox ID="txtExternalUrl" runat="server" CssClass="form-control" TextMode="Url" MaxLength="500" ValidateRequestMode="Disabled" placeholder="https://..." />
                    <asp:CustomValidator ID="cvUrl" runat="server" ValidationGroup="ResourceForm" OnServerValidate="cvUrl_ServerValidate"
                        ErrorMessage="Enter a full web address starting with http:// or https://" Display="Dynamic" CssClass="field-error" />
                </div>
            </div>

            <div class="d-flex flex-wrap gap-2 mt-4">
                <asp:Button ID="btnSave" runat="server" Text="Save resource" CssClass="btn btn-primary" ValidationGroup="ResourceForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-outline-primary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </asp:Panel>

        <%-- SEARCH + FILTERS --%>
        <section class="filter-bar" aria-label="Search and filter resources">
            <div class="row g-3 align-items-end">
                <div class="col-lg-3">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label" Text="Search" />
                    <asp:TextBox ID="txtSearch" runat="server" TextMode="Search" CssClass="form-control" MaxLength="100" placeholder="Title or description..." />
                </div>
                <div class="col-sm-4 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlCategoryFilter" CssClass="form-label" Text="Category" />
                    <asp:DropDownList ID="ddlCategoryFilter" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>
                <div class="col-sm-4 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlTypeFilter" CssClass="form-label" Text="Type" />
                    <asp:DropDownList ID="ddlTypeFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All types"></asp:ListItem>
                        <asp:ListItem Value="PDF" Text="PDF"></asp:ListItem>
                        <asp:ListItem Value="Video" Text="Video"></asp:ListItem>
                        <asp:ListItem Value="Audio" Text="Audio"></asp:ListItem>
                        <asp:ListItem Value="Article" Text="Article"></asp:ListItem>
                        <asp:ListItem Value="Link" Text="Link"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-sm-4 col-lg-2">
                    <asp:Label runat="server" AssociatedControlID="ddlStatusFilter" CssClass="form-label" Text="Status" />
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" Text="All statuses"></asp:ListItem>
                        <asp:ListItem Value="Published" Text="Published"></asp:ListItem>
                        <asp:ListItem Value="Draft" Text="Draft"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-lg-3 d-flex gap-2">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary flex-fill" CausesValidation="false" OnClick="btnSearch_Click" />
                    <a class="btn btn-outline-primary" runat="server" href="~/Admin/ManageResources">Reset</a>
                </div>
            </div>
        </section>

        <p class="result-count" role="status"><asp:Label ID="lblSummary" runat="server"></asp:Label></p>

        <%-- RESOURCES GRID --%>
        <div class="table-responsive data-table-wrap">
            <asp:GridView ID="gvResources" runat="server" AutoGenerateColumns="false" GridLines="None" DataKeyNames="ResourceID"
                CssClass="table table-hover align-middle data-table mb-0" UseAccessibleHeader="true"
                OnRowCommand="gvResources_RowCommand" EmptyDataText="No resources match your search.">
                <EmptyDataRowStyle CssClass="text-center text-muted py-4" />
                <Columns>
                    <asp:BoundField DataField="ResourceID" HeaderText="ID" />
                    <asp:TemplateField HeaderText="Title">
                        <ItemTemplate>
                            <strong><%#: Eval("Title") %></strong>
                            <div class="dash-meta"><%#: Eval("CategoryName") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Type">
                        <ItemTemplate><span class='<%# TypeClass(Eval("ResourceType")) %>'><%#: Eval("ResourceType") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class='<%# StatusClass(Eval("Status")) %>'><%#: Eval("Status") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="DateCreated" HeaderText="Created" DataFormatString="{0:dd MMM yyyy}" HtmlEncode="false" />
                    <asp:TemplateField>
                        <ItemStyle CssClass="text-end text-nowrap" />
                        <ItemTemplate>
                            <a class="btn btn-sm btn-outline-primary" href='<%# ViewUrl(Eval("ResourceID")) %>' target="_blank" rel="noopener">View</a>
                            <asp:LinkButton runat="server" Text="Edit" CssClass="btn btn-sm btn-outline-primary"
                                CommandName="EditResource" CommandArgument='<%# Eval("ResourceID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text='<%# Eval("Status").ToString() == "Published" ? "Unpublish" : "Publish" %>' CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="TogglePublish" CommandArgument='<%# Eval("ResourceID") %>' CausesValidation="false" />
                            <asp:LinkButton runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="DeleteResource" CommandArgument='<%# Eval("ResourceID") %>' CausesValidation="false"
                                OnClientClick="return confirm('Delete this resource and its uploaded file? This cannot be undone.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Literal ID="litPager" runat="server"></asp:Literal>
    </div>

    <script src="<%= ResolveUrl("~/Scripts/admin.js") %>"></script>
    <script src="<%= ResolveUrl("~/Scripts/validation.js") %>"></script>
</asp:Content>
