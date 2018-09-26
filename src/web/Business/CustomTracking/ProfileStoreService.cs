using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OxxCommerceStarterKit.Web.Business.CustomTracking
{
    public interface IProfileStoreService
    {        
        void UpdatePayload(List<string> categories, HttpContextBase context);
    }

    [ServiceConfiguration(typeof(IProfileStoreService))]
    public class ProfileStoreService : IProfileStoreService
    {
        private readonly HttpClient _client = new HttpClient();

        private readonly string _apiBaseUrl =  ConfigurationManager.AppSettings["ProfileStore.Url"];
        private readonly string _subscriptionKey = ConfigurationManager.AppSettings["ProfileStore.SubscriptionKey"];
        private readonly string getProfilesUrl = "api/v1.0/Profiles";


        public void UpdatePayload(List<string> categories, HttpContextBase context)
        {
            string sessionId = context.Request.Cookies["_madid"]?.Value;

            if (sessionId == null)
                return;

            var response = Request(HttpMethod.Get, $"{_apiBaseUrl}/{getProfilesUrl}/?$filter=DeviceIds+eq+" + sessionId);
            
            var getProfile = response.Content.ReadAsStringAsync();
                        
            var profileObject = UpdateData(JObject.Parse(getProfile.Result), categories);

            if (profileObject != null)
            {
                var profileId = profileObject["ProfileId"];

                Request(HttpMethod.Put, $"{_apiBaseUrl}/{getProfilesUrl}/{profileId}", profileObject.ToString());
            }
        }

      
        private JObject UpdateData(JObject jToken, List<string> categories)
        {
            if (jToken["total"].ToString() != "1")
                return null;
            
            var profile = jToken["items"][0];
            
            var originalPayload = JsonConvert.DeserializeObject<Dictionary<string, object>>(profile["Payload"].ToString());

            if (originalPayload == null)
                originalPayload = new Dictionary<string, object>();

            UpdateOriginalPayload(categories, ref originalPayload);

            profile["Payload"] = JObject.FromObject(originalPayload);
         
            return JObject.FromObject(profile);
        }


        private static void UpdateOriginalPayload(IEnumerable<string> categories, ref Dictionary<string, object> originalPayload)
        {

            if (originalPayload.ContainsKey("ProductsCategories"))
            {
                var list = JsonConvert.DeserializeObject<List<string>>(originalPayload["ProductsCategories"].ToString());

                if (list != null)
                {
                    list.AddRange(categories);
                    originalPayload["ProductsCategories"] = list.Distinct();
                }
            }
            else
            {
                originalPayload["ProductsCategories"] = categories.Distinct();
            }


            if (originalPayload.ContainsKey("NumberOfOrders"))
            {
                var numberOfOrdersString = JsonConvert.DeserializeObject<string>(originalPayload["NumberOfOrders"].ToString());
                var numberOfOrders = int.Parse(numberOfOrdersString);
                originalPayload["NumberOfOrders"] = (numberOfOrders + 1).ToString();
            }
            else
            {
                originalPayload["NumberOfOrders"] = "1";
            }      
        }


        private HttpResponseMessage Request(HttpMethod pMethod, string pUrl, string pJsonContent = "")
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = pMethod,
                RequestUri = new Uri(pUrl)
            };

            httpRequestMessage.Headers.Add("Authorization", "epi-single" + _subscriptionKey);

            switch (pMethod.Method)
            {
                case "POST":
                case "PUT":
                    HttpContent httpContent = new StringContent(pJsonContent, Encoding.UTF8, "application/json");
                    httpRequestMessage.Content = httpContent;
                    break;
            }

            var result = _client.SendAsync(httpRequestMessage);

            return result.Result;
        }
    }
}