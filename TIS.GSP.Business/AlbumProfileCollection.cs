using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// A collection of <see cref="IAlbumProfile" /> objects.
	/// </summary>
	public class AlbumProfileCollection : KeyedCollection<int, IAlbumProfile>, IAlbumProfileCollection
	{
		/// <summary>
		/// Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IAlbumProfile item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing AlbumSortDefinitionCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		/// <summary>
		/// Adds the <paramref name="items" /> to the current collection.
		/// </summary>
		/// <param name="items">The items to add to the current collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
		public void AddRange(IEnumerable<IAlbumProfile> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		/// <summary>
		/// Find the album profile in the collection that matches the specified <paramref name="albumId" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="albumId">The ID for the album to find.</param>
		/// <returns>Returns an <see cref="IAlbumProfile" />object from the collection that matches the specified <paramref name="albumId" />,
		/// or null if no matching object is found.</returns>
		public IAlbumProfile Find(int albumId)
		{
			return (base.Contains(albumId) ? base[albumId] : null);
		}

		/// <summary>
		/// Generates as string representation of the items in the collection.
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		public string Serialize()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(base.Items);
		}

		/// <summary>
		/// Perform a deep copy of this collection.
		/// </summary>
		/// <returns>Returns a deep copy of this collection.</returns>
		public IAlbumProfileCollection Copy()
		{
			IAlbumProfileCollection itemCollectionCopy = new AlbumProfileCollection();

			foreach (IAlbumProfile item in this.Items)
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
		protected override int GetKeyForItem(IAlbumProfile item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return item.AlbumId;
		}
	}
}
