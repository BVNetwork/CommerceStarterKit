define([
    "dojo",
    "dojo/_base/declare",
    "dojo/_base/lang",
    "dojo/_base/xhr",
    "dojo/html",
    "dojo/aspect",
    "dojo/on",
    "dojo/topic",
    "dojo/when",

    "dijit/_TemplatedMixin",
    "dijit/_Widget",

    "epi/dependency",
    "epi/username",
    "epi/shell/XhrWrapper",
    "epi-cms/ApplicationSettings",
    "epi-cms/_ContentContextMixin"

],

function (
    dojo,
    declare,
    lang,
    xhr,
    html,
    aspect,
    on,
    topic,
    when,

    _TemplatedMixin,
    _Widget,

    dependency,
    username,
    XhrWrapper,
    appSettings,
    _ContentContextMixin
) {

    // summary: Integration of SiteAttention in EPiServer.
    return declare("siteattention/Components/SiteAttentionWidget", [_Widget, _TemplatedMixin, _ContentContextMixin], {

        templateString: '<div id="siteattention_area">\
                            <div data-dojo-attach-point="contentName2" id="SAPL"></div>\
                        </div>',

        _xhrWrapper: new XhrWrapper(),

        _unwatchHandle: null,

        _innerUnwatchHandle: null,

        postCreate: function () {
            if (typeof SiteAttention !== 'undefined' ) {
                try {
                    SiteAttention.quit();
                } catch (ex) {
                    console.log(ex);
                }
            }

            if (!EPiServerSiteAttentionData.OnModelChangeSubscribed) {
                topic.subscribe("siteattention/modelchanged", lang.hitch(this, "_onModelChanged"));
                EPiServerSiteAttentionData.OnModelChangeSubscribed = true;
            }

            when(this.getCurrentContent(), function (contentData) {
                topic.publish("siteattention/modelchanged", { contentData: contentData });
            });
        },

        startup: function () {
            //var isWidgetAlreadyInitialized = function () {
            //    return !!document.getElementById("SAPL");
            //};

            require(["dijit/registry"], function (registry) {
                var componentContainers = document.getElementsByClassName("epi-componentContainer");
                for (var i = 0; i < componentContainers.length; i++) {
                    var widget = registry.byId(componentContainers[i].getAttribute("widgetid"));
                    aspect.around(widget, "addComponent", function (originalMethod) {
                        return function (component) {
                            if (typeof SiteAttention  !== 'undefined') {
                                if (component.moduleName === "SiteAttentionModule") {
                                    alert('SiteAttention widget has already been added.');
                                    return [];
                                }
                            }

                            return originalMethod.apply(this, arguments);
                        };
                    });
                }
            });
        },

        postMixInProperties: function () {
        },

        _loadPageDataAndSAScripts: function (pageInformation, properties) {

            this._xhrWrapper.xhrGet({
                url: "/SaSettingsSave/Get",
                handleAs: "json",
                content: pageInformation
            }).then(
                function (siteAttentionData) {
                    EPiServerSiteAttentionData.SiteAttentionSelectors = siteAttentionData.siteAttentionSelectors;
                    EPiServerSiteAttentionData.SiteAttentionUILanguage = siteAttentionData.siteAttentionUILanguage;
                    EPiServerSiteAttentionData.SiteAttentionPageExternalUrl = siteAttentionData.siteAttentionPageExternalUrl;

                    EPiServerSiteAttentionData.guid = siteAttentionData.siteAttentionGuid;
                    EPiServerSiteAttentionData.iid = siteAttentionData.siteAttentionIid;
                    EPiServerSiteAttentionData.licenseKey = siteAttentionData.siteAttentionLicenseKey;
                    EPiServerSiteAttentionData.PageInformation.externalUrl = siteAttentionData.siteAttentionPageExternalUrl;
                    EPiServerSiteAttentionData.PageInformation.uiLanguage = siteAttentionData.siteAttentionUILanguage;
                    EPiServerSiteAttentionData.PageInformation.username = (username.currentUsername || appSettings.userName);
                    EPiServerSiteAttentionData.onPageChange(pageInformation.pageName);
                },
                function (err) {
                }
            );
        },

        _onModelChanged: function (model) {
            SiteAttentionEpiServerWatcher.updateBaseModel(model);

            EPiServerSiteAttentionData.siteAttentionCurrentContentModel = model;
            EPiServerSiteAttentionData.PageInformation = {};
            EPiServerSiteAttentionData.PageInformation.pageName = model.contentData.properties.icontent_name;
            EPiServerSiteAttentionData.PageInformation.contentLink = model.contentData.contentLink;
            EPiServerSiteAttentionData.PageInformation.contentGuid = model.contentData.contentLink.split('_')[0];//model.contentData.contentId;
            EPiServerSiteAttentionData.PageInformation.languageId = (model.contentData.currentLanguageBranch.languageId || 'en');
            EPiServerSiteAttentionData.PageInformation.pageTypeName = model.contentData.properties.pageTypeName;
            EPiServerSiteAttentionData.PageInformation.uiLanguage = EPiServerSiteAttentionData.PageInformation.uiLanguage || 'eng'; // Only a default value. Will be poulated when a call is made to the server
            EPiServerSiteAttentionData.PageInformation.externalUrl = EPiServerSiteAttentionData.PageInformation.externalUrl || null; // Will be poulated when a call is made to the server,
            EPiServerSiteAttentionData.PageInformation.published = model.contentData.status === 4; // if status = 4, the page is published

            // Only load once for each page
            if (EPiServerSiteAttentionData.currentPage != model.contentData.contentGuid ||
                EPiServerSiteAttentionData.currentContentLink != model.contentData.contentLink ||
                (!window.SiteAttention)) {
                this._loadPageDataAndSAScripts(EPiServerSiteAttentionData.PageInformation, model.contentData.properties);
                EPiServerSiteAttentionData.currentPage = model.contentData.contentGuid;
                EPiServerSiteAttentionData.currentContentLink = model.contentData.contentLink;
            }

            // this._renderUserSupportInfoOnWidgets();
        }
    });
});
