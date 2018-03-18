using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
  /// <summary>
  /// Contains methods for Web API access for modifying metadata tags for multiple gallery objects.
  /// Use <see cref="MetaController" /> for updating a metadata item for a single gallery object.
  /// </summary>
  public class GalleryItemMetaController : ApiController
  {
    /// <summary>
    /// Gets the meta items for the specified <paramref name="galleryItems" />.
    /// </summary>
    /// <param name="galleryItems">An array of <see cref="Entity.GalleryItem" /> instances.</param>
    /// <returns>Returns a merged set of metadata.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    [HttpPost]
    [ActionName("GalleryItems")]
    public IQueryable<Entity.MetaItem> GetMetaItemsForGalleryItems(Entity.GalleryItem[] galleryItems)
    {
      // GET /api/meta/galleryitems - Gets metadata items for the specified objects
      try
      {
        return MetadataController.GetMetaItemsForGalleryItems(galleryItems).AsQueryable();
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
    /// Gets a value indicating whether the logged-on user has edit permission for all of the <paramref name="galleryItems" />.
    /// </summary>
    /// <param name="galleryItems">A collection of <see cref="Entity.GalleryItem" /> instances.</param>
    /// <returns><c>true</c> if the current user can edit the items; <c>false</c> otherwise.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    [HttpPost]
    public bool CanUserEdit(System.Collections.Generic.IEnumerable<Entity.GalleryItem> galleryItems)
    {
      // POST /api/meta/canuseredit
      try
      {
        return MetadataController.CanUserEditAllItems(galleryItems);
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
    /// Updates the gallery items with the specified metadata value. <see cref="Entity.GalleryItemMeta.ActionResult" />
    /// contains details about the success or failure of the operation.
    /// </summary>
    /// <param name="galleryItemMeta">An instance of <see cref="Entity.GalleryItemMeta" /> that defines
    /// the tag value to be added and the gallery items it is to be added to. It is expected that only
    /// the MTypeId and Value properties of <see cref="Entity.GalleryItemMeta.MetaItem" /> are populated.</param>
    /// <returns>An instance of <see cref="Entity.GalleryItemMeta" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the current user does not have permission
    /// to carry out the operation or an internal server error occurs.</exception>
    public Entity.GalleryItemMeta PutGalleryItemMeta(Entity.GalleryItemMeta galleryItemMeta)
    {
      // /api/galleryitemmeta
      try
      {
        MetadataController.SaveGalleryItemMeta(galleryItemMeta);

        if (galleryItemMeta.ActionResult == null)
        {
          galleryItemMeta.ActionResult = new ActionResult()
          {
            Status = ActionResultStatus.Success.ToString(),
            Title = "Save successful"
          };
        }
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

      return galleryItemMeta;
    }

    /// <summary>
    /// Deletes the meta tag value from the specified gallery items.
    /// </summary>
    /// <param name="galleryItemMeta">An instance of <see cref="Entity.GalleryItemMeta" /> that defines
    /// the tag value to be added and the gallery items it is to be added to.</param>
    /// <returns><see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    public HttpResponseMessage DeleteGalleryItemMeta(Entity.GalleryItemMeta galleryItemMeta)
    {
      // /api/galleryitemmeta
      try
      {
        var mType = (MetadataItemName)galleryItemMeta.MetaItem.MTypeId;
        if (mType == MetadataItemName.Tags || mType == MetadataItemName.People)
        {
          MetadataController.DeleteTag(galleryItemMeta);
        }
        else
        {
          MetadataController.Delete(galleryItemMeta);
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
          Content = new StringContent("Meta item deleted...")
        };
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