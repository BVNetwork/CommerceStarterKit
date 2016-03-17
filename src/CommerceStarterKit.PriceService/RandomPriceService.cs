using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;

namespace OxxCommerceStarterKit.PriceService
{
    public class RandomPriceService : IPriceService
    {
        private readonly ILogger _logger;
        private readonly IMarketService _marketService;

        public RandomPriceService(ILogger logger, IMarketService marketService)
        {
            _logger = logger;
            _marketService = marketService;
            IsReadOnly = true;
        }

        public IPriceValue GetDefaultPrice(MarketId market, DateTime validOn, CatalogKey catalogKey, Currency currency)
        {
            return GetRandomPrice(market, catalogKey);
        }

        private static IPriceValue GetRandomPrice(MarketId market, CatalogKey catalogKey)
        {
            IMarketService marketService =
                ServiceLocator.Current.GetInstance<IMarketService>();
            IMarket marketImpl = marketService.GetMarket(market);
            string hashCode = catalogKey.CatalogEntryCode.GetHashCode().ToString();
            int dummyPrice;
            int.TryParse(hashCode.Substring(hashCode.Length - 3), out dummyPrice);

            PriceValue price = new PriceValue()
            {
                CatalogKey = catalogKey,
                MarketId = market,
                MinQuantity = 0,
                UnitPrice = new Money((decimal)dummyPrice, marketImpl.DefaultCurrency),
                ValidFrom = DateTime.Now.AddHours(-1),
                CustomerPricing = CustomerPricing.AllCustomers
            };

            return price;
        }

        public IEnumerable<IPriceValue> GetPrices(MarketId market, DateTime validOn, CatalogKey catalogKey, PriceFilter filter)
        {
            List<IPriceValue> prices = new List<IPriceValue>();
            prices.Add(GetRandomPrice(market, catalogKey));
            return prices;
        }

        public IEnumerable<IPriceValue> GetPrices(MarketId market, DateTime validOn, IEnumerable<CatalogKey> catalogKeys, PriceFilter filter)
        {
            List<IPriceValue> prices = new List<IPriceValue>();
            foreach (CatalogKey catalogKey in catalogKeys)
            {
                prices.Add(GetRandomPrice(market, catalogKey));
            }
            
            return prices;
        }

        public IEnumerable<IPriceValue> GetPrices(MarketId market, DateTime validOn, IEnumerable<CatalogKeyAndQuantity> catalogKeysAndQuantities, PriceFilter filter)
        {
            List<IPriceValue> prices = new List<IPriceValue>();
            foreach (var catalogKey in catalogKeysAndQuantities)
            {
                prices.Add(GetRandomPrice(market, catalogKey.CatalogKey));
            }

            return prices;
            
        }

        public IEnumerable<IPriceValue> GetCatalogEntryPrices(CatalogKey catalogKey)
        {
            List<IPriceValue> prices = new List<IPriceValue>();

            foreach (var allMarket in _marketService.GetAllMarkets())
            {
                prices.AddRange(GetPrices(allMarket.MarketId, DateTime.Now, catalogKey, null));
            }

            return prices;
        }

        public IEnumerable<IPriceValue> GetCatalogEntryPrices(IEnumerable<CatalogKey> catalogKeys)
        {
            List<IPriceValue> prices = new List<IPriceValue>();

            foreach (var allMarket in _marketService.GetAllMarkets())
            {
                prices.AddRange(GetPrices(allMarket.MarketId, DateTime.Now, catalogKeys, null));
            }

            return prices;
        }

        public void SetCatalogEntryPrices(CatalogKey catalogKey, IEnumerable<IPriceValue> priceValues)
        {
            
        }

        public void SetCatalogEntryPrices(IEnumerable<CatalogKey> catalogKeys, IEnumerable<IPriceValue> priceValues)
        {
            
        }

        public void ReplicatePriceDetailChanges(IEnumerable<CatalogKey> catalogKeys, IEnumerable<IPriceValue> priceValues)
        {
            
        }

        public IEnumerable<IPriceValue> GetPrices(MarketId market, DateTime validOn, IEnumerable<CatalogKey> catalogKeys, PriceFilter filter, int startIndex,
            int numberOfRecord, out int total)
        {
            total = 1;
            return GetPrices(market, validOn, catalogKeys, null);
        }

        public IEnumerable<IPriceValue> GetCatalogEntryPrices(IEnumerable<CatalogKey> catalogKeys, int startIndex, int numberOfRecord, out int total)
        {
            
            var priceValues = GetCatalogEntryPrices(catalogKeys);
            total = priceValues.Count();
            return priceValues;
        }

        public bool IsReadOnly { get; private set; }
    }
}