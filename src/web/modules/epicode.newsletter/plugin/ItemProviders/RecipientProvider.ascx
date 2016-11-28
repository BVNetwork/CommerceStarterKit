<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecipientProvider.ascx.cs" Inherits="BVNetwork.EPiSendMail.Plugin.ItemProviders.RecipientProvider" %>
<script>
    function addEmailsFromRecipientList(sourceListId) {
        $.ajax({
            beforeSend: function () {
                $('#imgAjaxLoader').show();
            },
            complete: function () {
                $('#imgAjaxLoader').hide();
            },
            dataType: "json",
            type: 'POST',
            <%-- Important that this is added through databinding --%>
            url: '<%# ApiUrl %>&sourceListId=' + sourceListId,
            success: function (data) {
                var html = $('#successRecipientListTemplate').render(data);
                $('#addRecipientStatus').html(html);
                // Ask to update the total count too
                if (typeof updateListInfo === 'function') {
                    updateListInfo();
                }
                // Attempt to update the job status
                if (typeof updateStatus === 'function') {
                    updateStatus();
                }

            },
            error: function (jqXHR, textStatus, errorThrown) {
                var data = {
                    warning: 'Failed to add recipients.',
                    message: errorThrown
                };
                var html = $('#failedRecipientListTemplate').render(data);
                $('#addRecipientStatus').html(html);
            }
        });

        return false;
    }
</script>


<h3>Select a recipient list:</h3>
<asp:Repeater ID="rptAddFromRecipientLists" runat="server" enableviewstate="false">
    <HeaderTemplate>
        <div style="height: 300px !important; overflow: auto;">
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
            <td>
                <button class="btn btn-sm btn-success" onclick="return addEmailsFromRecipientList(<%# Eval("Id") %>);">Add</button>
            </td>
            <td><%# Eval("Name") %></td>
            <td><%# Eval("ListType") %></td>
            <td><span class="badge"><%# Eval("EmailAddressCount") %></span></td>
            <td><%# Eval("Description") %></td>
        </tr>        
    </ItemTemplate>
    <FooterTemplate>
        </table>
        </div>
    </FooterTemplate>
</asp:Repeater>
<div id="addRecipientStatus" style="padding-top: 1em;"></div>

    <script id="successRecipientListTemplate" type="text/x-jsrender">
        <div class="alert alert-success fade in" role="alert">
            <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
            <strong>Added Recipients</strong> 
            <p>Number of imported email addresses: <span class="badge">{{:ImportedEmails}}</span></p>
        </div>
    </script>

    <script id="failedRecipientListTemplate" type="text/x-jsrender">
        <div class="alert alert-danger fade in" role="alert">
            <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
            <h4>{{:warning}}</h4> 
            <p><strong>{{:message}}</strong></p>
        </div>
    </script>
