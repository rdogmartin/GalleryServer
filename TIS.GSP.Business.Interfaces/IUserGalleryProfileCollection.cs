using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IUserGalleryProfile" /> objects.
	/// </summary>
	public interface IUserGalleryProfileCollection : IEnumerable<IUserGalleryProfile>
	{
		///// <summary>
		///// Gets a reference to the <see cref="IUserGalleryProfile" /> object at the specified index position.
		///// </summary>
		///// <param name="indexPosition">An integer specifying the position of the object within this collection to
		///// return. Zero returns the first item.</param>
		///// <returns>Returns a reference to the <see cref="IUserGalleryProfile" /> object at the specified index position.</returns>
		//IUserGalleryProfile this[Int32 indexPosition]
		//{
		//	get;
		//	set;
		//}

		///// <summary>
		///// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		///// of the collection if they both have the same <see cref="IUserGalleryProfile.GalleryId" />.
		///// </summary>
		///// <param name="item">An <see cref="IUserGalleryProfile"/> to determine whether it is a member of the current collection.</param>
		///// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		///// otherwise returns <c>false</c>.</returns>
		//new bool Contains(IUserGalleryProfile item);

		/// <summary>
		/// Adds the specified user profile.
		/// </summary>
		/// <param name="item">The user profile to add.</param>
		void Add(IUserGalleryProfile item);

		/// <summary>
		/// Adds the gallery profiles to the current collection.
		/// </summary>
		/// <param name="galleryProfiles">The gallery profiles to add to the current collection.</param>
		void AddRange(System.Collections.Generic.IEnumerable<IUserGalleryProfile> galleryProfiles);

		/// <summary>
		/// Find the user account in the collection that matches the specified <paramref name="galleryId" />. Guaranteed to not return null.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery.</param>
		/// <returns>Returns an <see cref="IUserGalleryProfile" />object from the collection that matches the specified <paramref name="galleryId" />.</returns>
		IUserGalleryProfile FindByGalleryId(int galleryId);

		/// <summary>
		/// Creates a new instance of an <see cref="IUserGalleryProfile"/> object. This method can be used by code that only has a
		/// reference to the interface layer and therefore cannot create a new instance of an object on its own.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery.</param>
		/// <returns>
		/// Returns a new instance of an <see cref="IUserGalleryProfile"/> object.
		/// </returns>
		IUserGalleryProfile CreateNewUserGalleryProfile(int galleryId);

		/// <summary>
		/// Creates a new collection containing deep copies of the items it contains.
		/// </summary>
		/// <returns>Returns a new collection containing deep copies of the items it contains.</returns>
		IUserGalleryProfileCollection Copy();
	}
}
