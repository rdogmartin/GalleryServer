using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events;
using GalleryServer.Web.Controller;
using GalleryServer.WebControls;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering site-wide settings.
  /// </summary>
  public partial class sitesettings : Pages.AdminPage
  {
    #region Private Fields

    private AppSettingsEntity _appSettingUpdateable;
    private ILicense _license;
    private static readonly object _sharedLock = new object();

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the license for the current application.
    /// </summary>
    /// <value>The license.</value>
    public ILicense License
    {
      get
      {
        if (_license == null)
        {
          // Make a copy of the singleton license and return that.
          var curLicense = AppSettings.License;

          _license = new License()
          {
            LicenseEmail = curLicense.LicenseEmail,
            LicenseKey = curLicense.LicenseKey,
            InstallDate = curLicense.InstallDate,
            IsValid = curLicense.IsValid,
            KeyInvalidReason = curLicense.KeyInvalidReason,
            LicenseType = curLicense.LicenseType,
            InstanceId = curLicense.InstanceId,
            Version = curLicense.Version
          };
        }

        return _license;
      }
      set { _license = value; }
    }

    /// <summary>
    /// Gets an updateable instance of the current application settings.
    /// </summary>
    /// <value>The updateable instance of the current application settings.</value>
    protected AppSettingsEntity AppSettingsUpdateable
    {
      get
      {
        if (_appSettingUpdateable == null)
        {
          var app = AppSetting.Instance;

          _appSettingUpdateable = new AppSettingsEntity
          {
            LicenseEmail = app.LicenseEmail,
            LicenseKey = app.LicenseKey,
            Skin = app.Skin,
            EnableCache = app.EnableCache,
            AllowGalleryAdminToManageUsersAndRoles = app.AllowGalleryAdminToManageUsersAndRoles,
            AllowGalleryAdminToViewAllUsersAndRoles = app.AllowGalleryAdminToViewAllUsersAndRoles,
            MaxNumberErrorItems = app.MaxNumberErrorItems,
            JQueryScriptPath = app.JQueryScriptPath,
            JQueryMigrateScriptPath = app.JQueryMigrateScriptPath,
            JQueryUiScriptPath = app.JQueryUiScriptPath,
            ImageMagickPath = app.ImageMagickPath,
            EmailFromName = app.EmailFromName,
            EmailFromAddress = app.EmailFromAddress,
            SmtpServer = app.SmtpServer,
            SmtpServerPort = app.SmtpServerPort,
            SendEmailUsingSsl = app.SendEmailUsingSsl
          };
        }

        return _appSettingUpdateable;
      }
    }

    /// <summary>
    /// Gets the application settings for the current application.
    /// </summary>
    /// <value>The application settings.</value>
    public IAppSetting AppSettings
    {
      get { return AppSetting.Instance; }
    }

    /// <summary>
    /// Gets a value indicating whether application settings can be saved. Hard-coded to true to allow an admin to 
    /// enter a product key in case of expired trial.
    /// </summary>
    /// <value><c>true</c> if saving is enabled; otherwise, <c>false</c>.</value>
    private static bool SavingIsEnabled => true;

    /// <summary>
    /// Gets the membership object that manages users for the gallery.
    /// </summary>
    /// <value>The membership object.</value>
    public System.Configuration.Provider.ProviderBase MembershipGsp
    {
      get
      {
        return Controller.UserController.MembershipGsp;
      }
    }

    /// <summary>
    /// Gets the role object that manages roles for the gallery.
    /// </summary>
    /// <value>The role object.</value>
    public System.Configuration.Provider.ProviderBase RoleGsp
    {
      get
      {
        return Controller.RoleController.RoleGsp;
      }
    }

    /// <summary>
    /// Gets the connection string name as stored in web.config for the gallery data.
    /// </summary>
    /// <value>The connection string name.</value>
    private string ConnectionStringName
    {
      get
      {
        return Factory.GetConnectionStringName();
      }
    }

    /// <summary>
    /// Gets the connection string as stored in web.config for the gallery data.
    /// </summary>
    /// <value>The connection string.</value>
    private string ConnectionString
    {
      get
      {
        return Factory.GetConnectionStringSettings().ConnectionString;
      }
    }

    /// <summary>
    /// Gets the name of the data provider. Examples: "System.Data.SqlServerCe.4.0", "System.Data.SqlClient"
    /// </summary>
    /// <value>The provider name.</value>
    private string GalleryDataProvider
    {
      get { return Factory.GetConnectionStringSettings().ProviderName; }
    }

    /// <summary>
    /// Gets he connection string settings for the connection string associated with the gallery data.
    /// </summary>
    /// <returns>An instance of <see cref="System.Configuration.ConnectionStringSettings" />.</returns>
    protected ConnectionStringSettings ConnectionStringSettings
    {
      get
      {
        return Factory.GetConnectionStringSettings();
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether skin path has changed.
    /// </summary>
    /// <value><c>true</c> if skin path has changed; otherwise, <c>false</c>.</value>
    private bool SkinPathHasChanged { get; set; }

    /// <summary>
    /// Gets the CSS classes to use for the license message icon.
    /// </summary>
    protected string LicenseKeyIconCssClass
    {
      get
      {
        if (String.IsNullOrEmpty(License.LicenseKey))
        {
          return "gsp_msgfriendly fa-arrow-right";
        }
        else if (License.IsValid)
        {
          return "gsp_msgfriendly fa-check-circle";
        }
        else
        {
          return "gsp_msgwarning fa-exclamation-circle";
        }
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
      if (!UserCanAdministerSite && UserCanAdministerGallery)
      {
        Utils.Redirect(PageId.admin_galleries, "aid={0}", this.GetAlbumId());
      }

      this.CheckUserSecurity(SecurityActions.AdministerSite);

      ConfigureControlsEveryTime();

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }
    }

    /// <summary>
    /// Handles the Click event of the btnPurgeCache control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnPurgeCache_Click(object sender, EventArgs e)
    {
      CacheController.PurgeCache();

      ClientMessage = new ClientMessageOptions
      {
        Title = Resources.GalleryServer.Admin_Site_Settings_PurgeCache_Success_Hdr,
        Message = null,
        Style = MessageStyle.Success
      };
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

        // When auto trim is disabled, we store "0" in the config file, but we want to display an empty string
        // in the max # of items textbox. The event wwDataBinder_BeforeUnbindControl may have put a "0" in the 
        // textbox, so undo that now.
        if (txtMaxErrorItems.Text == "0")
          txtMaxErrorItems.Text = String.Empty;
      }

      return true;
    }

    /// <summary>
    /// Handles the Click event of the btnEmailTest control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void btnEmailTest_Click(object sender, EventArgs e)
    {
      SendTestEmail();
    }

    /// <summary>
    /// Handles the Click event of the lbCompactDb control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void lbCompactDb_Click(object sender, EventArgs e)
    {
      CompactAndRepairSqlCe();
    }

    /// <summary>
    /// Handles the OnClick event of the hlDeactivate control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void hlDeactivate_OnClick(object sender, EventArgs e)
    {
      if (License.Deactivate())
      {
        // Recalculate the license status. Expected to be either Trial or TrialExpired.
        License.Inflate();

        AppSetting.Instance.License = License;

        lock (_sharedLock)
        {
          AppSetting.Instance.LicenseEmail = License.LicenseEmail;
          AppSetting.Instance.LicenseKey = License.LicenseKey;
          AppSetting.Instance.InstanceId = License.InstanceId;

          AppSetting.Instance.Save();
        }

        AppSettingsUpdateable.LicenseEmail = License.LicenseEmail;
        AppSettingsUpdateable.LicenseKey = License.LicenseKey;

        this.wwDataBinder.DataBind();

        ClientMessage = new ClientMessageOptions
        {
          Title = "Deactivation Successful",
          Message = "The license key has been deactivated in this installation and can be used in another location.",
          Style = MessageStyle.Success,
          AutoCloseDelay = 0
        };
      }
      else
      {
        wwDataBinder.AddBindingError(License.KeyInvalidReason, txtLicenseKey);

        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_ProductKey_Incorrect_Hdr,
          Message = Utils.HtmlEncode(wwDataBinder.BindingErrors.ToString()),
          Style = MessageStyle.Error,
          AutoCloseDelay = 0
        };
      }

      UpdateUI();
    }

    /// <summary>
    /// Handles the OnBeforeUnBindControl event of the wwDataBinder control.
    /// </summary>
    /// <param name="item">The wwDataBindingItem item.</param>
    protected bool wwDataBinder_BeforeUnbindControl(wwDataBindingItem item)
    {
      // When auto trim is disabled, we store "0" in the config table.
      if (!chkAutoTrimLog.Checked && item.ControlId == txtMaxErrorItems.ID)
      {
        txtMaxErrorItems.Text = "0";
      }

      if (item.ControlId == txtJQueryScriptPath.ID)
      {
        string url = txtJQueryScriptPath.Text.Trim();

        if (!String.IsNullOrEmpty(url) && !Utils.IsAbsoluteUrl(url) && (!url.StartsWith("~", StringComparison.Ordinal)))
        {
          url = String.Concat("~", url); // Ensure relative URLs start with "~"
        }

        txtJQueryScriptPath.Text = url;
      }

      if (item.ControlId == txtJQueryUiScriptPath.ID)
      {
        string url = txtJQueryUiScriptPath.Text.Trim();

        if (!String.IsNullOrEmpty(url) && !Utils.IsAbsoluteUrl(url) && (!url.StartsWith("~", StringComparison.Ordinal)))
        {
          url = String.Concat("~", url); // Ensure relative URLs start with "~"
        }

        txtJQueryUiScriptPath.Text = url;
      }

      return true;
    }

    /// <summary>
    /// Handles the OnAfterBindControl event of the wwDataBinder control.
    /// </summary>
    /// <param name="item">The wwDataBindingItem item.</param>
    protected void wwDataBinder_AfterBindControl(wwDataBindingItem item)
    {
      // 
      if (item.ControlInstance == txtMaxErrorItems)
      {
        int maxErrorItems = Convert.ToInt32(this.txtMaxErrorItems.Text, CultureInfo.InvariantCulture);
        if (maxErrorItems == 0)
        {
          // Disable the checkbox because feature is turned off (a "0" indicates it is off). Set textbox to
          // an empty string because we don't want to display 0.
          chkAutoTrimLog.Checked = false;
          txtMaxErrorItems.Text = String.Empty;
        }
        else if (maxErrorItems > 0)
          chkAutoTrimLog.Checked = true; // Select the checkbox when max # of items is > 0
        else
        {
          // We'll never get here because the config definition uses an IntegerValidator to force the number
          // to be greater than 0.
        }
      }
    }

    /// <summary>
    /// Handles the OnValidateControl event of the wwDataBinder control.
    /// </summary>
    /// <param name="item">The wwDataBindingItem item.</param>
    /// <returns>Returns <c>true</c> if the item is valid; otherwise returns <c>false</c>.</returns>
    protected bool wwDataBinder_ValidateControl(wwDataBindingItem item)
    {
      if (item.ControlInstance == txtMaxErrorItems)
      {
        if ((chkAutoTrimLog.Checked) && (Convert.ToInt32(txtMaxErrorItems.Text, CultureInfo.InvariantCulture) <= 0))
        {
          item.BindingErrorMessage = Resources.GalleryServer.Admin_Error_Invalid_MaxNumberErrorItems_Msg;
          return false;
        }
      }

      if (item.ControlInstance == txtJQueryScriptPath)
      {
        if (!ValidateUrl(txtJQueryScriptPath.Text.Trim()))
        {
          item.BindingErrorMessage = Resources.GalleryServer.Admin_Site_Settings_InvalidJQueryPath;
          return false;
        }
      }

      if (item.ControlInstance == txtJQueryUiScriptPath)
      {
        if (!ValidateUrl(txtJQueryUiScriptPath.Text.Trim()))
        {
          item.BindingErrorMessage = Resources.GalleryServer.Admin_Site_Settings_InvalidJQueryPath;
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Private Methods

    private bool ValidateUrl(string url)
    {
      // Verify the jQuery path exists, but only for local URLs. We don't bother with absolute URLs.
      if (String.IsNullOrEmpty(url) || Utils.IsAbsoluteUrl(url))
        return true;
      else
        return System.IO.File.Exists(Server.MapPath(url));
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Site_Settings_General_Page_Header;
      
      ConfigureSqlCeControlsFirstTime();

      OkButtonBottom.Enabled = SavingIsEnabled;
      OkButtonTop.Enabled = SavingIsEnabled;

      this.wwDataBinder.DataBind();

      CheckForInstallFile();

      UpdateUI();

      BindSkin();

      CheckForMessages();
    }

    private void CheckForInstallFile()
    {
      if (File.Exists(Utils.InstallFilePath))
      {
        var installFilePath = Path.Combine(GlobalConstants.AppDataDirectory, GlobalConstants.InstallTriggerFileName);

        try
        {
          File.Delete(Utils.InstallFilePath);
        }
        catch (Exception)
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = "Install File Detected",
            Message = String.Format("An installation-related file was detected at {0}. Normally this file is removed during installation, but the application was not able to delete it. Navigate to this directory and delete the file. It is no longer needed.", installFilePath),
            Style = MessageStyle.Warning
          };
        }
      }
    }

    private void BindSkin()
    {
      var path = HttpContext.Current.Server.MapPath(Utils.GetUrl("skins"));

      var skinNames = Directory.EnumerateDirectories(path).Select(d => d.Substring(d.LastIndexOf(Path.DirectorySeparatorChar) + 1));

      if (!skinNames.Contains(AppSetting.Instance.Skin))
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = "Cannot Find Skin Path",
          Message = String.Format("The gallery is configured to use a skin named {0}, but no skin by that name was found in the skin directory. Instead, these skin names were found: {2}. Resolve this error by ensuring a directory named {0} exists at {1}.", AppSetting.Instance.Skin, path, String.Join(", ", skinNames)),
          Style = MessageStyle.Error
        };

        OkButtonBottom.Enabled = false;
        OkButtonTop.Enabled = false;
      }
      else
      {
        ddlSkin.DataSource = skinNames;
        ddlSkin.DataBind();
      }
    }

    private void ConfigureSqlCeControlsFirstTime()
    {
      if (!GalleryDataProvider.Contains("SqlServerCe"))
        return;

      trCompact.Visible = true;
      string dbFilePath = Utils.GetDbFilePathFromConnectionString(ConnectionString);

      if (dbFilePath != null)
      {
        var fi = new FileInfo(dbFilePath);
        var fileSizeKb = (int)(fi.Length / 1024);
        lblDbFileSize.Text = String.Concat("(", fileSizeKb.ToString("N0", CultureInfo.CurrentCulture), " KB)");
      }
      else
      {
        dbFilePath = ConnectionStringName;
      }

      lblDbFilename.Text = dbFilePath;
    }

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Site_Settings_General_Page_Header;
    }

    private void ConfigureVersionText()
    {
      var licenceType = (AppSettings.License.LicenseType == LicenseLevel.NotSet ? String.Empty : AppSettings.License.LicenseType.GetDescription());

      lblVersion.Text = String.Concat(licenceType, " ", Utils.GetGalleryServerVersion());
    }

    private void DetermineMessage()
    {
      if (ClientMessage == null)
      {
        if (License.LicenseType == LicenseLevel.Trial)
        {
          int daysLeftInTrial = (License.InstallDate.AddDays(GlobalConstants.TrialNumberOfDays) - DateTime.Today).Days;

          ClientMessage = new ClientMessageOptions
          {
            Title = Resources.GalleryServer.Site_Welcome_Msg,
            Message = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_In_Trial_Period_Msg, daysLeftInTrial),
            Style = MessageStyle.Success,
            AutoCloseDelay = 0
          };
        }
        else if (License.LicenseType == LicenseLevel.TrialExpired)
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = Resources.GalleryServer.Admin_Need_Product_Key_Hdr,
            Message = Resources.GalleryServer.Admin_Need_Product_Key_Msg,
            Style = MessageStyle.Warning,
            AutoCloseDelay = 0
          };
        }
      }
    }

    private void UpdateUI()
    {
      hlDeactivate.Visible = (License.IsValid && License.LicenseType >= LicenseLevel.Free && License.LicenseType != LicenseLevel.Trial);

      ConfigureVersionText();
      UpdateProductKeyValidationMessage();
      DetermineMessage();
    }

    private void UpdateProductKeyValidationMessage()
    {
      if (String.IsNullOrEmpty(License.LicenseKey))
      {
        lblProductKeyValidationMsg.Text = Resources.GalleryServer.Admin_Site_Settings_ProductKey_NotEntered_Label;
        lblProductKeyValidationMsg.CssClass = "gsp_msgfriendly";
      }
      else if (License.IsValid)
      {
        lblProductKeyValidationMsg.Text = Resources.GalleryServer.Admin_Site_Settings_ProductKey_Correct_Label;
        lblProductKeyValidationMsg.CssClass = "gsp_msgfriendly";
      }
      else
      {
        lblProductKeyValidationMsg.Text = License.KeyInvalidReason;
        lblProductKeyValidationMsg.CssClass = "gsp_msgwarning";
      }
    }

    /// <summary>
    /// Verify the license key is valid and displays to the user the results of the validation. The <see cref="License" />
    /// property is updated with the results of the validation.
    /// </summary>
    /// <param name="licenseEmail">The license email.</param>
    /// <param name="licenseKey">The license key to validate.</param>
    private void ActivateLicenseKey(string licenseEmail, string licenseKey)
    {
      License.Activate(licenseEmail, licenseKey, Utils.GetAppUrl());

      if (!string.IsNullOrEmpty(licenseKey))
      {
        if (License.IsValid)
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = Resources.GalleryServer.Admin_Save_ProductKey_Success_Hdr,
            Message = Resources.GalleryServer.Admin_Save_ProductKey_Success_Msg,
            Style = MessageStyle.Success,
            AutoCloseDelay = 0
          };
        }
        else
        {
          wwDataBinder.AddBindingError(License.KeyInvalidReason, txtLicenseKey);
        }
      }
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
        UpdateUI();
        return;
      }

      SkinPathHasChanged = (!AppSetting.Instance.Skin.Equals(AppSettingsUpdateable.Skin, StringComparison.OrdinalIgnoreCase));

      // Handle license changes
      var shouldUpdateLicenseInfo = false;
      var licenseKey = AppSettingsUpdateable.LicenseKey.Trim();
      var licenseEmail = AppSettingsUpdateable.LicenseEmail.Trim();

      if (AppSetting.Instance.LicenseKey != licenseKey || AppSetting.Instance.LicenseEmail != licenseEmail)
      {
        // The license key or email associated with the license changed. Revalidate license.
        ActivateLicenseKey(licenseEmail, licenseKey);

        if (wwDataBinder.BindingErrors.Count > 0)
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = Resources.GalleryServer.Admin_Save_ProductKey_Incorrect_Hdr,
            Message = wwDataBinder.BindingErrors.ToString(),
            Style = MessageStyle.Error,
            AutoCloseDelay = 0
          };

          UpdateUI();
          return;
        }

        if (String.IsNullOrEmpty(licenseKey) || License.IsValid)
        {
          AppSetting.Instance.License = License;
        }

        shouldUpdateLicenseInfo = true;
      }

      lock (_sharedLock)
      {
        var app = AppSetting.Instance;

        if (shouldUpdateLicenseInfo)
        {
          app.LicenseEmail = app.License.LicenseEmail;
          app.LicenseKey = app.License.LicenseKey;
          app.InstanceId = app.License.InstanceId;
        }

        app.Skin = AppSettingsUpdateable.Skin;
        app.JQueryScriptPath = AppSettingsUpdateable.JQueryScriptPath;
        app.JQueryMigrateScriptPath = AppSettingsUpdateable.JQueryMigrateScriptPath;
        app.JQueryUiScriptPath = AppSettingsUpdateable.JQueryUiScriptPath;
        app.ImageMagickPath = AppSettingsUpdateable.ImageMagickPath;
        app.EnableCache = AppSettingsUpdateable.EnableCache;
        app.AllowGalleryAdminToManageUsersAndRoles = AppSettingsUpdateable.AllowGalleryAdminToManageUsersAndRoles;
        app.AllowGalleryAdminToViewAllUsersAndRoles = AppSettingsUpdateable.AllowGalleryAdminToViewAllUsersAndRoles;
        app.MaxNumberErrorItems = AppSettingsUpdateable.MaxNumberErrorItems;
        app.EmailFromName = AppSettingsUpdateable.EmailFromName;
        app.EmailFromAddress = AppSettingsUpdateable.EmailFromAddress;
        app.SmtpServer = AppSettingsUpdateable.SmtpServer;
        app.SmtpServerPort = AppSettingsUpdateable.SmtpServerPort;
        app.SendEmailUsingSsl = AppSettingsUpdateable.SendEmailUsingSsl;

        app.Save();
      }

      if (ClientMessage == null)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
          Message = Resources.GalleryServer.Admin_Save_Success_Text,
          Style = MessageStyle.Success
        };
      }

      UpdateUI();

      if (SkinPathHasChanged)
      {
        // Since we changed a settings that affects how the page is rendered, let us redirect to the current page and
        // show the save success message. If we simply show a message without redirecting, the user doesn't see the effect
        // of their change until the next page load.
        Utils.RecalculateSkinPath();
        const MessageType msg = MessageType.SettingsSuccessfullyChanged;

        Utils.Redirect(PageId.admin_sitesettings, "aid={0}&msg={1}", GetAlbumId(), ((int)msg).ToString(CultureInfo.InvariantCulture));
      }
    }

    private void CompactAndRepairSqlCe()
    {
      if (AppSetting.Instance.ProviderDataStore != ProviderDataStore.SqlCe)
        return;

      string msg;
      bool successful = DbManager.CompactAndRepairSqlCeDatabase(out msg);

      ClientMessage = new ClientMessageOptions
      {
        Title = successful ? "Compact/Repair Complete" : "Error During Compact/Repair",
        Message = msg,
        Style = successful ? MessageStyle.Success : MessageStyle.Error
      };

      UpdateUI();
    }

    private void SendTestEmail()
    {
      string subject = Resources.GalleryServer.Admin_Gallery_Settings_Test_Email_Subject;
      string body = Resources.GalleryServer.Admin_Gallery_Settings_Test_Email_Body;
      string msgResult = String.Empty;
      bool emailSent = false;
      MessageStyle msgStyle;
      IUserAccount user = UserController.GetUser();

      if (HelperFunctions.IsValidEmail(user.Email))
      {
        try
        {
          EmailController.SendEmail(user, subject, body);
          emailSent = true;
        }
        catch (Exception ex)
        {
          string errorMsg = EventController.GetExceptionDetails(ex);

          msgResult = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_Gallery_Settings_Test_Email_Failure_Text, errorMsg);
        }
      }
      else
      {
        msgResult = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_Gallery_Settings_Test_Email_Invalid_Text, user.UserName);
      }

      if (emailSent)
      {
        msgStyle = MessageStyle.Success;
        msgResult = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_Gallery_Settings_Test_Email_Success_Text, user.UserName, user.Email);
      }
      else
      {
        msgStyle = MessageStyle.Error;
      }

      ClientMessage = new ClientMessageOptions
      {
        Title = "Send Test E-mail",
        Message = msgResult,
        Style = msgStyle
      };
    }

    /// <summary>
    ///   Determine if there are any messages we need to display to the user.
    /// </summary>
    private void CheckForMessages()
    {
      if (ClientMessage != null && ClientMessage.MessageId == MessageType.SettingsSuccessfullyChanged)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
          Message = Resources.GalleryServer.Admin_Save_Success_Text,
          Style = MessageStyle.Success
        };
      }
    }

    #endregion
  }

  /// <summary>
  /// A simple object to store application settings from the web page in preparation for being saved to the data store.
  /// </summary>
  public class AppSettingsEntity
  {
    /// <summary>
    /// Gets or sets the license key.
    /// </summary>
    /// <value>The license key.</value>
    public string LicenseKey { get; set; }
    /// <summary>
    /// Gets or sets the license email.
    /// </summary>
    /// <value>The license email.</value>
    public string LicenseEmail { get; set; }
    /// <summary>
    /// Gets or sets the skin.
    /// </summary>
    /// <value>The skin.</value>
    public string Skin { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to enable cache.
    /// </summary>
    /// <value><c>true</c> if enable cache; otherwise, <c>false</c>.</value>
    public bool EnableCache { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether allow gallery admin to manage users and roles.
    /// </summary>
    /// <value><c>true</c> if allow gallery admin to manage users and roles; otherwise, <c>false</c>.</value>
    public bool AllowGalleryAdminToManageUsersAndRoles { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to allow gallery admin to view all users and roles].
    /// </summary>
    /// <value><c>true</c> if allow gallery admin to view all users and roles; otherwise, <c>false</c>.</value>
    public bool AllowGalleryAdminToViewAllUsersAndRoles { get; set; }
    /// <summary>
    /// Gets or sets the maximum number of error items.
    /// </summary>
    /// <value>The maximum number of error items.</value>
    public int MaxNumberErrorItems { get; set; }
    /// <summary>
    /// Gets or sets the jQuery script path.
    /// </summary>
    /// <value>The jQuery script path.</value>
    public string JQueryScriptPath { get; set; }
    /// <summary>
    /// Gets or sets the jQuery Migrate script path.
    /// </summary>
    /// <value>The jQuery Migrate script path.</value>
    public string JQueryMigrateScriptPath { get; set; }
    /// <summary>
    /// Gets or sets the jQuery UI script path.
    /// </summary>
    /// <value>The jQuery UI script path.</value>
    public string JQueryUiScriptPath { get; set; }
    /// <summary>
    /// Gets or sets the ImageMagick path.
    /// </summary>
    /// <value>The ImageMagick path.</value>
    public string ImageMagickPath { get; set; }
    /// <summary>
    /// Gets or sets the the email from name.
    /// </summary>
    /// <value>The email from name.</value>
    public string EmailFromName { get; set; }
    /// <summary>
    /// Gets or sets the email from address.
    /// </summary>
    /// <value>The email from address.</value>
    public string EmailFromAddress { get; set; }
    /// <summary>
    /// Gets or sets the SMTP server.
    /// </summary>
    /// <value>The SMTP server.</value>
    public string SmtpServer { get; set; }
    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    /// <value>The SMTP server port.</value>
    public string SmtpServerPort { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to send email using SSL.
    /// </summary>
    /// <value><c>true</c> if sending email using SSL; otherwise, <c>false</c>.</value>
    public bool SendEmailUsingSsl { get; set; }
  }
}