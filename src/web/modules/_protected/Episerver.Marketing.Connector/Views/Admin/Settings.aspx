<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="Settings.aspx.cs" Inherits="Episerver.Marketing.Connector.Framework.Views.Admin.Settings" %>

<asp:content runat="server" contentplaceholderid="FullRegion">
    <div class="epi-contentContainer epi-padding">
         <div class="epi-contentContainer epi-padding">
              <h1 class="EP-prefix">
                  <%= Translate("/episerver/marketing/connectors/admin/globalsettings/displayname") %>
             </h1>
             <p class="EP-systemInfo">
               <%= Translate("/episerver/marketing/connectors/admin/globalsettings/description") %>
             </p>
             <div class="epi-formArea">
               <div class="epi-size25">
                 <div>
                  <asp:Label runat="server" ID="lblEnableFormsAutofill" AssociatedControlID="chkEnableFormsAutofill"></asp:Label>
                  <asp:CheckBox ID="chkEnableFormsAutofill" runat="server" />
                 </div>
                </div>
               </div>
         </div>
        <div id="statusMessage" runat="server" class="EP-systemMessage EP-systemMessage-Warning" visible="false"></div>
       <div class="epi-buttonContainer">
                    <span class="epi-cmsButton">
                        <asp:Button runat="server" ID="uxSave" OnClick="uxSave_OnClick" CssClass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Save" Text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /></span>
       </div>
    </div>
   
</asp:content>
