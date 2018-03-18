using System;
using Newtonsoft.Json;

namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// A client-optimized object containing information about the current user.
  /// </summary>
  public class User
  {
    /// <summary>
    /// Gets the logon name of the current user, or null if the current user is anonymous.
    /// </summary>
    /// <value>
    /// The name of the user, or null.
    /// </value>
    public string UserName { get; set; }

    /// <summary>
    /// Indicates whether the current user is authenticated.
    /// </summary>
    /// <value>
    /// <c>true</c> if the current user is authenticated; otherwise, <c>false</c>.
    /// </value>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has permision to add an album to at least one album in the
    /// current gallery.
    /// </summary>
    public bool? CanAddAlbumToAtLeastOneAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has permision to add a media object to at least one album in the
    /// current gallery.
    /// </summary>
    public bool? CanAddMediaToAtLeastOneAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user can edit at least one album in the current gallery.
    /// </summary>
    public bool? CanEditAtLeastOneAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user can edit at least one media asset in the current gallery.
    /// </summary>
    public bool? CanEditAtLeastOneMediaAsset { get; set; }

    /// <summary>
    /// Gets the ID of the user's album, or 0 if user albums are disabled or the current user
    /// is anonymous.
    /// </summary>
    /// <value>
    /// The user album ID, or 0.
    /// </value>
    public int UserAlbumId { get; set; }

    /// <summary>
    /// Gets or sets the gallery ID that is currently in context for the user. For example, this may indicate the gallery
    /// the <see cref="UserAlbumId" /> applies to.
    /// </summary>
    /// <value>The user album gallery ID.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int? GalleryId { get; set; }

    /// <summary>
    /// Gets or sets application-specific information for the membership user. 
    /// </summary>
    /// <value>Application-specific information for the membership user.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Comment { get; set; }

    /// <summary>
    /// Gets or sets the e-mail address for the membership user.
    /// </summary>
    /// <value>The e-mail address for the membership user.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets whether the membership user can be authenticated.
    /// </summary>
    /// <value><c>true</c> if user can be authenticated; otherwise, <c>false</c>.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? IsApproved { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has enabled or disabled her personal album (aka user album).
    /// </summary>
    /// <value>A value indicating whether the user has enabled or disabled her personal album (aka user album).</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? EnableUserAlbum { get; set; }

    /// <summary>
    /// Gets a value indicating whether the membership user is locked out and unable to be validated.
    /// </summary>
    /// <value><c>true</c> if the membership user is locked out and unable to be validated; otherwise, <c>false</c>.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? IsLockedOut { get; set; }

    /// <summary>
    /// Gets the date and time when the user was added to the membership data store.
    /// </summary>
    /// <value>The date and time when the user was added to the membership data store.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime? CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the membership user was last authenticated or accessed the application.
    /// </summary>
    /// <value>The date and time when the membership user was last authenticated or accessed the application.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime? LastActivityDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was last authenticated.
    /// </summary>
    /// <value>The date and time when the user was last authenticated.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Gets the date and time when the membership user's password was last updated.
    /// </summary>
    /// <value>The date and time when the membership user's password was last updated.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime? LastPasswordChangedDate { get; set; }

    /// <summary>
    /// Gets or sets the roles assigned to the user.
    /// </summary>
    /// <value>The roles.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[] Roles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has been persisted to the data store.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? IsNew { get; set; }

    /// <summary>
    /// Gets or sets the password for the user. This is populated *only* when creating a new user in javascript.
    /// Will be empty in all other cases.
    /// </summary>
    /// <value>The password.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a password reset is being requested. This will be <c>false</c> unless
    /// specifically set to <c>true</c> by client code.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? PasswordResetRequested { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to change the password to the value stored in <see cref="Password" />.
    /// This will be <c>false</c> unless specifically set to <c>true</c> by client code.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? PasswordChangeRequested { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to notify the user during a password change. This will be <c>false</c> unless
    /// specifically set to <c>true</c> by client code.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? NotifyUserOnPasswordChange { get; set; }

    /// <summary>
    /// Gets or sets the user's preferred size for viewing a single media asset. Will be <see cref="Business.DisplayObjectType.Unknown" />
    /// when no preference has been selected by user. Currently this property is not sent to the client as part of this class. Instead,
    /// <see cref="Settings.MediaViewSize" /> is set to the user's preference. However, this property is useful when hydrating user
    /// data from an ajax request to persist a new user preference.
    /// </summary>
    /// <value>An instance of <see cref="Business.DisplayObjectType" />.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Business.DisplayObjectType MediaViewSize { get; set; }

    /// <summary>
    /// Gets or sets the user's preference for the type of slide show. Will be <see cref="Business.SlideShowType.NotSet" />
    /// when no preference has been selected by user. Currently this property is not sent to the client as part of this class. Instead,
    /// the user's preference inherits from <see cref="Settings.SlideShowType" />. However, this property is useful when hydrating user
    /// data from an ajax request to persist a new user preference.
    /// </summary>
    /// <value>An instance of <see cref="Business.SlideShowType" />.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Business.SlideShowType SlideShowType { get; set; }

    /// <summary>
    /// Gets or sets the user's preference for whether a slide show loops. Currently this property is not sent to the client as part of this 
    /// class. Instead, the user's preference inherits from <see cref="Settings.SlideShowLoop" />. However, this property is useful when 
    /// hydrating user data from an ajax request to persist a new user preference.
    /// </summary>
    /// <value><c>true</c> when the slide show loops; otherwise <c>false</c>.</value>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool SlideShowLoop { get; set; }
  }
}