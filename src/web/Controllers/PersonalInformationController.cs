/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Web.Mvc;
using EPiServer.Tracking.PageView;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Repositories;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
	public class PersonalInformationController : PageControllerBase<PersonalInformationPage>
	{

	    private readonly IEspService _espService;
 
	    public PersonalInformationController(IEspService espService)
	    {
	        _espService = espService;
	    } 

		[RequireSSL]
		[PageViewTracking]
		public ActionResult Index(PersonalInformationPage currentPage)
		{
			PersonalInformationViewModel model = new PersonalInformationViewModel(currentPage);
            
		    var options = _espService.GetNewsletterOptions(model.PersonalSettingsForm.ContactInformation.Email);

		    if (!string.IsNullOrWhiteSpace(options))
		    {
		        model.PersonalSettingsForm.ConsentEmail = options.Contains("email"); 
		        model.PersonalSettingsForm.ConsentSms = options.Contains("sms"); 
		    }		    

			if (Request.IsAjaxRequest())
			{
				return PartialView(model);
			}

			return View(model);
		}

		[HttpPost]
		[RequireSSL]
		public ActionResult Index(PersonalInformationPage currentPage, PersonalSettingsForm personalSettingsForm)
        {
            PersonalInformationViewModel model = new PersonalInformationViewModel(currentPage);

            personalSettingsForm.ContactInformation.FirstName = personalSettingsForm.BillingAddress.FirstName;
            personalSettingsForm.ContactInformation.LastName = personalSettingsForm.BillingAddress.LastName;
            model.PersonalSettingsForm = personalSettingsForm;

            var options = GetSelectedNewsletterOptions(personalSettingsForm);
            var values = new 
            {
                marketingchannel = options,
                firstname = personalSettingsForm.ContactInformation.FirstName, 
                lastname = personalSettingsForm.ContactInformation.LastName, 
                street = personalSettingsForm.BillingAddress.StreetAddress, 
                postalcode = personalSettingsForm.BillingAddress.ZipCode, 
                city = personalSettingsForm.BillingAddress.City, 
                mobile = "0047" + personalSettingsForm.ContactInformation.PhoneNumber
            };
            _espService.SubscribeOrRemove(personalSettingsForm.ContactInformation.Email, values);


            ContactRepository contactRepository = new ContactRepository();
            contactRepository.Save(model.PersonalSettingsForm.ContactInformation);

            CustomerAddressRepository addressRepository = new CustomerAddressRepository();			
            personalSettingsForm.BillingAddress.CheckAndSetCountryCode();
            personalSettingsForm.BillingAddress.IsPreferredBillingAddress = true;
            addressRepository.Save(personalSettingsForm.BillingAddress);

            if (Request.IsAjaxRequest())
            {
                return PartialView(model);
            }

            //Save data
            return View(model);
        }


	    private string GetSelectedNewsletterOptions(PersonalSettingsForm personalSettingsForm) 
	    { 
 
	        if (personalSettingsForm.ConsentEmail && personalSettingsForm.ConsentSms) 
	        { 
	            return "sms,email"; 
	        } 
 
	        if (personalSettingsForm.ConsentEmail) 
	        { 
	            return "email"; 
	        } 
 
	        if (personalSettingsForm.ConsentSms) 
	        { 
	            return "sms"; 
	        } 
 
	        return string.Empty; 
	    } 
	}
}
