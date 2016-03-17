<%@ control language="C#" autoeventwireup="true" codebehind="JobStatus.ascx.cs" inherits="BVNetwork.EPiSendMail.Plugin.JobStatus" %>
<%@ Import Namespace="System.Globalization" %>
<%@ import namespace="BVNetwork.EPiSendMail.Configuration" %>
<script language="JavaScript">
<!--
    // Default is every 10 seconds
    var durationInSeconds = <%= UpdateInterval %>;
    var timerID = null;
    var delay = 1000; // One second increment

    function InitializeTimer() {
        timerID = setInterval(updateStatus, durationInSeconds * delay);
    }

    function StopTheTimer() {
        if (timerID != null) {
            clearInterval(timerID);
        }
    }

    $.views.helpers({
        time: function (val) {
            var value = new Date(val);
            return value.toLocaleTimeString('<%= CultureInfo.CurrentCulture.Name %>', {hour: '2-digit', minute:'2-digit', second: '2-digit'});
        }
    });

    function updateStatus() {
        $.ajax({
            dataType: "json",
            url: '<%= NewsLetterConfiguration.GetModuleBaseDir() %>/api/job/getjobstatus?jobid=<%= this.NewsletterJob.Id %>',
            success: function(data) {
                var html =  $('#jobStatusTemplate').render(data);
                $('#pnlStatusMessage').html(html);
            },
            error: function(data) {
                if (data.status === 500) {
                    var errorData = {};
                    errorData.status = data.status;
                    errorData.errorMessage = data.statusText;
                    if (data.responseJSON) {
                        errorData.detailedMessage = data.responseJSON.ExceptionMessage;
                    }
                    var html = $('#jobStatusErrorTemplate').render(errorData);
                    $('#pnlStatusMessage').html(html);
                } 
                else {
                    $('#pnlStatusMessage').html("An error occured.");
                }
            }
        });
    }

    function updateStatusManually() {
        StopTheTimer();
        InitializeTimer();
    }

    // Start the update timer
    $(document).ready(function() {
        InitializeTimer();
    });

    //-->
</script>

<h3>Status</h3>
<div id="pnlStatusMessage" style="margin-bottom: 10px;">
</div>

<a href="#" cssclass="btn btn-info" tooltip="Update the status with fresh values" onclick="updateStatusManually();return false;">
      <span style="font-size: 14px;" class="glyphicon glyphicon-refresh"></span> Update status
</a>

<script id="jobStatusErrorTemplate" type="text/x-jsrender">
    <div class="alert alert-warning" role="alert">
        <span class="label label-warning">{{:status}} - {{:errorMessage}}</span>
        <p>{{:detailedMessage}}</p>
    </div>
</script>

<script id="jobStatusTemplate" type="text/x-jsrender">
    {{if SchedulerIsOnline===false}}
    <div class="alert alert-warning" role="alert">
        <span class="label label-warning">Warning</span>
        <%= TranslateFallback("/bvnetwork/sendmail/plugin/sendmail/status/schedulernotrunning", "The Scheduler is not running!") %>
    </div>
    {{/if}}
    <table border="0" cellpadding="2" class="tableStatus">
        <tbody>
            <tr>
                <td>Send Status:</td>
                <td><b>{{:Status}}</b></td>
            </tr>
            <tr>
                <td>Recipients:</td>
                <td><span class="badge">{{: EmailsInQueue + EmailsNotSent}}</span></td>
            </tr>
            <tr>
                <td>Sending Now:</td>
                <td>{{:EmailsInQueue}}</td>
            </tr>
            <tr>
                <td>Not Sent:</td>
                <td>{{:EmailsNotSent}}</td>
            </tr>
            <tr>
                <td>Errors:</td>
                <td>{{:EmailsFailed}}</td>
            </tr>
            <tr>
                <td>Completed:</td>
                <td>{{:EmailsSent}}</td>
            </tr>
            <tr>
                <td>Updated:</td>
                <td>{{>~time(TimeStamp)}}
                </td>
            </tr>
            <tr>
                <td>Batch Size:</td>
                <td>{{:BatchSize}}
                    <span style="color: #999;" title="The number of emails that will be sent for each task " class="glyphicon glyphicon-info-sign"></span>
                </td>
            </tr>
            <tr>
                <td>Task Enabled:</td>
                <td>{{:ScheduledTaskIsEnabled}}
                    <span style="color: #999;" title="The scheduled task needs to be enabled for newsletters to be sent." class="glyphicon glyphicon-info-sign"></span>
                </td>
            </tr>
            <tr>
                <td>Next Run:</td>
                <td>{{>~time(ScheduledTaskNextRun)}}
                    <span style="color: #999;" title="Next time the scheduled task to send newsletters will run: {{: ScheduledTaskNextRun}}" class="glyphicon glyphicon-info-sign"></span>
                </td>
            </tr>
            
        </tbody>
    </table>

</script>
