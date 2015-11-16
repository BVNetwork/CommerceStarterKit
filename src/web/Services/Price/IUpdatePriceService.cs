using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OxxCommerceStarterKit.Web.Services.Price
{
    public interface IUpdatePriceService
    {
        void UpdatePrice(string code, IEnumerable<PriceInfo> prices);

    }
}