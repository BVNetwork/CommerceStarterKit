/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EPiServer;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Customers.Profile;
using Mediachase.Commerce.Security;
using OxxCommerceStarterKit.Core.Email;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Helpers;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.ResetPassword;
using OxxCommerceStarterKit.Web.Services;
using OxxCommerceStarterKit.Web.Services.Email;

namespace OxxCommerceStarterKit.Web.Controllers
{
	public class LoginController : PageControllerBase<LoginPage>
	{
		private readonly IContentRepository _contentRepository;
		private readonly UrlResolver _urlResolver;
		private readonly LocalizationService _localizationService;
	    private readonly IMetricsLoggingService _metrics;
	    private readonly ISiteSettingsProvider _siteConfiguration;

		public LoginController(IContentRepository contentRepository, 
            ISiteSettingsProvider siteConfiguration, 
            UrlResolver urlResolver, 
            LocalizationService localizationService,
            IMetricsLoggingService metrics)
		{
			_contentRepository = contentRepository;
		    _siteConfiguration = siteConfiguration;
			_urlResolver = urlResolver;
			_localizationService = localizationService;
		    _metrics = metrics;
		}

		[RequireSSL]
		public ActionResult Index(LoginPage currentPage)
		{
			LoginViewModel model = new LoginViewModel(currentPage);

			if (PageEditing.PageIsInEditMode)
			{
				return View("Edit", model);
			}
			
			if (ControllerContext.HttpContext.User.Identity.IsAuthenticated)
			{
				var returnUrl = Request["ReturnUrl"];
				//what if the user access a page they don't have access to?
				if (!string.IsNullOrEmpty(returnUrl))
				{
					return View("Index", model);
				}
				// logs the user out
				return View("Authenticated", model);
			}

			if (Request.IsAjaxRequest())
			{
				return PartialView(model);
			}

			return View("Index", model);
		}

		[HttpPost]
		[RequireSSL]
		public ActionResult Login(LoginPage currentPage, LoginViewModel model, LoginForm loginForm, string returnUrl)
		{

			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}

			string user = loginForm.Username;
			string pw = loginForm.Password;
			CustomerContact cc = null;

			if (Membership.ValidateUser(user, pw))
			{
				MembershipUser account = Membership.GetUser(user);
                if (account != null)
				{

					var profile = SecurityContext.Current.CurrentUserProfile as CustomerProfileWrapper;
					if (profile != null)
					{
						cc = CustomerContext.Current.GetContactForUser(account);
						CreateAuthenticationCookie(ControllerContext.HttpContext, user, Mediachase.Commerce.Core.AppContext.Current.ApplicationName, false);

						string url = returnUrl;

						if (string.IsNullOrEmpty(returnUrl))
						{
							if (currentPage.LoggedInPage != null)
							{
								url = _urlResolver.GetUrl(currentPage.LoggedInPage);
							}
							else
							{
								url = _urlResolver.GetUrl(ContentReference.StartPage);
							}
						}

                        _metrics.Count("Users", "Login Success");
                        return Redirect(url);
					}
				}
			}
			ModelState.AddModelError("LoginForm.ValidationMessage", _localizationService.GetString("/common/account/login_error"));
            _metrics.Count("Users", "Login Failure");
            return View("Index", model);
		}

		[HttpGet]
		[RequireSSL]
		public ActionResult ResetPassword(LoginPage currentPage, string token)
		{
			var service = ServiceLocator.Current.GetInstance<IResetPasswordService>();

			MembershipUser user;

		    bool isResetUrlValid = service.IsResetUrlValid(token, out user);
            if(isResetUrlValid)
            {

                ResetPasswordViewModel model = new ResetPasswordViewModel(currentPage);
                model.Token = token;
                model.ResetPasswordForm = new RegisterForm()
                {
                    Token = token,
                    UserName = user.UserName
                };
                return View(model);
            }
            else
            {
                // Token is invalid, we redirect to the login page
                string loginUrl = _urlResolver.GetUrl(_siteConfiguration.GetSettings().LoginPage);
                return new RedirectResult(loginUrl);
            }
		}

		[HttpPost]
		[RequireSSL]
		public ActionResult ResetPassword(LoginPage currentPage, RegisterForm resetPasswordForm)
		{
			if (!ModelState.IsValid)
			{
				return ResetPassword(currentPage, resetPasswordForm.Token);
			}

			var model = new ResetPasswordViewModel(currentPage);
			if (ModelState.IsValid)
			{
				// registerForm.SanitizeInput();
				IResetPasswordService passwordService = ServiceLocator.Current.GetInstance<IResetPasswordService>();

				if (!passwordService.IsValidPassword(resetPasswordForm.Password))
				{
					// Logger.Info("Too weak password");
					var errorMessage = _localizationService.GetString("/common/account/too_weak_password");
					ModelState.AddModelError("ResetPasswordForm.ValidationMessage", errorMessage);
				}
				else
				{
                    model.ResetPasswordForm = ResetUserPassword(resetPasswordForm, passwordService);
				}
			}
			if (resetPasswordForm.PasswordConfirm != resetPasswordForm.Password)
			{
				var errorMessage = _localizationService.GetString("/common/validation/compare_passwords");
				ModelState.AddModelError("ResetPasswordForm.ValidationMessage", errorMessage);
				ModelState.Remove("ResetPasswordForm.Password");
				ModelState.Remove("ResetPasswordForm.VerifyPassword");
			}
			if (!ModelState.IsValid)
			{
                _metrics.Count("Users", "Password Reset Failed");

                return View(model);
			}
            _metrics.Count("Users", "Password Reset");

            return Index(currentPage);
		}

		private RegisterForm ResetUserPassword(RegisterForm registerForm, IResetPasswordService passwordService)
		{
			MembershipUser user;
			registerForm.PasswordChanged = false;
			if (passwordService.IsResetUrlValid(registerForm.Token, out user))
			{
				passwordService.ChangePassword(user, registerForm.Password, registerForm.Token);
				registerForm.PasswordChanged = true;
			}
			return registerForm;
		}

		[HttpGet]
		[RequireSSL]
		public ActionResult RequestPassword(LoginPage currentPage)
		{
			ForgotPasswordForm forgotPasswordForm = new ForgotPasswordForm();

			ForgotPasswordViewModel model = new ForgotPasswordViewModel(currentPage);

			return View(model);
		}

		[HttpPost]
		[RequireSSL]
		public ActionResult RequestPassword(LoginPage currentPage, ForgotPasswordForm forgotPasswordForm)
		{
			string username = Membership.GetUserNameByEmail(forgotPasswordForm.Mail);
			if (string.IsNullOrEmpty(username))
			{
				ModelState.AddModelError("ForgotPasswordForm.Mail", _localizationService.GetString("/common/account/nonexisting_account"));
			}


			if (!ModelState.IsValid)
			{
				return RequestPassword(currentPage);
			}

			LoginViewModel model = new LoginViewModel(currentPage);
			var result = SendForgotPasswordEmail(forgotPasswordForm, currentPage);
            _metrics.Count("Users", "Forgot Password");

            return View("SentForgotPassword", model);
		}


		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			return Redirect(_urlResolver.GetUrl(ContentReference.StartPage));
		}

		public static void CreateAuthenticationCookie(HttpContextBase httpContext, string username, string domain, bool remember)
		{
			// this line is needed for cookieless authentication
			FormsAuthentication.SetAuthCookie(username, remember);
			var expirationDate = FormsAuthentication.GetAuthCookie(username, remember).Expires;

			// we need to handle ticket ourselves since we need to save session paremeters as well
			int timeout = httpContext.Session != null ? httpContext.Session.Timeout : 20;
			var ticket = new FormsAuthenticationTicket(2,
					username,
					DateTime.Now,
					expirationDate == DateTime.MinValue ? DateTime.Now.AddMinutes(timeout) : expirationDate,
					remember,
					domain,
					FormsAuthentication.FormsCookiePath);

			// Encrypt the ticket.
			string encTicket = FormsAuthentication.Encrypt(ticket);

			// remove the cookie, if one already exists with the same cookie name
			if (httpContext.Response.Cookies[FormsAuthentication.FormsCookieName] != null)
			{
				httpContext.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
			}

			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
			cookie.HttpOnly = true;

			cookie.Path = FormsAuthentication.FormsCookiePath;
			cookie.Secure = FormsAuthentication.RequireSSL;
			if (FormsAuthentication.CookieDomain != null)
			{
				cookie.Domain = FormsAuthentication.CookieDomain;
			}

			if (ticket.IsPersistent)
			{
				cookie.Expires = ticket.Expiration;
			}

			// Create the cookie.
			httpContext.Response.Cookies.Set(cookie);
		}

		private bool SendForgotPasswordEmail(ForgotPasswordForm forgotPasswordForm, LoginPage currentPage)
		{
			var passwordService = ServiceLocator.Current.GetInstance<IResetPasswordService>();

			var emailService = ServiceLocator.Current.GetInstance<IEmailService>();
			string token = passwordService.GenerateResetHash(forgotPasswordForm.Mail);

			return emailService.SendResetPasswordEmail(forgotPasswordForm.Mail,
				currentPage.RequestPasswordSubject,
				currentPage.RequestPasswordExisting.ToString(),
				token,
				GetNewPasswordUrl(token));

		}

		private string GetNewPasswordUrl(string hash)
		{
			var routeValues = RouteData.Values;
			routeValues.Add("token", hash);
			string fullUrl = ControllerContext.RequestContext.HttpContext.Request.GetFullUrl(Url.Action("ResetPassword", routeValues)).ToString();
			return fullUrl;
		}
	}
}
