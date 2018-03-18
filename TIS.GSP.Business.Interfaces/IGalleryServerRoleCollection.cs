using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// An unsorted collection of <see cref="IGalleryServerRole" /> objects.
  /// </summary>
  public interface IGalleryServerRoleCollection : IEnumerable<IGalleryServerRole>
  {
    /// <summary>
    /// Adds the roles to the current collection.
    /// </summary>
    /// <param name="roles">The roles to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roles" /> is null.</exception>
    void AddRange(IEnumerable<IGalleryServerRole> roles);

    /// <summary>
    /// Adds the specified gallery server role.
    /// </summary>
    /// <param name="item">The gallery server role to add.</param>
    void Add(IGalleryServerRole item);

    /// <summary>
    /// Return a list of roles sorted on the <see cref="IGalleryServerRole.RoleName" /> property.
    /// </summary>
    /// <returns>List&lt;IGalleryServerRole&gt;.</returns>
    List<IGalleryServerRole> GetSortedList();

    /// <summary>
    /// Creates a new collection containing deep copies of the items it contains.
    /// </summary>
    /// <returns>Returns a new collection containing deep copies of the items it contains.</returns>
    IGalleryServerRoleCollection Copy();

    /// <summary>
    /// Return the role that matches the specified <paramref name="roleName"/>. It is not case sensitive, so
    ///  that "ReadAll" matches "readall". Returns null if no match is found.
    /// </summary>
    /// <param name="roleName">The name of the role to return.</param>
    /// <returns>
    /// Returns the role that matches the specified role name. Returns null if no match is found.
    /// </returns>
    IGalleryServerRole GetRole(string roleName);

    /// <summary>
    /// Gets the Gallery Server roles that match the specified <paramref name="roleNames" />. It is not case sensitive, 
    /// so that "ReadAll" matches "readall".
    /// </summary>
    /// <param name="roleNames">The name of the roles to return.</param>
    /// <returns>
    /// Returns the Gallery Server roles that match the specified <paramref name="roleNames" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleNames" /> is null.</exception>
    IGalleryServerRoleCollection GetRoles(IEnumerable<string> roleNames);

    /// <summary>
    /// Gets the Gallery Server roles with AllowAdministerGallery permission, including roles with AllowAdministerSite permission.
    /// </summary>
    /// <returns>Returns the Gallery Server roles with AllowAdministerGallery permission.</returns>
    IGalleryServerRoleCollection GetRolesWithGalleryAdminPermission();

    /// <summary>
    /// Gets the album IDs for which the roles provide view permission. The list is generated from the album IDs already present 
    /// in the roles. May return album IDs that belong to other galleries, so if the presence of these IDs may cause an issue, 
    /// be sure to filter them out. An example of this can be found in 
    /// GalleryObjectSearcher.RemoveChildAlbumsAndAlbumsInOtherGalleries(IEnumerable&lt;int&gt;).
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>IEnumerable{System.Int32}.</returns>
    IEnumerable<int> GetViewableAlbumIdsForGallery(int galleryId);

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    /// <value>The count.</value>
    int Count { get; }

    //	/// <summary>
    ///// Gets a reference to the IGalleryServerRole object at the specified index position.
    ///// </summary>
    ///// <param name="indexPosition">An integer specifying the position of the object within this collection to
    ///// return. Zero returns the first item.</param>
    ///// <returns>Returns a reference to the IGalleryServerRole object at the specified index position.</returns>
    //IGalleryServerRole this[Int32 indexPosition]
    //{
    //	get;
    //	set;
    //}

    ///// <summary>
    ///// Searches for the specified object and returns the zero-based index of the first occurrence within the collection.  
    ///// </summary>
    ///// <param name="galleryServerRole">The gallery server role to locate in the collection. The value can be a null 
    ///// reference (Nothing in Visual Basic).</param>
    ///// <returns>The zero-based index of the first occurrence of galleryServerRole within the collection, if found; 
    ///// otherwise, –1. </returns>
    //Int32 IndexOf(IGalleryServerRole galleryServerRole);

  }
}
