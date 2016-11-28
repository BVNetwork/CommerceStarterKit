<%@ Page Language="c#" CodeBehind="JobEditStandAlone.aspx.cs" AutoEventWireup="true" 
    Inherits="BVNetwork.EPiSendMail.Plugin.JobEditStandAlone" %>
<%@ Register TagPrefix="EPiServerShell" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>
<%@ Register TagPrefix="EPiSendMail" TagName="JobEdit" Src="JobEdit.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" ContentPlaceHolderID="FullRegion" >
    <EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/newsletters" Area="Newsletter" />
    <EPiSendMail:JobEdit ClientIDMode="Static" runat="server" />
</asp:content>
