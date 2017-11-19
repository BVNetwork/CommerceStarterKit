using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace OxxCommerceStarterKit.Web.Api
{
    public class CampaignController : BaseApiController
    {
              
        [HttpPost]
        public async Task<string> WebhookSubscribe([FromBody]JToken jsonbody)
        {

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
                            return content;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static string GetUrl(string email)
        {
            var urlFormat = ConfigurationManager.AppSettings["CampaignSubscribeUrl"];
            var key = ConfigurationManager.AppSettings["CampaignKey"];
            var bmOptInId = ConfigurationManager.AppSettings["CampaignOptInId"];
            var bmOptinSource = ConfigurationManager.AppSettings["CampaignOptInSource"];

            return string.Format(urlFormat, key, bmOptInId, email, bmOptinSource);
        }

        internal class EmailResult
        {
            public string Email { get; set; }
        }
    }
}