using System;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Provides functionality for reading and writing metadata to or from a gallery object.
	/// </summary>
	public interface IMetadataReadWriter
	{
		/// <summary>
		/// Gets the gallery object from which metadata is to be extracted.
		/// </summary>
		/// <value>An instance of <see cref="IGalleryObject" />.</value>
		IGalleryObject GalleryObject { get; }

		/// <summary>
		/// Gets or sets the format string to use for <see cref="DateTime" /> metadata values. The date type of each meta item
		/// is specified by the <see cref="IMetadataDefinition.DataType" /> property.
		/// </summary>
		string DateTimeFormatString { get; }

		/// <summary>
		/// Extracts a metadata instance for the specified <paramref name="metaName" />.
		/// </summary>
		/// <param name="metaName">Name of the metadata item to retrieve.</param>
		/// <returns>An instance that implements <see cref="IMetaValue" />.</returns>
		IMetaValue GetMetaValue(MetadataItemName metaName);

		/// <summary>
		/// Persists the meta value identified by <paramref name="metaName" /> to the media file. It is expected the meta item
		/// exists in <see cref="IGalleryObject.MetadataItems" />. No action is taken if <see cref="IGalleryObjectMetadataItem.PersistToFile" />
		/// is <c>false</c>.
		/// </summary>
		/// <param name="metaName">Name of the meta item to persist.</param>
		void SaveMetaValue(MetadataItemName metaName);

		/// <summary>
		/// Permanently removes the meta value from the media file. The item is also removed from 
		/// <see cref="IGalleryObject.MetadataItems" />. No action is taken if <see cref="IGalleryObjectMetadataItem.PersistToFile" />
		/// is <c>false</c>.
		/// </summary>
		/// <param name="metaName">Name of the meta item to delete.</param>
		void DeleteMetaValue(MetadataItemName metaName);
	}
}