/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using OxxCommerceStarterKit.Web.Business.FacetRegistry;

namespace OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories
{
    public class FindProductFilterSortOrderFactory : GenericSelectionFactory
    {
        public override IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var selectionDictionary = new Dictionary<string, string>();
            
            selectionDictionary.Add("Relevance", "");
            selectionDictionary.Add("Popularity", "popularity");
            selectionDictionary.Add("Price Low - High", "priceAscending");
            selectionDictionary.Add("Price High - Low", "priceDescending");
            selectionDictionary.Add("Best review", "rating");

            return GetSelectionFromDictionary(selectionDictionary);
        }
    }
}
