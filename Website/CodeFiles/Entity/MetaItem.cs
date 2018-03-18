using GalleryServer.Business;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// A client-optimized object that stores a piece of information describing a gallery object.
  /// </summary>
  public class MetaItem
  {
    /// <summary>
    /// Gets the unique ID for this instance. Maps to MetadataId in the Metadata table.
    /// </summary>
    /// <value>An integer</value>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value that indentifies the media object or album this instance is associated with.
    /// Refer to <see cref="GTypeId" /> to determine which type of ID it is.
    /// </summary>
    /// <value>The value that uniquely indentifies the media object or album this instance is associated with.</value>
    public int MediaId { get; set; }

    /// <summary>
    /// Gets a value that uniquely identifies the type of metadata item for this instance
    /// (e.g. Filename, date picture taken, etc). The value maps to the numerical value of the
    /// <see cref="MetadataItemName" /> enumeration, which also maps to MetaName in the Metadata table.
    /// </summary>
    /// <value>An integer</value>
    public int MTypeId { get; set; }

    /// <summary>
    /// Gets a value that identifies the type of gallery item this instance describes. (e.g. album, image, etc).
    /// The value maps to the numerical value of the <see cref="GalleryObjectType" /> enumeration.
    /// </summary>
    /// <value>An integer</value>
    public int GTypeId { get; set; }

    /// <summary>
    /// Gets the description of the metadata item. Examples: "File name", "Date picture taken"
    /// </summary>
    /// <value>A string.</value>
    public string Desc { get; set; }

    /// <summary>
    /// Gets the value of the metadata item. Examples: "MyImageFilename.jpg", "Jan 30, 2014 9:38:21 AM"
    /// </summary>
    /// <value>A string.</value>
    public string Value { get; set; }


    /// <summary>
    /// Indicates the type of editor, if any, to use for editing this instance.
    /// </summary>
    /// <value>An instance of <see cref="PropertyEditorMode" />.</value>
    public PropertyEditorMode EditMode { get; set; }
  }

  /// <summary>
  /// A client-optimized object that wraps a meta item and the gallery items it applies to.
  /// </summary>
  public class GalleryItemMeta
  {
    /// <summary>
    /// An array of <see cref="GalleryItem" /> instances.
    /// </summary>
    /// <value>The gallery items.</value>
    public GalleryItem[] GalleryItems { get; set; }

    /// <summary>
    /// Gets or sets the meta item that applies to <see cref="GalleryItems" />.
    /// </summary>
    /// <value>The meta item.</value>
    public MetaItem MetaItem { get; set; }

    /// <summary>
    /// Gets or sets information about an action applied to this instance (e.g. when saving).
    /// </summary>
    public ActionResult ActionResult { get; set; }
  }
}