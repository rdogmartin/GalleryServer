using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Controller
{
    /// <summary>
    /// Contains functionality for interacting with galleries and gallery settings.
    /// </summary>
    public static class GalleryController
    {
        #region Fields

        private static readonly object _sharedLock = new object();
        private static bool _isInitialized;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the Gallery Server code has been initializaed.
        /// The code is initialized by calling <see cref="InitializeGspApplication" />.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the code is initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInitialized
        {
            get { return _isInitialized; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the Gallery Server application. This method is designed to be run at application startup. The business layer
        /// is initialized with the current trust level and a few configuration settings. The business layer also initializes
        /// the data store, including verifying a minimal level of data integrity, such as at least one record for the root album.
        /// Initialization that requires an HttpContext is also performed. When this method completes, <see cref="IAppSetting.IsInitialized" />
        /// will be <c>true</c>, but <see cref="GalleryController.IsInitialized" /> will be <c>true</c> only when an HttpContext instance
        /// exists. If this function is initially called from a place where an HttpContext doesn't exist, it will automatically be called 
        /// again later, eventually being called from a place where an HttpContext does exist, thus completing app initialization.
        /// </summary>
        public static void InitializeGspApplication()
        {
            try
            {
                InitializeApplication();

                lock (_sharedLock)
                {
                    if (IsInitialized)
                        return;

                    if (HttpContext.Current != null)
                    {
                        // Add a dummy value to session so that the session ID remains constant. (This is required by RoleController.GetRolesForUser())
                        // Check for null session first. It will be null when this is triggered by a web method that does not have
                        // session enabled (that is, the [WebMethod(EnableSession = true)] attribute). That's OK because the roles functionality
                        // will still work (we might have to an extra data call, though), and we don't want the overhead of session for some web methods.
                        if (HttpContext.Current.Session != null)
                            HttpContext.Current.Session.Add("1", "1");

                        // Update the user accounts in a few gallery settings. The DotNetNuke version requires this call to happen when there
                        // is an HttpContext, so to reduce differences between the two branches we put it here.
                        AddMembershipDataToGallerySettings();

                        _isInitialized = true;
                    }

                    AppEventController.LogEvent("Application has started.");

                    //InsertSampleUsersAndRoles();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (CannotWriteToDirectoryException ex)
            {
                // Let the error handler log it and try to redirect to a dedicated page for this error. The transfer will fail when the error occurs
                // during the app's init event, so when this happens don't re-throw (like we do in the generic catch below). This will allow the
                // initialize routine to run again from the GalleryPage constructor, and when the error happens again, this time the handler will be able to redirect.
                AppEventController.HandleGalleryException(ex);
                //throw; // Don't re-throw
            }
            catch (Exception ex)
            {
                // Let the error handler deal with it. It will decide whether to transfer the user to a friendly error page.
                // If the function returns, that means it didn't redirect, so we should re-throw the exception.
                AppEventController.HandleGalleryException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get a list of galleries the current user can administer. Site administrators can view all galleries, while gallery
        /// administrators may have access to zero or more galleries.
        /// </summary>
        /// <returns>Returns an <see cref="IGalleryCollection" /> containing the galleries the current user can administer.</returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static IGalleryCollection GetGalleriesCurrentUserCanAdminister()
        {
            return UserController.GetGalleriesCurrentUserCanAdminister();
        }

        /// <summary>
        /// Gets the ID of the template gallery.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public static int GetTemplateGalleryId()
        {
            return Factory.GetTemplateGalleryId();
        }

        /// <summary>
        /// Persist the <paramref name="gallery" /> to the data store.
        /// </summary>
        /// <param name="gallery">The gallery to persist to the data store.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
        [DataObjectMethod(DataObjectMethodType.Insert)]
        public static void AddGallery(Business.Gallery gallery)
        {
            if (gallery == null)
                throw new ArgumentNullException("gallery");

            gallery.Save();
        }

        /// <summary>
        /// Permanently delete the specified <paramref name="gallery" /> from the data store, including all related records. This action cannot
        /// be undone.
        /// </summary>
        /// <param name="gallery">The gallery to delete.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
        [DataObjectMethod(DataObjectMethodType.Delete)]
        public static void DeleteGallery(Business.Gallery gallery)
        {
            if (gallery == null)
                throw new ArgumentNullException("gallery");

            gallery.Delete();

            ProfileController.DeleteProfileForGallery(gallery);
        }

        /// <summary>
        /// Persist the <paramref name="gallery" /> to the data store.
        /// </summary>
        /// <param name="gallery">The gallery to persist to the data store.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
        [DataObjectMethod(DataObjectMethodType.Update)]
        public static void UpdateGallery(Business.Gallery gallery)
        {
            if (gallery == null)
                throw new ArgumentNullException("gallery");

            gallery.Save();
        }

        /// <summary>
        /// Execute install-related activities such as creating sample objects or an administrator account.
        /// </summary>
        /// <param name="galleryId">The ID for the gallery where the sample objects are to be created.</param>
        public static void ProcessInstallRequest(int galleryId)
        {
            CreateSampleObjects(galleryId);

            var user = CreateAdministrator(galleryId);

            if (user != null)
            {
                // User was successfully created or updated. Delete the install file and cancel the install request so that 
                // sample objects don't get created and the create user page converts to normal operation.
                UpdateRootAlbumTitleAfterAdminCreation(galleryId);
                DeleteInstallFile();
                AppSetting.Instance.InstallationRequested = false;
            }
        }

        /// <summary>
        /// Perform a synchronization according to the specified <paramref name="syncOptions" />. Any exceptions that occur during the
        /// sync are caught and logged to the event log. For auto-run syncs, the property <see cref="IGallerySettings.LastAutoSync" /> 
        /// is set to the current date/time and persisted to the data store.
        /// NOTE: This method does not perform any security checks; the calling code must ensure the requesting user is authorized to run the sync.
        /// </summary>
        /// <param name="syncOptions">An object specifying the parameters for the synchronization operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="syncOptions" /> is null.</exception>
        public static void BeginSync(SyncOptions syncOptions)
        {
            if (syncOptions == null)
                throw new ArgumentNullException("syncOptions");

            IAlbum album = null;

            try
            {
                album = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(syncOptions.AlbumIdToSynchronize) { IsWritable = true, InflateChildObjects = true });

                AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "{0} synchronization of album '{1}' (ID {2}) has started.", syncOptions.UserName, album.Title, album.Id), album.GalleryId);

                var synchMgr = new SynchronizationManager(album.GalleryId);

                synchMgr.IsRecursive = syncOptions.IsRecursive;
                synchMgr.RebuildThumbnail = syncOptions.RebuildThumbnails;
                synchMgr.RebuildOptimized = syncOptions.RebuildOptimized;

                synchMgr.Synchronize(syncOptions.SyncId, album, syncOptions.UserName);

                if (syncOptions.SyncInitiator == SyncInitiator.AutoSync)
                {
                    // Update the date/time of this auto-sync and save to data store.
                    IGallerySettings gallerySettings = Factory.LoadGallerySetting(album.GalleryId, true);
                    gallerySettings.LastAutoSync = DateTime.Now;
                    gallerySettings.Save(false);

                    // The above Save() only updated the database; now we need to update the in-memory copy of the settings.
                    // We have to do this instead of simply calling gallerySettings.Save(true) because that overload causes the
                    // gallery settings to be cleared and reloaded, and the reloading portion done by the AddMembershipDataToGallerySettings
                    // function fails in DotNetNuke because there isn't a HttpContext.Current instance at this moment (because this code is
                    // run on a separate thread).
                    IGallerySettings gallerySettingsReadOnly = Factory.LoadGallerySetting(album.GalleryId, false);
                    gallerySettingsReadOnly.LastAutoSync = gallerySettings.LastAutoSync;
                }

                AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "{0} synchronization of album '{1}' (ID {2}) has finished.", syncOptions.UserName, album.Title, album.Id), album.GalleryId);
            }
            catch (SynchronizationInProgressException)
            {
                var message = String.Format(CultureInfo.InvariantCulture, "{0} synchronization of album '{1}' (ID {2}) could not be started because another one is in progress.", syncOptions.UserName, album != null ? album.Title : "N/A", album != null ? album.Id.ToString(CultureInfo.InvariantCulture) : "N/A");
                AppEventController.LogEvent(message, album != null ? album.GalleryId : (int?)null);
            }
            catch (Exception ex)
            {
                if (album != null)
                {
                    AppEventController.LogError(ex, album.GalleryId);
                }
                else
                {
                    AppEventController.LogError(ex);
                }

                var msg = String.Format(CultureInfo.InvariantCulture, "{0} synchronization of album '{1}' (ID {2}) has encountered an error and could not be completed.", syncOptions.UserName, album != null ? album.Title : "N/A", album != null ? album.Id.ToString(CultureInfo.InvariantCulture) : "N/A");
                AppEventController.LogEvent(msg, album != null ? album.GalleryId : (int?)null);
            }
        }

        /// <summary>
        /// Retrieves the status of a synchronization for the gallery having the ID <paramref name="galleryId" /> and the
        /// synchronization ID <paramref name="syncId" />.
        /// </summary>
        /// <param name="syncId">The synchronization ID.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>An instance of <see cref="SynchStatusWebEntity" />.</returns>
        public static SynchStatusWebEntity GetSyncStatus(string syncId, int galleryId)
        {
            var synchStatus = SynchronizationStatus.GetInstance(galleryId);
            var synchStatusWeb = new SynchStatusWebEntity();

            try
            {
                if (!String.IsNullOrEmpty(synchStatus.SynchId) && !synchStatus.SynchId.Equals(syncId, StringComparison.OrdinalIgnoreCase))
                {
                    synchStatusWeb.SynchId = syncId;
                    synchStatusWeb.TotalFileCount = 0;
                    synchStatusWeb.CurrentFileIndex = 0;
                    synchStatusWeb.CurrentFile = String.Empty;
                    synchStatusWeb.Status = SynchronizationState.AnotherSynchronizationInProgress.ToString();
                    synchStatusWeb.StatusForUI = Resources.GalleryServer.Task_Synch_Progress_Status_SynchInProgressException_Hdr;
                    synchStatusWeb.PercentComplete = CalculatePercentComplete(synchStatus);
                    synchStatusWeb.SyncRate = String.Empty;

                    return synchStatusWeb;
                }

                synchStatusWeb.SynchId = synchStatus.SynchId;
                synchStatusWeb.Status = synchStatus.Status.ToString();
                synchStatusWeb.TotalFileCount = synchStatus.TotalFileCount;
                synchStatusWeb.CurrentFileIndex = (synchStatus.Status == SynchronizationState.Complete ? synchStatus.TotalFileCount : synchStatus.CurrentFileIndex);
                synchStatusWeb.StatusForUI = GetFriendlyStatusText(synchStatus);
                synchStatusWeb.PercentComplete = CalculatePercentComplete(synchStatus);
                synchStatusWeb.SyncRate = CalculateSyncRate(synchStatus);

                if ((synchStatus.CurrentFilePath != null) && (synchStatus.CurrentFileName != null))
                {
                    try
                    {
                        synchStatusWeb.CurrentFile = Path.Combine(synchStatus.CurrentFilePath, synchStatus.CurrentFileName);
                    }
                    catch (ArgumentException ex)
                    {
                        synchStatusWeb.CurrentFile = String.Empty;

                        ex.Data.Add("INFO", "This error was handled and should not affect the user experience unless it occurs frequently.");
                        ex.Data.Add("synchStatus.CurrentFilePath", synchStatus.CurrentFilePath);
                        ex.Data.Add("synchStatus.CurrentFileName", synchStatus.CurrentFileName);
                        AppEventController.LogError(ex, synchStatus.GalleryId);
                    }
                }

                // Update the Skipped Files, but only when the sync is complete. 
                lock (synchStatus)
                {
                    if (synchStatus.Status == SynchronizationState.Complete)
                    {
                        if (synchStatus.SkippedMediaObjects.Count > GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch)
                        {
                            // We have a large number of skipped media objects. We don't want to send it all to the browsers, because it might take
                            // too long or cause an error if it serializes to a string longer than int.MaxValue, so let's trim it down.
                            synchStatus.SkippedMediaObjects.RemoveRange(GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch, synchStatus.SkippedMediaObjects.Count - GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch);
                        }
                        synchStatusWeb.SkippedFiles = synchStatus.SkippedMediaObjects;
                    }
                }
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, synchStatus.GalleryId);

                synchStatusWeb.StatusForUI = "An error occurred while retrieving the status";
            }

            return synchStatusWeb;
        }

        /// <summary>
        /// Terminates the synchronization with the specified <paramref name="syncId"/> and <paramref name="galleryId" />.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="syncId">The synchronization ID representing the synchronization to cancel.</param>
        public static void AbortSync(string syncId, int galleryId)
        {
            SynchronizationStatus.GetInstance(galleryId).CancelSynchronization(syncId);
        }

        /// <summary>
        /// Gets a collection of all UI templates from the data store. Returns an empty collection if no
        /// errors exist.
        /// </summary>
        /// <returns>Returns a collection of all UI templates from the data store.</returns>
        public static IUiTemplateCollection GetUiTemplates()
        {
            return Factory.LoadUiTemplates();
        }

        /// <summary>
        /// Gets the gallery data for the specified <paramref name="mediaObject" />.
        /// <see cref="GalleryData.Settings" /> is set to null because those values
        /// are calculated from control-specific properties that are not known at this time (it is
        /// expected that that property is assigned by subsequent code - including javascript -
        /// when that data is able to be calculated). Guaranteed to not return null.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        /// <param name="mediaObjectContainer">The media object container.</param>
        /// <param name="options">Specifies options for configuring the return data. To use default
        /// settings, specify an empty instance with properties left at default values.</param>
        /// <returns>Returns an instance of <see cref="GalleryData" />.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to access the <paramref name="mediaObject" />.</exception>
        public static GalleryData GetGalleryDataForMediaObject(IGalleryObject mediaObject, IAlbum mediaObjectContainer, GalleryDataLoadOptions options)
        {
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), mediaObject.Parent.Id, mediaObject.GalleryId, Utils.IsAuthenticated, mediaObject.Parent.IsPrivate, ((IAlbum)mediaObject.Parent).IsVirtualAlbum);

            var data = new GalleryData
            {
                App = GetAppEntity(),
                Settings = null,
                Album = AlbumController.ToAlbumEntity(mediaObjectContainer, options),
                ActiveMetaItems = null, // Assigned on client
                ActiveGalleryItems = null, // Assigned on client
                Resource = ResourceController.GetResourceEntity()
            };

            data.MediaItem = GetCurrentMediaItem(data, mediaObject);

            // Assign user, but only grab the required fields. We do this to prevent unnecessary user data from traveling the wire.
            var user = UserController.GetUserEntity(Utils.UserName, mediaObject.GalleryId);
            data.User = new User()
            {
                UserName = user.UserName,
                IsAuthenticated = user.IsAuthenticated,
                CanAddAlbumToAtLeastOneAlbum = user.CanAddAlbumToAtLeastOneAlbum,
                CanAddMediaToAtLeastOneAlbum = user.CanAddMediaToAtLeastOneAlbum,
                CanEditAtLeastOneAlbum = user.CanEditAtLeastOneAlbum,
                CanEditAtLeastOneMediaAsset = user.CanEditAtLeastOneMediaAsset,
                UserAlbumId = user.UserAlbumId
            };

            return data;
        }

        /// <summary>
        /// Gets the gallery data for the specified <paramref name="album" />.
        /// <see cref="GalleryData.MediaItem" /> is set to null since no particular media object
        /// is in context. <see cref="GalleryData.Settings" /> is also set to null because those values
        /// are calculated from control-specific properties that are not known at this time (it is 
        /// expected that that property is assigned by subsequent code - including javascript - 
        /// when that data is able to be calculated). Guaranteed to not return null.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <param name="options">Specifies options for configuring the return data. To use default
        /// settings, specify an empty instance with properties left at default values.</param>
        /// <returns>Returns an instance of <see cref="GalleryData" />.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to access the <paramref name="album" />.</exception>
        public static GalleryData GetGalleryDataForAlbum(IAlbum album, GalleryDataLoadOptions options)
        {
            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

            var data = new GalleryData
            {
                App = GetAppEntity(),
                Settings = null,
                Album = AlbumController.ToAlbumEntity(album, options),
                MediaItem = null,
                ActiveMetaItems = null, // Assigned on client
                ActiveGalleryItems = null, // Assigned on client
                Resource = ResourceController.GetResourceEntity()
            };

            // Assign user, but only grab the required fields. We do this to prevent unnecessary user data from traveling the wire.
            var user = UserController.GetUserEntity(Utils.UserName, album.GalleryId);
            data.User = new User()
            {
                UserName = user.UserName,
                IsAuthenticated = user.IsAuthenticated,
                CanAddAlbumToAtLeastOneAlbum = user.CanAddAlbumToAtLeastOneAlbum,
                CanAddMediaToAtLeastOneAlbum = user.CanAddMediaToAtLeastOneAlbum,
                CanEditAtLeastOneAlbum = user.CanEditAtLeastOneAlbum,
                CanEditAtLeastOneMediaAsset = user.CanEditAtLeastOneMediaAsset,
                UserAlbumId = user.UserAlbumId
            };

            return data;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Find the <paramref name="mediaObject" /> in the <see cref="Entity.Album.MediaItems" /> property of <paramref name="data" />,
        /// configure it, and return it as an instance of <see cref="MediaItem" />. The metadata are
        /// assigned to the <see cref="Entity.MediaItem.MetaItems" /> property of the returned instance.
        /// </summary>
        /// <param name="data">The gallery data. It is expected, though not necessary, for the 
        /// <paramref name="mediaObject" /> to have a match in one of the items in 
        /// <see cref="Entity.Album.MediaItems" /> property of data.</param>
        /// <param name="mediaObject">The media object.</param>
        /// <returns>Returns an instance of <see cref="MediaItem" />.</returns>
        private static MediaItem GetCurrentMediaItem(GalleryData data, IGalleryObject mediaObject)
        {
            MediaItem mediaItem = null;

            if (data.Album.MediaItems != null)
                mediaItem = data.Album.MediaItems.FirstOrDefault(mo => mo.Id == mediaObject.Id);

            if (mediaItem == null)
                mediaItem = GalleryObjectController.ToMediaItem(mediaObject, 0, MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(mediaObject));

            mediaItem.MetaItems = GalleryObjectController.ToMetaItems(mediaObject.MetadataItems.GetVisibleItems(), mediaObject);

            return mediaItem;
        }

        /// <summary>
        /// Initialize the components of the Gallery Server application that do not require access to an HttpContext.
        /// This method is designed to be run at application startup. The business layer
        /// is initialized with the current trust level and a few configuration settings. The business layer also initializes
        /// the data store, including verifying a minimal level of data integrity, such as at least one record for the root album.
        /// </summary>
        /// <remarks>This is the only method, apart from those invoked through web services, that is not handled by the global error
        /// handling routine in Gallery.cs. This method wraps its calls in a try..catch that passes any exceptions to
        /// <see cref="AppEventController.HandleGalleryException(Exception, int?)"/>. If that method does not transfer the user to a friendly error page, the exception
        /// is re-thrown.</remarks>
        private static void InitializeApplication()
        {
            lock (_sharedLock)
            {
                if (AppSetting.Instance.IsInitialized)
                    return;

                string msg = CheckForDbCompactionRequest();

                GallerySettings.GallerySettingsSaved += new EventHandler<GallerySettingsEventArgs>(GallerySettingsSaved);

                // Set web-related variables in the business layer and initialize the data store.
                InitializeBusinessLayer();

                AppSetting.Instance.InstallationRequested = Utils.InstallRequested;

                // Make sure installation has its own unique encryption key.
                ValidateEncryptionKey();

                MediaConversionQueue.Instance.Process();

                ValidateActiveDirectoryRequirements();

                // If there is a message from the DB compaction, record it now. We couldn't do it before because the DB
                // wasn't fully initialized.
                if (!String.IsNullOrEmpty(msg))
                    AppEventController.LogEvent(msg);
            }
        }

        /// <summary>
        /// Verify that if the Active Directory membership or role provider is being used, it conforms to licensing and application requirements.
        /// </summary>
        private static void ValidateActiveDirectoryRequirements()
        {
            ValidateActiveDirectoryRoleProviderRequirements();
        }

        /// <summary>
        /// Verify that if the Active Directory role provider is being used, incompatible settings are made compatible. Specifically, verify
        /// that AspNetActiveDirectoryMembershipProvider is being used and that the gallery setting <see cref="IGallerySettings.DefaultRolesForUser" />
        /// is cleared out if it has any roles, since role management is handled by AD groups, not this application.
        /// </summary>
        private static void ValidateActiveDirectoryRoleProviderRequirements()
        {
            if (RoleController.RoleGsp.GetType().ToString() != GlobalConstants.ActiveDirectoryRoleProviderName)
            {
                return;
            }

            if (UserController.MembershipGsp.GetType().ToString() != GlobalConstants.ActiveDirectoryMembershipProviderName)
            {
                throw new System.Configuration.Provider.ProviderException($"Gallery Server requires the Active Directory Membership Provider (AspNetActiveDirectoryMembershipProvider) when the Active Directory Role Provider (ActiveDirectoryRoleProvider) is being used. Instead, the membership provider {UserController.MembershipGsp.GetType()} was detected.");
            }

            foreach (var gallery in Factory.LoadGalleries())
            {
                var gallerySetting = Factory.LoadGallerySetting(gallery.GalleryId);

                if (gallerySetting.DefaultRolesForUser != null && gallerySetting.DefaultRolesForUser.Length > 0)
                {
                    // Admin probably just switched to using the AD role provider, so we need to clear out the default roles.
                    var gallerySettingUpdateable = Factory.LoadGallerySetting(gallery.GalleryId, true);
                    gallerySettingUpdateable.DefaultRolesForUser = new string[0];
                    gallerySettingUpdateable.Save();
                }
            }
        }

        /// <summary>
        /// Check for the app setting 'CompactDatabaseOnStartup' in web.config. If true, then compact and repair the
        /// database. Applies only to SQL CE, this can be used if the database is corrupt and the user is not able to
        /// navigate to the Site admin page to manually invoke the operation.
        /// </summary>
        /// <returns>Returns a message indicating the result of the operation, or null if no operation was performed.</returns>
        private static string CheckForDbCompactionRequest()
        {
            string msg = null;
            bool compactDb;
            if (Boolean.TryParse(WebConfigurationManager.AppSettings["CompactAndRepairDatabaseOnStartup"], out compactDb) && compactDb)
            {
                DbManager.CompactAndRepairSqlCeDatabase(out msg);
            }
            return msg;
        }

        private static void InsertSampleUsersAndRoles()
        {
            // Get list of all album IDs
            List<int> albumIds = new List<int>();
            foreach (IGalleryServerRole role in RoleController.GetGalleryServerRoles())
            {
                if (role.RoleName == "System Administrator")
                {
                    albumIds.AddRange(role.AllAlbumIds);
                    albumIds.Sort();
                }
            }

            //// Create roles and assign each one to a random album
            Random rdm = new Random();
            const int numRoles = 100;
            for (int i = 0; i < numRoles; i++)
            {
                int albumId;
                do
                {
                    albumId = rdm.Next(albumIds[0], albumIds[albumIds.Count - 1]);
                } while (!albumIds.Contains(albumId));

                IIntegerCollection roleAlbums = new IntegerCollection();
                roleAlbums.Add(albumId);
                RoleController.CreateRole("Role " + i, true, false, true, false, true, false, true, false, true, false, false, false, roleAlbums);
            }

            // Create users and assign to random number of roles.
            const int numUsers = 100;
            for (int i = 0; i < numUsers; i++)
            {
                int numRolesToAssignToUser = rdm.Next(0, 5); // Add up to 5 roles to user
                List<String> roleNames = new List<string>(numRolesToAssignToUser);
                for (int j = 0; j < numRolesToAssignToUser; j++)
                {
                    // Pick a random role
                    string roleName = "Role " + rdm.Next(0, numRoles - 1);
                    if (!roleNames.Contains(roleName))
                        roleNames.Add(roleName);
                }

                string userName = "User " + i;
                if (UserController.GetUser(userName, false) == null)
                {
                    UserController.CreateUser(userName, "111", String.Empty, roleNames.ToArray(), false, 1);
                }
            }
        }

        /// <summary>
        /// Set up the business layer with information about this web application, such as its trust level and a few settings
        /// from the configuration file.
        /// </summary>
        /// <exception cref="CannotWriteToDirectoryException">
        /// Thrown when Gallery Server is unable to write to, or delete from, the media objects directory.</exception>
        private static void InitializeBusinessLayer()
        {
            // Determine the trust level this web application is running in and set to a global variable. This will be used 
            // throughout the application to gracefully degrade when we are not at Full trust.
            ApplicationTrustLevel trustLevel = Utils.GetCurrentTrustLevel();

            // Get the application path so that the business layer (and any dependent layers) has access to it. Don't use 
            // HttpContext.Current.Request.PhysicalApplicationPath because in some cases HttpContext.Current won't be available
            // (for example, when the DotNetNuke search engine indexer causes this code to trigger).
            string physicalApplicationPath = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1);
            physicalApplicationPath = physicalApplicationPath.Replace("/", "\\");

            // Pass these values to our global app settings instance, where the values can be used throughout the application.
            AppSetting.Instance.Initialize(trustLevel, physicalApplicationPath, Constants.APP_NAME, Utils.GalleryResourcesPath);

            //Business.Entity.VersionKey.GenerateEncryptedVersionKeys();
        }

        /// <summary>
        /// Verify that the encryption key in the application settings has been changed from its original, default value. The key is 
        /// updated with a new value if required. Each installation should have a unique key.
        /// </summary>
        private static void ValidateEncryptionKey()
        {
            // This function is called from a function using a lock, so we don't need to do our own locking.
            if (AppSetting.Instance.EncryptionKey.Equals(GlobalConstants.ENCRYPTION_KEY, StringComparison.Ordinal))
            {
                AppSetting.Instance.EncryptionKey = Utils.GenerateNewEncryptionKey();
                AppSetting.Instance.Save();
            }
        }

        /// <summary>
        /// Adds the user account information to gallery settings. Since the business layer does not have a reference to System.Web.dll,
        /// it could not load membership data when the gallery settings were first initialized. We know that information now, so let's
        /// populate the user accounts with the user data.
        /// </summary>
        private static void AddMembershipDataToGallerySettings()
        {
            // The UserAccount objects should have been created and initially populated with the UserName property,
            // so we'll use the user name to retrieve the user's info and populate the rest of the properties on each object.
            foreach (IGallery gallery in Factory.LoadGalleries())
            {
                IGallerySettings gallerySetting = Factory.LoadGallerySetting(gallery.GalleryId);

                // Populate user account objects with membership data
                foreach (IUserAccount userAccount in gallerySetting.UsersToNotifyWhenAccountIsCreated)
                {
                    UserController.LoadUser(userAccount);
                }

                foreach (IUserAccount userAccount in gallerySetting.UsersToNotifyWhenErrorOccurs)
                {
                    UserController.LoadUser(userAccount);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="GallerySettings.GallerySettingsSaved" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void GallerySettingsSaved(object sender, GallerySettingsEventArgs e)
        {
            // Finish populating those properties that weren't populated in the business layer.
            AddMembershipDataToGallerySettings();

            // If the default roles setting has changed, add or remove users to/from roles on a background thread.
            if ((e.DefaultRolesForUserAdded != null && e.DefaultRolesForUserAdded.Length > 0) || (e.DefaultRolesForUserRemoved != null && e.DefaultRolesForUserRemoved.Length > 0))
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                 {
                     try
                     {
                 // For each added role, find the users *NOT* in the role and add them to the role
                 var allUsers = UserController.GetAllUsers();
                         foreach (var roleName in e.DefaultRolesForUserAdded)
                         {
                             if (RoleController.RoleExists(roleName))
                             {
                                 RoleController.AddUsersToRole(allUsers.Select(u => u.UserName).Except(RoleController.GetUsersInRole(roleName)).ToArray(), roleName);
                             }
                         }

                 // For each removed role, find the users in the role and remove them from the role
                 foreach (var roleName in e.DefaultRolesForUserRemoved)
                         {
                             if (RoleController.RoleExists(roleName))
                             {
                                 RoleController.RemoveUsersFromRole(RoleController.GetUsersInRole(roleName), roleName);
                             }
                         }

                         CacheController.RemoveCache(CacheItem.GalleryServerRoles);
                     }
                     catch (Exception ex)
                     {
                         AppEventController.LogError(ex, e.GalleryId);
                     }
                 });
            }
        }

        /// <summary>
        /// Gets a data entity containing application-level properties. The instance can be JSON-parsed and sent to the 
        /// browser.
        /// </summary>
        /// <returns>Returns an instance of <see cref="App" />.</returns>
        private static App GetAppEntity()
        {
            return new App
            {
                GalleryResourcesPath = Utils.GalleryResourcesPath,
                Skin = Utils.Skin,
                SkinPath = Utils.SkinPath,
                CurrentPageUrl = Utils.GetCurrentPageUrl(),
                AppUrl = Utils.GetAppUrl(),
                LatestUrl = Utils.GetLatestUrl(),
                TopRatedUrl = Utils.GetTopRatedUrl(),
                HostUrl = Utils.GetHostUrl(),
                AllowGalleryAdminToManageUsersAndRoles = AppSetting.Instance.AllowGalleryAdminToManageUsersAndRoles,
                IsDebugEnabled = Utils.IsDebugEnabled
            };
        }

        private static string GetFriendlyStatusText(ISynchronizationStatus status)
        {
            switch (status.Status)
            {
                case SynchronizationState.AnotherSynchronizationInProgress:
                    return Resources.GalleryServer.Task_Synch_Progress_Status_SynchInProgressException_Hdr;
                case SynchronizationState.InterruptedByAppRecycle:
                    return "Synchronization interrupted by app recycle";
                case SynchronizationState.Complete:
                    return String.Concat(status.Status, GetProgressCount(status));
                case SynchronizationState.Error:
                    return String.Concat(status.Status, GetProgressCount(status));
                case SynchronizationState.PersistingToDataStore:
                    return String.Concat(Resources.GalleryServer.Task_Synch_Progress_Status_PersistingToDataStore_Hdr, GetProgressCount(status));
                case SynchronizationState.SynchronizingFiles:
                    return String.Concat(Resources.GalleryServer.Task_Synch_Progress_Status_SynchInProgress_Hdr, GetProgressCount(status));
                case SynchronizationState.Aborted:
                    return String.Concat(Resources.GalleryServer.Task_Synch_Progress_Status_Aborted_Hdr, GetProgressCount(status));
                default: throw new System.ComponentModel.InvalidEnumArgumentException("The GetFriendlyStatusText() method in synchronize.aspx encountered a SynchronizationState enum value it was not designed for. This method must be updated.");
            }
        }

        private static int CalculatePercentComplete(ISynchronizationStatus synchStatus)
        {
            if (synchStatus.Status == SynchronizationState.SynchronizingFiles)
                return (int)(((double)synchStatus.CurrentFileIndex / (double)synchStatus.TotalFileCount) * 100);
            else
                return 100;
        }

        private static string CalculateSyncRate(ISynchronizationStatus synchStatus)
        {
            if (synchStatus.CurrentFileIndex == 0)
                return String.Empty;

            var elapsedTime = DateTime.UtcNow.Subtract(synchStatus.BeginTimestampUtc).TotalSeconds;

            return String.Format(CultureInfo.CurrentCulture, "{0:N1} {1}", (synchStatus.CurrentFileIndex / elapsedTime), Resources.GalleryServer.Task_Synch_Progress_SynchRate_Units);
        }

        private static string GetProgressCount(ISynchronizationStatus status)
        {
            var curFileIndex = (status.Status == SynchronizationState.Complete ? status.TotalFileCount : status.CurrentFileIndex);

            return String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Task_Synch_Progress_Status, curFileIndex, status.TotalFileCount);
        }

        /// <summary>
        /// Create a sample album and media object. This method is intended to be invoked once just after the application has been 
        /// installed.
        /// </summary>
        /// <param name="galleryId">The ID for the gallery where the sample objects are to be created.</param>
        private static void CreateSampleObjects(int galleryId)
        {
            if (!AppSetting.Instance.InstallationRequested)
            {
                return;
            }

            if (Factory.LoadGallerySetting(galleryId).MediaObjectPathIsReadOnly)
            {
                return;
            }

            DateTime currentTimestamp = DateTime.UtcNow;
            IAlbum sampleAlbum = null;

            foreach (IAlbum album in Factory.LoadRootAlbumInstance(galleryId).GetChildGalleryObjects(GalleryObjectType.Album))
            {
                if (album.DirectoryName == "Samples")
                {
                    sampleAlbum = Factory.LoadAlbumInstance(new AlbumLoadOptions(album.Id) { IsWritable = true });
                    break;
                }
            }
            if (sampleAlbum == null)
            {
                // Create sample album.
                sampleAlbum = Factory.CreateEmptyAlbumInstance(galleryId, true);

                sampleAlbum.Parent = Factory.LoadRootAlbumInstance(galleryId);
                sampleAlbum.Title = "Samples";
                sampleAlbum.DirectoryName = "Samples";
                sampleAlbum.Caption = Resources.GalleryServer.Site_Welcome_Msg;
                sampleAlbum.CreatedByUserName = GlobalConstants.SystemUserName;
                sampleAlbum.DateAdded = currentTimestamp;
                sampleAlbum.LastModifiedByUserName = GlobalConstants.SystemUserName;
                sampleAlbum.DateLastModified = currentTimestamp;
                sampleAlbum.Save();
            }

            foreach (var sampleAssetFileName in Constants.SAMPLE_ASSET_FILENAMES)
            {
                // Look for sample asset in sample album.
                var sampleImage = sampleAlbum.GetChildGalleryObjects(GalleryObjectType.Image)
                  .FirstOrDefault(image => image.Original.FileName == sampleAssetFileName);

                if (sampleImage == null)
                {
                    // Sample image not found. Pull image from assembly and save to disk (if needed), then create a media object from it.
                    var sampleDirPath = Path.Combine(Factory.LoadGallerySetting(galleryId).FullMediaObjectPath, sampleAlbum.DirectoryName);
                    var sampleAssetFilePath = Path.Combine(sampleDirPath, sampleAssetFileName);

                    var sourceFilePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.AppDataDirectory, sampleAssetFileName);
                    if (!File.Exists(sampleAssetFilePath))
                    {
                        if (File.Exists(sourceFilePath))
                        {
                            HelperFunctions.MoveFileSafely(sourceFilePath, sampleAssetFilePath);
                        }
                    }
                    else
                    {
                        File.Delete(sourceFilePath);
                    }

                    if (File.Exists(sampleAssetFilePath))
                    {
                        // Temporarily change a couple settings so that the thumbnail and compressed images are high quality.
                        var gallerySettings = Factory.LoadGallerySetting(galleryId);
                        var optTriggerSizeKb = gallerySettings.OptimizedImageTriggerSizeKb;
                        var thumbImageJpegQuality = gallerySettings.ThumbnailImageJpegQuality;
                        gallerySettings.ThumbnailImageJpegQuality = 95;
                        gallerySettings.OptimizedImageTriggerSizeKb = 200;

                        // Create the media object from the file.
                        var image = Factory.CreateImageInstance(new FileInfo(sampleAssetFilePath), sampleAlbum);
                        image.CreatedByUserName = GlobalConstants.SystemUserName;
                        image.DateAdded = currentTimestamp;
                        image.LastModifiedByUserName = GlobalConstants.SystemUserName;
                        image.DateLastModified = currentTimestamp;
                        image.Save();

                        // Restore the default settings.
                        gallerySettings.OptimizedImageTriggerSizeKb = optTriggerSizeKb;
                        gallerySettings.ThumbnailImageJpegQuality = thumbImageJpegQuality;
                    }
                }
            }
        }

        private static IUserAccount CreateAdministrator(int galleryId)
        {
            var user = GetAdminUserFromInstallTextFile();

            if (user == null)
                return null;

            user.GalleryId = galleryId;

            if (UserController.MembershipGsp.GetType().ToString() == GlobalConstants.ActiveDirectoryMembershipProviderName)
            {
                return CreateActiveDirectoryAdministrator(user);
            }
            else
            {
                return CreateMembershipAdministrator(user);
            }
        }

        /// <summary>
        /// Configures the <paramref name="user" /> as a site administrator in the gallery. The user must already exist in
        /// Active Directory. A System Administrator role is created if it does not exist.
        /// </summary>
        /// <param name="user">The user to configure as a site administrator in the gallery. The only property that is
        /// references is <see cref="User.UserName" />.</param>
        /// <returns>Returns an <see cref="IUserAccount" /> representing the admin account, or null if <paramref name="user" />
        /// did not specify a username.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
        /// <exception cref="InvalidUserException">Thrown when the <paramref name="user" />
        /// does not exist in Active Directory.</exception>
        private static IUserAccount CreateActiveDirectoryAdministrator(User user)
        {
            if (user == null)
                throw new ArgumentNullException();

            if (UserController.MembershipGsp.GetType().ToString() != GlobalConstants.ActiveDirectoryMembershipProviderName)
            {
                throw new InvalidOperationException(String.Format("The function CreateActiveDirectoryAdministrator should be called only when using ActiveDirectoryMembershipProvider. Instead, {0} was detected.", UserController.MembershipGsp.GetType()));
            }

            var sysAdminRole = RoleController.ValidateSysAdminRole();

            IUserAccount userAccount = null;
            if (!String.IsNullOrEmpty(user.UserName))
            {
                userAccount = UserController.GetUser(user.UserName, false);

                if (userAccount == null)
                {
                    throw new InvalidUserException(string.Format("The Active Directory account {0} does not exist. Edit the text file at {1} to specify an existing AD account.", user.UserName, Utils.InstallFilePath));
                }

                if (!RoleController.IsUserInRole(user.UserName, sysAdminRole))
                {
                    RoleController.AddUserToRole(user.UserName, sysAdminRole);
                }
            }

            RoleController.CreateAuthUsersRole();
            if (!RoleController.IsUserInRole(user.UserName, Resources.GalleryServer.Site_Auth_Users_Role_Name))
            {
                RoleController.AddUserToRole(user.UserName, Resources.GalleryServer.Site_Auth_Users_Role_Name);
            }

            return userAccount;
        }

        /// <summary>
        /// Configures the <paramref name="user" /> as a site administrator in the gallery. The user is created if it doesn't
        /// exist. If the user exists, the user's password is updated with the specified password. A System Administrator role
        /// is created if it does not exist.
        /// </summary>
        /// <param name="user">The user to configure as a site administrator in the gallery. The <see cref="User.UserName" /> 
        /// and <see cref="User.Password" /> properties must both be specified. If both are null or empty, null is returned.</param>
        /// <returns>Returns an <see cref="IUserAccount" /> representing the admin account.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
        /// <exception cref="InvalidUserException">Thrown when <paramref name="user" />
        /// does not specify a username and password.</exception>
        private static IUserAccount CreateMembershipAdministrator(User user)
        {
            if (user == null)
                throw new ArgumentNullException();

            if (String.IsNullOrEmpty(user.UserName) && String.IsNullOrEmpty(user.Password))
                return null;

            if (!String.IsNullOrEmpty(user.UserName) && String.IsNullOrEmpty(user.Password))
            {
                throw new InvalidUserException(string.Format("No password was specified. Add a line to the text file at {0} that specifies a password. Example: Password=MyPassword", Utils.InstallFilePath));
            }

            var userAccount = UserController.GetUser(user.UserName, false);

            if (userAccount != null)
            {
                if (!UserController.MembershipGsp.ValidateUser(user.UserName, user.Password))
                {
                    // Password doesn't match. Try to update.
                    if (!UserController.EnablePasswordRetrieval)
                    {
                        throw new Exception(String.Format(CultureInfo.InvariantCulture, "Cannot change password because the membership's password retrieval setting is disabled. The password specified in {0} does not match the existing password for user {1}, so an attempt was made to change it. However, the membership provider does not allow it. Things you can try: (1) Specify a different username in the text file. (2) Enter the correct password for the user in the text file. (3) Edit web.config to allow password retrieval: Set enablePasswordRetrieval=\"true\" in the membership section.", Utils.InstallFilePath, user.UserName));
                    }

                    if (!UserController.ChangePassword(user.UserName, UserController.GetPassword(user.UserName), user.Password))
                    {
                        throw new Exception(String.Format(CultureInfo.InvariantCulture, "Cannot change password. The password specified in {0} does not match the existing password for user {1}, so an attempt was made to change it. However, the membership provider wouldn't allow it and did not specify a reason. Things you can try: (1) Specify a different username in the text file. (2) Enter a different password for the user in the text file, taking care to meet length and complexity requirements.", Utils.InstallFilePath, user.UserName));
                    }
                }

                RoleController.ValidateSysAdminRole();
                if (!RoleController.IsUserInRole(user.UserName, Resources.GalleryServer.Site_Sys_Admin_Role_Name))
                {
                    RoleController.AddUserToRole(user.UserName, Resources.GalleryServer.Site_Sys_Admin_Role_Name);
                }

                RoleController.CreateAuthUsersRole();
                if (!RoleController.IsUserInRole(user.UserName, Resources.GalleryServer.Site_Auth_Users_Role_Name))
                {
                    RoleController.AddUserToRole(user.UserName, Resources.GalleryServer.Site_Auth_Users_Role_Name);
                }
            }
            else
            {
                // User account doesn't exist. Create it.
                user.Roles = new[] { RoleController.ValidateSysAdminRole(), RoleController.CreateAuthUsersRole() };

                userAccount = UserController.CreateUser(user);
            }

            return userAccount;
        }

        /// <summary>
        /// Gets a <see cref="User" /> instance having the properties specified in <see cref="Utils.InstallFilePath" />.
        /// Supports these properties: UserName, Password, Email. Returns null if none of these exist in the text file.
        /// </summary>
        /// <returns>An instance of <see cref="User" />, or null.</returns>
        public static User GetAdminUserFromInstallTextFile()
        {
            User user = null;

            try
            {
                using (var sr = new StreamReader(Utils.InstallFilePath))
                {
                    var lineText = sr.ReadLine();
                    while (lineText != null)
                    {
                        var kvp = lineText.Split(new[] { '=' });

                        if (kvp.Length == 2)
                        {
                            if (kvp[0].Equals("UserName", StringComparison.OrdinalIgnoreCase))
                            {
                                if (user == null)
                                    user = new User();

                                user.UserName = kvp[1].Trim(); // Found username row
                            }

                            if (kvp[0].Equals("Password", StringComparison.OrdinalIgnoreCase))
                            {
                                if (user == null)
                                    user = new User();

                                user.Password = kvp[1].Trim(); // Found password row
                            }

                            if (kvp[0].Equals("Email", StringComparison.OrdinalIgnoreCase))
                            {
                                if (user == null)
                                    user = new User();

                                user.Email = kvp[1].Trim(); // Found email row
                            }
                        }

                        lineText = sr.ReadLine();
                    }
                }
            }
            catch (FileNotFoundException) { }

            return user;
        }

        /// <summary>
        /// Updates the root album title so that it no longer contains the message about creating an admin account.
        /// </summary>
        private static void UpdateRootAlbumTitleAfterAdminCreation(int galleryId)
        {
            var rootAlbum = Factory.LoadRootAlbumInstance(galleryId, true);
            var updateableRootAlbum = Factory.LoadAlbumInstance(new AlbumLoadOptions(rootAlbum.Id) { IsWritable = true });

            updateableRootAlbum.Caption = Resources.GalleryServer.Site_Welcome_Msg;
            GalleryObjectController.SaveGalleryObject(updateableRootAlbum);
        }

        private static void DeleteInstallFile()
        {
            try
            {
                File.Delete(Utils.InstallFilePath);
            }
            catch (Exception ex)
            {
                // IIS account indentiy doesn't have permission to delete install.txt. Tell user to it manually.
                ex.Data.Add("Info", String.Format(CultureInfo.InvariantCulture, "You must manually delete the file at {0}", Utils.InstallFilePath));
                AppEventController.LogError(ex);
            }
        }

        #endregion
    }
}
