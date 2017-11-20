using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json.Linq;

namespace OxxCommerceStarterKit.Web.Api
{
    public class CampaignController : BaseApiController
    {

        private Injected<ILogger> _logger;

        [HttpPost]
        public async Task<string> WebhookSubscribe([FromBody]JToken jsonbody)
        {
            if(_logger.Service.IsInformationEnabled())
                _logger.Service.Log(Level.Information, "WebhookSubscribe");

            var dataset = jsonbody.ToObject<EmailResult>();

            if (dataset != null)
            {
                if (!string.IsNullOrWhiteSpace(dataset.Email))
                {
                    var url = GetUrl(dataset.Email);

                    using (var client = new System.Net.Http.HttpClient())
                    {
                        using (var response = await client.GetAsync(new Uri(url)))
                        {
                            
                            var content = await response.Content.ReadAsStringAsync();

                            if (_logger.Service.IsInformationEnabled())
                                _logger.Service.Log(Level.Information, "WebhookSubscribe " + content);

                            return content;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static string GetUrl(string email)
        {
           // var urlFormat = ConfigurationManager.AppSettings["CampaignSubscribeUrl"];
            var key = ConfigurationManager.AppSettings["CampaignKey"];
            var bmOptInId = ConfigurationManager.AppSettings["CampaignOptInId"];
            var bmOptinSource = ConfigurationManager.AppSettings["CampaignOptInSource"];

            return string.Format("https://api.broadmail.de/http/form/{0}/subscribe?bmOptInId={1}&bmRecipientId={2}&bmOptinSource={3}", key, bmOptInId, email, bmOptinSource);
        }

        internal class EmailResult
        {
            public string Email { get; set; }
        }
    }
}