using System;
using System.IO;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// Provides functionality for persisting a media object to the data store and file system.
	/// </summary>
	public class MediaObjectSaveBehavior : ISaveBehavior
	{
		private readonly IGalleryObject _galleryObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectSaveBehavior"/> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public MediaObjectSaveBehavior(IGalleryObject galleryObject)
		{
			this._galleryObject = galleryObject;
		}

		/// <summary>
		/// Persist the object to which this behavior belongs to the data store. Also persist to the file system, if
		/// the object has a representation on disk, such as albums (stored as directories) and media objects (stored
		/// as files). New objects with ID = int.MinValue will have a new <see cref="IGalleryObject.Id"/> assigned
		/// and <see cref="IGalleryObject.IsNew"/> set to false.
		/// All validation should have taken place before calling this method.
		/// </summary>
		public void Save()
		{
			// If the user requested a rotation, then rotate and save the original. If no rotation is requested,
			// the following line does nothing.
			this._galleryObject.Original.GenerateAndSaveFile();

			// Generate the thumbnail and optimized versions. These must run after the previous statement because when
			// the image is rotated, these methods assume the original has already been rotated.
			try
			{
				this._galleryObject.Thumbnail.GenerateAndSaveFile();
				this._galleryObject.Optimized.GenerateAndSaveFile();

				try
				{
					// Now delete the temp file, but no worries if an error happens. The file is in the temp directory
					// which is cleaned out each time the app starts anyway.
					if (File.Exists(_galleryObject.Original.TempFilePath))
					{
						File.Delete(_galleryObject.Original.TempFilePath);
					}
				}
				catch (IOException ex)
				{
					Events.EventController.RecordError(ex, AppSetting.Instance, this._galleryObject.GalleryId, Factory.LoadGallerySettings());
				}
				catch (NotSupportedException ex)
				{
					Events.EventController.RecordError(ex, AppSetting.Instance, this._galleryObject.GalleryId, Factory.LoadGallerySettings());
				}
				catch (UnauthorizedAccessException ex)
				{
					Events.EventController.RecordError(ex, AppSetting.Instance, this._galleryObject.GalleryId, Factory.LoadGallerySettings());
				}
			}
			catch (UnsupportedImageTypeException)
			{
				// We'll get here when there is a corrupt image or the server's memory is not sufficient to process the image.
				// When this happens, replace the thumbnail creator object with a GenericThumbnailCreator. That one uses a
				// hard-coded thumbnail image rather than trying to generate a thumbnail from the original image.
				// Also, null out the Optimized object and don't bother to try to create an optimized image.
				this._galleryObject.Thumbnail.DisplayObjectCreator = new GenericThumbnailCreator(this._galleryObject);
				this._galleryObject.Thumbnail.GenerateAndSaveFile();

				this._galleryObject.Optimized = new NullObjects.NullDisplayObject();
			}

			var mediaItemsAreWritable = !Factory.LoadGallerySetting(_galleryObject.GalleryId).MediaObjectPathIsReadOnly;

			if (mediaItemsAreWritable)
			{
				SyncFilenameToMetadataFilename();
			}

			var isNew = this._galleryObject.IsNew;

			// Save the data to the data store
			using (var repo = new MediaObjectRepository())
			{
				repo.Save(this._galleryObject);
			}

			foreach (var item in this._galleryObject.MetadataItems)
			{
				if (mediaItemsAreWritable && !isNew && item.HasChanges && item.PersistToFile)
				{
					// Persist meta property to original file, but only in writable galleries and for existing media objects
					// (new ones have their meta extracted from the file so it doesn't make sense to write it back right away).
					this._galleryObject.MetadataReadWriter.SaveMetaValue(item.MetadataItemName);
				}

				item.HasChanges = false;
			}
		}

		/// <summary>
		/// Rename the media file's name if the filename metadata value has changed. The thumbnail
		/// and the optimized file name is not modified, nor is any action taken when a media file
		/// does not exist (such as for external media objects). Do not call this function when the
		/// gallery is read-only.
		/// </summary>
		private void SyncFilenameToMetadataFilename()
		{
			IGalleryObjectMetadataItem metaItem;
			if (this._galleryObject.MetadataItems.TryGetMetadataItem(MetadataItemName.FileName, out metaItem))
			{
				if (!this._galleryObject.Original.FileName.Equals(metaItem.Value, StringComparison.OrdinalIgnoreCase))
				{
					// The filename metadata item has been changed, so update the actual file name.
					bool optFilenameSameAsOriginal = (this._galleryObject.Original.FileName.Equals(_galleryObject.Optimized.FileName, StringComparison.OrdinalIgnoreCase));
					var albumPath = this._galleryObject.Parent.FullPhysicalPathOnDisk;

					var prevPath = this._galleryObject.Original.FileNamePhysicalPath;
					this._galleryObject.Original.FileName = HelperFunctions.ValidateFileName(albumPath, metaItem.Value);

					// Uncomment to prevent user from changing extension. In some cases the user
					// may want to do this (such as changing from MP4 to MOV), so we'll allow it for now.
					//if (!this._galleryObject.Original.FileName.EndsWith(Path.GetExtension(prevPath) ?? String.Empty))
					//{
					//	// Don't let user change the extension, as this could cause trouble.
					//	this._galleryObject.Original.FileName += Path.GetExtension(prevPath);
					//}

					this._galleryObject.Original.FileNamePhysicalPath = Path.Combine(albumPath, this._galleryObject.Original.FileName);

					// Need to update the metaitem value in case a filename conflict caused the name to be
					// altered slightly (like with a (1) at the end).
					metaItem.Value = this._galleryObject.Original.FileName;

					if (File.Exists(prevPath))
					{
						try
						{
						    HelperFunctions.MoveFileSafely(prevPath, this._galleryObject.Original.FileNamePhysicalPath);
						}
						catch (IOException ex)
						{
							// Record additional details and re-throw
							if (!ex.Data.Contains("Cannot Rename"))
							{
								ex.Data.Add("Cannot Rename", String.Format("Error occurred renaming file {0} to {1} (directory {2}).", Path.GetFileName(prevPath), Path.GetFileName(this._galleryObject.Original.FileNamePhysicalPath), Path.GetDirectoryName(prevPath)));
							}

							throw;
						}
					}

					// When the optimized filename is the same as the original filename, be sure to 
					// update that one as well.
					if (optFilenameSameAsOriginal)
					{
						this._galleryObject.Optimized.FileName = this._galleryObject.Original.FileName;
						this._galleryObject.Optimized.FileNamePhysicalPath = this._galleryObject.Original.FileNamePhysicalPath;
					}
				}
			}
		}

		///// <summary>
		///// If any of the metadata items for this media object has its <see cref="IGalleryObject.ExtractMetadataOnSave" /> property 
		///// set to true, then open the original file, extract the items, and update the <see cref="IGalleryObject.MetadataItems" /> 
		///// property on our media object. The <see cref="IGalleryObject.ExtractMetadataOnSave" /> property is not changed to false 
		///// at this time, since the Save method uses it to know which items to persist to the data store.
		///// </summary>
		//private void UpdateMetadata()
		//{
		//	if (this._galleryObject.ExtractMetadataOnSave)
		//	{
		//		// Replace all metadata with the metadata found in the original file.
		//		//Metadata.MediaObjectMetadataExtractor metadata;
		//		try
		//		{
		//			this._galleryObject.ExtractMetadata();

		//			//metadata = new Metadata.MediaObjectMetadataExtractor(this._galleryObject.Original.FileNamePhysicalPath, this._galleryObject);
		//		}
		//		catch (OutOfMemoryException)
		//		{
		//			// Normally, the Dispose method is called during the Image_Saved event. But when we get this exception, it
		//			// never executes and therefore doesn't release the file lock. So we explicitly do so here and then 
		//			// re-throw the exception.
		//			this._galleryObject.Original.Dispose();
		//			throw new UnsupportedImageTypeException();
		//		}

		//		//this._galleryObject.MetadataItems.Clear();
		//		//this._galleryObject.MetadataItems.AddRange(metadata.GetGalleryObjectMetadataItemCollection());
		//		this._galleryObject.ExtractMetadataOnSave = true;
		//	}
		//	else
		//	{
		//		// If any individual metadata items have been set to ExtractFromFileOnSave = true, then update those selected ones with
		//		// the latest metadata from the file. If the metadata item is not found in the file, then set the value to an empty string.
		//		IGalleryObjectMetadataItemCollection metadataItemsToUpdate = this._galleryObject.MetadataItems.GetItemsToUpdate();
		//		if (metadataItemsToUpdate.Count > 0)
		//		{
		//			//Metadata.MediaObjectMetadataExtractor metadata;
		//			try
		//			{
		//				//this._galleryObject.CreateMetaItem()
		//				//metadata = new Metadata.MediaObjectMetadataExtractor(this._galleryObject.Original.FileNamePhysicalPath, this._galleryObject);
		//			}
		//			catch (OutOfMemoryException)
		//			{
		//				// Normally, the Dispose method is called during the Image_Saved event. But when we get this exception, it
		//				// never executes and therefore doesn't release the file lock. So we explicitly do so here and then 
		//				// re-throw the exception.
		//				this._galleryObject.Original.Dispose();
		//				throw new UnsupportedImageTypeException();
		//			}

		//			foreach (IGalleryObjectMetadataItem metadataItem in metadataItemsToUpdate)
		//			{
		//				var metaItem = this._galleryObject.CreateMetaItem(metadataItem.MetaDefinition);

		//				metadataItem.Description = metaItem.Description;
		//				metadataItem.Value = metaItem.Value;
		//				metadataItem.IsVisible = metaItem.IsVisible;

		//				//IGalleryObjectMetadataItem extractedMetadataItem;
		//				//if (metadata.GetGalleryObjectMetadataItemCollection().TryGetMetadataItem(metadataItem.MetadataItemName, out extractedMetadataItem))
		//				//{
		//				//	metadataItem.Value = extractedMetadataItem.Value;
		//				//}
		//				//else
		//				//{
		//				//	metadataItem.Value = String.Empty;
		//				//}
		//			}
		//		}
		//	}
		//}
	}
}
