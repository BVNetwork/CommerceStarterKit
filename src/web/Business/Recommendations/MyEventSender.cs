using System;
using System.Web;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce;
using EPiServer.Tracking.Core;

namespace OxxCommerceStarterKit.Web.Business.Recommendations
{
    [ServiceConfiguration(typeof(MyEventSender))]
    public class MyEventSender
    {
        private readonly ITrackingService _trackingService;
        private readonly HttpContextBase _httpContextBase;
        private readonly IUserDataService _userDataService;

        public MyEventSender(ITrackingService trackingService, HttpContextBase httpContextBase, IUserDataService userDataService)
        {
            _trackingService = trackingService;
            _httpContextBase = httpContextBase;
            _userDataService = userDataService;
        }
        public void Track()
        {
            int num;

            if (_httpContextBase.Request.Cookies["weddingblog"] != null && 
                int.TryParse(_httpContextBase.Request.Cookies["weddingblog"].Value, out num))
            {
                num++;
            }
            else {
                num = 1;
            }

            _httpContextBase.Response.Cookies.Remove("weddingblog");
            _httpContextBase.Response.Cookies.Set(new HttpCookie("weddingblog", num + ""));


            var trackingData = new TrackingData<VisitingEvent2>
            {
                EventType = "weddingblog",
                User = new UserData
                {
                    Name = _userDataService.GetUserName(_httpContextBase),
                    Email = _userDataService.GetUserEmail(_httpContextBase)
                },
                Value = "Visiting the Wedding Blog. (Visit number: " + 1 + ")",
                Payload = new VisitingEvent2 { MyString = "For tracking, you know!" }
            };
            _trackingService.Track(trackingData, _httpContextBase);
        }
    }

    public class VisitingEvent2
    {
        public string MyString { get; internal set; }
    }
}