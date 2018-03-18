using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Controller
{
	/// <summary>
	/// Contains e-mail related functionality.
	/// </summary>
	public static class EmailController
	{
		#region Public Static Methods

		///// <summary>
		///// Send a plain text email with the specified properties. The email will appear to come from the name and email specified in the
		///// EmailFromName and EmailFromAddress configuration settings. The email
		///// is sent to the address configured in the emailToAddress setting in the configuration file. If
		///// <paramref name="sendOnBackgroundThread"/> is true, the e-mail is sent on a background thread and the function
		///// returns immediately. An exception is thrown if an error occurs while sending the e-mail, unless <paramref name="sendOnBackgroundThread"/>
		///// is true, in which case the error is logged but the exception does not propagate back to the UI thread.
		///// </summary>
		///// <param name="subject">The text to appear in the subject of the email.</param>
		///// <param name="body">The body of the email. If the body is HTML, specify true for the isBodyHtml parameter.</param>
		///// <param name="galleryId">The gallery ID.</param>
		///// <param name="sendOnBackgroundThread">If set to <c>true</c> send e-mail on a background thread. This causes any errors
		///// to be silently handled by the error logging system, so if it is important for any errors to propogate to the UI,
		///// such as when testing the e-mail function in the Site Administration area, set to <c>false</c>.</param>
		///// <overloads>
		///// Send an e-mail message.
		///// </overloads>
		///// <exception cref="WebException">Thrown when a SMTP Server is not specified. (Not thrown when
		///// <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		///// <exception cref="SmtpException">Thrown when the connection to the SMTP server failed, authentication failed,
		///// or the operation timed out. (Not thrown when <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		///// <exception cref="SmtpFailedRecipientsException">The message could not be delivered to one or more
		///// recipients. (Not thrown when <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		//public static void SendEmail(string subject, string body, int galleryId, bool sendOnBackgroundThread)
		//{
		//  MailAddress recipient = new MailAddress(Config.GetCore().EmailToAddress, Config.GetCore().EmailToName);

		//  SendEmail(recipient, subject, body, galleryId, sendOnBackgroundThread);
		//}

		/// <overloads>
		/// Send an e-mail.
		/// </overloads>
		/// <summary>
		/// Send a plain text e-mail to the <paramref name="user" />. The email will appear to come from the name and email specified in the
		/// <see cref="IAppSetting.EmailFromName" /> and <see cref="IAppSetting.EmailFromAddress" /> configuration settings.
		/// The e-mail is sent on a background thread, so if an error occurs on that thread no exception bubbles to the caller (the error, however, is
		/// recorded in the error log). If it is important to know if the e-mail was successfully sent, use the overload of this
		/// method that specifies a sendOnBackgroundThread parameter.
		/// </summary>
		/// <param name="user">The user to receive the email. If the user does not have a valid e-mail, no action is taken.</param>
		/// <param name="subject">The text to appear in the subject of the email.</param>
		/// <param name="body">The body of the email. Must be plain text.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
		public static void SendEmail(IUserAccount user, string subject, string body)
		{
			if (user == null)
				throw new ArgumentNullException("user");

			if (HelperFunctions.IsValidEmail(user.Email))
			{
				SendEmail(new MailAddress(user.Email, user.UserName), subject, body, false);
			}
		}

		/// <summary>
		/// Send a plain text e-mail with the specified properties. The email will appear to come from the name and email specified in the
		/// <see cref="IAppSetting.EmailFromName" /> and <see cref="IAppSetting.EmailFromAddress" /> configuration settings.
		/// The e-mail is sent on a background thread, so if an error occurs on that thread no exception bubbles to the caller (the error, however, is
		/// recorded in the error log). If it is important to know if the e-mail was successfully sent, use the overload of this
		/// method that specifies a sendOnBackgroundThread parameter.
		/// </summary>
		/// <param name="emailRecipient">The recipient of the email.</param>
		/// <param name="subject">The text to appear in the subject of the email.</param>
		/// <param name="body">The body of the email. Must be plain text.</param>
		/// <param name="galleryId">The gallery ID containing the e-mail configuration settings to use.</param>
		public static void SendEmail(MailAddress emailRecipient, string subject, string body, int galleryId)
		{
			MailAddressCollection mailAddresses = new MailAddressCollection();
			mailAddresses.Add(emailRecipient);

			SendEmail(mailAddresses, subject, body, false, true);
		}

		/// <summary>
		/// Send an e-mail with the specified properties. The e-mail will appear to come from the name and email specified in the
		/// <see cref="IAppSetting.EmailFromName" /> and <see cref="IAppSetting.EmailFromAddress" /> configuration settings.
		/// If <paramref name="sendOnBackgroundThread"/> is <c>true</c>, the e-mail is sent on a background thread and the function
		/// returns immediately. An exception is thrown if an error occurs while sending the e-mail, unless <paramref name="sendOnBackgroundThread"/>
		/// is true, in which case the error is logged but the exception does not propagate back to the calling thread.
		/// </summary>
		/// <param name="emailRecipient">The recipient of the email.</param>
		/// <param name="subject">The text to appear in the subject of the email.</param>
		/// <param name="body">The body of the email. Must be plain text.</param>
		/// <param name="sendOnBackgroundThread">If set to <c>true</c>, send e-mail on a background thread. This causes any errors
		/// to be silently handled by the error logging system, so if it is important for any errors to propogate to the caller,
		/// such as when testing the e-mail function in the Site Administration area, set to <c>false</c>.</param>
		/// <exception cref="WebException">Thrown when a SMTP Server is not specified. (Not thrown when
		/// <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		/// <exception cref="SmtpException">Thrown when the connection to the SMTP server failed, authentication failed,
		/// or the operation timed out. (Not thrown when <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		/// <exception cref="SmtpFailedRecipientsException">The message could not be delivered to of the <paramref name="emailRecipient"/>.
		/// (Not thrown when <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		public static void SendEmail(MailAddress emailRecipient, string subject, string body, bool sendOnBackgroundThread)
		{
			MailAddressCollection mailAddresses = new MailAddressCollection();
			mailAddresses.Add(emailRecipient);

			SendEmail(mailAddresses, subject, body, false, sendOnBackgroundThread);
		}

		/// <summary>
		/// Send an e-mail with the specified properties. The e-mail will appear to come from the name and email specified in the
		/// <see cref="IAppSetting.EmailFromName" /> and <see cref="IAppSetting.EmailFromAddress" /> configuration settings, if specified.
		/// If <paramref name="sendOnBackgroundThread"/> is <c>true</c>, the e-mail is sent on a background thread and the function
		/// returns immediately. An exception is thrown if an error occurs while sending the e-mail, unless <paramref name="sendOnBackgroundThread"/>
		/// is true, in which case the error is logged but the exception does not propagate back to the calling thread.
		/// </summary>
		/// <param name="emailRecipients">The e-mail recipients.</param>
		/// <param name="subject">The text to appear in the subject of the email.</param>
		/// <param name="body">The body of the e-mail. If the body is HTML, specify true for <paramref name="isBodyHtml" />.</param>
		/// <param name="isBodyHtml">Indicates whether the body of the e-mail is in HTML format. When false, the body is
		///   assumed to be plain text.</param>
		/// <param name="sendOnBackgroundThread">If set to <c>true</c>, send e-mail on a background thread. This causes any errors
		///   to be silently handled by the error logging system, so if it is important for any errors to propogate to the caller,
		///   such as when testing the e-mail function in the Site Administration area, set to <c>false</c>.</param>
		/// <exception cref="WebException">Thrown when a SMTP Server is not specified. (Not thrown when
		/// <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		/// <exception cref="SmtpException">Thrown when the connection to the SMTP server failed, authentication failed,
		/// or the operation timed out. (Not thrown when <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		/// <exception cref="SmtpFailedRecipientsException">The message could not be delivered to one or more of the
		/// <paramref name="emailRecipients"/>. (Not thrown when <paramref name="sendOnBackgroundThread"/> is true.)</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="emailRecipients" /> is null.</exception>
		public static void SendEmail(MailAddressCollection emailRecipients, string subject, string body, bool isBodyHtml, bool sendOnBackgroundThread)
		{
			if (emailRecipients == null)
				throw new ArgumentNullException("emailRecipients");

			var appSettings = AppSetting.Instance;

			MailMessage mail = null;
			try
			{
				mail = new MailMessage();
				foreach (MailAddress mailAddress in emailRecipients)
				{
					mail.To.Add(mailAddress);
				}

				if (HelperFunctions.IsValidEmail(appSettings.EmailFromAddress))
				{
					mail.From = new MailAddress(appSettings.EmailFromAddress, appSettings.EmailFromName);
				}

				mail.Subject = subject;
				mail.Body = body;
				mail.IsBodyHtml = isBodyHtml;

				// Because sending the e-mail takes a long time, spin off a thread to send it, unless caller specifically doesn't want to.
				if (sendOnBackgroundThread)
				{
					Task.Factory.StartNew(() => SendEmail(mail, false));
				}
				else
				{
					SendEmail(mail, false);
					mail.Dispose();
				}
			}
			catch
			{
				if (mail != null)
					mail.Dispose();

				throw;
			}
		}

		/// <overloads>
		/// Gets the email template.
		/// </overloads>
		/// <summary>
		/// Gets the email template. Replacement parameters in the template are replaced with their appropriate values. The data
		/// in the template can be used to construct an e-mail.
		/// </summary>
		/// <param name="template">The template to retrieve.</param>
		/// <param name="user">The user associated with the template.</param>
		/// <returns>Returns an e-mail template.</returns>
		public static EmailTemplate GetEmailTemplate(EmailTemplateForm template, IUserAccount user)
		{
			return GetEmailTemplate(template, user.UserName, user.Email);
		}

		/// <summary>
		/// Gets the email template. Replacement parameters in the template are replaced with their appropriate values. The data
		/// in the template can be used to construct an e-mail.
		/// </summary>
		/// <param name="template">The template to retrieve.</param>
		/// <param name="userName">The name of the user associated with the template.</param>
		/// <param name="email">The email of the user associated with the template.</param>
		/// <returns>Returns an e-mail template.</returns>
		private static EmailTemplate GetEmailTemplate(EmailTemplateForm template, string userName, string email)
		{
			EmailTemplate emailTemplate = new EmailTemplate();
			emailTemplate.EmailTemplateId = template;

			string filePath = Utils.GetPath(String.Format(CultureInfo.InvariantCulture, "/templates/{0}.txt", template));

			// Step 1: Get subject and body from text file and assign to fields.
			using (StreamReader sr = File.OpenText(filePath))
			{
				while (sr.Peek() >= 0)
				{
					string lineText = sr.ReadLine().Trim();

					if (lineText == "[Subject]")
						emailTemplate.Subject = sr.ReadLine();

					if (lineText == "[Body]")
						emailTemplate.Body = sr.ReadToEnd();
				}
			}

			// Step 2: Update replacement parameters with real values.
			emailTemplate.Body = emailTemplate.Body.Replace("{CurrentPageUrlFull}", Utils.GetCurrentPageUrlFull());
			emailTemplate.Body = emailTemplate.Body.Replace("{UserName}", userName);
			emailTemplate.Body = emailTemplate.Body.Replace("{Email}", String.IsNullOrEmpty(email) ? Resources.GalleryServer.Email_Template_No_Email_For_User_Replacement_Text : email);

			if (emailTemplate.Body.Contains("{VerificationUrl}"))
				emailTemplate.Body = emailTemplate.Body.Replace("{VerificationUrl}", GenerateVerificationLink(userName));

			if (emailTemplate.Body.Contains("{Password}"))
				emailTemplate.Body = emailTemplate.Body.Replace("{Password}", UserController.GetPassword(userName));

			if (emailTemplate.Body.Contains("{ManageUserUrl}"))
				emailTemplate.Body = emailTemplate.Body.Replace("{ManageUserUrl}", GenerateManageUserLink(userName));

			return emailTemplate;
		}

		/// <summary>
		/// Sends an e-mail based on the <paramref name="templateForm"/> to the specified <paramref name="user"/>.
		/// No action is taken if the user's e-mail is null or empty. The e-mail is sent on a
		/// background thread, so if an error occurs on that thread no exception bubbles to the caller (the error, however, is
		/// recorded in the error log).
		/// </summary>
		/// <param name="user">The user to receive the e-mail.</param>
		/// <param name="templateForm">The template form specifying the type of e-mail to send.</param>
		public static void SendNotificationEmail(IUserAccount user, EmailTemplateForm templateForm)
		{
			SendNotificationEmail(user.UserName, user.Email, templateForm, true);
		}

		/// <summary>
		/// Sends an e-mail based on the <paramref name="templateForm" /> to the <paramref name="userName" /> having the 
		/// <paramref name="email" />. No action is taken if the user's e-mail is null or empty. The e-mail is sent on a
		/// background thread, so if an error occurs on that thread no exception bubbles to the caller (the error, however, is
		/// recorded in the error log). If <paramref name="sendOnBackgroundThread" /> is true, the e-mail is sent on a background
		/// thread and the function returns immediately. An exception is thrown if an error occurs while sending the e-mail,
		/// unless <paramref name="sendOnBackgroundThread" /> is true, in which case the error is logged but the exception does
		/// not propagate back to the UI thread.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <param name="email">The email address of the user.</param>
		/// <param name="templateForm">The template form specifying the type of e-mail to send.</param>
		/// <param name="sendOnBackgroundThread">If set to <c>true</c> send e-mail on a background thread. This causes any errors
		/// to be silently handled by the error logging system, so if it is important for any errors to propogate to the UI,
		/// such as when testing the e-mail function in the Site Administration area, set to <c>false</c>.</param>
		/// <exception cref="System.ArgumentNullException">userName</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userName" /> is null.</exception>
		public static void SendNotificationEmail(string userName, string email, EmailTemplateForm templateForm, bool sendOnBackgroundThread)
		{
			if (userName == null)
				throw new ArgumentNullException("userName");

			if (String.IsNullOrEmpty(email))
				return;

			EmailTemplate emailTemplate = GetEmailTemplate(templateForm, userName, email);

			MailAddress emailRecipient = new MailAddress(email, userName);

			SendEmail(emailRecipient, emailTemplate.Subject, emailTemplate.Body, sendOnBackgroundThread);
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Sends the e-mail. If <paramref name="suppressException"/> is <c>true</c>, record any exception that occurs but do not
		/// let it propagate out of this function. When <c>false</c>, record the exception and re-throw it. The caller is
		/// responsible for disposing the <paramref name="mail"/> object.
		/// </summary>
		/// <param name="mail">The mail message to send.</param>
		/// <param name="suppressException">If <c>true</c>, record any exception that occurs but do not
		/// let it propagate out of this function. When <c>false</c>, record the exception and re-throw it.</param>
		private static void SendEmail(MailMessage mail, bool suppressException)
		{
			try
			{
				if (mail == null)
					throw new ArgumentNullException("mail");

				using (SmtpClient smtpClient = new SmtpClient())
				{
					var appSettings = AppSetting.Instance;

					smtpClient.EnableSsl = appSettings.SendEmailUsingSsl;

					string smtpServer = appSettings.SmtpServer;
					int smtpServerPort;
					if (!Int32.TryParse(appSettings.SmtpServerPort, out smtpServerPort))
						smtpServerPort = Int32.MinValue;

					// Specify SMTP server if it is specified in the gallery settings. The server might have been assigned via web.config,
					// so only update this if we have a setting.
					if (!String.IsNullOrEmpty(smtpServer))
					{
						smtpClient.Host = smtpServer;
					}

					// Specify port number if it is specified in the gallery settings and it's not the default value of 25. The port 
					// might have been assigned via web.config, so only update this if we have a setting.
					if ((smtpServerPort > 0) && (smtpServerPort != 25))
					{
						smtpClient.Port = smtpServerPort;
					}

					if (String.IsNullOrEmpty(smtpClient.Host))
						throw new WebException(@"Cannot send e-mail because a SMTP Server is not specified. This can be configured in any of the following places: (1) Site Admin - General page (preferred), or (2) web.config (Ex: configuration/system.net/mailSettings/smtp/network host='your SMTP server').");

					smtpClient.Send(mail);
				}
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				if (!suppressException)
					throw;
			}
		}

		private static string GenerateVerificationLink(string userName)
		{
			return String.Concat(Utils.GetHostUrl(), Utils.GetUrl(PageId.createaccount, "verify={0}", Utils.UrlEncode(HelperFunctions.Encrypt(userName))));
		}

		private static string GenerateManageUserLink(string userName)
		{
			return String.Concat(Utils.GetHostUrl(), Utils.GetUrl(PageId.admin_manageusers));
		}

		#endregion
	}
}
