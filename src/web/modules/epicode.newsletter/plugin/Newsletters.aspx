<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Newsletters.aspx.cs"
    Inherits="BVNetwork.EPiSendMail.Plugin.NewsLetters"%>

<%@ Import Namespace="BVNetwork.EPiSendMail.DataAccess" %>
<%@ Register TagPrefix="EPiServerShell" Assembly="EPiServer.Shell" Namespace="EPiServer.Shell.Web.UI.WebControls" %>
<%@ Register TagPrefix="EPiSendMail" TagName="PluginStyles" Src="PluginStyles.ascx" %>
<%@ Register TagPrefix="EPiSendMail" TagName="GettingStarted" Src="GettingStarted.ascx" %>

<asp:content runat="server" contentplaceholderid="HeaderContentRegion">
    <EPiSendMail:PluginStyles runat="server" />
</asp:content>
<asp:content runat="server" contentplaceholderid="FullRegion">
		<EPiServerShell:ShellMenu ID="ShellMenu2" runat="server" SelectionPath="/global/newsletter/newsletters" Area="Newsletter" />
		<div class="newsletter">
			<div class="container" id="pnlJobList" runat="server">
				<div class="row">
					<div class="col-lg-12">
						<h1>Newsletters</h1>
						<%-- Newsletters that has not been sent, or that is in progress of beeing sent. --%>
						<asp:Repeater runat="server" ID="rptNewsLettersInProgress" EnableViewState="False" >
							<HeaderTemplate>
                            <table class="table table-striped">
                                <thead>
                                    <tr>
                                        <th style="width: 20px;">&nbsp;</th>
                                        <th>Name</th>
                                        <th>Status</th>
                                        <th>Recipients</th>
                                        <th>Page</th>
                                        <th>Description</th>
                                    </tr>
                                </thead>
							</HeaderTemplate>
							<ItemTemplate>
                                <tr>
                                    <td style="width: 20px;">
                                        <span class="glyphicon glyphicon-envelope" style="color: <%# GetNewsLetterColorForStatus((JobStatus)Eval("Status")) %>"></span>
                                    </td>
                                    <td>
                                        <strong>
									    <a id="A1" 
                                            href='<%# BVNetwork.EPiSendMail.Configuration.NewsLetterConfiguration.GetModuleBaseDir() %>/Plugin/Jobs/JobEditStandalone.aspx?pid=<%# GetNewsletterLink((Job)Container.DataItem) %>'>
									         <%# Eval("Name") %></a>
                                    </td>
                                    <td><%# Eval("Status") %></td>
                                    <td> <span class="badge"><%# GetNumberOfWorkItems((Job)Container.DataItem) %></span></td>
                                    <td><a href="<%# GetPageEditUrl((Job)Container.DataItem) %>">Edit Page</a></td>
                                    <td><%#Eval("Description") %></td>
                                </tr>
							</ItemTemplate>
							<FooterTemplate>
                                </table>
							</FooterTemplate>
						</asp:Repeater>

						<asp:LinkButton CssClass="btn btn-info" ID="lnkShowAll" OnClick="lnkShowAll_Click" runat="server" Text="Show All" />
                        <p>To add a new newsletter, create a new page of the type Newsletter in your page tree, it will be listed here automatically.</p>
				    </div>
				</div>
			</div>

			<div runat="server" id="pnlNoJobs" Visible="false">
			    <EPiSendMail:GettingStarted runat="server" />
		    </div>
</asp:content>
