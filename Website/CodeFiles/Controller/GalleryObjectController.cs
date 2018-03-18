using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.NullObjects;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Controller
{
    /// <summary>
    /// Contains functionality for interacting with gallery objects (that is, media objects and albums). Typically web pages 
    /// directly call the appropriate business layer objects, but when a task involves multiple steps or the functionality 
    /// does not exist in the business layer, the methods here are used.
    /// </summary>
    public static class GalleryObjectController
    {
        #region Public Static Methods

        /// <overloads>
        /// Persist the gallery object to the data store.
        /// </overloads>
        /// <summary>
        /// Persist the <paramref name="galleryObject" /> to the data store. This method updates the audit fields before saving. The currently logged
        /// on user is recorded as responsible for the changes. NO SECURITY CHECK is made to verify user has permission to save the
        /// gallery object; the calling function should have already done this. All gallery objects should be
        /// saved through this method rather than directly invoking the gallery object's Save method, unless you want to 
        /// manually update the audit fields yourself.
        /// </summary>
        /// <param name="galleryObject">The gallery object to persist to the data store.</param>
        /// <remarks>When no user name is available through <see cref="Utils.UserName" />, the string &lt;unknown&gt; is
        /// substituted. Since GSP requires users to be logged on to edit objects, there will typically always be a user name 
        /// available. However, in some cases one won't be available, such as when an error occurs during self registration and
        /// the exception handling code needs to delete the just-created user album.</remarks>
        public static void SaveGalleryObject(IGalleryObject galleryObject)
        {
            var userName = (string.IsNullOrEmpty(Utils.UserName) ? Resources.GalleryServer.Site_Missing_Data_Text : Utils.UserName);
            SaveGalleryObject(galleryObject, userName);
        }

        /// <summary>
        /// Persist the <paramref name="galleryObject" /> to the data store, associating the changes with the specified <paramref name="userName" />.
        /// This method updates the audit fields before saving. NO SECURITY CHECK is made to verify user has permission to save the gallery object;
        /// the calling function should have already done this. All gallery objects should be saved through this method rather than directly invoking
        /// the gallery object's Save method, unless you want to manually update the audit fields yourself.
        /// </summary>
        /// <param name="galleryObject">The gallery object to persist to the data store.</param>
        /// <param name="userName">The user name to be associated with the modifications. This name is stored in the internal
        /// audit fields associated with this gallery object.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
        public static void SaveGalleryObject(IGalleryObject galleryObject, string userName)
        {
            if (galleryObject == null)
                throw new ArgumentNullException(nameof(galleryObject));

            DateTime currentTimestamp = DateTime.UtcNow;

            if (galleryObject.IsNew)
            {
                galleryObject.CreatedByUserName = userName;
                galleryObject.DateAdded = currentTimestamp;
            }

            if (galleryObject.HasChanges)
            {
                galleryObject.LastModifiedByUserName = userName;
                galleryObject.DateLastModified = currentTimestamp;
            }

            // Verify that any role needed for album ownership exists and is properly configured.
            RoleController.ValidateRoleExistsForAlbumOwner(galleryObject as IAlbum);

            // Persist to data store.
            galleryObject.Save();
        }

        /// <summary>
        /// Move the specified object to the specified destination album. This method moves the physical files associated with this
        /// object to the destination album's physical directory. The object's Save() method is invoked to persist the changes to the
        /// data store. When moving albums, all the album's children, grandchildren, etc are also moved. 
        /// The audit fields are automatically updated before saving.
        /// </summary>
        /// <param name="galleryObjectToMove">The gallery object to move.</param>
        /// <param name="destinationAlbum">The album to which the current object should be moved.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjectToMove" /> is null.</exception>
        public static void MoveGalleryObject(IGalleryObject galleryObjectToMove, IAlbum destinationAlbum)
        {
            if (galleryObjectToMove == null)
                throw new ArgumentNullException("galleryObjectToMove");

            string currentUser = Utils.UserName;
            DateTime currentTimestamp = DateTime.UtcNow;

            galleryObjectToMove.LastModifiedByUserName = currentUser;
            galleryObjectToMove.DateLastModified = currentTimestamp;

            galleryObjectToMove.MoveTo(destinationAlbum);
        }

        /// <summary>
        /// Copy the specified object and place it in the specified destination album. This method creates a completely separate copy
        /// of the original, including copying the physical files associated with this object. The copy is persisted to the data
        /// store and then returned to the caller. When copying albums, all the album's children, grandchildren, etc are also copied.
        /// The audit fields of the copied objects are automatically updated before saving.
        /// </summary>
        /// <param name="galleryObjectToCopy">The gallery object to copy.</param>
        /// <param name="destinationAlbum">The album to which the current object should be copied.</param>
        /// <returns>
        /// Returns a new gallery object that is an exact copy of the original, except that it resides in the specified
        /// destination album, and of course has a new ID. Child objects are recursively copied.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjectToCopy" /> is null.</exception>
        public static IGalleryObject CopyGalleryObject(IGalleryObject galleryObjectToCopy, IAlbum destinationAlbum)
        {
            if (galleryObjectToCopy == null)
                throw new ArgumentNullException("galleryObjectToCopy");

            string currentUser = Utils.UserName;

            return galleryObjectToCopy.CopyTo(destinationAlbum, currentUser);
        }

        /// <summary>
        /// Adds a media file to an album. Prior to calling this method, the file should exist in the
        /// temporary upload directory (<see cref="GlobalConstants.TempUploadDirectory" />) in the
        /// App_Data directory with the name <see cref="AddMediaObjectSettings.FileNameOnServer" />. The
        /// file is copied to the destination album and given the name of
        /// <see cref="AddMediaObjectSettings.FileName" /> (instead of whatever name it currently has, which
        /// may contain a GUID).
        /// </summary>
        /// <param name="settings">The settings that contain data and configuration options for the media file.</param>
        /// <returns>List&lt;ActionResult&gt;.</returns>
        /// <exception cref="Events.CustomExceptions.GallerySecurityException">Thrown when user is not authorized to add a media object to the album.</exception>
        public static List<ActionResult> AddMediaObject(AddMediaObjectSettings settings)
        {
            List<ActionResult> results = CreateMediaObjectFromFile(settings);

            return results;
        }

        /// <summary>
        /// Replace the original file associated with <paramref name="mediaAssetId" /> with <paramref name="editedFilePath" />. Most metadata is copied from
        /// the current file to <paramref name="editedFilePath" />. Orientation meta, if present, is removed. Some meta properties are updated (e.g. width, height).
        /// Thumbnail and optimized images are regenerated. Requires the application be running in trial mode or under a license of Home &amp; Nonprofit or higher.
        /// </summary>
        /// <param name="mediaAssetId">The ID of the media asset.</param>
        /// <param name="editedFilePath">The full path to the edited file. Ex: "C:\Dev\GS\Dev-Main\Website\App_Data\_Temp\85b74137-d795-40a5-8b93-bf31de0b0ca3.jpg"</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when a media asset is not found in the data store having ID <paramref name="mediaAssetId" />.</exception>
        /// <exception cref="GallerySecurityException">Thrown when user is not authorized to edit the media asset and when the current
        /// license does not support this functionality.</exception>
        public static ActionResult ReplaceWithEditedImage(int mediaAssetId, string editedFilePath)
        {
            var mediaAsset = Factory.LoadMediaObjectInstance(new MediaLoadOptions(mediaAssetId) { IsWritable = true });

            if (mediaAsset.MimeType.TypeCategory != MimeTypeCategory.Image)
            {
                return new ActionResult
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Unsupported File Type",
                    Message = "The function GalleryObjectController.ReplaceWithEditedImage() only supports image files."
                };
            }

            if (ShouldApplyWatermark(mediaAsset))
            {
                return new ActionResult
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Cannot Edit Watermarked Image",
                    Message = "The image you are editing has a watermark applied to it. Persisting this image would cause the watermark to become a permanent part of the image file, so we blocked the action. To edit this image, either temporarily turn off the watermarking feature or use another image editor to edit the un-watermarked image."
                };
            }

            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, RoleController.GetGalleryServerRolesForUser(), mediaAsset.Parent.Id, mediaAsset.GalleryId, Utils.IsAuthenticated, mediaAsset.Parent.IsPrivate, ((IAlbum)mediaAsset.Parent).IsVirtualAlbum);

            if (Factory.LoadGallerySetting(mediaAsset.GalleryId).MediaObjectPathIsReadOnly)
            {
                throw new GallerySecurityException(Resources.GalleryServer.Task_Modify_Asset_Cannot_Modify_MediaPathIsReadOnly);
            }

            if (AppSetting.Instance.License.LicenseType <= LicenseLevel.Free)
            {
                throw new GallerySecurityException("The image editor requires Gallery Server Home & Nonprofit or higher.");
            }

            // Grab a reference to the original file name so we can delete it later.
            var originalFilePath = mediaAsset.Original.FileNamePhysicalPath;

            mediaAsset.Original.FileInfo = MergeImages(editedFilePath, mediaAsset);

            UpdateMetadataAndDerivedImagesForEditedMediaAsset(mediaAsset);

            // Replace the existing image with the edited one, then update and save media asset.
            HelperFunctions.MoveFileSafely(mediaAsset.Original.FileNamePhysicalPath, originalFilePath);

            mediaAsset.Original.FileInfo = new FileInfo(originalFilePath);
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.FileName));

            SaveGalleryObject(mediaAsset);

            // Clean up: We no longer need the file the client uploaded to the App_Data\_Temp directory.
            if (File.Exists(editedFilePath))
            {
                File.Delete(editedFilePath);
            }

            return new ActionResult
            {
                Title = "Media Asset Updated",
                Status = ActionResultStatus.Success.ToString()
            };
        }

        /// <summary>
        /// Replace the original file associated with <paramref name="mediaAssetId" /> with <paramref name="fileNameOnServer" /> and giving it the name 
        /// <paramref name="fileName" />. Re-extract relevant metadata such as width, height, orientation, video/audio info, etc. Thumbnail and 
        /// optimized images are regenerated. The original file is not modified.
        /// </summary>
        /// <param name="mediaAssetId">The ID of the media asset.</param>
        /// <param name="fileNameOnServer">The full path to the edited file. Ex: "C:\Dev\GS\Dev-Main\Website\App_Data\_Temp\85b74137-d795-40a5-8b93-bf31de0b0ca3.jpg"</param>
        /// <param name="fileName">Name the file should be given when it is persisted to the album directory.</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when a media asset is not found in the data store having ID <paramref name="mediaAssetId" />.</exception>
        /// <exception cref="GallerySecurityException">Thrown when user does not have edit media asset permission.</exception>
        public static ActionResult ReplaceMediaAssetFile(int mediaAssetId, string fileNameOnServer, string fileName)
        {
            var mediaAsset = Factory.LoadMediaObjectInstance(new MediaLoadOptions(mediaAssetId) { IsWritable = true });

            var mimeType = Factory.LoadMimeType(mediaAsset.GalleryId, fileNameOnServer);
            if (mimeType == null || !mimeType.AllowAddToGallery)
            {
                return new ActionResult
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Unsupported File Type",
                    Message = $"The file extension {Path.GetExtension(fileNameOnServer)} is not recognized or it is not enabled."
                };
            }

            if (Factory.LoadMimeType(fileName).TypeCategory != mediaAsset.MimeType.TypeCategory)
            {
                return new ActionResult
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Incompatible media asset type",
                    Message = $"A media asset file can only be replaced by a similar type. For example, images can only be replaced with images, videos can only be replaced with videos, etc. Choose a compatible asset type and try again."
                };
            }

            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, RoleController.GetGalleryServerRolesForUser(), mediaAsset.Parent.Id, mediaAsset.GalleryId, Utils.IsAuthenticated, mediaAsset.Parent.IsPrivate, ((IAlbum)mediaAsset.Parent).IsVirtualAlbum);

            if (Factory.LoadGallerySetting(mediaAsset.GalleryId).MediaObjectPathIsReadOnly)
            {
                throw new GallerySecurityException(Resources.GalleryServer.Task_Modify_Asset_Cannot_Modify_MediaPathIsReadOnly);
            }

            if (File.Exists(mediaAsset.Original.FileNamePhysicalPath))
            {
                File.Delete(mediaAsset.Original.FileNamePhysicalPath);
            }

            var replacementFilePath = mediaAsset.Original.FileNamePhysicalPath;
            var fileNameHasChanged = false;

            if (!fileName.Equals(Path.GetFileName(mediaAsset.Original.FileName), StringComparison.InvariantCultureIgnoreCase))
            {
                // User uploaded a file with a name that is different than the original's file name. Verify it doesn't match any other filename in the album.
                fileNameHasChanged = true;
                var albumPhysicalPath = mediaAsset.Parent.FullPhysicalPathOnDisk;
                var tmpFilename = HelperFunctions.ValidateFileName(albumPhysicalPath, Path.GetFileName(fileName));
                replacementFilePath = Path.Combine(albumPhysicalPath, tmpFilename);
            }

            HelperFunctions.MoveFileSafely(fileNameOnServer, replacementFilePath);

            if (fileNameHasChanged)
            {
                // Update the filename and save. We need to do this before the next step so that the filename is safely in the DB before the next save tries
                // to generate optimized videos through the queue processor.
                mediaAsset.Original.FileName = Path.GetFileName(replacementFilePath);
                IGalleryObjectMetadataItem metaItem;
                if (mediaAsset.MetadataItems.TryGetMetadataItem(MetadataItemName.FileName, out metaItem))
                {
                    metaItem.Value = mediaAsset.Original.FileName;
                }

                SaveGalleryObject(mediaAsset);
            }

            mediaAsset.Original.FileInfo = new FileInfo(replacementFilePath);

            UpdateMetadataAndDerivedImagesForReplacedMediaAsset(mediaAsset, false);

            CacheController.RemoveMediaAssetFromCache(mediaAsset.Id);
            CacheController.RemoveInflatedAlbumsFromCache();

            return new ActionResult
            {
                Title = "Media Asset Replaced",
                Status = ActionResultStatus.Success.ToString()
            };
        }

        /// <summary>
        /// Executes the requested <paramref name="rotateFlip" /> action on the <paramref name="galleryItems" />. Validation is performed
        /// to ensure logged on user has <see cref="SecurityActions.EditMediaObject" /> permission and that none of the items are in a read-only
        /// gallery.
        /// </summary>
        /// <param name="galleryItems">The gallery items to rotate or flip.</param>
        /// <param name="rotateFlip">The requested rotate / flip action.</param>
        /// <param name="viewSize">The size of the image that is currently in context. Typically this means the size of the image the user 
        /// is looking at.</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleryItems" /> is null.</exception>
        public static ActionResult RotateFlip(GalleryItem[] galleryItems, MediaAssetRotateFlip rotateFlip, DisplayObjectType viewSize)
        {
            if (galleryItems == null)
                throw new ArgumentNullException(nameof(galleryItems));

            var errMsg = "<p>The following items could not be rotated/flipped:</p>";
            var hasError = false;
            var infoMsg = string.Empty;
            var rotatedItems = new List<GalleryItem>();
            var supportedRotateFlipTypes = new[] { GalleryObjectType.Image, GalleryObjectType.Video };
            var validationPassed = false;

            try
            {
                validationPassed = ValidateItemsAreNotInReadOnlyGallery(galleryItems);
            }
            catch (GallerySecurityException ex)
            {
                hasError = true;
                errMsg = $"<p>{ex.Message}</p>";
            }

            if (validationPassed) // Don't bother trying to delete anything if we got an error above
            {
                foreach (var galleryItem in galleryItems)
                {
                    try
                    {
                        IGalleryObject mo;
                        try
                        {
                            mo = Factory.LoadMediaObjectInstance(new MediaLoadOptions(galleryItem.Id) { IsWritable = true });

                            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, RoleController.GetGalleryServerRolesForUser(), mo.Parent.Id, mo.GalleryId, Utils.IsAuthenticated, mo.Parent.IsPrivate, ((IAlbum)mo.Parent).IsVirtualAlbum);

                            if (!supportedRotateFlipTypes.Contains(mo.GalleryObjectType))
                            {
                                errMsg += $"<p>{galleryItem.Title} ({mo.GalleryObjectType}) - This media type cannot be rotated or flipped.</p>";
                                hasError = true;
                                continue;
                            }

                            if (mo.GalleryObjectType == GalleryObjectType.Video && string.IsNullOrEmpty(AppSetting.Instance.FFmpegPath))
                            {
                                errMsg += $"<p>{galleryItem.Title} ({mo.GalleryObjectType}) - Gallery Server Binary Pack required to rotate or flip videos. <a href='https://galleryserverpro.com/try-it/'>Get it here.</a></p>";
                                hasError = true;
                                continue;
                            }
                        }
                        catch (InvalidMediaObjectException)
                        {
                            continue; // Media object may have been deleted by someone else, so just skip it.
                        }

                        mo.RotateFlip = GetRotateFlip(mo, rotateFlip, viewSize);

                        SaveGalleryObject(mo);

                        if (mo.GalleryObjectType == GalleryObjectType.Image)
                        {
                            rotatedItems.Add(galleryItem);
                        }
                        else if (mo.GalleryObjectType == GalleryObjectType.Video)
                        {
                            infoMsg += $"<p>The video '{galleryItem.Title}' is being processed on the server and will be finished shortly.</p>";
                        }
                    }
                    catch (UnsupportedImageTypeException ex)
                    {
                        errMsg += $"<p>{galleryItem.Title} - {ex.Message}</p>";
                        hasError = true;
                    }
                    catch (GallerySecurityException ex)
                    {
                        errMsg = $"<p>{ex.Message}</p>";
                        hasError = true;
                    }
                }
            }

            if (hasError)
            {
                return new ActionResult()
                {
                    // At least one error occurred: Status is 'Warning' if at least one item could be rotated/flipped; otherwise 'Error' if all failed
                    Status = (rotatedItems.Count > 0 ? ActionResultStatus.Warning.ToString() : ActionResultStatus.Error.ToString()),
                    Title = Resources.GalleryServer.Task_RotateFlip_Objects_Cannot_RotateFlip_Asset_Msg_Hdr,
                    Message = errMsg,
                    ActionTarget = rotatedItems.ToArray()
                };
            }
            else if (!string.IsNullOrEmpty(infoMsg))
            {
                return new ActionResult()
                {
                    // We have an informational message for the user
                    Status = ActionResultStatus.Success.ToString(),
                    Title = Resources.GalleryServer.Msg_ObjectsSuccessfullyRotatedFlipped_Hdr,
                    Message = infoMsg,
                    ActionTarget = rotatedItems.ToArray()
                };
            }
            else
            {
                return new ActionResult()
                {
                    Status = ActionResultStatus.Success.ToString(),
                    Title = Resources.GalleryServer.Msg_ObjectsSuccessfullyRotatedFlipped_Hdr,
                    Message = null,
                    ActionTarget = rotatedItems.ToArray()
                };
            }
        }

        /// <summary>
        /// Gets the gallery objects in the album. Includes albums and media objects.
        /// </summary>
        /// <param name="albumId">The album ID.</param>
        /// <param name="sortByMetaName">The sort by meta name id.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <returns>Returns an <see cref="IQueryable" /> instance of <see cref="Entity.GalleryItem" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when an album with the specified
        /// <paramref name="albumId" /> is not found in the data store.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user does not have at least one of the requested permissions to the
        /// specified album.</exception>
        public static IQueryable<GalleryItem> GetGalleryItemsInAlbum(int albumId, MetadataItemName sortByMetaName, bool sortAscending)
        {
            IAlbum album = Factory.LoadAlbumInstance(new AlbumLoadOptions(albumId) { InflateChildObjects = true });

            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

            IList<IGalleryObject> galleryObjects;

            if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(sortByMetaName))
            {
                galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.All, !Utils.IsAuthenticated).ToSortedList(sortByMetaName, sortAscending, album.GalleryId);
            }
            else
            {
                galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.All, !Utils.IsAuthenticated).ToSortedList();
            }

            return ToGalleryItems(galleryObjects).AsQueryable();
        }

        //public static IQueryable<GalleryItem> GetGalleryItemsHavingTags(string[] tags, string[] people, int galleryId, MetadataItemName sortByMetaName, bool sortAscending, GalleryObjectType filter)
        //{
        //	IAlbum album = GetGalleryObjectsHavingTags(tags, people, filter, galleryId);

        //	IList<IGalleryObject> galleryObjects;

        //	if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(sortByMetaName))
        //	{
        //		galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.All, !Utils.IsAuthenticated).ToSortedList(sortByMetaName, sortAscending, album.GalleryId);
        //	}
        //	else
        //	{
        //		galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.All, !Utils.IsAuthenticated).ToSortedList();
        //	}

        //	return ToGalleryItems(galleryObjects).AsQueryable();
        //}

        /// <summary>
        /// Return a virtual album containing gallery objects whose title or caption contain the specified search strings and
        /// for which the current user has authorization to view. Guaranteed to not return null. A gallery 
        /// object is considered a match when all search terms are found in the relevant fields.
        /// </summary>
        /// <param name="searchStrings">The strings to search for.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration.</param>
        /// <param name="galleryId">The ID for the gallery containing the objects to search.</param>
        /// <returns>
        /// Returns an <see cref="IAlbum" /> containing the matching items. This may include albums and media
        /// objects from different albums.
        /// </returns>
        public static IAlbum GetGalleryObjectsHavingTitleOrCaption(string[] searchStrings, GalleryObjectType filter, int galleryId)
        {
            if (searchStrings == null)
                throw new ArgumentNullException();

            var tmpAlbum = Factory.CreateEmptyAlbumInstance(galleryId);
            tmpAlbum.IsVirtualAlbum = true;
            tmpAlbum.VirtualAlbumType = VirtualAlbumType.TitleOrCaption;
            tmpAlbum.Title = Utils.HtmlEncode(String.Concat(Resources.GalleryServer.Site_Search_Title, String.Join(Resources.GalleryServer.Site_Search_Concat, searchStrings)));
            tmpAlbum.Caption = String.Empty;
            tmpAlbum.IsInflated = true;

            var searchOptions = new GalleryObjectSearchOptions
            {
                GalleryId = galleryId,
                SearchType = GalleryObjectSearchType.SearchByTitleOrCaption,
                SearchTerms = searchStrings,
                IsUserAuthenticated = Utils.IsAuthenticated,
                Roles = RoleController.GetGalleryServerRolesForUser(),
                Filter = filter
            };

            var searcher = new GalleryObjectSearcher(searchOptions);

            foreach (var galleryObject in searcher.Find())
            {
                tmpAlbum.AddGalleryObject(galleryObject);
            }

            return tmpAlbum;
        }

        /// <summary>
        /// Return a virtual album containing gallery objects that match the specified search strings and
        /// for which the current user has authorization to view. Guaranteed to not return null. A gallery 
        /// object is considered a match when all search terms are found in the relevant fields.
        /// </summary>
        /// <param name="searchStrings">The strings to search for.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration.</param>
        /// <param name="galleryId">The ID for the gallery containing the objects to search.</param>
        /// <returns>
        /// Returns an <see cref="IAlbum" /> containing the matching items. This may include albums and media
        /// objects from different albums.
        /// </returns>
        public static IAlbum GetGalleryObjectsHavingSearchString(string[] searchStrings, GalleryObjectType filter, int galleryId)
        {
            if (searchStrings == null)
                throw new ArgumentNullException();

            var tmpAlbum = Factory.CreateEmptyAlbumInstance(galleryId);
            tmpAlbum.IsVirtualAlbum = true;
            tmpAlbum.VirtualAlbumType = VirtualAlbumType.Search;
            tmpAlbum.Title = Utils.HtmlEncode(String.Concat(Resources.GalleryServer.Site_Search_Title, String.Join(Resources.GalleryServer.Site_Search_Concat, searchStrings)));
            tmpAlbum.Caption = String.Empty;
            tmpAlbum.IsInflated = true;

            var searchOptions = new GalleryObjectSearchOptions
            {
                GalleryId = galleryId,
                SearchType = GalleryObjectSearchType.SearchByKeyword,
                SearchTerms = searchStrings,
                IsUserAuthenticated = Utils.IsAuthenticated,
                Roles = RoleController.GetGalleryServerRolesForUser(),
                Filter = filter
            };

            var searcher = new GalleryObjectSearcher(searchOptions);

            foreach (var galleryObject in searcher.Find())
            {
                tmpAlbum.AddGalleryObject(galleryObject);
            }

            return tmpAlbum;
        }

        /// <summary>
        /// Gets a virtual album containing gallery objects that match the specified <paramref name="tags" /> or <paramref name="people" />
        /// belonging to the specified <paramref name="galleryId" />. Guaranteed to not return null. The returned album 
        /// is a virtual one (<see cref="IAlbum.IsVirtualAlbum" />=<c>true</c>) containing the collection of matching 
        /// items the current user has permission to view. Returns an empty album when no matches are found or the 
        /// query string does not contain the search terms.
        /// </summary>
        /// <param name="tags">The tags to search for. If specified, the <paramref name="people" /> parameter must be null.</param>
        /// <param name="people">The people to search for. If specified, the <paramref name="tags" /> parameter must be null.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration.</param>
        /// <param name="galleryId">The ID of the gallery. Only objects in this gallery are returned.</param>
        /// <returns>An instance of <see cref="IAlbum" />.</returns>
        /// <exception cref="System.ArgumentException">Throw when the tags and people parameters are both null or empty, or both
        /// have values.</exception>
        public static IAlbum GetGalleryObjectsHavingTags(string[] tags, string[] people, GalleryObjectType filter, int galleryId)
        {
            if (((tags == null) || (tags.Length == 0)) && ((people == null) || (people.Length == 0)))
                throw new ArgumentException("GalleryObjectController.GetGalleryObjectsHavingTags() requires the tags or people parameters to be specified, but they were both null or empty.");

            if ((tags != null) && (tags.Length > 0) && (people != null) && (people.Length > 0))
                throw new ArgumentException("GalleryObjectController.GetGalleryObjectsHavingTags() requires EITHER the tags or people parameters to be specified, but not both. Instead, they were both populated.");

            var searchType = (tags != null && tags.Length > 0 ? GalleryObjectSearchType.SearchByTag : GalleryObjectSearchType.SearchByPeople);
            var searchTags = (searchType == GalleryObjectSearchType.SearchByTag ? tags : people);

            var tmpAlbum = Factory.CreateEmptyAlbumInstance(galleryId);
            tmpAlbum.IsVirtualAlbum = true;
            tmpAlbum.VirtualAlbumType = (searchType == GalleryObjectSearchType.SearchByTag ? VirtualAlbumType.Tag : VirtualAlbumType.People);
            tmpAlbum.Title = Utils.HtmlEncode(String.Concat(Resources.GalleryServer.Site_Tag_Title, String.Join(Resources.GalleryServer.Site_Search_Concat, searchTags)));
            tmpAlbum.Caption = String.Empty;
            tmpAlbum.IsInflated = true;

            var searcher = new GalleryObjectSearcher(new GalleryObjectSearchOptions
            {
                SearchType = searchType,
                Tags = searchTags,
                GalleryId = galleryId,
                Roles = RoleController.GetGalleryServerRolesForUser(),
                IsUserAuthenticated = Utils.IsAuthenticated,
                Filter = filter
            });

            foreach (var galleryObject in searcher.Find())
            {
                tmpAlbum.AddGalleryObject(galleryObject);
            }

            return tmpAlbum;
        }

        /// <summary>
        /// Gets the gallery objects most recently added to the gallery having <paramref name="galleryId" />.
        /// </summary>
        /// <param name="top">The maximum number of results to return. Must be greater than zero.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.</param>
        /// <returns>An instance of <see cref="IAlbum" />.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="top" /> is less than or equal to zero.</exception>
        public static IAlbum GetMostRecentlyAddedGalleryObjects(int top, int galleryId, GalleryObjectType filter)
        {
            if (top <= 0)
                throw new ArgumentException("The top parameter must contain a number greater than zero.", "top");

            var tmpAlbum = Factory.CreateEmptyAlbumInstance(galleryId);

            tmpAlbum.IsVirtualAlbum = true;
            tmpAlbum.VirtualAlbumType = VirtualAlbumType.MostRecentlyAdded;
            tmpAlbum.Title = Resources.GalleryServer.Site_Recently_Added_Title;
            tmpAlbum.Caption = String.Empty;
            tmpAlbum.SortByMetaName = MetadataItemName.DateAdded;
            tmpAlbum.SortAscending = false;
            tmpAlbum.IsInflated = true;

            var searcher = new GalleryObjectSearcher(new GalleryObjectSearchOptions
            {
                SearchType = GalleryObjectSearchType.MostRecentlyAdded,
                GalleryId = galleryId,
                Roles = RoleController.GetGalleryServerRolesForUser(),
                IsUserAuthenticated = Utils.IsAuthenticated,
                MaxNumberResults = top,
                Filter = filter
            });

            foreach (var galleryObject in searcher.Find())
            {
                tmpAlbum.AddGalleryObject(galleryObject);
            }

            return tmpAlbum;
        }

        /// <summary>
        /// Gets the media objects having the specified <paramref name="rating" /> and belonging to the
        /// <paramref name="galleryId" />.
        /// </summary>
        /// <param name="rating">Identifies the type of rating to retrieve. Valid values: "highest", "lowest", "none", or a number
        /// from 0 to 5 in half-step increments (eg. 0, 0.5, 1, 1.5, ... 4.5, 5).</param>
        /// <param name="top">The maximum number of results to return. Must be greater than zero.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.</param>
        /// <returns>An instance of <see cref="IAlbum" />.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="top" /> is less than or equal to zero.</exception>
        public static IAlbum GetRatedMediaObjects(string rating, int top, int galleryId, GalleryObjectType filter)
        {
            if (top <= 0)
                throw new ArgumentException("The top parameter must contain a number greater than zero.", "top");

            var tmpAlbum = Factory.CreateEmptyAlbumInstance(galleryId);

            tmpAlbum.IsVirtualAlbum = true;
            tmpAlbum.VirtualAlbumType = VirtualAlbumType.Rated;
            tmpAlbum.Title = Utils.HtmlEncode(GetRatedAlbumTitle(rating));
            tmpAlbum.Caption = String.Empty;
            tmpAlbum.IsInflated = true;

            var ratingSortTrigger = new[] { "lowest", "highest" };
            if (ratingSortTrigger.Contains(rating))
            {
                // Sort on rating field for lowest or highest. All others use the default album sort setting.
                tmpAlbum.SortByMetaName = MetadataItemName.Rating;
                tmpAlbum.SortAscending = !rating.Equals("highest", StringComparison.OrdinalIgnoreCase);
            }

            var searcher = new GalleryObjectSearcher(new GalleryObjectSearchOptions
            {
                SearchType = GalleryObjectSearchType.SearchByRating,
                SearchTerms = new[] { rating },
                GalleryId = galleryId,
                Roles = RoleController.GetGalleryServerRolesForUser(),
                IsUserAuthenticated = Utils.IsAuthenticated,
                MaxNumberResults = top,
                Filter = filter
            });

            foreach (var galleryObject in searcher.Find())
            {
                tmpAlbum.AddGalleryObject(galleryObject);
            }

            return tmpAlbum;
        }

        /// <summary>
        /// Sorts the gallery items passed to this method and return. No changes are made to the data store.
        /// When the album is virtual, the <see cref="Entity.Album.GalleryItems" /> property
        /// must be populated with the items to sort. For non-virtual albums (those with a valid ID), the 
        /// gallery objects are retrieved based on the ID and then sorted. The sort preference is saved to 
        /// the current user's profile, except when the album is virtual or when the user has edit permission
        /// on the album (in which case the album sort is saved at the album level and not at the profile level).
        /// The method incorporates security to ensure only authorized items are returned to the user.
        /// </summary>
        /// <param name="albumEntity">The album to be sorted. If it's a virtual album (e.g. the ID is <see cref="int.MinValue" />),
        /// then the <see cref="Entity.Album.GalleryItems" /> property must be populated.</param>
        /// <returns>IQueryable{Entity.GalleryItem}.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the user does not have view permission to the specified album.
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist in the data store.</exception>
        /// the user does not have view permission to the specified album.</exception>
        public static IQueryable<Entity.GalleryItem> SortGalleryItems(Entity.Album albumEntity)
        {
            // If album.ID > int.minValue then load physical album, sort & save user preference;
            // Otherwise sort the album.GalleryItems

            IAlbum album;
            if (albumEntity.Id > int.MinValue)
            {
                album = Factory.LoadAlbumInstance(new AlbumLoadOptions(albumEntity.Id) { InflateChildObjects = true });

                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

                if (!SecurityManager.IsUserAuthorized(SecurityActions.EditAlbum, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum))
                {
                    // Save to user profile only when user doesn't have edit access to the album. A user with edit album permission
                    // will get to this method when copying an item to its own album, in which case the javascript copy routine
                    // requests a non-persisting sort.
                    PersistUserSortPreference(album, albumEntity.SortById, albumEntity.SortUp);
                }
            }
            else
            {
                album = Factory.CreateAlbumInstance(albumEntity.Id, albumEntity.GalleryId);
                album.IsVirtualAlbum = (albumEntity.VirtualType != VirtualAlbumType.NotVirtual);
                album.VirtualAlbumType = albumEntity.VirtualType;

                var roles = RoleController.GetGalleryServerRolesForUser();

                foreach (var galleryItem in albumEntity.GalleryItems)
                {
                    if (galleryItem.IsAlbum)
                    {
                        var childAlbum = Factory.LoadAlbumInstance(galleryItem.Id);

                        if (SecurityManager.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, childAlbum.Id, childAlbum.GalleryId, Utils.IsAuthenticated, childAlbum.IsPrivate, childAlbum.IsVirtualAlbum))
                            album.AddGalleryObject(childAlbum);
                    }
                    else
                    {
                        var mediaObject = Factory.LoadMediaObjectInstance(galleryItem.Id);

                        if (SecurityManager.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, mediaObject.Parent.Id, mediaObject.GalleryId, Utils.IsAuthenticated, mediaObject.Parent.IsPrivate, ((IAlbum)mediaObject.Parent).IsVirtualAlbum))
                            album.AddGalleryObject(mediaObject);
                    }
                }
            }

            var galleryObjects = album
              .GetChildGalleryObjects(GalleryObjectType.All, !Utils.IsAuthenticated)
              .ToSortedList(albumEntity.SortById, albumEntity.SortUp, album.GalleryId);

            return ToGalleryItems(galleryObjects).AsQueryable();
        }

        /// <summary>
        /// Gets the media objects in the album (excludes albums).
        /// </summary>
        /// <param name="albumId">The album id.</param>
        /// <param name="sortByMetaName">The sort by meta name id.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <returns>Returns an <see cref="IQueryable" /> instance of <see cref="Entity.MediaItem" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when an album with the specified 
        /// <paramref name = "albumId" /> is not found in the data store.</exception>
        /// <exception cref="GallerySecurityException">
        /// Throw when the user does not have view permission to the specified album.</exception>
        public static IQueryable<MediaItem> GetMediaItemsInAlbum(int albumId, MetadataItemName sortByMetaName, bool sortAscending)
        {
            IAlbum album = Factory.LoadAlbumInstance(new AlbumLoadOptions(albumId) { InflateChildObjects = true });
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

            IList<IGalleryObject> galleryObjects;

            if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(sortByMetaName))
            {
                galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).ToSortedList(sortByMetaName, sortAscending, album.GalleryId);
            }
            else
            {
                galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).ToSortedList();
            }

            //var galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).ToSortedList();

            return ToMediaItems(galleryObjects).AsQueryable();
        }

        /// <summary>
        /// Gets the meta items for the media asset having the ID matching <see cref="id" />.
        /// </summary>
        /// <param name="id">The ID of the media asset.</param>
        /// <returns>An array of <see cref="MetaItem" /> instances.</returns>
        public static MetaItem[] GetMetaItemsForMediaObject(int id)
        {
            IGalleryObject mo = Factory.LoadMediaObjectInstance(id);
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), mo.Parent.Id, mo.GalleryId, Utils.IsAuthenticated, mo.Parent.IsPrivate, ((IAlbum)mo.Parent).IsVirtualAlbum);

            return ToMetaItems(mo.MetadataItems.GetVisibleItems(), mo);
        }

        /// <summary>
        /// Converts the <paramref name="metadataItems" /> belonging to <paramref name="galleryObject" /> to an array of <see cref="MetaItem" /> instances.
        /// </summary>
        /// <param name="metadataItems">The metadata items.</param>
        /// <param name="galleryObject">The gallery object.</param>
        /// <returns>An array of <see cref="MetaItem" /> instances.</returns>
        public static MetaItem[] ToMetaItems(IGalleryObjectMetadataItemCollection metadataItems, IGalleryObject galleryObject)
        {
            var metaItems = new MetaItem[metadataItems.Count];
            var metaDefs = Factory.LoadGallerySetting(galleryObject.GalleryId).MetadataDisplaySettings;
            var moProfiles = ProfileController.GetProfile().MediaObjectProfiles;

            for (int i = 0; i < metaItems.Length; i++)
            {
                IGalleryObjectMetadataItem md = metadataItems[i];

                var metaDef = metaDefs.Find(md.MetadataItemName);

                // The HTML editor requires the trial version or Home & Nonprofit or higher.
                var editMode = (metaDef.UserEditMode == PropertyEditorMode.TinyMCEHtmlEditor && AppSetting.Instance.License.LicenseType < LicenseLevel.HomeNonprofit ? PropertyEditorMode.PlainTextEditor : metaDef.UserEditMode);

                metaItems[i] = new MetaItem
                {
                    Id = md.MediaObjectMetadataId,
                    MediaId = galleryObject.Id,
                    MTypeId = (int)md.MetadataItemName,
                    GTypeId = (int)galleryObject.GalleryObjectType,
                    Desc = md.Description,
                    Value = md.Value,
                    //IsEditable = metaDef.IsEditable,
                    EditMode = editMode
                };

                if (md.MetadataItemName == MetadataItemName.Rating)
                {
                    ReplaceAvgRatingWithUserRating(metaItems[i], moProfiles);
                }
            }

            return metaItems;
        }

        //public static IQueryable<Entity.MetaItem> GetMetaItemsForMediaObject(int id)
        //{
        //	var metadataItems = new List<Entity.MetaItem>();

        //	IGalleryObject mo = Factory.LoadMediaObjectInstance(id);
        //	SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), mo.Parent.Id, mo.GalleryId, Utils.IsAuthenticated, mo.Parent.IsPrivate);

        //	foreach (IGalleryObjectMetadataItem md in mo.MetadataItems.GetVisibleItems())
        //	{
        //		metadataItems.Add(new Entity.MetaItem
        //												{
        //													Id = md.MediaObjectMetadataId,
        //													TypeId = (int)md.MetadataItemName,
        //													Desc = md.Description,
        //													Value = md.Value,
        //													IsEditable = false
        //												});
        //	}

        //	return metadataItems.AsQueryable();
        //}

        /// <summary>
        /// Converts the <paramref name="galleryObjects" /> to an enumerable collection of 
        /// <see cref="Entity.GalleryItem" /> instances. Guaranteed to not return null.
        /// </summary>
        /// <param name="galleryObjects">The gallery objects.</param>
        /// <returns>An enumerable collection of <see cref="Entity.GalleryItem" /> instances.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static GalleryItem[] ToGalleryItems(IList<IGalleryObject> galleryObjects)
        {
            if (galleryObjects == null)
                throw new ArgumentNullException("galleryObjects");

            var gEntities = new List<GalleryItem>(galleryObjects.Count);

            gEntities.AddRange(galleryObjects.Select(galleryObject => ToGalleryItem(galleryObject, MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(galleryObject))));

            return gEntities.ToArray();
        }

        /// <summary>
        /// Converts the <paramref name="mediaObjects" /> to an enumerable collection of 
        /// <see cref="Entity.MediaItem" /> instances. Guaranteed to not return null. Do not pass any 
        /// <see cref="IAlbum" /> instances to this function.
        /// </summary>
        /// <param name="mediaObjects">The media objects.</param>
        /// <returns>An enumerable collection of <see cref="Entity.MediaItem" /> instances.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static MediaItem[] ToMediaItems(IList<IGalleryObject> mediaObjects)
        {
            if (mediaObjects == null)
                throw new ArgumentNullException("mediaObjects");

            var moEntities = new List<MediaItem>(mediaObjects.Count);
            var moBuilderOptions = MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(null);

            var i = 1;
            moEntities.AddRange(mediaObjects.Select(mo => ToMediaItem(mo, i++, moBuilderOptions)));

            return moEntities.ToArray();
        }

        /// <summary>
        /// Converts the <paramref name="galleryObject" /> to an instance of <see cref="Entity.GalleryItem" />.
        /// The instance can be JSON-serialized and sent to the browser.
        /// </summary>
        /// <param name="galleryObject">The gallery object to convert to an instance of
        /// <see cref="Entity.GalleryItem" />. It may be a media object or album.</param>
        /// <param name="moBuilderOptions">A set of properties to be used to build the HTML, JavaScript or URL for the 
        /// <paramref name="galleryObject" />.</param>
        /// <returns>Returns an <see cref="Entity.GalleryItem" /> object containing information
        /// about the requested item.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> or 
        /// <paramref name="moBuilderOptions" /> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="moBuilderOptions" /> does
        /// has a null or empty <see cref="MediaObjectHtmlBuilderOptions.Browsers" /> property.</exception>
        public static GalleryItem ToGalleryItem(IGalleryObject galleryObject, MediaObjectHtmlBuilderOptions moBuilderOptions)
        {
            if (galleryObject == null)
                throw new ArgumentNullException("galleryObject");

            if (moBuilderOptions == null)
                throw new ArgumentNullException("moBuilderOptions");

            if (moBuilderOptions.Browsers == null || moBuilderOptions.Browsers.Length == 0)
                throw new ArgumentOutOfRangeException("moBuilderOptions.Browsers", "The Browsers array property must have at least one element.");

            moBuilderOptions.GalleryObject = galleryObject;

            var gItem = new GalleryItem
            {
                Id = galleryObject.Id,
                ParentId = galleryObject.Parent.Id,
                Title = galleryObject.Title,
                Caption = galleryObject.Caption,
                Views = GetViews(moBuilderOptions).ToArray(),
                ViewIndex = 0,
                MimeType = (int)galleryObject.MimeType.TypeCategory,
                ItemType = (int)galleryObject.GalleryObjectType
            };

            IAlbum album = galleryObject as IAlbum;
            if (album != null)
            {
                gItem.IsAlbum = true;
                //gItem.DateStart = album.DateStart;
                //gItem.DateEnd = album.DateEnd;
                gItem.NumAlbums = album.GetChildGalleryObjects(GalleryObjectType.All, !Utils.IsAuthenticated).Count;
                gItem.NumMediaItems = album.GetChildGalleryObjects(GalleryObjectType.MediaObject, !Utils.IsAuthenticated).Count;
            }

            return gItem;
        }

        /// <summary>
        /// Converts the <paramref name="galleryItem" /> to a writable <see cref="IGalleryObject" /> instance. Note that this method does not copy
        /// properties of <paramref name="galleryItem" /> to the returned instance; rather, it retrieves an inflated instance from the business
        /// layer based on <see cref="GalleryItem.Id" />.
        /// </summary>
        /// <param name="galleryItem">The gallery item.</param>
        /// <returns>An instance of <see cref="IGalleryObject" />. Albums can be cast to <see cref="IAlbum" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryItem" />  is null.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when no album exists in the data store matching the ID stored in <see cref="GalleryItem.Id" />.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when no media object exists in the data store matching the ID stored in 
        /// <see cref="GalleryItem.Id" />.</exception>
        public static IGalleryObject ToWritableGalleryObject(GalleryItem galleryItem)
        {
            if (galleryItem == null)
                throw new ArgumentNullException(nameof(galleryItem));

            if (galleryItem.IsAlbum)
            {
                return AlbumController.LoadAlbumInstance(new AlbumLoadOptions(galleryItem.Id) { IsWritable = true });
            }
            else
            {
                return Factory.LoadMediaObjectInstance(new MediaLoadOptions(galleryItem.Id) { IsWritable = true });
            }
        }

        /// <summary>
        /// Converts the <paramref name="mediaObject"/> to an instance of <see cref="Entity.MediaItem" />.
        /// The returned object DOES have the <see cref="Entity.MediaItem.MetaItems" /> property assigned.
        /// The instance can be JSON-serialized and sent to the browser. Do not pass an 
        /// <see cref="IAlbum" /> to this function.
        /// </summary>
        /// <param name="mediaObject">The media object to convert to an instance of
        /// <see cref="Entity.MediaItem"/>.</param>
        /// <param name="indexInAlbum">The one-based index of this media object within its album. This value is assigned to 
        /// <see cref="Entity.MediaItem.Index" />.</param>
        /// <param name="moBuilderOptions">A set of properties to be used to build the HTML, JavaScript or URL for the 
        /// <paramref name="mediaObject" />.</param>
        /// <returns>Returns an <see cref="Entity.MediaItem"/> object containing information
        /// about the requested media object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> or 
        /// <paramref name="moBuilderOptions" /> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="moBuilderOptions" /> does
        /// has a null or empty <see cref="MediaObjectHtmlBuilderOptions.Browsers" /> property.</exception>
        public static MediaItem ToMediaItem(IGalleryObject mediaObject, int indexInAlbum, MediaObjectHtmlBuilderOptions moBuilderOptions)
        {
            if (mediaObject == null)
                throw new ArgumentNullException("mediaObject");

            if (moBuilderOptions == null)
                throw new ArgumentNullException("moBuilderOptions");

            if (moBuilderOptions.Browsers == null || moBuilderOptions.Browsers.Length == 0)
                throw new ArgumentOutOfRangeException("moBuilderOptions.Browsers", "The Browsers array property must have at least one element.");

            moBuilderOptions.GalleryObject = mediaObject;

            var isBeingProcessed = MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(mediaObject.Id, MediaQueueItemConversionType.CreateOptimized);

            var moEntity = new MediaItem
            {
                Id = mediaObject.Id,
                AlbumId = mediaObject.Parent.Id,
                AlbumTitle = mediaObject.Parent.Title,
                Index = indexInAlbum,
                Title = mediaObject.Title,
                Views = GetViews(moBuilderOptions).ToArray(),
                HighResAvailable = isBeingProcessed || (!String.IsNullOrEmpty(mediaObject.Optimized.FileName)) && (mediaObject.Original.FileName != mediaObject.Optimized.FileName),
                IsDownloadable = !(mediaObject is ExternalMediaObject),
                MimeType = (int)mediaObject.MimeType.TypeCategory,
                ItemType = (int)mediaObject.GalleryObjectType,
                MetaItems = ToMetaItems(mediaObject.MetadataItems.GetVisibleItems(), mediaObject)
            };

            return moEntity;
        }

        /// <summary>
        /// Deletes the specified <paramref name="galleryItems" /> from the data store, optionally also deleting the associated directories or
        /// media files. Validation is performed to ensure the logged in user has permission to delete the items and that no business rules
        /// are violated. The successfully deleted items are assigned to the <see cref="ActionResult.ActionTarget" /> property of the returned
        /// instance.
        /// </summary>
        /// <param name="galleryItems">The gallery items to delete.</param>
        /// <param name="deleteFromFileSystem">if set to <c>true</c> the files and directories associated with the gallery items
        /// are deleted from the hard disk. Set this to <c>false</c> to delete only the database records.</param>
        /// <returns>An instance of <see cref="ActionResult" /> describing the result of the deletion.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleryItems" /> is null.</exception>
        public static ActionResult DeleteGalleryItems(GalleryItem[] galleryItems, bool deleteFromFileSystem)
        {
            if (galleryItems == null)
                throw new ArgumentNullException(nameof(galleryItems));

            var errMsg = "<p>The following items could not be deleted:</p>";
            var validationPassed = false;
            var deletedItems = new List<GalleryItem>();

            try
            {
                validationPassed = ValidateItemsAreNotInReadOnlyGallery(galleryItems);
            }
            catch (GallerySecurityException ex)
            {
                errMsg = $"<p>{ex.Message}</p>";
            }

            if (validationPassed) // Don't bother trying to delete anything if we got an error above
            {
                foreach (var galleryItem in galleryItems)
                {
                    try
                    {
                        if (galleryItem.IsAlbum)
                        {
                            IAlbum album;
                            try
                            {
                                album = AlbumController.LoadAlbumInstance(galleryItem.Id);
                            }
                            catch (InvalidAlbumException)
                            {
                                continue; // Album may have been deleted by someone else, so just skip it.
                            }

                            AlbumController.DeleteAlbum(album, deleteFromFileSystem);
                        }
                        else
                        {
                            IGalleryObject mo;
                            try
                            {
                                mo = Factory.LoadMediaObjectInstance(galleryItem.Id);
                            }
                            catch (InvalidMediaObjectException)
                            {
                                continue; // Media object may have been deleted by someone else, so just skip it.
                            }

                            DeleteMediaObject(mo, deleteFromFileSystem);
                        }

                        deletedItems.Add(galleryItem);
                    }
                    catch (CannotDeleteAlbumException ex)
                    {
                        errMsg += $"<p>{galleryItem.Title} - {ex.Message}</p>";
                    }
                    catch (GallerySecurityException ex)
                    {
                        errMsg += $"<p>{galleryItem.Title} - {ex.Message}</p>";
                    }
                }
            }

            if (deletedItems.Count < galleryItems.Length)
            {
                return new ActionResult()
                {
                    // At least one error occurred: Status is 'Warning' if at least one item could be deleted; otherwise 'Error' if all failed
                    Status = (deletedItems.Count > 0 ? ActionResultStatus.Warning.ToString() : ActionResultStatus.Error.ToString()),
                    Title = Resources.GalleryServer.Task_Delete_Objects_Cannot_Delete_Asset_Msg_Hdr,
                    Message = errMsg,
                    ActionTarget = deletedItems.ToArray()
                };
            }

            return new ActionResult()
            {
                Status = ActionResultStatus.Success.ToString(),
                Title = Resources.GalleryServer.Task_Delete_Objects_ObjectsSuccessfullyDeleted_Hdr,
                Message = string.Empty,
                ActionTarget = deletedItems.ToArray()
            };
        }

        /// <summary>
        /// Permanently delete the original file for all <paramref name="galleryItems" />, including any children if a gallery item is an album.
        /// If no optimized version exists, no action is taken on that media asset. Validation is performed to ensure the logged in user has 
        /// permission to edit the items and that no business rules are violated. The successfully processed items are assigned to the 
        /// <see cref="ActionResult.ActionTarget" /> property of the returned instance.
        /// </summary>
        /// <param name="galleryItems">The gallery items for which the original files are to be deleted.</param>
        /// <returns>An instance of <see cref="ActionResult" /> describing the result of the deletion.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="galleryItems" /> is null.</exception>
        public static ActionResult DeleteOriginalFiles(GalleryItem[] galleryItems)
        {
            if (galleryItems == null)
                throw new ArgumentNullException(nameof(galleryItems));

            var errMsg = "<p>One or more items encountered an issue:</p>";
            var validationPassed = false;
            var processedItems = new List<GalleryItem>();

            try
            {
                validationPassed = ValidateItemsAreNotInReadOnlyGallery(galleryItems);
            }
            catch (GallerySecurityException ex)
            {
                errMsg = $"<p>{ex.Message}</p>";
            }

            if (validationPassed) // Don't bother trying to delete anything if we got an error above
            {
                foreach (var galleryItem in galleryItems)
                {
                    try
                    {
                        if (galleryItem.IsAlbum)
                        {
                            try
                            {
                                var album = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(galleryItem.Id) { IsWritable = true, InflateChildObjects = true });

                                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, ((IAlbum)album.Parent).IsVirtualAlbum);

                                DeleteOriginalFilesFromAlbum(album);
                            }
                            catch (InvalidAlbumException)
                            {
                                continue; // Album may have been deleted by someone else, so just skip it.
                            }
                        }
                        else
                        {
                            try
                            {
                                var mediaObject = Factory.LoadMediaObjectInstance(new MediaLoadOptions(galleryItem.Id) { IsWritable = true });

                                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, RoleController.GetGalleryServerRolesForUser(), mediaObject.Parent.Id, mediaObject.GalleryId, Utils.IsAuthenticated, mediaObject.IsPrivate, ((IAlbum)mediaObject.Parent).IsVirtualAlbum);

                                mediaObject.DeleteOriginalFile();

                                GalleryObjectController.SaveGalleryObject(mediaObject);
                            }
                            catch (InvalidMediaObjectException)
                            {
                                continue; // Media object may have been deleted by someone else, so just skip it.
                            }
                        }

                        var originalDisplayObject = galleryItem.Views.SingleOrDefault(v => v.ViewSize == (int)DisplayObjectType.Original);
                        if (originalDisplayObject != null)
                        {
                            originalDisplayObject.FileSizeKB = 0;
                        }

                        processedItems.Add(galleryItem);
                    }
                    catch (GallerySecurityException ex)
                    {
                        errMsg += $"<p>{galleryItem.Title} - {ex.Message}</p>";
                    }
                }
            }

            if (processedItems.Count < galleryItems.Length)
            {
                return new ActionResult()
                {
                    // At least one error occurred. Most likely we get here when user doesn't have permission.
                    Status = ActionResultStatus.Warning.ToString(),
                    Title = Resources.GalleryServer.Task_Delete_Objects_Cannot_Delete_Original_File_Msg_Hdr,
                    Message = errMsg,
                    ActionTarget = processedItems.ToArray()
                };
            }

            return new ActionResult()
            {
                Status = ActionResultStatus.Success.ToString(),
                Title = Resources.GalleryServer.Task_Delete_Objects_OriginalFilesSuccessfullyDeleted_Hdr,
                Message = string.Empty,
                ActionTarget = processedItems.ToArray()
            };
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Permanently delete this media object from the data store and optionally the hard drive. Validation is performed prior to deletion to ensure
        /// current user has delete permission and it can be safely deleted.
        /// </summary>
        /// <param name="mediaObject">The media object to delete. If null, the function returns without taking any action.</param>
        /// <param name="deleteFromFileSystem">if set to <c>true</c> the files associated with the media object
        /// are deleted from the hard disk. Set this to <c>false</c> to delete only the database records.</param>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to delete the media object or it is
        /// in a read-only gallery.</exception>
        private static void DeleteMediaObject(IGalleryObject mediaObject, bool deleteFromFileSystem = true)
        {
            if (mediaObject == null)
                return;

            ValidateBeforeMediaObjectDelete(mediaObject);

            if (deleteFromFileSystem)
            {
                mediaObject.Delete();
            }
            else
            {
                mediaObject.DeleteFromGallery();
            }
        }

        /// <summary>
        /// Throws a <see cref="GallerySecurityException" /> if any of the <paramref name="galleryItems" /> are in a read-only gallery. 
        /// Note that we do not verify any permissions for the current user (e.g. edit, delete, etc); it is expected that downstream code does that.
        /// </summary>
        /// <param name="galleryItems">The gallery items to validate.</param>
        /// <returns><c>true</c> if all <paramref name="galleryItems" /> are in a writable gallery, <c>false</c> otherwise.</returns>
        /// <exception cref="GallerySecurityException">Thrown if any of the <paramref name="galleryItems" /> are in a read-only gallery.</exception>
        private static bool ValidateItemsAreNotInReadOnlyGallery(IEnumerable<GalleryItem> galleryItems)
        {
            var galleryIds = new HashSet<int>();

            foreach (var galleryItem in galleryItems)
            {
                if (galleryItem.IsAlbum)
                {
                    try
                    {
                        var albumToDelete = AlbumController.LoadAlbumInstance(galleryItem.Id);
                        galleryIds.Add(albumToDelete.GalleryId);
                    }
                    catch (InvalidAlbumException)
                    {
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        var mediaObjectToDelete = Factory.LoadMediaObjectInstance(galleryItem.Id);

                        galleryIds.Add(mediaObjectToDelete.GalleryId);
                    }
                    catch (InvalidMediaObjectException)
                    {
                        continue;
                    }
                }
            }

            if (galleryIds.Any(galleryId => Factory.LoadGallerySetting(galleryId).MediaObjectPathIsReadOnly))
            {
                throw new GallerySecurityException(Resources.GalleryServer.Task_Modify_Asset_Cannot_Modify_MediaPathIsReadOnly);
            }

            return true;
        }

        /// <summary>
        /// Verifies that the media object meets the prerequisites to be safely deleted but does not actually delete it. Throws a
        /// <see cref="GallerySecurityException" /> when the current user does not have permission to delete the media object or the media object is
        /// in a read-only gallery.
        /// </summary>
        /// <param name="mediaObjectToDelete">The media object to delete.</param>
        /// <remarks>This function is automatically called when using the <see cref="DeleteMediaObject(IGalleryObject, bool)"/> method, so it is not necessary to 
        /// invoke when using that method. Typically you will call this method when there are several items to delete and you want to 
        /// check all of them before deleting any of them.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObjectToDelete" /> is null.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to delete the media object or it is
        /// in a read-only gallery.</exception>
        private static void ValidateBeforeMediaObjectDelete(IGalleryObject mediaObjectToDelete)
        {
            if (mediaObjectToDelete == null)
                throw new ArgumentNullException(nameof(mediaObjectToDelete));

            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.DeleteMediaObject, RoleController.GetGalleryServerRolesForUser(), mediaObjectToDelete.Parent.Id, mediaObjectToDelete.GalleryId, Utils.IsAuthenticated, mediaObjectToDelete.IsPrivate, ((IAlbum)mediaObjectToDelete.Parent).IsVirtualAlbum);

            if (Factory.LoadGallerySetting(mediaObjectToDelete.GalleryId).MediaObjectPathIsReadOnly)
            {
                throw new GallerySecurityException(Resources.GalleryServer.Task_Modify_Asset_Cannot_Modify_MediaPathIsReadOnly);
            }
        }

        /// <summary>
        /// Creates the media object from the file specified in <paramref name="options" />.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>List{ActionResult}.</returns>
        /// <exception cref="Events.CustomExceptions.GallerySecurityException">Thrown when user is not authorized to add a media object to the album.</exception>
        /// <remarks>This function can be invoked from a thread that does not have access to the current HTTP context (for example, when
        /// uploading ZIP files). Therefore, be sure nothing in this body (or the functions it calls) uses HttpContext.Current, or at 
        /// least check it for null first.</remarks>
        private static List<ActionResult> CreateMediaObjectFromFile(AddMediaObjectSettings options)
        {
            string sourceFilePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.TempUploadDirectory, options.FileNameOnServer);

            try
            {
                IAlbum album = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(options.AlbumId) { IsWritable = true, InflateChildObjects = true });

                if (HttpContext.Current != null)
                    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.AddMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);
                else
                {
                    // We are extracting files from a zip archive (we know this because this is the only scenario that happens on a background
                    // thread where HttpContext.Current is null). Tweak the security check slightly to ensure the HTTP context isn't used.
                    // The changes are still secure because options.CurrentUserName is assigned in the server's API method.
                    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.AddMediaObject, RoleController.GetGalleryServerRolesForUser(options.CurrentUserName), album.Id, album.GalleryId, !String.IsNullOrWhiteSpace(options.CurrentUserName), album.IsPrivate, album.IsVirtualAlbum);
                }

                var extension = Path.GetExtension(options.FileName);
                if (extension != null && ((extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)) && (options.ExtractZipFile)))
                {
                    List<ActionResult> result;

                    // Extract the files from the zipped file.
                    using (var zip = new ZipUtility(options.CurrentUserName, RoleController.GetGalleryServerRolesForUser(options.CurrentUserName)))
                    {
                        using (var fs = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                        {
                            result = zip.ExtractZipFile(fs, album, options.DiscardOriginalFile);
                        }
                    }

                    album.SortAsync(true, options.CurrentUserName, true);

                    return result;
                }
                else
                {
                    string albumPhysicalPath = album.FullPhysicalPathOnDisk;
                    string filename = HelperFunctions.ValidateFileName(albumPhysicalPath, options.FileName);
                    string filepath = Path.Combine(albumPhysicalPath, filename);

                    HelperFunctions.MoveFileSafely(sourceFilePath, filepath);

                    ActionResult result = CreateMediaObject(filepath, album, options);

                    album.Sort(true, options.CurrentUserName);

                    return new List<ActionResult> { result };
                }
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);
                return new List<ActionResult>
                 {
                  new ActionResult
                    {
                      Title = options.FileName,
                      Status = ActionResultStatus.Error.ToString(),
                      Message = "The event log may have additional details."
                    }
                };
            }
            finally
            {
                try
                {
                    // If the file still exists in the temp directory, delete it. Typically this happens when we've
                    // extracted the contents of a zip file (since other files will have already been moved to the dest album.)
                    if (File.Exists(sourceFilePath))
                    {
                        File.Delete(sourceFilePath);
                    }
                }
                catch (IOException) { } // Ignore an error; not a big deal if it continues to exist in the temp directory
                catch (UnauthorizedAccessException) { } // Ignore an error; not a big deal if it continues to exist in the temp directory
            }
        }

        private static ActionResult CreateMediaObject(string filePath, IAlbum album, AddMediaObjectSettings options)
        {
            var result = new ActionResult
            {
                Title = Path.GetFileName(filePath)
            };

            try
            {
                IGalleryObject go = Factory.CreateMediaObjectInstance(filePath, album);
                SaveGalleryObject(go, options.CurrentUserName);

                if (options.DiscardOriginalFile)
                {
                    go.DeleteOriginalFile();
                    SaveGalleryObject(go);
                }

                result.Status = ActionResultStatus.Success.ToString();
            }
            catch (UnsupportedMediaObjectTypeException ex)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (UnauthorizedAccessException) { } // Ignore an error; the file will continue to exist in the destination album directory

                result.Status = ActionResultStatus.Error.ToString();
                result.Message = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the <paramref name="galleryObject" /> has an optimized media object.
        /// </summary>
        /// <param name="galleryObject">The gallery object.</param>
        /// <returns>
        ///   <c>true</c> if it has an optimized media object; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private static bool HasOptimizedVersion(IGalleryObject galleryObject)
        {
            if (galleryObject == null)
                throw new ArgumentNullException("galleryObject");

            if (galleryObject.GalleryObjectType == GalleryObjectType.Album)
                return false;

            bool inQueue = MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(galleryObject.Id, MediaQueueItemConversionType.CreateOptimized);
            bool hasOptFile = !String.IsNullOrEmpty(galleryObject.Optimized.FileName);
            bool optFileDifferentThanOriginal = (galleryObject.Optimized.FileName != galleryObject.Original.FileName);

            return (inQueue || (hasOptFile && optFileDifferentThanOriginal));
        }

        /// <summary>
        /// Determines whether the <paramref name="galleryObject" /> has an original media object.
        /// Generally, all media objects do have one and all albums do not.
        /// </summary>
        /// <param name="galleryObject">The gallery object.</param>
        /// <returns>
        ///   <c>true</c> if it has an original media object; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private static bool HasOriginalVersion(IGalleryObject galleryObject)
        {
            if (galleryObject == null)
                throw new ArgumentNullException("galleryObject");

            return !(galleryObject.Original is NullDisplayObject);
        }

        /// <summary>
        /// Gets a collection of views corresponding to the gallery object and other specs in <paramref name="moBuilderOptions" />.
        /// </summary>
        /// <param name="moBuilderOptions">A set of properties to be used when building the output.</param>
        /// <returns>Returns a collection of <see cref="Entity.DisplayObject" /> instances.</returns>
        private static List<Entity.DisplayObject> GetViews(MediaObjectHtmlBuilderOptions moBuilderOptions)
        {
            var views = new List<Entity.DisplayObject>(3);

            moBuilderOptions.DisplayType = DisplayObjectType.Thumbnail;

            var moBuilder = new MediaObjectHtmlBuilder(moBuilderOptions);

            views.Add(new Entity.DisplayObject
            {
                ViewSize = (int)DisplayObjectType.Thumbnail,
                ViewType = (int)moBuilder.MimeType.TypeCategory,
                HtmlOutput = moBuilder.GenerateHtml(),
                ScriptOutput = moBuilder.GenerateScript(),
                Width = moBuilder.Width,
                Height = moBuilder.Height,
                Url = moBuilder.GetMediaObjectUrl(),
                FileSizeKB = moBuilder.DisplayObject.FileSizeKB
            });

            if (HasOptimizedVersion(moBuilderOptions.GalleryObject))
            {
                moBuilderOptions.DisplayType = DisplayObjectType.Optimized;

                moBuilder = new MediaObjectHtmlBuilder(moBuilderOptions);

                views.Add(new Entity.DisplayObject
                {
                    ViewSize = (int)DisplayObjectType.Optimized,
                    ViewType = (int)moBuilder.MimeType.TypeCategory,
                    HtmlOutput = moBuilder.GenerateHtml(),
                    ScriptOutput = moBuilder.GenerateScript(),
                    Width = moBuilder.Width,
                    Height = moBuilder.Height,
                    Url = moBuilder.GetMediaObjectUrl(),
                    FileSizeKB = moBuilder.DisplayObject.FileSizeKB
                });
            }

            if (HasOriginalVersion(moBuilderOptions.GalleryObject))
            {
                moBuilderOptions.DisplayType = moBuilderOptions.GalleryObject.Original.DisplayType; // May be Original or External

                moBuilder = new MediaObjectHtmlBuilder(moBuilderOptions);

                views.Add(new Entity.DisplayObject
                {
                    ViewSize = (int)DisplayObjectType.Original,
                    ViewType = (int)moBuilder.MimeType.TypeCategory,
                    HtmlOutput = moBuilder.GenerateHtml(),
                    ScriptOutput = moBuilder.GenerateScript(),
                    Width = moBuilder.Width,
                    Height = moBuilder.Height,
                    Url = moBuilder.GetMediaObjectUrl(),
                    FileSizeKB = moBuilder.DisplayObject.FileSizeKB
                });
            }

            return views;
        }

        /// <summary>
        /// Persists the current user's sort preference for the specified <paramref name="album" />. No action is taken if the 
        /// album is virtual. Anonymous user data is stored in session only; logged on users' data are permanently stored.
        /// </summary>
        /// <param name="album">The album whose sort preference is to be preserved.</param>
        /// <param name="sortByMetaName">Name of the metadata item to sort by.</param>
        /// <param name="sortAscending">Indicates the sort direction.</param>
        private static void PersistUserSortPreference(IAlbum album, MetadataItemName sortByMetaName, bool sortAscending)
        {
            if (album.IsVirtualAlbum)
                return;

            var profile = ProfileController.GetProfile();

            var aProfile = profile.AlbumProfiles.Find(album.Id);

            if (aProfile == null)
            {
                profile.AlbumProfiles.Add(new AlbumProfile(album.Id, sortByMetaName, sortAscending));
            }
            else
            {
                aProfile.SortByMetaName = sortByMetaName;
                aProfile.SortAscending = sortAscending;
            }

            ProfileController.SaveProfile(profile);
        }

        /// <summary>
        /// Gets the title for the album that is appropriate for the specified <paramref name="rating" />.
        /// </summary>
        /// <param name="rating">The rating. Valid values include "highest", "lowest", "none", or a decimal.</param>
        /// <returns>System.String.</returns>
        private static string GetRatedAlbumTitle(string rating)
        {
            switch (rating.ToLowerInvariant())
            {
                case "highest":
                    return Resources.GalleryServer.Site_Highest_Rated_Title; // "Highest rated items"
                case "lowest":
                    return Resources.GalleryServer.Site_Lowest_Rated_Title; // "Lowest rated items"
                case "none":
                    return Resources.GalleryServer.Site_None_Rated_Title; // "Items without a rating"
                default:
                    return String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Site_Rated_Title, rating); // "Items with a rating of 3"
            }
        }

        /// <summary>
        /// When the current user has previously rated an item, replace the average user rating with user's
        /// own rating.
        /// </summary>
        /// <param name="metaItem">The meta item. It must be a <see cref="MetadataItemName.Rating" /> item.</param>
        /// <param name="moProfiles"></param>
        private static void ReplaceAvgRatingWithUserRating(MetaItem metaItem, IMediaObjectProfileCollection moProfiles)
        {
            var moProfile = moProfiles.Find(metaItem.MediaId);

            if (moProfile != null)
            {
                metaItem.Desc = Resources.GalleryServer.UC_Metadata_UserRated_Rating_Lbl;
                metaItem.Value = moProfile.Rating;
            }
        }

        /// <summary>
        /// Permanently delete the original file for all media assets in the <paramref name="album" />. Requires that an optimized version exists.
        /// If no optimized version exists, no action is taken.
        /// </summary>
        /// <param name="album">The album.</param>
        private static void DeleteOriginalFilesFromAlbum(IAlbum album)
        {
            // Delete the original file for each item in the album. Then recursively do the same thing to all child albums.
            foreach (var mediaObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject))
            {
                mediaObject.DeleteOriginalFile();

                GalleryObjectController.SaveGalleryObject(mediaObject);
            }

            foreach (IAlbum childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
            {
                DeleteOriginalFilesFromAlbum(childAlbum);
            }
        }

        /// <summary>
        /// Generate the amount to rotate/flip the <paramref name="mediaAsset" /> based on the user's requested <paramref name="rotateFlip" /> 
        /// and the <paramref name="viewSize" /> representing the image size the user is looking at when making this request.
        /// </summary>
        /// <param name="mediaAsset">The media asset to rotate/flip.</param>
        /// <param name="rotateFlip">The user-requested amount of rotation/flipping.</param>
        /// <param name="viewSize">Size of image user is looking at.</param>
        /// <returns>An instance of <see cref="MediaAssetRotateFlip" />.</returns>
        private static MediaAssetRotateFlip GetRotateFlip(IGalleryObject mediaAsset, MediaAssetRotateFlip rotateFlip, DisplayObjectType viewSize)
        {
            // If user is viewing the original, we need to tweak the rotateFlip option as if she were viewing the auto-rotated option.
            if (viewSize != DisplayObjectType.Original)
            {
                return rotateFlip;
            }

            // In Gallery Server 3, users couldn't view the original image, so all rotate requests were done on the thumbnail or optimized image, which
            // were auto-rotated to the correct orientation for images having orientation metadata, but the originals were unmodified. Code in 
            // DisplayObjectCreator maps the user's request to the actual amount to rotate based on the original image's orientation. We didn't want to
            // modify that code when adding this in v4, so we need to detect when the user is viewing the original and adjust the rotate/flip value so
            // that later when it is adjusted in DisplayObjectCreator, it is the correct value.
            // IN SHORT: When the user is viewing the original image, this function balances the adjustment made later in DisplayObjectCreator.

            var fileRotation = mediaAsset.GetOrientation(); // Actual rotation of the original file, as discovered via orientation metadata
            var userRotation = rotateFlip; // Desired rotation by the user

            if (userRotation == MediaAssetRotateFlip.NotSpecified)
            {
                userRotation = MediaAssetRotateFlip.Rotate0FlipNone;
            }

            switch (fileRotation)
            {
                case Orientation.None:
                case Orientation.Normal:
                    return userRotation;

                case Orientation.Rotated90:
                    switch (userRotation)
                    {
                        case MediaAssetRotateFlip.Rotate0FlipNone: return MediaAssetRotateFlip.Rotate90FlipNone;
                        case MediaAssetRotateFlip.Rotate0FlipX: return MediaAssetRotateFlip.Rotate90FlipX;
                        case MediaAssetRotateFlip.Rotate0FlipY: return MediaAssetRotateFlip.Rotate90FlipY;
                        case MediaAssetRotateFlip.Rotate90FlipNone: return MediaAssetRotateFlip.Rotate180FlipNone;
                        case MediaAssetRotateFlip.Rotate180FlipNone: return MediaAssetRotateFlip.Rotate270FlipNone;
                        case MediaAssetRotateFlip.Rotate270FlipNone: return MediaAssetRotateFlip.Rotate0FlipNone;
                    }
                    break;

                case Orientation.Rotated180:
                    switch (userRotation)
                    {
                        case MediaAssetRotateFlip.Rotate0FlipNone: return MediaAssetRotateFlip.Rotate180FlipNone;
                        case MediaAssetRotateFlip.Rotate0FlipX: return MediaAssetRotateFlip.Rotate180FlipX;
                        case MediaAssetRotateFlip.Rotate0FlipY: return MediaAssetRotateFlip.Rotate180FlipY;
                        case MediaAssetRotateFlip.Rotate90FlipNone: return MediaAssetRotateFlip.Rotate270FlipNone;
                        case MediaAssetRotateFlip.Rotate180FlipNone: return MediaAssetRotateFlip.Rotate0FlipNone;
                        case MediaAssetRotateFlip.Rotate270FlipNone: return MediaAssetRotateFlip.Rotate90FlipNone;
                    }
                    break;

                case Orientation.Rotated270:
                    switch (userRotation)
                    {
                        case MediaAssetRotateFlip.Rotate0FlipNone: return MediaAssetRotateFlip.Rotate270FlipNone;
                        case MediaAssetRotateFlip.Rotate0FlipX: return MediaAssetRotateFlip.Rotate270FlipX;
                        case MediaAssetRotateFlip.Rotate0FlipY: return MediaAssetRotateFlip.Rotate270FlipY;
                        case MediaAssetRotateFlip.Rotate90FlipNone: return MediaAssetRotateFlip.Rotate0FlipNone;
                        case MediaAssetRotateFlip.Rotate180FlipNone: return MediaAssetRotateFlip.Rotate90FlipNone;
                        case MediaAssetRotateFlip.Rotate270FlipNone: return MediaAssetRotateFlip.Rotate180FlipNone;
                    }
                    break;
            }

            return MediaAssetRotateFlip.NotSpecified;
        }

        /// <summary>
        /// Copy most metadata from <paramref name="mediaAsset" /> into the image at <paramref name="editedImageFilePath" /> and persist to
        /// a new file stored in the same directory as <paramref name="mediaAsset" />. The only metadata that are copied are the ones present
        /// as <see cref="System.Drawing.Imaging.PropertyItem" /> instances in the file. No changes are persisted to the data store.
        /// </summary>
        /// <param name="editedImageFilePath">The full path to an image file.</param>
        /// <param name="mediaAsset">The media asset to be merged with <paramref name="editedImageFilePath" />.</param>
        /// <returns>An instance of <see cref="FileInfo" />.</returns>
        private static FileInfo MergeImages(string editedImageFilePath, IGalleryObject mediaAsset)
        {
            string tmpImagePath;

            using (var editedImage = System.Drawing.Image.FromFile(editedImageFilePath))
            {
                // Copy most property items from the original
                foreach (var propertyItem in ImageMetadataReadWriter.GetImagePropertyItems(mediaAsset.Original.FileNamePhysicalPath))
                {
                    // Don't copy width, height or orientation meta items.
                    var metasToNotCopy = new[]
                    {
            RawMetadataItemName.ImageWidth,
            RawMetadataItemName.ImageHeight,
            RawMetadataItemName.ExifPixXDim,
            RawMetadataItemName.ExifPixYDim,
            RawMetadataItemName.Orientation
          };

                    if (Array.IndexOf(metasToNotCopy, (RawMetadataItemName)propertyItem.Id) >= 0)
                        continue;

                    editedImage.SetPropertyItem(propertyItem);
                }

                // Save file to the destination album, but don't overwrite the original file (we'll do that later once we're sure everything succeeds).
                var dirName = Path.GetDirectoryName(mediaAsset.Original.FileNamePhysicalPath) ?? string.Empty;
                var tmpImageFileName = HelperFunctions.ValidateFileName(dirName, mediaAsset.Original.FileName);
                tmpImagePath = Path.Combine(dirName, tmpImageFileName);

                ImageHelper.SaveImageToDisk(editedImage, tmpImagePath, System.Drawing.Imaging.ImageFormat.Jpeg, Factory.LoadGallerySetting(mediaAsset.GalleryId).OriginalImageJpegQuality);
            }

            return new FileInfo(tmpImagePath);
        }

        /// <summary>
        /// Updates key meta properties and recreates the thumbnail/optimized images for the <paramref name="mediaAsset" />. Designed to be invoked
        /// after a media asset's original file has been edited on the client.
        /// </summary>
        /// <param name="mediaAsset">A writable media asset whose original file has been updated.</param>
        private static void UpdateMetadataAndDerivedImagesForEditedMediaAsset(IGalleryObject mediaAsset)
        {
            // Remove orientation property. We have no idea if the user rotated the image, but if they did and we don't change the orientation,
            // the derived images are going to be wrong. So let's assume they did.
            IGalleryObjectMetadataItem orientationMetaItem;
            if (mediaAsset.MetadataItems.TryGetMetadataItem(MetadataItemName.Orientation, out orientationMetaItem))
            {
                orientationMetaItem.IsDeleted = true;
            }

            UpdateMetadataAndDerivedImagesForReplacedMediaAsset(mediaAsset, true);
        }

        /// <summary>
        /// Updates key meta properties and recreates the thumbnail/optimized images for the <paramref name="mediaAsset" />. Designed to be invoked
        /// after a media asset's original file has been updated with a new file.
        /// </summary>
        /// <param name="mediaAsset">A writable media asset whose original file has been updated.</param>
        /// <param name="forceMetaWriteToFile">Indicates whether to write all meta properties in <paramref name="mediaAsset" /> to the original file.
        /// When <c>true</c>, the file's metadata is updated even if the 'write metadata to file' setting is turned off on the Metadata page in the
        /// site admin area. When <c>false</c>, the metadata is written to the file only when the setting is enabled.</param>
        private static void UpdateMetadataAndDerivedImagesForReplacedMediaAsset(IGalleryObject mediaAsset, bool forceMetaWriteToFile)
        {
            // Recalculate width and height and update a few properties.
            var editedSize = mediaAsset.Original.GetSize();
            mediaAsset.Original.Width = (int)editedSize.Width;
            mediaAsset.Original.Height = (int)editedSize.Height;

            // Update meta width, height & dimensions. These properties won't exist in the file but the algorithm will grab them from the Original.Width/Height properties we just updated.
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.Width));
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.Height));
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.Dimensions));
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.Orientation));

            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.Duration));
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.BitRate));
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.AudioFormat));
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.VideoFormat));

            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.FileName));
            mediaAsset.ExtractMetadata(mediaAsset.MetaDefinitions.Find(MetadataItemName.FileSizeKb));

            // Write all possible in-memory meta properties to the file.
            foreach (var metaItem in mediaAsset.MetadataItems)
            {
                metaItem.PersistToFile = forceMetaWriteToFile ? mediaAsset.MetaDefinitions.Find(metaItem.MetadataItemName).IsPersistable : mediaAsset.MetaDefinitions.Find(metaItem.MetadataItemName).PersistToFile.GetValueOrDefault();
                metaItem.HasChanges = true;
            }

            // Force regeneration of thumbnail and optimized images.
            if (File.Exists(mediaAsset.Optimized.FileNamePhysicalPath))
            {
                File.Delete(mediaAsset.Optimized.FileNamePhysicalPath);
            }

            if (File.Exists(mediaAsset.Thumbnail.FileNamePhysicalPath))
            {
                File.Delete(mediaAsset.Thumbnail.FileNamePhysicalPath);
            }

            mediaAsset.Optimized.FileName = string.Empty;
            mediaAsset.Optimized.FileNamePhysicalPath = string.Empty;
            mediaAsset.Thumbnail.FileName = string.Empty;
            mediaAsset.Thumbnail.FileNamePhysicalPath = string.Empty;
            mediaAsset.RegenerateThumbnailOnSave = true;
            mediaAsset.RegenerateOptimizedOnSave = true;

            SaveGalleryObject(mediaAsset);
        }

        /// <summary>
        /// Gets a value indicating whether the <paramref name="mediaAsset" /> requires a watermark.
        /// </summary>
        /// <param name="mediaAsset">The media asset.</param>
        /// <returns><c>true</c> if the media asset requires a watermark, <c>false</c> otherwise.</returns>
        /// <remarks>This function is similar to <see cref="Handler.getmedia.ShouldApplyWatermark" />, so if you edit this
        /// function check the other one, too.</remarks>
        private static bool ShouldApplyWatermark(IGalleryObject mediaAsset)
        {
            // Apply watermark to optimized and original images only when applyWatermark = true.
            if (mediaAsset.MimeType.TypeCategory == MimeTypeCategory.Image)
            {
                var gs = Factory.LoadGallerySetting(mediaAsset.GalleryId);
                bool requiresWatermark = false;
                bool applyWatermark = gs.ApplyWatermark;

                if (AppSetting.Instance.License.LicenseType == LicenseLevel.TrialExpired)
                {
                    requiresWatermark = true;
                }
                else if (applyWatermark)
                {
                    // If the user belongs to a role with watermarks set to visible, then show it; otherwise don't show the watermark.
                    if (!Utils.IsUserAuthorized(SecurityActions.HideWatermark, RoleController.GetGalleryServerRolesForUser(), mediaAsset.Parent.Id, mediaAsset.GalleryId, mediaAsset.IsPrivate, ((IAlbum)mediaAsset.Parent).IsVirtualAlbum))
                    {
                        // Show the image without the watermark.
                        requiresWatermark = true;
                    }
                }

                return requiresWatermark;
            }
            else
            {
                return false; // Watermarks are never applied to non-image media objects.
            }
        }

        #endregion
    }
}
