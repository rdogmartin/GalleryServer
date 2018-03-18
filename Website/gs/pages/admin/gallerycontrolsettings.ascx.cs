using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering gallery control settings.
  /// </summary>
  public partial class gallerycontrolsettings : Pages.AdminPage
  {
    #region Properties

    /// <summary>
    /// Gets or sets the name of the default album. This property is assigned during 
    /// <see cref="ConfigureDefaultAlbumComboBoxFirstTime" />, so be sure that function runs before
    /// accessing this value.
    /// </summary>
    /// <value>The name of the default album.</value>
    protected string DefaultAlbumTitle
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the text to use when no album has been assigned.
    /// </summary>
    private static string NoAlbumText
    {
      get { return Resources.GalleryServer.Admin_User_Settings_User_Album_Parent_Not_Assigned_Text; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether an error occurred while preparing the data to save.
    /// </summary>
    /// <value><c>true</c> if an error is preventing the data from being saved; otherwise, <c>false</c>.</value>
    private bool UnbindError { get; set; }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

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
      AdminHeaderPlaceHolder = phAdminHeader;
      AdminFooterPlaceHolder = phAdminFooter;
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
      PageTitle = Resources.GalleryServer.Admin_Gallery_Control_Settings_Page_Header;
      lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Control_Description_Label, GalleryControl.ClientID, Utils.GetCurrentPageUrl());
      tvUC.RequiredSecurityPermissions = SecurityActions.AdministerSite | SecurityActions.AdministerGallery;
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Gallery_Control_Settings_Page_Header;

      rbDefaultGallery.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Control_Settings_Default_Gallery_Label,
                                            Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));

      CheckForMessages();

      DataBindControlsFirstTime();
    }

    /// <summary>
    /// Determine if there are any messages we need to display to the user.
    /// </summary>
    private void CheckForMessages()
    {
      if (ClientMessage != null && ClientMessage.MessageId == MessageType.SettingsSuccessfullyChanged)
      {
        ClientMessage.Title = Resources.GalleryServer.Admin_Save_Success_Hdr;
        ClientMessage.Message = Resources.GalleryServer.Admin_Save_Success_Text;
      }

      if (GalleryControl.GalleryControlSettings.MediaObjectId.HasValue)
      {
        try
        {
          Factory.LoadMediaObjectInstance(GalleryControl.GalleryControlSettings.MediaObjectId.Value);
        }
        catch (InvalidMediaObjectException)
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = Resources.GalleryServer.Validation_Summary_Text,
            Message = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_Gallery_Control_Settings_Invalid_MediaObject_Msg, GalleryControl.GalleryControlSettings.MediaObjectId.Value),
            Style = MessageStyle.Error
          };
        }
      }
    }

    private void DataBindControlsFirstTime()
    {
      DataBindViewModeFirstTime();

      DataBindDefaultGalleryObjectFirstTime();

      DataBindBehaviorFirstTime();

      ConfigureDefaultAlbumComboBoxFirstTime();
    }

    private void DataBindViewModeFirstTime()
    {
      switch (GalleryControl.ViewMode)
      {
        case ViewMode.Multiple:
          rbViewModeMultiple.Checked = true;
          break;
        case ViewMode.Single:
          rbViewModeSingle.Checked = true;
          break;
      }
    }

    private void DataBindMediaViewSize()
    {
      // Select currently selected value or inherit from gallery setting if not specified
      var ssSize = (GalleryControl.MediaViewSize != DisplayObjectType.Unknown ? GalleryControl.MediaViewSize : GallerySettings.MediaViewSize);

      ddlMediaViewSize.Items.Add(new ListItem($"{DisplayObjectType.Thumbnail} - {GallerySettings.MaxThumbnailLength}{Resources.GalleryServer.Site_Pixel_Abbr}", DisplayObjectType.Thumbnail.ToString()));
      ddlMediaViewSize.Items.Add(new ListItem($"{DisplayObjectType.Optimized} - {GallerySettings.MaxOptimizedLength}{Resources.GalleryServer.Site_Pixel_Abbr}", DisplayObjectType.Optimized.ToString()));
      ddlMediaViewSize.Items.Add(new ListItem(DisplayObjectType.Original.ToString()));
      ddlMediaViewSize.DataBind();

      ddlMediaViewSize.SelectedIndex = ddlMediaViewSize.Items.IndexOf(ddlMediaViewSize.Items.FindByValue(ssSize.ToString()));
    }

    private void DataBindSlideShowType()
    {
      // Select currently selected value or inherit from gallery setting if not specified
      var ssType = (GalleryControl.SlideShowType != SlideShowType.NotSet ? GalleryControl.SlideShowType : GallerySettings.SlideShowType);

      ddlSlideShowType.DataSource = Enum.GetNames(typeof(SlideShowType)).Where(sst => sst != SlideShowType.NotSet.ToString()).ToList();
      ddlSlideShowType.DataBind();
      ddlSlideShowType.SelectedIndex = ddlSlideShowType.Items.IndexOf(ddlSlideShowType.Items.FindByValue(ssType.ToString()));
    }

    private void DataBindDefaultGalleryObjectFirstTime()
    {
      if (GalleryControl.GalleryControlSettings.AlbumId.HasValue)
      {
        rbDefaultAlbum.Checked = true;
      }
      else if (GalleryControl.GalleryControlSettings.MediaObjectId.HasValue)
      {
        rbDefaultMediaObject.Checked = true;
        txtDefaultMediaObjectId.Text = GalleryControl.GalleryControlSettings.MediaObjectId.Value.ToString(CultureInfo.InvariantCulture);
      }
      else
      {
        rbDefaultGallery.Checked = true;
      }
    }

    private void DataBindBehaviorFirstTime()
    {
      IGalleryControlSettings controlSettings = GalleryControl.GalleryControlSettings;

      chkAllowUrlOverride.Checked = (!controlSettings.AllowUrlOverride.HasValue || controlSettings.AllowUrlOverride.Value);

      DataBindOverridableSettingsFirstTime();

      SetOverrideCheckboxFirstTime();
    }

    /// <summary>
    /// Databind the control settings that override matching gallery-level settings.
    /// </summary>
    private void DataBindOverridableSettingsFirstTime()
    {
      chkShowLeftPaneForAlbum.Checked = ShowLeftPaneForAlbum;
      chkShowLeftPaneForMO.Checked = ShowLeftPaneForMediaObject;
      txtTreeviewNavigateUrl.Text = GalleryControl.TreeViewNavigateUrl;
      chkSlideShowLoop.Checked = SlideShowLoop;
      chkShowCenterPane.Checked = ShowCenterPane;
      chkShowRightPane.Checked = ShowRightPane;
      chkShowHeader.Checked = ShowHeader;
      txtGalleryTitle.Text = GalleryTitle;
      txtGalleryTitleUrl.Text = GalleryTitleUrl;
      chkShowLogin.Checked = ShowLogin;
      chkShowSearch.Checked = ShowSearch;
      chkShowRibbonToolbar.Checked = ShowRibbonToolbar;
      chkShowAlbumBreadcrumb.Checked = ShowAlbumBreadCrumb;
      chkAllowAnonBrowsing.Checked = AllowAnonymousBrowsing;
      chkShowMediaObjectNavigation.Checked = ShowMediaObjectNavigation;
      chkShowMediaObjectIndexPosition.Checked = ShowMediaObjectIndexPosition;
      chkShowMediaObjectTitle.Checked = ShowMediaObjectTitle;
      chkAutoPlaySlideshow.Checked = AutoPlaySlideShow;

      DataBindMediaViewSize();
      DataBindSlideShowType();
    }

    private void SetOverrideCheckboxFirstTime()
    {
      IGalleryControlSettings settings = GalleryControl.GalleryControlSettings;
      bool areAnyGallerySettingsOverridden = (settings.ShowLeftPaneForAlbum.HasValue || settings.ShowLeftPaneForMediaObject.HasValue ||
                                              settings.ShowCenterPane.HasValue || settings.ShowRightPane.HasValue || !String.IsNullOrEmpty(settings.TreeViewNavigateUrl) ||
                                              settings.ShowHeader.HasValue || !String.IsNullOrEmpty(settings.GalleryTitle) ||
                                              !String.IsNullOrEmpty(settings.GalleryTitleUrl) || settings.ShowLogin.HasValue || settings.ShowSearch.HasValue ||
                                              settings.AllowAnonymousBrowsing.HasValue || settings.ShowRibbonToolbar.HasValue || settings.ShowAlbumBreadCrumb.HasValue ||
                                              settings.ShowMediaObjectNavigation.HasValue || settings.ShowMediaObjectIndexPosition.HasValue ||
                                              settings.ShowMediaObjectTitle.HasValue || settings.AutoPlaySlideShow.HasValue ||
                                              settings.MediaViewSize != DisplayObjectType.Unknown || settings.SlideShowType != SlideShowType.NotSet ||
                                              settings.SlideShowLoop.HasValue
                                             );

      chkOverride.Checked = areAnyGallerySettingsOverridden;
    }

    private void ConfigureDefaultAlbumComboBoxFirstTime()
    {
      // Configure the album treeview ComboBox.
      int albumId = GalleryControl.GalleryControlSettings.AlbumId ?? 0;
      if (albumId > 0)
      {
        try
        {
          IAlbum albumToSelect = AlbumController.LoadAlbumInstance(albumId);
          DefaultAlbumTitle = albumToSelect.Title;
          tvUC.SelectedAlbumIds.Add(albumToSelect.Id);
        }
        catch (InvalidAlbumException)
        {
          DefaultAlbumTitle = Resources.GalleryServer.Admin_User_Settings_User_Album_Parent_Is_Invalid_Text;
        }
      }
      else
      {
        DefaultAlbumTitle = NoAlbumText;
      }
    }

    private void SaveSettings()
    {
      UnbindViewMode();

      UnbindDefaultGalleryObject();

      UnbindBehaviorSettings();

      if (!UnbindError)
      {
        GalleryControlSettingsUpdateable.Save();

        Factory.ClearGalleryControlSettingsCache();

        // Since we are changing settings that affect how and which controls are rendered to the page, let us redirect to the current page and
        // show the save success message. If we simply show a message without redirecting, two things happen: (1) the user doesn't see the effect
        // of their change until the next page load, (2) there is the potential for a viewstate validation error
        const MessageType msg = MessageType.SettingsSuccessfullyChanged;

        Utils.Redirect(PageId.admin_gallerycontrolsettings, "aid={0}&msg={1}", GetAlbumId(), ((int)msg).ToString(CultureInfo.InvariantCulture));
      }
    }

    private void UnbindViewMode()
    {
      if (rbViewModeMultiple.Checked && GalleryControl.GalleryControlSettings.ViewMode == ViewMode.NotSet)
      {
        // This setting remains at its default value, so don't set it.
        return;
      }

      if (rbViewModeMultiple.Checked)
      {
        GalleryControlSettingsUpdateable.ViewMode = ViewMode.Multiple;
      }
      else if (rbViewModeSingle.Checked)
      {
        GalleryControlSettingsUpdateable.ViewMode = ViewMode.Single;
      }
    }

    private void UnbindDefaultGalleryObject()
    {
      if (rbDefaultGallery.Checked)
      {
        ClearDefaultAlbum();
        GalleryControlSettingsUpdateable.MediaObjectId = null;
      }
      else if (rbDefaultAlbum.Checked)
      {
        if (tvUC.SelectedAlbum != null)
        {
          GalleryControlSettingsUpdateable.AlbumId = tvUC.SelectedAlbum.Id;
          GalleryControlSettingsUpdateable.MediaObjectId = null;
        }
        else
        {
          UnbindError = true;

          ClientMessage = new ClientMessageOptions
          {
            Title = Resources.GalleryServer.Validation_Summary_Text,
            Message = Resources.GalleryServer.Admin_Gallery_Control_Settings_InvalidAlbum_Msg,
            Style = MessageStyle.Error
          };
        }
      }
      else if (rbDefaultMediaObject.Checked)
      {
        ClearDefaultAlbum();

        int mediaObjectId;
        if (Int32.TryParse(txtDefaultMediaObjectId.Text, out mediaObjectId))
        {
          try
          {
            Factory.LoadMediaObjectInstance(mediaObjectId);

            GalleryControlSettingsUpdateable.MediaObjectId = mediaObjectId;
          }
          catch (InvalidMediaObjectException)
          {
            UnbindError = true;

            ClientMessage = new ClientMessageOptions
            {
              Title = Resources.GalleryServer.Validation_Summary_Text,
              Message = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_Gallery_Control_Settings_Invalid_MediaObject_Msg, mediaObjectId),
              Style = MessageStyle.Error
            };
          }
        }
        else
        {
          UnbindError = true;

          ClientMessage = new ClientMessageOptions
          {
            Title = Resources.GalleryServer.Validation_Summary_Text,
            Message = Resources.GalleryServer.Admin_Gallery_Control_Settings_InvalidMediaObject_Msg,
            Style = MessageStyle.Error
          };
        }
      }
    }

    private void ClearDefaultAlbum()
    {
      GalleryControlSettingsUpdateable.AlbumId = null;
      DefaultAlbumTitle = NoAlbumText;
      tvUC.SelectedAlbumIds.Clear();
    }

    private void UnbindBehaviorSettings()
    {
      GalleryControlSettingsUpdateable.AllowUrlOverride = chkAllowUrlOverride.Checked;

      if (chkOverride.Checked)
      {
        UnbindOverridableSettings();
      }
      else
      {
        SetOverridableSettingsToNull();
      }
    }

    private void UnbindOverridableSettings()
    {
      GalleryControlSettingsUpdateable.ShowLeftPaneForAlbum = chkShowLeftPaneForAlbum.Checked;
      GalleryControlSettingsUpdateable.ShowLeftPaneForMediaObject = chkShowLeftPaneForMO.Checked;
      GalleryControlSettingsUpdateable.TreeViewNavigateUrl = (String.IsNullOrEmpty(txtTreeviewNavigateUrl.Text) ? null : txtTreeviewNavigateUrl.Text);
      GalleryControlSettingsUpdateable.SlideShowLoop = chkSlideShowLoop.Checked;
      GalleryControlSettingsUpdateable.ShowCenterPane = chkShowCenterPane.Checked;
      GalleryControlSettingsUpdateable.ShowRightPane = chkShowRightPane.Checked;
      GalleryControlSettingsUpdateable.ShowHeader = chkShowHeader.Checked;

      if (chkShowHeader.Checked)
      {
        GalleryControlSettingsUpdateable.GalleryTitle = txtGalleryTitle.Text;
        GalleryControlSettingsUpdateable.GalleryTitleUrl = txtGalleryTitleUrl.Text;
        GalleryControlSettingsUpdateable.ShowLogin = chkShowLogin.Checked;
        GalleryControlSettingsUpdateable.ShowSearch = chkShowSearch.Checked;
        GalleryControlSettingsUpdateable.ShowRibbonToolbar = chkShowRibbonToolbar.Checked;
        GalleryControlSettingsUpdateable.ShowAlbumBreadCrumb = chkShowAlbumBreadcrumb.Checked;
      }
      else
      {
        txtGalleryTitle.Text = GallerySettingsUpdateable.GalleryTitle;
        txtGalleryTitleUrl.Text = GallerySettingsUpdateable.GalleryTitleUrl;
      }

      GalleryControlSettingsUpdateable.AllowAnonymousBrowsing = chkAllowAnonBrowsing.Checked;
      GalleryControlSettingsUpdateable.ShowMediaObjectNavigation = chkShowMediaObjectNavigation.Checked;
      GalleryControlSettingsUpdateable.ShowMediaObjectIndexPosition = chkShowMediaObjectIndexPosition.Checked;
      GalleryControlSettingsUpdateable.ShowMediaObjectTitle = chkShowMediaObjectTitle.Checked;
      GalleryControlSettingsUpdateable.AutoPlaySlideShow = chkAutoPlaySlideshow.Checked;

      UnbindMediaViewSize();
      UnbindSlideShowType();
    }

    private void UnbindMediaViewSize()
    {
      DisplayObjectType displayType;
      if (Enum.TryParse(ddlMediaViewSize.SelectedValue, false, out displayType))
      {
        GalleryControlSettingsUpdateable.MediaViewSize = displayType;
      }
    }

    private void UnbindSlideShowType()
    {
      SlideShowType sst;
      if (Enum.TryParse(ddlSlideShowType.SelectedValue, false, out sst))
      {
        GalleryControlSettingsUpdateable.SlideShowType = sst;
      }
    }

    private void SetOverridableSettingsToNull()
    {
      GalleryControlSettingsUpdateable.ShowLeftPaneForAlbum = null;
      GalleryControlSettingsUpdateable.ShowLeftPaneForMediaObject = null;
      GalleryControlSettingsUpdateable.ShowCenterPane = null;
      GalleryControlSettingsUpdateable.ShowRightPane = null;
      GalleryControlSettingsUpdateable.TreeViewNavigateUrl = null;
      GalleryControlSettingsUpdateable.SlideShowLoop = null;
      GalleryControlSettingsUpdateable.ShowHeader = null;
      GalleryControlSettingsUpdateable.GalleryTitle = null;
      GalleryControlSettingsUpdateable.GalleryTitleUrl = null;
      GalleryControlSettingsUpdateable.ShowLogin = null;
      GalleryControlSettingsUpdateable.ShowSearch = null;
      GalleryControlSettingsUpdateable.ShowRibbonToolbar = null;
      GalleryControlSettingsUpdateable.ShowAlbumBreadCrumb = null;
      GalleryControlSettingsUpdateable.AllowAnonymousBrowsing = null;
      GalleryControlSettingsUpdateable.ShowMediaObjectNavigation = null;
      GalleryControlSettingsUpdateable.ShowMediaObjectIndexPosition = null;
      GalleryControlSettingsUpdateable.ShowMediaObjectTitle = null;
      GalleryControlSettingsUpdateable.AutoPlaySlideShow = null;
      GalleryControlSettingsUpdateable.MediaViewSize = DisplayObjectType.Unknown;
      GalleryControlSettingsUpdateable.SlideShowType = SlideShowType.NotSet;
    }

    #endregion
  }
}