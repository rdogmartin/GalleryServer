using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IMediaObjectProfile" /> objects.
	/// </summary>
	public interface IMediaObjectProfileCollection : System.Collections.Generic.ICollection<IMediaObjectProfile>
	{
		/// <summary>
		/// Adds the specified <paramref name="item" />.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		new void Add(IMediaObjectProfile item);

		/// <summary>
		/// Adds the <paramref name="items" /> to the current collection.
		/// </summary>
		/// <param name="items">The items to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<IMediaObjectProfile> items);

		/// <summary>
		/// Find the media object profile in the collection that matches the specified <paramref name="mediaObjectId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID for the media object to find.</param>
		/// <returns>Returns an <see cref="IMediaObjectProfile" />object from the collection that matches the specified <paramref name="mediaObjectId" />,
		/// or null if no matching object is found.</returns>
		IMediaObjectProfile Find(int mediaObjectId);

		/// <summary>
		/// Generates as string representation of the items in the collection.
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		string Serialize();

		/// <summary>
		/// Perform a deep copy of this collection.
		/// </summary>
		/// <returns>Returns a deep copy of this collection.</returns>
		IMediaObjectProfileCollection Copy();
	}
}
