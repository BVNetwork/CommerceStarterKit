/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Castle.Core.Internal;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace OxxCommerceStarterKit.Web.Models.Catalog.Base
{
    public class ProductBase : ProductContent
    {

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 2,
            Name = "Description")]
        [CultureSpecific]
        public virtual XhtmlString Description { get; set; }

        [Display(
            GroupName = SystemTabNames.Settings,
            Order = 10000,
            Name = "inRiver Entity Id")]
        [CultureSpecific(true)]
        public virtual int inRiverEntityId { get; set; }



        public override void SetDefaultValues(ContentType contentType)
        {
            PropertyInfo[] properties = GetType().BaseType.GetProperties();
            foreach (var property in properties)
            {
                var defaultValueAttribute = property.GetAttribute<DefaultValueAttribute>();
                if (defaultValueAttribute != null)
                {
                    this[property.Name] = defaultValueAttribute.Value;
                }
            } base.SetDefaultValues(contentType);
        }
    }
}
