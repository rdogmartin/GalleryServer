using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IGallery" /> objects.
	/// </summary>
	public interface IGalleryCollection : IEnumerable<IGallery>
  {
    /// <summary>
    /// Gets the number of galleries in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Adds the specified <paramref name="gallery" /> to the current collection.
    /// </summary>
    /// <param name="gallery">The gallery to add.</param>
    void Add(IGallery gallery);

    /// <summary>
    /// Removes all galleries from the current collection.
    /// </summary>
    void Clear();

    /// <summary>
    /// Find the gallery in the collection that matches the specified <paramref name="galleryId" />. If no matching object is found,
    /// null is returned.
    /// </summary>
    /// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
    /// <returns>Returns an <see cref="IGallery" />object from the collection that matches the specified <paramref name="galleryId" />,
    /// or null if no matching object is found.</returns>
    IGallery FindById(int galleryId);

    /// <summary>
    /// Determines whether the <paramref name="gallery"/> is a member of the collection. An object is considered a member
    /// of the collection if they both have the same <see cref="IGallery.GalleryId" />.
    /// </summary>
    /// <param name="gallery">An <see cref="IGallery"/> to determine whether it is a member of the current collection.</param>
    /// <returns>Returns <c>true</c> if <paramref name="gallery"/> is a member of the current collection;
    /// otherwise returns <c>false</c>.</returns>
    bool Contains(IGallery gallery);
  }
}
