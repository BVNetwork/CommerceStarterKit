/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class ErrorController : PageControllerBase<PageData>
    {
        // GET: Error404
        [BVNetwork.NotFound.Core.NotFoundPage.NotFoundPage]
        public ActionResult Error404()
        {
            ErrorPageViewModel model = GetViewModel();

            // The Action Filter will add the following to the ViewBag:
            // Referrer, NotFoundUrl and StatusCode
            model.NotFoundUrl = GetSafeUrlFromViewBag(ViewBag.NotFoundUrl);
            model.Referer = GetSafeUrlFromViewBag(ViewBag.Referrer);

            return View("Error404", model);
        }

        private Uri GetSafeUrlFromViewBag(dynamic uri)
        {
            string url = uri;
            if (string.IsNullOrEmpty(url) == false)
            {
                Uri urlParsed = null;
                try
                {
                    urlParsed = new Uri(url);
                }
                catch 
                {
                    // Someone could hand-craft something nasty in there, we won't show it 
                }
                return urlParsed;
            }
            return null;
        }

        private static ErrorPageViewModel GetViewModel()
		{
			ErrorPageViewModel model;
			try
			{
				var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();				

				model = CreateErrorPageViewModel(contentLoader.Get<HomePage>(ContentReference.StartPage));
				model.HasDatabase = true;
			}
			catch (Exception)
			{
				model = new ErrorPageViewModel();
			}
			return model;
		}

        private static ErrorPageViewModel CreateErrorPageViewModel(HomePage pageData)
        {
            var model = new ErrorPageViewModel();
            model.CurrentPage = pageData;            
            return (ErrorPageViewModel)model;
        }

		public ActionResult Error500(Exception exception = null)
		{
			ErrorPageViewModel model = GetViewModel();

			if (exception != null && !(exception is OperationCanceledException || exception is TaskCanceledException) && (
				HttpContext == null || (HttpContext != null && !HttpContext.Request.Url.ToString().EndsWith("/find_v2/"))))
			{
				_log.Error(exception.Message,exception);
			}



			return View("Error500", model);
		}
    }
}
