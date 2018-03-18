using System;
using System.Diagnostics;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
  /// <summary>
  /// Represents the definition of a type of metadata that is associated with media objects. Note that this is not an actual
  /// piece of metadata, but rather defines the behavior of metadata stored in <see cref="IGalleryObjectMetadataItem" />.
  /// </summary>
  [DebuggerDisplay("\"{MetadataItem}\", VisibleForGalleryObject={IsVisibleForGalleryObject}, Seq={Sequence}, IsEditable={IsEditable}, EditMode={UserEditMode}")]
  public class MetadataDefinition : IMetadataDefinition
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataDefinition" /> class.
    /// </summary>
    /// <param name="metadataItem">The metadata item.</param>
    /// <param name="displayName">The user-friendly name that describes this metadata item (e.g. "Date picture taken")</param>
    /// <param name="isVisibleForAlbum">If set to <c>true</c> metadata items belonging to albums are visible
    /// in the user interface.</param>
    /// <param name="isVisibleForGalleryObject">If set to <c>true</c> metadata items belonging to media
    /// objects are visible in the user interface.</param>
    /// <param name="userEditMode">The user edit mode.</param>
    /// <param name="persistToFile">Indicates whether to persist this meta item to the media file during a save operation.</param>
    /// <param name="sequence">Indicates the display order of the metadata item.</param>
    /// <param name="defaultValue">The template to use when adding a metadata item for a new album or media object.</param>
    public MetadataDefinition(MetadataItemName metadataItem, string displayName, bool isVisibleForAlbum, bool isVisibleForGalleryObject, PropertyEditorMode userEditMode, bool? persistToFile, int sequence, string defaultValue)
    {
      MetadataItem = metadataItem;
      DisplayName = displayName;
      IsVisibleForAlbum = isVisibleForAlbum;
      IsVisibleForGalleryObject = isVisibleForGalleryObject;
      UserEditMode = userEditMode;
      PersistToFile = persistToFile;
      Sequence = sequence;
      DefaultValue = defaultValue;
    }

    /// <summary>
    /// Gets or sets the name of the metadata item.
    /// </summary>
    /// <value>The metadata item.</value>
    public MetadataItemName MetadataItem { get; set; }

    /// <summary>
    /// Gets the string representation of the <see cref="MetadataItem" /> property.
    /// </summary>
    /// <value>A string.</value>
    public string Name { get { return MetadataItem.ToString(); } }

    /// <summary>
    /// Gets or sets the user-friendly name to apply to this metadata item.
    /// </summary>
    /// <value>A string.</value>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether metadata items of this type are visible for albums.
    /// </summary>
    /// <value><c>true</c> if metadata items of this type are visible for albums; otherwise, <c>false</c>.</value>
    public bool IsVisibleForAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether metadata items of this type are visible in the gallery.
    /// </summary>
    /// <value><c>true</c> if metadata items of this type are visible in the gallery; otherwise, <c>false</c>.</value>
    public bool IsVisibleForGalleryObject { get; set; }

    /// <summary>
    /// Gets a value indicating whether an administrator has specified this metadata item can be edited by a user. This property 
    /// is calculated from the <see cref="UserEditMode" /> property. This value does not indicate whether the logged in user has 
    /// permission to edit the album or media object. It is expected additional code will check that separately before allowing an edit.
    /// </summary>
    /// <value><c>true</c> if this metadata item can be edited by the user; otherwise, <c>false</c>.</value>
    [Newtonsoft.Json.JsonIgnore]
    public bool IsEditable => (UserEditMode == PropertyEditorMode.PlainTextEditor || UserEditMode == PropertyEditorMode.TinyMCEHtmlEditor);

    /// <summary>
    /// Gets or sets whether an administrator has specified this metadata item can be edited by a user and, if so, the type of
    /// editor to use.
    /// </summary>
    /// <value>The user edit mode.</value>
    public PropertyEditorMode UserEditMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to persist this meta item to the media file during a save operation.
    /// When <c>true</c>, meta properties are persisted to the file. When <c>false</c>, they are not persisted and cannot
    /// be set to <c>true</c>. When null, they are not currently persisted but could be set to <c>true</c>.
    /// </summary>
    /// <value><c>true</c> if this meta item should be persisted to the media file, <c>false</c> if it cannot be
    /// persisted, or null when it is currently not persisted but could be changed to <c>true</c>.</value>
    public bool? PersistToFile { get; set; }

    /// <summary>
    /// Gets a value indicating whether this meta item is capable of being persisted to the original media file. The
    /// application hard-codes this capability. This property is primarily used for ensuring data integrity and for
    /// validating whether the user can set <see cref="PersistToFile" /> to <c>true</c> or null.
    /// </summary>
    /// <value><c>true</c> if this meta item is persistable; otherwise, <c>false</c>.</value>
    [Newtonsoft.Json.JsonIgnore]
    public bool IsPersistable
    {
      get
      {
        switch (MetadataItem)
        {
          case MetadataItemName.Author:
          case MetadataItemName.Copyright:
          case MetadataItemName.CameraModel:
          case MetadataItemName.EquipmentManufacturer:
          case MetadataItemName.Subject:
          case MetadataItemName.Title:
          case MetadataItemName.Caption:
          case MetadataItemName.DatePictureTaken:
          case MetadataItemName.Tags:
          case MetadataItemName.Rating:
          case MetadataItemName.Orientation:
          case MetadataItemName.IptcByline:
          case MetadataItemName.IptcBylineTitle:
          case MetadataItemName.IptcCaption:
          case MetadataItemName.IptcCity:
          case MetadataItemName.IptcCopyrightNotice:
          case MetadataItemName.IptcCountryPrimaryLocationName:
          case MetadataItemName.IptcCredit:
          case MetadataItemName.IptcDateCreated:
          case MetadataItemName.IptcHeadline:
          case MetadataItemName.IptcKeywords:
          case MetadataItemName.IptcObjectName:
          case MetadataItemName.IptcOriginalTransmissionReference:
          case MetadataItemName.IptcProvinceState:
            //case MetadataItemName.IptcRecordVersion: // Has to be written as 2-byte array (e.g. new byte[2] { 2, 3 }). Add support only if necessary.
          case MetadataItemName.IptcSource:
          case MetadataItemName.IptcSpecialInstructions:
          case MetadataItemName.IptcSublocation:
          case MetadataItemName.IptcWriterEditor:
            return true;

          default:
            return false; // All meta items not listed above are NEVER editable
        }
      }
    }

    /// <summary>
    /// Gets or sets the template to use when adding a metadata item for a new album or media object.
    /// Values of the <see cref="MetadataItemName" /> can be used as replacement parameters.
    /// Example: "{IsoSpeed} - {LensAperture}"
    /// </summary>
    /// <value>A string.</value>
    public string DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the order this metadata item is to be displayed in relation to other metadata items.
    /// </summary>
    /// <value>The order this metadata item is to be displayed in relation to other metadata items.</value>
    public int Sequence { get; set; }

    /// <summary>
    /// Gets the data type of the metadata item. Returns either <see cref="DateTime" /> or <see cref="System.String" />.
    /// </summary>
    /// <value>The type of the metadata item.</value>
    [Newtonsoft.Json.JsonIgnore]
    public Type DataType
    {
      get
      {
        switch (MetadataItem)
        {
          case MetadataItemName.DateAdded:
          case MetadataItemName.DateFileCreated:
          case MetadataItemName.DateFileCreatedUtc:
          case MetadataItemName.DateFileLastModified:
          case MetadataItemName.DateFileLastModifiedUtc:
          case MetadataItemName.DatePictureTaken:
            return typeof(DateTime);
          default:
            return typeof(String);
        }
      }
    }

    #region IComparable

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(IMetadataDefinition other)
    {
      if (other == null)
        return 1;
      else
      {
        return Sequence.CompareTo(other.Sequence);
      }
    }

    #endregion

    /// <summary>
    /// Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="MetadataDefinition"/>.
    /// </returns>
    public override int GetHashCode()
    {
      return MetadataItem.GetHashCode();
    }
  }
}
