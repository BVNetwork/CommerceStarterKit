using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Security;
using EPiServer.Shell;
using EPiServer.Shell.Navigation;

namespace OxxCommerceStarterKit.Web.modules._protected.Perform
{
    [MenuProvider]
    public class PerformMenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            var perform = new SectionMenuItem("Perform", "/global/perform")
            {
                IsAvailable = (request) => PrincipalInfo.CurrentPrincipal.IsInRole("CommerceAdmins"),
                SortIndex = 300
            };

            var overview = new UrlMenuItem("Overview", "/global/perform/overview", $"{Paths.ToResource("SiteModules", "perform")}/index/")
            {
                SortIndex = 10,
                IsAvailable = (request) => PrincipalInfo.CurrentPrincipal.IsInRole("CommerceAdmins")
            };

            return new MenuItem[]
            {
                perform,
                overview
            };
        }
    }
}