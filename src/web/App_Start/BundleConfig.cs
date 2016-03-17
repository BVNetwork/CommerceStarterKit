/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Web.Optimization;

namespace OxxCommerceStarterKit.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/libraries/jquery-2.1.0.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
						"~/Scripts/libraries/jquery-ui-1.10.4.custom.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/libraries/jquery.validate.min.js",
						"~/Scripts/libraries/jquery.validate.unobtrusive.min.js"
                        //, "~/Scripts/libraries/jquery.unobtrusive-ajax.min.js"
						));
            bundles.Add(new ScriptBundle("~/bundles/jqueryaddons").Include(
                "~/Scripts/libraries/jquery.touchSwipe.min.js",
                "~/Content/js/plugins/jquery.placeholder.js",
                "~/Content/js/plugins/jquery.stellar.min.js",
                "~/Content/js/plugins/jquery.shuffle.min.js"));

			// Note! Bootstrap needs to run after jQuery, or we'll get trouble
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/libraries/bootstrap.min.js",
				"~/Scripts/libraries/bootstrap-lightbox.js"));

			bundles.Add(new ScriptBundle("~/bundles/frontpage").Include(
				"~/Scripts/js/components/FrontPage.js"));

			bundles.Add(new ScriptBundle("~/bundles/checkoutpage").Include(
				"~/Scripts/js/components/CheckoutPage.js",
				"~/Scripts/js/components/Registration.js"));

			bundles.Add(new ScriptBundle("~/bundles/registration").Include(
				"~/Scripts/js/components/Registration.js"));

            // Note! Some of the included scripts comes from the Bushido template
            // and we want to be able to update them without merging changes, so
            // we keep them separate.
			bundles.Add(new ScriptBundle("~/bundles/general").Include(
                "~/Content/js/plugins/smoothscroll.js",
                "~/Content/js/plugins/icheck.min.js",
                "~/Content/js/plugins/lightGallery.min.js",
                "~/Content/js/plugins/lightslider.js",
                "~/Content/js/plugins/owl.carousel.min.js",
                "~/Content/js/plugins/masterslider.min.js",
                "~/Scripts/libraries/accounting.min.js", // http://openexchangerates.github.io/accounting.js
				"~/Scripts/js/Oxx/ObjectUtils.js",
				"~/Scripts/js/Oxx/AjaxUtils.js",
				"~/Scripts/js/components.js",
                "~/Scripts/js/starterkit.js",
                "~/Scripts/js/components/login.js",
				"~/Scripts/js/components/Product.js",
				"~/Scripts/js/components/WebComponent/HotSpotImage.js",
				"~/Scripts/js/components/WebComponent/HotSpot.js",
				"~/Scripts/js/components/ProductDialog.js",
				"~/Scripts/js/components/SizeGuideDialog.js",
				"~/Scripts/js/components/HelpDialog.js"));

            // Note - //"~/Content/bootstrap.min.css", // Part of the template
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/Content/css/jqueryui/jquery-ui-1.10.4.custom.min.css",
                "~/Content/masterslider/style/masterslider.css",
                "~/Content/css/flexslider/flexslider.css",
                "~/Content/css/lightslider.css")
                // Original Bushido template  + Fix font-references
                .Include("~/Content/less/styles.css", new CssRewriteUrlTransform())
                // Our Overrides
                .Include("~/Content/less/epicphoto.css")); 
                
            bundles.Add(new ScriptBundle("~/bundles/angular_app").IncludeDirectory(
                "~/Scripts/app/", "*.js",true
                ));
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
               "~/Scripts/angular.js",
               "~/Scripts/angular-resource.js",
               "~/Scripts/libraries/ui-bootstrap-tpls-0.10.0.js"
               ));
        }
    }
}
