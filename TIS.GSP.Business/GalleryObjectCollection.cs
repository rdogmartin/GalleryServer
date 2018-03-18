using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// An unsorted collection of <see cref="IGalleryObject" /> objects.
	/// </summary>
	public class GalleryObjectCollection : IGalleryObjectCollection
	{
		#region Properties

		/// <summary>
		/// Gets or sets the gallery objects in this collection. We prefer a dictionary over <see cref="ConcurrentBag&lt;IGalleryObject&gt;" />
		/// primarily because the dictionary enforces unique keys, while the bag might allows duplicates.
		/// </summary>
		/// <value>The items.</value>
		private ConcurrentDictionary<string, IGalleryObject> Items { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectCollection"/> class.
		/// </summary>
		public GalleryObjectCollection()
		{
			Items = new ConcurrentDictionary<string, IGalleryObject>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectCollection" /> class with the specified <paramref name="items" />.
		/// </summary>
		/// <param name="items">The items to add to the collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
		public GalleryObjectCollection(IEnumerable<IGalleryObject> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			Items = new ConcurrentDictionary<string, IGalleryObject>(items.ToDictionary(GetKey));
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the number of gallery objects in the collection.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get
			{
				return Items.Count;
			}
		}

		/// <summary>
		/// Adds the specified gallery object.
		/// </summary>
		/// <param name="item">The gallery object.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public void Add(IGalleryObject item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing GalleryObjectCollection. Items.Count = " + Items.Count);

			Items.TryAdd(GetKey(item), item);
		}

		/// <summary>
		/// Adds the galleryObjects to the current collection.
		/// </summary>
		/// <param name="galleryObjects">The gallery objects to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjects" /> is null.</exception>
		public void AddRange(IEnumerable<IGalleryObject> galleryObjects)
		{
			if (galleryObjects == null)
				throw new ArgumentNullException("galleryObjects");

			foreach (var galleryObject in galleryObjects)
			{
				Items.TryAdd(GetKey(galleryObject), galleryObject);
			}
		}

    /// <summary>
    /// Removes the specified gallery object.
    /// </summary>
    /// <param name="item">The gallery object.</param>
    public void Remove(IGalleryObject item)
		{
			IGalleryObject removedItem;
			Items.TryRemove(GetKey(item), out removedItem);
		}

		/// <summary>
		/// Creates a collection sorted on the <see cref="IGalleryObject.Sequence" /> property.
		/// </summary>
		/// <returns>An instance of IList{IGalleryObject}.</returns>
		public IList<IGalleryObject> ToSortedList()
		{
			var items = new List<IGalleryObject>(Items.Values);
			items.Sort();

			return items;
		}

		/// <summary>
		/// Sorts the gallery objects in this collection by <paramref name="sortByMetaName" /> in the order specified by
		/// <paramref name="sortAscending" />. The <paramref name="galleryId" /> is used to look up the applicable
		/// <see cref="IGallerySettings.MetadataDisplaySettings" />.
		/// </summary>
		/// <param name="sortByMetaName">The name of the metadata item to sort on.</param>
		/// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>An instance of IList{IGalleryObject}.</returns>
		public IList<IGalleryObject> ToSortedList(MetadataItemName sortByMetaName, bool sortAscending, int galleryId)
		{
			if (sortByMetaName == MetadataItemName.NotSpecified)
			{
				// This is a custom sort, so sort based on the Sequence property.
				return ToSortedList();
			}

			return SortByMetaValue(sortByMetaName, sortAscending, Factory.LoadGallerySetting(galleryId).MetadataDisplaySettings.Find(sortByMetaName).DataType);
		}

		/// <summary>
		/// Sorts the gallery objects in the collection by the property specified in <paramref name="sortByMetaName" />.
		/// </summary>
		/// <param name="sortByMetaName">The name of the metadata item to sort on.</param>
		/// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
		/// <param name="metaDataType">The data type of the meta data. When <see cref="DateTime" />, the results are sorted on the 
		/// <see cref="IGalleryObjectMetadataItem.RawValue" /> property, which are expected to be in a string-sortable format. For all other data types
		/// the results are sorted on the <see cref="IGalleryObjectMetadataItem.Value" /> property</param>
		/// <returns>Returns a collection of <see cref="IGalleryObject" /> instances.</returns>
		private IList<IGalleryObject> SortByMetaValue(MetadataItemName sortByMetaName, bool sortAscending, Type metaDataType)
		{
			Func<IGalleryObjectMetadataItem, string> sortProperty = mi => (metaDataType ==typeof(DateTime) ?  mi.RawValue : mi.Value);

			var childAlbums = Items.Values.Where(g => g.GalleryObjectType == GalleryObjectType.Album)
				.OrderBy(g => g.MetadataItems.Where(mi1 => mi1.MetadataItemName == sortByMetaName).Select(sortProperty).Distinct().FirstOrDefault()).Select(a => a);

			var mediaObjects = Items.Values.Where(g => g.GalleryObjectType != GalleryObjectType.Album)
				.OrderBy(g => g.MetadataItems.Where(mi1 => mi1.MetadataItemName == sortByMetaName).Select(sortProperty).Distinct().FirstOrDefault()).Select(mo => mo);

			if (!sortAscending)
			{
				childAlbums = childAlbums.Reverse();
				mediaObjects = mediaObjects.Reverse();
			}

			return childAlbums.Concat(mediaObjects).ToList();
		}

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
		public bool Contains(IGalleryObject item)
		{
			if (item == null)
				return false;

			foreach (IGalleryObject galleryObjectIterator in (Items.Values))
			{
				if (galleryObjectIterator == null)
					throw new BusinessException("Error in GalleryObjectCollection.Contains method: One of the objects in the Items property is null. Items.Count = " + Items.Count);

				bool existingObjectsAndEqual = ((galleryObjectIterator.Id > int.MinValue) && (galleryObjectIterator.Id.Equals(item.Id)) && (galleryObjectIterator.GetType() == item.GetType()));

				bool newObjectsAndFilepathsAreEqual = ((galleryObjectIterator.IsNew) && (item.IsNew)
																							 && (!String.IsNullOrEmpty(galleryObjectIterator.Original.FileNamePhysicalPath))
																							 && (!String.IsNullOrEmpty(item.Original.FileNamePhysicalPath))
																							 && (galleryObjectIterator.Original.FileNamePhysicalPath.Equals(item.Original.FileNamePhysicalPath)));

				if (existingObjectsAndEqual || newObjectsAndFilepathsAreEqual)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
		public IEnumerator<IGalleryObject> GetEnumerator()
		{
			return Items.Values.GetEnumerator();
		}

		#endregion

		#region Functions

		/// <summary>
		/// Gets a string that uniquely identifies the gallery object.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>System.String.</returns>
		private string GetKey(IGalleryObject item)
		{
			return (item.Id > int.MinValue ? String.Concat(item.Id, item.GalleryObjectType) : Guid.NewGuid().ToString());
		}

		#endregion
	}
}
