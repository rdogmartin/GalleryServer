using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Linq;
using ErikEJ.SqlCe;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Contains functionality for importing and exporting data to and from the gallery database.
  /// </summary>
  public static class BackupFileController
  {
    #region Properties

    /// <summary>
    /// Gets a collection of names of membership tables whose data is to be imported or exported into or from a data store.
    /// They are returned in the order in which they must be populated during a restoration. Reverse the collection if 
    /// you wish to delete the table contents.
    /// </summary>
    /// <value>The membership table names.</value>
    private static string[] MembershipTables26
    {
      get
      {
        return new[]
                 {
                   "aspnet_Applications", "aspnet_Users", "aspnet_Membership", "aspnet_Roles", "aspnet_UsersInRoles", "aspnet_Profile"
                 };
      }
    }

    /// <summary>
    /// Gets a collection of names of gallery tables whose data whose data we do NOT want to import from an earlier version. 
    /// This can be because those tables did not exist in that version or the data is obsolete.
    /// </summary>
    /// <value>The gallery table names.</value>
    private static string[] GalleryTableNamesToIgnoreDuringUpgrade
    {
      get
      {
        return new[]
                 {
                   "UiTemplate", "UiTemplateAlbum", "MediaTemplate", "MimeType",
                 };
      }
    }

    /// <summary>
    /// Gets a collection of names of gallery tables whose data is to be imported or exported into or from a data store.
    /// They are returned in the order in which they must be populated during a restoration. Reverse the collection if 
    /// you wish to delete the table contents.
    /// </summary>
    /// <value>The gallery table names.</value>
    private static string[] GalleryTableNames26
    {
      get
      {
        // Ignore these tables: gs_Synchronize, gs_AppError, gs_MediaQueue, gs_MimeType, gs_MimeTypeGallery
        return new[]
                 {
                   "gs_Gallery", "gs_GallerySetting", "gs_Album", "gs_Role", "gs_Role_Album",
                   "gs_MediaObject", "gs_MediaObjectMetadata",
                   "gs_UserGalleryProfile", "gs_AppSetting", "gs_GalleryControlSetting",
                 };
      }
    }

    /// <summary>
    /// Gets a dictionary that provides mapping between the 2.6 table names and their 3.0 equivalent.
    /// </summary>
    /// <value>A Dictionary instance.</value>
    private static Dictionary<string, string> TableName26To30Map
    {
      get
      {
        return new Dictionary<string, string>()
                 {
                   { "aspnet_Applications", "Applications" },
                   { "aspnet_Users", "Users" },
                   { "aspnet_Membership", "Memberships" },
                   { "aspnet_Roles", "Roles" },
                   { "aspnet_UsersInRoles", "UsersInRoles" },
                   { "aspnet_Profile", "Profiles" },
                   { "gs_Gallery", "Gallery" },
                   { "gs_GallerySetting", "GallerySetting" },
                   { "gs_Album", "Album" },
                   { "gs_Role", "Role" },
                   { "gs_Role_Album", "RoleAlbum" },
                   { "gs_MediaObject", "MediaObject" },
                   { "gs_MediaObjectMetadata", "Metadata" },
                   { "gs_UserGalleryProfile", "UserGalleryProfile" },
                   { "gs_AppSetting", "AppSetting" },
                   { "gs_GalleryControlSetting", "GalleryControlSetting" },
                 };
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Validates that the current backup file is valid and populates the remaining properties with information about the file.
    /// </summary>
    public static void ValidateFile(IBackupFile backupFile)
    {
      Validate(backupFile);
    }

    /// <summary>
    /// Imports the Gallery Server data into the current database, overwriting any existing data. Does not import the actual media
    /// files; they must be imported manually with a utility such as Windows Explorer. This method makes changes only to the database tables;
    /// no files in the media objects directory are affected. If both <see cref="IBackupFile.IncludeMembershipData" /> and 
    /// <see cref="IBackupFile.IncludeGalleryData" /> are false, then no action is taken.
    /// </summary>
    public static void ImportFromFile(IBackupFile backupFile)
    {
      Import(backupFile);
    }

    /// <summary>
    /// Exports the Gallery Server data in the current database to an XML-formatted string. Does not export the actual media files;
    /// they must be copied manually with a utility such as Windows Explorer. This method does not make any changes to the database tables
    /// or the files in the media objects directory.
    /// </summary>
    /// <returns>Returns an XML-formatted string containing the gallery data.</returns>
    public static string ExportToFile(IBackupFile backupFile)
    {
      return Export(backupFile);
    }

    /// <summary>
    /// Gets the data schema version of the database. May return null. Examples: "2.3.3421", "2.4.1"
    /// </summary>
    /// <returns>Returns an instance of <see cref="GalleryDataSchemaVersion"/> containing the database version.</returns>
    public static GalleryDataSchemaVersion GetDataSchemaVersion(DataSet ds)
    {
      var asTableNames = new[] { "AppSetting", "gs_AppSetting" };
      const string filter = "SettingName = 'DataSchemaVersion'";

      foreach (var asTableName in asTableNames)
      {
        if (!ds.Tables.Contains(asTableName))
          continue;

        var dr = ds.Tables[asTableName].Select(filter).FirstOrDefault();

        if (dr == null)
          throw new Exception(String.Format("The table {0} does not contain a row where {1}.", asTableName, filter));

        return GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToEnum(dr["SettingValue"].ToString());
      }

      throw new Exception(String.Format("The backup file does not contain one of the following required tables: {0} or {1}.", asTableNames[0], asTableNames[1]));
    }

    #endregion

    #region Functions

    /// <summary>
    /// Validates that the backup file specified is valid and populates the remaining properties with information about the file.
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    private static void Validate(IBackupFile backupFile)
    {
      using (var ds = GenerateDataSet(backupFile))
      {
        backupFile.SchemaVersion = GetDataSchemaVersion(ds);

        foreach (var tableName in GetTableNamesForValidation(backupFile))
        {
          var table = ds.Tables[tableName];
          var v3TableName = (backupFile.SchemaVersion >= GalleryDataSchemaVersion.V3_0_0 ? tableName : TableName26To30Map[tableName]);

          backupFile.DataTableRecordCount.Add(v3TableName, table.Rows.Count);
        }

        if (backupFile.SchemaVersion >= GalleryDataSchemaVersion.V2_6_0)
        {
          backupFile.IsValid = true;
        }
      }
    }

    private static void Import(IBackupFile backupFile)
    {
      switch (backupFile.GalleryDataStore)
      {
        case ProviderDataStore.SqlServer:
          ImportIntoSqlServer(backupFile);
          break;

        case ProviderDataStore.SqlCe:
          ImportIntoSqlCe(backupFile);
          break;

        default:
          throw new System.ComponentModel.InvalidEnumArgumentException(String.Format("The function was not designed to handle the enumeration value '{0}'", backupFile.GalleryDataStore));
      }
    }

    /// <summary>
    /// Imports the data into SQL server. This is identical to <see cref="ImportIntoSqlCe" /> except for the names of the classes
    /// SqlCeConnection/SqlConnection, SqlCeBulkCopy/SqlBulkCopy, SqlCeBulkCopyOptions/SqlBulkCopyOptions
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    private static void ImportIntoSqlServer(IBackupFile backupFile)
    {
      using (var ds = GenerateDataSet(backupFile))
      {
        backupFile.SchemaVersion = GetDataSchemaVersion(ds);

        using (var cn = new SqlConnection(backupFile.ConnectionString))
        {
          cn.Open();

          using (var tran = cn.BeginTransaction())
          {
            if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0)
            {
              Migrate26Controller.UpgradeData(ds, GalleryDataSchemaVersion.V3_2_1, backupFile.GalleryDataStore, cn, tran);
            }

            if (backupFile.SchemaVersion < GalleryDataSchemaVersion.V4_0_0)
            {
              MigrateController.UpgradeToCurrentSchema(ds);
            }

            DropSelfReferencingAlbumConstraint(backupFile, cn, tran);

            ClearData(backupFile, cn, tran);

            if (backupFile.IncludeMembershipData)
            {
              // SqlBulkCopy requires SQL permissions equivalent to that provided in the db_ddladmin or db_owner roles.
              using (var bulkCopy = new SqlBulkCopy(cn, SqlBulkCopyOptions.KeepIdentity, tran))
              {
                bulkCopy.BulkCopyTimeout = 3600; // 1 hour
                foreach (var tableName in backupFile.MembershipTables)
                {
                  bulkCopy.ColumnMappings.Clear();
                  bulkCopy.DestinationTableName = Utils.GetSqlName(tableName, backupFile.GalleryDataStore, "dbo");

                  foreach (DataColumn dc in ds.Tables[tableName].Columns)
                  {
                    bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                  }

                  // Write from the source to the destination.
                  try
                  {
                    bulkCopy.WriteToServer(ds.Tables[tableName]);
                  }
                  catch (Exception ex)
                  {
                    // Add a little info to exception and re-throw.
                    if (!ex.Data.Contains("SQL Bulk copy error"))
                    {
                      ex.Data.Add("SQL Bulk copy error", String.Format(CultureInfo.CurrentCulture, "Error occurred while importing table {0}.", tableName));
                    }
                    throw;
                  }
                }
              }
            }

            if (backupFile.IncludeGalleryData)
            {
              // Tables to skip: Event, MediaQueue, Synchronize

              // SqlBulkCopy requires SQL permissions equivalent to that provided in the db_ddladmin or db_owner roles.
              using (var bulkCopy = new SqlBulkCopy(cn, SqlBulkCopyOptions.KeepIdentity, tran))
              {
                bulkCopy.BulkCopyTimeout = 3600; // 1 hour
                foreach (var tableName in backupFile.GalleryTableNames)
                {
                  if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0 && Array.IndexOf(GalleryTableNamesToIgnoreDuringUpgrade, tableName) >= 0)
                    continue; // Don't import certain tables when upgrading because we want to keep the 3.0 data

                  bulkCopy.DestinationTableName = Utils.GetSqlName(tableName, backupFile.GalleryDataStore);
                  // Don't need to map the columns like we did in the membership section because it works without it.

                  // Write from the source to the destination.
                  try
                  {
                    bulkCopy.WriteToServer(ds.Tables[tableName]);
                  }
                  catch (Exception ex)
                  {
                    // Add a little info to exception and re-throw.
                    if (!ex.Data.Contains("SQL Bulk copy error"))
                    {
                      ex.Data.Add("SQL Bulk copy error", String.Format(CultureInfo.CurrentCulture, "Error occurred while importing table {0}.", tableName));
                    }
                    throw;
                  }
                }
              }
            }

            if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0)
            {
              Migrate26Controller.AddMissingMeta(backupFile.GalleryDataStore, cn, tran);
            }

            RestoreSelfReferencingAlbumConstraint(backupFile, cn, tran);

            tran.Commit();
          }
        }
      }

      CompactSqlCeDb(backupFile);
    }

    /// <summary>
    /// Imports the data into SQL CE. This is identical to <see cref="ImportIntoSqlServer" /> except for the names of the classes
    /// SqlCeConnection/SqlConnection, SqlCeBulkCopy/SqlBulkCopy, SqlCeBulkCopyOptions/SqlBulkCopyOptions
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    private static void ImportIntoSqlCe(IBackupFile backupFile)
    {
      using (var ds = GenerateDataSet(backupFile))
      {
        backupFile.SchemaVersion = GetDataSchemaVersion(ds);

        using (var cn = new SqlCeConnection(backupFile.ConnectionString))
        {
          cn.Open();

          using (var tran = cn.BeginTransaction())
          {
            if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0)
            {
              Migrate26Controller.UpgradeData(ds, GalleryDataSchemaVersion.V3_2_1, backupFile.GalleryDataStore, cn, tran);
            }

            if (backupFile.SchemaVersion < GalleryDataSchemaVersion.V4_0_0)
            {
              MigrateController.UpgradeToCurrentSchema(ds);
            }

            DropSelfReferencingAlbumConstraint(backupFile, cn, tran);

            ClearData(backupFile, cn, tran);

            if (backupFile.IncludeMembershipData)
            {
              // SqlBulkCopy requires SQL permissions equivalent to that provided in the db_ddladmin or db_owner roles.
              using (var bulkCopy = new SqlCeBulkCopy(cn, SqlCeBulkCopyOptions.KeepIdentity, tran))
              {
                bulkCopy.BulkCopyTimeout = 3600; // 1 hour
                foreach (var tableName in backupFile.MembershipTables)
                {
                  bulkCopy.ColumnMappings.Clear();
                  bulkCopy.DestinationTableName = Utils.GetSqlName(tableName, backupFile.GalleryDataStore, "dbo");

                  foreach (DataColumn dc in ds.Tables[tableName].Columns)
                  {
                    bulkCopy.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                  }

                  // Write from the source to the destination.
                  try
                  {
                    bulkCopy.WriteToServer(ds.Tables[tableName]);
                  }
                  catch (Exception ex)
                  {
                    // Add a little info to exception and re-throw.
                    if (!ex.Data.Contains("SQL Bulk copy error"))
                    {
                      ex.Data.Add("SQL Bulk copy error", String.Format(CultureInfo.CurrentCulture, "Error occurred while importing table {0}.", tableName));
                    }
                    throw;
                  }
                }
              }
            }

            if (backupFile.IncludeGalleryData)
            {
              // Tables to skip: Event, MediaQueue, Synchronize

              // SqlBulkCopy requires SQL permissions equivalent to that provided in the db_ddladmin or db_owner roles.
              using (var bulkCopy = new SqlCeBulkCopy(cn, SqlCeBulkCopyOptions.KeepIdentity, tran))
              {
                bulkCopy.BulkCopyTimeout = 3600; // 1 hour
                foreach (var tableName in backupFile.GalleryTableNames)
                {
                  if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0 && Array.IndexOf(GalleryTableNamesToIgnoreDuringUpgrade, tableName) >= 0)
                    continue; // Don't import certain tables when upgrading because we want to keep the 3.0 data

                  bulkCopy.DestinationTableName = Utils.GetSqlName(tableName, backupFile.GalleryDataStore);
                  // Don't need to map the columns like we did in the membership section because it works without it.

                  // Write from the source to the destination.
                  try
                  {
                    bulkCopy.WriteToServer(ds.Tables[tableName]);
                  }
                  catch (Exception ex)
                  {
                    // Add a little info to exception and re-throw.
                    if (!ex.Data.Contains("SQL Bulk copy error"))
                    {
                      ex.Data.Add("SQL Bulk copy error", String.Format(CultureInfo.CurrentCulture, "Error occurred while importing table {0}.", tableName));
                    }
                    throw;
                  }
                }
              }
            }

            if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0)
            {
              Migrate26Controller.AddMissingMeta(backupFile.GalleryDataStore, cn, tran);
            }

            RestoreSelfReferencingAlbumConstraint(backupFile, cn, tran);

            tran.Commit();
          }
        }
      }

      CompactSqlCeDb(backupFile);
    }

    /// <summary>
    /// Compacts the SQL CE database. No action is taken for other databases. Besides compacting the database, the
    /// process performs a critical task of resetting the identity columns of all tables. Without this step the app
    /// is at risk of generating the error 'A duplicate value cannot be inserted into a unique index' during a subsequent
    /// insert.
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    /// <exception cref="CannotCompactSqlCeException">Thrown when the database could not be compacted.</exception>
    private static void CompactSqlCeDb(IBackupFile backupFile)
    {
      if (backupFile.GalleryDataStore != ProviderDataStore.SqlCe)
        return;

      try
      {
        var engine = new SqlCeEngine(backupFile.ConnectionString);
        engine.Compact(null);
      }
      catch (SqlCeException)
      {
        // During testing it was observed that calling Compact could result in the error "Could not load database compaction library".
        // But if we pause and try again it succeeds.
        Pause();
        var engineSecondTry = new SqlCeEngine(backupFile.ConnectionString);
        try
        {
          engineSecondTry.Compact(null);
        }
        catch (SqlCeException ex)
        {
          throw new CannotCompactSqlCeException("The database was successfully restored, but it could not be compacted. Navigate to the Site Settings page and manually perform a compaction. Error: " + ex.Message, ex);
        }
      }
    }

    /// <summary>
    /// Exports the Gallery Server data in the current database to an XML-formatted string.
    /// </summary>
    /// <returns>Returns an XML-formatted string containing the gallery data.</returns>
    private static string Export(IBackupFile backupFile)
    {
      using (var ds = new DataSet("GalleryServerData"))
      {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        using (var stream = asm.GetManifestResourceStream("GalleryServer.Data.Schema.GalleryServerSchema.xml"))
        {
          ds.ReadXmlSchema(stream);
        }

        using (var cn = GetDbConnection(backupFile))
        {
          using (var cmd = cn.CreateCommand())
          {
            cn.Open();

            if (backupFile.IncludeMembershipData)
            {
              foreach (var tableName in backupFile.MembershipTables)
              {
                cmd.CommandText = String.Concat(@"SELECT * FROM ", Utils.GetSqlName(tableName, backupFile.GalleryDataStore, "dbo"), ";");
                ds.Load(cmd.ExecuteReader(), LoadOption.OverwriteChanges, tableName);
              }
            }

            if (backupFile.IncludeGalleryData)
            {
              foreach (var tableName in backupFile.GalleryTableNames)
              {
                cmd.CommandText = String.Concat(@"SELECT * FROM ", Utils.GetSqlName(tableName, backupFile.GalleryDataStore), ";");
                ds.Load(cmd.ExecuteReader(), LoadOption.OverwriteChanges, tableName);
              }
            }
            else
            {
              // We always want to export the AppSettings table because that contains the schema version, which we need during a restore.
              cmd.CommandText = String.Concat(@"SELECT * FROM ", Utils.GetSqlName("AppSetting", backupFile.GalleryDataStore), ";");
              ds.Load(cmd.ExecuteReader(), LoadOption.OverwriteChanges, "AppSetting");
            }

            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
              ds.WriteXml(sw, XmlWriteMode.WriteSchema);
              //ds.WriteXmlSchema(@"E:\GalleryServerSchema.xml"); // Use to create new schema file after a database change

              return sw.ToString();
            }
          }
        }
      }
    }

    /// <summary>
    /// Gets a database connection to the data store specified in <paramref name="backupFile" />.
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    /// <returns>DbConnection.</returns>
    /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when the data store in <paramref name="backupFile" />
    /// is not recognized.</exception>
    private static DbConnection GetDbConnection(IBackupFile backupFile)
    {
      DbConnection cn;
      switch (backupFile.GalleryDataStore)
      {
        case ProviderDataStore.SqlServer:
          cn = new SqlConnection(backupFile.ConnectionString);
          break;

        case ProviderDataStore.SqlCe:
          cn = new SqlCeConnection(backupFile.ConnectionString);
          break;

        default:
          throw new System.ComponentModel.InvalidEnumArgumentException(String.Format("The function was not designed to handle the enumeration value '{0}'", backupFile.GalleryDataStore));
      }
      return cn;
    }

    /// <summary>
    /// Generates a DataSet from the XML file specified in <paramref name="backupFile" />.
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    /// <returns>DataSet.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="backupFile" /> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when the file specified in <paramref name="backupFile" /> is not
    /// specified or does not exist.</exception>
    private static DataSet GenerateDataSet(IBackupFile backupFile)
    {
      if (backupFile == null)
        throw new ArgumentNullException("backupFile");

      if (String.IsNullOrWhiteSpace(backupFile.FilePath))
        throw new ArgumentException("A file path must be specified. (parameter backupFile, property FilePath)");

      if (!File.Exists(backupFile.FilePath))
        throw new ArgumentException(String.Format("The file {0} does not exist.", backupFile.FilePath));

      DataSet ds = null;
      try
      {
        ds = new DataSet("GalleryServerData")
        {
          Locale = CultureInfo.InvariantCulture
        };

        ds.ReadXml(backupFile.FilePath, XmlReadMode.Auto);

        return ds;
      }
      catch
      {
        if (ds != null)
          ds.Dispose();

        throw;
      }
    }

    /// <summary>
    /// Deletes the data in the database in prepration for a restore. Some tables may not be deleted per the business rules.
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    /// <param name="cn">The database connection.</param>
    /// <param name="tran">The transaction to use.</param>
    private static void ClearData(IBackupFile backupFile, DbConnection cn, DbTransaction tran)
    {
      using (var cmd = cn.CreateCommand())
      {
        cmd.Transaction = tran;

        if (backupFile.GalleryDataStore == ProviderDataStore.SqlServer)
        {
          // Increase timeout to give SQL Server time to clear the tables. (SQL CE throws an exception for any non-zero values, so we apply only for SQL Server)
          cmd.CommandTimeout = 3600; // 1 hour
        }

        if (backupFile.IncludeMembershipData)
        {
          foreach (var tableName in backupFile.MembershipTables.Reverse())
          {
            // Smoke the table contents.
            cmd.CommandText = String.Concat("DELETE FROM ", Utils.GetSqlName(tableName, backupFile.GalleryDataStore, "dbo"), ";");
            cmd.ExecuteNonQuery();
            Pause();
          }
        }

        if (backupFile.IncludeGalleryData)
        {
          foreach (var tableName in backupFile.GalleryTableNames.Reverse())
          {
            if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0 && Array.IndexOf(GalleryTableNamesToIgnoreDuringUpgrade, tableName) >= 0)
              continue; // Don't delete certain tables when upgrading because we don't have any source data from earlier version

            // Smoke the table contents.
            cmd.CommandText = String.Concat("DELETE FROM ", Utils.GetSqlName(tableName, backupFile.GalleryDataStore), ";");
            cmd.ExecuteNonQuery();
            Pause();
          }
        }
      }
    }

    /// <summary>
    /// Drop the self referencing album relationship, but only for SQL CE. This is required for SQL CE because it won't 
    /// let us delete the album records with the hierarchical foreign key present. It can be restored with 
    /// <see cref="RestoreSelfReferencingAlbumConstraint" />.
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    /// <param name="cn">The SQL connection.</param>
    /// <param name="tran">The transaction.</param>
    private static void DropSelfReferencingAlbumConstraint(IBackupFile backupFile, DbConnection cn, DbTransaction tran)
    {
      if (backupFile.GalleryDataStore == ProviderDataStore.SqlCe)
      {
        using (var cmd = cn.CreateCommand())
        {
          cmd.Transaction = tran;
          cmd.CommandText = "ALTER TABLE Album DROP CONSTRAINT [FK_gsp.Album_gsp.Album_FKAlbumParentId]";
          cmd.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Restore the self referencing album relationship that was deleted in <see cref="DropSelfReferencingAlbumConstraint" />, 
    /// but only for SQL CE.
    /// </summary>
    /// <param name="backupFile">The backup file.</param>
    /// <param name="cn">The SQL connection.</param>
    /// <param name="tran">The transaction.</param>
    private static void RestoreSelfReferencingAlbumConstraint(IBackupFile backupFile, DbConnection cn, DbTransaction tran)
    {
      if (backupFile.GalleryDataStore == ProviderDataStore.SqlCe)
      {
        using (var cmd = cn.CreateCommand())
        {
          cmd.Transaction = tran;
          cmd.CommandText = "ALTER TABLE Album ADD CONSTRAINT [FK_gsp.Album_gsp.Album_FKAlbumParentId] FOREIGN KEY(FKAlbumParentId) REFERENCES Album (AlbumId)";
          cmd.ExecuteNonQuery();
        }
      }
    }

    private static IEnumerable<string> GetTableNamesForValidation(IBackupFile backupFile)
    {
      if (backupFile.SchemaVersion == GalleryDataSchemaVersion.V2_6_0)
      {
        return MembershipTables26.Concat(GalleryTableNames26);
      }
      else
      {
        return backupFile.MembershipTables.Concat(backupFile.GalleryTableNames);
      }
    }

    /// <summary>
    /// Make the current thread sleep for 100 milliseconds. Without this pause, key constraint errors can occur during
    /// <see cref="ClearData" /> and the w3wp.exe process can fail during <see cref="Import" />. This appears to
    /// be a bug in the SQL CE 4.0 database engine and may be able to be removed in a later version.
    /// </summary>
    private static void Pause()
    {
      System.Threading.Thread.Sleep(100);
    }

    #endregion
  }
}
