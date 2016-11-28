<%@ Page Language="c#" EnableViewState="true" CodeBehind="NewRecipientList.aspx.cs" AutoEventWireup="True" Inherits="BVNetwork.EPiSendMail.Plugin.NewRecipientList" %>
<%@ Register TagPrefix="EPiServerShell" Namespace="EPiServer.Shell.Web.UI.WebControls" Assembly="EPiServer.Shell" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="StatusMessage" Src="../StatusMessage.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" ContentPlaceHolderID="FullRegion" >
    <EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/lists" Area="Newsletter" />

    <div class="container newsletter">
        <div class="col-lg-12">
            <h1>Create Recipient List</h1>        
            <EPiSendMail:statusmessage runat="server" id="ucStatusMessage" />
            <asp:Panel ID="Panel1" runat="server" DefaultButton="cmdSaveNewRecipientList" EnableViewState="True">
                Please give the recipient list a name and a description.
                <br />
                <br />
                Name: 
                <br />
                <asp:TextBox ID="txtRecipientListName" Width="50%" runat="server" />
                <br />
                Description:
                <br />
                <asp:TextBox ID="txtRecipientListDesc" TextMode="MultiLine" Rows="5" Width="50%"
                    runat="server" />
                <div style="margin-top: 0.5em;">
                    Type:&nbsp;
                    <asp:DropDownList ID="dropListRecipientTypes" Width="30%" runat="server" />
                </div>
                <br />
                <asp:Button ID="cmdSaveNewRecipientList" OnClick="cmdSaveNewRecipientList_ClickHandler"
                    CssClass="btn btn-primary" Style="margin-top: 1em;" runat="server" Text="Create Recipient List" />
            </asp:Panel>
        </div>
    </div>
</asp:content>