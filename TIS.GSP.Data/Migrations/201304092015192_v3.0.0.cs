namespace GalleryServer.Data.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  /// <summary>
  /// Represents the version 3.0.0 schema of the Gallery Server data structure.
  /// </summary>
  /// <remarks>
  /// To re-scaffold this migration, run 'Add-Migration 201304092015192_v3.0.0'
  /// To update DB to latest version, run 'Update-Database -Verbose'
  /// To roll back to empty DB, run 'Update-Database –TargetMigration: $InitialDatabase'
  /// To go to a particular version, run 'Update-Database –TargetMigration: v3.0.2'
  /// To start over, delete this file, force the auto-creation of the DB, enter Add-Migration v3.0.0, then replace the
  /// auto-generated v300 class with this one.
  /// </remarks>
  /// <seealso cref="System.Data.Entity.Migrations.DbMigration" />
  /// <seealso cref="System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata" />
  public partial class v300 : DbMigration
  {
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
      CreateTable(
    "gsp.Album",
    c => new
    {
      AlbumId = c.Int(nullable: false, identity: true),
      FKGalleryId = c.Int(nullable: false),
      FKAlbumParentId = c.Int(),
      DirectoryName = c.String(nullable: false, maxLength: 255),
      ThumbnailMediaObjectId = c.Int(nullable: false),
      SortByMetaName = c.Int(nullable: false),
      SortAscending = c.Boolean(nullable: false),
      Seq = c.Int(nullable: false),
      DateStart = c.DateTime(),
      DateEnd = c.DateTime(),
      DateAdded = c.DateTime(nullable: false),
      CreatedBy = c.String(nullable: false, maxLength: 256),
      LastModifiedBy = c.String(nullable: false, maxLength: 256),
      DateLastModified = c.DateTime(nullable: false),
      OwnedBy = c.String(nullable: false, maxLength: 256),
      OwnerRoleName = c.String(nullable: false, maxLength: 256),
      IsPrivate = c.Boolean(nullable: false),
    })
    .PrimaryKey(t => t.AlbumId)
    .ForeignKey("gsp.Gallery", t => t.FKGalleryId, cascadeDelete: true)
    .ForeignKey("gsp.Album", t => t.FKAlbumParentId)
    .Index(t => t.FKGalleryId)
    .Index(t => t.FKAlbumParentId);

      CreateTable(
          "gsp.Gallery",
          c => new
          {
            GalleryId = c.Int(nullable: false, identity: true),
            Description = c.String(nullable: false, maxLength: 1000),
            IsTemplate = c.Boolean(nullable: false),
            DateAdded = c.DateTime(nullable: false),
          })
          .PrimaryKey(t => t.GalleryId);

      CreateTable(
          "gsp.Metadata",
          c => new
          {
            MetadataId = c.Int(nullable: false, identity: true),
            MetaName = c.Int(nullable: false),
            FKMediaObjectId = c.Int(),
            FKAlbumId = c.Int(),
            Value = c.String(nullable: false),
          })
          .PrimaryKey(t => t.MetadataId)
          .ForeignKey("gsp.MediaObject", t => t.FKMediaObjectId, cascadeDelete: true)
          .ForeignKey("gsp.Album", t => t.FKAlbumId)
          .Index(t => t.FKMediaObjectId)
          .Index(t => t.FKAlbumId);

      CreateTable(
          "gsp.MediaObject",
          c => new
          {
            MediaObjectId = c.Int(nullable: false, identity: true),
            FKAlbumId = c.Int(nullable: false),
            HashKey = c.String(nullable: false, maxLength: 47),
            ThumbnailFilename = c.String(nullable: false, maxLength: 255),
            ThumbnailWidth = c.Int(nullable: false),
            ThumbnailHeight = c.Int(nullable: false),
            ThumbnailSizeKB = c.Int(nullable: false),
            OptimizedFilename = c.String(nullable: false, maxLength: 255),
            OptimizedWidth = c.Int(nullable: false),
            OptimizedHeight = c.Int(nullable: false),
            OptimizedSizeKB = c.Int(nullable: false),
            OriginalFilename = c.String(nullable: false, maxLength: 255),
            OriginalWidth = c.Int(nullable: false),
            OriginalHeight = c.Int(nullable: false),
            OriginalSizeKB = c.Int(nullable: false),
            ExternalHtmlSource = c.String(nullable: false),
            ExternalType = c.String(nullable: false, maxLength: 15),
            Seq = c.Int(nullable: false),
            CreatedBy = c.String(nullable: false, maxLength: 256),
            DateAdded = c.DateTime(nullable: false),
            LastModifiedBy = c.String(nullable: false, maxLength: 256),
            DateLastModified = c.DateTime(nullable: false),
            IsPrivate = c.Boolean(nullable: false),
          })
          .PrimaryKey(t => t.MediaObjectId)
          .ForeignKey("gsp.Album", t => t.FKAlbumId, cascadeDelete: true)
          .Index(t => t.FKAlbumId);

      CreateTable(
          "gsp.MetadataTag",
          c => new
          {
            FKMetadataId = c.Int(nullable: false),
            FKTagName = c.String(nullable: false, maxLength: 100),
            FKGalleryId = c.Int(nullable: false),
          })
          .PrimaryKey(t => new { t.FKMetadataId, t.FKTagName })
          .ForeignKey("gsp.Metadata", t => t.FKMetadataId, cascadeDelete: true)
          .ForeignKey("gsp.Tag", t => t.FKTagName, cascadeDelete: true)
          .Index(t => t.FKMetadataId)
          .Index(t => t.FKTagName);

      CreateTable(
          "gsp.Tag",
          c => new
          {
            TagName = c.String(nullable: false, maxLength: 100),
          })
          .PrimaryKey(t => t.TagName);

      CreateTable(
          "gsp.UiTemplateAlbum",
          c => new
          {
            FKUiTemplateId = c.Int(nullable: false),
            FKAlbumId = c.Int(nullable: false),
          })
          .PrimaryKey(t => new { t.FKUiTemplateId, t.FKAlbumId })
          .ForeignKey("gsp.UiTemplate", t => t.FKUiTemplateId, cascadeDelete: true)
          .ForeignKey("gsp.Album", t => t.FKAlbumId, cascadeDelete: true)
          .Index(t => t.FKUiTemplateId)
          .Index(t => t.FKAlbumId);

      CreateTable(
          "gsp.UiTemplate",
          c => new
          {
            UiTemplateId = c.Int(nullable: false, identity: true),
            TemplateType = c.Int(nullable: false),
            FKGalleryId = c.Int(nullable: false),
            Name = c.String(nullable: false, maxLength: 255),
            Description = c.String(nullable: false),
            HtmlTemplate = c.String(nullable: false),
            ScriptTemplate = c.String(nullable: false),
          })
          .PrimaryKey(t => t.UiTemplateId);

      CreateTable(
          "gsp.Event",
          c => new
          {
            EventId = c.Int(nullable: false, identity: true),
            FKGalleryId = c.Int(nullable: false),
            EventType = c.Int(nullable: false),
            TimeStampUtc = c.DateTime(nullable: false),
            Message = c.String(nullable: false, maxLength: 4000),
            EventData = c.String(nullable: false),
            ExType = c.String(nullable: false, maxLength: 1000),
            ExSource = c.String(nullable: false, maxLength: 1000),
            ExTargetSite = c.String(nullable: false),
            ExStackTrace = c.String(nullable: false),
            InnerExType = c.String(nullable: false, maxLength: 1000),
            InnerExMessage = c.String(nullable: false, maxLength: 4000),
            InnerExSource = c.String(nullable: false, maxLength: 1000),
            InnerExTargetSite = c.String(nullable: false),
            InnerExStackTrace = c.String(nullable: false),
            InnerExData = c.String(nullable: false),
            Url = c.String(nullable: false, maxLength: 1000),
            FormVariables = c.String(nullable: false),
            Cookies = c.String(nullable: false),
            SessionVariables = c.String(nullable: false),
            ServerVariables = c.String(nullable: false),
          })
          .PrimaryKey(t => t.EventId)
          .ForeignKey("gsp.Gallery", t => t.FKGalleryId, cascadeDelete: true)
          .Index(t => t.FKGalleryId);

      CreateTable(
          "gsp.AppSetting",
          c => new
          {
            AppSettingId = c.Int(nullable: false, identity: true),
            SettingName = c.String(nullable: false, maxLength: 200),
            SettingValue = c.String(nullable: false),
          })
          .PrimaryKey(t => t.AppSettingId);

      CreateTable(
          "gsp.MediaTemplate",
          c => new
          {
            MediaTemplateId = c.Int(nullable: false, identity: true),
            MimeType = c.String(nullable: false, maxLength: 200),
            BrowserId = c.String(nullable: false, maxLength: 50),
            HtmlTemplate = c.String(nullable: false),
            ScriptTemplate = c.String(nullable: false),
          })
          .PrimaryKey(t => t.MediaTemplateId);

      CreateTable(
          "gsp.GalleryControlSetting",
          c => new
          {
            GalleryControlSettingId = c.Int(nullable: false, identity: true),
            ControlId = c.String(nullable: false, maxLength: 350),
            SettingName = c.String(nullable: false, maxLength: 200),
            SettingValue = c.String(nullable: false),
          })
          .PrimaryKey(t => t.GalleryControlSettingId);

      CreateTable(
          "gsp.GallerySetting",
          c => new
          {
            GallerySettingId = c.Int(nullable: false, identity: true),
            FKGalleryId = c.Int(nullable: false),
            SettingName = c.String(nullable: false, maxLength: 200),
            SettingValue = c.String(nullable: false),
          })
          .PrimaryKey(t => t.GallerySettingId)
          .ForeignKey("gsp.Gallery", t => t.FKGalleryId, cascadeDelete: true)
          .Index(t => t.FKGalleryId);

      CreateTable(
          "gsp.MimeType",
          c => new
          {
            MimeTypeId = c.Int(nullable: false, identity: true),
            FileExtension = c.String(nullable: false, maxLength: 10),
            MimeTypeValue = c.String(nullable: false, maxLength: 200),
            BrowserMimeTypeValue = c.String(nullable: false, maxLength: 200),
          })
          .PrimaryKey(t => t.MimeTypeId);

      CreateTable(
          "gsp.MimeTypeGallery",
          c => new
          {
            MimeTypeGalleryId = c.Int(nullable: false, identity: true),
            FKGalleryId = c.Int(nullable: false),
            FKMimeTypeId = c.Int(nullable: false),
            IsEnabled = c.Boolean(nullable: false),
          })
          .PrimaryKey(t => t.MimeTypeGalleryId)
          .ForeignKey("gsp.Gallery", t => t.FKGalleryId, cascadeDelete: true)
          .ForeignKey("gsp.MimeType", t => t.FKMimeTypeId, cascadeDelete: true)
          .Index(t => t.FKGalleryId)
          .Index(t => t.FKMimeTypeId);

      CreateTable(
          "gsp.Role",
          c => new
          {
            RoleName = c.String(nullable: false, maxLength: 256),
            AllowViewAlbumsAndObjects = c.Boolean(nullable: false),
            AllowViewOriginalImage = c.Boolean(nullable: false),
            AllowAddChildAlbum = c.Boolean(nullable: false),
            AllowAddMediaObject = c.Boolean(nullable: false),
            AllowEditAlbum = c.Boolean(nullable: false),
            AllowEditMediaObject = c.Boolean(nullable: false),
            AllowDeleteChildAlbum = c.Boolean(nullable: false),
            AllowDeleteMediaObject = c.Boolean(nullable: false),
            AllowSynchronize = c.Boolean(nullable: false),
            HideWatermark = c.Boolean(nullable: false),
            AllowAdministerGallery = c.Boolean(nullable: false),
            AllowAdministerSite = c.Boolean(nullable: false),
          })
          .PrimaryKey(t => t.RoleName);

      CreateTable(
          "gsp.RoleAlbum",
          c => new
          {
            FKRoleName = c.String(nullable: false, maxLength: 256),
            FKAlbumId = c.Int(nullable: false),
          })
          .PrimaryKey(t => new { t.FKRoleName, t.FKAlbumId })
          .ForeignKey("gsp.Role", t => t.FKRoleName, cascadeDelete: true)
          .ForeignKey("gsp.Album", t => t.FKAlbumId, cascadeDelete: true)
          .Index(t => t.FKRoleName)
          .Index(t => t.FKAlbumId);

      CreateTable(
          "gsp.Synchronize",
          c => new
          {
            FKGalleryId = c.Int(nullable: false),
            SynchId = c.String(nullable: false, maxLength: 46),
            SynchState = c.Int(nullable: false),
            TotalFiles = c.Int(nullable: false),
            CurrentFileIndex = c.Int(nullable: false),
          })
          .PrimaryKey(t => t.FKGalleryId);

      CreateTable(
          "gsp.UserGalleryProfile",
          c => new
          {
            ProfileId = c.Int(nullable: false, identity: true),
            UserName = c.String(nullable: false, maxLength: 256),
            FKGalleryId = c.Int(nullable: false),
            SettingName = c.String(nullable: false, maxLength: 200),
            SettingValue = c.String(nullable: false),
          })
          .PrimaryKey(t => t.ProfileId)
          .ForeignKey("gsp.Gallery", t => t.FKGalleryId, cascadeDelete: true)
          .Index(t => t.FKGalleryId);

      CreateTable(
          "gsp.MediaQueue",
          c => new
          {
            MediaQueueId = c.Int(nullable: false, identity: true),
            FKMediaObjectId = c.Int(nullable: false),
            Status = c.String(nullable: false, maxLength: 256),
            StatusDetail = c.String(nullable: false),
            DateAdded = c.DateTime(nullable: false),
            DateConversionStarted = c.DateTime(),
            DateConversionCompleted = c.DateTime(),
          })
          .PrimaryKey(t => t.MediaQueueId)
          .ForeignKey("gsp.MediaObject", t => t.FKMediaObjectId, cascadeDelete: true)
          .Index(t => t.FKMediaObjectId);

      CreateIndex("gsp.MediaTemplate", new[] { "MimeType", "BrowserId" }, true, "UC_MediaTemplate_MimeType_BrowserId");
      CreateIndex("gsp.GallerySetting", new[] { "FKGalleryId", "SettingName" }, true, "UC_GallerySetting_FKGalleryId_SettingName");
      CreateIndex("gsp.GalleryControlSetting", new[] { "ControlId", "SettingName" }, true, "UC_GalleryControlSetting_ControlId_SettingName");
      CreateIndex("gsp.MimeTypeGallery", new[] { "FKGalleryId", "FKMimeTypeId" }, true, "UC_MimeTypeGallery_FKGalleryId_FKMimeTypeId");
      CreateIndex("gsp.UserGalleryProfile", new[] { "UserName", "FKGalleryId", "SettingName" }, true, "UC_UserGalleryProfile_UserName_FKGalleryId_SettingName");
      CreateIndex("gsp.UiTemplate", new[] { "TemplateType", "FKGalleryId", "Name" }, true, "UC_UiTemplate_TemplateType_Name");
      CreateIndex("gsp.MimeType", "FileExtension", true, "UC_MimeType_FileExtension");
    }

    /// <summary>
    /// Operations to be performed during the downgrade process.
    /// </summary>
    public override void Down()
    {
      DropIndex("gsp.MediaQueue", new[] { "FKMediaObjectId" });
      DropIndex("gsp.UserGalleryProfile", new[] { "FKGalleryId" });
      DropIndex("gsp.RoleAlbum", new[] { "FKAlbumId" });
      DropIndex("gsp.RoleAlbum", new[] { "FKRoleName" });
      DropIndex("gsp.MimeTypeGallery", new[] { "FKMimeTypeId" });
      DropIndex("gsp.MimeTypeGallery", new[] { "FKGalleryId" });
      DropIndex("gsp.GallerySetting", new[] { "FKGalleryId" });
      DropIndex("gsp.Event", new[] { "FKGalleryId" });
      DropIndex("gsp.UiTemplateAlbum", new[] { "FKAlbumId" });
      DropIndex("gsp.UiTemplateAlbum", new[] { "FKUiTemplateId" });
      DropIndex("gsp.MetadataTag", new[] { "FKTagName" });
      DropIndex("gsp.MetadataTag", new[] { "FKMetadataId" });
      DropIndex("gsp.MediaObject", new[] { "FKAlbumId" });
      DropIndex("gsp.Metadata", new[] { "FKAlbumId" });
      DropIndex("gsp.Metadata", new[] { "FKMediaObjectId" });
      DropIndex("gsp.Album", new[] { "FKAlbumParentId" });
      DropIndex("gsp.Album", new[] { "FKGalleryId" });
      DropForeignKey("gsp.MediaQueue", "FKMediaObjectId", "gsp.MediaObject");
      DropForeignKey("gsp.UserGalleryProfile", "FKGalleryId", "gsp.Gallery");
      DropForeignKey("gsp.RoleAlbum", "FKAlbumId", "gsp.Album");
      DropForeignKey("gsp.RoleAlbum", "FKRoleName", "gsp.Role");
      DropForeignKey("gsp.MimeTypeGallery", "FKMimeTypeId", "gsp.MimeType");
      DropForeignKey("gsp.MimeTypeGallery", "FKGalleryId", "gsp.Gallery");
      DropForeignKey("gsp.GallerySetting", "FKGalleryId", "gsp.Gallery");
      DropForeignKey("gsp.Event", "FKGalleryId", "gsp.Gallery");
      DropForeignKey("gsp.UiTemplateAlbum", "FKAlbumId", "gsp.Album");
      DropForeignKey("gsp.UiTemplateAlbum", "FKUiTemplateId", "gsp.UiTemplate");
      DropForeignKey("gsp.MetadataTag", "FKTagName", "gsp.Tag");
      DropForeignKey("gsp.MetadataTag", "FKMetadataId", "gsp.Metadata");
      DropForeignKey("gsp.MediaObject", "FKAlbumId", "gsp.Album");
      DropForeignKey("gsp.Metadata", "FKAlbumId", "gsp.Album");
      DropForeignKey("gsp.Metadata", "FKMediaObjectId", "gsp.MediaObject");
      DropForeignKey("gsp.Album", "FKAlbumParentId", "gsp.Album");
      DropForeignKey("gsp.Album", "FKGalleryId", "gsp.Gallery");
      DropTable("gsp.MediaQueue");
      DropTable("gsp.UserGalleryProfile");
      DropTable("gsp.Synchronize");
      DropTable("gsp.RoleAlbum");
      DropTable("gsp.Role");
      DropTable("gsp.MimeTypeGallery");
      DropTable("gsp.MimeType");
      DropTable("gsp.GallerySetting");
      DropTable("gsp.GalleryControlSetting");
      DropTable("gsp.MediaTemplate");
      DropTable("gsp.AppSetting");
      DropTable("gsp.Event");
      DropTable("gsp.UiTemplate");
      DropTable("gsp.UiTemplateAlbum");
      DropTable("gsp.Tag");
      DropTable("gsp.MetadataTag");
      DropTable("gsp.MediaObject");
      DropTable("gsp.Metadata");
      DropTable("gsp.Gallery");
      DropTable("gsp.Album");

      DropIndex("gsp.MediaTemplate", "UC_MediaTemplate_MimeType_BrowserId");
      DropIndex("gsp.GallerySetting", "UC_GallerySetting_FKGalleryId_SettingName");
      DropIndex("gsp.GalleryControlSetting", "UC_GalleryControlSetting_ControlId_SettingName");
      DropIndex("gsp.MimeTypeGallery", "UC_MimeTypeGallery_FKGalleryId_FKMimeTypeId");
      DropIndex("gsp.UserGalleryProfile", "UC_UserGalleryProfile_UserName_FKGalleryId_SettingName");
      DropIndex("gsp.UiTemplate", "UC_UiTemplate_TemplateType_Name");
      DropIndex("gsp.MimeType", "UC_MimeType_FileExtension");
    }
  }
}

