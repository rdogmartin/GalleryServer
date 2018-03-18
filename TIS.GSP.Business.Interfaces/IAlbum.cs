using System;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents an album in Gallery Server. An album may contain any object that implements <see cref="IGalleryObject" />.
	/// </summary>
	public interface IAlbum : IGalleryObject
	{
		/// <summary>
		/// Gets or sets the name of the directory where the album is stored. Example: summervacation.
		/// </summary>
		/// <value>The directory where the album is stored. Example: summervacation..</value>
		string DirectoryName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the user name of this gallery object's owner. This property and OwnerRoleName
		/// are closely related and both should be populated or both be empty.
		/// </summary>
		/// <value>The user name of this gallery object's owner.</value>
		string OwnerUserName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the owners the current album inherits from parent albums. Guaranteed to not return null.
		/// Will be empty when there aren't any inherited owners.
		/// </summary>
		/// <value>A collection of strings.</value>
		string[] InheritedOwners
		{
			get;
		}

		/// <summary>
		/// Gets or sets the name of the role associated with this gallery object's owner. This property and 
		/// OwnerUserName are closely related and both should be populated or both be empty.
		/// </summary>
		/// <value>The name of the role associated with this gallery object's owner.</value>
		string OwnerRoleName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this album is the top level album in the gallery.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is a root album; otherwise, <c>false</c>.
		/// </value>
		bool IsRootAlbum
		{
			get;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this album is a virtual album used only as a container for objects that are
		/// spread across multiple albums. A virtual album does not map to a physical folder and cannot be saved to the
		/// data store. Virtual albums are used as containers for search results and to contain the top level albums
		/// that a user has authorization to view.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is a virtual album; otherwise, <c>false</c>.
		/// </value>
		bool IsVirtualAlbum
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the type of the virtual album for this instance. Applies only when <see cref="IsVirtualAlbum" /> is <c>true</c>.
		/// </summary>
		/// <value>The type of the virtual album.</value>
		VirtualAlbumType VirtualAlbumType { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether metadata is to be loaded from the data store when an object is inflated. Setting
		/// this to false when metadata is not needed can improve performance, especially when large numbers of objects are being
		/// loading, such as during maintenance and synchronizations. The default value is <c>true</c>. When <c>false</c>, metadata
		/// is not extracted from the database and the <see cref="IGalleryObject.MetadataItems" /> collection is empty. As objects are lazily loaded,
		/// this value is inherited from its parent object.
		/// </summary>
		/// <value>
		/// 	<c>true</c> to allow metadata to be retrieved from the data store; otherwise, <c>false</c>.
		/// </value>
		bool AllowMetadataLoading
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the media object ID whose thumbnail image should be used as the thumbnail image to represent this album.
		/// </summary>
		/// <value>The thumbnail media object id.</value>
		int ThumbnailMediaObjectId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the metadata property to sort the album by.
		/// </summary>
		/// <value>The metadata property to sort the album by.</value>
		MetadataItemName SortByMetaName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the contents of the album are sorted in ascending order. A <c>false</c> value indicates
		/// a descending sort.
		/// </summary>
		/// <value><c>true</c> if an album's contents are sorted in ascending order; <c>false</c> if descending order.</value>
		bool SortAscending
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the child albums and media objects have been added and inflated for this album. 
		/// Use <see cref="Inflate(bool)" /> to add the child objects.
		/// </summary>
		/// <value><c>true</c> if the child albums and media objects in this album are inflated; otherwise, <c>false</c>.</value>
		bool AreChildrenInflated
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the feed formatter options. This property is used when generating an RSS/Atom feed.
		/// </summary>
		/// <value>The feed formatter options.</value>
		IFeedFormatterOptions FeedFormatterOptions
		{
			get;
			set;
		}

		/// <summary>
		/// Sorts the gallery objects in this album by the <see cref="SortByMetaName" /> field in the order specified by
		/// <see cref="SortAscending" />, optionally persisting the changes to the database, activing recursively and - when
		/// acting recursively, optionally replacing the sort field and direction on child albums with the values from the 
		/// current album. This method updates the <see cref="IGalleryObject.Sequence" /> property of each gallery object.
		/// </summary>
		/// <param name="persistToDataStore">if set to <c>true</c> persist the album and the new sequence of each child
		/// gallery object to the database.</param>
		/// <param name="userName">Name of the user. This is for auditing and used only when <paramref name="persistToDataStore" />
		/// is <c>true</c>; otherwise you may specify null.</param>
		/// <param name="isRecursive">If set to <c>true</c> act recursively on child albums. Defaults to <c>false</c>
		/// when not specified. This value is ignored when <paramref name="persistToDataStore" /> is <c>false</c>.</param>
		/// <param name="replaceChildSortFields">When <c>true</c>, replace the sort field and direction on child albums with 
		/// the values from the current album. This value is applied only when <paramref name="persistToDataStore" /> and
		/// <paramref name="isRecursive" /> are both <c>true</c>.</param>
		/// <exception cref="System.ArgumentException">Thrown when <paramref name="persistToDataStore" /> is <c>true</c>
		/// and <paramref name="userName" /> is null or empty.</exception>
		void Sort(bool persistToDataStore, string userName, bool isRecursive = false, bool replaceChildSortFields = false);

		/// <summary>
		/// Sorts the gallery objects in this album by the <see cref="SortByMetaName" /> field in the order specified by
		/// <see cref="SortAscending" />, optionally persisting the changes to the database, activing recursively and - when
		/// acting recursively, optionally replacing the sort field and direction on child albums with the values from the 
		/// current album. This method updates the <see cref="IGalleryObject.Sequence" /> property of each gallery object. 
		/// It runs asynchronously and returns immediately.
		/// </summary>
		/// <param name="persistToDataStore">if set to <c>true</c> persist the album and the new sequence of each child
		/// gallery object to the database.</param>
		/// <param name="userName">Name of the user. This is for auditing and used only when <paramref name="persistToDataStore" />
		/// is <c>true</c>; otherwise you may specify null.</param>
		/// <param name="isRecursive">If set to <c>true</c> act recursively on child albums. Defaults to <c>false</c>
		/// when not specified. This value is ignored when <paramref name="persistToDataStore" /> is <c>false</c>.</param>
		/// <param name="replaceChildSortFields">When <c>true</c>, replace the sort field and direction on child albums with 
		/// the values from the current album. This value is applied only when <paramref name="persistToDataStore" /> and
		/// <paramref name="isRecursive" /> are both <c>true</c>.</param>
		/// <exception cref="System.ArgumentException">Thrown when <paramref name="persistToDataStore" /> is <c>true</c>
		/// and <paramref name="userName" /> is null or empty.</exception>
		void SortAsync(bool persistToDataStore, string userName, bool isRecursive = false, bool replaceChildSortFields = false);

		/// <summary>
		/// Inflate the current object by loading all properties from the data store. If the object is already inflated (GalleryObject.IsInflated=true), no action is taken.
		/// </summary>
		/// <param name="inflateChildMediaObjects">When true, the child media objects are added and inflated. Note that child albums are added
		/// but not inflated.</param>
		void Inflate(bool inflateChildMediaObjects);

	  /// <summary>
	  /// Gets the total file size, in KB, of all the original files in the album, including all 
	  /// child albums. The total includes only those items where a web-optimized version also exists.
	  /// </summary>
	  /// <returns>Returns the total file size, in KB, of all the original files in the album.</returns>
	  long GetFileSizeKbAllOriginalFilesInAlbum();
	}
}
