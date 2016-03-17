<%@ Page Language="c#" EnableViewState="true" CodeBehind="ListEdit.aspx.cs" AutoEventWireup="True" Inherits="BVNetwork.EPiSendMail.Plugin.RecipientListEdit" %>
<%@ Import Namespace="BVNetwork.EPiSendMail.Configuration" %>

<%@ Register TagPrefix="EPiSendMail" TagName="StatusMessage" Src="../StatusMessage.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="AddRecipients" Src="../RecipientLists/AddRecipients.ascx" %>

<%@ Register TagPrefix="EPiServerShell" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>

<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
    <script type="text/javascript">
        var statusCountId = '#<%= lblCountOfEmails.ClientID %>';
        var listId = <%= RecipientListId %>;
        function updateListInfo() {
            $.ajax({
                dataType: "json",
                type: "GET",
                url: '<%= NewsLetterConfiguration.GetModuleBaseDir() + "/api/recipientlist/get/" %>' + listId,
                success: function (data) {
                    $(statusCountId).html(data.EmailAddressCount);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $(statusCountId).html('Error');
                }
            });
        }
    </script>
</asp:content>
<asp:content runat="server" contentplaceholderid="FullRegion">
    <EPiServerShell:ShellMenu ID="ShellMenu1" runat="server" SelectionPath="/global/newsletter/lists" Area="Newsletter" />

    <div class="container newsletter">
        <div class="col-lg-12">
            <h1 id="h2ListName" runat="server" class="page-header"></h1>
            <div class="col-lg-9">
                <asp:Label ID="lblListDescription" runat="server" />
                <%-- This table is shown by default, --%>

                <EPiSendMail:statusmessage runat="server" id="ucStatusMessage" />

                <%-- Add more recipiants from different sources --%>
                <asp:Panel ID="pnlAddEmailAddressesToList" runat="server">
                    <EPiSendMail:AddRecipients runat="server" id="ucAddRecipients" />
                </asp:Panel>
            </div>
            <div class="col-lg-3 well" style="padding-top: 1em;">
                <%-- Status of list --%>
                <p>
                Number of addresses: <span class="badge">
                <asp:Label ID="lblCountOfEmails" ClientIDMode="Predictable" Font-Bold="true" runat="server" /></span>
                </p>    
                <b>Actions</b>
                <%-- This link will get it's href from code --%>
                <ul class="listAction">
                    <li>
                        <a style="text-decoration: underline;" runat="server" id="lnkEditItems">See all Email Addresses</a>
                    </li>
                    <li>
                        <asp:LinkButton Style="text-decoration: underline;" ID="lnkRemoveList" OnClientClick="javascript:return confirm('Are you sure you want to delete the list?')" OnClick="lnkRemoveList_Click" runat="server" Text="Delete This List" />
                    </li>
                    <li>
                        <%-- Remove all email addresses from a list --%>
                        <asp:LinkButton Style="text-decoration: underline;" ID="lnkRemoveContents"
                            OnClientClick="javascript:return confirm('Are you sure you want to delete all email addresses in the list?');"
                            OnClick="lnkRemoveContents_Click" runat="server" Text="Delete all Email Addresses" />
                   </li>
                </ul>
            </div>
        </div>
    </div>
</asp:content>
