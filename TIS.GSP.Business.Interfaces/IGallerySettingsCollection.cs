using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// A collection of <see cref="IGallerySettings" /> objects.
  /// </summary>
  public interface IGallerySettingsCollection : IEnumerable<IGallerySettings>
  {
    /// <summary>
    /// Adds the gallery settings to the current collection.
    /// </summary>
    /// <param name="gallerySettings">The gallery settings to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="gallerySettings" /> is null.</exception>
    void AddRange(System.Collections.Generic.IEnumerable<IGallerySettings> gallerySettings);

    /// <summary>
    /// Adds the specified gallery settings.
    /// </summary>
    /// <param name="item">The gallery settings to add.</param>
    void Add(IGallerySettings item);

    /// <summary>
    /// Find the gallery settings in the collection that matches the specified <paramref name="galleryId" />. If no matching object is found,
    /// null is returned.
    /// </summary>
    /// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
    /// <returns>Returns an <see cref="IGallerySettings" />object from the collection that matches the specified <paramref name="galleryId" />,
    /// or null if no matching object is found.</returns>
    IGallerySettings FindByGalleryId(int galleryId);

    /// <summary>
    /// Remove the items in the collection.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    /// <value>The count.</value>
    int Count { get; }
  }
}
