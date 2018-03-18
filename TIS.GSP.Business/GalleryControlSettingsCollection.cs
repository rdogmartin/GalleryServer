using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// A collection of <see cref="IGalleryControlSettings" /> objects. There is a maximum of one item for each instance of a Gallery
  /// control that is used in an application. An item will exist in this collection only if at least one control-specific setting
  /// has been saved for a particular control.
  /// </summary>
  public class GalleryControlSettingsCollection : IGalleryControlSettingsCollection
  {
    // The items in the collection. The control ID is the key.
    private readonly ConcurrentDictionary<string, IGalleryControlSettings> _items = new ConcurrentDictionary<string, IGalleryControlSettings>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GallerySettingsCollection"/> class.
    /// </summary>
    public GalleryControlSettingsCollection()
    {
    }

    /// <summary>
    /// Adds the gallery control settings to the current collection.
    /// </summary>
    /// <param name="galleryControlSettings">The gallery control settings to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryControlSettings" /> is null.</exception>
    public void AddRange(IEnumerable<IGalleryControlSettings> galleryControlSettings)
    {
      if (galleryControlSettings == null)
        throw new ArgumentNullException(nameof(galleryControlSettings));
      
      foreach (IGalleryControlSettings galleryControlSetting in galleryControlSettings)
      {
        this.Add(galleryControlSetting);
      }
    }

    /// <summary>
    /// Adds the specified gallery control settings.
    /// </summary>
    /// <param name="item">The gallery control settings to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
    public void Add(IGalleryControlSettings item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item), "Cannot add null to an existing GalleryControlSettingsCollection. Items.Count = " + _items.Count);

      _items.TryAdd(item.ControlId.ToLowerInvariant(), item);
    }

    /// <summary>
    /// Find the gallery control settings in the collection that matches the specified <paramref name="controlId" /> (case insensitive).
    /// If no matching object is found, null is returned.
    /// </summary>
    /// <param name="controlId">The ID that uniquely identifies the control containing the gallery.</param>
    /// <returns>Returns an <see cref="IGalleryControlSettings" />object from the collection that matches the specified <paramref name="controlId" />,
    /// or null if no matching object is found.</returns>
    public IGalleryControlSettings FindByControlId(string controlId)
    {
      IGalleryControlSettings galleryControlSettings;

      _items.TryGetValue(controlId.ToLowerInvariant(), out galleryControlSettings);

      return galleryControlSettings;
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
    /// <returns><see cref="System.Collections.Generic.IEnumerator&lt;IGalleryControlSettings&gt;" />.</returns>
    public IEnumerator<IGalleryControlSettings> GetEnumerator()
    {
      return _items.Values.GetEnumerator();
    }
  }
}
