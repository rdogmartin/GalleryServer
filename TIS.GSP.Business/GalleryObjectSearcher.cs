using System.Data.Entity;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.Properties;
using GalleryServer.Data;

namespace GalleryServer.Business
{
    /// <summary>
    /// Provides functionality for finding one or more gallery objects.
    /// </summary>
    public class GalleryObjectSearcher
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
        private GalleryObjectSearchOptions SearchOptions { get; set; }

        /// <summary>
        /// Gets the type of the tag to search for. Applies only when the search type is <see cref="GalleryObjectSearchType.SearchByTag" />
        /// or <see cref="GalleryObjectSearchType.SearchByPeople" />.
        /// </summary>
        /// <value>The type of the tag.</value>
        private MetadataItemName TagType
        {
            get
            {
                return (SearchOptions.SearchType == GalleryObjectSearchType.SearchByTag ? MetadataItemName.Tags : MetadataItemName.People);
            }
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

        /// <summary>
        /// Gets the root album for the gallery identified in the <see cref="SearchOptions" />.
        /// </summary>
        private IAlbum RootAlbum
        {
            get { return _rootAlbum ?? (_rootAlbum = Factory.LoadRootAlbumInstance(SearchOptions.GalleryId)); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GalleryObjectSearcher" /> class.
        /// </summary>
        /// <param name="searchOptions">The search options.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="searchOptions" /> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when one or more properties of the <paramref name="searchOptions" /> parameter is invalid.</exception>
        /// <exception cref="Events.CustomExceptions.InvalidGalleryException">Thrown when the gallery ID specified in the <paramref name="searchOptions" />
        /// parameter is invalid.</exception>
        public GalleryObjectSearcher(GalleryObjectSearchOptions searchOptions)
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
        /// Finds the first gallery object that matches the criteria. Use this method when a single item is expected.
        /// Returns null when no matching items are found.
        /// </summary>
        /// <returns>An instance of <see cref="IGalleryObject" /> or null.</returns>
        public IGalleryObject FindOne()
        {
            return Find().FirstOrDefault();
        }

        /// <summary>
        /// Finds all gallery objects that match the search criteria. Guaranteed to not return null.
        /// </summary>
        /// <returns>IGalleryObjectCollection.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when an implementation is not found for one of the 
        /// search types.</exception>
        public IEnumerable<IGalleryObject> Find()
        {
            switch (SearchOptions.SearchType)
            {
                case GalleryObjectSearchType.SearchByTitleOrCaption:
                    return FindItemsMatchingTitleOrCaption();
                case GalleryObjectSearchType.SearchByKeyword:
                    return FindItemsMatchingKeywords();
                case GalleryObjectSearchType.SearchByTag:
                case GalleryObjectSearchType.SearchByPeople:
                    return FindItemsMatchingTags();
                case GalleryObjectSearchType.HighestAlbumUserCanView:
                    return WrapInGalleryObjectCollection(LoadRootAlbumForUser());
                case GalleryObjectSearchType.MostRecentlyAdded:
                    return FindRecentlyAdded();
                case GalleryObjectSearchType.SearchByRating:
                    return FindMediaObjectsMatchingRating();
                default:
                    throw new InvalidOperationException(string.Format("The method GalleryObjectSearcher.Find was not designed to handle SearchType={0}. The developer must update this method.", SearchOptions.SearchType));
            }
        }

        #endregion

        #region Functions

        private IEnumerable<IGalleryObject> FindItemsMatchingTitleOrCaption()
        {
            var galleryObjects = new GalleryObjectCollection();

            if (SearchOptions.Filter == GalleryObjectType.All || SearchOptions.Filter == GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetAlbumsHavingTitleOrCaption());
            }

            if (SearchOptions.Filter != GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetMediaObjectsHavingTitleOrCaption());
            }

            var filteredGalleryObjects = FilterGalleryObjects(galleryObjects);

            return (SearchOptions.MaxNumberResults > 0 ? filteredGalleryObjects.ToSortedList().Take(SearchOptions.MaxNumberResults) : filteredGalleryObjects);
        }

        private IEnumerable<IGalleryObject> GetAlbumsHavingTitleOrCaption()
        {
            var galleryObjects = new GalleryObjectCollection();

            var metaTagsToSearch = new[] { MetadataItemName.Title, MetadataItemName.Caption };

            using (var repo = new AlbumRepository())
            {
                var qry = repo.Where(a => true, a => a.Metadata);

                qry = SearchOptions.SearchTerms.Aggregate(qry, (current, searchTerm) => current.Where(a =>
                  a.FKGalleryId == SearchOptions.GalleryId &&
                  a.Metadata.Any(md => metaTagsToSearch.Contains(md.MetaName) && md.Value.Contains(searchTerm))));

                qry = RestrictForCurrentUser(qry);

                foreach (var album in qry)
                {
                    galleryObjects.Add(Factory.GetAlbumFromDto(album));
                }
            }

            return galleryObjects;
        }

        private IEnumerable<IGalleryObject> GetMediaObjectsHavingTitleOrCaption()
        {
            var galleryObjects = new GalleryObjectCollection();

            var metaTagsToSearch = new[] { MetadataItemName.Title, MetadataItemName.Caption };

            using (var repo = new MediaObjectRepository())
            {
                var qry = repo.Where(a => true, a => a.Metadata);

                qry = RestrictForCurrentUser(qry);

                qry = SearchOptions.SearchTerms.Aggregate(qry, (current, searchTerm) => current.Where(mo =>
                  mo.Album.FKGalleryId == SearchOptions.GalleryId &&
                  mo.Metadata.Any(md => metaTagsToSearch.Contains(md.MetaName) && md.Value.Contains(searchTerm))));

                foreach (var mediaObject in qry)
                {
                    galleryObjects.Add(Factory.GetMediaObjectFromDto(mediaObject, null));
                }
            }

            return galleryObjects;
        }

        private IEnumerable<IGalleryObject> FindItemsMatchingKeywords()
        {
            var galleryObjects = new GalleryObjectCollection();

            if (SearchOptions.Filter == GalleryObjectType.All || SearchOptions.Filter == GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetAlbumsMatchingKeywords());
            }

            if (SearchOptions.Filter != GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetMediaObjectsMatchingKeywords());
            }

            var filteredGalleryObjects = FilterGalleryObjects(galleryObjects);

            return (SearchOptions.MaxNumberResults > 0 ? filteredGalleryObjects.ToSortedList().Take(SearchOptions.MaxNumberResults) : filteredGalleryObjects);
        }

        private IEnumerable<IGalleryObject> GetAlbumsMatchingKeywords()
        {
            var galleryObjects = new GalleryObjectCollection();

            using (var repo = new AlbumRepository())
            {
                var qry = repo.Where(a => true, a => a.Metadata);

                qry = SearchOptions.SearchTerms.Aggregate(qry, (current, searchTerm) => current.Where(a =>
                  a.FKGalleryId == SearchOptions.GalleryId &&
                  a.Metadata.Any(md => md.Value.Contains(searchTerm))));

                qry = RestrictForCurrentUser(qry);

                foreach (var album in qry)
                {
                    galleryObjects.Add(Factory.GetAlbumFromDto(album));
                }
            }

            return galleryObjects;
        }

        private IEnumerable<IGalleryObject> GetMediaObjectsMatchingKeywords()
        {
            var galleryObjects = new GalleryObjectCollection();

            using (var repo = new MediaObjectRepository())
            {
                var qry = repo.Where(a => true, a => a.Metadata);

                qry = SearchOptions.SearchTerms.Aggregate(qry, (current, searchTerm) => current.Where(mo =>
                  mo.Album.FKGalleryId == SearchOptions.GalleryId &&
                  mo.Metadata.Any(md => md.Value.Contains(searchTerm))));

                qry = RestrictForCurrentUser(qry);

                foreach (var mediaObject in qry)
                {
                    galleryObjects.Add(Factory.GetMediaObjectFromDto(mediaObject, null));
                }
            }

            return galleryObjects;
        }

        /// <summary>
        /// Validates the specified search options. Throws an exception if not valid.
        /// </summary>
        /// <param name="searchOptions">The search options.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="searchOptions" /> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when one or more properties of the <paramref name="searchOptions" /> parameter is invalid.</exception>
        /// <exception cref="Events.CustomExceptions.InvalidGalleryException">Thrown when the gallery ID specified in the <paramref name="searchOptions" />
        /// parameter is invalid.</exception>
        private static void Validate(GalleryObjectSearchOptions searchOptions)
        {
            if (searchOptions == null)
                throw new ArgumentNullException("searchOptions");

            if (searchOptions.SearchType == GalleryObjectSearchType.NotSpecified)
                throw new ArgumentException("The SearchType property of the searchOptions parameter must be set to a valid search type.");

            if (searchOptions.IsUserAuthenticated && searchOptions.Roles == null)
                throw new ArgumentException("The Roles property of the searchOptions parameter must be specified when IsUserAuthenticated is true.");

            if (searchOptions.GalleryId < 0) // v3+ galleries start at 1, but galleries from earlier versions begin at 0
                throw new ArgumentException("Invalid gallery ID. The GalleryId property of the searchOptions parameter must refer to a valid gallery.");

            if ((searchOptions.SearchType == GalleryObjectSearchType.SearchByTag || searchOptions.SearchType == GalleryObjectSearchType.SearchByPeople) && (searchOptions.Tags == null || searchOptions.Tags.Length == 0))
                throw new ArgumentException("The Tags property of the searchOptions parameter must be specified when SearchType is SearchByTag or SearchByPeople.");

            if (searchOptions.SearchType == GalleryObjectSearchType.SearchByRating && (searchOptions.SearchTerms == null || searchOptions.SearchTerms.Length != 1))
                throw new ArgumentException("The SearchTerms property of the searchOptions parameter must contain a single string matching one of these values: highest, lowest, none, or a number from 0 to 5.");

            // This throws an exception when gallery ID doesn't exist or is the template gallery.
            Factory.LoadGallery(searchOptions.GalleryId);

            if (searchOptions.Filter == GalleryObjectType.Unknown || searchOptions.Filter == GalleryObjectType.NotSpecified)
                throw new ArgumentException(String.Format("The Filter property of the searchOptions parameter cannot be GalleryObjectType.{0}.", searchOptions.Filter));
        }

        /// <summary>
        /// Finds the gallery objects matching tags. Guaranteed to not return null. Call this function only when the search type
        /// is <see cref="GalleryObjectSearchType.SearchByTag" /> or <see cref="GalleryObjectSearchType.SearchByPeople" />.
        /// Only items the user has permission to view are returned.
        /// </summary>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        private IEnumerable<IGalleryObject> FindItemsMatchingTags()
        {
            var galleryObjects = new GalleryObjectCollection();

            if (SearchOptions.Filter == GalleryObjectType.All || SearchOptions.Filter == GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetAlbumsHavingTags());
            }

            if (SearchOptions.Filter != GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetMediaObjectsHavingTags());
            }

            var filteredGalleryObjects = FilterGalleryObjects(galleryObjects);

            return (SearchOptions.MaxNumberResults > 0 ? filteredGalleryObjects.ToSortedList().Take(SearchOptions.MaxNumberResults) : filteredGalleryObjects);
        }

        /// <summary>
        /// Gets the albums having all tags specified in the search options. Guaranteed to not return null. Only albums the 
        /// user has permission to view are returned.
        /// </summary>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        private IGalleryObjectCollection GetAlbumsHavingTags()
        {
            var galleryObjects = new GalleryObjectCollection();

            using (var repo = new AlbumRepository())
            {
                var qry = repo.Where(a =>
                  a.FKGalleryId == SearchOptions.GalleryId &&
                  a.Metadata.Any(md => md.MetaName == TagType && md.MetadataTags.Any(mdt => SearchOptions.Tags.Contains(mdt.FKTagName))), a => a.Metadata);

                foreach (var albumDto in RestrictForCurrentUser(qry))
                {
                    var album = Factory.GetAlbumFromDto(albumDto);

                    // We have an album that contains at least one of the tags. If we have multiple tags, do an extra test to ensure
                    // album matches ALL of them. (I wasn't able to write the LINQ to do this for me, so it's an extra step.)
                    if (SearchOptions.Tags.Length == 1)
                    {
                        galleryObjects.Add(album);
                    }
                    else if (MetadataItemContainsAllTags(album.MetadataItems.First(md => md.MetadataItemName == TagType)))
                    {
                        galleryObjects.Add(album);
                    }
                }
            }

            return galleryObjects;
        }

        /// <summary>
        /// Determines whether the current user can view the specified <paramref name="album" />.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <returns><c>true</c> if the user can view the album; otherwise, <c>false</c>.</returns>
        private bool CanUserViewAlbum(IAlbum album)
        {
            return SecurityManager.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, SearchOptions.Roles, album.Id, SearchOptions.GalleryId, SearchOptions.IsUserAuthenticated, album.IsPrivate, SecurityActionsOption.RequireOne, album.IsVirtualAlbum);
        }

        /// <summary>
        /// Returns a value indicating whether the <paramref name="mdItem" /> contains ALL the tags contained in SearchOptions.Tags.
        /// The comparison is case insensitive.
        /// </summary>
        /// <param name="mdItem">The metadata item.</param>
        /// <returns><c>true</c> if the metadata item contains all the tags, <c>false</c> otherwise</returns>
        private bool MetadataItemContainsAllTags(IGalleryObjectMetadataItem mdItem)
        {
            // First split the meta value into the separate tag items, trimming and converting to lower case.
            var albumTags = mdItem.Value.ToLowerInvariant().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

            // Now make sure that albumTags contains ALL the items in SearchOptions.Tags.
            return SearchOptions.Tags.Aggregate(true, (current, tag) => current & albumTags.Contains(tag.ToLowerInvariant()));
        }

        /// <summary>
        /// Gets the media objects having all tags specified in the search options. Guaranteed to not return null. 
        /// Only media objects the user has permission to view are returned.
        /// </summary>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        private IGalleryObjectCollection GetMediaObjectsHavingTags()
        {
            var galleryObjects = new GalleryObjectCollection();

            using (var repo = new MediaObjectRepository())
            {
                var qry = repo.Where(m =>
                  m.Album.FKGalleryId == SearchOptions.GalleryId &&
                  m.Metadata.Any(md => md.MetaName == TagType && md.MetadataTags.Any(mdt => SearchOptions.Tags.Contains(mdt.FKTagName))), m => m.Metadata);

                foreach (var moDto in RestrictForCurrentUser(qry))
                {
                    var mediaObject = Factory.GetMediaObjectFromDto(moDto, null);

                    // We have a media object that contains at least one of the tags. If we have multiple tags, do an extra test to ensure
                    // media object matches ALL of them. (I wasn't able to write the LINQ to do this for me, so it's an extra step.)
                    if (SearchOptions.Tags.Length == 1)
                    {
                        galleryObjects.Add(mediaObject);
                    }
                    else if (MetadataItemContainsAllTags(mediaObject.MetadataItems.First(md => md.MetadataItemName == TagType)))
                    {
                        galleryObjects.Add(mediaObject);
                    }
                }
            }

            return galleryObjects;
        }

        /// <summary>
        /// Gets the top level album the current user has permission to view. Returns null when the user does not 
        /// have permission to view any albums.
        /// </summary>
        /// <returns>An instance of <see cref="IAlbum" /> or null.</returns>
        private IAlbum LoadRootAlbumForUser()
        {
            // Get list of root album IDs with view permission.

            // Step 1: Compile a list of album IDs having the requested permissions.
            var rootAlbums = GetRootAlbumsUserCanView();

            // Step 3: Package results into an album container. If there is only one viewable root album, then just create an instance of that album.
            // Otherwise, create a virtual root album to contain the multiple viewable albums.
            IAlbum rootAlbum;

            if (rootAlbums.Count == 0)
                return null;

            if (rootAlbums.Count == 1)
            {
                rootAlbum = rootAlbums[0];
            }
            else
            {
                // Create virtual album to serve as a container for the child albums the user has permission to view.
                rootAlbum = Factory.CreateEmptyAlbumInstance(SearchOptions.GalleryId);
                rootAlbum.IsVirtualAlbum = true;
                rootAlbum.VirtualAlbumType = VirtualAlbumType.Root;
                rootAlbum.Title = Resources.Virtual_Album_Title;
                rootAlbum.Caption = String.Empty;
                rootAlbum.IsInflated = true;
                foreach (var album in rootAlbums)
                {
                    rootAlbum.AddGalleryObject(album);
                }
            }

            return rootAlbum;
        }

        /// <summary>
        /// Gets a list of the top-level albums the current user can view. Guaranteed to not return null. Will be empty 
        /// if user does not have access to any albums.
        /// </summary>
        /// <returns>An instance of <see cref="List{IAlbum}" />.</returns>
        private List<IAlbum> GetRootAlbumsUserCanView()
        {
            // If user can view the root album, just return that.
            var rootAlbum = Factory.LoadRootAlbumInstance(SearchOptions.GalleryId);

            if (CanUserViewAlbum(rootAlbum))
            {
                return new List<IAlbum>() { rootAlbum };
            }
            else if (!SearchOptions.IsUserAuthenticated)
            {
                // Anonymous user can't view any albums, so just return an empty list.
                return new List<IAlbum>();
            }

            // Logged on user can't see root album, so calculate the top-level list of album IDs they *can* see.
            var allRootAlbumIds = SearchOptions.Roles.GetViewableAlbumIdsForGallery(SearchOptions.GalleryId);

            // Step 2: Convert previous list to contain ONLY top-level albums in the current gallery.
            var rootAlbums = RemoveChildAlbumsAndAlbumsInOtherGalleries(allRootAlbumIds);

            return rootAlbums;
        }

        /// <summary>
        /// Generate a new list containing a subset of <paramref name="allRootAlbumIds" /> that contains only a list of 
        /// top-level album IDs and albums belonging to the gallery specified in the search options.
        /// Any albums that have a parent - at any level - in the list are not included. Guaranteed to not return null.
        /// </summary>
        /// <param name="allRootAlbumIds">All album IDs to process.</param>
        /// <returns>Returns an enumerable list of integers representing the album IDs that satisfy the criteria.</returns>
        private List<IAlbum> RemoveChildAlbumsAndAlbumsInOtherGalleries(IEnumerable<int> allRootAlbumIds)
        {
            // Loop through our list of album IDs. If any album has an ancestor that is also in the list, then remove it. 
            // We only want a list of top level albums.
            var rootAlbums = new List<IAlbum>();
            var albumsToRemove = new List<IAlbum>();
            foreach (int viewableAlbumId in allRootAlbumIds)
            {
                var album = Factory.LoadAlbumInstance(viewableAlbumId);

                if (album.GalleryId != SearchOptions.GalleryId)
                {
                    // The album belongs to a different gallery, so skip it. It won't get included in the returned collection.
                    continue;
                }

                rootAlbums.Add(album);

                var albumParent = album;

                while (true)
                {
                    albumParent = albumParent.Parent as IAlbum;
                    if (albumParent == null)
                        break;

                    if (allRootAlbumIds.Contains(albumParent.Id))
                    {
                        albumsToRemove.Add(album);
                        break;
                    }
                }
            }
            foreach (var album in albumsToRemove)
            {
                rootAlbums.Remove(album);
            }

            return rootAlbums;
        }

        /// <summary>
        /// Wraps the <paramref name="album" /> in a gallery object collection. When <paramref name="album" /> is null,
        /// an empty collection is returned. Guaranteed to no return null. 
        /// </summary>
        /// <param name="album">The album.</param>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        private static IEnumerable<IGalleryObject> WrapInGalleryObjectCollection(IAlbum album)
        {
            var result = new GalleryObjectCollection();

            if (album != null)
                result.Add(album);

            return result;
        }

        /// <summary>
        /// Finds the gallery objects that have been recently added to the gallery. Guaranteed to not return null.
        /// Only items the current user is authorized to view are returned.
        /// </summary>
        /// <returns><see cref="IEnumerable&lt;IGalleryObject&gt;" />.</returns>
        private IEnumerable<IGalleryObject> FindRecentlyAdded()
        {
            var galleryObjects = new GalleryObjectCollection();

            if (SearchOptions.Filter == GalleryObjectType.All || SearchOptions.Filter == GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetRecentlyAddedAlbums(SearchOptions.MaxNumberResults));
            }

            if (SearchOptions.Filter != GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetRecentlyAddedMediaObjects(SearchOptions.MaxNumberResults));
            }

            var filteredGalleryObjects = FilterGalleryObjects(galleryObjects);

            if (filteredGalleryObjects.Count != galleryObjects.Count && filteredGalleryObjects.Count < SearchOptions.MaxNumberResults && galleryObjects.Count >= SearchOptions.MaxNumberResults)
            {
                // We lost some objects in the filter and now we have less than the desired MaxNumberResults. Get more.
                // Note: Performance can be very poor for large galleries when using a filter. For example, a gallery where 20 videos
                // were added and then 200,000 images were added, a search for the most recent 20 videos causes this algorithm
                // to load all 200,000 images into memory before finding the videos. The good news is that by default the filter
                // is for media objects, which will be very fast. If filters end up being commonly used, this algorithm should be improved.
                var max = SearchOptions.MaxNumberResults * 2;
                var skip = SearchOptions.MaxNumberResults;
                const int maxTries = 5;

                for (var i = 0; i < maxTries; i++)
                {
                    // Add items up to maxTries times, each time doubling the number of items to retrieve.
                    filteredGalleryObjects.AddRange(GetRecentlyAddedAlbums(max, skip));
                    filteredGalleryObjects.AddRange(GetRecentlyAddedMediaObjects(max, skip));

                    filteredGalleryObjects = FilterGalleryObjects(filteredGalleryObjects);

                    if (filteredGalleryObjects.Count >= SearchOptions.MaxNumberResults)
                    {
                        break;
                    }

                    if (i < (maxTries - 1))
                    {
                        skip = skip + max;
                        max = max * 2;
                    }
                }

                if (filteredGalleryObjects.Count < SearchOptions.MaxNumberResults)
                {
                    // We still don't have enough objects. Search entire set of albums and media objects.
                    filteredGalleryObjects.AddRange(GetRecentlyAddedAlbums(int.MaxValue, skip));
                    filteredGalleryObjects.AddRange(GetRecentlyAddedMediaObjects(int.MaxValue, skip));

                    filteredGalleryObjects = FilterGalleryObjects(filteredGalleryObjects);
                }
            }

            if (SearchOptions.MaxNumberResults > 0 && filteredGalleryObjects.Count > SearchOptions.MaxNumberResults)
            {
                return filteredGalleryObjects.OrderByDescending(g => g.DateAdded).Take(SearchOptions.MaxNumberResults);
            }

            return filteredGalleryObjects;
        }

        /// <summary>
        /// Gets the <paramref name="top" /> most recently added albums, skipping the first
        /// <paramref name="skip" /> objects. Only albums the current user is authorized to
        /// view are returned.
        /// </summary>
        /// <param name="top">The number of items to retrieve.</param>
        /// <param name="skip">The number of items to skip over in the data store.</param>
        /// <returns><see cref="IEnumerable&lt;IGalleryObject&gt;" />.</returns>
        private IEnumerable<IGalleryObject> GetRecentlyAddedAlbums(int top, int skip = 0)
        {
            var galleryObjects = new GalleryObjectCollection();

            using (var repo = new AlbumRepository())
            {
                var qry = RestrictForCurrentUser(repo.Where(mo => mo.FKGalleryId == SearchOptions.GalleryId)
                  .OrderByDescending(m => m.DateAdded))
                  .Skip(skip).Take(top)
                  .Include(m => m.Metadata);

                foreach (var album in qry)
                {
                    galleryObjects.Add(Factory.GetAlbumFromDto(album));
                }
            }

            return galleryObjects;
        }

        /// <summary>
        /// Gets the <paramref name="top" /> most recently added media objects, skipping the first
        /// <paramref name="skip" /> objects. Only media objects the current user is authorized to
        /// view are returned.
        /// </summary>
        /// <param name="top">The number of items to retrieve.</param>
        /// <param name="skip">The number of items to skip over in the data store.</param>
        /// <returns><see cref="IEnumerable&lt;IGalleryObject&gt;" />.</returns>
        private IEnumerable<IGalleryObject> GetRecentlyAddedMediaObjects(int top, int skip = 0)
        {
            var galleryObjects = new GalleryObjectCollection();

            using (var repo = new MediaObjectRepository())
            {
                var qry = RestrictForCurrentUser(repo.Where(mo => mo.Album.FKGalleryId == SearchOptions.GalleryId)
                  .OrderByDescending(m => m.DateAdded))
                  .Skip(skip).Take(top)
                  .Include(m => m.Metadata);

                foreach (var mediaObject in qry)
                {
                    galleryObjects.Add(Factory.GetMediaObjectFromDto(mediaObject, null));
                }
            }

            return galleryObjects;
        }

        /// <summary>
        /// Finds the gallery objects with the specified rating. Guaranteed to not return null. Albums cannot be 
        /// rated and are thus not returned. Only items the current user is authorized to view are returned.
        /// </summary>
        /// <returns><see cref="IEnumerable&lt;IGalleryObject&gt;" />.</returns>
        private IEnumerable<IGalleryObject> FindMediaObjectsMatchingRating()
        {
            var galleryObjects = new GalleryObjectCollection();

            if (SearchOptions.Filter != GalleryObjectType.Album)
            {
                galleryObjects.AddRange(GetRatedMediaObjects(SearchOptions.MaxNumberResults));
            }

            var filteredGalleryObjects = FilterGalleryObjects(galleryObjects);

            if (filteredGalleryObjects.Count != galleryObjects.Count && filteredGalleryObjects.Count < SearchOptions.MaxNumberResults && galleryObjects.Count >= SearchOptions.MaxNumberResults)
            {
                // We lost some objects in the filter and now we have less than the desired MaxNumberResults. Get more.
                // Note: Performance can be very poor for large galleries when using a filter. For example, a gallery where 20 videos
                // were added and then 200,000 images were added, a search for the most recent 20 videos causes this algorithm
                // to load all 200,000 images into memory before finding the videos. The good news is that by default the filter
                // is for media objects, which will be very fast. If filters end up being commonly used, this algorithm should be improved.
                var max = SearchOptions.MaxNumberResults * 2;
                var skip = SearchOptions.MaxNumberResults;
                const int maxTries = 5;

                for (var i = 0; i < maxTries; i++)
                {
                    // Add items up to maxTries times, each time doubling the number of items to retrieve.
                    filteredGalleryObjects.AddRange(GetRatedMediaObjects(max, skip));

                    filteredGalleryObjects = FilterGalleryObjects(filteredGalleryObjects);

                    if (filteredGalleryObjects.Count >= SearchOptions.MaxNumberResults)
                    {
                        break;
                    }

                    if (i < (maxTries - 1))
                    {
                        skip = skip + max;
                        max = max * 2;
                    }
                }

                if (filteredGalleryObjects.Count < SearchOptions.MaxNumberResults)
                {
                    // We still don't have enough objects. Search entire set of albums and media objects.
                    filteredGalleryObjects.AddRange(GetRatedMediaObjects(int.MaxValue, skip));

                    filteredGalleryObjects = FilterGalleryObjects(filteredGalleryObjects);
                }
            }

            if (SearchOptions.MaxNumberResults > 0 && filteredGalleryObjects.Count > SearchOptions.MaxNumberResults)
            {
                return filteredGalleryObjects.OrderByDescending(g => g.DateAdded).Take(SearchOptions.MaxNumberResults);
            }

            return filteredGalleryObjects;
        }

        /// <summary>
        /// Gets the <paramref name="top" /> media objects having the specified rating, skipping the first
        /// <paramref name="skip" /> objects. Only media objects the current user is authorized to
        /// view are returned. Albums cannot be rated and are thus not returned.
        /// </summary>
        /// <param name="top">The number of items to retrieve.</param>
        /// <param name="skip">The number of items to skip over in the data store.</param>
        /// <returns><see cref="IEnumerable&lt;IGalleryObject&gt;" />.</returns>
        /// <remarks>
        /// SQL CE does not support the queries used in this function. We should never get here because Utils.GetTopRatedUrl()
        /// only generates an URL for SQL Server DB's, but better safe than sorry. FYI, these are the errors SQL CE generates:
        /// Caused by String.IsNullOrEmpty(md.Value): "The specified argument value for the function is not valid. [ Argument # = 1,Name of function(if known) = LEN ]"
        /// Caused by using md.Value in OrderBy: "Large objects (ntext and image) cannot be used in ORDER BY clauses."
        /// One way to make these queries work is to change Metadata.Value from NTEXT to NVARCHAR(4000), but this is undesirable because we
        /// want the max length to handle long meta values, especially in SQL Server, where these queries work fine. I looked into adding
        /// conditional logic to change the column definition for SQL CE but leaving it unchanged for SQL Server, but it was complex and had risk.
        /// </remarks>
        private IEnumerable<IGalleryObject> GetRatedMediaObjects(int top, int skip = 0)
        {
            var galleryObjects = new GalleryObjectCollection();

            if (AppSetting.Instance.ProviderDataStore == ProviderDataStore.SqlCe)
            {
                throw new NotSupportedException("SQL CE does not support the query syntax used in GalleryObjectSearcher.GetRatedMediaObjects(). Use SQL Server instead.");
            }

            using (var repo = new MediaObjectRepository())
            {
                IQueryable<MediaObjectDto> qry;

                switch (SearchOptions.SearchTerms[0].ToLowerInvariant())
                {
                    case "highest": // Highest rated objects
                        qry = RestrictForCurrentUser(repo.Where(mo =>
                          mo.Album.FKGalleryId == SearchOptions.GalleryId
                          && mo.Metadata.Any(md => md.MetaName == MetadataItemName.Rating && !String.IsNullOrEmpty(md.Value)))
                          .OrderByDescending(mo => mo.Metadata.Where(md => md.MetaName == MetadataItemName.Rating).Select(md => md.Value).FirstOrDefault())
                          .Include(mo => mo.Metadata)
                          .Skip(skip).Take(top));
                        break;

                    case "lowest": // Lowest rated objects
                        qry = RestrictForCurrentUser(repo.Where(mo =>
                          mo.Album.FKGalleryId == SearchOptions.GalleryId
                          //&& mo.Metadata.Any(md => md.MetaName == MetadataItemName.Rating && md.Value == "5"))
                          && mo.Metadata.Any(md => md.MetaName == MetadataItemName.Rating && !String.IsNullOrEmpty(md.Value)))
                          .OrderBy(mo => mo.Metadata.Where(md => md.MetaName == MetadataItemName.Rating).Select(md => md.Value).FirstOrDefault())
                          .Include(mo => mo.Metadata)
                          .Skip(skip).Take(top));
                        break;

                    case "none": // Having no rating
                        qry = RestrictForCurrentUser(repo.Where(mo =>
                          mo.Album.FKGalleryId == SearchOptions.GalleryId
                          && mo.Metadata.Any(md => md.MetaName == MetadataItemName.Rating && String.IsNullOrEmpty(md.Value)))
                          .OrderBy(mo => mo.DateAdded)
                          .Include(mo => mo.Metadata)
                          .Skip(skip).Take(top));
                        break;

                    default: // Look for a specific rating
                        var r = ParseRating(SearchOptions.SearchTerms[0]);
                        if (r != null)
                        {
                            qry = RestrictForCurrentUser(repo.Where(mo =>
                              mo.Album.FKGalleryId == SearchOptions.GalleryId
                              && mo.Metadata.Any(md => md.MetaName == MetadataItemName.Rating && r.Contains(md.Value)))
                              .OrderBy(mo => mo.DateAdded)
                              .Include(mo => mo.Metadata)
                              .Skip(skip).Take(top));
                        }
                        else
                        {
                            // The search term is a string other than highest, lowest, none or a decimal. Don't return anything.
                            qry = repo.Where(mo => false);
                        }
                        break;
                }

                foreach (var mediaObject in qry)
                {
                    galleryObjects.Add(Factory.GetMediaObjectFromDto(mediaObject, null));
                }
            }

            return galleryObjects;
        }

        /// <summary>
        /// Parses the <paramref name="rating" /> into an array of strings that may exist in the database.
        /// For example, a rating of "3" returns {"3", "3.", "3.0", "3.00", "3.000", "3.0000"}. A rating of
        /// "4.5" returns {"4.5", "4.50", "4.500", "4.5000"}. If the rating cannot be parsed into a decimal,
        /// null is returned.
        /// </summary>
        /// <param name="rating">The rating, in half step increments from 0 to 5. (eg. "3", "3.0000", "4.5", "4.5000").</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        private static IEnumerable<string> ParseRating(string rating)
        {
            string[] ratings = null;

            int ratingInt;
            if (Int32.TryParse(rating, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out ratingInt))
            {
                ratings = new[]
                  {
            ratingInt.ToString(CultureInfo.InvariantCulture), // Eg. "3"
            String.Concat(ratingInt.ToString(CultureInfo.InvariantCulture), "."), // Eg. "3."
          };
            }

            decimal ratingDecimal;
            if (Decimal.TryParse(rating, NumberStyles.Number, CultureInfo.InvariantCulture, out ratingDecimal))
            {
                if (ratings == null)
                {
                    ratings = new string[] { };
                }

                return ratings.Concat(new[]
                  {
            ratingDecimal.ToString("F1", CultureInfo.InvariantCulture), // Eg. "3.0"
            ratingDecimal.ToString("F2", CultureInfo.InvariantCulture), // Eg. "3.00"
            ratingDecimal.ToString("F3", CultureInfo.InvariantCulture), // Eg. "3.000"
            ratingDecimal.ToString("F4", CultureInfo.InvariantCulture), // Eg. "3.0000"
          });
            }

            return null;
        }

        /// <summary>
        /// Filters the <paramref name="galleryObjects" /> by the filter specified in <see cref="SearchOptions" />.
        /// </summary>
        /// <param name="galleryObjects">The gallery objects.</param>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        private IGalleryObjectCollection FilterGalleryObjects(IGalleryObjectCollection galleryObjects)
        {
            switch (SearchOptions.Filter)
            {
                case GalleryObjectType.Album:
                    return new GalleryObjectCollection(galleryObjects.Where(go => go.GalleryObjectType == GalleryObjectType.Album));

                case GalleryObjectType.MediaObject:
                    return new GalleryObjectCollection(galleryObjects.Where(go => go.GalleryObjectType != GalleryObjectType.Album));

                case GalleryObjectType.NotSpecified:
                case GalleryObjectType.All:
                    return galleryObjects;

                case GalleryObjectType.None:
                    return new GalleryObjectCollection();

                default:
                    return new GalleryObjectCollection(galleryObjects.Where(go => go.GalleryObjectType == SearchOptions.Filter));
            }
        }

        /// <summary>
        /// Modify the <paramref name="qry" /> so that it only returns albums the current user has permission
        /// to view.
        /// </summary>
        /// <param name="qry">The query.</param>
        /// <returns><see cref="IQueryable&lt;AlbumDto&gt;" />.</returns>
        private IQueryable<AlbumDto> RestrictForCurrentUser(IQueryable<AlbumDto> qry)
        {
            if (SearchOptions.IsUserAuthenticated)
            {
                var rootAlbum = Factory.LoadRootAlbumInstance(SearchOptions.GalleryId);

                if (!CanUserViewAlbum(rootAlbum))
                {
                    // User can't view the root album, so get a list of the albums she *can* see and make sure our 
                    // results only include albums that are viewable.
                    var albumIds = SearchOptions.Roles.GetViewableAlbumIdsForGallery(SearchOptions.GalleryId);

                    qry = qry.Where(a => albumIds.Contains(a.AlbumId));
                }
            }
            else if (Factory.LoadGallerySetting(SearchOptions.GalleryId).AllowAnonymousBrowsing)
            {
                // Anonymous user, so don't include any private albums in results.
                qry = qry.Where(m => !m.IsPrivate);
            }
            else
            {
                // Anonymous user & gallery is configured to prevent anonymous users, so force query to return nothing.
                qry = qry.Where(a => false);
            }

            return qry;
        }

        /// <summary>
        /// Modify the <paramref name="qry" /> so that it only returns media objects the current user has permission
        /// to view.
        /// </summary>
        /// <param name="qry">The query.</param>
        /// <returns><see cref="IQueryable&lt;MediaObjectDto&gt;" />.</returns>
        private IQueryable<MediaObjectDto> RestrictForCurrentUser(IQueryable<MediaObjectDto> qry)
        {
            if (SearchOptions.IsUserAuthenticated)
            {
                var rootAlbum = Factory.LoadRootAlbumInstance(SearchOptions.GalleryId);

                if (!CanUserViewAlbum(rootAlbum))
                {
                    // User can't view the root album, so get a list of the albums she *can* see and make sure our 
                    // results only include media objects that are viewable.
                    var albumIds = SearchOptions.Roles.GetViewableAlbumIdsForGallery(SearchOptions.GalleryId);

                    qry = qry.Where(a => albumIds.Contains(a.Album.AlbumId));
                }
            }
            else if (Factory.LoadGallerySetting(SearchOptions.GalleryId).AllowAnonymousBrowsing)
            {
                // Anonymous user, so don't include any private albums in results.
                qry = qry.Where(m => !m.IsPrivate);
            }
            else
            {
                // Anonymous user & gallery is configured to prevent anonymous users, so force query to return nothing.
                qry = qry.Where(a => false);
            }

            return qry;
        }

        #endregion
    }
}
