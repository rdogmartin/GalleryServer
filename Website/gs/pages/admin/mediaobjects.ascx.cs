using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using GalleryServer.WebControls;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering media object settings.
  /// </summary>
  public partial class mediaobjects : Pages.AdminPage
  {
    private bool _validateReadOnlyGalleryHasExecuted;
    private bool _validateReadOnlyGalleryResult;
    private bool _validatePathFailed;
    private readonly Dictionary<String, bool> _pathsThatHaveBeenTestedForWriteability = new Dictionary<string, bool>();

    #region Protected Events

    /// <summary>
    /// Handles the Init event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Init(object sender, EventArgs e)
    {
      this.AdminHeaderPlaceHolder = phAdminHeader;
      this.AdminFooterPlaceHolder = phAdminFooter;
    }

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

      ConfigureControlsEveryTime();

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }
    }

    /// <summary>
    /// Determines whether the event for the server control is passed up the page's UI server control hierarchy.
    /// </summary>
    /// <param name="source">The source of the event.</param>
    /// <param name="args">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
    /// <returns>
    /// true if the event has been canceled; otherwise, false. The default is false.
    /// </returns>
    protected override bool OnBubbleEvent(object source, EventArgs args)
    {
      //An event from the control has bubbled up.  If it's the Ok button, then run the
      //code to save the data to the database; otherwise ignore.
      Button btn = source as Button;
      if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
      {
        SaveSettings();
      }

      return true;
    }

    /// <summary>
    /// Handles the OnValidateControl event of the wwDataBinder control.
    /// </summary>
    /// <param name="item">The wwDataBindingItem item.</param>
    /// <returns>Returns <c>true</c> if the item is valid; otherwise returns <c>false</c>.</returns>
    protected bool wwDataBinder_ValidateControl(wwDataBindingItem item)
    {
      // Validate various settings to make sure they don't conflict with each other.

      if (item.ControlInstance == this.txtMoPath)
        return ValidateMediaObjectPath(item);

      else if (item.ControlInstance == this.txtThumbnailCachePath)
        return ValidateThumbnailPath(item);

      else if (item.ControlInstance == this.txtOptimizedCachePath)
        return ValidateOptimizedPath(item);

      else if (item.ControlInstance == this.chkPathIsReadOnly)
        return ValidateMediaObjectPath(item); // Validate the "media files are read-only" option

      return true;
    }

    private bool ValidateMediaObjectPath(wwDataBindingItem item)
    {
      string fullPathToTest = HelperFunctions.CalculateFullPath(AppSetting.Instance.PhysicalApplicationPath, this.txtMoPath.Text.Trim());

      return ValidatePath(item, fullPathToTest, this.lblMoPath, DisplayObjectType.Original);
    }

    private bool ValidateOptimizedPath(wwDataBindingItem item)
    {
      string pathToTest = (String.IsNullOrEmpty(this.txtOptimizedCachePath.Text.Trim()) ? this.txtMoPath.Text.Trim() : this.txtOptimizedCachePath.Text.Trim());

      string fullPathToTest = HelperFunctions.CalculateFullPath(AppSetting.Instance.PhysicalApplicationPath, pathToTest);

      return ValidatePath(item, fullPathToTest, this.lblOptimizedCachePath, DisplayObjectType.Optimized);
    }

    private bool ValidateThumbnailPath(wwDataBindingItem item)
    {
      string pathToTest = (String.IsNullOrEmpty(this.txtThumbnailCachePath.Text.Trim()) ? this.txtMoPath.Text.Trim() : this.txtThumbnailCachePath.Text.Trim());

      string fullPathToTest = HelperFunctions.CalculateFullPath(AppSetting.Instance.PhysicalApplicationPath, pathToTest);

      return ValidatePath(item, fullPathToTest, this.lblThumbnailCachePath, DisplayObjectType.Thumbnail);
    }

    /// <summary>
    /// Verifies the <paramref name="fullPathToTest" /> is valid and the logged-on user has the necessary permission to specify it.
    /// </summary>
    /// <param name="item">The binding item representing the control being tested.</param>
    /// <param name="fullPathToTest">The full file path the user wishes to use to store media objects, whether they are
    /// the original media object files, thumbnails, or optimized image files. 
    /// Examples: "C:\inetpub\wwwroot\galleryserverpro\myimages\", "C:/inetpub/wwwroot/galleryserverpro/myimages"</param>
    /// <param name="pathDisplayControl">The Label control used to display the full, calculated path.</param>
    /// <param name="displayType">Indicates whether the <paramref name="fullPathToTest" /> is the path for the original, thumbnail,
    /// or optimized media object files.</param>
    /// <returns>
    /// Returns <c>true</c> if the path is valid; otherwise returns <c>false</c>.
    /// </returns>
    private bool ValidatePath(wwDataBindingItem item, string fullPathToTest, Label pathDisplayControl, DisplayObjectType displayType)
    {
      if (_validatePathFailed)
      {
        // To help prevent repeated error messages (one each for original, thumbnail, and optimized), if a previous execution of
        // this test has failed, then let's just return true, thus allowing the user to focus on a single message.
        return true;
      }

      bool isValid;

      if (this.chkPathIsReadOnly.Checked)
      {
        if (displayType == DisplayObjectType.Original)
        {
          isValid = ValidateReadOnlyGallery(item) && ValidatePathIsReadable(item, fullPathToTest, pathDisplayControl);
        }
        else
        {
          isValid = ValidateReadOnlyGallery(item) && ValidatePathIsWritable(item, fullPathToTest);
        }
      }
      else
        isValid = ValidatePathIsWritable(item, fullPathToTest);

      isValid = isValid && ValidateUserHasPermissionToSpecifyPath(item, fullPathToTest);

      if (isValid)
      {
        pathDisplayControl.Text = fullPathToTest;
        pathDisplayControl.CssClass = "gsp_msgfriendly";
      }
      else
      {
        if (!this.GallerySettings.FullMediaObjectPath.Equals(fullPathToTest, StringComparison.OrdinalIgnoreCase))
        {
          pathDisplayControl.Text = String.Concat("&lt;", Resources.GalleryServer.Admin_MediaObjects_InvalidPath, "&gt;");
          pathDisplayControl.CssClass = "gsp_msgwarning";
        }
        _validatePathFailed = true;
      }

      return isValid;
    }

    /// <summary>
    /// Handles the OnBeforeUnBindControl event of the wwDataBinder control.
    /// </summary>
    /// <param name="item">The wwDataBindingItem item.</param>
    protected bool wwDataBinder_BeforeUnbindControl(WebControls.wwDataBindingItem item)
    {
      // Disabled HTML items are not posted during a postback, so we don't have accurate information about their states. 
      // Look for the checkboxes that cause other controls to be disabled, and assign the value of the disabled control to their
      // database setting. This allows disabled controls to retain their original value if an admin later re-enables them.
      if (!this.chkEnableSlideShow.Checked)
      {
        // When the slide show is unchecked, the slide show interval textbox is disabled via javascript.
        if (item.ControlId == this.txtSlideShowInterval.ID)
        {
          this.txtSlideShowInterval.Text = GallerySettings.SlideshowInterval.ToString(CultureInfo.CurrentCulture);
          return false;
        }
      }

      if (!this.chkEnableGoZipDownload.Checked)
      {
        // When the download ZIP objects feature is unchecked, the download albums in ZIP file checkbox is disabled via javascript.
        if (item.ControlId == this.chkEnableAlbumZipDownload.ID)
        {
          this.chkEnableAlbumZipDownload.Checked = GallerySettings.EnableAlbumZipDownload;
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Media_Objects_General_Page_Header;
      lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Media_Objects_General_Page_Header;

      if (AppSetting.Instance.License.LicenseType == LicenseLevel.TrialExpired)
      {
        ClientMessage = new ClientMessageOptions
                          {
                            Title = Resources.GalleryServer.Admin_Site_Settings_ProductKey_NotEntered_Label,
                            Message = Resources.GalleryServer.Admin_Need_Product_Key_Msg2,
                            Style = MessageStyle.Info
                          };

        OkButtonBottom.Enabled = false;
        OkButtonTop.Enabled = false;
      }

      this.wwDataBinder.DataBind();

      // Bind transition types. Exclude 'Transfer' since it requires parameters and will fail with the javascript that executes it.
      ddlTransType.DataSource = Enum.GetNames(typeof(MediaObjectTransitionType)).Where(sst => sst != MediaObjectTransitionType.Transfer.ToString()).ToList();
      ddlTransType.DataBind();

      // Bind slide show sizes.
      ddlMediaViewSize.Items.Add(new ListItem($"{DisplayObjectType.Thumbnail} - {GallerySettings.MaxThumbnailLength}{Resources.GalleryServer.Site_Pixel_Abbr}", DisplayObjectType.Thumbnail.ToString()));
      ddlMediaViewSize.Items.Add(new ListItem($"{DisplayObjectType.Optimized} - {GallerySettings.MaxOptimizedLength}{Resources.GalleryServer.Site_Pixel_Abbr}", DisplayObjectType.Optimized.ToString()));
      ddlMediaViewSize.Items.Add(new ListItem(DisplayObjectType.Original.ToString()));
      ddlMediaViewSize.DataBind();

      // Bind slide show types.
      ddlSlideShowType.DataSource = Enum.GetNames(typeof(SlideShowType)).Where(sst => sst != SlideShowType.NotSet.ToString()).ToList();
      ddlSlideShowType.DataBind();

      lblMoPath.Text = GallerySettings.FullMediaObjectPath;
      lblThumbnailCachePath.Text = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(GallerySettings.FullMediaObjectPath, GallerySettings.FullThumbnailPath, GallerySettings.FullMediaObjectPath);
      lblOptimizedCachePath.Text = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(GallerySettings.FullMediaObjectPath, GallerySettings.FullOptimizedPath, GallerySettings.FullMediaObjectPath);
    }

    private void SaveSettings()
    {
      this.wwDataBinder.Unbind(this);

      if (wwDataBinder.BindingErrors.Count > 0)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Validation_Summary_Text,
          Message = wwDataBinder.BindingErrors.ToString(),
          Style = MessageStyle.Error
        };

        return;
      }

      GallerySettingsUpdateable.Save();

      // In case the media objects path has changed, clear the inflated albums cache, since those albums hold a 
      // reference to the full path to the album/media asset.
      CacheController.RemoveInflatedAlbumsFromCache();

      ClientMessage = new ClientMessageOptions
      {
        Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
        Message = Resources.GalleryServer.Admin_Save_Success_Text,
        Style = MessageStyle.Success
      };
    }

    private static bool ValidatePathIsReadable(wwDataBindingItem item, string pathToTest, Label pathLabel)
    {
      // Verify that the IIS process identity has read permission to the specified path.
      bool isValid = false;

      string fullPhysicalPath = HelperFunctions.CalculateFullPath(AppSetting.Instance.PhysicalApplicationPath, pathToTest);
      try
      {
        HelperFunctions.ValidatePhysicalPathExistsAndIsReadable(fullPhysicalPath);
        isValid = true;
      }
      catch (CannotReadFromDirectoryException ex)
      {
        item.BindingErrorMessage = ex.Message;
      }

      if (isValid)
      {
        pathLabel.Text = fullPhysicalPath;
        pathLabel.CssClass = "gsp_msgfriendly";
      }
      else
      {
        pathLabel.Text = String.Concat("&lt;", Resources.GalleryServer.Admin_MediaObjects_InvalidPath, "&gt;");
        pathLabel.CssClass = "gsp_msgwarning";
      }

      return isValid;
    }

    private bool ValidatePathIsWritable(wwDataBindingItem item, string fullPathToTest)
    {
      // Verify that the IIS process identity has write permission to the specified path.

      // We only need to execute this once for each unique path. If we already tested this path, then return that test result. This helps
      // prevent the same error message from being shown multiple times.
      if (_pathsThatHaveBeenTestedForWriteability.ContainsKey(fullPathToTest))
        return _pathsThatHaveBeenTestedForWriteability[fullPathToTest];

      bool isValid = false;

      try
      {
        HelperFunctions.ValidatePhysicalPathExistsAndIsReadWritable(fullPathToTest);
        isValid = true;
      }
      catch (CannotWriteToDirectoryException ex)
      {
        item.BindingErrorMessage = ex.Message;
      }

      // Set the flag so we don't have to repeat the validation later in the page lifecycle.
      _pathsThatHaveBeenTestedForWriteability.Add(fullPathToTest, isValid);

      return isValid;
    }

    /// <summary>
    /// Verifies the currently logged-on user has permission to specify the <paramref name="mediaObjectPath"/> in this gallery. The
    /// path must not be used by any other galleries unless the user is a gallery admin for each of those galleries or a site
    /// admin. Returns <c>true</c> if the user has permission; otherwise returns <c>false</c>.
    /// </summary>
    /// <param name="item">The binding item representing the control being tested.</param>
    /// <param name="mediaObjectPath">The relative or full file path the user wishes to use to store media objects, whether they are
    /// the original media object files, thumbnails, or optimized image files. Relative paths should be relative
    /// to the root of the running application so that, when it is combined with physicalAppPath parameter, it creates a valid path.
    /// Examples: "C:\inetpub\wwwroot\galleryserverpro\myimages\", "C:/inetpub/wwwroot/galleryserverpro/myimages",
    /// "\myimages\", "\myimages", "myimages\", "myimages",	"/myimages/", "/myimages"</param>
    /// <returns>
    /// Returns <c>true</c> if the user has permission; otherwise returns <c>false</c>.
    /// </returns>
    private bool ValidateUserHasPermissionToSpecifyPath(wwDataBindingItem item, string mediaObjectPath)
    {
      if (UserCanAdministerSite)
        return true; // Site admins always have permission.

      if (!UserCanAdministerGallery)
        return false; // Must be at least a gallery admin. Kind of a redundant test but we include it for extra safety.

      string fullMediaObjectPath = HelperFunctions.CalculateFullPath(AppSetting.Instance.PhysicalApplicationPath, mediaObjectPath);

      bool isValid = true;

      // Get a list of galleries the current user is a gallery admin for.
      IGalleryCollection adminGalleries = UserController.GetGalleriesCurrentUserCanAdminister();

      // Iterate through each gallery and check to see if the path is used in it.
      foreach (IGallery gallery in Factory.LoadGalleries())
      {
        if (gallery.GalleryId == GalleryId)
          continue; // No need to evaluate the current gallery

        IGallerySettings gallerySettings = Factory.LoadGallerySetting(gallery.GalleryId);

        if ((fullMediaObjectPath.Equals(gallerySettings.FullMediaObjectPath, StringComparison.OrdinalIgnoreCase))
            || (fullMediaObjectPath.Equals(gallerySettings.FullThumbnailPath, StringComparison.OrdinalIgnoreCase))
            || (fullMediaObjectPath.Equals(gallerySettings.FullOptimizedPath, StringComparison.OrdinalIgnoreCase)))
        {
          // We found another gallery that is using this path. This is not valid unless the user is a gallery admin for it.
          if (!adminGalleries.Contains(gallery))
          {
            isValid = false;
            item.BindingErrorMessage = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_MediaObjects_MO_Path_Used_By_Another_Gallery, mediaObjectPath);
          }
        }
      }

      return isValid;
    }

    private bool ValidateReadOnlyGallery(wwDataBindingItem item)
    {
      // When a gallery is read only, the following must be true:
      // 1. The thumbnail and optimized path must be different than the media object path.
      // 2. The SynchAlbumTitleAndDirectoryName setting must be false.
      // 3. User albums must be disabled.

      // We only need to execute this once on a postback. If we already ran it, then return our previous result. This helps
      // prevent the same error message from being shown multiple times.
      if (_validateReadOnlyGalleryHasExecuted)
        return _validateReadOnlyGalleryResult;

      bool isValid = true;

      string mediaObjectPath = this.txtMoPath.Text;
      string thumbnailPath = (String.IsNullOrEmpty(this.txtThumbnailCachePath.Text) ? mediaObjectPath : this.txtThumbnailCachePath.Text);
      string optimizedPath = (String.IsNullOrEmpty(this.txtOptimizedCachePath.Text) ? mediaObjectPath : this.txtOptimizedCachePath.Text);

      // 1. The thumbnail and optimized path must be different than the media object path.
      if ((mediaObjectPath.Equals(thumbnailPath, StringComparison.OrdinalIgnoreCase)) ||
          (mediaObjectPath.Equals(optimizedPath, StringComparison.OrdinalIgnoreCase)))
      {
        isValid = false;
        item.BindingErrorMessage = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_MediaObjects_Cannot_Set_MO_Path_Read_Only_Cache_Location_Not_Set, mediaObjectPath, thumbnailPath, optimizedPath);
      }

      // 2. The SynchAlbumTitleAndDirectoryName setting must be false.
      if (chkSynchAlbumTitleAndDirectoryName.Checked)
      {
        isValid = false;
        item.BindingErrorMessage = Resources.GalleryServer.Admin_MediaObjects_Cannot_Set_MO_Path_Read_Only_Synch_Title_And_Directory_Enabled;
      }

      // 3. User albums must be disabled.
      if (GallerySettings.EnableUserAlbum)
      {
        isValid = false;
        item.BindingErrorMessage = Resources.GalleryServer.Admin_MediaObjects_Cannot_Set_MO_Path_Read_Only_User_Albums_Enabled;
      }

      // Set the flag so we don't have to repeat the validation later in the page lifecycle.
      this._validateReadOnlyGalleryHasExecuted = true;
      this._validateReadOnlyGalleryResult = isValid;

      return isValid;
    }

    #endregion
  }
}