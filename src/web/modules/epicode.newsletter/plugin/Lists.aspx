<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Lists.aspx.cs"
    Inherits="BVNetwork.EPiSendMail.Plugin.Lists" %>

<%@ Import Namespace="BVNetwork.EPiSendMail.DataAccess" %>
<%@ Import Namespace="BVNetwork.EPiSendMail.Configuration" %>
<%@ Register TagPrefix="EPiServerShell" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
    <script type="text/javascript">
        function deleteList(listId, rowId) {
            if (confirm("Are you sure you want to delete this list and all it's recipients?")) {
                var script = '<%= NewsLetterConfiguration.GetModuleBaseDir("/api/recipientlist/delete?id=") %>' + listId;
                $.ajax({
                    url: script,
                    type: 'DELETE',
                    success: function (result) {
                        $("#" + rowId).remove();
                    }
                });

            }
            return false;
        }
    </script>
</asp:content>
<asp:content runat="server" ContentPlaceHolderID="FullRegion" >
    <EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/lists" Area="Newsletter" />

    <div class="container newsletter">
        <div class="row">
            <div class="col-md-12">
                <div id="RecipientList">
                    <h1>Recipient Lists</h1>
                    <p style="color: gray">A recipient list can hold email addresses that can be added to one or more newsletters.</p>
                    

                    <asp:Repeater ID="rptRecipientLists" runat="server">
                        <HeaderTemplate>
                            <table class="table table-striped">
                                <thead>
                                    <tr>
                                        <th></th>
                                        <th>Name</th>
                                        <th>Type</th>
                                        <th>Recipients</th>
                                        <th>Description</th>
                                        <th>&nbsp;</th>
                                    </tr>
                                </thead>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr id="<%# "listRow" + Eval("Id") %>">
                                <td><span class="glyphicon glyphicon-list" style="font-size: 16px;"></span></td>
                                <td>
                                    <strong>
                                        <a href='<%# BVNetwork.EPiSendMail.Configuration.NewsLetterConfiguration.GetModuleBaseDir() + "/plugin/recipientlists/listedit.aspx?recipientlistid=" + Eval("Id") %>'
                                   runat="server">
                                   <%# Eval("Name") %> 
                                </a>
                                        </strong>
                                </td>
                                <td><%# GetRecipientListTypeString(Eval("ListType")) %></td>
                                <td><span class="badge"><%# ((RecipientList)Container.DataItem).EmailAddressCount %></span></td>
                                <td><%# Eval("Description") %></td>
                                <td><button class="btn btn-sm" onclick="<%# "return deleteList(" + Eval("Id") + ", 'listRow" + Eval("Id") + "')" %>">Delete</button></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>

                    <div>
                        <a id="A3" runat="server" class="btn btn-primary"><span class="glyphicon glyphicon-pencil"></span> Create New List</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:content>

