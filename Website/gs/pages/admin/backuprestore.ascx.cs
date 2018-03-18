using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for using the backup and restore feature.
  /// </summary>
  public partial class backuprestore : Pages.AdminPage
  {
    #region Properties

    /// <summary>
    /// Gets the name of the cookie that stores the index of the currently selected tab.
    /// </summary>
    /// <value>A string.</value>
    protected string SelectedTabCookieName { get { return String.Concat(cid, "_br_cookie"); } }

    #endregion

    #region Protected Methods

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
        Utils.Redirect(PageId.admin_gallerysettings, "aid={0}", this.GetAlbumId());
      }

      this.CheckUserSecurity(SecurityActions.AdministerSite);

      Page.MaintainScrollPositionOnPostBack = true;

      if (!IsPostBack)
      {
        ConfigureControlsFirstPageLoad();
      }

      ConfigureControlsEveryPageLoad();
    }

    /// <summary>
    /// Handles the Click event of the btnExportData control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void btnExportData_Click(object sender, EventArgs e)
    {
      string backupFilename = "GalleryServerBackup_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);

      IBackupFile bak = new BackupFile();

      bak.IncludeMembershipData = chkExportMembership.Checked;
      bak.IncludeGalleryData = chkExportGalleryData.Checked;

      var galleryData = bak.Create();

      IMimeType mimeType = Factory.LoadMimeType("dummy.zip");

      int bufferSize = AppSetting.Instance.MediaObjectDownloadBufferSize;
      byte[] buffer = new byte[bufferSize];

      Stream stream = null;
      try
      {
        // Create an in-memory ZIP file.
        stream = ZipUtility.CreateZipStream(galleryData, backupFilename + ".xml", 5);

        // Send to user.
        Response.AddHeader("Content-Disposition", "attachment; filename=" + backupFilename + ".zip");

        Response.Clear();
        Response.ContentType = (mimeType != null ? mimeType.FullType : "application/octet-stream");
        Response.Buffer = false;

        stream.Position = 0;
        int byteCount;
        while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
          if (Response.IsClientConnected)
          {
            Response.OutputStream.Write(buffer, 0, byteCount);
            Response.Flush();
          }
          else
          {
            return;
          }
        }
      }
      finally
      {
        if (stream != null)
          stream.Close();

        Response.End();
      }
    }

    /// <summary>
    /// Handles the Click event of the btnUpload control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnUpload_Click(object sender, EventArgs e)
    {
      DeletePreviouslyUploadedFile();
      string filePath = SaveFileToTempDirectory(fuRestoreFile);

      IBackupFile backupFile = new BackupFile(filePath);
      ValidateRestoreFile(backupFile);

      ConfigureBackupFileInfo(backupFile);

      if (!backupFile.IsValid && File.Exists(filePath))
        File.Delete(filePath);

    }

    /// <summary>
    /// Handles the Click event of the lbRemoveRestoreFile control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void lbRemoveRestoreFile_Click(object sender, EventArgs e)
    {
      DeletePreviouslyUploadedFile();

      ConfigureBackupFileInfo(null);
    }

    /// <summary>
    /// Handles the Click event of the btnRestore control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void btnRestore_Click(object sender, EventArgs e)
    {
      string filePath = ViewState["FilePath"].ToString();
      Page.MaintainScrollPositionOnPostBack = false;

      try
      {
        if (File.Exists(filePath))
        {
          IBackupFile bak = new BackupFile(filePath);

          bak.IncludeMembershipData = chkImportMembership.Checked;
          bak.IncludeGalleryData = chkImportGalleryData.Checked;

          bak.Import();
          UserController.LogOffUser();

          ClientMessage = new ClientMessageOptions
          {
            Title = "Restore Complete",
            Message = Resources.GalleryServer.Admin_Backup_Restore_Db_Successfully_Restored_Msg,
            Style = MessageStyle.Success,
            AutoCloseDelay = 0
          };
        }
        else
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = "Restore Aborted",
            Message = Resources.GalleryServer.Admin_Backup_Restore_Cannot_Restore_File_File_Not_Found_Msg,
            Style = MessageStyle.Error
          };
        }
      }
      catch (Exception ex)
      {
        LogError(ex);
        ClientMessage = new ClientMessageOptions
        {
          Title = "Restore Aborted",
          Message = String.Concat(Resources.GalleryServer.Admin_Backup_Restore_Cannot_Restore_File_Label, ex.Message),
          Style = MessageStyle.Error
        };
      }
      finally
      {
        DeletePreviouslyUploadedFile();

        ConfigureBackupFileInfo(null);

        CacheController.PurgeCache();

        bool adviseUserToManuallyRestartApp = false;
        try
        {
          // Recycle the app to force the providers to re-initialize. This will query the application ID from the database, which
          // may have changed during the restore. If any errors occur, advise the user to manually restart the app.
          Utils.ForceAppRecycle();
        }
        catch (IOException) { adviseUserToManuallyRestartApp = true; }
        catch (UnauthorizedAccessException) { adviseUserToManuallyRestartApp = true; }
        catch (PlatformNotSupportedException) { adviseUserToManuallyRestartApp = true; }

        if (adviseUserToManuallyRestartApp)
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = "Restore Complete",
            Message = Resources.GalleryServer.Admin_Backup_Restore_Db_Successfully_Restored_AppNotRecycled_Msg,
            Style = MessageStyle.Info
          };
        }
      }
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsFirstPageLoad()
    {
      OkButtonIsVisible = false;
      AdminPageTitle = Resources.GalleryServer.Admin_Backup_Restore_Page_Header;
    }

    private void ConfigureControlsEveryPageLoad()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Backup_Restore_Page_Header;

      // Don't disable backup/restore functionality based on license key. User should always be able to make a backup,
      // and the configuration of the data in the restored file should dictate gallery behavior, not the pre-restored gallery.
    }

    private string SaveFileToTempDirectory(FileUpload fileToRestore)
    {
      // Save file to temp directory, ensuring that we are not overwriting an existing file. If the uploaded file is a ZIP archive,
      // extract the embedded XML file and save that.
      var filePath = String.Empty;

      var fileExt = Path.GetExtension(fileToRestore.FileName);

      if (fileExt == null || String.IsNullOrWhiteSpace(fileExt))
        return filePath;

      if (fileExt.Equals(".zip", StringComparison.OrdinalIgnoreCase))
      {
        using (var zip = new ZipUtility(Utils.UserName, GetGalleryServerRolesForUser()))
        {
          filePath = zip.ExtractNextFileFromZip(fileToRestore.FileContent, AppSetting.Instance.TempUploadDirectory);
        }
      }
      else if (fileExt.Equals(".xml", StringComparison.OrdinalIgnoreCase))
      {
        string fileName = HelperFunctions.ValidateFileName(AppSetting.Instance.TempUploadDirectory, fileToRestore.FileName);
        filePath = Path.Combine(AppSetting.Instance.TempUploadDirectory, fileName);

        fileToRestore.SaveAs(filePath);
      }

      return filePath;
    }

    private void ValidateRestoreFile(IBackupFile backupFile)
    {
      var fileExt = Path.GetExtension(backupFile.FilePath);

      if (fileExt != null && fileExt.ToLowerInvariant() == ".xml")
      {
        try
        {
          backupFile.Validate();
        }
        catch (Exception ex)
        {
          LogError(ex);
          throw;
        }
      }
    }

    private void ConfigureBackupFileInfo(IBackupFile backupFile)
    {
      if (backupFile == null)
      {
        lblRestoreFilename.Text = Resources.GalleryServer.Admin_Backup_Restore_File_Not_Uploaded_Msg;
        lblRestoreFilename.CssClass = "gsp_msgwarning";
        lblNumApps.Text = String.Empty;
        lblNumProfiles.Text = String.Empty;
        lblNumRoles.Text = String.Empty;
        lblNumMembers.Text = String.Empty;
        lblNumUsers.Text = String.Empty;
        lblNumUsersInRoles.Text = String.Empty;
        lblNumGalleries.Text = String.Empty;
        lblNumAlbums.Text = String.Empty;
        lblNumMediaObjects.Text = String.Empty;
        lblNumMetadata.Text = String.Empty;
        lblNumTag.Text = String.Empty;
        lblNumMetadataTag.Text = String.Empty;
        lblNumRoleAlbums.Text = String.Empty;
        lblNumAppSettings.Text = String.Empty;
        lblNumGalleryControlSettings.Text = String.Empty;
        lblNumGallerySettings.Text = String.Empty;
        lblNumBrowserTemplates.Text = String.Empty;
        lblNumMimeTypes.Text = String.Empty;
        lblNumMimeTypeGalleries.Text = String.Empty;
        lblNumGalleryRoles.Text = String.Empty;
        lblNumUiTemplates.Text = String.Empty;
        lblNumUiTemplateAlbums.Text = String.Empty;
        lblNumUserGalleryProfiles.Text = String.Empty;

        btnRestore.Enabled = false;
        imgValidationResult.Visible = false;
        lblValidationResult.Text = String.Empty;
        lblValidationResult.CssClass = String.Empty;
        lbRemoveRestoreFile.Visible = false;

        return;
      }

      lblRestoreFilename.Text = Path.GetFileName(backupFile.FilePath);

      var tableNames = backupFile.MembershipTables.Concat(backupFile.GalleryTableNames);

      var dataRecords = backupFile.DataTableRecordCount;

      foreach (var tableName in tableNames)
      {
        switch (tableName)
        {
          case "Applications":
            lblNumApps.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);

            chkImportMembership.Checked = (dataRecords.ContainsKey(tableName) && backupFile.DataTableRecordCount[tableName] > 0);
            break;
          case "Profiles":
            lblNumProfiles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "Roles":
            lblNumRoles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "Memberships":
            lblNumMembers.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "Users":
            lblNumUsers.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "UsersInRoles":
            lblNumUsersInRoles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;

          case "Gallery":
            lblNumGalleries.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);

            chkImportGalleryData.Checked = (dataRecords.ContainsKey(tableName) && backupFile.DataTableRecordCount[tableName] > 0);
            break;
          case "Album":
            lblNumAlbums.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "MediaObject":
            lblNumMediaObjects.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "Metadata":
            lblNumMetadata.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "Tag": //**
            lblNumTag.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "MetadataTag": //**
            lblNumMetadataTag.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "RoleAlbum":
            lblNumRoleAlbums.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "Role":
            lblNumGalleryRoles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "AppSetting":
            lblNumAppSettings.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "GalleryControlSetting":
            lblNumGalleryControlSettings.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "GallerySetting":
            lblNumGallerySettings.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "MediaTemplate":
            lblNumBrowserTemplates.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "MimeType":
            lblNumMimeTypes.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "MimeTypeGallery":
            lblNumMimeTypeGalleries.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "UiTemplate":
            lblNumUiTemplates.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "UiTemplateAlbum":
            lblNumUiTemplateAlbums.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
          case "UserGalleryProfile":
            lblNumUserGalleryProfiles.Text = (dataRecords.ContainsKey(tableName) ? backupFile.DataTableRecordCount[tableName].ToString(CultureInfo.CurrentCulture) : String.Empty);
            break;
        }
      }

      if (backupFile.IsValid)
      {
        btnRestore.Enabled = true;
        imgValidationResult.ImageUrl = Utils.GetSkinnedUrl("/images/arrow-right-open-s.png");
        imgValidationResult.Visible = true;
        lblValidationResult.Text = Resources.GalleryServer.Admin_Backup_Restore_File_Valid_Msg;
        lblValidationResult.CssClass = "gsp_msgsuccess";
        lblRestoreFilename.CssClass = "gsp_msgattention";
        lbRemoveRestoreFile.Visible = true;
        lblSchemaVersion.Text = GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(backupFile.SchemaVersion);

        ViewState["FilePath"] = backupFile.FilePath;
      }
      else
      {
        btnRestore.Enabled = false;
        imgValidationResult.ImageUrl = Utils.GetSkinnedUrl("/images/warning-s.png");
        imgValidationResult.Visible = true;
        lblValidationResult.Text = Resources.GalleryServer.Admin_Backup_Restore_File_Not_Valid_Msg;
        lblValidationResult.CssClass = "gsp_msgfailure";
        lblRestoreFilename.CssClass = "gsp_msgattention";
        lbRemoveRestoreFile.Visible = false;
        lblSchemaVersion.Text = GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(backupFile.SchemaVersion);
      }
    }

    private void DeletePreviouslyUploadedFile()
    {
      string filePath = ViewState["FilePath"] as string;

      if (!String.IsNullOrEmpty(filePath))
      {
        File.Delete(filePath);

        ViewState["FilePath"] = null;
      }
    }

    #endregion
  }
}