using System;
using System.Collections.Generic;
using System.Web;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using EPiServer.Web;
using EPiServer.Web.PageExtensions;

namespace OxxCommerceStarterKit.Web.Business.UI
{
    [ServiceConfiguration(ServiceType = typeof (IQuickNavigatorItemProvider))]
    public class AdminModeQuickNavigator : IQuickNavigatorItemProvider
    {
        private int _sortOrder;

        public AdminModeQuickNavigator()
        {
            _sortOrder = 100;
        }

        public int SortOrder
        {
            get { return _sortOrder; }
            private set { _sortOrder = value; }
        }

        public IDictionary<string, QuickNavigatorMenuItem> GetMenuItems(ContentReference currentContent)
        {
            string adminModeUiLink = VirtualPathUtility.Combine(Paths.ProtectedRootPath,
                "CMS/Admin/default.aspx");
            var menuItems = new Dictionary<string, QuickNavigatorMenuItem>();
            menuItems.Add("qn-admin-mode", new QuickNavigatorMenuItem("Admin Mode", adminModeUiLink, null, null, null));
            return menuItems;

        }

        
    }
}