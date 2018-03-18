using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GalleryServer.Business;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Data
{
  /// <summary>
  /// Handle the migration of data from the 3.2.1 schema to the 4.0.0 schema.
  /// </summary>
  public static class Migrate40Controller
  {
    #region Methods


    #endregion

    #region Functions


    #endregion

    /// <summary>
    /// Upgrades the 3.2.1 data to the 4.0.0 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    /// <param name="galleryDataStore">The type of database used for the gallery data.</param>
    public static void UpgradeTo400(GalleryDb ctx, ProviderDataStore galleryDataStore)
    {
      UpgradeAppSettings(ctx);
      UpgradeGallerySettings(ctx);
      UpgradeUiTemplates(ctx);
      UpgradeGalleryControlSettings(ctx);
      UpdateMetadata(ctx);
      UpdateMediaTemplates(ctx);
      UpgradeMimeTypes(ctx);

      if (galleryDataStore == ProviderDataStore.SqlCe)
      {
        RevertCeEfBugWorkAround();
      }

      // Update data schema version to 4.0.0
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.0.0";

      ctx.SaveChanges();
    }

    private static void UpgradeAppSettings(GalleryDb ctx)
    {
      // Change AppSetting name "ProductKey" to "LicenseKey" and remove the key.
      var appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "ProductKey");

      if (appSetting != null)
      {
        appSetting.SettingName = "LicenseKey";
        appSetting.SettingValue = string.Empty;
      }

      // New AppSetting LicenseEmail
      if (!ctx.AppSettings.Any(a => a.SettingName == "LicenseEmail"))
      {
        ctx.AppSettings.Add(new AppSettingDto
        {
          SettingName = "LicenseEmail",
          SettingValue = string.Empty,
        });
      }

      // New AppSetting VersionKey
      if (!ctx.AppSettings.Any(a => a.SettingName == "VersionKey"))
      {
        ctx.AppSettings.Add(new AppSettingDto
        {
          SettingName = "VersionKey",
          SettingValue = string.Empty,
        });
      }

      // New AppSetting InstanceId
      if (!ctx.AppSettings.Any(a => a.SettingName == "InstanceId"))
      {
        ctx.AppSettings.Add(new AppSettingDto
        {
          SettingName = "InstanceId",
          SettingValue = string.Empty,
        });
      }

      // New AppSetting CustomCss
      if (!ctx.AppSettings.Any(a => a.SettingName == "CustomCss"))
      {
        ctx.AppSettings.Add(new AppSettingDto
        {
          SettingName = "CustomCss",
          SettingValue = string.Empty,
        });
      }

      // New AppSetting ImageMagickPath
      if (!ctx.AppSettings.Any(a => a.SettingName == "ImageMagickPath"))
      {
        ctx.AppSettings.Add(new AppSettingDto
        {
          SettingName = "ImageMagickPath",
          SettingValue = @"\bin",
        });
      }

      // New AppSetting InstallDateEncrypted. This is an encrypted version of the oldest gallery creation date.
      if (!ctx.AppSettings.Any(a => a.SettingName == "InstallDateEncrypted"))
      {
        var dateTime = ctx.Galleries.Min(g => g.DateAdded); // oldest gallery creation date.

        ctx.AppSettings.Add(new AppSettingDto
        {
          SettingName = "InstallDateEncrypted",
          SettingValue = Utils.Encrypt(dateTime.ToString("O", CultureInfo.InvariantCulture), ctx.AppSettings.First(a => a.SettingName == "EncryptionKey").SettingValue),
        });
      }

      // Upgrade jQuery path
      appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js")
      {
        appSetting.SettingValue = "//code.jquery.com/jquery-2.2.3.min.js";
      }

      // Upgrade jQuery Migrate path
      appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryMigrateScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//code.jquery.com/jquery-migrate-1.2.1.js")
      {
        appSetting.SettingValue = "//code.jquery.com/jquery-migrate-1.3.0.min.js";
      }
    }

    private static void UpgradeGallerySettings(GalleryDb ctx)
    {
      // Change GallerySetting name "DefaultRolesForSelfRegisteredUser" to "DefaultRolesForUser" and add "Authenticated Users".
      const string AUTHENTICATED_USERS = "Authenticated Users";

      foreach (var defaultRoleGallerySetting in ctx.GallerySettings.Where(a => a.SettingName == "DefaultRolesForSelfRegisteredUser"))
      {
        defaultRoleGallerySetting.SettingName = "DefaultRolesForUser";

        if (!defaultRoleGallerySetting.SettingValue.ToLower(CultureInfo.InvariantCulture).Contains(AUTHENTICATED_USERS.ToLower(CultureInfo.InvariantCulture)))
        {
          defaultRoleGallerySetting.SettingValue = string.Concat(defaultRoleGallerySetting.SettingValue, ",", AUTHENTICATED_USERS).TrimStart(',');
        }
      }

      // Create role and give it no permissions and apply it to no albums.
      if (!ctx.Roles.Any(r => r.RoleName.Equals(AUTHENTICATED_USERS, StringComparison.OrdinalIgnoreCase)))
      {
        ctx.Roles.Add(new RoleDto()
        {
          RoleName = AUTHENTICATED_USERS,
          AllowViewAlbumsAndObjects = false,
          AllowViewOriginalImage = false,
          AllowAddChildAlbum = false,
          HideWatermark = false,
          AllowAddMediaObject = false,
          AllowDeleteChildAlbum = false,
          AllowDeleteMediaObject = false,
          AllowEditAlbum = false,
          AllowEditMediaObject = false,
          AllowSynchronize = false,
          AllowAdministerGallery = false,
          AllowAdministerSite = false,
        });
      }

      foreach (var gallerySetting in ctx.GallerySettings.Where(a => a.SettingName == "ImageMagickFileTypes"))
      {
        gallerySetting.SettingValue = string.Concat(gallerySetting.SettingValue, ",.ai,.nef,.cr2,.ps").TrimStart(',');
      }

      // New GallerySetting MediaViewSize. We just add one for the template gallery. Normal validation code will add the rest.
      if (!ctx.GallerySettings.Any(a => a.SettingName == "MediaViewSize"))
      {
        ctx.GallerySettings.Add(new GallerySettingDto
        {
          FKGalleryId = ctx.Galleries.First(g => g.IsTemplate).GalleryId,
          SettingName = "MediaViewSize",
          SettingValue = "Optimized",
        });
      }

      // New GallerySetting SlideShowLoop. We just add one for the template gallery. Normal validation code will add the rest.
      if (!ctx.GallerySettings.Any(a => a.SettingName == "SlideShowLoop"))
      {
        ctx.GallerySettings.Add(new GallerySettingDto
        {
          FKGalleryId = ctx.Galleries.First(g => g.IsTemplate).GalleryId,
          SettingName = "SlideShowLoop",
          SettingValue = "False",
        });
      }

      // If AllowUnspecifiedMimeTypes is true, set it to false and enable all existing MIME types.
      foreach (var gallerySetting in ctx.GallerySettings.Where(a => a.SettingName == "AllowUnspecifiedMimeTypes"))
      {
        // Only modify when "True". We can't put this check in above statement because SQL CE gives error:
        // "The ntext and image data types cannot be used in WHERE, HAVING, GROUP BY, ON, or IN clauses, except when these data types are used with the LIKE or IS NULL predicates."
        if (gallerySetting.SettingValue == "True")
        {
          gallerySetting.SettingValue = "False";

          foreach (var mtg in ctx.MimeTypeGalleries.Where(mt => mt.FKGalleryId == gallerySetting.FKGalleryId && !mt.IsEnabled))
          {
            mtg.IsEnabled = true;
          }
        }
      }

      // Update GallerySetting.MetadataDisplaySettings. Replace IsEditable property with UserEditMode and add PersistToFile.
      foreach (var metaDef in ctx.GallerySettings.Where(gs => gs.SettingName == "MetadataDisplaySettings"))
      {
        // Replace "IsEditable:false" with "UserEditMode:1,PersistToFile:false". Replace "IsEditable:true" with "UserEditMode:2,PersistToFile:false"
        // Note that property names may be enclosed with quotes or apostrophes.
        metaDef.SettingValue = System.Text.RegularExpressions.Regex.Replace(metaDef.SettingValue, @"['""]IsEditable['""]:false", @"""UserEditMode"":1,""PersistToFile"":false");
        metaDef.SettingValue = System.Text.RegularExpressions.Regex.Replace(metaDef.SettingValue, @"['""]IsEditable['""]:true", @"""UserEditMode"":2,""PersistToFile"":false");

        // Change caption to use HTML editor. Note that .+? matches any text and $1 represents the text enclosed in parentheses.
        metaDef.SettingValue = System.Text.RegularExpressions.Regex.Replace(metaDef.SettingValue, @"(['""]Name['""]:['""]Caption['""].+?['""]UserEditMode['""]):2", "$1:3");

        // Set PersistToFile to null for all writable meta properties. This list must match the ones in MetadataDefinition.IsPersistable.
        const string writableMetas = "Author|Copyright|CameraModel|EquipmentManufacturer|Subject|Title|Caption|DatePictureTaken|Tags|Rating|Orientation|IptcByline|IptcBylineTitle|IptcCaption|IptcCity|IptcCopyrightNotice|IptcCountryPrimaryLocationName|IptcCredit|IptcDateCreated|IptcHeadline|IptcKeywords|IptcObjectName|IptcOriginalTransmissionReference|IptcProvinceState|IptcSource|IptcSpecialInstructions|IptcSublocation|IptcWriterEditor";
        metaDef.SettingValue = System.Text.RegularExpressions.Regex.Replace(metaDef.SettingValue, $@"(['""]Name['""]:['""]({writableMetas})['""].+?['""]PersistToFile['""]):false", "$1:null");
      }
    }

    private static void UpgradeUiTemplates(GalleryDb ctx)
    {
      // Deactivate and rename all existing templates with "(3.2.1 version)" suffix. Then insert new ones.
      foreach (var uiTemplateAlbumDto in ctx.UiTemplateAlbums)
      {
        ctx.UiTemplateAlbums.Remove(uiTemplateAlbumDto);
      }

      var tmplGalleryId = ctx.Galleries.Single(g => g.IsTemplate).GalleryId;

      foreach (var uiTemplateDto in ctx.UiTemplates)
      {
        if (uiTemplateDto.FKGalleryId == tmplGalleryId)
        {
          // Remove all UI templates associated with the template gallery.
          ctx.UiTemplates.Remove(uiTemplateDto);
        }
        else
        {
          uiTemplateDto.Name += " (3.2.1 version)";
        }
      }

      ctx.SaveChanges();

      SeedController.InsertDefaultUiTemplates(ctx);
      SeedController.InsertAdditionalUiTemplates(ctx);

    }

    private static void UpgradeGalleryControlSettings(GalleryDb ctx)
    {
      foreach (var gcSetting in ctx.GalleryControlSettings.Where(gcs => gcs.SettingName == "ShowActionMenu"))
      {
        gcSetting.SettingName = "ShowRibbonToolbar";
      }

      var settingsToDelete = new[] { "ShowMediaObjectToolbar", "ShowUrlsButton", "ShowSlideShowButton", "ShowTransferMediaObjectButton", "ShowCopyMediaObjectButton", "ShowRotateMediaObjectButton", "ShowDeleteMediaObjectButton" };

      foreach (var gcSetting in ctx.GalleryControlSettings.Where(gcs => settingsToDelete.Contains(gcs.SettingName)))
      {
        ctx.GalleryControlSettings.Remove(gcSetting);
      }

      // There are also two new settings: MediaViewSize and SlideShowLoop, but we don't need to add them because the app will
      // do that the next time the admin saves the Gallery Control Settings page. (The values will inherit from gallery settings in the meantime.)
    }

    private static void UpdateMetadata(GalleryDb ctx)
    {
      const string dateTimeFormatString261 = "ddd, dd MMM yyyy h:mm:ss tt";

      // Populate RawValue with date/time data. Required for the improved sorting algorithm in 4.0 (feature #62)
      foreach (var gallery in ctx.Galleries.Where(g => !g.IsTemplate))
      {
        var mdDtSetting = ctx.GallerySettings.SingleOrDefault(gs => gs.FKGalleryId == gallery.GalleryId && gs.SettingName == "MetadataDateTimeFormatString");

        // Get the format string used for the dates. If mdDtSetting is null it's because we're upgrading from 2.6, so in that case use the
        // same format string that was hard-coded in that version. (FYI, the reason it's null in this case is because the 2.6 upgrade only 
        // adds a setting to the template gallery and depends on later startup code to add a copy for each gallery, but at this point that code hasn't run yet.)
        var dateTimeFormatString = mdDtSetting?.SettingValue ?? dateTimeFormatString261;

        var dateTimeMetas = new[] { MetadataItemName.DateAdded, MetadataItemName.DatePictureTaken, MetadataItemName.DateFileCreated, MetadataItemName.DateFileCreatedUtc, MetadataItemName.DateFileLastModified, MetadataItemName.DateFileLastModifiedUtc };
        foreach (var dateTimeMeta in ctx.Metadatas.Where(m => dateTimeMetas.Contains(m.MetaName) && m.RawValue == null))
        {
          DateTime result;
          if (DateTime.TryParseExact(dateTimeMeta.Value, dateTimeFormatString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result))
          {
            dateTimeMeta.RawValue = result.ToString("O", CultureInfo.InvariantCulture);
          }
          else if (DateTime.TryParseExact(dateTimeMeta.Value, dateTimeFormatString261, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result))
          {
            // We'll get here when restoring a 3.X backup file that was previously upgraded from 2.6.
            dateTimeMeta.RawValue = result.ToString("O", CultureInfo.InvariantCulture);
          }
        }
      }

      // Delete duplicate rating counts (bug #43). Find all rating counts where there are more than one for a media asset, then delete
      // all but the one with the highest value. (Note that album ratings are not supported, so we don't have to check FKAlbumId.)
      foreach (var grp in ctx.Metadatas.Where(m => m.MetaName == MetadataItemName.RatingCount && m.FKMediaObjectId != null).GroupBy(m => m.FKMediaObjectId).Where(grp => grp.Count() > 1))
      {
        foreach (var metadataDto in grp.ToList().OrderByDescending(m => Convert.ToInt32(m.Value)).AsQueryable().Skip(1))
        {
          ctx.Metadatas.Remove(metadataDto);
        }
      }
    }

    private static void UpdateMediaTemplates(GalleryDb ctx)
    {
      foreach (var mediaTemplateDto in ctx.MediaTemplates)
      {
        mediaTemplateDto.ScriptTemplate = mediaTemplateDto.ScriptTemplate.Replace("window.Gsp.msAjaxComponentId", "Gs.Vars.msAjaxComponentId");
      }

      // Several MIME types were changed in 4.0, but only audio/x-mp3 has a matching template that needs to be updated.
      foreach (var mediaTemplateDto in ctx.MediaTemplates.Where(mt => mt.MimeType == "audio/x-mp3"))
      {
        mediaTemplateDto.MimeType = "audio/mpeg";
      }
    }

    private static void UpgradeMimeTypes(GalleryDb ctx)
    {
      // Add new file types: 7z, ai, config, cr2, csv, m2p, m2t, nef, ps, vob
      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".7z"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".7z", MimeTypeValue = "application/x-7z-compressed", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".ai"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".ai", MimeTypeValue = "image/postscript", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".config"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".config", MimeTypeValue = "application/xml", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".cr2"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".cr2", MimeTypeValue = "image/x-raw", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".csv"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".csv", MimeTypeValue = "text/csv", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".m2p"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".m2p", MimeTypeValue = "video/mpeg", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".m2t"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".m2t", MimeTypeValue = "video/mpeg", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".nef"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".nef", MimeTypeValue = "image/x-nikon-nef", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".ps"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".ps", MimeTypeValue = "image/postscript", BrowserMimeTypeValue = "" });
      }

      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".vob"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto { FileExtension = ".vob", MimeTypeValue = "video/mpeg", BrowserMimeTypeValue = "" });
      }

      // Update the MIME types for these file extensions: avi, ico, js, m2ts, mid, midi, mod, mp3, mts, ras
      var mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".avi" && mt.MimeTypeValue == "video/x-ms-wvx");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "video/x-msvideo";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".ico" && mt.MimeTypeValue == "image/ico");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "image/x-icon";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".js" && mt.MimeTypeValue == "text/javascript");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "application/javascript";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".m2ts" && mt.MimeTypeValue == "video/MP2T");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "video/vnd.dlna.mpeg-tts";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".mid" && mt.MimeTypeValue == "audio/midi");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "audio/mid";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".midi" && mt.MimeTypeValue == "audio/midi");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "audio/midi";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".mod" && mt.MimeTypeValue == "audio/mod");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "video/mpeg";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".mp3" && mt.MimeTypeValue == "audio/x-mp3");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "audio/mpeg";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".mts" && mt.MimeTypeValue == "video/MP2T");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "video/vnd.dlna.mpeg-tts";
      }

      mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".ras" && mt.MimeTypeValue == "image/cmu-raster");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "image/x-cmu-raster";
      }

      // NOT PERFORMED: Even though a new 4.0 installation has fewer file types than 3.X, we do not delete any during upgrades,
      // since users may be using them and there isn't any harm in leaving them.
    }

    private static void RevertCeEfBugWorkAround()
    {
      // In DbManager.ChangeNamespaceForVersion4Upgrade we had to rename a key on the __MigrationHistory table to avoid the error
      // "The foreign key constraint does not exist. [ PK___MigrationHistory ]" on SQL CE DBs using EF 5 (GS 3.0 - 3.1). Detect
      // this situation and rename it to the original value, which is what all other versions are using, too.
      // https://entityframework.codeplex.com/workitem/2659

      var sqlCeController = new SqlCeController();

      using (var cn = sqlCeController.GetDbConnection())
      {
        using (var cmd = cn.CreateCommand())
        {
          cmd.CommandText = "SELECT COUNT(*) FROM Information_SCHEMA.KEY_COLUMN_USAGE WHERE CONSTRAINT_NAME='PK___MigrationHistory' AND TABLE_NAME='__MigrationHistory';";
          cn.Open();

          var hasWorkAroundKeyName = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

          if (hasWorkAroundKeyName)
          {
            cmd.CommandText = "ALTER TABLE __MigrationHistory DROP CONSTRAINT [PK___MigrationHistory];";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "ALTER TABLE __MigrationHistory ADD CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId],[ContextKey]);";
            cmd.ExecuteNonQuery();
          }
        }

      }
    }

  }
}