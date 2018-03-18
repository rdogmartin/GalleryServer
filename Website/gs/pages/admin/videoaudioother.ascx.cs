using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering settings for video, audio, and generic media objects.
  /// </summary>
  public partial class videoaudioother : Pages.AdminPage
  {
    #region Event Handlers

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

    #region Properties

    /// <summary>
    /// Gets the string "Convert".
    /// </summary>
    /// <value>The string "Convert".</value>
    protected string EsConvertString
    {
      get { return Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_Convert; }
    }

    /// <summary>
    /// Gets the string "to".
    /// </summary>
    /// <value>The string "to".</value>
    protected string EsToString
    {
      get { return Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_To; }
    }

    /// <summary>
    /// Gets the encoder settings FFmpeg arguments string.
    /// </summary>
    /// <value>The encoder settings FFmpeg arguments string.</value>
    protected string EsFFmpegArgsString
    {
      get { return Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_FFmpegArgs; }
    }

    /// <summary>
    /// Gets the encoder settings move tooltip.
    /// </summary>
    /// <value>The encoder settings move tooltip.</value>
    protected string EsMoveTooltip
    {
      get { return Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_MoveTooltip; }
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Video_Audio_Other_General_Page_Header;
      lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));

      ConfigureEncoderStatus();
    }

    private void ConfigureEncoderStatus()
    {
      if (AppSetting.Instance.AppTrustLevel != ApplicationTrustLevel.Full)
      {
        lblEncoderStatus.Text = String.Format(Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_InsufficientTrust_Msg, AppSetting.Instance.AppTrustLevel.ToString().ToLowerInvariant());
        lblEncoderStatus.CssClass = "gsp_msgattention";
        return;
      }

      if (String.IsNullOrEmpty(AppSetting.Instance.FFmpegPath))
      {
        lblEncoderStatus.Text = Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_FFmpegMissing_Msg;
        lblEncoderStatus.CssClass = "gsp_msgattention";
        return;
      }

      lblEncoderStatus.Text = MediaConversionQueue.Instance.Status.ToString();
      lblEncoderStatus.CssClass = "gsp_msgfriendly";
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Video_Audio_Other_General_Page_Header;

      AssignHiddenFormFields();

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
    }

    private void AssignHiddenFormFields()
    {
      hdnEncoderSettings.Value = MediaEncoderSettingsController.ToEntities(GallerySettings.MediaEncoderSettings).ToJson();

      Entity.FileExtension[] destAvailableFileExtensions = MediaEncoderSettingsController.GetAvailableFileExtensions();
      var srcExtensions = new List<Entity.FileExtension>(destAvailableFileExtensions.Length + 2)
        {
          new Entity.FileExtension {Value = "*audio",Text = Resources.GalleryServer.Admin_VidAudOther_SourceFileExt_All_Audio},
          new Entity.FileExtension {Value = "*video",Text = Resources.GalleryServer.Admin_VidAudOther_SourceFileExt_All_Video}
        };

      srcExtensions.AddRange(destAvailableFileExtensions);

      hdnSourceAvailableFileExtensions.Value = srcExtensions.ToArray().ToJson();
      hdnDestinationAvailableFileExtensions.Value = destAvailableFileExtensions.ToJson();
    }

    private void SaveSettings()
    {
      string encoderSettingsStr = hdnEncoderSettings.Value;

      var encoderSettings = encoderSettingsStr.FromJson<Entity.MediaEncoderSettings[]>();

      GallerySettingsUpdateable.MediaEncoderSettings = MediaEncoderSettingsController.ToMediaEncoderSettingsCollection(encoderSettings);


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

      ClientMessage = new ClientMessageOptions
      {
        Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
        Message = Resources.GalleryServer.Admin_Save_Success_Text,
        Style = MessageStyle.Success
      };

      hdnEncoderSettings.Value = MediaEncoderSettingsController.ToEntities(GallerySettingsUpdateable.MediaEncoderSettings).ToJson();
    }

    #endregion
  }
}