using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;

namespace OxxCommerceStarterKit.Interfaces
{
    public interface IRecommendedProductsService
    {
        IRecommendations GetRecommendedProducts(EntryContentBase catalogEntry, string userId, int maxCount);

        IRecommendations GetRecommendedProducts(string userId, int maxCount, CultureInfo cultureInfo);

        IRecommendations GetRecommendedProductsByCategory(string userId, List<string> categories, int maxCount, CultureInfo cultureInfo);

        IRecommendations GetRecommendedProductsForCart(string userId, IEnumerable<string> productCodes, int maxCount, CultureInfo cultureInfo);

        Dictionary<string, double> GetScoreForItems(int maxCount = 10000);

    }
}
