/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Web.Controllers;


namespace OxxCommerceStarterKit.Web
{
    public class WebGlobal : EPiServer.Global
    {
        /// <summary>
        /// IMPORTANT! This needs to be static as that ensures it runs
        /// before the EPiServer.Global class constructor. In that one
        /// the initialization engine kicks in and loads modules that
        /// requires the databases to be available
        /// </summary>
        static WebGlobal()
        {
            // TODO: Remove this when you are not going to use LocalDb anymore.
            ILogger log = LogManager.GetLogger();
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\db\");
            log.Debug("Setting data directory for Local DB to: " + dir.FullName);
            AppDomain.CurrentDomain.SetData("DataDirectory", dir.FullName);
        }

        protected override void RegisterRoutes(RouteCollection routes)
        {
            base.RegisterRoutes(routes);


            //routes.IgnoreRoute("{*browserlink}", new { browserlink = @".*/arterySignalR/ping" });

            RouteTable.Routes.MapRoute(null, "Cart/GetDeliveryLocations", new { controller = "Cart", action = "GetDeliveryLocations" });
            RouteTable.Routes.MapRoute("defaultRoute", "{controller}/{action}");
            RouteTable.Routes.MapRoute("defaultRouteWithLanguage", "{language}/{controller}/{action}");
        }

        protected void Application_Start()
        {


            RegisterApis(GlobalConfiguration.Configuration);

            var options = ServiceLocator.Current.GetInstance<DisplayOptions>();
            options
                .Add("full", "/displayoptions/full", ContentAreaTags.FullWidth, "", "epi-icon__layout--full")
                .Add("wide", "/displayoptions/wide", ContentAreaTags.TwoThirdsWidth, "", "epi-icon__layout--two-thirds")
                .Add("half", "/displayoptions/half", ContentAreaTags.HalfWidth, "", "epi-icon__layout--half")
                .Add("narrow", "/displayoptions/narrow", ContentAreaTags.OneThirdWidth, "", "epi-icon__layout--one-third");

            AreaRegistration.RegisterAllAreas();

            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            JsonSerializerSettings serializerSettings = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings;
            serializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            // Security - remove information headers
            MvcHandler.DisableMvcResponseHeader = true;

        }


        /// <summary>
        /// Registers the WebAPI routes.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void RegisterApis(HttpConfiguration config)
        {
          

            config.Routes.MapHttpRoute(
             "Api", // Route name 
             "api/{controller}/{action}/{id}", // URL with parameters 
             new { id = RouteParameter.Optional } // Parameter defaults 
            );

            config.Routes.MapHttpRoute(
             "LanguageAwareApi", // Route name 
             "{language}/api/{controller}/{action}/{id}", // URL with parameters 
             new { id = RouteParameter.Optional } // Parameter defaults
            );

            // We only support JSON
            var appXmlType = GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
        }


#if !DEBUG
		protected void Application_Error()
		{
			var exception = Server.GetLastError();

            // We still need to log everything
            ILogger log = LogManager.GetLogger();
            log.Error("Handling exception: ", exception);

			Response.Clear();
			Server.ClearError();

			var routeData = new RouteData();
			routeData.Values["controller"] = "Error";
			routeData.Values["action"] = "Error500";
			routeData.Values["exception"] = exception;
			Response.StatusCode = 500;
			Response.TrySkipIisCustomErrors = true;

			var httpException = exception as HttpException;
			if (httpException != null)
			{
				Response.StatusCode = httpException.GetHttpCode();
				switch (Response.StatusCode)
				{
					//case 403:
					//	routeData.Values["action"] = "Http403";
					//	break;
					case 404:
						routeData.Values["action"] = "Error404";
						break;
				}
			}

			IController errorsController = new ErrorController();
			var rc = new RequestContext(new HttpContextWrapper(Context), routeData);
			errorsController.Execute(rc);

		}
#endif

        /// <summary>
        /// Tags to use for the main widths used in the Bootstrap HTML framework
        /// </summary>
        public static class ContentAreaTags
        {
            public const string Slider = "slidertag";
            public const string FullWidth = "col-sm-12";
            public const string TwoThirdsWidth = "col-sm-8";
            public const string HalfWidth = "col-sm-6";
            public const string OneThirdWidth = "col-sm-4";
            public const string NoRenderer = "norenderer";
            public const string RelatedProducts = "relatedProducts";
        }

        public static class GroupNames
        {
            public const string Default = "Default";
            public const string Content = "Content";
            public const string Campaign = "Campaign";
            public const string Section = "Section";
            public const string Commerce = "Commerce";
            public const string Contact = "Contact";
            public const string MetaData = "Metadata";
            public const string SiteSettings = "SiteSettings";
            public const string Specialized = "Specialized";
            public const string Analytics = "Analytics";
        }

        public static class SiteUIHints
        {
            public const string Strings = "StringList";
            public const string MediaUrl = "MediaUrl";
        }

        /// <summary>
        /// Virtual path to folder with static graphics, such as "~/Static/gfx/"
        /// </summary>
        public const string StaticGraphicsFolderPath = "~/Content/images/";
        public const string NoImageUrl = "/globalassets/system/no-image.png";
    }
}
