using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the Role table.
  /// </summary>
  public class RoleRepository : Repository<GalleryDb, RoleDto>
  {
    /// <summary>
    /// Saves the specified role.
    /// </summary>
    /// <param name="role">The role.</param>
    public void Save(IGalleryServerRole role)
    {
      PersistRoleToDataStore(role);

      var repo = new RoleAlbumRepository(Context);
      repo.Save(role.RoleName, role.RootAlbumIds);
    }

    private void PersistRoleToDataStore(IGalleryServerRole role)
    {
      // Update the existing role or insert if it doesn't exist.
      var roleDto = Find(role.RoleName);

      if (roleDto == null)
      {
        roleDto = new RoleDto
                    {
                      RoleName = role.RoleName,
                      AllowViewAlbumsAndObjects = role.AllowViewAlbumOrMediaObject,
                      AllowViewOriginalImage = role.AllowViewOriginalImage,
                      AllowAddChildAlbum = role.AllowAddChildAlbum,
                      AllowAddMediaObject = role.AllowAddMediaObject,
                      AllowEditAlbum = role.AllowEditAlbum,
                      AllowEditMediaObject = role.AllowEditMediaObject,
                      AllowDeleteChildAlbum = role.AllowDeleteChildAlbum,
                      AllowDeleteMediaObject = role.AllowDeleteMediaObject,
                      AllowSynchronize = role.AllowSynchronize,
                      HideWatermark = role.HideWatermark,
                      AllowAdministerGallery = role.AllowAdministerGallery,
                      AllowAdministerSite = role.AllowAdministerSite
                    };

        Add(roleDto);
      }
      else
      {
        roleDto.AllowViewAlbumsAndObjects = role.AllowViewAlbumOrMediaObject;
        roleDto.AllowViewOriginalImage = role.AllowViewOriginalImage;
        roleDto.AllowAddChildAlbum = role.AllowAddChildAlbum;
        roleDto.AllowAddMediaObject = role.AllowAddMediaObject;
        roleDto.AllowEditAlbum = role.AllowEditAlbum;
        roleDto.AllowEditMediaObject = role.AllowEditMediaObject;
        roleDto.AllowDeleteChildAlbum = role.AllowDeleteChildAlbum;
        roleDto.AllowDeleteMediaObject = role.AllowDeleteMediaObject;
        roleDto.AllowSynchronize = role.AllowSynchronize;
        roleDto.HideWatermark = role.HideWatermark;
        roleDto.AllowAdministerGallery = role.AllowAdministerGallery;
        roleDto.AllowAdministerSite = role.AllowAdministerSite;
      }

      Save();
    }

  }
}