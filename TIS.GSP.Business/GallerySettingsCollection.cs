using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a set of gallery-specific settings.
  /// </summary>
  public class GallerySettingsCollection : IGallerySettingsCollection
  {
    /// The items in the collection. The gallery ID is the key.
    private readonly ConcurrentDictionary<int, IGallerySettings> _items  = new ConcurrentDictionary<int, IGallerySettings>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GallerySettingsCollection"/> class.
    /// </summary>
    public GallerySettingsCollection()
    {
    }

    /// <summary>
    /// Adds the gallery settings to the current collection.
    /// </summary>
    /// <param name="gallerySettings">The gallery settings to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="gallerySettings" /> is null.</exception>
    public void AddRange(IEnumerable<IGallerySettings> gallerySettings)
    {
      if (gallerySettings == null)
        throw new ArgumentNullException(nameof(gallerySettings));

      foreach (IGallerySettings gallerySetting in gallerySettings)
      {
        this.Add(gallerySetting);
      }
    }

    /// <summary>
    /// Find the gallery settings in the collection that matches the specified <paramref name="galleryId"/>. If no matching object is found,
    /// null is returned.
    /// </summary>
    /// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
    /// <returns>
    /// Returns an <see cref="IGallerySettings"/>object from the collection that matches the specified <paramref name="galleryId"/>,
    /// or null if no matching object is found.
    /// </returns>
    public IGallerySettings FindByGalleryId(int galleryId)
    {
      IGallerySettings gallerySettings;

      _items.TryGetValue(galleryId, out gallerySettings);

      return gallerySettings;
    }

    /// <summary>
    /// Remove the items in the collection.
    /// </summary>
    public void Clear()
    {
      _items.Clear();
    }

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    /// <value>The count.</value>
    public int Count => _items.Count;

    /// <summary>
    /// Adds the specified gallery.
    /// </summary>
    /// <param name="item">The gallery to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
    public void Add(IGallerySettings item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Cannot add null to an existing GallerySettingsCollection. Items.Count = " + _items.Count);

      _items.TryAdd(item.GalleryId, item);
    }

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
    /// <returns><see cref="System.Collections.Generic.IEnumerator&lt;IGallerySettings&gt;" />.</returns>
    public IEnumerator<IGallerySettings> GetEnumerator()
    {
      return _items.Values.GetEnumerator();
    }
  }
}
