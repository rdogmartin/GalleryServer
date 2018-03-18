using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Defines an object that contains user preferences for an album.
	/// </summary>
	public interface IAlbumProfile
	{
		/// <summary>
		/// Gets or sets the album ID.
		/// </summary>
		/// <value>An integer.</value>
		int AlbumId { get; set; }

		/// <summary>
		/// Gets or sets the metadata name to sort the album by.
		/// </summary>
		/// <value>An instance of <see cref="MetadataItemName" />.</value>
		MetadataItemName SortByMetaName { get; set; }

		/// <summary>
		/// Indicates the direction the album is to be sorted. A value of <c>true</c> indicates ascending 
		/// order; a value of <c>false</c> indicates descending order.
		/// </summary>
		/// <value><c>true</c> if ascending order; otherwise, <c>false</c>.</value>
		bool SortAscending { get; set; }

		/// <summary>
		/// Perform a deep copy of this item.
		/// </summary>
		/// <returns>Returns a deep copy of this item.</returns>
		IAlbumProfile Copy();
	}
}