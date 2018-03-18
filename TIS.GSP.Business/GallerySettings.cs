using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Represents a set of gallery-specific settings.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Gallery ID = {_galleryId}")]
    public class GallerySettings : IGallerySettings
    {
        #region Private Fields

        private int _galleryId;
        private string _mediaObjectPath;
        private string _thumbnailPath;
        private string _optimizedPath;
        private bool _mediaObjectPathIsReadOnly;
        private bool _showHeader;
        private string _galleryTitle;
        private string _galleryTitleUrl;
        private bool _showLogin;
        private bool _showSearch;
        private bool _showErrorDetails;
        private bool _enableExceptionHandler;
        private int _defaultAlbumDirectoryNameLength;
        private bool _synchAlbumTitleAndDirectoryName;
        private string _emptyAlbumThumbnailBackgroundColor;
        private string _emptyAlbumThumbnailText;
        private string _emptyAlbumThumbnailFontName;
        private int _emptyAlbumThumbnailFontSize;
        private string _emptyAlbumThumbnailFontColor;
        private float _emptyAlbumThumbnailWidthToHeightRatio;
        private int _maxThumbnailTitleDisplayLength;
        private IMetadataDefinitionCollection _metadataDefinitions = new MetadataDefinitionCollection();
        private bool _allowUserEnteredHtml;
        private bool _allowUserEnteredJavascript;
        private string[] _allowedHtmlTags;
        private string[] _allowedHtmlAttributes;
        private bool _allowCopyingReadOnlyObjects;
        private bool _allowManageOwnAccount;
        private bool _allowDeleteOwnAccount;
        private MediaObjectTransitionType _mediaObjectTransitionType;
        private float _mediaObjectTransitionDuration;
        private int _slideshowInterval;
        private bool _allowUnspecifiedMimeTypes;
        private string[] _imageTypesStandardBrowsersCanDisplay;
        private string[] _imageMagickFileTypes;
        private bool _enableAnonymousOriginalMediaObjectDownload;
        private bool _extractMetadata;
        private bool _extractMetadataUsingWpf;
        private bool _enableMediaObjectDownload;
        private bool _enableGalleryObjectZipDownload;
        private bool _enableAlbumZipDownload;
        private bool _enableSlideShow;
        private int _maxThumbnailLength;
        private int _thumbnailImageJpegQuality;
        private string _thumbnailFileNamePrefix;
        private int _maxOptimizedLength;
        private int _optimizedImageJpegQuality;
        private int _optimizedImageTriggerSizeKb;
        private string _optimizedFileNamePrefix;
        private int _originalImageJpegQuality;
        private bool _discardOriginalImageDuringImport;
        private bool _applyWatermark;
        private bool _applyWatermarkToThumbnails;
        private string _watermarkText;
        private string _watermarkTextFontName;
        private int _watermarkTextFontSize;
        private int _watermarkTextWidthPercent;
        private string _watermarkTextColor;
        private int _watermarkTextOpacityPercent;
        private ContentAlignment _watermarkTextLocation;
        private string _watermarkImagePath;
        private int _watermarkImageWidthPercent;
        private int _watermarkImageOpacityPercent;
        private ContentAlignment _watermarkImageLocation;
        private bool _sendEmailOnError;
        private bool _autoStartMediaObject;
        private int _defaultVideoPlayerWidth;
        private int _defaultVideoPlayerHeight;
        private int _defaultAudioPlayerWidth;
        private int _defaultAudioPlayerHeight;
        private int _defaultGenericObjectWidth;
        private int _defaultGenericObjectHeight;
        private int _maxUploadSize;
        private bool _allowAddLocalContent;
        private bool _allowAddExternalContent;
        private bool _allowAnonymousBrowsing;
        private int _pageSize;
        private PagerPosition _pagerLocation;
        private bool _enableSelfRegistration;
        private bool _requireEmailValidationForSelfRegisteredUser;
        private bool _requireApprovalForSelfRegisteredUser;
        private bool _useEmailForAccountName;
        private string[] _defaultRolesForUser;
        private IUserAccountCollection _usersToNotifyWhenAccountIsCreated = new UserAccountCollection();
        private IUserAccountCollection _usersToNotifyWhenErrorOccurs = new UserAccountCollection();
        private bool _enableUserAlbum;
        private bool _enableUserAlbumDefaultForUser;
        private int _userAlbumParentAlbumId;
        private string _userAlbumNameTemplate;
        private string _userAlbumSummaryTemplate;
        private bool _redirectToUserAlbumAfterLogin;
        private int _videoThumbnailPosition;
        private bool _enableAutoSync;
        private int _autoSyncIntervalMinutes;
        private DateTime _lastAutoSync;
        private bool _enableRemoteSync;
        private string _remoteSyncPassword;
        private IMediaEncoderSettingsCollection _mediaEncoderSettings = new MediaEncoderSettingsCollection();
        private int _mediaEncoderTimeoutMs;

        private string _fullMediaObjectPath;
        private string _fullThumbnailPath;
        private string _fullOptimizedPath;

        private bool _isInitialized;
        private readonly bool _isTemplate;
        private bool _isWritable;
        private readonly StringCollection _verifiedFilePaths = new StringCollection();

        #endregion

        #region Constructors

        private GallerySettings(int galleryId, bool isTemplate)
        {
            _galleryId = galleryId;
            _isTemplate = isTemplate;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs immediately after the gallery settings are persisted to the data store.
        /// </summary>
        public static event EventHandler<GallerySettingsEventArgs> GallerySettingsSaved;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the ID for the gallery.
        /// </summary>
        /// <value>The gallery ID.</value>
        public int GalleryId
        {
            get { return _galleryId; }
            set { _galleryId = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the gallery settings have been populated with data for the current gallery.
        /// This library is initialized by calling <see cref="Initialize"/>.
        /// </summary>
        /// <value></value>
        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the gallery settings are the template settings used to populate the settings
        /// of new galleries.
        /// </summary>
        public bool IsTemplate
        {
            get
            {
                return _isTemplate;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current instance can be modified. Objects that are stored in a cache must
        /// be treated as read-only. Only objects that are instantiated right from the database and not shared across threads
        /// should be updated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can be modified; otherwise, <c>false</c>.
        /// </value>
        public bool IsWritable
        {
            get { return _isWritable; }
            set { _isWritable = value; }
        }

        /// <summary>
        /// Gets or sets the media object path. The path may be relative to the root of the web application
        /// (e.g. \gs\mediaobjects), a full path to a local resource (e.g. C:\mymedia), or a UNC path to a local or network
        /// resource (e.g. \\mynas\media). Mapped drives present a security risk and are not supported. The initial and
        /// trailing slashes are	optional. For relative paths, the directory separator character can be either a forward
        /// or backward slash. Use the property <see cref="FullMediaObjectPath"/> to retrieve the full physical path
        /// (such as "C:\inetpub\wwwroot\galleryserverpro\mediaobjects").
        /// </summary>
        /// <value>The media object path.</value>
        /// <remarks>The path is returned exactly how it appears in the configuration setting.</remarks>
        public string MediaObjectPath
        {
            get { return _mediaObjectPath; }
            set { _mediaObjectPath = value; }
        }

        /// <summary>
        /// Gets or sets the path to a directory where Gallery Server stores the thumbnail images of media objects. If
        /// this path is empty, the directory containing the original media object is used to store the thumbnail image.
        /// The path may be relative to the root of the web application (e.g. \gs\mediaobjects), a full path to a local
        /// resource (e.g. C:\mymedia), or a UNC path to a local or network resource (e.g. \\mynas\media). Mapped
        /// drives present a security risk and are not supported. The initial and trailing slashes are	optional.
        /// For relative paths, the directory separator character can be either a forward or backward slash. Use the
        /// property <see cref="FullThumbnailPath"/> to retrieve the full physical path
        /// (such as "C:\inetpub\wwwroot\galleryserverpro\mediaobjects").
        /// </summary>
        /// <value>
        /// The path to a directory where Gallery Server stores the thumbnail images of media objects.
        /// </value>
        public string ThumbnailPath
        {
            get { return _thumbnailPath; }
            set { _thumbnailPath = value; }
        }

        /// <summary>
        /// Gets or sets the path to a directory where Gallery Server stores the optimized images of media objects. If
        /// this path is empty, the directory containing the original media object is used to store the optimized image.
        /// The path may be relative to the root of the web application (e.g. \gs\mediaobjects), a full path to a local
        /// resource (e.g. C:\mymedia), or a UNC path to a local or network resource (e.g. \\mynas\media). Mapped
        /// drives present a security risk and are not supported. The initial and trailing slashes are	optional.
        /// For relative paths, the directory separator character can be either a forward or backward slash.
        /// Not applicable for non-image media objects. Use the property <see cref="FullOptimizedPath"/> to retrieve
        /// the full physical path (such as "C:\inetpub\wwwroot\galleryserverpro\mediaobjects").
        /// </summary>
        /// <value>
        /// The path to a directory where Gallery Server stores the optimized images of media objects.
        /// </value>
        public string OptimizedPath
        {
            get { return _optimizedPath; }
            set { _optimizedPath = value; }
        }

        /// <summary>
        /// Specifies that the directory containing the media objects should never be written to by Gallery Server.
        /// This is useful when configuring the gallery to expose an existing media library and the administrator will not
        /// add, move, or copy objects using the Gallery Server UI. Objects can be added or removed to the gallery
        /// only by the synchronize function. Functions that do not require modifying the original files are still
        /// available, such as editing titles and captions, rearranging items, and the security system. Configuring
        /// a read-only gallery requires setting the thumbnail and optimized paths to a different directory, disabling
        /// user albums (<see cref="EnableUserAlbum"/>), and disabling the album title / directory name synchronization
        /// setting (<see cref="SynchAlbumTitleAndDirectoryName"/>). This class does not enforce these business rules;
        /// validation must be performed by the caller.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the media objects directory is read-only; <c>false</c> if it can be written to.
        /// </value>
        public bool MediaObjectPathIsReadOnly
        {
            get { return _mediaObjectPathIsReadOnly; }
            set { _mediaObjectPathIsReadOnly = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the header at the top of the gallery. The default value is <c>true</c>. 
        /// The header includes the gallery title, login/logout controls, and search function. The login/logout controls 
        /// and search function can be individually controlled via the <see cref="IGallerySettings.ShowLogin" /> and <see cref="IGallerySettings.ShowSearch" /> properties.
        /// When <c>false</c>, the controls within the header are not shown, even if individually they are set to be visible
        /// (e.g. ShowSearch=<c>true</c>, ShowLogin=<c>true</c>).
        /// </summary>
        /// <value><c>true</c> if the header is to be dislayed; otherwise, <c>false</c>.</value>
        public bool ShowHeader
        {
            get { return _showHeader; }
            set { _showHeader = value; }
        }

        /// <summary>
        /// Gets or sets the header text that appears at the top of each web page. Requires that <see cref="ShowHeader"/> be set to
        /// <c>true</c> in order to be visible.
        /// </summary>
        /// <value>The gallery title.</value>
        public string GalleryTitle
        {
            get { return _galleryTitle; }
            set { _galleryTitle = value; }
        }

        /// <summary>
        /// Gets or sets the URL the user will be directed to when she clicks the gallery title. Optional. If not 
        /// present, no link will be rendered. Examples: "http://www.mysite.com", "/" (the root of the web site),
        /// "~/" (the top level album).
        /// </summary>
        /// <value>The gallery title URL.</value>
        public string GalleryTitleUrl
        {
            get { return _galleryTitleUrl; }
            set { _galleryTitleUrl = value; }
        }

        /// <summary>
        /// Indicates whether to show the login controls at the top right of each page. When false, no login controls
        /// are shown, but the user can navigate directly to the login page to log on. Requires that <see cref="ShowHeader"/>
        /// be set to <c>true</c> in order to be visible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if login controls are visible; otherwise, <c>false</c>.
        /// </value>
        public bool ShowLogin
        {
            get { return _showLogin; }
            set { _showLogin = value; }
        }

        /// <summary>
        /// Indicates whether to show the search box at the top right of each page. Requires that <see cref="ShowHeader"/>
        /// be set to <c>true</c> in order to be visible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the search box is visible; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSearch
        {
            get { return _showSearch; }
            set { _showSearch = value; }
        }

        /// <summary>
        /// Indicates whether to show the full details of any unhandled exception that occurs within the gallery. This can reveal
        /// sensitive information to the user, so it should only be used for debugging purposes. When false, a generic error 
        /// message is given to the user. This setting has no effect when enableExceptionHandler="false".
        /// </summary>
        /// <value><c>true</c> if error details are displayed in the browser; <c>false</c> if a generic error message is displayed.</value>
        public bool ShowErrorDetails
        {
            get { return _showErrorDetails; }
            set { _showErrorDetails = value; }
        }

        /// <summary>
        /// Indicates whether to use Gallery Server's internal exception handling mechanism. When true, unhandled exceptions
        /// are transferred to a custom error page and, if showErrorDetails="true", details about the error are displayed to the
        /// user. When false, the error is recorded and the exception is rethrown, allowing application-level error handling to
        /// handle it. This may include code in global.asax. The customErrors element in web.config may be used to manage error
        /// handling when this setting is false (the customErrors setting is ignored when this value is true).
        /// </summary>
        /// <value><c>true</c> if Gallery Server's internal exception handling mechanism manages unhandled exceptions; 
        /// <c>false</c> if unhandled exceptions are allowed to propagate to the parent application, allowing for application
        /// level error handling code to manage the error.</value>
        public bool EnableExceptionHandler
        {
            get { return _enableExceptionHandler; }
            set { _enableExceptionHandler = value; }
        }

        /// <summary>
        /// The maximum length of directory name when a user creates an album. By default, directory names are the same as the
        /// album's title, but are truncated when the title is longer than the value specified here.
        /// </summary>
        public int DefaultAlbumDirectoryNameLength
        {
            get { return _defaultAlbumDirectoryNameLength; }
            set { _defaultAlbumDirectoryNameLength = value; }
        }

        /// <summary>
        /// Indicates whether to update the directory name corresponding to an album when the album's title is changed. When 
        /// true, modifying the title of an album causes the directory name to change to the same value. If the 
        /// title is longer than the value specified in DefaultAlbumDirectoryNameLength, the directory name is truncated. You 
        /// may want to set this to false if you have a directory structure that you do not want Gallery Server to alter. 
        /// Note that even if this setting is false, directories will still be moved or copied when the user moves or copies
        /// an album. Also, Gallery Server always modifies the directory name when it is necessary to 
        /// make it unique within a parent directory. For example, this may happen if you give two sibling albums the same title 
        /// or you move/copy an album into a directory containing another album with the same name.
        /// </summary>
        public bool SynchAlbumTitleAndDirectoryName
        {
            get { return _synchAlbumTitleAndDirectoryName; }
            set { _synchAlbumTitleAndDirectoryName = value; }
        }

        /// <summary>
        /// Gets or sets the metadata property to sort albums by. This value is assigned to the <see cref="IAlbum.SortByMetaName" />
        /// property when an album is created.
        /// </summary>
        /// <value>The metadata property to sort albums by.</value>
        public MetadataItemName DefaultAlbumSortMetaName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an album's default sort order is ascending. A <c>false</c> value indicates
        /// a descending sort.
        /// </summary>
        /// <value><c>true</c> if an album is sorted in ascending order by default; <c>false</c> if descending order.</value>
        public bool DefaultAlbumSortAscending { get; set; }

        /// <summary>
        /// The color used for the background of the GIF image generated by Gallery Server when creating a default
        /// thumbnail image for a newly created album or an album without any objects. The color can be specified as
        /// hex (e.g. #336699), RGB (e.g. 127,55,95), or one of the System.Color.KnownColor enum values (e.g. Maroon).
        /// </summary>
        public string EmptyAlbumThumbnailBackgroundColor
        {
            get { return _emptyAlbumThumbnailBackgroundColor; }
            set { _emptyAlbumThumbnailBackgroundColor = value; }
        }

        /// <summary>
        /// The default text written on the GIF image generated by Gallery Server when creating a default thumbnail image 
        /// for a newly created album or an album without any objects. The GIF is 
        /// dynamically generated by the application when it is needed and is never actually stored on the hard drive.
        /// </summary>
        public string EmptyAlbumThumbnailText
        {
            get { return _emptyAlbumThumbnailText; }
            set { _emptyAlbumThumbnailText = value; }
        }

        /// <summary>
        /// The font used for text written on the GIF image generated by Gallery Server when creating a default
        /// thumbnail image for a newly created album or an album without any objects. The font must be installed on 
        /// the web server. If the font is not installed, a generic sans serif font will be substituted.
        /// </summary>
        public string EmptyAlbumThumbnailFontName
        {
            get { return _emptyAlbumThumbnailFontName; }
            set { _emptyAlbumThumbnailFontName = value; }
        }

        /// <summary>
        /// The size, in pixels, of the font used for text written on the GIF image generated by Gallery Server when 
        /// creating a default thumbnail image for a newly created album or an album without any objects. 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the value to a number outside the acceptable
        /// range of 6 to 100.</exception>
        public int EmptyAlbumThumbnailFontSize
        {
            get { return _emptyAlbumThumbnailFontSize; }
            set
            {
                if (value < 6 || value > 100)
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid EmptyAlbumThumbnailFontSize setting: The value must be between 6 and 100. Instead, the value was {0}.", value));
                }

                _emptyAlbumThumbnailFontSize = value;
            }
        }

        /// <summary>
        /// The color of the text specified in property EmptyAlbumThumbnailText. The color can be specified as
        /// hex (e.g. #336699), RGB (e.g. 127,55,95), or one of the System.Color.KnownColor enum values (e.g. Maroon).
        /// </summary>
        public string EmptyAlbumThumbnailFontColor
        {
            get { return _emptyAlbumThumbnailFontColor; }
            set { _emptyAlbumThumbnailFontColor = value; }
        }

        /// <summary>
        /// The ratio of the width to height of the default thumbnail image for an album that does not have a thumbnail
        /// image specified. The length of the longest side of the image is set by the MaxThumbnailLength property, and the
        /// length of the remaining side is calculated using this ratio. A ratio or more than 1.00 results in the width
        /// being greater than the height (landscape), while a ratio less than 1.00 results in the width being less
        /// than the height (portrait). Example: If MaxThumbnailLength = 115 and EmptyAlbumThumbnailWidthToHeightRatio = 1.50,
        /// the width of the default thumbnail image is 115 and the height is 77 (115 / 1.50). Value must be greater
        /// than zero. 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the value to a number less than or equal
        /// to zero.</exception>
        public float EmptyAlbumThumbnailWidthToHeightRatio
        {
            get { return _emptyAlbumThumbnailWidthToHeightRatio; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid EmptyAlbumThumbnailWidthToHeightRatio setting: The value must be greater than zero. Instead, the value was {0}.", value));
                }

                _emptyAlbumThumbnailWidthToHeightRatio = value;
            }
        }

        /// <summary>
        /// Maximum # of characters to display when showing the title of an album or media object in a thumbnail view.
        /// </summary>
        /// <value>The display length of the max thumbnail title.</value>
        public int MaxThumbnailTitleDisplayLength
        {
            get { return _maxThumbnailTitleDisplayLength; }
            set { _maxThumbnailTitleDisplayLength = value; }
        }

        /// <summary>
        /// Indicates whether HTML is allowed in user-entered text such as titles, captions, and external media objects.
        /// When true, the HTML tags specified in <see cref="IGallerySettings.AllowedHtmlTags"/> and the attributes in
        /// <see cref="IGallerySettings.AllowedHtmlAttributes"/> are allowed. Invalid tags are automatically removed from user
        /// input. This setting does not affect how javascript is treated; refer to <see cref="IGallerySettings.AllowUserEnteredJavascript"/>.
        /// If this value is changed from true to false, existing objects will not be immediately purged of all HTML
        /// tags. Instead, individual titles and captions are stripped of HTML as each object is edited and saved by the user.
        /// </summary>
        public bool AllowUserEnteredHtml
        {
            get { return _allowUserEnteredHtml; }
            set { _allowUserEnteredHtml = value; }
        }

        /// <summary>
        /// Indicates whether javascript is allowed in user-entered text such as titles, captions, and external media 
        /// objects. When false, script tags and the string "javascript:" is automatically removed from all user input.
        /// WARNING: Enabling this option makes the gallery vulnerable to a cross site scripting attack by any user with 
        /// permission to edit captions or upload external media objects.
        /// </summary>
        public bool AllowUserEnteredJavascript
        {
            get { return _allowUserEnteredJavascript; }
            set { _allowUserEnteredJavascript = value; }
        }

        /// <summary>
        /// A list of HTML tags that may be present in titles and captions of albums and media objects.
        /// The attributes that are allowed are specified in <see cref="IGallerySettings.AllowedHtmlAttributes"/>.
        /// Applies only when <see cref="IGallerySettings.AllowUserEnteredHtml"/> is <c>true</c>. Ex: p,a,div,span,...
        /// </summary>
        public string[] AllowedHtmlTags
        {
            get { return _allowedHtmlTags; }
            set { _allowedHtmlTags = ToLowerInvariant(value); }
        }

        /// <summary>
        /// A list of attributes that HTML tags are allowed to have. These attributes, when combined with the
        /// HTML tags in <see cref="IGallerySettings.AllowedHtmlTags"/>, define the HTML that is allowed in titles and captions of 
        /// albums and media objects. Applies only when <see cref="IGallerySettings.AllowUserEnteredHtml"/> is <c>true</c>. Ex: href,class,style,...
        /// </summary>
        public string[] AllowedHtmlAttributes
        {
            get { return _allowedHtmlAttributes; }
            set { _allowedHtmlAttributes = ToLowerInvariant(value); }
        }

        /// <summary>
        /// Indicates whether to allow the copying of objects a user has only view permissions for.
        /// </summary>
        public bool AllowCopyingReadOnlyObjects
        {
            get { return _allowCopyingReadOnlyObjects; }
            set { _allowCopyingReadOnlyObjects = value; }
        }

        /// <summary>
        /// Indicates whether to allow a logged-on user to manage their account. When false, the link to the account page 
        /// at the top right of each page is not shown and if the user navigates directly to the account page, they are redirected away.
        /// </summary>
        /// <value><c>true</c> if a logged-on user can manage their account; otherwise, <c>false</c>.</value>
        public bool AllowManageOwnAccount
        {
            get { return _allowManageOwnAccount; }
            set { _allowManageOwnAccount = value; }
        }

        /// <summary>
        /// Indicates whether a user is allowed to delete his or her own account.
        /// </summary>
        public bool AllowDeleteOwnAccount
        {
            get { return _allowDeleteOwnAccount; }
            set { _allowDeleteOwnAccount = value; }
        }

        /// <summary>
        /// Specifies the visual transition effect to use when moving from one media object to another.
        /// </summary>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when setting the value to an invalid
        /// enumeration.</exception>
        public MediaObjectTransitionType MediaObjectTransitionType
        {
            get { return _mediaObjectTransitionType; }
            set
            {
                if (!MediaObjectTransitionTypeEnumHelper.IsValidMediaObjectTransitionType(value))
                {
                    throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The configuration setting MediaObjectTransitionType is not one of the enum values of the MediaObjectTransitionType enumeration. Valid values are 'None' and 'Fade'. Instead, the value {0} was passed.", value));
                }

                _mediaObjectTransitionType = value;
            }
        }

        /// <summary>
        /// The duration of the transition effect, in seconds, when navigating between media objects. Value must be greater
        /// than zero. This setting has no effect when mediaObjectTransitionType = "None".
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the value to a number less than or equal
        /// to zero.</exception>
        public float MediaObjectTransitionDuration
        {
            get { return _mediaObjectTransitionDuration; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid MediaObjectTransitionDuration setting: The value must be greater than zero. Instead, the value was {0}.", value));
                }

                _mediaObjectTransitionDuration = value;
            }
        }

        /// <summary>
        /// The delay, in milliseconds, between images during a slide show.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the value to a number less than one.</exception>
        public int SlideshowInterval
        {
            get { return _slideshowInterval; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid SlideshowInterval setting: The value must be greater than one. Instead, the value was {0}.", value));
                }

                _slideshowInterval = value;
            }
        }

        /// <summary>
        /// Indicates whether a slide show continues from the beginning after showing the last media asset. When <c>false</c>, the user is
        /// redirected to the album page when the slide show ends.
        /// </summary>
        public bool SlideShowLoop { get; set; }

        /// <summary>
        /// Indicates whether to allow users to upload file types not explicitly specified in the mimeTypes configuration
        /// section. When false, any file with an extension not listed in the mimeTypes section is rejected. When true,
        /// Gallery Server accepts all file types regardless of their file extension.
        /// </summary>
        public bool AllowUnspecifiedMimeTypes
        {
            get { return _allowUnspecifiedMimeTypes; }
            set { _allowUnspecifiedMimeTypes = value; }
        }

        /// <summary>
        /// A comma-delimited list of file extensions, including the period, indicating types of images that a standard browser can display. When
        /// the user requests an original image (high resolution), the original is sent to the browser in an &lt;img&gt; HTML tag
        /// if its extension is one of those listed here.  If not, the user is presented with a message containing instructions
        /// for downloading the image file. Typically this setting should not be changed. Ex: .jpg,.jpeg,.gif,.png
        /// </summary>
        public string[] ImageTypesStandardBrowsersCanDisplay
        {
            get { return _imageTypesStandardBrowsersCanDisplay; }
            set { _imageTypesStandardBrowsersCanDisplay = ToLowerInvariant(value); }
        }

        /// <summary>
        /// A comma-delimited list of file extensions, including the period, indicating types of files that can be processed
        /// by ImageMagick. Gallery Server uses ImageMagick to extract images from files that cannot be processed by .NET.
        /// Ex: .pdf,.txt,.eps,.psd
        /// </summary>
        public string[] ImageMagickFileTypes
        {
            get { return _imageMagickFileTypes; }
            set { _imageMagickFileTypes = ToLowerInvariant(value); }
        }

        /// <summary>
        /// Specifies whether anonymous users are allowed to rate gallery objects.
        /// </summary>
        /// <value><c>true</c> if anonymous rating is allowed; otherwise, <c>false</c>.</value>
        public bool AllowAnonymousRating { get; set; }

        /// <summary>
        /// Specifies whether Gallery Server extracts metadata from image files. If the attribute
        /// <see cref="IGallerySettings.ExtractMetadataUsingWpf" /> is true, then additional metadata such as title, keywords,
        ///  and rating is extracted.
        /// </summary>
        public bool ExtractMetadata
        {
            get { return _extractMetadata; }
            set { _extractMetadata = value; }
        }

        /// <summary>
        /// Specifies whether metadata is extracted from image files using Windows Presentation Foundation (WPF) classes
        /// in .NET Framework 3.0 and higher. The WPF classes allow additional metadata to be extracted beyond those allowed by the
        /// .NET Framework 2.0, such as title, keywords, and rating. This attribute has no effect unless the following
        /// requirements are met: <see cref="ExtractMetadataUsingWpf"/> = true; .NET Framework 3.0 or higher is installed on the web
        /// server; and the web application is running in Full Trust. The WPF classes have exhibited some reliability issues
        /// during development, most notably causing the IIS worker process (w3wp.exe) to increase in memory usage and
        /// eventually crash during uploads and synchronizations. For this reason one may want to disable this feature
        /// until a .NET Framework service pack or future version provides better performance.
        /// </summary>
        /// <value></value>
        public bool ExtractMetadataUsingWpf
        {
            get { return _extractMetadataUsingWpf; }
            set { _extractMetadataUsingWpf = value; }
        }

        /// <summary>
        /// Gets or sets the metadata settings that define how metadata items are displayed to the user.
        /// </summary>
        /// <value>The metadata display options.</value>
        public IMetadataDefinitionCollection MetadataDisplaySettings
        {
            get { return _metadataDefinitions; }
            set { _metadataDefinitions = value; }
        }

        /// <summary>
        /// Gets or sets the format string to use for <see cref="DateTime" /> metadata values. The date type of each meta item
        /// is specified by the <see cref="IMetadataDefinition.DataType" /> property.
        /// </summary>
        /// <value>The metadata date time format string.</value>
        public string MetadataDateTimeFormatString { get; set; }

        /// <summary>
        /// Specifies whether Gallery Server renders user interface objects to allow a user to download the file for a media 
        /// object. Note that setting this value to false does not prevent a user from downloading a
        /// media object, since a user already has access to the media object if he or she can view it in the browser. To
        /// prevent certain users from viewing media objects (and thus downloading them), use private albums, disable
        /// anonymous viewing, or configure security to prevent users from viewing the objects.
        /// </summary>
        public bool EnableMediaObjectDownload
        {
            get { return _enableMediaObjectDownload; }
            set { _enableMediaObjectDownload = value; }
        }

        /// <summary>
        /// Specifies whether anonymous users are allowed to view the original versions of media objects. When no
        /// compressed (optimized) version exists, the user is allowed to view the original, regardless of this
        /// setting. This setting has no effect on logged on users.
        /// </summary>
        public bool EnableAnonymousOriginalMediaObjectDownload
        {
            get { return _enableAnonymousOriginalMediaObjectDownload; }
            set { _enableAnonymousOriginalMediaObjectDownload = value; }
        }

        /// <summary>
        /// Specifies whether users are allowed to download media objects and albums in a ZIP file. Downloading of albums can be
        /// restricted by setting <see cref="EnableAlbumZipDownload"/> to <c>false</c>.
        /// </summary>
        /// <value></value>
        public bool EnableGalleryObjectZipDownload
        {
            get { return _enableGalleryObjectZipDownload; }
            set { _enableGalleryObjectZipDownload = value; }
        }

        /// <summary>
        /// Specifies whether users are allowed to download albums in a ZIP file. This setting <see cref="EnableGalleryObjectZipDownload"/>
        /// must be enabled for this setting to take effect. In other words, albums can be downloaded only when
        /// <see cref="EnableGalleryObjectZipDownload"/> and <see cref="EnableAlbumZipDownload"/> are both enabled.
        /// </summary>
        /// <value></value>
        public bool EnableAlbumZipDownload
        {
            get { return _enableAlbumZipDownload; }
            set { _enableAlbumZipDownload = value; }
        }

        /// <summary>
        /// Specifies whether slide show functionality is enabled. When true, a start/pause slideshow button is displayed in the 
        /// toolbar that appears above a media object. The length of time each image is shown before automatically moving
        /// to the next one is controlled by the SlideshowInterval setting. Note that only images are shown during a slide
        /// show; other objects such as videos, audio files, and documents are skipped.
        /// </summary>
        public bool EnableSlideShow
        {
            get { return _enableSlideShow; }
            set { _enableSlideShow = value; }
        }

        /// <summary>
        /// Gets or sets the size of media assets to display when viewing a single media asset. The default value is <see cref="DisplayObjectType.Optimized" />.
        /// </summary>
        public DisplayObjectType MediaViewSize { get; set; }

        /// <summary>
        /// Gets or sets the type of the slide show. The default value is <see cref="Business.SlideShowType.FullScreen" />.
        /// </summary>
        /// <value>The type of the slide show.</value>
        public SlideShowType SlideShowType { get; set; }

        /// <summary>
        ///	The length (in pixels) of the longest edge of a thumbnail image.  This value is used when a thumbnail 
        ///	image is created. The length of the shorter side is calculated automatically based on the aspect ratio of the image.
        /// The value must be between 10 and 100,000.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting the value to a number outside of the valid range
        /// of 10 and 100,000.</exception>
        public int MaxThumbnailLength
        {
            get { return _maxThumbnailLength; }
            set
            {
                if ((value < 10) || (value > 100000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid MaxThumbnailLength setting: The value must be between 10 and 100,000. Instead, the value was {0}.", value));
                }

                _maxThumbnailLength = value;
            }
        }

        /// <summary>
        /// The quality level that thumbnail images are stored at (0 - 100).
        /// </summary>
        public int ThumbnailImageJpegQuality
        {
            get { return _thumbnailImageJpegQuality; }
            set
            {
                if ((value < 1) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid ThumbnailImageJpegQuality setting: The value must be between 1 and 100. Instead, the value was {0}.", value));
                }

                _thumbnailImageJpegQuality = value;
            }
        }

        /// <summary>
        /// The string that is prepended to the thumbnail filename for each media object. For example, if an image
        /// named puppy.jpg is added, and this setting is "zThumb_", the thumbnail image will be named 
        /// "zThumb_puppy.jpg".	NOTE: Any file named "zThumb_puppy.jpg" that already exists will be overwritten, 
        /// so it is important to choose a value that, when prepended to media object filenames, will not 
        /// conflict with existing media objects.
        /// </summary>
        public string ThumbnailFileNamePrefix
        {
            get { return _thumbnailFileNamePrefix; }
            set { _thumbnailFileNamePrefix = value; }
        }

        /// <summary>
        ///	The length (in pixels) of the longest edge of an optimized image.  This value is used when an optimized
        ///	image is created. The length of the shorter side is calculated automatically based on the aspect ratio of the image.
        /// </summary>
        public int MaxOptimizedLength
        {
            get { return _maxOptimizedLength; }
            set
            {
                if ((value < 10) || (value > 100000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid MaxOptimizedLength setting: The value must be between 10 and 100,000. Instead, the value was {0}.", value));
                }

                _maxOptimizedLength = value;
            }
        }

        /// <summary>
        /// The quality level that optimized JPG pictures are created with. This is a number from 1 - 100, with 1 
        /// being the worst quality and 100 being the best quality. Not applicable for non-image media objects.
        /// </summary>
        public int OptimizedImageJpegQuality
        {
            get { return _optimizedImageJpegQuality; }
            set
            {
                if ((value < 1) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid OptimizedImageJpegQuality setting: The value must be between 1 and 100. Instead, the value was {0}.", value));
                }

                _optimizedImageJpegQuality = value;
            }
        }

        /// <summary>
        /// The size (in KB) above which an image is compressed to create an optimized version.
        /// Not applicable for non-image media objects.
        /// </summary>
        public int OptimizedImageTriggerSizeKb
        {
            get { return _optimizedImageTriggerSizeKb; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid OptimizedImageTriggerSizeKb setting: The value must be greater than or equal to zero. Instead, the value was {0}.", value));
                }

                _optimizedImageTriggerSizeKb = value;
            }
        }

        /// <summary>
        /// The string that is prepended to the optimized filename for images. This setting is only used for image
        /// media objects where an optimized image file is created. For example, if an image named
        /// puppy.jpg is added, and this setting is "zOpt_", the optimized image will be named "zOpt_puppy.jpg".
        /// NOTE: Any file named "zOpt_puppy.jpg" that already exists will be overwritten, 
        /// so it is important to choose a value that, when prepended to media object filenames, will not 
        /// conflict with existing media objects.
        /// </summary>
        public string OptimizedFileNamePrefix
        {
            get { return _optimizedFileNamePrefix; }
            set { _optimizedFileNamePrefix = value; }
        }

        /// <summary>
        /// The quality level that original JPG pictures are saved at. This is only used when the original is 
        /// modified by the user, such as rotation. Not applicable for non-image media objects.
        /// </summary>
        public int OriginalImageJpegQuality
        {
            get { return _originalImageJpegQuality; }
            set
            {
                if ((value < 1) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid OriginalImageJpegQuality setting: The value must be between 1 and 100. Instead, the value was {0}.", value));
                }

                _originalImageJpegQuality = value;
            }
        }

        /// <summary>
        /// Specifies whether to discard the original image when it is added to the gallery. This option, when enabled, 
        /// helps reduce disk space usage. This option applies only to images, and only when they are added through an 
        /// upload or by synchronizing. Changing this setting does not affect existing media objects. When false, 
        /// users still have the option to discard the original image on the Add Objects page by unchecking the 
        /// corresponding checkbox.
        /// </summary>
        public bool DiscardOriginalImageDuringImport
        {
            get { return _discardOriginalImageDuringImport; }
            set { _discardOriginalImageDuringImport = value; }
        }

        /// <summary>
        /// Specifies whether to apply a watermark to optimized and original images. If true, the text in the watermarkText
        /// property is applied to images, and the image specified in watermarkImagePath is overlayed on the image. If
        /// watermarkText is empty, or if watermarkImagePath is empty or does not refer to a valid image, that watermark
        /// is not applied. If applyWatermarkToThumbnails = true, then the watermark is also applied to thumbnails.
        /// </summary>
        public bool ApplyWatermark
        {
            get { return _applyWatermark; }
            set { _applyWatermark = value; }
        }

        /// <summary>
        /// Specifies whether to apply the text and/or image watermark to thumbnail images. This property is ignored if 
        /// applyWatermark = false.
        /// </summary>
        public bool ApplyWatermarkToThumbnails
        {
            get { return _applyWatermarkToThumbnails; }
            set { _applyWatermarkToThumbnails = value; }
        }

        /// <summary>
        /// Specifies the text to apply to images in the gallery. The text is applied in a single line.
        /// </summary>
        public string WatermarkText
        {
            get { return _watermarkText; }
            set { _watermarkText = value; }
        }

        /// <summary>
        /// The font used for the watermark text. If the font is not installed on the web server, a generic font will 
        /// be substituted.
        /// </summary>
        public string WatermarkTextFontName
        {
            get { return _watermarkTextFontName; }
            set { _watermarkTextFontName = value; }
        }

        /// <summary>
        /// Gets or sets the height, in pixels, of the watermark text. This value is ignored if the property
        /// WatermarkTextWidthPercent is non-zero. Valid values are 0 - 10000.
        /// </summary>
        public int WatermarkTextFontSize
        {
            get { return _watermarkTextFontSize; }
            set
            {
                if ((value < 0) || (value > 10000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid WatermarkTextFontSize setting: The value must be between 0 and 10000. Instead, the value was {0}.", value));
                }

                _watermarkTextFontSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the percent of the overall width of the recipient image that should be covered with the
        /// watermark text. The size of the text is automatically scaled up or down to achieve the desired width. For example,
        /// a value of 50 means the text is 50% as wide as the recipient image. Valid values are 0 - 100. The text is never
        /// rendered in a font smaller than 6 pixels, so in cases of long text it may stretch wider than the percentage
        /// specified in this setting.
        /// A value of 0 turns off this feature and causes the text size to be determined by the 
        /// WatermarkTextFontSize property.
        /// </summary>
        public int WatermarkTextWidthPercent
        {
            get { return _watermarkTextWidthPercent; }
            set
            {
                if ((value < 0) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid WatermarkTextWidthPercent setting: The value must be between 0 and 100. Instead, the value was {0}.", value));
                }

                _watermarkTextWidthPercent = value;
            }
        }

        /// <summary>
        /// Specifies the color of the watermark text. The color can be specified as hex (e.g. #336699), RGB (e.g. 127,55,95),
        /// or one of the System.Color.KnownColor enum values (e.g. Maroon).
        /// </summary>
        public string WatermarkTextColor
        {
            get { return _watermarkTextColor; }
            set { _watermarkTextColor = value; }
        }

        /// <summary>
        /// The opacity of the watermark text. This is a value from 0 to 100, with 0 being invisible and 100 being solid, 
        /// with no transparency.
        /// </summary>
        public int WatermarkTextOpacityPercent
        {
            get { return _watermarkTextOpacityPercent; }
            set
            {
                if ((value < 0) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid WatermarkTextOpacityPercent setting: The value must be between 0 and 100. Instead, the value was {0}.", value));
                }

                _watermarkTextOpacityPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the location for the watermark text on the recipient image. This value maps to the 
        /// enumeration System.Drawing.ContentAlignment, and must be one of the following nine values:
        /// TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight.
        /// </summary>
        public ContentAlignment WatermarkTextLocation
        {
            get { return _watermarkTextLocation; }
            set
            {
                if (!ContentAlignmentEnumHelper.IsValidContentAlignment(value))
                {
                    throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The configuration setting WatermarkTextLocation is not one of the enum values of the System.Drawing.ContentAlignment enumeration. Valid values are 'BottomCenter', 'BottomLeft', 'BottomRight', 'MiddleCenter', 'MiddleLeft', 'MiddleRight', 'TopCenter', 'TopLeft', 'TopRight'. Instead, the value {0} was passed.", value));
                }

                _watermarkTextLocation = value;
            }
        }

        /// <summary>
        /// Gets or sets the filename of a watermark image to be applied to the recipient image. The image
        /// must be in a format that allows it to be instantiated in a <see cref="System.Drawing.Bitmap" /> object.
        /// Typically that means it should be jpg, gif, png, bmp, or tif. Ex: "logo.png"
        /// </summary>
        /// <remarks>The name implies the property stores a path rather than a name. That's because it used to store 
        /// a path in versions earlier than 4.2.0. We kept the name the same for backward compatibility and simplicity, 
        /// even though it is a bit misleading.</remarks>
        public string WatermarkImagePath
        {
            get { return _watermarkImagePath; }
            set { _watermarkImagePath = value; }
        }

        /// <summary>
        /// Gets or sets the percent of the overall width of the recipient image that should be covered with the
        /// watermark image. The size of the image is automatically scaled to achieve the desired width. For example,
        /// a value of 50 means the watermark image is 50% as wide as the recipient image. Valid values are 0 - 100.
        /// A value of 0 turns off this feature and causes the image to be rendered its actual size.
        /// </summary>
        public int WatermarkImageWidthPercent
        {
            get { return _watermarkImageWidthPercent; }
            set
            {
                if ((value < 0) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid WatermarkImageWidthPercent setting: The value must be between 0 and 100. Instead, the value was {0}.", value));
                }

                _watermarkImageWidthPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the opacity of the watermark image. Valid values are 0 - 100, with 0 being completely
        /// transparent and 100 completely opaque.
        /// </summary>
        public int WatermarkImageOpacityPercent
        {
            get { return _watermarkImageOpacityPercent; }
            set
            {
                if ((value < 0) || (value > 100))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid WatermarkImageOpacityPercent setting: The value must be between 0 and 100. Instead, the value was {0}.", value));
                }

                _watermarkImageOpacityPercent = value;
            }
        }

        /// <summary>
        /// Gets or sets the location for the watermark image on the recipient image. This value maps to the 
        /// enumeration System.Drawing.ContentAlignment, and must be one of the following nine values:
        /// TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight.
        /// </summary>
        public ContentAlignment WatermarkImageLocation
        {
            get { return _watermarkImageLocation; }
            set
            {
                if (!ContentAlignmentEnumHelper.IsValidContentAlignment(value))
                {
                    throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The configuration setting WatermarkImageLocation is not one of the enum values of the System.Drawing.ContentAlignment enumeration. Valid values are 'BottomCenter', 'BottomLeft', 'BottomRight', 'MiddleCenter', 'MiddleLeft', 'MiddleRight', 'TopCenter', 'TopLeft', 'TopRight'. Instead, the value {0} was passed.", value));
                }

                _watermarkImageLocation = value;
            }
        }

        /// <summary>
        /// Specifies whether the Gallery Server administrator (specified in EmailToName/EmailToAddress)
        /// is sent a report when a web site error occurs.  A valid SMTP server must be specified if this
        /// is set to true (attribute SmtpServer).
        /// </summary>
        public bool SendEmailOnError
        {
            get { return _sendEmailOnError; }
            set { _sendEmailOnError = value; }
        }

        /// <summary>
        /// Indicates whether a video, audio or other dynamic object will automatically start playing in the user's browser.
        /// </summary>
        public bool AutoStartMediaObject
        {
            get { return _autoStartMediaObject; }
            set { _autoStartMediaObject = value; }
        }

        /// <summary>
        /// Indicates the default width, in pixels, of the browser object that plays a video file. Typically 
        /// this refers to the &lt;object&gt; tag that contains the video, resulting in a tag similar to this:
        /// &lt;object style="width:640px;height:480px;" ... &gt;
        /// </summary>
        public int DefaultVideoPlayerWidth
        {
            get { return _defaultVideoPlayerWidth; }
            set
            {
                if ((value < 0) || (value > 10000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid DefaultVideoPlayerWidth setting: The value must be between 0 and 10,000. Instead, the value was {0}.", value));
                }

                _defaultVideoPlayerWidth = value;
            }
        }

        /// <summary>
        /// Indicates the default height, in pixels, of the browser object that plays a video file. Typically 
        /// this refers to the &lt;object&gt; tag that contains the video, resulting in a tag similar to this:
        /// &lt;object style="width:640px;height:480px;" ... &gt;
        /// </summary>
        public int DefaultVideoPlayerHeight
        {
            get { return _defaultVideoPlayerHeight; }
            set
            {
                if ((value < 0) || (value > 10000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid DefaultVideoPlayerHeight setting: The value must be between 0 and 10,000. Instead, the value was {0}.", value));
                }

                _defaultVideoPlayerHeight = value;
            }
        }

        /// <summary>
        /// Indicates the default width, in pixels, of the browser object that plays an audio file. Typically 
        /// this refers to the &lt;object&gt; tag that contains the audio file, resulting in a tag similar to this:
        /// &lt;object style="width:300px;height:200px;" ... &gt;
        /// </summary>
        public int DefaultAudioPlayerWidth
        {
            get { return _defaultAudioPlayerWidth; }
            set
            {
                if ((value < 0) || (value > 10000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid DefaultAudioPlayerWidth setting: The value must be between 0 and 10,000. Instead, the value was {0}.", value));
                }

                _defaultAudioPlayerWidth = value;
            }
        }

        /// <summary>
        /// Indicates the default height, in pixels, of the browser object that plays an audio file. Typically 
        /// this refers to the &lt;object&gt; tag that contains the audio file, resulting in a tag similar to this:
        /// &lt;object style="width:300px;height:200px;" ... &gt;
        /// </summary>
        public int DefaultAudioPlayerHeight
        {
            get { return _defaultAudioPlayerHeight; }
            set
            {
                if ((value < 0) || (value > 10000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid DefaultAudioPlayerHeight setting: The value must be between 0 and 10,000. Instead, the value was {0}.", value));
                }

                _defaultAudioPlayerHeight = value;
            }
        }

        /// <summary>
        /// Indicates the default width, in pixels, of the browser object that displays a generic media object.
        /// A generic media object is defined as any media object that is not an image,	audio, or video file. This
        /// includes Shockwave Flash, Adobe Reader, text files, Word documents and others. The value specified here
        /// is sent to the browser as the width for the object element containing this media object, resulting in syntax 
        /// similar to this: &lt;object style="width:640px;height:480px;" ... &gt; This setting applies only to objects 
        /// rendered within the browser, such as Shockwave Flash. Objects sent to the browser via a download
        /// link, such as text files, PDF files, and Word documents, ignore this setting.
        /// </summary>
        public int DefaultGenericObjectWidth
        {
            get { return _defaultGenericObjectWidth; }
            set
            {
                if ((value < 0) || (value > 10000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid DefaultGenericObjectWidth setting: The value must be between 0 and 10,000. Instead, the value was {0}.", value));
                }

                _defaultGenericObjectWidth = value;
            }
        }

        /// <summary>
        /// Indicates the default height, in pixels, of the browser object that displays a generic media object.
        /// A generic media object is defined as any media object that is not an image,	audio, or video file. This
        /// includes Shockwave Flash, Adobe Reader, text files, Word documents and others. The value specified here
        /// is sent to the browser as the width for the object element containing this media object, resulting in syntax 
        /// similar to this: &lt;object style="width:640px;height:480px;" ... &gt; This setting applies only to objects 
        /// rendered within the browser, such as Shockwave Flash. Objects sent to the browser via a download
        /// link, such as text files, PDF files, and Word documents, ignore this setting.
        /// </summary>
        public int DefaultGenericObjectHeight
        {
            get { return _defaultGenericObjectHeight; }
            set
            {
                if ((value < 0) || (value > 10000))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid DefaultGenericObjectHeight setting: The value must be between 0 and 10,000. Instead, the value was {0}.", value));
                }

                _defaultGenericObjectHeight = value;
            }
        }

        /// <summary>
        /// Indicates the maximum size, in kilobytes, of the files that can be uploaded.
        /// Use this setting to keep users from uploading very large files and to help guard against Denial of 
        /// Service (DOS) attacks. A value of zero (0) indicates there is no restriction on upload size (unlimited).
        /// This setting is not used during synchronization.
        /// </summary>
        public int MaxUploadSize
        {
            get { return _maxUploadSize; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid MaxUploadSize setting: The value must be between 0 and {0}. Instead, the value was {1}.", Int32.MaxValue, value));
                }

                _maxUploadSize = value;
            }
        }

        /// <summary>
        /// Indicates whether a user can upload a physical file to the gallery, such as an image or video file stored
        /// on a local hard drive. The user must also be authenticated and a member of a role with AllowAddMediaObject 
        /// or AllowAdministerSite permission. This setting is not used during synchronization.
        /// </summary>
        public bool AllowAddLocalContent
        {
            get { return _allowAddLocalContent; }
            set { _allowAddLocalContent = value; }
        }

        /// <summary>
        /// Indicates whether a user can add a link to external content, such as a YouTube video, to the gallery. 
        /// The user must also be authenticated and a member of a role with AllowAddMediaObject 
        /// or AllowAdministerSite permission. This setting is not used during synchronization.
        /// </summary>
        public bool AllowAddExternalContent
        {
            get { return _allowAddExternalContent; }
            set { _allowAddExternalContent = value; }
        }

        /// <summary>
        /// Indicates whether users can view galleries without logging in. When false, users are redirected to a login
        /// page when any album is requested. Private albums are never shown to anonymous users, even when this 
        /// property is true.
        /// </summary>
        public bool AllowAnonymousBrowsing
        {
            get { return _allowAnonymousBrowsing; }
            set { _allowAnonymousBrowsing = value; }
        }

        /// <summary>
        /// Indicates the number of objects to display at a time. For example, if an album has more than this number of
        /// gallery objects, paging controls appear to assist the user in navigating to them. A value of zero disables 
        /// the paging feature.
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid PageSize setting: The value must be between 0 and {0}. Instead, the value was {1}.", Int32.MaxValue, value));
                }

                _pageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the location for the pager used to navigate large collections of objects. This value maps to the 
        /// enumeration <see cref="PagerPosition" />, and must be one of the following values:
        /// Top, Bottom, TopAndBottom. This value is ignored when paging is disabled (<see cref="IGallerySettings.PageSize"/> = 0).
        /// </summary>
        public PagerPosition PagerLocation
        {
            get { return _pagerLocation; }
            set
            {
                if (!PagerPositionEnumHelper.IsValidPagerPosition(value))
                {
                    throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The configuration setting PagerLocation is not one of the enum values of the PagerPosition enumeration. Valid values are 'Top', 'Bottom', and 'TopAndBottom'. Instead, the value {0} was passed.", value));
                }

                _pagerLocation = value;
            }
        }

        /// <summary>
        /// Indicates whether anonymous users are allowed to create accounts.
        /// </summary>
        public bool EnableSelfRegistration
        {
            get { return _enableSelfRegistration; }
            set { _enableSelfRegistration = value; }
        }

        /// <summary>
        /// Indicates whether e-mail verification is required when a user registers an account. When true, the account is 
        /// initially disabled and an email is sent to the user with a verification link. When clicked, user is approved 
        /// and logged on, unless <see cref="IGallerySettings.RequireApprovalForSelfRegisteredUser"/> is enabled, in which case an administrator
        /// must approve the account before the user can log on. Setting this to true reduces spam activity and guarantees that 
        /// a valid e-mail address is associated with the user. When the setting is false, an e-mail address is not required 
        /// and the user account is immediately created. This setting is ignored when 
        /// <see cref="IGallerySettings.EnableSelfRegistration">self registration</see> is disabled.
        /// </summary>
        public bool RequireEmailValidationForSelfRegisteredUser
        {
            get { return _requireEmailValidationForSelfRegisteredUser; }
            set { _requireEmailValidationForSelfRegisteredUser = value; }
        }

        /// <summary>
        /// Indicates whether an administrator must approve newly created accounts before the user can log on. When true, 
        /// the account is disabled until it is approved by an administrator. When a user registers an account, an e-mail
        /// is sent to each user specified in <see cref="IGallerySettings.UsersToNotifyWhenAccountIsCreated"/>. Only users belonging to a
        /// role with AllowAdministerSite permission can approve a user. If <see cref="IGallerySettings.RequireEmailValidationForSelfRegisteredUser"/>
        /// is enabled, the e-mail requesting administrator approval is not sent until the user verifies the e-mail address.
        /// This setting is ignored when <see cref="IGallerySettings.EnableSelfRegistration">self registration</see> is disabled.
        /// </summary>
        public bool RequireApprovalForSelfRegisteredUser
        {
            get { return _requireApprovalForSelfRegisteredUser; }
            set { _requireApprovalForSelfRegisteredUser = value; }
        }

        /// <summary>
        /// Indicates whether account names are primarily e-mail addresses. When true, certain forms, such as the self registration
        /// wizard, assume e-mail addresses are used as account names. For example, when this value is false, the self registration
        /// wizard includes fields for both an account name and an e-mail address, but when true it only requests an e-mail address.
        /// This setting is ignored when <see cref="IGallerySettings.EnableSelfRegistration">self registration</see> is disabled.
        /// </summary>
        public bool UseEmailForAccountName
        {
            get { return _useEmailForAccountName; }
            set { _useEmailForAccountName = value; }
        }

        /// <summary>
        /// A list of roles that apply to all users. When the property is updated, the private properties <see cref="DefaultRolesForUserAdded" />
        /// and <see cref="DefaultRolesForUserRemoved" /> are updated with the added/removed roles.
        /// </summary>
        public string[] DefaultRolesForUser
        {
            get { return _defaultRolesForUser; }
            set
            {
                if (_defaultRolesForUser != null)
                {
                    DefaultRolesForUserAdded = (value == null ? null : value.Where(r => !_defaultRolesForUser.Contains(r)).ToArray());
                    DefaultRolesForUserRemoved = _defaultRolesForUser.Where(r => value == null || !value.Contains(r)).ToArray();
                }

                _defaultRolesForUser = value;
            }
        }

        /// <summary>
        /// Gets or sets the default roles that have been added to the <see cref="DefaultRolesForUser" /> property since it was initially 
        /// populated from the data store. The value can be used in the <see cref="GallerySettingsSaved" /> event to take action on the 
        /// setting change. This property is reset to null at the end of the <see cref="Save(bool)" /> method.
        /// </summary>
        private string[] DefaultRolesForUserAdded { get; set; }

        /// <summary>
        /// Gets or sets the default roles that have been removed from the <see cref="DefaultRolesForUser" /> property since it was initially 
        /// populated from the data store. The value can be used in the <see cref="GallerySettingsSaved" /> event to take action on the 
        /// setting change. This property is reset to null at the end of the <see cref="Save(bool)" /> method.
        /// </summary>
        private string[] DefaultRolesForUserRemoved { get; set; }

        /// <summary>
        /// A list of account names of users to receive an e-mail notification when an account is created.
        /// When <see cref="RequireEmailValidationForSelfRegisteredUser"/> is enabled, the e-mail is not sent until the
        /// user verifies the e-mail address. Applies whether an account is self-created or created by an administrator.
        /// </summary>
        public IUserAccountCollection UsersToNotifyWhenAccountIsCreated
        {
            get { return _usersToNotifyWhenAccountIsCreated; }
            set { _usersToNotifyWhenAccountIsCreated = value; }
        }

        /// <summary>
        /// A list of account names of users to receive an e-mail notification when an application error occurs.
        /// </summary>
        public IUserAccountCollection UsersToNotifyWhenErrorOccurs
        {
            get { return _usersToNotifyWhenErrorOccurs; }
            set { _usersToNotifyWhenErrorOccurs = value; }
        }

        /// <summary>
        /// Indicates whether each user is associated owner to a unique album. The title of the album is based on the 
        /// template in the <see cref="IGallerySettings.UserAlbumNameTemplate"/> property. The album is created when the account is created or
        /// if the album does not exist when the user logs on. It is created in the album specified in the 
        /// <see cref="IGallerySettings.UserAlbumParentAlbumId"/> property.</summary>
        public bool EnableUserAlbum
        {
            get { return _enableUserAlbum; }
            set { _enableUserAlbum = value; }
        }

        /// <summary>
        /// Indicates whether a user album is automatically created for a user the first time he or she logs on. This setting
        /// is used to seed the user's <see cref="IUserGalleryProfile.EnableUserAlbum" /> profile setting when it is created.
        /// This property applies only when <see cref="IGallerySettings.EnableUserAlbum" /> is <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a user album is created for each user the first time he or she logs on; otherwise, <c>false</c>.
        /// </value>
        public bool EnableUserAlbumDefaultForUser
        {
            get { return _enableUserAlbumDefaultForUser; }
            set { _enableUserAlbumDefaultForUser = value; }
        }

        /// <summary>
        /// Specifies the ID of the album containing user albums. This setting is ignored when <see cref="IGallerySettings.EnableUserAlbum"/>
        /// is false. This property may have a value of zero (0) when user albums are disabled.
        /// </summary>
        public int UserAlbumParentAlbumId
        {
            get { return _userAlbumParentAlbumId; }
            set { _userAlbumParentAlbumId = value; }
        }

        /// <summary>
        /// Specifies the template to use for naming the album that is created for new users. Applies only when 
        /// <see cref="IGallerySettings.EnableUserAlbum"/> is true. The placeholder string {UserName}, if present, is replaced 
        /// by the account name.
        /// </summary>
        public string UserAlbumNameTemplate
        {
            get { return _userAlbumNameTemplate; }
            set { _userAlbumNameTemplate = value; }
        }

        /// <summary>
        /// Specifies the template to use for the album summary of a newly created user album. Applies only when 
        /// <see cref="IGallerySettings.EnableUserAlbum"/> is true. No placeholder strings are supported.
        /// </summary>
        public string UserAlbumSummaryTemplate
        {
            get { return _userAlbumSummaryTemplate; }
            set { _userAlbumSummaryTemplate = value; }
        }

        /// <summary>
        /// Indicates whether to redirect the user to his or her album after logging in. If set to false, the current page is
        /// re-loaded or, if there isn't a page, the user is shown the top level album for which the user has view access. This setting 
        /// is ignored when <see cref="IGallerySettings.EnableUserAlbum"/> is false.</summary>
        public bool RedirectToUserAlbumAfterLogin
        {
            get { return _redirectToUserAlbumAfterLogin; }
            set { _redirectToUserAlbumAfterLogin = value; }
        }

        /// <summary>
        /// Gets or sets the position in the video where the thumbnail is generated from. The value is in seconds, so a value
        /// of three indicates the thumbnail for the video is generated from a frame three seconds into the video. The value must be 
        /// between 0 and 86,400 seconds.
        /// </summary>
        /// <value>The position, in seconds, in the video where the thumbnail image is generated from.</value>
        public int VideoThumbnailPosition
        {
            get { return _videoThumbnailPosition; }
            set
            {
                if ((value < 0) || (value > 86400))
                {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, "Invalid VideoThumbnailPosition setting: The value must be between 0 and 86400 (24 hours). Instead, the value was {0}.", value));
                }

                _videoThumbnailPosition = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically synchronize the current gallery on a periodic basis. The interval
        /// is defined in the <see cref="IGallerySettings.AutoSyncIntervalMinutes" /> property. The auto sync depends on periodic browser requests by 
        /// users to trigger the logic to check whether a sync is needed.
        /// </summary>
        /// <value><c>true</c> if auto sync is enabled; otherwise, <c>false</c>.</value>
        public bool EnableAutoSync
        {
            get { return _enableAutoSync; }
            set { _enableAutoSync = value; }
        }

        /// <summary>
        /// Gets or sets the minimum interval, in minutes, that an auto-synchronization is to occur. Since the auto sync feature 
        /// requires periodic browser requests, the actual interval may be longer for infrequently accessed galleries.
        /// </summary>
        /// <value>The auto sync interval, in minutes.</value>
        public int AutoSyncIntervalMinutes
        {
            get { return _autoSyncIntervalMinutes; }
            set { _autoSyncIntervalMinutes = value; }
        }

        /// <summary>
        /// Gets or sets the date/time of the last auto-sync. Value is <see cref="DateTime.MinValue" /> when <see cref="IGallerySettings.EnableAutoSync" />
        /// is disabled or when no auto-sync has yet been performed.
        /// </summary>
        /// <value>The date/time of the last auto-sync.</value>
        public DateTime LastAutoSync
        {
            get { return _lastAutoSync; }
            set { _lastAutoSync = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow external calls to the synchronize web service.
        /// </summary>
        /// <value><c>true</c> if a synchronization operation can be initiated through a web service; otherwise, <c>false</c>.</value>
        public bool EnableRemoteSync
        {
            get { return _enableRemoteSync; }
            set { _enableRemoteSync = value; }
        }

        /// <summary>
        /// Gets or sets the password that is passed to the remote synchronization web service methods. This password prevents
        /// malicious users from starting unauthorized synchronizations.
        /// </summary>
        /// <value>The remote sync password.</value>
        public string RemoteAccessPassword
        {
            get { return _remoteSyncPassword; }
            set { _remoteSyncPassword = value; }
        }

        /// <summary>
        /// Gets or sets the media encoder settings that define how media files may be encoded.
        /// </summary>
        /// <value>An instance that implements <see cref="IMediaEncoderSettingsCollection" />.</value>
        public IMediaEncoderSettingsCollection MediaEncoderSettings
        {
            get { return _mediaEncoderSettings; }
            set { _mediaEncoderSettings = value; }
        }

        /// <summary>
        /// Gets or sets the timeout setting, in milliseconds, for the media encoder function.
        /// </summary>
        /// <value>An integer</value>
        public int MediaEncoderTimeoutMs
        {
            get { return _mediaEncoderTimeoutMs; }
            set { _mediaEncoderTimeoutMs = value; }
        }

        /// <summary>
        /// Gets the full physical path to the directory containing the media objects. Example:
        /// "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting this property to a null or empty string.</exception>
        public string FullMediaObjectPath
        {
            get { return _fullMediaObjectPath; }
            private set
            {
                // Validate the path. Will throw an exception if a problem is found.
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (!_verifiedFilePaths.Contains(value))
                {
                    if (MediaObjectPathIsReadOnly)
                        HelperFunctions.ValidatePhysicalPathExistsAndIsReadable(value);
                    else
                    {
                        HelperFunctions.ValidatePhysicalPathExistsAndIsReadWritable(value);
                        _verifiedFilePaths.Add(value);
                    }
                }

                _fullMediaObjectPath = value;
            }
        }

        /// <summary>
        /// Gets the full physical path to the directory where Gallery Server stores the thumbnail images of media objects.
        /// If no directory is specified in the configuration setting, this returns the main media object path (that is, returns
        /// the same value as the <see cref="FullMediaObjectPath"/> property).
        /// Example: "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"
        /// </summary>
        /// <value>The full physical path to the directory where Gallery Server stores the thumbnail images of media objects.</value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting this property to a null or empty string.</exception>
        public string FullThumbnailPath
        {
            get { return _fullThumbnailPath; }
            private set
            {
                // Validate the path. Will throw an exception if a problem is found.
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (!_verifiedFilePaths.Contains(value))
                {
                    HelperFunctions.ValidatePhysicalPathExistsAndIsReadWritable(value);
                    _verifiedFilePaths.Add(value);
                }

                _fullThumbnailPath = value;
            }
        }

        /// <summary>
        /// Gets the full physical path to the directory where Gallery Server stores the optimized images of media objects.
        /// If no directory is specified in the configuration setting, this returns the main media object path (that is, returns
        /// the same value as the <see cref="FullMediaObjectPath"/> property).
        /// Example: "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"
        /// </summary>
        /// <value>The full physical path to the directory where Gallery Server stores the optimized images of media objects.</value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting this property to a null or empty string.</exception>
        public string FullOptimizedPath
        {
            get { return _fullOptimizedPath; }
            private set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentOutOfRangeException("value");

                // Validate the path. Will throw an exception if a problem is found.
                if (!_verifiedFilePaths.Contains(value))
                {
                    HelperFunctions.ValidatePhysicalPathExistsAndIsReadWritable(value);
                    _verifiedFilePaths.Add(value);
                }

                _fullOptimizedPath = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Perform any initialization tasks that must be performed before the object can be used by the application.
        /// This should be called after the core properties from the data store have been assigned.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when this method is called more than once during 
        /// the application's lifetime.</exception>
        public void Initialize()
        {
            #region Validation

            if (_isInitialized)
            {
                throw new InvalidOperationException("The GallerySetting instance has already been initialized. It cannot be initialized more than once.");
            }

            #endregion

            string mediaObjectPath = MediaObjectPath;
            string thumbnailPath = (String.IsNullOrEmpty(_thumbnailPath) ? mediaObjectPath : _thumbnailPath);
            string optimizedPath = (String.IsNullOrEmpty(_optimizedPath) ? mediaObjectPath : _optimizedPath);

            if (_mediaObjectPathIsReadOnly)
                ValidateReadOnlyGallery(mediaObjectPath, thumbnailPath, optimizedPath);

            // Calculate and verify a few file paths, but only for "real" gallery settings, not the template one.
            if (!IsTemplate)
            {
                // Setting the FullMediaObjectPath property will throw an exception if the directory does not exist or is not writable.
                string physicalAppPath = AppSetting.Instance.PhysicalApplicationPath;

                FullMediaObjectPath = HelperFunctions.CalculateFullPath(physicalAppPath, mediaObjectPath);

                // The property setter for the FullThumbnailPath and FullOptimizedPath properties will throw an exception if the directory 
                // does not exist or is not writable.
                FullThumbnailPath = HelperFunctions.CalculateFullPath(physicalAppPath, thumbnailPath);
                FullOptimizedPath = HelperFunctions.CalculateFullPath(physicalAppPath, optimizedPath);
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Persist the current gallery settings to the data store. Automatically clears and then reloads the gallery settings
        /// from the data store.
        /// </summary>
        /// <overload>
        /// Persist the current gallery settings to the data store.
        /// </overload>
        public void Save()
        {
            Save(true);
        }

        /// <summary>
        /// Persist the current gallery settings to the data store, optionally modifying the default behavior of clearing
        /// and then reloading the gallery settings from the data store.
        /// </summary>
        /// <param name="forceReloadFromDataStore">If set to <c>true</c>, clear the gallery settings stored in memory, which will
        /// force loading them from the data store. Setting this to <c>false</c> can be useful when updating a simple property that
        /// does not require a complex recalculation (like, say the <see cref="UsersToNotifyWhenErrorOccurs"/> does). It may also
        /// be needed when a separate thread is persisting the data and no instance of HttpContext exists, which can cause an
        /// exception in the DotNetNuke module during the reload process in the web layer.</param>
        public void Save(bool forceReloadFromDataStore)
        {
            ValidateSave();

            //Factory.GetDataProvider().GallerySetting_Save(this);
            using (var repo = new GallerySettingRepository())
            {
                repo.Save(this);
            }

            // Clear the settings stored in static variables so they are retrieved from the data store during the next access.
            IGallerySettingsCollection gallerySettings = Factory.LoadGallerySettings();

            if (forceReloadFromDataStore)
            {
                lock (gallerySettings)
                {
                    gallerySettings.Clear();
                }
            }

            // Invoke the GallerySettingsSaved event. This will be implemented in the web layer, which will finish populating any 
            // properties that can't be done here, such as those of type <see cref="IUserAccountCollection" /> (since they need
            // access to the Membership provider, which the business layer has no knowledge of).
            EventHandler<GallerySettingsEventArgs> gallerySaved = GallerySettingsSaved;
            if (gallerySaved != null)
            {
                gallerySaved(null, new GallerySettingsEventArgs(this.GalleryId, DefaultRolesForUserAdded, DefaultRolesForUserRemoved));
            }

            DefaultRolesForUserAdded = null;
            DefaultRolesForUserRemoved = null;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Verifies the current instance can be saved. Throws a <see cref="BusinessException" /> if it cannot.
        /// </summary>
        /// <exception cref="BusinessException">Thrown when the current instance cannot be saved.</exception>
        private void ValidateSave()
        {
            if (!IsWritable)
            {
                throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "This gallery setting container (Gallery ID {0}, {1}) is not updateable.", this.GalleryId, this.GetType()));
            }
        }

        private void ValidateReadOnlyGallery(string mediaObjectPath, string thumbnailPath, string optimizedPath)
        {
            // When a gallery is read only, the following must be true:
            // 1. The thumbnail and optimized path must be different than the media object path.
            // 2. The SynchAlbumTitleAndDirectoryName setting must be false.
            // 3. The EnableUserAlbum setting must be false.
            if ((mediaObjectPath.Equals(thumbnailPath, StringComparison.OrdinalIgnoreCase)) ||
                    (mediaObjectPath.Equals(optimizedPath, StringComparison.OrdinalIgnoreCase)))
            {
                throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Invalid configuration. A read-only gallery requires that the thumbnail and optimized file paths be different than the original media objects path. mediaObjectPath={0}; thumbnailPath={1}; optimizedPath={2}", mediaObjectPath, thumbnailPath, optimizedPath));
            }

            if (SynchAlbumTitleAndDirectoryName)
            {
                throw new BusinessException("Invalid configuration. A read-only gallery requires that the automatic renaming of directory names be disabled. Set this property on the Media Objects - General page in the Site admin area, or update it directly in the database (it is the SynchAlbumTitleAndDirectoryName property in the gallery settings table).");
            }

            if (EnableUserAlbum)
            {
                throw new BusinessException("Invalid configuration. A read-only gallery requires that user albums be disabled. Set this property on the Media Objects - General page in the Site admin area, or update it directly in the database (it is the EnableUserAlbum property in the gallery settings table).");
            }
        }

        /// <summary>
        /// Retrieves the gallery settings from the data store for all galleries.
        /// </summary>
        /// <returns>Returns an <see cref="IGallerySettingsCollection" /> containing the settings for all galleries.</returns>
        internal static IGallerySettingsCollection RetrieveGallerySettingsFromDataStore()
        {
            IGallerySettingsCollection gallerySettings = new GallerySettingsCollection();
            IGallerySettings gs = null;
            int? prevGalleryId = null;


            // Loop through each gallery setting and assign to the relevant property. When we encounter a record with a new gallery ID, 
            // automatically create a new GallerySetting instance and start populating that one. When we are done with the loop we will
            // have created one GallerySetting instance for each gallery and fully populated each one.

            // SQL:
            //SELECT
            //  GallerySettingId, FKGalleryId, IsTemplate, SettingName, SettingValue
            //FROM [gs_GallerySetting]
            //WHERE FKGalleryId = @GalleryId
            //ORDER BY FKGalleryId;
            //foreach (GallerySettingDto gallerySettingDto in Factory.GetDataProvider().GallerySetting_GetGallerySettings())
            using (var repo = new GallerySettingRepository())
            {
                foreach (GallerySettingDto gallerySettingDto in repo.All.OrderBy(s => s.FKGalleryId))
                {
                    #region Check for new gallery

                    if (!prevGalleryId.HasValue || (gallerySettingDto.FKGalleryId != prevGalleryId))
                    {
                        // We have encountered settings for a new gallery. Initialize the previous one, then create a new object and add it to our collection.
                        if ((gs != null) && (!gs.IsInitialized))
                        {
                            gs.Initialize();
                        }

                        gs = new GallerySettings(gallerySettingDto.FKGalleryId, new GalleryRepository().Where(g => g.GalleryId == gallerySettingDto.FKGalleryId).Select(g => g.IsTemplate).First());

                        gallerySettings.Add(gs);

                        prevGalleryId = gallerySettingDto.FKGalleryId;
                    }

                    #endregion

                    #region Assign property

                    UpdateGallerySettingFromDto(gallerySettingDto, gs);

                    #endregion
                }
            }

            // The last gallery setting will not be initialized by the previous loop, so when we finish processing the records and
            // get to this point, do one more initialization. It is expected that gs will never be null or initialized, but we
            // check anyway just to be safe.
            if ((gs != null) && (!gs.IsInitialized))
            {
                gs.Initialize();
            }

            return gallerySettings;
        }

        internal static IGallerySettings RetrieveGallerySettingsFromDataStore(int galleryId)
        {
            var gs = new GallerySettings(galleryId, new GalleryRepository().Where(g => g.GalleryId == galleryId).Select(g => g.IsTemplate).First());
            using (var repo = new GallerySettingRepository())
            {
                foreach (var gallerySettingDto in repo.Where(s => s.FKGalleryId == galleryId))
                {
                    UpdateGallerySettingFromDto(gallerySettingDto, gs);
                }
            }

            return gs;
        }

        private static void UpdateGallerySettingFromDto(GallerySettingDto gallerySettingDto, IGallerySettings gs)
        {
            Type gsType = typeof(GallerySettings);

            string boolType = typeof(bool).ToString();
            string intType = typeof(int).ToString();
            string stringType = typeof(string).ToString();
            string stringArrayType = typeof(string[]).ToString();
            string floatType = typeof(float).ToString();
            string dateTimeType = typeof(DateTime).ToString();

            // For each setting in the data store, find the matching property and assign the value to it.
            string settingName = gallerySettingDto.SettingName.Trim();

            PropertyInfo prop = gsType.GetProperty(settingName);

            if (prop == null)
            {
                throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture, "Invalid gallery setting. A gallery setting named '{0}' was found in the data store, but no property by that name exists in the class '{1}'. Check the gallery settings in the data store to ensure they are correct.", settingName, gsType));
            }

            if ((prop.PropertyType.FullName == null))
            {
                return;
            }

            if (prop.PropertyType.FullName.Equals(boolType))
            {
                prop.SetValue(gs, Convert.ToBoolean(gallerySettingDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
            }
            else if (prop.PropertyType.FullName.Equals(stringType))
            {
                prop.SetValue(gs, Convert.ToString(gallerySettingDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
            }
            else if (prop.PropertyType.FullName.Equals(intType))
            {
                prop.SetValue(gs, Convert.ToInt32(gallerySettingDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
            }
            else if (prop.PropertyType.FullName.Equals(floatType))
            {
                prop.SetValue(gs, Convert.ToSingle(gallerySettingDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
            }
            else if (prop.PropertyType.FullName.Equals(dateTimeType))
            {
                prop.SetValue(gs, HelperFunctions.ToDateTime(gallerySettingDto.SettingValue.Trim(), "O", CultureInfo.InvariantCulture), null);
            }
            else if (prop.PropertyType.FullName.Equals(stringArrayType))
            {
                // Parse comma-delimited string to array
                string[] strings = gallerySettingDto.SettingValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                List<String> stringList = new List<string>(strings.Length);

                // Trim any leading and trailing spaces
                for (int i = 0; i < strings.Length; i++)
                {
                    string stringValue = strings[i].Trim();

                    if (!String.IsNullOrEmpty(stringValue))
                    {
                        stringList.Add(stringValue);
                    }
                }

                prop.SetValue(gs, stringList.ToArray(), null);
            }
            else if (prop.PropertyType == typeof(MetadataItemName))
            {
                AssignMetadataItemNameProperty(gs, prop, gallerySettingDto.SettingValue.Trim());
            }
            else if (prop.PropertyType == typeof(MediaObjectTransitionType))
            {
                AssignMediaObjectTransitionTypeProperty(gs, prop, gallerySettingDto.SettingValue.Trim());
            }
            else if (prop.PropertyType == typeof(DisplayObjectType))
            {
                AssignDisplayObjectTypeTypeProperty(gs, prop, gallerySettingDto.SettingValue.Trim());
            }
            else if (prop.PropertyType == typeof(SlideShowType))
            {
                AssignSlideShowTypeProperty(gs, prop, gallerySettingDto.SettingValue.Trim());
            }
            else if (prop.PropertyType == typeof(ContentAlignment))
            {
                AssignContentAlignmentProperty(gs, prop, gallerySettingDto.SettingValue.Trim());
            }
            else if (prop.PropertyType == typeof(PagerPosition))
            {
                AssignPagerPositionProperty(gs, prop, gallerySettingDto.SettingValue.Trim());
            }
            else if (prop.PropertyType == typeof(IUserAccountCollection))
            {
                AssignUserAccountsProperty(gs, prop, gallerySettingDto.SettingValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
            else if (prop.PropertyType == typeof(IMetadataDefinitionCollection))
            {
                AssignMetadataDisplaySettingsProperty(gs, prop, gallerySettingDto.SettingValue.Trim());
            }
            else if (prop.PropertyType == typeof(IMediaEncoderSettingsCollection))
            {
                AssignMediaEncoderSettingsProperty(gs, prop, gallerySettingDto.SettingValue.Split(new[] { "~~" }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySettings.RetrieveGallerySettingsFromDataStore is not designed to process a property of type {0} (encountered in GallerySettings.{1})", prop.PropertyType, prop.Name));
            }
        }

        private static void AssignUserAccountsProperty(IGallerySettings gallerySetting, PropertyInfo property, string[] userNames)
        {
            IUserAccountCollection userAccounts = (IUserAccountCollection)property.GetValue(gallerySetting, null);

            foreach (string userName in userNames)
            {
                if (!String.IsNullOrEmpty(userName.Trim()))
                {
                    userAccounts.Add(new UserAccount(userName.Trim()));
                }
            }
        }

        private static void AssignMetadataDisplaySettingsProperty(IGallerySettings gallerySetting, PropertyInfo property, string metadataString)
        {
            var metaItemsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MetadataDefinition>>(metadataString);

            IMetadataDefinitionCollection metadataItems = (IMetadataDefinitionCollection)property.GetValue(gallerySetting, null);

            foreach (var mi in metaItemsList)
            {
                metadataItems.Add(mi);
            }
            //property.SetValue(gallerySetting, metadataItems, null);

            //IMetadataDefinitionCollection metadataItems = (IMetadataDefinitionCollection)property.GetValue(gallerySetting, null);
            //int seq = 0;

            //foreach (string nameValuePair in metadata)
            //{
            //	// Each string item is colon-delimited, with the first item being the numerical value of the MetadataItemName
            //	// enumeration, and the second value being a character 'T' or 'F' indicating whether the metadata item is visible.
            //	string[] nameOrValue = nameValuePair.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);

            //	if (nameOrValue.Length != 2)
            //	{
            //		throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySetting.RetrieveGallerySettingsFromDataStore cannot parse the metadata definitions for property {0}. Encountered invalid string: '{1}'", property.Name, nameValuePair));
            //	}

            //	bool isVisible = (nameOrValue[1].Trim() == "T" ? true : false);

            //	int metadataDefInt;
            //	if (Int32.TryParse(nameOrValue[0], out metadataDefInt))
            //	{
            //		if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName((MetadataItemName)metadataDefInt))
            //		{
            //			metadataItems.Add(new MetadataDefinition((MetadataItemName)metadataDefInt, isVisible, seq, gallerySetting.GalleryId));

            //			seq += 1;
            //		}
            //		else
            //		{
            //			throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The integer {0} does not map to a known value of {1}. Details: The function MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName() returned false when evaluating this value. If the enumeration definition has recently changed, this function must be updated to include the change. The MetadataDisplaySettings property in the gallery settings table may need to be manually updated to remove references to this invalid enumeration value.", metadataDefInt, typeof(MetadataItemName)));
            //		}
            //	}
            //	else
            //	{
            //		throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySetting.RetrieveGallerySettingsFromDataStore cannot parse the metadata definitions for property {0}. Encountered invalid string: '{1}'", property.Name, nameValuePair));
            //	}
            //}

            metadataItems.Validate();
        }

        private static void AssignMediaEncoderSettingsProperty(IGallerySettings gallerySetting, PropertyInfo property, string[] mediaEncodings)
        {
            var mediaEncoderSettings = property.GetValue(gallerySetting, null) as IMediaEncoderSettingsCollection ?? new MediaEncoderSettingsCollection();

            int seq = 0;

            foreach (string mediaEncStr in mediaEncodings)
            {
                // Each string item is double-pipe-delimited. Ex: ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}"
                string[] mediaEncoderItems = mediaEncStr.Split(new[] { "||" }, 3, StringSplitOptions.None);

                if (mediaEncoderItems.Length != 3)
                {
                    throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySetting.RetrieveGallerySettingsFromDataStore cannot parse the media encoder definitions for property {0}. Encountered invalid string: '{1}'", property.Name, mediaEncStr));
                }

                mediaEncoderSettings.Add(new MediaEncoderSettings(mediaEncoderItems[0], mediaEncoderItems[1], mediaEncoderItems[2], seq));
                seq++;
            }

            mediaEncoderSettings.Validate();
        }

        /// <summary>
        /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="gallerySetting" />
        /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
        /// </summary>
        /// <param name="gallerySetting">The gallery setting instance containing the <paramref name="property" /> to assign.</param>
        /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
        /// <param name="value">The value to assign to the <paramref name="property" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
        /// <see cref="MetadataItemName" /> value.</exception>
        private static void AssignMetadataItemNameProperty(IGallerySettings gallerySetting, PropertyInfo property, string value)
        {
            MetadataItemName metaName;

            try
            {
                metaName = (MetadataItemName)Enum.Parse(typeof(MetadataItemName), value, true);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySetting.RetrieveGallerySettingsFromDataStore cannot convert the string {0} to a MetadataItemName enumeration value.", value), ex);
            }

            property.SetValue(gallerySetting, metaName, null);
        }

        /// <summary>
        /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="gallerySetting" />
        /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
        /// </summary>
        /// <param name="gallerySetting">The gallery setting instance containing the <paramref name="property" /> to assign.</param>
        /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
        /// <param name="value">The value to assign to the <paramref name="property" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
        /// <see cref="MediaObjectTransitionType" /> value.</exception>
        private static void AssignMediaObjectTransitionTypeProperty(IGallerySettings gallerySetting, PropertyInfo property, string value)
        {
            MediaObjectTransitionType transitionType;

            try
            {
                transitionType = (MediaObjectTransitionType)Enum.Parse(typeof(MediaObjectTransitionType), value, true);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySetting.RetrieveGallerySettingsFromDataStore cannot convert the string {0} to a MediaObjectTransitionType enumeration value. The following values are valid: None, Fade", value), ex);
            }

            property.SetValue(gallerySetting, transitionType, null);
        }

        /// <summary>
        /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="gallerySetting" />
        /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
        /// </summary>
        /// <param name="gallerySetting">The gallery setting instance containing the <paramref name="property" /> to assign.</param>
        /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
        /// <param name="value">The value to assign to the <paramref name="property" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
        /// <see cref="DisplayObjectType" /> value.</exception>
        private static void AssignDisplayObjectTypeTypeProperty(IGallerySettings gallerySetting, PropertyInfo property, string value)
        {
            DisplayObjectType displayType;

            try
            {
                displayType = (DisplayObjectType)Enum.Parse(typeof(DisplayObjectType), value, true);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySettings.AssignDisplayObjectTypeTypeProperty cannot convert the string {0} to a DisplayObjectType enumeration value. The following values are valid: Unknown, Thumbnail, Optimized, Original, External", value), ex);
            }

            property.SetValue(gallerySetting, displayType, null);
        }

        /// <summary>
        /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="gallerySetting" />
        /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
        /// </summary>
        /// <param name="gallerySetting">The gallery setting instance containing the <paramref name="property" /> to assign.</param>
        /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
        /// <param name="value">The value to assign to the <paramref name="property" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
        /// <see cref="SlideShowType" /> value.</exception>
        private static void AssignSlideShowTypeProperty(IGallerySettings gallerySetting, PropertyInfo property, string value)
        {
            SlideShowType ssType;

            try
            {
                ssType = (SlideShowType)Enum.Parse(typeof(SlideShowType), value, true);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySettings.AssignSlideShowTypeProperty cannot convert the string {0} to a SlideShowType enumeration value. The following values are valid: NotSet, Inline, FullScreen", value), ex);
            }

            property.SetValue(gallerySetting, ssType, null);
        }

        /// <summary>
        /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="gallerySetting" />
        /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
        /// </summary>
        /// <param name="gallerySetting">The gallery setting instance containing the <paramref name="property" /> to assign.</param>
        /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
        /// <param name="value">The value to assign to the <paramref name="property" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
        /// <see cref="ContentAlignment" /> value.</exception>
        private static void AssignContentAlignmentProperty(IGallerySettings gallerySetting, PropertyInfo property, string value)
        {
            ContentAlignment contentAlignment;

            try
            {
                contentAlignment = (ContentAlignment)Enum.Parse(typeof(ContentAlignment), value, true);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySetting.RetrieveGallerySettingsFromDataStore cannot convert the string {0} to a ContentAlignment enumeration value. The following values are valid: TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight", value), ex);
            }

            property.SetValue(gallerySetting, contentAlignment, null);
        }

        /// <summary>
        /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="gallerySetting" />
        /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
        /// </summary>
        /// <param name="gallerySetting">The gallery setting instance containing the <paramref name="property" /> to assign.</param>
        /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
        /// <param name="value">The value to assign to the <paramref name="property" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
        /// <see cref="PagerPosition" /> value.</exception>
        private static void AssignPagerPositionProperty(IGallerySettings gallerySetting, PropertyInfo property, string value)
        {
            PagerPosition pagerPosition;

            try
            {
                pagerPosition = (PagerPosition)Enum.Parse(typeof(PagerPosition), value, true);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GallerySetting.RetrieveGallerySettingsFromDataStore cannot convert the string {0} to a PagerPosition enumeration value. The following values are valid: Top, Bottom, TopAndBottom", value), ex);
            }

            property.SetValue(gallerySetting, pagerPosition, null);
        }

        /// <summary>
        /// Converts each string in <paramref name="array" /> to a lower case invariant version of the original.
        /// </summary>
        /// <param name="array">An array of strings.</param>
        /// <returns>Returns the <paramref name="array" /> with each element converted to a lower case invariant.</returns>
        private static string[] ToLowerInvariant(string[] array)
        {
            return Array.ConvertAll(array, s => s.ToLowerInvariant());
        }

        #endregion
    }

    /// <summary>
    /// Provides data for the events relating to <see cref="IGallerySettings" />.
    /// </summary>
    public class GallerySettingsEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GallerySettingsEventArgs" /> class.
        /// </summary>
        /// <param name="galleryId">The gallery ID for the gallery related to the gallery settings.</param>
        /// <param name="defaultRolesForUserAdded">The roles that have been added to the <see cref="IGallerySettings.DefaultRolesForUser" />
        /// property since it was initially populated from the data store.</param>
        /// <param name="defaultRolesForUserRemoved">The roles that have been removed from the <see cref="IGallerySettings.DefaultRolesForUser" />
        /// property since it was initially populated from the data store.</param>
        public GallerySettingsEventArgs(int galleryId, string[] defaultRolesForUserAdded, string[] defaultRolesForUserRemoved)
        {
            GalleryId = galleryId;
            DefaultRolesForUserAdded = defaultRolesForUserAdded;
            DefaultRolesForUserRemoved = defaultRolesForUserRemoved;
        }

        /// <summary>
        /// Gets the gallery ID for the gallery related to the gallery settings.
        /// </summary>
        /// <value>The gallery ID.</value>
        public int GalleryId { get; private set; }

        /// <summary>
        /// Gets or sets the default roles that have been added to the <see cref="IGallerySettings.DefaultRolesForUser" /> property since it was initially 
        /// populated from the data store. The value can be used by event handlers to take action on the setting change.
        /// </summary>
        public string[] DefaultRolesForUserAdded { get; private set; }

        /// <summary>
        /// Gets or sets the default roles that have been removed from the <see cref="IGallerySettings.DefaultRolesForUser" /> property since it was initially 
        /// populated from the data store. The value can be used by event handlers to take action on the setting change.
        /// </summary>
        public string[] DefaultRolesForUserRemoved { get; private set; }
    }
}
