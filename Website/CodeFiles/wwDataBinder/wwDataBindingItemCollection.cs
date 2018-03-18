using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using GalleryServer.WebControls.Tools;


namespace GalleryServer.WebControls
{

	/// <summary>
	/// Collection of individual DataBindingItems. Implemented explicitly as
	/// a CollectionBase class rather than using List#wwDataBindingItems#
	/// so that Add can be overridden
	/// </summary>
	public class wwDataBindingItemCollection : CollectionBase
	{
		/// <summary>
		/// Internal reference to the wwDataBinder object
		/// that is passed to the individual items if available
		/// </summary>
		wwDataBinder _ParentDataBinder = null;

		/// <summary>
		/// Preferred Constructor - Add a reference to the wwDataBinder object here
		/// so a reference can be passed to the children.
		/// </summary>
		/// <param name="Parent"></param>
		public wwDataBindingItemCollection(wwDataBinder Parent)
		{
			this._ParentDataBinder = Parent;
		}

		/// <summary>
		/// Not the preferred constructor - If possible pass a reference to the
		/// Binder object in the overloaded version.
		/// </summary>
		public wwDataBindingItemCollection()
		{
		}

		/// <summary>
		/// Public indexer for the Items
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public wwDataBindingItem this[int index]
		{
			get
			{
				return this.InnerList[index] as wwDataBindingItem;
			}
			set
			{
				this.InnerList[index] = value;
			}
		}


		/// <summary>
		/// Add a wwDataBindingItem to the collection
		/// </summary>
		/// <param name="Item"></param>
		public void Add(wwDataBindingItem Item)
		{
			if (_ParentDataBinder != null)
			{
				Item.Page = _ParentDataBinder.Page;
				Item.Binder = _ParentDataBinder;

				// *** VS Designer adds new items as soon as their accessed
				// *** but items may not be valid so we have to clean up
				if (this._ParentDataBinder.DesignMode)
				{
					// *** Remove any blank items
					UpdateListInDesignMode();
				}
			}

			this.InnerList.Add(Item);
		}


		/// <summary>
		/// Add a wwDataBindingItem to the collection
		/// </summary>
		/// <param name="index"></param>
		/// <param name="Item"></param>
		public void AddAt(int index, wwDataBindingItem Item)
		{
			if (_ParentDataBinder != null)
			{
				Item.Page = _ParentDataBinder.Page;
				Item.Binder = _ParentDataBinder;

				// *** VS Designer adds new items as soon as their accessed
				// *** but items may not be valid so we have to clean up
				if (this._ParentDataBinder.DesignMode)
				{
					UpdateListInDesignMode();
				}
			}

			InnerList.Insert(index, Item);
		}

		/// <summary>
		/// We have to delete 'empty' items because the designer requires items to be 
		/// added to the collection just for editing. This way we may have one 'extra'
		/// item, but not a whole long list of items.
		/// </summary>
		private void UpdateListInDesignMode()
		{
			if (this._ParentDataBinder == null)
				return;

			bool Update = false;

			// *** Remove empty items - so the designer doesn't create excessive empties
			for (int x = 0; x < this.Count; x++)
			{
				if (string.IsNullOrEmpty(this[x].BindingSource) && string.IsNullOrEmpty(this[x].BindingSourceMember))
				{
					this.RemoveAt(x);
					Update = true;
				}
			}

			if (Update)
				this._ParentDataBinder.NotifyDesigner();
		}

	}
}