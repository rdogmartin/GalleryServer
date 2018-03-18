using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.Properties;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Represents a gallery object, which is an item that is managed by Gallery Server. Examples include
    /// albums, images, videos, audio files, and documents.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("({GalleryObjectType}) ID = {Id}; Title = {Title}")]
    public abstract class GalleryObject : IGalleryObject, IComparable
    {
        #region Private Fields

        private static Regex _metaRegEx;

        private bool _isNew;
        private bool _isInflated;
        private int _id;
        private int _galleryId;
        private bool _galleryIdHasChanged;
        private int _sequence;
        private DateTime _dateAdded;
        private bool _hasChanges;
        private bool _regenerateThumbnailOnSave;
        private bool _regenerateOptimizedOnSave;
        private IDisplayObject _thumbnail;
        private IDisplayObject _optimized;
        private IDisplayObject _original;
        private IGalleryObject _parent;
        private ISaveBehavior _saveBehavior;
        private IDeleteBehavior _deleteBehavior;
        private IMetadataDefinitionCollection _metaDefinitions;
        private IGalleryObjectMetadataItemCollection _metadataItems;
        private MediaAssetRotateFlip _rotateFlip;
        private string _createdByUsername;
        private string _lastModifiedByUsername;
        private DateTime _dateLastModified;
        private bool _isPrivate;
        private bool _isSynchronized;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GalleryObject"/> class.
        /// </summary>
        protected GalleryObject()
        {
            this._parent = new NullObjects.NullGalleryObject();
            this._thumbnail = new NullObjects.NullDisplayObject();
            this._optimized = new NullObjects.NullDisplayObject();
            this._original = new NullObjects.NullDisplayObject();

            // Default IsSynchronized to false. It is set to true during a synchronization.
            this.IsSynchronized = false;
            this.IsWritable = false;

            this.BeforeAddMetaItem += OnBeforeAddMetaItem;
            this.Saved += GalleryObject_Saved;
            this.Created += GalleryObject_Created;
            this.Deleted += GalleryObject_Deleted;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the unique identifier for this gallery object.
        /// </summary>
        /// <value>The unique identifier for this gallery object.</value>
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._isNew = (value == int.MinValue ? true : false);
                this._hasChanges = (this._id == value ? this._hasChanges : true);
                this._id = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that uniquely identifies the current gallery.
        /// </summary>
        /// <value>The value that uniquely identifies the current gallery.</value>
        public int GalleryId
        {
            get
            {
                return this._galleryId;
            }
            set
            {
                // Check if item is being assigned to another gallery, and set flag if it is. This will
                // be used to ensure data integrity. For example, when the flag is true, and an album is
                // being saved, all child albums will also be updated to the new gallery.
                if (this._galleryId > 0 && this._galleryId != value)
                    _galleryIdHasChanged = true;

                this._galleryId = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether a different gallery has been assigned
        /// to this object since it was retrieved from the data store. It is <c>false</c> at all
        /// other times, including once the new gallery assignment is persisted.
        /// </summary>
        /// <value>
        /// The value that indicates whether a different gallery has been assigned
        /// to this object since it was retrieved from the data store.
        /// </value>
        public bool GalleryIdHasChanged
        {
            get
            {
                return this._galleryIdHasChanged;
            }
        }

        /// <summary>
        /// Gets or sets the object that contains this gallery object.
        /// </summary>
        /// <value>The object that contains this gallery object.</value>
        /// <exception cref="ArgumentNullException">Thrown when setting this property to a null value.</exception>
        public IGalleryObject Parent
        {
            get
            {
                return this._parent;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", Resources.GalleryObject_Parent_Ex_Msg);

                var changingParent = (!(this._parent is NullObjects.NullGalleryObject) && this._parent.Id != value.Id);
                this._hasChanges = (this._parent == value ? this._hasChanges : true);

                if (changingParent)
                {
                    this._parent.RemoveGalleryObject(this);
                }

                value.DoAddGalleryObject(this);
                this._parent = value;

                // If we changed parents, recalculate the full path to this album
                if (changingParent)
                {
                    RecalculateFilePaths();
                }
            }
        }

        /// <summary>
        /// Gets or sets the title for this gallery object. This property is a pass-through to the 
        /// underlying <see cref="MetadataItemName.Title" /> item in the 
        /// <see cref="IGalleryObject.MetadataItems" /> collection.
        /// </summary>
        /// <value>The title for this gallery object.</value>
        public virtual string Title
        {
            get
            {
                IGalleryObjectMetadataItem metaItem;

                if (MetadataItems.TryGetMetadataItem(MetadataItemName.Title, out metaItem))
                    return metaItem.Value;
                else
                {
                    //throw new BusinessException(string.Format("No meta item 'MediaObjectTitle' exists for gallery object {0} ({1}).", Id, GalleryObjectType));
                    return String.Empty;
                }
            }
            set
            {
                var title = value;

                IGalleryObjectMetadataItem metaItem;
                if (MetadataItems.TryGetMetadataItem(MetadataItemName.Title, out metaItem))
                {
                    metaItem.Value = title;
                    this._hasChanges = metaItem.HasChanges;
                }
                else
                {
                    var metaItems = Factory.CreateMetadataCollection();
                    IMetadataDefinition metadataDef = MetaDefinitions.Find(MetadataItemName.Title);
                    metaItems.Add(Factory.CreateMetadataItem(int.MinValue, this, null, title, true, metadataDef));
                    AddMeta(metaItems);
                    this._hasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a long description for this gallery object. This property is a pass-through to the 
        /// underlying <see cref="MetadataItemName.Caption" /> item in the 
        /// <see cref="IGalleryObject.MetadataItems" /> collection.
        /// </summary>
        /// <value>The long description for this gallery object.</value>
        public string Caption
        {
            get
            {
                IGalleryObjectMetadataItem metaItem;
                if (MetadataItems.TryGetMetadataItem(MetadataItemName.Caption, out metaItem))
                    return metaItem.Value;
                else
                    //throw new BusinessException(string.Format("No meta item 'Caption' exists for gallery object {0} ({1}).", Id, GalleryObjectType));
                    return String.Empty;
            }
            set
            {
                IGalleryObjectMetadataItem metaItem;
                if (MetadataItems.TryGetMetadataItem(MetadataItemName.Caption, out metaItem))
                {
                    metaItem.Value = value;
                    HasChanges = metaItem.HasChanges;
                }
                else
                {
                    var metaItems = Factory.CreateMetadataCollection();
                    IMetadataDefinition metadataDef = MetaDefinitions.Find(MetadataItemName.Caption);
                    metaItems.Add(Factory.CreateMetadataItem(int.MinValue, this, null, value, true, metadataDef));
                    AddMeta(metaItems);
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this object has changes that have not been persisted to the database.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
        /// </value>
        public bool HasChanges
        {
            get
            {
                return this._hasChanges;
            }
            set
            {
                this._hasChanges = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
        /// </summary>
        /// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
        public bool IsNew
        {
            get
            {
                return this._isNew;
            }
            protected set
            {
                this._isNew = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this object has been fully populated with data from the data store.
        /// Once assigned a true value, it remains true for the lifetime of the object. Returns false for newly created 
        /// objects that have not been saved to the data store. Set to <c>true</c> after an object is saved if it hadn't 
        /// already been set to <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is inflated; otherwise, <c>false</c>.
        /// </value>
        public bool IsInflated
        {
            get { return this._isInflated; }
            set
            {
                if (this._isInflated)
                {
                    throw new System.InvalidOperationException(Resources.GalleryObject_IsInflated_Ex_Msg);
                }

                _isInflated = value;
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail information for this gallery object.
        /// </summary>
        /// <value>The thumbnail information for this gallery object.</value>
        public IDisplayObject Thumbnail
        {
            get
            {
                VerifyThumbnailIsInflated(this._thumbnail);

                return this._thumbnail;
            }
            set
            {
                if (value == null)
                    throw new BusinessException("Attempted to set GalleryObject.Thumbnail to null for MOID " + this.Id);

                this._hasChanges = (this._thumbnail == value ? this._hasChanges : true);
                this._thumbnail = value;
            }
        }

        /// <summary>
        /// Gets or sets the optimized information for this gallery object.
        /// </summary>
        /// <value>The optimized information for this gallery object.</value>
        public IDisplayObject Optimized
        {
            get
            {
                return this._optimized;
            }
            set
            {
                if (value == null)
                    throw new BusinessException("Attempted to set GalleryObject.Optimized to null for MOID " + this.Id);

                this._hasChanges = (this._optimized == value ? this._hasChanges : true);
                this._optimized = value;
            }
        }

        /// <summary>
        /// Gets or sets the information representing the original media object. (For example, the uncompressed photo, or the video / audio file.)
        /// </summary>
        /// <value>The information representing the original media object.</value>
        public IDisplayObject Original
        {
            get
            {
                return this._original;
            }
            set
            {
                if (value == null)
                    throw new BusinessException("Attempted to set GalleryObject.Original to null for MOID " + this.Id);

                this._hasChanges = (this._original == value ? this._hasChanges : true);
                this._original = value;
            }
        }

        /// <summary>
        /// Gets the physical path to this object. Does not include the trailing slash.
        /// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets
        /// </summary>
        /// <value>The full physical path to this object.</value>
        public virtual string FullPhysicalPath
        {
            get
            {
                return this._parent.FullPhysicalPath;
            }
        }

        /// <summary>
        /// Gets or sets the full physical path for this object as it currently exists on the hard drive. This property
        /// is updated when the object is loaded from the hard drive and when it is saved to the hard drive.
        /// Does not include the trailing slash.
        /// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets
        /// </summary>
        /// <value>The full physical path on disk.</value>
        public virtual string FullPhysicalPathOnDisk
        {
            get
            {
                return this._parent.FullPhysicalPathOnDisk;
            }
            set
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the gallery object type.
        /// </summary>
        /// <value>
        /// An instance of <see cref="GalleryObjectType" />.
        /// </value>
        public abstract GalleryObjectType GalleryObjectType { get; }

        /// <summary>
        /// Gets the MIME type for this media object. The MIME type is determined from the extension of the Filename on the <see cref="Original" /> property.
        /// </summary>
        /// <value>The MIME type for this media object.</value>
        public IMimeType MimeType
        {
            get
            {
                return this._original.MimeType;
            }
        }

        /// <summary>
        /// Gets or sets the sequence of this gallery object within the containing album.
        /// </summary>
        /// <value>The sequence of this gallery object within the containing album.</value>
        public int Sequence
        {
            get
            {
                VerifyObjectIsInflated(this._sequence);
                return this._sequence;
            }
            set
            {
                this._hasChanges = (this._sequence == value ? this._hasChanges : true);
                this._sequence = value;
            }
        }

        /// <summary>
        /// Gets or sets the date this gallery object was created.
        /// </summary>
        /// <value>The date this gallery object was created.</value>
        public DateTime DateAdded
        {
            get
            {
                VerifyObjectIsInflated(this._dateAdded);
                return this._dateAdded;
            }
            set
            {
                this._hasChanges = (this._dateAdded == value ? this._hasChanges : true);
                this._dateAdded = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thumbnail file is regenerated and overwritten on the file system. This value does not affect whether or how the data store is updated during a Save operation. This property is ignored for instances of the <see cref="Album" /> class.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the thumbnail file is regenerated and overwritten on the file system when this object is saved; otherwise, <c>false</c>.
        /// </value>
        public bool RegenerateThumbnailOnSave
        {
            get
            {
                return this._regenerateThumbnailOnSave;
            }
            set
            {
                this._hasChanges = (this._regenerateThumbnailOnSave == value ? this._hasChanges : true);
                this._regenerateThumbnailOnSave = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the optimized file is regenerated and overwritten on the file system during a Save operation. This value does not affect whether or how the data store is updated. This property is ignored for instances of the <see cref="Album" /> class.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the optimized file is regenerated and overwritten on the file system when this object is saved; otherwise, <c>false</c>.
        /// </value>
        public bool RegenerateOptimizedOnSave
        {
            get
            {
                return this._regenerateOptimizedOnSave;
            }
            set
            {
                this._hasChanges = (this._regenerateOptimizedOnSave == value ? this._hasChanges : true);
                this._regenerateOptimizedOnSave = value;
            }
        }

        ///// <summary>
        ///// Gets or sets a value indicating whether, during a <see cref="Save" /> operation, metadata embedded in the original media object file is
        ///// extracted and persisted to the data store, overwriting any previous extracted metadata. This property is a pass-through
        ///// to the <see cref="IGalleryObjectMetadataItemCollection.ExtractOnSave" /> property of the <see cref="MetadataItems" /> 
        ///// property of this object, which in turn is calculated based on the <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave" />
        ///// property on each metadata item in the collection. Specifically, this property returns true if <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave" /> =
        ///// true for *every* metadata item in the collection; otherwise it returns false. Setting this property causes the
        ///// <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave" /> property to be set to the specified value for *every* metadata item in the collection.
        ///// This property is ignored for Albums.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if metadata embedded in the original media object file is
        ///// extracted and persisted to the data store when this object is saved; otherwise, <c>false</c>.
        ///// </value>
        //public bool ExtractMetadataOnSave
        //{
        //	get
        //	{
        //		return this.MetadataItems.ExtractOnSave;
        //	}
        //	set
        //	{
        //		this._hasChanges = (this.MetadataItems.ExtractOnSave == value ? this._hasChanges : true);
        //		this.MetadataItems.ExtractOnSave = value;
        //	}
        //}

        /// <summary>
        /// Gets or sets a value indicating whether the current object is synchronized with the data store.
        /// This value is set to false at the beginning of a synchronization and set to true when it is
        /// synchronized with its corresponding file(s) on disk. At the conclusion of the synchronization,
        /// all objects where IsSynchronized = false are deleted. This property defaults to true for new instances.
        /// This property is not persisted in the data store, as it is only relevant during a synchronization.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
        /// </value>
        public bool IsSynchronized
        {
            get { return this._isSynchronized; }
            set { this._isSynchronized = value; }
        }

        /// <summary>
        /// Gets or sets the behavior for reading and writing file metadata.
        /// </summary>
        /// <value>The metadata read/writer behavior.</value>
        public IMetadataReadWriter MetadataReadWriter { get; set; }

        /// <summary>
        /// Gets the metadata definitions. These are used to determine which metadata to create for new
        /// objects and what their behavior should be.
        /// </summary>
        /// <value>An instance of <see cref="IMetadataDefinitionCollection" />.</value>
        public IMetadataDefinitionCollection MetaDefinitions
        {
            get { return this._metaDefinitions ?? (this._metaDefinitions = Factory.LoadGallerySetting(GalleryId).MetadataDisplaySettings); }
        }

        /// <summary>
        /// Gets the metadata items associated with this gallery object.
        /// </summary>
        /// <value>The metadata items.</value>
        public IGalleryObjectMetadataItemCollection MetadataItems
        {
            get
            {
                if (_metadataItems == null || _metadataItems.Count == 0)
                {
                    // Only verify inflation when there aren't any meta items. We can't rely on the IsNew or
                    // IsInflated properties inside VerifyObjectIsInflated() because this property will be 
                    // called during a save operation after the gallery object ID has been assigned, which 
                    // causes IsNew to switch to false.
                    VerifyObjectIsInflated();
                }

                if (_metadataItems == null)
                {
                    _metadataItems = Factory.CreateMetadataCollection();
                }

                return this._metadataItems;
            }
        }

        /// <summary>
        /// Gets or sets a rotate/flip request for this gallery object. The action is carried out when it is saved. Applies only to <see cref="Image" />
        /// and <see cref="Video" /> objects; all others throw a <see cref="NotSupportedException" />.
        /// </summary>
        /// <value>The amount to rotate/flip this media asset.</value>
        /// <exception cref="System.NotSupportedException">Thrown when the media asset does not allow rotation or flipping.</exception>
        public MediaAssetRotateFlip RotateFlip
        {
            get
            {
                return this._rotateFlip;
            }
            set
            {
                if (this._rotateFlip != value)
                {
                    this._hasChanges = true;
                    this._rotateFlip = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current instance can be modified. Objects that are shared across threads 
        /// must be treated as read-only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can be modified; otherwise, <c>false</c>.
        /// </value>
        public bool IsWritable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user name of the user who created this gallery object.
        /// </summary>
        /// <value>The name of the created by user.</value>
        public string CreatedByUserName
        {
            get
            {
                VerifyObjectIsInflated(this._createdByUsername);
                return this._createdByUsername;
            }
            set
            {
                this._hasChanges = (this._createdByUsername == value ? this._hasChanges : true);
                this._createdByUsername = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name of the user who last modified this gallery object.
        /// </summary>
        /// <value>The user name of the user who last modified this object.</value>
        public string LastModifiedByUserName
        {
            get
            {
                VerifyObjectIsInflated(this._lastModifiedByUsername);
                return this._lastModifiedByUsername;
            }
            set
            {
                this._hasChanges = (this._lastModifiedByUsername == value ? this._hasChanges : true);
                this._lastModifiedByUsername = value;
            }
        }

        /// <summary>
        /// Gets or sets the date and time this gallery object was last modified.
        /// </summary>
        /// <value>The date and time this gallery object was last modified.</value>
        public DateTime DateLastModified
        {
            get
            {
                VerifyObjectIsInflated(this._dateLastModified);
                return this._dateLastModified;
            }
            set
            {
                this._hasChanges = (this._dateLastModified == value ? this._hasChanges : true);
                this._dateLastModified = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this gallery object is hidden from anonymous users.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is private; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrivate
        {
            get
            {
                VerifyObjectIsInflated();
                return this._isPrivate;
            }
            set
            {
                this._hasChanges = (this._isPrivate == value ? this._hasChanges : true);
                this._isPrivate = value;
            }
        }

        #endregion

        #region Protected/Private Properties

        /// <summary>
        /// Gets or sets the save behavior.
        /// </summary>
        /// <value>The save behavior.</value>
        protected ISaveBehavior SaveBehavior
        {
            get
            {
                return this._saveBehavior;
            }
            set
            {
                this._saveBehavior = value;
            }
        }

        /// <summary>
        /// Gets or sets the delete behavior.
        /// </summary>
        /// <value>The delete behavior.</value>
        protected IDeleteBehavior DeleteBehavior
        {
            get
            {
                return this._deleteBehavior;
            }
            set
            {
                this._deleteBehavior = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="System.Text.RegularExpressions.Regex" /> instance that can be used to match the replacement tokens
        /// in the metadata definition's default value settings.
        /// </summary>
        /// <value>A  <see cref="System.Text.RegularExpressions.Regex" /> instance.</value>
        private static Regex MetaRegEx
        {
            get
            {
                if (_metaRegEx == null)
                {
                    _metaRegEx = new Regex(GetMetadataRegExPattern(), RegexOptions.Compiled);
                }

                return _metaRegEx;
            }
        }

        /// <summary>
        /// Gets an array of the required metadata items that all gallery objects must possess.
        /// </summary>
        /// <value>An array of <see cref="MetadataItemName" /> values.</value>
        private static MetadataItemName[] RequiredMetadataItems
        {
            get { return new[] { MetadataItemName.Title, MetadataItemName.Caption }; }
        }

        /// <summary>
        /// Gets an array of the metadata items that are not generated from a physical media file.
        /// </summary>
        /// <value>An array of <see cref="MetadataItemName" /> values.</value>
        /// <remarks>When metadata extraction is disabled (<see cref="IGallerySettings.ExtractMetadata" /> = <c>false</c>, we typically 
        /// don't want to use the extractor classes. However, there are exceptions when we want to use the extractor architecture to generate
        /// meta properties for things that don't original in a physical file.</remarks>
        private static MetadataItemName[] NonFileMetadataItems
        {
            get { return new[] { MetadataItemName.HtmlSource, MetadataItemName.DateAdded }; }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when the <see cref="Save"/> method has been invoked, but before the object has been saved. Validation within
        /// the GalleryObject class has occurred prior to this event.
        /// </summary>
        public event System.EventHandler Saving;

        /// <summary>
        /// Occurs when the <see cref="Save"/> method has been invoked and after the object has been saved.
        /// </summary>
        public event System.EventHandler Saved;

        /// <summary>
        /// Occurs when a gallery object has been added to the gallery. Fires after the <see cref="Saved"/>.
        /// </summary>
        public event System.EventHandler Created;

        /// <summary>
        /// Occurs when a gallery object has been deleted from the gallery.
        /// </summary>
        public event EventHandler<GalleryObjectEventArgs> Deleted;

        /// <summary>
        /// Occurs after a metadata item has been created for an object but before it has been added to
        /// the <see cref="MetadataItems" /> collection.
        /// </summary>
        public event EventHandler<AddMetaEventArgs> BeforeAddMetaItem;

        #endregion


        #region Static Methods

        /// <summary>
        /// Creates a properly typed instance implementing <see cref="IGalleryObject" /> from <paramref name="mediaAsset" /> and 
        /// conforming to the specified <paramref name="options" />. When no properties are explicitly set on the <paramref name="options" />,
        /// a read-only instance is returned.
        /// </summary>
        /// <param name="mediaAsset">The media asset.</param>
        /// <param name="options">The options that specify the configuration of the returned media asset. When the <see cref="MediaLoadOptions.Album" />
        /// property is null, an instance of IAlbum is created based on <see cref="CacheItemMedia.AlbumId" /> property of <paramref name="mediaAsset" />.</param>
        /// <returns>An instance implementing <see cref="IGalleryObject" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaAsset" /> is null.</exception>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when the <see cref="CacheItemMedia.GalleryObjectType" /> property of
        /// <paramref name="mediaAsset" /> is not recognized.</exception>
        public static IGalleryObject CreateFrom(CacheItemMedia mediaAsset, MediaLoadOptions options)
        {
            if (mediaAsset == null)
            {
                throw new ArgumentNullException(nameof(mediaAsset));
            }

            var parentAlbum = options.Album ?? Factory.LoadAlbumInstance(mediaAsset.AlbumId);

            IGalleryObject mo;

            switch (mediaAsset.GalleryObjectType)
            {
                case GalleryObjectType.Image:
                    mo = new Image(
                      mediaAsset.Id,
                      parentAlbum,
                      mediaAsset.DisplayObjects[0].FileName,
                      mediaAsset.DisplayObjects[0].Width,
                      mediaAsset.DisplayObjects[0].Height,
                      mediaAsset.DisplayObjects[0].FileSizeKB,
                      mediaAsset.DisplayObjects[1].FileName,
                      mediaAsset.DisplayObjects[1].Width,
                      mediaAsset.DisplayObjects[1].Height,
                      mediaAsset.DisplayObjects[1].FileSizeKB,
                      mediaAsset.DisplayObjects[2].FileName,
                      mediaAsset.DisplayObjects[2].Width,
                      mediaAsset.DisplayObjects[2].Height,
                      mediaAsset.DisplayObjects[2].FileSizeKB,
                      mediaAsset.Sequence,
                      mediaAsset.CreatedByUserName,
                      mediaAsset.DateAdded,
                      mediaAsset.LastModifiedByUserName,
                      mediaAsset.DateLastModified,
                      mediaAsset.IsPrivate,
                      true,
                      null,
                      null);

                    break;

                case GalleryObjectType.Video:
                    {
                        mo = new Video(
                          mediaAsset.Id,
                          parentAlbum,
                          mediaAsset.DisplayObjects[0].FileName,
                          mediaAsset.DisplayObjects[0].Width,
                          mediaAsset.DisplayObjects[0].Height,
                          mediaAsset.DisplayObjects[0].FileSizeKB,
                          mediaAsset.DisplayObjects[1].FileName,
                          mediaAsset.DisplayObjects[1].Width,
                          mediaAsset.DisplayObjects[1].Height,
                          mediaAsset.DisplayObjects[1].FileSizeKB,
                          mediaAsset.DisplayObjects[2].FileName,
                          mediaAsset.DisplayObjects[2].Width,
                          mediaAsset.DisplayObjects[2].Height,
                          mediaAsset.DisplayObjects[2].FileSizeKB,
                          mediaAsset.Sequence,
                          mediaAsset.CreatedByUserName,
                          mediaAsset.DateAdded,
                          mediaAsset.LastModifiedByUserName,
                          mediaAsset.DateLastModified,
                          mediaAsset.IsPrivate,
                          true,
                          null,
                          null);

                        break;
                    }
                case GalleryObjectType.Audio:
                    {
                        mo = new Audio(
                          mediaAsset.Id,
                          parentAlbum,
                          mediaAsset.DisplayObjects[0].FileName,
                          mediaAsset.DisplayObjects[0].Width,
                          mediaAsset.DisplayObjects[0].Height,
                          mediaAsset.DisplayObjects[0].FileSizeKB,
                          mediaAsset.DisplayObjects[1].FileName,
                          mediaAsset.DisplayObjects[1].Width,
                          mediaAsset.DisplayObjects[1].Height,
                          mediaAsset.DisplayObjects[1].FileSizeKB,
                          mediaAsset.DisplayObjects[2].FileName,
                          mediaAsset.DisplayObjects[2].Width,
                          mediaAsset.DisplayObjects[2].Height,
                          mediaAsset.DisplayObjects[2].FileSizeKB,
                          mediaAsset.Sequence,
                          mediaAsset.CreatedByUserName,
                          mediaAsset.DateAdded,
                          mediaAsset.LastModifiedByUserName,
                          mediaAsset.DateLastModified,
                          mediaAsset.IsPrivate,
                          true,
                          null,
                          null);

                        break;
                    }
                case GalleryObjectType.External:
                    {
                        mo = new ExternalMediaObject(
                          mediaAsset.Id,
                          parentAlbum,
                          mediaAsset.DisplayObjects[0].FileName,
                          mediaAsset.DisplayObjects[0].Width,
                          mediaAsset.DisplayObjects[0].Height,
                          mediaAsset.DisplayObjects[0].FileSizeKB,
                          mediaAsset.DisplayObjects[2].ExternalHtmlSource,
                          mediaAsset.DisplayObjects[2].ExternalType,
                          mediaAsset.Sequence,
                          mediaAsset.CreatedByUserName,
                          mediaAsset.DateAdded,
                          mediaAsset.LastModifiedByUserName,
                          mediaAsset.DateLastModified,
                          mediaAsset.IsPrivate,
                          true,
                          null);

                        break;
                    }
                case GalleryObjectType.Generic:
                case GalleryObjectType.Unknown:
                    {
                        mo = new GenericMediaObject(
                          mediaAsset.Id,
                          parentAlbum,
                          mediaAsset.DisplayObjects[0].FileName,
                          mediaAsset.DisplayObjects[0].Width,
                          mediaAsset.DisplayObjects[0].Height,
                          mediaAsset.DisplayObjects[0].FileSizeKB,
                          mediaAsset.DisplayObjects[2].FileName,
                          mediaAsset.DisplayObjects[2].Width,
                          mediaAsset.DisplayObjects[2].Height,
                          mediaAsset.DisplayObjects[2].FileSizeKB,
                          mediaAsset.Sequence,
                          mediaAsset.CreatedByUserName,
                          mediaAsset.DateAdded,
                          mediaAsset.LastModifiedByUserName,
                          mediaAsset.DateLastModified,
                          mediaAsset.IsPrivate,
                          true,
                          null,
                          null);
                        break;
                    }
                default:
                    {
                        throw new UnsupportedMediaObjectTypeException(Path.Combine(parentAlbum.FullPhysicalPath, mediaAsset.DisplayObjects[2].FileName));
                    }
            }

            if (parentAlbum.AllowMetadataLoading)
            {
                mo.AddMeta(GalleryObjectMetadataItemCollection.FromCacheItemMetas(mo, mediaAsset.MetaItems));
            }

            mo.IsWritable = options.IsWritable;

            return mo;
        }

        #endregion


        #region Public Virtual Methods (throw exception)

        /// <summary>
        /// Adds the specified gallery object as a child of this gallery object.
        /// </summary>
        /// <param name="galleryObject">The IGalleryObject to add as a child of this
        /// gallery object.</param>
        /// <exception cref="System.NotSupportedException">Thrown when an inherited type
        /// does not allow the addition of child gallery objects.</exception>
        public virtual void AddGalleryObject(IGalleryObject galleryObject)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds the specified gallery object as a child of this gallery object. This method is called by the <see cref="AddGalleryObject"/> method and should not be called directly.
        /// </summary>
        /// <param name="galleryObject">The gallery object to add as a child of this gallery object.</param>
        public virtual void DoAddGalleryObject(IGalleryObject galleryObject)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the specified gallery object from the collection of child objects
        /// of this gallery object.
        /// </summary>
        /// <param name="galleryObject">The IGalleryObject to remove as a child of this
        /// gallery object.</param>
        /// <exception cref="System.NotSupportedException">Thrown when an inherited type
        /// does not allow the addition of child gallery objects.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the specified
        /// gallery object is not child of this gallery object.</exception>
        public virtual void RemoveGalleryObject(IGalleryObject galleryObject)
        {
            throw new NotSupportedException();
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
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool excludePrivateObjects)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds the specified metadata item to this gallery object.
        /// </summary>
        /// <param name="metaItems">An instance of <see cref="IGalleryObjectMetadataItemCollection" /> 
        /// containing the items to add to this gallery object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaItems" /> is null.</exception>
        public void AddMeta(IGalleryObjectMetadataItemCollection metaItems)
        {
            if (metaItems == null)
                throw new ArgumentNullException("metaItems");

            if (_metadataItems == null)
                _metadataItems = metaItems;
            else
                _metadataItems.AddRange(metaItems);

            _metadataItems.ApplyDisplayOptions(MetaDefinitions);
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// This method provides an opportunity for a derived class to verify the thumbnail information for this instance has 
        /// been retrieved from the data store. This method is empty.
        /// </summary>
        /// <param name="thumbnail">A reference to the thumbnail display object for this instance.</param>
        protected virtual void VerifyThumbnailIsInflated(IDisplayObject thumbnail)
        {
            // Overridden in Album class.
        }

        /// <summary>
        /// Verifies the sequence of this instance within the album has been assigned. If the sequence has not yet been assigned, 
        /// default it to 1 higher than the highest sequence among its brothers and sisters.
        /// </summary>
        protected virtual void ValidateSequence()
        {
            if (this.Sequence == int.MinValue)
            {
                this.Sequence = this.Parent.GetChildGalleryObjects().Max(g => g.Sequence) + 1;
            }
        }

        /// <summary>
        /// Verifies that the thumbnail image for this instance maps to an existing image file on disk. If not, set the
        ///  <see cref="RegenerateThumbnailOnSave" />
        /// property to true so that the thumbnail image is created during the <see cref="Save" /> operation.
        /// <note type="implementnotes">The <see cref="Album" /> class overrides this method with an empty implementation, because albums don't have thumbnail
        /// images, at least not in the strictest sense.</note>
        /// </summary>
        protected virtual void CheckForThumbnailImage()
        {
            if (!System.IO.File.Exists(this.Thumbnail.FileNamePhysicalPath))
            {
                this.RegenerateThumbnailOnSave = true;
            }
        }

        ///// <summary>
        ///// Set the title for this instance based on the title metadata item, if present. No action is 
        ///// taken if the metadata item doesn't exist.
        ///// </summary>
        //protected void SetTitle()
        //{
        //	IGalleryObjectMetadataItem metaItem;
        //	if (MetadataItems.TryGetMetadataItem(MetadataItemName.Title, out metaItem))
        //	{
        //		this.Title = metaItem.Value;
        //	}
        //}

        /// <summary>
        /// This method provides an opportunity for a derived class to verify the optimized image maps to an existing file on disk.
        /// This method is empty.
        /// </summary>
        protected virtual void CheckForOptimizedImage()
        {
            // Overridden in Image class.
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Persist this gallery object to the data store.
        /// </summary>
        public void Save()
        {
            var isNew = IsNew;

            // Verify it is valid to save this object.
            ValidateSave();

            // Raise the Saving event.
            if (Saving != null)
            {
                Saving(this, new EventArgs());
            }

            // Persist to data store if the object is new (has not yet been saved) or it
            // has unsaved changes. The save behavior also updates the album's thumbnail if needed.
            if ((this._isNew) || (_hasChanges))
                this._saveBehavior.Save();

            this.HasChanges = false;
            this._galleryIdHasChanged = false;
            this.IsNew = false;
            this.RegenerateThumbnailOnSave = false;
            this.RegenerateOptimizedOnSave = false;
            if (!this.IsInflated)
                this.IsInflated = true;

            ValidateThumbnailsAfterSave();

            // Raise the Saved event.
            Saved?.Invoke(this, new EventArgs());

            if (isNew)
            {
                // Raise the Created event.
                Created?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Permanently delete this object from the data store and disk.
        /// </summary>
        public void Delete()
        {
            this.Delete(true);
        }

        /// <summary>
        /// Permanently delete this object from the data store, but leave it's associated file or directory on the hard disk.
        /// </summary>
        public void DeleteFromGallery()
        {
            this.Delete(false);
        }

        /// <summary>
        /// Permanently delete the original file for this gallery object. Requires that an optimized version exists.
        /// If no optimized version exists, no action is taken.
        /// </summary>
        public virtual void DeleteOriginalFile()
        {
            if (String.IsNullOrEmpty(Optimized.FileName) || (this.Original.FileName.Equals(this.Optimized.FileName, StringComparison.OrdinalIgnoreCase)))
            {
                return; // No optimized version exists.
            }

            string originalPath = this.Original.FileNamePhysicalPath;
            string originalExtension = Path.GetExtension(originalPath) ?? String.Empty; // Ex: .bmp
            string optimizedExtension = Path.GetExtension(this.Optimized.FileNamePhysicalPath); // Ex: .jpg

            // Delete the original this file
            File.Delete(originalPath);

            if (!originalExtension.Equals(optimizedExtension, StringComparison.OrdinalIgnoreCase))
            {
                // The original has a different file extension than the optimized, so update the original file name with 
                // the extension from the optimized file. For example, this can happen when the original does not end with
                // the .jpeg extension (it may be JPG, BMP, TIF, etc).
                originalPath = Path.ChangeExtension(originalPath, optimizedExtension);

                // Now validate that the new path is not already used by an existing file. For example, we might be renaming
                // zOpt_photo.jpeg to photo.jpg. If photo.jpg is already in use, we need to change it to something else.
                string dirPath = Path.GetDirectoryName(originalPath);
                string filename = Path.GetFileName(originalPath);
                string newFilename = HelperFunctions.ValidateFileName(dirPath, filename);

                if (!newFilename.Equals(filename, StringComparison.OrdinalIgnoreCase))
                    originalPath = Path.Combine(dirPath, newFilename);
            }

            // Rename the optimized file to the original file. This is required because
            // optimized file names can be slightly different than the original file names. For example, optimized images
            // are prefixed with "zOpt_" and are always a JPEG file type, while the original does not have a special prefix
            // and may be BMP, TIF, etc.
            HelperFunctions.MoveFileSafely(this.Optimized.FileNamePhysicalPath, originalPath);

            this.Original.FileInfo = new System.IO.FileInfo(originalPath);
            //this.Original.FileName = Path.GetFileName(originalPath);
            //this.Original.FileNamePhysicalPath = originalPath;


            this.Optimized.FileInfo = this.Original.FileInfo;
            //this.Optimized.FileName = this.Original.FileName;
            //this.Optimized.FileNamePhysicalPath = this.Original.FileNamePhysicalPath;

            this.Original.Width = this.Optimized.Width;
            this.Original.Height = this.Optimized.Height;
            this.Original.FileSizeKB = this.Optimized.FileSizeKB;

            this.RefreshMetadataAfterOriginalFileDeletion();
        }

        /// <summary>
        /// Set the parent of this gallery object to an instance of <see cref="NullObjects.NullGalleryObject" />.
        /// </summary>
        public void SetParentToNullObject()
        {
            this._parent = new NullObjects.NullGalleryObject();
        }

        /// <summary>
        /// Copy the current object and place it in the specified destination album. This method creates a completely separate copy
        /// of the original, including copying the physical files associated with this object. The copy is persisted to the data
        /// store and then returned to the caller.
        /// </summary>
        /// <param name="destinationAlbum">The album to which the current object should be copied.</param>
        /// <param name="userName">The user name of the currently logged on user. This will be used for the audit fields of the
        /// copied objects.</param>
        /// <returns>
        /// Returns a new gallery object that is an exact copy of the original, except that it resides in the specified
        /// destination album, and of course has a new ID.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
        public virtual IGalleryObject CopyTo(IAlbum destinationAlbum, string userName)
        {
            if (destinationAlbum == null)
                throw new ArgumentNullException("destinationAlbum");

            IGalleryObject goCopy;

            string destPath = destinationAlbum.FullPhysicalPathOnDisk;
            bool doesOptimizedImageExistAndIsDifferentThanOriginalImage = (!String.IsNullOrEmpty(this.Optimized.FileName) && (this.Optimized.FileName != this.Original.FileName));

            IGallerySettings gallerySetting = Factory.LoadGallerySetting(destinationAlbum.GalleryId);

            #region Copy original file

            if (this.Original.DisplayType == DisplayObjectType.External)
            {
                goCopy = Factory.CreateMediaObjectInstance(null, destinationAlbum, this.Original.ExternalHtmlSource, this.Original.ExternalType);
            }
            else
            {
                string destOriginalFilename = HelperFunctions.ValidateFileName(destPath, this.Original.FileName);
                string destOriginalPath = System.IO.Path.Combine(destPath, destOriginalFilename);
                System.IO.File.Copy(this.Original.FileNamePhysicalPath, destOriginalPath);

                goCopy = Factory.CreateMediaObjectInstance(destOriginalPath, destinationAlbum);
            }

            #endregion

            #region Copy optimized file

            // Determine path where optimized should be saved. If no optimized path is specified in the config file,
            // use the same directory as the original. Don't do anything if no optimized filename is specified or it's
            // the same file as the original.
            // FYI: Currently the optimized image is never external (only the original may be), but we test it anyway for future bullet-proofing.
            if ((this.Optimized.DisplayType != DisplayObjectType.External) && doesOptimizedImageExistAndIsDifferentThanOriginalImage)
            {
                string destOptimizedPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
                string destOptimizedFilepath = System.IO.Path.Combine(destOptimizedPathWithoutFilename, HelperFunctions.ValidateFileName(destOptimizedPathWithoutFilename, this.Optimized.FileName));
                if (System.IO.File.Exists(this.Optimized.FileNamePhysicalPath))
                {
                    System.IO.File.Copy(this.Optimized.FileNamePhysicalPath, destOptimizedFilepath);
                }

                // Assign newly created copy of optimized image to the copy of our media object instance and update
                // various properties.
                goCopy.Optimized.FileInfo = new System.IO.FileInfo(destOptimizedFilepath);
                goCopy.Optimized.Width = this.Optimized.Width;
                goCopy.Optimized.Height = this.Optimized.Height;
                goCopy.Optimized.FileSizeKB = this.Optimized.FileSizeKB;
            }

            #endregion

            #region Copy thumbnail file

            // Determine path where thumbnail should be saved. If no thumbnail path is specified in the config file,
            // use the same directory as the original.
            // FYI: Currently the thumbnail image is never external (only the original may be), but we test it anyway for future bullet-proofing.
            if (this.Thumbnail.DisplayType != DisplayObjectType.External)
            {
                string destThumbnailPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
                string destThumbnailFilepath = System.IO.Path.Combine(destThumbnailPathWithoutFilename, HelperFunctions.ValidateFileName(destThumbnailPathWithoutFilename, this.Thumbnail.FileName));
                if (System.IO.File.Exists(this.Thumbnail.FileNamePhysicalPath))
                {
                    System.IO.File.Copy(this.Thumbnail.FileNamePhysicalPath, destThumbnailFilepath);
                }

                // Assign newly created copy of optimized image to the copy of our media object instance and update
                // various properties.
                goCopy.Thumbnail.FileInfo = new System.IO.FileInfo(destThumbnailFilepath);
                goCopy.Thumbnail.Width = this.Thumbnail.Width;
                goCopy.Thumbnail.Height = this.Thumbnail.Height;
                goCopy.Thumbnail.FileSizeKB = this.Thumbnail.FileSizeKB;
            }

            #endregion

            //goCopy.Title = this.Title;
            goCopy.IsPrivate = destinationAlbum.IsPrivate;

            goCopy.MetadataItems.Clear();
            goCopy.MetadataItems.AddRange(MetadataItems.Copy());

            // Associate the new meta items with the copied object.
            foreach (var metadataItem in goCopy.MetadataItems)
            {
                metadataItem.GalleryObject = goCopy;
            }

            IGalleryObjectMetadataItem metaItem;
            if (goCopy.MetadataItems.TryGetMetadataItem(MetadataItemName.DateAdded, out metaItem))
            {
                metaItem.Value = DateTime.Now.ToString(gallerySetting.MetadataDateTimeFormatString, CultureInfo.InvariantCulture);
                metaItem.RawValue = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
            }

            if (!String.IsNullOrWhiteSpace(goCopy.Original.FileName) && goCopy.MetadataItems.TryGetMetadataItem(MetadataItemName.FileName, out metaItem))
            {
                metaItem.Value = goCopy.Original.FileName;
            }

            HelperFunctions.UpdateAuditFields(goCopy, userName);
            goCopy.Save();

            return goCopy;
        }

        /// <summary>
        /// Move the current object to the specified destination album. This method moves the physical files associated with this
        /// object to the destination album's physical directory. This instance's <see cref="Save" /> method is invoked to persist the changes to the
        /// data store. When moving albums, all the album's children, grandchildren, etc are also moved.
        /// </summary>
        /// <param name="destinationAlbum">The album to which the current object should be moved.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
        public virtual void MoveTo(IAlbum destinationAlbum)
        {
            if (destinationAlbum == null)
                throw new ArgumentNullException("destinationAlbum");

            // Get list of albums whose thumbnails we'll update after the move operation.
            IIntegerCollection albumsNeedingNewThumbnails = GetAlbumHierarchy(destinationAlbum.Id);

            string destPath = destinationAlbum.FullPhysicalPathOnDisk;

            IGallerySettings gallerySetting = Factory.LoadGallerySetting(destinationAlbum.GalleryId);

            #region Move original file

            string destOriginalPath = String.Empty;
            if (System.IO.File.Exists(this.Original.FileNamePhysicalPath))
            {
                string destOriginalFilename = HelperFunctions.ValidateFileName(destPath, this.Original.FileName);
                destOriginalPath = System.IO.Path.Combine(destPath, destOriginalFilename);
                HelperFunctions.MoveFileSafely(this.Original.FileNamePhysicalPath, destOriginalPath);
            }

            #endregion

            #region Move optimized file

            // Determine path where optimized should be saved. If no optimized path is specified in the config file,
            // use the same directory as the original.
            string destOptimizedFilepath = String.Empty;
            if ((!String.IsNullOrEmpty(this.Optimized.FileName)) && (!this.Optimized.FileName.Equals(this.Original.FileName)))
            {
                string destOptimizedPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
                destOptimizedFilepath = System.IO.Path.Combine(destOptimizedPathWithoutFilename, HelperFunctions.ValidateFileName(destOptimizedPathWithoutFilename, this.Optimized.FileName));
                if (System.IO.File.Exists(this.Optimized.FileNamePhysicalPath))
                {
                    HelperFunctions.MoveFileSafely(this.Optimized.FileNamePhysicalPath, destOptimizedFilepath);
                }
            }

            #endregion

            #region Move thumbnail file

            // Determine path where thumbnail should be saved. If no thumbnail path is specified in the config file,
            // use the same directory as the original.
            string destThumbnailPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
            string destThumbnailFilepath = System.IO.Path.Combine(destThumbnailPathWithoutFilename, HelperFunctions.ValidateFileName(destThumbnailPathWithoutFilename, this.Thumbnail.FileName));
            if (System.IO.File.Exists(this.Thumbnail.FileNamePhysicalPath))
            {
                HelperFunctions.MoveFileSafely(this.Thumbnail.FileNamePhysicalPath, destThumbnailFilepath);
            }

            #endregion

            var oldParentId = this.Parent.Id;

            this.Parent = destinationAlbum;
            this.GalleryId = destinationAlbum.GalleryId;
            this.IsPrivate = destinationAlbum.IsPrivate;
            this.Sequence = int.MinValue; // Reset the sequence so that it will be assigned a new value placing it at the end.

            // Update the FileInfo properties for the original, optimized and thumbnail objects. This is necessary in order to update
            // the filename, in case they were changed because the destination directory already had files with the same name.
            if (System.IO.File.Exists(destOriginalPath))
                this.Original.FileInfo = new System.IO.FileInfo(destOriginalPath);

            if (System.IO.File.Exists(destOptimizedFilepath))
                this.Optimized.FileInfo = new System.IO.FileInfo(destOptimizedFilepath);

            if (System.IO.File.Exists(destThumbnailFilepath))
                this.Thumbnail.FileInfo = new System.IO.FileInfo(destThumbnailFilepath);

            Save();

            CacheController.RemoveMediaAssetIdFromParentAlbumCacheItem(Id, oldParentId);
            CacheController.AddMediaAssetIdToAlbumCacheItem(Id, Parent.Id);

            // Now assign new thumbnails (if needed) to the albums we moved FROM. (The thumbnail for the destination album was updated in 
            // the Save() method.)
            foreach (int albumId in albumsNeedingNewThumbnails)
            {
                Album.AssignAlbumThumbnailIfMissing(Factory.LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = true }), false, false, this.LastModifiedByUserName);
            }
        }

        /// <summary>
        /// Build the set of metadata for the current gallery object and assign to the <see cref="MetadataItems" />
        /// property.
        /// </summary>
        public void ExtractMetadata()
        {
            // Iterate through the metadata definitions and add an instance for each one if appropriate.
            foreach (var metaDef in MetaDefinitions)
            {
                ExtractMetadata(metaDef);
            }

            MetadataItems.ApplyDisplayOptions(MetaDefinitions);
        }

        /// <summary>
        /// Extract the meta property <paramref name="metaDef" /> from the current gallery object and assign to the <see cref="MetadataItems" />
        /// property.
        /// </summary>
        /// <param name="metaDef">The meta definition.</param>
        public void ExtractMetadata(IMetadataDefinition metaDef)
        {
            if (MetadataDefinitionApplies(metaDef))
            {
                var metaItem = CreateMetaItem(metaDef);

                // Raise the BeforeAddMetaItem event.
                if (BeforeAddMetaItem != null)
                {
                    var args = new AddMetaEventArgs(metaItem);

                    BeforeAddMetaItem(this, args);

                    if (args.Cancel)
                    {
                        RemoveMetadataItem(metaDef.MetadataItem);
                        return;
                    }
                }

                // Add/update the item, but only when it is defined as being editable or has a value.
                if (metaItem.MetaDefinition.IsEditable || !String.IsNullOrWhiteSpace(metaItem.Value))
                    UpdateInternalMetaItem(metaItem);
                else if (Array.IndexOf(RequiredMetadataItems, metaDef.MetadataItem) < 0)
                    RemoveMetadataItem(metaDef.MetadataItem);
            }
            else
                RemoveMetadataItem(metaDef.MetadataItem);
        }

        /// <summary>
        /// Calculates the actual rotation amount that must be applied based on the user's requested rotation 
        /// and the file's actual orientation.
        /// </summary>
        /// <returns>An instance of <see cref="MediaAssetRotateFlip" />.</returns>
        public MediaAssetRotateFlip CalculateNeededRotation()
        {
            //TODO: FFMPEG now supports auto-rotation. See LordNeckbeard's answer in http://superuser.com/questions/578321/how-to-flip-a-video-180%C2%B0-vertical-upside-down-with-ffmpeg
            var fileRotation = GetOrientation(); // Actual rotation of the original file, as discovered via orientation metadata
            var userRotation = RotateFlip; // Desired rotation by the user

            if (userRotation == MediaAssetRotateFlip.NotSpecified)
            {
                userRotation = MediaAssetRotateFlip.Rotate0FlipNone;
            }

            switch (fileRotation)
            {
                case Orientation.None:
                case Orientation.Normal:
                    return userRotation;

                case Orientation.Rotated90:
                    switch (userRotation)
                    {
                        case MediaAssetRotateFlip.Rotate0FlipNone: return MediaAssetRotateFlip.Rotate270FlipNone;
                        case MediaAssetRotateFlip.Rotate0FlipX: return MediaAssetRotateFlip.Rotate270FlipX;
                        case MediaAssetRotateFlip.Rotate0FlipY: return MediaAssetRotateFlip.Rotate270FlipY;
                        case MediaAssetRotateFlip.Rotate90FlipNone: return MediaAssetRotateFlip.Rotate0FlipNone;
                        case MediaAssetRotateFlip.Rotate90FlipX: return MediaAssetRotateFlip.Rotate0FlipX;
                        case MediaAssetRotateFlip.Rotate90FlipY: return MediaAssetRotateFlip.Rotate0FlipY;
                        case MediaAssetRotateFlip.Rotate180FlipNone: return MediaAssetRotateFlip.Rotate90FlipNone;
                        case MediaAssetRotateFlip.Rotate180FlipX: return MediaAssetRotateFlip.Rotate90FlipX;
                        case MediaAssetRotateFlip.Rotate180FlipY: return MediaAssetRotateFlip.Rotate90FlipY;
                        case MediaAssetRotateFlip.Rotate270FlipNone: return MediaAssetRotateFlip.Rotate180FlipNone;
                        case MediaAssetRotateFlip.Rotate270FlipX: return MediaAssetRotateFlip.Rotate180FlipX;
                        case MediaAssetRotateFlip.Rotate270FlipY: return MediaAssetRotateFlip.Rotate180FlipY;
                    }
                    break;

                case Orientation.Rotated180:
                    switch (userRotation)
                    {
                        case MediaAssetRotateFlip.Rotate0FlipNone: return MediaAssetRotateFlip.Rotate180FlipNone;
                        case MediaAssetRotateFlip.Rotate0FlipX: return MediaAssetRotateFlip.Rotate180FlipX;
                        case MediaAssetRotateFlip.Rotate0FlipY: return MediaAssetRotateFlip.Rotate180FlipY;
                        case MediaAssetRotateFlip.Rotate90FlipNone: return MediaAssetRotateFlip.Rotate270FlipNone;
                        case MediaAssetRotateFlip.Rotate90FlipX: return MediaAssetRotateFlip.Rotate270FlipX;
                        case MediaAssetRotateFlip.Rotate90FlipY: return MediaAssetRotateFlip.Rotate270FlipY;
                        case MediaAssetRotateFlip.Rotate180FlipNone: return MediaAssetRotateFlip.Rotate0FlipNone;
                        case MediaAssetRotateFlip.Rotate180FlipX: return MediaAssetRotateFlip.Rotate0FlipX;
                        case MediaAssetRotateFlip.Rotate180FlipY: return MediaAssetRotateFlip.Rotate0FlipY;
                        case MediaAssetRotateFlip.Rotate270FlipNone: return MediaAssetRotateFlip.Rotate90FlipNone;
                        case MediaAssetRotateFlip.Rotate270FlipX: return MediaAssetRotateFlip.Rotate90FlipX;
                        case MediaAssetRotateFlip.Rotate270FlipY: return MediaAssetRotateFlip.Rotate90FlipY;
                    }
                    break;

                case Orientation.Rotated270:
                    switch (userRotation)
                    {
                        case MediaAssetRotateFlip.Rotate0FlipNone: return MediaAssetRotateFlip.Rotate90FlipNone;
                        case MediaAssetRotateFlip.Rotate0FlipX: return MediaAssetRotateFlip.Rotate90FlipX;
                        case MediaAssetRotateFlip.Rotate0FlipY: return MediaAssetRotateFlip.Rotate90FlipY;
                        case MediaAssetRotateFlip.Rotate90FlipNone: return MediaAssetRotateFlip.Rotate180FlipNone;
                        case MediaAssetRotateFlip.Rotate90FlipX: return MediaAssetRotateFlip.Rotate180FlipX;
                        case MediaAssetRotateFlip.Rotate90FlipY: return MediaAssetRotateFlip.Rotate180FlipY;
                        case MediaAssetRotateFlip.Rotate180FlipNone: return MediaAssetRotateFlip.Rotate270FlipNone;
                        case MediaAssetRotateFlip.Rotate180FlipX: return MediaAssetRotateFlip.Rotate270FlipX;
                        case MediaAssetRotateFlip.Rotate180FlipY: return MediaAssetRotateFlip.Rotate270FlipY;
                        case MediaAssetRotateFlip.Rotate270FlipNone: return MediaAssetRotateFlip.Rotate0FlipNone;
                        case MediaAssetRotateFlip.Rotate270FlipX: return MediaAssetRotateFlip.Rotate0FlipX;
                        case MediaAssetRotateFlip.Rotate270FlipY: return MediaAssetRotateFlip.Rotate0FlipY;
                    }
                    break;
            }

            return MediaAssetRotateFlip.NotSpecified;
        }

        /// <summary>
        /// Gets the orientation of the original media file. The value is retrieved from the metadata value for 
        /// <see cref="MetadataItemName.Orientation" />. Returns <see cref="Orientation.None" /> if no orientation 
        /// metadata is found, which will be the case for any media file not having orientation metadata embedded
        /// in the media file.
        /// </summary>
        /// <returns>An instance of <see cref="Orientation" />.</returns>
        public Orientation GetOrientation()
        {
            IGalleryObjectMetadataItem orientationMeta;
            if (MetadataItems.TryGetMetadataItem(MetadataItemName.Orientation, out orientationMeta) && !orientationMeta.IsDeleted)
            {
                ushort orientationRaw;
                if (UInt16.TryParse(orientationMeta.RawValue, out orientationRaw))
                {
                    var orientation = (Orientation)orientationRaw;

                    switch (orientation)
                    {
                        case Orientation.Rotated90:
                        case Orientation.Rotated180:
                        case Orientation.Rotated270:
                            return orientation;
                    }
                }
            }

            return Orientation.None;
        }

        /// <summary>
        /// Creates a metadata item for the current gallery object. The parameter <paramref name="metaDef" />
        /// contains the template and display name to use. Guaranteed to not return null.
        /// </summary>
        /// <param name="metaDef">The metadata definition.</param>
        /// <returns>An instance of <see cref="IGalleryObjectMetadataItem" />.</returns>
        public IGalleryObjectMetadataItem CreateMetaItem(IMetadataDefinition metaDef)
        {
            // Example: metaDef.DefaultValue = "Created at {DateCreated} - {Title}"
            // Loop through all token matches. For each one, extract the metavalue and replace the value. If there is only
            // one, use the raw value in the CreateMetadataItem line; otherwise use a null raw value.
            string rawValue = null;
            var formattedValue = metaDef.DefaultValue;
            var matches = MetaRegEx.Matches(metaDef.DefaultValue);

            // Extract if extraction is enabled or if we're processing an item that isn't extracted from a file (in which case we still
            // need to use the extractor algorithm to generate the item)
            var extractMetadata = Factory.LoadGallerySetting(GalleryId).ExtractMetadata || (Array.IndexOf(NonFileMetadataItems, metaDef.MetadataItem) >= 0);

            foreach (Match match in matches)
            {
                var metaValue = extractMetadata ? ExtractMetaValue(match) : null;

                if (metaValue != null)
                {
                    formattedValue = formattedValue.Replace(String.Concat("{", match.Groups[1].Value, "}"), metaValue.FormattedValue);
                    rawValue = metaValue.RawValue;
                }
                else
                {
                    formattedValue = formattedValue.Replace(String.Concat("{", match.Groups[1].Value, "}"), String.Empty);
                    rawValue = null;
                }
            }

            return Factory.CreateMetadataItem(int.MinValue, this, (matches.Count == 1 ? rawValue : null), formattedValue, true, metaDef);
        }

        #endregion

        #region Public Abstract Methods

        /// <summary>
        /// Inflate the current object by loading all properties from the data store. If the object is already inflated (<see cref="IsInflated" />=true), no action is taken.
        /// </summary>
        public abstract void Inflate();

        /// <summary>
        /// Gets a value indicating whether the specified <paramref name="metaDef" />
        /// applies to the current gallery object.
        /// </summary>
        /// <param name="metaDef">The metadata definition.</param>
        /// <returns><c>true</c> when the specified metadata item should be displayed; otherwise <c>false</c>.</returns>
        public virtual bool MetadataDefinitionApplies(IMetadataDefinition metaDef)
        {
            if (Array.IndexOf(RequiredMetadataItems, metaDef.MetadataItem) >= 0)
                return true; // We *ALWAYS* want to create certain items (such as Title and Caption).
            else
                return metaDef.IsVisibleForGalleryObject;
        }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="GalleryObject"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="GalleryObject"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Concat(base.ToString(), "; ID = ", this.Id, "; (", this.Title, ")");
        }

        /// <summary>
        /// Serves as a hash function for a particular type. The hash code is based on <see cref="Id" />.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get a list of album IDs between the current instance and the specified <paramref name="topAlbumId" />. It works by
        /// analyzing the parent albums, recursively, of the current gallery object, until reaching either the root album or the specified
        /// <paramref name="topAlbumId" />. The caller is responsible for iterating through this list and calling 
        /// <see cref="Album.AssignAlbumThumbnailIfMissing" /> for each album after the move operation is complete.
        /// This method should be called before the move operation takes place.
        /// </summary>
        /// <param name="topAlbumId">The ID of the album the current gallery object will be in after the move operation completes.</param>
        /// <returns>Return a list of album IDs whose thumbnail images will need updating after the move operation completes.</returns>
        protected IIntegerCollection GetAlbumHierarchy(int topAlbumId)
        {
            IIntegerCollection albumsInHierarchy = new IntegerCollection();
            IGalleryObject album = this.Parent;

            while (!(album is NullObjects.NullGalleryObject))
            {
                // If we're at the same level as the destination album, don't go any further.
                if (album.Id == topAlbumId)
                    break;

                albumsInHierarchy.Add(album.Id);

                album = album.Parent;
            }

            return albumsInHierarchy;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Re-extract several metadata values from the file that may now be inaccurate due to the deletion of the original
        /// media file. The new values are not persisted; it is expected a subsequent function will do that.
        /// </summary>
        private void RefreshMetadataAfterOriginalFileDeletion()
        {
            var metadataNames = new[] {
            MetadataItemName.DateFileCreated, MetadataItemName.DateFileCreatedUtc, MetadataItemName.DateFileLastModified,
            MetadataItemName.DateFileLastModifiedUtc, MetadataItemName.FileName, MetadataItemName.FileNameWithoutExtension,
            MetadataItemName.FileSizeKb, MetadataItemName.Width, MetadataItemName.Height, MetadataItemName.Dimensions,
            MetadataItemName.HorizontalResolution, MetadataItemName.VerticalResolution, MetadataItemName.Orientation};

            foreach (var mi in metadataNames)
            {
                ExtractMetadata(MetaDefinitions.Find(mi));
            }
        }

        private void UpdateInternalMetaItem(IGalleryObjectMetadataItem metaItem)
        {
            IGalleryObjectMetadataItem existingMetaItem;
            if (MetadataItems.TryGetMetadataItem(metaItem.MetadataItemName, out existingMetaItem))
            {
                existingMetaItem.Description = metaItem.Description;

                if (OkToUpdateMetaItemValue(metaItem))
                {
                    // Update value only when we have some data. This helps prevent overwriting user-entered data.
                    existingMetaItem.Value = metaItem.Value;
                    existingMetaItem.RawValue = metaItem.RawValue;
                }
            }
            else
            {
                var metaItems = Factory.CreateMetadataCollection();
                metaItems.Add(metaItem);

                AddMeta(metaItems);
            }
        }

        /// <summary>
        /// Determines whether we can retrieve the value from <paramref name="metaItemSource" /> and assign it to the
        /// actual metadata item for the gallery object. Returns <c>true</c> when the metaitem has a value and when it
        /// does not belong to an album title or caption; otherwise returns <c>false</c>.
        /// </summary>
        /// <param name="metaItemSource">The source metaitem.</param>
        /// <returns>Returns <c>true</c> or <c>false</c>.</returns>
        private static bool OkToUpdateMetaItemValue(IGalleryObjectMetadataItem metaItemSource)
        {
            var hasValue = !String.IsNullOrWhiteSpace(metaItemSource.Value);
            var isAlbumTitleOrCaption = (metaItemSource.GalleryObject.GalleryObjectType == GalleryObjectType.Album) && ((metaItemSource.MetadataItemName == MetadataItemName.Title) || (metaItemSource.MetadataItemName == MetadataItemName.Caption));

            return hasValue && !isAlbumTitleOrCaption;
        }

        private void RemoveMetadataItem(MetadataItemName metaName)
        {
            IGalleryObjectMetadataItem metaItem;
            if (MetadataItems.TryGetMetadataItem(metaName, out metaItem))
            {
                metaItem.IsDeleted = true;
            }
        }

        private void RecalculateFilePaths()
        {
            string albumPath = this._parent.FullPhysicalPathOnDisk;

            // Thumbnail
            if (!String.IsNullOrEmpty(this._thumbnail.FileName))
                this._thumbnail.FileNamePhysicalPath = System.IO.Path.Combine(albumPath, this._thumbnail.FileName);
            else
                this._thumbnail.FileNamePhysicalPath = String.Empty;

            // Optimized
            if (!String.IsNullOrEmpty(this._optimized.FileName))
                this._optimized.FileNamePhysicalPath = System.IO.Path.Combine(albumPath, this._optimized.FileName);
            else
                this._optimized.FileNamePhysicalPath = String.Empty;

            // Original
            if (!String.IsNullOrEmpty(this._original.FileName))
                this._original.FileNamePhysicalPath = System.IO.Path.Combine(albumPath, this._original.FileName);
            else
                this._original.FileNamePhysicalPath = String.Empty;
        }

        private void VerifyObjectIsInflated(string propertyValue)
        {
            // If the string is empty, and this is not a new object, and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if ((String.IsNullOrEmpty(propertyValue)) && (!this.IsNew) && (!this.IsInflated))
            {
                this.Inflate();
            }
        }

        private void VerifyObjectIsInflated(DateTime propertyValue)
        {
            // If the string is empty, and this is not a new object, and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if ((propertyValue == DateTime.MinValue) && (!this.IsNew) && (!this.IsInflated))
            {
                this.Inflate();
            }
        }

        private void VerifyObjectIsInflated(int propertyValue)
        {
            // If the int = int.MinValue, and this is not a new object, and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if ((propertyValue == int.MinValue) && (!this.IsNew) && (!this.IsInflated))
            {
                this.Inflate();
            }
        }

        private void VerifyObjectIsInflated()
        {
            // If this is a pre-existing object (i.e. one that exists in the data store), and it has not been inflated
            // from the database, go to the database and retrieve the info for this object.
            if ((!this.IsNew) && (!this.IsInflated))
            {
                this.Inflate();
            }
        }

        private void ValidateSave()
        {
            if ((!this.IsNew) && (!this.IsInflated))
            {
                throw new System.InvalidOperationException(Resources.GalleryObject_ValidateSave_Ex_Msg);
            }

            VerifyInstanceIsUpdateable();

            ValidateSequence();

            // Set RegenerateThumbnailOnSave to true if thumbnail image doesn't exist.
            CheckForThumbnailImage();

            // Set RegenerateOptimizedOnSave to true if optimized image doesn't exist. This is an empty virtual method
            // that is overridden in the Image class. That is, this method does nothing for non-images.
            CheckForOptimizedImage();

            // Make sure the audit fields have been set.
            ValidateAuditFields();
        }

        private void VerifyInstanceIsUpdateable()
        {
            if (!IsWritable)
            {
                throw new BusinessException(String.Format(CultureInfo.CurrentCulture, $"This gallery object (ID {Id}, {GalleryObjectType}) is not updateable."));
            }
        }

        private void ValidateAuditFields()
        {
            if (String.IsNullOrEmpty(this.CreatedByUserName))
                throw new BusinessException("The property CreatedByUsername must be set to the currently logged on user before this object can be saved.");

            if (this.DateAdded == DateTime.MinValue)
                throw new BusinessException("The property DateAdded must be assigned a valid date before this object can be saved.");

            if (String.IsNullOrEmpty(this.LastModifiedByUserName))
                throw new BusinessException("The property LastModifiedByUsername must be set to the currently logged on user before this object can be saved.");

            DateTime aFewMomentsAgo = DateTime.UtcNow.Subtract(new TimeSpan(0, 10, 0)); // 10 minutes ago
            if (this.HasChanges && (this.DateLastModified < aFewMomentsAgo))
                throw new BusinessException("The property DateLastModified must be assigned the current date before this object can be saved.");

            // Make sure a valid date is assigned to the DateAdded property. If it is still DateTime.MinValue,
            // update it with the current date/time.
            //System.Diagnostics.Debug.Assert((this.IsNew || ((!this.IsNew) && (this.DateAdded > DateTime.MinValue))),
            //  String.Format(CultureInfo.CurrentCulture, "Media objects and albums that have been saved to the data store should never have the property DateAdded=MinValue. IsNew={0}; DateAdded={1}",
            //  this.IsNew, this.DateAdded.ToLongDateString()));

            //if (this.DateAdded == DateTime.MinValue)
            //{
            //  this.DateAdded = DateTime.Now;
            //}
        }

        private void ValidateThumbnailsAfterSave()
        {
            // Update the album's thumbnail if necessary.
            IAlbum parentAlbum = this._parent as IAlbum;

            if ((parentAlbum != null) && (parentAlbum.ThumbnailMediaObjectId == 0))
            {
                Album.AssignAlbumThumbnailIfMissing(parentAlbum, true, false, this.LastModifiedByUserName);
            }
        }

        private void Delete(bool deleteFromFileSystem)
        {
            // Get affected child album IDs and child media IDs that exist in cache. We'll purge these from the cache in the Deleted event.
            List<int> childAlbumIds;
            List<int> childMediaIds;
            GenerateAffectedIdsThatAreInCache(out childAlbumIds, out childMediaIds);

            RemoveFromMediaConversionQueue(Id, childMediaIds);

            this._deleteBehavior.Delete(deleteFromFileSystem);

            var parentAlbum = Parent as IAlbum;
            var parentAlbumId = Parent.Id;

            parentAlbum?.RemoveGalleryObject(this);

            // Raise the Deleted event.
            Deleted?.Invoke(this, new GalleryObjectEventArgs(parentAlbumId, childAlbumIds, childMediaIds));

            if (parentAlbum != null)
            {
                Album.AssignAlbumThumbnailIfMissing(parentAlbum, true, false, this.LastModifiedByUserName);
            }
        }

        /// <summary>
        /// Removes the <paramref name="currentGalleryObjectId" /> and the <paramref name="mediaIds" /> from the media conversion queue,
        /// if they exist there.
        /// </summary>
        /// <param name="currentGalleryObjectId">The ID of the gallery object we want to remove from the queue. No action is taken if the
        /// current item is an album, since only media assets are processed in the queue.</param>
        /// <param name="mediaIds">The IDs of all media assets to remove from the queue.</param>
        private void RemoveFromMediaConversionQueue(int currentGalleryObjectId, IEnumerable<int> mediaIds)
        {
            var currentMediaQueueItem = MediaConversionQueue.Instance.GetCurrentMediaQueueItem();

            var mediaIdsNotDeletedFirstTime = new List<int>();

            // First we remove all items that aren't currently being processed.
            foreach (var mediaId in mediaIds)
            {
                if (currentMediaQueueItem?.MediaObjectId == mediaId)
                {
                    mediaIdsNotDeletedFirstTime.Add(mediaId);
                }
                else
                {
                    MediaConversionQueue.Instance.Remove(mediaId);
                }
            }

            // Then we iterate again, removing any we didn't get the first time. If we simply removed them all in the first iteration, it would
            // work, but we'd be fighting with the processor queue, which tries to start the next one as soon as one is removed.
            foreach (var mediaId in mediaIdsNotDeletedFirstTime)
            {
                MediaConversionQueue.Instance.Remove(mediaId);
            }

            // If the current item is a media object, we remove it.
            if (GalleryObjectType != GalleryObjectType.Album)
            {
                MediaConversionQueue.Instance.Remove(currentGalleryObjectId);
            }
        }

        /// <summary>
        /// Generates lists of album and media IDs belonging to the current album that exist in cache, which can be used to purge the cache when
        /// needed. For performance reasons, this function looks only at cache; it doesn't do any data store interaction. Note that the media IDs
        /// it generates are those that exist on the <see cref="CacheItemAlbum.ChildMediaObjectIds" /> property of cached albums, so it is possible
        /// some media assets exist in the media cache that are not referenced in the album cache. However, the only negative effect of this is 
        /// that these items may not get purged from the media cache, leaving the cache bigger than necessary.
        /// </summary>
        /// <param name="childAlbumIds">The child album IDs.</param>
        /// <param name="childMediaIds">The child media IDs.</param>
        private void GenerateAffectedIdsThatAreInCache(out List<int> childAlbumIds, out List<int> childMediaIds)
        {
            childMediaIds = new List<int>();

            if (Factory.LoadGallery(GalleryId).FlattenedAlbums.TryGetValue(Id, out childAlbumIds))
            {
                var albumCache = CacheController.GetAlbumAssetCache();
                foreach (var albumId in childAlbumIds)
                {
                    CacheItemAlbum cacheAlbum;
                    if (albumCache != null && albumCache.TryGetValue(albumId, out cacheAlbum))
                    {
                        childMediaIds.AddRange(cacheAlbum.ChildMediaObjectIds.Keys);
                    }
                }
            }
            else
            {
                childAlbumIds = new List<int>();
            }
        }

        /// <summary>
        /// Gets a regular expression pattern that can be used to match the replacement tokens in 
        /// <see cref="IMetadataDefinition.DefaultValue" />. Ex: "{(AudioBitRate|AudioFormat|Author|...IptcWriterEditor)}"
        /// The replacement tokens must be values of the <see cref="MetadataItemName" /> enumeration.
        /// </summary>
        /// <returns>Returns a string that can be used as a regular expression pattern.</returns>
        private static string GetMetadataRegExPattern()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{(");

            foreach (MetadataItemName metadataItemName in Enum.GetValues(typeof(MetadataItemName)))
            {
                sb.Append(metadataItemName);
                sb.Append("|");
            }

            sb.Append(")}");

            return sb.ToString(); // Ex: "{(AudioBitRate|AudioFormat|Author|...IptcWriterEditor)}"
        }

        /// <summary>
        /// Extracts and returns the meta value for the <see cref="MetadataItemName" /> found in the 
        /// <paramref name="match" />. Returns null if no meta item is found. HTML and javascript may be
        /// removed from the meta data.
        /// </summary>
        /// <param name="match">A match from the regular expression <see cref="MetaRegEx" />. The value of
        /// the first group contains the name of the meta data item to extract.</param>
        /// <returns>An instance of <see cref="IMetaValue" />, or null if no meta item is found.</returns>
        private IMetaValue ExtractMetaValue(Match match)
        {
            var metadataNameStr = match.Groups[1].Value;

            // Since the pattern is built from the enum, we are guaranteed to successfully parse the match, so no need to catch a parse exception.
            var metadataName = (MetadataItemName)Enum.Parse(typeof(MetadataItemName), metadataNameStr, true);

            var metaValue = MetadataReadWriter.GetMetaValue(metadataName);

            if (metaValue != null)
            {
                if (metadataName != MetadataItemName.HtmlSource)
                {
                    // Remove HTML/javascript if necessary for all fields other than HTML source. Ideally, we call the clean method
                    // for all fields and let the clean method do its job based on whether the current user is an admin (in which case
                    // all HTML would be preserved) and whether the setting for user-entered HTML/javascript is enabled (for all other
                    // users). However, the clean method has no knowledge of the current user, so it'll strip HTML whenever HTML is disabled,
                    // causing the HtmlSource value to lose data.
                    metaValue.FormattedValue = HtmlValidator.Clean(metaValue.FormattedValue, GalleryId);
                }

                metaValue = (!String.IsNullOrWhiteSpace(metaValue.FormattedValue) ? metaValue : TryGetFromExisting(metadataName));
            }

            return metaValue;
        }

        /// <summary>
        /// Attempts to get the requested <paramref name="metadataName" /> from the existing set of metadata
        /// items. This is used during metadata extraction when generating an item that is based on the 
        /// calculated value of another metadata item. May return null.
        /// </summary>
        /// <param name="metadataName">Name of the metadata.</param>
        /// <returns></returns>
        /// <example>An admin may create a custom meta item with the default value "{Title} - {GpsLocationWithMapLink}".
        /// The title is extracted from the file (and thus does not use this function), but the GPS map link 
        /// is based on a template and cannot be directly extracted from the image file. This function 
        /// will return the map link in this case. Note that this function will only find an item when
        /// it has already been created, so if the item it looks for does not yet exist, a null is
        /// returned. To prevent this, an admin should ensure meta items based on other templated items occur
        /// after them (as ordered on the admin metadata page).</example>
        private IMetaValue TryGetFromExisting(MetadataItemName metadataName)
        {
            IGalleryObjectMetadataItem metaItem;
            if (MetadataItems.TryGetMetadataItem(metadataName, out metaItem))
                return new MetaValue(metaItem.Value, metaItem.RawValue);
            else
                return null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called after a metadata item has been created for an object but before it has been added to
        /// the <see cref="IGalleryObject.MetadataItems" /> collection.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="AddMetaEventArgs" /> instance containing the event data.</param>
        private void OnBeforeAddMetaItem(object sender, AddMetaEventArgs e)
        {
            switch (e.MetaItem.MetadataItemName)
            {
                case MetadataItemName.GpsLocationWithMapLink:
                    if (MetadataReadWriter.GetMetaValue(MetadataItemName.GpsLocation) == null)
                    {
                        e.MetaItem.Value = String.Empty;
                    }
                    break;

                case MetadataItemName.GpsDestLocationWithMapLink:
                    if (MetadataReadWriter.GetMetaValue(MetadataItemName.GpsDestLocation) == null)
                    {
                        e.MetaItem.Value = String.Empty;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the <see cref="GalleryObject.Saved" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void GalleryObject_Saved(object sender, EventArgs e)
        {
            CacheController.PurgeCache(this);
        }

        /// <summary>
        /// Handles the <see cref="GalleryObject.Created" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void GalleryObject_Created(object sender, EventArgs e)
        {
            if (GalleryObjectType == GalleryObjectType.Album)
            {
                CacheController.AddAlbumIdToAlbumCacheItem(Id, Parent.Id);
            }
            else
            {
                CacheController.AddMediaAssetIdToAlbumCacheItem(Id, Parent.Id);
            }

            CacheController.RemoveInflatedAlbumsFromCache();
        }

        /// <summary>
        /// Handles the <see cref="GalleryObject.Deleted" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GalleryObjectEventArgs" /> instance containing the event data.</param>
        private void GalleryObject_Deleted(object sender, GalleryObjectEventArgs e)
        {
            if (GalleryObjectType != GalleryObjectType.Album)
            {
                CacheController.RemoveMediaAssetFromCache(Id);
                CacheController.RemoveMediaAssetIdFromParentAlbumCacheItem(Id, e.ParentId);
            }

            CacheController.RemoveInflatedAlbumsFromCache();
            CacheController.RemoveTagsFromCache();
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
        /// Less than 0: This instance is less than <paramref name="other"/>.
        /// 0: This instance is equal to <paramref name="other"/>.
        /// Greater than 0: This instance is greater than <paramref name="other"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="other"/> is not the same type as this instance. </exception>
        public int CompareTo(object other)
        {
            if (other == null)
                return 1;
            else
            {
                IAlbum thisAsAlbum = this as IAlbum;
                IAlbum otherAsAlbum = other as IAlbum;
                IGalleryObject otherAsGalleryObj = other as IGalleryObject;

                bool thisIsMediaObj = (thisAsAlbum == null); // If it's not an album, it must be a media object (or a NullGalleryObject, but that shouldn't happen)
                bool otherIsMediaObj = ((otherAsGalleryObj != null) && (otherAsAlbum == null));
                bool bothObjectsAreMediaObjects = (thisIsMediaObj && otherIsMediaObj);
                bool bothObjectsAreAlbums = ((thisAsAlbum != null) && (otherAsAlbum != null));


                if (otherAsGalleryObj == null)
                    return 1;

                if (bothObjectsAreAlbums || bothObjectsAreMediaObjects)
                {
                    return this.Sequence.CompareTo(otherAsGalleryObj.Sequence);
                }
                else if (thisIsMediaObj && (otherAsAlbum != null))
                {
                    return 1;
                }
                else
                {
                    return -1; // Current instance must be album and other is media object. Albums always come first.
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Provides data for the events related to a <see cref="IGalleryObject" />.
    /// </summary>
    public class GalleryObjectEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GalleryObjectEventArgs" /> class.
        /// </summary>
        /// <param name="parentId">The ID of the parent album of the GalleryObject instance.</param>
        /// <param name="flattenedChildAlbumIds">A flattened list of all album IDs for albums belonging to the album with ID <paramref name="parentId" />,
        /// including parent ID. Specify an empty collection when the gallery object is a media object.</param>
        /// <param name="flattenedChildMediaIds">A flattened list of all media IDs for media assets belonging to the album with ID <paramref name="parentId" />.
        /// Specify an empty collection when there aren't any child media assets or the gallery object is a media object.</param>
        public GalleryObjectEventArgs(int parentId, List<int> flattenedChildAlbumIds, List<int> flattenedChildMediaIds)
        {
            if (flattenedChildAlbumIds == null)
            {
                throw new ArgumentNullException(nameof(flattenedChildAlbumIds));
            }

            if (flattenedChildMediaIds == null)
            {
                throw new ArgumentNullException(nameof(flattenedChildMediaIds));
            }

            ParentId = parentId;
            FlattenedChildAlbumIds = flattenedChildAlbumIds;
            FlattenedChildMediaIds = flattenedChildMediaIds;
        }

        /// <summary>
        /// Gets the ID of the parent album of the GalleryObject instance.
        /// </summary>
        public int ParentId { get; }

        /// <summary>
        /// Gets a flattened list of all album IDs for albums belonging to the album with ID <see cref="ParentId" />. When the gallery object is an album, 
        /// this list includes the current album ID as well. Will be an empty collection for media objects. Guaranteed to not be null.
        /// </summary>
        public List<int> FlattenedChildAlbumIds { get; }

        /// <summary>
        /// Gets a flattened list of all media IDs for media assets belonging to the album with ID <see cref="ParentId" />. When the gallery object is a
        /// media object, this list is an empty collection. Guaranteed to not be null.
        /// </summary>
        public List<int> FlattenedChildMediaIds { get; }
    }
}
