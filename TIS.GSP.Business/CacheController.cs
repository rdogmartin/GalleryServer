using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// Provides functionality for interacting with the caching infrastructure.
  /// </summary>
  public static class CacheController
  {
    #region Fields

    private static ObjectCache _cacheManager;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the cache manager.
    /// </summary>
    /// <value>The cache manager.</value>
    private static ObjectCache CacheManager
    {
      get { return _cacheManager ?? (_cacheManager = MemoryCache.Default); }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.InflatedAlbums" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.Int32, IAlbum&gt;.</returns>
    public static ConcurrentDictionary<int, IAlbum> GetInflatedAlbumCache()
    {
      return GetCache<ConcurrentDictionary<int, IAlbum>>(CacheItem.InflatedAlbums);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.AlbumAssets" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.Int32, CacheItemAlbum&gt;.</returns>
    public static ConcurrentDictionary<int, CacheItemAlbum> GetAlbumAssetCache()
    {
      return GetCache<ConcurrentDictionary<int, CacheItemAlbum>>(CacheItem.AlbumAssets);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.MediaAssets" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.Int32, CacheItemMedia&gt;.</returns>
    public static ConcurrentDictionary<int, CacheItemMedia> GetMediaAssetCache()
    {
      return GetCache<ConcurrentDictionary<int, CacheItemMedia>>(CacheItem.MediaAssets);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.Users" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>IUserAccountCollection.</returns>
    public static IUserAccountCollection GetUsersCache()
    {
      return GetCache<IUserAccountCollection>(CacheItem.Users);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.UsersCurrentUserCanView" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.String, IUserAccountCollection&gt;.</returns>
    public static ConcurrentDictionary<string, IUserAccountCollection> GetUsersCurrentUserCanViewCache()
    {
      return GetCache<ConcurrentDictionary<string, IUserAccountCollection>>(CacheItem.UsersCurrentUserCanView);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.GalleryServerRoles" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.String, IGalleryServerRoleCollection&gt;.</returns>
    public static ConcurrentDictionary<string, IGalleryServerRoleCollection> GetGalleryServerRolesCache()
    {
      return GetCache<ConcurrentDictionary<string, IGalleryServerRoleCollection>>(CacheItem.GalleryServerRoles);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.ActiveDirectoryUserGroups" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns><see cref="System.Collections.Concurrent.ConcurrentDictionary{T, V}" /></returns>
    public static ConcurrentDictionary<string, string[]> GetActiveDirectoryUserGroupsCache()
    {
      return GetCache<ConcurrentDictionary<string, string[]>>(CacheItem.ActiveDirectoryUserGroups);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.ActiveDirectoryGroupUsers" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns><see cref="System.Collections.Concurrent.ConcurrentDictionary{T, V}" /></returns>
    public static ConcurrentDictionary<string, string[]> GetActiveDirectoryGroupUsersCache()
    {
      return GetCache<ConcurrentDictionary<string, string[]>>(CacheItem.ActiveDirectoryGroupUsers);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.AppEvents" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>IEventCollection.</returns>
    public static IEventCollection GetAppEventsCache()
    {
      return GetCache<IEventCollection>(CacheItem.AppEvents);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.MediaTemplates" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>IMediaTemplateCollection.</returns>
    public static IMediaTemplateCollection GetMediaTemplatesCache()
    {
      return GetCache<IMediaTemplateCollection>(CacheItem.MediaTemplates);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.MimeTypes" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.Int32, IMimeTypeCollection&gt;.</returns>
    public static ConcurrentDictionary<int, IMimeTypeCollection> GetMimeTypesCache()
    {
      return GetCache<ConcurrentDictionary<int, IMimeTypeCollection>>(CacheItem.MimeTypes);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.Profiles" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.String, IUserProfile&gt;.</returns>
    public static ConcurrentDictionary<string, IUserProfile> GetProfilesCache()
    {
      return GetCache<ConcurrentDictionary<string, IUserProfile>>(CacheItem.Profiles);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.UiTemplates" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>IUiTemplateCollection.</returns>
    public static IUiTemplateCollection GetUiTemplatesCache()
    {
      return GetCache<IUiTemplateCollection>(CacheItem.UiTemplates);
    }

    /// <summary>
    /// Gets a cache object representing the <see cref="CacheItem.Tags" /> item. Returns null when no cache instance exists.
    /// </summary>
    /// <returns>ConcurrentDictionary&lt;System.String, List&lt;Entity.TagCacheItem&gt;&gt;.</returns>
    public static ConcurrentDictionary<string, List<Entity.TagCacheItem>> GetTagsCache()
    {
      return GetCache<ConcurrentDictionary<string, List<Entity.TagCacheItem>>>(CacheItem.Tags);
    }

    /// <overloads>
    /// Adds the <paramref name="cacheItem"/> to the cache named <paramref name="cacheItemId"/>.
    /// </overloads>
    /// <summary>
    /// Adds the <paramref name="cacheItem"/> to the cache named <paramref name="cacheItemId"/>. If it exists it is overwritten.
    /// If <paramref name="cacheItem"/> is null, any existing cache named <paramref name="cacheItemId"/> is deleted.
    /// </summary>
    /// <param name="cacheItemId">The cache item ID for the cache item.</param>
    /// <param name="cacheItem">The item to be stored in cache.</param>
    public static void SetCache(CacheItem cacheItemId, object cacheItem)
    {
      SetCache(cacheItemId, cacheItem, ObjectCache.InfiniteAbsoluteExpiration);
    }

    /// <summary>
    /// Adds the <paramref name="cacheItem"/> to the cache named <paramref name="cacheItemId"/> and set to an absolute expiration
    /// time specified in <paramref name="dateTimeOffset"/>. If it exists it is overwritten.
    /// If <paramref name="cacheItem"/> is null, any existing cache named <paramref name="cacheItemId"/> is deleted.
    /// If caching is disabled (<see cref="IAppSetting.EnableCache" />=<c>false</c>), then no action is taken.
    /// </summary>
    /// <param name="cacheItemId">The cache item ID for the cache item.</param>
    /// <param name="cacheItem">The item to be stored in cache.</param>
    /// <param name="dateTimeOffset">The fixed date and time at which the cache entry will expire.</param>
    public static void SetCache(CacheItem cacheItemId, object cacheItem, DateTimeOffset dateTimeOffset)
    {
      if (!AppSetting.Instance.EnableCache)
        return;

      if (cacheItem != null)
      {
        CacheManager.Add(cacheItemId.ToString(), cacheItem, dateTimeOffset);
      }
      else
      {
        CacheManager.Remove(cacheItemId.ToString());
      }
    }

    /// <summary>
    /// Clears all cached representations of data. Includes both items in the cache manager and static variables that may need to be flushed.
    /// </summary>
    public static void PurgeCache()
    {
      Factory.ClearGalleryCache();
      Factory.ClearGalleryControlSettingsCache();
      Factory.ClearWatermarkCache();

      foreach (CacheItem cacheItem in Enum.GetValues(typeof(CacheItem)))
      {
        RemoveCache(cacheItem);
      }
    }

    /// <summary>
    /// Remove the cache representation of <paramref name="galleryObject" /> from the cache. Note that this function does not remove its ID
    /// from the parent album that may also be in cache. Therefore, this function is to be used when making changes to a gallery object that 
    /// DO NOT involve moving it to another album.
    /// </summary>
    /// <param name="galleryObject">The gallery object.</param>
    public static void PurgeCache(IGalleryObject galleryObject)
    {
      if (galleryObject == null)
      {
        throw new ArgumentNullException(nameof(galleryObject));
      }

      if (galleryObject.GalleryObjectType == GalleryObjectType.Album)
      {
        RemoveAlbumFromCache(galleryObject.Id);
      }
      else
      {
        RemoveMediaAssetFromCache(galleryObject.Id);
      }

      RemoveInflatedAlbumsFromCache();

      RemoveTagsFromCache();
    }

    /// <summary>
    /// Removes the data associated with the <paramref name="cacheItemId"/> from the cache.
    /// </summary>
    /// <param name="cacheItemId">The cache item ID for the cache item.</param>
    public static void RemoveCache(CacheItem cacheItemId)
    {
      CacheManager.Remove(cacheItemId.ToString());
    }

    /// <summary>
    /// Clears the in-memory copy of the current set of gallery control settings. This will force a database retrieval the next time
    /// they are requested.
    /// </summary>
    public static void ClearGalleryCache()
    {
      Factory.ClearGalleryCache();
    }

    /// <summary>
    /// Clears the in-memory copy of the current set of gallery control settings. This will force a database retrieval the next time
    /// they are requested.
    /// </summary>
    public static void ClearGalleryControlSettingsCache()
    {
      Factory.ClearGalleryControlSettingsCache();
    }

    /// <summary>
    /// Clears the in-memory copy of the current set of watermarks.
    /// </summary>
    public static void ClearWatermarkCache()
    {
      Factory.ClearWatermarkCache();
    }

    /// <summary>
    /// Adds the <paramref name="albumAsset" /> to the album asset cache (<see cref="CacheItem.AlbumAssets" />).
    /// The cache container is created if necessary.
    /// </summary>
    /// <param name="albumAsset">The album asset to store in cache.</param>
    public static void AddToAlbumAssetCache(CacheItemAlbum albumAsset)
    {
      if (!AppSetting.Instance.EnableCache)
        return;

      if (albumAsset == null)
      {
        throw new ArgumentNullException(nameof(albumAsset));
      }

      var albumAssetCache = GetAlbumAssetCache() ?? new ConcurrentDictionary<int, CacheItemAlbum>();

      albumAssetCache.TryAdd(albumAsset.Id, albumAsset);

      SetCache(CacheItem.AlbumAssets, albumAssetCache);
    }

    /// <summary>
    /// Adds the <paramref name="album" /> to the inflated album cache (<see cref="CacheItem.InflatedAlbums" />).
    /// The cache container is created if necessary.
    /// </summary>
    /// <param name="album">The album to store in cache.</param>
    public static void AddToInflatedAlbumCache(IAlbum album)
    {
      if (!AppSetting.Instance.EnableCache)
        return;

      if (album == null)
      {
        throw new ArgumentNullException(nameof(album));
      }

      var inflatedAlbumCache = GetInflatedAlbumCache() ?? new ConcurrentDictionary<int, IAlbum>();

      inflatedAlbumCache.TryAdd(album.Id, album);

      SetCache(CacheItem.InflatedAlbums, inflatedAlbumCache);
    }

    /// <summary>
    /// Adds the <paramref name="mediaAsset" /> to the media asset cache (<see cref="CacheItem.MediaAssets" />).
    /// The cache container is created if necessary.
    /// </summary>
    /// <param name="mediaAsset">The media asset to store in cache.</param>
    public static void AddToMediaAssetCache(CacheItemMedia mediaAsset)
    {
      if (!AppSetting.Instance.EnableCache)
        return;

      if (mediaAsset == null)
      {
        throw new ArgumentNullException(nameof(mediaAsset));
      }

      var mediaAssetCache = GetMediaAssetCache() ?? new ConcurrentDictionary<int, CacheItemMedia>();

      mediaAssetCache.TryAdd(mediaAsset.Id, mediaAsset);

      SetCache(CacheItem.MediaAssets, mediaAssetCache);
    }

    /// <summary>
    /// Removes all albums from the inflated albums cache.
    /// </summary>
    public static void RemoveInflatedAlbumsFromCache()
    {
      RemoveCache(CacheItem.InflatedAlbums);
    }

    /// <summary>
    /// Removes the album with ID <paramref name="albumId" /> from the cached collection of album assets associated with
    /// <see cref="CacheItem.AlbumAssets" />.
    /// </summary>
    /// <param name="albumId">The ID of the album.</param>
    /// <remarks>If you are calling this method because the album is being deleted or is being moved to another album, be sure
    /// to also call <see cref="RemoveAlbumIdFromParentAlbumCacheItem" />.</remarks>
    public static void RemoveAlbumFromCache(int albumId)
    {
      CacheItemAlbum albumAsset;

      GetAlbumAssetCache()?.TryRemove(albumId, out albumAsset);
    }

    /// <summary>
    /// Removes the <paramref name="albumId" /> from the album asset cache item having ID <paramref name="parentAlbumId" />.
    /// That is, the album ID is removed from <see cref="CacheItemAlbum.ChildAlbumIds" />. This function is intended to be used
    /// along with <see cref="RemoveAlbumFromCache" /> to purge all traces of an album from the cache.
    /// </summary>
    /// <param name="albumId">The album ID.</param>
    /// <param name="parentAlbumId">The ID of the album containing the album with the ID <paramref name="albumId" />.</param>
    public static void RemoveAlbumIdFromParentAlbumCacheItem(int albumId, int parentAlbumId)
    {
      var albumCache = GetAlbumAssetCache();

      if (albumCache != null)
      {
        CacheItemAlbum parentAlbumAsset;
        if (albumCache.TryGetValue(parentAlbumId, out parentAlbumAsset))
        {
          byte removedValue;
          parentAlbumAsset.ChildAlbumIds.TryRemove(albumId, out removedValue);
        }
      }
    }

    /// <summary>
    /// Removes the media asset with ID <paramref name="mediaId" /> from the cached collection of media assets associated with
    /// <see cref="CacheItem.MediaAssets" />.
    /// </summary>
    /// <param name="mediaId">The ID of the media asset.</param>
    /// <remarks>If you are calling this method because the media asset is being deleted or is being moved to another album, be sure
    /// to also call <see cref="RemoveMediaAssetIdFromParentAlbumCacheItem" />.</remarks>
    public static void RemoveMediaAssetFromCache(int mediaId)
    {
      CacheItemMedia mediaAsset;

      GetMediaAssetCache()?.TryRemove(mediaId, out mediaAsset);
    }

    /// <summary>
    /// Removes the <paramref name="mediaAssetId" /> from the album asset cache item having ID <paramref name="parentAlbumId" />.
    /// That is, the media ID is removed from <see cref="CacheItemAlbum.ChildMediaObjectIds" />. This function is intended to be used
    /// along with <see cref="RemoveMediaAssetFromCache" /> to purge all traces of a media asset from the cache.
    /// </summary>
    /// <param name="mediaAssetId">The media asset ID.</param>
    /// <param name="parentAlbumId">The ID of the album containing the media asset.</param>
    public static void RemoveMediaAssetIdFromParentAlbumCacheItem(int mediaAssetId, int parentAlbumId)
    {
      var albumCache = GetAlbumAssetCache();

      if (albumCache != null)
      {
        CacheItemAlbum parentAlbumAsset;
        if (albumCache.TryGetValue(parentAlbumId, out parentAlbumAsset))
        {
          byte removedValue;
          parentAlbumAsset.ChildMediaObjectIds.TryRemove(mediaAssetId, out removedValue);
        }
      }
    }

    /// <summary>
    /// Remove tags/people from the cache.
    /// </summary>
    public static void RemoveTagsFromCache()
    {
      RemoveCache(CacheItem.Tags);
    }

    /// <summary>
    /// Remove any cache items that hold a reference to media templates.
    /// </summary>
    public static void RemoveMediaTemplatesFromCache()
    {
      RemoveInflatedAlbumsFromCache();
      RemoveCache(CacheItem.MediaTemplates);
      RemoveCache(CacheItem.MimeTypes);
    }

    /// <summary>
    /// Adds the <paramref name="albumId" /> to the album asset cache item having ID <paramref name="parentAlbumId" />.
    /// That is, the album ID is added to <see cref="CacheItemAlbum.ChildAlbumIds" />. This function is useful when creating an
    /// album or moving one to another album.
    /// </summary>
    /// <param name="albumId">The album ID.</param>
    /// <param name="parentAlbumId">The ID of the album containing the album with the ID <paramref name="albumId" />.</param>
    public static void AddAlbumIdToAlbumCacheItem(int albumId, int parentAlbumId)
    {
      if (!AppSetting.Instance.EnableCache)
        return;

      var albumCache = GetAlbumAssetCache();

      if (albumCache != null)
      {
        CacheItemAlbum parentAlbumAsset;
        if (albumCache.TryGetValue(parentAlbumId, out parentAlbumAsset))
        {
          parentAlbumAsset.ChildAlbumIds.TryAdd(albumId, 0);
        }
      }
    }

    /// <summary>
    /// Adds the <paramref name="mediaAssetId" /> to the album asset cache item having ID <paramref name="parentAlbumId" />.
    /// That is, the media ID is added to <see cref="CacheItemAlbum.ChildMediaObjectIds" />. This function is useful when creating a
    /// media object or moving one to another album.
    /// </summary>
    /// <param name="mediaAssetId">The media asset ID.</param>
    /// <param name="parentAlbumId">The ID of the album containing the album with the ID <paramref name="mediaAssetId" />.</param>
    public static void AddMediaAssetIdToAlbumCacheItem(int mediaAssetId, int parentAlbumId)
    {
      if (!AppSetting.Instance.EnableCache)
        return;

      var albumCache = GetAlbumAssetCache();

      if (albumCache != null)
      {
        CacheItemAlbum parentAlbumAsset;
        if (albumCache.TryGetValue(parentAlbumId, out parentAlbumAsset))
        {
          parentAlbumAsset.ChildMediaObjectIds.TryAdd(mediaAssetId, 0);
        }
      }
    }

    /// <summary>
    /// Removes the entry in the <see cref="CacheItem.Users" /> and <see cref="CacheItem.UsersCurrentUserCanView" /> caches for 
    /// the specified <paramref name="userName" />.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    public static void RemoveUserFromCache(string userName)
    {
      if (string.IsNullOrWhiteSpace(userName))
      {
        return;
      }

      var usersCache = CacheController.GetUsersCache();

      var user = usersCache?.FindByUserName(userName);
      if (user != null)
      {
        usersCache.Remove(user);
      }

      var usersCurrentUserCanViewCache = GetUsersCurrentUserCanViewCache();

      if (usersCurrentUserCanViewCache != null)
      {
        foreach (var kvp in usersCurrentUserCanViewCache)
        {
          user = kvp.Value.FindByUserName(userName);
          kvp.Value.Remove(user);
        }
      }
    }

    /// <summary>
    /// Replaces the user in the <see cref="CacheItem.Users" /> and <see cref="CacheItem.UsersCurrentUserCanView" /> caches with the specified
    /// <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user.</param>
    public static void ReplaceUserInCache(IUserAccount user)
    {
      if (user == null)
        throw new ArgumentNullException(nameof(user));

      var usersCache = GetUsersCache();

      usersCache?.Remove(user);
      usersCache?.Add(user);

      var usersCurrentUserCanViewCache = GetUsersCurrentUserCanViewCache();

      if (usersCurrentUserCanViewCache != null)
      {
        foreach (var kvp in usersCurrentUserCanViewCache)
        {
          kvp.Value.Remove(user);
          kvp.Value.Add(user);
        }
      }
    }

    #endregion

    #region Functions

    /// <summary>
    /// Gets the data stored in cache that has the name <paramref name="cacheItemId" />. Returns null if no data is in the cache.
    /// </summary>
    /// <typeparam name="T">Specify the type of the item as it's stored in cache.</typeparam>
    /// <param name="cacheItemId">The cache item ID for the cache item.</param>
    /// <returns>Returns the data stored in cache that has the name <paramref name="cacheItemId" />.</returns>
    /// <exception cref="InvalidCastException">Thrown when a cache item exists and it cannot be cast to the requested type.</exception>
    private static T GetCache<T>(CacheItem cacheItemId)
    {
      return (T)CacheManager.Get(cacheItemId.ToString());
    }

    #endregion
  }
}
