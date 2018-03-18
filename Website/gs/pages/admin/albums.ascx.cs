using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Web.Controller;
using GalleryServer.WebControls;
using PagerPosition = GalleryServer.Business.PagerPosition;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering album settings.
  /// </summary>
  public partial class albums : Pages.AdminPage
  {
    #region Properties

    /// <summary>
    /// Gets the location to the URL for remotely invoking a synchronization.
    /// </summary>
    /// <value>A string.</value>
    protected string SyncAlbumUrl
    {
      get
      {
        return String.Format(CultureInfo.InvariantCulture, "{0}/api/task/startsync?albumId={1}&isRecursive=false&rebuildThumbnails=false&rebuildOptimized=false&password={2}", Utils.GetAppUrl(), this.GetAlbumId(), Utils.UrlEncode(txtRemoteAccessPassword.Text));
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
    /// <param name="args">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    /// <returns>true if the event has been canceled; otherwise, false. The default is false.</returns>
    protected override bool OnBubbleEvent(object source, EventArgs args)
    {
      //An event from the control has bubbled up.  If it's the Ok button, then run the
      //code to save the data to the database; otherwise ignore.
      Button btn = source as Button;
      if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
      {
        SaveSettings();

        // When paging is disabled, we store "0" in the config file, but we want to display an empty string
        // in the page size textbox. The event wwDataBinder_BeforeUnbindControl may have put a "0" in the 
        // textbox, so undo that now.
        if (txtPageSize.Text == "0")
          txtPageSize.Text = String.Empty;
      }

      return true;
    }

    /// <summary>
    /// Handles the ServerValidate event of the CompareValidator control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
    protected void cvColor_ServerValidate(object sender, ServerValidateEventArgs args)
    {
      try
      {
        HelperFunctions.GetColor(args.Value.Trim());
        args.IsValid = true;
      }
      catch (ArgumentNullException) { args.IsValid = false; }
      catch (ArgumentOutOfRangeException) { args.IsValid = false; }
    }

    /// <summary>
    /// Handles the OnAfterBindControl event of the wwDataBinder control.
    /// </summary>
    /// <param name="item">The wwDataBindingItem item.</param>
    protected void wwDataBinder_AfterBindControl(wwDataBindingItem item)
    {
      if (item.ControlInstance == txtPageSize)
      {
        int pageSize = Convert.ToInt32(this.txtPageSize.Text, CultureInfo.CurrentCulture);
        if (pageSize == 0)
        {
          // Disable the checkbox because feature is turned off (a "0" indicates it is off). Set textbox to
          // an empty string because we don't want to display 0.
          chkEnablePaging.Checked = false;
          txtPageSize.Text = String.Empty;
        }
        else if (pageSize > 0)
          chkEnablePaging.Checked = true; // Select the checkbox when max # of items is > 0
        else
        {
          // We'll never get here because the config definition uses an IntegerValidator to force the number
          // to be greater than 0.
        }
      }

      if (item.ControlInstance == lblLastAutoSync)
      {
        if (GallerySettings.LastAutoSync == DateTime.MinValue)
        {
          lblLastAutoSync.Text = Resources.GalleryServer.Admin_Albums_LastAutoSync_Never_Lbl;
        }
      }
    }

    /// <summary>
    /// Handles the OnBeforeUnBindControl event of the wwDataBinder control.
    /// </summary>
    /// <param name="item">The wwDataBindingItem item.</param>
    protected bool wwDataBinder_BeforeUnbindControl(wwDataBindingItem item)
    {
      if (!this.chkEnablePaging.Checked)
      {
        // When paging is disabled, we store "0" in the config file.
        if (item.ControlId == this.txtPageSize.ID)
        {
          txtPageSize.Text = "0";
          return true; // true indicates that we want to save this setting
        }

        // Disabled HTML items are not posted during a postback, so we don't have accurate information about their states. 
        // Look for the checkboxes that cause other controls to be disabled, and assign the value of the disabled control to their
        // database setting. This allows disabled controls to retain their original value if an admin later re-enables them.
        if (item.ControlId == this.ddlPagerLocation.ID)
        {
          this.ddlPagerLocation.SelectedValue = GallerySettingsUpdateable.PagerLocation.ToString();
          return false;
        }
      }

      if (!this.chkEnableAutoSync.Checked)
      {
        // When the auto-sync feature is unchecked, the interval textbox is disabled via javascript.
        if (item.ControlId == this.txtAutoSyncIntervalMinutes.ID)
        {
          this.txtAutoSyncIntervalMinutes.Text = GallerySettings.AutoSyncIntervalMinutes.ToString(CultureInfo.CurrentCulture);
          return false;
        }
      }

      if (!this.chkEnableRemoteSync.Checked)
      {
        // When the remote sync feature is unchecked, the remote access password textbox is disabled via javascript.
        if (item.ControlId == this.txtRemoteAccessPassword.ID)
        {
          this.txtRemoteAccessPassword.Text = GallerySettings.RemoteAccessPassword;
          return false;
        }
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
      if (item.ControlInstance == txtPageSize)
      {
        if ((chkEnablePaging.Checked) && (Convert.ToInt32(txtPageSize.Text, CultureInfo.CurrentCulture) <= 0))
        {
          item.BindingErrorMessage = Resources.GalleryServer.Admin_Error_Invalid_PageSize_Msg;
          return false;
        }
      }

      if (item.ControlInstance == txtRemoteAccessPassword)
      {
        if (chkEnableRemoteSync.Checked && String.IsNullOrEmpty(txtRemoteAccessPassword.Text))
        {
          item.BindingErrorMessage = Resources.GalleryServer.Admin_Albums_RemoteAccessPassword_Required_Msg;
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Albums_General_Page_Header;
      lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Albums_General_Page_Header;

      DataBindControlsFirstTime();
    }

    private void DataBindControlsFirstTime()
    {
      this.wwDataBinder.DataBind();

      ddlPagerLocation.DataSource = Enum.GetValues(typeof(PagerPosition));
      ddlPagerLocation.DataBind();

      ddlSortField.DataTextField = "Value";
      ddlSortField.DataValueField = "Key";
      ddlSortField.DataSource = GetActiveMetaItemNames();
      ddlSortField.DataBind();

      ddlSortDirection.SelectedIndex = (GallerySettingsUpdateable.DefaultAlbumSortAscending ? 0 : 1);
    }

    /// <summary>
    /// Gets the meta item names that are currently marked as visible for media objects or albums. Include the one assigned to 
    /// <see cref="IGallerySettings.DefaultAlbumSortMetaName" /> even if not currently visible.
    /// </summary>
    /// <returns>IEnumerable{MetadataItemName}.</returns>
    private IEnumerable<KeyValuePair<string, string>> GetActiveMetaItemNames()
    {
      // This is the list of items we always want in the dropdown.
      var alwaysActiveItems = new[] { GallerySettings.DefaultAlbumSortMetaName.ToString(), MetadataItemName.NotSpecified.ToString() };

      var activeMetaNames = GallerySettings.MetadataDisplaySettings.Where(md => md.IsVisibleForAlbum || md.IsVisibleForGalleryObject)
        .Select(md => md.MetadataItem.ToString())
        .Union(alwaysActiveItems);

      // Convert our list of meta names into key/value pairs containing human friendly descriptions.
      return Utils.GetEnumList(typeof(MetadataItemName)).AsQueryable()
        .Where(m => activeMetaNames.Contains(m.Key))
        .OrderBy(m => m.Value);
    }

    private void SaveSettings()
    {
      UnbindData();

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
        Message = GetSaveSuccessText(),
        Style = MessageStyle.Success
      };

      if (chkUpdateSort.Checked)
      {
        UpdateSortOnExistingAlbumsAsync();
      }
    }

    private string GetSaveSuccessText()
    {
      var msg = Resources.GalleryServer.Admin_Save_Success_Text;

      if (chkUpdateSort.Checked)
      {
        msg += Resources.GalleryServer.Admin_Albums_UpdateSortStarted_Msg;
      }

      return msg;
    }

    private void UpdateSortOnExistingAlbumsAsync()
    {
      var rootAlbumId = Factory.LoadRootAlbumInstance(GalleryId).Id;
      var rootAlbum = Factory.LoadAlbumInstance(new AlbumLoadOptions(rootAlbumId) { IsWritable = true });

      rootAlbum.SortByMetaName = GallerySettingsUpdateable.DefaultAlbumSortMetaName;
      rootAlbum.SortAscending = GallerySettingsUpdateable.DefaultAlbumSortAscending;

      GalleryObjectController.SaveGalleryObject(rootAlbum);

      rootAlbum.SortAsync(true, Utils.UserName, true, true);
    }

    private void UnbindData()
    {
      this.wwDataBinder.Unbind(this);

      GallerySettingsUpdateable.DefaultAlbumSortAscending = ddlSortDirection.SelectedValue.Equals("Ascending", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
  }
}