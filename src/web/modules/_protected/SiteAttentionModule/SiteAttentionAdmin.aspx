<%@ page language="c#" codebehind="SiteAttentionAdmin.aspx.cs" autoeventwireup="true" inherits="EPiServer.modules.SiteAttentionModule.SiteAttentionAdmin, SiteAttentionModule" title="SiteAttentionAdmin" %>

<%@ import namespace="EPiServer.modules.SiteAttentionModule" %>
<%@ import namespace="EPiServer.modules.SiteAttentionModule.Data" %>

<asp:content contentplaceholderid="FullRegion" runat="server">

<div class="sa-plugin-settings epi-padding">
    <div class="epi-contentArea">
        <h1 class="EP-prefix"><EPiServer:Translate runat="server" LocalizedText="/admin/siteattention/heading" /></h1>
        <p><EPiServer:Translate runat="server" LocalizedText="/admin/siteattention/description" /></p>
    </div>

    <div class="epi-formArea">

        <asp:Panel id="panelPageTypes" runat="server" Visible="true">
            <div class="epi-buttonDefault">
                <span class="epi-cmsButton">
                    <asp:Button id="btnLicenseInfo" runat="server" cssclass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-File" OnClick="btnLicenseInfo_Click"  Text="<%$ Resources: EPiServer, admin.siteattention.licenseinfobutton %>" />
                </span>
                <span class="epi-cmsButton">
                    <asp:Button id="btnGenericSettings" runat="server" cssclass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-File" OnClick="btnGenericSettings_Click"  Text="<%$ Resources: EPiServer, admin.siteattention.genericsettingsbutton %>" />
                </span>
            </div>

            <asp:Repeater id="repPageTypes" runat="server">
                <HeaderTemplate>
                    <table class="epi-default " cellspacing="0" border="0" style="border-style: none; border-collapse: collapse;">
                    <tr>
                        <th scope="col"><EPiServer:Translate runat="server" LocalizedText="/admin/siteattention/nameheader" /></th>
                        <th scope="col"><EPiServer:Translate runat="server" class="aligncentermiddle" LocalizedText="/admin/siteattention/customheader" /></th>
                        <th scope="col"><EPiServer:Translate ID="Translate1" runat="server" LocalizedText="/admin/siteattention/descriptionheader" /></th>
                    </tr>
                </HeaderTemplate>

                <ItemTemplate>
                    <tr>
                        <td class="nowrap">
                            <asp:LinkButton id="btnName" runat="server" OnCommand='btnPageType_Click' CommandArgument='<%# DataBinder.Eval(Container.DataItem, "Id") %>' >
                                <%# HttpUtility.HtmlEncode(DataBinder.Eval(Container.DataItem, "LocalizedName   ").ToString())%>
                            </asp:LinkButton>
                        </td>
                        <td  class="aligncentermiddle">
                            <%# ((bool)DataBinder.Eval(Container.DataItem, "Custom")) ? "*" : "" %>
                        </td>
                        <td>
                            <%# HttpUtility.HtmlEncode(DataBinder.Eval(Container.DataItem, "Description").ToString()) %>
                        </td>
                    </tr>
                </ItemTemplate>

                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel id="panelGenericSettings" runat="server" Visible="False">
            <div class="epi-formArea">
                <div class="epi-size25 epi-paddingVertical-small">
                    <div>
                        <asp:Label AssociatedControlId="tbSEOUrl" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seourlproperties %>"></asp:Label>
                        <asp:TextBox id="tbSEOUrl" runat="server" cssclass="episize400" value=""></asp:TextBox>
                    </div>
                    <div>
                        <asp:Label AssociatedControlId="tbSEOTitles" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seotitleproperties %>"></asp:Label>
                        <asp:TextBox id="tbSEOTitles" runat="server" cssclass="episize400" value=""></asp:TextBox>
                    </div>
                    <div>
                        <asp:Label AssociatedControlId="tbSEODescription" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seometadescriptionproperties %>"></asp:Label>
                        <asp:TextBox id="tbSEODescription" runat="server" cssclass="episize400" value=""></asp:TextBox>
                    </div>
                   <%-- <div>
                        <asp:Label AssociatedControlId="tbSEOKeywords" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seokeywordsproperties %>"></asp:Label>
                        <asp:TextBox id="tbSEOKeywords" runat="server" cssclass="episize300" value=""></asp:TextBox>
                    </div>--%>
                    <%--<div>
                        <asp:Label AssociatedControlId="tbSEOContent" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seocontentproperties %>"></asp:Label>
                        <asp:TextBox id="tbSEOContent" runat="server" cssclass="episize300" value=""></asp:TextBox>
                    </div>--%>
                    <div>
                        <asp:Label AssociatedControlId="tbSEORichContent" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seorichcontentproperties %>"></asp:Label>
                        <asp:TextBox id="tbSEORichContent" runat="server" cssclass="episize400" value="" TextMode="MultiLine" ></asp:TextBox>
                    </div>
                    <div>
                        <asp:Label AssociatedControlId="tbSEOHeaderH1" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seoheaderh1properties %>"></asp:Label>
                        <asp:TextBox id="tbSEOHeaderH1" runat="server" cssclass="episize400" value=""></asp:TextBox>
                    </div>
                    <div>
                        <asp:Label AssociatedControlId="tbSEOHeaderH2" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seoheaderh2properties %>"></asp:Label>
                        <asp:TextBox id="tbSEOHeaderH2" runat="server" cssclass="episize400" value=""></asp:TextBox>
                    </div>
                    <div>
                        <asp:Label AssociatedControlId="tbSEOHeaderH3" runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.seoheaderh3properties %>"></asp:Label>
                        <asp:TextBox id="tbSEOHeaderH3" runat="server" cssclass="episize400" value=""></asp:TextBox>
                    </div>

                    <br />
                    <div class="epi-buttonContainer">
                        <span class="epi-cmsButton">
                            <asp:Button id="btnSaveGenericSettings" runat="server" cssclass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Save" OnClick="btnGenericSettingsSave_Click" Text="<%$ Resources: EPiServer, button.save %>" />
                        </span>
                        <span class="epi-cmsButton">
                            <asp:Button id="btnCancelGenericSettings" runat="server" cssclass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Cancel" OnClick="btnCancel_Click" Text="<%$ Resources: EPiServer, button.cancel %>" />
                        </span>
                    </div>
                </div>
            </div>
        </asp:Panel>


        <asp:Panel id="panelLicenseInfo" runat="server" Visible="False">
            <div class="epi-formArea">
                <div class="epi-size10 epi-paddingVertical-small">

                    <div id="SiteAttentionSettings" class="epi-size10 epi-paddingVertical-small"></div>
                    
                    <%--Inject the SiteAttention Settings UI script--%>
                    <script type="text/javascript">

                        ! function saInjectSettings(data) {
                            var id = 'SiteAttentionSettingsScript'
                                , url = 'https://siteattentionstorage.blob.core.windows.net/settings/settings.js?'
                                , script = document.getElementById(id)
                                    ? document.getElementById(id)
                                    : document.createElement('script')
                                , cb = function (event) {
                                    document.readyState === 'complete' || document.readyState === 'interactive'
                                        ? init()
                                        : document.addEventListener('DOMContentLoaded', init);
                                }
                                , init = function () {
                                    new SiteAttentionSettings(data);
                                };

                            script.id ? cb() : null;

                            script.type = 'text/javascript';
                            script.src = url;
                            script.id = id;

                            script.addEventListener('load', cb, false);

                            document.body.appendChild(script);
                        }
                        ({
                            apiUrl: '<%= Util.saUrl %>',
                            containerSelector: '#SiteAttentionSettings',
                            cms: 'ES',
                            cmsUrl: '/SaSettingsSave/Post',
                            cmsHeaders:
                            {
                                "Content-type": "application/json",
                                "X-Requested-With": "XMLHttpRequest",
                                "X-EPiContentLanguage": "en",
                                "X-EpiRestAntiForgeryToken": '<%=AntiForgeryToken%>'
                            },
                            settings:
                            {
                                key: '<%= PluginSettings.Settings.LicenseKey %>',
                                iid: '<%= PluginSettings.Settings.Iid %>',
                                iname: '<%= PluginSettings.Settings.InstanceName %>',
                                locked: '<%= PluginSettings.Settings.Locked %>' == 'True'
                            }
                        });

                    </script>

                    <div class="epi-buttonContainer">
                        <span class="epi-cmsButton">
                            <asp:Button id="btnCancelLicenseInfo" runat="server" cssclass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Cancel" OnClick="btnCancel_Click" Text="Back" />
                        </span>
                    </div>

                    <asp:TextBox id="tbLicenseKey" runat="server" CssClass="input-full-width" value="" Visible="False"></asp:TextBox>
                    <asp:TextBox id="tbInstanceName" runat="server" CssClass="input-full-width" value="" Visible="False"></asp:TextBox>
                    <asp:TextBox id="tbInstanceId" runat="server" CssClass="input-full-width" value="" Visible="False"></asp:TextBox>

                </div>
            </div>
        </asp:Panel>

        <asp:Panel id="panelPageType" runat="server" Visible="False">
            <div class="epi-formArea">
                <div class="checkboxarea">
                    <h2><asp:Literal id="litName" runat="server" text="" /></h2>
                    <asp:CheckBox id="cbCustomPageTypeSettings" runat="server" AutoPostBack="True" Checked="True" OnCheckedChanged="cbCustomPageTypeSettings_Click" Text="<%$ Resources: EPiServer, admin.siteattention.custompagetypesettings %>" ></asp:CheckBox>
                </div>
            </div>

            <div class="epi-formArea">
                <asp:Panel id="panelPageTypeForm" runat="server" Visable="False">
                    <div class="epi-paddingVertical-small">
                        <div class="epi-contentArea">

                            <p class="EP-systemInfo">
                                <asp:Label runat="server" Text="<%$ Resources: EPiServer, admin.siteattention.pagetypedescription %>"></asp:Label>
                            </p>
                        </div>
                        <div class="checkboxarea">
                            <asp:CheckBox id="cbOnlySelectedBuiltIn" runat="server" AutoPostBack="True" Checked="False" OnCheckedChanged="cbOnlySelectedBuiltIn_Click" Text="<%$ Resources: EPiServer, admin.siteattention.onlyselectedbuiltin %>" ></asp:CheckBox>
                            <asp:CheckBox id="cbOnlyString" runat="server" AutoPostBack="True" Checked="True" OnCheckedChanged="cbOnlyString_Click" Text="<%$ Resources: EPiServer, admin.siteattention.onlystring %>" ></asp:CheckBox>
                        </div>
                        <asp:Repeater id="repPageDefinitions" runat="server">
                            <HeaderTemplate>
                                <table class="epi-default " cellspacing="0" border="0" style="border-style: none; border-collapse: collapse;">
                                <tr>
                                    <th scope="col"><EPiServer:Translate ID="Translate2" runat="server" LocalizedText="/admin/siteattention/nameheader" /></th>
                                    <th scope="col" class=""><EPiServer:Translate ID="Translate3" runat="server" LocalizedText="/admin/siteattention/typeheader" /></th>
                                    <th scope="col" class=""><EPiServer:Translate ID="Translate11" runat="server" LocalizedText="/admin/siteattention/contentfieldtype" /></th>
                                </tr>

                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td class="nowrap" title="<%# HttpUtility.HtmlEncode(DataBinder.Eval(Container.DataItem, "Description").ToString()) %>">
                                        <%# (bool)DataBinder.Eval(Container.DataItem, "BuiltIn") ? "<i>" : ""%>
                                        <%# HttpUtility.HtmlEncode(DataBinder.Eval(Container.DataItem, "PropertyName").ToString()) %></td>
                                    <%# (bool)DataBinder.Eval(Container.DataItem, "BuiltIn") ? "</i>" : ""%>
                                    <td class="checkboxpadding"><%# HttpUtility.HtmlEncode(DataBinder.Eval(Container.DataItem, "TypeName").ToString())%></td>
                                    <td class="checkboxpadding">
                                        <asp:DropDownList id="ddContentFieldType" DataValueField='<%# HttpUtility.HtmlEncode(DataBinder.Eval(Container.DataItem, "PropertyName").ToString()) %>' SelectedValue='<%# DataBinder.Eval(Container.DataItem, "PropertySettingType").ToString() %>' runat="server" >
                                            <asp:ListItem value="None" selected="True">
                                                -
                                            </asp:ListItem>
                                            <asp:ListItem value="Url" Text="<%$ Resources: EPiServer, admin.siteattention.dropurl %>" selected="False" />
                                            <asp:ListItem value="PageTitle" Text="<%$ Resources: EPiServer, admin.siteattention.droptitle %>" selected="False" />
                                            <asp:ListItem value="PageDescription" Text="<%$ Resources: EPiServer, admin.siteattention.dropdescription %>" selected="False" />
                                            <%--<asp:ListItem value="Keywords" Text="<%$ Resources: EPiServer, admin.siteattention.dropkeywords %>" selected="False" />
                                            <asp:ListItem value="PlainContent" Text="<%$ Resources: EPiServer, admin.siteattention.dropcontent %>" selected="False" />--%>
                                            <asp:ListItem value="RichContent" Text="<%$ Resources: EPiServer, admin.siteattention.droprichcontent %>" selected="False" />
                                            <asp:ListItem value="HeaderH1" Text="<%$ Resources: EPiServer, admin.siteattention.dropheadersh1 %>" selected="False" />
                                            <asp:ListItem value="HeaderH2" Text="<%$ Resources: EPiServer, admin.siteattention.dropheadersh2 %>" selected="False" />
                                            <asp:ListItem value="HeaderH3" Text="<%$ Resources: EPiServer, admin.siteattention.dropheadersh3 %>" selected="False" />

                                        </asp:DropDownList>
                                    </td>

                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>
                <div class="epi-buttonContainer">
                    <asp:Label id="lblSavePageTypeSettings" runat="server" cssclass="epi-cmsButton">
                        <asp:Button id="btnSavePageTypeSettings" runat="server" cssclass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Save" OnClick="btnPageTypeSettingsSave_Click" Text="<%$ Resources: EPiServer, button.save %>" />
                    </asp:Label>
                    <span class="epi-cmsButton">
                        <asp:Button id="btnCancelPageTypeSettings" runat="server" cssclass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Cancel" OnClick="btnCancel_Click" Text="<%$ Resources: EPiServer, button.cancel %>" />
                    </span>
                </div>
            </div>
        </asp:Panel>

    </div>
</div>

</asp:content>
