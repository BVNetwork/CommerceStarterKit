using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using inRiver.EPiServerCommerce.Interfaces;
using Mediachase.Commerce.Extensions;
using Mediachase.Commerce.Inventory;
using OxxCommerceStarterKit.Web.Services.Inventory;
using OxxCommerceStarterKit.Web.Services.Price;

namespace OxxCommerceStarterKit.Web.Business.Pim
{
    [ServiceConfiguration(ServiceType = typeof(ICatalogImportHandler))]
    public class PimPostImportHandler : ICatalogImportHandler
    {
        private static readonly ILogger _logger = EPiServer.Logging.LogManager.GetLogger();
        public PimPostImportHandler()
        {
            
        }

        public void PreImport(XDocument catalog)
        {
            _logger.DebugBeginMethod("Import from PIM started");
        }

        /// <summary>
        /// Process all incoming entries, check inventory and index them
        /// </summary>
        /// <param name="catalog"></param>
        public void PostImport(XDocument catalog)
        {
            _logger.DebugBeginMethod("Post Processing PIM import");
            Stopwatch tmr = Stopwatch.StartNew();
            
            List<string> variationCodes = new List<string>();
            List<string> entryCodes = new List<string>();

            XElement entriesElement = catalog.XPathSelectElement("/Catalogs/Catalog/Entries");
            foreach (XElement element in entriesElement.Elements())
            {
                string entryType = null;
                if (TryGetStringValueForElement(element, "EntryType", out entryType))
                {
                    string code = null;
                    if (TryGetStringValueForElement(element, "Code", out code))
                    {
                        // We want to index everything
                        entryCodes.Add(code);

                        // Find all varations, since those are the ones we want to check inventory for
                        if (string.Compare(entryType, "Variation", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            variationCodes.Add(code);
                        }
                    }
                }
            }

            _logger.Debug("Parsed catalog import, found {0} products and {1} variations", entryCodes.Count, variationCodes.Count);

            try
            {
                UpdateInventoryForVariations(variationCodes);
                tmr.Stop();

                _logger.Debug("Imported inventory for {0} variations in {1}sec {2}ms", variationCodes.Count,
                    tmr.Elapsed.Seconds, tmr.Elapsed.Milliseconds);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to add inevntory for variations", e);
            }

            tmr.Start();

            try
            {
                UpdatePricesForVariations(variationCodes);

                tmr.Stop();
                _logger.Debug("Imported prices for {0} variations in {1}sec {2}ms", variationCodes.Count,
                                tmr.Elapsed.Seconds, tmr.Elapsed.Milliseconds);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to add prices for variations", e);
            }
            
        }

        private void UpdatePricesForVariations(List<string> variationCodes)
        {
            IUpdatePriceService priceService = ServiceLocator.Current.GetInstance<IUpdatePriceService>();
            // TODO: Get real prices from ERP or other backend service

            foreach (string code in variationCodes)
            {
                List<PriceInfo> priceInfos = new List<PriceInfo>();
                priceInfos.Add(new PriceInfo()
                {
                    MarketId = "default",
                    Currency = "usd",
                    Price = 199
                });
                priceService.UpdatePrice(code, priceInfos);
            }
        }

        private void UpdateInventoryForVariations(List<string> variationCodes)
        {
            IUpdateInventoryService invService = ServiceLocator.Current.GetInstance<IUpdateInventoryService>();
            List<InventoryInfo> invInfos = new List<InventoryInfo>();

            // TODO: Get real inventory from ERP or other backend service

            foreach (string code in variationCodes)
            {
                invInfos.Add(new InventoryInfo()
                {
                    Code = code,
                    Inventory = 1000,
                    InventoryStatus = InventoryTrackingStatus.Disabled
                });
            }

            invService.UpdateInventory(invInfos);

        }

        protected bool TryGetStringValueForElement(XElement element, string elemName, out string value)
        {
            var elem = element.Element(elemName);
            if (elem != null)
            {
                if (string.IsNullOrEmpty(elem.Value) == false)
                {
                     value = elem.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}