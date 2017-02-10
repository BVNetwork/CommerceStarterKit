using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using OxxCommerceStarterKit.Web.Models.Blocks.Base;
using OxxCommerceStarterKit.Core.Attributes;

namespace OxxCommerceStarterKit.Web.Models.Blocks
{
    [ContentType(DisplayName = "Section Countdown", 
        GUID = "4638490e-72bf-4fb1-a3d5-9f5e24e58d32", 
        Description = "Count down block",
        GroupName = WebGlobal.GroupNames.Campaign)]
    [SiteImageUrl(thumbnail: EditorThumbnail.Multimedia)]
    public class SectionCountdownBlock : SectionBlockBase
    {
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 10,
            Description = "Header for countdown block")]
        public virtual string Header { get; set; }

        [Required]
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 20)]

        public virtual DateTime TargetDate { get; set; }

    }
}