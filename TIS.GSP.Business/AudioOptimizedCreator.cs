using System;
using System.IO;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// Provides functionality for creating and saving a web-friendly version of a <see cref="Audio" /> gallery object.
	/// </summary>
	public class AudioOptimizedCreator : DisplayObjectCreator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AudioOptimizedCreator"/> class.
		/// </summary>
		/// <param name="galleryObject">The media object.</param>
		public AudioOptimizedCreator(IGalleryObject galleryObject)
		{
			GalleryObject = galleryObject;
		}

		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
		/// persisted to the data store.
		/// </summary>
		public override void GenerateAndSaveFile()
		{
			// If necessary, generate and save the optimized version of the original file.
			if (!IsOptimizedAudioRequired() || !File.Exists(GalleryObject.Original.FileNamePhysicalPath))
			{
				return;
			}

			// Add to queue if an encoder setting exists for this file type.
			if (FFmpeg.IsAvailable && MediaConversionQueue.Instance.HasEncoderSetting(GalleryObject))
			{
				MediaConversionQueue.Instance.Add(GalleryObject, MediaQueueItemConversionType.CreateOptimized);
				MediaConversionQueue.Instance.Process();
			}
		}

		private bool IsOptimizedAudioRequired()
		{
			if (GalleryObject.IsNew)
				return false;
			else
				return (RequiresOptimizedAudio() || GalleryObject.RegenerateOptimizedOnSave);
		}

		private bool RequiresOptimizedAudio()
		{
			if (MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(GalleryObject.Id, MediaQueueItemConversionType.CreateOptimized))
			{
				// File is already in the queue, so we don't need to create one
				return false;
			}

			// We need an optimized file if the opt. and original file names are the same or if the file doesn't exist.
			var optFileSameAsOriginal = String.Equals(GalleryObject.Optimized.FileName, GalleryObject.Original.FileName, StringComparison.OrdinalIgnoreCase);

			return (optFileSameAsOriginal || !File.Exists(GalleryObject.Optimized.FileNamePhysicalPath));
		}
	}
}
