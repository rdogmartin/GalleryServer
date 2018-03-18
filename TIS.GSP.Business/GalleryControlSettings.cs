using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;

namespace GalleryServer.Business
{

  /// <summary>
  /// Represents a set of settings for a specific instance of a Gallery control.
  /// </summary>
  public class GalleryControlSettings : IGalleryControlSettings
  {
    #region Private Fields

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GalleryControlSettings"/> class.
    /// </summary>
    /// <param name="id">The value that uniquely identifies the gallery control setting.</param>
    /// <param name="controlId">The value that uniquely identifies the Gallery control. This is a concatenation of the relative
    /// path to the control and its client ID. For example: "\default.aspx|gsp"</param>
    internal GalleryControlSettings(int id, string controlId)
    {
      this.GalleryControlSettingId = id;
      this.ControlId = controlId;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the ID for the gallery control setting.
    /// </summary>
    /// <value>The gallery control setting ID.</value>
    public int GalleryControlSettingId { get; set; }

    /// <summary>
    /// Gets or sets the value that uniquely identifies the Gallery control. This is a concatenation of the full physical
    /// path to the control and its client ID. For example: "~/Default.aspx|gsp"
    /// </summary>
    /// <value>The value that uniquely identifies the Gallery control.</value>
    public string ControlId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the gallery associated with the control.
    /// </summary>
    /// <value>The gallery ID.</value>
    public int? GalleryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the header at the top of the gallery. If not specified, the application
    /// uses <see cref="IGallerySettings.ShowHeader"/>; when specified, this property overrides it. The header includes the
    /// gallery title, login/logout controls, and search function. The login/logout controls and search function can be individually
    /// controlled via the <see cref="ShowLogin"/> and <see cref="ShowSearch"/> properties.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the header is to be displayed; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowHeader { get; set; }

    /// <summary>
    /// Gets or sets the header text that appears at the top of each web page. Requires that <see cref="IGalleryControlSettings.ShowHeader" /> be set to
    /// <c>true</c> in order to be visible. If not specified, the application uses <see cref="IGallerySettings.GalleryTitle" />;
    /// when specified, this property overrides it.
    /// </summary>
    /// <value>The gallery title.</value>
    public string GalleryTitle { get; set; }

    /// <summary>
    /// Gets or sets the URL the user will be directed to when she clicks the gallery title. Optional. If not 
    /// present, no link will be rendered. Examples: "http://www.mysite.com", "/" (the root of the web site),
    /// "~/" (the top level album). If not specified, the application uses <see cref="IGallerySettings.GalleryTitleUrl" />;
    /// when specified, this property overrides it.
    /// </summary>
    /// <value>The gallery title URL.</value>
    public string GalleryTitleUrl { get; set; }

    /// <summary>
    /// Indicates whether to show the login controls at the top right of each page. When false, no login controls
    /// are shown, but the user can navigate directly to the login page to log on. If not specified, the application
    /// uses <see cref="IGallerySettings.ShowLogin"/>; when specified, this property overrides it.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if login controls are visible; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowLogin { get; set; }

    /// <summary>
    /// Indicates whether to show the search box at the top right of each page. If not specified, the application
    /// uses <see cref="IGallerySettings.ShowSearch"/>; when specified, this property overrides it.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the search box is visible; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowSearch { get; set; }

    /// <summary>
    /// Gets or sets the ID of the album to be displayed. This setting can be used to specify that a particular album be displayed. When
    /// specified, the <see cref="IGalleryControlSettings.GalleryId" /> is ignored. Only one of these properties should be set: <see cref="IGalleryControlSettings.GalleryId" />, 
    /// <see cref="IGalleryControlSettings.AlbumId" />, <see cref="IGalleryControlSettings.MediaObjectId" />.
    /// </summary>
    /// <value>The album ID.</value>
    public int? AlbumId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the media object to be displayed. This setting can be used to specify that a particular media object be displayed. When
    /// specified, the <see cref="IGalleryControlSettings.GalleryId" /> is ignored and the <see cref="IGalleryControlSettings.ViewMode" /> is 
    /// automatically set to <see cref="Business.ViewMode.Single" />. Only one of these properties should be set: 
    /// <see cref="IGalleryControlSettings.GalleryId" />, <see cref="IGalleryControlSettings.AlbumId" />, <see cref="IGalleryControlSettings.MediaObjectId" />.
    /// </summary>
    /// <value>The media object ID.</value>
    public int? MediaObjectId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating how the media objects are to be rendered in the browser. The default value is
    /// <see cref="Business.ViewMode.Multiple" />. When the value is <see cref="Business.ViewMode.Multiple" />,
    /// the contents of an album are shown as a set of thumbnail images. When set to <see cref="Business.ViewMode.Single" />, 
    /// a single media object is displayed. When set to <see cref="Business.ViewMode.SingleRandom" />, a single media object 
    /// is displayed that is randomly selected. When a <see cref="IGalleryControlSettings.MediaObjectId" /> is specified, the 
    /// <see cref="IGalleryControlSettings.ViewMode" /> is automatically set to <see cref="Business.ViewMode.Single" />.
    /// </summary>
    /// <value>A value indicating how the media objects are to be rendered in the browser.</value>
    public ViewMode ViewMode { get; set; }

    /// <summary>
    /// Gets or sets the base URL to invoke when a tree node is clicked.
    /// The album ID of the selected album is passed to the URL as the query string parameter "aid".
    /// Example: "Gallery.aspx, http://site.com/gallery.aspx"
    /// </summary>
    public string TreeViewNavigateUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether users can view galleries without logging in. When false, users are redirected to a login
    /// page when any album is requested. Private albums are never shown to anonymous users, even when this property is true. If not 
    /// specified, the application uses <see cref="IGallerySettings.AllowAnonymousBrowsing" />; when specified, this property overrides it.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if anonymous users can view the gallery; otherwise, <c>false</c>.
    /// </value>
    public bool? AllowAnonymousBrowsing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the left pane when an album is being displayed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the left pane is to be rendered; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowLeftPaneForAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the left pane when a single media object is being displayed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the left pane is to be rendered when a single media object is being displayed; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowLeftPaneForMediaObject { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the center pane of the user interface.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the center pane of the user interface is to be displayed; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowCenterPane { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the right pane of the user interface.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the right pane of the user interface is to be displayed; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowRightPane { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the ribbon toolbar. If not specified, the application uses a default value of <c>true</c>.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the ribbon toolbar is to be rendered; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowRibbonToolbar { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render the album bread crumb links, including the Actions menu.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the album bread crumb links are to be visible; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowAlbumBreadCrumb { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the title is displayed beneath individual media objects.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the title is displayed beneath individual media objects; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowMediaObjectTitle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the next and previous buttons are rendered for individual media objects.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the next and previous buttons are rendered for individual media objects; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowMediaObjectNavigation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to display the relative position of a media object within an album (example: (3 of 24)). 
    /// Applicable only when a single media object is displayed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the relative position of a media object within an album is to be rendered; otherwise, <c>false</c>.
    /// </value>
    public bool? ShowMediaObjectIndexPosition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a slide show of media objects automatically starts playing when the page loads. The 
    /// default value is <c>false</c>. This setting applies only when the <see cref="IGalleryControlSettings.ViewMode" /> is set to ViewMode.Single or ViewMode.SingleRandom
    /// and either an album or media object is specified (that is, the <see cref="IGalleryControlSettings.AlbumId" /> or <see cref="IGalleryControlSettings.MediaObjectId" /> is assigned a value). 
    /// If a media object is specified, all images in the object's album will be shown in the slide show.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if a slide show of media objects will automatically start playing; otherwise, <c>false</c>.
    /// </value>
    public bool? AutoPlaySlideShow { get; set; }

    /// <summary>
    /// Gets or sets the size of media assets to display when viewing a single media asset. The default value is <see cref="Business.DisplayObjectType.Unknown" />.
    /// </summary>
    public DisplayObjectType MediaViewSize { get; set; }

    /// <summary>
    /// Gets or sets the type of the slide show. The default value is <see cref="Business.SlideShowType.NotSet" />.
    /// </summary>
    /// <value>The type of the slide show.</value>
    public SlideShowType SlideShowType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a slide show continues from the beginning after showing the last media asset. When <c>false</c>, the user is
    /// redirected to the album page when the slide show ends. The default value is <c>false</c>.
    /// </summary>
    /// <value><c>true</c> when the slide show loops; otherwise <c>false</c>.</value>
    public bool? SlideShowLoop { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an album or media object specified in the URL can override the <see cref="IGalleryControlSettings.GalleryId" />,
    /// <see cref="IGalleryControlSettings.AlbumId" />, and <see cref="IGalleryControlSettings.MediaObjectId" /> properties of this control. Use the query string parameter "aid" to 
    /// specify an album; use "moid" for a media object (example: default.aspx?aid=12 for album ID=12, default.aspx?moid=37 for media
    /// object ID=37)
    /// </summary>
    /// <value><c>true</c> if an album or media object specified in the query string can override one specified as a control property; otherwise,
    ///  <c>false</c>.</value>
    public bool? AllowUrlOverride { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Persist the current gallery control settings to the data store.
    /// </summary>
    public void Save()
    {
      //Factory.GetDataProvider().GalleryControlSetting_Save(this);
      using (var repo = new GalleryControlSettingRepository())
      {
        repo.Save(this);
      }

      // Clear the settings stored in static variables so they are retrieved from the data store during the next access.
      Factory.ClearGalleryControlSettingsCache();
    }

    /// <summary>
    /// Delete the current gallery control settings from the data store.
    /// </summary>
    public void Delete()
    {
      // Set the view mode to ViewMode.NotSet, slide show type to SlideShowType.NotSet, and all nullable 
      // properties to null (except for the ControlId, which we need to identify the record to delete.
      // Then call save. This causes the matching records to get deleted from the data store.
      this.ViewMode = ViewMode.NotSet;
      this.SlideShowType = SlideShowType.NotSet;
      this.MediaViewSize = DisplayObjectType.Unknown;

      var propertiesToExclude = new string[] { "ControlId" };

      Type gsType = this.GetType();

      foreach (PropertyInfo prop in gsType.GetProperties())
      {
        if (Array.IndexOf<string>(propertiesToExclude, prop.Name) >= 0)
        {
          continue; // Skip this one.
        }

        bool isString = (prop.PropertyType == typeof(string));
        bool isNullableGeneric = (prop.PropertyType.IsGenericType && (prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)));

        if (isString || isNullableGeneric)
        {
          prop.SetValue(this, null, null); // Set to null
        }
      }

      Save();
    }

    /// <summary>
    /// Retrieves the gallery control settings from the data store for all controls containing galleries.
    /// </summary>
    /// <returns>Returns an <see cref="IGalleryControlSettingsCollection" /> containing the settings for all controls containing galleries.</returns>
    internal static IGalleryControlSettingsCollection RetrieveGalleryControlSettingsFromDataStore()
    {
      IGalleryControlSettingsCollection gallerySettings = new GalleryControlSettingsCollection();
      IGalleryControlSettings gs = null;
      string prevControlId = null;

      Type gsType = typeof(GalleryControlSettings);

      // Loop through each gallery control setting and assign to the relevant property. When we encounter a record with a new control ID, 
      // automatically create a new GalleryControlSetting instance and start populating that one. When we are done with the loop we will
      // have created one GalleryControlSetting instance for each control that contains a gallery.

      // SQL:
      // SELECT
      //  GalleryControlSettingId, ControlId, SettingName, SettingValue
      // FROM [gs_GalleryControlSetting]
      // ORDER BY ControlId;
      using (var repo = new GalleryControlSettingRepository())
      {
        foreach (GalleryControlSettingDto gcsDto in repo.GetAll().OrderBy(g => g.ControlId))
        {
          #region Check for new gallery

          string currControlId = gcsDto.ControlId.Trim();

          if (String.IsNullOrEmpty(prevControlId) || (!currControlId.Equals(prevControlId)))
          {
            // We have encountered settings for a new gallery. Create a new object and add it to our collection.
            gs = new GalleryControlSettings(gcsDto.GalleryControlSettingId, currControlId);

            gallerySettings.Add(gs);

            prevControlId = currControlId;
          }

          #endregion

          #region Assign property

          // For each setting in the data store, find the matching property and assign the value to it.
          string settingName = gcsDto.SettingName.Trim();

          PropertyInfo prop = gsType.GetProperty(settingName);

          if (prop == null)
          {
            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture, "Invalid gallery control setting. A gallery control setting named '{0}' was found in the data store, but no property by that name exists in the class '{1}'. Check the gallery control settings in the data store to ensure they are correct.", settingName, gsType));
          }
          else if (prop.PropertyType == typeof(bool?))
          {
            prop.SetValue(gs, Convert.ToBoolean(gcsDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
          }
          else if (prop.PropertyType == typeof(bool))
          {
            prop.SetValue(gs, Convert.ToBoolean(gcsDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
          }
          else if (prop.PropertyType == typeof(string))
          {
            prop.SetValue(gs, Convert.ToString(gcsDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
          }
          else if (prop.PropertyType == typeof(int))
          {
            prop.SetValue(gs, Convert.ToInt32(gcsDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
          }
          else if (prop.PropertyType == typeof(int?))
          {
            prop.SetValue(gs, Convert.ToInt32(gcsDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
          }
          else if (prop.PropertyType == typeof(Single))
          {
            prop.SetValue(gs, Convert.ToSingle(gcsDto.SettingValue.Trim(), CultureInfo.InvariantCulture), null);
          }
          else if (prop.PropertyType == typeof(ViewMode))
          {
            AssignViewModeProperty(gs, prop, gcsDto.SettingValue.Trim());
          }
          else if (prop.PropertyType == typeof(DisplayObjectType))
          {
            AssignDisplayObjectTypeTypeProperty(gs, prop, gcsDto.SettingValue.Trim());
          }
          else if (prop.PropertyType == typeof(SlideShowType))
          {
            AssignSlideShowTypeProperty(gs, prop, gcsDto.SettingValue.Trim());
          }
          else if (prop.PropertyType == typeof(String[]))
          {
            // Parse comma-delimited string to array
            string[] strings = gcsDto.SettingValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Trim any leading and trailing spaces
            for (int i = 0; i < strings.Length; i++)
            {
              strings[i] = strings[i].Trim();
            }

            prop.SetValue(gs, strings, null);
          }
          else
          {
            throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GalleryControlSettings.RetrieveGalleryControlSettingsFromDataStore is not designed to process a property of type {0} (encountered in GalleryControlSettings.{1})", prop.PropertyType, prop.Name));
          }

          #endregion
        }
      }

      return gallerySettings;
    }

    /// <summary>
    /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="galleryControlSetting" />
    /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
    /// </summary>
    /// <param name="galleryControlSetting">The gallery control setting instance containing the <paramref name="property" /> to assign.</param>
    /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
    /// <param name="value">The value to assign to the <paramref name="property" />.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
    /// <see cref="ViewMode" /> value.</exception>
    private static void AssignViewModeProperty(IGalleryControlSettings galleryControlSetting, PropertyInfo property, string value)
    {
      ViewMode viewMode;

      try
      {
        viewMode = (ViewMode)Enum.Parse(typeof(ViewMode), value, true);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GalleryControlSettings.AssignViewModeProperty cannot convert the string {0} to a ViewMode enumeration value. The following values are valid: NotSet, Multiple, Single, SingleRandom", value), ex);
      }

      property.SetValue(galleryControlSetting, viewMode, null);
    }

    /// <summary>
    /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="galleryControlSetting" />
    /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
    /// </summary>
    /// <param name="galleryControlSetting">The gallery control setting instance containing the <paramref name="property" /> to assign.</param>
    /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
    /// <param name="value">The value to assign to the <paramref name="property" />.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
    /// <see cref="DisplayObjectType" /> value.</exception>
    private static void AssignDisplayObjectTypeTypeProperty(IGalleryControlSettings galleryControlSetting, PropertyInfo property, string value)
    {
      DisplayObjectType displayType;

      try
      {
        displayType = (DisplayObjectType)Enum.Parse(typeof(DisplayObjectType), value, true);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GalleryControlSettings.AssignDisplayObjectTypeTypeProperty cannot convert the string {0} to a DisplayObjectType enumeration value. The following values are valid: Unknown, Thumbnail, Optimized, Original, External", value), ex);
      }

      property.SetValue(galleryControlSetting, displayType, null);
    }

    /// <summary>
    /// Assigns the <paramref name="value" /> to the specified <paramref name="property" /> of the <paramref name="galleryControlSetting" />
    /// instance. The <paramref name="value" /> is converted to the appropriate enumeration before assignment.
    /// </summary>
    /// <param name="galleryControlSetting">The gallery control setting instance containing the <paramref name="property" /> to assign.</param>
    /// <param name="property">The property to assign the <paramref name="value" /> to.</param>
    /// <param name="value">The value to assign to the <paramref name="property" />.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> cannot be parsed into a
    /// <see cref="SlideShowType" /> value.</exception>
    private static void AssignSlideShowTypeProperty(IGalleryControlSettings galleryControlSetting, PropertyInfo property, string value)
    {
      SlideShowType viewMode;

      try
      {
        viewMode = (SlideShowType)Enum.Parse(typeof(SlideShowType), value, true);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "GalleryControlSettings.AssignSlideShowTypeProperty cannot convert the string {0} to a SlideShowType enumeration value. The following values are valid: NotSet, Inline, FullScreen", value), ex);
      }

      property.SetValue(galleryControlSetting, viewMode, null);
    }

    #endregion
  }
}
