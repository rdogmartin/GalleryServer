using System;
using System.Globalization;
using System.Web;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Web
{
  /// <summary>
  /// Provides functionality for generating the HTML that can be sent to a client browser to render a
  /// particular media object. Objects implementing this interface use the HTML templates in the configuration
  /// file. Replaceable parameters in the template are indicated by the open and close brackets, such as 
  /// {Width}. These parameters are replaced with the relevant values.
  /// TODO: Add caching functionality to speed up the ability to generate HTML.
  /// </summary>
  public class MediaObjectHtmlBuilder
  {
    #region Private Fields

    private string _uniquePrefixId;
    private IMediaTemplate _mediaTemplate;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaObjectHtmlBuilder"/> class.
    /// </summary>
    /// <param name="options">The options that will dictate the HTML and URL generation.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="options" /> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="options" /> contains one or more
    /// invalid values.</exception>
    public MediaObjectHtmlBuilder(MediaObjectHtmlBuilderOptions options)
    {
      if (options == null)
        throw new ArgumentNullException("options");

      if ((options.Browsers == null) || (options.Browsers.Length < 1))
        throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.MediaObjectHtmlBuilder_Ctor_InvalidBrowsers_Msg));

      if (options.GalleryObject == null)
        throw new ArgumentException("The GalleryObject property of the options parameter cannot be null.", "options");

      if (options.DisplayType == DisplayObjectType.Unknown)
        throw new ArgumentException("The DisplayType property of the options parameter cannot be DisplayObjectType.Unknown.", "options");

      Options = options;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the options that dictate the HTML and URL generation.
    /// </summary>
    private MediaObjectHtmlBuilderOptions Options { get; set; }

    /// <summary>
    /// Gets the gallery object.
    /// </summary>
    public IGalleryObject GalleryObject
    {
      get { return Options.GalleryObject; }
    }

    /// <summary>
    /// Gets the ID of the media object associated with <see cref="GalleryObject" />. When <see cref="GalleryObject" />
    /// is an <see cref="IAlbum" />, this property returns the ID of the thumbnail image or zero if no thumbnail
    /// image is assigned.
    /// </summary>
    private int MediaObjectId
    {
      get
      {
        if (GalleryObject is IAlbum)
        {
          return GetAlbumThumbnailId();
        }

        return GalleryObject.Id;
      }
    }

    /// <summary>
    /// Gets the MIME type of the <see cref="DisplayObject" /> of the <see cref="GalleryObject" />.
    /// </summary>
    public IMimeType MimeType
    {
      get
      {
        return DisplayObject.MimeType;
      }
    }

    /// <summary>
    /// Gets the physical path to this media object, including the object's name. Example:
    /// C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets\sonorandesert.jpg
    /// </summary>
    public string MediaObjectPhysicalPath
    {
      get
      {
        return DisplayObject.FileNamePhysicalPath;
      }
    }

    /// <summary>
    /// Gets the width of this object, in pixels.
    /// </summary>
    public int Width
    {
      get
      {
        return DisplayObject.Width;
      }
    }

    /// <summary>
    /// Gets the height of this object, in pixels.
    /// </summary>
    public int Height
    {
      get
      {
        return DisplayObject.Height;
      }
    }

    /// <summary>
    /// Gets an <see cref="System.Array"/> of browser ids for the current browser. This is a list of strings,
    /// ordered from most general to most specific, that represent the various categories of browsers the current
    /// browser belongs to.
    /// </summary>
    public Array Browsers
    {
      get
      {
        return Options.Browsers;
      }
    }

    private IMediaTemplate MediaTemplate
    {
      get
      {
        return _mediaTemplate ?? (_mediaTemplate = MimeType.GetMediaTemplate(Browsers));
      }
    }

    /// <summary>
    /// Gets the HTML from the media template corresponding to the <see cref="MimeType" /> and <see cref="Browsers" />.
    /// If no media template exists, an empty string is returned.
    /// </summary>
    private string HtmlTemplate
    {
      get
      {
        return (MediaTemplate == null ? String.Empty : MediaTemplate.HtmlTemplate);
      }
    }

    /// <summary>
    /// Gets the JavaScript from the media template corresponding to the <see cref="MimeType" /> and <see cref="Browsers" />.
    /// If no media template exists, an empty string is returned.
    /// </summary>
    private string ScriptTemplate
    {
      get
      {
        return (MediaTemplate == null ? String.Empty : MediaTemplate.ScriptTemplate);
      }
    }

    /// <summary>
    /// Gets the type of the display object.
    /// </summary>
    public DisplayObjectType DisplayType
    {
      get
      {
        return Options.DisplayType;
      }
    }

    /// <summary>
    /// Gets the display object of the <see cref="GalleryObject" />.
    /// </summary>
    public IDisplayObject DisplayObject
    {
      get
      {
        switch (DisplayType)
        {
          case DisplayObjectType.Thumbnail:
            return GalleryObject.Thumbnail;
          case DisplayObjectType.Optimized:
            return GalleryObject.Optimized;
          default:
            return GalleryObject.Original;
        }
      }
    }

    /// <summary>
    /// Gets a generated string about twelve characters long that can be used as a unique identifier, such as the ID of
    /// an HTML element. The value is generated the first time the property is accessed, and subsequent reads return
    /// the same value. There is currently no support for generating more than one ID during the lifetime of an instance.
    /// Ex: "gsp_1c135176ed"
    /// </summary>
    private string UniquePrefixId
    {
      get
      {
        if (String.IsNullOrEmpty(_uniquePrefixId))
        {
          _uniquePrefixId = String.Concat("gsp_", Guid.NewGuid().ToString().Replace("-", String.Empty).Substring(0, 10));
        }

        return _uniquePrefixId;
      }
    }

    /// <summary>
    /// Gets the URL, relative to the website root and optionally including any query string parameters,
    /// to the page any generated URLs should point to. Examples: "/dev/gs/gallery.aspx",
    /// "/dev/gs/gallery.aspx?g=admin_email&amp;aid=2389"
    /// </summary>
    public string DestinationPageUrl
    {
      get { return Options.DestinationPageUrl; }
    }

    /// <summary>
    /// Gets the URI scheme, DNS host name or IP address, and port number for the current application. 
    /// Examples: "http://www.site.com", "http://localhost", "http://127.0.0.1", "http://godzilla"
    /// </summary>
    public string HostUrl
    {
      get { return Options.HostUrl; }
    }

    /// <summary>
    /// Gets the path, relative to the web site root, to the current web application.
    /// Example: "/dev/gallery".
    /// </summary>
    public string AppRoot
    {
      get { return Options.AppRoot; }
    }

    /// <summary>
    /// Gets the path, relative to the web site root, to the directory containing the Gallery Server user 
    /// controls and other resources. Example: "/dev/gallery/gs".
    /// </summary>
    public string GalleryRoot
    {
      get { return Options.GalleryRoot; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Generate the HTML that can be sent to a browser to render the media object. The HTML is generated from the
    /// media template associated with the media objects MIME type. If <see cref="GalleryObject" /> is an <see cref="IAlbum" />,
    /// <see cref="String.Empty" /> is returned. Guaranteed to not return null.
    /// </summary>
    /// <returns>Returns a string of valid HTML that can be sent to a browser.</returns>
    public string GenerateHtml()
    {
      if (GalleryObject is IAlbum)
        return String.Empty;

      var htmlOutput = GetHtmlTemplate();

      htmlOutput = htmlOutput.Replace("{HostUrl}", HostUrl);
      htmlOutput = htmlOutput.Replace("{MediaObjectUrl}", GetMediaObjectUrl());
      htmlOutput = htmlOutput.Replace("{MimeType}", MimeType.BrowserMimeType);
      htmlOutput = htmlOutput.Replace("{Width}", Width.ToString(CultureInfo.InvariantCulture));
      htmlOutput = htmlOutput.Replace("{Height}", Height.ToString(CultureInfo.InvariantCulture));
      htmlOutput = htmlOutput.Replace("{Title}", GalleryObject.Title);
      htmlOutput = htmlOutput.Replace("{TitleNoHtml}", Utils.RemoveHtmlTags(GalleryObject.Title, true));
      htmlOutput = htmlOutput.Replace("{UniqueId}", UniquePrefixId);
      htmlOutput = htmlOutput.Replace("{Caption}", GalleryObject.Caption);

      if (htmlOutput.Contains("{CaptionNoHtml}"))
      {
        htmlOutput = htmlOutput.Replace("{CaptionNoHtml}", Utils.RemoveHtmlTags(GalleryObject.Caption, true));
      }

      bool autoStartMediaObject = Factory.LoadGallerySetting(GalleryObject.GalleryId).AutoStartMediaObject;

      // Replace {AutoStartMediaObjectText} with "true" or "false".
      htmlOutput = htmlOutput.Replace("{AutoStartMediaObjectText}", autoStartMediaObject.ToString().ToLowerInvariant());

      // Replace {AutoStartMediaObjectInt} with "1" or "0".
      htmlOutput = htmlOutput.Replace("{AutoStartMediaObjectInt}", autoStartMediaObject ? "1" : "0");

      // Replace {AutoPlay} with "autoplay" or "".
      htmlOutput = htmlOutput.Replace("{AutoPlay}", autoStartMediaObject ? "autoplay" : String.Empty);

      if (htmlOutput.Contains("{MediaObjectAbsoluteUrlNoHandler}"))
        htmlOutput = ReplaceMediaObjectAbsoluteUrlNoHandlerParameter(htmlOutput);

      if (htmlOutput.Contains("{MediaObjectRelativeUrlNoHandler}"))
        htmlOutput = ReplaceMediaObjectRelativeUrlNoHandlerParameter(htmlOutput);

      if (htmlOutput.Contains("{GalleryPath}"))
        htmlOutput = htmlOutput.Replace("{GalleryPath}", GalleryRoot);

      return htmlOutput;
    }

    /// <summary>
    /// Generate the JavaScript that can be sent to a browser to assist with rendering the media object. 
    /// If <see cref="GalleryObject" /> is an <see cref="IAlbum" />, <see cref="String.Empty" /> is returned.
    /// If the configuration file does not specify a scriptOutput template for this MIME type, an empty string is returned.
    /// </summary>
    /// <returns>Returns the JavaScript that can be sent to a browser to assist with rendering the media object.</returns>
    public string GenerateScript()
    {
      if (GalleryObject is IAlbum)
        return String.Empty;

      if ((MimeType.MajorType.Equals("IMAGE", StringComparison.OrdinalIgnoreCase)) && (IsImageBrowserIncompatible()))
        return String.Empty; // Browsers can't display this image.

      var scriptOutput = ScriptTemplate;

      if (String.IsNullOrEmpty(scriptOutput))
        return String.Empty; // No ECMA script rendering info in config file.

      scriptOutput = scriptOutput.Replace("{HostUrl}", HostUrl);
      scriptOutput = scriptOutput.Replace("{MediaObjectUrl}", GetMediaObjectUrl());
      scriptOutput = scriptOutput.Replace("{MimeType}", MimeType.BrowserMimeType);
      scriptOutput = scriptOutput.Replace("{Width}", Width.ToString(CultureInfo.InvariantCulture));
      scriptOutput = scriptOutput.Replace("{Height}", Height.ToString(CultureInfo.InvariantCulture));
      scriptOutput = scriptOutput.Replace("{Title}", GalleryObject.Title);
      scriptOutput = scriptOutput.Replace("{TitleNoHtml}", Utils.RemoveHtmlTags(GalleryObject.Title, true));
      scriptOutput = scriptOutput.Replace("{UniqueId}", UniquePrefixId);
      scriptOutput = scriptOutput.Replace("{Caption}", GalleryObject.Caption);

      if (scriptOutput.Contains("{CaptionNoHtml}"))
      {
        scriptOutput = scriptOutput.Replace("{CaptionNoHtml}", Utils.RemoveHtmlTags(GalleryObject.Caption, true));
      }

      var autoStartMediaObject = Factory.LoadGallerySetting(GalleryObject.GalleryId).AutoStartMediaObject;

      // Replace {AutoStartMediaObjectText} with "true" or "false".
      scriptOutput = scriptOutput.Replace("{AutoStartMediaObjectText}", autoStartMediaObject.ToString().ToLowerInvariant());

      // Replace {AutoStartMediaObjectInt} with "1" or "0".
      scriptOutput = scriptOutput.Replace("{AutoStartMediaObjectInt}", autoStartMediaObject ? "1" : "0");

      if (scriptOutput.Contains("{MediaObjectAbsoluteUrlNoHandler}"))
        scriptOutput = ReplaceMediaObjectAbsoluteUrlNoHandlerParameter(scriptOutput);

      if (scriptOutput.Contains("{MediaObjectRelativeUrlNoHandler}"))
        scriptOutput = ReplaceMediaObjectRelativeUrlNoHandlerParameter(scriptOutput);

      if (scriptOutput.Contains("{GalleryPath}"))
        scriptOutput = scriptOutput.Replace("{GalleryPath}", GalleryRoot);

      return scriptOutput;
    }

    /// <summary>
    /// Generate an absolute URL to the gallery object. The url can be assigned to the src attribute of an img tag.
    /// Ex: "http://site.com/gallery/gs/handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
    /// The query string parameter will be encrypted if that option is enabled. If the <see cref="GalleryObject" />
    /// is an album, the URL points to the album's thumbnail media object.
    /// </summary>
    /// <returns>Gets the absolute URL to the gallery object.</returns>
    public string GetMediaObjectUrl()
    {
      var queryString = String.Format(CultureInfo.InvariantCulture, "moid={0}&dt={1}&g={2}", MediaObjectId, (int)DisplayType, GalleryObject.GalleryId);

      // If necessary, encrypt, then URL encode the query string.
      if (AppSetting.Instance.EncryptMediaObjectUrlOnClient)
      {
        queryString = $"q={Utils.UrlEncode(HelperFunctions.Encrypt(queryString))}";
      }

      return String.Concat(HostUrl, GalleryRoot, "/handler/getmedia.ashx?", queryString);
    }

    /// <summary>
    /// Get an absolute URL for the page containing the current gallery object. The URL refers to the page
    /// specified in <see cref="DestinationPageUrl" />.
    /// Examples: "http://site.com/gallery/default.aspx?moid=283", "http://site.com/gallery/default.aspx?aid=97"
    /// </summary>
    /// <returns>Returns an absolute URL for the page containing the current gallery object..</returns>
    public string GetPageUrl()
    {
      if (GalleryObject is IAlbum)
      {
        return GetPageUrl(PageId.album, "aid={0}", GalleryObject.Id);
      }

      return GetPageUrl(PageId.mediaobject, "moid={0}", GalleryObject.Id);
    }

    /// <summary>
    /// Generates the HTML to display a nicely formatted thumbnail image, including a
    /// border, shadows and (possibly) rounded corners.
    /// </summary>
    /// <returns>Returns HTML that displays a nicely formatted thumbnail image.</returns>
    public string GetThumbnailHtml()
    {
      return String.Format(CultureInfo.InvariantCulture, @"
    <div class='gsp_i_c'>
      <img src='{1}' title='{2}' alt='{2}' style='width:{0}px;height:{3}px;' class='gsp_thmb_img' />
    </div>
",
                                  GalleryObject.Thumbnail.Width, // 0
                                  GetMediaObjectUrl(), // 1
                                  Utils.HtmlEncode(Utils.RemoveHtmlTags(GalleryObject.Title)), // 2
                                  GalleryObject.Thumbnail.Height // 3
        );
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Gets an instance of <see cref="MediaObjectHtmlBuilderOptions" /> that can be supplied to the 
    /// <see cref="MediaObjectHtmlBuilder" /> constructor. This method requires access to <see cref="HttpContext.Current" />.
    /// </summary>
    /// <param name="galleryObject">The gallery object. May be null. If null, <see cref="MediaObjectHtmlBuilderOptions.GalleryObject" />
    /// must be assigned before passing this instance to the <see cref="MediaObjectHtmlBuilder" /> constructor.</param>
    /// <param name="displayType">The display type. Optional. If not assigned or set to <see cref="DisplayObjectType.Unknown" />,
    /// <see cref="MediaObjectHtmlBuilderOptions.DisplayType" /> must be assigned before passing this instance to the 
    /// <see cref="MediaObjectHtmlBuilder" /> constructor.</param>
    /// <returns>An instance of <see cref="MediaObjectHtmlBuilderOptions" />.</returns>
    public static MediaObjectHtmlBuilderOptions GetMediaObjectHtmlBuilderOptions(IGalleryObject galleryObject, DisplayObjectType displayType = DisplayObjectType.Unknown)
    {
      return new MediaObjectHtmlBuilderOptions(galleryObject, displayType, Utils.GetBrowserIdsForCurrentRequest(), Utils.GetCurrentPageUrl(), Utils.GetHostUrl(), Utils.AppRoot, Utils.GalleryRoot);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Replace the replacement parameter {MediaObjectAbsoluteUrlNoHandler} with an URL that points directly to the media object
    /// (ex: /gallery/videos/birthdayvideo.wmv). A BusinessException is thrown if the media objects directory is not
    /// within the web application directory. Note that using this parameter completely bypasses the HTTP handler that 
    /// normally streams the media object. The consequence is that there is no security check when the media object request
    /// is made and no watermarks are applied, even if watermark functionality is enabled. This option should only be
    /// used when it is not important to restrict access to the media objects.
    /// </summary>
    /// <param name="htmlOutput">A string representing the HTML that will be sent to the browser for the current media object.
    /// It is based on the template stored in the media template table.</param>
    /// <returns>Returns the htmlOutput parameter with the {MediaObjectAbsoluteUrlNoHandler} string replaced by the URL to the media
    /// object.</returns>
    /// <exception cref="BusinessException">Thrown when the media objects 
    /// directory is not within the web application directory.</exception>
    private string ReplaceMediaObjectAbsoluteUrlNoHandlerParameter(string htmlOutput)
    {
      var appPath = AppSetting.Instance.PhysicalApplicationPath;

      if (!MediaObjectPhysicalPath.StartsWith(appPath, StringComparison.OrdinalIgnoreCase))
        throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Expected this.MediaObjectPhysicalPath (\"{0}\") to start with AppSetting.Instance.PhysicalApplicationPath (\"{1}\"), but it did not. If the media objects are not stored within the Gallery Server web application, you cannot use the MediaObjectAbsoluteUrlNoHandler replacement parameter. Instead, use MediaObjectRelativeUrlNoHandler and specify the virtual path to your media object directory in the HTML template. For example: HtmlTemplate=\"<a href=\"{{HostUrl}}/media{{MediaObjectRelativeUrlNoHandler}}\">Click to open</a>\"", MediaObjectPhysicalPath, appPath));

      var relativePath = MediaObjectPhysicalPath.Remove(0, appPath.Length).Trim(new[] { System.IO.Path.DirectorySeparatorChar });

      relativePath = Utils.UrlEncode(relativePath, '\\');

      var directUrl = String.Concat(Utils.UrlEncode(AppRoot, '/'), "/", relativePath.Replace("\\", "/"));

      return htmlOutput.Replace("{MediaObjectAbsoluteUrlNoHandler}", directUrl);
    }

    /// <summary>
    /// Replace the replacement parameter {MediaObjectRelativeUrlNoHandler} with an URL that is relative to the media objects
    /// directory and which points directly to the media object (ex: /videos/birthdayvideo.wmv). Note 
    /// that using this parameter completely bypasses the HTTP handler that normally streams the media object. The consequence 
    /// is that there is no security check when the media object request is made and no watermarks are applied, even if 
    /// watermark functionality is enabled. This option should only be used when it is not important to restrict access to 
    /// the media objects.
    /// </summary>
    /// <param name="htmlOutput">A string representing the HTML that will be sent to the browser for the current media object.
    /// It is based on the template stored in the media template table.</param>
    /// <returns>Returns the htmlOutput parameter with the {MediaObjectRelativeUrlNoHandler} string replaced by the URL to the media
    /// object.</returns>
    /// <exception cref="BusinessException">Thrown when the current media object's
    /// physical path does not start with the same text as AppSetting.Instance.MediaObjectPhysicalPath.</exception>
    /// <remarks>Typically this parameter is used instead of {MediaObjectAbsoluteUrlNoHandler} when the media objects directory 
    /// is outside of the web application. If the user wants to allow direct access to the media objects using this parameter, 
    /// she must first configure the media objects directory as a virtual directory in IIS. Then the path to this virtual directory 
    /// must be manually entered into one or more HTML templates, so that it prepends the relative url returned from this method.</remarks>
    /// <example>If the media objects directory has been set to D:\media and a virtual directory named gallery has been configured 
    /// in IIS that is accessible via http://yoursite.com/gallery, then you can configure the HTML template like this:
    /// HtmlTemplate="&lt;a href=&quot;http://yoursite.com/gallery{MediaObjectRelativeUrlNoHandler}&quot;&gt;Click to open&lt;/a&gt;"
    /// </example>
    private string ReplaceMediaObjectRelativeUrlNoHandlerParameter(string htmlOutput)
    {
      var moPath = Factory.LoadGallerySetting(GalleryObject.GalleryId).FullMediaObjectPath;

      if (!MediaObjectPhysicalPath.StartsWith(moPath, StringComparison.OrdinalIgnoreCase))
        throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Expected this.MediaObjectPhysicalPath (\"{0}\") to start with AppSetting.Instance.MediaObjectPhysicalPath (\"{1}\"), but it did not.", MediaObjectPhysicalPath, moPath));

      var relativePath = MediaObjectPhysicalPath.Remove(0, moPath.Length).Trim(new[] { System.IO.Path.DirectorySeparatorChar });

      relativePath = Utils.UrlEncode(relativePath, '\\');

      var relativeUrl = String.Concat("/", relativePath.Replace("\\", "/"));

      return htmlOutput.Replace("{MediaObjectRelativeUrlNoHandler}", relativeUrl);
    }

    /// <summary>
    /// Determines if the image can be displayed in a standard web browser. For example, JPG, JPEG, PNG and GIF images can
    /// be displayed; WMF and TIF cannot.
    /// </summary>
    /// <returns>Returns true if the image cannot be displayed in a standard browser (e.g. WMF, TIF); returns false if it can
    /// (e.g. JPG, JPEG, PNG and GIF).</returns>
    private bool IsImageBrowserIncompatible()
    {
      var extension = System.IO.Path.GetExtension(MediaObjectPhysicalPath);

      if (extension == null)
      {
        return false;
      }

      var originalFileExtension = extension.ToLowerInvariant();

      return Array.IndexOf(Factory.LoadGallerySetting(GalleryObject.GalleryId).ImageTypesStandardBrowsersCanDisplay, originalFileExtension) < 0;
    }

    /// <summary>
    /// Gets the HTML template to use for rendering this media object. Guaranteed to not
    /// return null.
    /// </summary>
    /// <returns>Returns a string.</returns>
    private string GetHtmlTemplate()
    {
      if (DisplayType == DisplayObjectType.External)
      {
        return DisplayObject.ExternalHtmlSource;
      }

      var isInQueue = (DisplayType == DisplayObjectType.Optimized &&
        (GalleryObject.GalleryObjectType == GalleryObjectType.Audio || GalleryObject.GalleryObjectType == GalleryObjectType.Video) &&
        MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(MediaObjectId));

      if (isInQueue)
      {
        return String.Format(CultureInfo.CurrentCulture, "<p class='gsp_item_process_msg'>{0}</p>", Resources.GalleryServer.UC_MediaObjectView_Media_Object_Being_Processed_Text);
      }

      var isBrowserIncompatibleImage = (MimeType.MajorType.Equals("IMAGE", StringComparison.OrdinalIgnoreCase)) && (IsImageBrowserIncompatible());

      var htmlOutput = HtmlTemplate;

      if (isBrowserIncompatibleImage || String.IsNullOrEmpty(htmlOutput))
      {
        // Either (1) no applicable template exists or (2) this is an image that can't be natively displayed in a 
        // browser (e.g. PSD, ICO, etc). Determine the appropriate message and return that as the HTML template.
        var url = Utils.AddQueryStringParameter(GetMediaObjectUrl(), "sa=1"); // Get URL with the "send as attachment" query string parm
        var msg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.UC_MediaObjectView_Browser_Cannot_Display_Media_Object_Text, url);
        return String.Format(CultureInfo.InvariantCulture, "<p class='gsp_msgfriendly'>{0}</p>", msg);
      }

      return htmlOutput;
    }


    /// <summary>
    /// Get an absolute URL for the requested page (eg. "http://site.com/gallery/default.aspx?moid=283").
    /// This works similar to <see cref="Utils.GetUrl(PageId, string, object[])" /> except this has no 
    /// dependence on <see cref="HttpContext.Current" /> and it returns an absolute URL instead of a relative one.
    /// </summary>
    /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
    /// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
    /// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
    /// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
    /// <returns>Returns an absolute URL for the requested <paramref name="page"/>.</returns>
    private string GetPageUrl(PageId page, string format, params object[] args)
    {
      var queryString = String.Format(CultureInfo.InvariantCulture, format, args);

      if ((page != PageId.album) && (page != PageId.mediaobject))
      {
        // Don't use the "g" parameter for album or mediaobject pages, since we can deduce it by looking for the 
        // aid or moid query string parms. This results in a shorter, cleaner URL.
        queryString = String.Concat("g=", page, "&", queryString);
      }

      return Utils.AddQueryStringParameter(String.Concat(HostUrl, DestinationPageUrl), queryString);
    }

    /// <summary>
    /// Gets the media object ID for the album thumbnail. Relevant only when <see cref="GalleryObject" /> is 
    /// an <see cref="IAlbum" /> and <see cref="DisplayType" /> is <see cref="DisplayObjectType.Thumbnail" />;
    /// otherwise a <see cref="WebException" /> is thrown.
    /// </summary>
    /// <returns>The media object ID for the album thumbnail.</returns>
    /// <exception cref="WebException">Thrown when <see cref="GalleryObject" /> is not an <see cref="IAlbum" /> 
    /// or <see cref="DisplayType" /> is not <see cref="DisplayObjectType.Thumbnail" /></exception>
    private int GetAlbumThumbnailId()
    {
      if (!(GalleryObject is IAlbum))
      {
        throw new WebException(String.Format("The function GetAlbumThumbnailId should be called only when the gallery object is an album. Instead, it was a {0}.", GalleryObject.GalleryObjectType));
      }

      if (DisplayType != DisplayObjectType.Thumbnail)
      {
        throw new WebException(String.Format("The function GetAlbumThumbnailId should be called only when the display type is DisplayObjectType.Thumbnail. Instead, it was a {0}.", DisplayType));
      }

      if (GalleryObject.Thumbnail.MediaObjectId > 0)
      {
        try
        {
          var mediaObject = Factory.LoadMediaObjectInstance(GalleryObject.Thumbnail.MediaObjectId);

          if ((!mediaObject.Parent.IsPrivate && !mediaObject.IsPrivate) || Utils.IsAuthenticated)
          {
            return mediaObject.Id;
          }
        }
        catch (InvalidMediaObjectException)
        {
          // We'll get here if the ID for the thumbnail doesn't represent an existing media object.
        }
      }

      return 0; // 0 is a signal to getmedia.ashx to generate an empty album thumbnail image
    }

    #endregion
  }

  /// <summary>
  /// Contains options that direct the creation of HTML and URLs for a media object.
  /// </summary>
  public class MediaObjectHtmlBuilderOptions
  {
    /// <summary>
    /// Gets or sets the gallery object. Must be assigned to a value before this instance can be passed to the 
    /// <see cref="MediaObjectHtmlBuilder" /> constructor.
    /// </summary>
    public IGalleryObject GalleryObject { get; set; }

    /// <summary>
    /// Gets or sets the display type. Must be assigned to a value other than <see cref="DisplayObjectType.Unknown" />
    /// before this instance can be passed to the <see cref="MediaObjectHtmlBuilder" /> constructor.
    /// </summary>
    public DisplayObjectType DisplayType { get; set; }

    /// <summary>
    /// Gets the browser IDs for current request.
    /// </summary>
    public Array Browsers { get; private set; }

    /// <summary>
    /// Gets or sets the URL, relative to the website root and optionally including any query string parameters,
    /// to the page any generated URLs should point to. Examples: "/dev/gs/gallery.aspx",
    /// "/dev/gs/gallery.aspx?g=admin_email&amp;aid=2389"
    /// </summary>
    public string DestinationPageUrl { get; set; }

    /// <summary>
    /// Gets the URI scheme, DNS host name or IP address, and port number for the current application. 
    /// Examples: "http://www.site.com", "http://localhost", "http://127.0.0.1", "http://godzilla"
    /// </summary>
    public string HostUrl { get; private set; }

    /// <summary>
    /// Gets the path, relative to the web site root, to the current web application.
    /// Example: "/dev/gallery".
    /// </summary>
    public string AppRoot { get; private set; }

    /// <summary>
    /// Gets the path, relative to the web site root, to the directory containing the Gallery Server user 
    /// controls and other resources. Example: "/dev/gallery/gs".
    /// </summary>
    public string GalleryRoot { get; private set; }

    /// <summary>
    /// Private constructor. Prevents a default instance of the <see cref="MediaObjectHtmlBuilderOptions"/> class from being created.
    /// </summary>
    private MediaObjectHtmlBuilderOptions() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaObjectHtmlBuilderOptions"/> class.
    /// </summary>
    /// <param name="galleryObject">The gallery object. May be null. If null, <see cref="MediaObjectHtmlBuilderOptions.GalleryObject" />
    /// must be assigned before passing this instance to the <see cref="MediaObjectHtmlBuilder" /> constructor.</param>
    /// <param name="displayType">The display type. Optional. If not assigned or set to <see cref="DisplayObjectType.Unknown" />,
    /// <see cref="MediaObjectHtmlBuilderOptions.DisplayType" /> must be assigned before passing this instance to the 
    /// <see cref="MediaObjectHtmlBuilder" /> constructor.</param>
    /// <param name="browsers">The browser IDs for current request.</param>
    /// <param name="destinationPageUrl">The URL, relative to the website root and optionally including any query string parameters,
    /// to the page any generated URLs should point to. Examples: "/dev/gs/gallery.aspx", 
    /// "/dev/gs/gallery.aspx?g=admin_email&amp;aid=2389"</param>
    /// <param name="hostUrl">The URI scheme, DNS host name or IP address, and port number for the current application. 
    /// Examples: "http://www.site.com", "http://localhost", "http://127.0.0.1", "http://godzilla"</param>
    /// <param name="appRoot">The path, relative to the web site root, to the current web application.
    /// Example: "/dev/gallery".</param>
    /// <param name="galleryRoot">The path, relative to the web site root, to the directory containing the Gallery Server user 
    /// controls and other resources. Example: "/dev/gallery/gs".</param>
    /// 
    public MediaObjectHtmlBuilderOptions(IGalleryObject galleryObject, DisplayObjectType displayType, Array browsers, string destinationPageUrl, string hostUrl, string appRoot, string galleryRoot)
    {
      GalleryObject = galleryObject;
      DisplayType = displayType;
      Browsers = browsers;
      DestinationPageUrl = destinationPageUrl;
      HostUrl = hostUrl;
      AppRoot = appRoot;
      GalleryRoot = galleryRoot;
    }
  }
}
