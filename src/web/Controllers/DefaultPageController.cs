/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Framework.DataAnnotations;
using EPiServer.GoogleAnalytics.Helpers;
using EPiServer.Security;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Business.Recommendations;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    public class DefaultPageController : PageControllerBase<PageData>
    {
		private readonly IContentLoader _contentLoader;

        private readonly MyEventSender _myEventSender;

        public DefaultPageController(IContentLoader contentLoader, MyEventSender myEventSender )
        {            
			_contentLoader = contentLoader;
            _myEventSender = myEventSender;
        }

        public ViewResult Index(PageData currentPage)
        {
            _myEventSender.Track();

            var viewPath = GetViewForPageType(currentPage);

            var model = CreatePageViewModel(currentPage);

            return View(viewPath, model);
        }

        /// <summary>
        /// Used by the javascript popup to show content in overlays on a page.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="currentPage">The current page.</param>
        /// <returns></returns>
		[HttpPost]
		public ActionResult Get(int reference, HomePage currentPage)
		{
			currentPage = currentPage ?? _contentLoader.Get<HomePage>(ContentReference.StartPage);

			IContent page = _contentLoader.Get<IContent>(new ContentReference(reference));
            
            // Filter away stuff you're not allowed to see
            FilterContentForVisitor filter = new FilterContentForVisitor();
		    if(filter.ShouldFilter(page) == true)
                return new HttpNotFoundResult();

			if (page is ArticlePage)
			{
				var model = new PageViewModel<ArticlePage>((ArticlePage)page);
				return View("~/Views/ArticlePage/Index.cshtml", model);
			}
            return new HttpNotFoundResult();
		}
    }
}
