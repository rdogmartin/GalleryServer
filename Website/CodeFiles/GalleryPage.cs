using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages
{
    /// <summary>
    /// The base class user control used in Gallery Server to represent page-like functionality.
    /// </summary>
    public abstract class GalleryPage : UserControl
    {
        #region Private Fields

        private readonly object _lockObject = new object();

        private int _galleryId = int.MinValue;
        private IAlbum _album;
        private int? _mediaObjectId;
        private IGalleryObject _mediaObject;
        private ClientMessageOptions _clientMessage;
        private IGalleryServerRoleCollection _roles;
        private string _pageTitle = String.Empty;
        private bool? _userCanViewAlbumOrMediaObject;
        private bool? _userCanViewOriginal;
        private bool? _userCanAddAdministerSite;
        private bool? _userCanAdministerGallery;
        private bool? _userCanCreateAlbum;
        private bool? _userCanEditAlbum;
        private bool? _userCanAddMediaObject;
        private bool? _userCanEditMediaObject;
        private bool? _userCanDeleteCurrentAlbum;
        private bool? _userCanDeleteChildAlbum;
        private bool? _userCanDeleteMediaObject;
        private bool? _userCanSynchronize;
        private bool? _userDoesNotGetWatermark;
        private bool? _userCanAddMediaObjectToAtLeastOneAlbum;
        private bool? _userCanAddAlbumToAtLeastOneAlbum;
        private bool? _userCanEditAtLeastOneAlbum;
        private bool? _userCanEditAtLeastOneMediaAsset;
        private bool? _userIsAdminForAtLeastOneOtherGallery;
        private Gallery _galleryControl;
        private IGallerySettings _gallerySetting;
        private Controls.galleryheader _galleryHeader;
        private int _currentPage;
        private PageId _pageId;
        private bool? _showLogin;
        private bool? _showSearch;
        private bool? _allowAnonymousBrowsing;
        private bool? _showLeftPaneForAlbum;
        private bool? _showLeftPaneForMediaObject;
        private bool? _showCenterPane;
        private bool? _showRightPane;
        private bool? _showRibbonToolbar;
        private bool? _showAlbumBreadCrumb;
        private bool? _showHeader;
        private string _galleryTitle;
        private string _galleryTitleUrl;
        private bool? _showMediaObjectTitle;
        private bool? _showMediaObjectNavigation;
        private bool? _showMediaObjectIndexPosition;
        private bool? _autoPlaySlideShow;
        private SlideShowType _slideShowType;
        private bool? _slideShowLoop;
        private DisplayObjectType _mediaViewSize;
        private IUiTemplateCollection _uiTemplates;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="GalleryPage"/> class.
        /// </summary>
        static GalleryPage()
        {
            if (!GalleryController.IsInitialized)
            {
                GalleryController.InitializeGspApplication();
            }

            DetectPreviousInstallation();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GalleryPage"/> class.
        /// </summary>
        protected GalleryPage()
        {
            // Ensure the app is initialized. This should have been done in the static constructor, but if anything went wrong
            // there, it may not be initialized, so we check again.
            if (!GalleryController.IsInitialized)
            {
                GalleryController.InitializeGspApplication();
            }

            this.Init += GalleryPage_Init;
            //this.Load +=GalleryPage_Load;
            //this.Unload += this.GalleryPage_Unload;
            //this.Error += this.GalleryPage_Error;
            this.PreRender += (GalleryPage_PreRender);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Init event of the GalleryPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GalleryPage_Init(object sender, System.EventArgs e)
        {
            InitializePage();
        }

        /// <summary>
        /// Handles the PreRender event of the GalleryPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GalleryPage_PreRender(object sender, EventArgs e)
        {
            AddPageTitleIfMissing();

            ShowClientMessage();

			AddMaintenanceServiceCallIfNeeded();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the client ID for the current Gallery control. This value can be used in client
        /// script to differentiate variables and other script when multiple instances of the control
        /// are placed on the web page. Returns <see cref="Control.ClientID" />, prepended with "gsp_".
        /// </summary>
        /// <value>A string.</value>
        public string GspClientId
        {
            get { return GalleryControl.GspClientId; }
        }

        /// <summary>
        /// Gets the client ID for the current Gallery control. This value can be used in client
        /// script to differentiate variables and other script when multiple instances of the control
        /// are placed on the web page. Returns <see cref="Control.ClientID" />, prepended with "gsp_".
        /// </summary>
        /// <value>A string.</value>
        /// <remarks>This property is simply a pass-through for GspClientId. It's purpose is to be
        /// a very short variable that doesn't clutter up javascript.</remarks>
        public string cid
        {
            get { return GspClientId; }
        }

        /// <summary>
        /// Gets the client ID for the DOM element that is to receive the contents of the media
        /// object. Ex: "gsp_g_mediaHtml"
        /// </summary>
        public string MediaClientId
        {
            get { return String.Concat(this.GspClientId, "_mediaHtml"); }
        }

        /// <summary>
        /// Gets the name of the compiled jsRender template for the media object.
        /// Ex: "gsp_g_media_tmpl"
        /// </summary>
        public string MediaTmplName
        {
            get { return String.Concat(this.GspClientId, "_media_tmpl"); }
        }

        /// <summary>
        /// Gets the client ID for the DOM element that is to receive the contents of the gallery
        /// header. Ex: "gsp_g_gHdrHtml"
        /// </summary>
        public string HeaderClientId
        {
            get { return String.Concat(this.GspClientId, "_gHdrHtml"); }
        }

        /// <summary>
        /// Gets the name of the compiled jsRender template for the header. Ex: "gsp_g_gallery_header_tmpl"
        /// </summary>
        public string HeaderTmplName
        {
            get { return String.Concat(this.GspClientId, "_gallery_header_tmpl"); }
        }

        /// <summary>
        /// Gets the client ID for the DOM element that is to receive the contents of album thumbnail 
        /// images. Ex: "gsp_g_thmbHtml"
        /// </summary>
        public string ThumbnailClientId
        {
            get { return String.Concat(this.GspClientId, "_thmbHtml"); }
        }

        /// <summary>
        /// Gets the name of the compiled jsRender template for the album thumbnail images.
        /// Ex: "gsp_g_thumbnail_tmpl"
        /// </summary>
        public string ThumbnailTmplName
        {
            get { return String.Concat(this.GspClientId, "_thumbnail_tmpl"); }
        }

        /// <summary>
        /// Gets the client ID for the DOM element that is to receive the contents of the left pane
        /// of the media view page. Ex: "gsp_g_lpHtml"
        /// </summary>
        public string LeftPaneClientId
        {
            get { return String.Concat(this.GspClientId, "_lpHtml"); }
        }

        /// <summary>
        /// Gets the name of the compiled jsRender template for the left pane of the media view page.
        /// Ex: "gsp_g_lp_tmpl"
        /// </summary>
        public string LeftPaneTmplName
        {
            get { return String.Concat(this.GspClientId, "_lp_tmpl"); }
        }

        /// <summary>
        /// Gets the client ID for the DOM element that is to receive the contents of the left pane
        /// of the media view page. Ex: "gsp_g_lpHtml"
        /// </summary>
        public string RightPaneClientId
        {
            get { return String.Concat(this.GspClientId, "_rpHtml"); }
        }

        /// <summary>
        /// Gets the name of the compiled jsRender template for the right pane of the media view page.
        /// Ex: "gsp_g_rp_tmpl"
        /// </summary>
        public string RightPaneTmplName
        {
            get { return String.Concat(this.GspClientId, "_rp_tmpl"); }
        }

        /// <summary>
        /// Gets the value that uniquely identifies the gallery the current instance belongs to. This value is retrieved from the 
        /// requested media object or album, or from the <see cref="Gallery.GalleryId" /> property of the <see cref="Gallery" /> control 
        /// that created this instance. If no gallery ID is found by the previous search, then return the first gallery found in the database.
        /// Retrieving this value causes the <see cref="Gallery.GalleryId" /> on the containing control to be set to the same value.
        /// </summary>
        /// <value>The gallery ID for the current gallery.</value>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
        /// permission to view.</exception>
        public int GalleryId
        {
            get
            {
                if (_galleryId == int.MinValue)
                {
                    if (GetMediaObjectId() > int.MinValue)
                    {
                        _galleryId = GetMediaObject().GalleryId;
                    }
                    else if (ParseAlbumId() > int.MinValue)
                    {
                        _galleryId = GetAlbum().GalleryId;
                    }
                    else if (this.GalleryControl.GalleryId > int.MinValue && Factory.LoadGalleries().Any(g => g.GalleryId == this.GalleryControl.GalleryId))
                    {
                        _galleryId = this.GalleryControl.GalleryId;
                    }
                    else
                    {
                        // There is no album or media object to get the gallery ID from, and no gallery ID has been specified on the control.
                        // Just grab the first gallery in the database, creating it if necessary.
                        var gallery = Factory.LoadGalleries().FirstOrDefault();
                        if (gallery != null)
                        {
                            _galleryId = gallery.GalleryId;
                            this.GalleryControl.GalleryControlSettings.GalleryId = _galleryId;
                            this.GalleryControl.GalleryControlSettings.Save();
                        }
                        else
                        {
                            // No gallery found anywhere, including the data store. Create one and assign it to this control instance.
                            IGallery g = Factory.CreateGalleryInstance();
                            g.Description = "My gallery";
                            g.CreationDate = DateTime.UtcNow;
                            g.Save();
                            this.GalleryControl.GalleryControlSettings.GalleryId = g.GalleryId;
                            this.GalleryControl.GalleryControlSettings.Save();
                            _galleryId = g.GalleryId;
                        }
                    }
                }

                if (this.GalleryControl.GalleryId == int.MinValue)
                {
                    this.GalleryControl.GalleryId = _galleryId;
                }

                return _galleryId;
            }
        }

        /// <summary>
        /// Gets the gallery settings for the current gallery.
        /// </summary>
        /// <value>The gallery settings for the current gallery.</value>
        public IGallerySettings GallerySettings
        {
            get
            {
                return _gallerySetting;
            }
        }

        /// <summary>
        /// Gets or sets the page index when paging is enabled and active. This is one-based, so the first page is one, the second
        /// is two, and so one.
        /// </summary>
        /// <value>The current page index.</value>
        public int CurrentPage
        {
            get
            {
                if (this._currentPage == 0)
                {
                    int page = Utils.GetQueryStringParameterInt32("page");

                    this._currentPage = (page > 0 ? page : 1);
                }

                return this._currentPage;
            }
            set
            {
                this._currentPage = value;

                if (HttpContext.Current.Session != null)
                {
                    Uri backURL = this.PreviousUri;
                    if (backURL != null)
                    {
                        // Update the page query string parameter so that the referring url points to the current page index.
                        backURL = UpdateUriQueryString(backURL, "page", this._currentPage.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        backURL = UpdateUriQueryString(Utils.GetCurrentPageUri(), "page", this._currentPage.ToString(CultureInfo.InvariantCulture));
                    }
                    this.PreviousUri = backURL;
                }

            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user is anonymous. If the user has authenticated with a user name/password, 
        /// this property is false.
        /// </summary>
        public bool IsAnonymousUser
        {
            // Note: Do not store in a private field that lasts the lifetime of the page request, as this may give the wrong
            // value after logon and logoff events.
            get
            {
                return !Utils.IsAuthenticated;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to administer the site. If true, the user
        /// has all possible permissions and there is nothing he or she can't do.
        /// </summary>
        public bool UserCanAdministerSite
        {
            get
            {
                if (!this._userCanAddAdministerSite.HasValue)
                    EvaluateUserPermissions();

                return this._userCanAddAdministerSite.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the logged on user is a gallery administrator for the current gallery.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the logged on user is a gallery administrator for the current gallery; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanAdministerGallery
        {
            get
            {
                if (!this._userCanAdministerGallery.HasValue)
                    EvaluateUserPermissions();

                return this._userCanAdministerGallery.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to create a new album within the current album.
        /// </summary>
        public bool UserCanCreateAlbum
        {
            get
            {
                if (!this._userCanCreateAlbum.HasValue)
                    EvaluateUserPermissions();

                return this._userCanCreateAlbum.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to edit information about the current album.
        /// This includes changing the album's title, description, start and end dates, assigning the album's thumbnail image,
        /// and rearranging the order of objects within the album.
        /// </summary>
        public bool UserCanEditAlbum
        {
            get
            {
                if (!this._userCanEditAlbum.HasValue)
                    EvaluateUserPermissions();

                return this._userCanEditAlbum.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to add media objects to the current album.
        /// </summary>
        public bool UserCanAddMediaObject
        {
            get
            {
                if (!this._userCanAddMediaObject.HasValue)
                    EvaluateUserPermissions();

                return this._userCanAddMediaObject.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user is a site or gallery administrator for at least one other
        /// gallery besides the current one.
        /// </summary>
        public bool UserIsAdminForAtLeastOneOtherGallery
        {
            get
            {
                if (!this._userIsAdminForAtLeastOneOtherGallery.HasValue)
                {
                    this._userIsAdminForAtLeastOneOtherGallery = UserController.GetGalleriesCurrentUserCanAdminister().Any(g => g.GalleryId != this.GalleryId);
                }

                return this._userIsAdminForAtLeastOneOtherGallery.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to edit the current media object. This includes 
        /// changing the media object's caption, rotating the object (if it is an image), and deleting the high resolution
        /// version of the object (applies only if it is an image).
        /// </summary>
        public bool UserCanEditMediaObject
        {
            get
            {
                if (!this._userCanEditMediaObject.HasValue)
                    EvaluateUserPermissions();

                return this._userCanEditMediaObject.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to delete the current album.
        /// </summary>
        public bool UserCanDeleteCurrentAlbum
        {
            get
            {
                if (!this._userCanDeleteCurrentAlbum.HasValue)
                    EvaluateUserPermissions();

                return this._userCanDeleteCurrentAlbum.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to delete albums within the current album.
        /// </summary>
        public bool UserCanDeleteChildAlbum
        {
            get
            {
                if (!this._userCanDeleteChildAlbum.HasValue)
                    EvaluateUserPermissions();

                return this._userCanDeleteChildAlbum.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to delete a media object in the current album.
        /// </summary>
        public bool UserCanDeleteMediaObject
        {
            get
            {
                if (!this._userCanDeleteMediaObject.HasValue)
                    EvaluateUserPermissions();

                return this._userCanDeleteMediaObject.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to synchronize the current album.
        /// </summary>
        public bool UserCanSynchronize
        {
            get
            {
                if (!this._userCanSynchronize.HasValue)
                    EvaluateUserPermissions();

                return this._userCanSynchronize.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to view an image without a watermark applied to it.
        /// </summary>
        public bool UserDoesNotGetWatermark
        {
            get
            {
                if (!this._userDoesNotGetWatermark.HasValue)
                    EvaluateUserPermissions();

                return this._userDoesNotGetWatermark.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to view the current media object and album.
        /// </summary>
        public bool UserCanViewAlbumOrMediaObject
        {
            get
            {
                if (!this._userCanViewAlbumOrMediaObject.HasValue)
                    EvaluateUserPermissions();

                return this._userCanViewAlbumOrMediaObject.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has permission to view the original version of a media object.
        /// </summary>
        public bool UserCanViewOriginal
        {
            get
            {
                if (!this._userCanViewOriginal.HasValue)
                    EvaluateUserPermissions();

                return this._userCanViewOriginal.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has permission to add media objects to at least one album in the current gallery.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if current user has permission to add media objects to at least one album; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanAddMediaObjectToAtLeastOneAlbum
        {
            get
            {
                if (!this._userCanAddMediaObjectToAtLeastOneAlbum.HasValue)
                    EvaluateUserPermissions();

                return this._userCanAddMediaObjectToAtLeastOneAlbum.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has permission to add albums to at least one album in the current gallery.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the current user has permission to add albums to at least one album; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanAddAlbumToAtLeastOneAlbum
        {
            get
            {
                if (!this._userCanAddAlbumToAtLeastOneAlbum.HasValue)
                    EvaluateUserPermissions();

                return this._userCanAddAlbumToAtLeastOneAlbum.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has permission to edit at least one album in the current gallery.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the current user has permission to edit at least one album; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanEditAtLeastOneAlbum
        {
            get
            {
                if (!this._userCanEditAtLeastOneAlbum.HasValue)
                    EvaluateUserPermissions();

                return this._userCanEditAtLeastOneAlbum.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has permission to edit at least one media asset in the current gallery.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the current user has permission to edit at least one media asset; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanEditAtLeastOneMediaAsset
        {
            get
            {
                if (!this._userCanEditAtLeastOneMediaAsset.HasValue)
                    EvaluateUserPermissions();

                return this._userCanEditAtLeastOneMediaAsset.Value;
            }
        }

        /// <summary>
        /// Gets or sets the message to display to the user, such as "Invalid login". The value is retrieved from the
        /// "msgId" query string parameter or from a private field if it was explicitly assigned earlier in the current page's
        /// life cycle. Returns null if the parameter is not found, it is not a valid integer, or it is &lt;= 0.
        /// Setting this property sets a private field that lives as long as the current page lifecycle. It is not persisted across
        /// postbacks or added to the querystring. Set the value only when you will use it later in the current page's lifecycle.
        /// Defaults to null.
        /// </summary>
        protected ClientMessageOptions ClientMessage
        {
            get
            {
                if (_clientMessage == null)
                {
                    int msgId = Utils.GetQueryStringParameterInt32("msg");
                    if (msgId > int.MinValue)
                    {
                        var message = (MessageType)Enum.Parse(typeof(MessageType), msgId.ToString(CultureInfo.InvariantCulture));
                        _clientMessage = GetMessageOptions(message);
                    }
                }
                return _clientMessage;
            }
            set
            {
                _clientMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the value that identifies the type of gallery page that is currently being displayed.
        /// </summary>
        /// <value>The value that identifies the type of gallery page that is currently being displayed.</value>
        /// <exception cref="InvalidOperationException">Thrown when the property is accessed before it has been set.</exception>
        public PageId PageId
        {
            get
            {
                if (this._pageId == 0)
                    throw new InvalidOperationException("The PageId property has not been set to a valid value.");

                return this._pageId;
            }
            set
            {
                this._pageId = value;
            }
        }

        /// <summary>
        /// Gets or sets the instance of the user control that created this user control.
        /// </summary>
        /// <value>The user control that created this user control.</value>
        /// <exception cref="WebException">Thrown when an instance of the <see cref="Gallery" /> control is not found in the parent 
        /// heirarchy of the current control.</exception>
        public Gallery GalleryControl
        {
            get
            {
                if (_galleryControl != null)
                    return _galleryControl;

                System.Web.UI.Control ctl = Parent;
                while (ctl.GetType() != typeof(Gallery))
                {
                    ctl = ctl.Parent;
                    if (ctl == null)
                    {
                        throw new WebException(String.Format(CultureInfo.CurrentCulture, "Could not find an instance of {0} that contains the current control ({1}). All user controls in Gallery Server must be loaded dynamically within the {0} control.", typeof(Gallery), this.GetType()));
                    }
                }

                _galleryControl = (Gallery)ctl;
                return _galleryControl;
            }
            set
            {
                _galleryControl = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that can be used in the title tag in the HTML page header. If this property is not set by the user
        /// control, the current album's title is used.
        /// </summary>
        /// <value>A value that can be used in the title tag in the HTML page header.</value>
        public virtual string PageTitle
        {
            get
            {
                if (String.IsNullOrEmpty(_pageTitle))
                {
                    // Get an HTML-cleaned version of the current album's title, limited to the first 50 characters.
                    string title = Utils.RemoveHtmlTags(GetAlbum().Title);
                    title = title.Substring(0, title.Length < 50 ? title.Length : 50);

                    return String.Concat(Resources.GalleryServer.UC_ThumbnailView_Album_Title_Prefix_Text, " ", title);
                }
                else
                    return _pageTitle;
            }
            set
            {
                this._pageTitle = value;
            }
        }

        ///// <summary>
        ///// Gets a reference to the <see cref="albummenu"/> control on the page.
        ///// </summary>
        ///// <value>The <see cref="albummenu"/> control on the page.</value>
        //public Controls.albummenu AlbumMenu
        //{
        //  get
        //  {
        //    return this._albumMenu;
        //  }
        //}

        /// <summary>
        /// Gets a reference to the <see cref="Controls.galleryheader"/> control on the page.
        /// </summary>
        /// <value>The <see cref="Controls.galleryheader"/> control on the page.</value>
        public Controls.galleryheader GalleryHeader
        {
            get
            {
                return this._galleryHeader;
            }
        }

        /// <summary>
        /// Gets or sets the URI of the previous page the user was viewing. The value is stored in the user's session, and 
        /// can be used after a user has completed a task to return to the original page. If the Session object is not available,
        /// no value is saved in the setter and a null is returned in the getter.
        /// </summary>
        /// <value>The URI of the previous page the user was viewing.</value>
        public Uri PreviousUri
        {
            get
            {
                return Utils.PreviousUri;
            }
            set
            {
                Utils.PreviousUri = value;
            }
        }

        /// <summary>
        /// Gets the URL of the previous page the user was viewing. The value is based on the <see cref="PreviousUri" /> property
        /// and is relative to the website root. If <see cref="PreviousUri" /> is null, such as when the Session object is not
        /// available or it has never been assigned, return String.Empty. Remove the query string parameter "msg" if present. 
        /// Ex: "/gallery/gs/default.aspx?moid=770"
        /// </summary>
        /// <value>The URL of the previous page the user was viewing.</value>
        public string PreviousUrl
        {
            get
            {
                if (PreviousUri != null)
                    return Utils.RemoveQueryStringParameter(PreviousUri.PathAndQuery, "msg");
                else
                    return String.Empty;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to show the login controls at the top right of each page. When false, no login controls
        /// are shown, but the user can still navigate directly to the login page to log on. This value is retrieved from the 
        /// <see cref="Gallery.ShowLogin" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.ShowLogin" />. 
        /// </summary>
        /// <value><c>true</c> if login controls are visible; otherwise, <c>false</c>.</value>
        public bool ShowLogin
        {
            get
            {
                if (!_showLogin.HasValue)
                {
                    this._showLogin = this.GalleryControl.ShowLogin ?? this.GallerySettings.ShowLogin;
                }

                return this._showLogin.Value;
            }
            protected set { _showLogin = value; }
        }

        /// <summary>
        /// Gets a value indicating whether to show the search box at the top right of each page. This value is retrieved from the 
        /// <see cref="Gallery.ShowSearch" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.ShowSearch" />. 
        /// </summary>
        /// <value><c>true</c> if the search box is visible; otherwise, <c>false</c>.</value>
        public bool ShowSearch
        {
            get
            {
                if (!_showSearch.HasValue)
                {
                    this._showSearch = this.GalleryControl.ShowSearch ?? this.GallerySettings.ShowSearch;
                }

                return this._showSearch.Value;
            }
            protected set { _showSearch = value; }
        }

        /// <summary>
        /// Gets a value indicating whether users can view galleries without logging in. When false, users are redirected to a login
        /// page when any album is requested. Private albums are never shown to anonymous users, even when this property is true. 
        /// This value is retrieved from the <see cref="Gallery.AllowAnonymousBrowsing" /> property if specified; if not, it inherits 
        /// the value from <see cref="IGallerySettings.AllowAnonymousBrowsing" />.
        /// </summary>
        /// <value><c>true</c> if anonymous users can view the gallery; otherwise, <c>false</c>.</value>
        public bool AllowAnonymousBrowsing
        {
            get
            {
                if (!_allowAnonymousBrowsing.HasValue)
                {
                    this._allowAnonymousBrowsing = this.GalleryControl.AllowAnonymousBrowsing ?? this.GallerySettings.AllowAnonymousBrowsing;
                }

                return this._allowAnonymousBrowsing.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the left pane when an album is being displayed.
        /// This value is retrieved from the <see cref="Gallery.ShowLeftPaneForAlbum" /> property if specified; if not, it uses a 
        /// default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the left pane is to be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowLeftPaneForAlbum
        {
            get
            {
                if (!this._showLeftPaneForAlbum.HasValue)
                {
                    this._showLeftPaneForAlbum = this.GalleryControl.ShowLeftPaneForAlbum.GetValueOrDefault(true);
                }

                return this._showLeftPaneForAlbum.Value;
            }
            set
            {
                this._showLeftPaneForAlbum = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the left pane when a single media object is
        /// being displayed. This value is retrieved from the <see cref="Gallery.ShowLeftPaneForMediaObject" /> 
        /// property if specified; if not, it uses a default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the left pane is to be rendered when a single media object is being displayed; otherwise, <c>false</c>.
        /// </value>
        public bool ShowLeftPaneForMediaObject
        {
            get
            {
                if (!this._showLeftPaneForMediaObject.HasValue)
                {
                    this._showLeftPaneForMediaObject = this.GalleryControl.ShowLeftPaneForMediaObject.GetValueOrDefault(true);
                }

                return this._showLeftPaneForMediaObject.Value;
            }
            set
            {
                this._showLeftPaneForMediaObject = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the center pane. This value is retrieved from the <see cref="Gallery.ShowCenterPane" /> 
        /// property if specified; if not, it uses a default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the center pane is to be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCenterPane
        {
            get
            {
                if (!this._showCenterPane.HasValue)
                {
                    this._showCenterPane = this.GalleryControl.ShowCenterPane.GetValueOrDefault(true);
                }

                return this._showCenterPane.Value;
            }
            set
            {
                this._showCenterPane = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render the right pane. This value is retrieved from the <see cref="Gallery.ShowRightPane" /> 
        /// property if specified; if not, it uses a default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the right pane is to be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowRightPane
        {
            get
            {
                if (!this._showRightPane.HasValue)
                {
                    this._showRightPane = this.GalleryControl.ShowRightPane.GetValueOrDefault(true);
                }

                return this._showRightPane.Value;
            }
            set
            {
                this._showRightPane = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to render the ribbon toolbar. This value is retrieved from the 
        /// <see cref="Gallery.ShowRibbonToolbar" /> property if specified; if not, it uses a default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the ribbon toolbar is to be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowRibbonToolbar
        {
            get
            {
                if (!this._showRibbonToolbar.HasValue)
                {
                    this._showRibbonToolbar = this.GalleryControl.ShowRibbonToolbar ?? true;
                }

                return this._showRibbonToolbar.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to render the album bread crumb links. This value is retrieved from the 
        /// <see cref="Gallery.ShowAlbumBreadCrumb" /> property if specified; if not, it uses a default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the album bread crumb links are to be visible; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAlbumBreadCrumb
        {
            get
            {
                if (!this._showAlbumBreadCrumb.HasValue)
                {
                    this._showAlbumBreadCrumb = this.GalleryControl.ShowAlbumBreadCrumb ?? true;
                }

                return this._showAlbumBreadCrumb.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to render the header at the top of the gallery. This value is retrieved from the 
        /// <see cref="Gallery.ShowHeader" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.ShowHeader" />.
        /// The header includes the gallery title, login/logout controls, user account management link, and search 
        /// function. The title, login/logout controls and search function can be individually controlled via the <see cref="GalleryTitle" />,
        /// <see cref="ShowLogin" /> and <see cref="ShowSearch" /> properties.
        /// </summary>
        /// <value><c>true</c> if the header is to be dislayed; otherwise, <c>false</c>.</value>
        public bool ShowHeader
        {
            get
            {
                if (!this._showHeader.HasValue)
                {
                    this._showHeader = this.GalleryControl.ShowHeader ?? this.GallerySettings.ShowHeader;
                }

                return this._showHeader.Value;
            }
        }

        /// <summary>
        /// Gets the header text that appears at the top of each web page. This value is retrieved from the 
        /// <see cref="Gallery.GalleryTitle" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.GalleryTitle" />.
        /// </summary>
        /// <value>The gallery title.</value>
        public string GalleryTitle
        {
            get
            {
                if (_galleryTitle == null)
                {
                    this._galleryTitle = (GalleryControl.GalleryTitle != null ? this.GalleryControl.GalleryTitle : this.GallerySettings.GalleryTitle);
                }

                return this._galleryTitle;
            }
        }

        /// <summary>
        /// Gets the URL the user will be directed to when she clicks the gallery title. This value is retrieved from the 
        /// <see cref="Gallery.GalleryTitleUrl" /> property if specified; if not, it inherits the value from <see cref="IGallerySettings.GalleryTitleUrl" />.
        /// </summary>
        /// <value>The gallery title.</value>
        public string GalleryTitleUrl
        {
            get
            {
                if (_galleryTitleUrl == null)
                {
                    this._galleryTitleUrl = (GalleryControl.GalleryTitleUrl != null ? this.GalleryControl.GalleryTitleUrl : this.GallerySettings.GalleryTitleUrl);
                }

                return this._galleryTitleUrl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the title is displayed beneath individual media objects. This value is retrieved from the 
        /// <see cref="Gallery.ShowMediaObjectTitle" /> property if specified; if not, it uses a default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the title is displayed beneath individual media objects; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMediaObjectTitle
        {
            get
            {
                if (!this._showMediaObjectTitle.HasValue)
                {
                    this._showMediaObjectTitle = this.GalleryControl.ShowMediaObjectTitle ?? true;
                }

                return this._showMediaObjectTitle.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the next and previous buttons are rendered for individual media objects. This value is retrieved 
        /// from the <see cref="Gallery.ShowMediaObjectNavigation" /> property if specified; if not, it uses a default value of <c>true</c>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the next and previous buttons are rendered for individual media objects; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMediaObjectNavigation
        {
            get
            {
                if (!this._showMediaObjectNavigation.HasValue)
                {
                    this._showMediaObjectNavigation = this.GalleryControl.ShowMediaObjectNavigation ?? true;
                }

                return this._showMediaObjectNavigation.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to display the relative position of a media object within an album (example: (3 of 24)). 
        /// This value is retrieved from the <see cref="Gallery.ShowMediaObjectNavigation" /> property if specified; if not, it uses a 
        /// default value of <c>true</c>. Applicable only when a single media object is displayed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the relative position of a media object within an album is to be rendered; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMediaObjectIndexPosition
        {
            get
            {
                if (!this._showMediaObjectIndexPosition.HasValue)
                {
                    this._showMediaObjectIndexPosition = this.GalleryControl.ShowMediaObjectIndexPosition ?? true;
                }

                return this._showMediaObjectIndexPosition.Value;
            }
        }

        /// <summary>
        /// Gets or sets the size of media assets to display when viewing a single media asset. This value is retrieved from the user's profile 
        /// if it exists, then the <see cref="Gallery.MediaViewSize" /> property if specified. As a last resort, it inherits the value from 
        /// <see cref="IGallerySettings.MediaViewSize" />.
        /// </summary>
        /// <value>An instance of <see cref="DisplayObjectType" />.</value>
        public DisplayObjectType MediaViewSize
        {
            get
            {
                if (this._mediaViewSize == DisplayObjectType.Unknown)
                {
                    var userProfileMediaViewSize = ProfileController.GetProfileForGallery(Utils.UserName, GalleryId).MediaViewSize;

                    if (userProfileMediaViewSize != DisplayObjectType.Unknown)
                    {
                        this._mediaViewSize = userProfileMediaViewSize;
                    }
                    else
                    {
                        var mediaViewSize = this.GalleryControl.MediaViewSize;
                        this._mediaViewSize = (mediaViewSize != DisplayObjectType.Unknown ? mediaViewSize : GallerySettings.MediaViewSize);
                    }

                    // If user is not allowed to view original, then revert to optimized
                    if (_mediaViewSize == DisplayObjectType.Original && !UserCanViewOriginal)
                    {
                        _mediaViewSize = DisplayObjectType.Optimized;
                    }
                }

                return this._mediaViewSize;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the type of slide show to use for images. This value is retrieved from the user's profile 
        /// if it exists, then the <see cref="Gallery.SlideShowType" /> property if specified. As a last resort, it inherits the value from 
        /// <see cref="IGallerySettings.SlideShowType" />.
        /// </summary>
        /// <value>An instance of <see cref="SlideShowType" />.</value>
        public SlideShowType SlideShowType
        {
            get
            {
                if (this._slideShowType == SlideShowType.NotSet)
                {
                    var userProfileSlideShowType = ProfileController.GetProfileForGallery(Utils.UserName, GalleryId).SlideShowType;

                    if (userProfileSlideShowType != SlideShowType.NotSet)
                    {
                        this._slideShowType = userProfileSlideShowType;
                    }
                    else
                    {
                        var slideShowType = GalleryControl.SlideShowType;
                        this._slideShowType = (slideShowType != SlideShowType.NotSet ? slideShowType : GallerySettings.SlideShowType);
                    }
                }

                return this._slideShowType;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a slide show continues from the beginning after showing the last media asset. This value is retrieved 
        /// from the user's profile if it exists, then the <see cref="Gallery.SlideShowLoop" /> property if specified. As a last resort, it inherits the value from 
        /// <see cref="IGallerySettings.SlideShowLoop" />.
        /// </summary>
        /// <value><c>true</c> when the slide show loops; otherwise <c>false</c>.</value>
        public bool SlideShowLoop
        {
            get
            {
                if (!this._slideShowLoop.HasValue)
                {
                    var userProfileSlideShowLoop = ProfileController.GetProfileForGallery(Utils.UserName, GalleryId).SlideShowLoop;

                    if (userProfileSlideShowLoop.HasValue)
                    {
                        this._slideShowLoop = userProfileSlideShowLoop.Value;
                    }
                    else
                    {
                        this._slideShowLoop = this.GalleryControl.SlideShowLoop ?? GallerySettings.SlideShowLoop;
                    }
                }

                return this._slideShowLoop.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a slide show of image media objects automatically starts playing when the page loads. This value is retrieved 
        /// from the <see cref="Gallery.AutoPlaySlideShow" /> property if specified; if not, it uses a default value of <c>false</c>. This setting 
        /// applies only when the application is showing a single media object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a slide show of image media objects will automatically start playing; otherwise, <c>false</c>.
        /// </value>
        public bool AutoPlaySlideShow
        {
            get
            {
                if (!_autoPlaySlideShow.HasValue)
                {
                    _autoPlaySlideShow = Utils.GetQueryStringParameterBoolean("ss");

                    if (!_autoPlaySlideShow.HasValue)
                        _autoPlaySlideShow = GalleryControl.AutoPlaySlideShow.GetValueOrDefault(false);
                }

                return _autoPlaySlideShow.Value;
            }
        }

        /// <summary>
        /// Gets the ID for the hidden field that contains the media object ID. This hidden field is updated via javascript
        /// as a user navigates within an album and can be used by the server to determine the current media object the user
        /// is viewing.
        /// </summary>
        /// <value>The ID for the hidden field that contains the media object ID.</value>
        private string HiddenFieldMediaObjectId
        {
            get { return String.Concat(this.ClientID, "_moid"); }
        }

        /// <summary>
        /// Gets the UI templates used to render various aspects of the page. Returns templates belonging to <see cref="GalleryId" />.
        /// Guaranteed to not return null.
        /// </summary>
        /// <value>An instance of <see cref="IUiTemplateCollection" />.</value>
        public IUiTemplateCollection UiTemplates
        {
            get { return _uiTemplates ?? (_uiTemplates = new UiTemplateCollection(GalleryController.GetUiTemplates().Where(t => t.GalleryId == GalleryId))); }
        }

        /// <summary>
        /// Gets the Gallery Server logo.
        /// </summary>
        /// <value>An instance of <see cref="LiteralControl" />.</value>
        protected LiteralControl GsLogo
        {
            get
            {
                var tooltip = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Footer_Logo_Tooltip, Utils.GetGalleryServerVersion());

                return new LiteralControl(String.Format(CultureInfo.InvariantCulture, @"<footer class='gsp_addtopmargin5 gsp_footer'>
        <a href='https://galleryserverpro.com' title='{0}'>
         <img src='{1}' alt='{0}' />
        </a>
       </footer>",
                                                        tooltip,
                                                        Page.ClientScript.GetWebResourceUrl(this.GetType().BaseType, "GalleryServer.Web.App_GlobalResources.gs-ftr-logo.png")));
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs just before the gallery header and album breadcrumb menu controls are added to the control collection. This event is an
        /// opportunity for inheritors to insert controls of their own at the zero position using the Controls.AddAt(0, myControl) method.
        /// Viewstate is lost if inheritors add controls at any index other than 0, so the way to deal with this is to use this 
        /// event handler to add controls. For example, the Site Settings admin menu is added in the event handler in the <see cref="AdminPage"/> class.
        /// </summary>
        protected event System.EventHandler BeforeHeaderControlsAdded;

        #endregion

        #region Public Methods

        /// <overloads>
        /// Gets the album ID corresponding to the current album.
        /// </overloads>
        /// <summary>
        /// Gets the album ID corresponding to the current album. The value is determined in the following sequence: (1) If 
        /// <see cref="GetMediaObject" /> returns an object (which will happen when a particular media object has been requested), then 
        /// use the album ID of the media object's parent. (2) When no media object is available, then look for the "aid" query string 
        /// parameter. (3) If not there, or if <see cref="Gallery.AllowUrlOverride" /> has been set to <c>false</c>, look for an album 
        /// ID on the containing <see cref="Gallery" /> control. (4) If we haven't found an album yet, load the top-level album 
        /// for which the current user has view permission. This function verifies the album exists and the current user has permission 
        /// to view it. If the album does not exist, a <see cref="InvalidAlbumException" /> is thrown. If the user does not have permission to
        /// view the album, a <see cref="GallerySecurityException" /> is thrown. Guaranteed to return a valid album ID, except
        /// when the user does not have view permissions to any album and when the top-level album is a virtual album, in which case
        /// it returns <see cref="Int32.MinValue" />.
        /// </summary>
        /// <returns>Returns the album ID corresponding to the current album.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
        /// permission to view.</exception>
        public int GetAlbumId()
        {
            if (_album != null)
            {
                return _album.Id;
            }
            else
            {
                return GetAlbumId(out _album);
            }
        }

        /// <summary>
        /// Gets the album ID corresponding to the current album and assigns the album to the <paramref name="album" /> parameter. 
        /// The value is determined in the following sequence: (1) If <see cref="GalleryPage.GetMediaObject"/> returns an 
        /// object (which will happen when a particular media object has been requested), then use the album ID of the 
        /// media object's parent. (2) When no media object is available, then look for the "aid" query string parameter.
        /// (3) If not there, or if <see cref="Gallery.AllowUrlOverride"/> has been set to <c>false</c>, look for an album
        /// ID on the containing <see cref="Gallery"/> control. (4) If we haven't found an album yet, load the top-level album
        /// for which the current user has view permission. This function verifies the album exists and the current user has permission
        /// to view it. If the album does not exist, a <see cref="InvalidAlbumException" /> is thrown. If the user does not have permission to
        /// view the album, a <see cref="GallerySecurityException" /> is thrown. Guaranteed to return a valid album ID, except
        /// when the user does not have view permissions to any album and when the top-level album is a virtual album, in which case
        /// it returns <see cref="Int32.MinValue"/>.
        /// </summary>
        /// <param name="album">The album associated with the current page.</param>
        /// <returns>
        /// Returns the album ID corresponding to the current album. 
        /// </returns>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
        /// permission to view.</exception>
        public int GetAlbumId(out IAlbum album)
        {
            if (_album != null)
            {
                album = _album;
                return album.Id;
            }

            int aid;
            var navEnabled = this.GalleryControl.AllowUrlOverride;

            // First look for title/caption search text in the query string.
            if (navEnabled && Utils.IsQueryStringParameterPresent("title"))
            {
                _album = GalleryObjectController.GetGalleryObjectsHavingTitleOrCaption(Utils.GetQueryStringParameterStrings("title"), GetGalleryObjectFilter(), GalleryControl.GalleryId);
                aid = _album.Id;
            }
            // Then look for search text in the query string.
            else if (navEnabled && Utils.IsQueryStringParameterPresent("search"))
            {
                _album = GalleryObjectController.GetGalleryObjectsHavingSearchString(Utils.GetQueryStringParameterStrings("search"), GetGalleryObjectFilter(), GalleryControl.GalleryId);
                aid = _album.Id;
            }
            // Then look for tags in the query string.
            else if (navEnabled && (Utils.IsQueryStringParameterPresent("tag") || Utils.IsQueryStringParameterPresent("people")))
            {
                _album = GalleryObjectController.GetGalleryObjectsHavingTags(Utils.GetQueryStringParameterStrings("tag"), Utils.GetQueryStringParameterStrings("people"), GetGalleryObjectFilter(), GalleryControl.GalleryId);
                aid = _album.Id;
            }
            // Then look for a request for the rated objects in the query string.
            else if (navEnabled && Utils.IsQueryStringParameterPresent("rating") && AppSetting.Instance.License.LicenseType >= LicenseLevel.HomeNonprofit)
            {
                _album = GalleryObjectController.GetRatedMediaObjects(Utils.GetQueryStringParameterString("rating"), Utils.GetQueryStringParameterInt32("top"), GalleryControl.GalleryId, GetGalleryObjectFilter(GalleryObjectType.MediaObject));
                aid = _album.Id;
            }
            // Then look for a request for the latest objects in the query string.
            else if (navEnabled && Utils.IsQueryStringParameterPresent("latest") && AppSetting.Instance.License.LicenseType >= LicenseLevel.HomeNonprofit)
            {
                _album = GalleryObjectController.GetMostRecentlyAddedGalleryObjects(Utils.GetQueryStringParameterInt32("latest"), GalleryControl.GalleryId, GetGalleryObjectFilter(GalleryObjectType.MediaObject));
                aid = _album.Id;
            }
            else
            {
                // If we have a media object, get it's album ID.
                IGalleryObject mediaObject = GetMediaObject();

                aid = mediaObject != null ? mediaObject.Parent.Id : ParseAlbumId();

                if (aid > int.MinValue)
                {
                    ValidateAlbum(aid, out _album);
                }

                else
                {
                    // Nothing in viewstate, the query string, and no media object is specified. Get the highest album the user can view.
                    _album = GetHighestAlbumUserCanView();
                    aid = _album.Id;
                }
            }

            album = _album;

            return aid;
        }

        /// <summary>
        /// Get an inflated album instance for the current album. The album can be specified in the following places:  (1) Through 
        /// the <see cref="Gallery.AlbumId" /> property of the Gallery user control (2) From the requested media object by accessing its 
        /// parent object (3) Through the "aid" query string parameter. If this album contains child objects, they are added but not inflated. 
        /// If the album does not exist, a <see cref="InvalidAlbumException" /> is thrown. If the user does not have permission to
        /// view the album, a <see cref="GallerySecurityException" /> is thrown. Guaranteed to never return null.
        /// </summary>
        /// <returns>Returns an IAlbum object.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
        /// permission to view.</exception>
        public IAlbum GetAlbum()
        {
            if (this._album == null)
            {
                int albumId = GetAlbumId(); // Getting the album ID will set the _album variable.

                if (this._album == null)
                    throw new InvalidOperationException("Retrieving the album ID should have also assigned an album to the _album member variable, but it did not.");
            }

            return this._album;
        }

        //public void SetAlbumId(int albumId)
        //{
        //  ValidateAlbum(albumId);

        //  ViewState["aid"] = albumId;
        //  this._mediaObject = null;
        //  this._album = null;
        //  this._galleryId = int.MinValue;
        //}

        /// <summary>
        /// Gets the media object ID corresponding to the current media object, or <see cref="Int32.MinValue" /> if no valid media 
        /// object is available. The value is determined in the following sequence: (1) See if code earlier in the page's life cycle
        /// assigned an ID to the class member variable (this happens during Ajax postbacks). (2) Look for the "moid" query string parameter.
        /// (3) If not there, or if <see cref="Gallery.AllowUrlOverride" /> has been set to <c>false</c>, look at the <see cref="Gallery" />
        /// control to see if we need to get a media object. This function verifies the media object exists and the 
        /// current user has permission to view it. If either is not true, the function returns <see cref="Int32.MinValue"/>.
        /// </summary>
        /// <returns>Returns the media object ID corresponding to the current media object, or <see cref="Int32.MinValue" /> if 
        /// no valid media object is available.</returns>
        public int GetMediaObjectId()
        {
            if (_mediaObject != null)
            {
                return _mediaObject.Id; // We already figured out the media object for this page instance, so just get the ID.
            }

            int moid;

            // See if it has been assigned to the member variable. This happens during Ajax postbacks.
            if (this._mediaObjectId.HasValue)
            {
                moid = this._mediaObjectId.Value;
            }
            else
            {
                // Try to figure it out based on the query string and various <see cref="Gallery" /> control properties.
                this._mediaObjectId = ParseMediaObjectId();
                moid = this._mediaObjectId.Value;
            }

            if ((moid > int.MinValue) && !ValidateMediaObject(moid, out _mediaObject))
            {
                // Media object is not valid or user does not have permission to view it. Default to int.MinValue.
                moid = int.MinValue;
            }

            return moid;
        }

        /// <summary>
        /// Get a fully inflated, properly typed media object instance for the requested media object. The media object can be specified 
        /// in the following places:  (1) Through the <see cref="Gallery.MediaObjectId" /> property of the Gallery user control (2) Through 
        /// the "moid" query string parameter. If the requested media object doesn't exist or the user does not have permission to view it, 
        /// a null value is returned. An automatic security check is performed to make sure the user has view permission for the specified 
        /// media object.
        /// </summary>
        /// <returns>Returns an <see cref="IGalleryObject" /> object that represents the relevant derived media object type 
        /// (e.g. <see cref="Image" />, <see cref="Video" />, etc), or null if no media object is specified.</returns>
        public IGalleryObject GetMediaObject()
        {
            if (this._mediaObject == null)
            {
                int mediaObjectId = GetMediaObjectId(); // If a media object has been requested, getting its ID will set the _mediaObject variable.

                if ((mediaObjectId > int.MinValue) && this._mediaObject == null)
                    throw new InvalidOperationException("Retrieving the media object ID should have also assigned a media object to the _mediaObject member variable, but it did not.");
            }

            return this._mediaObject;
        }

        /// <summary>
        /// Get an absolute URL to the thumbnail image of the specified gallery object. Either a media object or album may be specified. 
        /// Ex: "http://site.com/gallery/gs/handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
        /// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
        /// </summary>
        /// <param name="galleryObject">The gallery object for which an URL to its thumbnail image is to be generated.
        /// Either a media object or album may be specified.</param>
        /// <returns>Returns the URL to the thumbnail image of the specified gallery object.</returns>
        public static string GetThumbnailUrl(IGalleryObject galleryObject)
        {
            return GetGalleryObjectUrl(galleryObject, DisplayObjectType.Thumbnail);
        }

        /// <summary>
        /// Get an absolute URL to the optimized image of the specified gallery object.
        /// Ex: "http://site.com/gallery/gs/handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
        /// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
        /// </summary>
        /// <param name="galleryObject">The gallery object for which an URL to its optimized image is to be generated.</param>
        /// <returns>Returns the URL to the optimized image of the specified gallery object.</returns>
        public static string GetOptimizedUrl(IGalleryObject galleryObject)
        {
            return GetGalleryObjectUrl(galleryObject, DisplayObjectType.Optimized);
        }

        /// <summary>
        /// Get an absolute URL to the original image of the specified gallery object.
        /// Ex: "http://site.com/gallery/gs/handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
        /// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
        /// </summary>
        /// <param name="galleryObject">The gallery object for which an URL to its original image is to be generated.</param>
        /// <returns>Returns the URL to the original image of the specified gallery object.</returns>
        public static string GetOriginalUrl(IGalleryObject galleryObject)
        {
            return GetGalleryObjectUrl(galleryObject, DisplayObjectType.Original);
        }

        /// <summary>
        /// Get an absolute URL to the thumbnail, optimized, or original media object.
        /// Ex: "http://site.com/gallery/gs/handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
        /// The URL can be used to assign to the src attribute of an image tag (&lt;img src='...' /&gt;).
        /// Not tested: It should be possible to pass an album and request the url to its thumbnail image.
        /// </summary>
        /// <param name="galleryObject">The gallery object for which an URL to the specified image is to be generated.</param>
        /// <param name="displayType">A DisplayObjectType enumeration value indicating the version of the
        /// object for which the URL should be generated. Possible values: Thumbnail, Optimized, Original.
        /// An exception is thrown if any other enumeration is passed.</param>
        /// <returns>Returns the URL to the thumbnail, optimized, or original version of the requested media object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
        public static string GetGalleryObjectUrl(IGalleryObject galleryObject, DisplayObjectType displayType)
        {
            if (galleryObject == null)
            {
                throw new ArgumentNullException("galleryObject");
            }

            if (galleryObject is Business.Album && (displayType != DisplayObjectType.Thumbnail))
            {
                throw new ArgumentException(String.Format("It is invalid to request an URL for an album display type '{0}'.", displayType));
            }

            var moBuilder = new MediaObjectHtmlBuilder(MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(galleryObject, displayType));

            return moBuilder.GetMediaObjectUrl();
        }

        /// <summary>
        /// Remove all HTML tags from the specified string and HTML-encodes the result.
        /// </summary>
        /// <param name="textWithHtml">The string containing HTML tags to remove.</param>
        /// <returns>Returns a string with all HTML tags removed, including the brackets.</returns>
        /// <returns>Returns an HTML-encoded string with all HTML tags removed.</returns>
        public string RemoveHtmlTags(string textWithHtml)
        {
            // Return the text with all HTML removed.
            return Utils.HtmlEncode(Utils.RemoveHtmlTags(textWithHtml));
        }

        /// <overloads>
        /// Throw a <see cref="GallerySecurityException" /> if the current user does not have the permission to perform the requested action.
        /// </overloads>
        /// <summary>
        /// Check to ensure user has permission to perform at least one of the specified security actions against the current album 
        /// (identified in <see cref="GetAlbumId()" />). Throw a <see cref="GallerySecurityException" />
        /// if the permission isn't granted to the logged on user. Un-authenticated users (anonymous users) are always considered 
        /// NOT authorized (that is, this method returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
        /// or <see cref="SecurityActions.ViewOriginalMediaObject" />, since Gallery Server is configured by default to allow anonymous viewing access but it does 
        /// not allow anonymous editing of any kind. This method behaves similarly to <see cref="IsUserAuthorized(SecurityActions)" /> except that it throws an
        /// exception instead of returning false when the user is not authorized.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />).
        /// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
        /// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
        /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
        /// <exception cref="GallerySecurityException">Thrown when the logged on user 
        /// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission 
        /// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or 
        /// <see cref="SecurityActions.ViewOriginalMediaObject" />).</exception>
        public void CheckUserSecurity(SecurityActions securityActions)
        {
            CheckUserSecurity(securityActions, SecurityActionsOption.RequireOne);
        }

        /// <summary>
        /// Check to ensure user has permission to perform the specified security actions against the current album (identified in 
        /// <see cref="GetAlbumId()" />). Throw a <see cref="GallerySecurityException"/>
        /// if the permission isn't granted to the logged on user. When multiple security actions are passed, use 
        /// <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or only one item
        /// must be satisfied. Un-authenticated users (anonymous users) are always considered NOT authorized (that is, this method 
        /// returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or 
        /// <see cref="SecurityActions.ViewOriginalMediaObject"/>, since Gallery Server is configured by default to allow anonymous viewing access 
        /// but it does not allow anonymous editing of any kind. This method behaves similarly to 
        /// <see cref="IsUserAuthorized(SecurityActions, SecurityActionsOption)"/> except that 
        /// it throws an exception instead of returning false when the user is not authorized.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
        /// to be successful or only one item must be satisfied.</param>
        /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
        /// to be successful or just one. This parameter is applicable only when <paramref name="securityActions" /> contains more than one item.</param>
        /// <exception cref="GallerySecurityException">Thrown when the logged on user
        /// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission
        /// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or
        /// <see cref="SecurityActions.ViewOriginalMediaObject"/>).</exception>
        public void CheckUserSecurity(SecurityActions securityActions, SecurityActionsOption secActionsOption)
        {
            if (!Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), this.GetAlbumId(), this.GalleryId, this.GetAlbum().IsPrivate, secActionsOption, this.GetAlbum().IsVirtualAlbum))
            {
                if (this.IsAnonymousUser)
                {
                    throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "Anonymous user does not have permission '{0}' for album ID {1}.", securityActions.ToString(), this.GetAlbumId()));
                }
                else
                {
                    throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, securityActions.ToString(), this.GetAlbumId()));
                }
            }
        }

        /// <summary>
        /// Check to ensure user has permission to perform at least one of the specified security actions for the specified <paramref name="album" />. 
        /// Throw a <see cref="GallerySecurityException" /> if the permission isn't granted to the logged on user. Un-authenticated users 
        /// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested security 
        /// action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalMediaObject" />, since 
        /// Gallery Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of any kind. 
        /// This method behaves similarly to <see cref="IsUserAuthorized(SecurityActions, IAlbum)" /> except that it throws an exception 
        /// instead of returning false when the user is not authorized.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
        /// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
        /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
        /// <param name="album">The album for which the security check is to be applied.</param>
        /// <exception cref="GallerySecurityException">Thrown when the logged on user
        /// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission
        /// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or
        /// <see cref="SecurityActions.ViewOriginalMediaObject"/>).</exception>
        public void CheckUserSecurity(SecurityActions securityActions, IAlbum album)
        {
            CheckUserSecurity(securityActions, album, SecurityActionsOption.RequireOne);
        }

        /// <summary>
        /// Check to ensure user has permission to perform the specified security actions for the specified <paramref name="album" />. 
        /// Throw a <see cref="GallerySecurityException" /> if the permission isn't granted to the logged on user. When multiple 
        /// security actions are passed, use <paramref name="secActionsOption" /> to specify whether all of the actions must be 
        /// satisfied to be successful or only one item must be satisfied. Un-authenticated users (anonymous users) are always 
        /// considered NOT authorized (that is, this method returns false) except when the requested security action is 
        /// <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or <see cref="SecurityActions.ViewOriginalMediaObject"/>, since Gallery 
        /// Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of any kind. 
        /// This method behaves similarly to <see cref="IsUserAuthorized(SecurityActions, IAlbum, SecurityActionsOption)"/> except 
        /// that it throws an exception instead of returning false when the user is not authorized.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
        /// to be successful or only one item must be satisfied.</param>
        /// <param name="album">The album for which the security check is to be applied.</param>
        /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
        /// to be successful or just one. This parameter is applicable only when <paramref name="securityActions" /> contains more than one item.</param>
        /// <exception cref="GallerySecurityException">Thrown when the logged on user
        /// does not belong to a role that authorizes the specified security action, or if an anonymous user is requesting any permission
        /// other than a viewing-related permission (i.e., <see cref="SecurityActions.ViewAlbumOrMediaObject"/> or
        /// <see cref="SecurityActions.ViewOriginalMediaObject"/>).</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        public void CheckUserSecurity(SecurityActions securityActions, IAlbum album, SecurityActionsOption secActionsOption)
        {
            if (album == null)
                throw new ArgumentNullException("album");

            if (!Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), album.Id, album.GalleryId, album.IsPrivate, secActionsOption, album.IsVirtualAlbum))
            {
                if (this.IsAnonymousUser)
                {
                    throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "Anonymous user does not have permission '{0}' for album ID {1}.", securityActions.ToString(), album.Id));
                }
                else
                {
                    throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "User '{0}' does not have permission '{1}' for album ID {2}.", Utils.UserName, securityActions.ToString(), album.Id));
                }
            }
        }

        /// <overloads>
        /// Determine if the current user has permission to perform the requested action.
        /// </overloads>
        /// <summary>
        /// Determine whether user has permission to perform at least one of the specified security actions against the current album 
        /// (identified in <see cref="GetAlbumId()" />). Un-authenticated users (anonymous users) are always considered NOT authorized (that 
        /// is, this method returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
        /// or <see cref="SecurityActions.ViewOriginalMediaObject" />, since Gallery Server is configured by default to allow anonymous viewing 
        /// access but it does not allow anonymous editing of any kind.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
        /// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
        /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
        /// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
        public bool IsUserAuthorized(SecurityActions securityActions)
        {
            return IsUserAuthorized(securityActions, SecurityActionsOption.RequireOne);
        }

        /// <summary>
        /// Determine whether user has permission to perform the specified security actions against the current album (identified in 
        /// <see cref="GetAlbumId()" />). When multiple security actions are passed, use 
        /// <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or only one item
        /// must be satisfied. Un-authenticated users (anonymous users) are always considered NOT authorized (that 
        /// is, this method returns false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
        /// or <see cref="SecurityActions.ViewOriginalMediaObject" />, since Gallery Server is configured by default to allow anonymous viewing 
        /// access but it does not allow anonymous editing of any kind.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
        /// to be successful or only one item must be satisfied. This parameter is applicable only when <paramref name="securityActions" /> 
        /// contains more than one item.</param>
        /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
        /// to be successful or just one.</param>
        /// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
        public bool IsUserAuthorized(SecurityActions securityActions, SecurityActionsOption secActionsOption)
        {
            return Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), this.GetAlbumId(), this.GalleryId, this.GetAlbum().IsPrivate, secActionsOption, this.GetAlbum().IsVirtualAlbum);
        }

        /// <summary>
        /// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users (anonymous users) are
        /// always considered NOT authorized (that is, this method returns false) except when the requested security action is
        /// <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalMediaObject" />, 
        /// since Gallery Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of 
        /// any kind. This method will continue to work correctly if the webmaster configures Gallery Server to require users to log 
        /// in in order to view objects, since at that point there will be no such thing as un-authenticated users, and the standard 
        /// gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
        /// 	a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// 	If multiple actions are specified, the method is successful if the user has permission for at least one of the actions.</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist 
        /// 	in this gallery.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="securityActions" /> is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> 
        /// or <see cref="SecurityActions.ViewOriginalMediaObject" /> and the user is anonymous (not logged on).</exception>
        internal bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId, bool isVirtualAlbum)
        {
            if (((securityActions == SecurityActions.ViewAlbumOrMediaObject) || (securityActions == SecurityActions.ViewOriginalMediaObject))
                && (!Utils.IsAuthenticated))
                throw new NotSupportedException("Wrong method call: You must call the overload of GalleryPage.IsUserAuthorized that has the isPrivate parameter when the security action is ViewAlbumOrMediaObject or ViewOriginalImage and the user is anonymous (not logged on).");

            return IsUserAuthorized(securityActions, albumId, galleryId, false, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users (anonymous users) are
        /// always considered NOT authorized (that is, this method returns false) except when the requested security action is
        /// <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalMediaObject" />,
        /// since Gallery Server is configured by default to allow anonymous viewing access but it does not allow anonymous editing of
        /// any kind. This method will continue to work correctly if the webmaster configures Gallery Server to require users to log
        /// in in order to view objects, since at that point there will be no such thing as un-authenticated users, and the standard
        /// gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />).
        /// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions.</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist
        /// in this gallery.</param>
        /// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
        /// is ignored for logged on users.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        internal bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId, bool isPrivate, bool isVirtualAlbum)
        {
            return Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), albumId, galleryId, isPrivate, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether user has permission to perform at least one of the specified security actions against the specified <paramref name="album" />. 
        /// Un-authenticated users (anonymous users) are always considered NOT authorized (that is, this method returns false) except 
        /// when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or 
        /// <see cref="SecurityActions.ViewOriginalMediaObject" />, since Gallery Server is configured by default to allow anonymous viewing access 
        /// but it does not allow anonymous editing of any kind.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using 
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
        /// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
        /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
        /// <param name="album">The album for which the security check is to be applied.</param>
        /// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
        public bool IsUserAuthorized(SecurityActions securityActions, IAlbum album)
        {
            return IsUserAuthorized(securityActions, album, SecurityActionsOption.RequireOne);
        }

        /// <summary>
        /// Determine whether user has permission to perform the specified security action against the specified album. If no album 
        /// is specified, then the current album (as returned by GetAlbum()) is used. Un-authenticated users (anonymous users) are 
        /// always considered NOT authorized (that is, this method returns false) except when the requested security action is 
        /// ViewAlbumOrMediaObject or ViewOriginalImage, since Gallery Server is configured by default to allow anonymous viewing access
        /// but it does not allow anonymous editing of any kind.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
        /// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied 
        /// to be successful or only one item must be satisfied.</param>
        /// <param name="album">The album for which the security check is to be applied.</param>
        /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
        /// to be successful or just one. This parameter is applicable only when <paramref name="securityActions" /> contains more than one item.</param>
        /// <returns>Returns true when the user is authorized to perform the specified security action; otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
        public bool IsUserAuthorized(SecurityActions securityActions, IAlbum album, SecurityActionsOption secActionsOption)
        {
            if (album == null)
                throw new ArgumentNullException("album");

            return Utils.IsUserAuthorized(securityActions, GetGalleryServerRolesForUser(), album.Id, album.GalleryId, album.IsPrivate, secActionsOption, album.IsVirtualAlbum);
        }

        /// <summary>
        /// Gets Gallery Server roles representing the roles for the currently logged-on user and belonging to the current gallery. 
        /// Returns an empty collection if no user is logged in or the user is logged in but not assigned to any roles relevant 
        /// to the current gallery (Count = 0).
        /// </summary>
        /// <returns>Returns a collection of Gallery Server roles representing the roles for the currently logged-on user. 
        /// Returns an empty collection if no user is logged in or the user is logged in but not assigned to any roles relevant 
        /// to the current gallery (Count = 0).</returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public IGalleryServerRoleCollection GetGalleryServerRolesForUser()
        {
            if (this._roles == null)
            {
                this._roles = RoleController.GetGalleryServerRolesForUser();
            }

            return this._roles;
        }

        /// <overloads>
        /// Redirect the user to the previous page he or she was on, optionally appending a query string name/value.
        /// </overloads>
        /// <summary>
        /// Redirect the user to the previous page he or she was on. The previous page is retrieved from a session variable that was stored during 
        /// the Page_Init event. If the original query string contains a "msg" parameter, it is removed so that the message 
        /// is not shown again to the user. If no previous page URL is available - perhaps because the user navigated directly to
        /// the page or has just logged in - the user is redirected to the application root.
        /// </summary>
        public void RedirectToPreviousPage()
        {
            RedirectToPreviousPage(String.Empty, String.Empty);
        }

        /// <summary>
        /// Redirect the user to the previous page he or she was on. If a query string name/pair value is specified, append that 
        /// to the URL.
        /// </summary>
        /// <param name="queryStringName">The query string name.</param>
        /// <param name="queryStringValue">The query string value.</param>
        public void RedirectToPreviousPage(string queryStringName, string queryStringValue)
        {
            #region Validation

            if (!String.IsNullOrEmpty(queryStringName) && String.IsNullOrEmpty(queryStringValue))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The queryStringValue parameter is required when the queryStringName parameter is specified. (queryStringName='{0}', queryStringValue='{1}')", queryStringName, queryStringValue));

            if (!String.IsNullOrEmpty(queryStringValue) && String.IsNullOrEmpty(queryStringName))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The queryStringName parameter is required when the queryStringValue parameter is specified. (queryStringName='{0}', queryStringValue='{1}')", queryStringName, queryStringValue));

            #endregion

            string url = this.PreviousUrl;

            if (String.IsNullOrEmpty(url))
                url = Utils.GetCurrentPageUrl(); // No previous url is available. Default to the current page.

            if (!String.IsNullOrEmpty(queryStringName))
                url = Utils.AddQueryStringParameter(url, String.Concat(queryStringName, "=", queryStringValue));

            this.PreviousUri = null;

            Page.Response.Redirect(url, true);
        }

        /// <overloads>Redirects to album view page of the current album.</overloads>
        /// <summary>
        /// Redirects to album view page of the current album.
        /// </summary>
        public void RedirectToAlbumViewPage()
        {
            Utils.Redirect(PageId.album, "aid={0}", GetAlbumId());
        }

        /// <summary>
        /// Redirects to album view page of the current album and with the specified <paramref name="args"/> appended as query string 
        /// parameters. Example: If the current page is /dev/gs/gallery.aspx, the user is viewing album 218, <paramref name="format"/> 
        /// is "msg={0}", and <paramref name="args"/> is "23", this function redirects to /dev/gs/gallery.aspx?g=album&amp;aid=218&amp;msg=23.
        /// </summary>
        /// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
        /// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
        /// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
        public void RedirectToAlbumViewPage(string format, params object[] args)
        {
            if (format == null)
                format = String.Empty;

            if (format.StartsWith("?", StringComparison.Ordinal))
                format = format.Remove(0, 1); // Remove leading '?' if present

            string queryString = String.Format(CultureInfo.InvariantCulture, format, args);
            if (!queryString.StartsWith("&", StringComparison.Ordinal))
                queryString = String.Concat("&", queryString); // Append leading '&' if not present

            Utils.Redirect(PageId.album, String.Concat("aid={0}", queryString), GetAlbumId());
        }

        /// <summary>
        /// Recursively iterate through the children of the specified containing control, searching for a child control with
        /// the specified server ID. If the control is found, return it; otherwise return null. This method is useful for finding
        /// child controls of composite controls like GridView.
        /// </summary>
        /// <param name="containingControl">The containing control whose child controls should be searched.</param>
        /// <param name="id">The server ID of the child control to search for.</param>
        /// <returns>Returns a Control matching the specified server id, or null if no matching control is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="containingControl" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id" /> is null or an empty string.</exception>
        public Control FindControlRecursive(Control containingControl, string id)
        {
            if (containingControl == null)
                throw new ArgumentNullException("containingControl");

            if (String.IsNullOrEmpty(id))
                throw new ArgumentException("The parameter 'id' is null or empty.");

            foreach (Control ctrl in containingControl.Controls)
            {
                if (ctrl.ID == id)
                    return ctrl;

                if (ctrl.HasControls())
                {
                    Control foundCtrl = FindControlRecursive(ctrl, id);
                    if (foundCtrl != null)
                        return foundCtrl;
                }
            }
            return null;
        }

        /// <summary>
        /// Record the error and optionally notify an administrator via e-mail.
        /// </summary>
        /// <param name="ex">The exception to record.</param>
        /// <returns>Returns an integer that uniquely identifies this application event (<see cref="IEvent.EventId"/>).</returns>
        public int LogError(Exception ex)
        {
            return AppEventController.LogError(ex, this.GalleryId).EventId;
        }

        /// <summary>
        /// Gets an unsorted collection of users the current user has permission to view. Users who have administer site permission can view all users.
        /// Users with administer gallery permission can only view users in galleries they have gallery admin permission in. Note that
        /// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
        /// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
        /// </summary>
        /// <returns>Returns an <see cref="IUserAccountCollection" /> containing a list of roles the user has permission to view.</returns>
        public IUserAccountCollection GetUsersCurrentUserCanView()
        {
            return UserController.GetUsersCurrentUserCanView(UserCanAdministerSite, UserCanAdministerGallery);
        }

        /// <summary>
        /// Gets a sorted list of roles the user has permission to view. Users who have administer site permission can view all roles.
        /// Users with administer gallery permission can only view roles they have been associated with or roles that aren't 
        /// associated with *any* gallery.
        /// </summary>
        /// <returns>Returns an <see cref="IGalleryServerRoleCollection" /> containing a list of roles the user has permission to view.</returns>
        public List<IGalleryServerRole> GetRolesCurrentUserCanView()
        {
            return RoleController.GetRolesCurrentUserCanView(UserCanAdministerSite, UserCanAdministerGallery);
        }

        /// <summary>
        /// Gets the HTML to display a nicely formatted thumbnail image of the specified <paramref name="galleryObject" />, including a 
        /// border, shadows and (possibly) rounded corners. This function is the same as calling the overloaded version with 
        /// includeHyperlinkToObject and allowAlbumTextWrapping parameters both set to <c>false</c>.
        /// </summary>
        /// <param name="galleryObject">The gallery object to be used as the source for the thumbnail image.</param>
        /// <returns>Returns HTML that displays a nicely formatted thumbnail image of the specified <paramref name="galleryObject" /></returns>
        public static string GetThumbnailHtml(IGalleryObject galleryObject)
        {
            var moBuilder = new MediaObjectHtmlBuilder(MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(galleryObject, DisplayObjectType.Thumbnail));

            return moBuilder.GetThumbnailHtml();
        }

        /// <summary>
        /// Gets the gallery data for the current media object, if one exists, or the current album.
        /// <see cref="Entity.GalleryData.Settings" /> is assigned, unlike when this object is retrieved
        /// through the web service (since the control-specific settings can't be determined in that case).
        /// </summary>
        /// <returns>Returns an instance of <see cref="Entity.GalleryData" />.</returns>
        public Entity.GalleryData GetClientGsData()
        {
            var data = GetMediaObjectId() > int.MinValue ?
              GalleryController.GetGalleryDataForMediaObject(GetMediaObject(), GetAlbum(), new Entity.GalleryDataLoadOptions { LoadMediaItems = true, Filter = GetGalleryObjectFilter() }) :
              GalleryController.GetGalleryDataForAlbum(GetAlbum(), new Entity.GalleryDataLoadOptions { LoadGalleryItems = true, Filter = GetGalleryObjectFilter() });

            data.Settings = GetSettingsEntity();

            return data;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            // Wrap HTML in an enclosing <div id="gsp_container" class="gsp_ns"> tag. The CSS class 'gsp_ns' is used as a pseudo namespace 
            // that is used to limit the influence CSS has to only the Gallery Server code, thus preventing the CSS from affecting 
            // HTML that may exist in the master page or other areas outside the user control.
            writer.AddAttribute("id", cid);
            writer.AddAttribute("class", "gsp_ns");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Write out the HTML for this control.
            base.Render(writer);

            // Close out the <div> tag.
            writer.RenderEndTag();
        }

        #endregion

        #region Private Static Methods


        /// <summary>
        /// Updates the query string parameter in the <paramref name="uri"/> with the specified value. If the 
        /// <paramref name="queryStringName"/> is not present, it is added. The modified URI is returned. The <paramref name="uri"/>
        /// is not modified.
        /// </summary>
        /// <param name="uri">The URI that is to receive the updated or added query string <paramref name="queryStringName">name</paramref>
        /// and <paramref name="queryStringValue">value</paramref>. This object is not modified; rather, a new URI is created
        /// and returned.</param>
        /// <param name="queryStringName">Name of the query string to include in the URI.</param>
        /// <param name="queryStringValue">The query string value to include in the URI.</param>
        /// <returns>Returns the uri with the specified query string name and value updated or added.</returns>
        private static Uri UpdateUriQueryString(Uri uri, string queryStringName, string queryStringValue)
        {
            Uri updatedUri = null;
            string newQueryString = uri.Query;

            if (Utils.IsQueryStringParameterPresent(uri, queryStringName))
            {
                if (Utils.GetQueryStringParameterString(uri, queryStringName) != queryStringValue)
                {
                    // The URI has the query string parm and it is different than the value. Update the URI.
                    newQueryString = Utils.RemoveQueryStringParameter(newQueryString, queryStringName);
                    newQueryString = Utils.AddQueryStringParameter(newQueryString, String.Format(CultureInfo.CurrentCulture, "{0}={1}", queryStringName, queryStringValue));

                    UriBuilder uriBuilder = new UriBuilder(uri);
                    uriBuilder.Query = newQueryString.TrimStart(new char[] { '?' });
                    updatedUri = uriBuilder.Uri;
                }
                //else {} // Query string is present and already has the requested value. Do nothing.
            }
            else
            {
                // Query string parm not present. Add it.
                newQueryString = Utils.AddQueryStringParameter(newQueryString, String.Format(CultureInfo.CurrentCulture, "{0}={1}", queryStringName, queryStringValue));

                UriBuilder uriBuilder = new UriBuilder(uri);
                uriBuilder.Query = newQueryString.TrimStart(new char[] { '?' });
                updatedUri = uriBuilder.Uri;
            }
            return updatedUri ?? uri;
        }

        private List<ActionResult> GetUploadErrors(IEnumerable<ActionResult> uploadResults)
        {
            if (uploadResults == null)
                return null;

            return (uploadResults.Where(m => m.Status == ActionResultStatus.Error.ToString())).ToList();
        }

        private static string ConvertListToHtmlBullets(IEnumerable<ActionResult> skippedFiles)
        {
            string html = "<ul class='gsp_addleftmargin5'>";
            foreach (ActionResult kvp in skippedFiles)
            {
                html += String.Format(CultureInfo.CurrentCulture, "<li>{0}: {1}</li>", kvp.Title, kvp.Message);
            }
            html += "</ul>";

            return html;
        }

        /// <summary>
        /// Verifies the media object exists and the user has permission to view it. If valid, the media object is assigned to the
        /// _mediaObject member variable and the function returns <c>true</c>; otherwise returns <c>false</c>.
        /// </summary>
        /// <param name="mediaObjectId">The media object ID to validate. Throws a <see cref="ArgumentOutOfRangeException"/>
        /// if the value is <see cref="Int32.MinValue"/>.</param>
        /// <param name="mediaObject">The media object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="mediaObjectId"/> is <see cref="Int32.MinValue"/>.</exception>
        private static bool ValidateMediaObject(int mediaObjectId, out IGalleryObject mediaObject)
        {
            if (mediaObjectId == int.MinValue)
                throw new ArgumentOutOfRangeException("mediaObjectId", String.Format(CultureInfo.CurrentCulture, "A valid media object ID must be passed to this function. Instead, the value was {0}.", mediaObjectId));

            mediaObject = null;
            bool isValid = false;
            IGalleryObject tempMediaObject = null;
            try
            {
                tempMediaObject = Factory.LoadMediaObjectInstance(mediaObjectId);
            }
            catch (ArgumentException) { }
            catch (InvalidMediaObjectException) { }

            if (tempMediaObject != null)
            {
                // Perform a basic security check to make sure user can view media object. Another, more detailed security check is performed by child
                // user controls if necessary. (e.g. Perhaps the user is requesting the high-res version but he does not have the ViewOriginalImage 
                // permission. The view media object user control will verify this.)
                if (Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject | SecurityActions.ViewOriginalMediaObject, RoleController.GetGalleryServerRolesForUser(), tempMediaObject.Parent.Id, tempMediaObject.GalleryId, tempMediaObject.IsPrivate, SecurityActionsOption.RequireOne, ((IAlbum)tempMediaObject.Parent).IsVirtualAlbum))
                {
                    // User is authorized. Assign to page-level variable.
                    mediaObject = tempMediaObject;

                    isValid = true;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Detect if an authentication cookie exists from a previous installation. If found, log off the user and reload the page.
        /// This is necessary because an error will occur when the app discovers that no user account exists for the associated 
        /// cookie.
        /// </summary>
        private static void DetectPreviousInstallation()
        {
            if (Utils.InstallRequested && !String.IsNullOrWhiteSpace(Utils.UserName))
            {
                UserController.LogOffUser();
                Utils.Redirect(PageId.album, false);
            }
        }

        #endregion

        #region Private Methods

        private void InitializePage()
        {
            InitializeGallerySettings();

            lock (_lockObject)
            {
                if (AppSetting.Instance.InstallationRequested)
                {
                    GalleryController.ProcessInstallRequest(GalleryId);

                    AppSetting.Instance.InstallationRequested = false;
                }
            }

            // Redirect to the logon page if the user has to log in. (Note that the InitializeGallerySettings() function
            // may also check RequiresLogin() and do a redirect when a GallerySecurityException is thrown.)
            if (RequiresLogin())
            {
                Utils.Redirect(PageId.login, true, "ReturnUrl={0}", Utils.UrlEncode(Utils.GetCurrentPageUrl(true)));
            }

            this.StoreCurrentPageUri();

            if (Utils.IsAuthenticated && GallerySettings.EnableUserAlbum)
            {
                UserController.ValidateUserAlbum(Utils.UserName, GalleryId);
            }

            if (IsPostBack)
            {
                // Postback (such as logon/logoff events): Since the user may have been navigating several media objects in this 
                // album through AJAX calls, we need to check a hidden field to discover the current media object. Assign this 
                // object's ID to our base user control. The base control is smart enough to retrieve the new media object if it 
                // is different than what was previously set.
                object formFieldMoid = Request.Form[HiddenFieldMediaObjectId];
                int moid;
                if ((formFieldMoid != null) && (Int32.TryParse(formFieldMoid.ToString(), out moid)))
                {
                    this.SetMediaObjectId(moid);
                }
            }

            if (!IsPostBack)
            {
                RegisterHiddenFields();
            }

            AddJavaScriptAndCss();

            // Add user controls to the page, such as the header and album breadcrumb menu.
            this.AddUserControls();

            RunAutoSynchIfNeeded();
        }

        /// <summary>
        /// Assign reference to gallery settings for the current gallery. If the user does not have permission to the requested
        /// album or media object, the user is automatically redirected as needed (e.g. the login page or the highest level album
        /// the user has permission to view). One exception to this is if a particular album is assigned to the control and the
        /// user does not have permission to view it, an empty album is used and a relevant message is assigned to the 
        /// <see cref="ClientMessage" /> property.
        /// </summary>
        private void InitializeGallerySettings()
        {
            try
            {
                LoadGallerySettings();
            }
            catch (InvalidMediaObjectException) { }
            catch (InvalidAlbumException ex)
            {
                CheckForInvalidAlbumIdInGalleryControlSetting(ex.AlbumId);

                Utils.Redirect(Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(), "msg=" + (int)MessageType.AlbumDoesNotExist));
            }
            catch (GallerySecurityException)
            {
                // Redirect to the logon page if the user has to log in.
                if ((this.PageId == PageId.login) || (this.PageId == PageId.createaccount) || (this.PageId == PageId.recoverpassword))
                {
                    // User is on one of the authentication pages, so just create an empty album. We'll get here when anon.
                    // browsing is disabled and a specific album is specified on the GCS page.
                    this._album = CreateEmptyAlbum(AlbumController.LoadAlbumInstance(this.GalleryControl.AlbumId).GalleryId);
                }
                else if (RequiresLogin())
                {
                    Utils.Redirect(PageId.login, true, "ReturnUrl={0}", Utils.UrlEncode(Utils.GetCurrentPageUrl(true)));
                }
                else
                {
                    if (this.GalleryControl.AlbumId > int.MinValue)
                    {
                        // User does not have access to the album specified as the default gallery object.
                        this._album = CreateEmptyAlbum(AlbumController.LoadAlbumInstance(this.GalleryControl.AlbumId).GalleryId);

                        ClientMessage = GetMessageOptions(MessageType.AlbumNotAuthorizedForUser);
                    }
                    else
                    {
                        Utils.Redirect(PageId.album);
                    }
                }
            }
        }

        /// <summary>
        /// Assign reference to gallery settings for the current gallery.
        /// </summary>
        /// <exception cref="InvalidAlbumException">Thrown when an album is requested but does not exist.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user is requesting an album or media object they don't have 
        /// permission to view.</exception>
        /// <remarks>This must be called from <see cref="GalleryPage_Init" />! It can't go in the <see cref="GalleryPage" /> constructor 
        /// because that is too early to access the GalleryId property, and it can't go in the GallerySettings property getter because 
        /// that is too late if a gallery has to be dynamically created.)</remarks>
        private void LoadGallerySettings()
        {
            try
            {
                this._gallerySetting = Factory.LoadGallerySetting(GalleryId);
            }
            catch (GallerySecurityException)
            {
                // The user is requesting an album or media object they don't have permission to view. Manually load the gallery settings
                // from the query string parameter and assign the gallery ID property so that they are available in the RequiresLogin() 
                // function later in GalleryPage_Init(). That code will take care of redirecting the user to the login page.
                int albumId = Utils.GetQueryStringParameterInt32("aid");
                int mediaObjectId = Utils.GetQueryStringParameterInt32("moid");

                if ((albumId == int.MinValue))
                {
                    albumId = this.GalleryControl.AlbumId;
                }

                if (mediaObjectId == int.MinValue)
                {
                    mediaObjectId = this.GalleryControl.MediaObjectId;
                }

                if (albumId > int.MinValue)
                {
                    try
                    {
                        _galleryId = AlbumController.LoadAlbumInstance(albumId).GalleryId;
                        this._gallerySetting = Factory.LoadGallerySetting(_galleryId);
                    }
                    catch (InvalidAlbumException) { }
                }
                else if (mediaObjectId > int.MinValue)
                {
                    try
                    {
                        _galleryId = Factory.LoadMediaObjectInstance(mediaObjectId).Parent.GalleryId;
                        this._gallerySetting = Factory.LoadGallerySetting(_galleryId);
                    }
                    catch (InvalidMediaObjectException) { }
                    catch (InvalidAlbumException) { }
                }

                throw; // Re-throw GallerySecurityException
            }
        }

        /// <summary>
        /// Determines whether the current user must be logged in to access the requested page.
        /// </summary>
        /// <returns>Returns <c>true</c> if the user must be logged in to access the requested page; otherwise
        /// returns <c>false</c>.</returns>
        private bool RequiresLogin()
        {
            if ((this.PageId == PageId.login) || (this.PageId == PageId.createaccount) || (this.PageId == PageId.recoverpassword))
                return false; // The login, create account, & recover password pages never require one to be logged in

            if (!this.IsAnonymousUser)
                return false; // Already logged in

            if (!AllowAnonymousBrowsing)
                return true; // Not logged in, anonymous browsing disabled

            // Some pages allow anonymous browsing. If it is one of those, return false; otherwise return true;
            switch (this.PageId)
            {
                //case PageId.createaccount:
                //case PageId.login:
                //case PageId.recoverpassword: // These 3 are redundent because we already handle them above
                case PageId.album:
                //case PageId.albumtreeview:
                case PageId.mediaobject:
                    //case PageId.search: // Removed in 3.0
                    //case PageId.task_downloadobjects:
                    return false;
                default:
                    return true;
            }
        }

        //private void AddAlbumMenu()
        //{
        //  Controls.albummenu albumMenu = (Controls.albummenu)LoadControl(Utils.GetUrl("/controls/albummenu.ascx"));
        //  this._albumMenu = albumMenu;
        //  this.Controls.AddAt(0, albumMenu);
        //}

        private void AddGalleryHeader()
        {
            Controls.galleryheader header = (Controls.galleryheader)LoadControl(Utils.GetUrl("/controls/galleryheader.ascx"));
            this._galleryHeader = header;
            this.Controls.AddAt(0, header);
        }

        /// <summary>
        /// Stores or updates the URI of the current album or media object page so that we can return to it later, if desired. This
        /// method store the current URI only for fresh page loads (no postbacks or callbacks) and when the current page
        /// is displaying an album view or media object. It also updates the URI with the current media object ID when the 
        /// current page is a task page. No action is taken for other pages, such as admin pages, since we do not want to return to 
        /// them. This method assigns or updates the URI in the <see cref="PreviousUri"/> property. After assigning this property, 
        /// one can use <see cref="RedirectToPreviousPage()"/> to navigate to the page. If session state is disabled, this method 
        /// does nothing.
        /// </summary>		
        private void StoreCurrentPageUri()
        {
            if (!IsPostBack)
            {
                if ((this.PageId == PageId.album) || (this.PageId == PageId.mediaobject))
                    this.PreviousUri = Utils.GetCurrentPageUri();
                else if (this.PageId.ToString().StartsWith("task", StringComparison.OrdinalIgnoreCase))
                {
                    // If we are on a task page and the QS contains a media object ID different than the one stored in 
                    // <see cref="PreviousUri" />, the update the previous URI to contain the new ID. This code assumes that
                    // the MO we want to go back to upon completion of the task is the same one in the QS.
                    int prevMoid = Utils.GetQueryStringParameterInt32(this.PreviousUri, "moid");
                    int currentMoid = Utils.GetQueryStringParameterInt32("moid");
                    if ((prevMoid > int.MinValue) && (currentMoid > int.MinValue) && (prevMoid != currentMoid))
                    {
                        this.PreviousUri = Utils.AddQueryStringParameter(this.PreviousUri, String.Concat("moid=", currentMoid));
                    }
                }
            }
        }

        /// <summary>
        /// Set the public properties on this class related to user permissions. This method is called as needed from
        /// within the property getters.
        /// </summary>
        private void EvaluateUserPermissions()
        {
            var perms = AlbumController.GetPermissionsEntity(this.GetAlbum());

            this._userCanViewAlbumOrMediaObject = perms.ViewAlbumOrMediaObject;
            this._userCanViewOriginal = perms.ViewOriginalMediaObject;
            this._userCanCreateAlbum = perms.AddChildAlbum;
            this._userCanEditAlbum = perms.EditAlbum;
            this._userCanAddMediaObject = perms.AddMediaObject;
            this._userCanEditMediaObject = perms.EditMediaObject;
            this._userCanDeleteCurrentAlbum = perms.DeleteAlbum;
            this._userCanDeleteChildAlbum = perms.DeleteChildAlbum;
            this._userCanDeleteMediaObject = perms.DeleteMediaObject;
            this._userCanSynchronize = perms.Synchronize;
            this._userDoesNotGetWatermark = perms.HideWatermark;

            this._userCanAddAdministerSite = this.IsUserAuthorized(SecurityActions.AdministerSite);
            this._userCanAdministerGallery = this.IsUserAuthorized(SecurityActions.AdministerGallery);

            if (this._userCanAddAdministerSite.GetValueOrDefault() || this._userCanAdministerGallery.GetValueOrDefault())
            {
                this._userCanAddMediaObjectToAtLeastOneAlbum = true;
                this._userCanAddAlbumToAtLeastOneAlbum = true;
                this._userCanEditAtLeastOneAlbum = true;
                this._userCanEditAtLeastOneMediaAsset = true;
            }
            else
            {
                var userPerms = SecurityManager.GetUserObjectPermissions(GetGalleryServerRolesForUser(), GalleryId);

                this._userCanAddAlbumToAtLeastOneAlbum = userPerms.UserCanAddAlbumToAtLeastOneAlbum;
                this._userCanAddMediaObjectToAtLeastOneAlbum = userPerms.UserCanAddMediaAssetToAtLeastOneAlbum;
                this._userCanEditAtLeastOneAlbum = userPerms.UserCanEditAtLeastOneAlbum;
                this._userCanEditAtLeastOneMediaAsset = userPerms.UserCanEditAtLeastOneMediaAsset;
            }
        }

        private void RegisterHiddenFields()
        {
            if (GetMediaObjectId() > int.MinValue)
                this.Page.ClientScript.RegisterHiddenField(HiddenFieldMediaObjectId, GetMediaObjectId().ToString(CultureInfo.InvariantCulture));

            //if (GetAlbumId() > int.MinValue)
            //  ScriptManager.RegisterHiddenField(this, "aid", GetAlbumId().ToString(CultureInfo.InvariantCulture));
        }

        private void AddUserControls()
        {
            // If any inheritors subscribed to the event, fire it.
            if (BeforeHeaderControlsAdded != null)
            {
                BeforeHeaderControlsAdded(this, new EventArgs());
            }

            if (this.ShowHeader)
            {
                this.AddGalleryHeader();
            }
        }

        /// <summary>
        /// Add a title to the page's title tag if it has not yet been assigned by any other process.
        /// </summary>
        private void AddPageTitleIfMissing()
        {
            HtmlHead head = this.Page.Header;
            if (head == null)
                throw new WebException(Resources.GalleryServer.Error_Head_Tag_Missing_Server_Attribute_Ex_Msg);

            if (String.IsNullOrEmpty(head.Title))
                head.Title = PageTitle;
        }

        private void AddJavaScriptAndCss()
        {
            AddStartupScript();

            if (!JavaScriptAndCssLinksAddedToHead())
            {
                HtmlHead head = this.Page.Header;
                if (head == null)
                    throw new WebException(Resources.GalleryServer.Error_Head_Tag_Missing_Server_Attribute_Ex_Msg);

                AddCss(head);

                AddScriptFiles(head);

                AddRssLink(head);

                AddGlobalStartupScript();

                HttpContext.Current.Items["GSP_HtmlHeadConfigured"] = bool.TrueString;
            }
        }

        private void AddCss(HtmlHead head)
        {
            if (head == null)
                throw new ArgumentNullException();

            foreach (string cssPath in GetCssPaths())
            {
                head.Controls.Add(MakeStyleSheetControl(cssPath));
            }

            if (!String.IsNullOrWhiteSpace(AppSetting.Instance.CustomCss))
            {
                head.Controls.Add(new LiteralControl(String.Concat("\n<style type=\"text/css\">\n", AppSetting.Instance.CustomCss, "\n</style>\n")));
            }
        }

        /// <summary>
        /// Gets the paths, relative to the web site root, of the CSS files needed by GSP. Example: "/dev/gsweb/gs/styles/gallery.css"
        /// </summary>
        /// <returns>Returns an array of strings containing the CSS paths.</returns>
        private static IEnumerable<string> GetCssPaths()
        {
            if (Utils.IsDebugEnabled) // debug="true" in web.config
            {
                return new string[] { Utils.GetSkinnedUrl("/styles/jquery-ui.css"), Utils.GetSkinnedUrl("/styles/gallery.css") };
            }
            else // debug="false" in web.config
            {
                // The Bunderl & Minifier VS extension is having trouble generating both bundle.css and bundle.min.css, so for now continue
                // referencing the original CSS files. When the extension is fixed, we can use the code on the next line.
                return new string[] { Utils.GetSkinnedUrl($"/styles/jquery-ui.css?v={Utils.GetGalleryServerVersion()}"), Utils.GetSkinnedUrl($"/styles/gallery.css?v={Utils.GetGalleryServerVersion()}") };
                //return new string[] { Utils.GetSkinnedUrl("/styles/bundle.min.css") };
            }
        }

        private static HtmlLink MakeStyleSheetControl(string href)
        {
            HtmlLink stylesheet = new HtmlLink();
            stylesheet.Href = href;
            stylesheet.Attributes.Add("rel", "stylesheet");
            stylesheet.Attributes.Add("type", "text/css");

            return stylesheet;
        }

        private void AddScriptFiles(HtmlHead head)
        {
            if (head == null)
                throw new ArgumentNullException();

            // Build up the script references, starting with the IE HTML5 shim.
            var isDebug = Utils.IsDebugEnabled;

            string script = string.Empty;

            // Add jQuery reference. Fall back to a local copy of jquery.js if the one specified
            // in the setting does not load for any reason (e.g. CDN is unavailable).
            if (!String.IsNullOrEmpty(AppSetting.Instance.JQueryScriptPath))
            {
                script += String.Format(CultureInfo.InvariantCulture, @"
  <script src='{0}'></script>
  <script>window.jQuery || document.write('<script src=""{1}"">\x3C/script>')
  </script>",
            GetJQueryPath(),
            isDebug ? ResolveClientUrl("~/Scripts/jquery-3.1.1.js") : ResolveClientUrl("~/Scripts/jquery-3.1.1.min.js")
         );
            }

            // Add jQuery Migrate Plugin reference. Fall back to a local copy if the one specified
            // in the setting does not load for any reason (e.g. CDN is unavailable).
            if (!String.IsNullOrEmpty(AppSetting.Instance.JQueryMigrateScriptPath))
            {
                script += String.Format(CultureInfo.InvariantCulture, @"
  <script src='{0}'></script>
  <script>jQuery.migrateWarnings || document.write('<script src=""{1}"">\x3C/script>')
  </script>",
            GetJQueryMigratePath(),
            isDebug ? Utils.GetUrl("/script/jquery-migrate.js") : Utils.GetUrl("/script/jquery-migrate.min.js")
         );
            }

            // Add jQuery UI reference. Fall back to a local copy of jquery-ui.min.js if the one 
            // specified in the setting does not load for any reason (e.g. CDN is unavailable).
            if (!String.IsNullOrEmpty(AppSetting.Instance.JQueryUiScriptPath))
            {
                script += string.Format(CultureInfo.InvariantCulture, @"
  <script src='{0}'></script>
  <script>window.jQuery.ui || document.write('<script src=""{1}"">\x3C/script>')
  </script>",
         GetJQueryUiPath(),
         isDebug ? Utils.GetUrl("/script/jquery-ui.js") : Utils.GetUrl("/script/jquery-ui.min.js"));
            }

            // Add reference to custom javascript and widgets
            foreach (var scriptPath in GetCustomScriptPaths())
            {
                script += string.Format(CultureInfo.InvariantCulture, @"
  <script src='{0}'></script>
", scriptPath);
            }

            head.Controls.Add(new LiteralControl(script));
        }

        /// <summary>
        /// Gets the paths, relative to the web site root, of the custom and widget script files needed by GSP. Example: "/dev/gsweb/gs/script/bundle.min.js"
        /// </summary>
        /// <returns>Returns an array of strings containing the CSS paths.</returns>
        private IEnumerable<string> GetCustomScriptPaths()
        {
            var scriptPaths = new List<string>();

            if (IsUserAuthorized(SecurityActions.EditMediaObject) || IsUserAuthorized(SecurityActions.EditAlbum) || IsUserAuthorized(SecurityActions.AddChildAlbum))
            {
                // Include tinyMCE when user has edit permission
                scriptPaths.Add(Utils.IsDebugEnabled ? ResolveClientUrl("~/Scripts/tinymce/tinymce.js") : ResolveClientUrl($"~/Scripts/tinymce/tinymce.min.js?v={Utils.GetGalleryServerVersion()}"));
            }

            if (Utils.IsDebugEnabled) // debug="true" in web.config
            {
                // This should contain the same files that are used to build bundle.js (see bundleconfig.json)
                scriptPaths.AddRange(new string[]
                {
          Utils.GetUrl("/script/jsviews.js"),
          Utils.GetUrl("/script/jquery.jstree.js"),
          Utils.GetUrl("/script/globalize.js"),
          Utils.GetUrl("/script/js-cookie.js"),
          Utils.GetUrl("/script/jquery.paging.js"),
          Utils.GetUrl("/script/jquery.splitter.js"),
          Utils.GetUrl("/script/jquery.autosuggest.js"),
          Utils.GetUrl("/script/jquery.ui.menubar.js"),
          Utils.GetUrl("/script/jquery.rateit.js"),
          Utils.GetUrl("/script/jquery.supersized.js"),
          Utils.GetUrl("/script/jquery.ui.multiselect.js"),
          Utils.GetUrl("/script/jquery.jqcloud.js"),
          Utils.GetUrl("/script/jquery.ui.touchpunch.js"),
          Utils.GetUrl("/script/jquery.touchwipe.js"),
          Utils.GetUrl("/script/jquery.textareafullscreen.js"),
          Utils.GetUrl("/script/gallery.js")
                });
            }
            else // debug="false" in web.config
            {
                scriptPaths.Add(Utils.GetUrl($"/script/bundle.min.js?v={Utils.GetGalleryServerVersion()}"));
            }

            return scriptPaths;
        }

        /// <summary>
        /// Add a link to the RSS feed for the current album to the <paramref name="head" />. This function
        /// has no effect unless gallery is running an Enterprise license.
        /// </summary>
        /// <param name="head">The head.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="head" /> is null.</exception>
        private void AddRssLink(HtmlHead head)
        {
            if (head == null)
                throw new ArgumentNullException();

            var rssUrl = AlbumController.GetRssUrl(GetAlbum());

            if (rssUrl != null)
            {
                var links = String.Format(CultureInfo.InvariantCulture, @"
  <link rel='alternate' type='application/rss+xml' title='{0}' href='{1}' />
  ",
           GetAlbum().Title.Replace(@"'", @""""),
           rssUrl.Replace(@"'", @""""));

                head.Controls.Add(new LiteralControl(links));
            }
        }

        /// <summary>
        /// Renders javascript that should run when the page loads in the browser.
        /// </summary>
        private void AddStartupScript()
        {
            // Set up a global javascript variable that is scoped to this gallery control instance.
            // Other controls can use this object to store variables and perform other functions that
            // must be isolated from any other gallery instances on the page.
            string script = String.Format(CultureInfo.InvariantCulture, @"
        Gs.Vars['{0}'] = {{ }};
        Gs.Vars['{0}'].gsData = JSON.parse('{1}');
        Gs.Vars['{0}'].gsData.ActiveMetaItems = (Gs.Vars['{0}'].gsData.MediaItem ? Gs.Vars['{0}'].gsData.MediaItem.MetaItems : Gs.Vars['{0}'].gsData.Album.MetaItems) || [];
        Gs.Vars['{0}'].gsData.ActiveGalleryItems = (Gs.Vars['{0}'].gsData.MediaItem ? [Gs.Utils.convertMediaItemToGalleryItem(Gs.Vars['{0}'].gsData.MediaItem)] : [Gs.Utils.convertAlbumToGalleryItem(Gs.Vars['{0}'].gsData.Album)]) || [];
        {2}
      ",
                GspClientId, // 0
                GetClientGsDataAsJson(), // 1
                GetAlbumTreeDataClientScript() // 2
                );

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.cid, "_initScript"), script, true);
        }

        /// <summary>
        /// Assign a few global javascript variables that can be used throughout the app. This should
        /// only be added once to a page, even if there are multiple instances of the <see cref="Gallery" />
        /// control.
        /// </summary>
        private void AddGlobalStartupScript()
        {
            string script = String.Format(CultureInfo.InvariantCulture, @"
  Gs.Vars.AppRoot = '{0}';
  Gs.Vars.AppUrl = '{1}';
  Gs.Vars.GalleryResourcesRoot = '{2}';
  Gs.Vars.IsPostBack = {3};
",
          Utils.AppRoot, // 0
          Utils.GetAppUrl(), // 1
          Utils.GalleryResourcesPath, // 2
          IsPostBack.ToString().ToLowerInvariant() // 3
          );

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.cid, "_initGlblScript"), script, true);
        }

        private string GetClientGsDataAsJson()
        {
            return GetClientGsData().ToJson().JsEncode();
        }

        /// <summary>
        /// Gets JavaScript that assigns a client variable containing data that can be consumed by the jQuery album tree plug-in.
        /// To increase performance, returns an empty string when the left pane is not visible.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetAlbumTreeDataClientScript()
        {
            if (ShowLeftPaneForAlbum || ShowLeftPaneForMediaObject)
            {
                return String.Format(CultureInfo.InvariantCulture, @"Gs.Vars['{0}'].gsAlbumTreeData = JSON.parse('{1}');", GspClientId, GetAlbumTreeDataAsJson());
            }

            return String.Empty;
        }

        private string GetAlbumTreeDataAsJson()
        {
            var tvOptions = new Entity.TreeViewOptions()
            {
                SelectedAlbumIds = (GetAlbumId() > int.MinValue ? new IntegerCollection(new int[] { GetAlbumId() }) : new IntegerCollection()),
                NumberOfLevels = 2,
                NavigateUrl = GalleryControl.TreeViewNavigateUrl ?? (GalleryControl.AllowUrlOverride ? Utils.GetCurrentPageUrl() : null),
                EnableCheckboxPlugin = false,
                RequiredSecurityPermissions = SecurityActions.ViewAlbumOrMediaObject,
                RootNodesPrefix = String.Empty,
                Galleries = new GalleryCollection() { Factory.LoadGallery(GalleryId) }
            };

            Entity.TreeView tv = AlbumTreeViewBuilder.GetAlbumsAsTreeView(tvOptions);

            return tv.ToJson().JsEncode();
        }

        /// <summary>
        /// Gets a value indicating whether the javascript and CSS files have already been added to the page
        /// output. This is useful in preventing multiple registrations when more than one
        /// <see cref="Gallery" /> control is on the page.
        /// </summary>
        /// <returns><c>true</c> if javascript and CSS files have already been added to the page; otherwise <c>false</c>.</returns>
        private static bool JavaScriptAndCssLinksAddedToHead()
        {
            object scriptFilesAddedObject = HttpContext.Current.Items["GSP_HtmlHeadConfigured"];
            bool scriptFilesAdded = false;
            bool foundScriptFilesAddedVar = ((scriptFilesAddedObject != null) && Boolean.TryParse(scriptFilesAddedObject.ToString(), out scriptFilesAdded));
            return (foundScriptFilesAddedVar && scriptFilesAdded);
        }

        private string GetJQueryPath()
        {
            IAppSetting appSetting = AppSetting.Instance;
            if (Utils.IsAbsoluteUrl(appSetting.JQueryScriptPath))
            {
                return appSetting.JQueryScriptPath;
            }
            else
            {
                return this.Page.ResolveUrl(appSetting.JQueryScriptPath);
            }
        }

        private string GetJQueryMigratePath()
        {
            IAppSetting appSetting = AppSetting.Instance;
            if (Utils.IsAbsoluteUrl(appSetting.JQueryMigrateScriptPath))
            {
                return appSetting.JQueryMigrateScriptPath;
            }
            else
            {
                return this.Page.ResolveUrl(appSetting.JQueryMigrateScriptPath);
            }
        }

        private string GetJQueryUiPath()
        {
            IAppSetting appSetting = AppSetting.Instance;
            if (Utils.IsAbsoluteUrl(appSetting.JQueryUiScriptPath))
            {
                return appSetting.JQueryUiScriptPath;
            }
            else
            {
                return this.Page.ResolveUrl(appSetting.JQueryUiScriptPath);
            }
        }

        /// <summary>
        /// If auto-sync is enabled and another synchronization is needed, start a synchronization of the root album in this gallery
        /// on a new thread.
        /// </summary>
        private void RunAutoSynchIfNeeded()
        {
            if (NeedToRunAutoSync())
            {
                // Start sync on new thread
                var syncOptions = new Entity.SyncOptions
                {
                    SyncId = Guid.NewGuid().ToString(),
                    SyncInitiator = Entity.SyncInitiator.AutoSync,
                    AlbumIdToSynchronize = Factory.LoadRootAlbumInstance(GalleryId).Id,
                    IsRecursive = true,
                    RebuildThumbnails = false,
                    RebuildOptimized = false,
                    UserName = GlobalConstants.SystemUserName
                };

                System.Threading.Tasks.Task.Factory.StartNew(() => GalleryController.BeginSync(syncOptions), TaskCreationOptions.LongRunning);
            }
        }

        /// <summary>
        /// Gets a value indicating whether an auto-sync must be performed. It is needed when auto-sync is enabled and the specified
        /// interval has passed since the last sync.
        /// </summary>
        /// <returns><c>true</c> if a sync must be run; otherwise <c>false</c>.</returns>
        private bool NeedToRunAutoSync()
        {
            IGallerySettings gallerySettings = Factory.LoadGallerySetting(GalleryId);

            if (gallerySettings.EnableAutoSync)
            {
                // Auto sync is enabled.
                double numMinutesSinceLastSync = DateTime.Now.Subtract(gallerySettings.LastAutoSync).TotalMinutes;

                if (numMinutesSinceLastSync > gallerySettings.AutoSyncIntervalMinutes)
                {
                    // It is time to do another sync.
                    ISynchronizationStatus synchStatus = SynchronizationStatus.GetInstance(GalleryId);

                    if ((synchStatus.Status != SynchronizationState.SynchronizingFiles) && (synchStatus.Status != SynchronizationState.PersistingToDataStore))
                    {
                        // No other sync is in progress - we need to do one!
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluate the query string and properties of the Gallery control to discover which, if any, media object to display.
        /// Returns <see cref="Int32.MinValue" /> if no ID is discovered. This function does not evaluate the ID to see if it is
        /// valid or whether the current user has permission to view it.
        /// </summary>
        /// <returns>Returns the ID for the media object to display, or <see cref="Int32.MinValue" /> if no ID is discovered.</returns>
        private int ParseMediaObjectId()
        {
            // Determine the ID for the media object to display, if any. Follow these rules:
            // 1. If an album has been requested and no media object specified, return Int32.MinValue.
            // 2. If AllowUrlOverride=true and a media object ID has been specified in the query string, use that.
            // 3. If AllowUrlOverride=true and an album ID has been specified in the query string, get one of it's media objects.
            // 4. If a media object ID has been specified on Gallery.MediaObjectId, use that.
            // 5. If ViewMode is Single or SingleRandom and an album ID has been specified on Gallery.AlbumId, get one of it's media objects.
            // 6. If ViewMode is Single or SingleRandom, get one of the media objects in the root album.
            // 7. If none of the above, return Int32.MinValue.

            int aidGc = this.GalleryControl.AlbumId;
            int moidGc = this.GalleryControl.MediaObjectId;
            int moidQs = Utils.GetQueryStringParameterInt32("moid");
            int aidQs = Utils.GetQueryStringParameterInt32("aid");
            bool isAlbumView = (this.GalleryControl.ViewMode == ViewMode.Multiple);
            bool allowUrlOverride = this.GalleryControl.AllowUrlOverride;

            if (isAlbumView && ((aidQs > int.MinValue) || (aidGc > int.MinValue)) && (moidQs == int.MinValue) && (moidGc == int.MinValue))
                return int.MinValue; // Matched rule 1

            if (allowUrlOverride)
            {
                if (moidQs > int.MinValue)
                    return moidQs; // Matched rule 2

                if (aidQs > int.MinValue)
                    return GetMediaObjectIdInAlbum(aidQs); // Matched rule 3
            }

            if (moidGc > int.MinValue)
                return moidGc; // Matched rule 4

            if (!isAlbumView && (aidGc > int.MinValue))
                return GetMediaObjectIdInAlbum(aidGc); // Matched rule 5

            if (!isAlbumView)
                return GetMediaObjectInRootAlbum(); // Matched rule 6

            return int.MinValue; // Matched rule 7
        }

        /// <summary>
        /// Get the ID for one of the media objects in the root album of the current gallery. The ID selected depends on the
        /// <see cref="Gallery.ViewMode" /> and whether <see cref="AutoPlaySlideShow" /> has been enabled. Returns <see cref="Int32.MinValue" />
        /// if the album does not contain a suitable media object.
        /// </summary>
        /// <returns>Returns the ID for one of the media objects in the root album, or <see cref="Int32.MinValue" /> if no suitable ID is found.</returns>
        private int GetMediaObjectInRootAlbum()
        {
            if (this.GalleryControl.GalleryId > int.MinValue)
            {
                return GetMediaObjectIdInAlbum(Factory.LoadRootAlbumInstance(this.GalleryControl.GalleryId).Id);
            }

            // No gallery ID has been assigned, so just use the first one we find. I am not sure this code will ever be hit, since it is possible
            // the gallery ID will always be assigned by this point.
            IGalleryCollection galleries = Factory.LoadGalleries();
            if (galleries.Count > 0)
            {
                return GetMediaObjectIdInAlbum(Factory.LoadRootAlbumInstance(galleries.First().GalleryId).Id);
            }

            return int.MinValue;
        }

        /// <summary>
        /// Get the ID for one of the media objects in the specified <paramref name="albumId" />. The ID selected depends on the
        /// <see cref="Gallery.ViewMode" /> and whether <see cref="AutoPlaySlideShow" /> has been enabled. Returns <see cref="Int32.MinValue" />
        /// if the album does not contain a suitable media object.
        /// </summary>
        /// <param name="albumId">The album ID.</param>
        /// <returns>Returns the ID for one of the media objects in the album, or <see cref="Int32.MinValue" /> if no suitable ID is found.</returns>
        private int GetMediaObjectIdInAlbum(int albumId)
        {
            int moid = int.MinValue;

            if (this.GalleryControl.ViewMode == ViewMode.Single)
            {
                // Choose the first media object in the album, unless <see cref="AutoPlaySlideShow" /> is enabled, in which case we want
                // to choose the first *image* in the album.
                IAlbum album = null;
                IList<IGalleryObject> galleryObjects = null;

                try
                {
                    album = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(albumId) { InflateChildObjects = true });
                }
                catch (InvalidAlbumException) { }

                if (album != null)
                {
                    if (this.AutoPlaySlideShow)
                    {
                        galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.Image).ToSortedList(); // Get all images in album
                    }
                    else
                    {
                        galleryObjects = album.GetChildGalleryObjects(GalleryObjectType.MediaObject).ToSortedList(); // Get all media objects in album
                    }
                }

                if ((galleryObjects != null) && (galleryObjects.Any()))
                {
                    moid = galleryObjects.First().Id;
                }
            }
            else if (this.GalleryControl.ViewMode == ViewMode.SingleRandom)
            {
                //TODO: Implement ViewMode.SingleRandom functionality
                throw new NotImplementedException("The functionality to support ViewMode.SingleRandom has not been implemented.");
            }

            return moid;
        }

        /// <summary>
        /// Gets the highest-level album the current user can view. Guaranteed to not return null. If a user does not have permission to 
        /// view any objects, this function returns a virtual album with no objects and automatically assigns the <see cref="ClientMessage" /> 
        /// property to <see cref="MessageType.NoAuthorizedAlbumForUser" />, which will cause a message to be displayed to the user.
        /// </summary>
        /// <returns>Returns an IAlbum representing the highest-level album the current user can view.</returns>
        private IAlbum GetHighestAlbumUserCanView()
        {
            var galleryObjectSearcher = new GalleryObjectSearcher(new GalleryObjectSearchOptions()
            {
                GalleryId = GalleryId,
                SearchType = GalleryObjectSearchType.HighestAlbumUserCanView,
                Roles = RoleController.GetGalleryServerRolesForUser(),
                IsUserAuthenticated = Utils.IsAuthenticated,
                Filter = GalleryObjectType.Album
            });

            var album = galleryObjectSearcher.FindOne();

            var tempAlbum = album as IAlbum;

            if (album != null && tempAlbum == null)
            {
                throw new WebException(String.Format(CultureInfo.InvariantCulture, "A gallery object search for {0} returned an object that couldn't be cast to IAlbum. It was a {1}.", GalleryObjectSearchType.HighestAlbumUserCanView, album.GetType()));
            }

            if (album == null)
            {
                // Create virtual album so that page has something to bind to.
                tempAlbum = Factory.CreateEmptyAlbumInstance(GalleryId);
                tempAlbum.IsVirtualAlbum = true;
                tempAlbum.VirtualAlbumType = VirtualAlbumType.Root;
                tempAlbum.Title = Resources.GalleryServer.Site_Virtual_Album_Title;
                tempAlbum.Caption = String.Empty;
                tempAlbum.IsInflated = true;

                if (Array.IndexOf(new[] { PageId.login, PageId.recoverpassword, PageId.createaccount }, PageId) < 0)
                {
                    ClientMessage = GetMessageOptions(MessageType.NoAuthorizedAlbumForUser);
                }
            }

            return tempAlbum;
        }

        /// <summary>
        /// Gets the album ID corresponding to the current album, or <see cref="Int32.MinValue" /> if no valid album is available. The value 
        /// is determined in the following sequence: (1) If no media object is available, then look for the "aid" query string parameter. 
        /// (2) If not there, or if <see cref="Gallery.AllowUrlOverride" /> has been set to <c>false</c>, look for an album ID on the 
        /// containing <see cref="Gallery" /> control. This function does NOT perform any validation that the album exists and the current 
        /// user has permission to view it.
        /// </summary>
        /// <returns>Returns the album ID corresponding to the current album, or <see cref="Int32.MinValue" /> if no valid album is available.</returns>
        private int ParseAlbumId()
        {
            int aid;
            object viewstateAid = ViewState["aid"];

            if ((viewstateAid == null) || (!Int32.TryParse(ViewState["aid"].ToString(), out aid)))
            {
                // Not in viewstate. See if it is on the "aid" query string.
                if ((this.GalleryControl.AllowUrlOverride) && (Utils.GetQueryStringParameterInt32("aid") > int.MinValue))
                {
                    aid = Utils.GetQueryStringParameterInt32("aid");
                }
                else
                {
                    // Use the album ID property on this user control. May return int.MinValue.
                    aid = this.GalleryControl.AlbumId;
                }

                ViewState["aid"] = aid;
            }

            return aid;
        }

        /// <summary>
        /// Verifies the album exists and the user has permission to view it Throws a <see cref="InvalidAlbumException" /> when an 
        /// album associated with the <paramref name="albumId" /> does not exist. Throws a <see cref="GallerySecurityException" /> 
        /// when the user requests an album he or she does not have permission to view. An instance of the album is assigned to the 
        /// album output parameter, and is guaranteed to not be null.
        /// </summary>
        /// <param name="albumId">The album ID to validate. Throws a <see cref="ArgumentOutOfRangeException"/>
        /// if the value is <see cref="Int32.MinValue"/>.</param>
        /// <param name="album">The album associated with the ID = <paramref name="albumId" />.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="albumId"/> is <see cref="Int32.MinValue"/>.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when an album associated with the <paramref name="albumId" /> does not exist.</exception>
        /// <exception cref="GallerySecurityException">Thrown when the user is requesting an album they don't have permission to view.</exception>
        private void ValidateAlbum(int albumId, out IAlbum album)
        {
            if (albumId == int.MinValue)
                throw new ArgumentOutOfRangeException("albumId", String.Format(CultureInfo.CurrentCulture, "A valid album ID must be passed to this function. Instead, the value was {0}.", albumId));

            album = null;
            IAlbum tempAlbum = null;

            // TEST 1: If the current media object's album matches the ID we are validating, get a reference to that album.
            IGalleryObject mediaObject = GetMediaObject();
            if (mediaObject != null)
            {
                if (mediaObject.Parent.Id != albumId)
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "The requested media object (ID={0}) does not exist in the requested album (ID={1}).", mediaObject.Id, albumId));

                // Instead of loading it from disk, just grab the reference to the media object's parent.
                tempAlbum = (IAlbum)mediaObject.Parent;
            }
            else
            {
                // No media object is part of this HTTP request, so load it from disk.
                tempAlbum = AlbumController.LoadAlbumInstance(albumId);
            }

            // TEST 2: Does user have permission to view it?
            if (tempAlbum != null)
            {
                if (Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, RoleController.GetGalleryServerRolesForUser(), tempAlbum.Id, tempAlbum.GalleryId, tempAlbum.IsPrivate, tempAlbum.IsVirtualAlbum))
                {
                    // User is authorized. Assign to output parameter.
                    album = tempAlbum;
                }
                else
                {
                    throw new GallerySecurityException(); // User does not have permission to view the album.
                }
            }
        }

        private static IAlbum CreateEmptyAlbum(int galleryId)
        {
            IAlbum album = Factory.CreateEmptyAlbumInstance(galleryId);
            album.IsVirtualAlbum = true;
            album.VirtualAlbumType = VirtualAlbumType.Root;
            album.Title = Resources.GalleryServer.Site_Virtual_Album_Title;
            album.IsInflated = true;

            return album;
        }

        /// <summary>
        /// Check the albumId to see if it matches the <see cref="IGalleryControlSettings.AlbumId" /> value for the 
        /// <see cref="Gallery.GalleryControlSettings" /> property on the <see cref="Gallery" /> user control. If it does, that means the setting
        /// contains an ID for an album that no longer exists. Delete the setting.
        /// </summary>
        /// <param name="albumId">The album ID.</param>
        private void CheckForInvalidAlbumIdInGalleryControlSetting(int albumId)
        {
            if (this.GalleryControl.GalleryControlSettings.AlbumId == albumId)
            {
                IGalleryControlSettings galleryControlSettings = Factory.LoadGalleryControlSetting(this.GalleryControl.ControlId, true);
                galleryControlSettings.AlbumId = null;
                galleryControlSettings.Save();
            }
        }

        private void SetMediaObjectId(int mediaObjectId)
        {
            this._mediaObjectId = mediaObjectId;
            this._mediaObject = null;
            this._album = null;
            this._galleryId = int.MinValue;
        }

        private string GetTitleUrl()
        {
            string url = GalleryTitleUrl.Trim();

            if (!String.IsNullOrWhiteSpace(GalleryTitle) && (url.Length > 0))
                return (url == "~/" ? Utils.GetCurrentPageUrl() : url);
            else
                return null;
        }

        private string GetTitleUrlTooltip()
        {
            string url = GalleryTitleUrl.Trim();

            if (!String.IsNullOrWhiteSpace(GalleryTitle) && (url.Length > 0))
            {
                switch (url)
                {
                    case "/":
                        {
                            return Resources.GalleryServer.Header_PageHeaderTextUrlToolTipWebRoot;
                        }
                    case "~/":
                        {
                            return Resources.GalleryServer.Header_PageHeaderTextUrlToolTipAppRoot;
                        }
                    default:
                        {
                            return String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Header_PageHeaderTextUrlToolTip, url);
                        }
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Gets an object that fully describes how the specified <paramref name="messageId" /> is to be
        /// displayed in the browser.
        /// </summary>
        /// <returns>Returns an instance of <see cref="ClientMessageOptions" />.</returns>
        private ClientMessageOptions GetMessageOptions(MessageType messageId)
        {
            if (messageId == MessageType.None)
            {
                return new ClientMessageOptions { MessageId = messageId };
            }

            const string resourcePrefix = "Msg_";
            const string headerSuffix = "_Hdr";
            const string detailSuffix = "_Dtl";

            string title = Resources.GalleryServer.ResourceManager.GetString(String.Concat(resourcePrefix, messageId.ToString(), headerSuffix)) ?? String.Empty;
            string msg = Resources.GalleryServer.ResourceManager.GetString(String.Concat(resourcePrefix, messageId.ToString(), detailSuffix)) ?? String.Empty;

            switch (messageId)
            {
                case MessageType.ObjectsSkippedDuringUpload:
                    {
                        var sessionObjectString = HttpContext.Current.Session[GlobalConstants.SkippedFilesDuringUploadSessionKey] as string;

                        List<ActionResult> uploadResults = null;
                        if (!String.IsNullOrWhiteSpace(sessionObjectString))
                        {
                            uploadResults = GetUploadErrors(Newtonsoft.Json.JsonConvert.DeserializeObject<List<ActionResult>>(sessionObjectString));
                        }

                        msg = string.Empty;
                        if (uploadResults != null)
                        {
                            // This message is unique in that we need to choose one of two detail messages from the resource file. One is for when a single
                            // file has been skipped; the other is when multiple files have been skipped.
                            if (uploadResults.Count == 1)
                            {
                                string detailMsgTemplate = Resources.GalleryServer.ResourceManager.GetString(String.Concat(resourcePrefix, messageId.ToString(), "Single", detailSuffix)) ?? String.Empty;
                                msg = String.Format(CultureInfo.CurrentCulture, detailMsgTemplate, uploadResults[0].Title, uploadResults[0].Message);
                            }
                            else if (uploadResults.Count > 1)
                            {
                                string detailMsgTemplate = Resources.GalleryServer.ResourceManager.GetString(String.Concat(resourcePrefix, messageId.ToString(), "Multiple", detailSuffix)) ?? String.Empty;
                                msg = String.Format(CultureInfo.CurrentCulture, detailMsgTemplate, ConvertListToHtmlBullets(uploadResults));
                            }
                        }
                        break;
                    }
            }

            return new ClientMessageOptions
            {
                MessageId = messageId,
                Title = title,
                Message = msg,
                Style = GetMessageStyle(messageId)
            };
        }

        private MessageStyle GetMessageStyle(MessageType messageId)
        {
            switch (messageId)
            {
                case MessageType.MediaObjectDoesNotExist:
                case MessageType.AlbumDoesNotExist:
                case MessageType.UserNameOrPasswordIncorrect:
                case MessageType.AlbumNotAuthorizedForUser:
                case MessageType.NoAuthorizedAlbumForUser:
                case MessageType.ObjectsSkippedDuringUpload:
                case MessageType.CannotEditGalleryIsReadOnly:
                    return MessageStyle.Info;

                case MessageType.None:
                case MessageType.GallerySuccessfullyChanged:
                case MessageType.SettingsSuccessfullyChanged:
                case MessageType.ObjectsBeingProcessedAsyncronously:
                    return MessageStyle.Success;

                default:
                    return MessageStyle.Success;
            }
        }

        /// <summary>
        /// Displays the message stored in <see cref="ClientMessage" /> to the user when the page is loaded in the browser.
        /// </summary>
        private void ShowClientMessage()
        {
            if (ClientMessage != null)
            {
                string script = string.Format(@"
(function ($) {{
  $(document).ready(function () {{
    Gs.Msg.show('{0}', '{1}', {{msgType: '{2}', autoCloseDelay: {3}}});
  }});
}})(jQuery);
",
               ClientMessage.Title != null ? ClientMessage.Title.JsEncode() : null,
               ClientMessage.Message != null ? ClientMessage.Message.JsEncode() : null,
               ClientMessage.Style.ToString().ToLowerInvariant(),
               ClientMessage.AutoCloseDelay);

                Page.ClientScript.RegisterStartupScript(GetType(), String.Concat(ClientID, "_msgScript"), script, true);
            }
        }

        /// <summary>
        /// Gets a data entity containing information about the current gallery. The instance can be JSON-parsed and sent to the 
        /// browser.
        /// </summary>
        /// <returns>Returns <see cref="Entity.Settings" /> object containing information about the current gallery.</returns>
        private Entity.Settings GetSettingsEntity()
        {
            var emptyAlbumThmbSize = Utils.CalculateSize(GallerySettings.EmptyAlbumThumbnailWidthToHeightRatio, GallerySettings.MaxThumbnailLength);

            return new Entity.Settings()
            {
                GalleryId = GalleryId,
                ClientId = GspClientId,
                PageId = PageId,
                MediaClientId = MediaClientId,
                MediaTmplName = MediaTmplName,
                HeaderClientId = HeaderClientId,
                HeaderTmplName = HeaderTmplName,
                ThumbnailClientId = ThumbnailClientId,
                ThumbnailTmplName = ThumbnailTmplName,
                LeftPaneClientId = LeftPaneClientId,
                LeftPaneTmplName = LeftPaneTmplName,
                RightPaneClientId = RightPaneClientId,
                RightPaneTmplName = RightPaneTmplName,
                ShowHeader = ShowHeader,
                ShowLogin = ShowLogin,
                ShowSearch = ShowSearch,
                ShowMediaObjectNavigation = ShowMediaObjectNavigation,
                ShowMediaObjectIndexPosition = ShowMediaObjectIndexPosition,
                EnableSelfRegistration = GallerySettings.EnableSelfRegistration,
                EnableUserAlbum = GallerySettings.EnableUserAlbum,
                AllowManageOwnAccount = GallerySettings.AllowManageOwnAccount,
                Title = GalleryTitle,
                TitleUrl = GetTitleUrl(),
                TitleUrlTooltip = GetTitleUrlTooltip(),
                ShowMediaObjectTitle = ShowMediaObjectTitle,
                MaxUploadSizeKB = GallerySettings.MaxUploadSize,
                PageSize = GallerySettings.PageSize,
                PagerLocation = GallerySettings.PagerLocation.ToString(),
                TransitionType = GallerySettings.MediaObjectTransitionType.ToString().ToLowerInvariant(),
                TransitionDurationMs = Convert.ToInt32(GallerySettings.MediaObjectTransitionDuration * 1000),
                AllowDownload = GallerySettings.EnableMediaObjectDownload,
                AllowZipDownload = GallerySettings.EnableGalleryObjectZipDownload,
                SlideShowIsRunning = AutoPlaySlideShow && GetAlbum().GetChildGalleryObjects(GalleryObjectType.Image).Any(),
                MediaViewSize = (int)MediaViewSize,
                EnableSlideShow = GallerySettings.EnableSlideShow,
                SlideShowType = (int)SlideShowType,
                SlideShowLoop = SlideShowLoop,
                SlideShowIntervalMs = GallerySettings.SlideshowInterval,
                MaxThumbnailLength = GallerySettings.MaxThumbnailLength,
                MaxThmbTitleDisplayLength = GallerySettings.MaxThumbnailTitleDisplayLength,
                AllowAnonymousRating = GallerySettings.AllowAnonymousRating,
                AllowAnonBrowsing = GallerySettings.AllowAnonymousBrowsing,
                AllowCopyingReadOnlyObjects = GallerySettings.AllowCopyingReadOnlyObjects,
                IsReadOnlyGallery = GallerySettings.MediaObjectPathIsReadOnly,
                EmptyAlbumThmbWidth = emptyAlbumThmbSize.Width,
                EmptyAlbumThmbHeight = emptyAlbumThmbSize.Height,
                AllowUrlOverride = GalleryControl.AllowUrlOverride,
                ShowRibbonToolbar = ShouldShowRibbonToolbar(),
                ShowAlbumBreadCrumb = ShouldShowAlbumBreadCrumb()
            };
        }

        /// <summary>
        /// Gets a value indicating whether to display the ribbon toolbar based on the current page, the <see cref="ShowRibbonToolbar" />
        /// setting, and the user's security context. Site and gallery admins always get the ribbon so that they are able to get to the admin pages.
        /// </summary>
        /// <returns><c>true</c> if the ribbon toolbar is to be visible, <c>false</c> otherwise.</returns>
        private bool ShouldShowRibbonToolbar()
        {
            var pagesToHideRibbon = new[] { PageId.changepassword, PageId.createaccount, PageId.login, PageId.myaccount, PageId.recoverpassword };

            if (pagesToHideRibbon.Contains(PageId))
            {
                // Never show the ribbon when we're on one of these pages
                return false;
            }
            else
            {
                // Show the ribbon if the setting is true or the user is an admin
                return (ShowRibbonToolbar || UserCanAdministerSite || UserCanAdministerGallery);
            }
        }

        /// <summary>
        /// Gets a value indicating whether to display the album breadcrumb menu.
        /// </summary>
        /// <returns><c>true</c> if the album breadcrumb is to be visible, <c>false</c> otherwise.</returns>
        private bool ShouldShowAlbumBreadCrumb()
        {
            var pagesToHideBreadcrumb = new[] { PageId.createaccount, PageId.login, PageId.recoverpassword };

            if (pagesToHideBreadcrumb.Contains(PageId))
            {
                // Only show the breadcrumb menu on these pages if anonymous browsing is enabled
                return GallerySettings.AllowAnonymousBrowsing;
            }
            else
            {
                return ShowAlbumBreadCrumb;
            }
        }

        /// <summary>
        /// Gets the gallery object filter specified in the filter query string parameter. If not present or is not a valid
        /// value, returns <paramref name="defaultFilter" />. If <paramref name="defaultFilter" /> is not specified, 
        /// it defaults to <see cref="GalleryObjectType.All" />.
        /// </summary>
        /// <param name="defaultFilter">The default filter. Defaults to <see cref="GalleryObjectType.All" /> when not specified.</param>
        /// <returns>An instance of <see cref="GalleryObjectType" />.</returns>
        private static GalleryObjectType GetGalleryObjectFilter(GalleryObjectType defaultFilter = GalleryObjectType.All)
        {
            if (Utils.IsQueryStringParameterPresent("filter"))
            {
                return GalleryObjectTypeEnumHelper.Parse(Utils.GetQueryStringParameterString("filter"), defaultFilter);
            }

            return defaultFilter;
        }

        /// <summary>
        /// If needed, start the maintenance routine.
        /// </summary>
        /// <remarks>The background thread cannot access HttpContext.Current, so this method will probably fail under DotNetNuke.
        /// To fix that, figure out what DNN needs (portal ID?), and pass it in as a parameter.
        /// so that approach was replaced with this one.</remarks>
        private void AddMaintenanceServiceCallIfNeeded()
        {
            if (AppSetting.Instance.MaintenanceStatus == MaintenanceStatus.NotStarted && !IsPostBack)
            {
                Utils.PerformMaintenance();
                //				const string script = @"
                //$(function() {
                //	Gsp.Gallery.PerformMaintenance(function() {}, function() {}); // Swallow error on client
                //});";

                //				this.Page.ClientScript.RegisterStartupScript(this.GetType(), "galleryPageStartupScript", script, true);
            }
        }

        #endregion
    }
}
