using System.Collections.Generic;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a piece of metadata about a gallery object that is suitable for storing in cache.
  /// </summary>
  [System.Diagnostics.DebuggerDisplay("Meta {MetaName} = {Value}")]
	public class CacheItemMetaItem
	{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheItemMetaItem"/> class.
    /// </summary>
    /// <param name="metadataId">The ID of the metadata property.</param>
    /// <param name="metaName">The name/type of the metadata property.</param>
    /// <param name="mediaObjectId">The ID of the media object this meta property belongs to. Specify null when it applies to an album.</param>
    /// <param name="albumId">The ID of the album this meta property belongs to. Specify null when it applies to a media object.</param>
    /// <param name="rawValue">The raw value of the metadata property.</param>
    /// <param name="value">The value of the metadata property.</param>
    public CacheItemMetaItem(int metadataId, MetadataItemName metaName, int? mediaObjectId, int? albumId, string rawValue, string value)
	  {
	    MetadataId = metadataId;
	    MetaName = metaName;
	    MediaObjectId = mediaObjectId;
	    AlbumId = albumId;
	    RawValue = rawValue;
	    Value = value;
	  }

    /// <summary>
    /// Gets the ID of the metadata property.
    /// </summary>
    public int MetadataId
		{
			get;
		}

    /// <summary>
    /// Gets the name/type of the metadata property.
    /// </summary>
    public MetadataItemName MetaName
		{
			get;
		}

    /// <summary>
    /// Gets the ID of the media object this meta property belongs to. Will be null when this instance applies to an album.
    /// </summary>
    public int? MediaObjectId
		{
			get;
		}

    /// <summary>
    /// Gets the ID of the album this meta property belongs to. Will be null when this instance applies to a media object.
    /// </summary>
    public int? AlbumId
		{
			get;
		}

    /// <summary>
    /// Gets the raw value of the metadata property.
    /// </summary>
    public string RawValue
		{
			get;
		}

    /// <summary>
    /// Gets the value of the metadata property.
    /// </summary>
    public string Value
		{
			get;
		}

    /// <summary>
    /// Creates a collection of <see cref="CacheItemMetaItem" /> instances from <paramref name="metadataItems" />. This instance is suitable for storing in cache.
    /// </summary>
    /// <param name="metadataItems">The metadata items.</param>
    /// <param name="galleryObjectId">The ID of the gallery object the <paramref name="metadataItems" /> belong to.</param>
    /// <param name="goType">The type of the gallery object the <paramref name="metadataItems" /> belong to. Any value other than <see cref="GalleryObjectType.Album" />
    /// is assumed to be a media object.</param>
    /// <returns>List&lt;CacheItemMetaItem&gt;.</returns>
    public static List<CacheItemMetaItem> FromMetaItems(IGalleryObjectMetadataItemCollection metadataItems, int galleryObjectId, GalleryObjectType goType)
	  {
	    var items = new List<CacheItemMetaItem>(metadataItems.Count);

	    var albumId = (goType == GalleryObjectType.Album ? new int?(galleryObjectId) : null);
	    var moId = (goType == GalleryObjectType.Album ? null : new int?(galleryObjectId));

	    foreach (var mi in metadataItems)
	    {
          items.Add(new CacheItemMetaItem(mi.MediaObjectMetadataId, mi.MetadataItemName, moId, albumId, mi.RawValue, mi.Value));
      }

      return items;
	  }
	}
}
