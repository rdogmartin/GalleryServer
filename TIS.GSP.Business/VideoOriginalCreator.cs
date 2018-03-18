using System;
using System.Globalization;
using System.IO;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// Contains functionality for manipulating the original video file associated with <see cref="Video" /> gallery objects.
	/// The only time a new original video must be generated is when the user rotates it. This will only
	/// occur for existing objects.
	/// </summary>
	public class VideoOriginalCreator : DisplayObjectCreator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VideoOriginalCreator"/> class.
		/// </summary>
		/// <param name="videoObject">The video object.</param>
		public VideoOriginalCreator(Video videoObject)
		{
			GalleryObject = videoObject;
		}
		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. No data is
		/// persisted to the data store.
		/// </summary>
		public override void GenerateAndSaveFile()
		{
			// The only time we need to generate a new original video is when the user rotates it. This will only
			// occur for existing objects.
			if ((GalleryObject.IsNew) || (GalleryObject.RotateFlip == MediaAssetRotateFlip.NotSpecified) || !File.Exists(GalleryObject.Original.FileNamePhysicalPath))
				return;

			// We have an existing object with a specific rotation requested. For example, if the requested rotation is
			// 0 degrees and the image is oriented 90 degrees CW, the file will be rotated 90 degrees CCW.
			var filePath = GalleryObject.Original.FileNamePhysicalPath;

			//if (!File.Exists(filePath))
			//	throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Cannot rotate video because no file exists at {0}.", filePath));

			MediaConversionQueue.Instance.Add(GalleryObject, MediaQueueItemConversionType.RotateVideo);
			MediaConversionQueue.Instance.Process();
		}
	}
}

