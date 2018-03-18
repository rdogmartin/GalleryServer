using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Events
{
	/// <summary>
	/// A collection of <see cref="IEvent" /> objects.
	/// </summary>
	public class EventCollection : Collection<IEvent>, IEventCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EventCollection"/> class.
		/// </summary>
		public EventCollection()
			: base(new List<IEvent>())
		{
		}

		/// <summary>
		/// Finds the application event with the specified <paramref name="eventId" /> or null if not found.
		/// </summary>
		/// <param name="eventId">The value that uniquely identifies the application event (<see cref="IEvent.EventId" />).</param>
		/// <returns>Returns an <see cref="IEvent" /> or null.</returns>
		public IEvent FindById(int eventId)
		{
			// We know Items is actually a List<IEvent> because we specified it in the constructor.
			return ((List<IEvent>)Items).Find(ev => (ev.EventId == eventId));
		}

		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IEvent.TimestampUtc"/> property,
		/// with the most recent timestamp first.
		/// </summary>
		public void Sort()
		{
			// We know Items is actually a List<IEvent> because we passed it to the constructor.
			((List<IEvent>)Items).Sort();
		}

		/// <summary>
		/// Adds the application events to the current collection.
		/// </summary>
		/// <param name="events">The application events to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="events" /> is null.</exception>
		public void AddRange(IEnumerable<IEvent> events)
		{
			if (events == null)
				throw new ArgumentNullException("events");

			foreach (var appError in events)
			{
				Add(appError);
			}
		}

		/// <summary>
		/// Gets the application events associated with the specified gallery.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns an <see cref="IEventCollection" /> containing events corresponding to the specified parameters.</returns>
		public IEventCollection FindAllForGallery(int galleryId)
		{
			// We know Items is actually a List<IEvent> because we passed it to the constructor.
			var events = (List<IEvent>)Items;

			IEventCollection matchingEvents = new EventCollection();

			matchingEvents.AddRange(events.FindAll(ev => (ev.GalleryId == galleryId)));

			return matchingEvents;
		}
	}
}
