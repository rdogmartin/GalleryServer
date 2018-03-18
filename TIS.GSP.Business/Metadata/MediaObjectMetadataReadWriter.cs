using System;
using System.Globalization;
using System.IO;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// Provides functionality for reading and writing metadata to or from a gallery object.
	/// </summary>
	public class MediaObjectMetadataReadWriter : GalleryObjectMetadataReadWriter
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectMetadataReadWriter" /> class.
		/// </summary>
		/// <param name="mediaObject">The media object.</param>
		protected MediaObjectMetadataReadWriter(IGalleryObject mediaObject)
		: base(mediaObject)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the metadata value for the specified <paramref name="metaName" />.
		/// </summary>
		/// <param name="metaName">Name of the metadata item to retrieve.</param>
		/// <returns>An instance that implements <see cref="IMetaValue" />.</returns>
		public override IMetaValue GetMetaValue(MetadataItemName metaName)
		{
			var fi = GalleryObject.Original.FileInfo;

			switch (metaName)
			{
				case MetadataItemName.Title:
					return new MetaValue(GalleryObject.Original.FileName);
				
				case MetadataItemName.FileName:
					return new MetaValue((fi != null ? fi.Name : null));

				case MetadataItemName.FileNameWithoutExtension:
					return new MetaValue((fi != null ? Path.GetFileNameWithoutExtension(fi.Name) : null));

				case MetadataItemName.FileSizeKb:
					if (fi == null)
						return null;

					int fileSize = (int)(fi.Length / 1024);
					fileSize = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
					return new MetaValue(String.Concat(fileSize.ToString("N0", CultureInfo.CurrentCulture), " ", Resources.Metadata_KB), fileSize.ToString(CultureInfo.InvariantCulture));

				case MetadataItemName.DateFileCreated:
					return (fi != null ? new MetaValue(fi.CreationTime.ToString(DateTimeFormatString, CultureInfo.InvariantCulture), fi.CreationTime.ToString("O", CultureInfo.InvariantCulture)) : null);

				case MetadataItemName.DateFileCreatedUtc:
					return (fi != null ? new MetaValue(fi.CreationTimeUtc.ToString(DateTimeFormatString, CultureInfo.InvariantCulture), fi.CreationTimeUtc.ToString("O", CultureInfo.InvariantCulture)) : null);

				case MetadataItemName.DateFileLastModified:
					return (fi != null ? new MetaValue(fi.LastWriteTime.ToString(DateTimeFormatString, CultureInfo.InvariantCulture), fi.LastWriteTime.ToString("O", CultureInfo.InvariantCulture)) : null);

				case MetadataItemName.DateFileLastModifiedUtc:
					return (fi != null ? new MetaValue(fi.LastWriteTimeUtc.ToString(DateTimeFormatString, CultureInfo.InvariantCulture), fi.LastWriteTimeUtc.ToString("O", CultureInfo.InvariantCulture)) : null);

				default:
					return base.GetMetaValue(metaName);
			}
		}

		#endregion
	}
}