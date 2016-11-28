<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="GettingStarted.aspx.cs"
    Inherits="BVNetwork.EPiSendMail.Plugin.GettingStarted" %>

<%@ Import Namespace="BVNetwork.EPiSendMail.DataAccess" %>
<%@ Register TagPrefix="EPiServerShell" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="PluginStyles.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="GettingStarted" Src="GettingStarted.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" contentplaceholderid="FullRegion">
	<EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/newsletters" Area="Newsletter" />
    <EPiSendMail:GettingStarted runat="server" />
</asp:content>
