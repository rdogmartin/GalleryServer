

using System.Data.Entity.Migrations;
using System.Linq;

namespace GalleryServer.Data.Migrations
{
  /// <summary>
  /// Represents the version 3.1.0 schema of the Gallery Server data structure.
  /// </summary>
  /// <remarks>To re-scaffold this migration, run 'Add-Migration 201309261612260_v3.1.0'</remarks>
  /// <seealso cref="System.Data.Entity.Migrations.DbMigration" />
  /// <seealso cref="System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata" />
  public partial class v310 : DbMigration
	{
		public override void Up()
		{
			AddColumn("gsp.Metadata", "RawValue", c => c.String(maxLength: 1000));
			AddColumn("gsp.MediaQueue", "ConversionType", c => c.Int(nullable: false));
			AddColumn("gsp.MediaQueue", "RotationAmount", c => c.Int(nullable: false));

			DropColumn("gsp.MediaObject", "HashKey");
		}

		public override void Down()
		{
			DropColumn("gsp.Metadata", "RawValue");
			DropColumn("gsp.MediaQueue", "ConversionType");
			DropColumn("gsp.MediaQueue", "RotationAmount");

			AddColumn("gsp.MediaObject", "HashKey", c => c.String(nullable: false, maxLength: 47));

			using (var ctx = new GalleryDb())
			{
				var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
				asDataSchema.SettingValue = "3.0.3";

				ctx.SaveChanges();
			}
		}
	}
}
