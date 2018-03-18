using System.Data.Entity.Migrations;
using System.Linq;

namespace GalleryServer.Data.Migrations
{

  /// <summary>
  /// Represents the version 3.2.1 schema of the Gallery Server data structure.
  /// </summary>
  /// <remarks>To re-scaffold this migration, run 'Add-Migration v3.2.1'</remarks>
  /// <seealso cref="System.Data.Entity.Migrations.DbMigration" />
  /// <seealso cref="System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata" />
  public partial class v321 : DbMigration
	{
		public override void Up()
		{
		}

		public override void Down()
		{
			using (var ctx = new GalleryDb())
			{
				var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
				asDataSchema.SettingValue = "3.2.0";

				ctx.SaveChanges();
			}
		}
	}
}
