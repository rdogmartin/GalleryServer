using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a metadata value. It is composed of two main properties - the raw value extracted
	/// from the media file and the formatted, user-friendly version.
	/// </summary>
	public interface IMetaValue
	{
		/// <summary>
		/// Gets the raw value as it extracted from the media file. This value will be null in these cases:
		/// (1) The meta item does not exist in the media file. (2) The meta item is a composite of
		/// multiple meta values (e.g. <see cref="MetadataItemName.GpsLocationWithMapLink" />)
		/// </summary>
		string RawValue { get; }

		/// <summary>
		/// Gets or sets the formatted, user-friendly value of the meta item.
		/// </summary>
		string FormattedValue { get; set; }
	}
}