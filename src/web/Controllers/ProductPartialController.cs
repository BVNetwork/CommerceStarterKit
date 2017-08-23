/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Framework.Web.Mvc;
using OxxCommerceStarterKit.Web.Models.ViewModels;

using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    [RequireClientResources]
    public class ProductPartialController : BasePartialContentController<CatalogContentBase>
    {
        private readonly ProductService _productService;

        public ProductPartialController(ProductService productService)
        {
            _productService = productService;
        }

        public override ActionResult Index(CatalogContentBase currentContent)
        {
            const string baseViewName = "ProductPartials/_ProductListView";
            ProductListViewModel productListViewModel = null;

            if (currentContent is IProductListViewModelInitializer)
            {
                productListViewModel = _productService.GetProductListViewModel((IProductListViewModelInitializer)currentContent);
                

                var actionResult = GetPartialViewForTag(baseViewName, productListViewModel);
                if (actionResult != null) return actionResult;


            }

            return PartialView(baseViewName, productListViewModel);
        }       
    }
}
