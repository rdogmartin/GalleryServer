using System;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.Properties;
using GalleryServer.Events;

namespace GalleryServer.Business
{
	/// <summary>
	/// Provides functionality for creating and saving the thumbnail image files associated with <see cref="Video" /> gallery objects.
	/// </summary>
	public class VideoThumbnailCreator : DisplayObjectCreator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VideoThumbnailCreator"/> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public VideoThumbnailCreator(IGalleryObject galleryObject)
		{
			GalleryObject = galleryObject;
		}

		/// <summary>
		/// Generate the thumbnail image for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
		/// persisted to the data store.
		/// </summary>
		public override void GenerateAndSaveFile()
		{
			// If necessary, generate and save the thumbnail version of the video.
			if (!(IsThumbnailImageRequired()))
			{
				return; // No thumbnail image required.
			}

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryObject.GalleryId);

			// Generate a temporary filename to store the thumbnail created by FFmpeg.
			string tmpVideoThumbnailPath = Path.Combine(AppSetting.Instance.TempUploadDirectory, String.Concat(Guid.NewGuid().ToString(), ".jpg"));

			// Request that FFmpeg create the thumbnail. If successful, the file will be created.
			FFmpeg.GenerateThumbnail(GalleryObject.Original.FileNamePhysicalPath, tmpVideoThumbnailPath, gallerySetting.VideoThumbnailPosition, GalleryObject.GalleryId);

			// Verify image was created from video, trying again using a different video position setting if necessary.
			ValidateVideoThumbnail(tmpVideoThumbnailPath, gallerySetting.VideoThumbnailPosition);

			// Determine file name and path of the thumbnail image. If a file name has already been previously
			// calculated for this media object, re-use it. Otherwise generate a unique name.
			var newFilename = GalleryObject.Thumbnail.FileName;
			var newFilePath = GalleryObject.Thumbnail.FileNamePhysicalPath;

      if (String.IsNullOrEmpty(newFilePath))
      {
				var thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this.GalleryObject.Original.FileInfo.DirectoryName, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
				newFilename = GenerateJpegFilename(thumbnailPath, gallerySetting.ThumbnailFileNamePrefix);
				newFilePath = Path.Combine(thumbnailPath, newFilename);
			}

			if (File.Exists(tmpVideoThumbnailPath))
			{
				// FFmpeg successfully created a thumbnail image the same size as the video. Now resize it to the width and height we need.
				using (Bitmap originalBitmap = new Bitmap(tmpVideoThumbnailPath))
				{
					var newSize = CalculateWidthAndHeight(new System.Windows.Size(originalBitmap.Width, originalBitmap.Height), gallerySetting.MaxThumbnailLength, false);

					// Get JPEG quality value (0 - 100). This is ignored if imgFormat = GIF.
					int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

					// Generate the new image and save to disk.
					var size = ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newSize.Width, newSize.Height, jpegQuality);

					var rotatedSize = ExecuteAutoRotation(newFilePath, jpegQuality);
					if (!rotatedSize.IsEmpty)
					{
						size = rotatedSize;
					}

					GalleryObject.Thumbnail.Width = (int)size.Width;
					GalleryObject.Thumbnail.Height = (int)size.Height;
				}

				try
				{
					// Now delete the thumbnail image created by FFmpeg, but no worries if an error happens. The file is in the temp directory
					// which is cleaned out each time the app starts anyway.
					File.Delete(tmpVideoThumbnailPath);
				}
				catch (IOException ex)
				{
					EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
				}
				catch (UnauthorizedAccessException ex)
				{
					EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
				}
				catch (NotSupportedException ex)
				{
					EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
				}
			}
			else
			{
				// FFmpeg didn't run or no thumbnail image was created by FFmpeg. Build a generic video thumbnail.
				using (Bitmap originalBitmap = Resources.GenericThumbnailImage_Video)
				{
					var newSize = CalculateWidthAndHeight(new System.Windows.Size(originalBitmap.Width, originalBitmap.Height), gallerySetting.MaxThumbnailLength, true);

					// Get JPEG quality value (0 - 100).
					int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

					// Generate the new image and save to disk.
					var size = ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newSize.Width, newSize.Height, jpegQuality);

					GalleryObject.Thumbnail.Width = (int)size.Width;
					GalleryObject.Thumbnail.Height = (int)size.Height;
				}
			}

			GalleryObject.Thumbnail.FileName = newFilename;
			GalleryObject.Thumbnail.FileNamePhysicalPath = newFilePath;

			int fileSize = (int)(GalleryObject.Thumbnail.FileInfo.Length / 1024);

			GalleryObject.Thumbnail.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
		}

		/// <summary>
		/// Verify the image was created from the video. If not, it might be because the video is shorter than the position
		/// where we tried to grab the image. If this is the case, try again, except grab an image from the beginning of the video.
		/// </summary>
		/// <param name="tmpVideoThumbnailPath">The video thumbnail path.</param>
		/// <param name="videoThumbnailPosition">The position, in seconds, in the video where the thumbnail is generated from a frame.</param>
		private void ValidateVideoThumbnail(string tmpVideoThumbnailPath, int videoThumbnailPosition)
		{
			if (!File.Exists(tmpVideoThumbnailPath))
			{
				IGalleryObjectMetadataItem metadataItem;
				if (GalleryObject.MetadataItems.TryGetMetadataItem(MetadataItemName.Duration, out metadataItem))
				{
					TimeSpan duration;
					if (TimeSpan.TryParse(metadataItem.Value, out duration))
					{
						if (duration < new TimeSpan(0, 0, videoThumbnailPosition))
						{
							// Video is shorter than the number of seconds where we are suppossed to grab the thumbnail.
							// Try again, except use 1 second instead of the gallery setting.
							const int videoThumbnailPositionFallback = 1;
							FFmpeg.GenerateThumbnail(GalleryObject.Original.FileNamePhysicalPath, tmpVideoThumbnailPath, videoThumbnailPositionFallback, GalleryObject.GalleryId);
						}
					}
				}
			}
		}

		private bool IsThumbnailImageRequired()
		{
			// We must create a thumbnail image in the following circumstances:
			// 1. The file corresponding to a previously created thumbnail image file does not exist.
			//    OR
			// 2. The overwrite flag is true.

			bool thumbnailImageMissing = IsThumbnailImageFileMissing(); // Test 1

			bool overwriteFlag = GalleryObject.RegenerateThumbnailOnSave; // Test 2

			return (thumbnailImageMissing || overwriteFlag);
		}

		private bool IsThumbnailImageFileMissing()
		{
			// Does the thumbnail image file exist? (Maybe it was accidentally deleted or moved by the user,
			// or maybe it's a new object.)
			bool thumbnailImageExists = false;

			if (File.Exists(GalleryObject.Thumbnail.FileNamePhysicalPath))
			{
				// Thumbnail image file exists.
				thumbnailImageExists = true;
			}

			bool thumbnailImageIsMissing = !thumbnailImageExists;

			return thumbnailImageIsMissing;
		}

		/// <summary>
		/// Determine name of new file and ensure it is unique in the directory. (Example: If original = puppy.jpg,
		/// thumbnail = zThumb_puppy.jpg)
		/// </summary>
		/// <param name="thumbnailPath">The path to the directory where the thumbnail file is to be created.</param>
		/// <param name="imgFormat">The image format of the thumbnail.</param>
		/// <param name="filenamePrefix">A string to prepend to the filename. Example: "zThumb_"</param>
		/// <returns>
		/// Returns the name of the new thumbnail file name and ensure it is unique in the directory.
		/// </returns>
		private string GenerateNewFilename(string thumbnailPath, ImageFormat imgFormat, string filenamePrefix)
		{
			string nameWithoutExtension = Path.GetFileNameWithoutExtension(GalleryObject.Original.FileInfo.Name);
			string thumbnailFilename = String.Format(CultureInfo.CurrentCulture, "{0}{1}.{2}", filenamePrefix, nameWithoutExtension, imgFormat.ToString().ToLower(CultureInfo.CurrentCulture));

			thumbnailFilename = HelperFunctions.ValidateFileName(thumbnailPath, thumbnailFilename);

			return thumbnailFilename;
		}
	}
}