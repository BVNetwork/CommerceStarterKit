/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Castle.Components.DictionaryAdapter;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Web.Mvc;
using Mediachase.Commerce;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Interfaces;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Business.Delivery;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;
using LineItem = Mediachase.Commerce.Orders.LineItem;

namespace OxxCommerceStarterKit.Web.Controllers
{

    [TemplateDescriptor(Inherited = true)]
    public class CartController : PageController<CartSimpleModulePage>
    {
        private readonly IPostNordClient _postNordClient;
        private readonly ProductService _productService;
        private readonly IContentLoader _contentLoader;


        public CartController(IPostNordClient postNordClient, 
            ProductService productService,
            IContentLoader contentLoader)
        {
            _postNordClient = postNordClient;
            _productService = productService;
            _contentLoader = contentLoader;
        }

        /// <summary>
        /// The main view for the cart.
        /// </summary>
        [Tracking(TrackingType.Basket)]
        public ViewResult Index(CartSimpleModulePage currentPage)
        {
            CartModel model = new CartModel(currentPage);

            // Get recommendations for the contents of the cart
            List<ContentReference> recommendedProductsForCart = new List<ContentReference>();

            recommendedProductsForCart = this.GetRecommendations()
                   .Where(x => x.Area == "basketWidget")
                   .SelectMany(x => x.RecommendedItems)
                   .ToList();

            PopulateRecommendations(model, recommendedProductsForCart, 3);

            Track(model);

            return View(model);
        }


        protected void PopulateRecommendations(CartModel model, List<ContentReference> recommendedProductsForCart, int maxCount = 6)
        {
            if (model.LineItems.Any())
            {               
                List<ProductListViewModel> recommendedProductList = new List<ProductListViewModel>();
                if (recommendedProductsForCart.Any())
                {
                    foreach (var product in recommendedProductsForCart.Where(x => x != null && x != ContentReference.EmptyReference).Select(x => _contentLoader.Get<CatalogContentBase>(x)).Take(3))
                    {
                        IProductListViewModelInitializer modelInitializer = product as IProductListViewModelInitializer;
                        if (modelInitializer != null)
                        {
                            var viewModel = _productService.GetProductListViewModel(modelInitializer);
                            // viewModel.TrackingName = recommendedProductsForCart.RecommenderName;
                            recommendedProductList.Add(viewModel);
                        }
                    }
                    model.Recommendations = recommendedProductList;
                    // model.RecommendationsTrackingName = recommendedProductsForCart.RecommenderName;
                }
            }
        }


        public async Task<JsonResult> GetDeliveryLocations(string streetAddress, string city, string postalCode)
        {
            string streetNumber = string.Empty;

            if (!string.IsNullOrEmpty(streetAddress))
            {
                streetAddress = streetAddress.TrimEnd();


                // Parse the street name and number (if any) from the street address

                var i = streetAddress.Length - 1;
                bool hasDigit = false;

                for (; 0 <= i; --i)
                {
                    if (char.IsDigit(streetAddress[i]))
                    {
                        hasDigit = true;
                        continue;
                    }

                    if (!char.IsLetter(streetAddress[i]))
                    {
                        break;
                    }
                }

                ++i;



                if (hasDigit && i < streetAddress.Length)
                {
                    streetNumber = streetAddress.Substring(i);
                    streetAddress = streetAddress.Substring(0, i).Trim();
                }
                else
                {
                    streetNumber = null;
                }
            }


            if (string.IsNullOrEmpty(postalCode))
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var x = (await _postNordClient.FindNearestByAddress(new RegionInfo("NO"), city, postalCode, streetAddress, streetNumber)).
                    Select(sp => new DeliveryLocation(
                        new ServicePoint() { Id = sp.Id, Name = sp.Name, Address = sp.DeliveryAddress.StreetName + ' ' + sp.DeliveryAddress.StreetNumber, City = sp.DeliveryAddress.City, PostalCode = sp.DeliveryAddress.PostalCode },
                        sp.Name));
                return Json(x, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }




        private void Track(CartModel model)
        {
            // Track Analytics. 
            // TODO: Remove when GA add-in is fixed
            GoogleAnalyticsTracking tracking = new GoogleAnalyticsTracking(ControllerContext.HttpContext);

            // Add the products
            int i = 1;
            foreach (LineItem lineItem in model.LineItems)
            {
                tracking.ProductAdd(code: lineItem.Code,
                    name: lineItem.DisplayName,
                    quantity: (int)lineItem.Quantity,
                    price: (double)lineItem.PlacedPrice,
                    position: i
                    );
                i++;
            }

            // Step 1 is to review the cart
            tracking.Action("checkout", "{\"step\":1}");
        }
    }
}
