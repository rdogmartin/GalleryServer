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
  /// Provides functionality for finding one or more descriptive tags or people.
  /// </summary>
  public class TagSearcher
  {
    #region Fields

    private IAlbum _rootAlbum;
    private bool? _userCanViewRootAlbum;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the search options.
    /// </summary>
    /// <value>The search options.</value>
    private TagSearchOptions SearchOptions { get; set; }

    /// <summary>
    /// Indicates the type of tag being searched. Must be <see cref="MetadataItemName.Tags" /> or 
    /// <see cref="MetadataItemName.People" />.
    /// </summary>
    private MetadataItemName TagName
    {
      get
      {
        switch (SearchOptions.SearchType)
        {
          case TagSearchType.TagsUserCanView:
          case TagSearchType.AllTagsInGallery:
            return MetadataItemName.Tags;

          case TagSearchType.PeopleUserCanView:
          case TagSearchType.AllPeopleInGallery:
            return MetadataItemName.People;

          default:
            throw new InvalidOperationException(String.Format("The property TagSearcher.TagName was not designed to handle the SearchType {0}. The developer must update this property.", SearchOptions.SearchType));
        }
      }
    }

    /// <summary>
    /// Gets the root album for the gallery identified in the <see cref="SearchOptions" />.
    /// </summary>
    private IAlbum RootAlbum
    {
      get { return _rootAlbum ?? (_rootAlbum = Factory.LoadRootAlbumInstance(SearchOptions.GalleryId)); }
    }

    /// <summary>
    /// Gets a value indicating whether the current user can view the root album.
    /// </summary>
    /// <returns><c>true</c> if the user can view the root album; otherwise, <c>false</c>.</returns>
    private bool UserCanViewRootAlbum
    {
      get
      {
        if (!_userCanViewRootAlbum.HasValue)
        {
          _userCanViewRootAlbum = HelperFunctions.CanUserViewAlbum(RootAlbum, SearchOptions.Roles, SearchOptions.IsUserAuthenticated);
        }

        return _userCanViewRootAlbum.Value;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TagSearcher" /> class.
    /// </summary>
    /// <param name="searchOptions">The search options.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="searchOptions" /> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when one or more properties of the <paramref name="searchOptions" /> parameter is invalid.</exception>
    public TagSearcher(TagSearchOptions searchOptions)
    {
      Validate(searchOptions);

      SearchOptions = searchOptions;

      if (SearchOptions.Roles == null)
      {
        SearchOptions.Roles = new GalleryServerRoleCollection();
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Finds all tags that match the search criteria. Guaranteed to not return null.
    /// </summary>
    /// <returns>A collection of <see cref="TagDto" /> instances.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when an implementation is not found for one of the 
    /// search types.</exception>
    public IEnumerable<Entity.Tag> Find()
    {
      switch (SearchOptions.SearchType)
      {
        case TagSearchType.AllTagsInGallery:
        case TagSearchType.AllPeopleInGallery:
          return GetTags(false);

        case TagSearchType.TagsUserCanView:
        case TagSearchType.PeopleUserCanView:
          if (UserCanViewRootAlbum && string.IsNullOrWhiteSpace(SearchOptions.SearchTerm))
          {
            // Perf optimization: User can see all non-private tags and is not doing a text search, so we can retrieve from cache.
            return GetTags(!SearchOptions.IsUserAuthenticated);
          }
          else
          {
            return GetTagsForUser();
          }
        default:
          throw new InvalidOperationException(string.Format("The method GalleryObjectSearcher.Find was not designed to handle SearchType={0}. The developer must update this method.", SearchOptions.SearchType));
      }
    }

    #endregion

    #region Functions

    /// <summary>
    /// Validates the specified search options. Throws an exception if not valid.
    /// </summary>
    /// <param name="searchOptions">The search options.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="searchOptions" /> is null.</exception>
    /// <exception cref="System.ArgumentException">Thrown when one or more properties of the <paramref name="searchOptions" /> parameter is invalid.</exception>
    private static void Validate(TagSearchOptions searchOptions)
    {
      if (searchOptions == null)
        throw new ArgumentNullException("searchOptions");

      if (searchOptions.SearchType == TagSearchType.NotSpecified)
        throw new ArgumentException("The SearchType property of the searchOptions parameter must be set to a valid search type.");

      if (searchOptions.IsUserAuthenticated && searchOptions.Roles == null)
        throw new ArgumentException("The Roles property of the searchOptions parameter must be specified when IsUserAuthenticated is true.");

      if (searchOptions.GalleryId < 0) // v3+ galleries start at 1, but galleries from earlier versions begin at 0
        throw new ArgumentException("Invalid gallery ID. The GalleryId property of the searchOptions parameter must refer to a valid gallery.");
    }

    /// <summary>
    /// Gets a list of tags the user can view. Guaranteed to not return null. Returns an empty collection
    /// when no tags are found.
    /// </summary>
    /// <returns>A collection of <see cref="Entity.Tag" /> instances.</returns>
    private IEnumerable<Entity.Tag> GetTagsForUser()
    {
      var tags = new List<Entity.Tag>();
      var hasMediaObjectTags = false;

      using (var repo = new MetadataTagRepository())
      {
        tags.AddRange(GetTagsForAlbums(repo));

        if (!UserCanViewRootAlbum)
        {
          // When the user can view the entire gallery, the GetTagsForAlbums() function returned all the tags, so we don't
          // need to look specifically at the tags belonging to media objects. But for restricted users we need this step.
          var tagsForMediaObjects = GetTagsForMediaObjects(repo);
          hasMediaObjectTags = tagsForMediaObjects.Any();

          tags.AddRange(tagsForMediaObjects);
        }
      }

      // Optimization: When there are media object tags, we need to combine the album and media object tags; otherwise we can
      // just return the tags.
      if (hasMediaObjectTags)
      {
        return Take(Sort(tags.GroupBy(t => t.Value).Select(t => new Entity.Tag { Value = t.Key, Count = t.Sum(t1 => t1.Count) }).AsQueryable()));
      }
      else
      {
        return Take(Sort(tags.AsQueryable()));
      }
    }

    /// <summary>
    /// Gets the tags associated with albums the user has permission to view. When the user has permission to view the
    /// entire gallery, then tags are also included for media objects.
    /// </summary>
    /// <param name="repo">The metadata tag repository.</param>
    /// <returns>A collection of <see cref="Entity.Tag" /> instances.</returns>
    /// <remarks>This function is similar to <see cref="GetTagsForMediaObjects(IRepository{MetadataTagDto})" />, so if a developer
    /// modifies it, be sure to check that function to see if it needs a similar change.</remarks>
    private IEnumerable<Entity.Tag> GetTagsForAlbums(IRepository<MetadataTagDto> repo)
    {
      var qry = repo.Where(
        m =>
        m.FKGalleryId == SearchOptions.GalleryId &&
        m.Metadata.MetaName == TagName);

      if (!String.IsNullOrEmpty(SearchOptions.SearchTerm))
      {
        qry = qry.Where(m => m.FKTagName.Contains(SearchOptions.SearchTerm));
      }

      if (SearchOptions.IsUserAuthenticated)
      {
        if (!UserCanViewRootAlbum)
        {
          // User can't view the root album, so get a list of the albums she *can* see and make sure our 
          // results only include albums that are viewable.
          var albumIds = SearchOptions.Roles.GetViewableAlbumIdsForGallery(SearchOptions.GalleryId).Cast<int?>();

          qry = qry.Where(a => albumIds.Contains(a.Metadata.FKAlbumId));
        }
      }
      else if (UserCanViewRootAlbum)
      {
        // Anonymous user, so don't include any private albums in results.
        qry = qry.Where(a =>
          (a.Metadata.Album != null && !a.Metadata.Album.IsPrivate) ||
          (a.Metadata.MediaObject != null && !a.Metadata.MediaObject.Album.IsPrivate));
      }
      else
      {
        // User is anonymous and does not have permission to view the root album, meaning they
        // can't see anything. Return empty collection.
        return new List<Entity.Tag>();
      }

      return qry.GroupBy(t => t.FKTagName).Select(t => new Entity.Tag { Value = t.Key, Count = t.Count() });
    }

    /// <summary>
    /// Gets the tags associated with media objects the user has permission to view.
    /// </summary>
    /// <param name="repo">The metadata tag repository.</param>
    /// <returns>A collection of <see cref="Entity.Tag" /> instances.</returns>
    /// <remarks>This function is similar to <see cref="GetTagsForAlbums(IRepository{MetadataTagDto})" />, so if a developer
    /// modifies it, be sure to check that function to see if it needs a similar change.</remarks>
    private IEnumerable<Entity.Tag> GetTagsForMediaObjects(IRepository<MetadataTagDto> repo)
    {
      var qry = repo.Where(
        m =>
        m.FKGalleryId == SearchOptions.GalleryId &&
        m.Metadata.MetaName == TagName);

      if (!String.IsNullOrEmpty(SearchOptions.SearchTerm))
      {
        qry = qry.Where(m => m.FKTagName.Contains(SearchOptions.SearchTerm));
      }

      if (SearchOptions.IsUserAuthenticated)
      {
        if (!UserCanViewRootAlbum)
        {
          // User can't view the root album, so get a list of the albums she *can* see and make sure our 
          // results only include albums that are viewable.
          var albumIds = SearchOptions.Roles.GetViewableAlbumIdsForGallery(SearchOptions.GalleryId);

          qry = qry.Where(a => albumIds.Contains(a.Metadata.MediaObject.FKAlbumId));
        }
      }
      else if (UserCanViewRootAlbum)
      {
        // Anonymous user, so don't include any private albums in results.
        qry = qry.Where(a => !a.Metadata.MediaObject.Album.IsPrivate);
      }
      else
      {
        // User is anonymous and does not have permission to view the root album, meaning they
        // can't see anything. Return empty collection.
        return new List<Entity.Tag>();
      }

      return qry.GroupBy(t => t.FKTagName).Select(t => new Entity.Tag { Value = t.Key, Count = t.Count() });
    }

    /// <summary>
    /// Apply the requested sort operation to the <paramref name="tags" />.
    /// </summary>
    /// <param name="tags">The tags.</param>
    /// <returns>IQueryable{Entity.Tag}.</returns>
    private IQueryable<Entity.Tag> Sort(IQueryable<Entity.Tag> tags)
    {
      switch (SearchOptions.SortProperty)
      {
        case TagSearchOptions.TagProperty.Value:
          return SearchOptions.SortAscending ? tags.OrderBy(t => t.Value) : tags.OrderByDescending(t => t.Value);

        case TagSearchOptions.TagProperty.Count:
          return SearchOptions.SortAscending ? tags.OrderBy(t => t.Count) : tags.OrderByDescending(t => t.Count);

        default:
          return tags;
      }
    }

    /// <summary>
    /// Apply the requested sort operation to the <paramref name="tags" />.
    /// </summary>
    /// <param name="tags">The tags.</param>
    /// <returns><see cref="IQueryable{T}" /> where T is <see cref="Entity.TagCacheItem" />.</returns>
    private IQueryable<Entity.TagCacheItem> Sort(IQueryable<Entity.TagCacheItem> tags)
    {
      switch (SearchOptions.SortProperty)
      {
        case TagSearchOptions.TagProperty.Value:
          return SearchOptions.SortAscending ? tags.OrderBy(t => t.Value) : tags.OrderByDescending(t => t.Value);

        case TagSearchOptions.TagProperty.Count:
          return SearchOptions.SortAscending ? tags.OrderBy(t => t.CountAll) : tags.OrderByDescending(t => t.CountAll);

        default:
          return tags;
      }
    }

    /// <summary>
    /// Return the top <see cref="TagSearchOptions.NumTagsToRetrieve" /> items from <paramref name="tags" />.
    /// </summary>
    /// <param name="tags">The tags.</param>
    /// <returns>IQueryable{Entity.Tag}.</returns>
    private IQueryable<Entity.Tag> Take(IQueryable<Entity.Tag> tags)
    {
      return (SearchOptions.NumTagsToRetrieve < int.MaxValue) ? tags.Take(SearchOptions.NumTagsToRetrieve) : tags;
    }

    /// <summary>
    /// Gets a list of all tags in the gallery, sorted and filtered as specified and optionally excluding tags solely associated
    /// with private albums. The logged on user's permission is not used to restrict results. This function ignores any
    /// <see cref="TagSearchOptions.SearchTerm" /> that may be specified. The data may be returned from a cache.
    /// </summary>
    /// <param name="excludePrivateTags">If set to <c>true</c>, exclude tags that apply exclusively to private albums. Set to 
    /// <c>true</c> when you want a list of tags to display to an anonymous user.</param>
    /// <returns><see cref="IEnumerable{T}" /> where T is <see cref="Entity.Tag" />.</returns>
    private IEnumerable<Entity.Tag> GetTags(bool excludePrivateTags)
    {
      var tagsCache = CacheController.GetTagsCache();

      List<Entity.TagCacheItem> tags;
      if ((tagsCache == null) || !tagsCache.TryGetValue(GetTagCacheKey(), out tags))
      {
        // No tags cache exists or the specific entry in the tag cache is not found. Retrieve from DB and add to cache.
        tags = GetAllSortedTagsFromDb();

        if (tagsCache == null)
        {
          tagsCache = new ConcurrentDictionary<string, List<Entity.TagCacheItem>>();
        }

        tagsCache.TryAdd(GetTagCacheKey(), tags);

        CacheController.SetCache(CacheItem.Tags, tagsCache);
      }

      IQueryable<Entity.Tag> tagsQueryable;

      if (excludePrivateTags)
      {
        tagsQueryable = tags.AsQueryable().Select(t => new Entity.Tag {Value = t.Value, Count = t.CountAll - t.CountPrivate}).Where(t => t.Count > 0);
      }
      else
      {
        tagsQueryable = tags.AsQueryable().Select(t => new Entity.Tag {Value = t.Value, Count = t.CountAll});
      }

      return Take(tagsQueryable);
    }

    /// <summary>
    /// Generates a string that can be used to uniquely identify a set of tags in the cache.
    /// </summary>
    /// <returns>System.String.</returns>
    private string GetTagCacheKey()
    {
      return $"{SearchOptions.GalleryId}|{TagName}|{SearchOptions.SortProperty}|{SearchOptions.SortAscending}";
    }

    /// <summary>
    /// Gets a list of tags from database for the gallery, sorted on the specified property.
    /// </summary>
    /// <returns><see cref="List{T}" /> where T is <see cref="Entity.TagCacheItem" />.</returns>
    private List<Entity.TagCacheItem> GetAllSortedTagsFromDb()
    {
      using (var repo = new MetadataTagRepository())
      {
        var query = repo.Where(t => t.FKGalleryId == SearchOptions.GalleryId && t.Metadata.MetaName == TagName)
          .GroupBy(t => t.FKTagName)
          .Select(t => new Entity.TagCacheItem
          {
            Value = t.Key,
            CountAll = t.Count(),
            CountPrivate = t.Count(mt => mt.Metadata.MediaObject.Album.IsPrivate || mt.Metadata.Album.IsPrivate)
          });

        return Sort(query).ToList();
      }
    }

    #endregion
  }
}
