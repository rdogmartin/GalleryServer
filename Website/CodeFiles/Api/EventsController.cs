using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
	/// <summary>
	/// Contains methods for Web API access to events.
	/// </summary>
	public class EventsController : ApiController
	{
		/// <summary>
		/// Gets an HTML formatted string representing the specified event <paramref name="id" />.
		/// </summary>
		/// <param name="id">The event ID.</param>
		/// <returns>A string.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when the event does not exist in the data store,
		/// the user does not have permission to view it, or some other error occurs.</exception>
		public string Get(int id)
		{
			IEvent appEvent = null;
			try
			{
				appEvent = Factory.GetAppEvents().FindById(id);

				if (appEvent == null)
				{
					throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
					{
						Content = new StringContent(String.Format("Could not find event with ID = {0}", id)),
						ReasonPhrase = "Event Not Found"
					});
				}

				// If the event has a non-template gallery ID (not all do), then check the user's permission. For those errors without a gallery ID,
				// just assume the user has permission, because there is no way to verify the user can view this event. We could do something
				// that mostly works like verifying the user is a gallery admin for at least one gallery, but the function we are trying to
				// protect is viewing an event message, which is not that important to worry about.
				if (appEvent.GalleryId != GalleryController.GetTemplateGalleryId())
				{
					SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), int.MinValue, appEvent.GalleryId, Utils.IsAuthenticated, false, false);
				}

				return appEvent.ToHtml();
			}
			catch (GallerySecurityException)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
			}
			catch (Exception ex)
			{
				if (appEvent != null)
					AppEventController.LogError(ex, appEvent.GalleryId);
				else
					AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}

		/// <summary>
		/// Deletes the event having the specified <paramref name="id" />.
		/// </summary>
		/// <param name="id">The ID of the event to delete.</param>
		/// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when the user does not have permission to delete
		/// the event or some other error occurs.
		/// </exception>
		public HttpResponseMessage Delete(int id)
		{
			IEvent appEvent = null;

			try
			{
				appEvent = Factory.GetAppEvents().FindById(id);

				if (appEvent == null)
				{
					// HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
					// the same effect as deleting an existing one. So we simply return HttpStatusCode.OK.
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(String.Format("Event with ID = {0} does not exist.", id)) };
				}

				var isAuthorized = true;

				// If the event has a non-template gallery ID (not all do), then check the user's permission. For those errors without a gallery ID,
				// just assume the user has permission, because there is no way to verify the user can delete this event. We could do something
				// that mostly works like verifying the user is a gallery admin for at least one gallery, but the function we are trying to
				// protect is deleting an event message, which is not that important to worry about.
				if (appEvent.GalleryId != GalleryController.GetTemplateGalleryId())
				{
					isAuthorized = Utils.IsUserAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), int.MinValue, appEvent.GalleryId, false, false);
				}

				if (isAuthorized)
				{
					Events.EventController.Delete(id);
			    CacheController.RemoveCache(CacheItem.AppEvents);

					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Event deleted...") };
				}
				else
				{
					throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
				}
			}
			catch (Exception ex)
			{
				if (appEvent != null)
					AppEventController.LogError(ex, appEvent.GalleryId);
				else
					AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}
	}
}