using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Pricing;

namespace OxxCommerceStarterKit.Web.Services.Price
{
    [ServiceConfiguration(typeof(IUpdatePriceService))]
    public class UpdatePriceService : IUpdatePriceService
    {
        private readonly ILogger _log;
        private readonly IPriceService _priceService;

        public UpdatePriceService(ILogger logger, IPriceService priceService)
        {
            _log = logger;
            _priceService = priceService;
        }

        public void UpdatePrice(string code, IEnumerable<PriceInfo> prices)
        {
            if (code == null) throw new ArgumentNullException("code");
            if (prices == null)
                return;

            CatalogKey key = new CatalogKey(AppContext.Current.ApplicationId, code);

            var catalogEntryPrices = _priceService.GetCatalogEntryPrices(key); //.ToList();
            List<IPriceValue> priceValues = new List<IPriceValue>(catalogEntryPrices);

            foreach (PriceInfo price in prices)
            {
                // Already there?
                IPriceValue priceValue = priceValues.FirstOrDefault(p => p.MarketId.Value == price.MarketId);
                if (priceValue == null)
                {
                    // No - add it
                    PriceValue newPrice = new PriceValue()
                    {
                        CatalogKey = key,
                        MarketId = price.MarketId,
                        UnitPrice = new Money(price.Price, new Currency(price.Currency)),
                        ValidFrom = DateTime.Now,
                        CustomerPricing = CustomerPricing.AllCustomers,
                        MinQuantity = 0

                    };
                    priceValues.Add(newPrice);
                }
                else
                {
                    // We don't touch prices for the same market
                }
            }
            _log.Debug("Saving {0} prices for {1}", priceValues.Count, code);
            // Save prices back, overwriting anything there
            _priceService.SetCatalogEntryPrices(key, priceValues);
        }

    }


}