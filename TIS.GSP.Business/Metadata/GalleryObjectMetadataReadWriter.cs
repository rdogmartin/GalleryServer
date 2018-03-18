using System;
using System.Globalization;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// An abstract base class that provides functionality for reading and writing metadata to and from a gallery 
	/// object. The concrete implementations (<see cref="MediaObjectMetadataReadWriter" />, 
	/// <see cref="ImageMetadataReadWriter" />, <see cref="AlbumMetadataReadWriter" />, 
	/// <see cref="VideoMetadataReadWriter" />, <see cref="AudioMetadataReadWriter" />, etc.) inherit from this 
	/// one and delegate to this class when the subclassed <see cref="IMetadataReadWriter.GetMetaValue" /> 
	/// method does not provide a specific implementation.
	/// </summary>
	/// <remarks>For example, because the <see cref="MetadataItemName.DateAdded" /> metadata
	/// item applies to all gallery objects, it is implemented in this class. Metadata specific to
	/// images are implemented in the <see cref="ImageMetadataReadWriter" /> class (e.g. 
	/// <see cref="MetadataItemName.FocalLength" />). Metadata that is common to all media
	/// objects are implemented in <see cref="MediaObjectMetadataReadWriter" />.</remarks>
	public abstract class GalleryObjectMetadataReadWriter : IMetadataReadWriter
	{
		private string _dateTimeFormatString;

		#region Properties

		/// <summary>
		/// Gets the gallery object from which metadata is to be extracted.
		/// </summary>
		/// <value>An instance of <see cref="IGalleryObject" />.</value>
		public IGalleryObject GalleryObject { get; private set; }

		/// <summary>
		/// Gets or sets the format string to use for <see cref="DateTime" /> metadata values. The date type of each meta item
		/// is specified by the <see cref="IMetadataDefinition.DataType" /> property.
		/// </summary>
		/// <value>The date time format string.</value>
		public string DateTimeFormatString
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_dateTimeFormatString))
				{
					_dateTimeFormatString = Factory.LoadGallerySetting(GalleryObject.GalleryId).MetadataDateTimeFormatString;
				}

				return _dateTimeFormatString;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectMetadataReadWriter" /> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		protected GalleryObjectMetadataReadWriter(IGalleryObject galleryObject)
		{
			GalleryObject = galleryObject;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Extracts a metadata instance for the specified <paramref name="metaName" />.
		/// </summary>
		/// <param name="metaName">Name of the metadata item to retrieve.</param>
		/// <returns>An instance that implements <see cref="IMetaValue" />.</returns>
		public virtual IMetaValue GetMetaValue(MetadataItemName metaName)
		{
			switch (metaName)
			{
				case MetadataItemName.DateAdded:
					return new MetaValue(GalleryObject.DateAdded.ToLocalTime().ToString(DateTimeFormatString, CultureInfo.InvariantCulture), GalleryObject.DateAdded.ToString("O", CultureInfo.InvariantCulture));
					// Use the following if you want it formatted in UTC instead (above will store the formatted time in server's time zone)
					//return new MetaValue(GalleryObject.DateAdded.ToString(DateTimeFormatString + " UTC", CultureInfo.InvariantCulture), GalleryObject.DateAdded.ToString("O", CultureInfo.InvariantCulture));

				default:
					return null;
			}
		}

		/// <summary>
		/// Persists the meta value identified by <paramref name="metaName" /> to the media file. It is expected the meta item
		/// exists in <see cref="IGalleryObject.MetadataItems" />.
		/// </summary>
		/// <param name="metaName">Name of the meta item to persist.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		public virtual void SaveMetaValue(MetadataItemName metaName)
		{
			// Do nothing. Functionality is contained in classes that inherit from this class (e.g. ImageMetadataReadWriter).
		}

		/// <summary>
		/// Permanently removes the meta value from the media file. The item is also removed from
		/// <see cref="IGalleryObject.MetadataItems" />.
		/// </summary>
		/// <param name="metaName">Name of the meta item to delete.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		public virtual void DeleteMetaValue(MetadataItemName metaName)
		{
			// Do nothing. Functionality is contained in classes that inherit from this class (e.g. ImageMetadataReadWriter).
		}

		#endregion
	}
}