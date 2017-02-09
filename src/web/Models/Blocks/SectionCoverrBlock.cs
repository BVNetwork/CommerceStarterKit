using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Web;
using OxxCommerceStarterKit.Core.Attributes;
using EPiServer.Shell.ObjectEditing;
using OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories;

namespace OxxCommerceStarterKit.Web.Models.Blocks
{
    [ContentType(DisplayName = "Section Coverr Video Block", 
        GUID = "fcf3f21f-1bc2-4c9a-b6b0-3cb3db76db13", 
        Description = "Coverr Video Block",
        GroupName = WebGlobal.GroupNames.Campaign)]
    [SiteImageUrl(thumbnail: EditorThumbnail.Multimedia)]
    public class SectionCoverrBlock : SectionBlock
    {

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 10)]
        [CultureSpecific]
        [UIHint(UIHint.MediaFile)]
        public virtual ContentReference MP4 { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 20)]
        [CultureSpecific]
        [UIHint(UIHint.MediaFile)]
        public virtual ContentReference WEBM { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 30)]
        [CultureSpecific]
        [UIHint(UIHint.MediaFile)]
        public virtual ContentReference JPG { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 40,
            Description = "Background color behind Coverr video")]
        [SelectOne(SelectionFactoryType = typeof(ColorSelectionFactory))]
        public virtual string ColorCover { get; set; }

    }
}