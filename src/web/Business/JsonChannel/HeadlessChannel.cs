using System.Web;
using EPiServer.Web;

namespace OxxCommerceStarterKit.Web.Business.JsonChannel
{
    public class HeadlessChannel : DisplayChannel
    {
        public override string ChannelName
        {
            get
            {
                return "Headless";
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Headless";
            }
        }

        public override bool IsActive(HttpContextBase context)
        {
            return context.Request.QueryString["headless"] != null;
        }
    }
}