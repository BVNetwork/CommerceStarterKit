<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextRemoveProvider.ascx.cs" 
            Inherits="BVNetwork.EPiSendMail.Plugin.WorkItemProviders.TextRemoveProvider" %>
<asp:Panel runat="server" DefaultButton="cmdRemoveCsvEmailAddresses">
    <!-- Add Work Items from csv text-->
    <b>Enter the email addresses you want to remove:</b>
    <div class="import-email-address">
    <asp:TextBox Rows="10" Width="100%" runat="server" CssClass="form-control" TextMode="MultiLine" ID="txtRemoveEmailWorkItems" enableviewstate="false" />
    </div>
    <asp:Button id="cmdRemoveCsvEmailAddresses" runat="server" 
                Text="Remove E-mail Addresses" CssClass="btn btn-danger"
                OnClick="cmdRemoveCsvEmailAddresses_Click" />
</asp:Panel>