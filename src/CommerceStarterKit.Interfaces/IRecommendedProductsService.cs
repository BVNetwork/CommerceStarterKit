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
        IEnumerable<IContent> GetRecommendedProducts(EntryContentBase catalogEntry, string userId, int maxCount);

        IEnumerable<IContent> GetRecommendedProducts(string userId, int maxCount, CultureInfo cultureInfo);

        IEnumerable<IContent> GetRecommendedProductsByCagetory(string userId, List<string> categories, int maxCount, CultureInfo cultureInfo);
    }
}
