/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.PlugIn;

namespace OxxCommerceStarterKit.Web.Models.CustomProperties
{
    [PropertyDefinitionTypePlugIn(Description = "Tag list", DisplayName = "Tag list")]
    public class PropertyTagList : PropertyLongString
    {
        public const string Separator = ",";

        public override Type PropertyValueType
        {
            get
            {
                return typeof(List<string>);
            }
        }

        public override object SaveData(PropertyDataCollection properties)
        {
            return LongString;
        }

        public override object Value
        {
            get
            {
                var value = base.Value as string;

                if (value == null)
                {
                    return null;
                }
                var brandids = value.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                return brandids;
            }
            set
            {
                if (value is List<string>)
                {
                    var tagList = value as List<string>;
                    var serialized = String.Join(Separator, tagList);
                    base.Value = serialized;
                }
                else
                {
                    base.Value = value;
                    
                }
            }
        }
    }
}
