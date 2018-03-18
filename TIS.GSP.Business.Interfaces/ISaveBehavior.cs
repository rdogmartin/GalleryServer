using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Provides functionality for persisting an object to the data store and file system.
	/// </summary>
	public interface ISaveBehavior
	{
		/// <summary>
		/// Persist the object to which this behavior belongs to the data store. Also persist to the file system, if 
		/// the object has a representation on disk, such as albums (stored as directories) and media objects (stored
		/// as files). New objects with ID = int.MinValue will have a new <see cref="IGalleryObject.Id" /> assigned 
		/// and <see cref="IGalleryObject.IsNew" /> set to false.
		/// All validation should have taken place before calling this method.
		/// </summary>
		void Save();
	}
}
