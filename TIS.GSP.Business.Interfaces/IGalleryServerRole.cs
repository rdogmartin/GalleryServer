using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a role that encapsulates a set of permissions for one or more albums in Gallery Server. Each user
	/// is assigned to zero or more roles.
	/// </summary>
	public interface IGalleryServerRole
	{
		/// <summary>
		/// Gets or sets a string that uniquely identifies the role.
		/// </summary>
		/// <value>The name of the role.</value>
		string RoleName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to create child albums.
		/// </summary>
		/// <value><c>true</c> if the user assigned to this role has permission to create child albums; otherwise, <c>false</c>.</value>
		bool AllowAddChildAlbum { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to add media objects to an album.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user assigned to this role has permission to add media objects to an album; otherwise, <c>false</c>.
		/// </value>
		bool AllowAddMediaObject { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user has administrative permission for all albums. This permission
		/// automatically applies to all albums; it cannot be selectively applied.
		/// </summary>
		/// <value><c>true</c> if the user has administrative permission for all albums; otherwise, <c>false</c>.</value>
		bool AllowAdministerSite { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user has administrative permission for all albums in the gallery
		/// associated with this role. This permission automatically applies to all albums in the gallery; it cannot be 
		/// selectively applied.
		/// </summary>
		/// <value><c>true</c> if the user has administrative permission for all albums in the gallery associated with 
		/// this role; otherwise, <c>false</c>.</value>
		bool AllowAdministerGallery { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to delete child albums.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user assigned to this role has permission to delete child albums; otherwise, <c>false</c>.
		/// </value>
		bool AllowDeleteChildAlbum { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to delete media objects within an album.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user assigned to this role has permission to delete media objects within an album; otherwise, <c>false</c>.
		/// </value>
		bool AllowDeleteMediaObject { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to edit an album.
		/// </summary>
		/// <value><c>true</c> if the user assigned to this role has permission to edit an album; otherwise, <c>false</c>.</value>
		bool AllowEditAlbum { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to edit a media object.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user assigned to this role has permission to edit a media object; otherwise, <c>false</c>.
		/// </value>
		bool AllowEditMediaObject { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has a watermark applied to images.
		/// This setting has no effect if watermarks are not used. A true value means the user does not see the watermark;
		/// a false value means the watermark is applied.
		/// </summary>
		/// <value><c>true</c> if the user assigned to this role has a watermark applied to images; otherwise, <c>false</c>.</value>
		bool HideWatermark { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to synchronize an album.
		/// </summary>
		/// <value><c>true</c> if the user assigned to this role has permission to synchronize an album; otherwise, <c>false</c>.</value>
		bool AllowSynchronize { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to view albums and media objects.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user assigned to this role has permission to view albums and media objects; otherwise, <c>false</c>.
		/// </value>
		bool AllowViewAlbumOrMediaObject { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user assigned to this role has permission to view the original,
		/// high resolution version of an image. This setting applies only to images. It has no effect if there are no
		/// high resolution images in the album or albums to which this role applies.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user assigned to this role has permission to view the original,
		/// high resolution version of an image; otherwise, <c>false</c>.
		/// </value>
		bool AllowViewOriginalImage { get; set; }

		/// <summary>
		/// Populate the <see cref="AllAlbumIds" /> and <see cref="Galleries" /> properties based on the contents of 
		/// <see cref="RootAlbumIds" /> and the flattened list of album IDs in <paramref name="galleries" />.
		/// </summary>
		/// <param name="galleries">A list of all galleries in the current application. The <see cref="IGallery.FlattenedAlbums" />
		/// property is used as a source for populating the <see cref="AllAlbumIds" /> and <see cref="Galleries" /> properties
		/// of the current instance.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleries" /> is null.</exception>
		void Inflate(IGalleryCollection galleries);

		/// <summary>
		/// Add the specified album to the list of all album IDs. This is used by data and business layer code to
		/// populate the list when it is instantiated or saved.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the album to add to the list.</param>
		void AddToAllAlbumIds(int albumId);

		/// <summary>
		/// Add the specified albums to the list of all album IDs. This is used by data and business layer code to
		/// populate the list when it is instantiated or saved.
		/// </summary>
		/// <param name="albumIds">The IDs that uniquely identify the albums to add to the list.</param>
		void AddToAllAlbumIds(IEnumerable<int> albumIds);

		/// <summary>
		/// Gets the list of all galleries to which this role applies. This property is dynamically populated based on the 
		/// albums in the <see cref="RootAlbumIds" /> property. Calling the Save() method automatically reloads this 
		/// property from the data store.
		/// </summary>
		/// <value>The list of all galleries to which this role applies.</value>
		IGalleryCollection Galleries { get; }

    /// <summary>
    /// Gets the list of all album IDs for which this role applies. Includes all descendents of all applicable albums.
    /// Calling the <see cref="Save" /> method automatically reloads this property from the data store.
    /// </summary>
    /// <value>The list of all album IDs for which this role applies.</value>
    IIntegerCollection AllAlbumIds { get; }

    /// <summary>
    /// Gets the list of all top-level album IDs for which this role applies. Does not include any descendants of the album.
    /// Note that adding or removing items to this list does not cause <see cref="AllAlbumIds" /> to be cleared out.
    /// </summary>
    /// <value>The list of all top-level album IDs for which this role applies.</value>
		IIntegerCollection RootAlbumIds { get; }

		/// <summary>
		/// Clears the list of album IDs stored in the <see cref="AllAlbumIds"/> property.
		/// </summary>
		void ClearAllAlbumIds();
		
		/// <summary>
		/// Persist this gallery server role to the data store. The list of top-level albums this role applies to, which is stored
		/// in the <see cref="RootAlbumIds" /> property, is also saved. The data provider automatically repopulates the 
		/// <see cref="AllAlbumIds" /> and <see cref="Galleries" /> properties.
		/// </summary>
		void Save();

		/// <summary>
		/// Permanently delete this gallery server role from the data store, including the list of role/album relationships
		/// associated with this role.
		/// </summary>
    /// <remarks>This procedure only deletes it from the custom gallery server tables,
    /// not the ASP.NET role membership table(s). The web application code that invokes this procedure also
    /// uses the standard ASP.NET technique to delete the role from the membership table(s).</remarks>
    void Delete();

		/// <summary>
		/// Creates a deep copy of this instance, including the Galleries, RootAlbumIds and AllAlbumIds properties.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		IGalleryServerRole Copy();

        /// <summary>
        /// Verify the role conforms to business rules. Specifically, if the role has administrative permissions
        /// (AllowAdministerSite = true or AllowAdministerGallery = true):
        /// 1. Make sure the role permissions - except HideWatermark - are set to true.
        /// 2. Make sure the root album IDs are a list containing the root album ID for each affected gallery.
        /// If anything needs updating, update the object and persist the changes to the data store. This helps keep the data store 
        /// valid in cases where the user is directly editing the tables (for example, adding/deleting records from the gs_Role_Album table).
        /// </summary>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        bool ValidateIntegrity();
	}
}
