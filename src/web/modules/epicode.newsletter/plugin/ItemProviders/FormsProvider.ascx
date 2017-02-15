<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FormsProvider.ascx.cs" 
    Inherits="BVNetwork.EPiSendMail.FormsProvider.Plugin.ItemProviders.FormsProvider" %>
<asp:Panel ID="Panel1" runat="server" DefaultButton="cmdCustomerViewEmailAddresses">
    <b>Select Form</b>
    <br />
    <asp:DropDownList runat="server" ID="dropListUserViews" enableviewstate="true" style="margin-top: 0.7em;"/>
    <br />
    <asp:Button id="cmdCustomerViewEmailAddresses" runat="server" 
                Text="Add" style="margin-top: 1em;"
                OnClick="cmdCustomerViewEmailAddresses_Click" />
</asp:Panel>