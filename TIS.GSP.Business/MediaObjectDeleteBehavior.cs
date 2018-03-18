using System;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;

namespace GalleryServer.Business
{
	/// <summary>
	/// Provides functionality for deleting a media object from the data store.
	/// </summary>
	public class MediaObjectDeleteBehavior : IDeleteBehavior
	{
		IGalleryObject _galleryObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectDeleteBehavior"/> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public MediaObjectDeleteBehavior(IGalleryObject galleryObject)
		{
			this._galleryObject = galleryObject;
		}

		/// <summary>
		/// Delete the object to which this behavior belongs from the data store and optionally the file system.
		/// </summary>
		/// <param name="deleteFromFileSystem">Indicates whether to delete the original file from the hard drive in addition
		/// to deleting it from the data store. When true, the object is deleted from both the data store and hard drive. When
		/// false, only the record in the data store and the thumbnail and optimized images are deleted; the original file
		/// is untouched.</param>
		public void Delete(bool deleteFromFileSystem)
		{
			DeleteFromFileSystem(this._galleryObject, deleteFromFileSystem);

			//Factory.GetDataProvider().MediaObject_Delete(this._galleryObject);
		  using (var repo = new MediaObjectRepository())
		  {
        repo.Delete(this._galleryObject);
		  }
		}

		private static void DeleteFromFileSystem(IGalleryObject galleryObject, bool deleteAllFromFileSystem)
		{
			// Delete thumbnail file.
			if (System.IO.File.Exists(galleryObject.Thumbnail.FileNamePhysicalPath))
			{
				System.IO.File.Delete(galleryObject.Thumbnail.FileNamePhysicalPath);
			}

			// Delete optimized file.
			if (!galleryObject.Optimized.FileName.Equals(galleryObject.Original.FileName))
			{
				if (System.IO.File.Exists(galleryObject.Optimized.FileNamePhysicalPath))
				{
					System.IO.File.Delete(galleryObject.Optimized.FileNamePhysicalPath);
				}
			}

			// Delete original file.
			if (deleteAllFromFileSystem && System.IO.File.Exists(galleryObject.Original.FileNamePhysicalPath))
			{
				System.IO.File.Delete(galleryObject.Original.FileNamePhysicalPath);
			}
		}
	}
}
