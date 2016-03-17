<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EPiServerGroupProvider.ascx.cs" Inherits="BVNetwork.EPiSendMail.Plugin.ItemProviders.EPiServerGroupProvider" %>

<script>
    
    function addEPiServerGroupRecipients() {
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
            url: '<%# ApiUrl %>&groupname=' + $("#dropListEPiGroups").val(),
            success: function (data) {
                var html = $('#successEPiServerGroupTemplate').render(data);
                $('#pnlSendStatus').html(html);
                // Ask to update the total count too
                if (typeof updateListInfo === 'function') {
                    updateListInfo();
                }

            },
            error: function (jqXHR, textStatus, errorThrown) {
                var data = {
                    warning: 'Failed to add recipients.',
                    message: errorThrown
                };
                var html = $('#failedEPiServerGroupTemplate').render(data);
                $('#pnlSendStatus').html(html);
            }
        });

        return false;
    }
</script>
<asp:Panel ID="Panel1" runat="server">
    <!-- Add Work Items from group in EPiServer-->
    <b>Select EPiServer group to add email addresses from:</b>
    <br />

    <asp:DropDownList runat="server" ID="dropListEPiGroups" enableviewstate="false"
        style="margin-top: 0.7em;"/>
    <img src="../../content/images/ajax-loader.gif"  id="imgAjaxLoader" style="display: none" />
    <br />

    <button class="btn btn-success" style="margin-top: 0.7em;" onclick="return addEPiServerGroupRecipients();">Add recipients</button>

    <script id="successEPiServerGroupTemplate" type="text/x-jsrender">
        <div class="alert alert-success fade in" role="alert">
            <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
      <strong>Recipients added to newsletter</strong> 
            <p>Recipients from selected EPiServer group has been added to newsletter.     </p>
            <p>Details:</p>
            <p>Number of imported emails: {{:ImportedEmails}}</p>
            <p>Number of duplicated emails: {{:DuplicatedEmails}}</p>
            <p>Number of invalid emails: {{:InvalidEmails}}</p>             
            <p>Invalid message: {{:InvalidMessage}}</p>            
            <p>Status: {{:Status}}</p>
            <p>Time to import: {{:TimeToImport}} ms</p>
        </div>
    </script>
    <script id="failedEPiServerGroupTemplate" type="text/x-jsrender">
        <div class="alert alert-warning fade in" role="alert">
            <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
            <strong>{{:warning}}</strong> {{:message}}
        </div>
    </script>

</asp:Panel>
