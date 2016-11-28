<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecipientListDeleted.aspx.cs" 
         Inherits="BVNetwork.EPiSendMail.Plugin.RecipientListDeleted" %>
<%@ Register TagPrefix="EPiServerShell" Namespace="EPiServer.Shell.Web.UI.WebControls" Assembly="EPiServer.Shell" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" ContentPlaceHolderID="FullRegion" >
    <EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/lists" Area="Newsletter" />
    <div class="epi-contentContainer epi-padding newsletter">
        <h1>The Recipient List has been deleted</h1>
    </div>
</asp:content>