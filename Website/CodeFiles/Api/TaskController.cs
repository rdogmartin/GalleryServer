using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Security;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Api
{
  /// <summary>
  /// Contains Web API methods for invoking actions in Gallery Server.
  /// </summary>
  public class TaskController : ApiController
  {
    /// <overloads>
    /// Synchronize the files in the media objects directory with the data store.
    /// </overloads>
    /// <summary>
    /// Invoke a synchronization having the specified options. It is initiated on a background thread and the current thread
    /// is immediately returned.
    /// </summary>
    /// <param name="syncOptions">An object containing settings for the synchronization.</param>
    /// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the caller does not have permission to start a
    /// synchronization.</exception>
    public HttpResponseMessage StartSync(SyncOptions syncOptions)
    {
      try
      {
        #region Check user authorization

        if (!Utils.IsAuthenticated)
        {
          var url = Utils.GetUrl(PageId.login, "ReturnUrl={0}", Utils.UrlEncode(Utils.GetCurrentPageUrl(true)));
          throw new GallerySecurityException($"You are not logged in. <a href='{url}'>Log in</a>");
          //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
        }

        IAlbum album = AlbumController.LoadAlbumInstance(syncOptions.AlbumIdToSynchronize);

        if (!Utils.IsUserAuthorized(SecurityActions.Synchronize, RoleController.GetGalleryServerRolesForUser(), syncOptions.AlbumIdToSynchronize, album.GalleryId, false, album.IsVirtualAlbum))
          throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));

        #endregion

        syncOptions.SyncId = GetSyncId();
        syncOptions.UserName = Utils.UserName;

        Task.Factory.StartNew(() => GalleryController.BeginSync(syncOptions), TaskCreationOptions.LongRunning);

        return new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("Synchronization started...")};
      }
      catch (GallerySecurityException ex)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
          Content = new StringContent(ex.Message),
          ReasonPhrase = "Log In Required"
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
    /// Invoke a synchronization having the specified parameters. It is initiated on a background thread and the current thread
    /// is immediately returned. This method is designed to be remotely invoked, using an URL like this:
    /// http://localhost/dev/gs/api/task/startsync?albumId=156&amp;isRecursive=false&amp;rebuildThumbnails=false&amp;rebuildOptimized=false&amp;password=1234
    /// </summary>
    /// <param name="albumId">The album ID for the album to synchronize. Specify 0 to force synchronizing all galleries
    /// from the root album.</param>
    /// <param name="isRecursive">If set to <c>true</c> the synchronization continues drilling 
    /// down into directories below the current one.</param>
    /// <param name="rebuildThumbnails">if set to <c>true</c> the thumbnail image for each media 
    /// object is deleted and overwritten with a new one based on the original file. Applies to 
    /// all media objects.</param>
    /// <param name="rebuildOptimized">if set to <c>true</c> the optimized version of each media 
    /// object is deleted and overwritten with a new one based on the original file. Only relevant 
    /// for images and for video/audio files when FFmpeg is installed and an applicable encoder
    /// setting exists.</param>
    /// <param name="password">The password that authorizes the caller to invoke a 
    /// synchronization.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when <paramref name="albumId" />
    /// does not represent an existing album or some other error occurs.
    /// </exception>
    /// <remarks>NOTE TO DEVELOPER: If you change the name of this controller or method, update the property 
    /// <see cref="Pages.Admin.albums.SyncAlbumUrl" />.</remarks>
    [HttpGet]
    public string StartSync(int albumId, bool isRecursive, bool rebuildThumbnails, bool rebuildOptimized, string password)
    {
      try
      {
        var syncOptions = GetRemoteSyncOptions(albumId, isRecursive, rebuildThumbnails, rebuildOptimized);

        if (albumId > 0)
        {
          StartRemoteSync(syncOptions, password);
        }
        else if (albumId == 0)
        {
          StartRemoteSyncForAllGalleries(syncOptions, password);
        }
        else
        {
          throw new InvalidAlbumException();
        }
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find album with ID = {0}", albumId)),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (HttpResponseException)
      {
        // Just rethrow - we don't want to log these.
        throw;
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

      return "Starting synchronization on background thread...";
    }

    /// <summary>
    /// Retrieves the status of a synchronization for the gallery having the ID <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The gallery ID. We must name the parameter 'id' rather than galleryId because the routing
    /// defined in <see cref="HttpModule.GspHttpApplication" /> requires that it have this name.</param>
    /// <returns>An instance of <see cref="SynchStatusWebEntity" />.</returns>
    [HttpGet]
    public SynchStatusWebEntity StatusSync(int id)
    {
      try
      {
        return GalleryController.GetSyncStatus(GetSyncId(), id);
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
    /// Aborts a synchronization for the gallery having the ID <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The gallery ID. We must name the parameter 'id' rather than galleryId because the routing
    /// defined in <see cref="HttpModule.GspHttpApplication" /> requires that it have this name.</param>
    /// <returns>A string.</returns>
    [HttpGet]
    public string AbortSync(int id)
    {
      try
      {
        GalleryController.AbortSync(GetSyncId(), id);

        return "Aborting synchronization...";
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
    /// Logs off the current user.
    /// </summary>
    /// <returns>A string.</returns>
    [HttpPost]
    public string Logoff()
    {
      try
      {
        UserController.LogOffUser();

        return "Current user has been logged off...";
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
    /// Clears all caches used in the application. The logged on user must be a site administrator.
    /// </summary>
    /// <returns>System.String.</returns>
    [HttpGet]
    public string PurgeCache()
    {
      if (!Utils.IsCurrentUserSiteAdministrator())
      {
        return "Insufficient permission for purging the cache.";
      }

      try
      {
        CacheController.PurgeCache();

        return "Cache purged...";
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

    #region Private Methods

    /// <summary>
    /// Gets the value of the X-ServerTask-TaskId request header.
    /// </summary>
    /// <returns>System.String.</returns>
    private static string GetSyncId()
    {
      return HttpContext.Current.Request.Headers["X-ServerTask-TaskId"];
    }

    /// <summary>
    /// Starts a synchronization on a background thread for the album specified in <paramref name="syncOptions" />.
    /// </summary>
    /// <param name="syncOptions">The synchronization options.</param>
    /// <param name="password">The password that allows remote access to the synchronization API.</param>
    private static void StartRemoteSync(SyncOptions syncOptions, string password)
    {
      IAlbum album = AlbumController.LoadAlbumInstance(syncOptions.AlbumIdToSynchronize);

      if (!ValidateRemoteSync(album, password))
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }

      Task.Factory.StartNew(() => GalleryController.BeginSync(syncOptions), TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Starts a synchronization on a background thread for all galleries.
    /// </summary>
    /// <param name="syncOptions">The synchronization options.</param>
    /// <param name="password">The password that allows remote access to the synchronization API.</param>
    private static void StartRemoteSyncForAllGalleries(SyncOptions syncOptions, string password)
    {
      // User is requesting that all galleries be synchronized.
      foreach (var gallery in Factory.LoadGalleries())
      {
        var rootAlbum = Factory.LoadRootAlbumInstance(gallery.GalleryId, false);

        if (!ValidateRemoteSync(rootAlbum, password))
          continue;

        var copiedSyncOptions = CopySyncOptions(syncOptions);
        copiedSyncOptions.AlbumIdToSynchronize = rootAlbum.Id;

        Task.Factory.StartNew(() => GalleryController.BeginSync(copiedSyncOptions), TaskCreationOptions.LongRunning);
      }
    }

    /// <summary>
    /// Generate an instance of <see cref="SyncOptions" /> corresponding to the specified parameters and configured for remotely
    /// initiating a synchronization. 
    /// </summary>
    /// <param name="albumId">The ID of the album to synchronize.</param>
    /// <param name="isRecursive">If set to <c>true</c> the synchronization continues drilling 
    /// down into directories below the current one.</param>
    /// <param name="rebuildThumbnails">if set to <c>true</c> the thumbnail image for each media 
    /// object is deleted and overwritten with a new one based on the original file. Applies to 
    /// all media objects.</param>
    /// <param name="rebuildOptimized">if set to <c>true</c> the optimized version of each media 
    /// object is deleted and overwritten with a new one based on the original file. Only relevant 
    /// for images and for video/audio files when FFmpeg is installed and an applicable encoder
    /// setting exists.</param>
    /// <returns>An instance of <see cref="SyncOptions" />.</returns>
    private static SyncOptions GetRemoteSyncOptions(int albumId, bool isRecursive, bool rebuildThumbnails, bool rebuildOptimized)
    {
      return new SyncOptions
      {
        AlbumIdToSynchronize = albumId,
        IsRecursive = isRecursive,
        RebuildThumbnails = rebuildThumbnails,
        RebuildOptimized = rebuildOptimized,
        SyncId = Guid.NewGuid().ToString(),
        SyncInitiator = SyncInitiator.RemoteApp,
        UserName = GlobalConstants.SystemUserName
      };
    }

    /// <summary>
    /// Generate a new instance of <see cref="SyncOptions" /> having the same properties as <paramref name="syncOptions" />, 
    /// with the exception of <see cref="SyncOptions.SyncId" />, which is set to a new value.
    /// </summary>
    /// <param name="syncOptions">The synchronization options to copy.</param>
    /// <returns>An instance of <see cref="SyncOptions" />.</returns>
    private static SyncOptions CopySyncOptions(SyncOptions syncOptions)
    {
      return new SyncOptions
      {
        AlbumIdToSynchronize = syncOptions.AlbumIdToSynchronize,
        IsRecursive = syncOptions.IsRecursive,
        RebuildThumbnails = syncOptions.RebuildThumbnails,
        RebuildOptimized = syncOptions.RebuildOptimized,
        SyncId = Guid.NewGuid().ToString(),
        SyncInitiator = syncOptions.SyncInitiator,
        UserName = syncOptions.UserName
      };
    }

    /// <summary>
    /// Validates that remote syncing is enabled and that the specified <paramref name="password" /> is valid.
    /// </summary>
    /// <param name="album">The album to synchronize.</param>
    /// <param name="password">The password that allows remote access to the synchronization API.</param>
    /// <returns><c>true</c> if validation succeeds, <c>false</c> otherwise</returns>
    private static bool ValidateRemoteSync(IAlbum album, string password)
    {
      IGallerySettings gallerySettings = Factory.LoadGallerySetting(album.GalleryId);

      if (!gallerySettings.EnableRemoteSync)
      {
        AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "Cannot start synchronization: A web request to start synchronizing album '{0}' (ID {1}) was received, but the gallery is currently configured to disallow remote synchronizations. This feature can be enabled on the Albums page in the Site admin area.", album.Title, album.Id), album.GalleryId, EventType.Info);

        return false;
      }

      if (!gallerySettings.RemoteAccessPassword.Equals(password))
      {
        AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "Cannot start synchronization: A web service request to start synchronizing album '{0}' (ID {1}) was received, but the specified password is incorrect.", album.Title, album.Id), album.GalleryId, EventType.Info);

        return false;
      }

      return true;
    }

    #endregion
  }
}