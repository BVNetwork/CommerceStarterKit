using System;
using System.Web.Http;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Core.Services;

namespace OxxCommerceStarterKit.Web.Api
{
    [Authorize(Roles = "Authenticated")]
    public class MyOrdersController : BaseApiController
    {
        [HttpGet]
        public bool SendOrderReceipt(string trackingNumber)
        {
            if(string.IsNullOrEmpty(trackingNumber))
                throw new ArgumentNullException("trackingNumber", "trackingNumber is a rquired parameter.");

            IOrderService orderService = ServiceLocator.Current.GetInstance<IOrderService>();
            orderService.SendOrderReceipt(trackingNumber);
            return true;
        }
    }
}
