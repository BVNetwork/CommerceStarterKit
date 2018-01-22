/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.ComponentModel.DataAnnotations;
//using Episerver.Labs.Cognitive.Attributes;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using OxxCommerceStarterKit.Core;

namespace OxxCommerceStarterKit.Web.Models.Files
{
    [ContentType(GUID = "EE3BD195-7CB0-4756-AB5F-E5E223CD9820")]
    [MediaDescriptor(ExtensionString = "jpg,jpeg,jpe,ico,gif,bmp,png")]
    public class ImageFile : ImageData
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

    //    public virtual String Description { get; set; }
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

        //Creates a descriptive text in english.
      //  [Vision(VisionType = VisionTypes.Description)]
        [UIHint(UIHint.Textarea)]
        public virtual string Description { get; set; }

        //Assigns tags for the image in a comma separated list
      //  [Vision(VisionType = VisionTypes.Tags, Separator = ",")]
        [UIHint(UIHint.Textarea)]
        [Display(Order = 310)]
        public virtual string Tags { get; set; }

        //A list of faces identified in the image with their age and gender. It's also possible to just extract ages or gender.
      //  [Vision(VisionType = VisionTypes.Faces, Separator = ",")]
        [UIHint(UIHint.Textarea)]
        [Display(Order = 305)]
        public virtual string Faces { get; set; }


        //True if the image contains adult content. Useful for moderation
      //  [Vision(VisionType = VisionTypes.Adult)]
        [Display(Order = 310)]
        public virtual bool IsAdultContent { get; set; }

        //True if the image contains racy content. USeful for moderation
      //  [Vision(VisionType = VisionTypes.Racy)]
        [Display(Order = 320)]
        public virtual bool IsRacyContent { get; set; }

        //True if the image is clipart
      //  [Vision(VisionType = VisionTypes.ClipArt)]
        [Display(Order = 330)]
        public virtual bool IsClipArt { get; set; }

        //True if the image is a line drawing
      //  [Vision(VisionType = VisionTypes.LineDrawing)]
        [Display(Order = 340)]
        public virtual bool IsLineDrawing { get; set; }

        //True if the image is black and white
     //   [Vision(VisionType = VisionTypes.BlackAndWhite)]
        [Display(Order = 350)]
        public virtual bool IsBlackAndWhite { get; set; }

        ////Hex code of the main accent color in the image. Useful for adopting the design to match the image
        //[Vision(VisionType = VisionTypes.AccentColor)]
        //public virtual string AccentColor { get; set; }

        //Hex code of the dominant background color
      //  [Vision(VisionType = VisionTypes.DominantBackgroundColor)]
        [Display(Order = 355)]
        public virtual string DominantBackgroundColor { get; set; }

        //Hex code of the foreground color
      //  [Vision(VisionType = VisionTypes.DominantForegroundColor)]
        [Display(Order = 360)]
        public virtual string DominantForegroundColor { get; set; }


        //Text recognized in the image
      //  [Vision(VisionType = VisionTypes.Text)]
        [Display(Order = 370)]
        public virtual string TextRecognized { get; set; }

      //  [Vision(VisionType = VisionTypes.Text)]
        [Display(Order = 380)]
        public virtual XhtmlString TextInPicture { get; set; }
    }

}
