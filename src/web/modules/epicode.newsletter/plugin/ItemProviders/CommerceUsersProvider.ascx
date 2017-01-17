<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CommerceUsersProvider.ascx.cs" 
    Inherits="BVNetwork.EPiSendMail.CommerceProvider.Plugin.ItemProviders.CommerceUsersProvider" %>
<asp:Panel ID="Panel1" runat="server" DefaultButton="cmdCustomerViewEmailAddresses">
    <b>Select Commerce users</b>
    <br />
    <asp:DropDownList runat="server" ID="dropListUserViews" enableviewstate="true" style="margin-top: 0.7em;"/>
    <br />
    <asp:Button id="cmdCustomerViewEmailAddresses" runat="server" 
                Text="Add" style="margin-top: 1em;"
                OnClick="cmdCustomerViewEmailAddresses_Click" />
</asp:Panel>