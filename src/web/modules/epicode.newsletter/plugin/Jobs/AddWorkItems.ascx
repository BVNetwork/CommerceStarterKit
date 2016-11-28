<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddWorkItems.ascx.cs" 
            Inherits="BVNetwork.EPiSendMail.Plugin.AddWorkItems" %>
<h2>Add Recipients</h2>

<%-- 
    Note! This list will be made into a dynamic one,
    using some sort of plug-in method to populate. If
    you want to add your own, please contact steve@episerver.no
    first, to get a status update, and some pointers
    about how to develop this.
--%>
Please choose a provider for importing email addresses:
<br />
<asp:Panel ID="pnlImportProviders" runat="server">
    <ul class="listAction">
        <asp:Repeater DataSource=<%# WorkItemProviders %> ID="rptWorkProviders" runat="server">
            <ItemTemplate>
                <li><asp:LinkButton Text='<%# Eval("Text") %>' 
                                    CommandName='<%# Eval("ProviderName") %>' 
                                    OnCommand="cmdImportWorkItems_Click" runat="server"/></li>
            </ItemTemplate>
        </asp:Repeater>
    </ul>
</asp:Panel>

<div id="pnlProviderUiContainer" runat="server" 
              visible="false">
    
        <asp:Panel ID="pnlProviderContainer" runat="Server" CssClass="well">
            <%-- Provider will be shown here --%>
        </asp:Panel>
   
</div>
