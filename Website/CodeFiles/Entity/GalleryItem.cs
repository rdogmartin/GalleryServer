using System;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A simple object that contains gallery item information. It is essentially a client-optimized
	/// version of <see cref="IGalleryObject" />. This class is used to pass information between 
	/// the browser and the web server via AJAX callbacks.
	/// </summary>
	public class GalleryItem
	{
		/// <summary>
		/// The gallery item ID.
		/// </summary>
		public int Id { get; set; }

    /// <summary>
    /// The ID of the album containing this gallery item.
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is an album.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is an album; otherwise, <c>false</c>.
    /// </value>
    public bool IsAlbum { get; set; }

		/// <summary>
		/// The MIME type of this gallery item.  Maps to the <see cref="MimeTypeCategory" />
		/// enumeration, so that 0=NotSet, 1=Other, 2=Image, 3=Video, 4=Audio. Will be NotSet (0)
		/// when the current instance is an album.
		/// </summary>
		public int MimeType { get; set; }

		/// <summary>
		/// The type of this gallery item.  Maps to the <see cref="GalleryObjectType" /> enumeration.
		/// </summary>
		public int ItemType { get; set; }

		/// <summary>
		/// The gallery item title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The gallery item caption.
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// When this instance represents an album, this property indicates the number of child 
		/// albums in this album. Will be zero when this instance is a media item.
		/// </summary>
		public int NumAlbums { get; set; }

		/// <summary>
		/// When this instance represents an album, this property indicates the number of media 
		/// objects in this album. Will be zero when this instance is a media item.
		/// </summary>
		public int NumMediaItems { get; set; }

		/// <summary>
		/// Gets or sets the views available for this gallery item.
		/// </summary>
		/// <value>The views.</value>
		public DisplayObject[] Views { get; set; }

		/// <summary>
		/// Gets or sets the index of the view currently being rendered. This value can be used to get 
		/// or set the desired view to display among the possibilities in <see cref="Views" />.
		/// </summary>
		/// <value>The index of the view currently being rendered.</value>
		public int ViewIndex { get; set; }

		///// <summary>
		///// When this instance represents an album, represents a user-entered beginning date. Will be
		///// null when this instance is a media item.
		///// </summary>
		//[Obsolete("This property has been rendered obsolete in 3.0 and may not be available in future versions.")]
		//public DateTime? DateStart { get; set; }

		///// <summary>
		///// When this instance represents an album, represents a user-entered beginning date. Will be
		///// null when this instance is a media item.
		///// </summary>
		//[Obsolete("This property has been rendered obsolete in 3.0 and may not be available in future versions.")]
		//public DateTime? DateEnd { get; set; }
	}
}
