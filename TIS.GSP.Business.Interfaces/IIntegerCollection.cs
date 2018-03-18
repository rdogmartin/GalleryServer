using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a collection of <see cref="int">integers</see>. This is used in various places in Gallery Server 
	/// instead of List&lt;<see cref="int" />&gt; per Microsoft best practices. Read about rule CA1002 for more information.
	/// </summary>
	public interface IIntegerCollection : System.Collections.Generic.ICollection<int>
	{
		/// <summary>
		/// This event fires after items have been removed from the collection through the Clear() method.
		/// </summary>
		event EventHandler Cleared;

		/// <summary>
		/// Add the list of integers to the collection.
		/// </summary>
		/// <param name="values">A list of integers to add to the collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<int> values);

		/// <summary>
		/// Gets or sets the integer at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the integer within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Gets or sets the integer at the specified index position.</returns>
		int this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Converts the integers in the collection to an array.
		/// </summary>
		/// <returns>Returns an array of integers.</returns>
		int[] ToArray();
	}
}
