using EPiServer.ServiceLocation;
using inRiver.EPiServerCommerce.Interfaces;

namespace OxxCommerceStarterKit.Web.Business.Pim
{
    [ServiceConfiguration(ServiceType = typeof(IDeleteActionsHandler))]
    public class DeleteActionsHandler : IDeleteActionsHandler
    {
        public void PreDeleteCatalog(int catalogId)
        {
        }

        public void PostDeleteCatalog(int catalogId)
        {
        }

        public void PreDeleteCatalogNode(int catalogNodeId, int catalogId)
        {
        }

        public void PostDeleteCatalogNode(int catalogNodeId, int catalogId)
        {
            // Remove from index if nodes are indexed
        }

        public void PreDeleteCatalogEntry(int catalogEntryId, int metaClassId, int catalogId)
        {
        }

        public void PostDeleteCatalogEntry(int catalogEntryId, int metaClassId, int catalogId)
        {
            // Remove from index
        }

        public void PreDeleteResource(IInRiverImportResource resource)
        {
        }

        public void PostDeleteResource(IInRiverImportResource resource)
        {
        }
    }
}