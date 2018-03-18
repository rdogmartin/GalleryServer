using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IEvent" /> objects.
	/// </summary>
	public interface IEventCollection : System.Collections.Generic.ICollection<IEvent>
	{
		/// <summary>
		/// Finds the application event with the specified <paramref name="eventId"/> or null if not found.
		/// </summary>
		/// <param name="eventId">The value that uniquely identifies the application event (<see cref="IEvent.EventId"/>).</param>
		/// <returns>Returns an <see cref="IEvent" /> or null.</returns>
		IEvent FindById(int eventId);
		
		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IEvent.TimestampUtc" /> property,
		/// with the most recent timestamp first.
		/// </summary>
		void Sort();

		/// <summary>
		/// Gets a reference to the <see cref="IEvent" /> object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the <see cref="IEvent" /> object at the specified index position.</returns>
		IEvent this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the collection.  
		/// </summary>
		/// <param name="ev">The application event to locate in the collection. The value can be a null 
		/// reference (Nothing in Visual Basic).</param>
		/// <returns>The zero-based index of the first occurrence of <paramref name="ev" /> within the collection, if found; 
		/// otherwise, –1. </returns>
		Int32 IndexOf(IEvent ev);

		/// <summary>
		/// Adds the application events to the current collection.
		/// </summary>
		/// <param name="events">The application events to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="events" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<IEvent> events);

		/// <summary>
		/// Gets the application events associated with the specified gallery.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns an <see cref="IEventCollection" /> containing events corresponding to the specified parameters.</returns>
		IEventCollection FindAllForGallery(int galleryId);
	}
}
