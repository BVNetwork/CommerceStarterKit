using System;
using System.Collections.Generic;
using System.Linq;
using CommerceStarterKit.CatalogImporter.DTO;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Provider;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Pricing;
using Price = CommerceStarterKit.CatalogImporter.DTO.Price;

namespace CommerceStarterKit.CatalogImporter
{
    [ServiceConfiguration]
    public class EntryImporter : ImporterBase
    {
        private readonly UrlResolver _urlResolver;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IWarehouseInventoryService _inventoryService;
        private readonly IPriceService _priceService;

        public EntryImporter(IContentRepository contentRepository,
            ReferenceConverter referenceConverter, IContentTypeRepository typeRepository,
            ILogger logger, UrlResolver urlResolver,
            IWarehouseRepository warehouseRepository,
            IWarehouseInventoryService inventoryService,
            IPriceService priceService)
            : base(contentRepository, referenceConverter, typeRepository, logger)
        {
            _urlResolver = urlResolver;
            _warehouseRepository = warehouseRepository;
            _inventoryService = inventoryService;
            _priceService = priceService;
        }

        public void Import(List<Entry> entries)
        {
            // TODO: Iterate over list trying to find parents as we go, the
            // first implementation requires the list to be ordered so we can
            // find the parent as we go.
            foreach (Entry entry in entries)
            {
                _log.Debug("Starting importing entry '{0}' ({1})", entry.name, entry.code);

                // Find it's parent, we need it in order to add it
                var parent = GetNodeContent(entry.parent);

                if (parent == null)
                {
                    throw new Exception("Cannot find parent content with code: " + entry.parent);
                }

                // See if it exist first
                var content = GetEntryContent(entry.code);
                if (content != null)
                {
                    // Existing content
                    _log.Debug("Starting importing EXISTING entry '{0}' ({1})", entry.name, entry.code);
                    content = (EntryContentBase)content.CreateWritableClone();
                    var publishAndClearAction = SaveAction.Publish.SetExtendedActionFlag(ExtendedSaveAction.ClearVersions);
                    Update(content, entry, publishAndClearAction);
                }
                else
                {
                    var parentNode = GetNodeContent(entry.parent);

                    // Get type of node
                    var entryType = GetContentType(entry.contentType);

                    // New content
                    _log.Debug("Starting importing NEW entry '{0}' ({1}) of type {2}", entry.name, entry.code, entryType.Name);

                    CreateNew(parentNode.ContentLink, entry, entryType);
                }
            }
        }


        public ContentReference CreateNew(ContentReference parentNodeLink, Entry entry, ContentType nodeType)
        {
            //Create a new instance of CatalogContentTypeSample that will be a child to the specified parentNode.
            var content = _contentRepository.GetDefault<EntryContentBase>(parentNodeLink, nodeType.ID);

            // Set required properties
            content.Code = entry.code;
            return Update(content, entry, SaveAction.Publish);
        }


        public ContentReference Update(EntryContentBase content, Entry entry, SaveAction saveAction)
        {
            if (string.IsNullOrEmpty(entry.urlSegment) == false)
            {
                content.RouteSegment = entry.urlSegment;
            }
            else
            {
                content.RouteSegment = entry.code.ToLowerInvariant();
            }

            //Set the Name
            content.Name = entry.name;
            content.DisplayName = entry.name;

            // Override with language
            var displayName = GetPropertyValue(entry.properties, content.Language.IetfLanguageTag, "DisplayName");
            if (string.IsNullOrEmpty(displayName) == false)
            {
                content.DisplayName = displayName;
            }

            SetProperties(content, entry.properties);


            // Configure Variation
            ConfigureVariationDefaults(content as VariationContent);

            // Add media
            AddMedia(content, entry);

            //Publish the new content and return its ContentReference.
            var contentReference = _contentRepository.Save(content, saveAction, AccessLevel.NoAccess);

            // Set default inventory
            if (Defaults.variationDefaultInventoryStock > 0)
            {
                SetDefaultInventory(entry.code, Defaults.variationDefaultInventoryStock);
            }

            // Set prices
            SetPrices(entry.code, entry.prices);

            return contentReference;
        }

        protected void SetProperties(EntryContentBase content, List<Property> properties)
        {
            if (properties == null)
                return;

            // Only look at properties for the current language
            foreach (Property importProperty in properties.Where(p => (string.IsNullOrEmpty(p.language) || p.language == content.Language.IetfLanguageTag)))
            {
                PropertyData propertyData = content.Property[importProperty.name];
                if(propertyData != null && 
                    (propertyData.Type == PropertyDataType.String || propertyData.Type == PropertyDataType.LongString))
                {
                    _log.Debug("Setting value for {0} ({1})", importProperty.name, importProperty.language ?? "null");
                    propertyData.Value = importProperty.value;
                }
            }
        }

        protected void ConfigureVariationDefaults(VariationContent variationContent)
        {
            if (variationContent == null)
                return;

            _log.Debug("Configuring variation details - MinQuantity: {0}, MaxQuantity: {1}, TrackInventory: {2}",
                Defaults.variationMinQuantity, Defaults.variationMaxQuantity, Defaults.variationEnableTracking);

            if (Defaults.variationMinQuantity > 0)
            {
                variationContent.MinQuantity = Defaults.variationMinQuantity;
            }

            if (Defaults.variationMaxQuantity > 0)
            {
                variationContent.MaxQuantity = Defaults.variationMaxQuantity;
            }

            variationContent.TrackInventory = Defaults.variationEnableTracking;
        }

        private void AddMedia(EntryContentBase content, Entry entry)
        {
            if (entry.images != null && entry.images.Any())
            {
                for (int i = 0; i < entry.images.Count; i++)
                {
                    var imageLink = entry.images[i];
                    var path = imageLink.path;
                    var groupName = string.IsNullOrEmpty(imageLink.groupName) ? "default" : imageLink.groupName;

                    var existingMedia = _urlResolver.Route(new UrlBuilder(path)) as MediaData;

                    if (existingMedia != null)
                    {
                        // See if it is already linked
                        var commerceMedia = content.CommerceMediaCollection.FirstOrDefault(m => m.AssetLink.Equals(existingMedia.ContentLink));
                        if (commerceMedia == null)
                        {
                            // Attach media to this product
                            content.CommerceMediaCollection.Add(
                                new CommerceMedia()
                                {
                                    AssetLink = existingMedia.ContentLink,
                                    AssetType = "episerver.core.icontentimage",
                                    GroupName = groupName,
                                    SortOrder = i
                                });
                            _log.Debug("Attaching media '{0}' to {1}", path, content.Name);
                        }
                    }
                    else
                    {
                        // NOTE! We do not fail the import if the media does not exist
                        // TODO: Upload media to folder. Needs to make sure
                        // the path is correct
                    }
                }
            }
        }

        protected void SetDefaultInventory(string code, decimal inStockQuantity, string warehouseCode = "default")
        {
            _log.Debug("Setting stock for {0} to {1}", code, inStockQuantity);
            var warehouse = _warehouseRepository.Get(warehouseCode);
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            if (inStockQuantity == 0)
            {
                throw new ArgumentNullException("inStockQuantity", "InStockQuantity is required");
            }

            CatalogKey key = new CatalogKey(Mediachase.Commerce.Core.AppContext.Current.ApplicationId, code);



            var existingInventory = _inventoryService.Get(key, warehouse);

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
            inv.InStockQuantity = inStockQuantity;

            _inventoryService.Save(inv);
        }

        protected void SetPrices(string code, List<Price> prices)
        {
            if (code == null) throw new ArgumentNullException("code");
            if (prices == null)
                return;

            CatalogKey key = new CatalogKey(Mediachase.Commerce.Core.AppContext.Current.ApplicationId, code);

            var catalogEntryPrices = _priceService.GetCatalogEntryPrices(key); //.ToList();
            List<IPriceValue> priceValues = new List<IPriceValue>(catalogEntryPrices);

            foreach (Price price in prices)
            {
                // Already there?
                IPriceValue priceValue = priceValues.FirstOrDefault(p => p.MarketId.Value == price.marketId);
                if(priceValue == null)
                {
                    // No - add it
                    PriceValue newPrice = new PriceValue()
                    {
                        CatalogKey = key,
                        MarketId = price.marketId,
                        UnitPrice = new Money(price.price, new Currency(price.currency)),
                        ValidFrom = DateTime.Now, 
                        CustomerPricing = CustomerPricing.AllCustomers,
                        MinQuantity = 0

                    };
                    priceValues.Add(newPrice);
                }
                else
                {
                    // We don't touch prices for the same market
                }
            }
            _log.Debug("Saving {0} prices for {1}", priceValues.Count, code);
            // Save prices back
            _priceService.SetCatalogEntryPrices(key, priceValues);
        }
    }
}