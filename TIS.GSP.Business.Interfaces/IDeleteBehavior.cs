using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Provides functionality for deleting an object from the data store.
	/// </summary>
	public interface IDeleteBehavior
	{
		/// <summary>
		/// Delete the object to which this behavior belongs from the data store and optionally the file system.
		/// </summary>
		/// <param name="deleteFromFileSystem">Indicates whether to delete the file or directory from the hard drive in addition
		/// to deleting it from the data store. When true, the object is deleted from both the data store and hard drive. When
		/// false, only the record in the data store is deleted.</param>
		void Delete(bool deleteFromFileSystem);
	}
}
