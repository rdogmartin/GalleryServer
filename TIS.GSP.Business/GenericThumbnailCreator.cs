using System;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;

namespace GalleryServer.Business
{
	/// <summary>
	/// Provides functionality for creating and saving the thumbnail image files associated with <see cref="GenericMediaObject" /> gallery objects.
	/// </summary>
	public class GenericThumbnailCreator : DisplayObjectCreator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GenericThumbnailCreator"/> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public GenericThumbnailCreator(IGalleryObject galleryObject)
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
			// If necessary, generate and save the thumbnail version of the original image.
			if (!(IsThumbnailImageRequired()))
			{
				return; // No thumbnail image required.
			}

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryObject.GalleryId);

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

			if (Array.IndexOf<string>(gallerySetting.ImageMagickFileTypes, Path.GetExtension(GalleryObject.Original.FileName).ToLowerInvariant()) >= 0)
			{
				GenerateThumbnailImageUsingImageMagick(newFilePath, gallerySetting);
			}
			else
			{
				GenerateGenericThumbnailImage(newFilePath, gallerySetting);
			}

			GalleryObject.Thumbnail.FileName = newFilename;
			GalleryObject.Thumbnail.FileNamePhysicalPath = newFilePath;

			int fileSize = (int)(GalleryObject.Thumbnail.FileInfo.Length / 1024);

			GalleryObject.Thumbnail.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
		}

		private void GenerateThumbnailImageUsingImageMagick(string newFilePath, IGallerySettings gallerySetting)
		{
			// Generate a temporary filename to store the thumbnail created by ImageMagick.
			string tmpImageThumbnailPath = Path.Combine(AppSetting.Instance.TempUploadDirectory, String.Concat(Guid.NewGuid().ToString(), ".jpg"));

			// Request that ImageMagick create the thumbnail. If successful, the file will be created. If not, it fails silently.
			ImageMagick.GenerateImage(GalleryObject.Original.FileNamePhysicalPath, tmpImageThumbnailPath, GalleryObject.GalleryId);

			if (File.Exists(tmpImageThumbnailPath))
			{
				try
				{
					// ImageMagick successfully created a thumbnail image. Now resize it to the width and height we need.
					using (var originalBitmap = new Bitmap(tmpImageThumbnailPath))
					{
						var newSize = CalculateWidthAndHeight(new System.Windows.Size(originalBitmap.Width, originalBitmap.Height), gallerySetting.MaxThumbnailLength, false);

						// Get JPEG quality value (0 - 100). This is ignored if imgFormat = GIF.
						int jpegQuality = gallerySetting.ThumbnailImageJpegQuality;

						// Generate the new image and save to disk.
						var size = ImageHelper.SaveImageFile(originalBitmap, newFilePath, ImageFormat.Jpeg, newSize.Width, newSize.Height, jpegQuality);

						GalleryObject.Thumbnail.Width = (int)size.Width;
						GalleryObject.Thumbnail.Height = (int)size.Height;
					}
				}
				catch (Exception ex)
				{
					ex.Data.Add("GSP Info", String.Format("This error occurred while trying to process the ImageMagick-generated file {0}. The original file is {1}. A generic thumbnail image will be created instead.", tmpImageThumbnailPath, GalleryObject.Original.FileNamePhysicalPath));
					Events.EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());

					// Default to a generic thumbnail image.
					GenerateGenericThumbnailImage(newFilePath, gallerySetting);
				}

				try
				{
					// Now delete the thumbnail image created by FFmpeg, but no worries if an error happens. The file is in the temp directory
					// which is cleaned out each time the app starts anyway.
					File.Delete(tmpImageThumbnailPath);
				}
				catch (IOException ex)
				{
					ex.Data.Add("GSP Info", "This error was handled and did not interfere with the user experience.");
					Events.EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
				}
				catch (UnauthorizedAccessException ex)
				{
					ex.Data.Add("GSP Info", "This error was handled and did not interfere with the user experience.");
					Events.EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
				}
				catch (NotSupportedException ex)
				{
					ex.Data.Add("GSP Info", "This error was handled and did not interfere with the user experience.");
					Events.EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
				}
			}
			else
			{
				// ImageMagick didn't create an image, so default to a generic one.
				GenerateGenericThumbnailImage(newFilePath, gallerySetting);
			}
		}

		private void GenerateGenericThumbnailImage(string newFilePath, IGallerySettings gallerySetting)
		{
			// Build a generic thumbnail.
			using (Bitmap originalBitmap = GetGenericThumbnailBitmap(GalleryObject.MimeType))
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

		private static Bitmap GetGenericThumbnailBitmap(IMimeType mimeType)
		{
			Bitmap thumbnailBitmap = null;

			switch (mimeType.MajorType.ToUpperInvariant())
			{
				case "AUDIO": thumbnailBitmap = Resources.GenericThumbnailImage_Audio; break;
				case "VIDEO": thumbnailBitmap = Resources.GenericThumbnailImage_Video; break;
				case "IMAGE": thumbnailBitmap = Resources.GenericThumbnailImage_Image; break;
				case "APPLICATION": thumbnailBitmap = GetGenericThumbnailBitmapByFileExtension(mimeType.Extension); break;
				default: thumbnailBitmap = Resources.GenericThumbnailImage_Unknown; break;
			}

			return thumbnailBitmap;
		}

		private static Bitmap GetGenericThumbnailBitmapByFileExtension(string fileExtension)
		{
			Bitmap thumbnailBitmap = null;

			switch (fileExtension)
			{
				case ".doc":
				case ".dot":
				case ".docm":
				case ".dotm":
				case ".dotx":
				case ".docx": thumbnailBitmap = Resources.GenericThumbnailImage_Doc; break;
				case ".xls":
				case ".xlam":
				case ".xlsb":
				case ".xlsm":
				case ".xltm":
				case ".xltx":
				case ".xlsx": thumbnailBitmap = Resources.GenericThumbnailImage_Excel; break;
				case ".ppt":
				case ".pps":
				case ".pptx":
				case ".potm":
				case ".ppam":
				case ".ppsm": thumbnailBitmap = Resources.GenericThumbnailImage_PowerPoint; break;
				case ".pdf": thumbnailBitmap = Resources.GenericThumbnailImage_PDF; break;
				default: thumbnailBitmap = Resources.GenericThumbnailImage_Unknown; break;
			}
			return thumbnailBitmap;
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