using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using OxxCommerceStarterKit.Web.Services.Email.Models;

namespace OxxCommerceStarterKit.Web.Services.Email
{
	public class EmailDispatcher : IEmailDispatcher
	{
		private readonly ILogger _logger;

		public EmailDispatcher(ILogger logger)
		{
			_logger = logger;
		}

		public SendEmailResponse SendEmail(Postal.Email email)
		{
			return SendEmail(email, _logger);
		}

		public SendEmailResponse SendEmail(Postal.Email email, ILogger log)
		{
			var output = new SendEmailResponse();
			// We need the full host to fix links in the email
			Uri host = GetHostUrl();
			_logger.Log(Level.Debug, "Sending email with using base uri: {0}", host.ToString());

#if !DEBUG
			try
			{
#endif
			// Process email with Postal
			var emailService = ServiceLocator.Current.GetInstance<Postal.IEmailService>();
			using (var message = emailService.CreateMailMessage(email))
			{
                var htmlView = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType.ToLower() == "text/html");
				if (htmlView != null)
                {
                    string body = new StreamReader(htmlView.ContentStream).ReadToEnd();

                    // move ink styles inline and fix urls with PreMailer.Net
                    var result = PreMailer.Net.PreMailer.MoveCssInline(host, body, false, "#ignore");

                    // Fix image resources and links in html content
                    string html = AddHostToUrls(result.Html, host);

                    htmlView.BaseUri = host;
                    // Explicit encoding, or we might get utf-16 and mail clients that interpret it as Chinese
                    htmlView.ContentType.CharSet = Encoding.UTF8.WebName;
                    htmlView.ContentStream.SetLength(0);

                    var streamWriter = new StreamWriter(htmlView.ContentStream);

                    streamWriter.Write(html);
                    streamWriter.Flush();

                    htmlView.ContentStream.Position = 0;
                }

                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;

                // send email with default smtp client. (the same way as Postal)
                using (var smtpClient = new SmtpClient())
				{
					try
					{
						smtpClient.Send(message);
						output.Success = true;
					}
					catch (SmtpException exception)
					{
						_logger.Error("Exception: {0}, inner message {1}",exception.Message,(exception.InnerException!=null) ? exception.InnerException.Message : string.Empty);
						output.Success = false;
						output.Exception = exception;
					}
				}
			}            

#if !DEBUG
			}


			catch (Exception ex)
			{
				_logger.Error("Error sending email", ex);
				output.Exception = ex;
			}

#endif
			return output;
		}

        protected string AddHostToUrls(string htmlContent, Uri host)
        {
            string html = htmlContent.Replace("src=\"/", "src=\"" + host + "/");
            // Fix relative links
            html = html.Replace("href=\"/", "href=\"" + host + "/");
            html = html.Replace("url(/", "url(" + host + "/");
            return html;
        }

        protected Uri GetHostUrl()
		{
			var siteDefinition = EPiServer.Web.SiteDefinition.Current;
			if (siteDefinition.SiteUrl == null || string.IsNullOrEmpty(siteDefinition.SiteUrl.ToString()))
			{
				throw new ConfigurationErrorsException("Cannot determine host name");
			}

			return siteDefinition.SiteUrl;
		}
	}



}