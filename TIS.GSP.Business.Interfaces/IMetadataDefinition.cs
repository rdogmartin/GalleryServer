using System;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Represents the definition of a type of metadata that is associated with media objects. Note that this is not an actual
  /// piece of metadata, but rather defines the behavior of metadata stored in <see cref="IGalleryObjectMetadataItem" />.
  /// </summary>
  public interface IMetadataDefinition : IComparable<IMetadataDefinition>
  {
    /// <summary>
    /// Gets or sets the name of the metadata item.
    /// </summary>
    /// <value>The metadata item.</value>
    MetadataItemName MetadataItem { get; set; }

    /// <summary>
    /// Gets the string representation of the <see cref="MetadataItem" /> property.
    /// </summary>
    /// <value>A string.</value>
    string Name { get; }

    /// <summary>
    /// Gets or sets the user-friendly name to apply to this metadata item.
    /// </summary>
    /// <value>A string.</value>
    string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether metadata items of this type are visible for albums.
    /// </summary>
    /// <value><c>true</c> if metadata items of this type are visible for albums; otherwise, <c>false</c>.</value>
    bool IsVisibleForAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether metadata items of this type are visible for gallery objects.
    /// </summary>
    /// <value><c>true</c> if metadata items of this type are visible for gallery objects; otherwise, <c>false</c>.</value>
    bool IsVisibleForGalleryObject { get; set; }

    /// <summary>
    /// Gets a value indicating whether an administrator has specified this metadata item can be edited by a user. This property 
    /// is calculated from the <see cref="UserEditMode" /> property. This value does not indicate whether the logged in user has 
    /// permission to edit the album or media object. It is expected additional code will check that separately before allowing an edit.
    /// </summary>
    /// <value><c>true</c> if this metadata item can be edited by the user; otherwise, <c>false</c>.</value>
    bool IsEditable { get; }

    /// <summary>
    /// Gets or sets whether an administrator has specified this metadata item can be edited by a user and, if so, the type of
    /// editor to use.
    /// </summary>
    /// <value>The user edit mode.</value>
    PropertyEditorMode UserEditMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to persist this meta item to the media file during a save operation.
    /// When <c>true</c>, meta properties are persisted to the file. When <c>false</c>, they are not persisted and cannot
    /// be set to <c>true</c>. When null, they are not currently persisted but could be set to <c>true</c>.
    /// </summary>
    /// <value><c>true</c> if this meta item should be persisted to the media file, <c>false</c> if it cannot be
    /// persisted, or null when it is currently not persisted but could be changed to <c>true</c>.</value>
    bool? PersistToFile { get; set; }

    /// <summary>
    /// Gets a value indicating whether this meta item is capable of being persisted to the original media file. The
    /// application hard-codes this capability. This property is primarily used for ensuring data integrity and for
    /// validating whether the user can set <see cref="PersistToFile" /> to <c>true</c> or null.
    /// </summary>
    /// <value><c>true</c> if this meta item is persistable; otherwise, <c>false</c>.</value>
    bool IsPersistable { get; }

    /// <summary>
    /// Gets or sets the template to use when adding a metadata item for a new album or media object.
    /// Values of the <see cref="MetadataItemName" /> can be used as replacement parameters.
    /// Example: "{IsoSpeed} - {LensAperture}"
    /// </summary>
    /// <value>A string.</value>
    string DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the order this metadata item is to be displayed in relation to other metadata items.
    /// </summary>
    /// <value>The order this metadata item is to be displayed in relation to other metadata items.</value>
    int Sequence { get; set; }

    /// <summary>
    /// Gets the data type of the metadata item. Returns either <see cref="DateTime" /> or <see cref="System.String" />.
    /// </summary>
    /// <value>The type of the metadata item.</value>
    Type DataType { get; }
  }
}