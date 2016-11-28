<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecipientItemsEdit.aspx.cs"
    Inherits="BVNetwork.EPiSendMail.Plugin.RecipientItemsEdit"
    EnableViewState="true" %>
<%@ Register TagPrefix="EPiServerShell" Namespace="EPiServer.Shell.Web.UI.WebControls" Assembly="EPiServer.Shell" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" ContentPlaceHolderID="FullRegion" >
    <EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/lists" Area="Newsletter" />

    <div class="container newsletter">
        <div class="row">
            <div class="col-md-12">
                <a style="color: #888;" href="ListEdit.aspx?recipientlistid=<%= RecipientList.Id.ToString() %>">Back to list</a>

                <h1 style="margin-bottom: 0.5em;">
                    <asp:label id="lblName" runat="server" />
                </h1>

                <div style="padding: 0.5em; margin: 0.5em; background-color: #F7F7FB;">
                    <asp:label id="lblDescription" runat="server" />
                    <br />
                    Number of email addresses: <span class="badge">
                        <asp:label id="lblItemCount" runat="server" />
                        </span>
                </div>

                <asp:panel id="pnlSearchFor" runat="server" style="padding: 0.5em; margin: 0.5em;">
                    Search for email addresses:
                    <asp:TextBox ID="txtSearchFor" runat="server" />
                    <asp:Button ID="cmdSearchFor" runat="server" Text="Search" OnClick="cmdSearchFor_Click" />
                    <br />Or, 
                    <asp:LinkButton ID="lnkShowAll" runat="server" Font-Bold="true" 
                                    style="text-decoration: underline;" 
                                    Text="you can show all recipients" OnClick="lnkShowAll_Click" /> 
                    <span style="color: #666;">(Note! This can take some time if the list is big)</span>
                </asp:panel>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12">

                <asp:datagrid cssclass="table table-striped table-bordered" id="grdItems" allowpaging="false" pagesize="20"
                    onitemcommand="grdWorkItemsItemCommandHandler"
                    runat="server" cellpadding="4" autogeneratecolumns="false">
                    <Columns>
                        <asp:BoundColumn DataField="Email" HeaderText="Email Address" HeaderStyle-Font-Bold="true" />
                        <asp:BoundColumn DataField="Added" HeaderText="Information" HeaderStyle-Font-Bold="true"/>
                        <asp:BoundColumn DataField="Source" HeaderText="Source"  HeaderStyle-Font-Bold="true" />
                        <asp:BoundColumn DataField="Comment" HeaderText="Comment"  HeaderStyle-Font-Bold="true" />
                        <asp:ButtonColumn ButtonType="LinkButton" CommandName="Delete" CausesValidation="false"
                            Text="Delete" />
                    </Columns>
                </asp:datagrid>
            </div>
        </div>
    </div>
</asp:content>