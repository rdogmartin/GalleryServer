using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// A collection of <see cref="IGallery" /> objects.
	/// </summary>
	public class GalleryCollection : IGalleryCollection
	{
    private readonly ConcurrentDictionary<int, IGallery> _galleries = new ConcurrentDictionary<int, IGallery>();

    /// <summary>
    /// Gets the number of galleries in the collection.
    /// </summary>
    public int Count => _galleries.Count;

    /// <summary>
    /// Adds the specified <paramref name="gallery" /> to the current collection.
    /// </summary>
    /// <param name="gallery">The gallery to add.</param>
    public void Add(IGallery gallery)
    {
      if (gallery == null)
        throw new ArgumentNullException(nameof(gallery), "Cannot add null to an existing GalleryCollection.");

      _galleries.TryAdd(gallery.GalleryId, gallery);
    }

    /// <summary>
    /// Removes all galleries from the current collection.
    /// </summary>
    public void Clear()
	  {
	    _galleries.Clear();
	  }

    /// <summary>
    /// Find the gallery in the collection that matches the specified <paramref name="galleryId" />. If no matching object is found,
    /// null is returned.
    /// </summary>
    /// <param name="galleryId">The ID that uniquely identifies the gallery.</param>
    /// <returns>Returns an <see cref="IGallery" />object from the collection that matches the specified <paramref name="galleryId" />,
    /// or null if no matching object is found.</returns>
    public IGallery FindById(int galleryId)
		{
		  IGallery gallery;

		  _galleries.TryGetValue(galleryId, out gallery);

		  return gallery;
		}

    /// <summary>
    /// Determines whether the <paramref name="gallery"/> is already a member of the collection. An object is considered a member
    /// of the collection if they both have the same <see cref="IGallery.GalleryId" />.
    /// </summary>
    /// <param name="gallery">An <see cref="IGallery"/> to determine whether it is a member of the current collection.</param>
    /// <returns>Returns <c>true</c> if <paramref name="gallery"/> is a member of the current collection;
    /// otherwise returns <c>false</c>.</returns>
    public bool Contains(IGallery gallery)
    {
      if (gallery == null)
        return false;

      return _galleries.ContainsKey(gallery.GalleryId);
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
    /// <returns><see cref="System.Collections.Generic.IEnumerator&lt;IGallery&gt;" />.</returns>
    public IEnumerator<IGallery> GetEnumerator()
    {
      return _galleries.Values.GetEnumerator();
    }
  }
}
