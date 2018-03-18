using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.NullObjects;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Controller
{
    /// <summary>
    /// Contains functionality for interacting with albums. Typically web pages directly call the appropriate business layer objects,
    /// but when a task involves multiple steps or the functionality does not exist in the business layer, the methods here are
    /// used.
    /// </summary>
    public static class AlbumController
    {
        #region Public Static Methods

        /// <overloads>
        /// Generate an <see cref="IAlbum" /> instance.
        /// </overloads>
        /// <summary>
        /// Generate a read-only <see cref="IAlbum" /> instance where child objects are not inflated. Use the overload method
        /// <see cref="LoadAlbumInstance(AlbumLoadOptions)" /> if you want a writable instance or other changes in default behavior.
        /// The album may be retrieved from cache. The album's <see cref="IAlbum.ThumbnailMediaObjectId" /> property is set to its value 
        /// from the data store, but the <see cref="IGalleryObject.Thumbnail" /> property is only inflated when accessed. Guaranteed to not return null. 
        /// This function DOES NOT VERIFY that the current user has access to the album - it is expected the caller will do that if necessary.
        /// </summary>
        /// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album to retrieve.</param>
        /// <returns>Returns an instance implementing <see cref="IAlbum" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name = "albumId" /> 
        /// is not found in the data store.</exception>
        public static IAlbum LoadAlbumInstance(int albumId)
        {
            return LoadAlbumInstance(new AlbumLoadOptions(albumId));
        }

        /// <summary>
        /// Generate an <see cref="IAlbum" /> instance conforming to the specified <paramref name="options" />. When options are set to default
        /// values, the album is read-only with child objects not inflated. The album may be retrieved from cache. Guaranteed to not return null. 
        /// This function DOES NOT VERIFY that the current user has access to the album - it is expected the caller will do that if necessary.
        /// </summary>
        /// <param name="options">The options that specify the configuration of the returned album.</param>
        /// <returns>An instance implementing <see cref="IAlbum" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the <see cref="AlbumLoadOptions.AlbumId" /> property of <paramref name="options" />
        /// does not represent a valid album.</exception>
        public static IAlbum LoadAlbumInstance(AlbumLoadOptions options)
        {
            IAlbum album = Factory.LoadAlbumInstance(options);

            ValidateAlbumOwner(album);

            return album;
        }

        /// <summary>
        /// Creates an album, assigns the user name as the owner, saves it, and returns the newly created album.
        /// A profile entry is created containing the album ID. Returns null if the ID specified in the gallery settings
        /// for the parent album does not represent an existing album. That is, returns null if <see cref="IGallerySettings.UserAlbumParentAlbumId" />
        /// does not match an existing album.
        /// </summary>
        /// <param name="userName">The user name representing the user who is the owner of the album.</param>
        /// <param name="galleryId">The gallery ID for the gallery in which the album is to be created.</param>
        /// <returns>
        /// Returns the newly created user album. It has already been persisted to the database.
        /// Returns null if the ID specified in the gallery settings for the parent album does not represent an existing album.
        /// That is, returns null if <see cref="IGallerySettings.UserAlbumParentAlbumId" />
        /// does not match an existing album.
        /// </returns>
        public static IAlbum CreateUserAlbum(string userName, int galleryId)
        {
            IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

            string albumNameTemplate = gallerySetting.UserAlbumNameTemplate;

            IAlbum parentAlbum;
            try
            {
                parentAlbum = AlbumController.LoadAlbumInstance(gallerySetting.UserAlbumParentAlbumId);
            }
            catch (InvalidAlbumException ex)
            {
                // The parent album does not exist. Record the error and return null.
                string galleryDescription = Utils.HtmlEncode(Factory.LoadGallery(gallerySetting.GalleryId).Description);
                string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Error_User_Album_Parent_Invalid_Ex_Msg, galleryDescription, gallerySetting.UserAlbumParentAlbumId);
                AppEventController.LogError(new WebException(msg, ex), galleryId);
                return null;
            }

            IAlbum album = Factory.CreateEmptyAlbumInstance(parentAlbum.GalleryId, true);

            album.Title = albumNameTemplate.Replace("{UserName}", userName);
            album.Caption = gallerySetting.UserAlbumSummaryTemplate;
            album.OwnerUserName = userName;
            //newAlbum.ThumbnailMediaObjectId = 0; // not needed
            album.Parent = parentAlbum;
            album.IsPrivate = parentAlbum.IsPrivate;
            GalleryObjectController.SaveGalleryObject(album, userName);

            SaveAlbumIdToProfile(album.Id, userName, album.GalleryId);

            return album;
        }

        /// <summary>
        /// Get a reference to the highest level album in the specified <paramref name="galleryId" /> the current user has permission 
        /// to add albums to. Returns null if no album meets this criteria.
        /// </summary>
        /// <param name="galleryId">The ID of the gallery.</param>
        /// <returns>Returns a reference to the highest level album the user has permission to add albums to.</returns>
        public static IAlbum GetHighestLevelAlbumWithCreatePermission(int galleryId)
        {
            // Step 1: Loop through the roles and compile a list of album IDs where the role has create album permission.
            IGallery gallery = Factory.LoadGallery(galleryId);
            List<int> rootAlbumIdsWithCreatePermission = new List<int>();

            foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser())
            {
                if (role.Galleries.Contains(gallery))
                {
                    if (role.AllowAddChildAlbum)
                    {
                        foreach (int albumId in role.RootAlbumIds)
                        {
                            if (!rootAlbumIdsWithCreatePermission.Contains(albumId))
                                rootAlbumIdsWithCreatePermission.Add(albumId);
                        }
                    }
                }
            }

            // Step 2: Loop through our list of album IDs. If any album belongs to another gallery, remove it. If any album has an ancestor 
            // that is also in the list, then remove it. We only want a list of top level albums.
            List<int> albumIdsToRemove = new List<int>();
            foreach (int albumIdWithCreatePermission in rootAlbumIdsWithCreatePermission)
            {
                IGalleryObject album = AlbumController.LoadAlbumInstance(albumIdWithCreatePermission);

                if (album.GalleryId != galleryId)
                {
                    // Album belongs to another gallery. Mark it for deletion.
                    albumIdsToRemove.Add(albumIdWithCreatePermission);
                }
                else
                {
                    while (true)
                    {
                        album = album.Parent as IAlbum;
                        if (album == null)
                            break;

                        if (rootAlbumIdsWithCreatePermission.Contains(album.Id))
                        {
                            // Album has an ancestor that is also in the list. Mark it for deletion.
                            albumIdsToRemove.Add(albumIdWithCreatePermission);
                            break;
                        }
                    }
                }
            }

            foreach (int albumId in albumIdsToRemove)
            {
                rootAlbumIdsWithCreatePermission.Remove(albumId);
            }

            // Step 3: Starting with the root album, start iterating through the child albums. When we get to
            // one in our list, we can conclude that is the highest level album for which the user has create album permission.
            return FindFirstMatchingAlbumRecursive(Factory.LoadRootAlbumInstance(galleryId), rootAlbumIdsWithCreatePermission);
        }

        /// <summary>
        /// Get a reference to the highest level album in the specified <paramref name="galleryId" /> the current user has permission to 
        /// add albums and/or media objects to. Returns null if no album meets this criteria.
        /// </summary>
        /// <param name="verifyAddAlbumPermissionExists">Specifies whether the current user must have permission to add child albums
        /// to the album.</param>
        /// <param name="verifyAddMediaObjectPermissionExists">Specifies whether the current user must have permission to add media objects
        /// to the album.</param>
        /// <param name="galleryId">The ID of the gallery.</param>
        /// <returns>
        /// Returns a reference to the highest level album the user has permission to add albums and/or media objects to.
        /// </returns>
        public static IAlbum GetHighestLevelAlbumWithAddPermission(bool verifyAddAlbumPermissionExists, bool verifyAddMediaObjectPermissionExists, int galleryId)
        {
            // Step 1: Loop through the roles and compile a list of album IDs where the role has the required permission.
            // If the verifyAddAlbumPermissionExists parameter is true, then the user must have permission to add child albums.
            // If the verifyAddMediaObjectPermissionExists parameter is true, then the user must have permission to add media objects.
            // If either parameter is false, then the absense of that permission does not disqualify an album.
            IGallery gallery = Factory.LoadGallery(galleryId);

            List<int> rootAlbumIdsWithPermission = new List<int>();
            foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser())
            {
                if (role.Galleries.Contains(gallery))
                {
                    bool albumPermGranted = (verifyAddAlbumPermissionExists ? role.AllowAddChildAlbum : true);
                    bool mediaObjectPermGranted = (verifyAddMediaObjectPermissionExists ? role.AllowAddMediaObject : true);

                    if (albumPermGranted && mediaObjectPermGranted)
                    {
                        // This role satisfies the requirements, so add each album to the list.
                        foreach (int albumId in role.RootAlbumIds)
                        {
                            if (!rootAlbumIdsWithPermission.Contains(albumId))
                                rootAlbumIdsWithPermission.Add(albumId);
                        }
                    }
                }
            }

            // Step 2: Loop through our list of album IDs. If any album belongs to another gallery, remove it. If any album has an ancestor 
            // that is also in the list, then remove it. We only want a list of top level albums.
            List<int> albumIdsToRemove = new List<int>();
            foreach (int albumIdWithPermission in rootAlbumIdsWithPermission)
            {
                IGalleryObject album = AlbumController.LoadAlbumInstance(albumIdWithPermission);

                if (album.GalleryId != galleryId)
                {
                    // Album belongs to another gallery. Mark it for deletion.
                    albumIdsToRemove.Add(albumIdWithPermission);
                }
                else
                {
                    while (true)
                    {
                        album = album.Parent as IAlbum;
                        if (album == null)
                            break;

                        if (rootAlbumIdsWithPermission.Contains(album.Id))
                        {
                            // Album has an ancestor that is also in the list. Mark it for deletion.
                            albumIdsToRemove.Add(albumIdWithPermission);
                            break;
                        }
                    }
                }
            }

            foreach (int albumId in albumIdsToRemove)
            {
                rootAlbumIdsWithPermission.Remove(albumId);
            }

            // Step 3: Starting with the root album, start iterating through the child albums. When we get to
            // one in our list, we can conclude that is the highest level album for which the user has create album permission.
            return FindFirstMatchingAlbumRecursive(Factory.LoadRootAlbumInstance(galleryId), rootAlbumIdsWithPermission);
        }

        /// <summary>
        /// Gets the meta items for the specified album <paramref name="id" />.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <returns></returns>
        /// <exception cref="GallerySecurityException">Thrown when the 
        /// user does not have view permission to the specified album.</exception>
        public static Entity.MetaItem[] GetMetaItemsForAlbum(int id)
        {
            IAlbum album = Factory.LoadAlbumInstance(id);
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

            return GalleryObjectController.ToMetaItems(album.MetadataItems.GetVisibleItems(), album);
        }

        /// <summary>
        /// Converts the <paramref name="albums" /> to an enumerable collection of
        /// <see cref="Entity.Album" /> instances. Guaranteed to not return null.
        /// </summary>
        /// <param name="albums">The albums.</param>
        /// <param name="options">The options.</param>
        /// <returns>An enumerable collection of <see cref="Entity.Album" /> instances.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="albums" /> is null.</exception>
        public static Entity.Album[] ToAlbumEntities(IList<IGalleryObject> albums, Entity.GalleryDataLoadOptions options)
        {
            if (albums == null)
                throw new ArgumentNullException("albums");

            var albumEntities = new List<Entity.Album>(albums.Count);

            foreach (IGalleryObject album in albums)
            {
                albumEntities.Add(ToAlbumEntity((IAlbum)album, options));
            }

            return albumEntities.ToArray();
        }

        /// <summary>
        /// Gets a data entity containing information about the current album. The instance can be JSON-parsed and sent to the
        /// browser. Returns null if the requested album does not exist or the user does not have permission to view it.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <param name="options">Specifies options for configuring the return data. To use default
        /// settings, specify an empty instance with properties left at default values.</param>
        /// <returns>
        /// Returns <see cref="Entity.Album" /> object containing information about the current album.
        /// </returns>
        /// <overloads>
        /// Converts the <paramref name="album" /> to an instance of <see cref="Entity.Album" />.
        ///   </overloads>
        public static Entity.Album ToAlbumEntity(IAlbum album, Entity.GalleryDataLoadOptions options)
        {
            try
            {
                return ToAlbumEntity(album, GetPermissionsEntity(album), options);
            }
            catch (InvalidAlbumException) { return null; }
            catch (GallerySecurityException) { return null; }
        }

        /// <summary>
        /// Gets a data entity containing album information for the specified <paramref name="album" />. Returns an object with empty
        /// properties if the user does not have permission to view the specified album. The instance can be JSON-parsed and sent to the
        /// browser.
        /// </summary>
        /// <param name="album">The album to convert to an instance of <see cref="Entity.Album" />.</param>
        /// <param name="perms">The permissions the current user has for the album.</param>
        /// <param name="options">Specifies options for configuring the return data. To use default
        /// settings, specify an empty instance with properties left at default values.</param>
        /// <returns>
        /// Returns an <see cref="Entity.Album" /> object containing information about the requested album.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Entity.Album ToAlbumEntity(IAlbum album, Entity.Permissions perms, Entity.GalleryDataLoadOptions options)
        {
            if (album == null)
                throw new ArgumentNullException("album");

            var albumEntity = new Entity.Album();

            albumEntity.Id = album.Id;
            albumEntity.ParentId = album.Parent.Id;
            albumEntity.GalleryId = album.GalleryId;
            albumEntity.Title = album.Title;
            albumEntity.Caption = album.Caption;
            albumEntity.Owner = (perms.AdministerGallery ? album.OwnerUserName : null);
            albumEntity.InheritedOwners = (perms.AdministerGallery ? String.Join(", ", album.InheritedOwners) : null);
            albumEntity.IsPrivate = album.IsPrivate;
            albumEntity.VirtualType = album.VirtualAlbumType;
            albumEntity.RssUrl = GetRssUrl(album);
            albumEntity.Permissions = perms;
            albumEntity.MetaItems = GalleryObjectController.ToMetaItems(album.MetadataItems.GetVisibleItems(), album);
            albumEntity.NumAlbums = album.GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated).Count;
            albumEntity.BreadCrumbLinks = GenerateBreadCrumbLinks(album);

            // Assign sorting fields from profile if present; otherwise use album sort settings.
            var albumSortDef = ProfileController.GetProfile().AlbumProfiles.Find(album.Id);
            if (albumSortDef != null)
            {
                albumEntity.SortById = albumSortDef.SortByMetaName;
                albumEntity.SortUp = albumSortDef.SortAscending;
            }
            else
            {
                albumEntity.SortById = album.SortByMetaName;
                albumEntity.SortUp = album.SortAscending;
            }

            // Optionally load gallery items
            if (options.LoadGalleryItems)
            {
                IList<IGalleryObject> items;
                if (albumSortDef != null)
                {
                    items = album
                      .GetChildGalleryObjects(options.Filter, !Utils.IsAuthenticated)
                      .ToSortedList(albumSortDef.SortByMetaName, albumSortDef.SortAscending, album.GalleryId);
                }
                else
                {
                    if (album.IsVirtualAlbum)
                    {
                        items = album.GetChildGalleryObjects(options.Filter, !Utils.IsAuthenticated).ToSortedList(album.SortByMetaName, album.SortAscending, album.GalleryId);
                    }
                    else
                    {
                        // Real (non-virtual) albums are already sorted on their Seq property, so return items based on that.
                        items = album.GetChildGalleryObjects(options.Filter, !Utils.IsAuthenticated).ToSortedList();
                    }
                }

                if (options.NumGalleryItemsToRetrieve > 0)
                    items = items.Skip(options.NumGalleryItemsToSkip).Take(options.NumGalleryItemsToRetrieve).ToList();

                albumEntity.GalleryItems = GalleryObjectController.ToGalleryItems(items);
                albumEntity.NumGalleryItems = albumEntity.GalleryItems.Length;
            }
            else
            {
                albumEntity.NumGalleryItems = album.GetChildGalleryObjects(options.Filter, !Utils.IsAuthenticated).Count;
            }

            // Optionally load media items
            if (options.LoadMediaItems)
            {
                IList<IGalleryObject> items;
                if (albumSortDef != null)
                {
                    items = album
                      .GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated)
                      .ToSortedList(albumSortDef.SortByMetaName, albumSortDef.SortAscending, album.GalleryId);
                }
                else
                {
                    if (album.IsVirtualAlbum)
                    {
                        items = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).ToSortedList(album.SortByMetaName, album.SortAscending, album.GalleryId);
                    }
                    else
                    {
                        // Real (non-virtual) albums are already sorted on their Seq property, so return items based on that.
                        items = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).ToSortedList();
                    }
                }

                //IList<IGalleryObject> items = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).ToSortedList();
                albumEntity.NumMediaItems = items.Count;
                albumEntity.MediaItems = GalleryObjectController.ToMediaItems(items);
            }
            else
            {
                albumEntity.NumMediaItems = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).Count;
            }

            return albumEntity;
        }

        /// <summary>
        /// Generate the HTML for the breadcrumb menu. The HTML includes the hierarchy of all albums the user has access to, including the specified
        /// <paramref name="album" />.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <returns>A <see cref="System.String" /> containing HTML for the album breadcrumb menu.</returns>
        private static string GenerateBreadCrumbLinks(IAlbum album)
        {
            // Ex (w/o HTML): ALL ALBUMS » Photos » Animals » Insects
            // Ex (w/ HTML): <a href="/gs/default.aspx?aid=1">ALL ALBUMS</a> &#187; <a href="/gs/default.aspx?aid=2368">Photos</a> &#187; <a href="/gs/default.aspx?aid=2369">Animals</a> &#187; <a id="currentAlbumLink" href="/dev/gs/default.aspx?aid=2371">Insects</a>
            string menuString = string.Empty;

            IGalleryServerRoleCollection roles = RoleController.GetGalleryServerRolesForUser();
            string dividerText = Resources.GalleryServer.UC_Album_Menu_Album_Divider_Text;
            bool foundTopAlbum = false;
            bool foundBottomAlbum = false;
            while (!foundTopAlbum)
            {
                // Iterate through each album and it's parents, working the way toward the top. For each album, build up a breadcrumb menu item.
                // Eventually we will reach one of three situations: (1) a virtual album that contains the child albums, (2) an album the current
                // user does not have permission to view, or (3) the actual top-level album.
                if (album.IsVirtualAlbum)
                {
                    menuString = menuString.Insert(0, string.Format(CultureInfo.CurrentCulture, " {0} <a href=\"{1}\">{2}</a>", dividerText, AlbumController.GetUrl(album), album.Title));

                    var searchVirtualAlbumTypes = new[] { VirtualAlbumType.Tag, VirtualAlbumType.People, VirtualAlbumType.Search, VirtualAlbumType.TitleOrCaption, VirtualAlbumType.MostRecentlyAdded, VirtualAlbumType.Rated };
                    var isAlbumSearchResult = searchVirtualAlbumTypes.Contains(album.VirtualAlbumType);

                    if (isAlbumSearchResult)
                    {
                        // Add one more link to represent the root album.  
                        menuString = menuString.Insert(0, string.Format(CultureInfo.CurrentCulture, " {0} <a href=\"{1}\">{2}</a>", dividerText, Utils.GetCurrentPageUrl(), Resources.GalleryServer.Site_Virtual_Album_Title));
                    }

                    foundTopAlbum = true;
                }
                else if (!Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, album.Id, album.GalleryId, album.IsPrivate, album.IsVirtualAlbum))
                {
                    // User is not authorized to view this album. If the user has permission to view more than one top-level album, then we want
                    // to display an "All albums" link. To determine this, load the root album. If a virtual album is returned, then we know the
                    // user has access to more than one top-level album. If it is an actual album (with a real ID and persisted in the data store),
                    // that means that album is the only top-level album the user can view, and thus we do not need to create a link that is one
                    // "higher" than that album.
                    IAlbum rootAlbum = Factory.LoadRootAlbum(album.GalleryId, roles, Utils.IsAuthenticated);
                    if (rootAlbum.IsVirtualAlbum)
                    {
                        menuString = menuString.Insert(0, string.Format(CultureInfo.CurrentCulture, " {0} <a href=\"{1}\">{2}</a>", dividerText, Utils.GetCurrentPageUrl(), Resources.GalleryServer.Site_Virtual_Album_Title));
                    }
                    foundTopAlbum = true;
                }
                else
                {
                    // Regular album somewhere in the hierarchy. Create a breadcrumb link.
                    string hyperlinkIdString = string.Empty;
                    if (!foundBottomAlbum)
                    {
                        hyperlinkIdString = " id=\"currentAlbumLink\""; // ID is referenced when inline editing an album's title
                        foundBottomAlbum = true;
                    }

                    menuString = menuString.Insert(0, string.Format(CultureInfo.CurrentCulture, " {0} <a{1} href=\"{2}\">{3}</a>", dividerText, hyperlinkIdString, AlbumController.GetUrl(album), Utils.RemoveHtmlTags(album.Title)));
                }

                if (album.Parent is NullGalleryObject)
                    foundTopAlbum = true;
                else
                    album = (IAlbum)album.Parent;
            }

            if (menuString.Length > (dividerText.Length + 2))
            {
                menuString = menuString.Substring(dividerText.Length + 2); // Remove the first divider character
            }

            return menuString;
        }

        /// <summary>
        /// Create an album based on <paramref name="album" />. The only properties used in the <paramref name="album" /> parameter are
        /// <see cref="Entity.Album.Title" /> and <see cref="Entity.Album.ParentId" />. Other properties are ignored, but if they need to be
        /// persisted in the future, this method can be modified to persist them. The parent album is resorted after the album is added.
        /// </summary>
        /// <param name="album">An <see cref="Entity.Album" /> instance containing data to be persisted to the data store.</param>
        /// <returns>The ID of the newly created album.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to create an album.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when the album is missing a title or an album doesn't exist for the specified 
        /// <see cref="Entity.Album.ParentId" /> property of <paramref name="album" />.</exception>
        public static int CreateAlbum(Entity.Album album)
        {
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            var parentAlbum = Factory.LoadAlbumInstance(new AlbumLoadOptions(album.ParentId) { InflateChildObjects = true, IsWritable = true });

            // Verify user has add child album permission.
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.AddChildAlbum, RoleController.GetGalleryServerRolesForUser(), parentAlbum.Id, parentAlbum.GalleryId, Utils.IsAuthenticated, parentAlbum.IsPrivate, parentAlbum.IsVirtualAlbum);

            IAlbum newAlbum = Factory.CreateEmptyAlbumInstance(parentAlbum.GalleryId, true);

            newAlbum.Title = Utils.CleanHtmlTags(album.Title.Trim(), parentAlbum.GalleryId);

            if (string.IsNullOrWhiteSpace(newAlbum.Title))
            {
                throw new InvalidAlbumException(Resources.GalleryServer.Task_Create_Album_Missing_Title_Text);
            }

            //newAlbum.ThumbnailMediaObjectId = 0; // not needed
            newAlbum.Parent = parentAlbum;
            newAlbum.IsPrivate = parentAlbum.IsPrivate;
            GalleryObjectController.SaveGalleryObject(newAlbum);

            // Assign properties back to entity, which will make its way back to the client.
            album.Id = newAlbum.Id;
            album.GalleryId = newAlbum.GalleryId;
            album.Title = newAlbum.Title;
            album.Caption = newAlbum.Caption;
            album.IsPrivate = newAlbum.IsPrivate;

            // Re-sort the items in the album. This will put the media object in the right position relative to its neighbors.
            ((IAlbum)newAlbum.Parent).Sort(true, Utils.UserName);

            return newAlbum.Id;
        }

        /// <summary>
        /// Update the album with the specified properties in the <paramref name="album" /> parameter. Only the following properties are
        /// persisted: <see cref="Entity.Album.SortById" />, <see cref="Entity.Album.SortUp" />, <see cref="Entity.Album.IsPrivate" />
        /// </summary>
        /// <param name="album">An <see cref="Entity.Album" /> instance containing data to be persisted to the data store.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the 
        /// user does not have edit permission to the specified album.</exception>
        public static void UpdateAlbum(Entity.Album album)
        {
            if (album == null)
                throw new ArgumentNullException("album");

            var alb = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(album.Id) { IsWritable = true });

            // Update remaining properties if user has edit album permission.
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditAlbum, RoleController.GetGalleryServerRolesForUser(), alb.Id, alb.GalleryId, Utils.IsAuthenticated, alb.IsPrivate, alb.IsVirtualAlbum);

            alb.SortByMetaName = (MetadataItemName)album.SortById;
            alb.SortAscending = album.SortUp;

            if (album.IsPrivate != alb.IsPrivate)
            {
                if (!album.IsPrivate && alb.Parent.IsPrivate)
                {
                    throw new NotSupportedException("Cannot make album public: It is invalid to make an album public when it's parent album is private.");
                }
                alb.IsPrivate = album.IsPrivate;

                var userName = Utils.UserName;
                Task.Factory.StartNew(() => SynchIsPrivatePropertyOnChildGalleryObjects(alb, userName));
            }

            GalleryObjectController.SaveGalleryObject(alb);
        }

        /// <overloads>
        /// Permanently delete this album from the data store and optionally the hard drive.
        /// </overloads>
        /// <summary>
        /// Permanently delete this album from the data store and optionally the hard drive. Validation is performed prior to deletion to ensure
        /// current user has delete permission and the album can be safely deleted. The validation is contained in the method 
        /// <see cref="ValidateBeforeAlbumDelete"/> and may be invoked separately if desired.
        /// </summary>
        /// <param name="albumId">The ID of the album to delete.</param>
        /// <exception cref="CannotDeleteAlbumException">Thrown when the album does not meet the requirements for safe deletion.
        /// This includes detecting when the media objects path is read only and when the album is or contains the user album
        /// parent album and user albums are enabled.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when <paramref name="albumId" /> does not represent an existing album.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to delete the album.</exception>
        public static void DeleteAlbum(int albumId)
        {
            DeleteAlbum(AlbumController.LoadAlbumInstance(albumId));
        }

        /// <summary>
        /// Permanently delete this album from the data store and optionally the hard drive. Validation is performed prior to deletion to ensure
        /// current user has delete permission and the album can be safely deleted. The validation is contained in the method 
        /// <see cref="ValidateBeforeAlbumDelete"/> and may be invoked separately if desired.
        /// </summary>
        /// <param name="album">The album to delete. If null, the function returns without taking any action.</param>
        /// <param name="deleteFromFileSystem">if set to <c>true</c> the files and directories associated with the album
        /// are deleted from the hard disk. Set this to <c>false</c> to delete only the database records.</param>
        /// <exception cref="CannotDeleteAlbumException">Thrown when the album does not meet the requirements for safe deletion.
        /// This includes detecting when the media objects path is read only and when the album is or contains the user album
        /// parent album and user albums are enabled.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to delete the album.</exception>
        public static void DeleteAlbum(IAlbum album, bool deleteFromFileSystem = true)
        {
            if (album == null)
                return;

            ValidateBeforeAlbumDelete(album);

            OnBeforeAlbumDelete(album);

            if (deleteFromFileSystem)
            {
                album.Delete();
            }
            else
            {
                album.DeleteFromGallery();
            }
        }

        /// <summary>
        /// Verifies that the album meets the prerequisites to be safely deleted but does not actually delete the album. Throws a
        /// <see cref="CannotDeleteAlbumException" /> when deleting it would violate a business rule. Throws a
        /// <see cref="GallerySecurityException" /> when the current user does not have permission to delete the album.
        /// </summary>
        /// <param name="albumToDelete">The album to delete.</param>
        /// <remarks>This function is automatically called when using the <see cref="DeleteAlbum(IAlbum, bool)"/> method, so it is not necessary to 
        /// invoke when using that method. Typically you will call this method when there are several items to delete and you want to 
        /// check all of them before deleting any of them, such as we have on the Delete Objects page.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="albumToDelete" /> is null.</exception>
        /// <exception cref="CannotDeleteAlbumException">Thrown when the album does not meet the 
        /// requirements for safe deletion.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to delete the album.</exception>
        public static void ValidateBeforeAlbumDelete(IAlbum albumToDelete)
        {
            if (albumToDelete == null)
                throw new ArgumentNullException("albumToDelete");

            var userAlbum = UserController.GetUserAlbum(Utils.UserName, albumToDelete.GalleryId);
            var curUserDeletingOwnUserAlbum = (userAlbum != null && userAlbum.Id == albumToDelete.Id);
            // Skip security check when user is deleting their own user album. Normally this won't happen (the menu action for deleting will be 
            // disabled), but it will happen when they delete their user album or their account on the account page, and this is one situation 
            // where it is OK for them to delete their album.
            if (!curUserDeletingOwnUserAlbum)
            {
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.DeleteAlbum, RoleController.GetGalleryServerRolesForUser(), albumToDelete.Id, albumToDelete.GalleryId, Utils.IsAuthenticated, albumToDelete.IsPrivate, albumToDelete.IsVirtualAlbum);
            }

            if (Factory.LoadGallerySetting(albumToDelete.GalleryId).MediaObjectPathIsReadOnly)
            {
                throw new CannotDeleteAlbumException(Resources.GalleryServer.Task_Modify_Asset_Cannot_Modify_MediaPathIsReadOnly);
            }

            var validator = new AlbumDeleteValidator(albumToDelete);

            validator.Validate();

            if (!validator.CanBeDeleted)
            {
                switch (validator.ValidationFailureReason)
                {
                    case GalleryObjectDeleteValidationFailureReason.AlbumSpecifiedAsUserAlbumContainer:
                    case GalleryObjectDeleteValidationFailureReason.AlbumContainsUserAlbumContainer:
                        {
                            string albumTitle = String.Concat("'", albumToDelete.Title, "' (ID# ", albumToDelete.Id, ")");
                            string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Task_Delete_Album_Cannot_Delete_Contains_User_Album_Parent_Ex_Msg, albumTitle);

                            throw new CannotDeleteAlbumException(msg);
                        }
                    case GalleryObjectDeleteValidationFailureReason.AlbumSpecifiedAsDefaultGalleryObject:
                    case GalleryObjectDeleteValidationFailureReason.AlbumContainsDefaultGalleryObjectAlbum:
                    case GalleryObjectDeleteValidationFailureReason.AlbumContainsDefaultGalleryObjectMediaObject:
                        {
                            string albumTitle = String.Concat("'", albumToDelete.Title, "' (ID# ", albumToDelete.Id, ")");
                            string msg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Task_Delete_Album_Cannot_Delete_Contains_Default_Gallery_Object_Ex_Msg, albumTitle);

                            throw new CannotDeleteAlbumException(msg);
                        }
                    default:
                        throw new InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function ValidateBeforeAlbumDelete is not designed to handle the enumeration value {0}. The function must be updated.", validator.ValidationFailureReason));
                }
            }
        }

        /// <summary>
        /// Gets the URL to the specified <paramref name="album" />.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the function encounters a virtual album
        /// type it was not designed to handle.</exception>
        public static string GetUrl(IAlbum album)
        {
            var appPath = Utils.GetCurrentPageUrl();

            switch (album.VirtualAlbumType)
            {
                case VirtualAlbumType.NotSpecified:
                case VirtualAlbumType.NotVirtual:
                    return Utils.GetUrl(PageId.album, "aid={0}", album.Id);
                case VirtualAlbumType.Root:
                    return appPath;
                case VirtualAlbumType.TitleOrCaption:
                    return Utils.GetUrl(PageId.album, "title={0}", Utils.UrlEncode(Utils.GetQueryStringParameterString("title")));
                case VirtualAlbumType.Tag:
                    return Utils.GetUrl(PageId.album, "tag={0}", Utils.UrlEncode(Utils.GetQueryStringParameterString("tag")));
                case VirtualAlbumType.People:
                    return Utils.GetUrl(PageId.album, "people={0}", Utils.UrlEncode(Utils.GetQueryStringParameterString("people")));
                case VirtualAlbumType.Search:
                    return Utils.GetUrl(PageId.album, "search={0}", Utils.UrlEncode(Utils.GetQueryStringParameterString("search")));
                case VirtualAlbumType.MostRecentlyAdded:
                    return Utils.GetUrl(PageId.album, "latest={0}", Utils.GetQueryStringParameterInt32("latest"));
                case VirtualAlbumType.Rated:
                    return Utils.GetUrl(PageId.album, "rating={0}&top={1}", Utils.GetQueryStringParameterString("rating"), Utils.GetQueryStringParameterInt32("top"));
                default:
                    throw new InvalidOperationException(String.Format("The method AlbumController.GetUrl() encountered a VirtualAlbumType ({0}) it was not designed to handle. The developer must update this method.", album.VirtualAlbumType));
            }
        }

        /// <summary>
        /// Gets the RSS URL for the specified <paramref name="album" />. Returns null if the user is not 
        /// running the Enterprise version or no applicable RSS URL exists for the album. For example, 
        /// virtual root albums that are used for restricted users will return null.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <returns>System.String.</returns>
        public static string GetRssUrl(IAlbum album)
        {
            switch (album.VirtualAlbumType)
            {
                case VirtualAlbumType.NotVirtual:
                    return String.Concat(Utils.AppRoot, "/api/feed/album?id=", album.Id);
                case VirtualAlbumType.TitleOrCaption:
                    return String.Format(CultureInfo.InvariantCulture, "{0}/api/feed/title?q={1}&galleryid={2}", Utils.AppRoot, Utils.UrlEncode(Utils.GetQueryStringParameterString("title")), album.GalleryId);
                case VirtualAlbumType.Search:
                    return String.Format(CultureInfo.InvariantCulture, "{0}/api/feed/search?q={1}&galleryid={2}", Utils.AppRoot, Utils.UrlEncode(Utils.GetQueryStringParameterString("search")), album.GalleryId);
                case VirtualAlbumType.Tag:
                    return String.Format(CultureInfo.InvariantCulture, "{0}/api/feed/tag?q={1}&galleryid={2}", Utils.AppRoot, Utils.UrlEncode(Utils.GetQueryStringParameterString("tag")), album.GalleryId);
                case VirtualAlbumType.People:
                    return String.Format(CultureInfo.InvariantCulture, "{0}/api/feed/people?q={1}&galleryid={2}", Utils.AppRoot, Utils.UrlEncode(Utils.GetQueryStringParameterString("people")), album.GalleryId);
                case VirtualAlbumType.MostRecentlyAdded:
                    return String.Format(CultureInfo.InvariantCulture, "{0}/api/feed/latest?galleryid={1}", Utils.AppRoot, album.GalleryId);
                case VirtualAlbumType.Rated:
                    return String.Format(CultureInfo.InvariantCulture, "{0}/api/feed/rating?rating={1}&top={2}&galleryid={3}", Utils.AppRoot, Utils.UrlEncode(Utils.GetQueryStringParameterString("rating")), Utils.GetQueryStringParameterInt32("top"), album.GalleryId);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Sorts the <paramref name="galleryItems" /> in the order in which they are passed.
        /// This method is used when a user is manually sorting an album and has dragged an item to a new position.
        /// </summary>
        /// <param name="galleryItems">The gallery objects to sort. Their position in the array indicates the desired
        /// sequence. Only <see cref="Entity.GalleryItem.Id" /> and <see cref="Entity.GalleryItem.ItemType" /> need be
        /// populated.</param>
        /// <param name="userName">Name of the logged on user.</param>
        public static void Sort(GalleryItem[] galleryItems, string userName)
        {
            if (galleryItems == null || galleryItems.Length == 0)
            {
                return;
            }

            try
            {
                // To improve performance, grab a collection of all the items in the album containing the first item.
                // At the time this function was written, the galleryItems parameter will only include items in a single album,
                // so this step allows us to get all the items in a single step. For robustness and to support all possible usage,
                // the code in the iteration manually loads a writable instance if it's not in the collection.
                var galleryObjects = GetWritableSiblingGalleryObjects(galleryItems[0]);

                var seq = 1;
                foreach (var galleryItem in galleryItems)
                {
                    // Loop through each item and update its Sequence property to match the order in which it was passed.
                    var item = galleryItem;

                    var galleryObject = galleryObjects.FirstOrDefault(go => go.Id == item.Id && go.GalleryObjectType == (GalleryObjectType)item.ItemType);

                    if (galleryObject == null)
                    {
                        // Not found, so load it manually. This is not expected to ever occur when manually sorting an album, but we 
                        // include it for robustness.
                        if ((GalleryObjectType)galleryItem.ItemType == GalleryObjectType.Album)
                        {
                            galleryObject = Factory.LoadAlbumInstance(new AlbumLoadOptions(galleryItem.Id) { IsWritable = true });
                        }
                        else
                        {
                            galleryObject = Factory.LoadMediaObjectInstance(new MediaLoadOptions(galleryItem.Id) { IsWritable = true });
                        }
                    }

                    galleryObject.Sequence = seq;
                    GalleryObjectController.SaveGalleryObject(galleryObject, userName);
                    seq++;
                }
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                throw;
            }
        }

        /// <summary>
        /// Re-sort the items in the album according to the criteria and store this updated sequence in the
        /// database. Verifies that users have <see cref="SecurityActions.EditAlbum" /> permission.
        /// </summary>
        /// <param name="albumId">The album ID.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
        /// <exception cref="GallerySecurityException">Thrown when the user does not have EditAlbum permission.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist in the data store.</exception>
        public static void Sort(int albumId, MetadataItemName sortByMetaNameId, bool sortAscending)
        {
            var album = LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = true, InflateChildObjects = true });

            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditAlbum, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

            var oldSortByMetaName = album.SortByMetaName;
            var oldSortAscending = album.SortAscending;

            album.SortByMetaName = sortByMetaNameId;
            album.SortAscending = sortAscending;

            ReverseCustomSortIfNeeded(album, oldSortByMetaName, oldSortAscending);

            album.Sort(true, Utils.UserName);

            GalleryObjectController.SaveGalleryObject(album);

            RemoveAlbumFromUserProfileIfPresent(albumId);
        }

        /// <summary>
        /// Moves or copies the <paramref name="itemsToTransfer" /> to the specified <paramref name="destinationAlbumId" />. Items that are created
        /// during a copy operation are assigned to <paramref name="createdGalleryItems" />; is null otherwise.
        /// </summary>
        /// <param name="destinationAlbumId">The ID of the album to move or copy the items to.</param>
        /// <param name="itemsToTransfer">The items to move or copy.</param>
        /// <param name="transferType">Indicates whether the <paramref name="itemsToTransfer" /> are being moved or copied.</param>
        /// <param name="createdGalleryItems">An array of <see cref="GalleryItem" /> instances representing gallery items created by this
        /// method. Applies only when <paramref name="transferType" /> is <see cref="GalleryAssetTransferType.Copy" />. Will be null
        /// when <paramref name="transferType" /> is <see cref="GalleryAssetTransferType.Move" />.</param>
        /// <returns>An instance of <see cref="IAlbum" /> representing the destination album.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when an album with the specified <paramref name="destinationAlbumId" />
        /// is not found in the data store.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have the necessary write permission to
        /// the <paramref name="destinationAlbumId" />, is not allowed to move or copy one or more <paramref name="itemsToTransfer" />, or the
        /// destination album is in a read-only gallery (<see cref="IGallerySettings.MediaObjectPathIsReadOnly" /> = <c>true</c>).</exception>
        /// <exception cref="CannotTransferAlbumToNestedDirectoryException">Thrown when the user tries to move or copy an album to one
        /// of its children albums.</exception>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when one or more of the <paramref name="itemsToTransfer" /> has a file extension
        /// that is not allowed in the gallery containing the <paramref name="destinationAlbumId" />.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when one of the <paramref name="itemsToTransfer" /> is a media object whose
        /// original file no longer exists.</exception>
        public static IAlbum TransferToAlbum(int destinationAlbumId, GalleryItem[] itemsToTransfer, GalleryAssetTransferType transferType, out GalleryItem[] createdGalleryItems)
        {
            var destinationAlbum = LoadAlbumInstance(new AlbumLoadOptions(destinationAlbumId) { IsWritable = true });

            if (itemsToTransfer == null || itemsToTransfer.Length == 0)
            {
                createdGalleryItems = null;
                return destinationAlbum;
            }

            var copiedItems = new List<IGalleryObject>();
            var assetsToTransfer = ThrowIfItemsCannotBeMovedOrCopied(itemsToTransfer, destinationAlbum, transferType);

            foreach (var itemToTransfer in assetsToTransfer)
            {
                switch (transferType)
                {
                    case GalleryAssetTransferType.Move:
                        GalleryObjectController.MoveGalleryObject(itemToTransfer, destinationAlbum);
                        break;

                    case GalleryAssetTransferType.Copy:
                        copiedItems.Add(GalleryObjectController.CopyGalleryObject(itemToTransfer, destinationAlbum));
                        break;

                    default:
                        throw new ArgumentException($"Encountered unexpected GalleryAssetTransferType enum value '{transferType}'.");
                }
            }

            // Resort the gallery objects in the album. We reload the album first because the moving/copying may have changed things.
            Factory.LoadAlbumInstance(new AlbumLoadOptions(destinationAlbumId) { IsWritable = true }).Sort(true, Utils.UserName);

            createdGalleryItems = copiedItems.Count > 0 ? GalleryObjectController.ToGalleryItems(copiedItems) : null;

            return destinationAlbum;
        }


        /// <summary>
        /// Calculates the total file size, in KB, of all the original files in the <paramref name="galleryItem" />, including all 
        /// child albums and assigns it to the <see cref="Entity.DisplayObject.FileSizeKB" /> property of the original display object 
        /// in the <see cref="GalleryItem.Views" /> property (the original display object view is added if it is not already present).
        /// The total includes only those items where a web-optimized version also exists. No action is taken if <paramref name="galleryItem" />
        /// is not an album or refers to an album that no longer exists. No action is taken if current user does not have view permission
        /// to the album.
        /// </summary>
        /// <param name="galleryItem">The gallery item. It is expected to be an album (<see cref="GalleryItem.IsAlbum" /> == <c>true</c>).
        /// It is updated with the calculated file size value.</param>
        public static void CalculateOriginalFileSize(GalleryItem galleryItem)
        {
            if (galleryItem == null)
                throw new ArgumentNullException(nameof(galleryItem));

            if (!galleryItem.IsAlbum)
                return;


            var dType = galleryItem.Views.SingleOrDefault(v => v.ViewSize == (int)DisplayObjectType.Original);

            if (dType == null)
            {
                // No display object for the 'original' view exists, so add one. This is expected to be the common scenario.
                dType = new Entity.DisplayObject()
                {
                    ViewSize = (int)DisplayObjectType.Original
                };

                galleryItem.Views = galleryItem.Views.Concat(Enumerable.Repeat(dType, 1)).ToArray();
            }

            try
            {
                var album = LoadAlbumInstance(galleryItem.Id);

                if (SecurityManager.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum))
                {
                    dType.FileSizeKB = album.GetFileSizeKbAllOriginalFilesInAlbum();
                }
            }
            catch (InvalidAlbumException)
            { }
        }

        /// <summary>
        /// Assign the thumbnail image assocated with <paramref name="galleryItem" /> to the <paramref name="albumId" />.
        /// </summary>
        /// <param name="galleryItem">The gallery item containing the thumbnail image to assign.</param>
        /// <param name="albumId">The ID of the album to be updated with a new thumbnail image.</param>
        /// <returns>An instance of <see cref="IAlbum" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when an album is not found in the data store.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the thumbnail media object is not found in the data store.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user does not have edit permission to <paramref name="albumId" /> 
        /// or view permission to the thumbnail media object associated with <paramref name="galleryItem" />.</exception>
        public static IAlbum AssignThumbnail(GalleryItem galleryItem, int albumId)
        {
            var album = LoadAlbumInstance(albumId);

            if (galleryItem == null)
                return album;

            // 1. Verify user has edit permission to album
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditAlbum, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

            // 2. Get the media ID
            var mediaObject = RetrieveMediaObjectForThumbnailAssignment(galleryItem);

            // 3. Verify user has view permission to it
            if (mediaObject != null)
            {
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), mediaObject.Parent.Id, mediaObject.GalleryId, Utils.IsAuthenticated, mediaObject.Parent.IsPrivate, ((IAlbum)mediaObject.Parent).IsVirtualAlbum);
            }

            // 4. Assign thumbnail
            Business.Album.AssignAlbumThumbnail(album, mediaObject?.Id ?? 0, Utils.UserName);

            return album;
        }

        /// <summary>
        /// Change the album owner for the <paramref name="albumId" />. The previous album owner name is returned on the <paramref name="oldOwnerName" /> parameter.
        /// Verifies that the user is a gallery or site admin.
        /// </summary>
        /// <param name="albumId">The ID of the album to be updated with the new owner.</param>
        /// <param name="ownerName">Name of the album owner. Must map to an existing user name</param>
        /// <param name="oldOwnerName">Name of the old album owner (the value it was before changing it in this method).</param>
        /// <returns>An instance of <see cref="IAlbum" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when an album is not found in the data store.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user does not have admin gallery or admin site permission to the gallery
        /// that album <paramref name="albumId" /> belongs to..</exception>
        public static IAlbum ChangeOwner(int albumId, string ownerName, out string oldOwnerName)
        {
            var album = LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = true });

            oldOwnerName = album.OwnerUserName;

            // If the owner has changed, update it, but only if the user is administrator.
            if (ownerName != album.OwnerUserName)
            {
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.AdministerSite | SecurityActions.AdministerGallery, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

                if (!string.IsNullOrEmpty(album.OwnerUserName))
                {
                    // Another user was previously assigned as owner. Delete role since this person will no longer be the owner.
                    RoleController.DeleteGalleryServerProRole(album.OwnerRoleName);
                }

                var user = UserController.GetUsersCurrentUserCanView(album.GalleryId).FindByUserName(ownerName);

                if (user != null || string.IsNullOrEmpty(ownerName))
                {
                    // GalleryObjectController.SaveGalleryObject will make sure there is a role created for this user.
                    album.OwnerUserName = user?.UserName ?? string.Empty;
                }
                else
                {
                    throw new GallerySecurityException($"The account {Utils.HtmlEncode(ownerName)} does not exist or your security settings prevent you from accessing it.");
                }
            }

            GalleryObjectController.SaveGalleryObject(album);

            return album;
        }

        /// <summary>
        /// Generate a ZIP archive for the requested <paramref name="galleryItems" /> and <paramref name="mediaSize" /> and persist to
        /// a file in the App_Data\_Temp directory. The filename is returned on the <see cref="ActionResult.ActionTarget" /> property.
        /// A <see cref="GallerySecurityException" /> is thrown if the user doesn't have permission to view all requested items.
        /// </summary>
        /// <param name="galleryItems">The gallery items to download.</param>
        /// <param name="mediaSize">Size of the items to include in the ZIP archive.</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the user does not have view permission to one or more
        /// <paramref name="galleryItems" /> or a setting prevents the user from downloading a ZIP archive.</exception>
        public static ActionResult PrepareZipDownload(GalleryItem[] galleryItems, DisplayObjectType mediaSize)
        {
            List<int> albumIds;
            List<int> mediaIds;
            int parentAlbumId;

            ValidateZipDownloadSecurity(galleryItems, mediaSize, out albumIds, out mediaIds, out parentAlbumId);

            var zip = new ZipUtility(Utils.UserName, RoleController.GetGalleryServerRolesForUser());
            var fileName = System.IO.Path.GetFileName(zip.CreateZipFile(parentAlbumId, albumIds, mediaIds, mediaSize));

            return new ActionResult
            {
                Title = "Media Assets Ready to Download",
                Status = ActionResultStatus.Success.ToString(),
                ActionTarget = fileName
            };
        }

        /// <summary>
        /// Gets a data entity containing permission information for the specified <paramref name="album" />.
        /// The instance can be JSON-parsed and sent to the browser. The permissions take into account whether the media files
        /// are configured as read only (<see cref="IGallerySettings.MediaObjectPathIsReadOnly" />).
        /// </summary>
        /// <param name="album">The album.</param>
        /// <returns>Returns <see cref="Entity.Permissions" /> object containing permission information.</returns>
        public static Entity.Permissions GetPermissionsEntity(IAlbum album)
        {
            int albumId = album.Id;
            int galleryId = album.GalleryId;
            bool isPrivate = album.IsPrivate;
            bool isVirtual = album.IsVirtualAlbum;
            var rootAlbum = Factory.LoadRootAlbumInstance(album.GalleryId);
            IGalleryServerRoleCollection roles = RoleController.GetGalleryServerRolesForUser();
            var isAdmin = Utils.IsUserAuthorized(SecurityActions.AdministerSite, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, isVirtual);
            var isGalleryAdmin = isAdmin || Utils.IsUserAuthorized(SecurityActions.AdministerGallery, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, isVirtual);
            var isGalleryWritable = !Factory.LoadGallerySetting(galleryId).MediaObjectPathIsReadOnly;

            var perms = new Entity.Permissions();

            perms.AdministerGallery = isGalleryAdmin;
            perms.AdministerSite = isAdmin;

            if (album.IsVirtualAlbum)
            {
                // When we have a virtual album we use the permissions assigned to the root album. 
                perms.ViewAlbumOrMediaObject = isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum);
                perms.ViewOriginalMediaObject = isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.ViewOriginalMediaObject, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum);
                perms.AddChildAlbum = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.AddChildAlbum, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum));
                perms.AddMediaObject = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.AddMediaObject, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum));
                perms.EditAlbum = false;
                perms.EditMediaObject = (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.EditMediaObject, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum));
                perms.DeleteAlbum = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.DeleteAlbum, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum));
                perms.DeleteChildAlbum = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.DeleteChildAlbum, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum));
                perms.DeleteMediaObject = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.DeleteMediaObject, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum));
                perms.Synchronize = isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.Synchronize, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum);
                perms.HideWatermark = Utils.IsUserAuthorized(SecurityActions.HideWatermark, roles, rootAlbum.Id, galleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum);
            }
            else
            {
                perms.ViewAlbumOrMediaObject = isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, albumId, galleryId, isPrivate, isVirtual);
                perms.ViewOriginalMediaObject = isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.ViewOriginalMediaObject, roles, albumId, galleryId, isPrivate, isVirtual);
                perms.AddChildAlbum = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.AddChildAlbum, roles, albumId, galleryId, isPrivate, isVirtual));
                perms.AddMediaObject = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.AddMediaObject, roles, albumId, galleryId, isPrivate, isVirtual));
                perms.EditAlbum = (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.EditAlbum, roles, albumId, galleryId, isPrivate, isVirtual));
                perms.EditMediaObject = (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.EditMediaObject, roles, albumId, galleryId, isPrivate, isVirtual));
                perms.DeleteAlbum = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.DeleteAlbum, roles, albumId, galleryId, isPrivate, isVirtual));
                perms.DeleteChildAlbum = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.DeleteChildAlbum, roles, albumId, galleryId, isPrivate, isVirtual));
                perms.DeleteMediaObject = isGalleryWritable && (isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.DeleteMediaObject, roles, albumId, galleryId, isPrivate, isVirtual));
                perms.Synchronize = isGalleryAdmin || Utils.IsUserAuthorized(SecurityActions.Synchronize, roles, albumId, galleryId, isPrivate, isVirtual);
                perms.HideWatermark = Utils.IsUserAuthorized(SecurityActions.HideWatermark, roles, albumId, galleryId, isPrivate, isVirtual);
            }

            return perms;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Verify the specified <paramref name="galleryItems" /> can be downloaded in a ZIP archive and generate a few variables that will be 
        /// needed by the <see cref="ZipUtility" /> class. If a gallery setting or the user's security context prevents downloading one or more
        /// items, a <see cref="GallerySecurityException" /> is thrown. If any gallery items do not exist, silently ignore them.
        /// </summary>
        /// <param name="galleryItems">The gallery items to be downloaded.</param>
        /// <param name="mediaSize">Size of the items to include in the ZIP archive.</param>
        /// <param name="albumIds">The IDs of albums to be downloaded. Generated from <paramref name="galleryItems" />.</param>
        /// <param name="mediaIds">The IDs of media assets to be downloaded. Generated from <paramref name="galleryItems" />.</param>
        /// <param name="parentAlbumId">The ID of the album that contains the <paramref name="galleryItems" />. When they belong to multiple albums,
        /// this value is the root album ID. Returns <see cref="int.MinValue" /> when none of the <paramref name="galleryItems" /> exist in the data store.</param>
        /// <exception cref="GallerySecurityException">Thrown when the user does not have view permission to one or more
        /// <paramref name="galleryItems" /> or a setting prevents the user from downloading a ZIP archive.</exception>
        private static void ValidateZipDownloadSecurity(IEnumerable<GalleryItem> galleryItems, DisplayObjectType mediaSize, out List<int> albumIds, out List<int> mediaIds, out int parentAlbumId)
        {
            // Check several things:
            // 1. The gallery setting EnableGalleryObjectZipDownload is true for all galleries associted with galleryItems.
            // 2. For all albums in galleryItems, EnableAlbumZipDownload is true and user has view permission.
            // 3. For all media assets in galleryItems, user has view permission.
            albumIds = new List<int>();
            mediaIds = new List<int>();
            var galleryIds = new HashSet<int>();
            var parentAlbumIds = new HashSet<int>();
            var roles = RoleController.GetGalleryServerRolesForUser();
            var secAction = (mediaSize == DisplayObjectType.Original ? SecurityActions.ViewOriginalMediaObject : SecurityActions.ViewAlbumOrMediaObject);

            foreach (var galleryItem in galleryItems)
            {
                if (galleryItem.IsAlbum)
                {
                    try
                    {
                        var album = Factory.LoadAlbumInstance(galleryItem.Id);

                        if (!Factory.LoadGallerySetting(album.GalleryId).EnableAlbumZipDownload)
                        {
                            throw new GallerySecurityException("Cannot generate ZIP archive. One ore more albums are in a gallery that has album ZIP downloading disabled.");
                        }

                        SecurityManager.ThrowIfUserNotAuthorized(secAction, roles, album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

                        galleryIds.Add(album.GalleryId);
                        parentAlbumIds.Add(album.Parent.Id);
                        albumIds.Add(galleryItem.Id);
                    }
                    catch (InvalidAlbumException) { continue; } // Album doesn't exist. Skip it.
                }
                else
                {
                    try
                    {
                        var mediaAsset = Factory.LoadMediaObjectInstance(galleryItem.Id);

                        SecurityManager.ThrowIfUserNotAuthorized(secAction, roles, mediaAsset.Parent.Id, mediaAsset.GalleryId, Utils.IsAuthenticated, mediaAsset.IsPrivate, ((IAlbum)mediaAsset.Parent).IsVirtualAlbum);

                        galleryIds.Add(mediaAsset.GalleryId);
                        parentAlbumIds.Add(mediaAsset.Parent.Id);
                        mediaIds.Add(galleryItem.Id);
                    }
                    catch (InvalidMediaObjectException) { continue; } // Media asset doesn't exist. Skip it.
                }
            }

            // Verify all items are in a gallery that allows ZIP downloading
            if (galleryIds.Any(galleryId => !Factory.LoadGallerySetting(galleryId).EnableGalleryObjectZipDownload))
            {
                throw new GallerySecurityException("Cannot generate ZIP archive. One or more items are in a gallery configured to prevent downloading multiple assets at once. Try downloading them one at a time.");
            }

            if (parentAlbumIds.Count == 1)
            {
                // All our items are in the same album. Use that as the parent album ID.
                parentAlbumId = parentAlbumIds.First();
            }
            else if (galleryIds.Any())
            {
                // Items come from multiple parents. Use the root album.
                parentAlbumId = Factory.LoadRootAlbumInstance(galleryIds.First()).Id;
            }
            else
            {
                // We'll get here if user is downloading items that have been deleted since the page was loaded.
                parentAlbumId = int.MinValue;
            }
        }

        /// <summary>
        /// Retrieves the thumbnail media object for <paramref name="galleryItem" />. Returns null if <paramref name="galleryItem" /> is an album
        /// with no assigned thumbnail.
        /// </summary>
        /// <param name="galleryItem">The gallery item.</param>
        /// <returns>An instance of <see cref="IGalleryObject" /> or null.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when <paramref name="galleryItem" /> is an album but not found in the data store.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the thumbnail media object is not found in the data store.</exception>
        private static IGalleryObject RetrieveMediaObjectForThumbnailAssignment(GalleryItem galleryItem)
        {
            var moid = (galleryItem.IsAlbum ? LoadAlbumInstance(galleryItem.Id).ThumbnailMediaObjectId : galleryItem.Id);

            return (moid > 0 ? Factory.LoadMediaObjectInstance(moid) : null);
        }

        /// <summary>
        /// Performs any necessary actions that must occur before an album is deleted. Specifically, it deletes the owner role 
        /// if one exists for the album, but only when this album is the only one assigned to the role. It also clears out  
        /// <see cref="IGallerySettings.UserAlbumParentAlbumId" /> if the album's ID matches it. This function recursively calls
        /// itself to make sure all child albums are processed.
        /// </summary>
        /// <param name="album">The album to be deleted, or one of its child albums.</param>
        private static void OnBeforeAlbumDelete(IAlbum album)
        {
            // If there is an owner role associated with this album, and the role is not assigned to any other albums, delete it.
            if (!String.IsNullOrEmpty(album.OwnerRoleName))
            {
                IGalleryServerRole role = RoleController.GetGalleryServerRoles().GetRole(album.OwnerRoleName);

                if ((role != null) && (role.AllAlbumIds.Count == 1) && role.AllAlbumIds.Contains(album.Id))
                {
                    RoleController.DeleteGalleryServerProRole(role.RoleName);
                }
            }

            // If the album is specified as the user album container, clear out the setting. The ValidateBeforeAlbumDelete()
            // function will throw an exception if user albums are enabled, so this should only happen when user albums
            // are disabled, so it is safe to clear it out.
            int userAlbumParentAlbumId = Factory.LoadGallerySetting(album.GalleryId).UserAlbumParentAlbumId;
            if (album.Id == userAlbumParentAlbumId)
            {
                IGallerySettings gallerySettingsWritable = Factory.LoadGallerySetting(album.GalleryId, true);
                gallerySettingsWritable.UserAlbumParentAlbumId = 0;
                gallerySettingsWritable.Save();
            }

            // Recursively validate child albums.
            foreach (IGalleryObject childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
            {
                OnBeforeAlbumDelete((IAlbum)childAlbum);
            }
        }

        /// <summary>
        /// Finds the first album within the heirarchy of the specified <paramref name="album"/> whose ID is in 
        /// <paramref name="albumIds"/>. Acts recursively in an across-first, then-down search pattern, resulting 
        /// in the highest level matching album to be returned. Returns null if there are no matching albums.
        /// </summary>
        /// <param name="album">The album to be searched to see if it, or any of its children, matches one of the IDs
        /// in <paramref name="albumIds"/>.</param>
        /// <param name="albumIds">Contains the IDs of the albums to search for.</param>
        /// <returns>Returns the first album within the heirarchy of the specified <paramref name="album"/> whose ID is in 
        /// <paramref name="albumIds"/>.</returns>
        private static IAlbum FindFirstMatchingAlbumRecursive(IAlbum album, ICollection<int> albumIds)
        {
            // Is the current album in the list?
            if (albumIds.Contains(album.Id))
                return album;

            // Nope, so look at the child albums of this album.
            IAlbum albumToSelect = null;
            var childAlbums = album.GetChildGalleryObjects(GalleryObjectType.Album).ToSortedList();

            foreach (IGalleryObject childAlbum in childAlbums)
            {
                if (albumIds.Contains(childAlbum.Id))
                {
                    albumToSelect = (IAlbum)childAlbum;
                    break;
                }
            }

            // Not the child albums either, so iterate through the children of the child albums. Act recursively.
            if (albumToSelect == null)
            {
                foreach (IGalleryObject childAlbum in childAlbums)
                {
                    albumToSelect = FindFirstMatchingAlbumRecursive((IAlbum)childAlbum, albumIds);

                    if (albumToSelect != null)
                        break;
                }
            }

            return albumToSelect; // Returns null if no matching album is found
        }

        private static void SaveAlbumIdToProfile(int albumId, string userName, int galleryId)
        {
            IUserProfile profile = ProfileController.GetProfile(userName);

            IUserGalleryProfile pg = profile.GetGalleryProfile(galleryId);
            pg.UserAlbumId = albumId;

            ProfileController.SaveProfile(profile);
        }

        /// <summary>
        /// Set the IsPrivate property of all child albums and media objects of the specified album to have the same value
        /// as the specified album. This can be a long running operation and should be scheduled on a background thread.
        /// This function, and its decendents, have no dependence on the HTTP Context.
        /// </summary>
        /// <param name="album">The album whose child objects are to be updated to have the same IsPrivate value.</param>
        /// <param name="userName">Name of the current user.</param>
        private static void SynchIsPrivatePropertyOnChildGalleryObjects(IAlbum album, string userName)
        {
            try
            {
                SynchIsPrivatePropertyOnChildGalleryObjectsRecursive(album, userName);
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);
            }
        }

        /// <summary>
        /// Set the IsPrivate property of all child albums and media objects of the specified album to have the same value
        /// as the specified album.
        /// </summary>
        /// <param name="album">The album whose child objects are to be updated to have the same IsPrivate value.</param>
        /// <param name="userName">Name of the current user.</param>
        private static void SynchIsPrivatePropertyOnChildGalleryObjectsRecursive(IAlbum album, string userName)
        {
            album.Inflate(true);
            foreach (IAlbum childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
            {
                childAlbum.Inflate(true); // The above Inflate() does not inflate child albums, so we need to explicitly inflate it.
                childAlbum.IsPrivate = album.IsPrivate;
                GalleryObjectController.SaveGalleryObject(childAlbum, userName);
                SynchIsPrivatePropertyOnChildGalleryObjectsRecursive(childAlbum, userName);
            }

            foreach (IGalleryObject childGalleryObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject))
            {
                childGalleryObject.IsPrivate = album.IsPrivate;
                GalleryObjectController.SaveGalleryObject(childGalleryObject, userName);
            }
        }

        /// <summary>
        /// Inspects the specified <paramref name="album" /> to see if the <see cref="IAlbum.OwnerUserName" /> is an existing user.
        /// If not, the property is cleared out (which also clears out the <see cref="IAlbum.OwnerRoleName" /> property).
        /// </summary>
        /// <param name="album">The album to inspect.</param>
        private static void ValidateAlbumOwner(IAlbum album)
        {
            if ((!String.IsNullOrEmpty(album.OwnerUserName)) && (!UserController.GetAllUsers().Contains(album.OwnerUserName)))
            {
                if (RoleController.GetUsersInRole(album.OwnerRoleName).Length == 0)
                {
                    RoleController.DeleteGalleryServerProRole(album.OwnerRoleName);
                }

                if (album.IsWritable)
                {
                    album.OwnerUserName = String.Empty; // This will also clear out the OwnerRoleName property.

                    GalleryObjectController.SaveGalleryObject(album);
                }
                else
                {
                    var albumToSave = Factory.LoadAlbumInstance(new AlbumLoadOptions(album.Id) { IsWritable = true });

                    albumToSave.OwnerUserName = String.Empty; // This will also clear out the OwnerRoleName property.

                    GalleryObjectController.SaveGalleryObject(albumToSave);
                }
            }
        }

        /// <summary>
        /// Gets a writable collection of the gallery objects in the album containing <paramref name="galleryItem" />, including 
        /// <paramref name="galleryItem" />. If <paramref name="galleryItem" /> does not represent a valid object, an empty 
        /// collection is returned. Guaranteed to not return null.
        /// </summary>
        /// <param name="galleryItem">A gallery item. The object must have the <see cref="GalleryItem.Id" /> and 
        /// <see cref="GalleryItem.ItemType" /> properties specified; the others are optional.</param>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        private static IGalleryObjectCollection GetWritableSiblingGalleryObjects(GalleryItem galleryItem)
        {
            IGalleryObject parentAlbum;

            try
            {
                int parentAlbumId;
                if ((GalleryObjectType)galleryItem.ItemType == GalleryObjectType.Album)
                {
                    parentAlbumId = LoadAlbumInstance(galleryItem.Id).Parent.Id;
                }
                else
                {
                    parentAlbumId = Factory.LoadMediaObjectInstance(galleryItem.Id).Parent.Id;
                }

                parentAlbum = LoadAlbumInstance(new AlbumLoadOptions(parentAlbumId) { IsWritable = true, InflateChildObjects = true });
            }
            catch (InvalidAlbumException)
            {
                parentAlbum = new NullGalleryObject();
            }
            catch (InvalidMediaObjectException)
            {
                parentAlbum = new NullGalleryObject();
            }

            return parentAlbum.GetChildGalleryObjects();
        }

        /// <summary>
        /// Reverse the gallery objects in the <paramref name="album" /> if they are custom sorted and the user
        /// clicked the reverse sort button (i.e. changed the <paramref name="previousSortAscending" /> value).
        /// This can't be handled by the normal sort routine because we aren't actually sorting on any particular
        /// metadata value.
        /// </summary>
        /// <param name="album">The album whose items are to be sorted.</param>
        /// <param name="previousSortByMetaName">The name of the metadata property the album was previously sorted on.</param>
        /// <param name="previousSortAscending">Indicates whether the album was previously sorted in ascending order.</param>
        private static void ReverseCustomSortIfNeeded(IAlbum album, MetadataItemName previousSortByMetaName, bool previousSortAscending)
        {
            var albumIsCustomSortedAndUserIsChangingSortDirection = ((album.SortByMetaName == MetadataItemName.NotSpecified)
              && (album.SortByMetaName == previousSortByMetaName)
              && (album.SortAscending != previousSortAscending));

            if (albumIsCustomSortedAndUserIsChangingSortDirection)
            {
                // Album is being manually sorted and user clicked the reverse button.
                var seq = 1;
                foreach (var galleryObject in album.GetChildGalleryObjects().ToSortedList().Reverse())
                {
                    galleryObject.Sequence = seq;
                    seq++;
                }
            }
        }

        /// <summary>
        /// Throw exception if the specified albums and/or media objects cannot be moved or copied for any reason,
        /// such as lack of user permission or trying to move/copy objects to itself. Return a writable collection of <see cref="IGalleryObject" />
        /// instances representing the <paramref name="itemsToMoveOrCopy" /> that can be used by the caller.
        /// </summary>
        /// <param name="itemsToMoveOrCopy">The albums or media objects to move or copy.</param>
        /// <param name="destinationAlbum">The album one wishes to move or copy <paramref name="itemsToMoveOrCopy" /> to.</param>
        /// <param name="transferType">Indicates whether the <paramref name="itemsToMoveOrCopy" /> are being moved or copied.</param>
        /// <returns>An instance of <see cref="IGalleryObjectCollection" />.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when one or more of the <paramref name="itemsToMoveOrCopy" /> has a file extension
        /// that is not allowed in the gallery containing the <paramref name="destinationAlbum" />.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when one of the <paramref name="itemsToMoveOrCopy" /> is a media object whose
        /// original file no longer exists.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have the necessary write permission to
        /// the <paramref name="destinationAlbum" />, is not allowed to move one or more <paramref name="itemsToMoveOrCopy" /> (when moving), or the
        /// destination album is in a read-only gallery (<see cref="IGallerySettings.MediaObjectPathIsReadOnly" /> = <c>true</c>).</exception>
        /// <exception cref="CannotTransferAlbumToNestedDirectoryException">Thrown when one or more of the <paramref name="itemsToMoveOrCopy" /> has a file extension
        /// that is not allowed in the gallery containing the <paramref name="destinationAlbum" />.</exception>
        private static IGalleryObjectCollection ThrowIfItemsCannotBeMovedOrCopied(GalleryItem[] itemsToMoveOrCopy, IAlbum destinationAlbum, GalleryAssetTransferType transferType)
        {
            bool movingOrCopyingAtLeastOneAlbum = false;
            bool movingOrCopyingAtLeastOneMediaObject = false;
            int? sourceGalleryId = null;
            IGalleryObjectCollection assetsToMoveOrCopy = new GalleryObjectCollection();

            #region Validate the albums and media objects we are moving or copying

            if (itemsToMoveOrCopy.Length == 1 && itemsToMoveOrCopy[0].Id == int.MinValue)
            {
                throw new InvalidAlbumException("Select an item and try again.");
            }

            bool securityCheckCompleteForAlbum = false;
            bool securityCheckCompleteForMediaObject = false;

            foreach (var galleryItem in itemsToMoveOrCopy)
            {
                IGalleryObject galleryObject;
                try
                {
                    galleryObject = GalleryObjectController.ToWritableGalleryObject(galleryItem);

                    assetsToMoveOrCopy.Add(galleryObject);
                }
                catch (InvalidAlbumException)
                {
                    continue; // Album may have been deleted by someone else, so just skip it.
                }
                catch (InvalidMediaObjectException)
                {
                    continue; // Media object may have been deleted by someone else, so just skip it.
                }

                if (!sourceGalleryId.HasValue)
                {
                    sourceGalleryId = galleryObject.GalleryId; // All items to transfer should be in same gallery, so we only need to get this once
                }

                if (galleryItem.IsAlbum)
                {
                    ThrowIfMovingOrCopyingAlbumToItselfOrChildAlbum((IAlbum)galleryObject, destinationAlbum);

                    if (!securityCheckCompleteForAlbum) // Only need to check albums once, since all albums belong to same parent.
                    {
                        if (transferType == GalleryAssetTransferType.Move)
                        {
                            ValidateSecurityForAlbumOrMediaObject(galleryObject, SecurityActions.DeleteAlbum);
                        }
                        securityCheckCompleteForAlbum = true;
                    }

                    movingOrCopyingAtLeastOneAlbum = true; // used below
                }
                else
                {
                    // Make sure file type is enabled (external objects don't have files so we don't check them)
                    if (galleryObject.GalleryObjectType != GalleryObjectType.External && !HelperFunctions.IsFileAuthorizedForAddingToGallery(galleryObject.Original.FileName, galleryObject.GalleryId))
                    {
                        throw new UnsupportedMediaObjectTypeException(galleryObject.Original.FileName);
                    }

                    // Make sure the file exists.
                    if (!String.IsNullOrWhiteSpace(galleryObject.Original.FileName) && !System.IO.File.Exists(galleryObject.Original.FileNamePhysicalPath))
                    {
                        throw new System.IO.FileNotFoundException(String.Format(Resources.GalleryServer.Task_Transfer_Objects_Cannot_Transfer_File_NotFound_Dtl, galleryObject.Original.FileName), galleryObject.Original.FileName);
                    }

                    if (!securityCheckCompleteForMediaObject) // Only need to check media objects once, since they all belong to same parent.
                    {
                        if (transferType == GalleryAssetTransferType.Move)
                        {
                            ValidateSecurityForAlbumOrMediaObject(galleryObject.Parent, SecurityActions.DeleteMediaObject);
                        }
                        securityCheckCompleteForMediaObject = true;
                    }

                    movingOrCopyingAtLeastOneMediaObject = true; // used below
                }
            }

            #endregion

            #region Validate user has permission to add objects to destination album

            if (destinationAlbum.GalleryId == sourceGalleryId)
            {
                // We're moving/copying items within the same gallery. Make sure gallery is writable.
                if (Factory.LoadGallerySetting(sourceGalleryId.GetValueOrDefault()).MediaObjectPathIsReadOnly)
                {
                    throw new GallerySecurityException("Cannot move or copy objects to a read only gallery");
                }
            }
            else
            {
                // User is transferring objects to another gallery. Make sure the user is an admin for the gallery
                // and that it is writable.
                var isReadOnly = Factory.LoadGallerySetting(destinationAlbum.GalleryId).MediaObjectPathIsReadOnly;
                var userIsNotAdmin = UserController.GetGalleriesCurrentUserCanAdminister().All(g => g.GalleryId != destinationAlbum.GalleryId);

                if (isReadOnly || userIsNotAdmin)
                {
                    throw new GallerySecurityException("Cannot move or copy objects to a read only gallery");
                }
            }

            if (movingOrCopyingAtLeastOneAlbum && (!Utils.IsUserAuthorized(SecurityActions.AddChildAlbum, RoleController.GetGalleryServerRolesForUser(), destinationAlbum.Id, destinationAlbum.GalleryId, destinationAlbum.IsPrivate, destinationAlbum.IsVirtualAlbum)))
            {
                throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, SecurityActions.AddChildAlbum, destinationAlbum.Id));
            }

            if (movingOrCopyingAtLeastOneMediaObject && (!Utils.IsUserAuthorized(SecurityActions.AddMediaObject, RoleController.GetGalleryServerRolesForUser(), destinationAlbum.Id, destinationAlbum.GalleryId, destinationAlbum.IsPrivate, destinationAlbum.IsVirtualAlbum)))
            {
                throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, SecurityActions.AddMediaObject, destinationAlbum.Id));
            }

            #endregion

            return assetsToMoveOrCopy;
        }

        /// <summary>
        /// Throw exception if user is trying to move or copy an album to itself or one of its children albums.
        /// </summary>
        /// <param name="albumToMoveOrCopy">The album to move or copy.</param>
        /// <param name="destinationAlbum">The album one wishes to move or copy <paramref name="albumToMoveOrCopy" /> to.</param>
        /// <exception cref="CannotTransferAlbumToNestedDirectoryException">Thrown when the user tries to move or copy an album to itself
        /// or one of its children albums.</exception>
        private static void ThrowIfMovingOrCopyingAlbumToItselfOrChildAlbum(IAlbum albumToMoveOrCopy, IAlbum destinationAlbum)
        {
            IAlbum albumParent = destinationAlbum;

            while (albumParent != null)
            {
                if (albumParent.Id == albumToMoveOrCopy.Id)
                {
                    throw new CannotTransferAlbumToNestedDirectoryException();
                }
                albumParent = albumParent.Parent as IAlbum;
            }
        }

        /// <summary>
        /// Throw exception if user does not have permission to move the specified gallery object out of the current album.
        /// Moving an album or media object means we are essentially deleting it from the source album, so make sure user has 
        /// the appropriate delete permission for the current album. Does not validate user has permission to add objects to 
        /// destination album. Assumes each gallery object is contained in the current album as retrieved by GspPage.GetAlbum().
        /// No validation is performed if we are copying since no special permissions are needed for copying (except a check 
        /// on the destination album, which we do elsewhere).
        /// </summary>
        /// <param name="galleryObjectToMoveOrCopy">The album or media object to move or copy.</param>
        /// <param name="securityActions">The security permission to validate.</param>
        /// <exception cref="GallerySecurityException">Thrown when the logged on 
        /// user does not belong to a role that authorizes the specified security action.</exception>
        private static void ValidateSecurityForAlbumOrMediaObject(IGalleryObject galleryObjectToMoveOrCopy, SecurityActions securityActions)
        {
            if (!Utils.IsUserAuthorized(securityActions, galleryObjectToMoveOrCopy.Id, galleryObjectToMoveOrCopy.GalleryId, galleryObjectToMoveOrCopy.IsPrivate, ((IAlbum)galleryObjectToMoveOrCopy).IsVirtualAlbum))
            {
                throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, securityActions, galleryObjectToMoveOrCopy.Id));
            }
        }

        /// <summary>
        /// Removes the album from the current user's profile, if present. This is useful when a user has an album sort preference stored in her
        /// profile because she only has read access to an album, but then later acquires edit access. In this case we need to delete the profile
        /// setting since this user should see the album sorted on the album sort property from now on.
        /// </summary>
        /// <param name="albumId">The album identifier.</param>
        private static void RemoveAlbumFromUserProfileIfPresent(int albumId)
        {
            var profile = ProfileController.GetProfile();

            var aProfile = profile.AlbumProfiles.Find(albumId);

            if (aProfile != null)
            {
                profile.AlbumProfiles.Remove(aProfile);
                ProfileController.SaveProfile(profile);
            }
        }

        #endregion
    }
}
