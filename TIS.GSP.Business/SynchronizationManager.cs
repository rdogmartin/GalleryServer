using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.Properties;
using GalleryServer.Events;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Contains functionality for synchronizing the media object files on the hard drive with the records in the data store.
    /// </summary>
    public class SynchronizationManager
    {
        #region Fields

        private readonly int _galleryId;
        private readonly string _thumbnailRootPath;
        private readonly string _optimizedRootPath;
        private readonly long _optimizedTriggerSizeKb;
        private readonly int _optimizedMaxLength;
        private readonly string _thumbnailPrefix;
        private readonly string _optimizedPrefix;
        private readonly int _fullMediaObjectPathLength;

        private bool _isRecursive;
        private bool _rebuildOptimized;
        private bool _rebuildThumbnail;
        private int _lastTransactionCommitFileIndex;
        private IGallerySettings _gallerySetting;

        // About the synch status object: When a synch is started, we grab a reference to the singleton synch status for the gallery and
        // update its properties with the current synch info, then we persist to the database so other processes (such as an external 
        // utility) can check for synch status info. However, as the synch progresses we only update the in-memory version of the object and
        // do not write to the data store until the synch is complete, where we then mark the synch record as being complete.
        private ISynchronizationStatus _synchStatus;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SynchronizationManager" /> object, with the properties 
        /// <see cref="IsRecursive" />, <see cref="RebuildOptimized" />, and <see cref="RebuildThumbnail" />
        /// all defaulted to true.
        /// </summary>
        /// <param name="galleryId">The value that uniquely identifies the gallery to be synchronized.</param>
        public SynchronizationManager(int galleryId)
        {
            _galleryId = galleryId;
            _thumbnailRootPath = GallerySettings.FullThumbnailPath;
            _optimizedRootPath = GallerySettings.FullOptimizedPath;
            _optimizedTriggerSizeKb = GallerySettings.OptimizedImageTriggerSizeKb;
            _optimizedMaxLength = GallerySettings.MaxOptimizedLength;
            _thumbnailPrefix = GallerySettings.ThumbnailFileNamePrefix;
            _optimizedPrefix = GallerySettings.OptimizedFileNamePrefix;
            _fullMediaObjectPathLength = GallerySettings.FullMediaObjectPath.Length;

            _isRecursive = true;
            _rebuildOptimized = true;
            _rebuildThumbnail = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether the synchronization continues drilling down into directories
        /// below the current one. The default value is true.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the synchronization procedure recursively
        /// synchronizes all directories within the current one; otherwise, <c>false</c>.
        /// </value>
        public bool IsRecursive
        {
            get { return _isRecursive; }
            set { _isRecursive = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the optimized version is deleted and overwritten 
        /// with a new one based on the original file. Only relevant for images and for video/audio 
        /// files when FFmpeg is installed and an applicable encoder setting exists. The default 
        /// value is true.
        /// </summary>
        /// <value><c>true</c> if optimized images are overwritten during a synchronization; 
        /// otherwise, <c>false</c>.</value>
        public bool RebuildOptimized
        {
            get { return _rebuildOptimized; }
            set { _rebuildOptimized = value; }
        }

        /// <summary>
        /// Gets or sets the user name for the logged on user. This is used for the audit fields in the album and media
        /// objects.
        /// </summary>
        /// <value>The user name for the logged on user.</value>
        private string UserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a thumbnail image is deleted and overwritten 
        /// with a new one based on the original file. Applies to all media objects. The default 
        /// value is true.
        /// </summary>
        /// <value><c>true</c> if thumbnail images are overwritten during a synchronization; 
        /// otherwise, <c>false</c>.</value>
        public bool RebuildThumbnail
        {
            get { return _rebuildThumbnail; }
            set { _rebuildThumbnail = value; }
        }

        private IGallerySettings GallerySettings
        {
            get
            {
                if (_gallerySetting == null)
                {
                    _gallerySetting = Factory.LoadGallerySetting(_galleryId);
                }

                return _gallerySetting;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Synchronize the media object library, starting with the root album. Optionally specify that only the 
        /// specified album is synchronized. If <see cref="IsRecursive" /> = true, then child albums are recursively synchronized;
        /// otherwise, only the root album (or the specified album if that overload is used) is synchronized.
        /// </summary>
        /// <param name="synchId">A GUID that uniquely identifies the synchronization. If another synchronization is in 
        /// progress, a <see cref="SynchronizationInProgressException" /> exception is thrown.</param>
        /// <param name="userName">The user name for the logged on user. This is used for the audit fields in the album 
        /// and media objects.</param>
        /// <exception cref="SynchronizationInProgressException">
        /// Thrown if another synchronization is in progress.</exception>
        public void Synchronize(string synchId, string userName)
        {
            Synchronize(synchId, Factory.LoadRootAlbumInstance(_galleryId, true), userName);
        }

        /// <summary>
        /// Synchronize the media object library, starting with the root album. Optionally specify that only the 
        /// specified album is synchronized. If <see cref="IsRecursive" /> = true, then child albums are recursively synchronized;
        /// otherwise, only the root album (or the specified album if that overload is used) is synchronized.
        /// </summary>
        /// <param name="synchId">A GUID that uniquely identifies the synchronization. If another synchronization is in 
        /// progress, a <see cref="SynchronizationInProgressException" /> exception is thrown.</param>
        /// <param name="userName">The user name for the logged on user. This is used for the audit fields in the album 
        /// and media objects.</param>
        /// <param name="album">The album to synchronize.</param>
        /// <exception cref="SynchronizationInProgressException">
        /// Thrown if another synchronization is in progress.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        public void Synchronize(string synchId, IAlbum album, string userName)
        {
            if (album == null)
                throw new ArgumentNullException("album");

            try
            {
                Initialize(synchId, album, userName); // Will throw SynchronizationInProgressException if another is in progress. Will be caught be upstream code.

                var albumDirectory = new DirectoryInfo(album.FullPhysicalPathOnDisk);

                // Synchronize the files in this album. No recursive action.
                SynchronizeMediaObjectFiles(albumDirectory, album);

                if (IsRecursive)
                {
                    // Synchronize the child directories and their files. Acts recursively.
                    SynchronizeChildDirectories(albumDirectory, album);
                }

                Album.AssignAlbumThumbnailIfMissing(album, false, true, UserName);

                album.SortAsync(true, UserName, true);

                if (_synchStatus != null)
                    _synchStatus.Finish();
            }
            catch (SynchronizationTerminationRequestedException)
            {
                // The user has canceled the synchronization. Swallow the exception and return.
                return;
            }
            catch (SynchronizationInProgressException)
            {
                // Another sync is in progress. We don't want the generic catch below to change the sync state, so we intercept it here.
                throw;
            }
            catch (System.Threading.ThreadAbortException)
            {
                if (_synchStatus != null)
                    UpdateStatus(0, syncState: SynchronizationState.InterruptedByAppRecycle, persistToDatabase: true);

                throw;
            }
            catch
            {
                if (_synchStatus != null)
                    UpdateStatus(0, syncState: SynchronizationState.Error, persistToDatabase: true);

                throw;
            }
            finally
            {
                CacheController.RemoveInflatedAlbumsFromCache();
            }
        }

        #endregion

        #region Private Functions

        private void Initialize(string synchId, IAlbum album, string userName)
        {
            if (album == null)
                throw new ArgumentNullException("album");

            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");

            UserName = userName;

            // Tell the status instance we are starting a new synchronization. It will throw
            // SynchronizationInProgressException if another is in progress.
            _synchStatus = SynchronizationStatus.Start(synchId, album.GalleryId);

            _synchStatus.Update(SynchronizationState.NotSet, CountFiles(album.FullPhysicalPathOnDisk), null, null, null, null, true);
        }

        /// <summary>
        /// Get the number of files in the specified directory path, including any subdirectories if
        /// IsRecursive = true. But don't count any optimized or thumbnail files.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        /// <exception cref="System.IO.DirectoryNotFoundException">Thrown if the specified directory does not exist.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="directoryPath" /> is null or an empty string.</exception>
        private int CountFiles(string directoryPath)
        {
            if (String.IsNullOrEmpty(directoryPath))
                throw new ArgumentOutOfRangeException("directoryPath");

            int countTotal;

            try
            {
                countTotal = Directory.GetFiles(directoryPath).Length;
            }
            catch (UnauthorizedAccessException)
            {
                return 0;
            }

            // Get a count of the thumbnail and optimized images, but only if they are stored in the media objects directory.
            int countThumbnail = 0;
            if (GallerySettings.FullThumbnailPath.Equals(GallerySettings.FullMediaObjectPath))
            {
                try
                {
                    countThumbnail = Directory.GetFiles(directoryPath, GallerySettings.ThumbnailFileNamePrefix + "*").Length;
                }
                catch (UnauthorizedAccessException) { }
            }

            int countOptimized = 0;
            if (GallerySettings.FullOptimizedPath.Equals(GallerySettings.FullMediaObjectPath))
            {
                try
                {
                    countOptimized = Directory.GetFiles(directoryPath, GallerySettings.OptimizedFileNamePrefix + "*").Length;
                }
                catch (UnauthorizedAccessException) { }
            }

            string[] dirs = null;
            try
            {
                dirs = Directory.GetDirectories(directoryPath);
            }
            catch (UnauthorizedAccessException) { }

            if (_isRecursive && (dirs != null))
            {
                foreach (string dir in dirs)
                {
                    countTotal += CountFiles(dir);
                }
            }

            int totalNumFiles = countTotal - countThumbnail - countOptimized;

            // If we compute a number < 0, then just return 0.
            return (totalNumFiles < 0 ? 0 : totalNumFiles);
        }

        /// <summary>
        /// Ensure the directories and media object files within parentDirectory have corresponding albums 
        /// and media objects. An exception is thrown if parentAlbum.FullPhysicalPathOnDisk does not equal
        /// parentDirectory.FullName. If IsRecursive = true, this method recursively calls itself.
        /// </summary>
        /// <param name="parentDirectory">A DirectoryInfo instance corresponding to the FullPhysicalPathOnDisk
        /// property of parentAlbum.</param>
        /// <param name="parentAlbum">An album instance. Directories under the parentDirectory parameter will be
        /// added (or updated if they already exist) as child albums of this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentDirectory" /> or <paramref name="parentAlbum" /> is null.</exception>
        private void SynchronizeChildDirectories(DirectoryInfo parentDirectory, IAlbum parentAlbum)
        {
            #region Parameter validation

            if (parentDirectory == null)
                throw new ArgumentNullException("parentDirectory");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            if (!parentDirectory.FullName.Equals(parentAlbum.FullPhysicalPathOnDisk, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException(String.Format("Synchronization error. parentAlbum.FullPhysicalPathOnDisk must be equal to parentDirectory.FullName. parentDirectory.FullName='{0}'; parentAlbum.FullPhysicalPathOnDisk='{1}'", parentDirectory.FullName, parentAlbum.FullPhysicalPathOnDisk));

            #endregion

            // Perform a garbage collection. This helps clean up the image stream references from earlier media assets.
            // This is especially important when running on web servers with limited memory or memory limits imposed on the application pool.
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Recursively traverse all subdirectories and their files and synchronize each object we find.
            // Skip any hidden directories.
            DirectoryInfo[] childDirectories;
            try
            {
                childDirectories = parentDirectory.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            foreach (DirectoryInfo subdirectory in childDirectories)
            {
                if ((subdirectory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    _synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(subdirectory.FullName.Remove(0, _fullMediaObjectPathLength + 1), Resources.SynchronizationStatus_Hidden_Directory_Msg));
                    continue;
                }

                IAlbum childAlbum = SynchronizeDirectory(subdirectory, parentAlbum);

                try
                {
                    SynchronizeMediaObjectFiles(subdirectory, childAlbum);

                    SynchronizeChildDirectories(subdirectory, childAlbum);
                }
                catch (UnauthorizedAccessException)
                {
                    childAlbum.DeleteFromGallery();
                }
            }

            DeleteOrphanedAlbumRecords(parentAlbum);

            DeleteOrphanCacheDirectories(parentAlbum);
        }

        /// <summary>
        /// Synchronizes the media object files in the <paramref name="directory" /> associated with the <paramref name="album" />.
        /// Does not act recursively.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="album">The album.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when the IIS app pool identity cannot access the files in the directory.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> or <paramref name="directory" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the full directory path of <paramref name="directory" /> does not match the directory path of 
        /// <paramref name="album" />.</exception>
        private void SynchronizeMediaObjectFiles(DirectoryInfo directory, IAlbum album)
        {
            #region Parameter validation

            if (directory == null)
                throw new ArgumentNullException("directory");

            if (album == null)
                throw new ArgumentNullException("album");

            if (!directory.FullName.Equals(album.FullPhysicalPath, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Error in SynchronizeMediaObjectFiles(): The full directory path of the parameter 'directory' does not match the directory path of the parameter 'album'. directory.FullName='{0}'; album.FullPhysicalPath='{1}'", directory.FullName, album.FullPhysicalPath));

            #endregion

            //Update the media object table in the database with the file attributes of all
            //files in the directory passed to this function. Skip any hidden files.
            FileInfo[] files;
            try
            {
                files = directory.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                _synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(directory.Name, Resources.SynchronizationStatus_Restricted_Directory_Msg));
                throw;
            }

            // First sort by the filename.
            Array.Sort(files, (a, b) => String.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase)); // Don't use Ordinal or OrdinalIgnoreCase, as it sorts unexpectedly (e.g. 100.pdf comes before _100.pdf)

            foreach (FileInfo file in files)
            {
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    _synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(file.FullName.Remove(0, _fullMediaObjectPathLength + 1), Resources.SynchronizationStatus_Hidden_File_Msg));
                    continue;
                }

                #region Process thumbnail or optimized image

                if (file.Name.StartsWith(_thumbnailPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // We have a thumbnail image. If we are storing thumbnails in a different directory, delete the file, but only if the path
                    // is writable. The user may have just specified a new thumbnail path, and we need to delete all the previous thumbnails 
                    // from their original location.
                    if (_thumbnailRootPath != GallerySettings.FullMediaObjectPath && !GallerySettings.MediaObjectPathIsReadOnly)
                    {
                        File.Delete(file.FullName);
                    }
                    continue;
                }

                if (file.Name.StartsWith(_optimizedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // We have an optimized image. If we are storing optimized images in a different directory, delete the file, but only if the path
                    // is writable. The user may have just specified a new optimized path, and we need to delete all the previous optimized images 
                    // from their original location.
                    if (_optimizedRootPath != GallerySettings.FullMediaObjectPath && !GallerySettings.MediaObjectPathIsReadOnly)
                    {
                        File.Delete(file.FullName);
                    }
                    continue;
                }

                #endregion

                // See if this file is an existing media object.
                var mediaObject = album
                  .GetChildGalleryObjects(GalleryObjectType.MediaObject)
                  .FirstOrDefault(mo => mo.Original.FileNamePhysicalPath.Equals(file.FullName, StringComparison.OrdinalIgnoreCase));

                if (mediaObject != null)
                {
                    // Found an existing media object matching the file on disk. Update properties, but only if its file extension
                    // is enabled. (If this is a media object that had been added to Gallery Server but its file type was 
                    // subsequently disabled, we do not want to synchronize it - we want its info in the data store to be deleted.)
                    if (HelperFunctions.IsFileAuthorizedForAddingToGallery(file.Name, album.GalleryId))
                    {
                        UpdateExistingMediaObject(mediaObject);
                    }
                }
                else
                {
                    // No media object exists for this file. Create a new one.
                    CreateNewMediaObject(album, file);
                }

                int newFileIndex = _synchStatus.CurrentFileIndex + 1;
                if (newFileIndex < _synchStatus.TotalFileCount)
                {
                    var persistToDatabase = (_synchStatus.CurrentFileIndex % 100) == 0; // Save to DB every 100 files

                    UpdateStatus(newFileIndex, file.DirectoryName, file.Name, persistToDatabase: persistToDatabase);
                }

                lock (_synchStatus)
                {
                    if (_synchStatus.ShouldTerminate)
                    {
                        // Immediately set this property back to false so that we don't trigger this code again, then throw a special exception
                        // that will be caught and used to cancel the synch.
                        _synchStatus.Update(SynchronizationState.Aborted, null, String.Empty, null, String.Empty, false, true);
                        throw new SynchronizationTerminationRequestedException();
                    }
                }
            }

            // Synchronize any external media objects previously added. No recursive action.
            SynchronizeExternalMediaObjects(album);

            DeleteOrphanedMediaObjectRecords(album);

            DeleteOrphanedThumbnailAndOptimizedFiles(album);
        }

        private void UpdateStatus(int currentFileIndex, string filepath = null, string filename = null, SynchronizationState syncState = SynchronizationState.NotSet, bool persistToDatabase = false)
        {
            var currentFilePath = (filepath != null ? filepath.Remove(0, _fullMediaObjectPathLength).TrimStart(new char[] { Path.DirectorySeparatorChar }) : null);

            _synchStatus.Update(syncState, null, filename, currentFileIndex, currentFilePath, null, persistToDatabase);
        }

        private void CreateNewMediaObject(IAlbum album, FileInfo file)
        {
            try
            {
                IGalleryObject mediaObject = Factory.CreateMediaObjectInstance(file, album);
                HelperFunctions.UpdateAuditFields(mediaObject, UserName);
                mediaObject.Save();

                if (!GallerySettings.MediaObjectPathIsReadOnly && (GallerySettings.DiscardOriginalImageDuringImport))
                {
                    mediaObject.DeleteOriginalFile();
                    mediaObject.Save();
                }

                mediaObject.IsSynchronized = true;
            }
            catch (UnsupportedMediaObjectTypeException)
            {
                _synchStatus.SkippedMediaObjects.Add(new KeyValuePair<string, string>(file.FullName.Remove(0, _fullMediaObjectPathLength + 1), Resources.SynchronizationStatus_Disabled_File_Type_Msg));
            }
        }

        private void UpdateExistingMediaObject(IGalleryObject mediaObject)
        {
            mediaObject.RegenerateThumbnailOnSave = RebuildThumbnail;
            mediaObject.RegenerateOptimizedOnSave = RebuildOptimized;

            // Check for existence of thumbnail.
            if (!File.Exists(mediaObject.Thumbnail.FileNamePhysicalPath))
            {
                mediaObject.RegenerateThumbnailOnSave = true;
            }

            switch (mediaObject.GalleryObjectType)
            {
                case GalleryObjectType.Image:
                    EvaluateOriginalImage((Image)mediaObject);
                    EvaluateOptimizedImage((Image)mediaObject);
                    break;

                case GalleryObjectType.Video:
                case GalleryObjectType.Audio:
                    EvaluateOptimizedVideoAudio(mediaObject);
                    break;

                default:
                    UpdateNonImageWidthAndHeight(mediaObject);
                    break;
            }

            UpdateMetadataFilename(mediaObject);

            if (mediaObject.HasChanges)
            {
                HelperFunctions.UpdateAuditFields(mediaObject, UserName);
                mediaObject.Save();
            }

            mediaObject.IsSynchronized = true;
        }

        private void SynchronizeExternalMediaObjects(IAlbum album)
        {
            foreach (IGalleryObject mediaObject in album.GetChildGalleryObjects(GalleryObjectType.External))
            {
                mediaObject.IsSynchronized = true;

                // Check for existence of thumbnail.
                if (RebuildThumbnail || !File.Exists(mediaObject.Thumbnail.FileNamePhysicalPath))
                {
                    mediaObject.RegenerateThumbnailOnSave = true;
                    HelperFunctions.UpdateAuditFields(mediaObject, UserName);
                    mediaObject.Save();
                    mediaObject.IsSynchronized = true;
                }
            }
        }

        /// <summary>
        /// Find, or create if necessary, the album corresponding to the specified directory and set it as the 
        /// child of the parentAlbum parameter.
        /// </summary>
        /// <param name="directory">The directory for which to obtain a matching album object.</param>
        /// <param name="parentAlbum">The album that contains the album at the specified directory.</param>
        /// <returns>Returns an album object corresponding to the specified directory and having the specified
        /// parent album.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="directory" /> or <paramref name="parentAlbum" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when </exception>
        /// <exception cref="ArgumentException">Thrown when the full directory path of the parent of <paramref name="directory" /> does not match the 
        /// directory path of <paramref name="parentAlbum" />.</exception>
        private IAlbum SynchronizeDirectory(DirectoryInfo directory, IAlbum parentAlbum)
        {
            #region Parameter validation

            if (directory == null)
                throw new ArgumentNullException("directory");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            if (!directory.Parent.FullName.Equals(parentAlbum.FullPhysicalPathOnDisk.TrimEnd(new char[] { Path.DirectorySeparatorChar }), StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException(String.Format("Error in SynchronizeDirectory(). directory.Parent.FullName='{0}'; parentAlbum.FullPhysicalPathOnDisk='{1}'", directory.Parent.FullName, parentAlbum.FullPhysicalPathOnDisk.TrimEnd(new char[] { Path.DirectorySeparatorChar })));

            #endregion

            var childAlbum = (IAlbum)parentAlbum.GetChildGalleryObjects(GalleryObjectType.Album)
              .FirstOrDefault(a => a.FullPhysicalPathOnDisk == directory.FullName);

            if (childAlbum != null)
            {
                // Found the album. Update properties.
                childAlbum.IsPrivate = (parentAlbum.IsPrivate ? true : childAlbum.IsPrivate); // Only set to private if parent is private
                childAlbum.RegenerateThumbnailOnSave = RebuildThumbnail;
            }
            else
            {
                // No album exists for this directory. Create a new one.
                childAlbum = Factory.CreateEmptyAlbumInstance(parentAlbum.GalleryId, true);
                childAlbum.Parent = parentAlbum;

                string directoryName = directory.Name;
                childAlbum.Title = directoryName;
                //childAlbum.ThumbnailMediaObjectId = 0; // not needed
                childAlbum.DirectoryName = directoryName;
                childAlbum.FullPhysicalPathOnDisk = Path.Combine(parentAlbum.FullPhysicalPathOnDisk, directoryName);
                childAlbum.IsPrivate = parentAlbum.IsPrivate;
            }

            childAlbum.IsSynchronized = true;

            if (childAlbum.IsNew || childAlbum.HasChanges)
            {
                HelperFunctions.UpdateAuditFields(childAlbum, UserName);
                childAlbum.Save();
            }

            // Commit the transaction to the database for every 100 media objects that are processed.
            if ((_synchStatus.CurrentFileIndex - _lastTransactionCommitFileIndex) >= 100)
            {
                HelperFunctions.CommitTransaction();
                HelperFunctions.BeginTransaction();
                _lastTransactionCommitFileIndex = _synchStatus.CurrentFileIndex;
            }

            return childAlbum;
        }

        private static void DeleteOrphanedAlbumRecords(IAlbum album)
        {
            // Delete album records that weren't sync'd.
            foreach (var childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album).Where(a => !a.IsSynchronized))
            {
                childAlbum.DeleteFromGallery();
            }
        }

        /// <summary>
        /// Deletes any unused directories in the thumbnail and optimized cache directories associated with <paramref name="parentAlbum" />. 
        /// No action is taken if separate cache directories are not used.
        /// </summary>
        /// <param name="parentAlbum">The parent album.</param>
        private void DeleteOrphanCacheDirectories(IAlbum parentAlbum)
        {
            var isThmbDirDifferentThanOriginalDir = (GallerySettings.MediaObjectPath != GallerySettings.ThumbnailPath);
            var isOptDirDifferentThanOriginalDir = (GallerySettings.MediaObjectPath != GallerySettings.OptimizedPath);

            if (!isThmbDirDifferentThanOriginalDir && !isOptDirDifferentThanOriginalDir)
            {
                // The thumbnails and optimized files are stored in the same directory as the originals, so there's nothing to clean up.
                return;
            }

            var thmbAndOptPaths = new List<string>();

            if (isThmbDirDifferentThanOriginalDir)
            {
                thmbAndOptPaths.Add(HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(parentAlbum.FullPhysicalPathOnDisk, GallerySettings.FullThumbnailPath, GallerySettings.FullMediaObjectPath));
            }

            if (isOptDirDifferentThanOriginalDir)
            {
                thmbAndOptPaths.Add(HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(parentAlbum.FullPhysicalPathOnDisk, GallerySettings.FullOptimizedPath, GallerySettings.FullMediaObjectPath));
            }

            var childAlbums = parentAlbum.GetChildGalleryObjects(GalleryObjectType.Album);

            // Loop through each cache directory and look for child directories that aren't used by any of the child albums.
            foreach (var cacheDirPath in thmbAndOptPaths)
            {
                if (Directory.Exists(cacheDirPath))
                {
                    // Generate a list of directory paths used by all child albums
                    var childAlbumThumbDirPaths = childAlbums.Select(childAlbum => Path.Combine(cacheDirPath, ((IAlbum)childAlbum).DirectoryName)).ToList();

                    foreach (var cacheDirPathChild in Directory.EnumerateDirectories(cacheDirPath))
                    {
                        if (childAlbumThumbDirPaths.All(dirPath => !dirPath.Equals(cacheDirPathChild, StringComparison.OrdinalIgnoreCase)))
                        {
                            try
                            {
                                // None of the child albums are using this directory, so it's not needed. Smoke it.
                                Directory.Delete(cacheDirPathChild, true);
                            }
                            catch (IOException ex)
                            {
                                // An exception occurred, probably because the account ASP.NET is running under does not
                                // have permission to delete the directory. Let's record the error, but otherwise ignore it.
                                EventController.RecordError(ex, AppSetting.Instance, _galleryId, Factory.LoadGallerySettings());
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                // An exception occurred, probably because the account ASP.NET is running under does not
                                // have permission to delete the directory. Let's record the error, but otherwise ignore it.
                                EventController.RecordError(ex, AppSetting.Instance, _galleryId, Factory.LoadGallerySettings());
                            }
                        }
                    }
                }
            }
        }

        private static void DeleteOrphanedMediaObjectRecords(IAlbum album)
        {
            // Delete media object records that weren't sync'd.
            var orphanMediaObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject)
              .Where(mo => !mo.IsSynchronized && mo.GalleryObjectType != GalleryObjectType.External);

            foreach (var mediaObject in orphanMediaObjects)
            {
                mediaObject.DeleteFromGallery();
            }
        }

        /// <summary>
        /// Delete any thumbnail and optimized files that do not have matching media objects.
        /// This can occur when a user manually transfers (e.g. uses Windows Explorer)
        /// original files to a new directory and leaves the thumbnail and optimized
        /// files in the original directory or when a user deletes the original media file in 
        /// Explorer. This function *only* deletes files that begin the the thumbnail and optimized
        /// prefix (e.g. zThumb_, zOpt_).
        /// </summary>
        /// <param name="album">The album whose directory is to be processed for orphaned image files.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        private void DeleteOrphanedThumbnailAndOptimizedFiles(IAlbum album)
        {
            if (album == null)
                throw new ArgumentNullException("album");

            // STEP 1: Get list of directories that may contain thumbnail or optimized images for the current album
            string originalPath = album.FullPhysicalPathOnDisk;
            string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPathOnDisk, GallerySettings.FullThumbnailPath, GallerySettings.FullMediaObjectPath);
            string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPathOnDisk, GallerySettings.FullOptimizedPath, GallerySettings.FullMediaObjectPath);

            List<string> albumPaths = new List<string>(3);

            // The original path may contain thumbnails or optimized images when the thumbnail/optimized path is the same as the original path
            if ((GallerySettings.FullThumbnailPath.Equals(GallerySettings.FullMediaObjectPath, StringComparison.OrdinalIgnoreCase)) ||
              (GallerySettings.FullOptimizedPath.Equals(GallerySettings.FullMediaObjectPath, StringComparison.OrdinalIgnoreCase)))
            {
                albumPaths.Add(originalPath);
            }

            if (!albumPaths.Contains(thumbnailPath))
                albumPaths.Add(thumbnailPath);

            if (!albumPaths.Contains(optimizedPath))
                albumPaths.Add(optimizedPath);


            string thumbnailPrefix = GallerySettings.ThumbnailFileNamePrefix;
            string optimizedPrefix = GallerySettings.OptimizedFileNamePrefix;

            IGalleryObjectCollection mediaObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject);

            // STEP 2: Loop through each path and make sure all thumbnail and optimized files in each directory have 
            // matching media objects. Delete any files that do not.
            foreach (string albumPath in albumPaths)
            {
                if (!Directory.Exists(albumPath))
                    return;

                DirectoryInfo directory = new DirectoryInfo(albumPath);

                // Loop through each file in the directory.
                FileInfo[] files;
                try
                {
                    files = directory.GetFiles();
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }

                var queueItems = GetCurrentAndCompleteMediaQueueItems();

                foreach (FileInfo file in files)
                {
                    if ((file.Name.StartsWith(thumbnailPrefix, StringComparison.OrdinalIgnoreCase)) || (file.Name.StartsWith(optimizedPrefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        // This file is a thumbnail or optimized file.

                        // TEST 1: Check to see if any media object in this album refers to it.
                        var foundMediaObject = false;
                        foreach (IGalleryObject mediaObject in mediaObjects)
                        {
                            if ((mediaObject.Optimized.FileName.Equals(file.Name, StringComparison.OrdinalIgnoreCase)) ||
                              (mediaObject.Thumbnail.FileName.Equals(file.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                foundMediaObject = true;
                                break;
                            }
                        }

                        if (!foundMediaObject)
                        {
                            // TEST 2: Maybe the encoder engine is currently creating the file or just finished it.

                            // First check to see if we started processing a new media item since we started this loop.
                            // If so, add it to our list of queue items.
                            var currentQueueItem = MediaConversionQueue.Instance.GetCurrentMediaQueueItem();
                            if (currentQueueItem != null && !queueItems.Any(mq => mq.MediaQueueId == currentQueueItem.MediaQueueId))
                            {
                                queueItems = queueItems.Concat(new[] { currentQueueItem });
                            }

                            // See if this file is mentioned in any of the media queue items
                            foundMediaObject = queueItems.Any(mq => mq.StatusDetail.Contains(file.Name));
                        }

                        if (!foundMediaObject)
                        {
                            // No media object in this album refers to this thumbnail or optimized image. Smoke it!
                            try
                            {
                                file.Delete();
                            }
                            catch (IOException ex)
                            {
                                // An exception occurred, probably because the account ASP.NET is running under does not
                                // have permission to delete the file. Let's record the error, but otherwise ignore it.
                                EventController.RecordError(ex, AppSetting.Instance, _galleryId, Factory.LoadGallerySettings());
                            }
                            catch (SecurityException ex)
                            {
                                // An exception occurred, probably because the account ASP.NET is running under does not
                                // have permission to delete the file. Let's record the error, but otherwise ignore it.
                                EventController.RecordError(ex, AppSetting.Instance, _galleryId, Factory.LoadGallerySettings());
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                // An exception occurred, probably because the account ASP.NET is running under does not
                                // have permission to delete the file. Let's record the error, but otherwise ignore it.
                                EventController.RecordError(ex, AppSetting.Instance, _galleryId, Factory.LoadGallerySettings());
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<MediaQueueItem> GetCurrentAndCompleteMediaQueueItems()
        {
            var queueItems = MediaConversionQueue.Instance.MediaQueueItems.Where(mq => mq.Status == MediaQueueItemStatus.Complete);

            var currentQueueItem = MediaConversionQueue.Instance.GetCurrentMediaQueueItem();
            if (currentQueueItem != null)
            {
                queueItems = queueItems.Concat(new[] { currentQueueItem });
            }

            return queueItems;
        }

        private bool DoesOriginalExceedOptimizedTriggers(IGalleryObject mediaObject)
        {
            // Note: This function also exists in the ImageOptimizedCreator class.

            // Test 1: Is the file size of the original greater than OptimizedImageTriggerSizeKB?
            bool isOriginalFileSizeGreaterThanTriggerSize = false;

            if (mediaObject.Original.FileSizeKB > _optimizedTriggerSizeKb)
            {
                isOriginalFileSizeGreaterThanTriggerSize = true;
            }

            // Test 2: Is the width or length of the original greater than the MaxOptimizedLength?
            bool isOriginalLengthGreaterThanMaxAllowedLength = false;

            double originalWidth = 0;
            double originalHeight = 0;
            try
            {
                var size = mediaObject.Original.GetSize();
                originalWidth = size.Width;
                originalHeight = size.Height;
            }
            catch (UnsupportedImageTypeException ex)
            {
                EventController.RecordError(ex, AppSetting.Instance, _galleryId, Factory.LoadGallerySettings());
            }

            if ((originalWidth > _optimizedMaxLength) || (originalHeight > _optimizedMaxLength))
            {
                isOriginalLengthGreaterThanMaxAllowedLength = true;
            }

            return (isOriginalFileSizeGreaterThanTriggerSize | isOriginalLengthGreaterThanMaxAllowedLength);
        }

        /// <summary>
        /// If the rebuild thumbnail or rebuild image options are selected, then get the latest statistics about the 
        /// original image. Perhaps the user edited the object (such as rotating) in another program.
        /// </summary>
        /// <param name="mediaObject">The media object whose original image is to be checked.</param>
        private void EvaluateOriginalImage(Image mediaObject)
        {
            if (mediaObject == null)
                return;

            if (_rebuildThumbnail || _rebuildOptimized)
            {
                try
                {
                    var size = mediaObject.Original.GetSize();
                    mediaObject.Original.Width = (int)size.Width;
                    mediaObject.Original.Height = (int)size.Height;
                }
                catch (UnsupportedImageTypeException) { }

                int fileSize = (int)(mediaObject.Original.FileInfo.Length / 1024);
                mediaObject.Original.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
            }
        }

        /// <summary>
        /// Evaluates the optimized file for video and audio objects. If the optimized file doesn't exist or is no 
        /// longer wanted, update the optimized properties to match those of the original. This helps when the
        /// encoder is configured to ignore this particular file type.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        private void EvaluateOptimizedVideoAudio(IGalleryObject mediaObject)
        {
            if (mediaObject == null)
                return;

            var optFileMissing = !File.Exists(mediaObject.Optimized.FileNamePhysicalPath);
            var optFileIsDifferentThanOriginal = !mediaObject.Optimized.FileName.Equals(mediaObject.Original.FileName, StringComparison.OrdinalIgnoreCase);
            var optFileNotWanted = (RebuildOptimized && optFileIsDifferentThanOriginal && !MediaConversionQueue.Instance.HasEncoderSetting(mediaObject));

            if (optFileMissing || optFileNotWanted)
            {
                // If the file exists, it will later be deleted in DeleteOrphanedThumbnailAndOptimizedFiles.
                mediaObject.Optimized.FileName = mediaObject.Original.FileName;
                mediaObject.Optimized.Width = mediaObject.Original.Width;
                mediaObject.Optimized.Height = mediaObject.Original.Height;
                mediaObject.Optimized.FileSizeKB = mediaObject.Original.FileSizeKB;
            }
        }

        /// <summary>
        /// Check that the optimized image exists. <paramref name="mediaObject"/> *must* be an <see cref="Image"/> type.
        /// If "overwrite compressed" option is selected, also check whether it the optimized version is really needed.
        /// </summary>
        /// <param name="mediaObject">The media object whose optimized image is to be checked.</param>
        /// <remarks>Note that the ValidateSave() method in the GalleryObject class also checks for the existence of 
        /// the thumbnail and optimized images. However, we need to do it here because the UpdateAuditFields method
        /// that is called after this function is executed updates the audit fields only when HasChanges = true. If 
        /// we don't check for these images, then the media object might have HasChanges = false, which causes the 
        /// audit fields to remain unchanged. But then if ValidateSave updates them, we'll get an error because the 
        /// GalleryObject class doesn't update the audit fields (it knows nothing about the current user.)</remarks>
        private void EvaluateOptimizedImage(Image mediaObject)
        {
            if (mediaObject == null)
                return;

            // Check for existence of optimized image.
            if (!File.Exists(mediaObject.Optimized.FileNamePhysicalPath))
            {
                // Optimized image doesn't exist, but maybe we don't need it anyway. Check for this possibility.
                if (DoesOriginalExceedOptimizedTriggers(mediaObject))
                {
                    mediaObject.RegenerateOptimizedOnSave = true; // Yup, we need to generate the opt. image.
                }
                else
                {
                    // The original isn't big enough to need an optimized image, so make sure the optimized properties
                    // are the same as the original's properties.
                    mediaObject.Optimized.FileName = mediaObject.Original.FileName;
                    mediaObject.Optimized.Width = mediaObject.Original.Width;
                    mediaObject.Optimized.Height = mediaObject.Original.Height;
                    mediaObject.Optimized.FileSizeKB = mediaObject.Original.FileSizeKB;
                }
            }
            else
            {
                // We have an image where the optimized image exists. But perhaps the user changed some optimized trigger settings
                // and we no longer need the optimized image. Check for this possibility, and if true, update the optimized properties
                // to be the same as the original. Note: We only check if user selected the "overwrite compressed" option - this is 
                // because checking the dimensions of an image is very resource intensive, so we'll only do this if necessary.
                if (RebuildOptimized && !DoesOriginalExceedOptimizedTriggers(mediaObject))
                {
                    mediaObject.Optimized.FileName = mediaObject.Original.FileName;
                    mediaObject.Optimized.Width = mediaObject.Original.Width;
                    mediaObject.Optimized.Height = mediaObject.Original.Height;
                    mediaObject.Optimized.FileSizeKB = mediaObject.Original.FileSizeKB;
                }
            }
        }

        /// <summary>
        /// Update the width and height values to the default values specified for audio, and generic objects.
        /// This method has no effect on <see cref="Image"/>, <see cref="Video" /> or <see cref="ExternalMediaObject"/> objects.
        /// </summary>
        /// <param name="mediaObject">The <see cref="IGalleryObject"/> whose <see cref="DisplayObject.Width"/> and 
        /// <see cref="DisplayObject.Height"/> properties of the <see cref="IGalleryObject.Original"/> property is to be 
        /// updated with the current default values.</param>
        /// <remarks>We don't want to overwrite the width and height for videos because they may have been assigned valid
        /// values. See <see cref="MediaConversionQueue.GetTargetWidth" /> for how this can happen.</remarks>
        private void UpdateNonImageWidthAndHeight(IGalleryObject mediaObject)
        {
            if ((mediaObject is GenericMediaObject) && (mediaObject.MimeType.TypeCategory == MimeTypeCategory.Other))
            {
                // We want to update the width and height only when the TypeCategory is Other. If we don't check for this, we might
                // assign a width and height to a corrupt JPG that is being treated as a GenericMediaObject.
                mediaObject.Original.Width = GallerySettings.DefaultGenericObjectWidth;
                mediaObject.Original.Height = GallerySettings.DefaultGenericObjectHeight;
            }
            else if (mediaObject is Audio)
            {
                mediaObject.Original.Width = GallerySettings.DefaultAudioPlayerWidth;
                mediaObject.Original.Height = GallerySettings.DefaultAudioPlayerHeight;
            }
        }

        /// <summary>
        /// Updates the filename metadata item with the current file name. If the metadata item does not exist, no action is taken.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        private static void UpdateMetadataFilename(IGalleryObject mediaObject)
        {
            IGalleryObjectMetadataItem metaItem;
            if (mediaObject.MetadataItems.TryGetMetadataItem(MetadataItemName.FileName, out metaItem))
            {
                metaItem.Value = mediaObject.Original.FileName;
            }
        }

        #endregion
    }
}
