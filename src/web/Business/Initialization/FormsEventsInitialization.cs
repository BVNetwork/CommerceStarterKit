using EPiServer.Forms.Core.Events;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Web.Business.CustomTracking;
using OxxCommerceStarterKit.Web.Business.Recommendations;

namespace OxxCommerceStarterKit.Web.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class FormsEventsInitialization : IInitializableModule
    {
    
        public Injected<MyEventTracker> InjectedEventTracker{ get; set; }

        public void Initialize(InitializationEngine context)
        {
            var formsEvents = ServiceLocator.Current.GetInstance<FormsEvents>();
            formsEvents.FormsSubmissionFinalized += OnFormFinalized;
        }

        private void OnFormFinalized(object sender, FormsEventArgs e)
        {
            InjectedEventTracker.Service.TrackFormSubmission(e.FormsContent.Name);
        }

        public void Uninitialize(InitializationEngine context)
        {
            
        }
    }
}