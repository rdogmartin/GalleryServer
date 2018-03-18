using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Entity;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Web.Api
{
  /// <summary>
  /// Contains methods for Web API access to media objects.
  /// </summary>
  public class GalleryItemsController : ApiController
  {
    /// <summary>
    /// Permanently deletes the specified <paramref name="galleryItems" /> from the file system and data store. No action is taken if the
    /// user does not have delete permission. The successfully deleted items are assigned to the <see cref="ActionResult.ActionTarget" />
    /// property of the returned instance.
    /// </summary>
    /// <param name="galleryItems">The gallery items to be deleted.</param>
    /// <param name="deleteFromFileSystem">if set to <c>true</c> [delete from file system].</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="HttpResponseException">Thrown when an unexpected error occurs.</exception>
    /// <exception cref="HttpResponseMessage">Thrown when an unexpected error occurs.</exception>
    [HttpDelete]
    [ActionName("Delete")]
    public ActionResult Delete(GalleryItem[] galleryItems, bool deleteFromFileSystem)
    {
      // DELETE galleryitems/delete
      try
      {
        return GalleryObjectController.DeleteGalleryItems(galleryItems, deleteFromFileSystem);
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
    /// Permanently delete the original file for all <paramref name="galleryItems" />, including any children if a gallery item is an album.
    /// If no optimized version exists, no action is taken on that media asset. Validation is performed to ensure the logged in user has 
    /// permission to edit the items and that no business rules are violated. The successfully processed items are assigned to the 
    /// <see cref="ActionResult.ActionTarget" /> property of the returned instance.
    /// </summary>
    /// <param name="galleryItems">The gallery items for which the original files are to be deleted.</param>
    /// <returns>An instance of <see cref="ActionResult" /> describing the result of the deletion.</returns>
    [HttpDelete]
    [ActionName("DeleteOriginalFiles")]
    public ActionResult DeleteOriginalFiles(GalleryItem[] galleryItems)
    {
      // DELETE galleryitems/deleteoriginalfiles
      try
      {
        return GalleryObjectController.DeleteOriginalFiles(galleryItems);
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
    /// Executes the requested <paramref name="rotateFlip" /> action on the <paramref name="galleryItems" />. Validation is performed
    /// to ensure logged on user has <see cref="SecurityActions.EditMediaObject" /> permission and that none of the items are in a read-only
    /// gallery.
    /// </summary>
    /// <param name="galleryItems">The gallery items to rotate or flip.</param>
    /// <param name="rotateFlip">The requested rotate / flip action.</param>
    /// <param name="viewSize">The size of the image the user is looking at.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    /// <exception cref="HttpResponseMessage"></exception>
    [HttpPost]
    [ActionName("RotateFlip")]
    public ActionResult RotateFlip(GalleryItem[] galleryItems, MediaAssetRotateFlip rotateFlip, DisplayObjectType viewSize)
    {
      try
      {
        return GalleryObjectController.RotateFlip(galleryItems, rotateFlip, viewSize);
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