<%@ Page Language="c#" CodeBehind="JobEdit.aspx.cs" AutoEventWireup="true" Inherits="BVNetwork.EPiSendMail.Plugin.NewsLetterJobEdit" %>
<%@ Register TagPrefix="EPiServerShell" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>
<%@ Register TagPrefix="EPiSendMail" TagName="JobEdit" Src="JobEdit.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" ContentPlaceHolderID="FullRegion" >
    <EPiSendMail:JobEdit ClientIDMode="Static" runat="server" />
</asp:content>
