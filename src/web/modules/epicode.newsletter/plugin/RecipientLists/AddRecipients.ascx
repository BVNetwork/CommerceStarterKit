<%@ Control Language="C#" AutoEventWireup="true" 
            CodeBehind="AddRecipients.ascx.cs" 
            Inherits="BVNetwork.EPiSendMail.Plugin.AddRecipients" %>

<h2>Add Recipients</h2>
Please choose a provider for importing recipients:
<br />
<asp:Panel ID="pnlImportProviders" runat="server" style="padding-left: 1.5em;">
    <asp:Repeater runat="server" ID="lstRecipientProviders2">
        <HeaderTemplate>
            <ul class="listAction" style="list-style-image: url(<%= BVNetwork.EPiSendMail.Configuration.NewsLetterConfiguration.GetModuleBaseDir() %>/content/images/bluearrowright.gif);">
        </HeaderTemplate>
        <ItemTemplate>
            <li><asp:LinkButton Text='<%# Eval("Text") %>' CommandName='<%# Eval("ProviderName") %>' OnCommand="cmdImportRecipientItems_Click" runat="server"/></li>
        </ItemTemplate>
        <FooterTemplate></ul></FooterTemplate>
    </asp:Repeater>
</asp:Panel>

<%-- Panel to hold the dynamically loaded user controls --%>

<asp:PlaceHolder id="pnlProviderUiContainer" runat="server" >
        <asp:Panel ID="pnlProviderContainer" runat="Server">
            <%-- Provider will be shown here --%>
        </asp:Panel>
</asp:PlaceHolder>
