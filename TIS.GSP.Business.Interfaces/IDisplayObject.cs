using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a human viewable representation of a gallery object. Examples include the thumbnail, optimized, or full-size version
	/// of an image, the video of a video file, and the content of a document.
	/// </summary>
	public interface IDisplayObject
	{
		/// <summary>
		/// Gets or sets the width of this object, in pixels.
		/// </summary>
		/// <value>The width of this object, in pixels.</value>
		int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the height of this object, in pixels.
		/// </summary>
		/// <value>The height of this object, in pixels.</value>
		int Height
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the file representing this object. Example: sonorandesert.jpg
		/// </summary>
		/// <value>The name of the file representing this object.</value>
		string FileName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the physical path to this object, including the object's name. Example:
		/// C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets\sonorandesert.jpg
		/// </summary>
		/// <value>The physical path to this object, including the object's name.</value>
		string FileNamePhysicalPath
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the physical path to a temporary version of this object. This property can be used as a holding area for
		/// an intermediate file that is created while processing the object, such as when ImageMagick is used to create a JPEG
		/// version of an object that is subsequently used by both the thumbnail and optimized image generators.
		/// Example: C:\Inetpub\wwwroot\galleryserverpro\App_Data\_Temp\sonorandesert.jpg
		/// </summary>
		/// <value>The physical path to a temporary version of this object.</value>
		string TempFilePath
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the MIME type for this display object. The MIME type is determined from the extension of the <see cref="FileName"/> property. Returns a NullMimeType object if the <see cref="FileName"/> property has not been set or a MIME type cannot be determined from the file's extension.
		/// </summary>
		/// <value>The MIME type for this display object.</value>
		IMimeType MimeType
		{
			get;
		}

		/// <summary>
		/// Gets or sets the type of the display object.
		/// </summary>
		/// <value>The type of the display object.</value>
		DisplayObjectType DisplayType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the ID of the media object that contains the file specified in this object. For albums, it refers to the media object used to represent the thumbnail image. For all other objects, it refers to this object's parent ID.
		/// </summary>
		/// <value>The ID of the media object that contains the file specified in this object.</value>
		int MediaObjectId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the object responsible for generating the file this display object points to.
		/// </summary>
		/// <value>The object responsible for generating the file this display object points to.</value>
		IDisplayObjectCreator DisplayObjectCreator
		{
			get;
			set;
		}

    /// <summary>
    /// Gets or sets the file representing this display object. Accessing this property causes the file to be
    /// generated if it does not exist (thumbnail images only; also, for Image instances, will generate the optimized image).
    /// Returns null for external objects (<see cref="ExternalType" /> = MimeTypeCategory.External).
    /// </summary>
    /// <value>The file representing this display object, or null when this instance represents and external object
    /// (<see cref="ExternalType" /> = MimeTypeCategory.External).</value>
    /// <remarks>Throws GalleryServer.Events.CustomExceptions.InvalidMediaObjectException if the file 
    /// is located in a different directory than the directory of this object's containing album.</remarks>
    System.IO.FileInfo FileInfo
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the size of the file, in KB, for this display object.
		/// </summary>
		/// <value>The size of the file, in KB, for this display object.</value>
		int FileSizeKB
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the gallery object this display object applies to.
		/// </summary>
		/// <value>The gallery object this display object applies to.</value>
		IGalleryObject Parent
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the HTML that defines an externally stored media object, such as videos hosted at YouTube. For local
		/// media objects, this property is an empty string.
		/// </summary>
		/// <example> 
		/// For example, for a YouTube video it may look like this:
		/// <code>
		/// <![CDATA[
		///		<object width="425" height="344">
		///			<param name="movie" value="http://www.youtube.com/v/0tNzoCw9xms&hl=en"></param>
		///			<param name="allowFullScreen" value="true"></param>
		///			<embed src="http://www.youtube.com/v/0tNzoCw9xms&hl=en" type="application/x-shockwave-flash" allowfullscreen="true" width="425" height="344"></embed>
		///		</object>]]> 
		/// </code>
		/// </example> 
		/// <value>The HTML that defines an externally stored media object, such as YouTube or Silverlight.net.</value>
		string ExternalHtmlSource
		{
			get; 
			set;
		}

		/// <summary>
		/// Gets or sets the MIME type category for an externally stored media object, such as videos hosted at YouTube or Silverlight.live.com.
		/// This property is not relevant for locally stored media objects.
		/// </summary>
		/// <value>The MIME type category for an externally stored media object.</value>
		MimeTypeCategory ExternalType
		{
			get; 
			set;
		}

		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. No data is persisted to the data
		/// store.
		/// </summary>
		void GenerateAndSaveFile();

		/// <summary>
		/// Gets the width and height of this display object. The value is calculated from the physical file. Returns an empty
		/// <see cref="System.Windows.Size" /> instance if the value cannot be computed or is not applicable to the object
		/// (for example, for audio files and external media objects).
		/// </summary>
		/// <returns><see cref="System.Windows.Size" />.</returns>
		System.Windows.Size GetSize();
	}
}
