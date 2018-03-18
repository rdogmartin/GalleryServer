using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// A collection of <see cref="IUserGalleryProfile" /> objects.
  /// </summary>
  /// <remarks>
  /// In March 2016 I changed this class to implement *only* IUserGalleryProfileCollection and not Collection, but I discovered that
  /// Newtonsoft is unable to deserialize a string in this scenario. There is probably a way to make it work but I couldn't figure it
  /// out after an hour, so I went back to the original implementation. I don't think we really needed to make this one thread-safe anyway.
  /// </remarks>
  [Serializable]
  public class UserGalleryProfileCollection : Collection<IUserGalleryProfile>, IUserGalleryProfileCollection
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserGalleryProfileCollection"/> class.
    /// </summary>
    public UserGalleryProfileCollection()
      : base(new List<IUserGalleryProfile>())
    {
    }

    /// <summary>
    /// Adds the specified user profile.
    /// </summary>
    /// <param name="item">The user profile to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
    public new void Add(IUserGalleryProfile item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item), "Cannot add null to an existing UserGalleryProfileCollection. Items.Count = " + Items.Count);

      base.Add(item);
    }

    /// <summary>
    /// Adds the gallery profiles to the current collection.
    /// </summary>
    /// <param name="galleryProfiles">The gallery profiles to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryProfiles" /> is null.</exception>
    public void AddRange(IEnumerable<IUserGalleryProfile> galleryProfiles)
    {
      if (galleryProfiles == null)
        throw new ArgumentNullException(nameof(galleryProfiles));

      foreach (IUserGalleryProfile galleryProfile in galleryProfiles)
      {
        this.Add(galleryProfile);
      }
    }

    /// <summary>
    /// Find the user account in the collection that matches the specified <paramref name="galleryId" />. If no matching object is found,
    /// null is returned.
    /// </summary>
    /// <param name="galleryId">The ID of the gallery.</param>
    /// <returns>Returns an <see cref="IUserGalleryProfile" />object from the collection that matches the specified <paramref name="galleryId" />,
    /// or null if no matching object is found.</returns>
    public IUserGalleryProfile FindByGalleryId(int galleryId)
    {
      List<IUserGalleryProfile> userAccounts = (List<IUserGalleryProfile>)Items;

      return userAccounts.Find(delegate (IUserGalleryProfile gallery)
      {
        return (gallery.GalleryId == galleryId);
      });
    }

    /// <summary>
    /// Creates a new instance of an <see cref="IUserGalleryProfile"/> object. This method can be used by code that only has a
    /// reference to the interface layer and therefore cannot create a new instance of an object on its own.
    /// </summary>
    /// <param name="galleryId">The ID of the gallery.</param>
    /// <returns>
    /// Returns a new instance of an <see cref="IUserGalleryProfile"/> object.
    /// </returns>
    public IUserGalleryProfile CreateNewUserGalleryProfile(int galleryId)
    {
      return new UserGalleryProfile(galleryId);
    }

    /// <summary>
    /// Creates a new collection containing deep copies of the items it contains.
    /// </summary>
    /// <returns>Returns a new collection containing deep copies of the items it contains.</returns>
    public IUserGalleryProfileCollection Copy()
    {
      IUserGalleryProfileCollection copy = new UserGalleryProfileCollection();

      foreach (var galleryProfile in (List<IUserGalleryProfile>)Items)
      {
        copy.Add(galleryProfile.Copy());
      }

      return copy;
    }
  }
}
