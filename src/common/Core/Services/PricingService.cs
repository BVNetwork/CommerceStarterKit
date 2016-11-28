using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using OxxCommerceStarterKit.Core.Facades;

namespace OxxCommerceStarterKit.Core.Services
{
    [ServiceConfiguration(typeof(IPricingService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PricingService : IPricingService
    {
        private readonly IPriceService _priceService;
        private readonly ICurrentMarket _currentMarket;
        private readonly AppContextFacade _appContext;

        public PricingService(IPriceService priceService,
            ICurrentMarket currentMarket, 
            AppContextFacade appContext)
        {
            _priceService = priceService;
            _currentMarket = currentMarket;
            _appContext = appContext;
        }

        public IList<IPriceValue> GetPriceList(string code, MarketId marketId, PriceFilter priceFilter)
        {
            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            var catalogKey = new CatalogKey(_appContext.ApplicationId, code);

            return _priceService.GetPrices(marketId, DateTime.Now, catalogKey, priceFilter)
                .OrderBy(x => x.UnitPrice.Amount)
                .ToList();
        }

        public IList<IPriceValue> GetPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId, PriceFilter priceFilter)
        {
            if (catalogKeys == null)
            {
                throw new ArgumentNullException("catalogKeys");
            }

            if (!catalogKeys.Any())
            {
                return Enumerable.Empty<IPriceValue>().ToList();
            }

            return _priceService.GetPrices(marketId, DateTime.Now, catalogKeys, priceFilter)
                .OrderBy(x => x.UnitPrice.Amount)
                .ToList();
        }

        public Money? GetCurrentPrice(string code)
        {
            var market = _currentMarket.GetCurrentMarket();
            return GetPrice(code, market.MarketId, market.DefaultCurrency);
        }

        public Money? GetPrice(string code, MarketId marketId, Currency currency)
        {
            var prices = GetPriceList(code, marketId,
                new PriceFilter
                {
                    Currencies = new[] { currency }
                });

            return prices.Any() ? prices.First().UnitPrice : (Money?)null;
        }
    }
}