using System.Data.Entity.Migrations;
using System.Linq;

namespace GalleryServer.Data.Migrations
{
  /// <summary>
  /// Represents the version 3.0.1 schema of the Gallery Server data structure.
  /// </summary>
  /// <remarks>
  /// To re-scaffold this migration, run 'Add-Migration 201306241655425_v3.0.1'
  /// To update DB to latest version, run 'Update-Database -Verbose'
  /// To downgrade to previous version, run 'Update-Database -TargetMigration: v3.0.0'
  /// </remarks>
  /// <seealso cref="System.Data.Entity.Migrations.DbMigration" />
  /// <seealso cref="System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata" />
  public partial class v301 : DbMigration
	{
		/// <summary>
		/// Operations to be performed during the upgrade process.
		/// </summary>
		public override void Up()
		{
		}

		/// <summary>
		/// Operations to be performed during the downgrade process.
		/// </summary>
		public override void Down()
		{
			using (var ctx = new GalleryDb())
			{
				var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
				asDataSchema.SettingValue = "3.0.0";

				ctx.SaveChanges();
			}
		}
	}
}
