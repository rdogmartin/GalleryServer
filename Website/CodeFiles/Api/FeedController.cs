using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Xml;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
    /// <summary>
    /// Contains methods for Web API access to RSS/Atom feeds. The feeds are generated through the
    /// <see cref="AlbumSyndicationFeedFormatter" /> attribute.
    /// </summary>
    /// <remarks>The formatter <see cref="AlbumSyndicationFeedFormatter" /> validates that the application
    /// is running an Enterprise License, throwing a <see cref="GallerySecurityException" /> when it isn't.
    /// This propagates to the client as an HTTP 503 error. If a more specific error is desired on the client
    /// (eg. 403 Forbidden), then move the license validation to the <see cref="FeedController" /> class.
    /// </remarks>
    [AlbumSyndicationFeedFormatter]
    public class FeedController : ApiController
    {
        /// <summary>
        /// Gets an album representing the specified <paramref name="id" />.
        /// </summary>
        /// <param name="id">The ID of the album to retrieve. Required.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on. Optional. Defaults to 
        /// <see cref="MetadataItemName.DateAdded" /> when not specified.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order. Optional. Defaults to
        /// <c>false</c> when not specified.</param>
        /// <param name="destinationUrl">The URL, relative to the website root, that page hyperlinks should point to.
        /// Ex: "/dev/gs/default.aspx" Optional. When not specified, URLs will point to the application root.</param>
        /// <returns>Returns an instance of <see cref="Atom10FeedFormatter" /> or <see cref="Rss20FeedFormatter" />
        /// representing the specified parameters.</returns>
        /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
        [HttpGet]
        [ActionName("Album")]
        public IAlbum GetById(int id, int sortByMetaNameId = int.MinValue, bool sortAscending = true, string destinationUrl = null)
        {
            IAlbum album = null;
            try
            {
                album = Factory.LoadAlbumInstance(new AlbumLoadOptions(id) { InflateChildObjects = true });
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, Utils.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

                album.FeedFormatterOptions = new FeedFormatterOptions()
                {
                    SortByMetaName = (MetadataItemName)sortByMetaNameId,
                    SortAscending = sortAscending,
                    DestinationUrl = String.IsNullOrWhiteSpace(destinationUrl) ? String.Concat(Utils.AppRoot, "/") : destinationUrl
                };

                return album;
            }
            catch (InvalidAlbumException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(String.Format("Could not find album with ID = {0}", id)),
                    ReasonPhrase = "Album Not Found"
                });
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        /// <summary>
        /// Gets an album containing the gallery objects having the specified <paramref name="q" /> string.
        /// </summary>
        /// <param name="q">The tag to search for. Required. May contain multiple tags separated by
        /// the '+' character. The '+' character must be encoded as %2b when used in an URL.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on. Optional. Defaults to
        /// <see cref="MetadataItemName.DateAdded" /> when not specified.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order. Optional. Defaults to
        /// <c>false</c> when not specified.</param>
        /// <param name="destinationUrl">The URL, relative to the website root, that page hyperlinks should point to.
        /// Ex: "/dev/gs/default.aspx" Optional. When not specified, URLs will point to the application root.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration. Optional. When not specified, defaults to
        /// <see cref="GalleryObjectType.All" /></param>
        /// <param name="galleryId">The gallery ID. Only items in this gallery are returned. Optional. When not 
        /// specified, the first gallery is assumed.</param>
        /// <returns>Returns an instance of <see cref="Atom10FeedFormatter" /> or <see cref="Rss20FeedFormatter" />
        /// representing the specified parameters.</returns>
        /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
        [HttpGet]
        [ActionName("Title")]
        public IAlbum GetByTitle(string q, int sortByMetaNameId = int.MinValue, bool sortAscending = true, string destinationUrl = null, string filter = "all", int galleryId = int.MinValue)
        {
            IAlbum album = null;
            try
            {
                album = GalleryObjectController.GetGalleryObjectsHavingTitleOrCaption(Utils.ToArray(q), GalleryObjectTypeEnumHelper.Parse(filter, GalleryObjectType.All), ValidateGallery(galleryId));

                album.FeedFormatterOptions = new FeedFormatterOptions()
                {
                    SortByMetaName = (MetadataItemName)sortByMetaNameId,
                    SortAscending = sortAscending,
                    DestinationUrl = String.IsNullOrWhiteSpace(destinationUrl) ? String.Concat(Utils.AppRoot, "/") : destinationUrl
                };

                return album;
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        /// <summary>
        /// Gets an album containing the gallery objects having the specified <paramref name="q" /> string.
        /// </summary>
        /// <param name="q">The tag to search for. Required. May contain multiple tags separated by
        /// the '+' character. The '+' character must be encoded as %2b when used in an URL.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on. Optional. Defaults to
        /// <see cref="MetadataItemName.DateAdded" /> when not specified.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order. Optional. Defaults to
        /// <c>false</c> when not specified.</param>
        /// <param name="destinationUrl">The URL, relative to the website root, that page hyperlinks should point to.
        /// Ex: "/dev/gs/default.aspx" Optional. When not specified, URLs will point to the application root.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration. Optional. When not specified, defaults to
        /// <see cref="GalleryObjectType.All" /></param>
        /// <param name="galleryId">The gallery ID. Only items in this gallery are returned. Optional. When not 
        /// specified, the first gallery is assumed.</param>
        /// <returns>Returns an instance of <see cref="Atom10FeedFormatter" /> or <see cref="Rss20FeedFormatter" />
        /// representing the specified parameters.</returns>
        /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
        [HttpGet]
        [ActionName("Search")]
        public IAlbum GetBySearch(string q, int sortByMetaNameId = int.MinValue, bool sortAscending = true, string destinationUrl = null, string filter = "all", int galleryId = int.MinValue)
        {
            IAlbum album = null;
            try
            {
                album = GalleryObjectController.GetGalleryObjectsHavingSearchString(Utils.ToArray(q), GalleryObjectTypeEnumHelper.Parse(filter, GalleryObjectType.All), ValidateGallery(galleryId));

                album.FeedFormatterOptions = new FeedFormatterOptions()
                {
                    SortByMetaName = (MetadataItemName)sortByMetaNameId,
                    SortAscending = sortAscending,
                    DestinationUrl = String.IsNullOrWhiteSpace(destinationUrl) ? String.Concat(Utils.AppRoot, "/") : destinationUrl
                };

                return album;
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        /// <summary>
        /// Gets an album containing the gallery objects having the specified <paramref name="q" />.
        /// </summary>
        /// <param name="q">The tag to search for. Required. May contain multiple tags separated by
        /// the '+' character. The '+' character must be encoded as %2b when used in an URL.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on. Optional. Defaults to
        /// <see cref="MetadataItemName.DateAdded" /> when not specified.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order. Optional. Defaults to
        /// <c>false</c> when not specified.</param>
        /// <param name="destinationUrl">The URL, relative to the website root, that page hyperlinks should point to.
        /// Ex: "/dev/gs/default.aspx" Optional. When not specified, URLs will point to the application root.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration. Optional. When not specified, defaults to
        /// <see cref="GalleryObjectType.All" />.</param>
        /// <param name="galleryId">The gallery ID. Only items in this gallery are returned. Optional. When not 
        /// specified, the first gallery is assumed.</param>
        /// <returns>Returns an instance of <see cref="Atom10FeedFormatter" /> or <see cref="Rss20FeedFormatter" />
        /// representing the specified parameters.</returns>
        /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
        [HttpGet]
        [ActionName("Tag")]
        public IAlbum GetByTag(string q, int sortByMetaNameId = int.MinValue, bool sortAscending = true, string destinationUrl = null, string filter = "all", int galleryId = int.MinValue)
        {
            IAlbum album = null;
            try
            {
                album = GalleryObjectController.GetGalleryObjectsHavingTags(Utils.ToArray(q), null, GalleryObjectTypeEnumHelper.Parse(filter, GalleryObjectType.All), ValidateGallery(galleryId));

                album.FeedFormatterOptions = new FeedFormatterOptions()
                {
                    SortByMetaName = (MetadataItemName)sortByMetaNameId,
                    SortAscending = sortAscending,
                    DestinationUrl = String.IsNullOrWhiteSpace(destinationUrl) ? String.Concat(Utils.AppRoot, "/") : destinationUrl
                };

                return album;
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        /// <summary>
        /// Gets an album containing the gallery objects having the specified <paramref name="q" />.
        /// </summary>
        /// <param name="q">The people tag to search for. Required. May contain multiple tags separated by
        /// the '+' character. The '+' character must be encoded as %2b when used in an URL.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on. Optional. Defaults to
        /// <see cref="MetadataItemName.DateAdded" /> when not specified.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order. Optional. Defaults to
        /// <c>false</c> when not specified.</param>
        /// <param name="destinationUrl">The URL, relative to the website root, that page hyperlinks should point to.
        /// Ex: "/dev/gs/default.aspx" Optional. When not specified, URLs will point to the application root.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration. Optional. When not specified, defaults to
        /// <see cref="GalleryObjectType.All" />.</param>
        /// <param name="galleryId">The gallery ID. Only items in this gallery are returned. Optional. When not 
        /// specified, the first gallery is assumed.</param>
        /// <returns>Returns an instance of <see cref="Atom10FeedFormatter" /> or <see cref="Rss20FeedFormatter" />
        /// representing the specified parameters.</returns>
        /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
        [HttpGet]
        [ActionName("People")]
        public IAlbum GetByPeople(string q, int sortByMetaNameId = int.MinValue, bool sortAscending = true, string destinationUrl = null, string filter = "all", int galleryId = int.MinValue)
        {
            IAlbum album = null;
            try
            {
                album = GalleryObjectController.GetGalleryObjectsHavingTags(null, Utils.ToArray(q), GalleryObjectTypeEnumHelper.Parse(filter, GalleryObjectType.All), ValidateGallery(galleryId));

                album.FeedFormatterOptions = new FeedFormatterOptions()
                {
                    SortByMetaName = (MetadataItemName)sortByMetaNameId,
                    SortAscending = sortAscending,
                    DestinationUrl = String.IsNullOrWhiteSpace(destinationUrl) ? String.Concat(Utils.AppRoot, "/") : destinationUrl
                };

                return album;
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        /// <summary>
        /// Gets an album containing the gallery objects most recently added to the gallery. Only items the current user 
        /// is authorized to view are returned.
        /// </summary>
        /// <param name="top">The maximum number of results to return. Must be a value greater than zero. Optional. When
        /// not specified, defaults to fifty.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on. Optional. Defaults to 
        /// <see cref="MetadataItemName.DateAdded" /> when not specified.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order. Optional. Defaults to
        /// <c>false</c> when not specified.</param>
        /// <param name="destinationUrl">The URL, relative to the website root, that page hyperlinks should point to.
        /// Ex: "/dev/gs/default.aspx" Optional. When not specified, URLs will point to the application root.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration. Optional. When not specified, defaults to
        /// <see cref="GalleryObjectType.MediaObject" /></param>
        /// <param name="galleryId">The gallery ID. Only items in this gallery are returned. Optional. When not specified,
        /// the first gallery is assumed.</param>
        /// <returns>Returns an instance of <see cref="Atom10FeedFormatter" /> or <see cref="Rss20FeedFormatter" />
        /// representing the specified parameters.</returns>
        /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
        [HttpGet]
        [ActionName("Latest")]
        public IAlbum GetLatest(int top = 50, int sortByMetaNameId = (int)MetadataItemName.DateAdded, bool sortAscending = false, string destinationUrl = null, string filter = "mediaobject", int galleryId = int.MinValue)
        {
            IAlbum album = null;
            try
            {
                album = GalleryObjectController.GetMostRecentlyAddedGalleryObjects(top, ValidateGallery(galleryId), GalleryObjectTypeEnumHelper.Parse(filter, GalleryObjectType.MediaObject));

                album.FeedFormatterOptions = new FeedFormatterOptions()
                {
                    SortByMetaName = (MetadataItemName)sortByMetaNameId,
                    SortAscending = sortAscending,
                    DestinationUrl = String.IsNullOrWhiteSpace(destinationUrl) ? String.Concat(Utils.AppRoot, "/") : destinationUrl
                };

                return album;
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        /// <summary>
        /// Gets an album containing the gallery objects with the specified <paramref name="rating" />. Only items the current user 
        /// is authorized to view are returned.
        /// </summary>
        /// <param name="rating">Identifies the type of rating to retrieve. Valid values: "highest", "lowest", "none", or a number
        /// from 0 to 5 in half-step increments (eg. 0, 0.5, 1, 1.5, ... 4.5, 5).</param>
        /// <param name="top">The maximum number of results to return. Must be a value greater than zero. Optional. When
        /// not specified, defaults to fifty.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on. Optional. Defaults to 
        /// <see cref="MetadataItemName.DateAdded" /> when not specified.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order. Optional. Defaults to
        /// <c>false</c> when not specified.</param>
        /// <param name="destinationUrl">The URL, relative to the website root, that page hyperlinks should point to.
        /// Ex: "/dev/gs/default.aspx" Optional. When not specified, URLs will point to the application root.</param>
        /// <param name="filter">A filter that limits the types of gallery objects that are returned.
        /// Maps to the <see cref="GalleryObjectType" /> enumeration. Optional. When not specified, defaults to
        /// <see cref="GalleryObjectType.MediaObject" /></param>
        /// <param name="galleryId">The gallery ID. Only items in this gallery are returned. Optional. When not specified,
        /// the first gallery is assumed.</param>
        /// <returns>Returns an instance of <see cref="Atom10FeedFormatter" /> or <see cref="Rss20FeedFormatter" />
        /// representing the specified parameters.</returns>
        /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
        [HttpGet]
        [ActionName("Rating")]
        public IAlbum GetByRating(string rating, int top = 50, int sortByMetaNameId = (int)MetadataItemName.DateAdded, bool sortAscending = false, string destinationUrl = null, string filter = "mediaobject", int galleryId = int.MinValue)
        {
            IAlbum album = null;
            try
            {
                album = GalleryObjectController.GetRatedMediaObjects(rating, top, ValidateGallery(galleryId), GalleryObjectTypeEnumHelper.Parse(filter, GalleryObjectType.MediaObject));

                album.FeedFormatterOptions = new FeedFormatterOptions()
                {
                    SortByMetaName = (MetadataItemName)sortByMetaNameId,
                    SortAscending = sortAscending,
                    DestinationUrl = String.IsNullOrWhiteSpace(destinationUrl) ? String.Concat(Utils.AppRoot, "/") : destinationUrl
                };

                return album;
            }
            catch (GallerySecurityException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, (album != null ? album.GalleryId : new int?()));

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = Utils.GetExStringContent(ex),
                    ReasonPhrase = "Server Error"
                });
            }
        }

        /// <summary>
        /// Verifies that <paramref name="galleryId" /> corresponds to an actual, non-template gallery and that 
        /// the current user has access to the gallery. When <paramref name="galleryId" /> is 
        /// <see cref="Int32.MinValue" />, then the ID of the first gallery is returned. If 
        /// <paramref name="galleryId" /> is greater than <see cref="Int32.MinValue" /> and is not
        /// valid, a <see cref="InvalidGalleryException" /> is thrown.
        /// </summary>
        /// <param name="galleryId">The gallery ID. Specify <see cref="Int32.MinValue" /> to have this function
        /// return the ID of the first non-template gallery.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="InvalidGalleryException">Thrown when the <paramref name="galleryId" /> is invalid.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user is anonymous and the <paramref name="galleryId" />
        /// is configured to disallow anonymous browsing.</exception>
        private static int ValidateGallery(int galleryId)
        {
            if (galleryId == int.MinValue)
            {
                galleryId = Factory.LoadGalleries().First().GalleryId;
            }
            else
            {
                // Verify the gallery ID maps to an actual gallery (exception will be thrown if not).
                Factory.LoadGallery(galleryId);
            }

            if (!Utils.IsAuthenticated && !Factory.LoadGallerySetting(galleryId).AllowAnonymousBrowsing)
            {
                // Anonymous user but the gallery does not allow anonymous users.
                throw new GallerySecurityException();
            }

            // If we get here then the gallery ID is valid.
            return galleryId;
        }
    }

    /// <summary>
    /// An implementation of <see cref="MediaTypeFormatter" /> that formats one ore more <see cref="IAlbum" />
    /// instances in ATOM or RSS syntax. Clients should specify "application/atom+xml" or "application/rss+xml" in 
    /// their ACCEPT headers. Defaults to "application/rss+xml" if one of these is not specified.
    /// NOTE: This class throws a <see cref="GallerySecurityException" /> when the application is not running
    /// an Enterprise License.
    /// </summary>
    /// <remarks>
    /// This class was inspired by the following articles: 
    /// http://www.strathweb.com/2012/04/different-mediatypeformatters-for-same-mediaheadervalue-in-asp-net-web-api/
    /// http://blogs.msdn.com/b/jmstall/archive/2012/05/11/per-controller-configuration-in-webapi.aspx
    /// </remarks>
    public class AlbumSyndicationFeedFormatter : MediaTypeFormatter
    {
        private const string AtomMediaType = "application/atom+xml";
        private const string RssMediaType = "application/rss+xml";

        private readonly Func<Type, bool> _supportedType = (type) => type == typeof(IAlbum);

        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumSyndicationFeedFormatter"/> class.
        /// </summary>
        public AlbumSyndicationFeedFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(AtomMediaType));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(RssMediaType));
        }

        /// <summary>
        /// Queries whether this <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can deserialize an object of the specified type.
        /// </summary>
        /// <param name="type">The type to deserialize.</param>
        /// <returns>true if the <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can deserialize the type; otherwise, false.</returns>
        public override bool CanReadType(Type type)
        {
            return _supportedType(type);
        }

        /// <summary>
        /// Queries whether this <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can serialize an object of the specified type.
        /// </summary>
        /// <param name="type">The type to serialize.</param>
        /// <returns>true if the <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> can serialize the type; otherwise, false.</returns>
        public override bool CanWriteType(Type type)
        {
            return _supportedType(type);
        }

        /// <summary>
        /// Asynchronously writes an object of the specified type.
        /// </summary>
        /// <param name="type">The type of the object to write.</param>
        /// <param name="value">The object value to write.  It may be null. If not null, must be able to 
        /// be cast to an <see cref="IAlbum" />.</param>
        /// <param name="writeStream">The <see cref="T:System.IO.Stream" /> to which to write.</param>
        /// <param name="content">The <see cref="T:System.Net.Http.HttpContent" /> if available. It may be null.</param>
        /// <param name="transportContext">The <see cref="T:System.Net.TransportContext" /> if available. It may be null.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that will perform the write.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the application is not running an Enterprise License.
        /// </exception>
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (value == null)
                return null;

            var album = (IAlbum)value;
            var options = MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(album, DisplayObjectType.Thumbnail);
            var isAuthenticated = Utils.IsAuthenticated;

            options.DestinationPageUrl = album.FeedFormatterOptions.DestinationUrl; // Ex: "/dev/gs/default.aspx"

            ValidateEnterpriseLicense(album);

            return Task.Factory.StartNew(() => BuildSyndicationFeed(album, writeStream, content.Headers.ContentType.MediaType, options, isAuthenticated));
        }

        /// <summary>
        /// Builds the syndication feed from the specified <paramref name="album" /> and write it to the <paramref name="stream" />.
        /// </summary>
        /// <param name="album">The album from which to build the syndication feed.</param>
        /// <param name="stream">The <see cref="T:System.IO.Stream" /> to which to write.</param>
        /// <param name="contentType">Type of the requested content. Examples: "application/atom+xml", "application/rss+xml"</param>
        /// <param name="moBuilderOptions">The options that direct the creation of HTML and URLs for a media object.</param>
        /// <param name="isAuthenticated">Indicates whether the current user is authenticated.</param>
        private static void BuildSyndicationFeed(IAlbum album, Stream stream, string contentType, MediaObjectHtmlBuilderOptions moBuilderOptions, bool isAuthenticated)
        {
            var fb = new AlbumSyndicationFeedBuilder(album, moBuilderOptions, isAuthenticated);

            var feed = fb.Generate();

            using (var writer = XmlWriter.Create(stream))
            {
                if (String.Equals(contentType, AtomMediaType, StringComparison.InvariantCultureIgnoreCase))
                {
                    var atomFormatter = new Atom10FeedFormatter(feed);
                    atomFormatter.WriteTo(writer);
                }
                else
                {
                    var rssFormatter = new Rss20FeedFormatter(feed);
                    rssFormatter.WriteTo(writer);
                }
            }
        }

        /// <summary>
        /// Verifies the application is running an Enterprise License, throwing a <see cref="GallerySecurityException" />
        /// if it is not.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <exception cref="GallerySecurityException">Thrown when the application is not running an Enterprise License.
        /// </exception>
        private static void ValidateEnterpriseLicense(IAlbum album)
        {
            if (AppSetting.Instance.License.LicenseType < LicenseLevel.Enterprise)
            {
                AppEventController.LogEvent("RSS/Atom feeds require an Enterprise License.", album.GalleryId, EventType.Warning);

                throw new GallerySecurityException("RSS/Atom feeds require an Enterprise License.");
            }
        }
    }

    /// <summary>
    /// Contains functionality for building a syndication feed for an album and it's immediate children.
    /// </summary>
    public class AlbumSyndicationFeedBuilder
    {
        /// <summary>
        /// Gets or sets the album.
        /// </summary>
        private IAlbum Album { get; set; }

        /// <summary>
        /// Gets the media objects belonging to the <see cref="Album" />. If a sort field is specified on the
        /// album, the children are sorted accordingly; otherwise they are returned in the order defined by the
        /// <see cref="IGalleryObject.Sequence" /> property.
        /// </summary>
        private IEnumerable<IGalleryObject> MediaObjects
        {
            get
            {
                if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(Album.FeedFormatterOptions.SortByMetaName) && Album.FeedFormatterOptions.SortByMetaName != MetadataItemName.NotSpecified)
                {
                    return Album.GetChildGalleryObjects(GalleryObjectType.All, !IsAuthenticated).ToSortedList(Album.FeedFormatterOptions.SortByMetaName, Album.FeedFormatterOptions.SortAscending, Album.GalleryId);
                }
                else
                {
                    return Album.GetChildGalleryObjects(GalleryObjectType.All, !IsAuthenticated).ToSortedList();
                }
            }
        }

        /// <summary>
        /// Gets or sets the options that direct the creation of HTML and URLs for a media object.
        /// </summary>
        private MediaObjectHtmlBuilderOptions Options { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        private bool IsAuthenticated { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumSyndicationFeedBuilder" /> class.
        /// </summary>
        /// <param name="album">The album used to generate the syndication feed.</param>
        /// <param name="moBuilderOptions">The options that direct the creation of HTML and URLs for a media object.</param>
        /// <param name="isAuthenticated">Indicates whether the current user is authenticated.</param>
        public AlbumSyndicationFeedBuilder(IAlbum album, MediaObjectHtmlBuilderOptions moBuilderOptions, bool isAuthenticated)
        {
            Album = album;

            Options = moBuilderOptions;

            IsAuthenticated = isAuthenticated;
        }

        /// <summary>
        /// Generates the syndication feed.
        /// </summary>
        /// <returns>An instance of <see cref="SyndicationFeed" />.</returns>
        public SyndicationFeed Generate()
        {
            var galleryTitle = Factory.LoadGallerySetting(Options.GalleryObject.GalleryId).GalleryTitle;
            var feedTitle = String.Concat(galleryTitle, ": ", RssEncode(HtmlValidator.RemoveHtml(Album.Title, false)));
            var feedDesc = RssEncode(HtmlValidator.RemoveHtml(Album.Caption, false));

            var feed = new SyndicationFeed(feedTitle, feedDesc, new Uri(String.Concat(Options.HostUrl, Options.DestinationPageUrl)));

            var email = AppSetting.Instance.EmailFromAddress;
            if (!String.IsNullOrWhiteSpace(email))
            {
                feed.Authors.Add(new SyndicationPerson(email));
            }

            feed.Categories.Add(new SyndicationCategory("Media"));

            // Get the album thumbnail image.
            if (!Album.IsVirtualAlbum)
            {
                feed.ImageUrl = new Uri(new MediaObjectHtmlBuilder(Options).GetMediaObjectUrl());
            }

            feed.Items = MediaObjects.Select(mediaObject => BuildSyndicationItem(mediaObject, Options)).ToList();

            return feed;
        }

        /// <summary>
        /// Builds the syndication item from the <paramref name="galleryObject" /> and having the properties specified
        /// in <paramref name="options" />.
        /// </summary>
        /// <param name="galleryObject">The gallery object.</param>
        /// <param name="options">The options that direct the creation of HTML and URLs for a media object.</param>
        /// <returns>An instance of <see cref="SyndicationItem" />.</returns>
        private static SyndicationItem BuildSyndicationItem(IGalleryObject galleryObject, MediaObjectHtmlBuilderOptions options)
        {
            options.GalleryObject = galleryObject;
            options.DisplayType = (galleryObject.GalleryObjectType == GalleryObjectType.External ? DisplayObjectType.External : DisplayObjectType.Optimized);

            var moBuilder = new MediaObjectHtmlBuilder(options);

            var pageUrl = moBuilder.GetPageUrl();

            var content = GetGalleryObjectContent(galleryObject, pageUrl, moBuilder);

            var item = new SyndicationItem(
              RssEncode(HtmlValidator.RemoveHtml(galleryObject.Title, false)),
              SyndicationContent.CreateHtmlContent(content),
              new Uri(pageUrl),
              galleryObject.Id.ToString(CultureInfo.InvariantCulture),
              galleryObject.DateLastModified);

            item.PublishDate = galleryObject.DateAdded;
            item.Authors.Add(new SyndicationPerson() { Name = galleryObject.CreatedByUserName });
            item.Categories.Add(new SyndicationCategory(galleryObject.GalleryObjectType.ToString()));

            return item;
        }

        /// <summary>
        /// Gets an HTML string representing the content of the <paramref name="galleryObject" />. For example,
        /// albums contain the title and caption while images contain a hyperlinked img tag pointing to 
        /// <paramref name="pageUrl" />. Other media objects contain the HTML generated by <paramref name="moBuilder" />.
        /// </summary>
        /// <param name="galleryObject">The gallery object.</param>
        /// <param name="pageUrl">An URL pointing to a gallery page for the <paramref name="galleryObject" />
        /// Images use this value to create a hyperlink that is wrapped around the img tag.</param>
        /// <param name="moBuilder">An instance of <see cref="MediaObjectHtmlBuilder" />.</param>
        /// <returns><see cref="System.String" />.</returns>
        private static string GetGalleryObjectContent(IGalleryObject galleryObject, string pageUrl, MediaObjectHtmlBuilder moBuilder)
        {
            switch (galleryObject.GalleryObjectType)
            {
                case GalleryObjectType.Image:
                    return String.Format("<div><a href='{0}'>{1}</a></div><p>{2}</p><p>{3}</p>", pageUrl, moBuilder.GenerateHtml(), galleryObject.Title, galleryObject.Caption);

                case GalleryObjectType.Album:
                    return String.Format("<p>{0}</p><p>{1}</p>", galleryObject.Title, galleryObject.Caption);

                default:
                    // Don't include the hyperlink around the MO HTML because that interferes with audio/video controls.
                    return String.Format("<div>{0}</div><p>{1}</p><p>{2}</p>", moBuilder.GenerateHtml(), galleryObject.Title, galleryObject.Caption);
            }
        }

        /// <summary>
        /// Encode the <paramref name="text" /> for use in a syndication item.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>System.String.</returns>
        private static string RssEncode(string text)
        {
            // Recommendation from http://www.rssboard.org/rss-profile. We don't bother encoding < and > because we
            // stripped them using HtmlValidator.RemoveHtml().
            return text.Replace("&", "&#x26;");
        }
    }

    /// <summary>
    /// Contains functionality for specifying that a particular <see cref="ApiController" /> use the 
    /// <see cref="AlbumSyndicationFeedFormatter" /> when generating the output.
    /// </summary>
    /// <remarks>
    /// Inspired from http://blogs.msdn.com/b/jmstall/archive/2012/05/11/per-controller-configuration-in-webapi.aspx
    /// </remarks>
    public class AlbumSyndicationFeedFormatterAttribute : Attribute, IControllerConfiguration
    {
        /// <summary>
        /// Callback invoked to set per-controller overrides for this <paramref name="controllerDescriptor" />.
        /// </summary>
        /// <param name="controllerSettings">The controller settings to initialize.</param>
        /// <param name="controllerDescriptor">The controller descriptor. Note that the 
        /// <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> can be associated with the derived controller 
        /// type given that <see cref="T:System.Web.Http.Controllers.IControllerConfiguration" /> is inherited.</param>
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Clear();
            controllerSettings.Formatters.Add(new AlbumSyndicationFeedFormatter());
        }
    }
}
