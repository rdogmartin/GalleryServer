using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.NullObjects;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Represents an album in Gallery Server. An album is a container for zero or more gallery objects. A gallery object 
    /// may be a media object such as image, video, audio file, or document, or it may be another album.
    /// </summary>
    public class Album : GalleryObject, IAlbum
    {
        #region Private Fields

        private string _directoryName;
        private string _fullPhysicalPathOnDisk;
        private string _ownerUsername;
        private string _ownerRoleName;
        private readonly IGalleryObjectCollection _galleryObjects;
        private int _thumbnailMediaObjectId;
        private bool _areChildrenInflated;
        private bool _isThumbnailInflated;
        private bool _isVirtualAlbum;
        private bool _allowMetadataLoading;
        private MetadataItemName _sortByMetaName = MetadataItemName.NotSpecified;
        private bool? _sortAscending;
        private List<string> _inheritedOwners;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Album"/> class.
        /// </summary>
        /// <param name="albumId">The album ID.</param>
        /// <param name="galleryId">The gallery ID.</param>
        internal Album(int albumId, int galleryId)
          : this(albumId, galleryId, new NullGalleryObject(), string.Empty, int.MinValue, MetadataItemName.NotSpecified, null, int.MinValue, String.Empty, DateTime.UtcNow, String.Empty, DateTime.MinValue, String.Empty, String.Empty, false, false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Album" /> class.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="parentAlbum">The parent album that contains this album.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="thumbnailMediaObjectId">The thumbnail media object id.</param>
        /// <param name="sortByMetaName">The metadata item to sort the album by.</param>
        /// <param name="sortAscending">Indicates whether the contents of the album are sorted in ascending order. Specify null if 
        ///   the sort order is not known.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="createdByUsername">The user name of the user who created this gallery object.</param>
        /// <param name="dateAdded">The date this gallery object was created.</param>
        /// <param name="lastModifiedByUsername">The user name of the user who last modified this gallery object.</param>
        /// <param name="dateLastModified">The date and time this gallery object was last modified.</param>
        /// <param name="ownerUsername">The user name of this gallery object's owner.</param>
        /// <param name="ownerRoleName">The name of the role associated with this gallery object's owner.</param>
        /// <param name="isPrivate"><c>true</c> this gallery object is hidden from anonymous users; otherwise <c>false</c>.</param>
        /// <param name="isInflated">A bool indicating whether this object is fully inflated.</param>
        /// <param name="metadata">A collection of <see cref="Data.MetadataDto" /> instances containing metadata for the
        ///   object. Specify null if not available.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">galleryId</exception>
        internal Album(int id, int galleryId, IGalleryObject parentAlbum, string directoryName, int thumbnailMediaObjectId, MetadataItemName sortByMetaName, bool? sortAscending, int sequence, string createdByUsername, DateTime dateAdded, string lastModifiedByUsername, DateTime dateLastModified, string ownerUsername, string ownerRoleName, bool isPrivate, bool isInflated, IEnumerable<MetadataDto> metadata)
        {
            if (galleryId == int.MinValue)
            {
                throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "Gallery ID must be set to a valid value. Instead, the value was {0}.", galleryId));
            }

            this._galleryObjects = new GalleryObjectCollection();
            System.Diagnostics.Debug.Assert(this._areChildrenInflated == false, String.Format(CultureInfo.CurrentCulture, "The private boolean field _areChildrenInflated should have been initialized to false, but instead it was {0}.", this._areChildrenInflated));

            this.Id = id;

            this.GalleryId = galleryId;
            this.Parent = parentAlbum;

            this._directoryName = directoryName;
            this.Sequence = sequence;
            this.CreatedByUserName = createdByUsername;
            this.DateAdded = dateAdded;
            this.LastModifiedByUserName = lastModifiedByUsername;
            this._ownerUsername = ownerUsername;
            this._ownerRoleName = ownerRoleName;
            this.DateLastModified = dateLastModified;
            this.IsPrivate = isPrivate;
            this.AllowMetadataLoading = true;
            this._fullPhysicalPathOnDisk = string.Empty;

            this.ThumbnailMediaObjectId = (thumbnailMediaObjectId == int.MinValue ? 0 : thumbnailMediaObjectId);
            this._isThumbnailInflated = false;

            if (this._thumbnailMediaObjectId > 0)
            {
                this.Thumbnail = DisplayObject.CreateInstance(this, this._thumbnailMediaObjectId, DisplayObjectType.Thumbnail);
            }
            else
            {
                this.Thumbnail = GetDefaultAlbumThumbnail();
            }

            this.VirtualAlbumType = (id > int.MinValue ? VirtualAlbumType.NotVirtual : VirtualAlbumType.NotSpecified);

            this.SaveBehavior = Factory.GetAlbumSaveBehavior(this);
            this.DeleteBehavior = Factory.GetAlbumDeleteBehavior(this);
            this.MetadataReadWriter = Factory.GetMetadataReadWriter(this);

            IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryId);

            if (this.IsNew)
            {
                if (sortByMetaName == MetadataItemName.NotSpecified)
                {
                    sortByMetaName = gallerySetting.DefaultAlbumSortMetaName;
                }

                if (!sortAscending.HasValue)
                {
                    sortAscending = gallerySetting.DefaultAlbumSortAscending;
                }

                ExtractMetadata();
            }

            if (sortByMetaName != MetadataItemName.NotSpecified)
                this.SortByMetaName = sortByMetaName;

            if (sortAscending.HasValue)
                this.SortAscending = sortAscending.Value;

            if (metadata != null)
                AddMeta(GalleryObjectMetadataItemCollection.FromMetaDtos(this, metadata));

            this.IsInflated = isInflated;

            // Setting the previous properties has caused HasChanges = true, but we don't want this while
            // we're instantiating a new object. Reset to false.
            this.HasChanges = false;

            this.Saving += Album_Saving;
            this.Saved += Album_Saved;
            this.Deleted += Album_Deleted;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the directory where the album is stored. Example: summervacation.
        /// </summary>
        /// <value>
        /// The directory where the album is stored. Example: summervacation..
        /// </value>
        public string DirectoryName
        {
            get
            {
                VerifyObjectIsInflated(this._directoryName);
                return this._directoryName;
            }
            set
            {
                this.HasChanges = (this._directoryName == value ? this.HasChanges : true);

                this._directoryName = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name of this gallery object's owner. This property and OwnerRoleName
        /// are closely related and both should be populated or both be empty.
        /// </summary>
        /// <value>The user name of this gallery object's owner.</value>
        public string OwnerUserName
        {
            get
            {
                VerifyObjectIsInflated(this._ownerUsername);
                return this._ownerUsername;
            }
            set
            {
                this.HasChanges = (this._ownerUsername == value ? this.HasChanges : true);
                this._ownerUsername = value;

                if (String.IsNullOrEmpty(this._ownerUsername))
                    this.OwnerRoleName = String.Empty;
            }
        }

        /// <summary>
        /// Gets the owners the current album inherits from parent albums. Guaranteed to not return null.
        /// Will be empty when there aren't any inherited owners.
        /// </summary>
        /// <value>A collection of strings.</value>
        public string[] InheritedOwners
        {
            get
            {
                if (_inheritedOwners == null)
                {
                    this._inheritedOwners = new List<string>();

                    var album = this.Parent as IAlbum;
                    while (album != null)
                    {
                        if (!String.IsNullOrEmpty(album.OwnerUserName))
                        {
                            _inheritedOwners.Add(album.OwnerUserName);
                        }

                        album = album.Parent as IAlbum; // Will be null when it gets to the top album, since NullGalleryObject can't cast to IAlbum
                    }
                }

                return _inheritedOwners.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the name of the role associated with this gallery object's owner. This property and
        /// OwnerUserName are closely related and both should be populated or both be empty.
        /// </summary>
        /// <value>
        /// The name of the role associated with this gallery object's owner.
        /// </value>
        public string OwnerRoleName
        {
            get
            {
                VerifyObjectIsInflated(this._ownerRoleName);
                return this._ownerRoleName;
            }
            set
            {
                this.HasChanges = (this._ownerRoleName == value ? this.HasChanges : true);
                this._ownerRoleName = value;
            }
        }

        /// <summary>
        /// Gets or sets the media object ID whose thumbnail image should be used as the thumbnail image to represent this album.
        /// </summary>
        /// <value>The thumbnail media object id.</value>
        public int ThumbnailMediaObjectId
        {
            get
            {
                // If the int = 0, and this is not a new object, and it has not been inflated
                // from the database, go to the database and retrieve the info for this object.
                // Don't use VerifyObjectIsInflated() method because we need to compare the value
                // to 0, not int.MinValue.
                if ((this._thumbnailMediaObjectId == 0) && (!this.IsNew) && (!this.IsInflated))
                {
                    Factory.LoadAlbumInstance(this, false);
                }

                // The value could still be 0, even after inflating from the data store, because
                // 0 is a valid value that indicates no thumbanil has been assigned to this album.
                return this._thumbnailMediaObjectId;
            }
            set
            {
                if (this._thumbnailMediaObjectId != value)
                {
                    // Reset the thumbnail flag so next time the album's thumbnail properties are accessed, 
                    // VerifyThumbnailIsInflated() will know to refresh the properties.
                    this._isThumbnailInflated = false;
                }
                this.HasChanges = (this._thumbnailMediaObjectId == value ? this.HasChanges : true);

                this._thumbnailMediaObjectId = value;
            }
        }

        /// <summary>
        /// Gets or sets the metadata property to sort the album by.
        /// </summary>
        /// <value>The metadata property to sort the album by.</value>
        public MetadataItemName SortByMetaName
        {
            get
            {
                VerifyObjectIsInflated(this._sortByMetaName);
                return _sortByMetaName;
            }
            set
            {
                this.HasChanges = (this._sortByMetaName == value ? this.HasChanges : true);

                _sortByMetaName = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the contents of the album are sorted in ascending order. A <c>false</c> value indicates
        /// a descending sort.
        /// </summary>
        /// <value><c>true</c> if an album's contents are sorted in ascending order; <c>false</c> if descending order.</value>
        public bool SortAscending
        {
            get
            {
                VerifyObjectIsInflated(this._sortAscending);

                if (!_sortAscending.HasValue)
                    throw new BusinessException("The Album.SortAscending value was null. It should have been assigned a value by the VerifyObjectIsInflated() function.");

                return _sortAscending.Value;
            }
            set
            {
                this.HasChanges = (this._sortAscending == value ? this.HasChanges : true);

                _sortAscending = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this album is the top level album in the gallery.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a root album; otherwise, <c>false</c>.
        /// </value>
        public bool IsRootAlbum
        {
            get
            {
                return (this.Parent is NullObjects.NullGalleryObject);
            }
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
        public bool IsVirtualAlbum
        {
            get
            {
                return (this._isVirtualAlbum);
            }
            set
            {
                if ((this.Id > int.MinValue) && value)
                {
                    throw new BusinessException("Cannot mark an existing album as virtual.");
                }
                this._isVirtualAlbum = value;

                if (_isVirtualAlbum)
                {
                    // Clear any meta items that were created. In a future version we might enable permanent
                    // storage of meta items for virtual albums, but today we don't have that capability.
                    MetadataItems.Clear();

                    // Mark object as inflated. This can save a call to the DB later if Inflate() is called.
                    AreChildrenInflated = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the virtual album for this instance. Applies only when <see cref="IsVirtualAlbum" /> is <c>true</c>.
        /// </summary>
        /// <value>The type of the virtual album.</value>
        public VirtualAlbumType VirtualAlbumType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether metadata is to be loaded from the data store when an object is inflated. Setting
        /// this to false when metadata is not needed can improve performance, especially when large numbers of objects are being
        /// loading, such as during maintenance and synchronizations. The default value is <c>true</c>. When <c>false</c>, metadata
        /// is not extracted from the database and the <see cref="IGalleryObject.MetadataItems"/> collection is empty. As objects are lazily loaded,
        /// this value is inherited from its parent object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to allow metadata to be retrieved from the data store; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMetadataLoading
        {
            get { return this._allowMetadataLoading; }
            set { this._allowMetadataLoading = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the child albums and media objects have been added and inflated for this album. 
        /// Use <see cref="Inflate(bool)" /> to add the child objects.
        /// </summary>
        /// <value><c>true</c> if the child albums and media objects in this album are inflated; otherwise, <c>false</c>.</value>
        public bool AreChildrenInflated
        {
            get
            {
                return this._areChildrenInflated;
            }
            set
            {
                this._areChildrenInflated = value;
            }
        }

        /// <summary>
        /// Gets or sets the feed formatter options. This property is used when generating an RSS/Atom feed.
        /// </summary>
        /// <value>The feed formatter options.</value>
        public IFeedFormatterOptions FeedFormatterOptions { get; set; }

        #endregion

        #region Override Properties

        /// <summary>
        /// Gets or sets the title for this gallery object. This property is a pass-through to the 
        /// underlying <see cref="MetadataItemName.Title" /> item in the 
        /// <see cref="IGalleryObject.MetadataItems" /> collection.
        /// </summary>
        /// <value>The title for this gallery object.</value>
        public override string Title
        {
            get
            {
                IGalleryObjectMetadataItem metaItem;
                if (MetadataItems.TryGetMetadataItem(MetadataItemName.Title, out metaItem))
                    return metaItem.Value;
                else
                    //throw new BusinessException(string.Format("No meta item 'Title' exists for album {0}.", Id));
                    return String.Empty;
            }
            set
            {
                var title = value;

                IGalleryObjectMetadataItem metaItem;
                if (MetadataItems.TryGetMetadataItem(MetadataItemName.Title, out metaItem))
                {
                    metaItem.Value = title;
                    HasChanges = metaItem.HasChanges;
                }
                else
                {
                    var metaItems = Factory.CreateMetadataCollection();
                    IMetadataDefinition metadataDef = MetaDefinitions.Find(MetadataItemName.Title);
                    metaItems.Add(Factory.CreateMetadataItem(int.MinValue, this, null, title, true, metadataDef));
                    AddMeta(metaItems);
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets the gallery object type.
        /// </summary>
        /// <value>
        /// An instance of <see cref="GalleryObjectType" />.
        /// </value>
        public override GalleryObjectType GalleryObjectType
        {
            get { return GalleryObjectType.Album; }
        }

        /// <summary>
        /// Gets the physical path to this object. Does not include the trailing slash.
        /// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets\
        /// </summary>
        /// <value>The full physical path to this object.</value>
        public override string FullPhysicalPath
        {
            get
            {
                this.Inflate(false);

                if (this.IsRootAlbum)
                {
                    if (!(String.IsNullOrEmpty(this.DirectoryName)))
                        throw new BusinessException(String.Format(CultureInfo.CurrentCulture, Resources.Album_FullPhysicalPath_Ex_Msg, this.DirectoryName));

                    if (String.IsNullOrEmpty(this._fullPhysicalPathOnDisk))
                    {
                        this._fullPhysicalPathOnDisk = Factory.LoadGallerySetting(GalleryId).FullMediaObjectPath;
                    }

                    return this._fullPhysicalPathOnDisk;
                }
                else
                {
                    return String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", this.Parent.FullPhysicalPath, this.DirectoryName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the full physical path for this object as it currently exists on the hard drive. This property
        /// is updated when the object is loaded from the hard drive and when it is saved to the hard drive.
        /// Does not include the trailing slash.
        /// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets
        /// </summary>
        /// <value>The full physical path on disk.</value>
        public override string FullPhysicalPathOnDisk
        {
            get
            {
                if (this._fullPhysicalPathOnDisk.Length > 0)
                {
                    return this._fullPhysicalPathOnDisk;
                }
                else if (this.IsNew)
                {
                    // Return an empty string for new albums that haven't been persisted to the data store.
                    return string.Empty;
                }
                else if ((!this.IsNew) && (!this.IsInflated))
                {
                    // Album exists on disk but is not inflated. Load it now, which will set the private variable.
                    Factory.LoadAlbumInstance(this, false);

                    System.Diagnostics.Debug.Assert(this._fullPhysicalPathOnDisk.Length > 0);

                    return this._fullPhysicalPathOnDisk;
                }

                // If we get here IsNew must be false and IsInflated must be true. Throw assertion.
                throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Invalid object state. Album.IsNew = {0}, Album.IsInflated = {1}, and the private member variable _fullPhysicalPathOnDisk is either null or empty.", this.IsNew, this.IsInflated));
            }
            set
            {
                this._fullPhysicalPathOnDisk = value;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates an inflated album from <paramref name="albumAsset" /> having parent album <paramref name="parentAlbum" />. No child albums 
        /// or child media assets are added (<see cref="IAlbum.AreChildrenInflated" /> = <c>false</c>). If <paramref name="parentAlbum" /> is
        /// null, an album is instantiated based on the <see cref="CacheItemMedia.Id" /> property of <paramref name="albumAsset" />.
        /// Guaranteed to not return null.
        /// </summary>
        /// <param name="albumAsset">The album asset to be used as the source.</param>
        /// <param name="parentAlbum">The album containing the <paramref name="albumAsset" />. May be null. When null, an instance of <see cref="IAlbum" />
        /// is created based on the <see cref="CacheItemMedia.Id" /> property of <paramref name="albumAsset" />.</param>
        /// <returns>An instance implementing <see cref="IAlbum" />.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="albumAsset" /> is null.</exception>
        public static IAlbum CreateFrom(CacheItemAlbum albumAsset, IGalleryObject parentAlbum = null)
        {
            if (albumAsset == null)
            {
                throw new ArgumentNullException(nameof(albumAsset));
            }

            if (parentAlbum == null)
            {
                parentAlbum = (albumAsset.AlbumId > 0 ? (IGalleryObject)Factory.LoadAlbumInstance(albumAsset.AlbumId) : new NullGalleryObject());
            }

            IAlbum album = new Album(albumAsset.Id,
              albumAsset.GalleryId,
              parentAlbum,
              albumAsset.DirectoryName,
              albumAsset.ThumbnailMediaObjectId,
              albumAsset.SortByMetaName,
              albumAsset.SortAscending,
              albumAsset.Sequence,
              albumAsset.CreatedByUserName,
              albumAsset.DateAdded,
              albumAsset.LastModifiedByUserName,
              albumAsset.DateLastModified,
              albumAsset.OwnedBy,
              albumAsset.OwnerRoleName,
              albumAsset.IsPrivate,
              true,
              null);

            album.FullPhysicalPathOnDisk = album.FullPhysicalPath;
            album.AddMeta(GalleryObjectMetadataItemCollection.FromCacheItemMetas(album, albumAsset.MetaItems));

            album.HasChanges = false;

            return album;
        }

        /// <summary>
        /// Assigns properties from <paramref name="albumAsset" /> to <paramref name="albumToInflate" />, but does not add child albums or child media assets.
        /// Note that <see cref="IGalleryObject.IsInflated" /> is not set to <c>true</c> in this function. It is expected the calling function takes care of that at the
        /// appropriate moment.
        /// </summary>
        /// <param name="albumToInflate">The album to assign properties to.</param>
        /// <param name="albumAsset">The album asset to be used as the source data.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static void InflateFromCacheItem(IAlbum albumToInflate, CacheItemAlbum albumAsset)
        {
            if (albumToInflate == null)
            {
                throw new ArgumentNullException(nameof(albumToInflate));
            }

            if (albumAsset == null)
            {
                throw new ArgumentNullException(nameof(albumAsset));
            }

            // Assign parent if it hasn't already been assigned.
            if ((albumToInflate.Parent.Id == Int32.MinValue) && (albumAsset.AlbumId > 0))
            {
                albumToInflate.Parent = Factory.LoadAlbumInstance(albumAsset.AlbumId);
            }

            albumToInflate.GalleryId = albumAsset.GalleryId;
            albumToInflate.DirectoryName = albumAsset.DirectoryName;
            albumToInflate.SortByMetaName = albumAsset.SortByMetaName;
            albumToInflate.SortAscending = albumAsset.SortAscending;
            albumToInflate.Sequence = albumAsset.Sequence;
            albumToInflate.CreatedByUserName = albumAsset.CreatedByUserName;
            albumToInflate.DateAdded = albumAsset.DateAdded;
            albumToInflate.LastModifiedByUserName = albumAsset.LastModifiedByUserName;
            albumToInflate.DateLastModified = albumAsset.DateLastModified;
            albumToInflate.OwnerUserName = albumAsset.OwnedBy;
            albumToInflate.OwnerRoleName = albumAsset.OwnerRoleName;
            albumToInflate.IsPrivate = albumAsset.IsPrivate;

            // Set the album's thumbnail media object ID. Setting this property sets an internal flag that will cause
            // the media object info to be retrieved when the Thumbnail property is accessed. That's why we don't
            // need to set any of the thumbnail properties.
            // WARNING: No matter what, do not call DisplayObject.CreateInstance() because that creates a new object, 
            // and we might be  executing this method from within our Thumbnail display object. Trust me, this 
            // creates hard to find bugs!
            albumToInflate.ThumbnailMediaObjectId = albumAsset.ThumbnailMediaObjectId;

            albumToInflate.AddMeta(GalleryObjectMetadataItemCollection.FromCacheItemMetas(albumToInflate, albumAsset.MetaItems));
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Verify the properties have been set for the thumbnail image in this album, retrieving the information
        /// from the data store if necessary. This method also inflates the album if it is not already inflated 
        /// (but doesn't inflate the children objects).
        /// </summary>
        /// <param name="thumbnail">A reference to the thumbnail display object for this album. The instance
        /// is passed as a parameter rather than directly addressed as a property of our base class because we don't 
        /// want to trigger the property get {} code, which calls this method (and would thus result in an infinite
        /// loop).</param>
        /// <remarks>To be perfectly clear, let me say again that the thumbnail parameter is the same instance
        /// as album.Thumbnail. They both refer to the same memory space. This method updates the albumThumbnail 
        /// parameter, which means that album.Thumbnail is updated as well.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="thumbnail" /> is null.</exception>
        protected override void VerifyThumbnailIsInflated(IDisplayObject thumbnail)
        {
            if (thumbnail == null)
                throw new ArgumentNullException("thumbnail");

            // Verify album is inflated (the method only inflates the album if it's not already inflated).
            Inflate(false);

            System.Diagnostics.Debug.Assert(this._thumbnailMediaObjectId >= 0, String.Format(CultureInfo.CurrentCulture, "Album.Inflate(false) should have set ThumbnailMediaObjectId >= 0. Instead, it is {0}.", this._thumbnailMediaObjectId));

            if (!this._isThumbnailInflated)
            {
                // Need to inflate thumbnail.
                if (this._thumbnailMediaObjectId > 0)
                {
                    // ID has been specified. Find media object and retrieve it's thumbnail properties.
                    var originalHasChanges = HasChanges;

                    #region Get reference to the media object used for the album's thumbnail

                    // If thumbnail media object is one of the album's children, use that. Otherwise, load from data store.
                    IGalleryObject thumbnailMediaObject = null;
                    if (this.AreChildrenInflated)
                    {
                        foreach (IGalleryObject mediaObject in this.GetChildGalleryObjects(GalleryObjectType.MediaObject))
                        {
                            if (this._thumbnailMediaObjectId == mediaObject.Id)
                            {
                                thumbnailMediaObject = mediaObject;
                                break;
                            }
                        }
                    }

                    if (thumbnailMediaObject == null)
                    {
                        // this._thumbnailMediaObjectId does not refer to a media object that is a direct child of this 
                        // album, so just go to the data store and retrieve it.
                        try
                        {
                            thumbnailMediaObject = Factory.LoadMediaObjectInstance(this._thumbnailMediaObjectId);
                        }
                        catch (InvalidMediaObjectException)
                        {
                            // Get default thumbnail. Copy properties instead of reassigning the albumThumbnail parameter
                            // so we don't lose the reference.
                            IDisplayObject defaultAlbumThumb = GetDefaultAlbumThumbnail();
                            thumbnail.MediaObjectId = defaultAlbumThumb.MediaObjectId;
                            thumbnail.DisplayType = defaultAlbumThumb.DisplayType;
                            thumbnail.FileName = defaultAlbumThumb.FileName;
                            thumbnail.Width = defaultAlbumThumb.Width;
                            thumbnail.Height = defaultAlbumThumb.Height;
                            thumbnail.FileSizeKB = defaultAlbumThumb.FileSizeKB;
                            thumbnail.FileNamePhysicalPath = defaultAlbumThumb.FileNamePhysicalPath;
                        }
                    }

                    #endregion

                    if (thumbnailMediaObject != null)
                    {
                        thumbnail.MediaObjectId = this._thumbnailMediaObjectId;
                        thumbnail.DisplayType = DisplayObjectType.Thumbnail;
                        thumbnail.FileName = thumbnailMediaObject.Thumbnail.FileName;
                        thumbnail.Width = thumbnailMediaObject.Thumbnail.Width;
                        thumbnail.Height = thumbnailMediaObject.Thumbnail.Height;
                        thumbnail.FileSizeKB = thumbnailMediaObject.Thumbnail.FileSizeKB;
                        thumbnail.FileNamePhysicalPath = thumbnailMediaObject.Thumbnail.FileNamePhysicalPath;
                    }

                    if (!originalHasChanges && HasChanges)
                    {
                        // We don't want inflating the thumbnail to make it look like the album has changes that need persisting, so revert.
                        HasChanges = false;
                    }
                }
                else
                {
                    // ID = 0. Set to default values. This is a repeat of what happens in the Album() constructor,
                    // but we need it again just in case the user changes it to 0 and immediately retrieves its properties.
                    // Copy properties instead of reassigning the albumThumbnail parameter so we don't lose the reference.
                    IDisplayObject defaultAlbumThumb = GetDefaultAlbumThumbnail();
                    thumbnail.MediaObjectId = defaultAlbumThumb.MediaObjectId;
                    thumbnail.DisplayType = defaultAlbumThumb.DisplayType;
                    thumbnail.FileName = defaultAlbumThumb.FileName;
                    thumbnail.Width = defaultAlbumThumb.Width;
                    thumbnail.Height = defaultAlbumThumb.Height;
                    thumbnail.FileSizeKB = defaultAlbumThumb.FileSizeKB;
                    thumbnail.FileNamePhysicalPath = defaultAlbumThumb.FileNamePhysicalPath;
                }

                this._isThumbnailInflated = true;
            }

        }

        /// <summary>
        /// Overrides the method from <see cref="GalleryObject" />. This implementation  is empty, because albums don't have thumbnail
        /// images, at least not in the strictest sense.
        /// </summary>
        protected override void CheckForThumbnailImage()
        {
            // Do nothing: Strictly speaking, albums don't have thumbnail images. Only the media object that is assigned
            // as the thumbnail for an album has a thumbnail image. The code that verifies the media object has a thumbnail
            // image during a save is sufficient.
        }

        /// <summary>
        /// Gets a value indicating whether the administrator has indicated the specified <paramref name="metaDef" />
        /// applies to the current gallery object.
        /// </summary>
        /// <param name="metaDef">The metadata definition.</param>
        /// <returns><c>true</c> when the specified metadata item should be displayed; otherwise <c>false</c>.</returns>
        public override bool MetadataDefinitionApplies(IMetadataDefinition metaDef)
        {
            if (metaDef.MetadataItem == MetadataItemName.Title || metaDef.MetadataItem == MetadataItemName.Caption)
                return true; // We *ALWAYS* want to create a Title and Caption item.
            else
                return metaDef.IsVisibleForAlbum;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified gallery object as a child of this gallery object.
        /// </summary>
        /// <param name="galleryObject">The <see cref="IGalleryObject" /> to add as a child of this
        /// gallery object.</param>
        /// <exception cref="System.NotSupportedException">Thrown when an inherited type
        /// does not allow the addition of child gallery objects.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
        public override void AddGalleryObject(IGalleryObject galleryObject)
        {
            if (galleryObject == null)
                throw new ArgumentNullException("galleryObject");

            // Do not add object if it already exists in our collection. An object is uniquely identified by its ID and type.
            // For example, this album may contain a gallery object of type Image with ID=25 and also a child album of type Album
            // with ID = 25.
            if (galleryObject.Id > int.MinValue)
            {
                //System.Diagnostics.Debug.Assert(this._galleryObjects.Count == 0, String.Format(CultureInfo.CurrentCulture, "this._galleryObjects.Count = {0}", this._galleryObjects.Count));
                lock (this._galleryObjects)
                {
                    foreach (IGalleryObject go in this._galleryObjects)
                    {
                        if ((go.Id == galleryObject.Id) && (go.GetType() == galleryObject.GetType()))
                            return;
                    }
                }
            }

            // If the current album is virtual, meaning that it is a temporary container for one or more objects and not the actual
            // parent album, then we want to add the object as a child of this album but we don't want to set the Parent property
            // of the child object, since that will cause the filepaths to recalculate and become inaccurate.
            if (this.IsVirtualAlbum)
            {
                DoAddGalleryObject(galleryObject);
            }
            else
            {
                galleryObject.Parent = this;
            }
        }

        /// <summary>
        /// Adds the specified gallery object as a child of this gallery object. This method is called by the <see cref="AddGalleryObject"/> 
        /// method and should not be called directly.
        /// </summary>
        /// <param name="galleryObject">The gallery object to add as a child of this gallery object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
        public override void DoAddGalleryObject(IGalleryObject galleryObject)
        {
            if (galleryObject == null)
                throw new ArgumentNullException("galleryObject");

            // Contains() compares based on ID, which doesn't work when adding multiple new objects all having
            // ID = int.MinVAlue.
            lock (this._galleryObjects)
            {
                if ((galleryObject.IsNew) || ((!galleryObject.IsNew) && !(this._galleryObjects.Contains(galleryObject))))
                {
                    this._galleryObjects.Add(galleryObject);
                }
            }
        }

        /// <summary>
        /// Removes the specified gallery object from the collection of child objects
        /// of this gallery object.
        /// </summary>
        /// <param name="galleryObject">The <see cref="IGalleryObject" /> to remove as a child of this
        /// gallery object.</param>
        /// <exception cref="System.NotSupportedException">Thrown when an inherited type
        /// does not allow the addition of child gallery objects.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the specified
        /// gallery object is not child of this gallery object.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
        public override void RemoveGalleryObject(IGalleryObject galleryObject)
        {
            if (galleryObject == null)
                throw new ArgumentNullException("galleryObject");

            if (!this._galleryObjects.Contains(galleryObject))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.Album_Remove_Ex_Msg, this.Id, galleryObject.Id, galleryObject.Parent.Id));

            galleryObject.SetParentToNullObject();

            lock (this._galleryObjects)
            {
                this._galleryObjects.Remove(galleryObject);
            }
        }

        /// <summary>
        /// Permanently delete the original file for this gallery object. Requires that an optimized version exists.
        /// If no optimized version exists, no action is taken.
        /// </summary>
        public override void DeleteOriginalFile()
        {
            // Do nothing, since albums do not have original files.
        }

        /// <summary>
        /// Inflate the current object by loading all properties from the data store. If the object is already inflated (<see cref="GalleryObject.IsInflated"/>=true), no action is taken.
        /// </summary>
        public override void Inflate()
        {
            Inflate(false);
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
        public void Sort(bool persistToDataStore, string userName, bool isRecursive = false, bool replaceChildSortFields = false)
        {
            if (persistToDataStore && String.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("The parameter userName must be specified when persistToDataStore is true.");

            // Step 1: Sort the gallery objects and update the Sequence property
            var seq = 1;
            foreach (var galleryObject in GetChildGalleryObjects().ToSortedList(SortByMetaName, SortAscending, GalleryId))
            {
                var go = _galleryObjects.FirstOrDefault(g => g.Id == galleryObject.Id && g.GalleryObjectType == galleryObject.GalleryObjectType);
                if (go != null)
                {
                    go.Sequence = seq++;
                }
            }

            // Step 2: If specified, save to database and act recursively.
            if (persistToDataStore)
            {
                foreach (var go in GetChildGalleryObjects())
                {
                    if (go.HasChanges)
                    {
                        go.LastModifiedByUserName = userName;
                        go.DateLastModified = DateTime.UtcNow;

                        go.Save();
                    }

                    if (isRecursive && go.GalleryObjectType == GalleryObjectType.Album)
                    {
                        var album = (IAlbum)go;
                        if (replaceChildSortFields)
                        {
                            album.SortByMetaName = SortByMetaName;
                            album.SortAscending = SortAscending;

                            if (album.HasChanges)
                            {
                                album.Save();
                            }
                        }

                        album.Sort(persistToDataStore, userName, isRecursive, replaceChildSortFields);
                    }
                }
            }
        }

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
        public void SortAsync(bool persistToDataStore, string userName, bool isRecursive, bool replaceChildSortFields)
        {
            Task.Factory.StartNew(() => SortAsyncBegin(persistToDataStore, userName, isRecursive, replaceChildSortFields));
        }

        /// <summary>
        /// Inflate the current object by loading all properties from the data store. If the object is already inflated (<see cref="GalleryObject.IsInflated"/>=true), no action is taken.
        /// </summary>
        /// <param name="inflateChildMediaObjects">When true, the child media objects are added and inflated. Note that child albums are added
        /// but not inflated.</param>
        public void Inflate(bool inflateChildMediaObjects)
        {
            // If this is not a new object, and it has not been inflated from the database,
            // OR we want to force the inflation of the child media objects (which might be happening even though
            // the album properties are already inflated), go to the data store and retrieve the info for this object.

            bool existingAlbumThatIsNotInflated = ((!this.IsNew) && (!this.IsInflated));
            bool needToLoadChildAlbumsAndObjects = (inflateChildMediaObjects && !this.AreChildrenInflated);

            if (existingAlbumThatIsNotInflated || needToLoadChildAlbumsAndObjects)
            {
                Factory.LoadAlbumInstance(this, inflateChildMediaObjects);

                System.Diagnostics.Debug.Assert(!existingAlbumThatIsNotInflated || (existingAlbumThatIsNotInflated && ((this.IsInflated) || (!this.HasChanges))),
                                                String.Format(CultureInfo.CurrentCulture, @"Album.Inflate() was invoked on an existing, uninflated album (IsNew = false, IsInflated = false), 
            which should have triggered the Factory.LoadAlbumInstance() method to set IsInflated=true and HasChanges=false. Instead, this album currently 
            has these values: IsInflated={0}; HasChanges={1}.", this.IsInflated, this.HasChanges));

                System.Diagnostics.Debug.Assert(inflateChildMediaObjects == this.AreChildrenInflated, String.Format(CultureInfo.CurrentCulture,
                                                                                                                    "The inflateChildren parameter must match the AreChildrenInflated property. inflateChildren={0}; AreChildrenInflated={1}",
                                                                                                                    inflateChildMediaObjects, this.AreChildrenInflated));

                System.Diagnostics.Debug.Assert(this.ThumbnailMediaObjectId > int.MinValue,
                                                "The album's ThumbnailMediaObjectId should have been assigned in this method.");
            }

        }

        /// <summary>
        /// Gets the total file size, in KB, of all the original files in the album, including all 
        /// child albums. The total includes only those items where a web-optimized version also exists.
        /// </summary>
        /// <returns>Returns the total file size, in KB, of all the original files in the album.</returns>
        public long GetFileSizeKbAllOriginalFilesInAlbum()
        {
            // Get the total file size, in KB, of all the high resolution images in the specified album
            long sumTotal = 0;
            foreach (IGalleryObject go in GetChildGalleryObjects(GalleryObjectType.MediaObject))
            {
                if (DoesOriginalExist(go.Optimized.FileName, go.Original.FileName))
                    sumTotal += go.Original.FileSizeKB;
            }

            foreach (IAlbum childAlbum in GetChildGalleryObjects(GalleryObjectType.Album))
            {
                sumTotal += childAlbum.GetFileSizeKbAllOriginalFilesInAlbum();
            }

            return sumTotal;
        }

        /// <summary>
        /// Returns an unsorted collection of gallery objects that are direct children of the current gallery object or
        /// an empty list (Count = 0) if there are no child objects. Use the <paramref name="excludePrivateObjects" />
        /// parameter to optionally filter out private objects (if not specified, private objects are returned).
        /// </summary>
        /// <param name="galleryObjectType">A <see cref="GalleryObjectType" /> enum indicating the
        /// desired type of child objects to return.</param>
        /// <param name="excludePrivateObjects">Indicates whether to exclude objects that are marked as private
        /// (<see cref="IGalleryObject.IsPrivate" /> = <c>true</c>). Objects that are private should not be shown to anonymous users.</param>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        public override IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType = GalleryObjectType.All, bool excludePrivateObjects = false)
        {
            this.Inflate(true);

            switch (galleryObjectType)
            {
                case GalleryObjectType.All:
                    return new GalleryObjectCollection(_galleryObjects.Where(g => !g.IsPrivate || !excludePrivateObjects));
                case GalleryObjectType.MediaObject:
                    return new GalleryObjectCollection(_galleryObjects.Where(g => (!g.IsPrivate || !excludePrivateObjects) && g.GalleryObjectType != GalleryObjectType.Album));
                case GalleryObjectType.Album:
                    return new GalleryObjectCollection(_galleryObjects.Where(g => (!g.IsPrivate || !excludePrivateObjects) && g.GalleryObjectType == GalleryObjectType.Album));
                default:
                    return new GalleryObjectCollection(_galleryObjects.Where(g => (!g.IsPrivate || !excludePrivateObjects) && g.GalleryObjectType == galleryObjectType));
            }
        }

        /// <summary>
        /// Move the current object to the specified destination album. This method moves the physical files associated with this
        /// object to the destination album's physical directory. This instance's <see cref="GalleryObject.Save"/> method is invoked to persist the changes to the
        /// data store. When moving albums, all the album's children, grandchildren, etc are also moved.
        /// </summary>
        /// <param name="destinationAlbum">The album to which the current object should be moved.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
        public override void MoveTo(IAlbum destinationAlbum)
        {
            if (destinationAlbum == null)
                throw new ArgumentNullException("destinationAlbum");

            // Step 1: Get list of albums whose thumbnails we'll update after the move operation.
            IIntegerCollection albumsNeedingNewThumbnails = GetAlbumHierarchy(destinationAlbum.Id);

            var oldParentId = this.Parent.Id;

            // Step 2: Assign the new parent album and gallery ID to this album and save.
            this.Parent = destinationAlbum;

            if (destinationAlbum.IsPrivate)
            {
                this.IsPrivate = true; // If the destination album is private, then the one we are moving into it must be private as well.
            }

            this.GalleryId = destinationAlbum.GalleryId;
            this.Sequence = int.MinValue; // Reset the sequence so that it will be assigned a new value placing it at the end.
            Save();

            CacheController.RemoveAlbumIdFromParentAlbumCacheItem(Id, oldParentId);
            CacheController.AddAlbumIdToAlbumCacheItem(Id, Parent.Id);

            // Step 3: Remove any explicitly defined roles that the album may now be inheriting in its new location.
            UpdateRoleSecurityForMovedAlbum(this);

            // Step 4: Now assign new thumbnails (if needed) to the albums we moved FROM. (The thumbnail for the destination album was updated in 
            // the Save() method.)
            foreach (int albumId in albumsNeedingNewThumbnails)
            {
                Album.AssignAlbumThumbnailIfMissing(Factory.LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = true }), false, false, this.LastModifiedByUserName);
            }
        }

        /// <summary>
        /// Copy the current object and place it in the specified destination album. This method creates a completely separate copy
        /// of the original, including copying the physical files associated with this object. The copy is persisted to the data
        /// store and then returned to the caller. When copying albums, all the album's children, grandchildren, etc are copied,
        /// and any role permissions that are explicitly assigned to the source album are copied to the destination album, unless
        /// the copied album inherits the role throught the destination parent album. Inherited role permissions are not copied.
        /// </summary>
        /// <param name="destinationAlbum">The album to which the current object should be copied.</param>
        /// <param name="userName">The user name of the currently logged on user. This will be used for the audit fields of the
        /// copied objects.</param>
        /// <returns>
        /// Returns a new gallery object that is an exact copy of the original, except that it resides in the specified
        /// destination album, and of course has a new ID. Child objects are recursively copied.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
        public override IGalleryObject CopyTo(IAlbum destinationAlbum, string userName)
        {
            if (destinationAlbum == null)
                throw new ArgumentNullException("destinationAlbum");

            // Step 1: Copy the album.

            IAlbum albumCopy = Factory.CreateEmptyAlbumInstance(destinationAlbum.GalleryId, true);

            //albumCopy.Title = this.Title;
            //albumCopy.Summary = this.Summary;
            //albumCopy.DateStart = this.DateStart;
            //albumCopy.DateEnd = this.DateEnd;
            //albumCopy.OwnerUserName = this.OwnerUserName; // Do not copy this one
            //albumCopy.OwnerRoleName = this.OwnerRoleName; // Do not copy this one

            albumCopy.MetadataItems.Clear();
            albumCopy.MetadataItems.AddRange(MetadataItems.Copy());

            // Associate the new meta items with the copied object.
            foreach (var metadataItem in albumCopy.MetadataItems)
            {
                metadataItem.GalleryObject = albumCopy;
            }

            IGalleryObjectMetadataItem metaItem;
            if (albumCopy.MetadataItems.TryGetMetadataItem(MetadataItemName.DateAdded, out metaItem))
            {
                var gallerySetting = Factory.LoadGallerySetting(destinationAlbum.GalleryId);

                metaItem.Value = DateTime.Now.ToString(gallerySetting.MetadataDateTimeFormatString, CultureInfo.InvariantCulture);
                metaItem.RawValue = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            }

            albumCopy.Parent = destinationAlbum;

            if (destinationAlbum.IsPrivate)
            {
                albumCopy.IsPrivate = true; // If the destination album is private, then the one we are copying into it must be private as well.
            }

            HelperFunctions.UpdateAuditFields(albumCopy, userName);
            albumCopy.Save();

            // Step 2: Copy any roles that are explicitly assigned to the original album.
            UpdateRoleSecurityForCopiedAlbum(albumCopy, this);

            // Step 3: Copy all child gallery objects of this album (including child albums).
            foreach (IGalleryObject galleryObject in this.GetChildGalleryObjects())
            {
                IGalleryObject copiedObject = galleryObject.CopyTo(albumCopy, userName);

                //If we just copied the media object that is the thumbnail for this album, then set the newly assigned ID of the
                //copied media object to the new album's ThumbnailMediaObjectId property.
                if ((this.ThumbnailMediaObjectId == galleryObject.Id) && (!(galleryObject is Album)))
                {
                    albumCopy.ThumbnailMediaObjectId = copiedObject.Id;
                    albumCopy.Save();
                }
            }

            return albumCopy;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Assign the specified <paramref name="mediaAssetId" /> as the thumbnail image for the <paramref name="album" />.
        /// </summary>
        /// <param name="album">The album whose thumbnail image is to be assigned. It can be read only.</param>
        /// <param name="mediaAssetId">The media asset ID. May be zero to indicate no thumbnail image is assigned.</param>
        /// <param name="userName">The user name for the logged on user. This is used for the audit fields.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        public static void AssignAlbumThumbnail(IAlbum album, int mediaAssetId, string userName)
        {
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (!album.IsRootAlbum)
            {
                // Update only the Album.ThumbnailMediaObjectID column and clear the relevant cache item(s)
                using (var repo = new AlbumRepository())
                {
                    var albumDto = repo.Find(album.Id);

                    albumDto.ThumbnailMediaObjectId = mediaAssetId;
                    albumDto.LastModifiedBy = userName;
                    albumDto.DateLastModified = DateTime.UtcNow;

                    repo.Save();
                }
                CacheController.RemoveAlbumFromCache(album.Id);
                CacheController.RemoveInflatedAlbumsFromCache();
            }
        }

        /// <summary>
        /// Assign a thumbnail image to the album if it is missing one. Use the thumbnail image of the first media object in the album or,
        /// if no objects exist in the album, the first image in any child albums, searching recursively. If no images
        /// can be found, set <see cref="ThumbnailMediaObjectId" /> = 0.
        /// </summary>
        /// <param name="album">The album whose thumbnail image is to be assigned.</param>
        /// <param name="recursivelyAssignParentAlbums">Specifies whether to recursively iterate through the
        /// parent, grandparent, and so on until the root album, assigning a thumbnail, if necessary, to each
        /// album along the way.</param>
        /// <param name="recursivelyAssignChildrenAlbums">Specifies whether to recursively iterate through
        /// all children albums of this album, assigning a thumbnail to each child album, if necessary, along
        /// the way.</param>
        /// <param name="userName">The user name for the logged on user. This is used for the audit fields.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        public static void AssignAlbumThumbnailIfMissing(IAlbum album, bool recursivelyAssignParentAlbums, bool recursivelyAssignChildrenAlbums, string userName)
        {
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            var albumToUpdate = (album.IsWritable ? album : Factory.LoadAlbumInstance(new AlbumLoadOptions(album.Id) { IsWritable = true }));

            if ((!albumToUpdate.IsRootAlbum) && !System.IO.File.Exists(album.Thumbnail.FileNamePhysicalPath))
            {
                // Update only the Album.ThumbnailMediaObjectID column and clear the relevant cache item(s)
                var mediaId = GetIdOfFirstMediaObject(album);
                if (album.ThumbnailMediaObjectId != mediaId)
                {
                    // Step 1: Update the in-memory version. This is necessary when syncing because subsequently discovered media files
                    // in this album (found during the current sync) will return here when they are created (saved event) and those will
                    // use the same album reference we have here, despite us clearing the cache below.
                    albumToUpdate.ThumbnailMediaObjectId = mediaId;

                    using (var repo = new AlbumRepository())
                    {
                        // Step 2: Update the DB
                        var albumDto = repo.Find(album.Id);

                        if (albumDto.ThumbnailMediaObjectId != mediaId)
                        {
                            albumDto.ThumbnailMediaObjectId = mediaId;
                            albumDto.LastModifiedBy = userName;
                            albumDto.DateLastModified = DateTime.UtcNow;

                            repo.Save();

                            CacheController.RemoveAlbumFromCache(album.Id);
                            CacheController.RemoveInflatedAlbumsFromCache();
                        }
                    }
                }

                // Version 4.4.0 and earlier used the following code, but this clears the gallery cache, requiring an expensive 
                // recomputation of all albums IDs and more, resulting in long delays in large galleries.
                //albumToUpdate.ThumbnailMediaObjectId = GetIdOfFirstMediaObject(albumToUpdate);
                //HelperFunctions.UpdateAuditFields(albumToUpdate, userName);
                //albumToUpdate.Save();
            }

            if (recursivelyAssignChildrenAlbums)
            {
                foreach (IAlbum childAlbum in albumToUpdate.GetChildGalleryObjects(GalleryObjectType.Album))
                {
                    AssignAlbumThumbnailIfMissing(childAlbum, false, recursivelyAssignChildrenAlbums, userName);
                }
            }

            if (recursivelyAssignParentAlbums)
            {
                while (!(albumToUpdate.Parent is NullObjects.NullGalleryObject))
                {
                    AssignAlbumThumbnailIfMissing((IAlbum)albumToUpdate.Parent, recursivelyAssignParentAlbums, false, userName);
                    albumToUpdate = (IAlbum)albumToUpdate.Parent;
                }
            }
        }

        public static int GetIdOfFirstMediaObject(IAlbum album)
        {
            int firstMediaObjectId = 0;

            foreach (IGalleryObject mediaObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject).ToSortedList())
            {
                if (!mediaObject.IsNew) // We might encounter new, unsaved objects while synchronizing. Need to skip these since their ID=int.MinValue
                {
                    firstMediaObjectId = mediaObject.Id;
                    break;
                }
            }

            if (firstMediaObjectId == 0)
            {
                foreach (IGalleryObject childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album).ToSortedList())
                {
                    firstMediaObjectId = GetIdOfFirstMediaObject((IAlbum)childAlbum);
                    if (firstMediaObjectId > 0)
                        break;
                }
            }

            return firstMediaObjectId;
        }

        #endregion

        #region Private Functions

        private void VerifyObjectIsInflated(string propertyValue)
        {
            // If the string is empty, and this is not a new object, and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if (String.IsNullOrEmpty(propertyValue) && (!this.IsNew) && (!this.IsInflated))
            {
                Inflate();
            }
        }

        private void VerifyObjectIsInflated(MetadataItemName propertyValue)
        {
            // If no meta name has been specified, and this is not a new object, and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if ((propertyValue == MetadataItemName.NotSpecified) && (!this.IsNew) && (!this.IsInflated))
            {
                Inflate();
            }
        }

        private void VerifyObjectIsInflated(bool? propertyValue)
        {
            // If no value has been specified, and this is not a new object, and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if ((!propertyValue.HasValue) && (!this.IsNew) && (!this.IsInflated))
            {
                Inflate();
            }
        }

        private void VerifyObjectIsInflated(int propertyValue)
        {
            // If the int = int.MinValue, and this is not a new object, and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if ((propertyValue == int.MinValue) && (!this.IsNew) && (!this.IsInflated))
            {
                Inflate();
            }
        }

        private void VerifyObjectIsInflated(DateTime propertyValue)
        {
            // If the property value is not the default DateTime value, and this is not a new object,
            // and it has not been inflated from the database, go to the database and retrieve 
            // the info for this object.
            if ((propertyValue == DateTime.MinValue) && (!this.IsNew) && (!this.IsInflated))
            {
                Inflate();
            }
        }

        /// <summary>
        /// Verify the directory name for this album is valid by checking that it satisfies the max length criteria,
        /// OS requirements for valid directory names, and that the name is unique in the specified parent directory.
        /// If the DirectoryName property is empty, it is assigned the title value, shortening it if necessary. If the
        /// DirectoryName property is specified, its length is checked to ensure it does not exceed the configuration
        /// setting AlbumDirectoryNameLength. If it does, a BusinessException is thrown. 
        /// This function automatically removes invalid characters and generates a unique name if needed.
        /// </summary>
        /// <exception cref="BusinessException">Thrown when the DirectoryName
        /// property has a value and its length exceeds the value set in the AlbumDirectoryNameLength configuration setting.</exception>
        private void ValidateDirectoryName()
        {
            if ((this.IsRootAlbum) || (this.IsVirtualAlbum))
                return;

            if (String.IsNullOrEmpty(this.DirectoryName))
            {
                this.DirectoryName = this.Title;
                string dirPath = this.Parent.FullPhysicalPath;
                string dirName = this.DirectoryName;

                string newDirName = HelperFunctions.ValidateDirectoryName(dirPath, dirName, Factory.LoadGallerySetting(GalleryId).DefaultAlbumDirectoryNameLength);

                if (!this.DirectoryName.Equals(newDirName))
                {
                    this.DirectoryName = newDirName;
                }
            }

            if (this.DirectoryName.Length > GlobalConstants.AlbumDirectoryNameLength)
                throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Invalid directory name. The maximum length for a directory name is {0} characters, but one was specified that is {1} characters. More info: album ID = {2}; album title = '{3}'", GlobalConstants.AlbumDirectoryNameLength, this.DirectoryName.Length, this.Id, this.Title));
        }

        private IDisplayObject GetDefaultAlbumThumbnail()
        {
            string defaultFilename = String.Empty;

            IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryId);

            int maxLength = gallerySetting.MaxThumbnailLength;
            float ratio = gallerySetting.EmptyAlbumThumbnailWidthToHeightRatio;

            int width, height;
            if (ratio > 1)
            {
                width = maxLength;
                height = Convert.ToInt32((float)maxLength / ratio);
            }
            else
            {
                height = maxLength;
                width = Convert.ToInt32((float)maxLength * ratio);
            }

            var nullGalleryObject = new NullGalleryObject();
            var albumThumbnail = DisplayObject.CreateInstance(nullGalleryObject, defaultFilename, width, height, DisplayObjectType.Thumbnail, new NullDisplayObjectCreator());

            albumThumbnail.MediaObjectId = this._thumbnailMediaObjectId;
            albumThumbnail.FileNamePhysicalPath = defaultFilename;

            return albumThumbnail;
        }

        /// <summary>
        /// Validate album-specific fields before saving to data store.
        /// </summary>
        private static void ValidateAuditFields()
        {
        }

        /// <summary>
        /// Any roles explicitly assigned to the moved album automatically "follow" it to the new location.
        /// But if the moved album has an explicitly assigned role permission and also inherits that role in the 
        /// new location, then the explicit role assignment is removed. We do this to enforce the rule that 
        /// child albums are never explicitly assigned a role permission if an ancestor already has that permission.
        /// </summary>
        /// <param name="movedAlbum">The album that has just been moved to a new destination album.</param>
        private static void UpdateRoleSecurityForMovedAlbum(IAlbum movedAlbum)
        {
            foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
            {
                // This role applies to this object.
                if (role.RootAlbumIds.Contains(movedAlbum.Id))
                {
                    // The album is directly specified in this role, but if any of this album's new parents are explicitly
                    // specified, then it is not necessary to specify it at this level. Iterate through all the album's new 
                    // parent albums to see if this is the case.
                    if (role.AllAlbumIds.Contains(movedAlbum.Parent.Id))
                    {
                        role.RootAlbumIds.Remove(movedAlbum.Id);
                        role.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Make sure the newly copied album has the same role permissions that are explicitly assigned to the 
        /// source album. Do not copy role permissions that are inherited in the source album.
        /// </summary>
        /// <param name="copiedAlbum">The album that was just copied.</param>
        /// <param name="sourceAlbum">The album the copy was made from.</param>
        private static void UpdateRoleSecurityForCopiedAlbum(IAlbum copiedAlbum, IAlbum sourceAlbum)
        {
            foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
            {
                if (role.RootAlbumIds.Contains(sourceAlbum.Id))
                {
                    // The original album is explicitly assigned this role, so assign it also to the copied album, unless
                    // the copied album is already inheriting the role from an ancestor album.
                    if (!role.AllAlbumIds.Contains(copiedAlbum.Parent.Id))
                    {
                        role.RootAlbumIds.Add(copiedAlbum.Id);
                        role.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the asynchronous request to sort the current album, optionally persisting the changes to the data store and acting
        /// recursively. This function is a wrapper around the call to <see cref="Sort(bool, string, bool, bool)" />, adding logging,
        /// error handling, and cache purging.
        /// </summary>
        /// <param name="persistToDataStore">if set to <c>true</c> persist the album and the new sequence of each child
        ///   gallery object to the database.</param>
        /// <param name="userName">Name of the user. This is for auditing and used only when <paramref name="persistToDataStore" />
        ///   is <c>true</c>; otherwise you may specify null.</param>
        /// <param name="isRecursive">If set to <c>true</c> act recursively on child albums. Defaults to <c>false</c>
        /// when not specified. This value is ignored when <paramref name="persistToDataStore" /> is <c>false</c>.</param>
        /// <param name="replaceChildSortFields">When <c>true</c>, replace the sort field and direction on child albums with 
        /// the values from the current album. This value is applied only when <paramref name="persistToDataStore" /> and
        /// <paramref name="isRecursive" /> are both <c>true</c>.</param>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="persistToDataStore" /> is <c>true</c>
        /// and <paramref name="userName" /> is null or empty.</exception>
        private void SortAsyncBegin(bool persistToDataStore, string userName, bool isRecursive, bool replaceChildSortFields)
        {
            try
            {
                EventController.RecordEvent(String.Format(CultureInfo.CurrentCulture, "INFO: Beginning sort of album {0} ({1}) by property '{2}' (ascending={3}; recursive={4}, replaceChildSortFields={5}).", Id, Title, SortByMetaName, SortAscending, isRecursive, replaceChildSortFields), EventType.Info, GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);

                Sort(persistToDataStore, userName, isRecursive, replaceChildSortFields);

                EventController.RecordEvent(String.Format(CultureInfo.CurrentCulture, "INFO: Successfully finished sorting album {0}.", Id), EventType.Info, GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
            }
            catch (Exception ex)
            {
                EventController.RecordError(ex, AppSetting.Instance, GalleryId, Factory.LoadGallerySettings());
                EventController.RecordEvent(String.Format(CultureInfo.CurrentCulture, "CANCELED: The sorting of album '{0}' has been canceled due to the previously logged error.", Id), EventType.Info, GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
                throw;
            }

            CacheController.RemoveInflatedAlbumsFromCache();
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="originalFileName" /> is different from <paramref name="optimizedFileName" />, thus indicating
        /// that both an optimized and original version exists for a media asset.
        /// </summary>
        /// <param name="optimizedFileName">Name of the optimized file.</param>
        /// <param name="originalFileName">Name of the original file.</param>
        /// <returns><c>true</c> if the parameters are different, <c>false</c> otherwise.</returns>
        private static bool DoesOriginalExist(string optimizedFileName, string originalFileName)
        {
            // An original file exists if an optimized file exists and the optimized and original filenames are different.
            return (!string.IsNullOrWhiteSpace(optimizedFileName) && !string.Equals(optimizedFileName, originalFileName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Event Handlers

        void Album_Saving(object sender, EventArgs e)
        {
            // Raised after validation but before persisting to data store. This is our chance to do validation
            // for album-specific properties.
            if (this.IsNew)
            {
                ValidateAuditFields();

                ValidateDirectoryName();

                if ((String.IsNullOrEmpty(this.Title)) && (!String.IsNullOrEmpty(this.DirectoryName)))
                {
                    // No title is specified but we have a directory name. Use that for the title.
                    this.Title = this.DirectoryName;
                }
            }
        }

        void Album_Saved(object sender, EventArgs e)
        {
            // Raised after the album is persisted to the data store.
            this._fullPhysicalPathOnDisk = this.FullPhysicalPath;

            // Since galleries and roles store a list of albums, we must clear them out anytime an album is added, deleted, or moved.
            // If this proves too expensive, it could be refactored to update only the changed albums, although that may be difficult.
            Factory.ClearGalleryCache();

            CacheController.RemoveCache(CacheItem.GalleryServerRoles);
        }

        void Album_Deleted(object sender, GalleryObjectEventArgs e)
        {
            CacheController.RemoveAlbumIdFromParentAlbumCacheItem(Id, e.ParentId);

            foreach (var albumId in e.FlattenedChildAlbumIds)
            {
                CacheController.RemoveAlbumFromCache(albumId);
            }

            foreach (var mediaId in e.FlattenedChildMediaIds)
            {
                CacheController.RemoveMediaAssetFromCache(mediaId);
            }

            // Since galleries and roles store a list of albums, we must clear them out anytime an album is added, deleted, or moved.
            Factory.ClearGalleryCache();

            CacheController.RemoveCache(CacheItem.GalleryServerRoles);
            CacheController.RemoveTagsFromCache();
        }

        #endregion
    }
}
