using System;
using System.Web;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce;
using EPiServer.Tracking.Core;

namespace OxxCommerceStarterKit.Web.Business.CustomTracking
{
    [ServiceConfiguration(typeof(MyEventTracker))]
    public class MyEventTracker
    {
        private readonly ITrackingService _trackingService;
        private readonly HttpContextBase _httpContextBase;
        private readonly IUserDataService _userDataService;

        public MyEventTracker(ITrackingService trackingService, HttpContextBase httpContextBase, IUserDataService userDataService)
        {
            _trackingService = trackingService;
            _httpContextBase = httpContextBase;
            _userDataService = userDataService;
        }
        public void TrackFormSubmission(string name)
        {
            var trackingData = new TrackingData<VisitingEvent2>
            {
                EventType = "formSubmission",
                User = new UserData
                {
                    Name = _userDataService.GetUserName(_httpContextBase),
                    Email = _userDataService.GetUserEmail(_httpContextBase)
                },
                Value = "Submitted form: " + name,
                Payload = new VisitingEvent2 { MyString = "For tracking, you know!" },
                EventTime = DateTime.Now
            };
            _trackingService.Track(trackingData, _httpContextBase);
        }
    }

    public class VisitingEvent2
    {
        public string MyString { get; internal set; }
    }
}