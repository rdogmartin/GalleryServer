using System.Collections.Generic;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a display object that is suitable for storing in cache.
  /// </summary>
  public class CacheItemDisplayObject
	{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheItemDisplayObject"/> class.
    /// </summary>
    /// <param name="width">The width of this object, in pixels.</param>
    /// <param name="height">The height of this object, in pixels.</param>
    /// <param name="fileName">The name of the file representing this object. Example: sonorandesert.jpg</param>
    /// <param name="fileSizeKb">The size of the file, in KB, for this display object.</param>
    /// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as videos hosted at YouTube.</param>
    /// <param name="externalType">The MIME type category for an externally stored media object. This property is not relevant for locally stored media objects.</param>
    public CacheItemDisplayObject(int width, int height, string fileName, int fileSizeKb, string externalHtmlSource, MimeTypeCategory externalType)
    {
	    Width = width;
	    Height = height;
	    FileName = fileName;
	    FileSizeKB = fileSizeKb;
	    ExternalHtmlSource = externalHtmlSource;
	    ExternalType = externalType;
	  }

	  #endregion

		#region Properties

		/// <summary>
		/// Gets the width of this object, in pixels.
		/// </summary>
		public int Width
		{
			get;
		}

		/// <summary>
		/// Gets the height of this object, in pixels.
		/// </summary>
		public int Height
		{
			get;
		}

		/// <summary>
		/// Gets the name of the file representing this object. Example: sonorandesert.jpg
		/// </summary>
		public string FileName
		{
			get;
		}

		/// <summary>
		/// Gets the size of the file, in KB, for this display object.
		/// </summary>
		public int FileSizeKB
		{
			get;
		}

		/// <summary>
		/// Gets the HTML that defines an externally stored media object, such as videos hosted at YouTube.
		/// </summary>
		public string ExternalHtmlSource
		{
			get;
		}

		/// <summary>
		/// Gets or sets the MIME type category for an externally stored media object. This property is not relevant for locally stored media objects.
		/// </summary>
		public MimeTypeCategory ExternalType
		{
			get;
		}

    #endregion

    #region Methods

    /// <summary>
    /// Creates an array of three of <see cref="CacheItemDisplayObject" /> instances representing the three parameters. This instance is suitable for storing in cache.
    /// </summary>
    /// <param name="thumbnail">The thumbnail display object.</param>
    /// <param name="optimized">The optimized display object.</param>
    /// <param name="original">The original display object.</param>
    /// <returns>A three-item array of <see cref="CacheItemDisplayObject" /> instances.</returns>
    public static CacheItemDisplayObject[] CreateFrom(IDisplayObject thumbnail, IDisplayObject optimized, IDisplayObject original)
	  {
	    return new[]
	    {
	      new CacheItemDisplayObject(thumbnail.Width, thumbnail.Height, thumbnail.FileName, thumbnail.FileSizeKB, thumbnail.ExternalHtmlSource, thumbnail.ExternalType),
	      new CacheItemDisplayObject(optimized.Width, optimized.Height, optimized.FileName, optimized.FileSizeKB, optimized.ExternalHtmlSource, optimized.ExternalType),
	      new CacheItemDisplayObject(original.Width, original.Height, original.FileName, original.FileSizeKB, original.ExternalHtmlSource, original.ExternalType)
      };
	  }

	  #endregion
	}
}
