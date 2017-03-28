using System;
using System.Web;
using System.Web.Helpers;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Core.Models;

namespace OxxCommerceStarterKit.Core.Services
{
    [ServiceConfiguration(typeof(ICookieService))]
    public class CookieService : ICookieService
    {
        public QuickBuyModel GetFromCookie()
        {
            var cookie = HttpContext.Current.Request.Cookies["QuickBuy"];
            return cookie == null ? new QuickBuyModel() : JsonConvert.DeserializeObject<QuickBuyModel>(cookie.Value);
        }

        public void SaveCookie(QuickBuyModel model)
        {
            var cookie = HttpContext.Current.Request.Cookies["QuickBuy"] ?? new HttpCookie("QuickBuy");
            cookie.Expires = DateTime.Now.AddDays(1d);
            cookie.HttpOnly = true;
            cookie.Value = JsonConvert.SerializeObject(model);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}
