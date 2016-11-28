<%@ Control Language="C#" AutoEventWireup="true" 
            CodeBehind="PluginStyles.ascx.cs" enableviewstate="false"
            Inherits="BVNetwork.EPiSendMail.Plugin.PluginStyles" %>
<%@ Import Namespace="EPiServer.Framework.Web.Mvc.Html" %>

<%= Page.ClientResources("ShellWidgets")%>
<link rel="stylesheet" type="text/css" href="<%= BVNetwork.EPiSendMail.Configuration.NewsLetterConfiguration.GetModuleBaseDir("/content/css/bootstrap.min.css") %>">
<link rel="stylesheet" type="text/css" href="<%= BVNetwork.EPiSendMail.Configuration.NewsLetterConfiguration.GetModuleBaseDir("/content/css/newsletterstyle.css") %>">
