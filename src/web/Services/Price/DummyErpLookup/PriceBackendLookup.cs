using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using CommerceStarterKit.CatalogImporter.DTO;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace OxxCommerceStarterKit.Web.Services.Price.DummyErpLookup
{
    [ServiceConfiguration(typeof(IPriceLookupService))]
    public class PriceBackendLookup : IPriceLookupService
    {
        private PriceRoot _priceRoot = null;

        public PriceBackendLookup()
        {
            ReadPricesFromBackend("priceData.json");
        }

        public void ReadPricesFromBackend(string appDataRelativePath)
        {

            string path = HttpContext.Current.Server.MapPath("~/App_Data/" + appDataRelativePath);
            _priceRoot = ReadDataFromFile(path);

            // \App_Data\priceData.json
            
        }

        public IEnumerable<PriceInfo> GetPricesForVariation(string code)
        {
            if(_priceRoot == null)
            {
                throw new NullReferenceException("PriceRoot must be loaded first.");
            }

            IEnumerable<PriceInfo> priceInfos = _priceRoot.Prices.Where(p => p.Code == code).Select(p => new PriceInfo()
            {
                Currency = p.Currency,
                Price = p.Price,
                MarketId = p.Market
            });

            return priceInfos;
        }

        protected PriceRoot ReadDataFromFile(string path)
        {
            if (File.Exists(path))
            {
                var prices = JsonConvert.DeserializeObject<PriceRoot>(File.ReadAllText(path));

                foreach (var priceInfo in prices.Prices)
                {
                    if (string.IsNullOrEmpty(priceInfo.Market))
                    {
                        priceInfo.Market = prices.DefaultMarket;
                    }
                    if (string.IsNullOrEmpty(priceInfo.Currency))
                    {
                        priceInfo.Currency = prices.DefaultCurrency;
                    }
                }
                return prices;
            }
            else
            {
                throw new FileNotFoundException("Cannot load prices from " + path);
            }
        }

        public class PriceRoot
        {
            public string DefaultMarket { get; set; }
            public string DefaultCurrency { get; set; }
            public PriceInfoJson[] Prices { get; set; }
        }

        public class PriceInfoJson
        {
            public string Code { get; set; }
            public decimal Price { get; set; }
            public string Currency { get; set; }
            public string Market { get; set; }
        }

    }
}