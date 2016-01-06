using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace OxxCommerceStarterKit.Web.Models.Blocks
{
    [ContentType(GUID = "c279b6d5-8569-9166-7cf1-a121edc963c9",
       DisplayName = "Recommended Products",
       Description = "",
       GroupName = WebGlobal.GroupNames.Commerce
       )]
    public class RecommendedProductsBlock : BlockData
    {
        [Display(Name = "Number of products",
               Description = "The number of products to show in the list. Default is 6.",
               Order = 9)]
        [CultureSpecific]
        public virtual int MaxCount { get; set; }
    }
}