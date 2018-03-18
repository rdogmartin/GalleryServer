using System.Collections.Generic;
using System.Linq;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the RoleAlbum table.
  /// </summary>
  public class RoleAlbumRepository : Repository<GalleryDb, RoleAlbumDto>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleAlbumRepository"/> class.
    /// </summary>
    public RoleAlbumRepository() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleAlbumRepository"/> class.
    /// </summary>
    /// <param name="ctx">The data context.</param>
    public RoleAlbumRepository(GalleryDb ctx)
    {
      Context = ctx;
    }

    /// <summary>
    /// Save the list of root album IDs to the data store. The table gs_Role_Album contains one record for each role/album
    /// relationship. This procedure adds and deletes records as needed.
    /// </summary>
    public void Save(string roleName, IEnumerable<int> albumIds)
    {
      // Step 1: Copy the list of root album IDs to a new list. We'll be removing items from the list as we process them,
      // so we don't want to mess with the actual list attached to the object.
      var roleAlbumRelationshipsToPersist = albumIds.ToList();

      // Step 2: Get a list of all root album IDs in the data store for this role.
      var roleAlbumRelationshipsToDelete = new List<int>();
      //foreach (int albumId in (from ra in ctx.RoleAlbums where ra.FKRoleName == role.RoleName select ra.FKAlbumId))
      foreach (var albumId in Where(r => r.FKRoleName == roleName).Select(r => r.FKAlbumId))
      {
        // Step 3: Iterate through each role/album relationship that is stored in the data store. If it is in our list, then
        // remove it from the list (see step 5 why). If not, the user must have unchecked it so add it to a list of 
        // relationships to be deleted.
        if (roleAlbumRelationshipsToPersist.Contains(albumId))
        {
          roleAlbumRelationshipsToPersist.Remove(albumId);
        }
        else
        {
          roleAlbumRelationshipsToDelete.Add(albumId);
        }
      }

      // Step 4: Delete the records we accumulated in our list.
      foreach (var roleAlbumDto in Where(r => r.FKRoleName == roleName && roleAlbumRelationshipsToDelete.Contains(r.FKAlbumId)))
      {
        Delete(roleAlbumDto);
      }

      // Step 5: Any items still left in the roleAlbumRelationshipsToPersist list must be new ones checked by the user. Add them.
      foreach (var albumid in roleAlbumRelationshipsToPersist)
      {
        Add(new RoleAlbumDto { FKAlbumId = albumid, FKRoleName = roleName });
      }

      Save();
    }
  }
}