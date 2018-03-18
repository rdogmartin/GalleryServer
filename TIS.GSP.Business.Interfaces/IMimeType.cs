using System;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Represents a mime type associated with a file's extension.
  /// </summary>
  public interface IMimeType
  {
    /// <summary>
    /// Gets or sets the value that uniquely identifies this MIME type. Each application has a master list of MIME types it works with;
    /// this value identifies that MIME type.
    /// </summary>
    /// <value>The MIME type ID.</value>
    int MimeTypeId { get; set; }

    /// <summary>
    /// Gets or sets the value that uniquely identifies the MIME type that applies to a particular gallery. This value is <see cref="Int32.MinValue" />
    /// when the current instance is an application-level MIME type and not associated with a particular gallery. In this case, 
    /// <see cref="IMimeType.GalleryId" /> will also be <see cref="Int32.MinValue" />.
    /// </summary>
    /// <value>The value that uniquely identifies the MIME type that applies to a particular gallery.</value>
    int MimeTypeGalleryId { get; set; }

    /// <summary>
    /// Gets or sets the gallery ID this MIME type is associated with. May be <see cref="Int32.MinValue" /> when the instance is not
    /// assocated with a particular gallery.
    /// </summary>
    /// <value>The gallery ID this MIME type is associated with.</value>
    int GalleryId { get; set; }

    /// <summary>
    /// Gets the file extension this mime type is associated with, including the period (e.g. ".jpg", ".avi").
    /// </summary>
    /// <value>The file extension this mime type is associated with.</value>
    string Extension { get; }

    /// <summary>
    /// Gets or set the full mime type. This is the <see cref="MajorType" /> concatenated with the <see cref="Subtype" />, with a '/' between them
    /// (e.g. image/jpeg, video/quicktime).
    /// </summary>
    /// <value>The full mime type.</value>
    string FullType { get; set; }

    /// <summary>
    /// Gets the subtype this mime type is associated with (e.g. jpeg, quicktime).
    /// </summary>
    /// <value>The subtype this mime type is associated with (e.g. jpeg, quicktime).</value>
    string Subtype { get; }

    /// <summary>
    /// Gets the major type this mime type is associated with (e.g. image, video).
    /// </summary>
    /// <value>The major type this mime type is associated with (e.g. image, video).</value>
    string MajorType { get; }

    /// <summary>
    /// Gets the type category this mime type is associated with (e.g. image, video, other).
    /// </summary>
    /// <value>The type category this mime type is associated with (e.g. image, video, other).</value>
    MimeTypeCategory TypeCategory { get; }

    /// <summary>
    /// Gets the MIME type that should be sent to the browser. In most cases this is the same as the <see cref="FullType" />,
    /// but in some cases is different. For example, the MIME type for a .wav file is audio/wav, but the browser requires a 
    /// value of application/x-mplayer2.
    /// </summary>
    /// <value>The MIME type that should be sent to the browser.</value>
    string BrowserMimeType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether objects of this MIME type can be added to Gallery Server.
    /// </summary>
    /// <value><c>true</c> if objects of this MIME type can be added to Gallery Server; otherwise, <c>false</c>.</value>
    bool AllowAddToGallery { get; set; }

    /// <summary>
    /// Gets the collection of media templates for the current MIME type.
    /// </summary>
    /// <value>The media templates for the current MIME type.</value>
    IMediaTemplateCollection MediaTemplates { get; }

    /// <summary>
    /// Creates a deep copy of this instance.
    /// </summary>
    /// <returns>Returns a deep copy of this instance.</returns>
    IMimeType Copy();

    /// <summary>
    /// Persist this instance to the data store, updating an existing one or adding a new one as required.
    /// </summary>
    void Save();

    /// <summary>
    /// Gets the most specific <see cref="IMediaTemplate" /> item that matches one of the <paramref name="browserIds" />, or 
    /// null if no match is found. This method loops through each of the browser IDs in <paramref name="browserIds" />, 
    /// starting with the most specific item, and looks for a match in the current collection.
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
    /// is found. If no match is found, a null is returned.
    /// </example>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="browserIds" /> does not have any items.</exception>
    IMediaTemplate GetMediaTemplate(Array browserIds);

    /// <summary>
    /// Permanently delete this MIME type from the data store.
    /// </summary>
    void Delete();
  }
}