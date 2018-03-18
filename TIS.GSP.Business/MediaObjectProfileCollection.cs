using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// A collection of <see cref="IMediaObjectProfile" /> objects.
	/// </summary>
	public class MediaObjectProfileCollection : KeyedCollection<int, IMediaObjectProfile>, IMediaObjectProfileCollection
	{
		/// <summary>
		/// Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IMediaObjectProfile item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing MediaObjectProfileCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		/// <summary>
		/// Adds the <paramref name="items" /> to the current collection.
		/// </summary>
		/// <param name="items">The items to add to the current collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
		public void AddRange(IEnumerable<IMediaObjectProfile> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			foreach (var item in items)
			{
                if (item != null)
                {
                    Add(item);
                }
            }
		}

		/// <summary>
		/// Find the media object profile in the collection that matches the specified <paramref name="mediaObjectId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID for the media object to find.</param>
		/// <returns>Returns an <see cref="IMediaObjectProfile" />object from the collection that matches the specified <paramref name="mediaObjectId" />,
		/// or null if no matching object is found.</returns>
		public IMediaObjectProfile Find(int mediaObjectId)
		{
			return (Contains(mediaObjectId) ? base[mediaObjectId] : null);
		}

		/// <summary>
		/// Generates as string representation of the items in the collection.
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		public string Serialize()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(Items);
		}

		/// <summary>
		/// Perform a deep copy of this collection.
		/// </summary>
		/// <returns>Returns a deep copy of this collection.</returns>
		public IMediaObjectProfileCollection Copy()
		{
			IMediaObjectProfileCollection itemCollectionCopy = new MediaObjectProfileCollection();

			foreach (IMediaObjectProfile item in Items)
			{
				itemCollectionCopy.Add(item.Copy());
			}

			return itemCollectionCopy;
		}

		/// <summary>
		/// Gets the key for item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>System.Int32.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		protected override int GetKeyForItem(IMediaObjectProfile item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return item.MediaObjectId;
		}
	}
}
