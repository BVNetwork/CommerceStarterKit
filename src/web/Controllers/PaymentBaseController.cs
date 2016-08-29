/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Orders;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class PaymentBaseController<T> : PageControllerBase<T> where T : PageData
    {
        /// <summary>
        /// Expires any products where no variants have any inventory. This effectively means there
        /// is nothing to sell for this product.
        /// </summary>
        /// <param name="expirationCandidates">The expiration candidates.</param>
        /// <param name="contentRepository">The content repository.</param>
        protected static void ExpireProductsWithNoInventory(HashSet<ProductContent> expirationCandidates, IContentRepository contentRepository)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var linksRepository = ServiceLocator.Current.GetInstance<ILinksRepository>();
            var languageSelector = ServiceLocator.Current.GetInstance<LanguageSelector>();
            var warehouseInventoryService = ServiceLocator.Current.GetInstance<IWarehouseInventoryService>();

            foreach (var p in expirationCandidates)
            {
                // TODO: Perform quality check on this. Not well tested, and we swallow the exception
                try
                {
                    var variants =
                        contentLoader.GetChildren<VariationContent>(p.ContentLink)
                            .Concat(
                                contentLoader.GetItems(p.GetVariantRelations(linksRepository).Select(x => x.Target),
                                    languageSelector).OfType<VariationContent>());

                    // If no variants for a product has inventory, expire the product
                    if (!variants.Any(v =>
                    {
                        var catalogKey = new CatalogKey(Mediachase.Commerce.Core.AppContext.Current.ApplicationId, v.Code);

                        return warehouseInventoryService.List(catalogKey).Any(inventory => inventory.InStockQuantity > 0);
                    }))
                    {
                        var writableClone = (ProductContent)p.CreateWritableClone();

                        writableClone.StopPublish = DateTime.Now;
                        contentRepository.Save(writableClone, SaveAction.Publish, AccessLevel.NoAccess);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Cannot expire products with no inventory", ex);
                }
            }
        }
    }
}
