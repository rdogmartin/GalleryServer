namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// A client-optimized object representing a MIME type associated with a file's extension.
  /// </summary>
  public class MimeType
  {
    /// <summary>
    /// Gets or sets the ID of the MIME type.
    /// </summary>
    public int Id;

    /// <summary>
    /// Gets or sets a value indicating whether the MIME type is enabled.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Gets or sets a value indicating whether the MIME type is marked for eventual deletion.
    /// </summary>
    public bool IsDeleted;

    /// <summary>
    /// Gets the file extension this mime type is associated with, including the period (e.g. ".jpg", ".avi").
    /// </summary>
    /// <value>The file extension this mime type is associated with.</value>
    public string Extension;

    /// <summary>
    /// Gets the full mime type. This is the <see cref="Business.Interfaces.IMimeType.MajorType"/> concatenated with the
    /// <see cref="Business.Interfaces.IMimeType.Subtype"/>, with a '/' between them (e.g. image/jpeg, video/quicktime).
    /// </summary>
    /// <value>The full mime type.</value>
    public string FullType;
  }
}