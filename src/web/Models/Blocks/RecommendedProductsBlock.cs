﻿using System;
using System.Collections.Generic;
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

    }
}