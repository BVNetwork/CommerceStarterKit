using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Core.Services;

namespace OxxCommerceStarterKit.Web.Business.Payment
{
    [Obsolete("This should be handled by standard activityflows or calculators")]
    public class StockUpdater : IStockUpdater
    {
        private static readonly ILogger Log = LogManager.GetLogger();
            
        public void AdjustStocks(PurchaseOrderModel order)
        {
            // TODO: Verify if you need custom inventory adjustment
            // The workflows will adjust the inventory for us, as long as inventory tracking
            // is enabled on the variation and the warehouse inventory for this variation. If you
            // want to refresh the inventory from a back-end system, do it here.
            
            // AdjustOrderInventoryFromWarehouse(order);
        }

        protected void AdjustOrderInventoryFromWarehouse(PurchaseOrderModel order)
        {
            var warehouseRepository = ServiceLocator.Current.GetInstance<IWarehouseRepository>();
            var warehousesCache = warehouseRepository.List();
            var warehouseInventory = ServiceLocator.Current.GetInstance<IDefaultInventoryService>();

            var expirationCandidates = new HashSet<ProductContent>();

            var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            // Adjust inventory
            foreach (var f in order.OrderForms)
            {
                foreach (var i in f.LineItems)
                {
                    try
                    {
                        var warehouse = warehousesCache.First(w => w.Code == i.WarehouseCode);
                        var catalogEntry = CatalogContext.Current.GetCatalogEntry((string) i.CatalogEntryId);
                        var catalogKey = new CatalogKey(catalogEntry);
                        var inventory = warehouseInventory.GetForDefaultWarehouse((string)i.CatalogEntryId);

                        if (inventory.PurchaseAvailableQuantity - i.Quantity <= 0)
                        {
                            var contentLink = referenceConverter.GetContentLink(i.CatalogEntryId);
                            var variant = contentRepository.Get<VariationContent>(contentLink);

                            expirationCandidates.Add((ProductContent) variant.GetParent());
                        }

                        // NOTE! Default implementation is to NOT save the inventory here,
                        // as it will subtract double if the workflows also do this.
                        // warehouseInventory.Save(inventory);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Unable to adjust inventory.", ex);
                    }
                }
            }

            // TODO: Determine if you want to unpublish products with no sellable variants
            ExpireProductsWithNoInventory(expirationCandidates, contentRepository);
            // Alterntive approach is to notify the commerce admin about the products without inventory
        }

        private void ExpireProductsWithNoInventory(HashSet<ProductContent> expirationCandidates, IContentRepository contentRepository)
        {
            // TODO: Custom implementation
        }
    }
}