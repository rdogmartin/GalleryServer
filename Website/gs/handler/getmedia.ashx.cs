using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.SessionState;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using Image = System.Drawing.Image;

namespace GalleryServer.Web.Handler
{
    /// <summary>
    /// Defines a handler that sends the specified media object to the output stream.
    /// </summary>
    [System.Web.Services.WebService(Namespace = "http://tempuri.org/")]
    [System.Web.Services.WebServiceBinding(ConformsTo = System.Web.Services.WsiProfiles.BasicProfile1_1)]
    public class getmedia : RangeRequestHandlerBase, IReadOnlySessionState
    {
        #region Private Fields

        private static int _bufferSize;

        private HttpContext _context;
        private int _galleryIdInQueryString = int.MinValue;
        private int _galleryId = int.MinValue;
        private int _mediaObjectId;
        private DisplayObjectType _displayType;

        private IGalleryObject _mediaObject;
        private string _mediaObjectFilePath;
        private IGallerySettings _gallerySetting;
        private Stream _stream;
        private FileInfo _mediaObjectFileInfo;
        private bool _sendAsAttachment;

        #endregion

        #region Enumerations

        /// <summary>
        /// Specifies a type of resource served by this HTTP handler.
        /// </summary>
        private enum MediaType
        {
            /// <summary>
            /// Specifies that no type has been specified.
            /// </summary>
            NotSet = 0,

            /// <summary>
            /// Specifies that a media object has been requested.
            /// </summary>
            MediaObject,

            /// <summary>
            /// Specifies that a watermarked media object has been requested.
            /// </summary>
            MediaObjectWithWatermark,

            /// <summary>
            /// Specifies that an empty album thumbnail has been requested.
            /// </summary>
            EmptyAlbumThumbnail
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the media object being requested. Guaranteed to not return null; returns 
        /// <see cref="Business.NullObjects.NullGalleryObject" /> when no media object is being requested or 
        /// it is invalid. This property does not verify the user has permission to view the media object.
        /// </summary>
        /// <value>An instance of <see cref="IGalleryObject" />.</value>
        private IGalleryObject MediaObject
        {
            get
            {
                if (_mediaObject == null)
                {
                    if (_mediaObjectId > 0)
                    {
                        try
                        {
                            _mediaObject = Factory.LoadMediaObjectInstance(_mediaObjectId);
                        }
                        catch (InvalidMediaObjectException)
                        {
                            _mediaObject = new Business.NullObjects.NullGalleryObject();
                        }
                    }
                    else
                    {
                        _mediaObject = new Business.NullObjects.NullGalleryObject();
                    }
                }

                return _mediaObject;
            }
        }

        /// <summary>
        /// Gets the type of resource that has been requested by the user.
        /// </summary>
        /// <value>An instance of <see cref="MediaType" />.</value>
        private MediaType ResourceType { get; set; }

        /// <summary>
        /// Gets the file path to the requested media object. It will be the thumbnail, optimized, or original file depending
        /// on which version is being requested. May return null or an empty string when an invalid media object is
        /// requested or the default album thumbnail is requested.
        /// </summary>
        /// <value>The file path to the requested media object.</value>
        private string MediaObjectFilePath
        {
            get
            {
                if (_mediaObjectFilePath == null)
                {
                    switch (_displayType)
                    {
                        case DisplayObjectType.Thumbnail:
                            _mediaObjectFilePath = MediaObject.Thumbnail.FileNamePhysicalPath;
                            break;
                        case DisplayObjectType.Optimized:
                            _mediaObjectFilePath = MediaObject.Optimized.FileNamePhysicalPath;
                            break;
                        case DisplayObjectType.Original:
                            _mediaObjectFilePath = MediaObject.Original.FileNamePhysicalPath;
                            break;
                    }
                }

                return _mediaObjectFilePath;
            }
        }

        /// <summary>
        /// Gets a reference to the file associated with the requested media object. Returns null when
        /// <see cref="ResourceType" /> = <see cref="MediaType.EmptyAlbumThumbnail" /> or <see cref="MediaType.NotSet" />.
        /// </summary>
        /// <value>A <see cref="FileInfo" /> instance, or null.</value>
        private FileInfo MediaObjectFileInfo
        {
            get
            {
                if ((_mediaObjectFileInfo == null) && (File.Exists(MediaObjectFilePath)))
                {
                    _mediaObjectFileInfo = new FileInfo(MediaObjectFilePath);
                }

                return _mediaObjectFileInfo;
            }
        }

        /// <summary>
        /// Gets the MIME type for the requested media object. It will be for the thumbnail, optimized, or original file depending
        /// on which version is being requested.
        /// </summary>
        /// <value>The MIME type for the requested media object.</value>
        private IMimeType MimeType
        {
            get
            {
                switch (ResourceType)
                {
                    case MediaType.NotSet:
                        return new Business.NullObjects.NullMimeType();
                    case MediaType.MediaObject:
                        return GetMimeTypeForMediaObject();
                    case MediaType.MediaObjectWithWatermark:
                    case MediaType.EmptyAlbumThumbnail:
                        return Factory.LoadMimeType("dummy.jpg");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Gets the gallery ID associated with the media object being requested. If no media object is available (perhaps an empty
        /// album thumbnail is being requested), then use the gallery ID specified in the query string.
        /// </summary>
        /// <value>The gallery ID.</value>
        private int GalleryId
        {
            get
            {
                if (_galleryId == int.MinValue)
                {
                    if (!(MediaObject is Business.NullObjects.NullGalleryObject))
                    {
                        _galleryId = MediaObject.GalleryId;
                    }
                    else
                    {
                        _galleryId = _galleryIdInQueryString;
                    }
                }

                return _galleryId;
            }
        }

        /// <summary>
        /// Gets the gallery settings for the gallery the requested media object is in.
        /// </summary>
        /// <value>The gallery settings.</value>
        private IGallerySettings GallerySettings
        {
            get
            {
                if (_gallerySetting == null)
                {
                    _gallerySetting = Factory.LoadGallerySetting(GalleryId);
                }

                return _gallerySetting;
            }
        }

        /// <summary>
        /// Gets the size of each chunk of data streamed back to the client.
        /// </summary>
        /// <value>An integer</value>
        /// <remarks>
        /// When a client makes a range request the requested stream's contents are
        /// read in BufferSize chunks, with each chunk flushed to the output stream
        /// until the requested byte range has been read.
        /// </remarks>
        public override int BufferSize
        {
            get
            {
                if (_bufferSize == 0)
                {
                    _bufferSize = AppSetting.Instance.MediaObjectDownloadBufferSize;
                }

                return _bufferSize;
            }
        }

        /// <summary>
        /// Gets the value to use for the response's content disposition. Default value is
        /// ResponseHeaderContentDisposition.Inline.
        /// </summary>
        /// <value>The value to use for the response's content disposition.</value>
        protected override ResponseHeaderContentDisposition ContentDisposition
        {
            get
            {
                return _sendAsAttachment ? ResponseHeaderContentDisposition.Attachment : ResponseHeaderContentDisposition.Inline;
            }
        }

        /// <summary>
        /// Gets the name of the requested file. Used to set the Content-Disposition response header. Specify
        /// <see cref="String.Empty"/> or null if no file name is applicable.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> instance, or null if no file name is applicable.
        /// </value>
        public override string FileName
        {
            get
            {
                return Path.GetFileName(MediaObjectFilePath).Replace(GallerySettings.OptimizedFileNamePrefix, String.Empty);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>
        /// Returns <c>true</c> when the method succeeds; otherwise <c>false</c>.
        /// </returns>
        public override bool InitializeRequest(HttpContext context)
        {
            bool isSuccessfullyInitialized = false;

            try
            {
                if (!GalleryController.IsInitialized)
                {
                    GalleryController.InitializeGspApplication();
                }

                if (InitializeVariables(context))
                {
                    isSuccessfullyInitialized = true;
                }
                else
                {
                    _context.Response.StatusCode = 404;
                }

                return (base.InitializeRequest(context) & isSuccessfullyInitialized);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw; // We don't want these to fall into the generic catch because we don't want them logged.
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);
            }

            return isSuccessfullyInitialized;
        }

        /// <summary>
        /// Gets a <see cref="Stream"/> object representing the requested content.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="Stream"/> instance.
        /// </returns>
        public override Stream GetResourceStream()
        {
            switch (ResourceType)
            {
                case MediaType.MediaObject:
                    return File.OpenRead(MediaObjectFilePath);
                case MediaType.MediaObjectWithWatermark:
                    return _stream ?? (_stream = GetWatermarkedImageStream());
                case MediaType.EmptyAlbumThumbnail:
                    return _stream ?? (_stream = GetDefaultThumbnailStream());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the length of the requested resource.
        /// </summary>
        /// <returns>Returns a long.</returns>
        public override long GetResourceLength()
        {
            if (ResourceType == MediaType.MediaObject)
            {
                return MediaObjectFileInfo.Length;
            }
            else
            {
                return GetResourceStream().Length;
            }
        }

        /// <summary>
        /// Gets the timestamp of the last write time of the requested resource. Returns <see cref="DateTime.MinValue"/>
        /// for a dynamically created resource.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> instance.</returns>
        public override DateTime GetResourceLastWriteTimeUtc()
        {
            if ((ResourceType == MediaType.MediaObject) && (MediaObjectFileInfo != null))
            {
                return MediaObjectFileInfo.LastWriteTimeUtc;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Cleans up resources. This is called in a finally block at the end of the ProcessRequest method of the
        /// base class.
        /// method.
        /// </summary>
        public override void CleanUpResources()
        {
            if (_stream != null)
            {
                _stream.Dispose();
            }
        }

        /// <summary>
        /// Returns the Entity Tag (ETag) for the requested content. Returns an empty string if an ETag value
        /// is not applicable or if the derived class does not provide an implementation.
        /// </summary>
        /// <returns>A <see cref="System.String"/> instance.</returns>
        public override string GetResourceFileEntityTag()
        {
            if (ResourceType != MediaType.MediaObject)
                return string.Empty;

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] sourceBytes = ascii.GetBytes(string.Concat(MediaObjectFileInfo.FullName, "|", MediaObjectFileInfo.LastWriteTimeUtc));

            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(sourceBytes));
        }

        /// <summary>
        /// Returns the MIME type for the requested content (e.g. image/jpeg, video/quicktime).
        /// </summary>
        /// <returns>A <see cref="System.String" />.</returns>
        public override string GetResourceMimeType()
        {
            return MimeType.FullType;
        }

        /// <summary>
        /// Verifies that the current user can access the requested resource.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if validation succeeds; otherwise <c>false</c>.
        /// </returns>
        protected override bool CheckAuthorizationRules()
        {
            if (!IsUserAuthorized())
            {
                _context.Response.StatusCode = 403;
                AddHeader(_context.Response, "Content-Type", "text/html");
                _context.Response.Write("<h1>Unauthorized user</h1>");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies that the requested resource exists and can be sent to the user.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if validation succeeds; otherwise <c>false</c>.
        /// </returns>
        protected override bool CheckResourceRequested()
        {
            bool isAlbumThumbnail = (ResourceType == MediaType.EmptyAlbumThumbnail);

            if (isAlbumThumbnail || File.Exists(MediaObjectFilePath))
            {
                // Our test succeeds. Call base method to continue validation.
                return base.CheckResourceRequested();
            }
            else
            {
                _context.Response.StatusCode = 404;
                return false;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Initialize the class level variables with information from the query string. Returns false if the 
        /// variables cannot be properly initialized.
        /// </summary>
        /// <param name="context">The HttpContext for the current request.</param>
        /// <returns>Returns true if all variables were initialized; returns false if there was a problem and 
        /// one or more variables could not be set.</returns>
        private bool InitializeVariables(HttpContext context)
        {
            _context = context;

            if (!ExtractQueryStringParms(context.Request.Url.Query))
                return false;

            ResourceType = DetermineResourceType();

            return DisplayObjectTypeEnumHelper.IsValidDisplayObjectType(_displayType);
        }

        private MediaType DetermineResourceType()
        {
            if (_mediaObjectId == 0)
            {
                // User specified moid=0 in the query string, which is the signal to request the empty album thumbnail.
                return MediaType.EmptyAlbumThumbnail;
            }
            else if (ShouldApplyWatermark())
            {
                return MediaType.MediaObjectWithWatermark;
            }
            else
            {
                return MediaType.MediaObject;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the media asset requires a watermark.
        /// </summary>
        /// <returns><c>true</c> if the media asset requires a watermark, <c>false</c> otherwise.</returns>
        /// <remarks>This function is similar to <see cref="GalleryObjectController.ShouldApplyWatermark" />, so if you edit this
        /// function check the other one, too.</remarks>
        private bool ShouldApplyWatermark()
        {
            // Apply watermark to thumbnails only when the config setting applyWatermarkToThumbnails = true.
            // Apply watermark to optimized and original images only when applyWatermark = true.
            if ((_displayType == DisplayObjectType.Thumbnail) || MediaObject.MimeType.TypeCategory == MimeTypeCategory.Image)
            {
                bool requiresWatermark = false;
                bool applyWatermark = GallerySettings.ApplyWatermark;
                bool applyWatermarkToThumbnails = GallerySettings.ApplyWatermarkToThumbnails;
                bool isThumbnail = (_displayType == DisplayObjectType.Thumbnail);

                if (AppSetting.Instance.License.LicenseType == LicenseLevel.TrialExpired && !isThumbnail)
                {
                    requiresWatermark = true;
                }
                else if ((applyWatermark && !isThumbnail) || (applyWatermark && applyWatermarkToThumbnails && isThumbnail))
                {
                    // If the user belongs to a role with watermarks set to visible, then show it; otherwise don't show the watermark.
                    if (!Utils.IsUserAuthorized(SecurityActions.HideWatermark, RoleController.GetGalleryServerRolesForUser(), MediaObject.Parent.Id, GalleryId, MediaObject.IsPrivate, ((IAlbum)MediaObject.Parent).IsVirtualAlbum))
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

        /// <summary>
        /// Extract information from the query string and assign to our class level variables. Return false if 
        /// something goes wrong and the variables cannot be set. This will happen when the query string is in 
        /// an unexpected format.
        /// </summary>
        /// <param name="queryString">The query string for the current request. Can be populated with 
        /// HttpContext.Request.Url.Query. Must start with a question mark (?).</param>
        /// <returns>Returns true if all relevant variables were assigned from the query string; returns false 
        /// if there was a problem.</returns>
        private bool ExtractQueryStringParms(string queryString)
        {
            if (String.IsNullOrEmpty(queryString)) return false;

            queryString = queryString.Remove(0, 1); // Strip off the ?

            bool filepathIsEncrypted = AppSetting.Instance.EncryptMediaObjectUrlOnClient;
            if (filepathIsEncrypted)
            {
                // Ex: getmedia.ashx?q=PneHH0S5VrXVtZWMki2k867KRGyCExF7 (most common)
                // Ex: getmedia.ashx?q=PneHH0S5VrXVtZWMki2k867KRGyCExF7&sa=1 (created by client script when user downloads a single asset)
                // Ex: getmedia.ashx?q=PneHH0S5VrXVtZWMki2k867KRGyCExF7&sa=1&extra=somevalue (user may add extra parameters)
                foreach (var nameValuePair in queryString.Split(new[] { '&' }))
                {
                    var nameOrValue = nameValuePair.Split(new[] { '=' });
                    switch (nameOrValue[0])
                    {
                        case "q":
                            queryString = nameOrValue[1];
                            break;
                        case "sa":
                            _sendAsAttachment = ((nameOrValue[1].Equals("1", StringComparison.Ordinal)) || (nameOrValue[1].Equals("TRUE", StringComparison.OrdinalIgnoreCase)));
                            break;
                    }
                }

                // Decode, then decrypt the query string. Note that we must replace spaces with a '+'. This is required when the the URL is
                // used in javascript to create the Silverlight media player. Apparently, Silverlight or the media player javascript decodes
                // the query string when it requests the URL, so that means any instances of '%2b' are decoded into '+' before it gets here.
                // Ideally, we wouldn't even call UrlDecode in this case, but we don't have a way of knowing that it has already been decoded.
                // So we decode anyway, which doesn't cause any harm *except* it converts '+' to a space, so we need to convert them back.
                try
                {
                    queryString = HelperFunctions.Decrypt(HttpUtility.UrlDecode(queryString).Replace(" ", "+"));
                }
                catch (FormatException)
                {
                    // We'll get here when user creates a new album and the client script creates an URL like getmedia.ashx?moid=0&dt=1&g=1
                    // In this case ignore the error and process the string as normal.
                }
            }

            //moid={0}&dt={1}g={2}
            foreach (string nameValuePair in queryString.Split(new[] { '&' }))
            {
                string[] nameOrValue = nameValuePair.Split(new[] { '=' });
                switch (nameOrValue[0])
                {
                    case "g":
                        {
                            int gid;
                            if (Int32.TryParse(nameOrValue[1], out gid))
                                _galleryIdInQueryString = gid;
                            else
                                return false;
                            break;
                        }
                    case "moid":
                        {
                            int moid;
                            if (Int32.TryParse(nameOrValue[1], out moid))
                                _mediaObjectId = moid;
                            else
                                return false;
                            break;
                        }
                    case "dt":
                        {
                            int dtInt;
                            if (Int32.TryParse(nameOrValue[1], out dtInt))
                            {
                                if (DisplayObjectTypeEnumHelper.IsValidDisplayObjectType((DisplayObjectType)dtInt))
                                {
                                    _displayType = (DisplayObjectType)dtInt; break;
                                }
                                else
                                    return false;
                            }
                            else
                                return false;
                        }
                    case "sa":
                        {
                            _sendAsAttachment = ((nameOrValue[1].Equals("1", StringComparison.Ordinal)) || (nameOrValue[1].Equals("TRUE", StringComparison.OrdinalIgnoreCase)));
                            break;
                        }
                        // NEW in 4.0: Allow unrecognized query string parameters. This allows adding an arbitrary parameter to force a browser refresh of an image.
                        //default: return false; // Unexpected query string parm. Return false so execution is aborted.
                }
            }

            ValidateDisplayType();

            return true;
        }

        /// <summary>
        /// If an optimized version is being requested, make sure a file name is specified for it. If not, switch to the
        /// original version. This switch will be necessary for most non-image media objects, since the client usually 
        /// requests optimized versions for everything.
        /// </summary>
        /// <remarks>This function became necessary when switching to the ID-based request in 2.4 (rather than the 
        /// file-based request). It was considered to change the requesting logic to ensure the correct display type 
        /// is specified, and while that seems preferable from an architectural perspective, it was more complex to 
        /// implement and potentially more fragile than this simple function.</remarks>
        private void ValidateDisplayType()
        {
            if ((_displayType == DisplayObjectType.Optimized) && (String.IsNullOrEmpty(MediaObjectFilePath)))
            {
                _displayType = DisplayObjectType.Original;
                _mediaObjectFilePath = null;

                // Comment out the exception, as it generates unnecessary errors when bots request deleted items
                //if (String.IsNullOrEmpty(MediaObjectFilePath))
                //{
                //  throw new InvalidMediaObjectException(String.Format(CultureInfo.CurrentCulture, "A request was made to the Gallery Server HTTP handler to serve the optimized image for media object ID {0}, but either the media object does not exist or neither the optimized nor the original has a filename stored in the database, and therefore cannot be served.", _mediaObjectId));
                //}
            }
        }

        private bool IsUserAuthorized()
        {
            // If no media object is specified, then return true (this happens for empty album thumbnails).
            if ((ResourceType == MediaType.MediaObject) || (ResourceType == MediaType.MediaObjectWithWatermark))
            {
                SecurityActions requestedPermission = SecurityActions.ViewAlbumOrMediaObject;

                if ((_displayType == DisplayObjectType.Original))
                {
                    var optFileDiffThanOriginal = !MediaObject.Original.FileName.Equals(MediaObject.Optimized.FileName, StringComparison.OrdinalIgnoreCase);

                    if (optFileDiffThanOriginal)
                        requestedPermission = SecurityActions.ViewOriginalMediaObject;
                }

                return Utils.IsUserAuthorized(requestedPermission, RoleController.GetGalleryServerRolesForUser(), MediaObject.Parent.Id, GalleryId, MediaObject.IsPrivate, ((IAlbum)MediaObject.Parent).IsVirtualAlbum);
            }
            else
            {
                return true; // Non-media object requests are always valid (i.e. default album thumbnails)
            }
        }

        private IMimeType GetMimeTypeForMediaObject()
        {
            switch (_displayType)
            {
                case DisplayObjectType.Thumbnail:
                    return MediaObject.Thumbnail.MimeType;
                case DisplayObjectType.Optimized:
                    return MediaObject.Optimized.MimeType;
                case DisplayObjectType.Original:
                    return MediaObject.Original.MimeType;
                default:
                    return MediaObject.Original.MimeType;
            }
        }

        private Stream GetDefaultThumbnailStream()
        {
            using (Bitmap bmp = GetDefaultThumbnailBitmap())
            {
                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, ImageFormat.Jpeg);
                return stream;
            }
        }

        private Bitmap GetDefaultThumbnailBitmap()
        {
            //Return a bitmap of a default album image.  This will be used when no actual
            //image is available to serve as the pictorial view of the album.
            string imageText = GallerySettings.EmptyAlbumThumbnailText;
            string fontName = GallerySettings.EmptyAlbumThumbnailFontName;
            int fontSize = GallerySettings.EmptyAlbumThumbnailFontSize;
            Color bgColor = HelperFunctions.GetColor(GallerySettings.EmptyAlbumThumbnailBackgroundColor);
            Color fontColor = HelperFunctions.GetColor(GallerySettings.EmptyAlbumThumbnailFontColor);

            var thmbSize = Utils.CalculateSize(GallerySettings.EmptyAlbumThumbnailWidthToHeightRatio, GallerySettings.MaxThumbnailLength);

            Bitmap bmp = null;
            Graphics g = null;
            try
            {
                // If the font name does not match an installed font, .NET will substitute Microsoft Sans Serif.
                Font fnt = new Font(fontName, fontSize);
                Rectangle rct = new Rectangle(0, 0, thmbSize.Width, thmbSize.Height);
                bmp = new Bitmap(rct.Width, rct.Height);
                g = Graphics.FromImage(bmp);

                // Calculate x and y offset for text
                Size textSize = g.MeasureString(imageText, fnt).ToSize();

                int x = (thmbSize.Width - textSize.Width) / 2; //Starting point from left for the text
                int y = (thmbSize.Height - textSize.Height) / 2; //Start point from top for the text

                if (x < 0) x = 0;
                if (y < 0) y = 0;

                // Generate image
                g.FillRectangle(new SolidBrush(bgColor), rct);
                g.DrawString(imageText, fnt, new SolidBrush(fontColor), x, y);
            }
            catch
            {
                if (bmp != null)
                    bmp.Dispose();

                throw;
            }
            finally
            {
                if (g != null)
                    g.Dispose();
            }

            return bmp;
        }

        private Stream GetWatermarkedImageStream()
        {
            Image watermarkedImage = null;
            try
            {
                try
                {
                    watermarkedImage = ImageHelper.AddWatermark(MediaObjectFilePath, MediaObject.GalleryId);
                }
                catch (Exception ex)
                {
                    // Can't apply watermark to image. Substitute an error image and send that to the user.
                    if (!(ex is FileNotFoundException))
                    {
                        // Don't log FileNotFoundException exceptions. This helps avoid clogging the error log 
                        // with entries caused by search engine retrieving media objects that have been moved or deleted.
                        AppEventController.LogError(ex);
                    }
                    watermarkedImage = Image.FromFile(_context.Request.MapPath(String.Concat(Utils.SkinPath, "/images/error-xl.png")));
                }

                MemoryStream stream = new MemoryStream();
                watermarkedImage.Save(stream, ImageFormat.Jpeg);
                return stream;
            }
            finally
            {
                if (watermarkedImage != null)
                    watermarkedImage.Dispose();
            }
        }

        #endregion
    }
}
