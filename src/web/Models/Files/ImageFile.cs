/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using inRiver.EPiServerCommerce.Import.ResourceModels;
using inRiver.EPiServerCommerce.Interfaces;
using OxxCommerceStarterKit.Core;

namespace OxxCommerceStarterKit.Web.Models.Files
{
    [ContentType(GUID = "EE3BD195-7CB0-4756-AB5F-E5E223CD9820")]
    [MediaDescriptor(ExtensionString = "jpg,jpeg,jpe,ico,gif,bmp,png")]
    public class ImageFile : ImageData, IInRiverResource
    {
        public static class ImageWidths
        {
            public const string NARROW = "?preset=imagenarrow";
            public const string HALF = "?preset=imagehalf";
            public const string WIDE = "?preset=imagewide";
            public const string FULL = "?preset=imagefull";
        }

        public static class BoxSizes
        {
            public const string NARROW = "?preset=boxnarrow";
            public const string HALF = "?preset=boxhalf";
            public const string WIDE = "?preset=boxwide";
            public const string FULL = "?preset=boxfull";
        }

        public static class NewsletterWidths
        {
            public const string NARROW = "?preset=newsletternarrow";
            public const string HALF = "?preset=newsletterhalf";
            public const string WIDE = "?preset=newsletterwide";
            public const string FULL = "?preset=newsletterfull";
        }

        public static class Thumbnails
        {
            public const string Normal = "?preset=thumbnail";
            public const string Large = "?preset=largethumbnail";
        }

        public virtual String Description { get; set; }
        public virtual Url Link { get; set; }
        public virtual String Copyright { get; set; }
        public virtual String VideoUrl { get; set; }


        [Display(
            Name = "HotSpot configuration",
            Description = "HotSpot editor",
            GroupName = "HotSpots"
            )]
        [Searchable(false)]
        [UIHint(Constants.UIHint.HotSpotsEditor)]
        public virtual String HotSpotSettings { get; set; }

        /// <summary>
        /// Gets or sets the large thumbnail used by Commerce UI
        /// </summary>
        /// <remarks>
        /// You can also inherit from CommerceMedia
        /// </remarks>
        [Editable(false)]
        [ImageDescriptor(Width = 256, Height = 256)]
        public virtual Blob LargeThumbnail { get; set; }


        public virtual int ResourceFileId { get; set; }

        public virtual int EntityId { get; set; }

        [UIHint(UIHint.Textarea)]
        public virtual string ResourceDescriptionEN { get; set; }
        [UIHint(UIHint.Textarea)]
        public virtual string ResourceDescriptionSV { get; set; }
        [UIHint(UIHint.Textarea)]
        public virtual string ResourceDescriptionNO { get; set; }

        public virtual string ResourceName { get; set; }
        public virtual string ResourceType { get; set; }
        public virtual string ResourceFileName { get; set; }
        public virtual string ResourceMimeType { get; set; }
        [UIHint(UIHint.Textarea)]
        public virtual string ResourceImageMap { get; set; }
        

        public void HandleMetaData(List<ResourceMetaField> metaFields)
        {
            foreach (ResourceMetaField resourceMetaField in metaFields)
            {
                switch (resourceMetaField.Id)
                {
                    case "ResourceName":
                        ResourceName = resourceMetaField.Values.First().value;
                        break;

                    case "ResourceFileId":
                        ResourceFileId = int.Parse(resourceMetaField.Values.First().value);
                        break;

                    case "ResourceType":
                        ResourceType = resourceMetaField.Values.First().value;
                        break;

                    case "ResourceFilename":
                        ResourceFileName = resourceMetaField.Values.First().value;
                        break;

                    case "ResourceMimeType":
                        ResourceMimeType = resourceMetaField.Values.First().value;
                        break;

                    case "ResourceDescription":
                        ResourceDescriptionEN = resourceMetaField.Values.Find(d => d.languagecode == "en").value;
                        ResourceDescriptionSV = resourceMetaField.Values.Find(d => d.languagecode == "sv").value;
                        ResourceDescriptionNO = resourceMetaField.Values.Find(d => d.languagecode == "no").value;

                        break;
                    case "ResourceImageMap":
                        ResourceImageMap = resourceMetaField.Values.First().value;
                        break;
                }

            }

        }


    }

}
