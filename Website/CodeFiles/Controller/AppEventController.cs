using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Web.Controller
{
	/// <summary>
	/// Contains functionality for interacting with the event handling layer. Objects in the web layer should use these
	/// methods rather than directly invoking the objects in the error handling layer.
	/// </summary>
	public static class AppEventController
	{
		#region Public Methods

		/// <summary>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and optionally notify
		/// zero or more users via e-mail. The users to be notified are specified in the <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs"/>
		/// property of the gallery settings object associated with <paramref name="galleryId" />.
		/// </summary>
		/// <param name="ex">The exception to record.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
		/// If the exception is not specific to a particular gallery, specify null and it will automatically be linked to
		/// the template gallery.</param>
		/// <returns>An instance of <see cref="IEvent" />.</returns>
		public static IEvent LogError(Exception ex, int? galleryId = null)
		{
			var gallerySettings = Factory.LoadGallerySettings();

			var ev = EventController.RecordError(ex, AppSetting.Instance, galleryId, gallerySettings);

			CacheController.PurgeCache();

			return ev;
		}

		/// <summary>
		/// Records the specified <paramref name="message" /> to the event log. The event is associated with the specified
		/// <paramref name="galleryId" />.
		/// </summary>
		/// <param name="message">The message to record in the event log.</param>
		/// <param name="galleryId">The gallery ID to associate with the event. Specify null if the
		/// gallery ID is not known.</param>
		/// <param name="eventType">Type of the event.</param>
		/// <param name="data">Additional optional data to record. May be null.</param>
		/// <returns>An instance of <see cref="IEvent" />.</returns>
		public static IEvent LogEvent(string message, int? galleryId = null, EventType eventType = EventType.Info, Dictionary<string, string> data = null)
		{
			return EventController.RecordEvent(message, eventType, galleryId, Factory.LoadGallerySettings(), AppSetting.Instance, data);
		}

		#endregion

		#region Private Methods

		#endregion

		/// <summary>
		/// Handles an exception that occurs. First, the error is recorded and e-mail notification is sent to users who are subscribed 
		/// to error notification (stored in the configuration setting <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs"/>). 
		/// Certain types, such as security exceptions and directory permission errors, are rendered to the user with user-friendly 
		/// text. For other exceptions, a generic message is displayed, unless the system is configured to show detailed error messages 
		/// (<see cref="IGallerySettings.ShowErrorDetails"/>=<c>true</c>), in which case full details about the exception is displayed. 
		/// If the user has disabled the exception handler (<see cref="IGallerySettings.EnableExceptionHandler"/>=<c>false</c>), then 
		/// the error is recorded but no other action is taken. This allows global error handling in web.config or global.asax to deal with it.
		/// </summary>
		/// <param name="ex">The exception to handle.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with. If the
		/// ID is unknown, use <see cref="Int32.MinValue" />.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId" /> is <see cref="Int32.MinValue" />.</exception>
		public static void HandleGalleryException(Exception ex, int? galleryId = null)
		{
			if (ex == null)
			{
				return;
			}

			if (galleryId == int.MinValue)
				throw new ArgumentOutOfRangeException("galleryId", string.Format("The galleryId parameter in this function must represent an existing gallery. Instead, it was {0}", galleryId));

			IEvent ev = null;
			try
			{
				ev = LogError(ex, galleryId);
			}
			catch (Exception errHandlingEx)
			{
				if (!ex.Data.Contains("Error Handling Exception"))
				{
					ex.Data.Add("Error Handling Exception", String.Format(CultureInfo.CurrentCulture, "The function HandleGalleryException experienced the following error while trying to log an error: {0} - {1} Stack trace: {2}", errHandlingEx.GetType(), errHandlingEx.Message, errHandlingEx.StackTrace));
				}
			}

			// If the error is security related, go to a special page that offers a friendly error message.
			if (ex is GallerySecurityException)
			{
				// User is not allowed to access the requested page. Redirect to home page.
				if (HttpContext.Current != null)
				{
					HttpContext.Current.Server.ClearError();
				}

				Utils.Redirect(PageId.album);
			}
			else if (ex is CannotWriteToDirectoryException)
			{
				// Gallery Server cannot write to a directory. Application startup code checks for this condition,
				// so we'll get here most often when Gallery Server is first configured and the required permissions were not given.
				// Provide friendly, customized message to help the user resolve the situation.
				if (HttpContext.Current != null)
				{
					HttpContext.Current.Server.ClearError();
					HttpContext.Current.Items["CurrentException"] = ex;
				}

				try
				{
					Utils.Transfer(PageId.error_cannotwritetodirectory);
				}
				catch (HttpException)
				{
					// We get this exception when transferring within the app's Init event. When the initialize routine runs
					// later (since it failed on this attempt), we'll be able to successfully transfer.
				}
			}
			else
			{
				// An unexpected exception is happening.
				// If Gallery Server's exception handling is enabled, clear the error and display the relevant error message.
				// Otherwise, don't do anything, which lets it propagate up the stack, thus allowing for error handling code in
				// global.asax and/or web.config (e.g. <customErrors...> or some other global error handler) to handle it.
				bool enableExceptionHandler = false;
				try
				{
					if (galleryId > Int32.MinValue)
					{
						enableExceptionHandler = Factory.LoadGallerySetting(galleryId.Value).EnableExceptionHandler;
					}
				}
				catch { }

				if (enableExceptionHandler)
				{
					// Redirect to generic error page.
					if (HttpContext.Current != null)
					{
						HttpContext.Current.Server.ClearError();
						HttpContext.Current.Items["CurrentAppError"] = ev;
					}

					try
					{
						Utils.Transfer(PageId.error_generic);
					}
					catch (HttpException)
					{
						// We get this exception when transferring within the app's Init event. When the initialize routine runs
						// later (since it failed on this attempt), we'll be able to successfully transfer.
					}
				}
			}
		}
	}
}
