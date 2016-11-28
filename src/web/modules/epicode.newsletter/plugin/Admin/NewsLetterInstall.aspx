<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="NewsLetterInstall.aspx.cs"
    Inherits="BVNetwork.EPiSendMail.Plugin.Admin.NewsLetterInstall" %>

<%@ Import Namespace="BVNetwork.EPiSendMail.Configuration" %>

<%@ Register TagPrefix="EPiServerShell" Namespace="EPiServer.Shell.Web.UI.WebControls" Assembly="EPiServer.Shell" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="../PluginStyles.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" contentplaceholderid="FullRegion">
    
	<EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/install" Area="Newsletter" />
    <div class="container newsletter">
        <div class="row" style="padding-top: 2em;">
            <div class="col-md-2"></div>
            <div class="col-md-8">
                <div id="pnlWarning" class="panel panel-warning" runat="server">
                    <div class="panel-heading">
                        <h3 class="panel-title">Database not Installed</h3>
                    </div>
                    <div class="panel-body">
                        <p>
                            The Newsletter module requires some tables and procedures to be installed into the EPiServer CMS database. Don't worry, this will not affect other parts of the database.
                        </p>
                        <div class="text-center">
                            <button id="btnInstallDatabase" class="btn btn-primary">Install Database Now</button>
                        </div>
                        <p class="help-text">Note! If you cannot install the necessary database requirements from this page, the module installation package contains the scripts in the tools folder.</p>
                    </div>
                </div>
                <div id="result"></div>
            </div>
            <div class="col-md-2"></div>
        </div>
    </div>
	 <script type="text/javascript">
	     $(document).ready(function () {
	         $('#btnInstallDatabase').click(function (e) {
	             installDatabase();
	             return false;
	         });
	     });

	     function installDatabase() {
	         $.ajax({
	             dataType: 'json',
	             url: '<%= NewsLetterConfiguration.GetModuleBaseDir("/api/installer/InstallDatabase") %>',
	             type: 'POST'
	         })
	             .done(function (data) {
	                 var html = $('#successTemplate').render(data.database);
	                 $('#result').html(html);

	                 // done installing
	                 setTimeout(function () {
	                     window.location.replace('<%= NewsLetterConfiguration.GetModuleBaseDir("/plugin/newsletters.aspx") %>');
	                 },
	                    5000);
	             })
	             .fail(function (jqXHR, textStatus, errorThrown) {
	                 var html = $('#exceptionTemplate').render(jqXHR.responseJSON);
	                 $('#result').html(html);
	             });

             }
	 </script>
    <script id="exceptionTemplate" type="text/x-jsrender">
        <div class="alert alert-danger fade in" role="alert" style="padding-top: 1em;">
          <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
          <strong>{{:Message}}</strong> <br />
            {{:ExceptionMessage}}
            <pre>{{:StackTrace}}</pre>
        </div>
    </script>

    <script id="successTemplate" type="text/x-jsrender">
        <div class="alert alert-success fade in" role="alert" style="padding-top: 1em;">
          <button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span><span class="sr-only">Close</span></button>
          <strong>Database created</strong>
            <p>
            The Database tables and scripts were successfully created. Current version is <strong>{{:version}}</strong>
            </p>
            <p>This page will close in a few seconds, please wait.</p>
        </div>
    </script>

</asp:content>
