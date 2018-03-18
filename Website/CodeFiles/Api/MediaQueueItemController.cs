using GalleryServer.Business.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
  /// <summary>
  /// Contains methods for Web API access to the media processing queue. Current user must be
  /// a site administrator; otherwise no action is taken.
  /// </summary>
  public class MediaQueueItemController : ApiController
  {
    /// <summary>
    /// Cancels the currently processing queue item having <paramref name="mediaQueueId" />. The task is forcefully canceled and the item
    /// is assigned a status of <see cref="MediaQueueItemStatus.Canceled" />. If the ID of current item does not match
    /// <paramref name="mediaQueueId" />, no action is taken.
    /// </summary>
    /// <param name="mediaQueueId">The media queue ID.</param>
    /// <returns><see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="GallerySecurityException">Thrown when the user has insufficient privileges to cancel a media queue item.</exception>
    [HttpPost]
    [ActionName("Cancel")]
    public HttpResponseMessage CancelMediaQueueItem(int mediaQueueId)
    {
      try
      {
        if (!Utils.IsCurrentUserSiteAdministrator())
        {
          throw new GallerySecurityException();
        }

        MediaConversionQueue.Instance.CancelMediaQueueItem(mediaQueueId);

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Successfully canceled...") };
      }
      catch (InvalidMediaObjectException)
      {
        // HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
        // the same effect as deleting an existing one. So we do nothing here and let the method return HttpStatusCode.OK.
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Successfully canceled...") };
      }
      catch (GallerySecurityException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }

    /// <summary>
    /// Permanently deletes the specified queue items from the data store. Current user must be
    /// a site administrator; otherwise no action is taken.
    /// </summary>
    /// <param name="mediaQueueIds">The media queue IDs.</param>
    /// <returns><see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="GallerySecurityException">Thrown when the user has insufficient privileges to delete a media queue item.</exception>
    public HttpResponseMessage Delete(int[] mediaQueueIds)
    {
      try
      {
        if (!Utils.IsCurrentUserSiteAdministrator())
        {
          throw new GallerySecurityException();
        }

        foreach (int mediaQueueId in mediaQueueIds)
        {
          MediaConversionQueue.Instance.RemoveMediaQueueItem(mediaQueueId);
        }

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Successfully deleted...") };
      }
      catch (InvalidMediaObjectException)
      {
        // HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
        // the same effect as deleting an existing one. So we do nothing here and let the method return HttpStatusCode.OK.
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Successfully deleted...") };
      }
      catch (GallerySecurityException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }
      catch (Exception ex)
      {
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