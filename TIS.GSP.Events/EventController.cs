using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Xml.Serialization;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;
using GalleryServer.Events.Properties;

namespace GalleryServer.Events
{
	/// <summary>
	/// Contains event handling functionality for Gallery Server.
	/// </summary>
	public static class EventController
	{
		#region Fields

		private static readonly object _sharedLock = new object();

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets a collection of all application events from the data store. The items are sorted in descending order on the
		/// <see cref="IEvent.TimestampUtc"/> property, so the most recent event is first. Returns an empty collection if no
		/// events exist.
		/// </summary>
		/// <returns>Returns a collection of all application events from the data store.</returns>
		public static IEventCollection GetAppEvents()
		{
			using (var repo = new EventRepository())
			{
				return GetEventsFromDtos(repo.GetAll().OrderByDescending(e => e.TimeStampUtc));
			}
		}

		/// <summary>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and return
		/// the instance. Send an e-mail notification if that option is enabled.
		/// </summary>
		/// <param name="ex">The exception to be recorded to the data store.</param>
		/// <param name="appSettings">The application settings containing the e-mail configuration data.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
		/// If the exception is not specific to a particular gallery, specify null.</param>
		/// <param name="gallerySettingsCollection">The collection of gallery settings for all galleries. You may specify
		/// null if the value is not known; however, this value must be specified for e-mail notification to occur.</param>
		/// <returns>An instance of <see cref="IEvent" />.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
		public static IEvent RecordError(Exception ex, IAppSetting appSettings, int? galleryId = null, IGallerySettingsCollection gallerySettingsCollection = null)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			if (galleryId == null)
			{
				using (var repo = new GalleryRepository())
				{
					galleryId = repo.Where(g => g.IsTemplate).First().GalleryId;
				}
			}

			var ev = new Event(ex, galleryId.Value);

			Save(ev);

			if (gallerySettingsCollection != null)
			{
				SendEmail(ev, appSettings, gallerySettingsCollection);
			}

			if (appSettings != null)
			{
				ValidateLogSize(appSettings.MaxNumberErrorItems);
			}

			return ev;
		}

		/// <summary>
		/// Persist information about the specified <paramref name="msg" /> to the data store and return
		/// the instance. Send an e-mail notification if that option is enabled.
		/// </summary>
		/// <param name="msg">The message to be recorded to the data store.</param>
		/// <param name="eventType">Type of the event. Defaults to <see cref="EventType.Info" /> if not specified.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="msg" /> is associated with.
		/// If the message is not specific to a particular gallery, specify null.</param>
		/// <param name="gallerySettingsCollection">The collection of gallery settings for all galleries. You may specify
		/// null if the value is not known; however, this value must be specified for e-mail notification to occur.</param>
		/// <param name="appSettings">The application settings. You may specify null if the value is not known.</param>
		/// <param name="data">Additional optional data to record. May be null.</param>
		/// <returns>An instance of <see cref="IEvent" />.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">galleryId</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId" /> is <see cref="Int32.MinValue" />.</exception>
		public static IEvent RecordEvent(string msg, EventType eventType = EventType.Info, int? galleryId = null, IGallerySettingsCollection gallerySettingsCollection = null, IAppSetting appSettings = null, Dictionary<string, string> data = null)
		{
			if (galleryId == Int32.MinValue)
				throw new ArgumentOutOfRangeException("galleryId", String.Format("The galleryId parameter must represent an existing gallery. Instead, it was {0}", galleryId));

			if (galleryId == null)
			{
				using (var repo = new GalleryRepository())
				{
					galleryId = repo.Where(g => g.IsTemplate).First().GalleryId;
				}
			}

			var ev = new Event(msg, galleryId.Value, eventType, data);

			Save(ev);

			if (appSettings != null && gallerySettingsCollection != null)
			{
				SendEmail(ev, appSettings, gallerySettingsCollection);
			}

			if (appSettings != null)
			{
				ValidateLogSize(appSettings.MaxNumberErrorItems);
			}

			return ev;
		}

		/// <summary>
		/// Permanently remove the specified event from the data store.
		/// </summary>
		/// <param name="eventId">The value that uniquely identifies this application event (<see cref="IEvent.EventId"/>).</param>
		public static void Delete(int eventId)
		{
			using (var repo = new EventRepository())
			{
				var aeDto = repo.Find(eventId);

				if (aeDto != null)
				{
					repo.Delete(aeDto);
					repo.Save();
				}
			}
		}

		/// <summary>
		/// Permanently delete all events from the data store that are system-wide (that is, not associated with a specific gallery) and also
		/// those events belonging to the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		public static void ClearEventLog(int galleryId)
		{
			using (var repo = new EventRepository())
			{
				foreach (var eDto in repo.Where(e => e.FKGalleryId == galleryId || e.Gallery.IsTemplate))
				{
					repo.Delete(eDto);
				}

				repo.Save();
			}
		}

		/// <summary>
		/// Serializes the specified collection into an XML string. The data can be converted back into a collection using
		/// the <see cref="Deserialize"/> method.
		/// </summary>
		/// <param name="list">The collection to serialize to XML.</param>
		/// <returns>Returns an XML string.</returns>
		public static string Serialize(ICollection<KeyValuePair<string, string>> list)
		{
			if ((list == null) || (list.Count == 0))
				return String.Empty;

			using (var dt = new DataTable("Collection"))
			{
				dt.Locale = CultureInfo.InvariantCulture;
				dt.Columns.Add("key");
				dt.Columns.Add("value");

				foreach (var pair in list)
				{
					var dr = dt.NewRow();
					dr[0] = pair.Key;
					dr[1] = pair.Value;
					dt.Rows.Add(dr);
				}

				var ser = new XmlSerializer(typeof(DataTable));
				using (var writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					ser.Serialize(writer, dt);

					return writer.ToString();
				}
			}
		}

		/// <summary>
		/// Deserializes <paramref name="xmlToDeserialize"/> into a collection. This method assumes the XML was serialized 
		/// using the <see cref="Serialize"/> method.
		/// </summary>
		/// <param name="xmlToDeserialize">The XML to deserialize.</param>
		/// <returns>Returns a collection.</returns>
		private static List<KeyValuePair<string, string>> Deserialize(string xmlToDeserialize)
		{
			var list = new List<KeyValuePair<string, string>>();

			if (String.IsNullOrEmpty(xmlToDeserialize))
				return list;

			using (var dt = new DataTable("Collection"))
			{
				dt.Locale = CultureInfo.InvariantCulture;
				dt.ReadXml(new StringReader(xmlToDeserialize));

				list.AddRange(from DataRow row in dt.Rows select new KeyValuePair<string, string>(row[0].ToString(), row[1].ToString()));
			}

			return list;
		}

		/// <summary>
		/// If automatic log size trimming is enabled and the log contains more items than the specified limit, delete the oldest 
		/// event records. No action is taken if <paramref name="maxNumberEventItems"/> is set to zero. Return the number of
		/// items that were deleted, if any.
		/// </summary>
		/// <param name="maxNumberEventItems">The maximum number of event items that should be stored in the log. If the count exceeds 
		/// this amount, the oldest items are deleted. No action is taken if <paramref name="maxNumberEventItems"/> is set to zero.</param>
		/// <returns>Returns the number of items that were deleted from the log.</returns>
		public static int ValidateLogSize(int maxNumberEventItems)
		{
			if (maxNumberEventItems == 0)
				return 0; // Auto trimming is disabled, so just return.

			var numErrorDeleted = 0;

			lock (_sharedLock)
			{
				using (var repo = new EventRepository())
				{
					foreach (var e in repo.All.OrderByDescending(ae => ae.EventId).Skip(maxNumberEventItems))
					{
						repo.Delete(e);
						numErrorDeleted++;
					}

					repo.Save();
				}
			}

			return numErrorDeleted;
		}

		/// <summary>
		/// Gets detailed information about the <paramref name="ex"/> that can be presented to an administrator. This is essentially 
		/// a string that combines the exception type with its message. It recursively checks for an InnerException and appends that 
		/// type and message if present. It does not include stack trace or other information. Callers to this method should ensure 
		/// that this information is shown to the user only if he or she is a system administrator and/or the ShowErrorDetails setting 
		/// of the configuration file to true.
		/// </summary>
		/// <param name="ex">The exception for which detailed information is to be returned.</param>
		/// <returns>Returns detailed information about the <paramref name="ex"/> that can be presented to an administrator.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ex" /> is null.</exception>
		public static string GetExceptionDetails(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			string exMsg = String.Concat(ex.GetType(), ": ", ex.Message);
			Exception innerException = ex.InnerException;
			while (innerException != null)
			{
				exMsg += String.Concat(" ", innerException.GetType(), ": ", innerException.Message);
				innerException = innerException.InnerException;
			}

			return exMsg;
		}

		/// <summary>
		/// Gets a human readable text representation for the specified <paramref name="enumItem"/>. The text is returned from the resource
		/// file. Example: If <paramref name="enumItem"/> = ErrorItem.StackTrace, the text "Stack Trace" is used.
		/// </summary>
		/// <param name="enumItem">The enum value for which to get human readable text.</param>
		/// <returns>Returns human readable text representation for the specified <paramref name="enumItem"/></returns>
		internal static string GetFriendlyEnum(EventItem enumItem)
		{
			switch (enumItem)
			{
				case EventItem.EventId: return Resources.Err_AppErrorId_Lbl;
				case EventItem.EventType: return Resources.Err_EventType_Lbl;
				case EventItem.Url: return Resources.Err_Url_Lbl;
				case EventItem.Timestamp: return Resources.Err_Timestamp_Lbl;
				case EventItem.ExType: return Resources.Err_ExceptionType_Lbl;
				case EventItem.Message: return Resources.Err_Message_Lbl;
				case EventItem.ExSource: return Resources.Err_Source_Lbl;
				case EventItem.ExTargetSite: return Resources.Err_TargetSite_Lbl;
				case EventItem.ExStackTrace: return Resources.Err_StackTrace_Lbl;
				case EventItem.ExData: return Resources.Err_ExceptionData_Lbl;
				case EventItem.InnerExType: return Resources.Err_InnerExType_Lbl;
				case EventItem.InnerExMessage: return Resources.Err_InnerExMessage_Lbl;
				case EventItem.InnerExSource: return Resources.Err_InnerExSource_Lbl;
				case EventItem.InnerExTargetSite: return Resources.Err_InnerExTargetSite_Lbl;
				case EventItem.InnerExStackTrace: return Resources.Err_InnerExStackTrace_Lbl;
				case EventItem.InnerExData: return Resources.Err_InnerExData_Lbl;
				case EventItem.GalleryId: return Resources.Err_GalleryId_Lbl;
				case EventItem.HttpUserAgent: return Resources.Err_HttpUserAgent_Lbl;
				case EventItem.FormVariables: return Resources.Err_FormVariables_Lbl;
				case EventItem.Cookies: return Resources.Err_Cookies_Lbl;
				case EventItem.SessionVariables: return Resources.Err_SessionVariables_Lbl;
				case EventItem.ServerVariables: return Resources.Err_ServerVariables_Lbl;
				default: throw new CustomExceptions.BusinessException(String.Format(CultureInfo.CurrentCulture, "Encountered unexpected EventItem enum value {0}. EventController.GetFriendlyEnum is not designed to handle this enum value. The function must be updated.", enumItem));
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the app events from the DTO objects. Returns an empty collection if no events.
		/// </summary>
		/// <param name="eventDtos">An enumerable object containing the app event data transfer objects.</param>
		/// <returns>Returns an <see cref="IEventCollection" />.</returns>
		private static IEventCollection GetEventsFromDtos(IEnumerable<EventDto> eventDtos)
		{
			var events = new EventCollection();

			try
			{
				foreach (var ev in eventDtos)
				{
					events.Add(new Event(ev.EventId,
						ev.EventType,
						ev.FKGalleryId,
						ev.TimeStampUtc,
						ev.ExType,
						ev.Message,
						Deserialize(ev.EventData),
						ev.ExSource,
						ev.ExTargetSite,
						ev.ExStackTrace,
						ev.InnerExType,
						ev.InnerExMessage,
						ev.InnerExSource,
						ev.InnerExTargetSite,
						ev.InnerExStackTrace,
						Deserialize(ev.InnerExData),
						ev.Url,
						Deserialize(ev.FormVariables), Deserialize(ev.Cookies), Deserialize(ev.SessionVariables), Deserialize(ev.ServerVariables)));
				}
			}
			catch (InvalidOperationException ex)
			{
				if (ex.Source.Equals("System.Data.SqlServerCe"))
				{
					// We hit the SQL CE bug described here: http://connect.microsoft.com/SQLServer/feedback/details/606152
					// Clear the table.
					var sqlCeController = new SqlCeController();

					sqlCeController.ClearEventLog();

					events.Clear();
				}
				else throw;
			}

			return events;
		}

		/// <summary>
		/// Sends an e-mail containing details about the <paramref name="ev" /> to all users who are configured to receive event
		/// notifications in the gallery identified by <see cref="IEvent.GalleryId" />. If the event is not associated with a particular
		/// gallery (that is, <see cref="IEvent.GalleryId" /> is the ID of the template gallery, then e-mails are sent to users in all
		/// galleries who are configured to receive e-mailed event reports. The property <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs" />
		/// defines this list of users.
		/// </summary>
		/// <param name="ev">The application event to be sent to users.</param>
		/// <param name="appSettings">The application settings containing the e-mail configuration data.</param>
		/// <param name="gallerySettingsCollection">The settings for all galleries. If the <paramref name="ev" /> is associated with
		/// a particular gallery, then only the settings for that gallery are used by this function; otherwise users in all galleries are
		/// notified.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ev" />, <paramref name="appSettings" /> or 
		/// <paramref name="gallerySettingsCollection" /> is null.</exception>
		private static void SendEmail(IEvent ev, IAppSetting appSettings, IGallerySettingsCollection gallerySettingsCollection)
		{
			#region Validation

			if (ev == null)
				throw new ArgumentNullException("ev");

			if (appSettings == null)
				throw new ArgumentNullException("appSettings");

			if (gallerySettingsCollection == null)
				throw new ArgumentNullException("gallerySettingsCollection");

			// We only want to send en email for INFO events.
			if (ev.EventType == EventType.Info || ev.EventType == EventType.Warning)
			{
				return;
			}

			#endregion

			if (gallerySettingsCollection.FindByGalleryId(ev.GalleryId).IsTemplate)
			{
				// This is an application-wide event, so loop through every gallery and notify all users, making sure we don't notify anyone more than once.
				var notifiedUsers = new List<string>();

				foreach (var gallerySettings in gallerySettingsCollection)
				{
					notifiedUsers.AddRange(SendMail(ev, appSettings, gallerySettings, notifiedUsers));
				}
			}
			else
			{
				// Use settings from the gallery associated with the event.
				var gallerySettings = gallerySettingsCollection.FindByGalleryId(ev.GalleryId);

				if (gallerySettings != null)
				{
					SendMail(ev, appSettings, gallerySettingsCollection.FindByGalleryId(ev.GalleryId), null);
				}
			}

		}

		/// <summary>
		/// Sends an e-mail containing details about the <paramref name="ev" /> to all users who are configured to receive e-mail
		/// notifications in the specified <paramref name="gallerySettings" /> and who have valid e-mail addresses. (That is, e-mails are
		/// sent to users identified in the property <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs" />.) A list of usernames
		/// of those were were notified is returned. No e-mails are sent to any usernames in <paramref name="usersWhoWereAlreadyNotified" />.
		/// </summary>
		/// <param name="ev">The application event to be sent to users.</param>
		/// <param name="appSettings">The application settings containing the e-mail configuration data.</param>
		/// <param name="gallerySettings">The gallery settings containing configuration data such as the list of users to be notified.
		/// The users are identified in the <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs" /> property.</param>
		/// <param name="usersWhoWereAlreadyNotified">The users who were previously notified about the <paramref name="ev" />.</param>
		/// <returns>Returns a list of usernames of those were were notified during execution of this function.</returns>
		/// <exception cref="System.ArgumentNullException">ev</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ev" /> or <paramref name="gallerySettings" />
		/// is null.</exception>
		private static List<string> SendMail(IEvent ev, IAppSetting appSettings, IGallerySettings gallerySettings, List<string> usersWhoWereAlreadyNotified)
		{
			#region Validation

			if (ev == null)
				throw new ArgumentNullException("ev");

			if (appSettings == null)
				throw new ArgumentNullException("appSettings");

			if (gallerySettings == null)
				throw new ArgumentNullException("gallerySettings");

			#endregion

			if (usersWhoWereAlreadyNotified == null)
			{
				usersWhoWereAlreadyNotified = new List<string>();
			}

			var notifiedUsers = new List<string>();

			//If email reporting has been turned on, send detailed event report.
			if (!gallerySettings.SendEmailOnError)
			{
				return notifiedUsers;
			}

			MailAddress emailSender = null;

			if (!String.IsNullOrWhiteSpace(appSettings.EmailFromAddress))
			{
				emailSender = new MailAddress(appSettings.EmailFromAddress, appSettings.EmailFromName);
			}

			foreach (var user in gallerySettings.UsersToNotifyWhenErrorOccurs)
			{
				if (!usersWhoWereAlreadyNotified.Contains(user.UserName))
				{
					if (SendMail(ev, user, appSettings, emailSender))
					{
						notifiedUsers.Add(user.UserName);
					}
				}
			}

			return notifiedUsers;
		}

		/// <summary>
		/// Sends an e-mail containing details about the <paramref name="ev" /> to the specified <paramref name="user" />. Returns
		/// <c>true</c> if the e-mail is successfully sent.
		/// </summary>
		/// <param name="ev">The application event to be sent to users.</param>
		/// <param name="user">The user to send the e-mail to.</param>
		/// <param name="appSettings">The application settings containing the e-mail configuration data.</param>
		/// <param name="emailSender">The account that that will appear in the "From" portion of the e-mail. If null, then the 
		/// 'from' account specified in the SMTP section of web.config is used (and is therefore required in this case).</param>
		/// <returns>Returns <c>true</c> if the e-mail is successfully sent; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ev" />, <paramref name="user" />, 
		/// or <paramref name="appSettings" /> is null.</exception>
		private static bool SendMail(IEvent ev, IUserAccount user, IAppSetting appSettings, MailAddress emailSender)
		{
			#region Validation

			if (ev == null)
				throw new ArgumentNullException("ev");

			if (user == null)
				throw new ArgumentNullException("user");

			if (appSettings == null)
				throw new ArgumentNullException("appSettings");

			#endregion

			var emailWasSent = false;

			if (!IsValidEmail(user.Email))
			{
				return false;
			}

			var emailRecipient = new MailAddress(user.Email, user.UserName);
			try
			{
				using (var mail = new MailMessage())
				{
					if (emailSender != null)
					{
						mail.From = emailSender;
					}

					mail.To.Add(emailRecipient);

					if (String.IsNullOrEmpty(ev.ExType))
						mail.Subject = Resources.Email_Subject_When_No_Ex_Type_Present;
					else
						mail.Subject = String.Concat(Resources.Email_Subject_Prefix_When_Ex_Type_Present, " ", ev.ExType);

					mail.Body = ev.ToHtmlPage();
					mail.IsBodyHtml = true;

					using (var smtpClient = new SmtpClient())
					{
						smtpClient.EnableSsl = appSettings.SendEmailUsingSsl;

						// Specify SMTP server if it is specified. The server might have been assigned via web.config,
						// so only update this if we have a config setting.
						if (!String.IsNullOrEmpty(appSettings.SmtpServer))
						{
							smtpClient.Host = appSettings.SmtpServer;
						}

						// Specify port number if it is specified and it's not the default value of 25. The port 
						// might have been assigned via web.config, so only update this if we have a config setting.
						int smtpServerPort;
						if (!Int32.TryParse(appSettings.SmtpServerPort, out smtpServerPort))
							smtpServerPort = Int32.MinValue;

						if ((smtpServerPort > 0) && (smtpServerPort != 25))
						{
							smtpClient.Port = smtpServerPort;
						}

						smtpClient.Send(mail);
					}

					emailWasSent = true;
				}
			}
			catch (Exception ex2)
			{
				string eventMsg = String.Concat(ex2.GetType(), ": ", ex2.Message);

				if (ex2.InnerException != null)
					eventMsg += String.Concat(" ", ex2.InnerException.GetType(), ": ", ex2.InnerException.Message);

				ev.EventData.Add(new KeyValuePair<string, string>(Resources.Cannot_Send_Email_Lbl, eventMsg));
			}

			return emailWasSent;
		}

		/// <summary>
		/// Determines whether the specified string is formatted as a valid email address. This is determined by performing 
		/// two tests: (1) Comparing the string to a regular expression. (2) Using the validation built in to the .NET 
		/// constructor for the <see cref="System.Net.Mail.MailAddress"/> class. The method does not determine that the 
		/// email address actually exists.
		/// </summary>
		/// <param name="email">The string to validate as an email address.</param>
		/// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
		/// returns false.</returns>
		private static bool IsValidEmail(string email)
		{
			if (String.IsNullOrEmpty(email))
				return false;

			return (ValidateEmailByRegEx(email) && ValidateEmailByMailAddressCtor(email));
		}

		/// <summary>
		/// Validates that the e-mail address conforms to a regular expression pattern for e-mail addresses.
		/// </summary>
		/// <param name="email">The string to validate as an email address.</param>
		/// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
		/// returns false.</returns>
		private static bool ValidateEmailByRegEx(string email)
		{
			const string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

			return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
		}

		/// <summary>
		/// Uses the validation built in to the .NET constructor for the <see cref="System.Net.Mail.MailAddress"/> class
		/// to determine if the e-mail conforms to the expected format of an e-mail address.
		/// </summary>
		/// <param name="email">The string to validate as an email address.</param>
		/// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
		/// returns false.</returns>
		private static bool ValidateEmailByMailAddressCtor(string email)
		{
			var passesMailAddressTest = false;
			try
			{
				new MailAddress(email);
				passesMailAddressTest = true;
			}
			catch (FormatException) { }

			return passesMailAddressTest;
		}

		private static void Save(IEvent ev)
		{
			if (ev == null)
				throw new ArgumentNullException("ev");

			var eDto = new EventDto
			{
				EventType = ev.EventType,
				FKGalleryId = ev.GalleryId,
				TimeStampUtc = ev.TimestampUtc,
				ExType = ev.ExType,
				Message = ev.Message,
				ExSource = ev.ExSource,
				ExTargetSite = ev.ExTargetSite,
				ExStackTrace = ev.ExStackTrace,
				EventData = Serialize(ev.EventData),
				InnerExType = ev.InnerExType,
				InnerExMessage = ev.InnerExMessage,
				InnerExSource = ev.InnerExSource,
				InnerExTargetSite = ev.InnerExTargetSite,
				InnerExStackTrace = ev.InnerExStackTrace,
				InnerExData = Serialize(ev.InnerExData),
				Url = ev.Url,
				FormVariables = Serialize(ev.FormVariables),
				Cookies = Serialize(ev.Cookies),
				SessionVariables = Serialize(ev.SessionVariables),
				ServerVariables = Serialize(ev.ServerVariables)
			};

			using (var repo = new EventRepository())
			{
				repo.Add(eDto);
				repo.Save();

				ev.EventId = eDto.EventId;
			}
		}

		#endregion
	}
}
