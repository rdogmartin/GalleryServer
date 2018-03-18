using System;
using System.Collections.Generic;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// An unsorted collection of <see cref="IGalleryObject" /> objects.
	/// </summary>
	public interface IGalleryObjectCollection : IEnumerable<IGalleryObject>
	{
		/// <summary>
		/// Gets the number of gallery objects in the collection.
		/// </summary>
		/// <value>The count.</value>
		int Count { get; }

		/// <summary>
		/// Adds the specified gallery object.
		/// </summary>
		/// <param name="item">The gallery object.</param>
		void Add(IGalleryObject item);

		/// <summary>
		/// Adds the galleryObjects to the current collection.
		/// </summary>
		/// <param name="galleryObjects">The gallery objects to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjects" /> is null.</exception>
		void AddRange(IEnumerable<IGalleryObject> galleryObjects);

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if one of the following scenarios is true: (1) They are both of the same type, each ID is 
		/// greater than int.MinValue, and the IDs are equal to each other, or (2) They are new objects that haven't yet
		/// been saved to the data store, the physical path to the original file has been specified, and the paths
		/// are equal to each other.
		/// </summary>
		/// <param name="item">An <see cref="IGalleryObject"/> to determine whether it is a member of the current collection.</param>
		/// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		bool Contains(IGalleryObject item);

		/// <summary>
		/// Removes the specified gallery object.
		/// </summary>
		/// <param name="item">The gallery object.</param>
		void Remove(IGalleryObject item);

		/// <summary>
		/// Creates a collection sorted on the <see cref="IGalleryObject.Sequence" /> property.
		/// </summary>
		/// <returns>An instance of IList{IGalleryObject}.</returns>
		IList<IGalleryObject> ToSortedList();

		/// <summary>
		/// Sorts the gallery objects in this collection by <paramref name="sortByMetaName" /> in the order specified by
		/// <paramref name="sortAscending" />. The <paramref name="galleryId" /> is used to look up the applicable
		/// <see cref="IGallerySettings.MetadataDisplaySettings" />.
		/// </summary>
		/// <param name="sortByMetaName">The name of the metadata item to sort on.</param>
		/// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>An instance of IList{IGalleryObject}.</returns>
		IList<IGalleryObject> ToSortedList(MetadataItemName sortByMetaName, bool sortAscending, int galleryId);
	}
}
