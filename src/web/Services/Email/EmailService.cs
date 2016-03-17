/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using OxxCommerceStarterKit.Core.Email;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.ViewModels.Email;
using OxxCommerceStarterKit.Web.Services.Email.Models;

namespace OxxCommerceStarterKit.Web.Services.Email
{
    public class EmailService : IEmailService
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private readonly INotificationSettingsRepository _notificationSettingsRepository;
        private readonly IEmailDispatcher _emailDispatcher;
        private readonly ICurrentMarket _currentMarket;
        private readonly IContentLoader _contentLoader;
        private readonly IMarketService _marketService;

        public EmailService(INotificationSettingsRepository notificationSettingsRepository, IEmailDispatcher emailDispatcher, ICurrentMarket currentMarket, IContentLoader contentLoader, IMarketService marketService)
        {
            _notificationSettingsRepository = notificationSettingsRepository;
            _emailDispatcher = emailDispatcher;
            _currentMarket = currentMarket;
            _contentLoader = contentLoader;
            _marketService = marketService;
        }

        public bool SendResetPasswordEmail(string email, string subject, string body, string passwordHash, string resetUrl)
        {
            var mailSettings = _notificationSettingsRepository.GetNotificationSettings();
            if (mailSettings != null)
                return AttemptSendOf(CreateResetPasswordEmailMessage(email, subject, body, passwordHash, resetUrl, mailSettings));
            Log.Error("Unable to get notification settings");
            return false;
        }

        private static Models.ResetPassword CreateResetPasswordEmailMessage(string email, string subject, string body,
                                                                     string passwordHash, string resetUrl,
                                                                     NotificationSettings mailSettings)
        {
            var emailMessage = new Models.ResetPassword();
            emailMessage.From = mailSettings.From;
            emailMessage.To = email;
            emailMessage.Subject = subject;
            emailMessage.Header = mailSettings.MailHeader.ToString();
            emailMessage.Footer = mailSettings.MailFooter.ToString();
            emailMessage.Body = body;
            emailMessage.Token = passwordHash;
            emailMessage.ResetUrl = resetUrl;
            return emailMessage;
        }

        private bool AttemptSendOf(Postal.Email emailMessage)
        {
            try
            {
                return Send(emailMessage);
            }
            catch (Exception ex)
            {
                Log.Error("Error occured when sending ResetPassword emailMessage.", ex);
                return false;
            }
        }

        private bool Send(Postal.Email emailMessage)
        {
            var result = _emailDispatcher.SendEmail(emailMessage, Log);
            if (result.Success)
                return true;
            Log.Error(result.Exception.Message, result.Exception);
            return false;
        }

        public bool SendWelcomeEmail(string emailAddress)
        {
            return SendWelcomeEmail(emailAddress, null);
        }

        public bool SendWelcomeEmail(string emailAddress, RegisterPage currentPage)
        {
            if (currentPage == null)
                currentPage = GetRegisterPage();
            if (currentPage != null)
                return SendWelcomeEmail(emailAddress, currentPage.EmailSubject, currentPage.EmailBody.ToString());
            return false;
        }

        private RegisterPage GetRegisterPage()
        {
            var homePage = _contentLoader.Get<HomePage>(ContentReference.StartPage);
            if (homePage != null && homePage.Settings != null && homePage.Settings.LoginPage != null)
            {
                var loginPage = _contentLoader.Get<LoginPage>(homePage.Settings.LoginPage);
                if (loginPage != null && loginPage.RegisterPage != null)
                    return _contentLoader.Get<RegisterPage>(loginPage.RegisterPage);
            }
            return null;
        }

        public bool SendWelcomeEmail(string emailAddress, string subject, string body)
        {
            var mailSettings = _notificationSettingsRepository.GetNotificationSettings();
            if (mailSettings != null)
            {
                var emailMessage = new Welcome();
                emailMessage.From = mailSettings.From;
                emailMessage.To = emailAddress;
                emailMessage.Subject = subject;
                emailMessage.Header = mailSettings.MailHeader.ToString();
                emailMessage.Footer = mailSettings.MailFooter.ToString();
                emailMessage.Body = body;
                var result = _emailDispatcher.SendEmail(emailMessage, Log);
                if (result.Success)
                {
                    return true;
                }
                else
                {
                    Log.Error(result.Exception.Message, result.Exception);
                    return false;
                }
            }
            Log.Error("Unable to get notification settings");
            return false;
        }

        public bool SendOrderReceipt(PurchaseOrderModel order)
        {
            var mailSettings = _notificationSettingsRepository.GetNotificationSettings();
            if (mailSettings != null)
            {
                IMarket market = _marketService.GetMarket(order.MarketId);
                var emailMessage = new Receipt(market, order);
                if (string.IsNullOrEmpty(mailSettings.From))
                    throw new ArgumentException("Missing From address in email settings");
                
                emailMessage.From = mailSettings.From;
                emailMessage.Header = mailSettings.MailHeader.ToHtmlString();
                emailMessage.Footer = mailSettings.MailFooter.ToHtmlString();

                if (string.IsNullOrEmpty(emailMessage.To))
                    throw new ArgumentException("Missing To address");

                var result = _emailDispatcher.SendEmail(emailMessage);
                if (result.Success)
                {
                    return true;
                }
                else
                {
                    Log.Error(result.Exception.Message, result.Exception);
                    return false;
                }
            }
            Log.Error("Unable to get notification settings");
            return false;
        }

        public bool SendDeliveryReceipt(PurchaseOrderModel order, string language = null)
        {
            var mailSettings = _notificationSettingsRepository.GetNotificationSettings(language);
            if (mailSettings != null)
            {
                var emailMessage = new DeliveryReceipt(_currentMarket, order);
                emailMessage.From = mailSettings.From;
                emailMessage.Header = mailSettings.MailHeader.ToString();
                emailMessage.Footer = mailSettings.MailFooter.ToString();

                var result = _emailDispatcher.SendEmail(emailMessage);
                if (result.Success)
                {
                    return true;
                }
                else
                {
                    Log.Error(result.Exception.Message, result.Exception);
                    return false;
                }
            }
            Log.Error("Unable to get notification settings");
            return false;
        }
    }

}
