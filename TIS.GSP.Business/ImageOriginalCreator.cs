using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// Contains functionality for manipulating the original image files associated with <see cref="Image" /> gallery objects.
	/// The only time a new original image must be generated is when the user rotates it. This will only
	/// occur for existing objects.
	/// </summary>
	public class ImageOriginalCreator : DisplayObjectCreator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImageOriginalCreator"/> class.
		/// </summary>
		/// <param name="imageObject">The image object.</param>
		public ImageOriginalCreator(Image imageObject)
		{
			GalleryObject = imageObject;
		}
		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. No data is
		/// persisted to the data store.
		/// </summary>
		/// <exception cref="UnsupportedImageTypeException">Thrown when Gallery Server cannot process the image, 
		/// most likely because it is corrupt or an unsupported image type.</exception>
		public override void GenerateAndSaveFile()
		{
			// The only time we need to generate a new original image is when the user rotates it. This will only
			// occur for existing objects.
			if ((GalleryObject.IsNew) || (GalleryObject.RotateFlip == MediaAssetRotateFlip.NotSpecified))
				return;

			// We have an existing object with a specific rotation requested. For example, if the requested rotation is
			// 0 degrees and the image is oriented 90 degrees CW, the file will be rotated 90 degrees CCW.
			var filePath = GalleryObject.Original.FileNamePhysicalPath;

			if (!File.Exists(filePath))
				throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Cannot rotate image because no file exists at {0}.", filePath));

			var rotateFlipResult = RotateFlip(filePath, GallerySettings.OriginalImageJpegQuality);

			if (rotateFlipResult.Item1 > MediaAssetRotateFlip.Rotate0FlipNone)
			{
				if (!rotateFlipResult.Item2.IsEmpty)
				{
					GalleryObject.Original.Width = (int)rotateFlipResult.Item2.Width;
					GalleryObject.Original.Height = (int)rotateFlipResult.Item2.Height;
				}

				RefreshImageMetadata();

				int fileSize = (int)(GalleryObject.Original.FileInfo.Length / 1024);
				GalleryObject.Original.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
			}
			else
			{
				// Turns out the file wasn't actually rotated, but we need to remove the orientation flag to prevent the 
				// auto-rotate functionality from doing the wrong thing in the future.
				IGalleryObjectMetadataItem metaItem;
				if (GalleryObject.MetadataItems.TryGetMetadataItem(MetadataItemName.Orientation, out metaItem))
				{
					metaItem.IsDeleted = true;
					Factory.SaveGalleryObjectMetadataItem(metaItem, GalleryObject.LastModifiedByUserName);
				}
			}
		}

		/// <summary>
		/// Re-extract several metadata values from the file. Call this function when performing an action on a file
		/// that may render existing metadata items inaccurate, such as width and height. The new values are not persisted;
		/// it is expected a subsequent function will do that.
		/// </summary>
		private void RefreshImageMetadata()
		{
			GalleryObject.ExtractMetadata(GalleryObject.MetaDefinitions.Find(MetadataItemName.Width));
			GalleryObject.ExtractMetadata(GalleryObject.MetaDefinitions.Find(MetadataItemName.Height));
			GalleryObject.ExtractMetadata(GalleryObject.MetaDefinitions.Find(MetadataItemName.Dimensions));
			GalleryObject.ExtractMetadata(GalleryObject.MetaDefinitions.Find(MetadataItemName.HorizontalResolution));
			GalleryObject.ExtractMetadata(GalleryObject.MetaDefinitions.Find(MetadataItemName.VerticalResolution));
			GalleryObject.ExtractMetadata(GalleryObject.MetaDefinitions.Find(MetadataItemName.Orientation));
		}
	}
}

