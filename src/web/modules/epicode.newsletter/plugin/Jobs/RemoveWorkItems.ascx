<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoveWorkItems.ascx.cs" 
            Inherits="BVNetwork.EPiSendMail.Plugin.RemoveWorkItems" %>
<h2>Remove Recipients</h2>
    Clean your list for email addresses that is not valid, or where the recipient has opted out.
<br />
<asp:Panel ID="pnlProviders" runat="server">
    <ul class="listAction">
        <asp:Repeater DataSource=<%# WorkItemProviders %> ID="rptWorkProviders" runat="server">
            <ItemTemplate>
                <li><asp:LinkButton Text='<%# Eval("Text") %>' 
                                    CommandName='<%# Eval("ProviderName") %>' 
                                    OnCommand="cmdWorkItems_Click" runat="server"/></li>
            </ItemTemplate>
        </asp:Repeater>
    </ul>
</asp:Panel>

<asp:PlaceHolder id="pnlProviderUiContainer" runat="server" >
    <div style="margin-left: 0.5em; padding-bottom: 0.5em; padding-right: 0.5em; padding-top:0em;">
        <asp:Panel ID="pnlProviderContainer" runat="Server" CssClass="well">
            <%-- Provider will be shown here --%>
        </asp:Panel>
    </div>
</asp:PlaceHolder>


   