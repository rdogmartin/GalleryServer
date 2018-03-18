using System;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the MediaObject table.
  /// </summary>
  public class MediaObjectRepository : Repository<GalleryDb, MediaObjectDto>
	{
    /// <summary>
    /// Saves the specified media object.
    /// </summary>
    /// <param name="mediaObject">The media object.</param>
    /// <exception cref="System.ArgumentNullException">mediaObject</exception>
    public void Save(IGalleryObject mediaObject)
		{
			if (mediaObject == null)
				throw new ArgumentNullException("mediaObject");

			if (mediaObject.IsNew)
			{
				var moDto = new MediaObjectDto
											{
												FKAlbumId = mediaObject.Parent.Id,
												ThumbnailFilename = mediaObject.Thumbnail.FileName,
												ThumbnailWidth = mediaObject.Thumbnail.Width,
												ThumbnailHeight = mediaObject.Thumbnail.Height,
												ThumbnailSizeKB = mediaObject.Thumbnail.FileSizeKB,
												OptimizedFilename = mediaObject.Optimized.FileName,
												OptimizedWidth = mediaObject.Optimized.Width,
												OptimizedHeight = mediaObject.Optimized.Height,
												OptimizedSizeKB = mediaObject.Optimized.FileSizeKB,
												OriginalFilename = mediaObject.Original.FileName,
												OriginalWidth = mediaObject.Original.Width,
												OriginalHeight = mediaObject.Original.Height,
												OriginalSizeKB = mediaObject.Original.FileSizeKB,
												ExternalHtmlSource = mediaObject.Original.ExternalHtmlSource,
												ExternalType = (mediaObject.Original.ExternalType == MimeTypeCategory.NotSet ? String.Empty : mediaObject.Original.ExternalType.ToString()),
												Seq = mediaObject.Sequence,
												CreatedBy = mediaObject.CreatedByUserName,
												DateAdded = mediaObject.DateAdded,
												LastModifiedBy = mediaObject.LastModifiedByUserName,
												DateLastModified = mediaObject.DateLastModified,
												IsPrivate = mediaObject.IsPrivate
											};

				Add(moDto);
				Save(); // Save now so we can get at the ID

				if (mediaObject.Id != moDto.MediaObjectId)
					mediaObject.Id = moDto.MediaObjectId;

				// Insert metadata items, if any, into metadata table.
				var repo = new MetadataRepository(Context); // Don't put in using construct because we don't want our Context disposed
				repo.Save(mediaObject.MetadataItems);
			}
			else
			{
				MediaObjectDto moDto = Find(mediaObject.Id);

				if (moDto != null)
				{
					moDto.FKAlbumId = mediaObject.Parent.Id;
					moDto.ThumbnailFilename = mediaObject.Thumbnail.FileName;
					moDto.ThumbnailWidth = mediaObject.Thumbnail.Width;
					moDto.ThumbnailHeight = mediaObject.Thumbnail.Height;
					moDto.ThumbnailSizeKB = mediaObject.Thumbnail.FileSizeKB;
					moDto.OptimizedFilename = mediaObject.Optimized.FileName;
					moDto.OptimizedWidth = mediaObject.Optimized.Width;
					moDto.OptimizedHeight = mediaObject.Optimized.Height;
					moDto.OptimizedSizeKB = mediaObject.Optimized.FileSizeKB;
					moDto.OriginalFilename = mediaObject.Original.FileName;
					moDto.OriginalWidth = mediaObject.Original.Width;
					moDto.OriginalHeight = mediaObject.Original.Height;
					moDto.OriginalSizeKB = mediaObject.Original.FileSizeKB;
					moDto.ExternalHtmlSource = mediaObject.Original.ExternalHtmlSource;
					moDto.ExternalType = (mediaObject.Original.ExternalType == MimeTypeCategory.NotSet ? String.Empty : mediaObject.Original.ExternalType.ToString());
					moDto.Seq = mediaObject.Sequence;
					moDto.CreatedBy = mediaObject.CreatedByUserName;
					moDto.DateAdded = mediaObject.DateAdded;
					moDto.LastModifiedBy = mediaObject.LastModifiedByUserName;
					moDto.DateLastModified = mediaObject.DateLastModified;
					moDto.IsPrivate = mediaObject.IsPrivate;

					Save();
					
					// Update metadata items, if necessary, in metadata table.
					var repo = new MetadataRepository(Context); // Don't put in using construct because we don't want our Context disposed
					repo.Save(mediaObject.MetadataItems);
				}
			}
		}

    /// <summary>
    /// Deletes the specified media object.
    /// </summary>
    /// <param name="mediaObject">The media object.</param>
    public void Delete(IGalleryObject mediaObject)
		{
			var mDto = Find(mediaObject.Id);

			if (mDto != null)
			{
				Delete(mDto);
				Save(); // Cascade relationship will auto-delete metadata items
			}

			var tagRepo = new TagRepository(Context);
			tagRepo.DeleteUnusedTags();
		}
	}
}