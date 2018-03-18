using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// A singleton class that manages the media encoder queue.
    /// </summary>
    public class MediaConversionQueue
    {
        #region Private Static Fields

        private readonly static Lazy<MediaConversionQueue> _instance = new Lazy<MediaConversionQueue>(() => new MediaConversionQueue());

        #endregion

        #region Private Fields

        private int _currentMediaQueueItemId;
        private IMediaEncoderSettingsCollection _attemptedEncoderSettings;
        private MediaQueueStatus _status;

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets a reference to the <see cref="MediaConversionQueue" /> singleton for this app domain.
        /// </summary>
        public static MediaConversionQueue Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the status of the media conversion queue. Setting this property triggers the <see cref="MediaQueueStatusChanged" /> event.
        /// </summary>
        /// <value>The status of the media conversion queue.</value>
        public MediaQueueStatus Status
        {
            get { return _status; }
            private set
            {
                _status = value;

                MediaQueueStatusChanged?.Invoke(null, new MediaConversionQueueEventArgs(null, _status, null));
            }
        }

        /// <summary>
        /// Gets the media items in the queue, including ones that have finished processing.
        /// </summary>
        /// <value>A collection of media queue items.</value>
        public ICollection<MediaQueueItem> MediaQueueItems { get { return MediaQueueItemDictionary.Values; } }

        /// <summary>
        /// Gets or sets an instance that can be used to cancel the media conversion process
        /// executing on the background thread.
        /// </summary>
        /// <value>An instance of <see cref="CancellationTokenSource" />.</value>
        protected CancellationTokenSource CancelTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the media conversion task executing as an asynchronous operation.
        /// </summary>
        /// <value>An instance of <see cref="Task" />.</value>
        protected Task Task { get; set; }

        /// <summary>
        /// Gets the collection of encoder settings that have already been tried for the
        /// current media queue item.
        /// </summary>
        /// <value>An instance of <see cref="IMediaEncoderSettingsCollection" />.</value>
        protected IMediaEncoderSettingsCollection AttemptedEncoderSettings
        {
            get { return _attemptedEncoderSettings ?? (_attemptedEncoderSettings = new MediaEncoderSettingsCollection()); }
        }

        /// <summary>
        /// Gets or sets the media items in the queue, including ones that have finished processing.
        /// </summary>
        /// <value>A thread-safe dictionary of media queue items.</value>
        private ConcurrentDictionary<int, MediaQueueItem> MediaQueueItemDictionary { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaConversionQueue"/> class.
        /// </summary>
        private MediaConversionQueue()
        {
            using (var repo = new MediaQueueRepository())
            {
                var items = MediaQueueItem.ToMediaQueueItems(repo.GetAll().OrderBy(mq => mq.DateAdded));

                MediaQueueItemDictionary = new ConcurrentDictionary<int, MediaQueueItem>(items.ToDictionary(m => m.MediaQueueId));
            }

            Reset();

            Status = MediaQueueStatus.Idle;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the status of the queue processor changes.
        /// </summary>
        public static event EventHandler<MediaConversionQueueEventArgs> MediaQueueStatusChanged;

        /// <summary>
        /// Occurs when a media queue item is added to the queue.
        /// </summary>
        public static event EventHandler<MediaConversionQueueEventArgs> MediaQueueItemAdded;

        /// <summary>
        /// Occurs when a media queue item is being processed and one of its properties has begun processing.
        /// </summary>
        public static event EventHandler<MediaConversionQueueEventArgs> MediaQueueItemStarted;

        /// <summary>
        /// Occurs when a media queue item is being processed and one of its properties has been updated.
        /// </summary>
        public static event EventHandler<MediaConversionQueueEventArgs> ActiveMediaQueueItemUpdated;

        /// <summary>
        /// Occurs when a media queue item has finished processing, either successfully or unsuccessfully.
        /// </summary>
        public static event EventHandler<MediaConversionQueueEventArgs> MediaQueueItemCompleted;

        /// <summary>
        /// Occurs when a media queue item has been deleted.
        /// </summary>
        public static event EventHandler<MediaConversionQueueEventArgs> MediaQueueItemDeleted;

        /// <summary>
        /// Occurs when a media queue item is being processed and data has been added to its <see cref="MediaQueueItem.StatusDetail" /> property.
        /// </summary>
        public static event EventHandler<MediaConversionQueueEventArgs> MediaQueueItemStatusDetailAppended;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the specified media queue item or null if no item matching the ID exists.
        /// </summary>
        /// <param name="mediaQueueId">The media queue ID.</param>
        /// <returns>An instance of <see cref="MediaQueueItem" /> or null.</returns>
        public MediaQueueItem Get(int mediaQueueId)
        {
            MediaQueueItem item;
            return MediaQueueItemDictionary.TryGetValue(mediaQueueId, out item) ? item : null;
        }

        /// <summary>
        /// Adds the specified <paramref name="mediaObject" /> to the queue. It will be processed in a first-in, first-out
        /// order. If the media object is already waiting in the queue, no action is taken.
        /// </summary>
        /// <param name="mediaObject">The media object to be processed.</param>
        /// <param name="conversionType">Type of the conversion.</param>
        public void Add(IGalleryObject mediaObject, MediaQueueItemConversionType conversionType)
        {
            var mqItem = new MediaQueueItem
            {
                MediaQueueId = int.MinValue,
                MediaObjectId = mediaObject.Id,
                Status = MediaQueueItemStatus.Waiting,
                ConversionType = conversionType,
                RotateFlipAmount = mediaObject.CalculateNeededRotation(),
                StatusDetail = String.Empty,
                DateAdded = DateTime.UtcNow,
                DateConversionStarted = null,
                DateConversionCompleted = null
            };

            mqItem.Save();
            //Factory.GetDataProvider().MediaQueue_Save(mediaQueueDto);

            MediaQueueItemDictionary.TryAdd(mqItem.MediaQueueId, mqItem);

            MediaQueueItemAdded?.Invoke(null, new MediaConversionQueueEventArgs(mqItem, _instance.Value.Status, null));
        }

        /// <summary>
        /// Cancels the currently processing queue item having <paramref name="mediaQueueId" />. The task is forcefully canceled and the item
        /// is assigned a status of <see cref="MediaQueueItemStatus.Canceled" />. If the ID of current item does not match 
        /// <paramref name="mediaQueueId" />, no action is taken.
        /// </summary>
        /// <param name="mediaQueueId">The media queue ID.</param>
        public void CancelMediaQueueItem(int mediaQueueId)
        {
            MediaQueueItem item;
            if (MediaQueueItemDictionary.TryGetValue(mediaQueueId, out item))
            {
                MediaQueueItem currentItem = GetCurrentMediaQueueItem();
                if ((currentItem != null) && (currentItem.MediaQueueId == mediaQueueId))
                {
                    CancelTokenSource.Cancel();

                    // Wait until queue is idle or it's moved on to the next item
                    do
                    {
                        System.Threading.Thread.Sleep(500);

                        currentItem = GetCurrentMediaQueueItem();
                    }
                    while (currentItem != null && currentItem.MediaQueueId == mediaQueueId);
                }
            }
        }

        /// <summary>
        /// Removes the item from the queue. If the item is currently being processed, the task
        /// is canceled.
        /// </summary>
        /// <param name="mediaObjectId">The media object ID.</param>
        public void Remove(int mediaObjectId)
        {
            foreach (var item in MediaQueueItemDictionary.Values.Where(m => m.MediaObjectId == mediaObjectId))
            {
                RemoveMediaQueueItem(item.MediaQueueId);
            }
        }

        /// <summary>
        /// Removes the item from the queue. If the item is currently being processed, the task
        /// is canceled. No action is taken if a queue item having <paramref name="mediaQueueId" /> doesn't exist.
        /// </summary>
        /// <param name="mediaQueueId">The media queue ID.</param>
        public void RemoveMediaQueueItem(int mediaQueueId)
        {
            MediaQueueItem item;
            if (MediaQueueItemDictionary.TryGetValue(mediaQueueId, out item))
            {
                MediaQueueItem currentItem = GetCurrentMediaQueueItem();
                if ((currentItem != null) && (currentItem.MediaQueueId == mediaQueueId))
                {
                    CancelTokenSource.Cancel();

                    // Wait until queue is idle or it's moved on to the next item
                    do
                    {
                        System.Threading.Thread.Sleep(500);

                        currentItem = GetCurrentMediaQueueItem();
                    }
                    while (currentItem != null && currentItem.MediaQueueId == mediaQueueId);

                    Instance.Status = MediaQueueStatus.Idle;
                }

                item.Delete();

                MediaQueueItemDictionary.TryRemove(mediaQueueId, out item);

                MediaQueueItemDeleted?.Invoke(null, new MediaConversionQueueEventArgs(item, _instance.Value.Status, null));
            }
        }

        /// <summary>
        /// Deletes all queue items older than 180 days.
        /// </summary>
        public void DeleteOldQueueItems()
        {
            DateTime purgeDate = DateTime.Today.AddDays(-180);

            foreach (var item in MediaQueueItemDictionary.Values.Where(m => m.DateAdded < purgeDate))
            {
                RemoveMediaQueueItem(item.MediaQueueId);
            }
        }

        /// <summary>
        /// Processes the items in the queue asynchronously. If the instance is already processing 
        /// items, no additional action is taken.
        /// </summary>
        public void Process()
        {
            if (Status == MediaQueueStatus.Processing)
                return;

            if (!FFmpeg.IsAvailable)
                return;

            Status = MediaQueueStatus.Processing;

            Task = Task.Factory.StartNew(() =>
            {
                try
                {
                    var mqItem = GetNextItemInQueue();

                    while (mqItem != null)
                    {
                        Reset();

                        _currentMediaQueueItemId = mqItem.MediaQueueId;

                        CancelTokenSource = new CancellationTokenSource();

                        ProcessItem();

                        mqItem = GetNextItemInQueue();
                    }
                }
                finally
                {
              // If we get here we've worked through the queue or an error happened.
              Instance.Status = MediaQueueStatus.Idle;
                }

            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Gets the media item currently being processed. If no item is being processed, the value 
        /// will be null.
        /// </summary>
        /// <returns>Returns the media item currently being processed, or null if no items are being processed.</returns>
        public MediaQueueItem GetCurrentMediaQueueItem()
        {
            MediaQueueItem item;
            return (MediaQueueItemDictionary.TryGetValue(_currentMediaQueueItemId, out item) ? item : null);
        }

        /// <summary>
        /// Determines whether the specified media object undergoing the specified <paramref name="conversionType" /> 
        /// is currently being processed by the media queue or is waiting in the queue.
        /// </summary>
        /// <param name="mediaObjectId">The ID of the media object.</param>
        /// <param name="conversionType">Type of the conversion. If the parameter is omitted, then a matching 
        /// media object having any conversion type will cause the method to return <c>true</c>.</param>
        /// <returns>Returns <c>true</c> if the media object is currently being processed by the media queue
        /// or is waiting in the queue; otherwise, <c>false</c>.</returns>
        public bool IsWaitingInQueueOrProcessing(int mediaObjectId, MediaQueueItemConversionType conversionType = MediaQueueItemConversionType.Unknown)
        {
            MediaQueueItem item = GetCurrentMediaQueueItem();

            if ((item != null) && item.MediaObjectId == mediaObjectId && (item.ConversionType == conversionType || conversionType == MediaQueueItemConversionType.Unknown))
                return true;
            else
                return IsWaitingInQueue(mediaObjectId, conversionType);
        }

        /// <summary>
        /// Determines whether the specified media object has an applicable encoder setting.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        /// <returns>
        /// 	<c>true</c> if the media object has an encoder setting; otherwise, <c>false</c>.
        /// </returns>
        public bool HasEncoderSetting(IGalleryObject mediaObject)
        {
            // Return true if the encoder args for the first match has a value; otherwise false.
            return GetEncoderSettings(mediaObject.Original.MimeType, mediaObject.GalleryId)
              .Select(encoderSetting => !String.IsNullOrWhiteSpace(encoderSetting.EncoderArguments))
              .FirstOrDefault();
        }

        /// <summary>
        /// Triggers the <see cref="MediaQueueItemStatusDetailAppended" /> event.
        /// </summary>
        /// <param name="mediaQueueItem">The media queue item.</param>
        /// <param name="statusDetailAppended">A string representing additional status detail information that has been added to the
        /// <see cref="MediaQueueItem.StatusDetail" /> property.</param>
        public void TriggerMediaQueueItemStatusDetailAppendedEvent(MediaQueueItem mediaQueueItem, string statusDetailAppended)
        {
            MediaQueueItemStatusDetailAppended?.Invoke(null, new MediaConversionQueueEventArgs(mediaQueueItem, _instance.Value.Status, statusDetailAppended));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the specified media object undergoing the specified <paramref name="conversionType" /> 
        /// is currently waiting to be processed in the media queue.
        /// </summary>
        /// <param name="mediaObjectId">The media object ID.</param>
        /// <param name="conversionType">Type of the conversion. If the parameter omitted, then a matching 
        /// media object having any conversion type will cause the method to return <c>true</c>.</param>
        /// <returns>Returns <c>true</c> if the media object is currently being processed by the media queue;
        /// otherwise, <c>false</c>.</returns>
        private bool IsWaitingInQueue(int mediaObjectId, MediaQueueItemConversionType conversionType = MediaQueueItemConversionType.Unknown)
        {
            return MediaQueueItemDictionary.Any(mq =>
              mq.Value.MediaObjectId == mediaObjectId &&
              mq.Value.Status == MediaQueueItemStatus.Waiting &&
              (mq.Value.ConversionType == conversionType || conversionType == MediaQueueItemConversionType.Unknown)
              );
        }

        /// <summary>
        /// Processes the current media queue item. This can be a long running process and is 
        /// intended to be invoked on a background thread.
        /// </summary>
        private void ProcessItem()
        {
            try
            {
                if (!BeginProcessItem())
                    return;

                MediaConversionSettings conversionResults = ExecuteMediaConversion();

                OnMediaConversionComplete(conversionResults);
            }
            catch (Exception ex)
            {
                // I know it's bad form to catch all exceptions, but I don't know how to catch all
                // non-fatal exceptions (like ArgumentNullException) while letting the catastrophic
                // ones go through (like StackOverFlowException) unless we explictly catch and then
                // rethrow them, but that seems like it could have its own issues.
                Events.EventController.RecordError(ex, AppSetting.Instance, null, Factory.LoadGallerySettings());
            }
        }

        /// <summary>
        /// Executes the actual media conversion, returning an object that contains settings and the 
        /// results of the conversion. Returns null if the media object has been deleted since it was
        /// first put in the queue.
        /// </summary>
        /// <returns>Returns an instance of <see cref="MediaConversionSettings" /> containing settings and
        /// results used in the conversion, or null if the media object no longer exists.</returns>
        private MediaConversionSettings ExecuteMediaConversion()
        {
            IGalleryObject mediaObject;
            try
            {
                var queueItem = GetCurrentMediaQueueItem();
                mediaObject = Factory.LoadMediaObjectInstance(new MediaLoadOptions(queueItem.MediaObjectId) { IsWritable = true });
                mediaObject.RotateFlip = queueItem.RotateFlipAmount;
            }
            catch (InvalidMediaObjectException)
            {
                return null;
            }

            return ExecuteMediaConversion(mediaObject, GetEncoderSetting(mediaObject));
        }

        /// <summary>
        /// Executes the actual media conversion, returning an object that contains settings and the
        /// results of the conversion.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        /// <param name="encoderSetting">The encoder setting that defines the conversion parameters.</param>
        /// <returns>
        /// Returns an instance of <see cref="MediaConversionSettings"/> containing settings and
        /// results used in the conversion.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> or
        /// <paramref name="encoderSetting" /> is null.</exception>
        private MediaConversionSettings ExecuteMediaConversion(IGalleryObject mediaObject, IMediaEncoderSettings encoderSetting)
        {
            if (mediaObject == null)
                throw new ArgumentNullException("mediaObject");

            if (encoderSetting == null)
                throw new ArgumentNullException("encoderSetting");

            var mqi = GetCurrentMediaQueueItem();

            switch (mqi.ConversionType)
            {
                case MediaQueueItemConversionType.CreateOptimized:
                    return CreateOptimizedMediaObject(mediaObject, encoderSetting);

                case MediaQueueItemConversionType.RotateVideo:
                    return RotateVideo(mediaObject);

                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException(string.Format("MediaConversionQueue.ExecuteMediaConversion is not designed to handled the enumeration value {0}. It must be updated.", mqi.ConversionType));
            }
        }

        private MediaConversionSettings RotateVideo(IGalleryObject mediaObject)
        {
            var gallerySetting = Factory.LoadGallerySetting(mediaObject.GalleryId);

            // Determine file name and path of the new file.
            var dirName = Path.GetDirectoryName(mediaObject.Original.FileNamePhysicalPath) ?? String.Empty;
            var newFilename = HelperFunctions.ValidateFileName(dirName, mediaObject.Original.FileName);
            var newFilePath = Path.Combine(dirName, newFilename);

            const string args = @"-i ""{SourceFilePath}"" -vf ""{AutoRotateFilter}"" -q:a 0 -q:v 0 -acodec copy -metadata:s:v:0 rotate=0 ""{DestinationFilePath}""";
            var encoderSetting = new MediaEncoderSettings(Path.GetExtension(newFilename), Path.GetExtension(mediaObject.Original.FileName), args, 0);

            var mediaSettings = new MediaConversionSettings
            {
                FilePathSource = mediaObject.Original.FileNamePhysicalPath,
                FilePathDestination = newFilePath,
                EncoderSetting = encoderSetting,
                GalleryId = mediaObject.GalleryId,
                MediaQueueId = _currentMediaQueueItemId,
                TimeoutMs = gallerySetting.MediaEncoderTimeoutMs,
                MediaObjectId = mediaObject.Id,
                TargetWidth = 0,
                TargetHeight = 0,
                FFmpegArgs = String.Empty,
                FFmpegOutput = String.Empty,
                CancellationToken = CancelTokenSource.Token
            };

            // Trigger the updated event
            var mqItem = GetCurrentMediaQueueItem();
            mqItem.NewFilename = newFilename;
            ActiveMediaQueueItemUpdated?.Invoke(null, new MediaConversionQueueEventArgs(mqItem, _instance.Value.Status, null));

            mediaSettings.FFmpegOutput = FFmpeg.CreateMedia(mediaSettings);
            mediaSettings.FileCreated = ValidateFile(mediaSettings.FilePathDestination);

            return mediaSettings;
        }

        private MediaConversionSettings CreateOptimizedMediaObject(IGalleryObject mediaObject, IMediaEncoderSettings encoderSetting)
        {
            AttemptedEncoderSettings.Add(encoderSetting);

            IGallerySettings gallerySetting = Factory.LoadGallerySetting(mediaObject.GalleryId);

            // Determine file name and path of the new file.
            string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(mediaObject.Original.FileInfo.DirectoryName, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaObject.Original.FileInfo.Name);
            string newFilename = GenerateNewFilename(optimizedPath, fileNameWithoutExtension, encoderSetting.DestinationFileExtension, gallerySetting.OptimizedFileNamePrefix);
            string newFilePath = Path.Combine(optimizedPath, newFilename);

            // Trigger the updated event
            var mqItem = GetCurrentMediaQueueItem();
            mqItem.NewFilename = newFilename;
            ActiveMediaQueueItemUpdated?.Invoke(null, new MediaConversionQueueEventArgs(mqItem, _instance.Value.Status, null));

            var mediaSettings = new MediaConversionSettings
            {
                FilePathSource = mediaObject.Original.FileNamePhysicalPath,
                FilePathDestination = newFilePath,
                EncoderSetting = encoderSetting,
                GalleryId = mediaObject.GalleryId,
                MediaQueueId = _currentMediaQueueItemId,
                TimeoutMs = gallerySetting.MediaEncoderTimeoutMs,
                MediaObjectId = mediaObject.Id,
                TargetWidth = GetTargetWidth(mediaObject, gallerySetting, encoderSetting),
                TargetHeight = GetTargetHeight(mediaObject, gallerySetting, encoderSetting),
                FFmpegArgs = String.Empty,
                FFmpegOutput = String.Empty,
                CancellationToken = CancelTokenSource.Token
            };

            mediaSettings.FFmpegOutput = FFmpeg.CreateMedia(mediaSettings);
            mediaSettings.FileCreated = ValidateFile(mediaSettings.FilePathDestination);

            if (!mediaSettings.FileCreated && !mediaSettings.CancellationToken.IsCancellationRequested)
            {
                // Could not create the requested version of the file. Record the event, then try again,
                // using the next encoder setting (if one exists).
                string msg = String.Format(CultureInfo.CurrentCulture, "FAILURE: FFmpeg was not able to create file '{0}'.", Path.GetFileName(mediaSettings.FilePathDestination));
                RecordEvent(msg, mediaSettings);

                IMediaEncoderSettings nextEncoderSetting = GetEncoderSetting(mediaObject);
                if (nextEncoderSetting != null)
                {
                    return ExecuteMediaConversion(mediaObject, nextEncoderSetting);
                }
            }

            return mediaSettings;
        }

        /// <summary>
        /// Gets the encoder setting to use for processing the <paramref name="mediaObject" />.
        /// If more than one encoder setting is applicable, this function automatically returns 
        /// the first item that has not yet been tried. If no items are applicable, returns
        /// null.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        /// <returns>An instance of <see cref="IMediaEncoderSettings" /> or null.</returns>
        private IMediaEncoderSettings GetEncoderSetting(IGalleryObject mediaObject)
        {
            var encoderSettings = GetEncoderSettings(mediaObject.Original.MimeType, mediaObject.GalleryId);

            return encoderSettings.FirstOrDefault(encoderSetting => AttemptedEncoderSettings.All(es => es.Sequence != encoderSetting.Sequence));
        }

        private static IOrderedEnumerable<IMediaEncoderSettings> GetEncoderSettings(IMimeType mimeType, int galleryId)
        {
            return Factory.LoadGallerySetting(galleryId).MediaEncoderSettings
              .Where(es => (
                            (es.SourceFileExtension == mimeType.Extension) ||
                            (es.SourceFileExtension == String.Concat("*", mimeType.MajorType))))
              .OrderBy(es => es.Sequence);
        }

        /// <summary>
        /// Performs post-processing tasks on the media object and media queue items. Specifically, 
        /// if the file was successfully created, updates the media object instance with information 
        /// about the new file. Updates the media queue instance and resets the status of the 
        /// conversion queue.
        /// </summary>
        /// <param name="settings">An instance of <see cref="MediaConversionSettings" /> containing
        /// settings and results used in the conversion. May be null.</param>
        private void OnMediaConversionComplete(MediaConversionSettings settings)
        {
            try
            {
                switch (GetCurrentMediaQueueItem().ConversionType)
                {
                    case MediaQueueItemConversionType.CreateOptimized:
                        OnMediaConversionCompleteOptimizedCreated(settings);
                        break;
                    case MediaQueueItemConversionType.RotateVideo:
                        OnMediaConversionCompleteVideoRotated(settings);
                        break;
                }
            }
            finally
            {
                CacheController.RemoveMediaAssetFromCache(settings.MediaObjectId);
                CacheController.RemoveInflatedAlbumsFromCache();
                CacheController.RemoveTagsFromCache();

                CompleteProcessItem(settings);
            }
        }

        /// <summary>
        /// Performs post-processing tasks on the media object after an optimized file has been created. Specifically, 
        /// if the file was successfully created, update the media object instance with information 
        /// about the new file. No action is taken if <paramref name="settings" /> is null.
        /// </summary>
        /// <param name="settings">An instance of <see cref="MediaConversionSettings" /> containing
        /// settings and results used in the conversion. May be null.</param>
        private static void OnMediaConversionCompleteOptimizedCreated(MediaConversionSettings settings)
        {
            if (settings == null)
                return;

            var mediaObject = Factory.LoadMediaObjectInstance(new MediaLoadOptions(settings.MediaObjectId) { IsWritable = true });

            // Step 1: Update the media object with info about the newly created file.
            if (settings.FileCreated)
            {
                string msg = String.Format(CultureInfo.CurrentCulture, "FFmpeg created file '{0}'.", Path.GetFileName(settings.FilePathDestination));
                RecordEvent(msg, settings);

                if (mediaObject.GalleryObjectType == GalleryObjectType.Video)
                {
                    var width = FFmpeg.ParseOutputVideoWidth(settings.FFmpegOutput);
                    var height = FFmpeg.ParseOutputVideoHeight(settings.FFmpegOutput);

                    if (width > int.MinValue)
                        mediaObject.Optimized.Width = width;

                    if (height > int.MinValue)
                        mediaObject.Optimized.Height = height;
                }
                else
                {
                    mediaObject.Optimized.Width = settings.TargetWidth;
                    mediaObject.Optimized.Height = settings.TargetHeight;
                }

                // Step 2: If we already had an optimized file and we just created a second one, delete the first one
                // and rename the new one to match the first one.
                var optFileDifferentThanOriginal = !String.Equals(mediaObject.Optimized.FileName, mediaObject.Original.FileName, StringComparison.InvariantCultureIgnoreCase);
                var optFileDifferentThanCreatedFile = !String.Equals(mediaObject.Optimized.FileName, Path.GetFileName(settings.FilePathDestination), StringComparison.InvariantCultureIgnoreCase);

                if (optFileDifferentThanOriginal && optFileDifferentThanCreatedFile && File.Exists(mediaObject.Optimized.FileNamePhysicalPath))
                {
                    var curFilePath = mediaObject.Optimized.FileNamePhysicalPath;
                    File.Delete(curFilePath);

                    var optFileExtDifferentThanCreatedFileExt = !Path.GetExtension(curFilePath).Equals(Path.GetExtension(settings.FilePathDestination), StringComparison.InvariantCultureIgnoreCase);
                    if (optFileExtDifferentThanCreatedFileExt)
                    {
                        // Extension of created file is different than current optimized file. This can happen, for example, when syncing after
                        // changing encoder settings to produce MP4's instead of FLV's. Use the filename of the current optimized file and combine
                        // it with the extension of the created file.
                        var newOptFileName = String.Concat(Path.GetFileNameWithoutExtension(curFilePath), Path.GetExtension(settings.FilePathDestination));
                        var newOptFilePath = String.Concat(Path.GetDirectoryName(curFilePath), Path.DirectorySeparatorChar, newOptFileName);

                        if (!settings.FilePathDestination.Equals(newOptFilePath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Calculated file name differs from the one that was generated, so rename it, deleting any existing file first.
                            HelperFunctions.MoveFileSafely(settings.FilePathDestination, newOptFilePath);
                            settings.FilePathDestination = newOptFilePath;
                        }

                        mediaObject.Optimized.FileName = newOptFileName;
                        mediaObject.Optimized.FileNamePhysicalPath = newOptFilePath;
                    }
                    else
                    {
                        HelperFunctions.MoveFileSafely(settings.FilePathDestination, curFilePath);
                        settings.FilePathDestination = curFilePath;
                    }
                }
                else
                {
                    // We typically get here when the media object is first added.
                    mediaObject.Optimized.FileName = Path.GetFileName(settings.FilePathDestination);
                    mediaObject.Optimized.FileNamePhysicalPath = settings.FilePathDestination;
                }

                // Now that we have the optimized file name all set, grab it's size.
                int fileSize = (int)(mediaObject.Optimized.FileInfo.Length / 1024);
                mediaObject.Optimized.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
            }

            // Step 3: Save and finish up.
            mediaObject.LastModifiedByUserName = GlobalConstants.SystemUserName;
            mediaObject.DateLastModified = DateTime.UtcNow;
            mediaObject.Save();
        }

        /// <summary>
        /// Performs post-processing tasks on the media object after a video has been rotated. Specifically, 
        /// if the file was successfully created, update the media object instance with information 
        /// about the new file. No action is taken if <paramref name="settings" /> is null.
        /// </summary>
        /// <param name="settings">An instance of <see cref="MediaConversionSettings" /> containing
        /// settings and results used in the conversion. May be null.</param>
        /// <remarks>This function is invoked only when a video is manually rotated by the user, and 
        /// only for the original video file. Videos that are auto-rotated will be the optimized ones
        /// and will end up running the <see cref="OnMediaConversionCompleteOptimizedCreated(MediaConversionSettings)" />
        /// function instead of this one.</remarks>
        private static void OnMediaConversionCompleteVideoRotated(MediaConversionSettings settings)
        {
            if (settings == null)
                return;

            var mediaObject = Factory.LoadMediaObjectInstance(new MediaLoadOptions(settings.MediaObjectId) { IsWritable = true });

            if (settings.FileCreated)
            {
                string msg = String.Format(CultureInfo.CurrentCulture, "FFmpeg created file '{0}'.", Path.GetFileName(settings.FilePathDestination));
                RecordEvent(msg, settings);

                // Step 1: Update the width and height of the original video file, if we have that info.
                var originalWidth = FFmpeg.ParseOutputVideoWidth(settings.FFmpegOutput);
                var originalHeight = FFmpeg.ParseOutputVideoHeight(settings.FFmpegOutput);

                if (originalWidth > int.MinValue)
                    mediaObject.Original.Width = originalWidth;

                if (originalHeight > int.MinValue)
                    mediaObject.Original.Height = originalHeight;

                // Step 2: Delete the original file and rename the new one to match the original.
                if ((settings.FilePathDestination != mediaObject.Original.FileNamePhysicalPath) && File.Exists(mediaObject.Original.FileNamePhysicalPath))
                {
                    var curFilePath = mediaObject.Original.FileNamePhysicalPath;
                    HelperFunctions.MoveFileSafely(settings.FilePathDestination, curFilePath);
                    settings.FilePathDestination = curFilePath;
                }
                else
                {
                    // I don't expect we'll ever get here, but just to be safe...
                    mediaObject.Original.FileName = Path.GetFileName(settings.FilePathDestination);
                    mediaObject.Original.FileNamePhysicalPath = settings.FilePathDestination;
                }

                int fileSize = (int)(mediaObject.Original.FileInfo.Length / 1024);
                mediaObject.Original.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.

                RefreshOriginalVideoMetadata(mediaObject);
            }

            // Step 3: Save and finish up.
            mediaObject.LastModifiedByUserName = GlobalConstants.SystemUserName;
            mediaObject.DateLastModified = DateTime.UtcNow;
            mediaObject.RegenerateOptimizedOnSave = true;
            mediaObject.RegenerateThumbnailOnSave = true;
            mediaObject.Save();
        }

        /// <summary>
        /// Complete processing the current media item by updating the media queue instance and 
        /// reseting the status of the conversion queue.
        /// </summary>
        /// <param name="settings">An instance of <see cref="MediaConversionSettings" /> containing 
        /// settings and results used in the conversion. A null value is acceptable.</param>
        private void CompleteProcessItem(MediaConversionSettings settings)
        {
            // Update status and persist to data store
            MediaQueueItem mqItem = GetCurrentMediaQueueItem();

            mqItem.DateConversionCompleted = DateTime.UtcNow;

            if (settings != null)
            {
                if (settings.FileCreated)
                {
                    mqItem.Status = MediaQueueItemStatus.Complete;
                }
                else if (settings.CancellationToken.IsCancellationRequested)
                {
                    mqItem.Status = MediaQueueItemStatus.Canceled;

                    var fileName = (settings != null && !String.IsNullOrEmpty(settings.FilePathSource) ? Path.GetFileName(settings.FilePathSource) : "<Unknown>");
                    string msg = String.Format(CultureInfo.CurrentCulture, "Administrator canceled the processing of '{0}'.", fileName);
                    RecordEvent(msg, settings);
                }
                else
                {
                    mqItem.Status = MediaQueueItemStatus.Error;

                    var fileName = (settings != null && !String.IsNullOrEmpty(settings.FilePathSource) ? Path.GetFileName(settings.FilePathSource) : "<Unknown>");
                    string msg = String.Format(CultureInfo.CurrentCulture, "Unable to process file '{0}'.", fileName);
                    RecordEvent(msg, settings);
                }
            }

            //Factory.GetDataProvider().MediaQueue_Save(mediaQueueDto);
            mqItem.Save();

            // Update the item in the collection.
            //MediaQueueItems[mediaQueueDto.MediaQueueId] = mediaQueueDto;

            Reset();

            MediaQueueItemCompleted?.Invoke(null, new MediaConversionQueueEventArgs(mqItem, MediaQueueStatus.Unknown));
        }

        /// <summary>
        /// Begins processing the current media item, returning <c>true</c> when the action succeeds. 
        /// Specifically, a few properties are updated and the item is persisted to the data store.
        /// If the item cannot be processed (may be null or has a status other than 'Waiting'), this
        /// function returns <c>false</c>.
        /// </summary>
        /// <returns>Returns <c>true</c> when the item has successfully started processing; otherwise 
        /// <c>false</c>.</returns>
        private bool BeginProcessItem()
        {
            MediaQueueItem mqItem = GetCurrentMediaQueueItem();

            if (mqItem == null)
                return false;

            if (!mqItem.Status.Equals(MediaQueueItemStatus.Waiting))
            {
                return false;
            }

            mqItem.Status = MediaQueueItemStatus.Processing;
            mqItem.DateConversionStarted = DateTime.UtcNow;

            MediaQueueItemStarted?.Invoke(null, new MediaConversionQueueEventArgs(mqItem, MediaQueueStatus.Unknown));

            mqItem.Save();

            return true;
        }

        /// <summary>
        /// Determine name of new file and ensure it is unique in the directory.
        /// </summary>
        /// <param name="dirPath">The path to the directory where the file is to be created.</param>
        /// <param name="fileNameWithoutExtension">The file name without extension.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <param name="filenamePrefix">A string to prepend to the filename. Example: "zThumb_"</param>
        /// <returns>
        /// Returns the name of the new file name and ensures it is unique in the directory.
        /// </returns>
        private static string GenerateNewFilename(string dirPath, string fileNameWithoutExtension, string fileExtension, string filenamePrefix)
        {
            string optimizedFilename = String.Concat(filenamePrefix, fileNameWithoutExtension, fileExtension);

            optimizedFilename = HelperFunctions.ValidateFileName(dirPath, optimizedFilename);

            return optimizedFilename;
        }

        /// <summary>
        /// Gets the next item in the queue with a status of <see cref="MediaQueueItemStatus.Waiting" />,
        /// returning null if the queue is empty or no eligible items exist.
        /// </summary>
        /// <returns>Returns an instance of <see cref="MediaQueueItem" />, or null.</returns>
        private MediaQueueItem GetNextItemInQueue()
        {
            return MediaQueueItemDictionary.Values.OrderBy(q => q.DateAdded).FirstOrDefault(m => m.Status == MediaQueueItemStatus.Waiting);
        }

        /// <summary>
        /// Validate the specified file, returning <c>true</c> if it exists and has a non-zero length;
        /// otherwise returning <c>false</c>. If the file exists but the length is zero, it is deleted.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>Returns <c>true</c> if <paramref name="filePath" /> exists and has a non-zero length;
        /// otherwise returns <c>false</c>.</returns>
        private static bool ValidateFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);

                if (fi.Length > 0)
                    return true;
                else
                {
                    fi.Delete();
                    return false;
                }
            }
            else
                return false;
        }

        private static void RecordEvent(string msg, MediaConversionSettings settings)
        {
            int? galleryId = null;
            Dictionary<string, string> data = null;

            if (settings != null)
            {
                galleryId = settings.GalleryId;

                data = new Dictionary<string, string>
               {
                 {"FFmpeg args", settings.FFmpegArgs},
                 {"FFmpeg output", settings.FFmpegOutput},
                 {"StackTrace", Environment.StackTrace}
               };
            }

            Events.EventController.RecordEvent(msg, EventType.Info, galleryId, Factory.LoadGallerySettings(), AppSetting.Instance, data);
        }

        /// <summary>
        /// Update settings to prepare for the conversion of a media item.
        /// </summary>
        private void Reset()
        {
            _currentMediaQueueItemId = int.MinValue;
            AttemptedEncoderSettings.Clear();

            // Update the status of any 'Processing' items to 'Waiting'. This is needed to reset any items that 
            // were being processed but were never finished (this can happen if the app pool recycles).
            foreach (var item in MediaQueueItemDictionary.Where(m => m.Value.Status == MediaQueueItemStatus.Processing).Select(m => m.Value))
            {
                ChangeStatus(item, MediaQueueItemStatus.Waiting);
            }
        }

        /// <summary>
        /// Update the status of the <paramref name="item" /> to the specified <paramref name="status" />.
        /// </summary>
        /// <param name="item">The item whose status is to be updated.</param>
        /// <param name="status">The status to update the item to.</param>
        private static void ChangeStatus(MediaQueueItem item, MediaQueueItemStatus status)
        {
            item.Status = status;
            item.Save();
            //Factory.GetDataProvider().MediaQueue_Save(item);
        }

        /// <summary>
        /// Gets the target width for the optimized version of the <paramref name="mediaObject"/>. This value is applied to 
        /// the {Width} replacement parameter in the encoder settings, if present. The first matching rule is returned:
        /// 1. The <paramref name="mediaObject" /> has a width meta value.
        /// 2. The width of the original file (videos only).
        /// 3. The default value for the media type (e.g. <see cref="IGallerySettings.DefaultVideoPlayerWidth" /> for video and
        /// <see cref="IGallerySettings.DefaultAudioPlayerWidth" /> for audio).
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        /// <param name="gallerySettings">The gallery settings.</param>
        /// <param name="encoderSetting">An instance of <see cref="IMediaEncoderSettings" />.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when the <paramref name="mediaObject" /> is not a
        /// video, audio, or generic item.</exception>
        private static int GetTargetWidth(IGalleryObject mediaObject, IGallerySettings gallerySettings, IMediaEncoderSettings encoderSetting)
        {
            IGalleryObjectMetadataItem miWidth;
            if (mediaObject.MetadataItems.TryGetMetadataItem(MetadataItemName.Width, out miWidth))
            {
                return HelperFunctions.ParseInteger(miWidth.Value);
            }

            switch (mediaObject.GalleryObjectType)
            {
                case GalleryObjectType.Video:
                    var width = FFmpeg.ParseSourceVideoWidth(FFmpeg.GetOutput(mediaObject.Original.FileNamePhysicalPath, mediaObject.GalleryId));

                    return width > int.MinValue ? width : gallerySettings.DefaultVideoPlayerWidth;

                case GalleryObjectType.Audio:
                    return gallerySettings.DefaultAudioPlayerWidth;

                case GalleryObjectType.Generic: // Should never hit this because we don't encode generic objects, but for completeness let's put it in
                    return gallerySettings.DefaultGenericObjectWidth;

                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException(String.Format("MediaConversionQueue.GetTargetWidth was not designed to handle the enum value {0}. The function must be updated.", mediaObject.GalleryObjectType));
            }
        }

        /// <summary>
        /// Gets the target height for the optimized version of the <paramref name="mediaObject"/>. This value is applied to 
        /// the {Height} replacement parameter in the encoder settings, if present. The first matching rule is returned:
        /// 1. The <paramref name="mediaObject" /> has a height meta value.
        /// 2. The height of the original file (videos only).
        /// 3. The default value for the media type (e.g. <see cref="IGallerySettings.DefaultVideoPlayerHeight" /> for video and
        /// cref="IGallerySettings.DefaultAudioPlayerHeight" /> for audio).
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        /// <param name="gallerySettings">The gallery settings.</param>
        /// <param name="encoderSetting">An instance of <see cref="IMediaEncoderSettings" />.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown when the <paramref name="mediaObject" /> is not a
        /// video, audio, or generic item.</exception>
        private static int GetTargetHeight(IGalleryObject mediaObject, IGallerySettings gallerySettings, IMediaEncoderSettings encoderSetting)
        {
            IGalleryObjectMetadataItem miHeight;
            if (mediaObject.MetadataItems.TryGetMetadataItem(MetadataItemName.Height, out miHeight))
            {
                return HelperFunctions.ParseInteger(miHeight.Value);
            }

            switch (mediaObject.GalleryObjectType)
            {
                case GalleryObjectType.Video:
                    var height = FFmpeg.ParseSourceVideoHeight(FFmpeg.GetOutput(mediaObject.Original.FileNamePhysicalPath, mediaObject.GalleryId));

                    return height > int.MinValue ? height : mediaObject.Original.Height;

                case GalleryObjectType.Audio:
                    return gallerySettings.DefaultAudioPlayerHeight;

                case GalleryObjectType.Generic: // Should never hit this because we don't encode generic objects, but for completeness let's put it in
                    return gallerySettings.DefaultGenericObjectHeight;

                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException(String.Format("MediaConversionQueue.GetTargetHeight was not designed to handle the enum value {0}. The function must be updated.", mediaObject.GalleryObjectType));
            }
        }

        /// <summary>
        /// Re-extract several metadata values from the file. Call this function when performing an action on a file
        /// that may render existing metadata items inaccurate, such as width and height. The new values are not persisted;
        /// it is expected a subsequent function will do that.
        /// </summary>
        private static void RefreshOriginalVideoMetadata(IGalleryObject mediaObject)
        {
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.Width));
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.Height));
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.VideoFormat));
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.BitRate));
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.VideoBitRate));
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.Dimensions));
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.FileSizeKb));
            mediaObject.ExtractMetadata(mediaObject.MetaDefinitions.Find(MetadataItemName.Orientation));
        }

        #endregion
    }

    /// <summary>
    /// Provides data for the events relating to <see cref="MediaConversionQueue" />.
    /// </summary>
    public class MediaConversionQueueEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaConversionQueueEventArgs" /> class.
        /// </summary>
        /// <param name="mediaQueueItem">The media queue item.</param>
        /// <param name="queueStatus">The status of the queue processor.</param>
        /// <param name="statusDetailAppended">A string representing additional status detail information that has been added to the
        ///   <see cref="Business.MediaQueueItem.StatusDetail" /> property. Optional.</param>
        public MediaConversionQueueEventArgs(MediaQueueItem mediaQueueItem, MediaQueueStatus queueStatus, string statusDetailAppended = null)
        {
            MediaQueueItem = mediaQueueItem;
            StatusDetailAppended = statusDetailAppended;
            QueueStatus = queueStatus;
        }

        /// <summary>
        /// Gets the media queue item.
        /// </summary>
        public MediaQueueItem MediaQueueItem { get; private set; }

        /// <summary>
        /// Gets a string representing additional status detail information that has been added to the
        /// <see cref="Business.MediaQueueItem.StatusDetail" /> property. This property is only set for the 
        /// <see cref="MediaConversionQueue.MediaQueueItemStatusDetailAppended" /> event; otherwise will be null.
        /// </summary>
        public string StatusDetailAppended { get; private set; }

        /// <summary>
        /// Gets the status of the queue processor.
        /// </summary>
        public MediaQueueStatus QueueStatus { get; private set; }
    }
}
