<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="JobEdit.ascx.cs" Inherits="BVNetwork.EPiSendMail.Plugin.JobEditControl" %>
<%@ Import Namespace="BVNetwork.EPiSendMail.Configuration" %>
<%@ Register TagPrefix="EPiSendMail" TagName="JobStatus" Src="JobStatus.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="AddWorkItems" Src="AddWorkItems.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="RemoveWorkItems" Src="RemoveWorkItems.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="CreateJob" Src="CreateJob.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="StatusMessage" Src="../StatusMessage.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="MiscActions" Src="MiscActions.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="EnvironmentVerification" Src="EnvironmentVerification.ascx" %>

<script type="text/javascript">
    function sendNewsletter(dialogId) {
        $.ajax({
            dataType: "json",
            type: 'POST',
            url: '<%= NewsLetterConfiguration.GetModuleBaseDir() %>/api/job/send?jobid=<%= this.NewsletterJob.Id %>',
            success: function (data) {
                var html = $('#sendNewsletterResultTemplate').render(data);
                $('#pnlSendStatus').html(html);

                // Attempt to update the job status
                if (typeof updateStatus === 'function') {
                    updateStatus();
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                var data = {
                    warning: 'Failed to Send Newsletter',
                    message: errorThrown
                };
                var html = $('#warningTemplate').render(data);
                $('#pnlSendStatus').html(html);
            }
        });

        // Close dialog
        $('#' + dialogId).modal('hide');

        return false;
    }

</script>
<script id="sendNewsletterResultTemplate" type="text/x-jsrender">
    <div class="alert alert-success fade in" role="alert">
        <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
        <strong>Started Sending</strong> The newsletter status has been changed to Sending. See the status to the right for more information.     
        <p>
            Note! Depending on the site settings, it might take some time before
            the actual sending process is started.
        </p>
    </div>
</script>
<script id="warningTemplate" type="text/x-jsrender">
    <div class="alert alert-warning fade in" role="alert">
        <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
        <strong>{{:warning}}</strong> {{:message}}
    </div>
</script>

<div class="container newsletter">
    <div class="row">
        <div class="col-md-12">
            <h1 id="h2JobName" class="page-header" runat="server"></h1>
            <div style="margin-left: 1em; margin-bottom: 1em;">
                <asp:Label ID="jobDescription" runat="server" />
            </div>
        </div>
    </div>
    <div class="row" id="tblJobEditContent" runat="server">
        <%-- This table is shown by default --%>
        
        <%--Note! Sometimes the Send button is below the visible window, hack to make it show--%>
        <div class="col-md-8 col-lg-9" style="padding-bottom: 5em;">

            <EPiSendMail:statusmessage runat="server" id="ucStatusMessage" />
            <div id="pnlEditJobMainContainer">
                <%-- 
                If not a valid id, let the user create a new newsletter 
                --%>
                <asp:Panel ID="pnlNewNewsLetter" runat="server" Visible="false">
                    <EPiSendMail:CreateJob runat="server" DoRedirect="False" />
                </asp:Panel>
                <%-- 
                The result of the send process 
                --%>
                <div id="pnlSendStatus"></div>
                <asp:Panel CssClass="panel panel-success" role="panel" ID="pnlSendResult" ClientIDMode="Static" runat="server" EnableViewState="false" Visible="false">
                    <div class="panel-heading">
                        <button type="button" class="close" onclick="$('#pnlSendResult').hide();return false;"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
                        <h3 class="panel-title">Send Result</h3>
                    </div>
                    <div class="panel-body">
                        <asp:Literal ID="lblSendResult" runat="server" />
                    </div>
                </asp:Panel>
                <%-- 
                Verification status 
                --%>
                <EPiSendMail:EnvironmentVerification runat="server" visible="false" id="ucEnvironmentVerification" />

                <div runat="server" id="tblEditJob">
                    <div class="row">
                        <div class="col-lg-2 visible-lg">
                            <div class="fancy-number">1</div>
                        </div>
                        <div class="col-md-12 col-lg-10">
                            <%-- Add more work items from different sources --%>
                            <asp:Panel ID="pnlAddWorkItems" runat="server">
                                <EPiSendMail:AddWorkItems runat="Server" />
                            </asp:Panel>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-lg-2 visible-lg">
                            <div class="fancy-number">2</div>
                        </div>
                        <div class="col-md-12 col-lg-10">
                            <asp:Panel ID="pnlRemoveWorkItems" runat="server">
                                <EPiSendMail:RemoveWorkItems runat="Server" />
                            </asp:Panel>
                        </div>
                    </div>
                    <%-- Test-sends the newsletter --%>
                    <div class="row">
                        <div class="col-lg-2 visible-lg">
                            <div class="fancy-number">3</div>
                        </div>
                        <div class="col-md-12 col-lg-10">
                            <asp:Panel CssClass="row" DefaultButton="cmdSendTest" runat="server" Style="margin-bottom: 1em;">

                                <div class="col-md-12">
                                    <h2>Test the Newsletter</h2>
                                    <p>Send a test newsletter to make sure it looks OK. Add email addresses separated by comma or newline.
                                    </p>
                                    <p>
                                    <b>Note!</b> The email subject will show "(TEST)"
                                    </p>
                                </div>

                                <div class="col-md-12 col-lg-12 import-email-address">
                                    <asp:TextBox ID="txtSendTestTo" CssClass="form-control" runat="server" TextMode="multiLine" Rows="3" Width="100%" placeholder="Enter email addresses here, separated by commas or newlines" />
                                </div>

                                <div class="col-sm-12">
                                    <asp:Button Enabled="<%# !CurrentPage.IsPendingPublish %>" ID="cmdSendTest" runat="server"
                                        Text="Test Newsletter" CssClass="btn btn-primary" />
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                    <%-- Sends the newsletter --%>
                    <div class="row" style="padding-bottom: 2em;">
                        <div class="col-lg-2 visible-lg">
                            <div class="fancy-number">4</div>
                        </div>
                        <div class="col-md-12 col-lg-10">
                            <h2>Send the Newsletter</h2>
                            Click the button to start sending the newsletter
                        <br />
                            <button <%= ButtonEnabled() %> class="btn btn-primary" data-toggle="modal" data-target="#send-newsletter-confirmation-dialog">Send Newsletter...</button>

                            <div class="modal fade" id="send-newsletter-confirmation-dialog" tabindex="-1" role="dialog" aria-labelledby="sendNewsletterLabel" aria-hidden="true">
                                <div class="modal-dialog modal-lg">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <h3 class="modal-title">Ready to Send Newsletter</h3>
                                        </div>
                                        <div class="modal-body">
                                            Please confirm that you want to start sending the newsletter by clicking the Send Newsletter button below.
                                    <p>
                                        Tips:
                                        <ul class="list list-indented">
                                            <li>Check that you have selected the correct recipients</li>
                                            <li>Remove any recipients that have unsubscribed to the newsletter</li>
                                            <li>Preview the newsletter and verify that all images are present and links works</li>
                                            <li>Check that there are no security settings that will prevent recipients to read it</li>
                                            <li>Give the newsletter a good email title</li>
                                            <li>Check the spelling</li>
                                        </ul>
                                    </p>
                                        </div>
                                        <div class="modal-footer">
                                            <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                                            <button class="btn btn-success" onclick="return sendNewsletter('send-newsletter-confirmation-dialog');">Send Newsletter</button>
                                        </div>
                                    </div>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>
            </div>
        </div>
        <%-- Right Content Area --%>
        <div class="col-md-4 col-lg-3">
            <%-- Job Status --%>
            <div class="well" id="pnlJobStatus" runat="server">
                <EPiSendMail:jobstatus updateautomatically="False"
                    updateinterval="5" runat="Server" />
            </div>
            <%-- Preview page--%>
            <div class="well" id="pnlPreviewNewsLetter" runat="server">
                <h3><EPiServer:Translate text="/bvnetwork/sendmail/plugin/sendmail/previewbutton" runat="server" /></h3>
                <p>
                    Click the button to preview the newsletter in a new window.
                </p>

                <button class="btn btn-info" <%= ButtonEnabled() %>
                    onclick="window.open('<%= NewsLetterConfiguration.GetModuleBaseDir() %>/preview.aspx?pageid=<%= CurrentPage.PageLink.ID.ToString() %>&epslanguage=<%= CurrentPage.LanguageBranch %>').focus()"
                    type="button">
                    <span class="glyphicon glyphicon-eye-open"></span>
                    <episerver:translate text="/bvnetwork/sendmail/plugin/sendmail/previewbutton" runat="server" />
                </button>
            </div>

            <%-- Misc Actions for the newsletter --%>
            <div class="well" id="pnlMiscActions" runat="server">

                <EPiSendMail:miscactions runat="Server" />
            </div>
            <br />
        </div>
    </div>
</div>

