using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediachase.Commerce.InventoryService;

namespace OxxCommerceStarterKit.Core.Services
{
    public interface IDefaultInventoryService
    {
        InventoryRecord GetForDefaultWarehouse(string code);
        InventoryRecord Get(string code, string warehouseCode);
    }
}
