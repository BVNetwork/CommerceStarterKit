using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.Commerce;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using OxxCommerceStarterKit.Core.Attributes;

namespace OxxCommerceStarterKit.Web.Models.Blocks
{
    [ContentType(GUID = "c279b6d5-8569-9166-7cf1-a121edc963c9",
       DisplayName = "Recommended Products",
       Description = "",
       GroupName = WebGlobal.GroupNames.Commerce
       )]
    [SiteImageUrl(thumbnail: EditorThumbnail.Commerce)]
    public class RecommendedProductsBlock : BlockData
    {
        [Display(Name = "Heading",
               Description = "",
               Order = 10)]
        [CultureSpecific]
        public virtual string Heading { get; set; }

        [Display(Name = "Category",
               Description = "",
               Order = 20)]
        [UIHint(UIHint.CatalogContent)]
        public virtual ContentReference Category { get; set; }

        [Display(Name = "Number of products",
               Description = "The number of products to show in the list. Default is 6.",
               Order = 30)]
        public virtual int MaxCount { get; set; }
    }
}