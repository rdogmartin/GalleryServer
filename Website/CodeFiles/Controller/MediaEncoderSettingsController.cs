using System;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Controller
{
	/// <summary>
	/// Contains functionality for performing web-related tasks on media encoder settings.
	/// </summary>
	public static class MediaEncoderSettingsController
	{
		/// <summary>
		/// Gets an array of data entities representing the specified <paramref name="mediaEncoderSettings" />.
		/// The instance can be converted to a JSON string and sent to the browser.
		/// </summary>
		/// <param name="mediaEncoderSettings">The settings to convert.</param>
		/// <returns>
		/// An array of <see cref="Entity.MediaEncoderSettings" /> instances.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaEncoderSettings"/> is null.</exception>
		public static Entity.MediaEncoderSettings[] ToEntities(IMediaEncoderSettingsCollection mediaEncoderSettings)
		{
			if (mediaEncoderSettings == null)
				throw new ArgumentNullException("mediaEncoderSettings");

			List<Entity.MediaEncoderSettings> entities = new List<Entity.MediaEncoderSettings>(mediaEncoderSettings.Count);

			foreach (IMediaEncoderSettings encoderSetting in mediaEncoderSettings)
			{
				entities.Add(ToMediaEncoderSettingsEntity(encoderSetting));
			}

			return entities.ToArray();
		}

		/// <summary>
		/// Converts the <paramref name="entities" /> to a <see cref="IMediaEncoderSettingsCollection" /> collection. 
		/// Returns an empty collection if entities is null.
		/// </summary>
		/// <param name="entities">An array of <see cref="Entity.MediaEncoderSettings" /> items.</param>
		/// <returns>Returns an instance that implements <see cref="IMediaEncoderSettingsCollection" />.</returns>
		public static IMediaEncoderSettingsCollection ToMediaEncoderSettingsCollection(Entity.MediaEncoderSettings[] entities)
		{
			if (entities == null)
				return new Business.MediaEncoderSettingsCollection();

			IMediaEncoderSettingsCollection items = new Business.MediaEncoderSettingsCollection();

			int seq = 1;
			foreach (Entity.MediaEncoderSettings entity in entities)
			{
				items.Add(new Business.MediaEncoderSettings(entity.SourceFileExtension, entity.DestinationFileExtension, entity.EncoderArguments, seq));
				seq++;
			}

			return items;
		}

		/// <summary>
		/// Gets an array of file extensions that may be used in a media encoder setting. Does not include the
		/// 'All audio' or 'All video' items.
		/// </summary>
		/// <returns>An array of <see cref="Entity.FileExtension" /> items.</returns>
		public static Entity.FileExtension[] GetAvailableFileExtensions()
		{
			List<Entity.FileExtension> availFileExtensions = new List<Entity.FileExtension>();

			foreach (IMimeType mimeType in Business.Factory.LoadMimeTypes())
			{
				if ((mimeType.TypeCategory == Business.MimeTypeCategory.Video) || (mimeType.TypeCategory == Business.MimeTypeCategory.Audio))
				{
					availFileExtensions.Add(new Entity.FileExtension { Value = mimeType.Extension, Text = mimeType.Extension });
				}
			}

			return availFileExtensions.ToArray();
		}

		private static Entity.MediaEncoderSettings ToMediaEncoderSettingsEntity(IMediaEncoderSettings mediaEncoderSettings)
		{
			return new Entity.MediaEncoderSettings
							{
								SourceFileExtension = mediaEncoderSettings.SourceFileExtension,
								DestinationFileExtension = mediaEncoderSettings.DestinationFileExtension,
								EncoderArguments = mediaEncoderSettings.EncoderArguments
							};
		}
	}
}