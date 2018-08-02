using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Events;
using EPiServer.Events.Clients;
using EPiServer.Find;
using EPiServer.Find.Framework;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog.Events;
using Mediachase.Commerce.Engine.Events;
using OxxCommerceStarterKit.Web.Models.Blocks.Contracts;
using OxxCommerceStarterKit.Web.Models.Catalog;
using OxxCommerceStarterKit.Web.Models.FindModels;

namespace OxxCommerceStarterKit.Web.Business.Search
{
    public class CatalogContentEventIndexer
    {
        private static readonly ILogger _log = LogManager.GetLogger();
        private readonly IContentEvents _contentEvents;

        /// <summary>
        /// Set this flag to temporary disable indexing while running long jobs. It is enabled by default.
        /// </summary>
        public static bool IndexingEnabled = true;


        public CatalogContentEventIndexer(IContentEvents contentEvents)
        {
            _contentEvents = contentEvents;
        }

        public virtual void EnableEventListeners()
        {
            _log.Debug("Listening to content indexing events");
            _contentEvents.PublishedContent += PublishedContent;

            Event.Get(CatalogEventBroadcaster.CommerceProductUpdated).Raised += CatalogEventUpdated;
            Event.Get(CatalogKeyEventBroadcaster.CatalogKeyEventGuid).Raised += CatalogKeyEventUpdated;

        }

        private void CatalogKeyEventUpdated(object sender, EventNotificationEventArgs eventNotificationEventArgs)
        {
            LogEventDetails(eventNotificationEventArgs, "Catalog Key Event");
        }

        private void CatalogEventUpdated(object sender, EventNotificationEventArgs eventNotificationEventArgs)
        {
            LogEventDetails(eventNotificationEventArgs, "Catalog Event");
        }


        protected void LogEventDetails(EventNotificationEventArgs eventNotificationEventArgs, string eventType)
        {
            if (_log.IsDebugEnabled())
            {
                string eventName = eventNotificationEventArgs.EventId.ToString();
                string subType = "";
                string ids = null;

                // Deserialize event, we need more data from it
                bool isRemoteEvent = !IsSelfRaised(eventNotificationEventArgs);
                byte[] param = eventNotificationEventArgs.Param as byte[];
                if (param == null)
                {
                    _log.Debug("{0} '{1}' raised. Cannot deserialize event since Param is null.", eventType, eventName);
                }
                EventArgs args = DeSerialize(param);
                if (args != null)
                {
                    CatalogKeyEventArgs keyArgs = args as CatalogKeyEventArgs;
                    if (keyArgs != null)
                    {
                        subType = "CatalogKey";
                        eventName = keyArgs.Name;
                        if(keyArgs.CatalogKeys != null && keyArgs.CatalogKeys.Any())
                        {
                            ids = string.Join(", ", keyArgs.CatalogKeys.Select(k => k.CatalogEntryCode));
                        }
                    }

                    CatalogContentUpdateEventArgs updateArgs = args as CatalogContentUpdateEventArgs;
                    if (updateArgs != null)
                    {
                        subType = "CatalogContentUpdate";
                        eventName = updateArgs.EventType;
                        if (updateArgs.CatalogEntryIds != null && updateArgs.CatalogEntryIds.Any())
                        {
                            ids = "Entries: " + string.Join(", ", updateArgs.CatalogEntryIds.Select(k => k));
                        }
                        if (updateArgs.CatalogNodeIds != null && updateArgs.CatalogNodeIds.Any())
                        {
                            if (string.IsNullOrEmpty(ids) == false)
                                ids += " ";
                            ids += "Nodes: " + string.Join(", ", updateArgs.CatalogNodeIds.Select(k => k));
                        }
                        if (updateArgs.CatalogAssociationIds != null && updateArgs.CatalogAssociationIds.Any())
                        {
                            if (string.IsNullOrEmpty(ids) == false)
                                ids += " ";
                            ids += "Associations: " + string.Join(", ", updateArgs.CatalogAssociationIds.Select(k => k));
                        }
                    }
                }

                if (string.IsNullOrEmpty(ids))
                    ids += "none";

                _log.Debug("{0} '{1}' raised with data: {2} (remote: {3}, subtype: {4})", eventType, eventName, ids, isRemoteEvent, subType);
            }
        }


        private bool IsSelfRaised(EventNotificationEventArgs e)
        {
            if (!(e.RaiserId == CatalogKeyEventBroadcaster.EventRaiserId))
            {
                return (bool)(e.RaiserId == CatalogEventBroadcaster.EventRaiserId);
            }
            return true;
        }

        private static EventArgs DeSerialize(byte[] buffer)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                return (formatter.Deserialize(stream) as EventArgs);
            }
        }

        /// <summary>
        /// Note! This method should not fail, it will prevent the product from 
        /// being saved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PublishedContent(object sender, ContentEventArgs e)
        {
            if (IndexingEnabled == false)
                return;


            if (e.Content is IIndexableContent)
            {
                try
                {
                    IndexProduct(e.Content as IIndexableContent);
                }
                catch (Exception exception)
                {
                    _log.Error("Cannot index: " + e.ContentLink.ToString(), exception);
                }
            }
            else if (e.Content is FashionItemContent)
            {
                EntryContentBase parent = GetParent(e.Content as EntryContentBase);

                FashionProductContent productContent = parent as FashionProductContent;
                if (productContent != null)
                {
                    try
                    {
                        IndexProduct(productContent);
                    }
                    catch (Exception exception)
                    {
                        _log.Error("Cannot index: " + e.ContentLink.ToString(), exception);
                    }

                }

            }
        }

        protected void IndexProduct(IIndexableContent p)
        {
            if (p.ShouldIndex())
            {
                var currentMarket = ServiceLocator.Current.GetInstance<Mediachase.Commerce.ICurrentMarket>().GetCurrentMarket();
                FindProduct findProduct = p.GetFindProduct(currentMarket);
                if (findProduct != null)
                {
                    _log.Debug("Indexing {0} - {1}", findProduct.Code, findProduct.Name);
                    IClient client = SearchClient.Instance;
                    client.Index(findProduct);
                }
            }
            else
            {
                //TODO: remove product from index
                _log.Debug("TODO: Remove from index {0}", p.Name);
                //IClient client = SearchClient.Instance;
                //var lang = p.Language;
                //client.Delete<FindProduct>(productContent.ContentLink.ID + "_" + (lang == null ? string.Empty : lang.Name));
            }
        }

        private EntryContentBase GetParent(CatalogContentBase content)
        {

            var relationRepository = ServiceLocator.Current.GetInstance<IRelationRepository>();
            IContentLoader contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

            IEnumerable<Relation> parentRelations = relationRepository.GetRelationsByTarget(content.ContentLink);
            if (parentRelations.Any())
            {
                Relation firstRelation = parentRelations.FirstOrDefault();
                if (firstRelation != null)
                {
                    var parentProductContent = contentLoader.Get<EntryContentBase>(firstRelation.Source);
                    return parentProductContent;
                }
            }
            return null;
        }


    }
}