namespace OxxCommerceStarterKit.Web.Services
{
    public interface IMetricsLoggingService
    {
        void Count(string category, string metric);
    }
}