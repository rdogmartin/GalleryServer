namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A client-optimized object that contains gallery data. This class is designed to be sent to
	/// the client (e.g. as JSON) and used by javascript, including as the data source for a client
	/// templating engine.
	/// </summary>
	public class GalleryData
	{
		/// <summary>
		/// Gets the application-level properties for the gallery.
		/// </summary>
		/// <value>
		/// An instance of <see cref="App" />.
		/// </value>
		public App App { get; set; }

		/// <summary>
		/// Gets the properties that affect the user experience.
		/// </summary>
		/// <value>
		/// An instance of <see cref="Settings" />.
		/// </value>
		public Settings Settings { get; set; }

		/// <summary>
		/// Gets information about the current user.
		/// </summary>
		/// <value>
		/// An instance of <see cref="User" />.
		/// </value>
		public User User { get; set; }

		/// <summary>
		/// Gets information about an album. Child properties <see cref="Entity.Album.GalleryItems" />
		/// and <see cref="Entity.Album.MediaItems" /> may be null in certain situations to keep the 
		/// object size as small as possible.
		/// </summary>
		/// <value>
		/// An instance of <see cref="Album" />.
		/// </value>
		public Album Album { get; set; }

		/// <summary>
		/// Gets information about a media object.
		/// </summary>
		/// <value>
		/// An instance of <see cref="MediaItem" />.
		/// </value>
		public MediaItem MediaItem { get; set; }

		/// <summary>
		/// Gets or sets the currently active metadata. For a single media object or album, it is the 
		/// metadata associated with it. When multiple items are selected on the thumbnail view, it
		/// is a combination of merged data (for tagged items such as keywords) and the metadata for
		/// the last item in the array.
		/// </summary>
		/// <value>An array of <see cref="MetaItem" /> instances.</value>
		public MetaItem[] ActiveMetaItems { get; set; }

		/// <summary>
		/// Gets or sets the currently selected or displayed gallery item(s).
		/// </summary>
		/// <value>An array of <see cref="GalleryItem" /> instances.</value>
		public GalleryItem[] ActiveGalleryItems { get; set; }

		/// <summary>
		/// Gets language resources.
		/// </summary>
		/// <value>
		/// An instance of <see cref="Resource" />.
		/// </value>
		public Resource Resource { get; set; }
	}

	/// <summary>
	/// Allows specifying options for populating an instance of <see cref="GalleryData" />.
	/// </summary>
	public class GalleryDataLoadOptions
	{
    /// <summary>
    /// Specifies that  <see cref="Album.GalleryItems" /> should be populated with the items that match the filter specified in <see cref="Filter" />,
    /// the number specified in <see cref="NumGalleryItemsToRetrieve" />, while skipping the number specified in <see cref="NumGalleryItemsToSkip" />.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool LoadGalleryItems { get; set; }

    /// <summary>
    /// Specifies that <see cref="Album.MediaItems" /> should be populated with the media objects. Defaults to <c>false</c>.
    /// belonging to the album.
    /// </summary>
    public bool LoadMediaItems { get; set; }

    /// <summary>
    /// Specifies the number of gallery items to retrieve. A value of zero or less indicates all items are to be retrieved.
    /// Defaults to zero (retrieve all gallery items). This property applies only when <see cref="LoadGalleryItems" /> is <c>true</c>.
    /// </summary>
    public int NumGalleryItemsToRetrieve { get; set; }

    /// <summary>
    /// Specifies the number of gallery items to skip. Use this property along with <see cref="NumGalleryItemsToRetrieve" />
    /// to support paged results. Defaults to zero. This property applies only when <see cref="LoadGalleryItems" /> is <c>true</c>.
    /// </summary>
    public int NumGalleryItemsToSkip { get; set; }

    /// <summary>
    /// A filter specifying the type of gallery items to load. Defaults to <see cref="Business.GalleryObjectType.All" /> when not specified.
    /// This property applies only when <see cref="LoadGalleryItems" /> is <c>true</c>.
    /// </summary>
    public Business.GalleryObjectType Filter { get; set; } = Business.GalleryObjectType.All;
	}
}

