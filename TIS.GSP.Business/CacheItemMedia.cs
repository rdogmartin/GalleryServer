using System;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a gallery asset that is suitable for storing in cache.
  /// </summary>
  [System.Diagnostics.DebuggerDisplay("({GalleryObjectType}) ID = {Id}; Album ID = {AlbumId}")]
  public class CacheItemMedia
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheItemMedia"/> class.
    /// </summary>
    /// <param name="id">The ID for this gallery object.</param>
    /// <param name="galleryId">The ID for the gallery.</param>
    /// <param name="albumId">The ID of the album this gallery asset belongs to.</param>
    /// <param name="displayObjects">An array of cache-friendly display objects, representing the thumbnail, optimized and original display objects.
    /// Specify null for albums.</param>
    /// <param name="galleryObjectType">Type of the gallery asset.</param>
    /// <param name="sequence">The sequence of this gallery asset.</param>
    /// <param name="dateAdded">The date this gallery asset was added.</param>
    /// <param name="metaItems">The meta items belonging to this gallery asset.</param>
    /// <param name="createdByUserName">Name of the user who created this gallery asset.</param>
    /// <param name="lastModifiedByUserName">The name of the user who last modified this gallery asset.</param>
    /// <param name="dateLastModified">The date this gallery asset was last modified.</param>
    /// <param name="isPrivate">A value indicating whether this instance if private.</param>
    public CacheItemMedia(int id, int galleryId, int albumId, CacheItemDisplayObject[] displayObjects, GalleryObjectType galleryObjectType, int sequence, DateTime dateAdded, List<CacheItemMetaItem> metaItems, string createdByUserName, string lastModifiedByUserName, DateTime dateLastModified, bool isPrivate)
    {
      Id = id;
      GalleryId = galleryId;
      AlbumId = albumId;
      DisplayObjects = displayObjects;
      GalleryObjectType = galleryObjectType;
      Sequence = sequence;
      DateAdded = dateAdded;
      MetaItems = metaItems;
      CreatedByUserName = createdByUserName;
      LastModifiedByUserName = lastModifiedByUserName;
      DateLastModified = dateLastModified;
      IsPrivate = isPrivate;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique identifier for this gallery asset.
    /// </summary>
    public int Id
    {
      get;
    }

    /// <summary>
    /// Gets the ID for the gallery.
    /// </summary>
    public int GalleryId
    {
      get;
    }

    /// <summary>
    /// Gets the ID of the album this gallery asset belongs to.
    /// </summary>
    public int AlbumId
    {
      get;
    }

    /// <summary>
    /// Gets an array of cache-friendly display objects, representing the thumbnail, optimized and original display objects. Will be null when 
    /// this instance represents an album.
    /// </summary>
    public CacheItemDisplayObject[] DisplayObjects
    {
      get;
    }

    /// <summary>
    /// Gets the type of the gallery asset.
    /// </summary>
    public GalleryObjectType GalleryObjectType
    {
      get;
    }

    /// <summary>
    /// Gets the sequence of this gallery asset.
    /// </summary>
    public int Sequence
    {
      get;
    }

    /// <summary>
    /// Gets the date this gallery asset was added.
    /// </summary>
    public DateTime DateAdded
    {
      get;
    }

    /// <summary>
    /// Gets the meta items belonging to this gallery asset.
    /// </summary>
    public List<CacheItemMetaItem> MetaItems
    {
      get;
    }

    /// <summary>
    /// Gets the name of the user who created this gallery asset.
    /// </summary>
    public string CreatedByUserName
    {
      get;
    }

    /// <summary>
    /// Gets the name of the user who last modified this gallery asset.
    /// </summary>
    public string LastModifiedByUserName
    {
      get;
    }

    /// <summary>
    /// Gets the date this gallery asset was last modified.
    /// </summary>
    public DateTime DateLastModified
    {
      get;
    }

    /// <summary>
    /// Gets a value indicating whether this instance if private.
    /// </summary>
    public bool IsPrivate
    {
      get;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates an instance of <see cref="CacheItemMedia" /> from <paramref name="go" />. This instance is suitable for storing in cache.
    /// </summary>
    /// <param name="go">The media object.</param>
    /// <returns>An instance of <see cref="CacheItemMedia" />.</returns>
    public static CacheItemMedia CreateFrom(IGalleryObject go)
    {
      return new CacheItemMedia(go.Id, go.GalleryId, go.Parent.Id, CacheItemDisplayObject.CreateFrom(go.Thumbnail, go.Optimized, go.Original), go.GalleryObjectType, go.Sequence, go.DateAdded, CacheItemMetaItem.FromMetaItems(go.MetadataItems, go.Id, go.GalleryObjectType), go.CreatedByUserName, go.LastModifiedByUserName, go.DateLastModified, go.IsPrivate);
    }

    #endregion
  }
}
