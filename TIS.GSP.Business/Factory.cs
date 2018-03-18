using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.NullObjects;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Contains functionality for creating and retrieving various business objects. Use methods in this class instead of instantiating
    /// certain objects directly. This includes instances of <see cref="Image" />, <see cref="Video" />, <see cref="Audio" />, 
    /// <see cref="GenericMediaObject" />, and <see cref="Album" />.
    /// </summary>
    public class Factory
    {
        #region Private Fields

        private static readonly object _sharedLock = new object();
        private static readonly ConcurrentDictionary<int, ISynchronizationStatus> _syncStatuses = new ConcurrentDictionary<int, ISynchronizationStatus>();
        private static int? _templateGalleryId;
        private static bool _galleriesLoaded;
        private static readonly IGalleryCollection _galleries = new GalleryCollection();
        private static readonly ConcurrentDictionary<int, Watermark> _watermarks = new ConcurrentDictionary<int, Watermark>();
        private static readonly IGallerySettingsCollection _gallerySettings = new GallerySettingsCollection();

        private static readonly IGalleryControlSettingsCollection _galleryControlSettings = new GalleryControlSettingsCollection();

        #endregion

        #region Gallery Object Methods

        /// <overloads>Create a fully inflated, properly typed gallery object instance based on the specified parameters.</overloads>
        /// <summary>
        /// Create a fully inflated, properly typed instance based on the specified <see cref="IGalleryObject.Id">ID</see>. An 
        /// additional call to the data store is made to determine the object's type. When you know the type you want (<see cref="Album" />,
        /// <see cref="Image" />, etc), use the overload that takes the galleryObjectType parameter, or call the specific Factory method that 
        /// loads the desired type, as those are more efficient. This method is guaranteed to not return null. If no object is found
        /// that matches the ID, an <see cref="UnsupportedMediaObjectTypeException" /> exception is thrown. If both a media object and an 
        /// album exist with the <paramref name = "id" />, the media object reference is returned.
        /// </summary>
        /// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object or album to retrieve from the
        /// data store.</param>
        /// <returns>Returns an <see cref="IGalleryObject" /> object for the <see cref="IGalleryObject.Id">ID</see>. This method is guaranteed to not
        /// return null.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when no media object with the specified <see cref="IGalleryObject.Id">ID</see> 
        /// is found in the data store.</exception>
        public static IGalleryObject LoadGalleryObjectInstance(int id)
        {
            // Figure out what type the ID refers to (album, image, video, etc) and then call the overload of this method.
            return LoadGalleryObjectInstance(id, HelperFunctions.DetermineGalleryObjectType(id));
        }

        /// <summary>
        /// Create a fully inflated, properly typed instance based on the specified parameters. If the galleryObjectType
        /// parameter is All, None, or Unknown, then an additional call to the data store is made
        /// to determine the object's type. If no object is found that matches the ID and gallery object type, an 
        /// <see cref="UnsupportedMediaObjectTypeException" /> exception is thrown. When you know the type you want (<see cref="Album" />,
        /// <see cref="Image" />, etc), specify the exact galleryObjectType, or call the specific Factory method that 
        /// loads the desired type, as that is more efficient. This method is guaranteed to not return null.
        /// </summary>
        /// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object or album to retrieve from the
        /// data store.</param>
        /// <param name="galleryObjectType">The type of gallery object that the id parameter represents. If the type is 
        /// unknown, the Unknown enum value can be specified. Specify the actual type if possible (e.g. Video, Audio, Image, 
        /// etc.), as it is more efficient.</param>
        /// <returns>Returns an <see cref="IGalleryObject" /> based on the ID. This method is guaranteed to not return null.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when a particular media object type is requested (e.g. Image, Video, etc.), 
        /// but no media object with the specified ID is found in the data store.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when an album is requested but no album with the specified ID is found in the data store.</exception>
        public static IGalleryObject LoadGalleryObjectInstance(int id, GalleryObjectType galleryObjectType)
        {
            // If the gallery object type is vague, we need to figure it out.
            if ((galleryObjectType == GalleryObjectType.All) || (galleryObjectType == GalleryObjectType.NotSpecified) || (galleryObjectType == GalleryObjectType.Unknown))
            {
                galleryObjectType = HelperFunctions.DetermineGalleryObjectType(id);
            }

            IGalleryObject go;

            switch (galleryObjectType)
            {
                case GalleryObjectType.Album:
                    {
                        go = LoadAlbumInstance(id);
                        break;
                    }
                case GalleryObjectType.Image:
                case GalleryObjectType.Video:
                case GalleryObjectType.Audio:
                case GalleryObjectType.Generic:
                case GalleryObjectType.Unknown:
                    {
                        go = LoadMediaObjectInstance(id);
                        break;
                    }
                default:
                    {
                        throw new UnsupportedMediaObjectTypeException();
                    }
            }

            return go;
        }

        #endregion

        #region Media Object Methods

        #region General Media Object Methods

        /// <overloads>
        /// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) from the specified parameters.
        /// </overloads>
        /// <summary>
        /// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) for the media file
        /// represented by <paramref name = "mediaObjectFilePath" /> and belonging to the album specified by <paramref name = "parentAlbum" />.
        /// </summary>
        /// <param name="mediaObjectFilePath">The fully qualified name of the media object file, or the relative filename.
        /// The file must already exist in the album's directory. If the file has a matching record in the data store,
        /// a reference to the existing object is returned. Otherwise, a new instance is returned. For new instances,
        /// call <see cref="IGalleryObject.Save" /> to persist the object to the data store. A
        /// <see cref="UnsupportedMediaObjectTypeException" /> is thrown when the specified file cannot 
        /// be added to Gallery Server, perhaps because it is an unsupported type or the file is corrupt.</param>
        /// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFilePath" /> has a file 
        /// extension that Gallery Server is configured to reject.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the  
        /// mediaObjectFilePath parameter refers to a file that is not in the same directory as the parent album's directory.</exception>
        public static IGalleryObject CreateMediaObjectInstance(string mediaObjectFilePath, IAlbum parentAlbum)
        {
            return CreateMediaObjectInstance(new FileInfo(mediaObjectFilePath), parentAlbum);
        }

        /// <summary>
        /// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) for the media file
        /// represented by <paramref name = "mediaObjectFile" /> and belonging to the album specified by <paramref name = "parentAlbum" />.
        /// </summary>
        /// <param name="mediaObjectFile">A <see cref="System.IO.FileInfo" /> object representing a supported media object type. The file must already
        /// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
        /// object is returned; otherwise, a new instance is returned. For new instances, call <see cref="IGalleryObject.Save" /> 
        ///		to persist the object to the data store. A <see cref="UnsupportedMediaObjectTypeException" /> is thrown when the specified file cannot 
        /// be added to Gallery Server, perhaps because it is an unsupported type or the file is corrupt.</param>
        /// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFile" /> has a file 
        /// extension that Gallery Server is configured to reject.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when   
        /// <paramref name = "mediaObjectFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name = "parentAlbum" /> is null.</exception>
        /// <remarks>
        /// This method is marked internal to ensure it is not called from the web layer. It was noticed that
        /// calling this method from the web layer caused the file referenced in the mediaObjectFile parameter to remain
        /// locked beyond the conclusion of the page lifecycle, preventing manual deletion using Windows Explorer. Note 
        /// that restarting IIS (iisreset.exe) released the file lock, and presumably the next garbage collection would 
        /// have released it as well. The web page was modified to call the overload of this method that takes the filepath
        /// as a string parameter and then instantiates a <see cref="System.IO.FileInfo" /> object. I am not sure why, 
        /// but instantiating the <see cref="System.IO.FileInfo" /> object within this DLL in this way caused the file 
        /// lock to be released at the end of the page lifecycle.
        /// </remarks>
        internal static IGalleryObject CreateMediaObjectInstance(FileInfo mediaObjectFile, IAlbum parentAlbum)
        {
            return CreateMediaObjectInstance(mediaObjectFile, parentAlbum, String.Empty, MimeTypeCategory.NotSet);
        }

        /// <summary>
        /// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.). If 
        /// <paramref name = "externalHtmlSource" /> is specified, then an <see cref="ExternalMediaObject" /> is created with the
        /// specified <paramref name = "mimeTypeCategory" />; otherwise a new instance is created based on <paramref name = "mediaObjectFile" />,
        /// where the exact type (e.g. <see cref="Image" />, <see cref="Video" />, etc.) is determined by the file's extension.
        /// </summary>
        /// <param name="mediaObjectFile">A <see cref="System.IO.FileInfo" /> object representing a supported media object type. The file must already
        /// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
        /// object is returned; otherwise, a new instance is returned. For new instances, call <see cref="IGalleryObject.Save" /> to 
        ///		persist the object to the data store. A <see cref="UnsupportedMediaObjectTypeException" /> is thrown when the specified file cannot 
        /// be added to Gallery Server, perhaps because it is an unsupported type or the file is corrupt. Do not specify this parameter
        /// when using the <paramref name = "externalHtmlSource" /> parameter.</param>
        /// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the data store).</param>
        /// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at 
        /// Silverlight.net or youtube.com. Using this parameter also requires specifying <paramref name = "mimeTypeCategory" />
        /// and passing null for <paramref name = "mediaObjectFile" />.</param>
        /// <param name="mimeTypeCategory">Specifies the category to which an externally stored media object belongs. 
        /// Must be set to a value other than MimeTypeCategory.NotSet when the <paramref name = "externalHtmlSource" /> is specified.</param>
        /// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name = "mediaObjectFile" /> and <paramref name = "externalHtmlSource" />
        /// are either both specified, or neither.</exception>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFile" /> has a file 
        /// extension that Gallery Server is configured to reject.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the  
        /// mediaObjectFile parameter refers to a file that is not in the same directory as the parent album's directory.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name = "parentAlbum" /> is null.</exception>
        /// <remarks>
        /// This method is marked internal to ensure it is not called from the web layer. It was noticed that
        /// calling this method from the web layer caused the file referenced in the mediaObjectFile parameter to remain
        /// locked beyond the conclusion of the page lifecycle, preventing manual deletion using Windows Explorer. Note 
        /// that restarting IIS (iisreset.exe) released the file lock, and presumably the next garbage collection would 
        /// have released it as well. The web page was modified to call the overload of this method that takes the filepath
        /// as a string parameter and then instantiates a FileInfo object. I am not sure why, but instantiating the FileInfo 
        /// object within this DLL in this way caused the file lock to be released at the end of the page lifecycle.
        /// </remarks>
        internal static IGalleryObject CreateMediaObjectInstance(FileInfo mediaObjectFile, IAlbum parentAlbum, string externalHtmlSource, MimeTypeCategory mimeTypeCategory)
        {
            #region Validation

            // Either mediaObjectFile or externalHtmlSource must be specified, but not both.
            if ((mediaObjectFile == null) && (String.IsNullOrEmpty(externalHtmlSource)))
                throw new ArgumentException("The method GalleryServer.Business.Factory.CreateMediaObjectInstance was invoked with invalid parameters. The parameters mediaObjectFile and externalHtmlSource cannot both be null or empty. One of these - but not both - must be populated.");

            if ((mediaObjectFile != null) && (!String.IsNullOrEmpty(externalHtmlSource)))
                throw new ArgumentException("The method GalleryServer.Business.Factory.CreateMediaObjectInstance was invoked with invalid parameters. The parameters mediaObjectFile and externalHtmlSource cannot both be specified.");

            if ((!String.IsNullOrEmpty(externalHtmlSource)) && (mimeTypeCategory == MimeTypeCategory.NotSet))
                throw new ArgumentException("The method GalleryServer.Business.Factory.CreateMediaObjectInstance was invoked with invalid parameters. The parameters mimeTypeCategory must be set to a value other than MimeTypeCategory.NotSet when the externalHtmlSource parameter is specified.");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            #endregion

            if (String.IsNullOrEmpty(externalHtmlSource))
                return CreateLocalMediaObjectInstance(mediaObjectFile, parentAlbum);
            else
                return CreateExternalMediaObjectInstance(externalHtmlSource, mimeTypeCategory, parentAlbum);
        }

        /// <summary>
        /// Create a properly typed Gallery Object instance (e.g. <see cref="Image" />, <see cref="Video" />, etc.) from the specified parameters.
        /// </summary>
        /// <param name="mediaObjectFile">A <see cref="System.IO.FileInfo" /> object representing a supported media object type. The file must already
        /// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
        /// object is returned; otherwise, a new instance is returned. For new instances, call <see cref="IGalleryObject.Save" /> 
        ///		to persist the object to the data store.</param>
        /// <param name="parentAlbum">The album in which the media object exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns a properly typed Gallery Object instance corresponding to the specified parameters.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when <paramref name = "mediaObjectFile" /> has a file 
        /// extension that Gallery Server is configured to reject.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when   
        /// <paramref name = "mediaObjectFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name = "mediaObjectFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
        private static IGalleryObject CreateLocalMediaObjectInstance(FileInfo mediaObjectFile, IAlbum parentAlbum)
        {
            if (mediaObjectFile == null)
                throw new ArgumentNullException("mediaObjectFile");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            IGalleryObject go;

            GalleryObjectType goType = HelperFunctions.DetermineMediaObjectType(mediaObjectFile.Name);

            if (goType == GalleryObjectType.Unknown)
            {
                bool allowUnspecifiedMimeTypes = LoadGallerySetting(parentAlbum.GalleryId).AllowUnspecifiedMimeTypes;
                // If we have an unrecognized media object type (because no MIME type element exists in the configuration
                // file that matches the file extension), then treat the object as a generic media object, but only if
                // the "allowUnspecifiedMimeTypes" configuration setting allows adding unknown media object types.
                // If allowUnspecifiedMimeTypes = false, goType remains "Unknown", and we'll be throwing an 
                // UnsupportedMediaObjectTypeException at the end of this method.
                if (allowUnspecifiedMimeTypes)
                {
                    goType = GalleryObjectType.Generic;
                }
            }

            switch (goType)
            {
                case GalleryObjectType.Image:
                    {
                        try
                        {
                            go = CreateImageInstance(mediaObjectFile, parentAlbum);
                            break;
                        }
                        catch (UnsupportedImageTypeException)
                        {
                            go = CreateGenericObjectInstance(mediaObjectFile, parentAlbum);
                            break;
                        }
                    }
                case GalleryObjectType.Video:
                    {
                        go = CreateVideoInstance(mediaObjectFile, parentAlbum);
                        break;
                    }
                case GalleryObjectType.Audio:
                    {
                        go = CreateAudioInstance(mediaObjectFile, parentAlbum);
                        break;
                    }
                case GalleryObjectType.Generic:
                    {
                        go = CreateGenericObjectInstance(mediaObjectFile, parentAlbum);
                        break;
                    }
                default:
                    {
                        throw new UnsupportedMediaObjectTypeException(mediaObjectFile);
                    }
            }

            return go;
        }

        /// <overloads>
        /// Generate an <see cref="IGalleryObject" /> instance representing an existing media asset.
        /// </overloads>
        /// <summary>
        /// Create a read-only, properly typed media object instance from the specified <paramref name="id" />. If
        /// <paramref name="id" /> is an image, video, audio, etc, then the appropriate object is returned. An
        /// exception is thrown if the <paramref name="id" /> refers to an <see cref="Album" /> (use the 
        /// <see cref="LoadGalleryObjectInstance(int)" /> or <see cref="LoadAlbumInstance(int)" /> method if  the 
        /// <paramref name="id" /> refers to an album). An exception is also thrown if no matching record exists for this 
        /// <paramref name="id" />. Guaranteed to not return null.
        /// </summary>
        /// <param name="id">An integer representing the <see cref="IGalleryObject.Id">ID</see> of the media object to retrieve
        /// from the data store.</param>
        /// <returns>Returns an instance implementing <see cref="IGalleryObject" />.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when no record exists in the data store for the specified
        /// <paramref name="id" />, or when the id parameter refers to an album.</exception>
        public static IGalleryObject LoadMediaObjectInstance(int id)
        {
            return LoadMediaObjectInstance(new MediaLoadOptions(id));
        }

        /// <summary>
        /// Generate an <see cref="IGalleryObject" /> instance conforming to the specified <paramref name="options" />. When options are set to default
        /// values, the media asset is read-only. The media asset may be retrieved from cache. Guaranteed to not return null. 
        /// If <see cref="MediaLoadOptions.MediaId" /> is an image, video, audio, etc, then the appropriate object is returned. An
        /// exception is thrown if the ID refers to an <see cref="Album" /> (use the <see cref="LoadGalleryObjectInstance(int)" /> 
        /// or <see cref="LoadAlbumInstance(int)" /> method if it refers to an album). An exception is also thrown if no matching 
        /// record exists for the ID. Guaranteed to not return null.
        /// </summary>
        /// <param name="options">The options that specify the configuration of the returned media asset.</param>
        /// <returns>An instance implementing <see cref="IGalleryObject" />.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when no record exists in the data store for the specified
        /// <see cref="MediaLoadOptions.MediaId" /> or when the it refers to an album.</exception>
        public static IGalleryObject LoadMediaObjectInstance(MediaLoadOptions options)
        {
            var mediaAssetCache = CacheController.GetMediaAssetCache();

            CacheItemMedia mediaAsset;
            if (mediaAssetCache != null && mediaAssetCache.TryGetValue(options.MediaId, out mediaAsset))
            {
                return GalleryObject.CreateFrom(mediaAsset, options);
            }

            // There is no cache item. Retrieve from data store and create cache item so it's there next time we want it.
            var mediaObject = RetrieveMediaObjectFromDataStore(options.MediaId, options);

            mediaObject.IsWritable = options.IsWritable;

            CacheController.AddToMediaAssetCache(CacheItemMedia.CreateFrom(mediaObject));

            return mediaObject;
        }

        /// <summary>
        /// Create a fully inflated, properly typed, media object instance from the specified <paramref name="moDto" />.
        /// This method is guaranteed to never return null.
        /// </summary>
        /// <param name="moDto">A media object entity. Typically this is generated from a database query.</param>
        /// <param name="parentAlbum">The album containing the media object. Specify null when it is not known, and the
        /// function will automatically generate it.</param>
        /// <returns>Returns a read-only, fully inflated, properly typed media object instance.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException"></exception>
        public static IGalleryObject GetMediaObjectFromDto(MediaObjectDto moDto, IAlbum parentAlbum)
        {
            if (parentAlbum == null)
            {
                parentAlbum = LoadAlbumInstance(moDto.FKAlbumId);
            }

            IGalleryObject mo;

            var goType = HelperFunctions.DetermineMediaObjectType(moDto);

            switch (goType)
            {
                case GalleryObjectType.Image:
                    mo = new Image(
                      moDto.MediaObjectId,
                      parentAlbum,
                      moDto.ThumbnailFilename,
                      moDto.ThumbnailWidth,
                      moDto.ThumbnailHeight,
                      moDto.ThumbnailSizeKB,
                      moDto.OptimizedFilename.Trim(),
                      moDto.OptimizedWidth,
                      moDto.OptimizedHeight,
                      moDto.OptimizedSizeKB,
                      moDto.OriginalFilename.Trim(),
                      moDto.OriginalWidth,
                      moDto.OriginalHeight,
                      moDto.OriginalSizeKB,
                      moDto.Seq,
                      moDto.CreatedBy.Trim(),
                      Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
                      moDto.LastModifiedBy.Trim(),
                      HelperFunctions.ToDateTime(moDto.DateLastModified),
                      moDto.IsPrivate,
                      true,
                      null,
                      moDto.Metadata);
                    break;

                case GalleryObjectType.Video:
                    {
                        mo = new Video(
                          moDto.MediaObjectId,
                          parentAlbum,
                          moDto.ThumbnailFilename,
                          moDto.ThumbnailWidth,
                          moDto.ThumbnailHeight,
                          moDto.ThumbnailSizeKB,
                          moDto.OptimizedFilename.Trim(),
                          moDto.OptimizedWidth,
                          moDto.OptimizedHeight,
                          moDto.OptimizedSizeKB,
                          moDto.OriginalFilename.Trim(),
                          moDto.OriginalWidth,
                          moDto.OriginalHeight,
                          moDto.OriginalSizeKB,
                          moDto.Seq,
                          moDto.CreatedBy.Trim(),
                          Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
                          moDto.LastModifiedBy.Trim(),
                          HelperFunctions.ToDateTime(moDto.DateLastModified),
                          moDto.IsPrivate,
                          true,
                          null,
                          moDto.Metadata);
                        break;
                    }
                case GalleryObjectType.Audio:
                    {
                        mo = new Audio(
                          moDto.MediaObjectId,
                          parentAlbum,
                          moDto.ThumbnailFilename,
                          moDto.ThumbnailWidth,
                          moDto.ThumbnailHeight,
                          moDto.ThumbnailSizeKB,
                          moDto.OptimizedFilename.Trim(),
                          moDto.OptimizedWidth,
                          moDto.OptimizedHeight,
                          moDto.OptimizedSizeKB,
                          moDto.OriginalFilename.Trim(),
                          moDto.OriginalWidth,
                          moDto.OriginalHeight,
                          moDto.OriginalSizeKB,
                          moDto.Seq,
                          moDto.CreatedBy.Trim(),
                          Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
                          moDto.LastModifiedBy.Trim(),
                          HelperFunctions.ToDateTime(moDto.DateLastModified),
                          moDto.IsPrivate,
                          true,
                          null,
                          moDto.Metadata);
                        break;
                    }
                case GalleryObjectType.External:
                    {
                        mo = new ExternalMediaObject(
                          moDto.MediaObjectId,
                          parentAlbum,
                          moDto.ThumbnailFilename,
                          moDto.ThumbnailWidth,
                          moDto.ThumbnailHeight,
                          moDto.ThumbnailSizeKB,
                          moDto.ExternalHtmlSource.Trim(),
                          MimeTypeEnumHelper.ParseMimeTypeCategory(moDto.ExternalType),
                          moDto.Seq,
                          moDto.CreatedBy.Trim(),
                          Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
                          moDto.LastModifiedBy.Trim(),
                          HelperFunctions.ToDateTime(moDto.DateLastModified),
                          moDto.IsPrivate,
                          true,
                          moDto.Metadata);
                        break;
                    }
                case GalleryObjectType.Generic:
                case GalleryObjectType.Unknown:
                    {
                        mo = new GenericMediaObject(
                          moDto.MediaObjectId,
                          parentAlbum,
                          moDto.ThumbnailFilename,
                          moDto.ThumbnailWidth,
                          moDto.ThumbnailHeight,
                          moDto.ThumbnailSizeKB,
                          moDto.OriginalFilename.Trim(),
                          moDto.OriginalWidth,
                          moDto.OriginalHeight,
                          moDto.OriginalSizeKB,
                          moDto.Seq,
                          moDto.CreatedBy.Trim(),
                          Convert.ToDateTime(moDto.DateAdded, CultureInfo.CurrentCulture),
                          moDto.LastModifiedBy.Trim(),
                          HelperFunctions.ToDateTime(moDto.DateLastModified),
                          moDto.IsPrivate,
                          true,
                          null,
                          moDto.Metadata);
                        break;
                    }
                default:
                    {
                        throw new UnsupportedMediaObjectTypeException(Path.Combine(parentAlbum.FullPhysicalPath, moDto.OriginalFilename));
                    }
            }

            mo.IsWritable = parentAlbum.IsWritable;

            return mo;
        }

        /// <summary>
        /// Returns an object that knows how to persist media objects to the data store.
        /// </summary>
        /// <param name="galleryObject">A media object to which the save behavior applies. Must be a valid media
        /// object such as <see cref="Image" />, <see cref="Video" />, etc. Do not pass an <see cref="Album" />.</param>
        /// <returns>Returns an object that implements ISaveBehavior.</returns>
        public static ISaveBehavior GetMediaObjectSaveBehavior(IGalleryObject galleryObject)
        {
            Debug.Assert((!(galleryObject is Album)), "It is invalid to pass an album as a parameter to the GetMediaObjectSaveBehavior() method.");

            return new MediaObjectSaveBehavior(galleryObject as GalleryObject);
        }

        /// <summary>
        /// Returns an object that knows how to delete media objects from the data store.
        /// </summary>
        /// <param name="galleryObject">A media object to which the delete behavior applies. Must be a valid media
        /// object such as Image, Video, etc. Do not pass an Album; use <see cref="GetAlbumDeleteBehavior" /> for configuring <see cref="Album" /> objects.</param>
        /// <returns>Returns an object that implements <see cref="IDeleteBehavior" />.</returns>
        public static IDeleteBehavior GetMediaObjectDeleteBehavior(IGalleryObject galleryObject)
        {
            Debug.Assert((!(galleryObject is Album)), "It is invalid to pass an album as a parameter to the GetMediaObjectDeleteBehavior() method.");

            return new MediaObjectDeleteBehavior(galleryObject);
        }

        #endregion

        #region Image Methods

        /// <summary>
        /// Create a writable, minimally populated <see cref="Image" /> instance from the specified parameters.
        /// </summary>
        /// <param name="imageFile">A <see cref="System.IO.FileInfo" /> object representing a supported image type. The file must already
        /// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
        /// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
        ///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
        /// <param name="parentAlbum">The album in which the image exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns an <see cref="Image" /> instance corresponding to the specified parameters.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when 
        /// <paramref name = "imageFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
        /// <paramref name = "imageFile" /> has a file extension that Gallery Server is configured to reject, or it is
        /// associated with a non-image MIME type.</exception>
        /// <exception cref="UnsupportedImageTypeException">Thrown when the 
        /// .NET Framework is unable to load an image file into the <see cref="System.Drawing.Bitmap" /> class. This is 
        /// probably because it is corrupted, not an image supported by the .NET Framework, or the server does not have 
        /// enough memory to process the image. The file cannot, therefore, be handled using the <see cref="Image" /> 
        /// class; use <see cref="GenericMediaObject" /> instead.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name = "imageFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
        public static IGalleryObject CreateImageInstance(FileInfo imageFile, IAlbum parentAlbum)
        {
            if (imageFile == null)
                throw new ArgumentNullException("imageFile");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            // Validation check: Make sure the configuration settings allow for this particular type of file to be added.
            if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(imageFile.Name, parentAlbum.GalleryId))
                throw new UnsupportedMediaObjectTypeException(imageFile.FullName);

            // If the file belongs to an existing media object, return a reference to it.
            foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Image))
            {
                if (childMediaObject.Original.FileNamePhysicalPath == imageFile.FullName)
                    return childMediaObject;
            }

            // Create a new image object, which will cause a new record to be inserted in the data store when Save() is called.
            return new Image(imageFile, parentAlbum) { IsWritable = true };
        }

        /// <summary>
        /// Create a fully inflated image instance based on the <see cref="IGalleryObject.Id">ID</see> of the image parameter. Overwrite
        /// properties of the image parameter with the retrieved values from the data store. The returned image
        /// is the same object reference as the image parameter.
        /// </summary>
        /// <param name="image">The image whose properties should be overwritten with the values from the data store.</param>
        /// <returns>Returns an inflated image instance with all properties set to the values from the data store.
        /// </returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when
        /// an image is not found in the data store that matches the <see cref="IGalleryObject.Id">ID</see> of the image parameter in the current gallery.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image" /> is null.</exception>
        public static IGalleryObject LoadImageInstance(IGalleryObject image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            IGalleryObject retrievedImage = LoadMediaObjectInstance(new MediaLoadOptions(image.Id) { Album = (IAlbum)image.Parent, IsWritable = image.Parent.IsWritable });

            image.GalleryId = retrievedImage.GalleryId;
            //image.Title = retrievedImage.Title;
            image.CreatedByUserName = retrievedImage.CreatedByUserName;
            image.DateAdded = retrievedImage.DateAdded;
            image.LastModifiedByUserName = retrievedImage.LastModifiedByUserName;
            image.DateLastModified = retrievedImage.DateLastModified;
            image.IsPrivate = retrievedImage.IsPrivate;
            image.Sequence = retrievedImage.Sequence;
            image.MetadataItems.Clear();
            image.MetadataItems.AddRange(retrievedImage.MetadataItems.Copy());

            string albumPhysicalPath = image.Parent.FullPhysicalPathOnDisk;

            #region Thumbnail

            image.Thumbnail.MediaObjectId = retrievedImage.Id;
            image.Thumbnail.FileName = retrievedImage.Thumbnail.FileName;
            image.Thumbnail.Height = retrievedImage.Thumbnail.Height;
            image.Thumbnail.Width = retrievedImage.Thumbnail.Width;

            IGallerySettings gallerySetting = LoadGallerySetting(image.GalleryId);

            // The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
            string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
            image.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, image.Thumbnail.FileName);

            #endregion

            #region Optimized

            image.Optimized.MediaObjectId = retrievedImage.Id;
            image.Optimized.FileName = retrievedImage.Optimized.FileName;
            image.Optimized.Height = retrievedImage.Optimized.Height;
            image.Optimized.Width = retrievedImage.Optimized.Width;

            // Calcululate the full file path to the optimized image. If the optimized filename is equal to the original filename, then no
            // optimized version exists, and we'll just point to the original. If the names are different, then there is a separate optimized
            // image file, and it is stored in either the album's physical path or an alternate location (if optimizedPath config setting is specified).
            string optimizedPath = albumPhysicalPath;

            if (retrievedImage.Optimized.FileName != retrievedImage.Original.FileName)
                optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);

            image.Optimized.FileNamePhysicalPath = Path.Combine(optimizedPath, image.Optimized.FileName);

            #endregion

            #region Original

            image.Original.MediaObjectId = retrievedImage.Id;
            image.Original.FileName = retrievedImage.Original.FileName;
            image.Original.Height = retrievedImage.Original.Height;
            image.Original.Width = retrievedImage.Original.Width;
            image.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, image.Original.FileName);
            image.Original.ExternalHtmlSource = retrievedImage.Original.ExternalHtmlSource;
            image.Original.ExternalType = retrievedImage.Original.ExternalType;

            #endregion

            image.IsInflated = true;
            image.HasChanges = false;

            return image;
        }

        /// <summary>
        /// Create a fully inflated image instance based on the mediaObjectId.
        /// </summary>
        /// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing image media object.</param>
        /// <returns>Returns an inflated image instance with all properties set to the values from the data store.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when
        /// an image is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
        public static IGalleryObject LoadImageInstance(int mediaObjectId)
        {
            return LoadImageInstance(mediaObjectId, null);
        }

        /// <summary>
        /// Create a fully inflated image instance based on the mediaObjectId.
        /// </summary>
        /// <param name="mediaObjectId">An <see cref="IGalleryObject.Id">ID</see> that uniquely represents an existing image media object.</param>
        /// <param name="parentAlbum">The album containing the media object specified by mediaObjectId. Specify
        /// null if a reference to the album is not available, and it will be created based on the parent album
        /// specified in the data store.</param>
        /// <returns>Returns an inflated image instance with all properties set to the values from the data store.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when
        /// an image is not found in the data store that matches the mediaObjectId parameter and the current gallery.</exception>
        public static IGalleryObject LoadImageInstance(int mediaObjectId, IAlbum parentAlbum)
        {
            return LoadMediaObjectInstance(new MediaLoadOptions(mediaObjectId) { Album = parentAlbum, IsWritable = parentAlbum.IsWritable });
        }

        #endregion

        #region Video Methods

        /// <summary>
        /// Create a writable, minimally populated <see cref="Video" /> instance from the specified parameters.
        /// </summary>
        /// <param name="videoFile">A <see cref="System.IO.FileInfo" /> object representing a supported video type. The file must already
        /// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
        /// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
        ///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
        /// <param name="parentAlbum">The album in which the video exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns a <see cref="Video" /> instance corresponding to the specified parameters.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
        /// <paramref name = "videoFile" /> has a file extension that Gallery Server is configured to reject, or it is
        /// associated with a non-video MIME type.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when   
        /// <paramref name = "videoFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name = "videoFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
        public static IGalleryObject CreateVideoInstance(FileInfo videoFile, IAlbum parentAlbum)
        {
            if (videoFile == null)
                throw new ArgumentNullException("videoFile");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            // Validation check: Make sure the configuration settings allow for this particular type of file to be added.
            if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(videoFile.Name, parentAlbum.GalleryId))
                throw new UnsupportedMediaObjectTypeException(videoFile.FullName);

            // If the file belongs to an existing media object, return a reference to it.
            foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Video))
            {
                if (childMediaObject.Original.FileNamePhysicalPath == videoFile.FullName)
                    return childMediaObject;
            }

            // Create a new video object, which will cause a new record to be inserted in the data store when Save() is called.
            return new Video(videoFile, parentAlbum) { IsWritable = true };
        }

        /// <summary>
        /// Create a fully inflated <see cref="Video" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the video parameter. Overwrite
        /// properties of the video parameter with the retrieved values from the data store. The returned video
        /// is the same object reference as the video parameter.
        /// </summary>
        /// <param name="video">The video whose properties should be overwritten with the values from the data store.</param>
        /// <returns>Returns an inflated <see cref="Video" /> instance with all properties set to the values from the data store.
        /// </returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when a video is not found in the data store that matches the 
        /// <see cref="IGalleryObject.Id">ID</see> of the video parameter in the current gallery.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="video" /> is null.</exception>
        public static IGalleryObject LoadVideoInstance(IGalleryObject video)
        {
            if (video == null)
                throw new ArgumentNullException(nameof(video));

            IGalleryObject retrievedVideo = LoadMediaObjectInstance(new MediaLoadOptions(video.Id) { Album = (IAlbum)video.Parent, IsWritable = video.Parent.IsWritable });

            video.GalleryId = retrievedVideo.GalleryId;
            //video.Title = retrievedVideo.Title;
            video.CreatedByUserName = retrievedVideo.CreatedByUserName;
            video.DateAdded = retrievedVideo.DateAdded;
            video.LastModifiedByUserName = retrievedVideo.LastModifiedByUserName;
            video.DateLastModified = retrievedVideo.DateLastModified;
            video.IsPrivate = retrievedVideo.IsPrivate;
            video.Sequence = retrievedVideo.Sequence;
            video.MetadataItems.Clear();
            video.MetadataItems.AddRange(retrievedVideo.MetadataItems.Copy());

            string albumPhysicalPath = video.Parent.FullPhysicalPathOnDisk;

            #region Thumbnail

            video.Thumbnail.MediaObjectId = retrievedVideo.Id;
            video.Thumbnail.FileName = retrievedVideo.Thumbnail.FileName;
            video.Thumbnail.Height = retrievedVideo.Thumbnail.Height;
            video.Thumbnail.Width = retrievedVideo.Thumbnail.Width;

            IGallerySettings gallerySetting = LoadGallerySetting(video.GalleryId);

            // The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
            string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
            video.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, video.Thumbnail.FileName);

            #endregion

            #region Optimized

            video.Optimized.MediaObjectId = retrievedVideo.Id;
            video.Optimized.FileName = retrievedVideo.Optimized.FileName;
            video.Optimized.Height = retrievedVideo.Optimized.Height;
            video.Optimized.Width = retrievedVideo.Optimized.Width;

            #endregion

            #region Original

            video.Original.MediaObjectId = retrievedVideo.Id;
            video.Original.FileName = retrievedVideo.Original.FileName;
            video.Original.Height = retrievedVideo.Original.Height;
            video.Original.Width = retrievedVideo.Original.Width;
            video.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, video.Original.FileName);
            video.Original.ExternalHtmlSource = retrievedVideo.Original.ExternalHtmlSource;
            video.Original.ExternalType = retrievedVideo.Original.ExternalType;

            #endregion

            video.IsInflated = true;
            video.HasChanges = false;

            return video;
        }

        #endregion

        #region Audio Methods

        /// <summary>
        /// Create a writable, minimally populated <see cref="Audio" /> instance from the specified parameters.
        /// </summary>
        /// <param name="audioFile">A <see cref="System.IO.FileInfo" /> object representing a supported audio type. The file must already
        /// exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
        /// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
        ///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
        /// <param name="parentAlbum">The album in which the audio exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns an <see cref="Audio" /> instance corresponding to the specified parameters.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when 
        /// <paramref name = "audioFile" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
        /// <paramref name = "audioFile" /> has a file extension that Gallery Server is configured to reject, or it is
        /// associated with a non-audio MIME type.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name = "audioFile" /> or <paramref name = "parentAlbum" /> is null.</exception>
        public static IGalleryObject CreateAudioInstance(FileInfo audioFile, IAlbum parentAlbum)
        {
            if (audioFile == null)
                throw new ArgumentNullException("audioFile");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            // Validation check: Make sure the configuration settings allow for this particular type of file to be added.
            if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(audioFile.Name, parentAlbum.GalleryId))
                throw new UnsupportedMediaObjectTypeException(audioFile.FullName);

            // If the file belongs to an existing media object, return a reference to it.
            foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Audio))
            {
                if (childMediaObject.Original.FileNamePhysicalPath == audioFile.FullName)
                    return childMediaObject;
            }

            // Create a new audio object, which will cause a new record to be inserted in the data store when Save() is called.
            return new Audio(audioFile, parentAlbum) { IsWritable = true };
        }

        /// <summary>
        /// Create a fully inflated <see cref="Audio" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the audio parameter. Overwrite
        /// properties of the audio parameter with the retrieved values from the data store. The returned audio
        /// is the same object reference as the audio parameter.
        /// </summary>
        /// <param name="audio">The <see cref="Audio" /> instance whose properties should be overwritten with the values from the data store.</param>
        /// <returns>Returns an inflated <see cref="Audio" /> instance with all properties set to the values from the data store.
        /// </returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when a audio file is not found in the data store that matches the 
        /// <see cref="IGalleryObject.Id">ID</see> of the audio parameter in the current gallery.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="audio" /> is null.</exception>
        public static IGalleryObject LoadAudioInstance(IGalleryObject audio)
        {
            if (audio == null)
                throw new ArgumentNullException("audio");

            IGalleryObject retrievedAudio = LoadMediaObjectInstance(new MediaLoadOptions(audio.Id) { Album = (IAlbum)audio.Parent, IsWritable = audio.Parent.IsWritable });

            audio.GalleryId = retrievedAudio.GalleryId;
            //audio.Title = retrievedAudio.Title;
            audio.CreatedByUserName = retrievedAudio.CreatedByUserName;
            audio.DateAdded = retrievedAudio.DateAdded;
            audio.LastModifiedByUserName = retrievedAudio.LastModifiedByUserName;
            audio.DateLastModified = retrievedAudio.DateLastModified;
            audio.IsPrivate = retrievedAudio.IsPrivate;
            audio.Sequence = retrievedAudio.Sequence;
            audio.MetadataItems.Clear();
            audio.MetadataItems.AddRange(retrievedAudio.MetadataItems.Copy());

            string albumPhysicalPath = audio.Parent.FullPhysicalPathOnDisk;

            #region Thumbnail

            audio.Thumbnail.MediaObjectId = retrievedAudio.Id;
            audio.Thumbnail.FileName = retrievedAudio.Thumbnail.FileName;
            audio.Thumbnail.Height = retrievedAudio.Thumbnail.Height;
            audio.Thumbnail.Width = retrievedAudio.Thumbnail.Width;

            IGallerySettings gallerySetting = LoadGallerySetting(audio.GalleryId);

            // The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
            string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
            audio.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, audio.Thumbnail.FileName);

            #endregion

            #region Optimized

            audio.Optimized.MediaObjectId = retrievedAudio.Id;
            audio.Optimized.FileName = retrievedAudio.Optimized.FileName;
            audio.Optimized.Height = retrievedAudio.Optimized.Height;
            audio.Optimized.Width = retrievedAudio.Optimized.Width;

            #endregion

            #region Original

            audio.Original.MediaObjectId = retrievedAudio.Id;
            audio.Original.FileName = retrievedAudio.Original.FileName;
            audio.Original.Height = retrievedAudio.Original.Height;
            audio.Original.Width = retrievedAudio.Original.Width;
            audio.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, audio.Original.FileName);
            audio.Original.ExternalHtmlSource = retrievedAudio.Original.ExternalHtmlSource;
            audio.Original.ExternalType = retrievedAudio.Original.ExternalType;

            #endregion

            audio.IsInflated = true;
            audio.HasChanges = false;

            return audio;
        }

        #endregion

        #region Generic Media Object Methods

        /// <summary>
        /// Create a writable, minimally populated <see cref="GenericMediaObject" /> instance from the specified parameters.
        /// </summary>
        /// <param name="file">A <see cref="System.IO.FileInfo" /> object representing a file to be managed by Gallery Server. The file must 
        /// already exist in the album's directory. If the file has a matching record in the data store, a reference to the existing 
        /// object is returned; otherwise, a new instance is returned. Otherwise, a new instance is returned. For new instances, 
        ///		call <see cref="IGalleryObject.Save" /> to persist the object to the data store.</param>
        /// <param name="parentAlbum">The album in which the file exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns a <see cref="GenericMediaObject" /> instance corresponding to the specified parameters.</returns>
        /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
        /// <paramref name = "file" /> has a file extension that Gallery Server is configured to reject.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when   
        /// <paramref name = "file" /> refers to a file that is not in the same directory as the parent album's directory.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name = "file" /> or <paramref name = "parentAlbum" /> is null.</exception>
        public static IGalleryObject CreateGenericObjectInstance(FileInfo file, IAlbum parentAlbum)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            // Validation check: Make sure the configuration settings allow for this particular type of file to be added.
            if (!HelperFunctions.IsFileAuthorizedForAddingToGallery(file.Name, parentAlbum.GalleryId))
                throw new UnsupportedMediaObjectTypeException(file.FullName);

            // If the file belongs to an existing media object, return a reference to it.
            foreach (IGalleryObject childMediaObject in parentAlbum.GetChildGalleryObjects(GalleryObjectType.Generic))
            {
                if (childMediaObject.Original.FileNamePhysicalPath == file.FullName)
                    return childMediaObject;
            }

            // Create a new generic media object, which will cause a new record to be inserted in the data store when Save() is called.
            return new GenericMediaObject(file, parentAlbum) { IsWritable = true };
        }

        /// <summary>
        /// Create a fully inflated <see cref="GenericMediaObject" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the 
        /// <paramref name = "genericMediaObject" /> parameter. 
        /// Overwrite properties of the <paramref name = "genericMediaObject" /> parameter with the retrieved values from the data store. 
        /// The returned instance is the same object reference as the <paramref name = "genericMediaObject" /> parameter.
        /// </summary>
        /// <param name="genericMediaObject">The object whose properties should be overwritten with the values from 
        /// the data store.</param>
        /// <returns>Returns an inflated <see cref="GenericMediaObject" /> instance with all properties set to the values from the 
        /// data store.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when a record is not found in the data store that matches the 
        /// <see cref="IGalleryObject.Id">ID</see> of the <paramref name = "genericMediaObject" /> parameter in the current gallery.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="genericMediaObject" /> is null.</exception>
        public static IGalleryObject LoadGenericMediaObjectInstance(IGalleryObject genericMediaObject)
        {
            if (genericMediaObject == null)
                throw new ArgumentNullException("genericMediaObject");

            IGalleryObject retrievedGenericMediaObject = LoadMediaObjectInstance(new MediaLoadOptions(genericMediaObject.Id) { Album = (IAlbum)genericMediaObject.Parent, IsWritable = genericMediaObject.Parent.IsWritable });

            genericMediaObject.GalleryId = retrievedGenericMediaObject.GalleryId;
            //genericMediaObject.Title = retrievedGenericMediaObject.Title;
            genericMediaObject.CreatedByUserName = retrievedGenericMediaObject.CreatedByUserName;
            genericMediaObject.DateAdded = retrievedGenericMediaObject.DateAdded;
            genericMediaObject.LastModifiedByUserName = retrievedGenericMediaObject.LastModifiedByUserName;
            genericMediaObject.DateLastModified = retrievedGenericMediaObject.DateLastModified;
            genericMediaObject.IsPrivate = retrievedGenericMediaObject.IsPrivate;
            genericMediaObject.Sequence = retrievedGenericMediaObject.Sequence;
            genericMediaObject.MetadataItems.Clear();
            genericMediaObject.MetadataItems.AddRange(retrievedGenericMediaObject.MetadataItems.Copy());

            string albumPhysicalPath = genericMediaObject.Parent.FullPhysicalPathOnDisk;

            #region Thumbnail

            genericMediaObject.Thumbnail.MediaObjectId = retrievedGenericMediaObject.Id;
            genericMediaObject.Thumbnail.FileName = retrievedGenericMediaObject.Thumbnail.FileName;
            genericMediaObject.Thumbnail.Height = retrievedGenericMediaObject.Thumbnail.Height;
            genericMediaObject.Thumbnail.Width = retrievedGenericMediaObject.Thumbnail.Width;

            IGallerySettings gallerySetting = LoadGallerySetting(genericMediaObject.GalleryId);

            // The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
            string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
            genericMediaObject.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, genericMediaObject.Thumbnail.FileName);

            #endregion

            #region Optimized

            // No optimized object for a generic media object.

            #endregion

            #region Original

            genericMediaObject.Original.MediaObjectId = retrievedGenericMediaObject.Id;
            genericMediaObject.Original.FileName = retrievedGenericMediaObject.Original.FileName;
            genericMediaObject.Original.Height = retrievedGenericMediaObject.Original.Height;
            genericMediaObject.Original.Width = retrievedGenericMediaObject.Original.Width;
            genericMediaObject.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, genericMediaObject.Original.FileName);
            genericMediaObject.Original.ExternalHtmlSource = retrievedGenericMediaObject.Original.ExternalHtmlSource;
            genericMediaObject.Original.ExternalType = retrievedGenericMediaObject.Original.ExternalType;

            #endregion

            genericMediaObject.IsInflated = true;
            genericMediaObject.HasChanges = false;

            return genericMediaObject;
        }

        #endregion

        #region External Media Object Methods

        /// <summary>
        /// Create a writable, minimally populated <see cref="ExternalMediaObject" /> instance from the specified parameters.
        /// </summary>
        /// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at 
        /// YouTube or Silverlight.live.com.</param>
        /// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
        /// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
        /// <param name="parentAlbum">The album in which the file exists (for media objects that already exist
        /// in the data store), or should be added to (for new media objects which need to be inserted into the 
        /// data store).</param>
        /// <returns>Returns a minimally populated <see cref="ExternalMediaObject" /> instance from the specified parameters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name = "externalHtmlSource" /> is an empty string or null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
        public static IGalleryObject CreateExternalMediaObjectInstance(string externalHtmlSource, MimeTypeCategory mimeType, IAlbum parentAlbum)
        {
            if (String.IsNullOrEmpty(externalHtmlSource))
                throw new ArgumentOutOfRangeException("externalHtmlSource", "The parameter is either null or an empty string.");

            if (parentAlbum == null)
                throw new ArgumentNullException("parentAlbum");

            // Create a new generic media object, which will cause a new record to be inserted in the data store when Save() is called.
            return new ExternalMediaObject(externalHtmlSource, mimeType, parentAlbum) { IsWritable = true };
        }

        /// <summary>
        /// Create a fully inflated <see cref="ExternalMediaObject" /> instance based on the <see cref="IGalleryObject.Id">ID</see> of the 
        /// <paramref name = "externalMediaObject" /> parameter. 
        /// Overwrite properties of the <paramref name = "externalMediaObject" /> parameter with the retrieved values from the data store. 
        /// The returned instance is the same object reference as the <paramref name = "externalMediaObject" /> parameter.
        /// </summary>
        /// <param name="externalMediaObject">The object whose properties should be overwritten with the values from 
        /// the data store.</param>
        /// <returns>Returns an inflated <see cref="ExternalMediaObject" /> instance with all properties set to the values from the 
        /// data store.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when a record is not found in the data store that matches the 
        /// <see cref="IGalleryObject.Id">ID</see> of the <paramref name = "externalMediaObject" /> parameter in the current gallery.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="externalMediaObject" /> is null.</exception>
        public static IGalleryObject LoadExternalMediaObjectInstance(IGalleryObject externalMediaObject)
        {
            if (externalMediaObject == null)
                throw new ArgumentNullException(nameof(externalMediaObject));

            IGalleryObject retrievedExternalMediaObject = LoadMediaObjectInstance(new MediaLoadOptions(externalMediaObject.Id) { Album = (IAlbum)externalMediaObject.Parent, IsWritable = externalMediaObject.Parent.IsWritable });

            externalMediaObject.GalleryId = retrievedExternalMediaObject.GalleryId;
            //externalMediaObject.Title = retrievedGenericMediaObject.Title;
            externalMediaObject.CreatedByUserName = retrievedExternalMediaObject.CreatedByUserName;
            externalMediaObject.DateAdded = retrievedExternalMediaObject.DateAdded;
            externalMediaObject.LastModifiedByUserName = retrievedExternalMediaObject.LastModifiedByUserName;
            externalMediaObject.DateLastModified = retrievedExternalMediaObject.DateLastModified;
            externalMediaObject.IsPrivate = retrievedExternalMediaObject.IsPrivate;
            externalMediaObject.Sequence = retrievedExternalMediaObject.Sequence;
            externalMediaObject.MetadataItems.Clear();
            externalMediaObject.MetadataItems.AddRange(retrievedExternalMediaObject.MetadataItems.Copy());

            string albumPhysicalPath = externalMediaObject.Parent.FullPhysicalPathOnDisk;

            #region Thumbnail

            externalMediaObject.Thumbnail.FileName = retrievedExternalMediaObject.Thumbnail.FileName;
            externalMediaObject.Thumbnail.Height = retrievedExternalMediaObject.Thumbnail.Height;
            externalMediaObject.Thumbnail.Width = retrievedExternalMediaObject.Thumbnail.Width;

            IGallerySettings gallerySetting = LoadGallerySetting(externalMediaObject.GalleryId);

            // The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
            string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
            externalMediaObject.Thumbnail.FileNamePhysicalPath = Path.Combine(thumbnailPath, externalMediaObject.Thumbnail.FileName);

            #endregion

            #region Optimized

            // No optimized image for a generic media object.

            #endregion

            #region Original

            externalMediaObject.Original.FileName = retrievedExternalMediaObject.Original.FileName;
            externalMediaObject.Original.Height = retrievedExternalMediaObject.Original.Height;
            externalMediaObject.Original.Width = retrievedExternalMediaObject.Original.Width;
            externalMediaObject.Original.FileNamePhysicalPath = Path.Combine(albumPhysicalPath, externalMediaObject.Original.FileName);
            externalMediaObject.Original.ExternalHtmlSource = retrievedExternalMediaObject.Original.ExternalHtmlSource;
            externalMediaObject.Original.ExternalType = retrievedExternalMediaObject.Original.ExternalType;

            #endregion

            externalMediaObject.IsInflated = true;
            externalMediaObject.HasChanges = false;

            return externalMediaObject;
        }

        #endregion

        #endregion

        #region Album Methods

        /// <summary>
        /// Create a new, read-only <see cref="Album" /> instance with an unassigned <see cref="IGalleryObject.Id">ID</see> and properties set to default values.
        /// Set the optional parameter <paramref name="isWritable" /> to <c>true</c> to create an instance that can be persisted to the data store.
        /// A valid <see cref="IGalleryObject.Id">ID</see> will be generated when the object is persisted to the data store during
        /// the <see cref="IGalleryObject.Save" /> method. Use this method when creating a new album and it has not yet been persisted
        /// to the data store or when you need a temporary virtual album container for gallery items. Guaranteed to not return null.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="isWritable">Specifies whether the returned instance can be persisted to disk. When not specified, defaults to <c>false</c>.</param>
        /// <returns>Returns an <see cref="Album" /> instance corresponding to the specified parameters.</returns>
        public static IAlbum CreateEmptyAlbumInstance(int galleryId, bool isWritable = false)
        {
            return new Album(Int32.MinValue, galleryId) { IsWritable = isWritable };
        }

        /// <summary>
        /// Creates an empty gallery instance. The <see cref="IGallery.GalleryId" /> will be set to <see cref="int.MinValue" />. 
        /// Generally, gallery instances should be loaded from the data store, but this method can be used to create a new gallery.
        /// </summary>
        /// <returns>Returns an <see cref="IGallery" /> instance.</returns>
        public static IGallery CreateGalleryInstance()
        {
            return new Gallery();
        }

        /// <summary>
        /// Create a minimally populated <see cref="Album" /> instance corresponding to the specified <paramref name = "albumId" />. 
        /// Use this overload when the album already exists in the data store but you do not necessarily need to retrieve its properties. 
        /// A lazy load is performed the first time a property is accessed.
        /// </summary>
        /// <param name="albumId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies an existing album.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>
        /// Returns an instance that implements <see cref="IAlbum" /> corresponding to the specified parameters.
        /// </returns>
        public static IAlbum CreateAlbumInstance(int albumId, int galleryId)
        {
            return new Album(albumId, galleryId);
        }

        /// <overloads>
        /// Loads an instance of the top-level album from the data store for the specified gallery. 
        /// </overloads>
        ///  <summary>
        /// Loads a read-only instance of the top-level album from the data store for the specified gallery. Metadata is
        /// automatically loaded. If this album contains child objects, they are added but not inflated. If this album contains
        /// child objects, they are automatically inflated. Guaranteed to not return null.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>
        /// Returns an instance that implements <see cref="IAlbum" /> with all properties set to the values from the data store.
        /// </returns>
        public static IAlbum LoadRootAlbumInstance(int galleryId)
        {
            return LoadRootAlbumInstance(galleryId, true);
        }

        /// <summary>
        /// Loads a read-only instance of the top-level album from the data store for the specified gallery, optionally specifying
        /// whether to suppress the loading of media object metadata. Suppressing metadata loading offers a performance improvement,
        /// so when this data is not needed, set <paramref name="allowMetadataLoading" /> to <c>false</c>. If this album contains
        /// child objects, they are automatically inflated. Guaranteed to not return null.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="allowMetadataLoading">if set to <c>false</c> the metadata for media objects are not loaded.</param>
        /// <returns>Returns an instance that implements <see cref="IAlbum" /> with all properties set to the values from the data store.</returns>
        public static IAlbum LoadRootAlbumInstance(int galleryId, bool allowMetadataLoading)
        {
            IAlbum album;

            try
            {
                album = LoadAlbumInstance(new AlbumLoadOptions(LoadGallery(galleryId).RootAlbumId)
                {
                    InflateChildObjects = true,
                    AllowMetadataLoading = allowMetadataLoading
                });
            }
            catch (InvalidAlbumException)
            {
                album = CreateRootAlbum(galleryId);
            }

            return album;
        }

        /// <summary>
        /// Return all top-level albums in the specified <paramref name = "galleryId">gallery</paramref> where the <paramref name = "roles" /> 
        /// provide view permission to the album. If more than one album is found, they are wrapped in a virtual container 
        /// album where the <see cref="IAlbum.IsVirtualAlbum" /> property is set to true. If the roles do not provide permission to any
        /// objects in the gallery, then a virtual album is returned where <see cref="IAlbum.IsVirtualAlbum" />=<c>true</c> and 
        /// <see cref="IGalleryObject.Id" />=<see cref="Int32.MinValue" />. Returns null if no matching albums are found.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="roles">The roles belonging to a user.</param>
        /// <param name="isAuthenticated">Indicates whether the user belonging to the <paramref name="roles" /> is authenticated.</param>
        /// <returns>
        /// Returns an <see cref="IAlbum" /> that is or contains the top-level album(s) that the <paramref name = "roles" />
        /// provide view permission for. Returns null if no matching albums are found.
        /// </returns>
        public static IAlbum LoadRootAlbum(int galleryId, IGalleryServerRoleCollection roles, bool isAuthenticated)
        {
            var galleryObjectSearcher = new GalleryObjectSearcher(new GalleryObjectSearchOptions()
            {
                GalleryId = galleryId,
                SearchType = GalleryObjectSearchType.HighestAlbumUserCanView,
                Roles = roles,
                IsUserAuthenticated = isAuthenticated,
                Filter = GalleryObjectType.Album
            });

            return galleryObjectSearcher.FindOne() as IAlbum;
        }

        /// <overloads>
        /// Generate an <see cref="IAlbum" /> instance representing an existing album.
        /// </overloads>
        /// <summary>
        /// Generate an <see cref="IAlbum" /> instance with optionally inflated child objects. The album's <see cref="IAlbum.ThumbnailMediaObjectId" />
        /// property is set to its value from the data store, but the <see cref="IGalleryObject.Thumbnail" /> property is only inflated when accessed.
        /// </summary>
        /// <param name="album">The album whose properties should be overwritten with the values from the data store.</param>
        /// <param name="inflateChildMediaObjects">When <c>true</c>, the child album and media objects of the album are added and inflated.
        /// When <c>false</c>, they are not added or inflated.</param>
        /// <exception cref="InvalidAlbumException">Thrown when an album is not found in the data store that matches the 
        /// <see cref="IGalleryObject.Id">ID</see> of the album parameter.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="inflateChildMediaObjects" /> is <c>false</c> and the album is inflated.</exception>
        public static void LoadAlbumInstance(IAlbum album, bool inflateChildMediaObjects)
        {
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (album.IsInflated && !inflateChildMediaObjects)
                throw new InvalidOperationException(Resources.Factory_LoadAlbumInstance_Ex_Msg);

            lock (_sharedLock)
            {
                #region Inflate the album, but only if it's not already inflated.

                CacheItemAlbum albumAsset = null;
                if (!(album.IsInflated))
                {
                    if (album.Id > Int32.MinValue)
                    {
                        var albumCache = CacheController.GetAlbumAssetCache();
                        if (albumCache != null && albumCache.TryGetValue(album.Id, out albumAsset))
                        {
                            Album.InflateFromCacheItem(album, albumAsset);
                        }
                        else
                        {
                            using (var repo = new AlbumRepository())
                            {
                                InflateAlbumFromDto(album, repo.Where(a => a.AlbumId == album.Id, m => m.Metadata).FirstOrDefault());
                            }
                        }
                    }

                    if (!(album.Parent is NullGalleryObject))
                    {
                        album.AllowMetadataLoading = ((IAlbum)album.Parent).AllowMetadataLoading;
                    }

                    album.FullPhysicalPathOnDisk = album.FullPhysicalPath;
                    album.IsInflated = true;

                    Debug.Assert(album.ThumbnailMediaObjectId > Int32.MinValue, "The album's ThumbnailMediaObjectId should have been assigned in this method.");

                    album.HasChanges = false;
                }

                #endregion

                #region Add child objects (CreateInstance)

                // Add child albums and objects, if they exist, and if the album wasn't already inflated by another thread.
                if (inflateChildMediaObjects && !album.AreChildrenInflated)
                {
                    var albumCache = CacheController.GetAlbumAssetCache();
                    if (albumCache != null)
                    {
                        albumCache.TryGetValue(album.Id, out albumAsset);
                    }

                    AddChildObjects(album, albumAsset);
                }

                #endregion
            }

            if (!album.IsInflated)
            {
                throw new InvalidAlbumException(album.Id);
            }
        }

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
        /// <remarks>The album may come from one of three sources:
        /// 1. The inflated album cache (aka primary cache). Inflated instances of <see cref="IAlbum" /> may exist in the cache. These are the 
        ///    fastest to retrieve, so we prefer these when they exist and the user is fine with a read-only instance.
        /// 2. The album asset cache (aka secondary cache). Albums are represented as <see cref="CacheItemAlbum" /> instances, which are lightweight, 
        ///    stand-alone albums which can be hydrated into <see cref="IAlbum" /> instances faster than getting them from the data store. They
        ///    can also be turned into writable instances when desired.
        /// 3. The data store. When not present in either of the caches, the album is hydrated from data retrieved from the data store. It is 
        ///    also added to the cache for faster subsequent retrieval (one exception is that writable albums are not added to the inflated album cache).
        /// </remarks>
        /// <exception cref="InvalidAlbumException">Thrown when the <see cref="AlbumLoadOptions.AlbumId" /> property of <paramref name="options" />
        /// does not represent a valid album.</exception>
        public static IAlbum LoadAlbumInstance(AlbumLoadOptions options)
        {
            IAlbum album = null;

            if (!options.IsWritable && options.AllowMetadataLoading)
            {
                // First look in the inflated album cache and return if found.
                var inflatedAlbumCache = CacheController.GetInflatedAlbumCache();

                if (inflatedAlbumCache != null && inflatedAlbumCache.TryGetValue(options.AlbumId, out album))
                {
                    return album;
                }
            }

            // Album not in inflated album cache. Starting with the root album, inflate it (either from the secondary cache or from the DB)
            // and move to the next album down in the hierarchy until we get to the requested album.
            var albumHierarchy = new List<int>(GetAlbumHierarchy(options.AlbumId));

            albumHierarchy.Add(options.AlbumId);

            IAlbum parentAlbum = null;
            CacheItemAlbum albumAsset = null;
            var albumsToAddToAssetCache = new List<IAlbum>();
            var albumCache = CacheController.GetAlbumAssetCache();

            // Load the root album, then each child until we get to the requested one. This is necessary to be able to calculate the 
            // FullPhysicalPath property.
            foreach (var albumId in albumHierarchy)
            {
                if (albumCache != null && albumCache.TryGetValue(albumId, out albumAsset))
                {
                    album = Album.CreateFrom(albumAsset, parentAlbum);
                }
                else
                {
                    // There is no cache item. Retrieve from data store and create cache item so it's there next time we want it.
                    album = RetrieveAlbumFromDataStore(albumId, parentAlbum);

                    albumsToAddToAssetCache.Add(album);
                }

                // Calculate and assign the physical path to the album. This step is necessary before adding child objects.
                album.AllowMetadataLoading = options.AllowMetadataLoading;
                album.IsWritable = options.IsWritable;

                parentAlbum = album;
            }

            Debug.Assert(album.IsInflated, "The album should be inflated, but it was not.");
            Debug.Assert(!album.AreChildrenInflated, "The album's children should NOT have been added to the album yet, but they were.");

            if (options.InflateChildObjects)
            {
                AddChildObjects(album, albumAsset);
            }

            foreach (var albumToAddToCache in albumsToAddToAssetCache)
            {
                CacheController.AddToAlbumAssetCache(CacheItemAlbum.CreateFrom(albumToAddToCache));
            }

            if (!options.IsWritable)
            {
                CacheController.AddToInflatedAlbumCache(album);
            }

            return album;
        }

        /// <summary>
        /// Returns an instance of an object that knows how to persist albums to the data store.
        /// </summary>
        /// <param name="albumObject">An <see cref="IAlbum" /> to which the save behavior applies.</param>
        /// <returns>Returns an object that implements <see cref="ISaveBehavior" />.</returns>
        public static ISaveBehavior GetAlbumSaveBehavior(IAlbum albumObject)
        {
            return new AlbumSaveBehavior(albumObject);
        }

        /// <summary>
        /// Returns an instance of an object that knows how to delete albums from the data store.
        /// </summary>
        /// <param name="albumObject">An <see cref="IAlbum" /> to which the delete behavior applies.</param>
        /// <returns>Returns an object that implements <see cref="IDeleteBehavior" />.</returns>
        public static IDeleteBehavior GetAlbumDeleteBehavior(IAlbum albumObject)
        {
            return new AlbumDeleteBehavior(albumObject);
        }

        /// <summary>
        /// Returns an instance of an object that knows how to read and write metadata to and from a gallery object.
        /// </summary>
        /// <param name="galleryObject">A <see cref="IGalleryObject" /> for which to retrieve metadata.</param>
        /// <returns>Returns an object that implements <see cref="IMetadataReadWriter" />.</returns>
        public static IMetadataReadWriter GetMetadataReadWriter(IGalleryObject galleryObject)
        {
            switch (galleryObject.GalleryObjectType)
            {
                case GalleryObjectType.Album: return new AlbumMetadataReadWriter(galleryObject);
                case GalleryObjectType.Image: return new ImageMetadataReadWriter(galleryObject);
                case GalleryObjectType.Audio: return new AudioMetadataReadWriter(galleryObject);
                case GalleryObjectType.Video: return new VideoMetadataReadWriter(galleryObject);
                case GalleryObjectType.External: return new ExternalMetadataReadWriter(galleryObject);
                case GalleryObjectType.Generic: return new GenericMetadataReadWriter(galleryObject);
                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Factory.GetMetadataExtractor() does not support gallery objects with type {0}. A developer may need to update this method.", galleryObject.GalleryObjectType));
            }

        }

        /// <summary>
        /// Creates an inflated album based on <paramref name="albumDto" /> having parent album <paramref name="parentAlbum" />. No child albums 
        /// or child media assets are added (<see cref="IAlbum.AreChildrenInflated" /> = <c>false</c>). If <paramref name="parentAlbum" /> is
        /// null, an album is instantiated based on the <see cref="AlbumDto.FKAlbumParentId" /> property of <paramref name="albumDto" />.
        /// </summary>
        /// <param name="albumDto">The album data transfer object.</param>
        /// <param name="parentAlbum">The album containing the <paramref name="albumDto" />. May be null. When null, an instance of <see cref="IAlbum" />
        /// is created based on the <see cref="AlbumDto.FKAlbumParentId" /> property of <paramref name="albumDto" />.</param>
        /// <returns>Returns an instance implementing <see cref="IAlbum" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="albumDto" /> is null.</exception>
        public static IAlbum GetAlbumFromDto(AlbumDto albumDto, IGalleryObject parentAlbum = null)
        {
            if (albumDto == null)
            {
                throw new ArgumentNullException(nameof(albumDto));
            }

            if (parentAlbum == null)
            {
                parentAlbum = (albumDto.FKAlbumParentId.HasValue ? (IGalleryObject)Factory.LoadAlbumInstance(albumDto.FKAlbumParentId.Value) : new NullGalleryObject());
            }

            IAlbum album = new Album(albumDto.AlbumId,
                                     albumDto.FKGalleryId,
                                     parentAlbum,
                                     albumDto.DirectoryName,
                                     albumDto.ThumbnailMediaObjectId,
                                     albumDto.SortByMetaName,
                                     albumDto.SortAscending,
                                     albumDto.Seq,
                                     albumDto.CreatedBy.Trim(),
                                     HelperFunctions.ToDateTime(albumDto.DateAdded),
                                     albumDto.LastModifiedBy.Trim(),
                                     HelperFunctions.ToDateTime(albumDto.DateLastModified),
                                     albumDto.OwnedBy.Trim(),
                                     albumDto.OwnerRoleName.Trim(),
                                     albumDto.IsPrivate,
                                     true,
                                     albumDto.Metadata);

            album.FullPhysicalPathOnDisk = album.FullPhysicalPath;

            return album;
        }

        #endregion

        #region Metadata Methods

        /// <summary>
        /// Creates a new, empty metadata collection.
        /// </summary>
        /// <returns>Returns an instance of <see cref="IGalleryObjectMetadataItemCollection" />.</returns>
        public static IGalleryObjectMetadataItemCollection CreateMetadataCollection()
        {
            return new GalleryObjectMetadataItemCollection();
        }

        /// <summary>
        /// Create a new <see cref="IGalleryObjectMetadataItem" /> item from the specified parameters.
        /// </summary>
        /// <param name="id">A value that uniquely indentifies this metadata item.</param>
        /// <param name="galleryObject">The gallery object the metadata item applies to.</param>
        /// <param name="rawValue">The raw value of the metadata item. Typically this is the value extracted from 
        /// the metadata of the media file.</param>
        /// <param name="value">The value of the metadata item (e.g. "F5.7", "1/500 sec.").</param>
        /// <param name="hasChanges">A value indicating whether this metadata item has changes that have not been persisted to the database.</param>
        /// <param name="metaDef">The meta definition.</param>
        /// <returns>Returns a reference to the new item.</returns>
        public static IGalleryObjectMetadataItem CreateMetadataItem(int id, IGalleryObject galleryObject, string rawValue, string value, bool hasChanges, IMetadataDefinition metaDef)
        {
            return new GalleryObjectMetadataItem(id, galleryObject, rawValue, value, hasChanges, metaDef);
        }

        /// <summary>
        /// Loads the metadata item for the specified <paramref name="metadataId" />. If no matching 
        /// object is found in the data store, null is returned.
        /// </summary>
        /// <param name="metadataId">The ID that uniquely identifies the metadata item.</param>
        /// <returns>An instance of <see cref="IGalleryObjectMetadataItem" />, or null if not matching
        /// object is found.</returns>
        public static IGalleryObjectMetadataItem LoadGalleryObjectMetadataItem(int metadataId)
        {
            //var mDto = GetDataProvider().Metadata_GetMetadataItem(metadataId);
            var mDto = new MetadataRepository().Find(metadataId);

            if (mDto == null)
                return null;

            var go = mDto.FKAlbumId.HasValue ? LoadAlbumInstance(mDto.FKAlbumId.Value) : LoadMediaObjectInstance(mDto.FKMediaObjectId.GetValueOrDefault(0));

            var metaDefs = LoadGallerySetting(go.GalleryId).MetadataDisplaySettings;

            return CreateMetadataItem(mDto.MetadataId, go, mDto.RawValue, mDto.Value, false, metaDefs.Find(mDto.MetaName));
        }

        /// <summary>
        /// Persists the metadata item to the data store, or deletes it when the delete flag is set. 
        /// For certain items (title, filename, etc.), the associated gallery object's property is also 
        /// updated. For items that are being deleted, it is also removed from the gallery object's metadata
        /// collection.
        /// </summary>
        /// <param name="md">An instance of <see cref="IGalleryObjectMetadataItem" /> to persist to the data store.</param>
        /// <param name="userName">The user name of the currently logged on user. This will be used for the audit fields.</param>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested meta item  does not exist 
        /// in the data store.</exception>
        public static void SaveGalleryObjectMetadataItem(IGalleryObjectMetadataItem md, string userName)
        {
            using (var repo = new MetadataRepository())
            {
                repo.Save(md);
            }

            SyncWithGalleryObjectProperties(md, userName);

            var gallerySettings = Factory.LoadGallerySetting(md.GalleryObject.GalleryId);

            // We always want to allow deleting the orientation (unless gallery is read-only), since not doing so
            // will result in mismatched metadata after a rotation.
            var isDeletingOrientation = md.IsDeleted && md.MetadataItemName == MetadataItemName.Orientation;

            if (!gallerySettings.MediaObjectPathIsReadOnly && (md.PersistToFile || isDeletingOrientation))
            {
                if (md.IsDeleted)
                {
                    md.GalleryObject.MetadataReadWriter.DeleteMetaValue(md.MetadataItemName);
                }
                else
                {
                    md.GalleryObject.MetadataReadWriter.SaveMetaValue(md.MetadataItemName);
                }
            }
        }

        /// <summary>
        /// Delete any tags in the Tag table that don't exist in the MetadataTag table.
        /// </summary>
        public static void DeleteUnusedTags()
        {
            var tagRepo = new TagRepository();

            tagRepo.DeleteUnusedTags();
        }

        #endregion

        #region Security Methods

        /// <summary>
        /// Create a Gallery Server role corresponding to the specified parameters. Throws an exception if a role with the
        /// specified name already exists in the data store. The role is not persisted to the data store until the
        /// <see cref="IGalleryServerRole.Save"/> method is called.
        /// </summary>
        /// <param name="roleName">A string that uniquely identifies the role.</param>
        /// <param name="allowViewAlbumOrMediaObject">A value indicating whether the user assigned to this role has permission to view albums
        /// and media objects.</param>
        /// <param name="allowViewOriginalImage">A value indicating whether the user assigned to this role has permission to view the original,
        /// high resolution version of an image. This setting applies only to images. It has no effect if there are no
        /// high resolution images in the album or albums to which this role applies.</param>
        /// <param name="allowAddMediaObject">A value indicating whether the user assigned to this role has permission to add media objects to an album.</param>
        /// <param name="allowAddChildAlbum">A value indicating whether the user assigned to this role has permission to create child albums.</param>
        /// <param name="allowEditMediaObject">A value indicating whether the user assigned to this role has permission to edit a media object.</param>
        /// <param name="allowEditAlbum">A value indicating whether the user assigned to this role has permission to edit an album.</param>
        /// <param name="allowDeleteMediaObject">A value indicating whether the user assigned to this role has permission to delete media objects within an album.</param>
        /// <param name="allowDeleteChildAlbum">A value indicating whether the user assigned to this role has permission to delete child albums.</param>
        /// <param name="allowSynchronize">A value indicating whether the user assigned to this role has permission to synchronize an album.</param>
        /// <param name="allowAdministerSite">A value indicating whether the user has administrative permission for all albums. This permission
        /// automatically applies to all albums across all galleries; it cannot be selectively applied.</param>
        /// <param name="allowAdministerGallery">A value indicating whether the user has administrative permission for all albums. This permission
        /// automatically applies to all albums in a particular gallery; it cannot be selectively applied.</param>
        /// <param name="hideWatermark">A value indicating whether the user assigned to this role has a watermark applied to images.
        /// This setting has no effect if watermarks are not used. A true value means the user does not see the watermark;
        /// a false value means the watermark is applied.</param>
        /// <returns>
        /// Returns an <see cref="IGalleryServerRole"/> object corresponding to the specified parameters.
        /// </returns>
        /// <exception cref="InvalidGalleryServerRoleException">Thrown when a role with the specified role name already exists in the data store.</exception>
        public static IGalleryServerRole CreateGalleryServerRoleInstance(string roleName, bool allowViewAlbumOrMediaObject,
                                                                         bool allowViewOriginalImage, bool allowAddMediaObject,
                                                                         bool allowAddChildAlbum, bool allowEditMediaObject,
                                                                         bool allowEditAlbum, bool allowDeleteMediaObject,
                                                                         bool allowDeleteChildAlbum, bool allowSynchronize,
                                                                         bool allowAdministerSite, bool allowAdministerGallery,
                                                                         bool hideWatermark)
        {
            if (LoadGalleryServerRole(roleName) != null)
            {
                throw new InvalidGalleryServerRoleException(Resources.Factory_CreateGalleryServerRoleInstance_Ex_Msg);
            }

            return new GalleryServerRole(roleName, allowViewAlbumOrMediaObject, allowViewOriginalImage, allowAddMediaObject,
                                         allowAddChildAlbum, allowEditMediaObject, allowEditAlbum, allowDeleteMediaObject,
                                         allowDeleteChildAlbum, allowSynchronize, allowAdministerSite, allowAdministerGallery,
                                         hideWatermark);
        }

        /// <overloads>Retrieve a collection of Gallery Server roles.</overloads>
        /// <summary>
        /// Retrieve a collection of all Gallery Server roles. The roles may be returned from a cache. Guaranteed to not return null.
        /// </summary>
        /// <returns>Returns an <see cref="IGalleryServerRoleCollection" /> object that contains all Gallery Server roles.</returns>
        /// <remarks>
        /// The collection of all Gallery Server roles are stored in a cache to improve
        /// performance. <note type = "implementnotes">Note to developer: Any code that modifies the roles in the data store should purge the cache so 
        ///              	that they can be freshly retrieved from the data store during the next request. The cache is identified by the
        ///              	<see cref="CacheItem.GalleryServerRoles" /> enum.</note>
        /// </remarks>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static IGalleryServerRoleCollection LoadGalleryServerRoles()
        {
            var rolesCache = CacheController.GetGalleryServerRolesCache();

            IGalleryServerRoleCollection roles;

            if ((rolesCache != null) && (rolesCache.TryGetValue(GlobalConstants.GalleryServerRoleAllRolesCacheKey, out roles)))
            {
                return roles;
            }

            // No roles in the cache, so get from data store and add to cache.
            roles = GetGalleryServerRolesFromDataStore();

            rolesCache = new ConcurrentDictionary<string, IGalleryServerRoleCollection>();
            rolesCache.TryAdd(GlobalConstants.GalleryServerRoleAllRolesCacheKey, roles);
            CacheController.SetCache(CacheItem.GalleryServerRoles, rolesCache);

            return roles;
        }

        /// <summary>
        /// Retrieve a collection of Gallery Server roles that match the specified <paramref name = "roleNames" />. 
        /// It is not case sensitive, so that "ReadAll" matches "readall". The roles may be returned from a cache.
        ///  Guaranteed to not return null.
        /// </summary>
        /// <param name="roleNames">The name of the roles to return.</param>
        /// <returns>
        /// Returns an <see cref="IGalleryServerRoleCollection" /> object that contains all Gallery Server roles that
        /// match the specified role names.
        /// </returns>
        /// <remarks>
        /// The collection of all Gallery Server roles for the current gallery are stored in a cache to improve
        /// performance. <note type = "implementnotes">Note to developer: Any code that modifies the roles in the data store should purge the cache so 
        ///              	that they can be freshly retrieved from the data store during the next request. The cache is identified by the
        ///              	<see cref="CacheItem.GalleryServerRoles" /> enum.</note>
        /// </remarks>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static IGalleryServerRoleCollection LoadGalleryServerRoles(IEnumerable<string> roleNames)
        {
            return LoadGalleryServerRoles().GetRoles(roleNames);
        }

        /// <overloads>
        /// Retrieve the Gallery Server role that matches the specified role name. The role may be returned from a cache.
        /// Returns null if no matching role is found.
        /// </overloads>
        /// <summary>
        /// Retrieve the Gallery Server role that matches the specified role name. The role may be returned from a cache.
        /// Returns null if no matching role is found.
        /// </summary>
        /// <param name="roleName">The name of the role to return.</param>
        /// <returns>
        /// Returns an <see cref="IGalleryServerRole" /> object that matches the specified role name, or null if no matching role is found.
        /// </returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static IGalleryServerRole LoadGalleryServerRole(string roleName)
        {
            return LoadGalleryServerRole(roleName, false);
        }

        /// <summary>
        /// Retrieve the Gallery Server role that matches the specified role name. When <paramref name="isWritable"/>
        /// is <c>true</c>, then return a unique instance that is not shared across threads, thus creating a thread-safe object that can
        /// be updated and persisted back to the data store. Calling this method with <paramref name="isWritable"/> set to <c>false</c>
        /// is the same as calling the overload of this method that takes only a role name. Returns null if no matching role is found.
        /// </summary>
        /// <param name="roleName">The name of the role to return.</param>
        /// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
        /// <returns>
        /// Returns a writable instance of <see cref="IGalleryServerRole"/> that matches the specified role name, or null if no matching role is found.
        /// </returns>
        public static IGalleryServerRole LoadGalleryServerRole(string roleName, bool isWritable)
        {
            IGalleryServerRole role = LoadGalleryServerRoles().GetRole(roleName);

            if ((role == null) || (!isWritable))
            {
                return role;
            }
            else
            {
                return role.Copy();
            }
        }

        #endregion

        #region AppError Methods

        /// <summary>
        /// Gets a collection of all application events from the data store. The items are sorted in descending order on the
        /// <see cref="IEvent.TimestampUtc" /> property, so the most recent error is first. Returns an empty collection if no
        /// errors exist.
        /// </summary>
        /// <returns>Returns a collection of all application events from the data store.</returns>
        public static IEventCollection GetAppEvents()
        {
            var appEvents = CacheController.GetAppEventsCache();

            if (appEvents != null)
            {
                return appEvents;
            }

            // No events in the cache, so get from data store and add to cache.
            appEvents = EventController.GetAppEvents();

            CacheController.SetCache(CacheItem.AppEvents, appEvents);

            return appEvents;
        }

        /// <summary>
        /// Creates an empty event collection.
        /// </summary>
        /// <returns>An instance of <see cref="IEventCollection" />.</returns>
        public static IEventCollection CreateEventCollection()
        {
            return new EventCollection();
        }

        #endregion

        #region Gallery and Gallery Setting Methods

        /// <summary>
        /// Gets the ID of the template gallery (that is, the one where <see cref="GalleryDto.IsTemplate" /> = <c>true</c>).
        /// </summary>
        /// <returns>System.Int32.</returns>
        public static int GetTemplateGalleryId()
        {
            if (!_templateGalleryId.HasValue)
            {
                using (var repo = new GalleryRepository())
                {
                    _templateGalleryId = repo.Where(g => g.IsTemplate).Select(g => g.GalleryId).FirstOrDefault();
                }
            }

            return _templateGalleryId.Value;
        }

        /// <summary>
        /// Loads the gallery specified by the <paramref name = "galleryId" />. Throws a <see cref="InvalidGalleryException" /> if no matching 
        /// gallery is found or the requested gallery is the template gallery.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>Returns an instance of <see cref="IGallery" /> containing information about the gallery.</returns>
        /// <exception cref="InvalidGalleryException">Thrown when no gallery matching <paramref name="galleryId" /> exists in the data store.</exception>
        public static IGallery LoadGallery(int galleryId)
        {
            return LoadGalleries().FindById(galleryId);
        }

        /// <summary>
        /// Gets a list of all the galleries in the current application. The template gallery is not included. Guaranteed to not be null.
        /// </summary>
        /// <returns>Returns a <see cref="ConcurrentDictionary{TKey,TValue}" /> representing the galleries in the current application.</returns>
        public static IGalleryCollection LoadGalleries()
        {
            if (!_galleriesLoaded)
            {
                lock (_galleries)
                {
                    if (_galleries.Count == 0)
                    {
                        // Ensure that writes related to instantiation are flushed.
                        Thread.MemoryBarrier();

                        using (var repo = new GalleryRepository())
                        {
                            var galleries = repo.Where(g => !g.IsTemplate);

                            foreach (var gallery in galleries)
                            {

                                IGallery g = new Gallery();

                                g.GalleryId = gallery.GalleryId;
                                g.Description = gallery.Description;
                                g.CreationDate = gallery.DateAdded;

                                g.LoadData();

                                _galleries.Add(g);
                            }
                        }

                        _galleriesLoaded = true;
                    }
                }
            }

            return _galleries;
        }

        /// <summary>
        /// Inspect the database for missing records associated with the galleries; inserting if necessary.
        /// </summary>
        public static void ValidateGalleries()
        {
            foreach (var gallery in Factory.LoadGalleries())
            {
                gallery.Validate();
            }
        }

        /// <overloads>
        ///		Loads the gallery settings for the gallery specified by <paramref name = "galleryId" />.
        /// </overloads>
        /// <summary>
        /// Loads a read-only instance of gallery settings for the gallery specified by <paramref name = "galleryId" />. Automatically 
        ///		creates the gallery and	gallery settings if the data is not found in the data store. Guaranteed to not return null, except 
        ///		for when <paramref name = "galleryId" /> is <see cref="Int32.MinValue" />, in which case it throws an <see cref="ArgumentOutOfRangeException" />.
        ///		The returned value is a static instance that is shared across threads, so it should be used only for read-only access. Use
        ///		a different overload of this method to return a writable copy of the instance. Calling this method is the same as calling
        ///		the overloaded method with the isWritable parameter set to false.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>Returns a read-only instance of <see cref="IGallerySettings" />containing  the gallery settings for the gallery specified by 
        /// <paramref name = "galleryId" />. This is a reference to a static variable that may be shared across threads.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the gallery ID is <see cref="Int32.MinValue" />.</exception>
        public static IGallerySettings LoadGallerySetting(int galleryId)
        {
            if (galleryId == Int32.MinValue)
            {
                throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "The gallery ID must be a valid ID. Instead, the value passed was {0}.", galleryId));
            }

            IGallerySettingsCollection gallerySettings = LoadGallerySettings();

            IGallerySettings gs = gallerySettings.FindByGalleryId(galleryId);

            if (gs == null || (!gs.IsInitialized))
            {
                // There isn't an item for the requested gallery ID, *OR* there is an item but it hasn't been initialized (this
                // can happen when an error occurs during initialization, such as a CannotWriteToDirectoryException occurring when checking
                // the media object path).

                // If we didn't find a gallery, create it.
                if (gs == null)
                {
                    IGallery gallery = CreateGalleryInstance();
                    gallery.GalleryId = galleryId;

                    gallery.Validate();
                    gallery.LoadData();

                    // Need to clear the gallery server roles so that they are reloaded from the data store, which should now include sys admin
                    // permission to the new gallery.
                    CacheController.PurgeCache();
                }

                // Reload the data from the data store.
                _gallerySettings.Clear();
                gallerySettings = LoadGallerySettings();

                gs = gallerySettings.FindByGalleryId(galleryId);

                if (gs == null)
                {
                    throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Factory.LoadGallerySetting() should have created gallery setting records for gallery {0}, but it has not.", galleryId));
                }
            }

            return gs;
        }

        /// <summary>
        /// Loads the gallery settings for the gallery specified by <paramref name="galleryId"/>. When <paramref name="isWritable"/>
        /// is <c>true</c>, then return a unique instance that is not shared across threads, thus creating a thread-safe object that can
        /// be updated and persisted back to the data store. Calling this method with <paramref name="isWritable"/> set to <c>false</c>
        /// is the same as calling the overload of this method that takes only a gallery ID. Guaranteed to not return null, except for when <paramref name="galleryId"/>
        /// is <see cref="Int32.MinValue"/>, in which case it throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
        /// <returns>
        /// Returns a writable instance of <see cref="IGallerySettings"/>containing  the gallery settings for the gallery specified by
        /// <paramref name="galleryId"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the gallery ID is <see cref="Int32.MinValue"/>.</exception>
        public static IGallerySettings LoadGallerySetting(int galleryId, bool isWritable)
        {
            if (galleryId == Int32.MinValue)
            {
                throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "The gallery ID must be a valid ID. Instead, the value passed was {0}.", galleryId));
            }

            if (isWritable)
            {
                IGallerySettings gallerySettings = GallerySettings.RetrieveGallerySettingsFromDataStore(galleryId);
                gallerySettings.IsWritable = true;
                return gallerySettings;
            }
            else
            {
                return LoadGallerySetting(galleryId);
            }
        }

        /// <summary>
        /// Loads the settings for all galleries in the application. Guaranteed to not return null.
        /// </summary>
        /// <returns>Returns an <see cref="IGallerySettingsCollection" /> containing settings for all galleries in the application.</returns>
        public static IGallerySettingsCollection LoadGallerySettings()
        {
            lock (_gallerySettings)
            {
                if (_gallerySettings.Count == 0)
                {
                    // Ensure that writes related to instantiation are flushed.
                    Thread.MemoryBarrier();

                    _gallerySettings.AddRange(GallerySettings.RetrieveGallerySettingsFromDataStore());
                }
            }

            return _gallerySettings;
        }

        /// <summary>
        /// Loads the settings for all galleries in the application. Guaranteed to not return null.
        /// </summary>
        /// <returns>Returns an <see cref="IGallerySettingsCollection" /> containing settings for all galleries in the application.</returns>
        public static IGalleryControlSettingsCollection LoadGalleryControlSettings()
        {
            lock (_galleryControlSettings)
            {
                if (_galleryControlSettings.Count == 0)
                {
                    // Ensure that writes related to instantiation are flushed.
                    Thread.MemoryBarrier();

                    _galleryControlSettings.AddRange(GalleryControlSettings.RetrieveGalleryControlSettingsFromDataStore());
                }
            }

            return _galleryControlSettings;
        }

        /// <summary>
        /// Clears all in-memory representations of data.
        /// </summary>
        public static void ClearAllCaches()
        {
            ClearGalleryCache();
            ClearGallerySettingsCache();
            ClearGalleryControlSettingsCache();
            ClearWatermarkCache();
            foreach (CacheItem cacheItem in Enum.GetValues(typeof(CacheItem)))
            {
                CacheController.RemoveCache(cacheItem);
            }
        }

        /// <summary>
        /// Clears the in-memory copy of the current set of gallery control settings. This will force a database retrieval the next time
        /// they are requested.
        /// </summary>
        public static void ClearGalleryCache()
        {

            lock (_galleries)
            {
                _galleries.Clear();
                _galleriesLoaded = false;
            }
        }

        /// <summary>
        /// Clears the in-memory copy of the current set of gallery control settings. This will force a database retrieval the next time
        /// they are requested.
        /// </summary>
        public static void ClearGalleryControlSettingsCache()
        {
            _galleryControlSettings.Clear();
        }

        /// <summary>
        /// Clears the in-memory copy of the current set of watermarks.
        /// </summary>
        public static void ClearWatermarkCache()
        {
            _watermarks.Clear();
        }

        /// <overloads>Loads the gallery control settings for the specified <paramref name="controlId"/>.</overloads>
        /// <summary>
        /// Loads the gallery control settings for the specified <paramref name="controlId"/>.
        /// </summary>
        /// <param name="controlId">The value that uniquely identifies the control containing the gallery. Example: "Default.aspx|gsp"</param>
        /// <returns>
        /// Returns an instance of <see cref="IGalleryControlSettings"/>containing  the gallery control settings for the gallery 
        /// control specified by <paramref name="controlId"/>.
        /// </returns>
        public static IGalleryControlSettings LoadGalleryControlSetting(string controlId)
        {
            return LoadGalleryControlSetting(controlId, false);
        }

        /// <summary>
        /// Loads the gallery control settings for the specified <paramref name="controlId"/> (case insensitive). When <paramref name="isWritable"/>
        /// is <c>true</c>, then return a unique instance that is not shared across threads, thus creating a thread-safe object that can
        /// be updated and persisted back to the data store. Calling this method with <paramref name="isWritable"/> set to <c>false</c>
        /// is the same as calling the overload of this method that takes only a control ID. Guaranteed to not return null.
        /// </summary>
        /// <param name="controlId">The value that uniquely identifies the control containing the gallery. Example: "Default.aspx|gsp"</param>
        /// <param name="isWritable">When set to <c>true</c> then return a unique instance that is not shared across threads.</param>
        /// <returns>
        /// Returns a writable instance of <see cref="IGalleryControlSettings"/>containing  the gallery control settings for the gallery 
        /// control specified by <paramref name="controlId"/>.
        /// </returns>
        public static IGalleryControlSettings LoadGalleryControlSetting(string controlId, bool isWritable)
        {
            IGalleryControlSettings galleryControlSettings;

            if (isWritable)
            {
                galleryControlSettings = GalleryControlSettings.RetrieveGalleryControlSettingsFromDataStore().FindByControlId(controlId);
            }
            else
            {
                galleryControlSettings = LoadGalleryControlSettings().FindByControlId(controlId);
            }

            if (galleryControlSettings == null)
            {
                galleryControlSettings = new GalleryControlSettings(Int32.MinValue, controlId);
            }

            return galleryControlSettings;
        }

        /// <summary>
        /// Gets the watermark instance for the specified <paramref name="galleryId" />.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>Returns a <see cref="Watermark" /> instance for the specified <paramref name="galleryId" />.</returns>
        public static Watermark GetWatermarkInstance(int galleryId)
        {
            if (galleryId == Int32.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(galleryId), String.Format(CultureInfo.CurrentCulture, "The gallery ID must be a valid ID. Instead, the value passed was {0}.", galleryId));
            }

            Watermark watermark;

            if (!_watermarks.TryGetValue(galleryId, out watermark))
            {
                // A watermark object for the gallery was not found. Create it and add it to the dictionary.
                Watermark tempWatermark = AppSetting.Instance.License.LicenseType == LicenseLevel.TrialExpired ? Watermark.GetReducedFunctionalityModeWatermark(galleryId) : Watermark.GetUserSpecifiedWatermark(galleryId);

                _watermarks.TryAdd(galleryId, tempWatermark);

                watermark = tempWatermark;
            }

            return watermark;
        }

        #endregion

        #region General

        /// <summary>
        /// Gets an instance of the HTML validator.
        /// </summary>
        /// <param name="html">The HTML to pass to the HTML validator.</param>
        /// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
        /// <returns>Returns an instance of the HTML validator.</returns>
        public static IHtmlValidator GetHtmlValidator(string html, int galleryId)
        {
            return HtmlValidator.Create(html, galleryId);
        }

        /// <summary>
        /// Retrieves a singleton object that represents the current state of a synchronization in the specified gallery, retrieving it from
        /// the data store if necessary. This method will also insert a record into the Synchronize table if one is not present for the 
        /// specified <paramref name="galleryId" />. It also determines if an app restart has interrupted a previous sync and assigns the 
        /// <see cref="ISynchronizationStatus.Status" /> to <see cref="SynchronizationState.InterruptedByAppRecycle" />. if required.
        /// Guaranteed to not return null.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>Returns an instance of <see cref="ISynchronizationStatus" /> that represents the current state of a 
        /// synchronization in a particular gallery.</returns>
        public static ISynchronizationStatus LoadSynchronizationStatus(int galleryId)
        {
            ISynchronizationStatus syncStatus;

            if (!_syncStatuses.TryGetValue(galleryId, out syncStatus))
            {
                // Don't have it in memory yet. Get it from the DB, inserting a record if necessary.
                using (var repo = new SynchronizeRepository())
                {
                    var sDto = repo.Find(galleryId);

                    if (sDto == null)
                    {
                        sDto = new SynchronizeDto { FKGalleryId = galleryId, SynchId = string.Empty, SynchState = SynchronizationState.Complete, CurrentFileIndex = 0, TotalFiles = 0 };
                        repo.Add(sDto);
                        repo.Save();
                    }

                    syncStatus = new SynchronizationStatus(galleryId, sDto.SynchId, sDto.SynchState, sDto.TotalFiles, string.Empty, sDto.CurrentFileIndex, string.Empty);
                }

                _syncStatuses.TryAdd(galleryId, syncStatus);
            }

            return syncStatus;
        }

        /// <summary>
        /// Create a new <see cref="IMediaTemplate" /> instance with properties set to default values.
        /// A valid <see cref="IMediaTemplate.MediaTemplateId">ID</see> will be generated when the object is persisted to the data store 
        /// when saved. Guaranteed to not return null.
        /// </summary>
        /// <returns>Returns an <see cref="IMediaTemplate" /> instance.</returns>
        public static IMediaTemplate CreateEmptyMediaTemplate()
        {
            return new MediaTemplate();
        }

        /// <summary>
        /// Gets a collection of the media templates from the data store. The items may be returned from a cache.
        /// Returns an empty collection if no items exist.
        /// </summary>
        /// <returns>Returns a <see cref="IMediaTemplateCollection" /> representing the media templates in the current application.</returns>
        public static IMediaTemplateCollection LoadMediaTemplates()
        {
            var tmpl = CacheController.GetMediaTemplatesCache();

            if (tmpl != null)
            {
                return tmpl;
            }

            // Nothing in the cache, so get from data store and add to cache.
            using (var repo = new MediaTemplateRepository())
            {
                tmpl = repo.GetMediaTemplates(new MediaTemplateCollection());
            }

            CacheController.SetCache(CacheItem.MediaTemplates, tmpl);

            return tmpl;
        }

        /// <summary>
        /// Gets a collection of the media templates from the data store. The items may be returned from a cache.
        /// Returns an empty collection if no items exist.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>Returns a <see cref="IMediaTemplateCollection" /> representing the media templates in the current application.</returns>
        public static IMimeTypeCollection LoadMimeTypes(int galleryId = Int32.MinValue)
        {
            var mimeTypesCache = CacheController.GetMimeTypesCache();

            IMimeTypeCollection mimeTypes;

            if ((mimeTypesCache != null) && (mimeTypesCache.TryGetValue(galleryId, out mimeTypes)))
            {
                return mimeTypes;
            }

            // Nothing in the cache, so get from data store and add to cache.
            mimeTypes = MimeType.LoadMimeTypes(galleryId);

            if (mimeTypesCache == null)
            {
                mimeTypesCache = new ConcurrentDictionary<int, IMimeTypeCollection>();
            }

            lock (_sharedLock)
            {
                if (!mimeTypesCache.ContainsKey(galleryId))
                {
                    mimeTypesCache.TryAdd(galleryId, mimeTypes);

                    CacheController.SetCache(CacheItem.MimeTypes, mimeTypesCache);
                }
            }

            return mimeTypes;
        }

        /// <overloads>
        /// Loads a <see cref="IMimeType" /> object corresponding to the extension of the specified file.
        /// </overloads>
        /// <summary>
        /// Loads a <see cref="IMimeType" /> object corresponding to the extension of the specified <paramref name="filePath" />.
        /// The returned instance is not associated with a particular gallery (that is, <see cref="IMimeType.GalleryId" /> is set 
        /// to <see cref="Int32.MinValue" />) and the <see cref="IMimeType.AllowAddToGallery" /> property is <c>false</c>. If 
        /// no matching MIME type is found, this method returns null.
        /// </summary>
        /// <param name="filePath">A string representing the filename or the path to the file
        /// (e.g. "C:\mypics\myprettypony.jpg", "myprettypony.jpg"). It is not case sensitive.</param>
        /// <returns>
        /// Returns a <see cref="IMimeType" /> instance corresponding to the specified filepath, or null if no matching MIME
        /// type is found.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="filePath" /> contains one or more of
        /// the invalid characters defined in <see cref="System.IO.Path.GetInvalidPathChars" />, or contains a wildcard character.</exception>
        public static IMimeType LoadMimeType(string filePath)
        {
            return LoadMimeType(Int32.MinValue, filePath);
        }

        /// <summary>
        /// Loads a <see cref="IMimeType"/> object corresponding to the specified <paramref name="galleryId" /> and extension 
        /// of the specified <paramref name="filePath"/>. When <paramref name="galleryId" /> is <see cref="Int32.MinValue"/>, the 
        /// returned instance is not associated with a particular gallery (that is, <see cref="IMimeType.GalleryId"/> is set
        /// to <see cref="Int32.MinValue"/>) and the <see cref="IMimeType.AllowAddToGallery"/> property is <c>false</c>. When 
        /// <paramref name="galleryId" /> is specified, then the <see cref="IMimeType.AllowAddToGallery"/> property is set according
        /// to the gallery's configuration. If no matching MIME type is found, this method returns null.
        /// </summary>
        /// <param name="galleryId">The ID representing the gallery associated with the file stored at <paramref name="filePath" />.
        /// Specify <see cref="Int32.MinValue"/> when the gallery is not known or relevant. Setting this parameter will cause the
        /// <see cref="IMimeType.AllowAddToGallery"/> property to be set according to the gallery's configuration.</param>
        /// <param name="filePath">A string representing the filename or the path to the file
        /// (e.g. "C:\mypics\myprettypony.jpg", "myprettypony.jpg"). It is not case sensitive.</param>
        /// <returns>
        /// Returns a <see cref="IMimeType"/> instance corresponding to the specified <paramref name="galleryId" /> and extension 
        /// of the specified <paramref name="filePath"/>, or null if no matching MIME type is found.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="filePath"/> contains one or more of
        /// the invalid characters defined in <see cref="System.IO.Path.GetInvalidPathChars"/>, or contains a wildcard character.</exception>
        public static IMimeType LoadMimeType(int galleryId, string filePath)
        {
            return LoadMimeTypes(galleryId).Find(Path.GetExtension(filePath));
        }

        /// <summary>
        /// Gets the connection string settings for the connection string associated with the gallery data.
        /// </summary>
        /// <returns>An instance of <see cref="System.Configuration.ConnectionStringSettings" />.</returns>
        public static ConnectionStringSettings GetConnectionStringSettings()
        {
            return Data.Utils.GetConnectionStringSettings();
        }

        /// <summary>
        /// Gets the name of the connection string for the gallery data.
        /// </summary>
        /// <returns>System.String.</returns>
        public static string GetConnectionStringName()
        {
            using (var repo = new GalleryRepository())
            {
                return repo.ConnectionStringName;
            }
        }

        #endregion

        #region Profile Methods

        /// <summary>
        /// Retrieves the profile for the specified <paramref name="userName" />. The profile is 
        /// retrieved from the cache if it is there. If not, it is retrieved from the data store and
        /// added to the cache. Guaranteed to not return null.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Returns the profile for the specified <paramref name="userName" /></returns>
        public static IUserProfile LoadUserProfile(string userName)
        {
            var profileCache = CacheController.GetProfilesCache();

            IUserProfile profile;
            if ((profileCache == null) || !profileCache.TryGetValue(userName, out profile))
            {
                //profile = GetDataProvider().Profile_GetUserProfile(userName, new Factory());
                profile = UserProfile.RetrieveFromDataStore(userName);

                // Add profile to cache.
                if (profileCache == null)
                {
                    profileCache = new ConcurrentDictionary<string, IUserProfile>();
                }

                profileCache.TryAdd(userName, profile);

                CacheController.SetCache(CacheItem.Profiles, profileCache);
            }

            return profile;
        }

        /// <summary>
        /// Persist the user profile to the data store. The profile cache is cleared.
        /// </summary>
        /// <param name="userProfile">The user profile.</param>
        public static void SaveUserProfile(IUserProfile userProfile)
        {
            //GetDataProvider().Profile_Save(userProfile);
            UserProfile.Save(userProfile);

            CacheController.RemoveCache(CacheItem.Profiles);
        }

        /// <summary>
        /// Permanently delete the profile records for the specified <paramref name="userName" />.
        /// The profile cache is cleared.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public static void DeleteUserProfile(string userName)
        {
            //GetDataProvider().Profile_DeleteProfileForUser(userName);
            using (var repo = new ProfileRepository())
            {
                foreach (var pDto in repo.Where(p => p.UserName == userName))
                {
                    repo.Delete(pDto);
                }

                repo.Save();
            }

            CacheController.RemoveCache(CacheItem.Profiles);
        }

        /// <summary>
        /// Permanently delete the profile records associated with the specified <paramref name="gallery" />.
        /// </summary>
        /// <param name="gallery">The gallery.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
        public static void DeleteProfileForGallery(Gallery gallery)
        {
            if (gallery == null)
                throw new ArgumentNullException("gallery");

            using (var repo = new ProfileRepository())
            {
                foreach (var pDto in repo.Where(p => p.FKGalleryId == gallery.GalleryId))
                {
                    repo.Delete(pDto);
                }

                repo.Save();
            }

            CacheController.RemoveCache(CacheItem.Profiles);
        }

        /// <summary>
        /// Create a new <see cref="IAlbumProfile" /> item from the specified parameters.
        /// </summary>
        /// <param name="albumId">The album ID.</param>
        /// <param name="sortByMetaName">Name of the metadata item to sort by.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort the album in ascending order.</param>
        /// <returns>An instance of <see cref="IAlbumProfile" />.</returns>
        public static IAlbumProfile CreateAlbumProfile(int albumId, MetadataItemName sortByMetaName, bool sortAscending)
        {
            return new AlbumProfile(albumId, sortByMetaName, sortAscending);
        }

        /// <summary>
        /// Create a new <see cref="IMediaObjectProfile" /> item from the specified parameters.
        /// </summary>
        /// <param name="mediayObjectId">The mediay object ID.</param>
        /// <param name="rating">The rating a user has assigned to the media object.</param>
        /// <returns>An instance of <see cref="IMediaObjectProfile" />.</returns>
        public static IMediaObjectProfile CreateMediaObjectProfile(int mediayObjectId, string rating)
        {
            return new MediaObjectProfile(mediayObjectId, rating);
        }

        #endregion

        #region UI Template Methods

        /// <summary>
        /// Gets a collection of all UI templates from the data store. The items may be returned from a cache.
        /// Returns an empty collection if no items exist.
        /// </summary>
        /// <returns>Returns a collection of all UI templates from the data store.</returns>
        public static IUiTemplateCollection LoadUiTemplates()
        {
            var tmpl = CacheController.GetUiTemplatesCache();

            if (tmpl != null)
            {
                return tmpl;
            }

            // Nothing in the cache, so get from data store and add to cache.
            tmpl = UiTemplate.GetUiTemplates();

            CacheController.SetCache(CacheItem.UiTemplates, tmpl);

            return tmpl;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets a collection of album IDs from the root album ID down to the parent of the <paramref name="albumId" />. The return instance
        /// does not include <paramref name="albumId" />. Guaranteed to not return null.
        /// </summary>
        /// <param name="albumId">The album ID.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the <paramref name="albumId" /> was not found in any of the 
        /// <see cref="IGallery.AlbumHierarchies" /> properties of all galleries.</exception>
        private static List<int> GetAlbumHierarchy(int albumId)
        {
            foreach (var gallery in Factory.LoadGalleries())
            {
                List<int> albumHierarchy;
                if (gallery.AlbumHierarchies.TryGetValue(albumId, out albumHierarchy))
                {
                    return albumHierarchy;
                }
            }

            throw new InvalidAlbumException($"Could not find album ID {albumId} in any of the galleries.");
        }

        /// <summary>
        /// Retrieves the media object from data store. Guaranteed to not return null.
        /// </summary>
        /// <param name="mediaObjectId">The media object identifier.</param>
        /// <param name="options">The options that specify the configuration of the returned media asset. When the <see cref="MediaLoadOptions.Album" />
        /// property is null, an instance of <see cref="IAlbum" /> is created based on album ID retrieved from the data store.</param>
        /// <returns>An instance of <see cref="IGalleryObject" />.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when <paramref name="mediaObjectId" /> does not correspond to an existing
        /// media asset in the data store.</exception>
        private static IGalleryObject RetrieveMediaObjectFromDataStore(int mediaObjectId, MediaLoadOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            bool includeMetadata = (options.Album == null || options.Album.AllowMetadataLoading);

            MediaObjectDto moDto = GetMediaObjectById(mediaObjectId, includeMetadata);

            if (moDto == null)
            {
                throw new InvalidMediaObjectException(mediaObjectId);
            }

            var mo = GetMediaObjectFromDto(moDto, options.Album);

            mo.IsWritable = options.IsWritable;

            return mo;
        }

        private static MediaObjectDto GetMediaObjectById(int mediaObjectId, bool includeMetadata)
        {
            using (var repo = new MediaObjectRepository())
            {
                return (includeMetadata ? repo.Where(m => m.MediaObjectId == mediaObjectId, m => m.Metadata).FirstOrDefault() : repo.Find(mediaObjectId));
            }
        }

        /// <summary>
        /// Creates an inflated album for the the specified <paramref name="albumId" /> by retrieving it from the data store. Child albums and 
        /// media objects are not added (<see cref="IAlbum.AreChildrenInflated" /> = <c>false</c>). Guaranteed to not return null.
        /// </summary>
        /// <param name="albumId">The ID that uniquely identifies the album to retrieve.</param>
        /// <param name="parentAlbum">The album containing the <paramref name="albumId" />. May be null. When null, an instance of <see cref="IAlbum" />
        /// is created.</param>
        /// <returns>An instance implementing <see cref="IAlbum" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when an album with the specified album ID is not found in the data store.</exception>
        private static IAlbum RetrieveAlbumFromDataStore(int albumId, IAlbum parentAlbum)
        {
            IAlbum album;

            try
            {
                using (var repo = new AlbumRepository())
                {
                    album = GetAlbumFromDto(repo.Where(a => a.AlbumId == albumId, m => m.Metadata).FirstOrDefault(), parentAlbum);
                }
            }
            catch (ArgumentNullException)
            {
                // No record was found with the album ID.
                throw new InvalidAlbumException(albumId);
            }

            Debug.Assert(album.ThumbnailMediaObjectId > Int32.MinValue, "The album's ThumbnailMediaObjectId should have been assigned in this method.");

            return album;
        }

        /// <summary>
        /// Adds and inflates the child albums and media objects to the <paramref name="album" />.
        /// The <see cref="IAlbum.AreChildrenInflated" /> property of <paramref name="album" /> is set to <c>true</c>.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <param name="albumAsset">The album asset. May be null. When null, the child objects are retrieved from the data store.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <see cref="IAlbum.AreChildrenInflated" /> property of <paramref name="album" />
        /// is <c>true</c>.</exception>
        private static void AddChildObjects(IAlbum album, CacheItemAlbum albumAsset)
        {
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (album.AreChildrenInflated)
            {
                throw new ArgumentException("It is invalid to call AddChildObjects() for an album when AreChildrenInflated is already set to true.", nameof(album));
            }

            if (albumAsset != null)
            {
                AddChildObjectsFromAsset(album, albumAsset);
            }
            else
            {
                AddChildObjectsFromDataStore(album);
            }
        }

        /// <summary>
        /// Adds and inflates the child albums and media objects to the <paramref name="album" /> based on the
        /// <see cref="CacheItemAlbum.ChildAlbumIds" /> and <see cref="CacheItemAlbum.ChildMediaObjectIds" /> properties 
        /// of <paramref name="albumAsset" />.
        /// The <see cref="IAlbum.AreChildrenInflated" /> property of <paramref name="album" /> is set to <c>true</c>.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <param name="albumAsset">The album asset.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="albumAsset" /> is null.</exception>
        private static void AddChildObjectsFromAsset(IAlbum album, CacheItemAlbum albumAsset)
        {
            lock (_sharedLock)
            {
                if (album.AreChildrenInflated)
                {
                    // Another thread added the children, so just return.
                    return;
                }

                if (albumAsset == null)
                    throw new ArgumentNullException(nameof(albumAsset));

                #region Add child albums

                foreach (var childAlbumId in albumAsset.ChildAlbumIds.Keys)
                {
                    try
                    {
                        album.AddGalleryObject(LoadAlbumInstance(new AlbumLoadOptions(childAlbumId) { IsWritable = album.IsWritable, AllowMetadataLoading = album.AllowMetadataLoading }));
                    }
                    catch (InvalidAlbumException ex)
                    {
                        if (!ex.Data.Contains("ADDITIONAL INFO"))
                        {
                            ex.Data.Add("ADDITIONAL INFO", $"This error was gracefully handled and did not disrupt the user. It occurred because the function AddChildObjectsFromAsset() tried to load an album with ID {childAlbumId} that it retrieved from the ChildAlbumIds property of the cached album asset having ID {albumAsset.Id}. This can occur in multi-threaded scenarios and is not cause for alarm. If it occurs frequently, however, there may be a bug and this should be reported.");
                        }

                        Events.EventController.RecordError(ex, AppSetting.Instance, album.GalleryId, Factory.LoadGallerySettings());
                    }
                }

                #endregion

                #region Add child media objects

                foreach (var childMediaId in albumAsset.ChildMediaObjectIds.Keys)
                {
                    try
                    {
                        album.AddGalleryObject(LoadMediaObjectInstance(new MediaLoadOptions(childMediaId) { Album = album, IsWritable = album.IsWritable }));
                    }
                    catch (InvalidMediaObjectException ex)
                    {
                        if (!ex.Data.Contains("ADDITIONAL INFO"))
                        {
                            ex.Data.Add("ADDITIONAL INFO", $"This error was gracefully handled and did not disrupt the user. It occurred because the function AddChildObjectsFromAsset() tried to load a media asset with ID {childMediaId} that it retrieved from the ChildMediaObjectIds property of the cached album asset having ID {albumAsset.Id}. This can occur in multi-threaded scenarios and is not cause for alarm. If it occurs frequently, however, there may be a bug and this should be reported.");
                        }

                        Events.EventController.RecordError(ex, AppSetting.Instance, album.GalleryId, Factory.LoadGallerySettings());
                    }
                }

                #endregion

                album.AreChildrenInflated = true;

                foreach (var childAssset in album.GetChildGalleryObjects())
                {
                    childAssset.HasChanges = false;
                }
            }
        }

        /// <summary>
        /// Adds and inflates the child albums and media objects to the <paramref name="album" /> from the data store.
        /// The <see cref="IAlbum.AreChildrenInflated" /> property of <paramref name="album" /> is set to <c>true</c>.
        /// </summary>
        /// <param name="album">The album.</param>
        private static void AddChildObjectsFromDataStore(IAlbum album)
        {
            lock (_sharedLock)
            {
                if (album.AreChildrenInflated)
                {
                    // Another thread added the children, so just return.
                    return;
                }

                #region Add child albums

                using (var repo = new AlbumRepository())
                {
                    foreach (int albumId in repo.Where(a => a.FKAlbumParentId == album.Id).Select(a => a.AlbumId))
                    {
                        album.AddGalleryObject(LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = album.IsWritable }));
                    }
                }

                #endregion

                #region Add child media objects

                using (var repo = new MediaObjectRepository())
                {
                    var moDtos = (album.AllowMetadataLoading ? repo.Where(m => m.FKAlbumId == album.Id, m => m.Metadata) : repo.Where(m => m.FKAlbumId == album.Id));

                    foreach (MediaObjectDto moDto in moDtos)
                    {
                        var mediaObject = GetMediaObjectFromDto(moDto, album);

                        CacheController.AddToMediaAssetCache(CacheItemMedia.CreateFrom(mediaObject));

                        album.AddGalleryObject(mediaObject);
                    }
                }

                #endregion

                album.AreChildrenInflated = true;
            }
        }

        private static void InflateAlbumFromDto(IAlbum album, AlbumDto albumDto)
        {
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            if (albumDto == null)
                throw new ArgumentNullException(nameof(albumDto));

            // A parent ID = null indicates the root album. Use int.MinValue to send to Album constructor.
            int albumParentId = albumDto.FKAlbumParentId.GetValueOrDefault(Int32.MinValue);

            // Assign parent if it hasn't already been assigned.
            if ((album.Parent.Id == Int32.MinValue) && (albumParentId > Int32.MinValue))
            {
                album.Parent = LoadAlbumInstance(albumParentId);
            }

            album.GalleryId = albumDto.FKGalleryId;
            //album.Title = albumDto.Title;
            album.DirectoryName = albumDto.DirectoryName;
            //album.Summary = albumDto.Summary;
            album.SortByMetaName = albumDto.SortByMetaName;
            album.SortAscending = albumDto.SortAscending;
            album.Sequence = albumDto.Seq;
            album.CreatedByUserName = albumDto.CreatedBy.Trim();
            album.DateAdded = HelperFunctions.ToDateTime(albumDto.DateAdded);
            album.LastModifiedByUserName = albumDto.LastModifiedBy.Trim();
            album.DateLastModified = HelperFunctions.ToDateTime(albumDto.DateLastModified);
            album.OwnerUserName = albumDto.OwnedBy.Trim();
            album.OwnerRoleName = albumDto.OwnerRoleName.Trim();
            album.IsPrivate = albumDto.IsPrivate;

            // Set the album's thumbnail media object ID. Setting this property sets an internal flag that will cause
            // the media object info to be retrieved when the Thumbnail property is accessed. That's why we don't
            // need to set any of the thumbnail properties.
            // WARNING: No matter what, do not call DisplayObject.CreateInstance() because that creates a new object, 
            // and we might be  executing this method from within our Thumbnail display object. Trust me, this 
            // creates hard to find bugs!
            album.ThumbnailMediaObjectId = albumDto.ThumbnailMediaObjectId;

            album.AddMeta(GalleryObjectMetadataItemCollection.FromMetaDtos(album, albumDto.Metadata));
        }

        private static IGalleryServerRoleCollection GetRolesFromRoleDtos(IEnumerable<RoleDto> roleDtos)
        {
            IGalleryServerRoleCollection roles = new GalleryServerRoleCollection();

            foreach (RoleDto roleDto in roleDtos)
            {
                IGalleryServerRole role = new GalleryServerRole(
                  roleDto.RoleName,
                  roleDto.AllowViewAlbumsAndObjects,
                  roleDto.AllowViewOriginalImage,
                  roleDto.AllowAddMediaObject,
                  roleDto.AllowAddChildAlbum,
                  roleDto.AllowEditMediaObject,
                  roleDto.AllowEditAlbum,
                  roleDto.AllowDeleteMediaObject,
                  roleDto.AllowDeleteChildAlbum,
                  roleDto.AllowSynchronize,
                  roleDto.AllowAdministerSite,
                  roleDto.AllowAdministerGallery,
                  roleDto.HideWatermark);

                role.RootAlbumIds.AddRange(from r in roleDto.RoleAlbums select r.FKAlbumId);

                roles.Add(role);
            }

            return roles;
        }

        /// <summary>
        /// Get all Gallery Server roles for the current gallery. Guaranteed to not return null.
        /// </summary>
        /// <returns>Returns all Gallery Server roles for the current gallery.</returns>
        private static IGalleryServerRoleCollection GetGalleryServerRolesFromDataStore()
        {
            // Create the roles.
            IGalleryServerRoleCollection roles;
            using (var repo = new RoleRepository())
            {
                //IGalleryServerRoleCollection roles = GetRolesFromRoleDtos(GetDataProvider().Roles_GetRoles());
                roles = GetRolesFromRoleDtos(repo.GetAll(r => r.RoleAlbums));
            }

            IGalleryCollection galleries = LoadGalleries();
            foreach (IGalleryServerRole role in roles)
            {
                role.Inflate(galleries);
            }

            return roles;
        }

        /// <summary>
        /// Create a new top-level album for the specified <paramref name = "galleryId" /> and persist to the data store. The newly created
        /// album is returned. Guaranteed to not return null.
        /// </summary>
        /// <param name="galleryId">The gallery ID for which the new album is to be the root album.</param>
        /// <returns>Returns an <see cref="Album" /> instance representing the top-level album for the specified <paramref name = "galleryId" />.</returns>
        private static IAlbum CreateRootAlbum(int galleryId)
        {
            IAlbum album = CreateEmptyAlbumInstance(galleryId, true);

            DateTime currentTimestamp = DateTime.UtcNow;

            album.Parent.Id = 0; // The parent ID of the root album is always zero.
            album.Title = Resources.Root_Album_Default_Title;
            album.DirectoryName = String.Empty; // The root album must have an empty directory name;
            album.Caption = Resources.Root_Album_Default_Summary;
            album.CreatedByUserName = GlobalConstants.SystemUserName;
            album.DateAdded = currentTimestamp;
            album.LastModifiedByUserName = GlobalConstants.SystemUserName;
            album.DateLastModified = currentTimestamp;

            album.Save();

            return album;
        }

        /// <summary>
        /// Syncs the metadata value with the corresponding property on the album or media object.
        /// The album/media object properties are deprecated in version 3, but we want to sync them
        /// for backwards compatibility. A future version is expected to remove these properties, at
        /// which time this method will no longer be needed.
        /// </summary>
        /// <param name="md">An instance of <see cref="IGalleryObjectMetadataItem" /> being persisted to the data store.</param>
        /// <param name="userName">The user name of the currently logged on user. This will be used for the audit fields.</param>
        private static void SyncWithGalleryObjectProperties(IGalleryObjectMetadataItem md, string userName)
        {
            if ((md.MetadataItemName == MetadataItemName.Title) && (md.GalleryObject.GalleryObjectType == GalleryObjectType.Album))
            {
                var album = (IAlbum)md.GalleryObject;

                // If necessary, sync the directory name with the album title.
                var gs = LoadGallerySetting(album.GalleryId);
                if ((!album.IsRootAlbum) && (!album.IsVirtualAlbum) && (gs.SynchAlbumTitleAndDirectoryName))
                {
                    // Root albums do not have a directory name that reflects the album's title, so only update this property for non-root albums.
                    if (!album.DirectoryName.Equals(album.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        // We only update the directory name when it is different. Without this check a user saving a 
                        // title without any changes would cause the directory name to get changed (e.g. 'Samples'
                        // might get changed to 'Sample(1)')
                        var albumWritable = LoadAlbumInstance(new AlbumLoadOptions(album.Id) { IsWritable = true });
                        albumWritable.DirectoryName = HelperFunctions.ValidateDirectoryName(album.Parent.FullPhysicalPath, album.Title, gs.DefaultAlbumDirectoryNameLength);

                        HelperFunctions.UpdateAuditFields(albumWritable, userName);
                        albumWritable.Save();
                    }
                }
            }

            if (md.MetadataItemName == MetadataItemName.FileName)
            {
                // We are editing the filename item, so we want to rename the actual media file. Remove it from cache so that we get the new filename when we
                // load the media object, then trigger a save. The save routine will detect the metadata change and perform the rename for us.
                CacheController.PurgeCache(md.GalleryObject);
                var mediaObject = LoadMediaObjectInstance(new MediaLoadOptions(md.GalleryObject.Id) { IsWritable = true, Album = (IAlbum)md.GalleryObject.Parent });
                HelperFunctions.UpdateAuditFields(mediaObject, userName);
                mediaObject.Save();
            }

            if (md.MetadataItemName == MetadataItemName.HtmlSource)
            {
                // We are editing the HTML content. This is the same as the media object's ExternalHtmlSource, so update that item, too. As with the logic 
                // above, we remove the media object from cache so we get the updated HTML when we load the writable media object, then we save.
                CacheController.PurgeCache(md.GalleryObject);
                var mediaObject = LoadMediaObjectInstance(new MediaLoadOptions(md.GalleryObject.Id) { IsWritable = true, Album = (IAlbum)md.GalleryObject.Parent });

                // Verify the media object is an external one. It always should be, but we'll double check to be sure.
                if (mediaObject.GalleryObjectType == GalleryObjectType.External)
                {
                    mediaObject.Original.ExternalHtmlSource = md.Value;
                    HelperFunctions.UpdateAuditFields(mediaObject, userName);
                    mediaObject.Save();
                }
            }
        }

        /// <summary>
        /// Clears the in-memory copy of the current set of gallery settings. This will force a database retrieval the next time
        /// they are requested.
        /// </summary>
        private static void ClearGallerySettingsCache()
        {
            _gallerySettings.Clear();
        }

        #endregion
    }
}