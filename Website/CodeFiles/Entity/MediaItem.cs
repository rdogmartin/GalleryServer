using System.Diagnostics;
using GalleryServer.Business;

namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A client-optimized object that contains media object information. This class is used to 
	/// pass information between the browser and the web server via AJAX callbacks.
	/// </summary>
	[DebuggerDisplay("ID {Id}: Title={Title} (Album {AlbumTitle}))")]
	public class MediaItem
	{
		/// <summary>
		/// The media object ID.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the ID of the physical album this media object belongs to. This is useful when the item is packaged
		/// in a virtual album.
		/// </summary>
		public int AlbumId { get; set; }

		/// <summary>
		/// Gets or sets the title of the physical album this media object belongs to. This is useful when the item is packaged
		/// in a virtual album.
		/// </summary>
		public string AlbumTitle { get; set; }

		/// <summary>
		/// Specifies the one-based index of this media object among the others in the containing album.
		/// The first media object in an album has index = 1.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// The media object title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the views available for this media object.
		/// </summary>
		/// <value>The views.</value>
		public DisplayObject[] Views { get; set; }

		/// <summary>
		/// Indicates whether a high resolution version of this image exists and is available for viewing.
		/// </summary>
		public bool HighResAvailable { get; set; }

		/// <summary>
		/// Indicates whether a downloadable version of this media object exists and can be downloaded. External media objects
		/// cannot be downloaded.
		/// </summary>
		public bool IsDownloadable { get; set; }

		/// <summary>
		/// Gets or sets the index of the view currently being rendered. This value can be used to get 
		/// or set the desired view to display among the possibilities in <see cref="Views" />.
		/// </summary>
		/// <value>The index of the view currently being rendered.</value>
		public int ViewIndex { get; set; }

		/// <summary>
		/// The MIME type of this media object.  Maps to the <see cref="MimeTypeCategory" />
		/// enumeration, so that 0=NotSet, 1=Other, 2=Image, 3=Video, 4=Audio
		/// </summary>
		public int MimeType { get; set; }

		/// <summary>
		/// The type of this gallery item.  Maps to the <see cref="GalleryObjectType" /> enumeration.
		/// </summary>
		public int ItemType { get; set; }

		/// <summary>
		/// Gets or sets the metadata available for this media object.
		/// </summary>
		/// <value>The metadata.</value>
		public MetaItem[] MetaItems { get; set; }
	}
}
