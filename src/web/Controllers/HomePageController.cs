/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using OxxCommerceStarterKit.Web.Business.Recommendations;
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
        private readonly IRecommendationService _recommendationService;

        public HomePageController(IContentLoader contentLoader, ProductService productService, IRecommendationService recommendationService)
        {
            _productService = productService;
            _recommendationService = recommendationService;
        }

        public ViewResult Index(HomePage currentPage)
        {
            var virtualPath = String.Format("~/Views/{0}/Index.cshtml", currentPage.GetOriginalType().Name);
            if (System.IO.File.Exists(Request.MapPath(virtualPath)) == false)
            {
                virtualPath = "Index";
            }

            var model = new HomePageViewModel(currentPage);

            var result = _recommendationService.GetRecommendationsForHomePage(HttpContext, currentPage);
            if (result != null)
            {
                model.RecommendationsForHomePage = _productService.GetProductListViewModels(result, 3).ToList();
            }

            var editHints = ViewData.GetEditHints<Chrome, HomePage>();
            editHints.AddConnection(c => c.GlobalFooterContent, p => p.GlobalFooterContent);

            return View(virtualPath, model);
        }
    }
}
