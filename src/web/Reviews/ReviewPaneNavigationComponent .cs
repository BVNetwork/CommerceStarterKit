using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Shell.ViewComposition;

namespace OxxCommerceStarterKit.Web.Reviews
{
    [Component]
    public class ReviewPaneNavigationComponent : ComponentDefinitionBase
    {
        public ReviewPaneNavigationComponent()
            : base("epi-cms/component/SharedBlocks")
        {
            PlugInAreas = new[] { "/episerver/cms/assets/defaultgroup", "/episerver/commerce/assets/defaultgroup" };
            Categories = new[] { "commerce", "cms", "content" };
            LanguagePath = "/components/review";
            SortOrder = 11090;
            Settings.Add(new Setting("repositoryKey", ReviewPaneDescriptor.RepositoryKey));
        }
    }
}