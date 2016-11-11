using EPiServer.ServiceLocation;

namespace OxxCommerceStarterKit.Web.Services
{
    [ServiceConfiguration(typeof(IMetricsLoggingService))]
    public class MetricsLoggingService : IMetricsLoggingService
    {
        public void Count(string category, string metric)
        {
            StackifyLib.Metrics.Count(category, metric);
        }
    }
}
