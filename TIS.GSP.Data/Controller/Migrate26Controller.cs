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
  /// Handle the migration of data from the 2.6 schema to the current schema.
  /// </summary>
  public static class Migrate26Controller
  {
    #region Methods

    /// <summary>
    /// Upgrades the <paramref name="ds" /> from the 2.6 schema to the 3.2.1 schema. This is done by creating a new set of tables in the
    /// dataset that match the 3.2.1 schema, then populating them with data from the 2.6 tables that are already present. As the data
    /// is copied, it is converted into the required format. The <paramref name="dataStore" />, <paramref name="cn" />, and 
    /// <paramref name="tran" /> are used only for updating the UI templates to point to the template gallery in the data to be imported.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    /// <param name="targetSchema">The schema version to convert to. Must be <see cref="GalleryDataSchemaVersion.V3_2_1" />; otherwise an 
    /// <see cref="ArgumentException" /> is thrown.</param>
    /// <param name="dataStore">The current data store.</param>
    /// <param name="cn">The connection being used to import the data.</param>
    /// <param name="tran">The transaction being used to import the data.</param>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="targetSchema" /> is not <see cref="GalleryDataSchemaVersion.V3_2_1" />.</exception>
    /// <exception cref="System.Exception">Thrown when the existing schema of the data in the <paramref name="ds" /> is not 
    /// <see cref="GalleryDataSchemaVersion.V2_6_0" />.</exception>
    public static void UpgradeData(DataSet ds, GalleryDataSchemaVersion targetSchema, ProviderDataStore dataStore, IDbConnection cn, IDbTransaction tran)
    {
      if (targetSchema != GalleryDataSchemaVersion.V3_2_1)
      {
        throw new ArgumentException(String.Format("This method is designed to upgrade only to version {0}.", GalleryDataSchemaVersion.V3_2_1), "targetSchema");
      }

      if (BackupFileController.GetDataSchemaVersion(ds) != GalleryDataSchemaVersion.V2_6_0)
      {
        throw new Exception(String.Format("This method is designed to upgrade from version {0} only.", GalleryDataSchemaVersion.V2_6_0));
      }

      var asm = Assembly.GetExecutingAssembly();
      using (var stream = asm.GetManifestResourceStream("GalleryServer.Data.Schema.GalleryServerSchema321.xml"))
      {
        // Read in the current schema. This creates new, empty tables we'll populate from the existing data.
        ds.ReadXmlSchema(stream);
      }

      MigrateApplications(ds);

      // Don't import profiles. Several columns have changed data types and GSP doesn't use it anyway.
      //ds.Tables["Profiles"].Merge(ds.Tables["aspnet_Profile"]);

      MigrateAspNetRoles(ds);

      MigrateUsers(ds);

      MigrateMemberships(ds);

      MigrateUsersInRoles(ds);

      MigrateAppSettings(ds, targetSchema);

      MigrateGalleries(ds);

      MigrateGallerySettings(ds);

      MigrateAlbums(ds);

      MigrateRoles(ds);

      MigrateRoleAlbums(ds);

      MigrateMediaObjects(ds);

      MigrateMetadata(ds);

      MigrateMimeTypeGalleries(ds, dataStore, cn, tran);

      MigrateTags(ds);

      MigrateUserGalleryProfiles(ds);

      MigrateGalleryControlSettings(ds);

      MigrateUiTemplates(ds, dataStore, cn, tran);
    }

    /// <summary>
    /// Adds the missing metadata that must be added for a 2.6 => 3 migration. This includes captions for all 
    /// media objects and tags/people for albums and media objects. No tags or people are assigned to the root album.
    /// All values are set to empty strings.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="cn">The connection.</param>
    /// <param name="tran">The transaction.</param>
    public static void AddMissingMeta(ProviderDataStore dataStore, IDbConnection cn, IDbTransaction tran)
    {
      // Add blank captions for all media objects and blank tags/people for albums and media objects.
      if (dataStore == ProviderDataStore.SqlCe)
      {
        ResetSqlCeIdentityColumn("Metadata", cn, tran);
      }

      var metaTableName = Utils.GetSqlName("Metadata", dataStore);
      var sqls = new[]
                   {
                     String.Format("INSERT INTO {0} (MetaName,FKMediaObjectId,FKAlbumId,Value) SELECT {1}, MediaObjectId, null, '' FROM {2};", metaTableName, (int)MetadataItemName.Caption, Utils.GetSqlName("MediaObject", dataStore)),
                     String.Format("INSERT INTO {0} (MetaName,FKMediaObjectId,FKAlbumId,Value) SELECT {1}, MediaObjectId, null, '' FROM {2} WHERE MediaObjectId NOT IN (SELECT FKMediaObjectId FROM {0} WHERE MetaName={1});", metaTableName, (int)MetadataItemName.Tags, Utils.GetSqlName("MediaObject", dataStore)),
                     String.Format("INSERT INTO {0} (MetaName,FKMediaObjectId,FKAlbumId,Value) SELECT {1}, MediaObjectId, null, '' FROM {2};", metaTableName, (int)MetadataItemName.People, Utils.GetSqlName("MediaObject", dataStore)),
                     String.Format("INSERT INTO {0} (MetaName,FKMediaObjectId,FKAlbumId,Value) SELECT {1}, null, AlbumId, '' FROM {2} WHERE FKAlbumParentId IS NOT NULL;", metaTableName, (int)MetadataItemName.Tags, Utils.GetSqlName("Album", dataStore)),
                     String.Format("INSERT INTO {0} (MetaName,FKMediaObjectId,FKAlbumId,Value) SELECT {1}, null, AlbumId, '' FROM {2} WHERE FKAlbumParentId IS NOT NULL;", metaTableName, (int)MetadataItemName.People, Utils.GetSqlName("Album", dataStore)),
                   };

      using (var cmd = cn.CreateCommand())
      {
        cmd.Transaction = tran;
        foreach (var sql in sqls)
        {
          cmd.CommandText = sql;
          try
          {
            cmd.ExecuteNonQuery();
          }
          catch (Exception ex)
          {
            if (!ex.Data.Contains("SQL"))
            {
              ex.Data.Add("SQL", sql);
            }
            throw;
          }
        }
      }
    }

    #endregion

    #region Functions

    /// <summary>
    /// Migrates the data from the aspnet_Applications table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateApplications(DataSet ds)
    {
      var dtSource = ds.Tables["aspnet_Applications"];
      var dtDest = ds.Tables["Applications"];

      var colsToSkip = new[] { "LoweredApplicationName" };

      dtSource.CopyTo(dtDest, colsToSkip);
    }

    /// <summary>
    /// Migrates the data from the aspnet_Roles table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateAspNetRoles(DataSet ds)
    {
      var dtSource = ds.Tables["aspnet_Roles"];
      var dtDest = ds.Tables["Roles"];

      var colsToSkip = new[] { "LoweredRoleName" };

      dtSource.CopyTo(dtDest, colsToSkip);
    }

    /// <summary>
    /// Migrates the data from the aspnet_Users table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateUsers(DataSet ds)
    {
      var dtSource = ds.Tables["aspnet_Users"];
      var dtDest = ds.Tables["Users"];

      var colsToSkip = new[] { "MobileAlias", "LoweredUserName" };

      dtSource.CopyTo(dtDest, colsToSkip);
    }

    /// <summary>
    /// Migrates the data from the aspnet_Membership table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateMemberships(DataSet ds)
    {
      // Copy from aspnet_Membership to Memberships, following these rules:
      // We don't copy MobilePIN or LoweredEmail.
      // Column 'Comment' changed from ntext to nvarchar(256).
      // Column 'FailedPasswordAnswerAttemptWindowStart' has been renamed 'FailedPasswordAnswerAttemptWindowsStart'.
      // All other columns can be copied
      var dtSource = ds.Tables["aspnet_Membership"];
      var dtDest = ds.Tables["Memberships"];

      var colsToSkip = new[] { "MobilePIN", "LoweredEmail" };

      foreach (DataRow rSource in dtSource.Rows)
      {
        var rDest = dtDest.NewRow();
        foreach (DataColumn col in dtSource.Columns)
        {
          if (Array.IndexOf(colsToSkip, col.ColumnName) >= 0)
            continue;

          switch (col.ColumnName)
          {
            case "FailedPasswordAnswerAttemptWindowStart":
              rDest["FailedPasswordAnswerAttemptWindowsStart"] = rSource[col];
              break;

            case "Comment":
              var comment = rSource[col] as string;
              if (!String.IsNullOrWhiteSpace(comment))
              {
                rDest[col.ColumnName] = comment.Substring(0, Math.Min(comment.Length, 256));
              }
              break;

            default:
              rDest[col.ColumnName] = rSource[col];
              break;
          }
        }

        dtDest.Rows.Add(rDest);
      }
    }

    private static void MigrateUsersInRoles(DataSet ds)
    {
      var dtSource = ds.Tables["aspnet_UsersInRoles"];
      var dtDest = ds.Tables["UsersInRoles"];

      dtSource.CopyTo(dtDest);
    }

    /// <summary>
    /// Migrates the data from the gs_AppSetting table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    /// <param name="targetSchema">Specifies the schema version to write to the DataSchemaVersion property in the AppSetting table.</param>
    private static void MigrateAppSettings(DataSet ds, GalleryDataSchemaVersion targetSchema)
    {
      // Copy from gs_AppSetting to AppSetting, following these rules:
      // Change value of ProductKey to ""; change DataSchemaVersion to the current version.
      // Update jQuery paths to most recent versions
      // Add settings Skin, JQueryMigrateScriptPath
      // Copy settings from gs_GallerySetting: EmailFromName, EmailFromAddress, SmtpServer, SmtpServerPort, SendEmailUsingSsl
      // All other columns can be copied

      // Copy all records from gs_AppSetting to AppSetting.
      var dtSource = ds.Tables["gs_AppSetting"];
      var dtDest = ds.Tables["AppSetting"];

      dtSource.CopyTo(dtDest);

      // Now change the values for ProductKey and DataSchemaVersion.
      dtDest.Select("SettingName = 'ProductKey'").First()["SettingValue"] = "";
      dtDest.Select("SettingName = 'DataSchemaVersion'").First()["SettingValue"] = GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(targetSchema);
      dtDest.Select("SettingName = 'JQueryScriptPath'").First()["SettingValue"] = "//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js";
      dtDest.Select("SettingName = 'JQueryUiScriptPath'").First()["SettingValue"] = "//ajax.googleapis.com/ajax/libs/jqueryui/1.10.4/jquery-ui.min.js";

      // Add new settings Skin and JQueryMigrateScriptPath.
      var dr = dtDest.NewRow();
      dr["SettingName"] = "Skin";
      dr["SettingValue"] = "dark";

      dtDest.Rows.Add(dr);

      dr = dtDest.NewRow();
      dr["SettingName"] = "JQueryMigrateScriptPath";
      dr["SettingValue"] = "//code.jquery.com/jquery-migrate-1.2.1.js";

      dtDest.Rows.Add(dr);

      // Copy a few settings from gs_GallerySetting
      var dtGs = ds.Tables["gs_GallerySetting"];
      var gsSettingNames = new[] { "EmailFromName", "EmailFromAddress", "SmtpServer", "SmtpServerPort", "SendEmailUsingSsl" };

      foreach (var gsSettingName in gsSettingNames)
      {
        var drGs = dtGs.Select(String.Format(CultureInfo.InvariantCulture, "IsTemplate = False AND SettingName = '{0}'", gsSettingName)).First();

        dr = dtDest.NewRow();
        dr["SettingName"] = drGs["SettingName"];
        dr["SettingValue"] = drGs["SettingValue"];

        dtDest.Rows.Add(dr);
      }
    }

    /// <summary>
    /// Migrates the data from the gs_Gallery table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateGalleries(DataSet ds)
    {
      // Copy from gs_Gallery to Gallery, following these rules:
      // Change template gallery ID from int.MinValue to -1 (required because 3 treats int.MinValue as an invalid gallery ID)
      // Set the IsTemplate property
      // Change the DateAdded property to the current date. This gives the user another 30 day trial period.
      var dtSource = ds.Tables["gs_Gallery"];
      var dtDest = ds.Tables["Gallery"];

      foreach (DataRow rSource in dtSource.Rows)
      {
        var rDest = dtDest.NewRow();
        foreach (DataColumn col in dtSource.Columns)
        {
          switch (col.ColumnName)
          {
            case "GalleryId":
              var isTmpl = Convert.ToInt32(rSource[col]) == int.MinValue;

              if (isTmpl)
                rDest[col.ColumnName] = -1; // Change template gallery ID from int.MinValue to -1
              else
                rDest[col.ColumnName] = rSource[col];

              rDest["IsTemplate"] = isTmpl;
              break;

            case "DateAdded":
              rDest[col.ColumnName] = DateTime.UtcNow;
              break;

            default:
              rDest[col.ColumnName] = rSource[col];
              break;
          }
        }

        dtDest.Rows.Add(rDest);
      }
    }

    /// <summary>
    /// Migrates the data from the gs_GallerySetting table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateGallerySettings(DataSet ds)
    {
      // Copy from gs_GallerySetting to GallerySetting, following these rules:
      // Change setting names: AllowAnonymousHiResViewing => EnableAnonymousOriginalMediaObjectDownload; MaxAlbumThumbnailTitleDisplayLength => MaxThumbnailTitleDisplayLength
      // Change setting values: MaxThumbnailTitleDisplayLength (50)
      // Change setting values to use v3 defaults: MetadataDisplaySettings, MediaEncoderSettings
      // Change the ID of the template gallery from int.MinValue to -1.
      // Add new settings: DefaultAlbumSortMetaName, DefaultAlbumSortAscending, AllowAnonymousRating, MetadataDateTimeFormatString, SlideShowType
      // Remove obsolete settings

      // Remove the column IsTemplate from the source table.
      ds.Tables["gs_GallerySetting"].Columns.Remove("IsTemplate");

      // Copy all records from gs_GallerySetting to GallerySetting.
      var dtSource = ds.Tables["gs_GallerySetting"];
      var dtDest = ds.Tables["GallerySetting"];

      dtSource.CopyTo(dtDest);

      // Rename settings.
      foreach (var r in dtDest.Select("SettingName = 'AllowAnonymousHiResViewing '"))
      {
        r["SettingName"] = "EnableAnonymousOriginalMediaObjectDownload";
      }

      foreach (var r in dtDest.Select("SettingName = 'MaxAlbumThumbnailTitleDisplayLength '"))
      {
        r["SettingName"] = "MaxThumbnailTitleDisplayLength";
      }

      // Change values for a few settings.
      foreach (var r in dtDest.Select("SettingName = 'MaxThumbnailTitleDisplayLength'"))
      {
        r["SettingValue"] = "50";
      }

      foreach (var r in dtDest.Select("SettingName = 'MetadataDisplaySettings'"))
      {
        r["SettingValue"] = "[{'MetadataItem':29,'Name':'Title','DisplayName':'TITLE','IsVisibleForAlbum':true,'IsVisibleForGalleryObject':true,'IsEditable':true,'DefaultValue':'{Title}','Sequence':0},{'MetadataItem':41,'Name':'Caption','DisplayName':'CAPTION','IsVisibleForAlbum':true,'IsVisibleForGalleryObject':true,'IsEditable':true,'DefaultValue':'{Comment}','Sequence':1},{'MetadataItem':22,'Name':'Tags','DisplayName':'TAGS','IsVisibleForAlbum':true,'IsVisibleForGalleryObject':true,'IsEditable':true,'DefaultValue':'{Tags}','Sequence':2},{'MetadataItem':42,'Name':'People','DisplayName':'PEOPLE','IsVisibleForAlbum':true,'IsVisibleForGalleryObject':true,'IsEditable':true,'DefaultValue':'{People}','Sequence':3},{'MetadataItem':112,'Name':'HtmlSource','DisplayName':'SOURCE HTML','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':true,'DefaultValue':'{HtmlSource}','Sequence':4},{'MetadataItem':34,'Name':'FileName','DisplayName':'File name','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{FileName}','Sequence':5},{'MetadataItem':35,'Name':'FileNameWithoutExtension','DisplayName':'File name','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{FileNameWithoutExtension}','Sequence':6},{'MetadataItem':111,'Name':'DateAdded','DisplayName':'Date Added','IsVisibleForAlbum':true,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{DateAdded}','Sequence':7},{'MetadataItem':8,'Name':'DatePictureTaken','DisplayName':'Date photo taken','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{DatePictureTaken}','Sequence':8},{'MetadataItem':26,'Name':'Rating','DisplayName':'Rating','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':true,'DefaultValue':'{Rating}','Sequence':9},{'MetadataItem':102,'Name':'GpsLocationWithMapLink','DisplayName':'Geotag','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'<a href=\\'http://maps.google.com/maps?q={GpsLatitude},{GpsLongitude}\\' target=\\'_blank\\' title=\\'View map\\'>{GpsLocation}</a>','Sequence':10},{'MetadataItem':106,'Name':'GpsDestLocationWithMapLink','DisplayName':'Geotag','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'<a href=\\'http://maps.google.com/maps?q={GpsLatitude},{GpsLongitude}\\' target=\\'_blank\\' title=\\'View map\\'>{GpsLocation}</a>','Sequence':11},{'MetadataItem':43,'Name':'Orientation','DisplayName':'Orientation','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Orientation}','Sequence':12},{'MetadataItem':14,'Name':'ExposureProgram','DisplayName':'Exposure program','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{ExposureProgram}','Sequence':13},{'MetadataItem':9,'Name':'Description','DisplayName':'Description','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Description}','Sequence':14},{'MetadataItem':5,'Name':'Comment','DisplayName':'Comment','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{Comment}','Sequence':15},{'MetadataItem':28,'Name':'Subject','DisplayName':'Subject','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Subject}','Sequence':16},{'MetadataItem':2,'Name':'Author','DisplayName':'Author','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Author}','Sequence':17},{'MetadataItem':4,'Name':'CameraModel','DisplayName':'Camera model','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{CameraModel}','Sequence':18},{'MetadataItem':6,'Name':'ColorRepresentation','DisplayName':'Color representation','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{ColorRepresentation}','Sequence':19},{'MetadataItem':7,'Name':'Copyright','DisplayName':'Copyright','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Copyright}','Sequence':20},{'MetadataItem':12,'Name':'EquipmentManufacturer','DisplayName':'Camera maker','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{EquipmentManufacturer}','Sequence':21},{'MetadataItem':13,'Name':'ExposureCompensation','DisplayName':'Exposure compensation','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{ExposureCompensation}','Sequence':22},{'MetadataItem':15,'Name':'ExposureTime','DisplayName':'Exposure time','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{ExposureTime}','Sequence':23},{'MetadataItem':16,'Name':'FlashMode','DisplayName':'Flash mode','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{FlashMode}','Sequence':24},{'MetadataItem':17,'Name':'FNumber','DisplayName':'F-stop','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{FNumber}','Sequence':25},{'MetadataItem':18,'Name':'FocalLength','DisplayName':'Focal length','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{FocalLength}','Sequence':26},{'MetadataItem':21,'Name':'IsoSpeed','DisplayName':'ISO speed','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IsoSpeed}','Sequence':27},{'MetadataItem':23,'Name':'LensAperture','DisplayName':'Aperture','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{LensAperture}','Sequence':28},{'MetadataItem':24,'Name':'LightSource','DisplayName':'Light source','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{LightSource}','Sequence':29},{'MetadataItem':10,'Name':'Dimensions','DisplayName':'Dimensions (pixels)','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{Dimensions}','Sequence':30},{'MetadataItem':25,'Name':'MeteringMode','DisplayName':'Metering mode','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{MeteringMode}','Sequence':31},{'MetadataItem':27,'Name':'SubjectDistance','DisplayName':'Subject distance','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{SubjectDistance}','Sequence':32},{'MetadataItem':11,'Name':'Duration','DisplayName':'Duration','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Duration}','Sequence':33},{'MetadataItem':1,'Name':'AudioFormat','DisplayName':'Audio format','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{AudioFormat}','Sequence':34},{'MetadataItem':32,'Name':'VideoFormat','DisplayName':'Video format','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{VideoFormat}','Sequence':35},{'MetadataItem':3,'Name':'BitRate','DisplayName':'Bit rate','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{BitRate}','Sequence':36},{'MetadataItem':0,'Name':'AudioBitRate','DisplayName':'AudioBitRate','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{AudioBitRate}','Sequence':37},{'MetadataItem':31,'Name':'VideoBitRate','DisplayName':'VideoBitRate','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{VideoBitRate}','Sequence':38},{'MetadataItem':20,'Name':'HorizontalResolution','DisplayName':'Horizontal resolution','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{HorizontalResolution}','Sequence':39},{'MetadataItem':30,'Name':'VerticalResolution','DisplayName':'Vertical resolution','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{VerticalResolution}','Sequence':40},{'MetadataItem':33,'Name':'Width','DisplayName':'Width','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Width}','Sequence':41},{'MetadataItem':19,'Name':'Height','DisplayName':'Height','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Height}','Sequence':42},{'MetadataItem':36,'Name':'FileSizeKb','DisplayName':'File size','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{FileSizeKb}','Sequence':43},{'MetadataItem':37,'Name':'DateFileCreated','DisplayName':'File created','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{DateFileCreated}','Sequence':44},{'MetadataItem':38,'Name':'DateFileCreatedUtc','DisplayName':'File created (UTC)','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{DateFileCreatedUtc}','Sequence':45},{'MetadataItem':39,'Name':'DateFileLastModified','DisplayName':'File last modified','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{DateFileLastModified}','Sequence':46},{'MetadataItem':40,'Name':'DateFileLastModifiedUtc','DisplayName':'File last modified (UTC)','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{DateFileLastModifiedUtc}','Sequence':47},{'MetadataItem':101,'Name':'GpsLocation','DisplayName':'GPS location','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsLocation}','Sequence':48},{'MetadataItem':103,'Name':'GpsLatitude','DisplayName':'GPS latitude','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsLatitude}','Sequence':49},{'MetadataItem':104,'Name':'GpsLongitude','DisplayName':'GPS longitude','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsLongitude}','Sequence':50},{'MetadataItem':105,'Name':'GpsDestLocation','DisplayName':'GPS dest. location','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsDestLocation}','Sequence':51},{'MetadataItem':108,'Name':'GpsDestLongitude','DisplayName':'GPS dest. longitude','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsDestLongitude}','Sequence':52},{'MetadataItem':107,'Name':'GpsDestLatitude','DisplayName':'GPS dest. latitude','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsDestLatitude}','Sequence':53},{'MetadataItem':110,'Name':'GpsVersion','DisplayName':'GPS version','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsVersion}','Sequence':54},{'MetadataItem':109,'Name':'GpsAltitude','DisplayName':'GPS altitude','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'{GpsAltitude}','Sequence':55},{'MetadataItem':113,'Name':'RatingCount','DisplayName':'Number of ratings','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'0','Sequence':56},{'MetadataItem':1012,'Name':'IptcOriginalTransmissionReference','DisplayName':'Transmission ref.','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcOriginalTransmissionReference}','Sequence':57},{'MetadataItem':1013,'Name':'IptcProvinceState','DisplayName':'Province/State','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcProvinceState}','Sequence':58},{'MetadataItem':1010,'Name':'IptcKeywords','DisplayName':'IptcKeywords','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcKeywords}','Sequence':59},{'MetadataItem':1011,'Name':'IptcObjectName','DisplayName':'Object name','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcObjectName}','Sequence':60},{'MetadataItem':1014,'Name':'IptcRecordVersion','DisplayName':'Record version','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcRecordVersion}','Sequence':61},{'MetadataItem':1017,'Name':'IptcSublocation','DisplayName':'Sub-location','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcSublocation}','Sequence':62},{'MetadataItem':1018,'Name':'IptcWriterEditor','DisplayName':'Writer/Editor','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcWriterEditor}','Sequence':63},{'MetadataItem':1015,'Name':'IptcSource','DisplayName':'Source','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcSource}','Sequence':64},{'MetadataItem':1016,'Name':'IptcSpecialInstructions','DisplayName':'Instructions','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcSpecialInstructions}','Sequence':65},{'MetadataItem':1003,'Name':'IptcCaption','DisplayName':'Caption','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcCaption}','Sequence':66},{'MetadataItem':1004,'Name':'IptcCity','DisplayName':'City','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcCity}','Sequence':67},{'MetadataItem':1001,'Name':'IptcByline','DisplayName':'By-line','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcByline}','Sequence':68},{'MetadataItem':1002,'Name':'IptcBylineTitle','DisplayName':'By-line title','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcBylineTitle}','Sequence':69},{'MetadataItem':1005,'Name':'IptcCopyrightNotice','DisplayName':'Copyright','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcCopyrightNotice}','Sequence':70},{'MetadataItem':1008,'Name':'IptcDateCreated','DisplayName':'Date created','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcDateCreated}','Sequence':71},{'MetadataItem':1009,'Name':'IptcHeadline','DisplayName':'Headline','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcHeadline}','Sequence':72},{'MetadataItem':1006,'Name':'IptcCountryPrimaryLocationName','DisplayName':'Country','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcCountryPrimaryLocationName}','Sequence':73},{'MetadataItem':1007,'Name':'IptcCredit','DisplayName':'Credit','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{IptcCredit}','Sequence':74},{'MetadataItem':2000,'Name':'Custom1','DisplayName':'Custom1','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':75},{'MetadataItem':2001,'Name':'Custom2','DisplayName':'Custom2','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':76},{'MetadataItem':2002,'Name':'Custom3','DisplayName':'Custom3','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':77},{'MetadataItem':2003,'Name':'Custom4','DisplayName':'Custom4','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':78},{'MetadataItem':2004,'Name':'Custom5','DisplayName':'Custom5','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':79},{'MetadataItem':2005,'Name':'Custom6','DisplayName':'Custom6','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':80},{'MetadataItem':2006,'Name':'Custom7','DisplayName':'Custom7','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':81},{'MetadataItem':2007,'Name':'Custom8','DisplayName':'Custom8','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':82},{'MetadataItem':2008,'Name':'Custom9','DisplayName':'Custom9','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':83},{'MetadataItem':2009,'Name':'Custom10','DisplayName':'Custom10','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':84},{'MetadataItem':2010,'Name':'Custom11','DisplayName':'Custom11','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':85},{'MetadataItem':2011,'Name':'Custom12','DisplayName':'Custom12','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':86},{'MetadataItem':2012,'Name':'Custom13','DisplayName':'Custom13','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':87},{'MetadataItem':2013,'Name':'Custom14','DisplayName':'Custom14','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':88},{'MetadataItem':2014,'Name':'Custom15','DisplayName':'Custom15','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':89},{'MetadataItem':2015,'Name':'Custom16','DisplayName':'Custom16','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':90},{'MetadataItem':2016,'Name':'Custom17','DisplayName':'Custom17','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':91},{'MetadataItem':2017,'Name':'Custom18','DisplayName':'Custom18','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':92},{'MetadataItem':2018,'Name':'Custom19','DisplayName':'Custom19','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':93},{'MetadataItem':2019,'Name':'Custom20','DisplayName':'Custom20','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':false,'IsEditable':false,'DefaultValue':'','Sequence':94}]";
      }

      foreach (var r in dtDest.Select("SettingName = 'MediaEncoderSettings'"))
      {
        r["SettingValue"] = @".mp3||.mp3||~~.flv||.flv||~~.m4a||.m4a||~~*video||.mp4||-y -i ""{SourceFilePath}"" -vf ""scale=min(iw*min(640/iw\,480/ih)\,iw):min(ih*min(640/iw\,480/ih)\,ih){AutoRotateFilter}"" -vcodec libx264 -movflags +faststart -metadata:s:v:0 rotate=0 ""{DestinationFilePath}""~~*audio||.m4a||-i ""{SourceFilePath}"" -y ""{DestinationFilePath}""";
      }

      // Change the ID of the template gallery from int.MinValue to -1.
      foreach (var r in dtDest.Select(String.Concat("FKGalleryId = ", int.MinValue)))
      {
        r["FKGalleryId"] = -1;
      }

      // Add new settings DefaultAlbumSortMetaName, DefaultAlbumSortAscending, AllowAnonymousRating, MetadataDateTimeFormatString, SlideShowType
      var tmplGalleryId = Convert.ToInt32(ds.Tables["Gallery"].Select("IsTemplate = true").First()["GalleryId"]);

      var dr = dtDest.NewRow();
      dr["FKGalleryId"] = tmplGalleryId;
      dr["SettingName"] = "DefaultAlbumSortMetaName";
      dr["SettingValue"] = "111";

      dtDest.Rows.Add(dr);

      dr = dtDest.NewRow();
      dr["FKGalleryId"] = tmplGalleryId;
      dr["SettingName"] = "DefaultAlbumSortAscending";
      dr["SettingValue"] = "True";

      dtDest.Rows.Add(dr);

      dr = dtDest.NewRow();
      dr["FKGalleryId"] = tmplGalleryId;
      dr["SettingName"] = "AllowAnonymousRating";
      dr["SettingValue"] = "True";

      dtDest.Rows.Add(dr);

      dr = dtDest.NewRow();
      dr["FKGalleryId"] = tmplGalleryId;
      dr["SettingName"] = "MetadataDateTimeFormatString";
      dr["SettingValue"] = "MMM dd, yyyy h:mm:ss tt";

      dtDest.Rows.Add(dr);

      dr = dtDest.NewRow();
      dr["FKGalleryId"] = tmplGalleryId;
      dr["SettingName"] = "SlideShowType";
      dr["SettingValue"] = "FullScreen";

      dtDest.Rows.Add(dr);

      // Remove unused settings
      const string filter = "SettingName IN ('ThumbnailWidthBuffer', 'ThumbnailHeightBuffer', 'EnableMetadata', 'ThumbnailClickShowsOriginal', 'SilverlightFileTypes', " +
                            "'EnablePermalink', 'MaxMediaObjectThumbnailTitleDisplayLength', 'MediaObjectCaptionTemplate', 'GpsMapUrlTemplate', 'EmailFromName', " +
                            "'EmailFromAddress', 'SmtpServer', 'SmtpServerPort', 'SendEmailUsingSsl')";

      foreach (var r in dtDest.Select(filter))
      {
        dtDest.Rows.Remove(r);
      }
    }

    /// <summary>
    /// Migrates the data from the gs_Album table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateAlbums(DataSet ds)
    {
      // Copy from gs_Album to Album, following these rules:
      // When we encounter the title and summary, insert them as rows in the metadata table
      // AlbumParentId => FKAlbumParentId
      // When AlbumParentId = 0, switch to null
      // Populate default values for SortByMetaName, SortAscending
      // All other columns can be copied
      var dtSource = ds.Tables["gs_Album"];
      var dtDest = ds.Tables["Album"];
      var dtMeta = ds.Tables["Metadata"];

      foreach (DataRow rSource in dtSource.Rows)
      {
        var rDest = dtDest.NewRow();
        foreach (DataColumn col in dtSource.Columns)
        {
          switch (col.ColumnName)
          {
            case "Title":
            case "Summary":
              // Insert a row into the metadata table for the title or summary.
              var r = dtMeta.NewRow();
              r["MetaName"] = (int)(col.ColumnName == "Title" ? MetadataItemName.Title : MetadataItemName.Caption);
              r["FKAlbumId"] = rSource["AlbumId"];
              r["Value"] = rSource[col];
              dtMeta.Rows.Add(r);
              break;

            case "AlbumParentId":
              if (Convert.ToInt32(rSource[col]) == 0)
                rDest["FKAlbumParentId"] = DBNull.Value;
              else
                rDest["FKAlbumParentId"] = rSource[col];
              break;

            default:
              rDest[col.ColumnName] = rSource[col];
              break;
          }
        }

        rDest["SortByMetaName"] = 111;
        rDest["SortAscending"] = true;

        dtDest.Rows.Add(rDest);
      }
    }

    /// <summary>
    /// Migrates the data from the gs_Role table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateRoles(DataSet ds)
    {
      var dtSource = ds.Tables["gs_Role"];
      var dtDest = ds.Tables["Role"];

      dtSource.CopyTo(dtDest);
    }

    /// <summary>
    /// Migrates the data from the gs_Role_Album table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateRoleAlbums(DataSet ds)
    {
      var dtSource = ds.Tables["gs_Role_Album"];
      var dtDest = ds.Tables["RoleAlbum"];

      dtSource.CopyTo(dtDest);
    }

    /// <summary>
    /// Migrates the data from the gs_MediaObject table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateMediaObjects(DataSet ds)
    {
      // Copy from gs_MediaObject to MediaObject, following these rules:
      // When we encounter the title, insert it as a row in the metadata table
      // When OptimizedFilename is blank, copy the values over from the original columns
      // Don't copy column HashKey
      // All other columns can be copied
      var dtSource = ds.Tables["gs_MediaObject"];
      var dtDest = ds.Tables["MediaObject"];
      var dtMeta = ds.Tables["Metadata"];
      var colsToSkip = new[] { "HashKey" };

      foreach (DataRow rSource in dtSource.Rows)
      {
        bool? requiresDataFromOriginalColumns = null;
        var rDest = dtDest.NewRow();
        foreach (DataColumn col in dtSource.Columns)
        {
          if (Array.IndexOf(colsToSkip, col.ColumnName) >= 0)
            continue;

          switch (col.ColumnName)
          {
            case "Title":
              // Insert a row into the metadata table for the title.
              var r = dtMeta.NewRow();
              r["MetaName"] = (int)MetadataItemName.Title;
              r["FKMediaObjectId"] = rSource["MediaObjectId"];
              r["Value"] = rSource[col];
              dtMeta.Rows.Add(r);
              break;

            case "OptimizedFilename":
              if (String.IsNullOrWhiteSpace(rSource[col].ToString()))
              {
                requiresDataFromOriginalColumns = true;
                rDest[col.ColumnName] = rSource["OriginalFilename"];
              }
              else
              {
                requiresDataFromOriginalColumns = false;
                rDest[col.ColumnName] = rSource[col];
              }
              break;

            case "OptimizedWidth":
            case "OptimizedHeight":
            case "OptimizedSizeKB":
              if (!requiresDataFromOriginalColumns.HasValue)
                throw new DataException("The function expected the variable requiresDataFromOriginalColumns to be assigned a value, but it wasn't.");

              if (requiresDataFromOriginalColumns.Value)
              {
                rDest[col.ColumnName] = rSource[col.ColumnName.Replace("Optimized", "Original")];
              }
              else
              {
                rDest[col.ColumnName] = rSource[col.ColumnName];
              }
              break;

            default:
              rDest[col.ColumnName] = rSource[col];
              break;
          }
        }

        dtDest.Rows.Add(rDest);
      }
    }

    /// <summary>
    /// Migrates the data from the gs_MediaObjectMetadata table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateMetadata(DataSet ds)
    {
      // Copy from gs_MediaObjectMetadata to Metadata, following these rules:
      // Skip any rows for title, caption, or people
      // Skip MediaObjectMetadataId and Description (we skip MediaObjectMetadataId because it's value could conflict
      // with new ID's assigned during the title and caption imports for albums and media objects.
      // MetadataNameIdentifier => MetaName
      // For tags (MetadataNameIdentifier = 22), replace semi-colon with comma.
      // All other columns can be copied

      CheckFor23Metadata(ds);

      var dtSource = ds.Tables["gs_MediaObjectMetadata"];
      var dtDest = ds.Tables["Metadata"];

      var rowsToSkip = new[] { MetadataItemName.Title, MetadataItemName.Caption, MetadataItemName.People };
      var colsToSkip = new[] { "MediaObjectMetadataId", "Description" };

      foreach (DataRow rSource in dtSource.Rows)
      {
        var metaId = (MetadataItemName)Convert.ToInt32(rSource["MetadataNameIdentifier"]);

        if (!MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(metaId) || (Array.IndexOf(rowsToSkip, metaId) >= 0))
          continue;

        bool? isTagRow = null;
        var rDest = dtDest.NewRow();
        foreach (DataColumn col in dtSource.Columns)
        {
          if (Array.IndexOf(colsToSkip, col.ColumnName) >= 0)
            continue;

          switch (col.ColumnName)
          {
            case "MetadataNameIdentifier":
              isTagRow = (metaId == MetadataItemName.Tags);

              rDest["MetaName"] = (int)metaId;
              break;

            case "Value":
              if (!isTagRow.HasValue)
                throw new DataException("The function expected the variable isTagRow to be assigned a value, but it wasn't.");

              if (isTagRow.Value)
              {
                rDest[col.ColumnName] = ValidateTags(rSource[col].ToString());
              }
              else
                rDest[col.ColumnName] = rSource[col];
              break;

            default:
              rDest[col.ColumnName] = rSource[col];
              break;
          }
        }

        dtDest.Rows.Add(rDest);
      }
    }

    /// <summary>
    /// Verify that each tag is less than 100 characters, do not contain apostrophes, and that they are separated by commas, not semi-colons.
    /// </summary>
    /// <param name="tagList">A semi-colon separated list of tags.</param>
    /// <returns>System.String.</returns>
    private static string ValidateTags(string tagList)
    {
      var tags = new List<string>(tagList.Trim().Split(new[] { "; ", ";" }, StringSplitOptions.RemoveEmptyEntries));
      var validatedTags = new List<string>(tags.Count);

      validatedTags.AddRange(tags.Select(t => t.Length > 100 ? t.Substring(0, 100).Replace("'", "") : t.Replace("'", "")));

      return String.Join(",", validatedTags);
    }

    /// <summary>
    /// See if there are any metadata with the old 2.3 and earlier values. If so, re-map to the 2.4+ values.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    /// <remarks>The values of the enum FormattedMetadataItemName changed between 2.3 and 2.4, but it had little impact on users.
    /// However, in v3, this is a problem because we expect the ID values in the table to map to the right enumeration. So
    /// we loop through and fix the MetadataNameIdentifier column if necessary.</remarks>
    private static void CheckFor23Metadata(DataSet ds)
    {
      // A pretty good indication of having the old values is when we discover a value of 0 for the tag (22) value.
      // The old system stored rating in 22, with 0 being the default value.
      if (ds.Tables["gs_MediaObjectMetadata"].Select("MetadataNameIdentifier = 22").All(dr => dr["Value"].ToString() != "0"))
        return;

      // We have the old values. Re-map.
      var dt = ds.Tables["gs_MediaObjectMetadata"];

      var kvps = new List<KeyValuePair<string, int>>
                   {
                     new KeyValuePair<string, int>("Author", 2),
                     new KeyValuePair<string, int>("Camera model", 4),
                     new KeyValuePair<string, int>("Comment", 5),
                     new KeyValuePair<string, int>("Color representation", 6),
                     new KeyValuePair<string, int>("Copyright", 7),
                     new KeyValuePair<string, int>("Date taken", 8),
                     new KeyValuePair<string, int>("Description", 9),
                     new KeyValuePair<string, int>("Dimensions (pixels)", 10),
                     new KeyValuePair<string, int>("Camera maker", 12),
                     new KeyValuePair<string, int>("Exposure compensation", 13),
                     new KeyValuePair<string, int>("Exposure program", 14),
                     new KeyValuePair<string, int>("Exposure time", 15),
                     new KeyValuePair<string, int>("Flash mode", 16),
                     new KeyValuePair<string, int>("F-stop", 17),
                     new KeyValuePair<string, int>("Focal length", 18),
                     new KeyValuePair<string, int>("Height", 19),
                     new KeyValuePair<string, int>("Horizontal resolution", 20),
                     new KeyValuePair<string, int>("ISO speed", 21),
                     new KeyValuePair<string, int>("Keywords", 22),
                     new KeyValuePair<string, int>("Aperture", 23),
                     new KeyValuePair<string, int>("Light source", 24),
                     new KeyValuePair<string, int>("Metering mode", 25),
                     new KeyValuePair<string, int>("Rating (out of 5)", 26),
                     new KeyValuePair<string, int>("Subject distance", 27),
                     new KeyValuePair<string, int>("Subject", 28),
                     new KeyValuePair<string, int>("Title", 29),
                     new KeyValuePair<string, int>("Vertical resolution", 30),
                     new KeyValuePair<string, int>("Width", 33)
                   };

      foreach (var kvp in kvps)
      {
        foreach (var dr in dt.Select(String.Format(CultureInfo.InvariantCulture, "Description = '{0}'", kvp.Key)))
        {
          dr["MetadataNameIdentifier"] = kvp.Value;
        }
      }
    }

    /// <summary>
    /// Migrates the IsEnabled property from the gs_MimeTypeGallery table. It's OK if there are new MIME types in later versions that
    /// aren't present in the old table - validation code during app startup verifies there is a record in MimeTypeGallery for each
    /// gallery and MIME type.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    /// <param name="dataStore">The data store currently being used for gallery data.</param>
    /// <param name="cn">The connection.</param>
    /// <param name="tran">The transaction.</param>
    private static void MigrateMimeTypeGalleries(DataSet ds, ProviderDataStore dataStore, IDbConnection cn, IDbTransaction tran)
    {
      var dtSource = ds.Tables["gs_MimeTypeGallery"];
      var dtDest = ds.Tables["MimeTypeGallery"];
      var mimeType26 = ds.Tables["gs_MimeType"];
      var colsToSkip = new[] { "MimeTypeGalleryId" };

      using (var cmd = cn.CreateCommand())
      {
        cmd.Transaction = tran;

        foreach (DataRow rSource in dtSource.Rows)
        {
          var rDest = dtDest.NewRow();
          var fileExt = mimeType26.Select(String.Concat("MimeTypeId = ", rSource["FKMimeTypeId"])).First()["FileExtension"];

          // Get the MimeTypeId from the MimeType table. We can't use the ID from gs_MimeType because we aren't migrating that
          // data and the ID values in MimeType might be different.
          cmd.CommandText = String.Format(CultureInfo.InvariantCulture, @"SELECT MimeTypeId FROM {0} WHERE FileExtension='{1}'",
            Utils.GetSqlName("MimeType", dataStore), 
            fileExt);

          var mimeTypeId = cmd.ExecuteScalar();

          if (mimeTypeId == null)
            continue;

          foreach (DataColumn col in dtSource.Columns)
          {
            if (Array.IndexOf(colsToSkip, col.ColumnName) >= 0)
              continue;

            switch (col.ColumnName)
            {
              case "FKMimeTypeId":
                rDest[col.ColumnName] = mimeTypeId;
                break;

              default:
                rDest[col.ColumnName] = rSource[col];
                break;
            }
          }

          dtDest.Rows.Add(rDest);
        }
      }
    }

    /// <summary>
    /// Find any tags that are in the metadata table and make sure parsed versions of them exist in the Tag and MetadataTag tables.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateTags(DataSet ds)
    {
      var dtMeta = ds.Tables["Metadata"];
      var galleryId = int.MinValue;
      var nonTmplGalleries = ds.Tables["Gallery"].Select("IsTemplate = False");

      if (nonTmplGalleries.Count() == 1)
      {
        // There's only a single gallery, so we know that's the only gallery ID we'll be needing below. This will allow us to avoid looking
        // up the gallery ID for every media object.
        galleryId = Convert.ToInt32(nonTmplGalleries.First()["GalleryId"]);
      }

      var dtMTag = ds.Tables["MetadataTag"];
      var dtTag = ds.Tables["Tag"];
      foreach (var dr in dtMeta.Select(String.Format(CultureInfo.InvariantCulture, "MetaName = {0} AND FKMediaObjectId IS NOT NULL AND LEN(Value) > 0", (int)MetadataItemName.Tags)))
      {
        var gId = (galleryId > int.MinValue ? galleryId : GetGalleryId(ds, Convert.ToInt32(dr["FKMediaObjectId"])));

        foreach (var tag in ParseTags(dr["Value"].ToString()))
        {
          // First add to Tag table if it doesn't already exist.
          if (!dtTag.Select(String.Format(CultureInfo.InvariantCulture, "TagName = '{0}'", tag)).Any())
          {
            var rTag = dtTag.NewRow();

            rTag["TagName"] = tag;

            dtTag.Rows.Add(rTag);
          }

          // Now add to MetadataTag table.
          var r = dtMTag.NewRow();

          r["FKMetadataId"] = dr["MetadataId"];
          r["FKTagName"] = tag;
          r["FKGalleryId"] = gId;

          dtMTag.Rows.Add(r);
        }
      }
    }

    /// <summary>
    /// Gets the gallery ID for the specified <paramref name="mediaObjectId" />. The value is retrieved from the MediaObject 
    /// and Album tables in the <paramref name="ds" />.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    /// <param name="mediaObjectId">The media object ID.</param>
    /// <returns>System.Int32.</returns>
    private static int GetGalleryId(DataSet ds, int mediaObjectId)
    {
      var albumId = Convert.ToInt32(ds.Tables["MediaObject"].Select("MediaObjectId = " + mediaObjectId).First()["FKAlbumId"]);

      return Convert.ToInt32(ds.Tables["Album"].Select("AlbumId = " + albumId).First()["FKGalleryId"]);
    }

    /// <summary>
    /// Migrates the data from the gs_GallerySetting table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateUserGalleryProfiles(DataSet ds)
    {
      // Copy from gs_UserGalleryProfile to UserGalleryProfile, following these rules:
      // Delete the setting named ShowMediaObjectMetadata

      var dtSource = ds.Tables["gs_UserGalleryProfile"];
      var dtDest = ds.Tables["UserGalleryProfile"];

      dtSource.CopyTo(dtDest);

      // Remove unused settings
      foreach (var dr in dtDest.Select("SettingName = 'ShowMediaObjectMetadata'"))
      {
        dtDest.Rows.Remove(dr);
      }
    }

    /// <summary>
    /// Migrates the data from the gs_GalleryControlSetting table.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    private static void MigrateGalleryControlSettings(DataSet ds)
    {
      // Copy from gs_GalleryControlSetting to GalleryControlSetting, following these rules:
      // Rename these settings: ShowAlbumTreeViewForAlbum (ShowLeftPaneForAlbum), ShowAlbumTreeViewForMediaObject (ShowLeftPaneForMediaObject), ShowPermalinkButton (ShowUrlsButton)
      // Delete these: ShowMetadataButton, ShowMediaObjectDownloadButton, ShowMediaObjectZipDownloadButton, ShowHighResImageButton
      
      // Copy all records from gs_GalleryControlSetting to GalleryControlSetting.
      var dtSource = ds.Tables["gs_GalleryControlSetting"];
      var dtDest = ds.Tables["GalleryControlSetting"];

      dtSource.CopyTo(dtDest);

      // Change a few setting names.
      foreach (var dr in dtDest.Select("SettingName = 'ShowAlbumTreeViewForAlbum'"))
      {
        dr["SettingName"] = "ShowLeftPaneForAlbum";
      }

      foreach (var dr in dtDest.Select("SettingName = 'ShowAlbumTreeViewForMediaObject'"))
      {
        dr["SettingName"] = "ShowLeftPaneForMediaObject";
      }

      foreach (var dr in dtDest.Select("SettingName = 'ShowPermalinkButton'"))
      {
        dr["SettingName"] = "ShowUrlsButton";
      }

      // Remove unused settings
      foreach (var dr in dtDest.Select("SettingName IN ('ShowMetadataButton', 'ShowMediaObjectDownloadButton', 'ShowMediaObjectZipDownloadButton', 'ShowHighResImageButton')"))
      {
        dtDest.Rows.Remove(dr);
      }
    }

    /// <summary>
    /// Update the UITemplate table to work with the new data. We don't have any templates from a previous version, but we have the current ones for 3 in 
    /// the database, so what we do is find the UI templates that map to the template gallery and update the FKGalleryId value to the template gallery ID
    /// in the data we are importing, then we'll delete any UI templates not belonging to the template gallery.
    /// </summary>
    /// <param name="ds">The DataSet.</param>
    /// <param name="dataStore">The data store currently being used for gallery data.</param>
    /// <param name="cn">The connection.</param>
    /// <param name="tran">The transaction.</param>
    private static void MigrateUiTemplates(DataSet ds, ProviderDataStore dataStore, IDbConnection cn, IDbTransaction tran)
    {
      // Get the template gallery ID for the gallery we'll be importing. At this moment this info is in the dataset because we haven't yet
      // imported it into the table.
      var tmplGalleryId = Convert.ToInt32(ds.Tables["Gallery"].Select("IsTemplate = true").First()["GalleryId"]);

      using (var cmd = cn.CreateCommand())
      {
        // Get the UI templates belonging to the template gallery. We have to do a join here because the data
        // model doesn't have a relationship. (Doing so would conflict with the relationship between
        // the UITemplateAlbum and Album tables.)
        var uiTmplTableName = Utils.GetSqlName("UiTemplate", dataStore);
        using (var repo = new UiTemplateRepository())
        {
          var ctx = repo.Context;
          var tmplForTmplGallery = from uiTmpl in ctx.UiTemplates join g in ctx.Galleries on uiTmpl.FKGalleryId equals g.GalleryId where g.IsTemplate select uiTmpl;

          // For each UI template, make sure one exists in the gallery
          cmd.Transaction = tran;
          foreach (var uiTmpl in tmplForTmplGallery)
          {
            var sql = String.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET FKGalleryId = {1} WHERE UiTemplateId = {2};", uiTmplTableName, tmplGalleryId, uiTmpl.UiTemplateId);
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
          }
        }

        using (var cmdDr = cn.CreateCommand())
        {
          cmdDr.Transaction = tran;
          cmdDr.CommandText = String.Format(CultureInfo.InvariantCulture, "SELECT UiTemplateId FROM {0} WHERE FKGalleryId <> {1};", uiTmplTableName, tmplGalleryId);
          using (var dr = cmdDr.ExecuteReader())
          {
            while (dr != null && dr.Read())
            {
              var sql = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE UiTemplateId = {1};", uiTmplTableName, dr[0]);
              cmd.CommandText = sql;
              cmd.ExecuteNonQuery();
            }
          }
        }
      }
    }

    /// <summary>
    /// Parses the comma separated tags into a collection of string values.
    /// </summary>
    /// <param name="value">The comma separated tags (e.g. "Vacation, New York, 2013").</param>
    /// <returns>Returns a list of strings.</returns>
    private static IEnumerable<string> ParseTags(string value)
    {
      return new List<string>(value.Trim().Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Resets the SQL CE identity column of the <paramref name="tableName" /> to the next available ID. This is necessary
    /// because SQL CE does not automatically keep track of ID values when IDENTITY_INSERT is on, which is what we're doing
    /// during the bulk insert.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="cn">The connection.</param>
    /// <param name="tran">The transaction.</param>
    private static void ResetSqlCeIdentityColumn(string tableName, IDbConnection cn, IDbTransaction tran)
    {
      var maxId = GetMaxId(tableName, "MetadataId", cn, tran);
      var sql = String.Format(CultureInfo.InvariantCulture, "ALTER TABLE Metadata ALTER COLUMN MetadataId IDENTITY({0}, 1)", ++maxId);

      using (var cmd = cn.CreateCommand())
      {
        cmd.Transaction = tran;
        cmd.CommandText = sql;
        try
        {
          cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
          if (!ex.Data.Contains("SQL"))
          {
            ex.Data.Add("SQL", sql);
          }
          throw;
        }
      }
    }

    /// <summary>
    /// Gets the largest value of the <paramref name="idColName" /> in the table <paramref name="tableName" />.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="idColName">Name of the column. The data type is expected to be a number that can be processed with the MAX() function.</param>
    /// <param name="cn">The connection.</param>
    /// <param name="tran">The transaction.</param>
    /// <returns>System.Int32.</returns>
    private static int GetMaxId(string tableName, string idColName, IDbConnection cn, IDbTransaction tran)
    {
      var sql = String.Format(CultureInfo.InvariantCulture, "SELECT MAX({0}) FROM {1}", idColName, tableName);

      using (var cmd = cn.CreateCommand())
      {
        cmd.Transaction = tran;
        cmd.CommandText = sql;
        try
        {
          return (int)cmd.ExecuteScalar();
        }
        catch (Exception ex)
        {
          if (!ex.Data.Contains("SQL"))
          {
            ex.Data.Add("SQL", sql);
          }
          throw;
        }
      }
    }

    /// <summary>
    /// Copies the contents of <paramref name="dtSource" /> into <paramref name="dtDest" />. Requires that the structure be the same
    /// between the two tables (column names and data types).
    /// </summary>
    /// <param name="dtSource">The dt source.</param>
    /// <param name="dtDest">The dt dest.</param>
    /// <param name="columnNamesToSkip">The column names to skip.</param>
    private static void CopyTo(this DataTable dtSource, DataTable dtDest, string[] columnNamesToSkip = null)
    {
      foreach (DataRow rSource in dtSource.Rows)
      {
        var rDest = dtDest.NewRow();
        foreach (DataColumn col in dtSource.Columns)
        {
          if (columnNamesToSkip != null && Array.IndexOf(columnNamesToSkip, col.ColumnName) >= 0)
            continue;

          rDest[col.ColumnName] = rSource[col];
        }

        dtDest.Rows.Add(rDest);
      }
    }

    #endregion
  }
}