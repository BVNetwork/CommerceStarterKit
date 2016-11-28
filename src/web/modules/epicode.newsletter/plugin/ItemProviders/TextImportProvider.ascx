<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextImportProvider.ascx.cs" 
	Inherits="BVNetwork.EPiSendMail.Plugin.ItemProviders.TextImportProvider" %>
<asp:Panel runat="server" DefaultButton="cmdAddCsvEmailAddresses">
    <!-- Add Work Items from csv text-->
    <b>Enter the email addresses you want to add:</b>
    <div class="import-email-address">
    <asp:TextBox Rows="10" Width="100%" runat="server" CssClass="form-control import-email-address" TextMode="MultiLine" ID="txtAddEmailWorkItems" enableviewstate="false" />
    </div>
    <asp:Button id="cmdAddCsvEmailAddresses" runat="server" 
                Text="Add Email Addresses" CssClass="btn btn-primary"
		        OnClick="cmdAddCsvEmailAddresses_Click" />
</asp:Panel>