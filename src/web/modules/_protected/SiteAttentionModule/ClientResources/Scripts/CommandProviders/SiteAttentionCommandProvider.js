define([
    "dojo/Stateful",
    "dojo/_base/declare",
    "dojo/_base/lang",
    "dojo/_base/xhr",
    "dojo/topic",

    "epi/dependency",
    "epi/shell/XhrWrapper",
    "epi-cms/core/ContentReference",
    "epi/shell/command/_CommandProviderMixin"
],

function (
    Stateful,
    declare,
    lang,
    xhr,
    topic,

    dependency,
    XhrWrapper,
    ContentReference,
    _CommandProviderMixin
) {

    return declare([_CommandProviderMixin], {

        _propertyEditedHandle: null,
        _someArray: null,

        someData: null,
        _xhrHandler: null,
        _evaluationQueue: [],

        _profile: null,

        constructor: function () {
            if (!this._xhrHandler) {
                this._xhrHandler = new XhrWrapper();
            }
            this._profile = this._profile || dependency.resolve("epi.shell.Profile");

            // https://tc39.github.io/ecma262/#sec-array.prototype.find
            if (!Array.prototype.find) {
                Object.defineProperty(Array.prototype, 'find', {
                    value: function (predicate) {
                        // 1. Let O be ? ToObject(this value).
                        if (this == null) {
                            throw new TypeError('"this" is null or not defined');
                        }

                        var o = Object(this);

                        // 2. Let len be ? ToLength(? Get(O, "length")).
                        var len = o.length >>> 0;

                        // 3. If IsCallable(predicate) is false, throw a TypeError exception.
                        if (typeof predicate !== 'function') {
                            throw new TypeError('predicate must be a function');
                        }

                        // 4. If thisArg was supplied, let T be thisArg; else let T be undefined.
                        var thisArg = arguments[1];

                        // 5. Let k be 0.
                        var k = 0;

                        // 6. Repeat, while k < len
                        while (k < len) {
                            // a. Let Pk be ! ToString(k).
                            // b. Let kValue be ? Get(O, Pk).
                            // c. Let testResult be ToBoolean(? Call(predicate, T, « kValue, k, O »)).
                            // d. If testResult is true, return kValue.
                            var kValue = o[k];
                            if (predicate.call(thisArg, kValue, k, o)) {
                                return kValue;
                            }
                            // e. Increase k by 1.
                            k++;
                        }

                        // 7. Return undefined.
                        return undefined;
                    }
                });
            }
        },

        _refreshDataModel: function () {
            if (this._model !== undefined) {
                if (this._model.metadata) {
                    for (var propName in this._model.contentData.properties) {
                        if (this._model.contentData.properties.hasOwnProperty(propName)) {
                            this._refreshDataModelProperty(propName, this._model.contentData.properties[propName]);
                        }
                    }
                }
            }

            // Because of updateDataModel only be called when the content context change,
            // so we need to watch content model change incase editor update property value.
            if (this._propertyEditedHandle) {
                this._propertyEditedHandle.remove();
            }

            this._propertyEditedHandle = this._model.contentModel.watch(lang.hitch(this, function (name, oldValue, newValue) {
                this._refreshDataModelProperty(name, newValue);
            }));
        },

        _findPropertyByName: function (name) {
            return function (pro) {
                return pro.name.toLowerCase() === name.toLowerCase();
            };
        },

        _startsWith: function (inputString, searchString, position) {
            position = position || 0;
            return inputString.indexOf(searchString, position) === position;
        },

        _evaluateForContentArea: function (name, value, pro) {
            var reference = ContentReference.toContentReference(this._model.contentLink);
            var postData = {
                propertyName: name,
                newPropertyValue: JSON.stringify(value),
                id: reference.toString(),
                epslanguage: this._profile ? this._profile.contentLanguage : ""
                // The default model binder in MVC needs the id to be part of the posted data.
            };

            this._xhrHandler.xhrPost({
                url: "/SaPropertyRender/Render",
                handleAs: "json",
                content: postData
            }).then
            (
                function (data) {
                    SiteAttentionEpiServerWatcher.updateValue(name, !data.value ? '' : data.value, '');
                },
                function (err) {
                }
            );
        },

        _evaluateForRouteSegment: function (name, value, pro) {
            var pageStatusIsPublished = this._model.contentData.status === 4;
            var publicUrl = '';
            if (this._model.contentData.publicUrl === '/') {
                publicUrl = this._model.contentData.publicUrl + "/" + value;
            } else {
                var segmentsUrl = this._model.contentData.publicUrl.split('/');
                var lastSegment = '';
                for (var i = segmentsUrl.length - 1; i--; i >= 0) {
                    if (segmentsUrl[i] !== '') {
                        lastSegment = segmentsUrl[i];
                        break;
                    }
                }
                var newUrl = this._model.contentData.publicUrl.replace(lastSegment, value);
                publicUrl = window.location.protocol +
                    '//' +
                    window.location.host +
                    (this._startsWith(newUrl, "/") ? "" : "/") +
                    newUrl;
            }
            SiteAttentionEpiServerWatcher.updateValue(name, publicUrl, '');

            // POST to SA after the page's url changed
            if (window.SiteAttention && EPiServerSiteAttentionData.iid && EPiServerSiteAttentionData.licenseKey) {
                SiteAttention.lib.promise.post(EPiServerSiteAttentionData.siteAttentionPluginUrl + EPiServerSiteAttentionData.licenseKey,    // siteAttentionPluginUrl declared in initializer
                    {
                        'func': 'publish',
                        'published': pageStatusIsPublished.toString().toLocaleLowerCase(),
                        'url': publicUrl,
                        'pid': EPiServerSiteAttentionData.PageInformation.contentGuid,
                        'lang': EPiServerSiteAttentionData.PageInformation.languageId,
                        'iid': EPiServerSiteAttentionData.iid
                    },
                    { "Content-type": "application/json", "X-SiteAttention": EPiServerSiteAttentionData.licenseKey }).then(
                    function (error, data, xhr) {

                    });
            }
        },

        _debounce: function (func, wait, immediate) {
            var timeout;
            return function () {
                var context = this, args = arguments;
                var later = function () {
                    timeout = null;
                    if (!immediate) func.apply(context, args);
                };
                var callNow = immediate && !timeout;
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
                if (callNow) func.apply(context, args);
            };
        },

        _processOtherPropertyQueue: function (queue, data) {
            var doc = document.implementation.createHTMLDocument('tmp');
            doc.documentElement.innerHTML = data;
            for (i = 0; i < queue.length; i++) {
                var property = queue[i];

                if (property.pro) {
                    if (property.name === 'icontent_name') {
                        var tag = doc.querySelector('[data-epi-property-name="' + "PageName" + '"]');
                    } else {
                        var tag = doc.querySelector('[data-epi-property-name="' + property.pro.name + '"]');
                    }
                    if (tag) {
                        SiteAttentionEpiServerWatcher.updateValue(property.name,
                            property.value,
                            tag.outerHTML);
                    } else {
                        SiteAttentionEpiServerWatcher.updateValue(property.name, property.value, '');
                    }
                }
            }
        },

        _evaluateForOtherProperty: function (name, value, pro) {

            // if the name is existing in queue, we find out its index and remove it before push
            var existingIndex = -1;
            for (var idx = 0; idx < this._evaluationQueue.length; idx++) {
                var element = this._evaluationQueue[idx];
                if (element.name === name) {
                    existingIndex == idx;
                    break;
                }
            }

            // remove the existing item with name
            if (existingIndex > -1) {
                this._evaluationQueue.splice(existingIndex, 1);
            }

            this._evaluationQueue.push({ name: name, value: value, pro: pro });

            if (!this._debouncedEvaluateOtherPropertyRequest) {
                this._debouncedEvaluateOtherPropertyRequest = this._debounce(function () {
                    var currentQueue = this._evaluationQueue.splice(0);
                    this._xhrHandler.xhrGet({
                        url: this._model.contentData.editablePreviewUrl
                    }).then(function (data) {
                        this._processOtherPropertyQueue(currentQueue, data);
                    }.bind(this));
                }.bind(this), 1000, false);
            }
            this._debouncedEvaluateOtherPropertyRequest();
        },

        _refreshDataModelProperty: function (name, value) {
            if (name) {
                var pro = this._model.metadata.properties.find(this._findPropertyByName(name));
                if (pro && (pro.modelType === "EPiServer.Core.ContentArea")) {
                    this._evaluateForOtherProperty(name, value, pro);

                } else if (pro && (name === 'iroutable_routesegment')) {
                    this._evaluateForRouteSegment(name, value, pro);

                } else {
                    this._evaluateForOtherProperty(name, value, pro);
                }
            }
        },

        updateCommandModel: function (model) {

            this.inherited(arguments);

            this._model = model;
            this._refreshDataModel();

            topic.publish("siteattention/modelchanged", model);
        }
    });
});
