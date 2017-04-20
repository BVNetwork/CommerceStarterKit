var Recommendations = {
    init: function () {
        $(".container").find("[data-recommendation-id]").on("click",  Recommendations.clickTracking);
        $(".container").find("[data-recommendation-id]").on("mouseup", ".product > a", Recommendations.clickTracking);
        $(".container").find("[data-recommendation-id]").on("contextmenu", ".product > a", Recommendations.keyboardHandle);
        $(".container").find("[data-recommendation-id]").on("keydown", ".product > a, .product img", Recommendations.keyboardHandle);
    },

    clickTracking: function (evt) {
        var recommendationId = $(evt.delegateTarget).data("recommendation-id");
        console.log(recommendationId);
        Misc && Misc.setCookie("EPiServer_Commerce_RecommendationId", recommendationId, 60);//set lifetime of this cookie to only 1 minute.
    },

    keyboardHandle: function (evt) {
        if ((evt.type == "keydown" && evt.which == 13) || (evt.type == "contextmenu" && evt.which != 3)) {
            Recommendations.clickTracking(evt); //handle keyup event of enter key, contextmenu event of menu key.
        }
    }
};

var Misc = {
    init: function () {
        if (Misc.getCookie("AcceptedCookies") != 1) {
            $(document).on("click", ".jsCookies", Misc.acceptCookies);
            $(".jsCookies").show();
        }
    },
    acceptCookies: function () {
        Misc.setCookie("AcceptedCookies", "1", (365 * 24 * 60 * 60));
        $(".jsCookies").hide();
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
    },
    getCookie: function (cname) {
        var name = cname + "=";
        var ca = document.cookie.split(";");
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == " ") c = c.substring(1);
            if (c.indexOf(name) != -1) return c.substring(name.length, c.length);
        }
        return "";
    },
    updateValidation: function (formClass) {
        var currForm = $("." + formClass);
        currForm.removeData("validator");
        currForm.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(currForm);
        currForm.validate();
    }
};

$(document).ready(function () {

    Misc.init();
    Recommendations.init();

});