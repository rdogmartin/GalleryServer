using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a gallery within Gallery Server.
	/// </summary>
	public interface IGallery
	{
		/// <summary>
		/// Gets or sets the unique identifier for this gallery.
		/// </summary>
		/// <value>The unique identifier for this gallery.</value>
		int GalleryId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		bool IsNew { get; }

		/// <summary>
		/// Gets or sets the description for this gallery.
		/// </summary>
		/// <value>The description for this gallery.</value>
		string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the date this gallery was created.
		/// </summary>
		/// <value>The date this gallery was created.</value>
		DateTime CreationDate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the root album of this gallery.
		/// </summary>
		/// <value>The ID of the root album of this gallery</value>
		int RootAlbumId
		{
			get;
		}

    /// <summary>
    /// Gets or sets a thread-safe dictionary containing a list of album IDs (key) and the flattened list of
    /// all child album IDs within each album. The list includes the album identified in the key.
    /// </summary>
    /// <value>An instance of ConcurrentDictionary&lt;int, List&lt;int&gt;&gt;.</value>
    ConcurrentDictionary<int, List<int>> FlattenedAlbums
		{
			get;
			set;
		}

    /// <summary>
    /// Gets or sets a thread-safe dictionary containing a list of album IDs (key) and the hierarchical path of the root
    /// album to the album specified in the key, but does not include album identified in the key.
    /// </summary>
    /// <value>An instance of Dictionary&lt;int, List&lt;int&gt;&gt;.</value>
    ConcurrentDictionary<int, List<int>> AlbumHierarchies
		{
			get;
			set;
		}

		/// <summary>
		/// Creates a deep copy of this instance.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		IGallery Copy();

		/// <summary>
		/// Persist this gallery object to the data store.
		/// </summary>
		void Save();

		/// <summary>
		/// Permanently delete the current gallery from the data store, including all related records. This action cannot
		/// be undone.
		/// </summary>
		void Delete();

		/// <summary>
		/// Configure the gallery by verifying that a default set of
		/// records exist in the relevant tables (gsp_Gallery, gsp_Album, gsp_GallerySetting, gsp_MimeTypeGallery, gsp_Role_Album). 
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for 
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method 
		/// will ensure that the new records are associated with this gallery.
		/// </summary>
		void LoadData();

	    /// <summary>
	    /// Inspect the database for missing records; inserting if necessary.
	    /// </summary>
	    void Validate();
	}
}
