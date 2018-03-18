using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// A collection of <see cref="IGalleryControlSettings" /> objects. There is a maximum of one item for each instance of a Gallery
  /// control that is used in an application. An item will exist in this collection only if at least one control-specific setting
  /// has been saved for a particular control.
  /// </summary>
  public interface IGalleryControlSettingsCollection : IEnumerable<IGalleryControlSettings>
  {
    /// <summary>
    /// Adds the gallery control settings to the current collection.
    /// </summary>
    /// <param name="galleryControlSettings">The gallery control settings to add to the current collection.</param>
    void AddRange(IEnumerable<IGalleryControlSettings> galleryControlSettings);

    /// <summary>
    /// Adds the specified gallery control settings.
    /// </summary>
    /// <param name="item">The gallery control settings to add.</param>
    void Add(IGalleryControlSettings item);

    /// <summary>
    /// Find the gallery control settings in the collection that matches the specified <paramref name="controlId" /> (case insensitive).
    /// If no matching object is found, null is returned.
    /// </summary>
    /// <param name="controlId">The ID that uniquely identifies the control containing the gallery.</param>
    /// <returns>Returns an <see cref="IGalleryControlSettings" />object from the collection that matches the specified <paramref name="controlId" />,
    /// or null if no matching object is found.</returns>
    IGalleryControlSettings FindByControlId(string controlId);

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
