/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Provider;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;

namespace OxxCommerceStarterKit.Web.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class CommerceCachePrimer : IInitializableModule
    {
        private readonly ILogger _log = LogManager.GetLogger();

        public void Initialize(InitializationEngine context)
        {
            _log.Debug("Initializing Commerce Cache Primer");

            // Only run if enabled
            string statusFlag = ConfigurationManager.AppSettings["CachePrimer.Enabled"];
            if(string.Compare(statusFlag, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // Call this async, we don't need to wait for it
                Task.Run(() => PrimeCache());
                _log.Debug("Commerce Cache Primer is now running in background thread");
				_log.Debug("To disable the Commerce Cache Primer, add the key \"CachePrimer.Enabled\" with value \"false\" to appSettings.");
            }
            else
            {
                _log.Debug("Commerce Cache Primer is not enabled in appSettings. Add key \"CachePrimer.Enabled\" with value \"true\" to enable.");
            }

        }

        public void Preload(string[] parameters) { }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }

        private void PrimeCache()
        {
            _log.Debug("Now priming cache, getting all catalogs");
            Stopwatch tmr = Stopwatch.StartNew();

            try
            {
                // We do not want an exception in a background tree
                PrimeCacheImpl();
            }
            catch (Exception e)
            {
                _log.Error("Failed to prime cache.", e);
            }

            tmr.Stop();
            _log.Debug("Priming cache took: {0}ms", tmr.ElapsedMilliseconds);

        }

        protected void PrimeCacheImpl()
        {
            ICatalogSystem catalog = ServiceLocator.Current.GetInstance<ICatalogSystem>();
            IContentRepository repository = ServiceLocator.Current.GetInstance<IContentRepository>();
            ReferenceConverter referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            CatalogContentLoader contentLoader = ServiceLocator.Current.GetInstance<CatalogContentLoader>();

            // Get all catalogs
            CatalogDto catalogDto = catalog.GetCatalogDto();
            _log.Debug("Found {0} catalogs. Start iterating.", catalogDto.Catalog.Count);
            foreach (CatalogDto.CatalogRow catalogRow in catalogDto.Catalog)
            {
                _log.Debug("Loading all categories for catalog {0} ({1})", catalogRow.Name, catalogRow.CatalogId);
                // Get all Categories on first level
                CatalogNodes nodes = catalog.GetCatalogNodes(catalogRow.CatalogId,
                    new CatalogNodeResponseGroup(CatalogNodeResponseGroup.ResponseGroup.CatalogNodeInfo));
                _log.Debug("Loaded {0} categories using ICatalogSystem", nodes.CatalogNode.Count());
                // Get them as content too
                foreach (CatalogNode node in nodes.CatalogNode)
                {
                    ContentReference nodeReference = referenceConverter.GetContentLink(node.CatalogNodeId, CatalogContentType.CatalogNode, 0);
                    NodeContent content = repository.Get<EPiServer.Commerce.Catalog.ContentTypes.NodeContent>(nodeReference);
                    _log.Debug("Loded Category Content: {0}", content.Name);
                    WalkCategoryTree(content, repository, contentLoader, catalog, referenceConverter);
                }
            }

        }

        
        protected void WalkCategoryTree(NodeContent node, 
            IContentRepository repository, 
            CatalogContentLoader contentLoader, 
            ICatalogSystem catalog,
            ReferenceConverter referenceConverter)
        {
            // ReSharper disable PossibleMultipleEnumeration
            // Get all products
            Stopwatch tmr = Stopwatch.StartNew();
            IEnumerable<EntryContentBase> entries = repository.GetChildren<EPiServer.Commerce.Catalog.ContentTypes.EntryContentBase>(node.ContentLink);
            _log.Debug("Loaded {0} entries in category {1} using IContentRepository in {2}ms",
                            entries.Count(),
                            node.Name,
                            tmr.ElapsedMilliseconds);

            // Load and cache Entry objects. Still a lot of code that uses this
            tmr = Stopwatch.StartNew();
            foreach (EntryContentBase entryAsContent in entries)
            {
                // Load full entry
                int entryId = referenceConverter.GetObjectId(entryAsContent.ContentLink);
				// Catalog Gadget uses info
				//catalog.GetCatalogEntry(entryId,
				//	new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryInfo));
				catalog.GetCatalogEntry(entryId,
					new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryFull));
            }

			// Prime the catalog gadget
	        // IEnumerable<IContent> children = repository.GetChildren<IContent>(node.ContentLink, new LanguageSelector("en"), 0, int.MaxValue);
			// _log.Debug("Loaded {0} children", children.Count());

	        // .GetDescendents(node.ContentLink);
			
			tmr.Stop();
            _log.Debug("Loaded {0} entries in category {1} using ICatalogSystem in {2}ms", 
                            entries.Count(), 
                            node.Name,
                            tmr.ElapsedMilliseconds);

            // Get all products the way it is done in edit mode, but this does not seem to
            // use the cache.
            //int loadedEntries;
            //contentLoader.GetCatalogEntries(node.ContentLink, 0, int.MaxValue, out loadedEntries);
            //_log.Debug("Loaded {0} entries in category {1} using CatalogContentLoader", loadedEntries, node.Name);

            // Get child nodes the same way done by the UI
            IList<CatalogGetChildrenReferenceResult> catalogNodes = contentLoader.GetCatalogNodes(node.ContentLink);
            _log.Debug("Loaded {0} categories in category {1} using CatalogContentLoader", catalogNodes.Count, node.Name);
            foreach (GetChildrenReferenceResult catalogNode in catalogNodes)
            {
                NodeContent childNode = repository.Get<NodeContent>(catalogNode.ContentLink);
                WalkCategoryTree(childNode, repository, contentLoader, catalog, referenceConverter);
            }
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}
