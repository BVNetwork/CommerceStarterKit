using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace OxxCommerceStarterKit.Web.Models.Catalog.Base
{
    public class VariationContentBase: VariationContent
    {

        [Display(
            GroupName = SystemTabNames.Settings,
            Order = 10000,
            Name = "inRiver Entity Id")]
        [CultureSpecific(true)]
        public virtual int inRiverEntityId { get; set; }
    }
}