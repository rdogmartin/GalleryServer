using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// An object that specifies options for creating <see cref="IGalleryObject" /> instances representing albums existing in the data store.
  /// </summary>
  [System.Diagnostics.DebuggerDisplay("ID = {MediaId}; IsWritable = {IsWritable}")]
  public class MediaLoadOptions
  {
	  /// <summary>
		/// The ID of the media asset to load.
		/// </summary>
		public int MediaId { get; set; }

    /// <summary>
    /// Gets or sets the album that contains the media asset. Specify it when known as this will eliminate having to look up and 
    /// inflate the parent album, thereby improving performance.
    /// </summary>
    public IAlbum Album { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the media asset can persist changes to the data store. Defaults to <c>false</c>.
    /// </summary>
    public bool IsWritable { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLoadOptions" /> class that generates a read-only instance. Set properties 
    /// to desired values if you want non-default behavior.
    /// </summary>
    /// <param name="mediaId">The ID of the media asset.</param>
    public MediaLoadOptions(int mediaId)
    {
      MediaId = mediaId;
    }
  }
}
