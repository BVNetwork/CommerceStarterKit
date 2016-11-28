<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="WorkItemsEdit.aspx.cs"
    EnableViewState="true"
    Inherits="BVNetwork.EPiSendMail.Plugin.WorkItemsEdit" %>
<%@ Register TagPrefix="EPiServerShell" Namespace="EPiServer.Shell.Web.UI.WebControls" Assembly="EPiServer.Shell" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" ContentPlaceHolderID="FullRegion" >
    <EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/newsletters" Area="Newsletter" />

    <div class="newsletter">
        <asp:panel CssClass="container" runat="server" defaultbutton="cmdSearchFor" defaultfocus="txtSearchFor">
            <div class="row">
                <div class="col-md-12">                
                    <h1 style="margin-bottom: 0.5em;">Newsletter Recipients</h1>
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h3 class="panel-title"><asp:Label ID="lblJobName" runat="server" /></h3>
                        </div>
                        <div class="panel-body">
                            Number of recipients: 
                            <span class="badge">
                                <asp:label id="lblWorkItemCount" runat="server" />
                            </span>
                            <br />
                            <asp:Label ID="lblDescription" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:Panel ID="pnlSearchFor" runat="server" style="padding: 0.5em; margin: 0.5em;">
                        Search for email addresses:
                        <asp:TextBox ID="txtSearchFor" runat="server" />
                        <asp:Button CssClass="btn btn-primary" ID="cmdSearchFor" runat="server" Text="Search" OnClick="cmdSearchFor_Click" />
                        <br />
                        Or: <asp:LinkButton ID="lnkShowError" runat="server" Text="Show Errors" OnClick="lnkShowError_Click" />
                        | <asp:LinkButton ID="lnkShowNotSent" runat="server" Text="Show Not Sent" OnClick="lnkShowNotSent_Click" />
                        | <asp:LinkButton ID="lnkShowSent" runat="server" Text="Show Sent" OnClick="lnkShowSent_Click" />
                        | <asp:LinkButton ID="lnkShowSending" runat="server" Text="Show Sending" OnClick="lnkShowSending_Click" />
                        | <asp:LinkButton ID="lnkShowAll" runat="server" Text="Show All" OnClick="lnkShowAll_Click" /> (Note! This can take a long time)
                    </asp:Panel>
            
                    <asp:Panel ID="pnlWorkItems" runat="Server">
                        <asp:Panel ID="pnlGridCount" runat="server" Visible="false" Style="margin: 1em 0em 1em 0em;">
                            Showing <asp:Literal ID="lblGridCount" runat="server" /> email addresses.
                        </asp:Panel>
                        <asp:DataGrid
                            ID="grdWorkItems" 
                            CssClass="table table-striped table-bordered"
                            OnItemCreated="grdWorkItemsItemCreatedHandler"
                            OnItemDataBound="grdWorkItemsItemDataBoundHandler" OnItemCommand="grdWorkItemsItemCommandHandler"
                            runat="server" CellPadding="4" AutoGenerateColumns="false">
                            <Columns>
                                <asp:BoundColumn DataField="Status" HeaderText="Status" HeaderStyle-Font-Bold="true" />
                                <asp:BoundColumn DataField="EmailAddress" HeaderText="Email Address" HeaderStyle-Font-Bold="true" />
                                <asp:BoundColumn DataField="Info" HeaderText="Information" HeaderStyle-Font-Bold="true" />
                                <asp:ButtonColumn ButtonType="LinkButton" CommandName="Delete" CausesValidation="false"
                                    Text="Delete" />
                            </Columns>
                        </asp:DataGrid>
                    </asp:Panel>
                </div>
            </div>
        </asp:panel>
    </div>

</asp:content>
