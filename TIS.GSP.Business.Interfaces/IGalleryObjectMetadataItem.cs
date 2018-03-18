using System;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Represents an item of metadata for a gallery object.
  /// </summary>
  public interface IGalleryObjectMetadataItem : IComparable<IGalleryObjectMetadataItem>
  {
    /// <summary>
    /// Gets or sets a value that uniquely indentifies this metadata item.
    /// </summary>
    /// <value>The value that uniquely indentifies this metadata item.</value>
    int MediaObjectMetadataId { get; set; }

    /// <summary>
    /// Gets or sets the object this instance applies to.
    /// </summary>
    /// <value>The object this instance applies to.</value>
    IGalleryObject GalleryObject
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the description of the metadata item (e.g. "Exposure time", "Camera model"). Setting this to a new
    /// value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The description of the metadata item.</value>
    string Description { get; set; }

    /// <summary>
    /// Gets or sets the raw value of the metadata item. Typically this is the value extracted from the metadata of the
    /// media file. Setting this to a new value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The value of the metadata item.</value>
    string RawValue { get; set; }

    /// <summary>
    /// Gets or sets the value of the metadata item (e.g. "F5.7", "1/500 sec."). Setting this to a new
    /// value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The value of the metadata item.</value>
    string Value { get; set; }

    ///// <summary>
    ///// Gets or sets a value indicating whether this metadata item should be extracted from the original image file the
    ///// next time the media object is saved. This will cause the existing metadata item in the data store to be overwritten
    ///// with the new value. The default value is false.
    ///// </summary>
    ///// <value>
    ///// 	<c>true</c> if this metadata item should be extracted from the original image file the
    ///// next time the media object is saved; otherwise, <c>false</c>.
    ///// </value>
    //bool ExtractFromFileOnSave { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this object has changes that have not been persisted to the database.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
    /// </value>
    bool HasChanges { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this metadata item is visible in the UI.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this metadata item is visible in the UI; otherwise, <c>false</c>.
    /// </value>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets a value indicating whether this metadata item is defined as being editable. The value is 
    /// retrieved from the <see cref="IMetadataDefinition" /> object for this item. The calling code
    /// must also verify the user has permission to edit the album or media object.
    /// </summary>
    /// <value><c>true</c> if this metadata item is defined as being editable; otherwise, <c>false</c>.</value>
    bool IsEditable { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is to be permanently removed from the data 
    /// store.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is to be deleted the next time the gallery object is saved; 
    /// otherwise, <c>false</c>.
    /// </value>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the name of this metadata item. Setting this to a new
    /// value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The name of the metadata item.</value>
    MetadataItemName MetadataItemName { get; set; }

    /// <summary>
    /// Gets or sets the meta definition for this instance.
    /// </summary>
    /// <value>An instance of <see cref="IMetadataDefinition" />.</value>
    IMetadataDefinition MetaDefinition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to persist this meta item to the media file during a save operation. Defaults to 
    /// <see cref="IMetadataDefinition.PersistToFile" /> when not specified.
    /// </summary>
    /// <value><c>true</c> if this meta item should be persisted to the media file; otherwise, <c>false</c>.</value>
    bool PersistToFile { get; set; }

    /// <summary>
    /// Perform a deep copy of this metadata item.
    /// </summary>
    /// <returns>Returns a deep copy of this metadata item.</returns>
    IGalleryObjectMetadataItem Copy();
  }
}
