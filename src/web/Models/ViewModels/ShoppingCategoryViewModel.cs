/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels.Simple;

namespace OxxCommerceStarterKit.Web.Models.ViewModels
{
    public class ShoppingCategoryViewModel : PageViewModel<ShoppingCategoryPage>
    {
        public ShoppingCategoryViewModel(ShoppingCategoryPage currentPage)
            : base(currentPage)
        {
            
        }
        public string CommerceRootCategoryName { get; set; }
        public string CommerceCategoryIds { get; set; }
		public string ParentName { get; set; }

        public int NumberOfProductsToShow { get; set; }


        public IEnumerable<ShoppingCategoryPage> CategoryPages { get; set; }
        public FeatureProductViewModel MenuFeatureProduct { get; set; }
        public List<ProductListViewModel> Recommendations { get; set; }
    }
}
