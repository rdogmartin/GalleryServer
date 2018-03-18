
using System;
using GalleryServer.Data;
using GalleryServer.Events;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// Contains functionality for performing administrative and maintenance tasks of the database.
  /// </summary>
  public static class DbManager
  {
    #region Methods

    /// <summary>
    /// Compacts and, if necessary, repairs the database. Applies only to SQL CE. A detailed message describing 
    /// the result of the operation is assigned to <paramref name="message" />.
    /// </summary>
    /// <param name="message">A detailed message describing the result of the operation.</param>
    /// <returns>Returns <c>true</c> if the operation is successful; otherwise returns <c>false</c>.</returns>
    public static bool CompactAndRepairSqlCeDatabase(out string message)
    {
      var sqlCeController = new SqlCeController();

      var compactSuccessful = false;
      var repairNeeded = false;
      var repairSuccessful = false;
      Exception ex = null;

      try
      {
        if (!sqlCeController.Verify())
        {
          repairNeeded = true;
          sqlCeController.Repair();

          repairSuccessful = sqlCeController.Verify();
        }

        sqlCeController.Compact();
        compactSuccessful = true;
      }
      catch (Exception exception)
      {
        ex = exception;
        EventController.RecordError(ex, AppSetting.Instance);
      }
      message = GetCompactAndRepairMessage(ex, compactSuccessful, repairNeeded, repairSuccessful);

      return (compactSuccessful && (!repairNeeded || repairSuccessful));
    }

    #endregion

    #region Functions

    private static string GetCompactAndRepairMessage(Exception ex, bool compactSuccessful, bool repairNeeded, bool repairSuccessful)
    {
      string msg = null;

      if (ex != null) // An exception occurred.
      {
        if (!compactSuccessful)
          msg = String.Concat("The following error occurred while compacting the database: ", EventController.GetExceptionDetails(ex));
        else if (compactSuccessful && !repairNeeded)
          msg = String.Concat("The database was successfully compacted but the following error occurred while checking the database for errors: ", EventController.GetExceptionDetails(ex));
        else if (compactSuccessful && repairNeeded && !repairSuccessful)
          msg = String.Concat("The database was successfully compacted. However, data corruption was found and the following error occurred while attempting to fix the errors: ", EventController.GetExceptionDetails(ex));
        else
          msg = String.Concat("The following error occurred: ", EventController.GetExceptionDetails(ex)); // This should never execute unless a dev changed the logic in CompactAndRepairSqlCe()
      }
      else // No exception occurred, compactSuccessful is guaranteed to be true
      {
        if (compactSuccessful && !repairNeeded)
          msg = "The SQL CE database was successfully compacted. No corruption was found.";
        else if (compactSuccessful && repairNeeded && !repairSuccessful)
          msg = "The SQL CE database was successfully compacted. Data corruption was found but could not be automatically repaired. Consider using the backup function to back up your data and restore to a new instance of your gallery.";
        else if (compactSuccessful && repairNeeded && repairSuccessful)
          msg = "The SQL CE database was successfully compacted. Data corruption was found and automatically repaired.";
        else
          throw new BusinessException(String.Format("An unexpected combination of parameters was passed to GetCompactAndRepairMessage(). ex != null; compactSuccessful={0}; repairNeeded={1}; repairSuccessful={2}", compactSuccessful, repairNeeded, repairSuccessful));
      }

      return msg;
    }

    #endregion

    /// <summary>
    /// Update database values that are affected by changing the name from Gallery Server Pro to Gallery Server. Specifically, (1) Change the 
    /// ContextKey in the __MigrationHistory table from "GalleryServerPro.Data.Migrations.GalleryDbMigrationConfiguration" to
    /// "GalleryServer.Data.Migrations.GalleryDbMigrationConfiguration" (required because the namespace was updated). (2) Change the Membership 
    /// application name. This is required because of the update to the applicationName attribute in the membership and roleManager sections of web.config.
    /// </summary>
    /// <param name="providerDataStore">The provider data store.</param>
    /// <param name="v4SchemaUpdateRequiredFilePath">The full path to the semaphore whose presence indicates that this function must run.
    /// This file is deleted at the end of this method. Example: "C:\Dev\GS\Dev-Main\Website\App_Data\v4_schema_update_required.txt"</param>
    /// <remarks>WARNING: If the membership points to a different database than the one the gallery data is in, or the current application name is something
    /// other than "Gallery Server Pro", the upgrade will fail to change the application name in the database. In this case, the admin should 
    /// manually update the text in the Applications.ApplicationName column or update the applicationName attribute in web.config to match the DB value.
    /// </remarks>
    public static void ChangeNamespaceForVersion4Upgrade(ProviderDataStore providerDataStore, string v4SchemaUpdateRequiredFilePath)
    {
      Data.Interfaces.IDbController sqlController = null;

      switch (providerDataStore)
      {
        case ProviderDataStore.SqlCe:
          sqlController = new SqlCeController();
          break;

        case ProviderDataStore.SqlServer:
          sqlController = new SqlServerController();
          break;
      }

      if (sqlController != null)
      {
        using (var cn = sqlController.GetDbConnection())
        {
          using (var cmd = cn.CreateCommand())
          {
            cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='__MigrationHistory' AND COLUMN_NAME='ContextKey'";

            try
            {
              cn.Open();
            }
            catch
            {
              // We'll typically get here when a user is installing Gallery Server since at that point the database may not exist. In this scenario
              // we don't want to run this upgrade script anyway, so just exit. We don't delete the semaphore file, though, because it's possible
              // the user *is* upgrading and got the connection string wrong. In that case, we want the error to bubble up to the user so she can
              // fix it. Eventually this function will run again and succeed, and the file will be deleted at the end of the function.
              return;
            }

            var hasContextKey = Convert.ToInt32(cmd.ExecuteScalar()) > 0; // GS 3.0-3.1 won't have a ContextKey column because EF 5 didn't have it

            if (hasContextKey)
            {
              // Change the EF context key to reflect the namespace change from GalleryServerPro to GalleryServer.
              cmd.CommandText = $"UPDATE {Utils.GetSqlName("__MigrationHistory", providerDataStore, "dbo")} SET ContextKey='GalleryServer.Data.Migrations.GalleryDbMigrationConfiguration' WHERE ContextKey='GalleryServerPro.Data.Migrations.GalleryDbMigrationConfiguration'";

              cmd.ExecuteNonQuery();
            }

            cmd.CommandText = "SELECT COUNT(*) from INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Applications'";

            var hasAppTable = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

            if (hasAppTable)
            {
              // Change the application name from "Gallery Server Pro" to "Gallery Server" in the Membership system.
              cmd.CommandText = $"UPDATE {Utils.GetSqlName("Applications", providerDataStore, "dbo")} SET ApplicationName='Gallery Server' WHERE ApplicationName='Gallery Server Pro'";
              cmd.ExecuteNonQuery();
            }

            // We need a workaround for the issue described at https://entityframework.codeplex.com/workitem/2659. If user is coming from a
            // version of GS that used EF 5 (GS 3.0-3.1), then rename the primary key on __MigrationHistory. Later in the Seed override,
            // we'll revert this change (Migrate40Controller.RevertCeEfBugWorkAround()). Without this, we'll get the error
            // "The foreign key constraint does not exist. [ PK___MigrationHistory ]" in DbMigrator.Update() method.
            if (providerDataStore == ProviderDataStore.SqlCe)
            {
              cmd.CommandText = "SELECT COUNT(*) FROM __MigrationHistory WHERE ProductVersion LIKE '6%'";

              var comingFromEf5 = Convert.ToInt32(cmd.ExecuteScalar()) == 0;

              if (comingFromEf5)
              {
                cmd.CommandText = "SELECT COUNT(*) FROM Information_SCHEMA.KEY_COLUMN_USAGE WHERE CONSTRAINT_NAME='PK_dbo.__MigrationHistory' AND TABLE_NAME='__MigrationHistory';";

                var hasKeyName = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

                if (hasKeyName)
                {
                  cmd.CommandText = "ALTER TABLE __MigrationHistory DROP CONSTRAINT [PK_dbo.__MigrationHistory];";
                  cmd.ExecuteNonQuery();
                  cmd.CommandText = "ALTER TABLE __MigrationHistory ADD CONSTRAINT [PK___MigrationHistory] PRIMARY KEY (MigrationId);";
                  cmd.ExecuteNonQuery();
                }
              }
            }
          }
        }
      }

      // Delete the semaphore file App_Data\v4_schema_update_required.txt so that this never runs again.
      System.IO.File.Delete(v4SchemaUpdateRequiredFilePath);
    }
  }
}
