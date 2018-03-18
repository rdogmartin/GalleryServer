using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IAlbumProfile" /> objects.
	/// </summary>
	public interface IAlbumProfileCollection : System.Collections.Generic.ICollection<IAlbumProfile>
	{
		/// <summary>
		/// Adds the specified <paramref name="item" />.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		new void Add(IAlbumProfile item);

		/// <summary>
		/// Adds the <paramref name="items" /> to the current collection.
		/// </summary>
		/// <param name="items">The items to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<IAlbumProfile> items);

		/// <summary>
		/// Find the album profile in the collection that matches the specified <paramref name="albumId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="albumId">The ID for the album to find.</param>
		/// <returns>Returns an <see cref="IAlbumProfile" />object from the collection that matches the specified <paramref name="albumId" />,
		/// or null if no matching object is found.</returns>
		IAlbumProfile Find(int albumId);

		/// <summary>
		/// Generates as string representation of the items in the collection.
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		string Serialize();

		/// <summary>
		/// Perform a deep copy of this collection.
		/// </summary>
		/// <returns>Returns a deep copy of this collection.</returns>
		IAlbumProfileCollection Copy();
	}
}
