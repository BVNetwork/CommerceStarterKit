/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Framework.Localization;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
	public class RegisterPageController : PageControllerBase<RegisterPage>
	{
		private readonly UrlResolver _urlResolver;
		private readonly LocalizationService _localizationService;
	   // private readonly IEmailService _emailService;
	    private readonly IEspService _espService;

	    public RegisterPageController(UrlResolver urlResolver, LocalizationService localizationService, IEspService espService)
		{
			_urlResolver = urlResolver;
			_localizationService = localizationService;
            _espService = espService;
		}

		public ActionResult Index(RegisterPage currentPage)
		{
			RegisterPageViewModel model = new RegisterPageViewModel(currentPage);
			model.RegisterForm.AvailableCategories = GetAvailableCategories();

			if (PageEditing.PageIsInEditMode)
			{
				return View("Edit", model);
			}

			if (Request.IsAjaxRequest())
			{
				return PartialView(model);
			}

			return View("Index", model);
		}

		private Dictionary<string, string> GetAvailableCategories()
		{
			var output = new Dictionary<string, string>();

			// get the Category type from the BusinessFoundation
			var category = DataContext.Current.GetMetaFieldType(Constants.Metadata.Customer.Category);
			if (category != null)
			{
				if (category.EnumItems != null)
				{
					foreach (var item in category.EnumItems.OrderBy(x => x.OrderId))
					{
						output.Add(item.Handle.ToString(), item.Name);
					}
				}
			}
			return output;
		}


		[HttpPost]
		public ActionResult Register(RegisterPage currentPage, RegisterPageViewModel model, RegisterForm registerForm, int[] SelectedCategories)
		{
			model.RegisterForm.AvailableCategories = GetAvailableCategories();
			model.RegisterForm.SelectedCategories = SelectedCategories;
			if (registerForm.Password != registerForm.PasswordConfirm)
			{
				ModelState.AddModelError("RegisterForm.ValidationMessage", _localizationService.GetString("/common/validation/compare_passwords"));
			}

			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}

			string emailAddress = registerForm.UserName.Trim();
			string password = registerForm.Password;

			// Account
			MembershipUser user = null;
			MembershipCreateStatus createStatus;
			user = Membership.CreateUser(emailAddress, password, emailAddress, null, null, true, out createStatus);

			bool existingUserWithoutPassword = false;

			if (createStatus == MembershipCreateStatus.DuplicateUserName)
			{
				user = Membership.GetUser(emailAddress);
				var customer1 = CustomerContext.Current.GetContactForUser(user);

                if (customer1 == null)
                {

                    customer1 = CustomerContact.CreateInstance(user);
                }


                if (customer1.GetHasPassword())
				{
					ModelState.AddModelError("RegisterForm.ValidationMessage", _localizationService.GetString("/common/account/register_error_unique_username"));
				}
				else
				{
					existingUserWithoutPassword = true;
				}
			}
			else if (user == null)
			{
				ModelState.AddModelError("RegisterForm.ValidationMessage", _localizationService.GetString("/common/account/register_error"));
			}

			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}

			if (!existingUserWithoutPassword)
			{
                Roles.AddUserToRole(user.UserName, AppRoles.EveryoneRole);
                Roles.AddUserToRole(user.UserName, AppRoles.RegisteredRole);
			}
			else
			{
				// set new password
				var pass = user.ResetPassword();
				user.ChangePassword(pass, password);
			}

			var customer = CustomerContext.Current.GetContactForUser(user);

            if (customer == null)
            {

                customer = CustomerContact.CreateInstance(user);
            }

            customer.FirstName = registerForm.FirstName;
			customer.LastName = registerForm.LastName;
			//customer.SetPhoneNumber(registerForm.Phone);
			//customer.FullName = string.Format("{0} {1}", customer.FirstName, customer.LastName);
			customer.SetHasPassword(true);

			// member club
			if (registerForm.MemberClub)
			{
				customer.CustomerGroup = Constants.CustomerGroup.CustomerClub;
			}

		    var options = string.Empty;
            // Newsletter 
            if (registerForm.ConfirmSms || registerForm.ConfirmNewsletter)
		    {
                
		        if (registerForm.ConfirmSms && registerForm.ConfirmNewsletter)
		        {
		            options = "sms,email";
		        }
                else if (registerForm.ConfirmSms)
		        {
		            options = "sms";
                }
		        else if (registerForm.ConfirmNewsletter)
		        {
		            options = "email";
		        }

                var optionsList = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("interests", options) };

		        if (!string.IsNullOrWhiteSpace(registerForm.FirstName))
		        {
		            optionsList.Add(new KeyValuePair<string, string>("firstname", registerForm.FirstName));
		        }

		        if (!string.IsNullOrWhiteSpace(registerForm.LastName))
		        {
		            optionsList.Add(new KeyValuePair<string, string>("lastname", registerForm.LastName));
		        }

                Task.Run(() => Subscribe(emailAddress, optionsList));		        
		    }

			// categories
			customer.SetCategories(SelectedCategories);

			customer.SaveChanges();

			//var CustomerAddressRepository = ServiceLocator.Current.GetInstance<ICustomerAddressRepository>();
			//CustomerAddressRepository.SetCustomer(customer);

			//// copy address fields to shipping address
			//registerForm.Address.CheckAndSetCountryCode();

			//var ShippingAddress = (Address)registerForm.Address.Clone();
			//ShippingAddress.IsPreferredShippingAddress = true;
			//CustomerAddressRepository.Save(ShippingAddress);

			//registerForm.Address.IsPreferredBillingAddress = true;
			//CustomerAddressRepository.Save(registerForm.Address);

			LoginController.CreateAuthenticationCookie(ControllerContext.HttpContext, emailAddress, AppContext.Current.ApplicationName, false);

			//bool mail_sent = SendWelcomeEmail(registerForm.UserName, currentPage);

            if(!string.IsNullOrWhiteSpace(options) && CurrentPage.PostRegisterPage != null)			   
		        return Redirect(_urlResolver.GetUrl(CurrentPage.PostRegisterPage));

		    return Redirect(_urlResolver.GetUrl(ContentReference.StartPage));
        }


		//public bool SendWelcomeEmail(string email, RegisterPage currentPage = null)
		//{
		//    return _emailService.SendWelcomeEmail(email);
		//}

	    private async Task Subscribe(string email, List<KeyValuePair<string, string>> keyValuePairs)
	    {
	         await _espService.Subscribe(email, keyValuePairs);
	    }
    }
}
