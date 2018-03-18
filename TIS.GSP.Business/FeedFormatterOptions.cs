using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business
{
	/// <summary>
	/// Represents the feed formatter options for generating an RSS/Atom feed.
	/// </summary>
	public class FeedFormatterOptions : IFeedFormatterOptions
	{
		/// <summary>
		/// Gets or sets the metadata property to sort the album by.
		/// </summary>
		/// <value>The metadata property to sort the album by.</value>
		public MetadataItemName SortByMetaName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the contents of the album are sorted in ascending order. A 
		/// <c>false</c> value indicates
		/// a descending sort.
		/// </summary>
		/// <value><c>true</c> if an album's contents are sorted in ascending order; <c>false</c> if descending order.</value>
		public bool SortAscending { get; set; }

		/// <summary>
		/// Gets or sets the URL, relative to the website root, that hyperlinks should point to.
		/// Ex: "/dev/gs/default.aspx"
		/// </summary>
		/// <value>The destination URL.</value>
		public string DestinationUrl { get; set; }
	}
}
