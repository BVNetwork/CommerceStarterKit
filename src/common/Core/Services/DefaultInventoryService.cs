using EPiServer.ServiceLocation;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;

namespace OxxCommerceStarterKit.Core.Services
{
    [ServiceConfiguration(typeof(IDefaultInventoryService))]
    public class DefaultInventoryService : IDefaultInventoryService
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWarehouseRepository _warehouseRepository;

        public DefaultInventoryService(IInventoryService inventoryService, IWarehouseRepository warehouseRepository)
        {
            _inventoryService = inventoryService;
            _warehouseRepository = warehouseRepository;
        }
        public InventoryRecord GetForDefaultWarehouse(string code)
        {
            return _inventoryService.Get(code, _warehouseRepository.GetDefaultWarehouse().Code);
        }

        public InventoryRecord Get(string code, string warehouseCode)
        {
            return _inventoryService.Get(code, warehouseCode);
        }
    }
}