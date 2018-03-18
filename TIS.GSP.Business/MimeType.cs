using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a mime type associated with a file's extension.
  /// </summary>
  [DebuggerDisplay("{_majorType}/{_subtype} ({_extension}, Gallery ID = {_galleryId})")]
  public class MimeType : IMimeType
  {
    #region Private Fields

    private int _mimeTypeId;
    private int _mimeTypeGalleryId;
    private int _galleryId;
    private readonly string _extension;
    private readonly MimeTypeCategory _typeCategory;
    private string _majorType;
    private string _subtype;
    private bool _allowAddToGallery;
    private readonly string _browserMimeType;
    private readonly IMediaTemplateCollection _mediaTemplates = new MediaTemplateCollection();

    private static readonly object _sharedLock = new object();

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MimeType"/> class.
    /// </summary>
    /// <param name="mimeTypeId">The value that uniquely identifies the MIME type.</param>
    /// <param name="mimeTypeGalleryId">The value that uniquely identifies the MIME type that applies to a particular gallery.</param>
    /// <param name="galleryId">The gallery ID. Specify <see cref="Int32.MinValue"/> if creating an instance that is not
    /// specific to a particular gallery.</param>
    /// <param name="fileExtension">A string representing the file's extension, including the period (e.g. ".jpg", ".avi").
    /// It is not case sensitive.</param>
    /// <param name="mimeTypeValue">The full mime type. This is the <see cref="MajorType"/> concatenated with the <see cref="Subtype"/>,
    /// with a '/' between them (e.g. image/jpeg, video/quicktime).</param>
    /// <param name="browserMimeType">The MIME type that can be understood by the browser for displaying this media object.  Specify null or
    /// <see cref="String.Empty"/> if the MIME type appropriate for the browser is the same as <paramref name="mimeTypeValue"/>.</param>
    /// <param name="allowAddToGallery">Indicates whether a file having this MIME type can be added to Gallery Server.
    /// This parameter is only relevant when a valid <paramref name="galleryId"/> is specified.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fileExtension" /> or <paramref name="mimeTypeValue" /> is
    /// null or an empty string.</exception>
    private MimeType(int mimeTypeId, int mimeTypeGalleryId, int galleryId, string fileExtension, string mimeTypeValue, string browserMimeType, bool allowAddToGallery)
    {
      #region Validation

      if (String.IsNullOrEmpty(fileExtension))
        throw new ArgumentOutOfRangeException("fileExtension", "Parameter cannot be null or empty.");

      if (String.IsNullOrEmpty(mimeTypeValue))
        throw new ArgumentOutOfRangeException("mimeTypeValue", "Parameter cannot be null or empty.");

      // If browserMimeType is specified, it better be valid.
      if (!String.IsNullOrEmpty(browserMimeType))
      {
        ValidateMimeType(browserMimeType);
      }

      // Validate fullMimeType and separate it into its major and sub types.
      string majorType;
      string subType;
      ValidateMimeType(mimeTypeValue, out majorType, out subType);

      #endregion

      this._mimeTypeId = mimeTypeId;
      this._mimeTypeGalleryId = mimeTypeGalleryId;
      this._galleryId = galleryId;
      this._extension = fileExtension;
      this._typeCategory = MimeTypeEnumHelper.ParseMimeTypeCategory(majorType);
      this._majorType = majorType;
      this._subtype = subType;
      this._browserMimeType = (String.IsNullOrEmpty(browserMimeType) ? mimeTypeValue : browserMimeType);
      this._allowAddToGallery = allowAddToGallery;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MimeType"/> class with the specified MIME type category and <paramref name="fileExtension" />.
    /// The <see cref="MajorType" /> property is assigned the string representation of the <paramref name="mimeType"/>. Remaining properties
    /// are set to empty strings or false  (<see cref="AllowAddToGallery" />). This constructor can be used to help describe an external 
    /// media asset, which is not represented by a locally stored file but for which it is useful to describe its general type (audio, video, etc).
    /// </summary>
    /// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
    /// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
    /// <param name="fileExtension">The file extension. Specify an empty string if no file extension is appropriate. (e.g. ".jpg")</param>
    private MimeType(MimeTypeCategory mimeType, string fileExtension)
    {
      this._galleryId = Int32.MinValue;
      this._typeCategory = mimeType;
      this._majorType = mimeType.ToString();
      this._extension = fileExtension;
      this._subtype = String.Empty;
      this._browserMimeType = String.Empty;
      this._allowAddToGallery = false;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the value that uniquely identifies this MIME type. Each application has a master list of MIME types it works with;
    /// this value identifies that MIME type.
    /// </summary>
    /// <value>The MIME type ID.</value>
    public int MimeTypeId
    {
      get { return _mimeTypeId; }
      set { _mimeTypeId = value; }
    }

    /// <summary>
    /// Gets or sets the value that uniquely identifies the MIME type that applies to a particular gallery. This value is <see cref="Int32.MinValue" />
    /// when the current instance is an application-level MIME type and not associated with a particular gallery. In this case, 
    /// <see cref="IMimeType.GalleryId" /> will also be <see cref="Int32.MinValue" />.
    /// </summary>
    /// <value>The value that uniquely identifies the MIME type that applies to a particular gallery.</value>
    public int MimeTypeGalleryId
    {
      get { return _mimeTypeGalleryId; }
      set { _mimeTypeGalleryId = value; }
    }

    /// <summary>
    /// Gets or sets the gallery ID this MIME type is associated with. May be <see cref="Int32.MinValue"/> when the instance is not
    /// assocated with a particular gallery.
    /// </summary>
    /// <value>The gallery ID this MIME type is associated with.</value>
    public int GalleryId
    {
      get
      {
        return this._galleryId;
      }
      set
      {
        this._galleryId = value;
      }
    }

    /// <summary>
    /// Gets the file extension this mime type is associated with, including the period (e.g. ".jpg", ".avi").
    /// </summary>
    /// <value>The file extension this mime type is associated with.</value>
    public string Extension
    {
      get
      {
        return this._extension;
      }
    }

    /// <summary>
    /// Gets the type category this mime type is associated with (e.g. image, video, other).
    /// </summary>
    /// <value>
    /// The type category this mime type is associated with (e.g. image, video, other).
    /// </value>
    public MimeTypeCategory TypeCategory
    {
      get
      {
        return this._typeCategory;
      }
    }

    /// <summary>
    /// Gets the MIME type that should be sent to the browser. In most cases this is the same as the <see cref="IMimeType.FullType" />,
    /// but in some cases is different. For example, the MIME type for a .wav file is audio/wav, but the browser requires a 
    /// value of application/x-mplayer2.
    /// </summary>
    /// <value>The MIME type that should be sent to the browser.</value>
    public string BrowserMimeType
    {
      get
      {
        return this._browserMimeType;
      }
    }

    /// <summary>
    /// Gets the major type this mime type is associated with (e.g. image, video).
    /// </summary>
    /// <value>
    /// The major type this mime type is associated with (e.g. image, video).
    /// </value>
    public string MajorType
    {
      get
      {
        return this._majorType;
      }
    }

    /// <summary>
    /// Gets the subtype this mime type is associated with (e.g. jpeg, quicktime).
    /// </summary>
    /// <value>
    /// The subtype this mime type is associated with (e.g. jpeg, quicktime).
    /// </value>
    public string Subtype
    {
      get
      {
        return this._subtype;
      }
    }

    /// <summary>
    /// Gets or sets the full mime type. This is the <see cref="MajorType"/> concatenated with the <see cref="Subtype"/>, with a '/' between them
    /// (e.g. image/jpeg, video/quicktime).
    /// </summary>
    /// <value>The full mime type.</value>
    public string FullType
    {
      get
      {
        return String.Format(CultureInfo.CurrentCulture, "{0}/{1}", this._majorType.ToLower(CultureInfo.CurrentCulture), this._subtype);
      }
      set
      {
        string majorType, subType;
        ValidateMimeType(value, out majorType, out subType);

        this._majorType = majorType;
        this._subtype = subType;

      }
    }

    /// <summary>
    /// Gets a value indicating whether objects of this MIME type can be added to Gallery Server.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if objects of this MIME type can be added to Gallery Server; otherwise, <c>false</c>.
    /// </value>
    public bool AllowAddToGallery
    {
      get
      {
        return this._allowAddToGallery;
      }
      set
      {
        this._allowAddToGallery = value;
      }
    }

    /// <summary>
    /// Gets the collection of media templates for the current MIME type.
    /// </summary>
    /// <value>The media templates for the current MIME type.</value>
    public IMediaTemplateCollection MediaTemplates
    {
      get { return _mediaTemplates; }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates a deep copy of this instance.
    /// </summary>
    /// <returns>Returns a deep copy of this instance.</returns>
    public IMimeType Copy()
    {
      IMimeType copy = new MimeType(this.MimeTypeId, this.MimeTypeGalleryId, this.GalleryId, this.Extension, this.FullType, this.BrowserMimeType, this.AllowAddToGallery);

      if (this.MediaTemplates.Count > 0)
      {
        copy.MediaTemplates.AddRange(this.MediaTemplates.Copy());
      }

      return copy;
    }

    /// <summary>
    /// Persist this instance to the data store, updating an existing one or adding a new one as required.
    /// </summary>
    public void Save()
    {
      if (MimeTypeId == int.MinValue)
      {
        SaveNew();
      }
      else
      {
        SaveExisting();
      }
    }

    /// <summary>
    /// Permanently delete this MIME type from the data store.
    /// </summary>
    public void Delete()
    {
      if (MimeTypeId == int.MinValue)
      {
        return;
      }

      using (var repo = new MimeTypeRepository())
      {
        var mtDto = repo.Find(MimeTypeId);
        if (mtDto != null)
        {
          repo.Delete(mtDto);
          repo.Save();
        }
      }
    }

    /// <summary>
    /// Gets the most specific <see cref="IMediaTemplate" /> item that matches one of the <paramref name="browserIds" />. This 
    /// method loops through each of the browser IDs in <paramref name="browserIds" />, starting with the most specific item, and 
    /// looks for a match in the current collection. This method is guaranteed to return a <see cref="IMediaTemplate" /> object, 
    /// provided the collection, at the very least, contains a browser element with id = "default".
    /// </summary>
    /// <param name="browserIds">A <see cref="System.Array"/> of browser ids for the current browser. This is a list of strings,
    /// ordered from most general to most specific, that represent the various categories of browsers the current
    /// browser belongs to. This is typically populated by calling ToArray() on the Request.Browser.Browsers property.
    /// </param>
    /// <returns>The <see cref="IMediaTemplate" /> that most specifically matches one of the <paramref name="browserIds" />; 
    /// otherwise, a null reference.</returns>
    /// <example>During a request where the client is Firefox, the Request.Browser.Browsers property returns an ArrayList with these 
    /// five items: default, mozilla, gecko, mozillarv, and mozillafirefox. This method starts with the most specific item 
    /// (mozillafirefox) and looks in the current collection for an item with this browser ID. If a match is found, that item 
    /// is returned. If no match is found, the next item (mozillarv) is used as the search parameter.  This continues until a match 
    /// is found. Since there should always be a browser element with id="default", there will always - eventually - be a match.
    /// </example>
    public IMediaTemplate GetMediaTemplate(Array browserIds)
    {
      return MediaTemplates.Find(browserIds);
    }

    #endregion

    #region Public static methods

    /// <summary>
    /// Initializes a new instance of the <see cref="MimeType" /> class with the specified MIME type category and <paramref name="fileExtension" />.
    /// The <see cref="MajorType" /> property is assigned the string representation of the <paramref name="mimeType" />. Remaining properties 
    /// are set to empty strings or false (<see cref="AllowAddToGallery" />). This method can be used to help describe an external media asset,
    /// which is not represented by a locally stored file but for which it is useful to describe its general type (audio, video, etc).
    /// </summary>
    /// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of
    /// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
    /// <param name="fileExtension">The file extension. Specify an empty string if no file extension is appropriate. (e.g. ".jpg")</param>
    /// <returns>Returns a new instance of <see cref="IMimeType" />.</returns>
    public static IMimeType CreateInstance(MimeTypeCategory mimeType, string fileExtension)
    {
      return new MimeType(mimeType, fileExtension);
    }

    /// <summary>
    /// Loads the collection of MIME types for the specified <paramref name="galleryId" /> from the data store.
    /// When <paramref name="galleryId" /> is <see cref="Int32.MinValue" />, a generic collection that is not 
    /// specific to a particular gallery is returned.
    /// </summary>
    /// <param name="galleryId">The gallery ID. Specify <see cref="Int32.MinValue" /> to retrieve a generic 
    /// collection that is not specific to a particular gallery.</param>
    /// <returns>Returns a <see cref="IMimeTypeCollection" /> containing MIME types for the specified 
    /// <paramref name="galleryId" /></returns>
    public static IMimeTypeCollection LoadMimeTypes(int galleryId)
    {
      var mimeTypes = LoadMimeTypesFromDataStore();

      if (galleryId == Int32.MinValue)
      {
        // User wants the master list. Load from data store and return (this also adds it to the static var for next time).
        return mimeTypes;
      }

      // User wants the MIME types for a specific gallery that we haven't yet loaded from disk. Do so now.
      if (ConfigureMimeTypesForGallery(mimeTypes, galleryId))
      {
        return mimeTypes;
      }

      // If we get here then no records existed in the data store for the gallery MIME types (gsp.MimeTypeGallery). Validate
      // the gallery, which will create these records while not harming any pre-existing records that may exist in other
      // tables such as gsp.GallerySetting.
      Factory.LoadGallery(galleryId).Validate();

      // Note: If CreateGallery() fails to create records in gs_MimeTypeGallery, we will end up in an infinite loop.
      // But that should never happen, right?
      return LoadMimeTypes(galleryId);
    }

    #endregion

    #region Private methods

    private static void ValidateMimeType(string fullMimeType)
    {
      string majorType;
      string subType;
      ValidateMimeType(fullMimeType, out majorType, out subType);
    }

    private static void ValidateMimeType(string fullMimeType, out string majorType, out string subType)
    {
      int slashLocation = fullMimeType.IndexOf("/", StringComparison.Ordinal);
      if (slashLocation < 0)
      {
        throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.MimeType_Ctor_Ex_Msg, fullMimeType), nameof(fullMimeType));
      }

      majorType = fullMimeType.Substring(0, slashLocation);
      subType = fullMimeType.Substring(slashLocation + 1);

      if ((String.IsNullOrEmpty(majorType)) || (String.IsNullOrEmpty(subType)))
      {
        throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.MimeType_Ctor_Ex_Msg, fullMimeType), nameof(fullMimeType));
      }
    }

    /// <summary>
    /// Updates the <paramref name="baseMimeTypes" /> with configuration data for the <paramref name="galleryId" />.
    /// Returns <c>true</c> when at least one record in the <see cref="MimeTypeGalleryDto" /> table exists for the 
    /// <paramref name="galleryId" />; otherwise returns <c>false</c>.
    /// </summary>
    /// <param name="baseMimeTypes">A collection of MIME types to be updated with gallery-specific data.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>Returns <c>true</c> when at least one record in the <see cref="MimeTypeGalleryDto" /> table exists for the 
    /// <paramref name="galleryId" />; otherwise returns <c>false</c>.</returns>
    private static bool ConfigureMimeTypesForGallery(IMimeTypeCollection baseMimeTypes, int galleryId)
    {
      //IMimeTypeCollection baseMimeTypes = LoadMimeTypes(Int32.MinValue);
      //IMimeTypeCollection newMimeTypes = new MimeTypeCollection();
      var mediaTemplates = Factory.LoadMediaTemplates();

      var foundRows = false;
      using (var repo = new MimeTypeGalleryRepository())
      {
        foreach (var mtgDto in repo.Where(m => m.FKGalleryId == galleryId, m => m.MimeType))
        {
          foundRows = true;
          var mimeType = baseMimeTypes.Find(mtgDto.MimeType.FileExtension);

          if (mimeType == null)
          {
            throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Could not find a IMimeType with file extension \"{0}\" in the list of base MIME types.", mtgDto.MimeType.FileExtension));
          }

          mimeType.GalleryId = galleryId;
          mimeType.MimeTypeGalleryId = mtgDto.MimeTypeGalleryId;
          mimeType.AllowAddToGallery = mtgDto.IsEnabled;

          // Populate the media template collection.
          mimeType.MediaTemplates.AddRange(mediaTemplates.Find(mimeType));

          // Validate the media templates. There may not be any, which is OK (for example, there isn't one defined for 'application/msword').
          // But if there *IS* one defined, there must be one with a browser ID of "default".
          if ((mimeType.MediaTemplates.Count > 0) && (mimeType.MediaTemplates.Find("default") == null))
          {
            throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "No default media template. Could not find a media template for MIME type \"{0}\" or \"{1}\" with browser ID = \"default\".", mimeType.FullType, String.Concat(mimeType.MajorType, "/*")));
          }
        }
      }

      return foundRows;
    }

    /// <summary>
    /// Loads the set of MIME types from the data store. These MIME types are the master list of MIME types and are not
    /// specific to a particular gallery. That is, the <see cref="IMimeType.GalleryId" /> property is set to <see cref="Int32.MinValue" />
    /// and the <see cref="IMimeType.AllowAddToGallery" /> property is <c>false</c> for all items.
    /// </summary>
    /// <returns>Returns a <see cref="IMimeTypeCollection" /> containing MIME types..</returns>
    /// <exception cref="BusinessException">Thrown when no records were found in the master list of MIME types in the data store.</exception>
    private static IMimeTypeCollection LoadMimeTypesFromDataStore()
    {
      IMimeTypeCollection baseMimeTypes = new MimeTypeCollection();

      using (var repo = new MimeTypeRepository())
      {
        foreach (var mimeTypeDto in repo.GetAll().OrderBy(m => m.FileExtension))
        {
          baseMimeTypes.Add(new MimeType(mimeTypeDto.MimeTypeId, Int32.MinValue, Int32.MinValue, mimeTypeDto.FileExtension.Trim(), mimeTypeDto.MimeTypeValue.Trim(), mimeTypeDto.BrowserMimeTypeValue.Trim(), false));
        }
      }

      if (baseMimeTypes.Count == 0)
      {
        throw new BusinessException("No records were found in the master list of MIME types in the data store.");
      }

      return baseMimeTypes;
    }

    /// <summary>
    /// Create a new MimeType and MimeTypeGallery record for this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="MimeTypeId" /> is any value other than <see cref="Int32.MinValue" /></exception>
    private void SaveNew()
    {
      if (MimeTypeId != int.MinValue)
        throw new InvalidOperationException("Cannot call SaveNew when the MIME type already exists in the data store.");

      int mimeTypeId;
      using (var repo = new MimeTypeRepository())
      {
        var mimeTypeDto = new MimeTypeDto() { FileExtension = Extension, MimeTypeValue = FullType, BrowserMimeTypeValue = BrowserMimeType };
        repo.Add(mimeTypeDto);
        repo.Save();

        mimeTypeId = mimeTypeDto.MimeTypeId;
      }

      using (var repo = new MimeTypeGalleryRepository())
      {
        repo.Add(new MimeTypeGalleryDto() { FKGalleryId = GalleryId, FKMimeTypeId = mimeTypeId, IsEnabled = AllowAddToGallery });
        repo.Save();
      }

      // Clear the gallery cache. This will eventually trigger Gallery.Configure(), which will ensure that all galleries have a MimeTypeGallery record for 
      // this MIME type.
      Factory.ClearGalleryCache();
    }

    /// <summary>
    /// Updates the data store records with the values of this instance.
    /// </summary>
    private void SaveExisting()
    {
      if (MimeTypeId == int.MinValue)
        throw new InvalidOperationException("Cannot call SaveExisting for new MIME types.");

      using (var repo = new MimeTypeRepository())
      {
        var mtDto = repo.Find(MimeTypeId);
        if (mtDto != null && mtDto.MimeTypeValue != FullType)
        {
          mtDto.MimeTypeValue = FullType;
          repo.Save();
        }
      }

      using (var repo = new MimeTypeGalleryRepository())
      {
        var mtDto = repo.Find(MimeTypeGalleryId);
        if (mtDto != null && mtDto.IsEnabled != AllowAddToGallery)
        {
          mtDto.IsEnabled = AllowAddToGallery;
          repo.Save();
        }
      }
    }

    #endregion
  }
}
