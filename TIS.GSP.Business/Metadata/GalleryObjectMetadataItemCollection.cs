using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// A collection of <see cref="IGalleryObjectMetadataItem" /> objects.
	/// </summary>
	[Serializable]
	class GalleryObjectMetadataItemCollection : Collection<IGalleryObjectMetadataItem>, IGalleryObjectMetadataItemCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectMetadataItemCollection"/> class.
		/// </summary>
		public GalleryObjectMetadataItemCollection()
			: base(new System.Collections.Generic.List<IGalleryObjectMetadataItem>())
		{
		}

		/// <summary>
		/// Determines whether the <paramref name="item"/> is a member of the collection. An object is considered a member
		/// of the collection if the value of its <see cref="IGalleryObjectMetadataItem.MetadataItemName"/> property matches one in the existing collection.
		/// </summary>
		/// <param name="item">The <see cref="IGalleryObjectMetadataItem"/> to search for.</param>
		/// <returns>
		/// Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.
		/// </returns>
		/// <overloads>
		/// Determines whether the collection contains a particular item.
		/// </overloads>
		public new bool Contains(IGalleryObjectMetadataItem item)
		{
			if (item == null)
				return false;

			foreach (IGalleryObjectMetadataItem metadataItemIterator in (System.Collections.Generic.List<IGalleryObjectMetadataItem>)Items)
			{
				if (item.MetadataItemName == metadataItemIterator.MetadataItemName)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether the <paramref name="metadataItemName"/> is a member of the collection.
		/// </summary>
		/// <param name="metadataItemName">The <see cref="MetadataItemName"/> to search for.</param>
		/// <returns>Returns <c>true</c> if <paramref name="metadataItemName"/> is in the current collection;
		/// otherwise returns <c>false</c>.
		/// </returns>
		public bool Contains(MetadataItemName metadataItemName)
		{
			IGalleryObjectMetadataItem metadataItem;
			return TryGetMetadataItem(metadataItemName, out metadataItem);
		}

		/// <summary>
		/// Adds an object to the end of the <see cref="T:System.Collections.ObjectModel.Collection`1" />.
		/// </summary>
		/// <param name="item">The object to be added to the end of the <see cref="T:System.Collections.ObjectModel.Collection`1" />. The value can be null for reference types.</param>
		public new void Add(IGalleryObjectMetadataItem item)
		{
			item.GalleryObject.HasChanges = true;

			base.Add(item);
		}

		/// <summary>
		/// Adds the metadata items to the current collection.
		/// </summary>
		/// <param name="galleryObjectMetadataItems">The metadata items to add to the collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjectMetadataItems" /> is null.</exception>
		public void AddRange(IGalleryObjectMetadataItemCollection galleryObjectMetadataItems)
		{
			if (galleryObjectMetadataItems == null)
				throw new ArgumentNullException("galleryObjectMetadataItems");

			foreach (IGalleryObjectMetadataItem metadataItem in galleryObjectMetadataItems)
			{
				Items.Add(metadataItem);
			}
		}

		/// <summary>
		/// Apply the <paramref name="metadataDisplayOptions"/> to the items in the collection. This includes sorting the items and updating
		/// the <see cref="IGalleryObjectMetadataItem.IsVisible"/> property.
		/// </summary>
		/// <param name="metadataDisplayOptions">A collection of metadata definition items. Specify <see cref="IGallerySettings.MetadataDisplaySettings"/>
		/// for this parameter.</param>
		public void ApplyDisplayOptions(IMetadataDefinitionCollection metadataDisplayOptions)
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;

			galleryObjectMetadataItems.Sort(new GalleryObjectMetadataItemComparer(metadataDisplayOptions));

			galleryObjectMetadataItems.ForEach(metaItem =>
				{
					IMetadataDefinition metadataDef = metadataDisplayOptions.Find(metaItem.MetadataItemName);

					if (metaItem.GalleryObject.GalleryObjectType == GalleryObjectType.Album)
						metaItem.IsVisible = metadataDef.IsVisibleForAlbum;
					else
						metaItem.IsVisible = metadataDef.IsVisibleForGalleryObject;
				});
		}

		/// <summary>
		/// Gets the <see cref="IGalleryObjectMetadataItem"/> object that matches the specified
		/// <see cref="MetadataItemName"/>. The <paramref name="metadataItem"/>
		/// parameter remains null if no matching object is in the collection.
		/// </summary>
		/// <param name="metadataName">The <see cref="MetadataItemName"/> of the
		/// <see cref="IGalleryObjectMetadataItem"/> to get.</param>
		/// <param name="metadataItem">When this method returns, contains the <see cref="IGalleryObjectMetadataItem"/> associated with the
		/// specified <see cref="MetadataItemName"/>, if the key is found; otherwise, the
		/// parameter remains null. This parameter is passed uninitialized.</param>
		/// <returns>
		/// Returns true if the <see cref="IGalleryObjectMetadataItemCollection"/> contains an element with the specified
		/// <see cref="MetadataItemName"/>; otherwise, false.
		/// </returns>
		public bool TryGetMetadataItem(MetadataItemName metadataName, out IGalleryObjectMetadataItem metadataItem)
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;

			metadataItem = galleryObjectMetadataItems.Find(delegate(IGalleryObjectMetadataItem metaItem)
				{
					return (metaItem.MetadataItemName == metadataName);
				});

			return (metadataItem != null);
		}

		/// <summary>
		/// Get a list of items whose metadata must be persisted to the data store, either because it has been added or because
		/// it has been modified. All IGalleryObjectMetadataItem whose HasChanges property are true are returned. This is called during a
		/// save operation to indicate which metadata items must be saved. Guaranteed to not return null. If no items
		/// are found, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a list of items whose metadata must be updated with the metadata currently in the media object's file.
		/// </returns>
		public IGalleryObjectMetadataItemCollection GetItemsToSave()
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			var galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;
			IGalleryObjectMetadataItemCollection metadataItemsCollection = new GalleryObjectMetadataItemCollection();

			galleryObjectMetadataItems.ForEach(delegate(IGalleryObjectMetadataItem metaItem)
				{
					if (metaItem.HasChanges)
					{
						metadataItemsCollection.Add(metaItem);
					}
				});

			return metadataItemsCollection;
		}

		/// <summary>
		/// Perform a deep copy of this metadata collection.
		/// </summary>
		/// <returns>
		/// Returns a deep copy of this metadata collection.
		/// </returns>
		public IGalleryObjectMetadataItemCollection Copy()
		{
			IGalleryObjectMetadataItemCollection metaDataItemCollectionCopy = new GalleryObjectMetadataItemCollection();

			foreach (IGalleryObjectMetadataItem metaDataItem in this.Items)
			{
				metaDataItemCollectionCopy.Add(metaDataItem.Copy());
			}

			return metaDataItemCollectionCopy;
		}

		/// <summary>
		/// Gets the items in the collection that are visible to the UI. That is, get the items where <see cref="IGalleryObjectMetadataItem.IsVisible" />
		/// = <c>true</c>.
		/// </summary>
		/// <returns>Returns a list of items that are visible to the UI.</returns>
		public IGalleryObjectMetadataItemCollection GetVisibleItems()
		{
			// We know galleryObjectMetadataItems is actually a List<IGalleryObjectMetadataItem> because we passed it to the constructor.
			List<IGalleryObjectMetadataItem> galleryObjectMetadataItems = (List<IGalleryObjectMetadataItem>)Items;
			IGalleryObjectMetadataItemCollection metadataItemsCollection = new GalleryObjectMetadataItemCollection();

			galleryObjectMetadataItems.ForEach(delegate(IGalleryObjectMetadataItem metaItem)
				{
					if (metaItem.IsVisible)
					{
						metadataItemsCollection.Add(metaItem);
					}
				});

			return metadataItemsCollection;
		}

		/// <summary>
		/// Converts the <paramref name="metaDtos" /> to an instance of <see cref="IGalleryObjectMetadataItemCollection" /> and
		/// returns it. An empty collection is returned if <paramref name="metaDtos" /> is null or empty. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryObject">The gallery object the <paramref name="metaDtos" /> belong to.</param>
		/// <param name="metaDtos">An enumerable collection of <see cref="Data.MetadataDto" /> instances.</param>
		/// <returns>An instance of <see cref="IGalleryObjectMetadataItemCollection" />.</returns>
		public static IGalleryObjectMetadataItemCollection FromMetaDtos(IGalleryObject galleryObject, IEnumerable<Data.MetadataDto> metaDtos)
		{
			var metaDefs = Factory.LoadGallerySetting(galleryObject.GalleryId).MetadataDisplaySettings;

			var metadata = Factory.CreateMetadataCollection();

			if (metaDtos != null)
			{
				foreach (var mDto in metaDtos)
				{
					metadata.Add(Factory.CreateMetadataItem(
						mDto.MetadataId, 
						galleryObject,
						mDto.RawValue,
						mDto.Value.Trim(),
						false,
						metaDefs.Find(mDto.MetaName)));
				}
			}

			return metadata;
		}

		/// <summary>
		/// Converts the <paramref name="metaItems" /> to an instance of <see cref="IGalleryObjectMetadataItemCollection" /> and
		/// returns it. An empty collection is returned if <paramref name="metaItems" /> is null or empty. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryObject">The gallery object the <paramref name="metaItems" /> belong to.</param>
		/// <param name="metaItems">An enumerable collection of <see cref="Data.MetadataDto" /> instances.</param>
		/// <returns>An instance of <see cref="IGalleryObjectMetadataItemCollection" />.</returns>
		public static IGalleryObjectMetadataItemCollection FromCacheItemMetas(IGalleryObject galleryObject, IEnumerable<CacheItemMetaItem> metaItems)
		{
			var metaDefs = Factory.LoadGallerySetting(galleryObject.GalleryId).MetadataDisplaySettings;

			var metadata = Factory.CreateMetadataCollection();

			if (metaItems != null)
			{
				foreach (var mDto in metaItems)
				{
					metadata.Add(Factory.CreateMetadataItem(
						mDto.MetadataId, 
						galleryObject,
						mDto.RawValue,
						mDto.Value.Trim(),
						false,
						metaDefs.Find(mDto.MetaName)));
				}
			}

			return metadata;
		}
	}

	/// <summary>
	/// Defines a method for comparing two instances of <see cref="IGalleryObjectMetadataItem" /> objects. The items are compared using
	/// the <see cref="IMetadataDefinition.Sequence" /> property of the <see cref="IMetadataDefinitionCollection" /> passed to the
	/// constructor.
	/// </summary>
	/// <remarks>Instances of <see cref="IMetadataDefinitionCollection" /> are sorted according to the sequence defined in the 
	/// gallery setting <see cref="IGallerySettings.MetadataDisplaySettings" />. That is, this class looks up the corresponding
	/// metadata item in this property and uses its <see cref="IMetadataDefinition.Sequence" /> property for the comparison.</remarks>
	public class GalleryObjectMetadataItemComparer : IComparer<IGalleryObjectMetadataItem>
	{
		private readonly IMetadataDefinitionCollection _metadataDisplayOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectMetadataItemComparer"/> class. The items are compared using
		/// the <see cref="IMetadataDefinition.Sequence" /> property of the <paramref name="metadataDisplayOptions" /> parameter.
		/// </summary>
		/// <param name="metadataDisplayOptions">The metadata display options.</param>
		public GalleryObjectMetadataItemComparer(IMetadataDefinitionCollection metadataDisplayOptions)
		{
			_metadataDisplayOptions = metadataDisplayOptions;
		}

		/// <summary>
		/// Compares the two instances and returns a value indicating their sort relation to each other.
		/// -1: obj1 is less than obj2
		/// 0: obj1 is equal to obj2
		/// 1: obj1 is greater than obj2
		/// </summary>
		/// <param name="x">One of the instances to compare.</param>
		/// <param name="y">One of the instances to compare.</param>
		/// <returns>Returns in integer indicating the objects' sort relation to each other.</returns>
		public int Compare(IGalleryObjectMetadataItem x, IGalleryObjectMetadataItem y)
		{
			if (x == null)
			{
				// If obj1 is null and obj2 is null, they're equal.
				// If obj1 is null and obj2 is not null, obj2 is greater.
				return (y == null ? 0 : -1);
			}
			else
			{
				if (y == null)
				{
					return 1; // obj1 is not null and obj2 is null, so obj1 is greater.
				}

				// Neither is null. Look up the display settings for each item and sort by its associated sequence property.
				IMetadataDefinition obj1MetadataDefinition = _metadataDisplayOptions.Find(x.MetadataItemName);
				IMetadataDefinition obj2MetadataDefinition = _metadataDisplayOptions.Find(y.MetadataItemName);

				if ((obj1MetadataDefinition != null) && (obj2MetadataDefinition != null))
				{
					return obj1MetadataDefinition.Sequence.CompareTo(obj2MetadataDefinition.Sequence);
				}
				else
				{
					// Can't find one of the display settings. This should never occur because the IMetadataDefinitionCollection should 
					// have an entry for every value of the MetadataItemName enumeration.
					throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "The IMetadataDefinitionCollection instance passed to the GalleryObjectMetadataItemComparer constructor did not have an item corresponding to one of these MetadataItemName enum values: {0}, {1}. This collection should contain an item for every enum value.", x.MetadataItemName, y.MetadataItemName));
				}
			}
		}
	}
}
