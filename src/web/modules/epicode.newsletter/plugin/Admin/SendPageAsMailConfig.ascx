<%@ Control Language="c#" AutoEventWireup="false" Codebehind="SendPageAsMailConfig.ascx.cs" Inherits="BVNetwork.EPiSendMail.Plugin.SendPageAsMailConfig" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%--
<%@ Register TagPrefix="EPiMail" TagName="SelectUsers" Src="~/modules/bvn/SendMail/Units/SelectUsers.ascx" %>
--%>
<style>
	.textbox {
		width: 300px;
	}
</style>
<br />

<b>Master settings:</b>
<table cellpadding="2" cellspacing="0" border="0">
	<tr>
		<td><asp:checkbox runat="server" id="EPfEnableSendMailPlugIn" text="Enable the Mail Plug-in" /></td>
	</tr>
	<tr>
		<td style="padding-left: 10px; color: gray;">
			Master switch for the newsletter plug-in. The plug-in will not be shown if this checkbox is unticked.
		</td>
	</tr>
	
</table>
<br />

<b>Configuration settings:</b>
<table cellpadding="2" cellspacing="0" border="0" width="500">
	<tr>
		<td nowrap="true" width="190">Default From Address:</td>
		<td><asp:textbox runat="server" id="EPsSendMailFromAddress" cssclass="textbox" /></td>
	</tr>


	<tr>
		<td>Default Mail Subject:</td>
		<td><asp:textbox runat="server" id="EPsSendMailSubject" cssclass="textbox" /></td>
	</tr>
	

	<tr>
		<td>Enable plug-in for the following page types:</td>
		<td><asp:textbox runat="server" id="EPsSendMailPageType" cssclass="textbox" /></td>
	</tr>
    <tr>
		<td colspan="2" style="padding-left: 10px; color: gray;">
			By default the page plug-in will only be shown for pages of the type "Newsletter".
			If you want to show the plug-in for more page types, add a comma separated list of the page type id´s in the textbox above.
			Remember to include the id of the "Newsletter" page type. 
		</td>
	</tr>
	
	<tr>
		<td>Newsletter tab ID:</td>
		<td><asp:textbox runat="server" id="EPnSendMailEditPanelTabId" cssclass="textbox" /></td>
	</tr>

	<!-- Email templates -->
	<tr>
		<td>Location of email templates:</td>
		<td><asp:textbox runat="server" id="EPsSendMailTemplateDir" cssclass="textbox" /></td>
	</tr>
	<tr>
		<td colspan="2" style="padding-left: 10px; color: gray;">
			Location of email templates used by the system. The admin report is
			located here. If you want to change any of the built-in templates you
			should copy the directory out of the bvn directory structure and change
			this setting.
		</td>
	</tr>
	
	
	<tr>
		<td>Mail sender type:</td>
		<td><asp:textbox runat="server" id="EPsSendMailSenderType" cssclass="textbox" /></td>
	</tr>
	<tr>
		<td colspan="2" style="padding-left: 10px; color: gray;">
			A class inheriting from MailSenderBase, responsible for sending the email.
			The MailSenderNetSmtp type is default. You can also use MailSenderAdvIntellect
			from the same namespace:
			<br />
			<br />
			Built-in:
			<br />
			&nbsp;BVNetwork.EPiSendMail.Library.MailSenderNetSmtp, BVNetwork.EPiSendMail
			<br />
			&nbsp;BVNetwork.EPiSendMail.Library.MailSenderAdvIntellect, BVNetwork.EPiSendMail
			<br />
			<br />
			<b>Note!</b> The MailSenderAdvIntellect requires a license for the aspNET component.
		</td>
	</tr>
</table>

