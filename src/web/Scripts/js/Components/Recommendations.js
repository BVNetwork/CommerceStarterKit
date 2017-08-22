var Recommendations = {
    init: function () {
        $(".container").find("[data-recommendation-id]").on("click",  Recommendations.clickTracking);
        $(".container").find("[data-recommendation-id]").on("mouseup", ".product > a", Recommendations.clickTracking);
        $(".container").find("[data-recommendation-id]").on("contextmenu", ".product > a", Recommendations.keyboardHandle);
        $(".container").find("[data-recommendation-id]").on("keydown", ".product > a, .product img", Recommendations.keyboardHandle);
    },

    clickTracking: function (evt) {
        var recommendationId = $(evt.delegateTarget).data("recommendation-id");
        Recommendations.setCookie("EPiServer_Perform_RecommendationId", recommendationId, 60);//set lifetime of this cookie to only 1 minute.
    },

    keyboardHandle: function (evt) {
        if ((evt.type == "keydown" && evt.which == 13) || (evt.type == "contextmenu" && evt.which != 3)) {
            Recommendations.clickTracking(evt); //handle keyup event of enter key, contextmenu event of menu key.
        }
    },

    setCookie: function (cname, cvalue, exSeconds) {
        //  Session cookie
        if (!exSeconds) {
            document.cookie = cname + "=" + cvalue + "; path=/";
            return;
        }

        var date = new Date();
        date.setTime(date.getTime() + (exSeconds * 1000));
        document.cookie = cname + "=" + cvalue + "; expires=" + date.toUTCString() + "; path=/";
    }
};

$(document).ready(function () {

    Recommendations.init();

});