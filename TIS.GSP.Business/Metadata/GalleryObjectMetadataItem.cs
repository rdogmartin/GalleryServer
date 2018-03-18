using System;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
  /// <summary>
  /// Represents an item of metadata for a gallery object.
  /// </summary>
  [System.Diagnostics.DebuggerDisplay("{_description} = {_value}")]
  [Serializable]
  public class GalleryObjectMetadataItem : IGalleryObjectMetadataItem
  {
    #region Private Fields

    private int _mediaObjectMetadataId;
    private MetadataItemName _metadataItemName;
    private string _description;
    private string _rawValue;
    private string _value;
    private bool _hasChanges;
    private bool _isVisible;
    private bool _isDeleted;
    private bool? _persistToFile;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GalleryObjectMetadataItem" /> class.
    /// </summary>
    /// <param name="mediaObjectMetadataId">The value that uniquely indentifies this metadata item.</param>
    /// <param name="galleryObject">The gallery object this metadata item applies to.</param>
    /// <param name="rawValue">The raw value of the metadata item. Typically this is the value extracted from 
    /// the metadata of the media file.</param>
    /// <param name="value">The value of the metadata item (e.g. "F5.7", "1/500 sec.").</param>
    /// <param name="hasChanges">if set to <c>true</c> this object has changes that have not been persisted to the database.</param>
    /// <param name="metaDef">The meta definition.</param>
    public GalleryObjectMetadataItem(int mediaObjectMetadataId, IGalleryObject galleryObject, string rawValue, string value, bool hasChanges, IMetadataDefinition metaDef)
    {
      _mediaObjectMetadataId = mediaObjectMetadataId;
      GalleryObject = galleryObject;
      _metadataItemName = metaDef.MetadataItem;
      _description = metaDef.DisplayName;
      _rawValue = rawValue;
      _value = value;
      _hasChanges = hasChanges;
      MetaDefinition = metaDef;
      _isVisible = false;
      IsDeleted = false;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets a value that uniquely indentifies this metadata item.
    /// </summary>
    /// <value>The value that uniquely indentifies this metadata item.</value>
    public int MediaObjectMetadataId
    {
      get { return _mediaObjectMetadataId; }
      set { _mediaObjectMetadataId = value; }
    }

    /// <summary>
    /// Gets or sets the object this instance applies to.
    /// </summary>
    /// <value>The object this instance applies to.</value>
    public IGalleryObject GalleryObject { get; set; }

    /// <summary>
    /// Gets or sets the description of the metadata item (e.g. "Exposure time", "Camera model"). Setting this to a new
    /// value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The description of the metadata item.</value>
    public string Description
    {
      get { return _description; }
      set
      {
        if (_description != value)
        {
          _description = value;
          _hasChanges = true;
          GalleryObject.HasChanges = true;
        }
      }
    }

    /// <summary>
    /// Gets or sets the raw value of the metadata item. Typically this is the value extracted from the metadata of the
    /// media file. Setting this to a new value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The value of the metadata item.</value>
    public string RawValue
    {
      get { return _rawValue; }
      set
      {
        if (_rawValue != value)
        {
          _rawValue = value;
          _hasChanges = true;
          GalleryObject.HasChanges = true;
        }
      }
    }

    /// <summary>
    /// Gets or sets the value of the metadata item (e.g. "F5.7", "1/500 sec."). Setting this to a new
    /// value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The value of the metadata item.</value>
    public string Value
    {
      get { return _value; }
      set
      {
        if (_value != value)
        {
          _value = value;
          _hasChanges = true;
          GalleryObject.HasChanges = true;
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this metadata item is visible in the UI. Setting this to a new
    /// value does not affect <see cref="HasChanges" />.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this metadata item is visible in the UI; otherwise, <c>false</c>.
    /// </value>
    public bool IsVisible
    {
      get { return _isVisible; }
      set { _isVisible = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this metadata item is defined as being editable. The value is 
    /// retrieved from the <see cref="IMetadataDefinition" /> object for this item. The calling code
    /// must also verify the user has permission to edit the album or media object.
    /// </summary>
    /// <value><c>true</c> if this metadata item is defined as being editable; otherwise, <c>false</c>.</value>
    public bool IsEditable
    {
      get
      {
        return Factory.LoadGallerySetting(GalleryObject.GalleryId).MetadataDisplaySettings.Find(MetadataItemName).IsEditable;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is to be permanently removed from the data 
    /// store.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is to be deleted the next time the gallery object is saved; 
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsDeleted
    {
      get { return _isDeleted; }
      set
      {
        if (_isDeleted != value)
        {
          _isDeleted = value;
          _hasChanges = true;
          GalleryObject.HasChanges = true;
        }
      }
    }

    /// <summary>
    /// Gets or sets the name of this metadata item. Setting this to a new
    /// value causes <see cref="HasChanges" /> to be <c>true</c>.
    /// </summary>
    /// <value>The name of the metadata item.</value>
    public MetadataItemName MetadataItemName
    {
      get { return _metadataItemName; }
      set
      {
        if (_metadataItemName != value)
        {
          _metadataItemName = value;
          _hasChanges = true;
          GalleryObject.HasChanges = true;
        }
      }
    }

    /// <summary>
    /// Gets or sets the meta definition for this instance.
    /// </summary>
    /// <value>An instance of <see cref="IMetadataDefinition" />.</value>
    public IMetadataDefinition MetaDefinition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to persist this meta item to the media file during a save operation. Defaults to 
    /// <see cref="IMetadataDefinition.PersistToFile" /> when not specified.
    /// </summary>
    /// <value><c>true</c> if this meta item should be persisted to the media file; otherwise, <c>false</c>.</value>
    public bool PersistToFile
    {
      get
      {
        return _persistToFile.GetValueOrDefault(MetaDefinition.PersistToFile.GetValueOrDefault());
      }
      set { _persistToFile = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this object has changes that have not been persisted to the database.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
    /// </value>
    public bool HasChanges
    {
      get { return _hasChanges; }
      set { _hasChanges = value; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Perform a deep copy of this metadata item.
    /// </summary>
    /// <returns>
    /// Returns a deep copy of this metadata item.
    /// </returns>
    public IGalleryObjectMetadataItem Copy()
    {
      return Factory.CreateMetadataItem(int.MinValue, GalleryObject, RawValue, Value, true, MetaDefinition);
    }

    #endregion

    #region IComparable

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
    /// </returns>
    public int CompareTo(IGalleryObjectMetadataItem other)
    {
      if (other == null)
        return 1;
      else
      {
        return String.Compare(Description, other.Description, StringComparison.CurrentCulture);
      }
    }

    #endregion

    /// <summary>
    /// Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="GalleryObjectMetadataItem"/>.
    /// </returns>
    public override int GetHashCode()
    {
      return ((IGalleryObjectMetadataItem)this).MetadataItemName.GetHashCode();
    }

  }
}
