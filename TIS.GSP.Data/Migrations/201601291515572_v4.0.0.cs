namespace GalleryServer.Data.Migrations
{
  using System;
  using System.Data.Entity.Migrations;
  using System.Linq;

  /// <summary>
  /// Represents the version 4.0.0 schema of the Gallery Server data structure.
  /// </summary>
  /// <remarks>To re-scaffold this migration, run 'Add-Migration v4.0.0'</remarks>
  /// <seealso cref="System.Data.Entity.Migrations.DbMigration" />
  /// <seealso cref="System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata" />
  public partial class v400 : DbMigration
  {
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
      DropIndex("gsp.MimeType", "UC_MimeType_FileExtension");
      AlterColumn("gsp.MimeType", "FileExtension", c => c.String(nullable: false, maxLength: 30));
      CreateIndex("gsp.MimeType", "FileExtension", true, "UC_MimeType_FileExtension");

      DropColumn("gsp.Album", "DateStart");
      DropColumn("gsp.Album", "DateEnd");
    }

    /// <summary>
    /// Operations to be performed during the downgrade process.
    /// </summary>
    public override void Down()
    {
      AddColumn("gsp.Album", "DateEnd", c => c.DateTime());
      AddColumn("gsp.Album", "DateStart", c => c.DateTime());

      // Don't actually reduce column length to 10 chars, since we'll lose data
      //DropIndex("gsp.MimeType", "UC_MimeType_FileExtension");
      //AlterColumn("gsp.MimeType", "FileExtension", c => c.String(nullable: false, maxLength: 10));
      //CreateIndex("gsp.MimeType", "FileExtension", true, "UC_MimeType_FileExtension");

      using (var ctx = new GalleryDb())
      {
        var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
        asDataSchema.SettingValue = "3.2.1";

        ctx.SaveChanges();
      }
    }
  }
}
