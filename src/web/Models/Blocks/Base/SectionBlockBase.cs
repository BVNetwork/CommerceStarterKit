using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;
using OxxCommerceStarterKit.Web.EditorDescriptors.SelectionFactories;
using EPiServer.Web;

namespace OxxCommerceStarterKit.Web.Models.Blocks.Base
{
    [ContentType(DisplayName = "SectionBlockBase", GUID = "259969f6-b4a1-42e1-a667-5ad27a126a65", Description = "", AvailableInEditMode = false)]
    public class SectionBlockBase : SiteBlockData
    {
        [Display(
            GroupName = WebGlobal.GroupNames.Section,
            Order = 100,
            Description = "Background color for entire section")]
        [SelectOne(SelectionFactoryType = typeof(ColorSelectionFactory))]
        public virtual string ColorSection { get; set; }

        [Display(
            GroupName = WebGlobal.GroupNames.Section,
            Order = 200,
            Description = "Background color for text area")]
        [SelectOne(SelectionFactoryType = typeof(ColorSelectionFactory))]
        public virtual string ColorText { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Section background image",
            Description = "Image to cover entire section background",
            GroupName = WebGlobal.GroupNames.Section,
            Order = 300)]
        [UIHint(UIHint.Image)]
        public virtual ContentReference Image { get; set; }

        [Display(
            GroupName = WebGlobal.GroupNames.Section,
            Order = 400,
            Name = "Show arrow",
            Description = "Show down arrow link in section bottom")]
        [CultureSpecific]
        public virtual bool ShowArrow { get; set; }

        [Display(
            GroupName = WebGlobal.GroupNames.Section,
            Order = 500,
            Name = "Full height",
            Description = "Show section block with full height")]
        [CultureSpecific]
        public virtual bool FullHeight { get; set; }

    }
}