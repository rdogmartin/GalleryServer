using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
  /// <summary>
  /// Contains methods for Web API access to albums.
  /// </summary>
  public class AlbumsController : ApiController
  {
    /// <summary>
    /// Gets the album with the specified <paramref name="id" />. The properties 
    /// <see cref="Entity.Album.GalleryItems" /> and <see cref="Entity.Album.MediaItems" /> 
    /// are set to null to keep the instance small. Example: api/albums/4/get
    /// </summary>
    /// <param name="id">The album ID.</param>
    /// <returns>An instance of <see cref="Entity.Album" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    public Entity.Album Get(int id)
    {
      IAlbum album = null;
      try
      {
        album = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(id) { InflateChildObjects = true });
        SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);
        var permissionsEntity = new Entity.Permissions();

        return AlbumController.ToAlbumEntity(album, permissionsEntity, new Entity.GalleryDataLoadOptions());
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find album with ID = {0}", id)),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }

    /// <summary>
    /// Gets a comprehensive set of data about the specified album.
    /// </summary>
    /// <param name="id">The album ID.</param>
    /// <param name="top">Specifies the number of child gallery objects to retrieve. Specify 0 to retrieve all items.</param>
    /// <param name="skip">Specifies the number of child gallery objects to skip.</param>
    /// <returns>An instance of <see cref="Entity.GalleryData" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">
    /// </exception>
    /// <exception cref="HttpResponseMessage">
    /// </exception>
    /// <exception cref="StringContent"></exception>
    [ActionName("Inflated")]
    public Entity.GalleryData GetInflatedAlbum(int id, int top = 0, int skip = 0)
    {
      // GET /api/albums/12/inflated // Return data for album # 12
      IAlbum album = null;
      try
      {
        album = Factory.LoadAlbumInstance(new AlbumLoadOptions(id) { InflateChildObjects = true });
        var loadOptions = new Entity.GalleryDataLoadOptions
        {
          LoadGalleryItems = true,
          NumGalleryItemsToRetrieve = top,
          NumGalleryItemsToSkip = skip
        };

        return GalleryController.GetGalleryDataForAlbum(album, loadOptions);
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find album with ID = {0}", id)),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }

    /// <summary>
    /// Gets the gallery items for the specified album, optionally sorting the results.
    /// </summary>
    /// <param name="id">The album ID.</param>
    /// <param name="sortByMetaNameId">The name of the metadata item to sort on.</param>
    /// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
    /// <returns>IQueryable{Entity.GalleryItem}.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    [ActionName("GalleryItems")]
    public IQueryable<Entity.GalleryItem> GetGalleryItemsForAlbumId(int id, int sortByMetaNameId = int.MinValue, bool sortAscending = true)
    {
      // GET /api/albums/12/galleryitems?sortByMetaNameId=11&sortAscending=true - Gets gallery items for album #12
      try
      {
        return GalleryObjectController.GetGalleryItemsInAlbum(id, (MetadataItemName)sortByMetaNameId, sortAscending);
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find album with ID = {0}", id)),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

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
    /// Gets the media items for the specified album.
    /// </summary>
    /// <param name="id">The album ID.</param>
    /// <param name="sortByMetaNameId">The name of the metadata item to sort on.</param>
    /// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
    /// <returns>IQueryable{Entity.MediaItem}.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    [ActionName("MediaItems")]
    public IQueryable<Entity.MediaItem> GetMediaItemsForAlbumId(int id, int sortByMetaNameId = int.MinValue, bool sortAscending = true)
    {
      // GET /api/albums/12/mediaitems - Gets media items for album #12
      try
      {
        return Controller.GalleryObjectController.GetMediaItemsInAlbum(id, (MetadataItemName)sortByMetaNameId, sortAscending);
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find album with ID = {0}", id)),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

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
    /// Gets the meta items for the specified album <paramref name="id" />.
    /// </summary>
    /// <param name="id">The album ID.</param>
    /// <returns>IQueryable&lt;Entity.MetaItem&gt;.</returns>
    /// <exception cref="StringContent"></exception>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    [ActionName("Meta")]
    public IQueryable<Entity.MetaItem> GetMetaItemsForAlbumId(int id)
    {
      // GET /api/albums/12/meta - Gets metadata items for album #12
      try
      {
        return AlbumController.GetMetaItemsForAlbum(id).AsQueryable();
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find album with ID = {0}", id)),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

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
    /// Persists the <paramref name="album" /> to the data store. Only the following properties are persisted: 
    /// <see cref="Entity.Album.SortById" />, <see cref="Entity.Album.SortUp" />, <see cref="Entity.Album.IsPrivate" />
    /// </summary>
    /// <param name="album">The album to persist.</param>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the album isn't found in the data store,
    /// the current user doesn't have permission to edit the album, or some other error occurs.
    /// </exception>
    public void Post(Entity.Album album)
    {
      // POST api/albums/post
      try
      {
        AlbumController.UpdateAlbum(album);
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent(String.Format("Could not find album with ID = {0}", album.Id)),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
          Content = new StringContent(ex.Message),
          ReasonPhrase = "Cannot Save Album"
        });
      }
      catch (NotSupportedException ex)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = new StringContent(ex.Message),
          ReasonPhrase = "Business Rule Violation"
        });
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex, album.GalleryId);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }

    /// <summary>
    /// Create an album based on <paramref name="album" />. The only properties used in the <paramref name="album" /> parameter are
    /// <see cref="Entity.Album.Title" /> and <see cref="Entity.Album.ParentId" />. If <see cref="Entity.Album.GalleryId" /> is 
    /// specified and an error occurs, it is used to help with error logging. Other properties are ignored, but if they need to be
    /// persisted in the future, this method can be modified to persist them. The parent album is resorted after the album is added.
    /// </summary>
    /// <param name="album">An <see cref="Entity.Album" /> instance containing data to be persisted to the data store.</param>
    /// <returns>The ID of the newly created album.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the current user doesn't have permission to edit the album
    /// or some other error occurs.
    /// </exception>
    [ActionName("CreateAlbum")]
    public ActionResult Put(Entity.Album album)
    {
      try
      {
        AlbumController.CreateAlbum(album);

        return new ActionResult
        {
          Status = ActionResultStatus.Success.ToString(),
          Title = $"Successfully created album {album.Title}",
          Message = string.Empty,
          ActionTarget = album
        };
      }
      catch (InvalidAlbumException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = "Cannot Create Album",
          Message = ex.Message
        };
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex, album.GalleryId);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }

    /// <summary>
    /// Deletes the album with the specified <paramref name="id" /> from the data store.
    /// </summary>
    /// <param name="id">The ID of the album to delete.</param>
    /// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the current user doesn't have
    /// permission to delete the album, deleting the album would violate a business rule, or some other
    /// error occurs.
    /// </exception>
    public HttpResponseMessage Delete(int id)
    {
      try
      {
        AlbumController.DeleteAlbum(id);

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(String.Format("Album {0} deleted...", id)) };
      }
      catch (InvalidAlbumException)
      {
        // HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
        // the same effect as deleting an existing one. So we simply return HttpStatusCode.OK.
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(String.Format("Album with ID = {0} does not exist.", id)) };
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }
      catch (CannotDeleteAlbumException ex)
      {
        AppEventController.LogError(ex);

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
    /// Sorts the <paramref name="galleryItems" /> in the order in which they are passed.
    /// This method is used when a user is manually sorting an album and has dragged an item to a new position.
    /// The operation occurs asynchronously and returns immediately.
    /// </summary>
    /// <param name="galleryItems">The gallery objects to sort. Their position in the array indicates the desired
    /// sequence. Only <see cref="Entity.GalleryItem.Id" /> and <see cref="Entity.GalleryItem.ItemType" /> need be 
    /// populated.</param>
    [HttpPost]
    [ActionName("SortGalleryObjects")]
    public void Sort(Entity.GalleryItem[] galleryItems)
    {
      try
      {
        var userName = Utils.UserName;
        Task.Factory.StartNew(() => AlbumController.Sort(galleryItems, userName));
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
    /// Sorts the <paramref name="album" /> by the <see cref="Entity.Album.SortById" /> and <see cref="Entity.Album.SortUp" /> properties,
    /// optionally updating the album with this sort preference. When <paramref name="persistToAlbum" /> is <c>true</c>, a physical album
    /// must be specified (ID > 0). When <c>false</c> and the album is virtual, the <see cref="Entity.Album.GalleryItems" /> property must 
    /// be specified.
    /// </summary>
    /// <param name="album">The album to be sorted.</param>
    /// <param name="persistToAlbum">if set to <c>true</c> the album is updated to use the specified sort preferences for all users.</param>
    /// <returns>IQueryable&lt;Entity.GalleryItem&gt;.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the user lacks appropriate permission or an unexpected error occurs.</exception>
    [HttpPost]
    [ActionName("SortAlbum")]
    public IQueryable<Entity.GalleryItem> Sort(Entity.Album album, bool persistToAlbum)
    {
      try
      {
        if (persistToAlbum)
        {
          // Change the sort of an existing album for all users.
          if (album.Id <= 0)
          {
            throw new ArgumentException("An album ID must be specified when calling the AlbumsController.Sort() Web.API method with the persistToAlbum parameter set to true.");
          }

          AlbumController.Sort(album.Id, album.SortById, album.SortUp);

          return GalleryObjectController.ToGalleryItems(AlbumController.LoadAlbumInstance(album.Id).GetChildGalleryObjects().ToSortedList()).AsQueryable();
        }
        else
        {
          return GalleryObjectController.SortGalleryItems(album);
        }
      }
      catch (InvalidAlbumException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
          Content = new StringContent($"Could not find album with ID {album.Id}. It may have been deleted by another user."),
          ReasonPhrase = "Album Not Found"
        });
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

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
    /// Moves the <paramref name="itemsToMove" /> to the <paramref name="destinationAlbumId" />.
    /// </summary>
    /// <param name="destinationAlbumId">The ID of the destination album.</param>
    /// <param name="itemsToMove">The items to transfer.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    [HttpPost, ActionName("MoveToAlbum")]
    public ActionResult MoveTo(int destinationAlbumId, Entity.GalleryItem[] itemsToMove)
    {
      // POST /api/albums/movetoalbum?destinationAlbumId=99
      return TransferTo(destinationAlbumId, itemsToMove, GalleryAssetTransferType.Move);
    }

    /// <summary>
    /// Copies the <paramref name="itemsToCopy" /> to the <paramref name="destinationAlbumId" />.
    /// </summary>
    /// <param name="destinationAlbumId">The ID of the destination album.</param>
    /// <param name="itemsToCopy">The items to transfer.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    [HttpPost, ActionName("CopyToAlbum")]
    public ActionResult CopyTo(int destinationAlbumId, Entity.GalleryItem[] itemsToCopy)
    {
      // POST /api/albums/copytoalbum?destinationAlbumId=99
      return TransferTo(destinationAlbumId, itemsToCopy, GalleryAssetTransferType.Copy);
    }

    /// <summary>
    /// Assign the thumbnail image assocated with <paramref name="galleryItem" /> to the <paramref name="albumId" />.
    /// </summary>
    /// <param name="galleryItem">The gallery item containing the thumbnail image to assign.</param>
    /// <param name="albumId">The ID of the album to be updated with a new thumbnail image.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    [HttpPost, ActionName("AssignThumbnail")]
    public ActionResult AssignThumbnail(Entity.GalleryItem galleryItem, int albumId)
    {
      // POST /api/albums/assignthumbnail?albumId=99
      try
      {
        var album = AlbumController.AssignThumbnail(galleryItem, albumId);

        return new ActionResult()
        {
          Status = ActionResultStatus.Success.ToString(),
          Title = "Thumbnail Assigned",
          Message = $"The media asset '{Utils.RemoveHtmlTags(galleryItem.Title)}' has been set as the thumbnail image for the album '{Utils.RemoveHtmlTags(album.Title)}'."
        };
      }
      catch (GallerySecurityException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = Resources.GalleryServer.Task_AssignThumbnail_Cannot_Assign_Thumbnail_Msg_Hdr,
          Message = ex.Message
        };
      }
      catch (InvalidAlbumException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = Resources.GalleryServer.Task_AssignThumbnail_Cannot_Assign_Thumbnail_Msg_Hdr,
          Message = ex.Message
        };
      }
      catch (InvalidMediaObjectException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = Resources.GalleryServer.Task_AssignThumbnail_Cannot_Assign_Thumbnail_Msg_Hdr,
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
    /// Change the album owner for the <paramref name="albumId" />. When successful, the updated album owner name is returned as a string on the
    /// <see cref="ActionResult.ActionTarget" /> property (with the case corrected to match the username if necessary).
    /// </summary>
    /// <param name="albumId">The ID of the album to be updated with the new owner.</param>
    /// <param name="ownerName">Name of the album owner. Must map to an existing user name</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    [HttpPost, ActionName("ChangeAlbumOwner")]
    public ActionResult ChangeAlbumOwner(int albumId, string ownerName)
    {
      // POST /api/albums/changealbumowner?albumId=99&ownerName=Bob
      try
      {
        string oldOwnerName;
        var album = AlbumController.ChangeOwner(albumId, ownerName, out oldOwnerName);

        return new ActionResult()
        {
          Status = ActionResultStatus.Success.ToString(),
          Title = Resources.GalleryServer.UC_Album_Owner_Changed_Hdr,
          Message = (string.IsNullOrWhiteSpace(ownerName) ? string.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.UC_Album_Owner_Removed, oldOwnerName) : string.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.UC_Album_Owner_Changed_Dtl, album.OwnerUserName)),
          ActionTarget = album.OwnerUserName
        };
      }
      catch (GallerySecurityException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = "Cannot Change Album Owner",
          Message = ex.Message
        };
      }
      catch (InvalidAlbumException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = "Cannot Change Album Owner",
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
    /// Calculates the total file size, in KB, of all the original files in the <paramref name="galleryItem" />, including all
    /// child albums and assigns it to the <see cref="Entity.DisplayObject.FileSizeKB" /> property of the original display object
    /// in the <see cref="Entity.GalleryItem.Views" /> property (the original display object view is added if it is not already present).
    /// The total includes only those items where a web-optimized version also exists. No action is taken if <paramref name="galleryItem" />
    /// is not an album or refers to an album that no longer exists.
    /// </summary>
    /// <param name="galleryItem">The gallery item. It is expected to be an album (<see cref="Entity.GalleryItem.IsAlbum" /> == <c>true</c>).
    /// It is updated with the calculated file size value.</param>
    /// <returns>An instance of <see cref="Entity.GalleryItem" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an internal server error occurs.</exception>
    [HttpPost]
    [ActionName("CalculateOriginalFileSize")]
    public Entity.GalleryItem CalculateOriginalFileSize(Entity.GalleryItem galleryItem)
    {
      try
      {
        AlbumController.CalculateOriginalFileSize(galleryItem);

        return galleryItem;
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
    /// Moves or copies the <paramref name="itemsToTransfer" /> to the <paramref name="destinationAlbumId" />.
    /// </summary>
    /// <param name="destinationAlbumId">The ID of the destination album.</param>
    /// <param name="itemsToTransfer">The items to transfer.</param>
    /// <param name="transferType">Type of the transfer.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    private static ActionResult TransferTo(int destinationAlbumId, Entity.GalleryItem[] itemsToTransfer, GalleryAssetTransferType transferType)
    {
      try
      {
        Entity.GalleryItem[] createdGalleryItems;
        var destinationAlbum = AlbumController.TransferToAlbum(destinationAlbumId, itemsToTransfer, transferType, out createdGalleryItems);

        return new ActionResult()
        {
          Status = ActionResultStatus.Success.ToString(),
          Title = GetTransferSuccessHeader(transferType),
          Message = GetTransferSuccessMessage(destinationAlbum, itemsToTransfer, transferType),
          ActionTarget = createdGalleryItems
        };
      }
      catch (GallerySecurityException)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = Resources.GalleryServer.Task_Transfer_Objects_Cannot_Transfer_No_Permission_Msg_Hdr,
          Message = Resources.GalleryServer.Task_Transfer_Objects_Cannot_Transfer_No_Permission_Msg_Dtl
        };
      }
      catch (InvalidAlbumException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = Resources.GalleryServer.Task_Transfer_Objects_Cannot_Transfer_To_Nested_Album_Msg_Hdr,
          Message = ex.Message
        };
      }
      catch (CannotTransferAlbumToNestedDirectoryException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = Resources.GalleryServer.Task_Transfer_Objects_Cannot_Transfer_To_Nested_Album_Msg_Hdr,
          Message = ex.Message
        };
      }
      catch (UnsupportedMediaObjectTypeException ex)
      {
        return new ActionResult()
        {
          Status = ActionResultStatus.Error.ToString(),
          Title = Resources.GalleryServer.Task_Transfer_Objects_Cannot_Transfer_UnsupportedFileType_Msg_Hdr,
          Message = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Task_Transfer_Objects_Cannot_Transfer_UnsupportedFileType_Msg_Dtl, System.IO.Path.GetExtension(ex.MediaObjectFilePath)),
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
    /// Gets a friendly header message to be shown to the user when a transfer successfully completes.
    /// </summary>
    /// <param name="transferType">Type of the transfer.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="transferType" /> has an unexpected value.</exception>
    private static string GetTransferSuccessHeader(GalleryAssetTransferType transferType)
    {
      var hdr = Resources.GalleryServer.Task_Transfer_Objects_Transfer_Successful_Hdr;

      switch (transferType)
      {
        case GalleryAssetTransferType.Move:
          return string.Format(hdr, Resources.GalleryServer.Task_Transfer_Objects_TransferType_Move_Text2);

        case GalleryAssetTransferType.Copy:
          return string.Format(hdr, Resources.GalleryServer.Task_Transfer_Objects_TransferType_Copy_Text2);

        default:
          throw new ArgumentException($"Encountered unexpected GalleryAssetTransferType enum value '{transferType}'.");
      }
    }

    /// <summary>
    /// Generates a friendly message to be shown to the user when a transfer successfully completes.
    /// </summary>
    /// <param name="destinationAlbum">The destination album.</param>
    /// <param name="itemsTransferred">The items that were moved or copied.</param>
    /// <param name="transferType">Type of the transfer.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="transferType" /> has an unexpected value.</exception>
    private static string GetTransferSuccessMessage(IAlbum destinationAlbum, Entity.GalleryItem[] itemsTransferred, GalleryAssetTransferType transferType)
    {
      // Ex: Graduation day.JPG was successfully copied to the album My Vacation.
      // Ex: The selected items were successfully copied to the album My Vacation.
      var destAlbumUrl = Utils.GetUrl(PageId.album, "aid={0}", destinationAlbum.Id);
      var destAlbumTitle = Utils.RemoveHtmlTags(destinationAlbum.Title);

      string transferString;
      switch (transferType)
      {
        case GalleryAssetTransferType.Move:
          transferString = Resources.GalleryServer.Task_Transfer_Objects_TransferType_Move_Text1;
          break;
        case GalleryAssetTransferType.Copy:
          transferString = Resources.GalleryServer.Task_Transfer_Objects_TransferType_Copy_Text1;
          break;
        default:
          throw new ArgumentException($"Encountered unexpected GalleryAssetTransferType enum value '{transferType}'.");
      }

      if (itemsTransferred.Length == 1)
      {
        var url = (itemsTransferred[0].IsAlbum ? Utils.GetUrl(PageId.album, "aid={0}", itemsTransferred[0].Id) : Utils.GetUrl(PageId.mediaobject, "moid={0}", itemsTransferred[0].Id));

        return string.Format(Resources.GalleryServer.Task_Transfer_Objects_Transfer_Successful_Single_Dtl, url, itemsTransferred[0].Title, transferString, destAlbumUrl, destAlbumTitle);
      }
      else
      {
        return string.Format(Resources.GalleryServer.Task_Transfer_Objects_Transfer_Successful_Multiple_Dtl, transferString, destAlbumUrl, destAlbumTitle);

      }
    }

  }
}