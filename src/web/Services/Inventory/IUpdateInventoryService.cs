using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OxxCommerceStarterKit.Web.Services.Inventory
{
    public interface IUpdateInventoryService
    {
        void UpdateInventory(IEnumerable<InventoryInfo> inventoyInfoList);

    }
}