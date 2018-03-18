
namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Represents a set of properties for a user that are specific to a particular gallery.
  /// </summary>
  public interface IUserGalleryProfile
  {
    /// <summary>
    /// Gets or sets the ID of the gallery the profile properties are associated with.
    /// </summary>
    /// <value>The gallery ID.</value>
    int GalleryId { get; set; }

    /// <summary>
    /// Gets or sets the ID for the user's personal album (aka user album).
    /// </summary>
    /// <value>The ID for the user's personal album (aka user album).</value>
    int UserAlbumId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has enabled or disabled her personal album (aka user album).
    /// </summary>
    /// <value>A value indicating whether the user has enabled or disabled her personal album (aka user album).</value>
    bool EnableUserAlbum { get; set; }

    /// <summary>
    /// Gets or sets the user's preferred size for viewing media assets. This overrides the setting at the gallery control level, 
    /// which in turn overrides the setting at the gallery level.
    /// </summary>
    /// <value>An instance of <see cref="DisplayObjectType" />.</value>
    DisplayObjectType MediaViewSize { get; set; }

    /// <summary>
    /// Gets or sets the user's preferred type of slide show. This overrides the setting at the gallery control level, 
    /// which in turn overrides the setting at the gallery level.
    /// </summary>
    /// <value>An instance of <see cref="SlideShowType" />.</value>
    SlideShowType SlideShowType { get; set; }

    /// <summary>
    /// Gets or sets the user's preference for whether a slide show loops.
    /// </summary>
    /// <value><c>true</c> if the slide show loops; <c>false</c> if no looping; null when user has not selected a preference.</value>
    bool? SlideShowLoop { get; set; }

    /// <summary>
    /// Creates a new instance containing a deep copy of the items it contains.
    /// </summary>
    /// <returns>Returns a new instance containing a deep copy of the items it contains.</returns>
    IUserGalleryProfile Copy();
  }
}