using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Data;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents an album asset that is suitable for storing in cache.
  /// </summary>
  [System.Diagnostics.DebuggerDisplay("({GalleryObjectType}) ID = {Id}; Parent Album ID = {AlbumId}")]
  public class CacheItemAlbum : CacheItemMedia
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheItemAlbum" /> class.
    /// </summary>
    /// <param name="id">The ID for this gallery object.</param>
    /// <param name="galleryId">The ID for the gallery.</param>
    /// <param name="albumId">The ID of the album this gallery asset belongs to.</param>
    /// <param name="galleryObjectType">Type of the gallery asset.</param>
    /// <param name="sequence">The sequence of this gallery asset.</param>
    /// <param name="dateAdded">The date this gallery asset was added.</param>
    /// <param name="metaItems">The meta items belonging to this gallery asset.</param>
    /// <param name="createdByUserName">Name of the user who created this gallery asset.</param>
    /// <param name="lastModifiedByUserName">The name of the user who last modified this gallery asset.</param>
    /// <param name="dateLastModified">The date this gallery asset was last modified.</param>
    /// <param name="isPrivate">A value indicating whether this instance if private.</param>
    /// <param name="directoryName">The name of the directory where the album is stored. Example: summervacation.</param>
    /// <param name="thumbnailMediaObjectId">The media object ID whose thumbnail image is to be used as the thumbnail image to represent this album.</param>
    /// <param name="sortByMetaName">The metadata property to sort the album by.</param>
    /// <param name="sortAscending">A value indicating whether the contents of the album are sorted in ascending order. A <c>false</c> value indicates
    /// a descending sort.</param>
    /// <param name="ownedBy">The user name of this album's owner.</param>
    /// <param name="ownerRoleName">The name of the role associated with this album's owner.</param>
    /// <param name="childAlbumIds">The ID's of all child albums in this album. The IDs must be in the key; the value is ignored.</param>
    /// <param name="childMediaObjectIds">The ID's of all child media assets in this album. The IDs must be in the key; the value is ignored.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when a required parameter is null.</exception>
    public CacheItemAlbum(int id, int galleryId, int albumId, GalleryObjectType galleryObjectType, int sequence, DateTime dateAdded, List<CacheItemMetaItem> metaItems, string createdByUserName, string lastModifiedByUserName, DateTime dateLastModified, bool isPrivate, string directoryName, int thumbnailMediaObjectId, MetadataItemName sortByMetaName, bool sortAscending, string ownedBy, string ownerRoleName, ConcurrentDictionary<int, byte> childAlbumIds, ConcurrentDictionary<int, byte> childMediaObjectIds)
      : base(id, galleryId, albumId, null, galleryObjectType, sequence, dateAdded, metaItems, createdByUserName, lastModifiedByUserName, dateLastModified, isPrivate)
    {
      if (metaItems == null)
        throw new ArgumentNullException(nameof(metaItems));

      if (childAlbumIds == null)
        throw new ArgumentNullException(nameof(childAlbumIds));

      if (childMediaObjectIds == null)
        throw new ArgumentNullException(nameof(childMediaObjectIds));

      DirectoryName = directoryName;
      ThumbnailMediaObjectId = thumbnailMediaObjectId;
      SortByMetaName = sortByMetaName;
      SortAscending = sortAscending;
      OwnedBy = ownedBy;
      OwnerRoleName = ownerRoleName;
      ChildAlbumIds = childAlbumIds;
      ChildMediaObjectIds = childMediaObjectIds;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the name of the directory where the album is stored. Example: summervacation.
    /// </summary>
    public string DirectoryName
    {
      get;
    }

    /// <summary>
    /// Gets the media object ID whose thumbnail image is to be used as the thumbnail image to represent this album.
    /// </summary>
    public int ThumbnailMediaObjectId
    {
      get;
    }

    /// <summary>
    /// Gets the metadata property to sort the album by.
    /// </summary>
    public MetadataItemName SortByMetaName
    {
      get;
    }

    /// <summary>
    /// Gets a value indicating whether the contents of the album are sorted in ascending order. A <c>false</c> value indicates
    /// a descending sort.
    /// </summary>
    public bool SortAscending
    {
      get;
    }

    /// <summary>
    /// Gets the user name of this album's owner.
    /// </summary>
    public string OwnedBy
    {
      get;
    }

    /// <summary>
    /// Gets the name of the role associated with this album's owner.
    /// </summary>
    public string OwnerRoleName
    {
      get;
    }

    /// <summary>
    /// Gets the ID's of all child albums in this album. Ideally we'd use a ConcurrentHashSet, but that doesn't exist, so we use a
    /// <see cref="ConcurrentDictionary{TKey,TValue}" /> where the album IDs are the keys and the values are 0-valued bytes 
    /// serving no function. Guaranteed to not return null.
    /// </summary>
    public ConcurrentDictionary<int, byte> ChildAlbumIds { get; }

    /// <summary>
    /// Gets the ID's of all child media assets in this album. Ideally we'd use a ConcurrentHashSet, but that doesn't exist, so we use a
    /// <see cref="ConcurrentDictionary{TKey,TValue}" /> where the media assets IDs are the keys and the values are 0-valued bytes 
    /// serving no function. Guaranteed to not return null.
    /// </summary>
    public ConcurrentDictionary<int, byte> ChildMediaObjectIds { get; }

    #endregion

    /// <summary>
    /// Creates an instance of <see cref="CacheItemAlbum" /> from <paramref name="album" />. This instance is suitable for storing in cache.
    /// If <paramref name="album" /> is not inflated, a call to the data store is made to retrieve the child albums and media assets.
    /// </summary>
    /// <param name="album">The album.</param>
    /// <returns>An instance of <see cref="CacheItemAlbum" />.</returns>
    public static CacheItemAlbum CreateFrom(IAlbum album)
    {
      ConcurrentDictionary<int, byte> childAlbumIds;
      ConcurrentDictionary<int, byte> childMediaObjectIds;

      if (album.AreChildrenInflated)
      {
        childAlbumIds = new ConcurrentDictionary<int, byte>(album.GetChildGalleryObjects(GalleryObjectType.Album).ToDictionary(k => k.Id, v => (byte)0));
        childMediaObjectIds = new ConcurrentDictionary<int, byte>(album.GetChildGalleryObjects(GalleryObjectType.MediaObject).ToDictionary(k => k.Id, v => (byte)0));
      }
      else
      {
        using (var repo = new AlbumRepository())
        {
          childAlbumIds = new ConcurrentDictionary<int, byte>(repo.Where(a => a.FKAlbumParentId == album.Id).ToDictionary(k => k.AlbumId, v => (byte)0));
        }

        using (var repo = new MediaObjectRepository())
        {
          childMediaObjectIds = new ConcurrentDictionary<int, byte>(repo.Where(a => a.FKAlbumId == album.Id).ToDictionary(k => k.MediaObjectId, v => (byte)0));
        }
      }

      return new CacheItemAlbum(album.Id, album.GalleryId, album.Parent.Id, album.GalleryObjectType, album.Sequence, album.DateAdded, CacheItemMetaItem.FromMetaItems(album.MetadataItems, album.Id, album.GalleryObjectType), album.CreatedByUserName, album.LastModifiedByUserName, album.DateLastModified, album.IsPrivate, album.DirectoryName, album.ThumbnailMediaObjectId, album.SortByMetaName, album.SortAscending, album.OwnerUserName, album.OwnerRoleName, childAlbumIds, childMediaObjectIds);
    }
  }
}
