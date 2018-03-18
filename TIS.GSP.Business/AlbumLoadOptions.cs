namespace GalleryServer.Business
{
  /// <summary>
  /// An object that specifies options for creating <see cref="Interfaces.IAlbum" /> instances representing albums existing in the data store.
  /// </summary>
  [System.Diagnostics.DebuggerDisplay("ID = {AlbumId}; IsWritable = {IsWritable}; InflateChildObjects = {InflateChildObjects}; AllowMetadataLoading = {AllowMetadataLoading}")]
  public class AlbumLoadOptions
	{
	  /// <summary>
		/// The ID of the album to load.
		/// </summary>
		public int AlbumId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the child albums and media objects should be inflated. Default value is <c>false</c>.
    /// </summary>
    public bool InflateChildObjects { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the metadata for child media objects of this album should be loaded. Defaults to <c>true</c>.
    /// To improve performance, set to <c>false</c> if the metadata is not need. Does not affect the loading of metadata for albums.
    /// </summary>
    public bool AllowMetadataLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the album can persist changes to the data store. Defaults to <c>false</c>.
    /// </summary>
    public bool IsWritable { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumLoadOptions" /> class that generates a read-only instance with metadata
    /// loaded and child objects not inflated. Set properties to desired values if you want non-default behavior.
    /// </summary>
    /// <param name="albumId">The ID of the album.</param>
    public AlbumLoadOptions(int albumId)
    {
      AlbumId = albumId;
    }
  }
}
