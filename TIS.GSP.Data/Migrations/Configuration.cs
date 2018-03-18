using System;
using System.Data.Entity.Migrations;

namespace GalleryServer.Data.Migrations
{
  /// <summary>
  /// Configuration related to the use of Code First data migrations for the gallery database.
  /// </summary>
  /// <remarks>
  /// To add a new migration, run Add-Migration v3.0.1 (replace 3.0.1 with the new version).
  /// To re-scaffold a migration, run 'Add-Migration v4.0.0' (replace name with the one you want to re-scaffold)
  /// </remarks>
  public sealed class GalleryDbMigrationConfiguration : DbMigrationsConfiguration<GalleryDb>
  {
 
    public Business.ProviderDataStore GalleryDataStore { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GalleryDbMigrationConfiguration"/> class.
    /// </summary>
    public GalleryDbMigrationConfiguration()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GalleryDbMigrationConfiguration"/> class.
    /// </summary>
    /// <param name="galleryDataStore">The gallery data store.</param>
    public GalleryDbMigrationConfiguration(Business.ProviderDataStore galleryDataStore)
    {
      GalleryDataStore = galleryDataStore;

      if (galleryDataStore == Business.ProviderDataStore.SqlServer)
      {
        // Increase the timeout used to apply migration changes, but only for SQL Server (SQL CE will throw an exception).
        CommandTimeout = 3600;
      }
      
      //AutomaticMigrationsEnabled = true;
      //AutomaticMigrationDataLossAllowed = true;
    }

    /// <summary>
    /// Runs after upgrading to the latest migration to allow seed data to be updated. Use this opportunity to apply bug
    /// fixes requiring updates to database values and to update the data schema in the AppSetting table.
    /// </summary>
    /// <param name="ctx">Context to be used for updating seed data.</param>
    protected override void Seed(GalleryDb ctx)
    {
      MigrateController.ApplyDbUpdates(ctx, GalleryDataStore);
    }
  }
}
