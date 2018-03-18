using System;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;

namespace GalleryServer.Business
{
	/// <summary>
	/// Provides functionality for deleting an album from the data store.
	/// </summary>
	public class AlbumDeleteBehavior : IDeleteBehavior
	{
		IAlbum _albumObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="AlbumDeleteBehavior"/> class.
		/// </summary>
		/// <param name="albumObject">The album object.</param>
		public AlbumDeleteBehavior(IAlbum albumObject)
		{
			this._albumObject = albumObject;
		}

		/// <summary>
		/// Delete the object to which this behavior belongs from the data store and optionally the file system.
		/// </summary>
		/// <param name="deleteFromFileSystem">Indicates whether to delete the file or directory from the hard drive in addition
		/// to deleting it from the data store. When true, the object is deleted from both the data store and hard drive. When
		/// false, only the record in the data store is deleted.</param>
		public void Delete(bool deleteFromFileSystem)
		{
			if (deleteFromFileSystem)
			{
				DeleteFromFileSystem(this._albumObject);
			}
			else
			{
				DeleteSupportFilesOnly(this._albumObject);
			}

			if (this._albumObject.IsRootAlbum)
			{
				// Don't delete the root album; just its contents.
				DeleteAlbumContents(deleteFromFileSystem);
			}
			else
			{
			  using (var repo = new AlbumRepository())
			  {
			    repo.Delete(this._albumObject);
          repo.Save();
			  }
			}
		}

		/// <summary>
		/// Deletes the albums and media objects in the album, but not the album itself.
		/// </summary>
		/// <param name="deleteFromFileSystem">Indicates whether to delete the file or directory from the hard drive in addition
		/// to deleting it from the data store. When true, the object is deleted from both the data store and hard drive. When
		/// false, only the record in the data store is deleted.</param>
		private void DeleteAlbumContents(bool deleteFromFileSystem)
		{
			List<IGalleryObject> itemsToDelete = new List<IGalleryObject>();

			// Step 1: Get a list of all items in this album.
			foreach (IGalleryObject childItem in this._albumObject.GetChildGalleryObjects())
			{
				itemsToDelete.Add(childItem);
			}

			// Now iterate through each one, deleting it as we go.
			foreach (IGalleryObject galleryObject in itemsToDelete)
			{
				if (deleteFromFileSystem)
				{
					galleryObject.Delete();
				}
				else
				{
					galleryObject.DeleteFromGallery();
				}
			}
		}

		/// <summary>
		/// Deletes the thumbnail and optimized images associated with this album and all its children, but do not delete the 
		/// album's directory or the any other files it contains.
		/// </summary>
		/// <param name="album">The album.</param>
		private static void DeleteSupportFilesOnly(IAlbum album)
		{
			foreach (IGalleryObject childGalleryObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject))
			{
				DeleteThumbnailAndOptimizedImagesFromFileSystem(childGalleryObject);
			}

			foreach (IAlbum childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
			{
				DeleteSupportFilesOnly(childAlbum);
			}
		}

		private static void DeleteThumbnailAndOptimizedImagesFromFileSystem(IGalleryObject galleryObject)
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
		}

		private static void DeleteFromFileSystem(IAlbum album)
		{
			string albumPath = album.FullPhysicalPath;
			if (album.IsRootAlbum)
			{
				DeleteRootAlbumDirectory(albumPath, album.GalleryId);
			}
			else
			{
				DeleteAlbumDirectory(albumPath, album.GalleryId);
			}
		}

		private static void DeleteAlbumDirectory(string albumPath, int galleryId)
		{
			// Delete the directory (recursive).
			if (System.IO.Directory.Exists(albumPath))
			{
				System.IO.Directory.Delete(albumPath, true);
			}

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

			// Delete files and folders from thumbnail cache, if needed.
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			if (thumbnailPath != albumPath)
			{
				if (System.IO.Directory.Exists(thumbnailPath))
				{
					System.IO.Directory.Delete(thumbnailPath, true);
				}
			}

			// Delete files and folders from optimized image cache, if needed.
			string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
			if (optimizedPath != thumbnailPath && optimizedPath != albumPath)
			{
				if (System.IO.Directory.Exists(optimizedPath))
				{
					System.IO.Directory.Delete(optimizedPath, true);
				}
			}
		}

		private static void DeleteRootAlbumDirectory(string albumPath, int galleryId)
		{
			// User is trying to delete the root album. We only want to delete any subdirectories and files,
			// but not the folder itself.
			DeleteChildFilesAndDirectories(albumPath);

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

			// Delete files and folders from thumbnail cache, if needed.
			string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			if (thumbnailPath != albumPath)
			{
				DeleteChildFilesAndDirectories(thumbnailPath);
			}

			// Delete files and folders from optimized image cache, if needed.
			string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
			if (optimizedPath != albumPath)
			{
				DeleteChildFilesAndDirectories(optimizedPath);
			}
		}

		private static void DeleteChildFilesAndDirectories(string albumPath)
		{
			string[] aFiles = System.IO.Directory.GetFiles(albumPath);
			string[] aFolders = System.IO.Directory.GetDirectories(albumPath);

			// Delete each directory
			for (int i = 0; i <= aFolders.GetUpperBound(0); i++)
			{
				System.Diagnostics.Debug.Assert(System.IO.Directory.Exists(aFolders[i]));
				System.IO.Directory.Delete(aFolders[i], true);
			}

			// Delete each file.
			for (int i = 0; i <= aFiles.GetUpperBound(0); i++)
			{
				System.Diagnostics.Debug.Assert(System.IO.File.Exists(aFiles[i]));
				System.IO.File.Delete(aFiles[i]);
			}
		}

	}
}
