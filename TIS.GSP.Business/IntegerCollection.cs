using System.Collections.Generic;
using System;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// Represents a collection of <see cref="int">integers</see>. This is used in various places in Gallery Server 
	/// instead of List&lt;<see cref="int" />&gt; per Microsoft best practices. Read about rule CA1002 for more information.
	/// </summary>
	public class IntegerCollection : System.Collections.ObjectModel.Collection<int>, IIntegerCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IntegerCollection"/> class.
		/// </summary>
		public IntegerCollection() : base(new System.Collections.Generic.List<int>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IntegerCollection"/> class.
		/// </summary>
		/// <param name="items">A collection of integers with which to seed the collection.</param>
		public IntegerCollection(IEnumerable<int> items) : base(new System.Collections.Generic.List<int>(items))
		{
		}

		/// <summary>
		/// This event fires after items have been removed from the collection through the Clear() method.
		/// </summary>
		public event System.EventHandler Cleared;

		/// <summary>
		/// Add the list of integers to the collection.
		/// </summary>
		/// <param name="values">A list of integers to add to the collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values" /> is null.</exception>
		public void AddRange(System.Collections.Generic.IEnumerable<int> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");

			foreach (int value in values)
			{
				Items.Add(value);
			}
		}

		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// </summary>
		public new void Clear()
		{
			base.Clear();

			if (Cleared != null)
			{
				Cleared(this, new EventArgs());
			}
		}

		/// <summary>
		/// Converts the integers in the collection to an array.
		/// </summary>
		/// <returns>Returns an array of integers.</returns>
		public int[] ToArray()
		{
			int[] array = new int[base.Count];
			base.CopyTo(array, 0);
			return array;
		}
	}
}
