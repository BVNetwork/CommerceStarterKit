using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using OxxCommerceStarterKit.Core.Attributes;
using EPiServer.Web;
using EPiServer.Shell.ObjectEditing;
using OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories;

namespace OxxCommerceStarterKit.Web.Models.Blocks
{
    [ContentType(DisplayName = "SectionBlock", 
        GUID = "9a5824e4-6fb7-4d1b-9efd-0c4f655fde70",
        GroupName = WebGlobal.GroupNames.Campaign)]
    [SiteImageUrl(thumbnail: EditorThumbnail.Multimedia)]
    public class SectionBlock : SiteBlockData
    {
        [CultureSpecific]
        [Display(
            Name = "Header",
            Description = "Section header",
            GroupName = SystemTabNames.Content,
            Order = 10)]
        public virtual String Header { get; set; }


        [Display(
            Name = "Left main body",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 510)]
        public virtual XhtmlString LeftMainBody { get; set; }

        [Display(
            Name = "Right main body",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 520)]
        public virtual XhtmlString RightMainBody { get; set; }

        [Display(
            Name = "Left content area",
            Description = "Full width bottom area",
            GroupName = SystemTabNames.Content,
            Order = 600)]
        public virtual ContentArea BottomLeftContentArea { get; set; }

        [Display(
            Name = "Right content area",
            Description = "Full width bottom area",
            GroupName = SystemTabNames.Content,
            Order = 620)]
        public virtual ContentArea BottomRightContentArea { get; set; }

        [Display(
            Name = "Wide content area",
            Description = "Full width bottom area",
            GroupName = SystemTabNames.Content,
            Order = 630)]
        public virtual ContentArea BottomWideContentArea { get; set; }

        [Display(
            Name = "Main body",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 640)]
        public virtual XhtmlString MainBody { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 530)]
        [SelectOne(SelectionFactoryType = typeof(ColorSelectionFactory))]
        public virtual string Color { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Image",
            Description = "Section image",
            GroupName = SystemTabNames.Content,
            Order = 20)]
        [UIHint(UIHint.Image)]
        public virtual ContentReference Image { get; set; }


    }
}