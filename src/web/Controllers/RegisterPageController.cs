/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using EPiServer.ConnectForCampaign.Services.Implementation;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Repositories;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;
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
	    private readonly IRecipientService _recipientService;

	    public RegisterPageController(UrlResolver urlResolver, LocalizationService localizationService, IEspService espService, IRecipientService recipientService)
		{
			_urlResolver = urlResolver;
			_localizationService = localizationService;
            _espService = espService;
		    _recipientService = recipientService;
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

			var customer = CustomerContext.Current.GetContactForUser(user) ?? CustomerContact.CreateInstance(user);

		    customer.FirstName = registerForm.FirstName;
			customer.LastName = registerForm.LastName;
			customer.SetPhoneNumber(registerForm.Phone);
			customer.SetHasPassword(true);

			// member club
			if (registerForm.MemberClub)
			{
				customer.CustomerGroup = Constants.CustomerGroup.CustomerClub;
			}
		    customer.SaveChanges();

		    var customer2 = CustomerContext.Current.GetContactForUser(user);
            customer2.SetPhoneNumber(registerForm.Phone);
		    customer2.SetCategories(SelectedCategories);
            customer2.SaveChanges();

            var subscribe = Subscribe(registerForm, emailAddress);

            var customerAddressRepository = ServiceLocator.Current.GetInstance<ICustomerAddressRepository>();
            customerAddressRepository.SetCustomer(customer2);

		    // copy fields to billing address
            var address = new Address
		    {
		        FirstName = customer2.FirstName,
		        LastName = customer2.LastName,
		        IsPreferredBillingAddress = true
		    };
		    address.CheckAndSetCountryCode();
            customerAddressRepository.Save(address);

            LoginController.CreateAuthenticationCookie(ControllerContext.HttpContext, emailAddress, AppContext.Current.ApplicationName, false);

            if(subscribe && CurrentPage.PostRegisterPage != null)			   
		        return Redirect(_urlResolver.GetUrl(CurrentPage.PostRegisterPage));

		    return Redirect(_urlResolver.GetUrl(ContentReference.StartPage));
        }

	    private bool Subscribe(RegisterForm registerForm, string emailAddress)
	    {
	        var options = string.Empty;
            
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

	            var values = new 
	            {
	                interests = options,
	                firstname = registerForm.FirstName, 
	                lastname = registerForm.LastName,
	                mobile = "0047" + registerForm.Phone
	            };

	            Task.Run(() => _espService.Subscribe(emailAddress, values));
	        }

	        return !string.IsNullOrWhiteSpace(options);
	    }
    }
}
