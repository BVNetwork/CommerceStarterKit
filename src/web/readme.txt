Episerver.Labs.Cognitive - Vision DAM Enhancements
===================================================

This EXPERIMENTAL library significantly enhances the Digital Asset Management capabilities in Episerver CMS.
It works by enriching properties you define on your Image model with content retrieved from the Vision API of Microsoft Azure Cognitive Services.
For instance it can automatically tag images, describe them, identify text and people in the image and product smart thumbnails.
This can be useful for many things - including having the content indexed by Episerver Find and making the image more easily accessible to the editors.


=== Getting Started ===
After installing the Nuget package in your Episerver CMS 10 project, add the following key to your <appSettings> in Web.config:
    <add key="Vision:Key" value="ad1f95d8379d4975ac1d90389e8f5c8e" />

Replace the Key with a key you retrieve from the azure portal (create a Vision instance and fetch the key - the price starts at free!). 
For small experiments and demos feel free to use this one, however we cannot guarantee that it will be kept active.


=== Usage ===

Attach the Vision or SmartThumbnail attributes to your image media models in your code (derived from ImageData).
When new images are uploaded, the values will be populated. A scheduled job to ensure all images are populated is also included.

Here are some examples of how to attach the attributes (feel free to copy and place in your Image Model):

//Creates a descriptive text in english.
        [Vision(VisionType =VisionTypes.Description)] 
        public virtual string Description { get; set; }

//Assigns tags for the image in a comma separated list
        [Vision(VisionType=VisionTypes.Tags,Separator =",")]
        public virtual string Tags { get; set; }

//Also assigns tags, but this time to a string array
        [Vision(VisionType = VisionTypes.Tags)]
        [BackingType(typeof(PropertyStringList))]
        [Display(Order = 305)]
        [UIHint(Global.SiteUIHints.Strings)]
        public virtual string[] TagList { get; set; }

//Assigns image categories to a string array
        [Vision(VisionType = VisionTypes.Categories)]
        [BackingType(typeof(PropertyStringList))]
        [Display(Order = 305)]
        [UIHint(Global.SiteUIHints.Strings)]
        public virtual string[] ImageCategories { get; set; }

//True if the image contains adult content. Useful for moderation
        [Vision(VisionType = VisionTypes.Adult)]
        public virtual bool IsAdultContent { get; set; }

//True if the image contains racy content. USeful for moderation
        [Vision(VisionType = VisionTypes.Racy)]
        public virtual bool IsRacyContent { get; set; }

//True if the image is clipart
        [Vision(VisionType = VisionTypes.ClipArt)]
        public virtual bool IsClipArt { get; set; }

//True if the image is a line drawing
        [Vision(VisionType =VisionTypes.LineDrawing)]
        public virtual bool IsLineDrawing { get; set; }

//True if the image is black and white
        [Vision(VisionType = VisionTypes.BlackAndWhite)]
        public virtual bool IsBlackAndWhite { get; set; }

//Hex code of the main accent color in the image. Useful for adopting the design to match the image
        [Vision(VisionType= VisionTypes.AccentColor)]
        public virtual string AccentColor { get; set; }

//Hex code of the dominant background color
        [Vision(VisionType = VisionTypes.DominantBackgroundColor)]
        public virtual string DominantBackgroundColor { get; set; }

//Hex code of the foreground color
        [Vision(VisionType = VisionTypes.DominantForegroundColor)]
        public virtual string DominantForegroundColor { get; set; }

//A list of faces identified in the image with their age and gender. It's also possible to just extract ages or gender.
        [Vision(VisionType = VisionTypes.Faces)]
        [BackingType(typeof(PropertyStringList))]
        [Display(Order = 305)]
        [UIHint(Global.SiteUIHints.Strings)]
        public virtual string[] Faces { get; set; }

//Text recognized in the image
        [Vision(VisionType=VisionTypes.Text)]
        public virtual string TextRecognized { get; set; }

        [Vision(VisionType = VisionTypes.Text)]
        public virtual XhtmlString TextInPicture { get; set; }

//A smart thumbnail at a given size, focussing on the subject matter in the image. Most be a Blob. 
//Just like with other Blobs stored on an image media, you can access their image by getting the url to the image and appending /[blobname] in the url
        [ScaffoldColumn(false)]
        [SmartThumbnail(100,100)]
        public virtual Blob SmartThumbnail{ get; set; }




=== Use at your own risk ===
This is an EXPERIMENTAL open-source add-on. Please use at your own risk.
