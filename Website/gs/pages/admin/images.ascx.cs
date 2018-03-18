using System;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering image settings.
  /// </summary>
  public partial class images : Pages.AdminPage
  {
    #region Properties

    /// <summary>
    /// Gets the full file path to the current watermark image. Returns null when <see cref="IGallerySettings.WatermarkImagePath" /> is null or white space.
    /// Ex: "C:\Website\App_Data\Watermark_Images\2\logo.png"
    /// </summary>
    /// <value>The full file path to the current watermark image.</value>
    private string WatermarkImageFilepath
    {
      get
      {
        if (string.IsNullOrWhiteSpace(GallerySettingsUpdateable.WatermarkImagePath))
          return null;
        
        return Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.WatermarkDirectory, GalleryId.ToString(), GallerySettingsUpdateable.WatermarkImagePath);
      }
    }

    /// <summary>
    /// Gets a base-64 encoded string representing the watermark image. This value can be used in the src attribute of an img tag.
    /// Returns null when no watermark image has been specified.
    /// </summary>
    /// <value>A base-64 encoded string representing the watermark image.</value>
    private string WatermarkImageFileSource
    {
      get
      {
        if (!File.Exists(WatermarkImageFilepath))
        {
          return null;
        }

        using (var fs = new FileStream(WatermarkImageFilepath, FileMode.Open, FileAccess.Read))
        {
          var filebytes = new byte[fs.Length];
          fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
          return "data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None);
        }
      }
    }

    /// <summary>
    /// Gets the size, in bytes, of the watermark image file. Returns 0 when no watermark image has been specified.
    /// </summary>
    /// <value>The size, in bytes, of the watermark image file.</value>
    public long WatermarkImageFileSizeBytes
    {
      get
      {
        if (File.Exists(WatermarkImageFilepath))
        {
          return new FileInfo(WatermarkImageFilepath).Length;
        }

        return 0;
      }
    }

    #endregion

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

    #endregion

    #region Private Methods

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Images_General_Page_Header;
      lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));

      RegisterJavaScriptFiles();
    }

    private void RegisterJavaScriptFiles()
    {
      HtmlHead head = this.Page.Header;
      if (head == null)
        throw new WebException(Resources.GalleryServer.Error_Head_Tag_Missing_Server_Attribute_Ex_Msg);

      if (Utils.IsDebugEnabled) // debug="true" in web.config
      {
        var script = $@"
<script src='{Utils.GetUrl("/script/plupload/moxie.js")}'></script>
<script src='{Utils.GetUrl("/script/plupload/plupload.dev.js")}'></script>
<script src='{Utils.GetUrl("/script/plupload/jquery.ui.plupload.js")}'></script>";

        head.Controls.Add(new LiteralControl(script));
      }
      else
      {
        var script = $@"
<script src='{Utils.GetUrl("/script/plupload/plupload.full.min.js")}'></script>
<script src='{Utils.GetUrl("/script/plupload/jquery.ui.plupload.min.js")}'></script>";

        head.Controls.Add(new LiteralControl(script));
      }
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Images_General_Page_Header;

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

      ddlWmkTextLocation.DataSource = Enum.GetValues(typeof(System.Drawing.ContentAlignment));
      ddlWmkTextLocation.DataBind();

      ddlWmkImageLocation.DataSource = Enum.GetValues(typeof(System.Drawing.ContentAlignment));
      ddlWmkImageLocation.DataBind();

      hdnWatermarkTempFileName.Value = string.Empty;
      hdnWatermarkFileName.Value = GallerySettings.WatermarkImagePath;

      imgWatermarkImage.ImageUrl = WatermarkImageFileSource;
    }

    private void SaveSettings()
    {
      UnbindWatermarkImage();

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

      Factory.ClearWatermarkCache();
      imgWatermarkImage.ImageUrl = WatermarkImageFileSource;

      ClientMessage = new ClientMessageOptions
      {
        Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
        Message = Resources.GalleryServer.Admin_Save_Success_Text,
        Style = MessageStyle.Success
      };
    }

    /// <summary>
    /// Detect if user added, replaced, or removed the watermark image. If a new image has been uploaded, copy it to the destination
    /// directory. If one has been removed or replaced, delete the old one. Update <see cref="IGallerySettings.WatermarkImagePath" />.
    /// </summary>
    private void UnbindWatermarkImage()
    {
      var watermarkFileName = hdnWatermarkFileName.Value; // Set by JavaScript when user selected new image
      var watermarkTempFileName = hdnWatermarkTempFileName.Value; // Holds temp name of uploaded image in App_Data\_Temp
      var watermarkDirContainer = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.WatermarkDirectory); // e.g. "C:\Website\App_Data\Watermark_Images"
      var watermarkDir = Path.Combine(watermarkDirContainer, GalleryId.ToString()); // e.g. "C:\Website\App_Data\Watermark_Images\2"
      var currentWatermarkImageDestPath = Path.Combine(watermarkDir, GallerySettings.WatermarkImagePath); // e.g. "C:\Website\App_Data\Watermark_Images\2\logo.png"

      if (string.IsNullOrWhiteSpace(watermarkFileName))
      {
        // User removed watermark image or it has been left blank.
        if (File.Exists(currentWatermarkImageDestPath))
        {
          // Delete old watermark image from App_Data\Watermark_Images
          File.Delete(currentWatermarkImageDestPath);
        }

        GallerySettingsUpdateable.WatermarkImagePath = string.Empty;

        return;
      }

      if (!string.IsNullOrWhiteSpace(watermarkTempFileName))
      {
        // User uploaded a new watermark image. Copy it to the destination and update setting.
        var watermarkImageSourcePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.TempUploadDirectory, watermarkTempFileName);

        if (!Directory.Exists(watermarkDirContainer))
        {
          Directory.CreateDirectory(watermarkDirContainer); // Create Watermark_Images directory in App_Data
        }

        if (!Directory.Exists(watermarkDir))
        {
          Directory.CreateDirectory(watermarkDir); // Create {GalleryId}} directory in App_Data\Watermark_Images
        }

        var newWatermarkImageDestPath = Path.Combine(watermarkDir, watermarkFileName);

        if (File.Exists(watermarkImageSourcePath))
        {
          if (File.Exists(currentWatermarkImageDestPath))
          {
            File.Delete(currentWatermarkImageDestPath); // Delete the old watermark image
          }

          HelperFunctions.MoveFileSafely(watermarkImageSourcePath, newWatermarkImageDestPath);

          GallerySettingsUpdateable.WatermarkImagePath = watermarkFileName;
        }
      }
    }

    #endregion
  }
}