<%@ Control Language="C#" AutoEventWireup="true" 
            CodeBehind="RecipientRemoveProvider.ascx.cs" 
            Inherits="BVNetwork.EPiSendMail.Plugin.WorkItemProviders.RecipientRemoveProvider" %>

<b>Select a Block List:</b>
<br />
<asp:Repeater ID="rptRemoveFromRecipientLists" runat="server" enableviewstate="false">
    <HeaderTemplate>
        <table class="table table-striped">
            <tr>
                <th>&nbsp;</th>
                <th>Name</th>
                <th>Type</th>
                <th>Count</th>
                <th>Description</th>
            </tr>
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td><asp:LinkButton ID="cmdSelecteRecipList" runat="server" 
                                Text="Clean" CommandArgument='<%# Eval("Id") %>' 
                                OnCommand="cmdSelect_Click"
                                /></td>
            <td><%# Eval("Name") %></td>
            <td><%# Eval("ListType") %></td>
            <td><span class="badge"><%# Eval("EmailAddressCount") %></span></td>
            <td><%# Eval("Description") %></td>
        </tr>        
    </ItemTemplate>
    <FooterTemplate>
        <asp:PlaceHolder runat="server" Visible='<%# ShowAll == false %>'>
        <tr>
            <td colspan="5" style="text-align: right;">
                [<asp:LinkButton ID="cmdShowAllRecipLists" runat="server" Text="Show all recipient lists" OnClick="cmdShowAll_Click" />]
            </td>
        </tr>
        </asp:PlaceHolder>
        </table>
    </FooterTemplate>
</asp:Repeater>