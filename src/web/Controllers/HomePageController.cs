/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    [TemplateDescriptor()]
    public class HomePageController : PageControllerBase<HomePage>
    {

        private readonly ProductService _productService;
        private readonly IRecommendationsService _recommendationsService;

        public HomePageController(IContentLoader contentLoader, ProductService productService, IRecommendationsService recommendationsService)
        {
            _productService = productService;
            _recommendationsService = recommendationsService;
        }

        public ViewResult Index(HomePage currentPage)
        {
            var virtualPath = String.Format("~/Views/{0}/Index.cshtml", currentPage.GetOriginalType().Name);
            if (System.IO.File.Exists(Request.MapPath(virtualPath)) == false)
            {
                virtualPath = "Index";
            }

            var model = new HomePageViewModel(currentPage);

            var result = _recommendationsService.GetRecommendationsForHomePage(HttpContext)?.ToList() ?? new List<Recommendation>();
            if (result.Any())
                model.RecommendationsForHomePage = _productService.GetProductListViewModels(result, 3).ToList();

            var editHints = ViewData.GetEditHints<Chrome, HomePage>();
            editHints.AddConnection(c => c.GlobalFooterContent, p => p.GlobalFooterContent);

            return View(virtualPath, model);
        }
    }
}
