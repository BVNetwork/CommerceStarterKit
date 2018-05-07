define([
    "dojo",
    "dojo/_base/declare",
    "dojo/_base/xhr",
    "dojo/_base/lang",
    "dojo/aspect",
    "dojo/when",

    "epi/_Module",
    "epi/dependency",
    "epi/routes",
    "epi/shell/XhrWrapper",

    "siteattention/CommandProviders/SiteAttentionCommandProvider",
    "siteattention/SiteAttentionContent"
],

function (
    dojo,
    declare,
    xhr,
    lang,
    aspect,
    when,

    _Module,
    dependency,
    routes,
    XhrWrapper,

    SiteAttentionCommandProvider,
    SiteAttentionContent
) {

    // summary: Module initializer for the default module.
    return declare("siteattention.SiteAttentionModule", [_Module], {
        SACommandProvider: new SiteAttentionCommandProvider(),

        initialize: function () {
            this.inherited(arguments);
            var content = new SiteAttentionContent();
            content.initialize();

            // Register the Command Provider
            var commandregistry = dependency.resolve("epi.globalcommandregistry");
            //console.info('dependency.resolve("epi.globalcommandregistry")');
            commandregistry.registerProvider("epi.cms.publishmenu", this.SACommandProvider);
            //console.info('In siteattention.DefaultModule - initialize - End');

            EPiServerSiteAttentionData.SACommandProvider = this.SACommandProvider;
            EPiServerSiteAttentionData.siteAttentionPluginUrl = 'https://api.siteattention.com/';
            EPiServerSiteAttentionData._xhrHandler = new XhrWrapper();
        },
    });
});

// The Data Helper object
var EPiServerSiteAttentionData = {
    licenseKey: null,
    iid: null,
    locked: false,
    PageInformation: {},
    siteAttentionCurrentContentModel: null,
    siteAttentionLoaded: false,
    SiteAttentionSelectors: [],
    SiteAttentionPageExternalUrl: null,
    SiteAttentionUILanguage: null,

    onPageChange: function () {
        if ( typeof SiteAttention !== 'undefined' )
        {
            SiteAttention.stop();
        }

        SiteAttentionEpiServerWatcher.clearHandlers();
        this.initSiteAttentionPlugin();
    },

    initSiteAttentionPlugin: function () {
        if (this.SiteAttentionLoading) {
            return;
        }

        if (this.SiteAttentionLoaded && !!window.SiteAttention) {
            this.loadSiteAttentionForPage();
            return;
        }

        this.SiteAttentionLoaded = true;
        this.SiteAttentionLoading = true;
        this._inject_script(this.siteAttentionPluginUrl + (this.licenseKey ? this.licenseKey : ""), this.loadSiteAttentionForPage.bind(this));
    },

    _saveLicenseAndIid: function (license, iid, name, locked) {
        this._xhrHandler.xhrPost({
            url: "/SaSettingsSave/Post",
            handleAs: "json",
            content: { key: license, iid: iid, iname: name, locked: locked }
        })
        .then
        (
            function (data) {
            },
            function (err) {
            }
        );
    },

    _saveLicense: function (status, key, savedData) {
        if (status === true) {
            this.licenseKey = key;
            this.iid = savedData.iid;
            this.locked = savedData.locked;
            this._saveLicenseAndIid(this.licenseKey, this.iid, savedData.name, this.locked);
        }
    },

    _saveInstance: function (status, instance) {
        if (status === true) {
            this.iid = instance.iid;
            this._saveLicenseAndIid(this.licenseKey, instance.iid, instance.name, this.locked);
        }
    },

    _inject_script: function ( url , cb , doc )
    {
        if ( ! doc )
        {
            doc = document;
        }

        let script = document.createElement( 'script' );
            script.type = 'text/javascript';
            script.async = true;
            script.src = url;

        if ( cb === undefined )
        {
            return;
        }

        if ( script.addEventListener )
        {
            script.addEventListener ( 'load' , cb );
        }

        else if ( script.readyState )
        {
            script.onreadystatechange = cb;
        }

        doc.body.appendChild ( script );
    },

    loadSiteAttentionForPage: function () {
        this.SiteAttentionLoading = false;

        if (!this.SiteAttentionHooked) {
            SiteAttentionModule.hooks.add
                (
                'after',
                'register',
                'save license',
                this._saveLicense,
                this
                );
            SiteAttentionModule.hooks.add
                (
                'after',
                'instance',
                'save instance data',
                this._saveInstance,
                this
                );
            SiteAttentionModule.hooks.add
                (
                'after',
                'license',
                'save instance data',
                this._saveLicense,
                this
                );

            this.SiteAttentionHooked = true;
        }

        if (!SiteAttention.started) {
            SiteAttention.play({
                container: document.querySelector("#SAPL"),
                minimised: false
            });
        }

        var fields = this.SiteAttentionSelectors.map(function (selector) {
            return {
                seo: selector.contentfield,
                name: selector.name,
                selector: StringHelper.lowerCaseFirstLetter(selector.contentId),
                type: 'FieldEPiServerWrapper'
            }
        });

        let map = SiteAttentionModule.FieldFactory(fields);

        //TODO: dummy data need to be populated from this.PageInformation
        var url = document.location.origin + EPiServerSiteAttentionData.SiteAttentionPageExternalUrl;

        SiteAttention.load
        ({
                cms: SiteAttentionModule.Cms.EPIServer,
                guid: this.guid,
                pid: this.PageInformation.contentGuid,
                iid: this.iid ? this.iid : null,
                type: this.PageInformation.pageTypeName,
                lang: this.PageInformation.languageId,
                user: this.PageInformation.username,
                url: url,//this.EPiServerSiteAttentionData.SiteAttentionPageExternalUrl,
                published: this.PageInformation.published,
                map: map
        });

        this.SACommandProvider._refreshDataModel();
    }
}

//main data dispatcher to support FieldEPiServerWrapper
var SiteAttentionEpiServerWatcher = {
    handlers: {},
    model: {},
    metadata: {},
    renderedModel: {},
    renderedHtml: {},

    hasField: function (field) {
        return true;
    },

    clearHandlers: function (field) {
        handlers = {};
        renderedModel = {};
        renderedHtml = {};
    },

    register: function (field, cb) {
        var correctFieldName = StringHelper.lowerCaseFirstLetter(field);
        if (!this.handlers.hasOwnProperty(correctFieldName))
            this.handlers[correctFieldName] = [];

        this.handlers[correctFieldName].push(cb);
    },

    unregister: function (field) {
        var correctFieldName = StringHelper.lowerCaseFirstLetter(field);
        if (this.handlers.hasOwnProperty(correctFieldName)) {
            this.handlers[correctFieldName] = [];
        }
    },

    getValue: function (field) {
        var epiField = 'epi-' + StringHelper.lowerCaseFirstLetter(field);
        if (this.renderedModel.hasOwnProperty(epiField)) {
            return this.renderedModel[epiField];
        }

        if (this.model && this.model.contentModel) {
            if (this.model.contentModel.hasOwnProperty(epiField)) {
                return this.model.contentModel[epiField];
            }
        }

        return '';
    },

    getHtmlValue: function (field) {
        var result = "";
        var epiField = 'epi-' + StringHelper.lowerCaseFirstLetter(field);

        if (this.renderedHtml.hasOwnProperty(epiField)) {
            result = this.renderedHtml[epiField];
        }

        else if (this.model && this.model.renderedHtml) {
             if (this.model.renderedHtml.hasOwnProperty(epiField)) {
                 result = this.model.renderedHtml[epiField];
             }
        }
        
        else {
            return this.getValue(field);
        }

        for(i = 0; i < EPiServerSiteAttentionData.SiteAttentionSelectors.length; i++) {
            var fieldObj = EPiServerSiteAttentionData.SiteAttentionSelectors[i];

            if
            (
                fieldObj.contentId == field &&
                fieldObj.wrapperTag != "" &&
                !result.startsWith('<' + fieldObj.wrapperTag)
            ) {
                return '<' + fieldObj.wrapperTag + '>' + result + '</' + fieldObj.wrapperTag + '>';
            }
        }
        
        return result;
    },

    updateValue: function (field, newValue, newHtml) {
        this.renderedModel['epi-' + field] = newValue;
        this.renderedHtml['epi-' + field] = (newHtml ? newHtml : newValue);

        var correctFieldName = StringHelper.lowerCaseFirstLetter(field);
        if (this.handlers.hasOwnProperty(correctFieldName)) {
            var cbs = this.handlers[correctFieldName];
            for (var index = 0; index < cbs.length; index++) {
                var cb = cbs[index];
                cb();
            }
        }
    },

    notifyAllHandlers: function () {
        Object.keys(this.handlers).forEach(function (field) {
            this.updateValue(field);
        }.bind(this));
    },

    updateBaseModel: function (model) {
        this.model = model;
        this.renderedModel = {};
        this.renderedHtml = {};
    },

    focus: function (propertyId) {
        if (propertyId === "pageExternalURL" || propertyId === "iroutable_routesegment" || propertyId === "icontent_name") {
            $(".epi-form-container.epi-cmsEditingForm.epi-cmsEditingFormOpe.dijitContainer.epi-cardContainer-child.epi-cardContainer-epi-cmsEditingForm.dijitHidden").removeClass("dijitHidden").addClass("dijitVisible");
            var attr = "[name*='" + propertyId + "']";
            var ctr = $(attr);
            try {
                ctr[0].click();
            } catch (ex) {
                console.warn(ex);
            }

        }
        this.model.setActiveProperty(propertyId);
    }
}

var StringHelper = {
    lowerCaseFirstLetter: function (value) {
        return value.charAt(0).toLowerCase() + value.slice(1);
    }
}
