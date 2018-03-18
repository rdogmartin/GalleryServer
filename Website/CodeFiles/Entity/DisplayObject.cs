using System;
using GalleryServer.Business;

namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A client-optimized object that contains information about a particular view of a media object.
	/// </summary>
	public class DisplayObject
	{
		/// <summary>
		/// The size of this display object. Maps to the <see cref="DisplayObjectType" /> enumeration, so that
		/// 0=Unknown, 1=Thumbnail, 2=Optimized, 3=Original, 4=External, etc.
		/// </summary>
		public int ViewSize { get; set; }

		/// <summary>
		/// The type of this display object.  Maps to the <see cref="MimeTypeCategory" /> enumeration, so that
		/// 0=NotSet, 1=Other, 2=Image, 3=Video, 4=Audio
		/// </summary>
		public int ViewType { get; set; }

		/// <summary>
		/// The HTML fragment that renders this media object.
		/// </summary>
		public string HtmlOutput { get; set; }

		/// <summary>
		/// The ECMA script fragment that renders this media object.
		/// </summary>
		public string ScriptOutput { get; set; }

		/// <summary>
		/// The width, in pixels, of this media object.
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// The height, in pixels, of this media object.
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Gets or sets the path to the media object.
		/// </summary>
		public string Url { get; set; }

    /// <summary>
    /// Gets or sets the size of the file, in KB, for this display object.
    /// </summary>
    public long FileSizeKB { get; set; }
  }
}