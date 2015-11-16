using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Inventory;

namespace OxxCommerceStarterKit.Web.Services.Inventory
{
    [ServiceConfiguration(typeof(IUpdateInventoryService))]
    public class UpdateInventoryService : IUpdateInventoryService
    {
        private readonly IWarehouseInventoryService _warehouseInventoryService;
        private readonly IWarehouseRepository _warehouseRepository;

        public UpdateInventoryService(IWarehouseInventoryService warehouseInventoryService, IWarehouseRepository warehouseRepository)
        {
            _warehouseInventoryService = warehouseInventoryService;
            _warehouseRepository = warehouseRepository;
        }

        public void UpdateInventory(IEnumerable<InventoryInfo> inventoyInfoList)
        {
            var inventoryService = _warehouseInventoryService;
            
            foreach (InventoryInfo inventoryInfo in inventoyInfoList)
            {
                UpdateInventoryForEntry(inventoryService, inventoryInfo);
            }
        }

        private void UpdateInventoryForEntry(IWarehouseInventoryService inventoryService, InventoryInfo invInfo)
        {
            IWarehouseRepository warehouseRepository = _warehouseRepository;
            var warehouse = warehouseRepository.Get(invInfo.Warehouse);

            CatalogKey key = new CatalogKey(AppContext.Current.ApplicationId, invInfo.Code);
            var existingInventory = inventoryService.Get(key, warehouse);

            WarehouseInventory inv;
            if (existingInventory != null)
            {
                inv = new WarehouseInventory(existingInventory);
            }
            else
            {
                inv = new WarehouseInventory();
                inv.WarehouseCode = warehouse.Code;
                inv.CatalogKey = key;
            }
            
            inv.InventoryStatus = invInfo.InventoryStatus;

            // Skip saving if the inventory is the same
            if (inv.InStockQuantity != invInfo.Inventory)
            {
                inv.InStockQuantity = invInfo.Inventory;
                inventoryService.Save(inv);
            }
        }
    }

    public class InventoryInfo
    {
        public InventoryInfo()
        {
            Warehouse = "default";
            Inventory = 0;
            InventoryStatus = InventoryTrackingStatus.Disabled;
        }
        public string Code { get; set; }
        public decimal Inventory { get; set; }
        public string Warehouse { get; set; }
        public InventoryTrackingStatus InventoryStatus { get; set; }
    }
}