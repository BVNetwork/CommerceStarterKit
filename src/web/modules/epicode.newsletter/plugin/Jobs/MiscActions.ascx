<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MiscActions.ascx.cs" 
            Inherits="BVNetwork.EPiSendMail.Plugin.MiscActions" %>

<h3>Actions</h3>
Miscellaneous actions to perform on the newsletter.
<ul class="listAction">
    <li><asp:HyperLink ID="lnkWorkItems" Target="_blank" Text="See all Recipients" runat="server"/></li>
    <li><asp:LinkButton Text="Reset all Recipients Statuses" runat="server"
                        CommandName="ResetWorkItemStatus" OnCommand="cmdActionClickHandler" /></li>
    <li><asp:LinkButton Text="Remove all Recipients" runat="server" 
                        CommandName="RemoveAllWorkItems" OnCommand="cmdActionClickHandler" /></li>
</ul>
