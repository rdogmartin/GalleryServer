using System;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a set of properties for a user that are specific to a particular gallery.
  /// </summary>
  [Serializable]
  public class UserGalleryProfile : IUserGalleryProfile, IComparable
  {
    #region Private Fields

    private int _galleryId;
    private int _userAlbumId;
    private bool _enableUserAlbum;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UserGalleryProfile" /> class.
    /// </summary>
    public UserGalleryProfile()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserGalleryProfile"/> class.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    public UserGalleryProfile(int galleryId)
    {
      _galleryId = galleryId;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the ID of the gallery the profile properties are associated with.
    /// </summary>
    /// <value>The gallery ID.</value>
    public int GalleryId
    {
      get { return _galleryId; }
      set { _galleryId = value; }
    }

    /// <summary>
    /// Gets or sets the ID for the user's personal album (aka user album).
    /// </summary>
    /// <value>The ID for the user's personal album (aka user album).</value>
    public int UserAlbumId
    {
      get { return _userAlbumId; }
      set { _userAlbumId = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user has enabled or disabled her personal album (aka user album).
    /// </summary>
    /// <value>
    /// A value indicating whether the user has enabled or disabled her personal album (aka user album).
    /// </value>
    public bool EnableUserAlbum
    {
      get { return _enableUserAlbum; }
      set { _enableUserAlbum = value; }
    }

    /// <summary>
    /// Gets or sets the user's preferred size for viewing media assets. This overrides the setting at the gallery control level, 
    /// which in turn overrides the setting at the gallery level.
    /// </summary>
    /// <value>An instance of <see cref="DisplayObjectType" />.</value>
    public DisplayObjectType MediaViewSize { get; set; }

    /// <summary>
    /// Gets or sets the user's preferred type of slide show. This overrides the setting at the gallery control level, 
    /// which in turn overrides the setting at the gallery level.
    /// </summary>
    /// <value>An instance of <see cref="SlideShowType" />.</value>
    public SlideShowType SlideShowType { get; set; }

    /// <summary>
    /// Gets or sets the user's preference for whether a slide show loops.
    /// </summary>
    /// <value><c>true</c> if the slide show loops; <c>false</c> if no looping; null when user has not selected a preference.</value>
    public bool? SlideShowLoop { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a new instance containing a deep copy of the items it contains.
    /// </summary>
    /// <returns>Returns a new instance containing a deep copy of the items it contains.</returns>
    public IUserGalleryProfile Copy()
    {
      IUserGalleryProfile copy = new UserGalleryProfile(GalleryId);

      copy.EnableUserAlbum = this.EnableUserAlbum;
      copy.UserAlbumId = this.UserAlbumId;
      copy.MediaViewSize = MediaViewSize;
      copy.SlideShowType = SlideShowType;
      copy.SlideShowLoop = SlideShowLoop;

      return copy;
    }

    #endregion

    #region IComparable

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
    /// </returns>
    /// <exception cref="T:System.ArgumentException">
    /// 	<paramref name="obj"/> is not the same type as this instance. </exception>
    public int CompareTo(object obj)
    {
      if (obj == null)
        return 1;
      else
      {
        var other = obj as IUserGalleryProfile;
        if (other != null)
          return this.GalleryId.CompareTo(other.GalleryId);
        else
          return 1;
      }
    }

    #endregion
  }
}