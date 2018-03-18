namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A data object that contains permissions relevant to the current user. The instance can be serialized to JSON and
	/// subsequently used in the browser as a data object.
	/// </summary>
	public class Permissions
	{
		/// <summary>
		/// Represents the ability to view an album or media object. Does not include the ability to view high resolution
		/// versions of images. Includes the ability to download the media object and view a slide show.
		/// </summary>
		public bool ViewAlbumOrMediaObject { get; set; }

		/// <summary>
		/// Represents the ability to view the original media object, if it exists.
		/// </summary>
		public bool ViewOriginalMediaObject { get; set; }

		/// <summary>
		/// Represents the ability to create a new album within the current album. This includes the ability to move or
		/// copy an album into the current album.
		/// </summary>
		public bool AddChildAlbum { get; set; }

		/// <summary>
		/// Represents the ability to add a new media object to the current album. This includes the ability to move or
		/// copy a media object into the current album.
		/// </summary>
		public bool AddMediaObject { get; set; }

		/// <summary>
		/// Represents the ability to edit an album's title, summary, and begin and end dates. Also includes rearranging the
		/// order of objects within the album and assigning the album's thumbnail image. Does not include the ability to
		/// add or delete child albums or media objects.
		/// </summary>
		public bool EditAlbum { get; set; }

		/// <summary>
		/// Represents the ability to edit a media object's caption, rotate it, and delete the high resolution version of
		/// an image.
		/// </summary>
		public bool EditMediaObject { get; set; }

		/// <summary>
		/// Represents the ability to delete the current album. This permission is required to move 
		/// albums to another album, since it is effectively deleting it from the current album's parent.
		/// </summary>
		public bool DeleteAlbum { get; set; }

		/// <summary>
		/// Represents the ability to delete child albums within the current album.
		/// </summary>
		public bool DeleteChildAlbum { get; set; }

		/// <summary>
		/// Represents the ability to delete media objects within the current album. This permission is required to move 
		/// media objects to another album, since it is effectively deleting it from the current album.
		/// </summary>
		public bool DeleteMediaObject { get; set; }

		/// <summary>
		/// Represents the ability to synchronize media objects on the hard drive with records in the data store.
		/// </summary>
		public bool Synchronize { get; set; }

		/// <summary>
		/// Represents the ability to administer a particular gallery. Automatically includes all other permissions except
		/// AdministerSite.
		/// </summary>
		public bool AdministerGallery { get; set; }

		/// <summary>
		/// Represents the ability to administer all aspects of Gallery Server. Automatically includes all other permissions.
		/// </summary>
		public bool AdministerSite { get; set; }

		/// <summary>
		/// Represents the ability to not render a watermark over media objects.
		/// </summary>
		public bool HideWatermark { get; set; }
	}
}