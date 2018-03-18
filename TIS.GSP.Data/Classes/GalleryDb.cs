using System.Data.Entity;
using GalleryServer.Business;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the database. Inherits <see cref="DbContext" />.
  /// </summary>
  /// <seealso cref="System.Data.Entity.DbContext" />
  public class GalleryDb : DbContext
	{
    /// <summary>
    /// Initializes a new instance of the <see cref="GalleryDb"/> class.
    /// </summary>
    public GalleryDb()
		{
			this.Configuration.LazyLoadingEnabled = false;
		}

		// Uncomment if access to the underlying ObjectContext is needed.
		//public System.Data.Objects.ObjectContext ObjectContext
		//{
		//  get { return ((System.Data.Entity.Infrastructure.IObjectContextAdapter) this).ObjectContext; }
		//}

		public DbSet<AlbumDto> Albums { get; set; }
		public DbSet<EventDto> Events { get; set; }
		public DbSet<AppSettingDto> AppSettings { get; set; }
		public DbSet<MediaTemplateDto> MediaTemplates { get; set; }
		public DbSet<GalleryControlSettingDto> GalleryControlSettings { get; set; }
		public DbSet<GalleryDto> Galleries { get; set; }
		public DbSet<GallerySettingDto> GallerySettings { get; set; }
		public DbSet<MediaObjectDto> MediaObjects { get; set; }
		public DbSet<MetadataDto> Metadatas { get; set; }
		public DbSet<MimeTypeDto> MimeTypes { get; set; }
		public DbSet<MimeTypeGalleryDto> MimeTypeGalleries { get; set; }
		public DbSet<RoleDto> Roles { get; set; }
		public DbSet<SynchronizeDto> Synchronizes { get; set; }
		public DbSet<RoleAlbumDto> RoleAlbums { get; set; }
		public DbSet<UserGalleryProfileDto> UserGalleryProfiles { get; set; }
		public DbSet<MediaQueueDto> MediaQueues { get; set; }
		public DbSet<UiTemplateDto> UiTemplates { get; set; }
		public DbSet<UiTemplateAlbumDto> UiTemplateAlbums { get; set; }
		public DbSet<TagDto> Tags { get; set; }
		public DbSet<MetadataTagDto> MetadataTags { get; set; }

		/// <summary>
		/// Gets the version of the current data schema.
		/// </summary>
		/// <value>The data schema version.</value>
		public static GalleryDataSchemaVersion DataSchemaVersion
		{
			get
			{
				return GalleryDataSchemaVersion.V4_4_2;
			}
		}

    /// <summary>
    /// This method is called when the model for a derived context has been initialized, but
    /// before the model has been locked down and used to initialize the context.  The default
    /// implementation of this method does nothing, but it can be overridden in a derived class
    /// such that the model can be further configured before it is locked down.
    /// </summary>
    /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
    /// <remarks>Typically, this method is called only once when the first instance of a derived context
    /// is created.  The model for that context is then cached and is for all further instances of
    /// the context in the app domain.  This caching can be disabled by setting the ModelCaching
    /// property on the given ModelBuidler, but note that this can seriously degrade performance.
    /// More control over caching is provided through use of the DbModelBuilder and DbContextFactory
    /// classes directly.</remarks>
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			// Set up relationship to enforce cascade delete between media objects and their metadata (by default it is set to NO ACTION)
			modelBuilder.Entity<MetadataDto>()
				.HasOptional(t => t.MediaObject)
				.WithMany(t => t.Metadata)
				.HasForeignKey(t => t.FKMediaObjectId)
				.WillCascadeOnDelete(true);

			// Can't create a cascade delete between albums and their metadata, as we get this error when we try:
			// "Introducing FOREIGN KEY constraint 'FK_dbo.gsp_Metadata_dbo.gsp_Album_FKAlbumId' on table 'gsp_Metadata' may cause cycles or multiple cascade paths."
			// We just have to make sure the app deletes 
			//modelBuilder.Entity<MetadataDto>()
			//  .HasOptional(t => t.Album)
			//  .WithMany(t => t.Metadata)
			//  .HasForeignKey(t => t.FKAlbumId)
			//  .WillCascadeOnDelete(true);
		}
	}
}
