using System.Collections.Generic;
using EPiServer.Security;
using EPiServer.Shell;
using EPiServer.Shell.Navigation;

namespace OxxCommerceStarterKit.Web.modules._protected.Campaign
{
    [MenuProvider]
    public class CampaignMenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            var campaign = new SectionMenuItem("Campaign", "/global/campaign")
            {
                IsAvailable = (request) => PrincipalInfo.CurrentPrincipal.IsInRole("CommerceAdmins"),
                SortIndex = 300
            };

            var overview = new UrlMenuItem("Overview", "/global/campaign/overview", $"{Paths.ToResource("SiteModules", "campaign")}/index/")
            {
                SortIndex = 10,
                IsAvailable = (request) => PrincipalInfo.CurrentPrincipal.IsInRole("CommerceAdmins")
            };

            return new MenuItem[]
            {
                campaign,
                overview
            };
        }
    }
}