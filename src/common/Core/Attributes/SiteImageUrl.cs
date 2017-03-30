/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer.DataAnnotations;

namespace OxxCommerceStarterKit.Core.Attributes
{
    /// <summary>
    /// Attribute to set the default thumbnail for site page and block types
    /// </summary>
    public class SiteImageUrl : ImageUrlAttribute
    {
        private const string BasePath = "~/Content/Images/EditorThumbnails/";
        /// <summary>
        /// The parameterless constructor will initialize a SiteImageUrl attribute with a default thumbnail
        /// </summary>
        public SiteImageUrl()
            : this(EditorThumbnail.Content)
        {

        }

        public SiteImageUrl(string path)
            : base(path)
        {

        }

        public SiteImageUrl(EditorThumbnail thumbnail)
            : base(BasePath + thumbnail.ToString() + "-thumbnail.png")
        {
        }
    }

    /// <summary>
    /// Predefined editor thumbnails for ease of use
    /// </summary>
    public enum EditorThumbnail
    {
        Content,
        Commerce,
        Multimedia,
        Social,
        System,
        Newsletter,
        CommerceGreen
    }
}
