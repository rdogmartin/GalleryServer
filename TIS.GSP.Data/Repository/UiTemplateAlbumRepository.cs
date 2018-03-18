using System.Collections.Generic;
using System.Linq;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the UiTemplateAlbum table.
  /// </summary>
  public class UiTemplateAlbumRepository : Repository<GalleryDb, UiTemplateAlbumDto>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UiTemplateAlbumRepository"/> class.
    /// </summary>
    public UiTemplateAlbumRepository() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UiTemplateAlbumRepository"/> class.
    /// </summary>
    /// <param name="ctx">The data context.</param>
    public UiTemplateAlbumRepository(GalleryDb ctx)
    {
      Context = ctx;
    }

    /// <summary>
    /// Saves the specified <paramref name="rootAlbumIds" /> to the UI template having <paramref name="uiTemplateId" />.
    /// </summary>
    /// <param name="uiTemplateId">The UI template identifier.</param>
    /// <param name="rootAlbumIds">The root album ids.</param>
    public void Save(int uiTemplateId, IIntegerCollection rootAlbumIds)
    {
      // Step 1: Copy the list of root album IDs to a new list. We'll be removing items from the list as we process them,
      // so we don't want to mess with the actual list attached to the object.
      var templateAlbumRelationshipsToPersist = new List<int>();
      foreach (var albumId in rootAlbumIds)
      {
        templateAlbumRelationshipsToPersist.Add(albumId);
      }

      // Step 2: Iterate through each template/album relationship in the data store. If it is in our list, then
      // remove it (see step 4 why). If not, the user must have unchecked it so add it to a list of 
      // relationships to be deleted.
      var templateAlbumRelationshipsToDelete = new List<int>();
      foreach (var albumId in Where(j => j.FKUiTemplateId == uiTemplateId).Select(j => j.FKAlbumId))
      {
        if (templateAlbumRelationshipsToPersist.Contains(albumId))
        {
          templateAlbumRelationshipsToPersist.Remove(albumId);
        }
        else
        {
          templateAlbumRelationshipsToDelete.Add(albumId);
        }
      }

      // Step 3: Delete the records we accumulated in our list.
      foreach (UiTemplateAlbumDto roleAlbumDto in Where(j => j.FKUiTemplateId == uiTemplateId && templateAlbumRelationshipsToDelete.Contains(j.FKAlbumId)))
      {
        Delete(roleAlbumDto);
      }

      // Step 4: Any items still left in the templateAlbumRelationshipsToPersist list must be new ones 
      // checked by the user. Add them.
      foreach (int albumId in templateAlbumRelationshipsToPersist)
      {
        Add(new UiTemplateAlbumDto { FKUiTemplateId = uiTemplateId, FKAlbumId = albumId });
      }

      Save();
    }
  }
}