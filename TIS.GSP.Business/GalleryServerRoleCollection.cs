using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// An unsorted collection of <see cref="IGalleryServerRole" /> objects.
  /// </summary>
  public class GalleryServerRoleCollection : IGalleryServerRoleCollection
  {
    /// The items in the collection. The role name (lower case) is the key.
    private readonly ConcurrentDictionary<string, IGalleryServerRole> _items = new ConcurrentDictionary<string, IGalleryServerRole>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GalleryServerRoleCollection"/> class.
    /// </summary>
    public GalleryServerRoleCollection()
    {
    }

    /// <summary>
    /// Adds the roles to the current collection.
    /// </summary>
    /// <param name="roles">The roles to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roles" /> is null.</exception>
    public void AddRange(IEnumerable<IGalleryServerRole> roles)
    {
      if (roles == null)
        throw new ArgumentNullException(nameof(roles));

      foreach (var role in roles)
      {
        this.Add(role);
      }
    }

    /// <summary>
    /// Adds the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
    public void Add(IGalleryServerRole item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item), "Cannot add null to an existing GalleryServerRoleCollection. Items.Count = " + _items.Count);

      _items.TryAdd(item.RoleName.ToLowerInvariant(), item);
    }

    /// <summary>
    /// Return a list of roles sorted on the <see cref="P:GalleryServer.Business.Interfaces.IGalleryServerRole.RoleName" /> property.
    /// </summary>
    /// <returns>List&lt;IGalleryServerRole&gt;.</returns>
    public List<IGalleryServerRole> GetSortedList()
    {
      var roles = new List<IGalleryServerRole>(_items.Values);

      roles.Sort();

      return roles;
    }

    ///// <summary>
    ///// Sort the objects in this collection by the <see cref="IGalleryServerRole.RoleName" /> property.
    ///// </summary>
    //public void Sort()
    //{
    //	// We know galleryServerRoles is actually a List<IGalleryServerRole> because we passed it to the constructor.
    //	System.Collections.Generic.List<IGalleryServerRole> galleryServerRoles = (System.Collections.Generic.List<IGalleryServerRole>)Items;

    //	galleryServerRoles.Sort();
    //}

    /// <summary>
    /// Creates a new collection containing deep copies of the items it contains.
    /// </summary>
    /// <returns>
    /// Returns a new collection containing deep copies of the items it contains.
    /// </returns>
    public IGalleryServerRoleCollection Copy()
    {
      IGalleryServerRoleCollection copy = new GalleryServerRoleCollection();

      foreach (var role in _items.Values)
      {
        copy.Add(role.Copy());
      }

      return copy;
    }

    /// <summary>
    /// Return the role that matches the specified <paramref name="roleName"/>. It is not case sensitive, so that 
    /// "ReadAll" matches "readall". Returns null if no match is found.
    /// </summary>
    /// <param name="roleName">The name of the role to return.</param>
    /// <returns>
    /// Returns the role that matches the specified role name. Returns null if no match is found.
    /// </returns>
    public IGalleryServerRole GetRole(string roleName)
    {
      IGalleryServerRole role;

      _items.TryGetValue(roleName.ToLowerInvariant(), out role);

      return role;
    }

    /// <summary>
    /// Gets the Gallery Server roles that match the specified <paramref name="roleName"/>. It is not case sensitive,
    /// so that "ReadAll" matches "readall". Will return multiple roles with the same name when the gallery is assigned
    /// to more than one gallery.
    /// </summary>
    /// <param name="roleName">The name of the role to return.</param>
    /// <returns>
    /// Returns the Gallery Server roles that match the specified <paramref name="roleName"/>.
    /// </returns>
    /// <overloads>
    /// Gets the Gallery Server roles that match the specified parameters.
    /// </overloads>
    public IGalleryServerRoleCollection GetRoles(string roleName)
    {
      return GetRoles(new string[] { roleName });
    }

    /// <overloads>
    /// Gets the Gallery Server roles that match the specified parameters.
    /// </overloads>
    /// <summary>
    /// Gets the Gallery Server roles that match the specified <paramref name="roleNames"/>. It is not case sensitive,
    /// so that "ReadAll" matches "readall".
    /// </summary>
    /// <param name="roleNames">The name of the roles to return.</param>
    /// <returns>
    /// Returns the Gallery Server roles that match the specified <paramref name="roleNames"/>.
    /// </returns>
    /// <exception cref="InvalidGalleryServerRoleException">Thrown when one or more of the requested role names could not be found
    /// in the current collection.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roleNames" /> is null.</exception>
    public IGalleryServerRoleCollection GetRoles(IEnumerable<string> roleNames)
    {
      if (roleNames == null)
        throw new ArgumentNullException(nameof(roleNames));

      IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();
      foreach (var roleName in roleNames)
      {
        IGalleryServerRole role;

        if (_items.TryGetValue(roleName.ToLowerInvariant(), out role))
        {
          roles.Add(role);
        }
        else
        {
          throw new InvalidGalleryServerRoleException(String.Format(CultureInfo.CurrentCulture, "Could not find a Gallery Server role named '{0}'. Verify the data table contains a record for this role, and that the cache is being properly managed.", roleName));
        }
      }

      return roles;
    }

    /// <summary>
    /// Gets the Gallery Server roles with AllowAdministerGallery permission, including roles with AllowAdministerSite permission.
    /// </summary>
    /// <returns>Returns the Gallery Server roles with AllowAdministerGallery permission.</returns>
    public IGalleryServerRoleCollection GetRolesWithGalleryAdminPermission()
    {
      IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

      roles.AddRange(_items.Values.Where(r => r.AllowAdministerGallery));

      return roles;
    }

    /// <summary>
    /// Gets the album IDs for which the roles provide view permission. The list is generated from the album IDs already present 
    /// in the roles. May return album IDs that belong to other galleries, so if the presence of these IDs may cause an issue, 
    /// be sure to filter them out. An example of this can be found in 
    /// <see cref="GalleryObjectSearcher.RemoveChildAlbumsAndAlbumsInOtherGalleries(IEnumerable&lt;int&gt;)" />.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>IEnumerable{System.Int32}.</returns>
    public IEnumerable<int> GetViewableAlbumIdsForGallery(int galleryId)
    {
      IIntegerCollection albumIds = new IntegerCollection();

      foreach (var role in _items.Values.Where(role => role.AllowViewAlbumOrMediaObject && role.Galleries.Any(g => g.GalleryId == galleryId)))
      {
        albumIds.AddRange(role.AllAlbumIds);
      }

      return albumIds.Distinct();
    }

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    /// <value>The count.</value>
    public int Count => _items.Count;

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns><see cref="System.Collections.IEnumerator" />.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns><see cref="System.Collections.Generic.IEnumerator&lt;IGalleryServerRole&gt;" />.</returns>
    public IEnumerator<IGalleryServerRole> GetEnumerator()
    {
      return _items.Values.GetEnumerator();
    }
  }
}
