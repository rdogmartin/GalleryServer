using System;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Represents a set of gallery-specific settings.
  /// </summary>
  public interface IGallerySettings
  {
    /// <summary>
    /// Gets or sets the ID for the gallery.
    /// </summary>
    /// <value>The gallery ID.</value>
    int GalleryId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the gallery settings have been populated with data for the current gallery.
    /// This library is initialized by calling <see cref="Initialize" />.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets a value indicating whether the gallery settings are the template settings used to populate the settings
    /// of new galleries.
    /// </summary>
    bool IsTemplate { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the current instance can be modified. Objects that are stored in a cache must
    /// be treated as read-only. Only objects that are instantiated right from the database and not shared across threads
    /// should be updated.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance can be modified; otherwise, <c>false</c>.
    /// </value>
    bool IsWritable
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the media object path. The path may be relative to the root of the web application
    /// (e.g. \gs\mediaobjects), a full path to a local resource (e.g. C:\mymedia), or a UNC path to a local or network
    /// resource (e.g. \\mynas\media). Mapped drives present a security risk and are not supported. The initial and 
    /// trailing slashes are	optional. For relative paths, the directory separator character can be either a forward 
    /// or backward slash. Use the property <see cref="FullMediaObjectPath" /> to retrieve the full physical path
    /// (such as "C:\inetpub\wwwroot\galleryserverpro\mediaobjects").
    /// </summary>
    /// <value>The media object path.</value>
    /// <remarks>The path is returned exactly how it appears in the configuration setting.</remarks>
    string MediaObjectPath { get; set; }

    /// <summary>
    /// Gets or sets the path to a directory where Gallery Server stores the thumbnail images of media objects. If 
    /// this path is empty, the directory containing the original media object is used to store the thumbnail image. 
    /// The path may be relative to the root of the web application (e.g. \gs\mediaobjects), a full path to a local 
    /// resource (e.g. C:\mymedia), or a UNC path to a local or network resource (e.g. \\mynas\media). Mapped 
    /// drives present a security risk and are not supported. The initial and trailing slashes are	optional. 
    /// For relative paths, the directory separator character can be either a forward or backward slash. Use the 
    /// property <see cref="FullThumbnailPath" /> to retrieve the full physical path
    /// (such as "C:\inetpub\wwwroot\galleryserverpro\mediaobjects").
    /// </summary>
    /// <value>The path to a directory where Gallery Server stores the thumbnail images of media objects.</value>
    string ThumbnailPath { get; set; }

    /// <summary>
    /// Gets or sets the path to a directory where Gallery Server stores the optimized images of media objects. If 
    /// this path is empty, the directory containing the original media object is used to store the optimized image. 
    /// The path may be relative to the root of the web application (e.g. \gs\mediaobjects), a full path to a local 
    /// resource (e.g. C:\mymedia), or a UNC path to a local or network resource (e.g. \\mynas\media). Mapped 
    /// drives present a security risk and are not supported. The initial and trailing slashes are	optional. 
    /// For relative paths, the directory separator character can be either a forward or backward slash.
    /// Not applicable for non-image media objects. Use the property <see cref="FullOptimizedPath" /> to retrieve 
    /// the full physical path (such as "C:\inetpub\wwwroot\galleryserverpro\mediaobjects").
    /// </summary>
    /// <value>The path to a directory where Gallery Server stores the optimized images of media objects.</value>
    string OptimizedPath { get; set; }

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
    /// <value><c>true</c> if the media objects directory is read-only; <c>false</c> if it can be written to.</value>
    bool MediaObjectPathIsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the header at the top of the gallery. The default value is <c>true</c>. 
    /// The header includes the gallery title, login/logout controls, and search function. The login/logout controls 
    /// and search function can be individually controlled via the <see cref="ShowLogin" /> and <see cref="ShowSearch" /> properties.
    /// When <c>false</c>, the controls within the header are not shown, even if individually they are set to be visible
    /// (e.g. ShowSearch=<c>true</c>, ShowLogin=<c>true</c>).
    /// </summary>
    /// <value><c>true</c> if the header is to be dislayed; otherwise, <c>false</c>.</value>
    bool ShowHeader { get; set; }

    /// <summary>
    /// Gets or sets the header text that appears at the top of each web page. Requires that <see cref="ShowHeader" /> be set to
    /// <c>true</c> in order to be visible.
    /// </summary>
    /// <value>The gallery title.</value>
    string GalleryTitle { get; set; }

    /// <summary>
    /// Gets or sets the URL the user will be directed to when she clicks the gallery title. Optional. If not 
    /// present, no link will be rendered. Examples: "http://www.mysite.com", "/" (the root of the web site),
    /// "~/" (the top level album).
    /// </summary>
    /// <value>The gallery title URL.</value>
    string GalleryTitleUrl { get; set; }

    /// <summary>
    /// Indicates whether to show the login controls at the top right of each page. When false, no login controls
    /// are shown, but the user can navigate directly to the login page to log on. Requires that <see cref="ShowHeader" /> 
    /// be set to <c>true</c> in order to be visible.
    /// </summary>
    /// <value><c>true</c> if login controls are visible; otherwise, <c>false</c>.</value>
    bool ShowLogin { get; set; }

    /// <summary>
    /// Indicates whether to show the search box at the top right of each page. Requires that <see cref="ShowHeader" /> 
    /// be set to <c>true</c> in order to be visible.
    /// </summary>
    /// <value><c>true</c> if the search box is visible; otherwise, <c>false</c>.</value>
    bool ShowSearch { get; set; }

    /// <summary>
    /// Indicates whether to show the full details of any unhandled exception that occurs within the gallery. This can reveal
    /// sensitive information to the user, so it should only be used for debugging purposes. When false, a generic error 
    /// message is given to the user. This setting has no effect when enableExceptionHandler="false".
    /// </summary>
    /// <value><c>true</c> if error details are displayed in the browser; <c>false</c> if a generic error message is displayed.</value>
    bool ShowErrorDetails { get; set; }

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
    bool EnableExceptionHandler { get; set; }


    /// <summary>
    /// The maximum length of directory name when a user creates an album. By default, directory names are the same as the
    /// album's title, but are truncated when the title is longer than the value specified here.
    /// </summary>
    int DefaultAlbumDirectoryNameLength { get; set; }
    //[IntegerValidator(MinValue = 1, MaxValue = 255)]
    //{
    //  get { return Convert.ToInt32(this[CoreAttributes.defaultAlbumDirectoryNameLength.ToString()], CultureInfo.InvariantCulture); }
    //  set { this[CoreAttributes.defaultAlbumDirectoryNameLength.ToString()] = value; }
    //}

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
    bool SynchAlbumTitleAndDirectoryName { get; set; }


    /// <summary>
    /// Gets or sets the metadata property to sort albums by. This value is assigned to the <see cref="IAlbum.SortByMetaName" />
    /// property when an album is created.
    /// </summary>
    /// <value>The metadata property to sort albums by.</value>
    MetadataItemName DefaultAlbumSortMetaName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an album's default sort order is ascending. A <c>false</c> value indicates
    /// a descending sort.
    /// </summary>
    /// <value><c>true</c> if an album's contents are sorted in ascending order by default; <c>false</c> if descending order.</value>
    bool DefaultAlbumSortAscending { get; set; }

    /// <summary>
    /// The color used for the background of the GIF image generated by Gallery Server when creating a default
    /// thumbnail image for a newly created album or an album without any objects. The color can be specified as
    /// hex (e.g. #336699), RGB (e.g. 127,55,95), or one of the System.Color.KnownColor enum values (e.g. Maroon).
    /// </summary>
    string EmptyAlbumThumbnailBackgroundColor { get; set; }

    /// <summary>
    /// The default text written on the GIF image generated by Gallery Server when creating a default thumbnail image 
    /// for a newly created album or an album without any objects. The GIF is 
    /// dynamically generated by the application when it is needed and is never actually stored on the hard drive.
    /// </summary>
    string EmptyAlbumThumbnailText { get; set; }

    /// <summary>
    /// The font used for text written on the GIF image generated by Gallery Server when creating a default
    /// thumbnail image for a newly created album or an album without any objects. The font must be installed on 
    /// the web server. If the font is not installed, a generic sans serif font will be substituted.
    /// </summary>
    string EmptyAlbumThumbnailFontName { get; set; }

    /// <summary>
    /// The size, in pixels, of the font used for text written on the GIF image generated by Gallery Server when 
    /// creating a default thumbnail image for a newly created album or an album without any objects. 
    /// </summary>
    int EmptyAlbumThumbnailFontSize { get; set; }

    /// <summary>
    /// The color of the text specified in property EmptyAlbumThumbnailText. The color can be specified as
    /// hex (e.g. #336699), RGB (e.g. 127,55,95), or one of the System.Color.KnownColor enum values (e.g. Maroon).
    /// </summary>
    string EmptyAlbumThumbnailFontColor { get; set; }

    /// <summary>
    /// The ratio of the width to height of the default thumbnail image for an album that does not have a thumbnail
    /// image specified. The length of the longest side of the image is set by the MaxThumbnailLength property, and the
    /// length of the remaining side is calculated using this ratio. A ratio or more than 1.00 results in the width
    /// being greater than the height (landscape), while a ratio less than 1.00 results in the width being less
    /// than the height (portrait). Example: If MaxThumbnailLength = 115 and EmptyAlbumThumbnailWidthToHeightRatio = 1.50,
    /// the width of the default thumbnail image is 115 and the height is 77 (115 / 1.50).
    /// </summary>
    System.Single EmptyAlbumThumbnailWidthToHeightRatio { get; set; }

    /// <summary>
    /// Maximum # of characters to display when showing the title of an album or media object in a thumbnail view.
    /// </summary>
    int MaxThumbnailTitleDisplayLength { get; set; }

    /// <summary>
    /// Indicates whether HTML is allowed in user-entered text such as titles, captions, and external media objects.
    /// When true, the HTML tags specified in <see cref="IGallerySettings.AllowedHtmlTags"/> and the attributes in
    /// <see cref="IGallerySettings.AllowedHtmlAttributes"/> are allowed. Invalid tags are automatically removed from user
    /// input. This setting does not affect how javascript is treated; refer to <see cref="IGallerySettings.AllowUserEnteredJavascript"/>.
    /// If this value is changed from true to false, existing objects will not be immediately purged of all HTML
    /// tags. Instead, individual titles and captions are stripped of HTML as each object is edited and saved by the user.
    /// </summary>
    bool AllowUserEnteredHtml { get; set; }

    /// <summary>
    /// Indicates whether javascript is allowed in user-entered text such as titles, captions, and external media 
    /// objects. When false, script tags and the string "javascript:" is automatically removed from all user input.
    /// WARNING: Enabling this option makes the gallery vulnerable to a cross site scripting attack by any user with 
    /// permission to edit captions or upload external media objects.
    /// </summary>
    bool AllowUserEnteredJavascript { get; set; }

    /// <summary>
    /// A list of HTML tags that may be present in titles and captions of albums and media objects.
    /// The attributes that are allowed are specified in <see cref="AllowedHtmlAttributes"/>.
    /// Applies only when <see cref="AllowUserEnteredHtml"/> is <c>true</c>. Ex: p,a,div,span,...
    /// </summary>
    string[] AllowedHtmlTags { get; set; }

    /// <summary>
    /// A list of attributes that HTML tags are allowed to have. These attributes, when combined with the
    /// HTML tags in <see cref="AllowedHtmlTags"/>, define the HTML that is allowed in titles and captions of 
    /// albums and media objects. Applies only when <see cref="AllowUserEnteredHtml"/> is <c>true</c>. Ex: href,class,style,...
    /// </summary>
    string[] AllowedHtmlAttributes { get; set; }

    /// <summary>
    /// Indicates whether to allow the copying of objects a user has only view permissions for.
    /// </summary>
    bool AllowCopyingReadOnlyObjects { get; set; }

    /// <summary>
    /// Indicates whether to allow a logged-on user to manage their account. When false, the link to the account page 
    /// at the top right of each page is not shown and if the user navigates directly to the account page, they are redirected away.
    /// </summary>
    /// <value><c>true</c> if a logged-on user can manage their account; otherwise, <c>false</c>.</value>
    bool AllowManageOwnAccount { get; set; }

    /// <summary>
    /// Indicates whether a user is allowed to delete his or her own account.
    /// </summary>
    bool AllowDeleteOwnAccount { get; set; }

    /// <summary>
    /// Specifies the visual transition effect to use when moving from one media object to another. 
    /// </summary>
    MediaObjectTransitionType MediaObjectTransitionType { get; set; }

    /// <summary>
    /// The duration of the transition effect, in seconds, when navigating between media objects. This 
    /// setting has no effect when mediaObjectTransitionType = "None".
    /// </summary>
    System.Single MediaObjectTransitionDuration { get; set; }

    /// <summary>
    /// The delay, in milliseconds, between images during a slide show.
    /// </summary>
    int SlideshowInterval { get; set; }

    /// <summary>
    /// Indicates whether a slide show continues from the beginning after showing the last media asset. When <c>false</c>, the user is
    /// redirected to the album page when the slide show ends.
    /// </summary>
    bool SlideShowLoop { get; set; }

    /// <summary>
    /// Indicates whether to allow users to upload file types not explicitly specified in the mimeTypes configuration
    /// section. When false, any file with an extension not listed in the mimeTypes section is rejected. When true,
    /// Gallery Server accepts all file types regardless of their file extension.
    /// </summary>
    bool AllowUnspecifiedMimeTypes { get; set; }

    /// <summary>
    /// A comma-delimited list of file extensions, including the period, indicating types of images that a standard browser can display. When
    /// the user requests an original image (high resolution), the original is sent to the browser in an &lt;img&gt; HTML tag
    /// if its extension is one of those listed here.  If not, the user is presented with a message containing instructions
    /// for downloading the image file. Typically this setting should not be changed. Ex: .jpg,.jpeg,.gif,.png
    /// </summary>
    string[] ImageTypesStandardBrowsersCanDisplay { get; set; }

    /// <summary>
    /// A comma-delimited list of file extensions, including the period, indicating types of files that can be processed
    /// by ImageMagick. Gallery Server uses ImageMagick to extract images from files that cannot be processed by .NET.
    /// Ex: .pdf,.txt,.eps,.psd
    /// </summary>
    string[] ImageMagickFileTypes { get; set; }

    /// <summary>
    /// Specifies whether anonymous users are allowed to rate gallery objects.
    /// </summary>
    /// <value><c>true</c> if anonymous rating is allowed; otherwise, <c>false</c>.</value>
    bool AllowAnonymousRating { get; set; }

    ///// <summary>
    ///// Specifies whether Gallery Server renders the right pane in the user interface objects. By default this pane
    ///// contains metadata associated with a gallery object.
    ///// </summary>
    //bool ShowRightPane { get; set; }

    /// <summary>
    /// Specifies whether Gallery Server extracts metadata from image files. If the attribute
    /// <see cref="ExtractMetadataUsingWpf" /> is true, then additional metadata such as title, keywords, and rating is extracted.
    /// </summary>
    bool ExtractMetadata { get; set; }

    /// <summary>
    /// Specifies whether metadata is extracted from image files using Windows Presentation Foundation (WPF) classes
    /// in .NET Framework 3.0 and higher. The WPF classes allow additional metadata to be extracted beyond those allowed by the
    /// .NET Framework 2.0, such as title, keywords, and rating. This attribute has no effect unless the following
    /// requirements are met: <see cref="ExtractMetadataUsingWpf" /> = true; .NET Framework 3.0 or higher is installed on the web
    /// server; and the web application is running in Full Trust. The WPF classes have exhibited some reliability issues
    /// during development, most notably causing the IIS worker process (w3wp.exe) to increase in memory usage and 
    /// eventually crash during uploads and synchronizations. For this reason one may want to disable this feature
    /// until a .NET Framework service pack or future version provides better performance.
    /// </summary>
    bool ExtractMetadataUsingWpf { get; set; }

    /// <summary>
    /// Gets the metadata settings that define how metadata items are displayed to the user.
    /// </summary>
    /// <value>The metadata display options.</value>
    IMetadataDefinitionCollection MetadataDisplaySettings { get; }

    /// <summary>
    /// Gets or sets the format string to use for <see cref="DateTime" /> metadata values. The date type of each meta item
    /// is specified by the <see cref="IMetadataDefinition.DataType" /> property.
    /// </summary>
    string MetadataDateTimeFormatString { get; set; }

    /// <summary>
    /// Specifies whether Gallery Server renders user interface objects to allow a user to download the file for a media 
    /// object. Note that setting this value to false does not prevent a user from downloading a
    /// media object, since a user already has access to the media object if he or she can view it in the browser. To
    /// prevent certain users from viewing media objects (and thus downloading them), use private albums, disable
    /// anonymous viewing, or configure security to prevent users from viewing the objects.
    /// </summary>
    bool EnableMediaObjectDownload { get; set; }

    /// <summary>
    /// Specifies whether anonymous users are allowed to view the original versions of media objects. When no
    /// compressed (optimized) version exists, the user is allowed to view the original, regardless of this
    /// setting. This setting has no effect on logged on users.
    /// </summary>
    bool EnableAnonymousOriginalMediaObjectDownload { get; set; }

    /// <summary>
    /// Specifies whether users are allowed to download media objects and albums in a ZIP file. Downloading of albums can be
    /// restricted by setting <see cref="EnableAlbumZipDownload" /> to <c>false</c>.
    /// </summary>
    bool EnableGalleryObjectZipDownload { get; set; }

    /// <summary>
    /// Specifies whether users are allowed to download albums in a ZIP file. This setting <see cref="EnableGalleryObjectZipDownload" />
    /// must be enabled for this setting to take effect. In other words, albums can be downloaded only when 
    /// <see cref="EnableGalleryObjectZipDownload" /> and <see cref="EnableAlbumZipDownload" /> are both enabled.
    /// </summary>
    bool EnableAlbumZipDownload { get; set; }

    /// <summary>
    /// Specifies whether slide show functionality is enabled. When true, a start/pause slideshow button is displayed in the 
    /// toolbar that appears above a media object. The length of time each image is shown before automatically moving
    /// to the next one is controlled by the SlideshowInterval setting. Note that only images are shown during a slide
    /// show; other objects such as videos, audio files, and documents are skipped.
    /// </summary>
    bool EnableSlideShow { get; set; }

    /// <summary>
    /// Gets or sets the size of media assets to display when viewing a single media asset.
    /// </summary>
    DisplayObjectType MediaViewSize { get; set; }

    /// <summary>
    /// Gets or sets the type of the slide show.
    /// </summary>
    /// <value>The type of the slide show.</value>
    SlideShowType SlideShowType { get; set; }

    /// <summary>
    ///	The length (in pixels) of the longest edge of a thumbnail image.  This value is used when a thumbnail 
    ///	image is created. The length of the shorter side is calculated automatically based on the aspect ratio of the image.
    /// </summary>
    int MaxThumbnailLength { get; set; }

    /// <summary>
    /// The quality level that thumbnail images are stored at (0 - 100).
    /// </summary>
    int ThumbnailImageJpegQuality { get; set; }

    /// <summary>
    /// The string that is prepended to the thumbnail filename for each media object. For example, if an image
    /// named puppy.jpg is added, and this setting is "zThumb_", the thumbnail image will be named 
    /// "zThumb_puppy.jpg".	NOTE: Any file named "zThumb_puppy.jpg" that already exists will be overwritten, 
    /// so it is important to choose a value that, when prepended to media object filenames, will not 
    /// conflict with existing media objects.
    /// </summary>
    string ThumbnailFileNamePrefix { get; set; }

    /// <summary>
    ///	The length (in pixels) of the longest edge of an optimized image.  This value is used when an optimized
    ///	image is created. The length of the shorter side is calculated automatically based on the aspect ratio of the image.
    /// </summary>
    int MaxOptimizedLength { get; set; }

    /// <summary>
    /// The quality level that optimized JPG pictures are created with. This is a number from 1 - 100, with 1 
    /// being the worst quality and 100 being the best quality. Not applicable for non-image media objects.
    /// </summary>
    int OptimizedImageJpegQuality { get; set; }

    /// <summary>
    /// The size (in KB) above which an image is compressed to create an optimized version.
    /// Not applicable for non-image media objects.
    /// </summary>
    int OptimizedImageTriggerSizeKb { get; set; }

    /// <summary>
    /// The string that is prepended to the optimized filename for images. This setting is only used for image
    /// media objects where an optimized image file is created. For example, if an image named
    /// puppy.jpg is added, and this setting is "zOpt_", the optimized image will be named "zOpt_puppy.jpg".
    /// NOTE: Any file named "zOpt_puppy.jpg" that already exists will be overwritten, 
    /// so it is important to choose a value that, when prepended to media object filenames, will not 
    /// conflict with existing media objects.
    /// </summary>
    string OptimizedFileNamePrefix { get; set; }

    /// <summary>
    /// The quality level that original JPG pictures are saved at. This is only used when the original is 
    /// modified by the user, such as rotation. Not applicable for non-image media objects.
    /// </summary>
    int OriginalImageJpegQuality { get; set; }

    /// <summary>
    /// Specifies whether to discard the original image when it is added to the gallery. This option, when enabled, 
    /// helps reduce disk space usage. This option applies only to images, and only when they are added through an 
    /// upload or by synchronizing. Changing this setting does not affect existing media objects. When false, 
    /// users still have the option to discard the original image on the Add Objects page by unchecking the 
    /// corresponding checkbox.
    /// </summary>
    bool DiscardOriginalImageDuringImport { get; set; }

    /// <summary>
    /// Specifies whether to apply a watermark to optimized and original images. If true, the text in the watermarkText
    /// property is applied to images, and the image specified in watermarkImagePath is overlayed on the image. If
    /// watermarkText is empty, or if watermarkImagePath is empty or does not refer to a valid image, that watermark
    /// is not applied. If applyWatermarkToThumbnails = true, then the watermark is also applied to thumbnails.
    /// </summary>
    bool ApplyWatermark { get; set; }

    /// <summary>
    /// Specifies whether to apply the text and/or image watermark to thumbnail images. This property is ignored if 
    /// applyWatermark = false.
    /// </summary>
    bool ApplyWatermarkToThumbnails { get; set; }

    /// <summary>
    /// Specifies the text to apply to images in the gallery. The text is applied in a single line.
    /// </summary>
    string WatermarkText { get; set; }

    /// <summary>
    /// The font used for the watermark text. If the font is not installed on the web server, a generic font will 
    /// be substituted.
    /// </summary>
    string WatermarkTextFontName { get; set; }

    /// <summary>
    /// Gets or sets the height, in pixels, of the watermark text. This value is ignored if the property
    /// WatermarkTextWidthPercent is non-zero. Valid values are 0 - 10000.
    /// </summary>
    int WatermarkTextFontSize { get; set; }

    /// <summary>
    /// Gets or sets the percent of the overall width of the recipient image that should be covered with the
    /// watermark text. The size of the text is automatically scaled up or down to achieve the desired width. For example,
    /// a value of 50 means the text is 50% as wide as the recipient image. Valid values are 0 - 100. The text is never
    /// rendered in a font smaller than 6 pixels, so in cases of long text it may stretch wider than the percentage
    /// specified in this setting.
    /// A value of 0 turns off this feature and causes the text size to be determined by the 
    /// WatermarkTextFontSize property.
    /// </summary>
    int WatermarkTextWidthPercent { get; set; }

    /// <summary>
    /// Specifies the color of the watermark text. The color can be specified as hex (e.g. #336699), RGB (e.g. 127,55,95),
    /// or one of the System.Color.KnownColor enum values (e.g. Maroon).
    /// </summary>
    string WatermarkTextColor { get; set; }

    /// <summary>
    /// The opacity of the watermark text. This is a value from 0 to 100, with 0 being invisible and 100 being solid, 
    /// with no transparency.
    /// </summary>
    int WatermarkTextOpacityPercent { get; set; }

    /// <summary>
    /// Gets or sets the location for the watermark text on the recipient image. This value maps to the 
    /// enumeration System.Drawing.ContentAlignment, and must be one of the following nine values:
    /// TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight.
    /// </summary>
    System.Drawing.ContentAlignment WatermarkTextLocation { get; set; }

    /// <summary>
    /// Gets or sets the filename of a watermark image to be applied to the recipient image. The image
    /// must be in a format that allows it to be instantiated in a <see cref="System.Drawing.Bitmap" /> object.
    /// Typically that means it should be jpg, gif, png, bmp, or tif. Ex: "logo.png"
    /// </summary>
    /// <remarks>The name implies the property stores a path rather than a name. That's because it used to store 
    /// a path in versions earlier than 4.2.0. We kept the name the same for backward compatibility and simplicity, 
    /// even though it is a bit misleading.</remarks>
    string WatermarkImagePath { get; set; }

    /// <summary>
    /// Gets or sets the percent of the overall width of the recipient image that should be covered with the
    /// watermark image. The size of the image is automatically scaled to achieve the desired width. For example,
    /// a value of 50 means the watermark image is 50% as wide as the recipient image. Valid values are 0 - 100.
    /// A value of 0 turns off this feature and causes the image to be rendered its actual size.
    /// </summary>
    int WatermarkImageWidthPercent { get; set; }

    /// <summary>
    /// Gets or sets the opacity of the watermark image. Valid values are 0 - 100, with 0 being completely
    /// transparent and 100 completely opaque.
    /// </summary>
    int WatermarkImageOpacityPercent { get; set; }

    /// <summary>
    /// Gets or sets the location for the watermark image on the recipient image. This value maps to the 
    /// enumeration System.Drawing.ContentAlignment, and must be one of the following nine values:
    /// TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight.
    /// </summary>
    System.Drawing.ContentAlignment WatermarkImageLocation { get; set; }

    /// <summary>
    /// Specifies whether the Gallery Server administrator (specified in EmailToName/EmailToAddress)
    /// is sent a report when a web site error occurs.  A valid SMTP server must be specified if this
    /// is set to true (attribute SmtpServer).
    /// </summary>
    bool SendEmailOnError { get; set; }

    /// <summary>
    /// Indicates whether a video, audio or other dynamic object will automatically start playing in the user's browser.
    /// </summary>
    bool AutoStartMediaObject { get; set; }

    /// <summary>
    /// Indicates the default width, in pixels, of the browser object that plays a video file. Typically 
    /// this refers to the &lt;object&gt; tag that contains the video, resulting in a tag similar to this:
    /// &lt;object style="width:640px;height:480px;" ... &gt;
    /// </summary>
    int DefaultVideoPlayerWidth { get; set; }

    /// <summary>
    /// Indicates the default height, in pixels, of the browser object that plays a video file. Typically 
    /// this refers to the &lt;object&gt; tag that contains the video, resulting in a tag similar to this:
    /// &lt;object style="width:640px;height:480px;" ... &gt;
    /// </summary>
    int DefaultVideoPlayerHeight { get; set; }

    /// <summary>
    /// Indicates the default width, in pixels, of the browser object that plays an audio file. Typically 
    /// this refers to the &lt;object&gt; tag that contains the audio file, resulting in a tag similar to this:
    /// &lt;object style="width:300px;height:200px;" ... &gt;
    /// </summary>
    int DefaultAudioPlayerWidth { get; set; }

    /// <summary>
    /// Indicates the default height, in pixels, of the browser object that plays an audio file. Typically 
    /// this refers to the &lt;object&gt; tag that contains the audio file, resulting in a tag similar to this:
    /// &lt;object style="width:300px;height:200px;" ... &gt;
    /// </summary>
    int DefaultAudioPlayerHeight { get; set; }

    /// <summary>
    /// Indicates the default width, in pixels, of the browser object that displays a generic media object.
    /// A generic media object is defined as any media object that is not an image,	audio, or video file. This
    /// includes Shockwave Flash, Adobe Reader, text files, Word documents and others. The value specified here
    /// is sent to the browser as the width for the object element containing this media object, resulting in syntax 
    /// similar to this: &lt;object style="width:640px;height:480px;" ... &gt; This setting applies only to objects 
    /// rendered within the browser, such as Shockwave Flash. Objects sent to the browser via a download
    /// link, such as text files, PDF files, and Word documents, ignore this setting.
    /// </summary>
    int DefaultGenericObjectWidth { get; set; }

    /// <summary>
    /// Indicates the default height, in pixels, of the browser object that displays a generic media object.
    /// A generic media object is defined as any media object that is not an image,	audio, or video file. This
    /// includes Shockwave Flash, Adobe Reader, text files, Word documents and others. The value specified here
    /// is sent to the browser as the width for the object element containing this media object, resulting in syntax 
    /// similar to this: &lt;object style="width:640px;height:480px;" ... &gt; This setting applies only to objects 
    /// rendered within the browser, such as Shockwave Flash. Objects sent to the browser via a download
    /// link, such as text files, PDF files, and Word documents, ignore this setting.
    /// </summary>
    int DefaultGenericObjectHeight { get; set; }

    /// <summary>
    /// Indicates the maximum size, in kilobytes, of the files that can be uploaded.
    /// Use this setting to keep users from uploading very large files and to help guard against Denial of 
    /// Service (DOS) attacks. A value of zero (0) indicates there is no restriction on upload size (unlimited).
    /// This setting is not used during synchronization.
    /// </summary>
    int MaxUploadSize { get; set; }

    /// <summary>
    /// Indicates whether a user can upload a physical file to the gallery, such as an image or video file stored
    /// on a local hard drive. The user must also be authenticated and a member of a role with AllowAddMediaObject 
    /// or AllowAdministerSite permission. This setting is not used during synchronization.
    /// </summary>
    bool AllowAddLocalContent { get; set; }

    /// <summary>
    /// Indicates whether a user can add a link to external content, such as a YouTube video, to the gallery. 
    /// The user must also be authenticated and a member of a role with AllowAddMediaObject 
    /// or AllowAdministerSite permission. This setting is not used during synchronization.
    /// </summary>
    bool AllowAddExternalContent { get; set; }

    /// <summary>
    /// Indicates whether users can view galleries without logging in. When false, users are redirected to a login
    /// page when any album is requested. Private albums are never shown to anonymous users, even when this 
    /// property is true.
    /// </summary>
    bool AllowAnonymousBrowsing { get; set; }

    /// <summary>
    /// Indicates the number of objects to display at a time. For example, if an album has more than this number of
    /// gallery objects, paging controls appear to assist the user in navigating to them. A value of zero disables 
    /// the paging feature.
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the location for the pager used to navigate large collections of objects. This value maps to the 
    /// enumeration GalleryServer.Business.PagerPosition, and must be one of the following values:
    /// Top, Bottom, TopAndBottom. This value is ignored when paging is disabled (<see cref="PageSize"/> = 0).
    /// </summary>
    PagerPosition PagerLocation { get; set; }

    /// <summary>
    /// Indicates whether anonymous users are allowed to create accounts.
    /// </summary>
    bool EnableSelfRegistration { get; set; }

    /// <summary>
    /// Indicates whether e-mail verification is required when a user registers an account. When true, the account is 
    /// initially disabled and an email is sent to the user with a verification link. When clicked, user is approved 
    /// and logged on, unless <see cref="RequireApprovalForSelfRegisteredUser"/> is enabled, in which case an administrator
    /// must approve the account before the user can log on. Setting this to true reduces spam activity and guarantees that 
    /// a valid e-mail address is associated with the user. When the setting is false, an e-mail address is not required 
    /// and the user account is immediately created. This setting is ignored when 
    /// <see cref="EnableSelfRegistration">self registration</see> is disabled.
    /// </summary>
    bool RequireEmailValidationForSelfRegisteredUser { get; set; }

    /// <summary>
    /// Indicates whether an administrator must approve newly created accounts before the user can log on. When true, 
    /// the account is disabled until it is approved by an administrator. When a user registers an account, an e-mail
    /// is sent to each user specified in <see cref="UsersToNotifyWhenAccountIsCreated"/>. Only users belonging to a
    /// role with AllowAdministerSite permission can approve a user. If <see cref="RequireEmailValidationForSelfRegisteredUser"/>
    /// is enabled, the e-mail requesting administrator approval is not sent until the user verifies the e-mail address.
    /// This setting is ignored when <see cref="EnableSelfRegistration">self registration</see> is disabled.
    /// </summary>
    bool RequireApprovalForSelfRegisteredUser { get; set; }

    /// <summary>
    /// Indicates whether account names are primarily e-mail addresses. When true, certain forms, such as the self registration
    /// wizard, assume e-mail addresses are used as account names. For example, when this value is false, the self registration
    /// wizard includes fields for both an account name and an e-mail address, but when true it only requests an e-mail address.
    /// This setting is ignored when <see cref="EnableSelfRegistration">self registration</see> is disabled.
    /// </summary>
    bool UseEmailForAccountName { get; set; }

    /// <summary>
    /// A list of roles that apply to all users.
    /// </summary>
    string[] DefaultRolesForUser { get; set; }

    /// <summary>
    /// A list of account names of users to receive an e-mail notification when an account is created.
    /// When <see cref="RequireEmailValidationForSelfRegisteredUser"/> is enabled, the e-mail is not sent until the
    /// user verifies the e-mail address. Applies whether an account is self-created or created by an administrator.
    /// </summary>
    IUserAccountCollection UsersToNotifyWhenAccountIsCreated { get; set; }

    /// <summary>
    /// A list of account names of users to receive an e-mail notification when an application error occurs.
    /// </summary>
    IUserAccountCollection UsersToNotifyWhenErrorOccurs { get; set; }

    /// <summary>
    /// Indicates whether each user is associated owner to a unique album. The title of the album is based on the 
    /// template in the <see cref="UserAlbumNameTemplate"/> property. The album is created when the account is created or
    /// if the album does not exist when the user logs on. It is created in the album specified in the 
    /// <see cref="UserAlbumParentAlbumId"/> property.</summary>
    bool EnableUserAlbum { get; set; }

    /// <summary>
    /// Indicates whether a user album is automatically created for a user the first time he or she logs on. This setting
    /// is used to seed the user's <see cref="IUserGalleryProfile.EnableUserAlbum" /> profile setting when it is created.
    /// This property applies only when <see cref="IGallerySettings.EnableUserAlbum" /> is <c>true</c>.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if a user album is created for each user the first time he or she logs on; otherwise, <c>false</c>.
    /// </value>
    bool EnableUserAlbumDefaultForUser { get; set; }

    /// <summary>
    /// Specifies the ID of the album containing user albums. This setting is ignored when <see cref="EnableUserAlbum"/>
    /// is false. This property may have a value of zero (0) when user albums are disabled.
    /// </summary>
    int UserAlbumParentAlbumId { get; set; }

    /// <summary>
    /// Specifies the template to use for naming the album that is created for new users. Applies only when 
    /// <see cref="EnableUserAlbum"/> is true. The placeholder string {UserName}, if present, is replaced 
    /// by the account name.
    /// </summary>
    string UserAlbumNameTemplate { get; set; }

    /// <summary>
    /// Specifies the template to use for the album summary of a newly created user album. Applies only when 
    /// <see cref="EnableUserAlbum"/> is true. No placeholder strings are supported.
    /// </summary>
    string UserAlbumSummaryTemplate { get; set; }

    /// <summary>
    /// Indicates whether to redirect the user to his or her album after logging in. If set to false, the current page is
    /// re-loaded or, if there isn't a page, the user is shown the top level album for which the user has view access. This setting 
    /// is ignored when <see cref="EnableUserAlbum"/> is false.</summary>
    bool RedirectToUserAlbumAfterLogin { get; set; }

    /// <summary>
    /// Gets or sets the position in the video where the thumbnail is generated from. The value is in seconds, so a value
    /// of three indicates the thumbnail for the video is generated from a frame three seconds into the video. The value must be 
    /// between 0 and 86,400 seconds.
    /// </summary>
    /// <value>The position, in seconds, in the video where the thumbnail image is generated from.</value>
    int VideoThumbnailPosition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically synchronize the current gallery on a periodic basis. The interval
    /// is defined in the <see cref="AutoSyncIntervalMinutes" /> property. The auto sync depends on periodic browser requests by 
    /// users to trigger the logic to check whether a sync is needed.
    /// </summary>
    /// <value><c>true</c> if auto sync is enabled; otherwise, <c>false</c>.</value>
    bool EnableAutoSync { get; set; }

    /// <summary>
    /// Gets or sets the minimum interval, in minutes, that an auto-synchronization is to occur. Since the auto sync feature 
    /// requires periodic browser requests, the actual interval may be longer for infrequently accessed galleries.
    /// </summary>
    /// <value>The auto sync interval, in minutes.</value>
    int AutoSyncIntervalMinutes { get; set; }

    /// <summary>
    /// Gets or sets the date/time of the last auto-sync. Value is <see cref="DateTime.MinValue" /> when <see cref="EnableAutoSync" />
    /// is disabled or when no auto-sync has yet been performed.
    /// </summary>
    /// <value>The date/time of the last auto-sync.</value>
    DateTime LastAutoSync { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow external calls to the synchronize web service.
    /// </summary>
    /// <value><c>true</c> if a synchronization operation can be initiated through a web service; otherwise, <c>false</c>.</value>
    bool EnableRemoteSync { get; set; }

    /// <summary>
    /// Gets or sets the password that is passed to the remote synchronization web service methods. This password prevents
    /// malicious users from starting unauthorized synchronizations.
    /// </summary>
    /// <value>The remote sync password.</value>
    string RemoteAccessPassword { get; set; }

    /// <summary>
    /// Gets or sets the media encoder settings that define how media files may be encoded.
    /// </summary>
    /// <value>An instance that implements <see cref="IMediaEncoderSettingsCollection" />.</value>
    IMediaEncoderSettingsCollection MediaEncoderSettings { get; set; }

    /// <summary>
    /// Gets or sets the timeout setting, in milliseconds, for the media encoder function.
    /// </summary>
    /// <value>An integer</value>
    int MediaEncoderTimeoutMs { get; set; }

    /// <summary>
    /// Gets the full physical path to the directory containing the media objects. Example:
    /// "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"
    /// </summary>
    string FullMediaObjectPath { get; }

    /// <summary>
    /// Gets the full physical path to the directory where Gallery Server stores the thumbnail images of media objects.
    /// If no directory is specified in the configuration setting, this returns the main media object path (that is, returns
    /// the same value as the <see cref="FullMediaObjectPath" /> property).
    /// Example: "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"
    /// </summary>
    string FullThumbnailPath { get; }

    /// <summary>
    /// Gets the full physical path to the directory where Gallery Server stores the optimized images of media objects.
    /// If no directory is specified in the configuration setting, this returns the main media object path (that is, returns
    /// the same value as the <see cref="FullMediaObjectPath" /> property).
    /// Example: "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"
    /// </summary>
    string FullOptimizedPath { get; }

    /// <summary>
    /// Perform any initialization tasks that must be performed before the object can be used by the application.
    /// This should be called after the core properties from the data store have been assigned.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown when this method is called more than once during 
    /// the application's lifetime.</exception>
    void Initialize();

    /// <overload>
    /// Persist the current gallery settings to the data store.
    /// </overload>
    /// <summary>
    /// Persist the current gallery settings to the data store. Automatically clears and then reloads the gallery settings 
    /// from the data store.
    /// </summary>
    void Save();

    /// <summary>
    /// Persist the current gallery settings to the data store, optionally modifying the default behavior of clearing
    /// and then reloading the gallery settings from the data store.
    /// </summary>
    /// <param name="forceReloadFromDataStore">If set to <c>true</c>, clear the gallery settings stored in memory, which will
    /// force loading them from the data store. Setting this to <c>false</c> can be useful when updating a simple property that 
    /// does not require a complex recalculation (like, say the <see cref="UsersToNotifyWhenErrorOccurs" /> does). It may also
    /// be needed when a separate thread is persisting the data and no instance of HttpContext exists, which can cause an 
    /// exception in the DotNetNuke module during the reload process in the web layer.</param>
    void Save(bool forceReloadFromDataStore);
  }
}