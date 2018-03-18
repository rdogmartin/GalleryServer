using System;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Represents a gallery object, which is an item that is managed by Gallery Server. Examples include
  /// albums, images, videos, audio files, and documents.
  /// </summary>
  public interface IGalleryObject
  {
    /// <summary>
    /// Occurs when the <see cref="Save" /> method has been invoked, but before the object has been saved. Validation within
    /// the GalleryObject class has occured prior to this event.
    /// </summary>
    event EventHandler Saving;

    /// <summary>
    /// Occurs when the <see cref="Save" /> method has been invoked and after the object has been saved.
    /// </summary>
    event EventHandler Saved;

    /// <summary>
    /// Gets or sets the unique identifier for this gallery object.
    /// </summary>
    /// <value>The unique identifier for this gallery object.</value>
    int Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the title for this gallery object.
    /// </summary>
    /// <value>The title for this gallery object.</value>
    string Title
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a long description for this gallery object.
    /// </summary>
    /// <value>The long description for this gallery object.</value>
    string Caption
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the value that uniquely identifies the current gallery.
    /// </summary>
    /// <value>The value that uniquely identifies the current gallery.</value>
    int GalleryId
    {
      get;
      set;
    }

    /// <summary>
    /// Gets a value that indicates whether a different gallery has been assigned
    /// to this object since it was retrieved from the data store. It is <c>false</c> at all 
    /// other times, including once the new gallery assignment is persisted.
    /// </summary>
    /// <value>The value that indicates whether a different gallery has been assigned
    /// to this object since it was retrieved from the data store.</value>
    bool GalleryIdHasChanged
    {
      get;
    }

    /// <summary>
    /// Gets or sets the thumbnail information for this gallery object.
    /// </summary>
    /// <value>The thumbnail information for this gallery object.</value>
    Interfaces.IDisplayObject Thumbnail
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the physical path to this object. Does not include the trailing slash.
    /// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets
    /// </summary>
    /// <value>The full physical path to this object.</value>
    string FullPhysicalPath
    {
      get;
    }

    /// <summary>
    /// Gets or sets the full physical path for this object as it currently exists on the hard drive. This property
    /// is updated when the object is loaded from the hard drive and when it is saved to the hard drive.
    /// Does not include the trailing slash.
    /// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets
    /// </summary>
    /// <value>The full physical path on disk.</value>
    string FullPhysicalPathOnDisk
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this object has changes that have not been persisted to the database.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
    /// </value>
    bool HasChanges
    {
      get;
      set;
    }

    /// <summary>
    /// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
    /// </summary>
    /// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
    bool IsNew
    {
      get;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this object has been fully populated with data from the data store.
    /// Once assigned a true value, it remains true for the lifetime of the object.
    /// Returns false for newly created objects that have not been saved to the data store. Set to true after an object
    /// is saved.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is inflated; otherwise, <c>false</c>.
    /// </value>
    bool IsInflated
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the information representing the original media object. (For example, the uncompressed photo, or the video / audio file.)
    /// </summary>
    /// <value>The information representing the original media object.</value>
    IDisplayObject Original
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the gallery object type.
    /// </summary>
    /// <value>An instance of <see cref="GalleryObjectType" />.</value>
    GalleryObjectType GalleryObjectType
    {
      get;
    }

    /// <summary>
    /// Gets the MIME type for this media object. The MIME type is determined from the extension of the Filename on the Original property.
    /// </summary>
    /// <value>The MIME type for this media object.</value>
    IMimeType MimeType
    {
      get;
    }

    /// <summary>
    /// Gets or sets the sequence of this gallery object within the containing album.
    /// </summary>
    /// <value>The sequence of this gallery object within the containing album.</value>
    int Sequence
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the optimized information for this gallery object.
    /// </summary>
    /// <value>The optimized information for this gallery object.</value>
    IDisplayObject Optimized
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the thumbnail file is regenerated and overwritten on the file system.
    /// This value does not affect whether or how the data store is updated during a Save operation. This property is ignored for Albums.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the thumbnail file is regenerated and overwritten on the file system when this object is saved; otherwise, <c>false</c>.
    /// </value>
    bool RegenerateThumbnailOnSave
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the optimized file is regenerated and overwritten on the file system during a Save operation. This value does not affect whether or how the data store is updated. This property is ignored for Albums.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the optimized file is regenerated and overwritten on the file system when this object is saved; otherwise, <c>false</c>.
    /// </value>
    bool RegenerateOptimizedOnSave
    {
      get;
      set;
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
    //bool ExtractMetadataOnSave
    //{
    //	get;
    //	set;
    //}

    /// <summary>
    /// Gets or sets the object that contains this gallery object.
    /// </summary>
    /// <value>The object that contains this gallery object.</value>
    IGalleryObject Parent
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the date this gallery object was created.
    /// </summary>
    /// <value>The date this gallery object was created.</value>
    DateTime DateAdded
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the user name of the user who created this gallery object.
    /// </summary>
    /// <value>The name of the created by user.</value>
    string CreatedByUserName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the user name of the user who last modified this gallery object.
    /// </summary>
    /// <value>The user name of the user who last modified this object.</value>
    string LastModifiedByUserName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the date and time this gallery object was last modified.
    /// </summary>
    /// <value>The date and time this gallery object was last modified.</value>
    DateTime DateLastModified
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this gallery object is hidden from anonymous users.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is private; otherwise, <c>false</c>.
    /// </value>
    bool IsPrivate
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the current instance is synchronized with the data store.
    /// This value is set to false at the beginning of a synchronization and set to true when it is
    /// synchronized with its corresponding file(s) on disk. At the conclusion of the synchronization,
    /// all objects where IsSynchronized = false are deleted. This property defaults to true for new instances.
    /// This property is not persisted in the data store, as it is only relevant during a synchronization.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
    /// </value>
    bool IsSynchronized
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the behavior for reading and writing file metadata.
    /// </summary>
    /// <value>The metadata read/writer behavior.</value>
    IMetadataReadWriter MetadataReadWriter { get; set; }

    /// <summary>
    /// Gets the metadata definitions. These are used to determine which metadata to create for new
    /// objects and what their behavior should be.
    /// </summary>
    /// <value>An instance of <see cref="IMetadataDefinitionCollection" />.</value>
    IMetadataDefinitionCollection MetaDefinitions
    {
      get;
    }

    /// <summary>
    /// Gets the metadata items associated with this gallery object.
    /// </summary>
    /// <value>The metadata items.</value>
    IGalleryObjectMetadataItemCollection MetadataItems
    {
      get;
    }

    /// <summary>
    /// Gets or sets a rotate/flip request for this gallery object. The action is carried out when it is saved. 
    /// Applies only to image and video objects; all others throw a NotSupportedException.
    /// </summary>
    /// <value>The amount to rotate/flip this media asset.</value>
    /// <exception cref="System.NotSupportedException">Thrown when an inherited type does not allow rotation or flipping.</exception>
    MediaAssetRotateFlip RotateFlip
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the current instance can be modified. Objects that are shared across threads 
    /// must be treated as read-only.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance can be modified; otherwise, <c>false</c>.
    /// </value>
    bool IsWritable
    {
      get;
      set;
    }

    /// <summary>
    /// Adds the specified gallery object as a child of this gallery object.
    /// </summary>
    /// <param name="galleryObject">The IGalleryObject to add as a child of this
    /// gallery object.</param>
    /// <exception cref="System.NotSupportedException">Thrown when an inherited type
    /// does not allow the addition of child gallery objects.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
    void AddGalleryObject(IGalleryObject galleryObject);

    /// <summary>
    /// Adds the specified gallery object as a child of this gallery object. This method is called by the <see cref="AddGalleryObject" /> method and should not be called directly.
    /// </summary>
    /// <param name="galleryObject">The gallery object to add as a child of this gallery object.</param>
    void DoAddGalleryObject(IGalleryObject galleryObject);

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
    void RemoveGalleryObject(IGalleryObject galleryObject);

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
    /// <exception cref="System.NotSupportedException">Thrown when an inherited type
    /// does not allow the addition of child gallery objects.</exception>
    IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType = GalleryObjectType.All, bool excludePrivateObjects = false);

    /// <summary>
    /// Adds the specified metadata item to this gallery object.
    /// </summary>
    /// <param name="metaItems">An instance of <see cref="IGalleryObjectMetadataItemCollection" /> 
    /// containing the items to add to this gallery object.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaItems" /> is null.</exception>
    void AddMeta(IGalleryObjectMetadataItemCollection metaItems);

    /// <summary>
    /// Persist this gallery object to the data store.
    /// </summary>
    void Save();

    /// <summary>
    /// Permanently delete this object from the data store and disk.
    /// </summary>
    void Delete();

    /// <summary>
    /// Permanently delete this object from the data store, but leave it's associated file or directory on the hard disk.
    /// </summary>
    void DeleteFromGallery();

    /// <summary>
    /// Permanently delete the original file for this gallery object. Requires that an optimized version exists.
    /// If no optimized version exists, no action is taken.
    /// </summary>
    void DeleteOriginalFile();

    /// <summary>
    /// Inflate the current object by loading all properties from the data store. If the object is already inflated (<see cref="IsInflated" />=true), no action is taken.
    /// </summary>
    void Inflate();

    /// <summary>
    /// Gets a value indicating whether the specified <paramref name="metaDef" />
    /// applies to the current gallery object.
    /// </summary>
    /// <param name="metaDef">The metadata definition.</param>
    /// <returns><c>true</c> when the specified metadata item should be displayed; otherwise <c>false</c>.</returns>
    bool MetadataDefinitionApplies(IMetadataDefinition metaDef);

    /// <summary>
    /// Set the parent of this gallery object to an instance of NullGalleryObject.
    /// </summary>
    void SetParentToNullObject();

    /// <summary>
    /// Move the current object to the specified destination album. This method moves the physical files associated with this
    /// object to the destination album's physical directory. The objects <see cref="Save" /> method is invoked to persist the changes to the
    /// data store. When moving albums, all the album's children, grandchildren, etc are also moved.
    /// </summary>
    /// <param name="destinationAlbum">The album to which the current object should be moved.</param>
    void MoveTo(IAlbum destinationAlbum);

    /// <summary>
    /// Copy the current object and places it in the specified destination album. This method creates a completely separate copy
    /// of the original, including copying the physical files associated with this object. The copy is persisted to the data 
    /// store and then returned to the caller. When copying albums, all the album's children, grandchildren, etc are copied,
    /// and any role permissions that are explicitly assigned to the source album are copied to the destination album, unless
    /// the copied album inherits the role throught the destination parent album. Inherited role permissions are not copied.
    /// </summary>
    /// <param name="destinationAlbum">The album to which the current object should be copied.</param>
    /// <param name="userName">The user name of the currently logged on user. This will be used for the audit fields of the
    /// copied objects.</param>
    /// <returns>Returns a new gallery object that is an exact copy of the original, except that it resides in the specified
    /// destination album, and of course has a new ID. Child objects are recursively copied.</returns>
    IGalleryObject CopyTo(IAlbum destinationAlbum, string userName);

    /// <summary>
    /// Build the set of metadata for the current gallery object and assign to the <see cref="MetadataItems" />
    /// property.
    /// </summary>
    void ExtractMetadata();

    /// <summary>
    /// Extract the meta property <paramref name="metaDef" /> from the current gallery object and assign to the <see cref="MetadataItems" />
    /// property.
    /// </summary>
    /// <param name="metaDef">The meta definition.</param>
    void ExtractMetadata(IMetadataDefinition metaDef);

    /// <summary>
    /// Creates a metadata item for the current gallery object. The parameter <paramref name="metaDef" />
    /// contains the template and display name to use. Guaranteed to not return null.
    /// </summary>
    /// <param name="metaDef">The metadata definition.</param>
    /// <returns>An instance of <see cref="IGalleryObjectMetadataItem" />.</returns>
    IGalleryObjectMetadataItem CreateMetaItem(IMetadataDefinition metaDef);

    /// <summary>
    /// Calculates the actual rotation amount that must be applied based on the user's requested rotation 
    /// and the file's actual orientation.
    /// </summary>
    /// <returns>An instance of <see cref="MediaAssetRotateFlip" />.</returns>
    MediaAssetRotateFlip CalculateNeededRotation();

    /// <summary>
    /// Gets the orientation of the original media file. The value is retrieved from the metadata value for 
    /// <see cref="MetadataItemName.Orientation" />. Returns <see cref="Orientation.None" /> if no orientation 
    /// metadata is found, which will be the case for any media file not having orientation metadata embedded
    /// in the media file.
    /// </summary>
    /// <returns>An instance of <see cref="Orientation" />.</returns>
    Orientation GetOrientation();
  }
}
