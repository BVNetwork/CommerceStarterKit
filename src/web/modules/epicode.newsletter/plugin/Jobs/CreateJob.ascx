<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreateJob.ascx.cs" Inherits="BVNetwork.EPiSendMail.Plugin.CreateJob" %>
<h1>Create Newsletter</h1>
<asp:Panel runat="server" DefaultButton="cmdSaveNewNewsletter">
	<small>Please give the newsletter a name and description. This is used in the list of newsletters and will help you
    identify the newsletter later.
	</small>
	<div style="padding-bottom: 1em;">
	Name:
    <br />
	<asp:TextBox ID="txtNewNewsletterName" Width="50%" runat="server" />
	</div>
	<div style="padding-bottom: 1em;">
		Description:
    <br />
		<asp:TextBox ID="txtNewNewsletterDesc" TextMode="MultiLine" Rows="5" Width="100%"
			runat="server" />
	</div>
	<div>
		<asp:Button ID="cmdSaveNewNewsletter" runat="server" Text="Create Newsletter" CssClass="btn btn-primary" />
	</div>
</asp:Panel>
