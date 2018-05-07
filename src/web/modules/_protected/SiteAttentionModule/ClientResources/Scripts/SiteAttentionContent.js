define([
    "dojo",
    "dojo/_base/declare",
    "epi/_Module",
    "epi/dependency",
    "epi/routes",
    "epi/shell/_ContextMixin",
    "siteattention/CommandProviders/SiteAttentionCommandProvider"
],

function (
    e,
    t,
    n,
    r,
    i,
    s,
    o
) {
    return t("siteattention.SiteAttentionContent", [s], {
        initialize: function () {
            e.when(this.getCurrentContext(), function (e) { });
        }
    });
});