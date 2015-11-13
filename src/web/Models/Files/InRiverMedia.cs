using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using inRiver.EPiServerCommerce.Import.ResourceModels;
using inRiver.EPiServerCommerce.Interfaces;

namespace OxxCommerceStarterKit.Web.Models.Files
{
    [ContentType(GUID = "1A89E464-56D4-449F-AEA8-2BF774AB8731")]
    [MediaDescriptor(ExtensionString = "url")]
    public class UrlFile : MediaData, IInRiverResource
    {
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
                        //ResourceFileId = int.Parse(resourceMetaField.Values.First().value);
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
                }
            }
        }
    }

    [ContentType(GUID = "2A89E464-56D4-449F-AEA8-2BF774AB8732")]
    [MediaDescriptor(ExtensionString = "wmv")]
    public class InRiverVideoFile : VideoData, IInRiverResource
    {
        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>
        /// The copyright.
        /// </value>
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
                        //ResourceFileId = int.Parse(resourceMetaField.Values.First().value);
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

                }
            }
        }
    }
}

