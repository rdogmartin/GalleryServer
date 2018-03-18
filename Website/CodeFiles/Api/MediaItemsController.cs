using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
  /// <summary>
  /// Contains methods for Web API access to media objects.
  /// </summary>
  public class MediaItemsController : ApiController
  {
    /// <summary>
    /// Gets the media object with the specified <paramref name="id" />.
    /// Example: api/mediaitems/4/get
    /// </summary>
    /// <param name="id">The media object ID.</param>
    /// <returns>An instance of <see cref="Entity.MediaItem" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the media object is not found, the user
    /// doesn't have permission to view it, or some other error occurs.</exception>
    public Entity.MediaItem Get(int id)
    {
      try
      {
        IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(id);
        SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), mediaObject.Parent.Id, mediaObject.GalleryId, Utils.IsAuthenticated, mediaObject.Parent.IsPrivate, ((IAlbum)mediaObject.Parent).IsVirtualAlbum);
        var siblings = mediaObject.Parent.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).ToSortedList();
        int mediaObjectIndex = siblings.IndexOf(mediaObject);

        return GalleryObjectController.ToMediaItem(mediaObject, mediaObjectIndex + 1, MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(mediaObject));
      }
      catch (InvalidMediaObjectException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find media object with ID = {0}", id)),
          ReasonPhrase = "Media Object Not Found"
        });
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
    /// Generate a ZIP archive for the requested <paramref name="galleryItems" /> and <paramref name="mediaSize" /> and persist to
    /// a file in the App_Data\_Temp directory. The filename is returned on the <see cref="ActionResult.ActionTarget" /> property.
    /// </summary>
    /// <param name="galleryItems">The gallery items to download.</param>
    /// <param name="mediaSize">Size of the items to include in the ZIP archive.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    [HttpPost]
    public ActionResult PrepareZipDownload(Entity.GalleryItem[] galleryItems, DisplayObjectType mediaSize)
    {
      try
      {
        return AlbumController.PrepareZipDownload(galleryItems, mediaSize);
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        return new ActionResult
        {
          Title = "Cannot Download",
          Status = ActionResultStatus.Warning.ToString(),
          Message = ex.Message
        };
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
    /// Retrieve the ZIP file having the name <paramref name="filename" />. It is expected that it was written to the App_Data\_Temp directory
    /// by a client call to <see cref="PrepareZipDownload" />. If it no longer exists (the app may have restarted, which clears out this directory),
    /// a 404 is returned to the client. No security check is performed since that was already done by <see cref="PrepareZipDownload" /> and it 
    /// would be highly unlikely for a user to guess the name of an existing file.
    /// </summary>
    /// <param name="filename">The filename. Example: "a5f23059-479e-456f-adac-a90d790d09b3.zip"</param>
    /// <returns>An instance of <see cref="System.Net.Http.HttpResponseMessage" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    [HttpGet]
    public HttpResponseMessage DownloadZip(string filename)
    {
      try
      {
        var stream = new FileStream(Path.Combine(AppSetting.Instance.TempUploadDirectory, filename), FileMode.Open);

        var mimeType = Factory.LoadMimeType("dummy.zip")?.FullType ?? "application/octet-stream";
        var zipFilename = Utils.UrlEncode("Media Files".Replace(" ", "_") + ".zip");

        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };

        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        response.Content.Headers.Add("Content-Disposition", "attachment; filename=" + zipFilename);

        return response;
      }
      catch (FileNotFoundException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent("The ZIP file was not found on the server. This can happen when temporary files are removed during an application recycle. Try again."),
          ReasonPhrase = "ZIP Archive Not Found"
        });
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
    /// Gets a comprehensive set of data about the specified media object.
    /// </summary>
    /// <param name="id">The media object ID.</param>
    /// <returns>An instance of <see cref="Entity.GalleryData" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the media object is not found, the user
    /// doesn't have permission to view it, or some other error occurs.</exception>
    [ActionName("Inflated")]
    public Entity.GalleryData GetInflatedMediaObject(int id)
    {
      try
      {
        IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(id);
        return GalleryController.GetGalleryDataForMediaObject(mediaObject, (IAlbum)mediaObject.Parent, new Entity.GalleryDataLoadOptions { LoadMediaItems = true });
      }
      catch (InvalidMediaObjectException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find media object with ID = {0}", id)),
          ReasonPhrase = "Media Object Not Found"
        });
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
    /// Gets the meta items for the specified media object <paramref name="id" />.
    /// </summary>
    /// <param name="id">The media object ID.</param>
    /// <returns>IQueryable{Entity.MetaItem}.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the media object is not found, the user
    /// doesn't have permission to view it, or some other error occurs.</exception>
    [ActionName("Meta")]
    public IQueryable<Entity.MetaItem> GetMetaItemsForMediaObjectId(int id)
    {
      // GET /api/mediaitems/12/meta - Gets metadata items for media object #12
      try
      {
        return GalleryObjectController.GetMetaItemsForMediaObject(id).AsQueryable();
      }
      catch (InvalidMediaObjectException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find media object with ID = {0}", id)),
          ReasonPhrase = "Media Object Not Found"
        });
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
    /// Adds a media file to an album. Prior to calling this method, the file should exist in the
    /// temporary upload directory (<see cref="GlobalConstants.TempUploadDirectory" />) in the
    /// App_Data directory with the name <see cref="AddMediaObjectSettings.FileNameOnServer" />. The
    /// file is copied to the destination album and given the name of
    /// <see cref="AddMediaObjectSettings.FileName" /> (instead of whatever name it currently has, which
    /// may contain a GUID).
    /// </summary>
    /// <param name="settings">The settings that contain data and configuration options for the media file.</param>
    /// <returns>List{ActionResult}.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the user does not have permission to add media
    /// objects or some other error occurs.</exception>
    public List<ActionResult> CreateFromFile(AddMediaObjectSettings settings)
    {
      try
      {
        settings.CurrentUserName = Utils.UserName;

        var fileExt = Path.GetExtension(settings.FileName);

        if (fileExt != null && fileExt.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
          Task.Factory.StartNew(() =>
                                  {
                                    var results = GalleryObjectController.AddMediaObject(settings);

                                    // Since we don't have access to the user's session here, let's create a log entry.
                                    LogUploadZipFileResults(results, settings);
                                  });

          return new List<ActionResult>
                   {
                     new ActionResult
                       {
                         Title = settings.FileName,
                         Status = ActionResultStatus.Async.ToString()
                       }
                   };
        }
        else
        {
          var results = GalleryObjectController.AddMediaObject(settings);

          Utils.AddResultToSession(results);

          return results;
        }
      }
      catch (GallerySecurityException)
      {
        AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "Unauthorized access detected. The security system prevented a user from adding a media object."), null, EventType.Warning);

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
    /// Replace the original file associated with <paramref name="mediaAssetId" /> with <paramref name="fileNameOnServer" />. Most metadata is copied from
    /// the current file to <paramref name="fileNameOnServer" />. Orientation meta, if present, is removed. Some meta properties are updated (e.g. width, height).
    /// Thumbnail and optimized images are regenerated. Requires the application be running in trial mode or under a license of Home &amp; Nonprofit or higher.
    /// </summary>
    /// <param name="mediaAssetId">The ID of the media asset.</param>
    /// <param name="fileNameOnServer">The full path to the edited file. Ex: "C:\Dev\GS\Dev-Main\Website\App_Data\_Temp\85b74137-d795-40a5-8b93-bf31de0b0ca3.jpg"</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    public ActionResult ReplaceWithEditedImage(int mediaAssetId, string fileNameOnServer)
    {
      try
      {
        var filePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.TempUploadDirectory, fileNameOnServer);

        return GalleryObjectController.ReplaceWithEditedImage(mediaAssetId, filePath);
      }
      catch (InvalidMediaObjectException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = "Cannot Edit Media Asset",
          Message = ex.Message
        };
      }
      catch (GallerySecurityException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = "Cannot Edit Media Asset",
          Message = ex.Message
        };
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
    /// Replace the original file associated with <paramref name="mediaAssetId" /> with <paramref name="fileNameOnServer" /> and giving it the name 
    /// <paramref name="fileName" />. Re-extract relevant metadata such as width, height, orientation, video/audio info, etc. Thumbnail and 
    /// optimized images are regenerated. The original file is not modified.
    /// </summary>
    /// <param name="mediaAssetId">The ID of the media asset.</param>
    /// <param name="fileNameOnServer">The full path to the edited file. Ex: "C:\Dev\GS\Dev-Main\Website\App_Data\_Temp\85b74137-d795-40a5-8b93-bf31de0b0ca3.jpg"</param>
    /// <param name="fileName">Name the file should be given when it is persisted to the album directory.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    public ActionResult ReplaceMediaAssetFile(int mediaAssetId, string fileNameOnServer, string fileName)
    {
      try
      {
        var filePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.TempUploadDirectory, fileNameOnServer);

        return GalleryObjectController.ReplaceMediaAssetFile(mediaAssetId, filePath, fileName);
      }
      catch (InvalidMediaObjectException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = "Cannot Edit Media Asset",
          Message = ex.Message
        };
      }
      catch (GallerySecurityException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = "Cannot Replace Media Asset",
          Message = ex.Message
        };
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

    private static void LogUploadZipFileResults(List<ActionResult> results, AddMediaObjectSettings settings)
    {
      var isSuccessful = results.All(r => r.Status == ActionResultStatus.Success.ToString());
      int? galleryId = null;
      IAlbum album = null;
      try
      {
        album = Factory.LoadAlbumInstance(settings.AlbumId);
        galleryId = album.GalleryId;
      }
      catch (InvalidAlbumException) { }

      if (isSuccessful)
      {
        AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "{0} files were successfully extracted from the file '{1}' and added to album '{2}'.", results.Count, settings.FileName, album != null ? album.Title : "<Unknown>"), galleryId);

        return;
      }

      // If we get here at least one of the results was an info, warning, error, etc.
      var succesfulResults = results.Where(m => m.Status == ActionResultStatus.Success.ToString());
      var unsuccesfulResults = results.Where(m => m.Status != ActionResultStatus.Success.ToString());
      var msg = String.Format(CultureInfo.InvariantCulture, "{0} items in the uploaded file '{1}' were added to the gallery, but {2} files were skipped. Review the details for additional information. The file was uploaded by user {3}.", succesfulResults.Count(), settings.FileName, unsuccesfulResults.Count(), settings.CurrentUserName);
      var ex = new UnsupportedMediaObjectTypeException(msg, null);

      var i = 1;
      foreach (var result in unsuccesfulResults)
      {
        ex.Data.Add("File " + i++, String.Format(CultureInfo.InvariantCulture, "{0}: {1}", result.Title, result.Message));
      }

      AppEventController.LogError(ex, galleryId);
    }

    ///// <summary>
    ///// Persists the media item to the data store. The current implementation requires that
    ///// an existing item exist in the data store.
    ///// </summary>
    ///// <param name="mediaItem">An instance of <see cref="Entity.MediaItem"/> to persist to the data 
    ///// store.</param>
    ///// <exception cref="System.Web.Http.HttpResponseException"></exception>
    //public ActionResult PutMediaItem(Entity.MediaItem mediaItem)
    //{
    //	try
    //	{
    //		var mo = Factory.LoadMediaObjectInstance(new MediaLoadOptions(mediaItem.Id) { IsWritable = true });

    //		var isUserAuthorized = Utils.IsUserAuthorized(SecurityActions.EditMediaObject, mo.Parent.Id, mo.GalleryId, mo.IsPrivate, ((IAlbum)mo.Parent).IsVirtualAlbum);
    //		if (!isUserAuthorized)
    //		{
    //			AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "Unauthorized access detected. The security system prevented a user from editing media object {0}.", mo.Id), mo.GalleryId, EventType.Warning);

    //			throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
    //		}

    //		mo.Title = mediaItem.Title;
    //		GalleryObjectController.SaveGalleryObject(mo);

    //		return new ActionResult
    //		{
    //			Status = ActionResultStatus.Success.ToString(),
    //			Title = String.Empty,
    //			Message = String.Empty,
    //			ActionTarget = mediaItem
    //		};
    //	}
    //	catch (InvalidMediaObjectException)
    //	{
    //		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
    //		{
    //			Content = new StringContent(String.Format("Could not find media item with ID = {0}", mediaItem.Id)),
    //			ReasonPhrase = "Media Object Not Found"
    //		});
    //	}
    //	catch (HttpResponseException)
    //	{
    //		throw; // Rethrow, since we've already logged it above
    //	}
    //	catch (Exception ex)
    //	{
    //		AppEventController.LogError(ex);

    //		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
    //		{
    //			Content = Utils.GetExStringContent(ex),
    //			ReasonPhrase = "Server Error"
    //		});
    //	}
    //}

    /// <summary>
    /// Permanently deletes the specified media object from the file system and data store. No action is taken if the
    /// user does not have delete permission.
    /// </summary>
    /// <param name="id">The ID of the media object to be deleted.</param>
    //public HttpResponseMessage DeleteMediaItem(Entity.MediaItem mediaItem)
    public void Delete(int id)
    {
      IGalleryObject mo = null;

      try
      {
        mo = Factory.LoadMediaObjectInstance(id);
        var isUserAuthorized = Utils.IsUserAuthorized(SecurityActions.DeleteMediaObject, mo.Parent.Id, mo.GalleryId, mo.IsPrivate, ((IAlbum)mo.Parent).IsVirtualAlbum);
        var isGalleryReadOnly = Factory.LoadGallerySetting(mo.GalleryId).MediaObjectPathIsReadOnly;
        if (!isUserAuthorized || isGalleryReadOnly)
        {
          AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "Unauthorized access detected. The security system prevented a user from deleting media object {0}.", mo.Id), mo.GalleryId, EventType.Warning);

          throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
        }

        mo.Delete();
      }
      catch (InvalidMediaObjectException)
      {
        // HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
        // the same effect as deleting an existing one. So we do nothing here and let the method return HttpStatusCode.OK.
      }
      catch (HttpResponseException)
      {
        throw; // Rethrow, since we've already logged it above
      }
      catch (Exception ex)
      {
        if (mo != null)
          AppEventController.LogError(ex, mo.GalleryId);
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