using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GalleryServer.Events.CustomExceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Xml;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;
using SecurityManager = GalleryServer.Business.SecurityManager;

namespace GalleryServer.Web
{
    /// <summary>
    /// Contains general purpose routines useful for this website as well as a convenient
    /// gateway to functionality provided in other business layers.
    /// </summary>
    public static class Utils
    {
        #region Private Static Fields

        private static readonly object _sharedLock = new object();
        private static string _galleryRoot;
        private static string _galleryResourcesPath;
        private static string _skinPath;
        private static string _webConfigFilePath;
        private static string _lastKnownHostUrl;

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets or sets the name of the current user. Returns an empty string for anonymous users. This property becomes 
        /// available immediately after a user logs in, even within the current page's life cycle. This property is preferred 
        /// over HttpContext.Current.User.Identity.Name, which does not contain the user's name until the next page load. 
        /// This property should be set only when the user logs in. When the property is not explicitly assigned, it 
        /// automatically returns the value of HttpContext.Current.User.Identity.Name. When no HTTP context is available 
        /// (such as during async method calls), this property returns null.
        /// </summary>
        /// <value>The name of the current user.</value>
        public static string UserName
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return null;
                }

                object userName = HttpContext.Current.Items["UserName"];
                if (userName != null)
                {
                    return userName.ToString();
                }
                else
                {
                    return ParseUserName(HttpContext.Current.User.Identity.Name);
                }
            }
            set { HttpContext.Current.Items["UserName"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated. This property becomes true available immediately after 
        /// a user logs in, even within the current page's life cycle. This property is preferred over 
        /// HttpContext.Current.User.Identity.IsAuthenticated, which does not become true until the next page load. 
        /// This property should be set only when the user logs in. When the property is not explicitly assigned, it automatically 
        /// returns the value of HttpContext.Current.User.Identity.IsAuthenticated. When no HTTP context is available (such as during
        /// async method calls), this property returns <c>false</c>.
        /// </summary>
        public static bool IsAuthenticated
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return false;
                }

                bool isAuthenticated;
                object objIsAuthenticated = HttpContext.Current.Items["IsAuthenticated"];

                if ((objIsAuthenticated != null) && Boolean.TryParse(objIsAuthenticated.ToString(), out isAuthenticated))
                {
                    return isAuthenticated;
                }
                else
                {
                    return HttpContext.Current.User.Identity.IsAuthenticated;
                }
            }
            set { HttpContext.Current.Items["IsAuthenticated"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the current request is from the local computer. Returns <c>false</c> if 
        /// <see cref="HttpContext.Current" /> is null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the current request is from the local computer; otherwise, <c>false</c>.
        /// </value>
        public static bool IsLocalRequest
        {
            get
            {
                if (HttpContext.Current == null)
                    return false;

                return HttpContext.Current.Request.IsLocal;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current request is in debug mode. That is, it returns <c>true</c> when 
        /// debug = "true" in web.config and returns <c>false</c> when debug = "false".
        /// </summary>
        /// <value><c>true</c> if the current request is in debug mode; otherwise, <c>false</c>.</value>
        public static bool IsDebugEnabled
        {
            get
            {
                return HttpContext.Current.IsDebuggingEnabled;
            }
        }

        /// <summary>
        /// Get the path, relative to the web site root, to the directory containing the Gallery Server user controls and 
        /// other resources. Does not include the containing page or the trailing slash. Example: If GSP is installed at 
        /// C:\inetpub\wwwroot\dev\gallery, where C:\inetpub\wwwroot\ is the parent web site, and the gallery support files are in 
        /// the gsp directory, this property returns /dev/gallery/gsp. Guaranteed to not return null.
        /// </summary>
        /// <value>Returns the path, relative to the web site root, to the directory containing the Gallery Server user 
        /// controls and other resources.</value>
        public static string GalleryRoot
        {
            get
            {
                if (_galleryRoot == null)
                {
                    _galleryRoot = CalculateGalleryRoot();
                }

                return _galleryRoot;
            }
        }

        /// <summary>
        /// Gets the path, relative to the current application, to the directory containing the Gallery Server
        /// resources such as images, user controls, scripts, etc. This value is pulled from the AppSettings value "GalleryResourcesPath"
        /// if present; otherwise it defaults to "gs". Examples: "gs", "GalleryServer\resources"
        /// </summary>
        /// <value>Returns the path, relative to the current application, to the directory containing the Gallery Server
        /// resources such as images, user controls, scripts, etc.</value>
        public static string GalleryResourcesPath
        {
            get
            {
                if (_galleryResourcesPath == null)
                {
                    _galleryResourcesPath = GetGalleryResourcesPath();
                }

                return _galleryResourcesPath;
            }
        }

        /// <summary>
        /// Gets the path, relative to the current application, to the directory containing the Gallery Server
        /// skin resources for the currently selected skin. Examples: "gs/skins/dark", "/dev/gallery/gsp/skins/light"
        /// </summary>
        /// <value>Returns the path, relative to the current application, to the directory containing the Gallery Server
        /// skin resources.</value>
        public static string SkinPath
        {
            get
            {
                if (_skinPath == null)
                {
                    _skinPath = String.Concat(GalleryRoot, "/skins/", Skin);
                }

                return _skinPath;
            }
        }

        /// <summary>
        /// Gets the name of the currently selected skin. Examples: "dark", "light"
        /// </summary>
        /// <value>Returns the name of the currently selected skin.</value>
        public static string Skin
        {
            get
            {
                return AppSetting.Instance.Skin;
            }
        }

        /// <summary>
        /// Gets the fully qualified file path to web.config. Guaranteed to not return null.
        /// Example: C:\inetpub\wwwroot\gallery\web.config
        /// </summary>
        /// <value>The fully qualified file path to web.config.</value>
        public static string WebConfigFilePath
        {
            get
            {
                if (_webConfigFilePath == null)
                {
                    _webConfigFilePath = HttpContext.Current.Server.MapPath("~/web.config");
                }

                return _webConfigFilePath;
            }
        }

        /// <summary>
        /// Get the path, relative to the web site root, to the current web application. Does not include the containing page 
        /// or the trailing slash. Example: If GSP is installed at C:\inetpub\wwwroot\dev\gallery, and C:\inetpub\wwwroot\ is 
        /// the parent web site, this property returns /dev/gallery. Guaranteed to not return null.
        /// </summary>
        /// <value>Get the path, relative to the web site root, to the current web application.</value>
        public static string AppRoot
        {
            get
            {
                return HttpRuntime.AppDomainAppVirtualPath.TrimEnd(new char[] { '/' });

                // OBSOLETE: Used to use the following but it works only when a context is available:
                //_appRoot = HttpContext.Current.Request.ApplicationPath.TrimEnd(new char[] { '/' });
            }
        }

        /// <summary>
        /// Gets or sets the URI of the previous page the user was viewing. The value is stored in the user's session, and 
        /// can be used after a user has completed a task to return to the original page. If the Session object is not available,
        /// no value is saved in the setter and a null is returned in the getter.
        /// </summary>
        /// <value>The URI of the previous page the user was viewing.</value>
        public static Uri PreviousUri
        {
            get
            {
                if (HttpContext.Current.Session != null)
                    return (Uri)HttpContext.Current.Session["ReferringUrl"];
                else
                    return null;
            }
            set
            {
                if (HttpContext.Current.Session == null)
                    return; // Session is disabled for this page.

                HttpContext.Current.Session["ReferringUrl"] = value;
            }
        }

        /// <summary>
        /// Gets the path to the install trigger file. Example: "C:\websites\gallery\App_Data\install.txt". This file is expected to be
        /// an empty text file. When present, it is a signal to the application that an installation is being requested.
        /// </summary>
        /// <value>A <see cref="String" />.</value>
        public static string InstallFilePath
        {
            get
            {
                return Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, GlobalConstants.AppDataDirectory, GlobalConstants.InstallTriggerFileName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether an installation is being requested. Returns <c>true</c> when a text file
        /// named install.txt is present in the App_Data directory.
        /// </summary>
        /// <value><c>true</c> if an install is requested; otherwise, <c>false</c>.</value>
        public static bool InstallRequested
        {
            get
            {
                return File.Exists(InstallFilePath);
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users
        /// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested
        /// security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalMediaObject" />,
        /// since Gallery Server is configured by default to allow anonymous viewing access
        /// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
        /// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
        /// un-authenticated users, and the standard gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are
        /// specified, the method is successful if the user has permission for at least one of the actions. If you require that all actions
        /// be satisfied to be successful, call one of the overloads that accept a SecurityActionsOption and
        /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in
        /// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify
        /// <see cref="int.MinValue" />).</param>
        /// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
        /// is ignored for logged on users.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        /// <overloads>
        /// Determine if the current user has permission to perform the requested action.
        ///   </overloads>
        public static bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId, bool isPrivate, bool isVirtualAlbum)
        {
            return IsUserAuthorized(securityActions, RoleController.GetGalleryServerRolesForUser(), albumId, galleryId, isPrivate, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether user has permission to perform the specified security actions. Un-authenticated users
        /// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested
        /// security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalMediaObject" />,
        /// since Gallery Server is configured by default to allow anonymous viewing access
        /// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
        /// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
        /// un-authenticated users, and the standard gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery).</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in
        /// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify
        /// <see cref="int.MinValue" />).</param>
        /// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
        /// is ignored for logged on users.</param>
        /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
        /// to be successful or just one.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        public static bool IsUserAuthorized(SecurityActions securityActions, int albumId, int galleryId, bool isPrivate, SecurityActionsOption secActionsOption, bool isVirtualAlbum)
        {
            return IsUserAuthorized(securityActions, RoleController.GetGalleryServerRolesForUser(), albumId, galleryId, isPrivate, secActionsOption, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether user has permission to perform at least one of the specified security actions. Un-authenticated users
        /// (anonymous users) are always considered NOT authorized (that is, this method returns false) except when the requested
        /// security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or <see cref="SecurityActions.ViewOriginalMediaObject" />,
        /// since Gallery Server is configured by default to allow anonymous viewing access
        /// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
        /// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
        /// un-authenticated users, and the standard gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are
        /// specified, the method is successful if the user has permission for at least one of the actions. If you require that all actions
        /// be satisfied to be successful, call one of the overloads that accept a SecurityActionsOption and
        /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
        /// for anonymous users. The parameter may be null.</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in
        /// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify
        /// <see cref="int.MinValue" />).</param>
        /// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
        /// is ignored for logged on users.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        public static bool IsUserAuthorized(SecurityActions securityActions, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isPrivate, bool isVirtualAlbum)
        {
            return IsUserAuthorized(securityActions, roles, albumId, galleryId, isPrivate, SecurityActionsOption.RequireOne, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether user has permission to perform the specified security actions. When multiple security actions are passed, use
        /// <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or only one item
        /// must be satisfied. Un-authenticated users (anonymous users) are always considered NOT authorized (that is, this method returns
        /// false) except when the requested security action is <see cref="SecurityActions.ViewAlbumOrMediaObject" /> or
        /// <see cref="SecurityActions.ViewOriginalMediaObject" />, since Gallery Server is configured by default to allow anonymous viewing access
        /// but it does not allow anonymous editing of any kind. This method will continue to work correctly if the webmaster configures
        /// Gallery Server to require users to log in in order to view objects, since at that point there will be no such thing as
        /// un-authenticated users, and the standard gallery server role functionality applies.
        /// </summary>
        /// <param name="securityActions">Represents the permission or permissions being requested. Multiple actions can be specified by using
        /// a bitwise OR between them (example: SecurityActions.AdministerSite | SecurityActions.AdministerGallery). If multiple actions are
        /// specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied to be successful or
        /// only one item must be satisfied.</param>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
        /// for anonymous users. The parameter may be null.</param>
        /// <param name="albumId">The album ID to which the security action applies.</param>
        /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in
        /// this gallery. This parameter is not required <paramref name="securityActions" /> is SecurityActions.AdministerSite (you can specify
        /// <see cref="int.MinValue" />).</param>
        /// <param name="isPrivate">Indicates whether the specified album is private (hidden from anonymous users). The parameter
        /// is ignored for logged on users.</param>
        /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityActions" />
        /// to be successful or just one.</param>
        /// <param name="isVirtualAlbum">if set to <c>true</c> the album is a virtual album.</param>
        /// <returns>
        /// Returns true when the user is authorized to perform the specified security action against the specified album;
        /// otherwise returns false.
        /// </returns>
        public static bool IsUserAuthorized(SecurityActions securityActions, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isPrivate, SecurityActionsOption secActionsOption, bool isVirtualAlbum)
        {
            return SecurityManager.IsUserAuthorized(securityActions, roles, albumId, galleryId, IsAuthenticated, isPrivate, secActionsOption, isVirtualAlbum);
        }

        /// <summary>
        /// Determine whether the user belonging to the specified <paramref name="roles" /> is a site administrator. The user is considered a site
        /// administrator if at least one role has Allow Administer Site permission.
        /// </summary>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. The parameter may be null.</param>
        /// <returns>
        /// 	<c>true</c> if the user is a site administrator; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUserSiteAdministrator(IGalleryServerRoleCollection roles)
        {
            return SecurityManager.IsUserSiteAdministrator(roles);
        }

        /// <summary>
        /// Determine whether the user belonging to the specified <paramref name="roles"/> is a gallery administrator for the specified
        /// <paramref name="galleryId"/>. The user is considered a gallery administrator if at least one role has Allow Administer Gallery permission.
        /// </summary>
        /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. The parameter may be null.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>
        /// 	<c>true</c> if the user is a gallery administrator; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsUserGalleryAdministrator(IGalleryServerRoleCollection roles, int galleryId)
        {
            return SecurityManager.IsUserGalleryAdministrator(roles, galleryId);
        }

        /// <summary>
        /// Determine whether the currently logged-on user is a site administrator. The user is considered a site
        /// administrator if at least one role has Allow Administer Site permission.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the user is a site administrator; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentUserSiteAdministrator()
        {
            return IsUserSiteAdministrator(RoleController.GetGalleryServerRolesForUser());
        }

        /// <summary>
        /// Determine whether the currently logged-on user is a gallery administrator for the specified <paramref name="galleryId"/>. 
        /// The user is considered a gallery administrator if at least one role has Allow Administer Gallery permission.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <returns>
        /// 	<c>true</c> if the user is a gallery administrator; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCurrentUserGalleryAdministrator(int galleryId)
        {
            return SecurityManager.IsUserGalleryAdministrator(RoleController.GetGalleryServerRolesForUser(), galleryId);
        }

        /// <summary>
        /// Determines whether the current request is a Web.API request.
        /// </summary>
        /// <returns><c>true</c> if it is web API request; otherwise, <c>false</c>.</returns>
        public static bool IsWebApiRequest()
        {
            if (HttpContext.Current == null)
                return false;

            var urlPath = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

            return (urlPath != null && urlPath.StartsWith("~/api"));
        }

        /// <summary>
        /// Determine the trust level of the currently running application.
        /// </summary>
        /// <returns>Returns the trust level of the currently running application.</returns>
        public static ApplicationTrustLevel GetCurrentTrustLevel()
        {
            AspNetHostingPermissionLevel aspnetTrustLevel = AspNetHostingPermissionLevel.None;

            foreach (AspNetHostingPermissionLevel aspnetTrustLevelIterator in
              new AspNetHostingPermissionLevel[] {
                                            AspNetHostingPermissionLevel.Unrestricted,
                                            AspNetHostingPermissionLevel.High,
                                            AspNetHostingPermissionLevel.Medium,
                                            AspNetHostingPermissionLevel.Low,
                                            AspNetHostingPermissionLevel.Minimal
                                                 })
            {
                try
                {
                    new AspNetHostingPermission(aspnetTrustLevelIterator).Demand();
                    aspnetTrustLevel = aspnetTrustLevelIterator;
                    break;
                }
                catch (SecurityException)
                {
                    continue;
                }
            }

            ApplicationTrustLevel trustLevel = ApplicationTrustLevel.None;

            switch (aspnetTrustLevel)
            {
                case AspNetHostingPermissionLevel.Minimal: trustLevel = ApplicationTrustLevel.Minimal; break;
                case AspNetHostingPermissionLevel.Low: trustLevel = ApplicationTrustLevel.Low; break;
                case AspNetHostingPermissionLevel.Medium: trustLevel = ApplicationTrustLevel.Medium; break;
                case AspNetHostingPermissionLevel.High: trustLevel = ApplicationTrustLevel.High; break;
                case AspNetHostingPermissionLevel.Unrestricted: trustLevel = ApplicationTrustLevel.Full; break;
                default: trustLevel = ApplicationTrustLevel.Unknown; break;
            }

            return trustLevel;
        }

        /// <summary>
        /// Get the path, relative to the web site root, to the specified resource. Example: If the web application is at
        /// /dev/gsweb/, the directory containing the resources is /gs/, and the desired resource is /images/info.gif, this function
        /// will return /dev/gsweb/gs/images/info.gif.
        /// </summary>
        /// <param name="resource">A path relative to the directory containing the Gallery Server resource files (ex: images/info.gif).
        /// The leading forward slash ('/') is optional.</param>
        /// <returns>Returns the path, relative to the web site root, to the specified resource.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resource" /> is null.</exception>
        public static string GetUrl(string resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            if (!resource.StartsWith("/", StringComparison.Ordinal))
                resource = resource.Insert(0, "/"); // Make sure it starts with a '/'

            resource = String.Concat(GalleryRoot, resource);

            //#if DEBUG
            //      if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath(resource)))
            //        throw new System.IO.FileNotFoundException(String.Format(CultureInfo.CurrentCulture, "No file exists at {0}.", resource), resource);
            //#endif

            return resource;
        }

        /// <summary>
        /// Get the path, relative to the web site root, to the specified resource in the current skin directory. Example: 
        /// If the web application is at /dev/gsweb/, the directory containing the skin resources is /gs/skins/simple-grey,
        /// and the desired resource is /images/info.gif, this function will return /dev/gsweb/gs/skins/simple-grey/images/info.gif.
        /// </summary>
        /// <param name="resource">A path relative to the skin directory containing the Gallery Server resource files (ex: images/info.gif).
        /// The leading forward slash ('/') is optional but recommended for readability and a slight performance improvement.</param>
        /// <returns>Returns the path, relative to the web site root, to the specified skin resource.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resource" /> is null.</exception>
        public static string GetSkinnedUrl(string resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            if (!resource.StartsWith("/", StringComparison.Ordinal))
                resource = resource.Insert(0, "/"); // Make sure it starts with a '/'

            resource = String.Concat(SkinPath, resource);

            //#if DEBUG
            //      if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath(resource)))
            //        throw new System.IO.FileNotFoundException(String.Format(CultureInfo.CurrentCulture, "No file exists at {0}.", resource), resource);
            //#endif

            return resource;
        }

        /// <overloads>Get an URL relative to the website root for the requested page.</overloads>
        /// <summary>
        /// Get an URL relative to the website root for the requested <paramref name="page"/>. Example: If 
        /// <paramref name="page"/> is PageId.album and the current page is /dev/gs/gallery.aspx, this function 
        /// returns /dev/gs/gallery.aspx?g=album. Returns null if <see cref="HttpContext.Current" /> is null.
        /// </summary>
        /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
        /// <returns>Returns an URL relative to the website root for the requested <paramref name="page"/>, or null 
        /// if <see cref="HttpContext.Current" /> is null.</returns>
        public static string GetUrl(PageId page)
        {
            if (HttpContext.Current == null)
                return null;

            return AddQueryStringParameter(GetCurrentPageUrl(), String.Concat("g=", page));
        }

        /// <summary>
        /// Get an URL relative to the website root for the requested <paramref name="page"/> and with the specified 
        /// <paramref name="args"/> appended as query string parameters. Example: If <paramref name="page"/> is PageId.task_addobjects, 
        /// the current page is /dev/gs/gallery.aspx, <paramref name="format"/> is "aid={0}", and <paramref name="args"/>
        /// is "23", this function returns /dev/gs/gallery.aspx?g=task_addobjects&amp;aid=23. If the <paramref name="page"/> is
        /// <see cref="PageId.album"/> or <see cref="PageId.mediaobject"/>, don't include the "g" query string parameter, since 
        /// we can deduce it by looking for the aid or moid query string parms. Returns null if <see cref="HttpContext.Current" /> is null.
        /// </summary>
        /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
        /// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
        /// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
        /// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
        /// <returns>Returns an URL relative to the website root for the requested <paramref name="page"/>, or 
        /// null if <see cref="HttpContext.Current" /> is null.</returns>
        public static string GetUrl(PageId page, string format, params object[] args)
        {
            if (HttpContext.Current == null)
                return null;

            string queryString = String.Format(CultureInfo.InvariantCulture, format, args);

            if ((page != PageId.album) && (page != PageId.mediaobject))
            {
                // Don't use the "g" parameter for album or mediaobject pages, since we can deduce it by looking for the 
                // aid or moid query string parms. This results in a shorter, cleaner URL.
                queryString = String.Concat("g=", page, "&", queryString);
            }

            return AddQueryStringParameter(GetCurrentPageUrl(), queryString);
        }

        /// <summary>
        /// Get the physical path to the <paramref name="resource"/>. Example: If the web application is at
        /// C:\inetpub\wwwroot\dev\gsweb\, the directory containing the resources is \gs\, and the desired resource is
        /// /templates/AdminNotificationAccountCreated.txt, this function will return 
        /// C:\inetpub\wwwroot\dev\gsweb\gs\templates\AdminNotificationAccountCreated.txt.
        /// </summary>
        /// <param name="resource">A path relative to the directory containing the Gallery Server resource files (ex: images/info.gif).
        /// The slash may be forward (/) or backward (\), although there is a slight performance improvement if it is forward (/).
        /// The parameter does not require a leading slash, although there is a slight performance improvement if it is present.</param>
        /// <returns>Returns the physical path to the requested <paramref name="resource"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resource" /> is null.</exception>
        public static string GetPath(string resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            // Convert back slash (\) to forward slash, if present.
            resource = resource.Replace(Path.DirectorySeparatorChar, '/');

            return HttpContext.Current.Server.MapPath(GetUrl(resource));
        }

        /// <summary>
        /// Gets the URI of the current page request. Automatically handles port forwarding configurations by incorporating the port in the
        /// HTTP_HOST server variable in the URI. Ex: "http://75.135.92.12:8080/dev/gs/default.aspx?moid=770"
        /// Returns null if <see cref="HttpContext.Current" /> is null.
        /// </summary>
        /// <returns>Returns the URI of the current page request, or null if <see cref="HttpContext.Current" /> is null.</returns>
        public static Uri GetCurrentPageUri()
        {
            if (HttpContext.Current == null)
                return null;

            UriBuilder uriBuilder = new UriBuilder(HttpContext.Current.Request.Url);
            uriBuilder.Host = GetHostName();
            int? port = GetPort();
            if (port.HasValue)
            {
                uriBuilder.Port = port.Value;
            }

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Gets the URL, relative to the website root and optionally including any query string parameters, to the current page.
        /// This method is a wrapper for a call to HttpContext.Current.Request.Url. If the current URL is an API call (i.e. it starts
        /// with "~/api", the referrer is used instead. Returns null if <see cref="HttpContext.Current" /> is null.
        /// Examples: "/dev/gs/gallery.aspx", "/dev/gs/gallery.aspx?g=admin_email&amp;aid=2389" 
        /// </summary>
        /// <param name="includeQueryString">When <c>true</c> the query string is included.</param>
        /// <returns>Returns the URL, relative to the website root and including any query string parameters, to the current page,
        /// or null if <see cref="HttpContext.Current" /> is null.</returns>
        public static string GetCurrentPageUrl(bool includeQueryString = false)
        {
            if (HttpContext.Current == null)
                return null;

            var urlPath = HttpContext.Current.Request.Url.AbsolutePath;
            var query = HttpContext.Current.Request.Url.Query;

            if (IsWebApiRequest())
            {
                if (HttpContext.Current.Request.UrlReferrer != null)
                {
                    urlPath = HttpContext.Current.Request.UrlReferrer.AbsolutePath;
                    query = HttpContext.Current.Request.UrlReferrer.Query;
                }
                else
                {
                    // We don't know the current web page, so just return an empty string. This should not typically occur.
                    urlPath = query = String.Empty;
                }
            }

            if (includeQueryString)
                return String.Concat(urlPath, query);
            else
                return urlPath;
        }

        /// <summary>
        /// Get the full path to the current web page. Does not include any query string parms. Returns null if 
        /// <see cref="HttpContext.Current" /> is null. Example: "http://www.techinfosystems.com/gs/default.aspx"
        /// </summary>
        /// <returns>Returns the full path to the current web page, or null if <see cref="HttpContext.Current" /> is null.</returns>
        /// <remarks>This value is calculated each time it is requested because the URL may be different for different users 
        /// (a local admin's URL may be http://localhost/gs/default.aspx, someone on the intranet may get the server's name
        /// (http://Server1/gs/default.aspx), and someone on the internet may get the full name (http://www.bob.com/gs/default.aspx).</remarks>
        public static string GetCurrentPageUrlFull()
        {
            if (HttpContext.Current == null)
                return null;

            return String.Concat(GetHostUrl(), GetCurrentPageUrl());
        }

        /// <summary>
        /// Get the URI scheme, DNS host name or IP address, and port number for the current application. 
        /// Examples: http://www.site.com, http://localhost, http://127.0.0.1, http://godzilla
        /// Returns null if <see cref="HttpContext.Current" /> is null and no host URL has ever been calculated during this app's lifetime.
        /// </summary>
        /// <returns>Returns the URI scheme, DNS host name or IP address, and port number for the current application, 
        /// or null.</returns>
        /// <remarks>This value is retrieved from the user's session. If not present in the session, such as when the user first arrives, it
        /// is calculated by parsing the appropriate pieces from HttpContext.Current.Request.Url and the HTTP_HOST server variable. The path is 
        /// calculated on a per-user basis because the URL may be different for different users (a local admin's URL may be 
        /// http://localhost, someone on the intranet may get the server's name (http://Server1), and someone on the internet may get 
        /// the full name (http://www.site.com).</remarks>
        public static string GetHostUrl()
        {
            if (HttpContext.Current == null)
            {
                // This is not a fail-safe approach since it might return a server name  (e.g. http://Server1) and then be accessed by a user on 
                // the internet, but what is a better option? At the time of this writing the only case where the HTTP context is null is when
                // SignalR is trying to generate an URL to the media object for the media queue page, leaving the possibility multiple admins
                // on different hosts may see a broken image link on that page.
                return _lastKnownHostUrl;
            }

            string hostUrl = null;

            if (HttpContext.Current.Session != null)
            {
                hostUrl = (String)HttpContext.Current.Session["HostUrl"];
            }

            if (String.IsNullOrEmpty(hostUrl))
            {
                hostUrl = String.Concat(HttpContext.Current.Request.Url.Scheme, "://", GetHostNameAndPort());

                if (HttpContext.Current.Session != null)
                    HttpContext.Current.Session["HostUrl"] = hostUrl;
            }

            // Save to static variable. We'll use this later if we ever call this function and we don't have an HTTP context to use.
            _lastKnownHostUrl = hostUrl;

            return hostUrl;
        }

        /// <summary>
        /// Gets the URL to the current web application. Does not include the containing page or the trailing slash. 
        /// Guaranteed to not return null. Example: If the gallery is installed in a virtual directory 'gallery'
        /// on domain 'www.site.com', this returns 'http://www.site.com/gallery'.
        /// </summary>
        /// <returns>Returns the URL to the current web application.</returns>
        public static string GetAppUrl()
        {
            return String.Concat(GetHostUrl(), AppRoot);
        }

        /// <summary>
        /// Gets the URL to the list of recently added media objects. Ex: http://site.com/gallery/default.aspx?latest=50
        /// Requires gallery to be running in trial mode or under a Home &amp; Nonprofit edition or higher license; otherwise it returns null.
        /// </summary>
        /// <returns>Returns the URL to the recently added media objects.</returns>
        public static string GetLatestUrl()
        {
            if (AppSetting.Instance.License.LicenseType >= LicenseLevel.HomeNonprofit)
                return AddQueryStringParameter(GetCurrentPageUrl(), "latest=50");
            else
                return null;
        }

        /// <summary>
        /// Gets the URL to the list of top rated media objects. Ex: http://site.com/gallery/default.aspx?latest=50
        /// Requires gallery to be running in trial mode or under a Home &amp; Nonprofit edition or higher license; otherwise it returns null.
        /// </summary>
        /// <returns>Returns the URL to the top rated media objects.</returns>
        public static string GetTopRatedUrl()
        {
            if (AppSetting.Instance.License.LicenseType >= LicenseLevel.HomeNonprofit && AppSetting.Instance.ProviderDataStore != ProviderDataStore.SqlCe)
                return AddQueryStringParameter(GetCurrentPageUrl(), "rating=highest&top=50");
            else
                return null;
        }

        /// <summary>
        /// Gets the full URL to the directory containing the gallery resources. Does not include the containing page or 
        /// the trailing slash. Guaranteed to not return null. Example: If the gallery is installed in a virtual directory 'gallery'
        /// on domain 'www.site.com' and the resources are in directory 'gs', this returns 'http://www.site.com/gallery/gs'.
        /// </summary>
        /// <returns>Returns the full URL to the directory containing the gallery resources.</returns>
        public static string GetGalleryResourcesUrl()
        {
            return String.Concat(GetHostUrl(), GalleryRoot);
        }

        /// <summary>
        /// Gets the Domain Name System (DNS) host name or IP address and the port number for the current web application. Includes the
        /// port number if it differs from the default port. The value is generated from the HTTP_HOST server variable if present; 
        /// otherwise HttpContext.Current.Request.Url.Authority is used. Ex: "www.site.com", "www.site.com:8080", "192.168.0.50", "75.135.92.12:8080"
        /// </summary>
        /// <returns>A <see cref="String" /> containing the authority component of the URI for the current web application.</returns>
        /// <remarks>This function correctly handles configurations where the web application is port forwarded through a router. For 
        /// example, if the router is configured to map incoming requests at www.site.com:8080 to an internal IP 192.168.0.100:8056,
        /// this function returns "www.site.com:8080". This is accomplished by using the HTTP_HOST server variable rather than 
        /// HttpContext.Current.Request.Url.Authority (when HTTP_HOST is present).</remarks>
        public static string GetHostNameAndPort()
        {
            string httpHost = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];

            return (!String.IsNullOrEmpty(httpHost) ? httpHost : HttpContext.Current.Request.Url.Authority);
        }

        /// <summary>
        /// Gets the host name for the current request. Does not include port number or scheme. The value is generated from the 
        /// HTTP_HOST server variable if present; otherwise HttpContext.Current.Request.Url.Authority is used. 
        /// Ex: "www.site.com", "75.135.92.12"
        /// </summary>
        /// <returns>Returns the host name for the current request.</returns>
        public static string GetHostName()
        {
            string host = GetHostNameAndPort();

            return (host.IndexOf(":", StringComparison.Ordinal) < 0 ? host : host.Substring(0, host.IndexOf(":", StringComparison.Ordinal)));
        }

        /// <summary>
        /// Gets the port for the current request if one is specified; otherwise returns null. The value is generated from the 
        /// HTTP_HOST server variable if present; otherwise HttpContext.Current.Request.Url.Authority is used. 
        /// </summary>
        /// <returns>Returns the port for the current request if one is specified; otherwise returns null.</returns>
        public static int? GetPort()
        {
            string host = GetHostNameAndPort();

            if (host.IndexOf(":", StringComparison.Ordinal) >= 0)
            {
                string portString = host.Substring(host.IndexOf(":", StringComparison.Ordinal) + 1);

                int port;
                if (Int32.TryParse(portString, out port))
                {
                    return port;
                }
            }

            return null;
        }

        /// <overloads>Redirects the user to the specified <paramref name="page"/>.</overloads>
        /// <summary>
        /// Redirects the user to the specified <paramref name="page"/>. The redirect occurs immediately.
        /// </summary>
        /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
        public static void Redirect(PageId page)
        {
            HttpContext.Current.Response.Redirect(GetUrl(page), true);
        }

        /// <summary>
        /// Redirects the user, using Response.Redirect, to the specified <paramref name="page"/>. If <paramref name="endResponse"/> is true, the redirect occurs 
        /// when the page has finished processing all events. When false, the redirect occurs immediately.
        /// </summary>
        /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
        /// <param name="endResponse">When <c>true</c> the redirect occurs immediately. When false, the redirect is delayed until the
        /// page processing is complete.</param>
        public static void Redirect(PageId page, bool endResponse)
        {
            HttpContext.Current.Response.Redirect(GetUrl(page), endResponse);
        }

        /// <summary>
        /// Redirects the user, using Response.Redirect, to the specified <paramref name="page"/> and with the specified 
        /// <paramref name="args"/> appended as query string parameters. Example: If <paramref name="page"/> is PageId.album, 
        /// the current page is /dev/gs/gallery.aspx, <paramref name="format"/> is "aid={0}", and <paramref name="args"/>
        /// is "23", this function redirects to /dev/gs/gallery.aspx?g=album&amp;aid=23.
        /// </summary>
        /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
        /// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
        /// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
        /// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
        public static void Redirect(PageId page, string format, params object[] args)
        {
            HttpContext.Current.Response.Redirect(GetUrl(page, format, args), true);
        }

        /// <summary>
        /// Redirects the user, using Response.Redirect, to the specified <paramref name="url"/>
        /// </summary>
        /// <param name="url">The URL to redirect the user to.</param>
        public static void Redirect(string url)
        {
            HttpContext.Current.Response.Redirect(url, true);
        }

        /// <summary>
        /// Transfers the user, using Server.Transfer, to the specified <paramref name="page"/>.
        /// </summary>
        /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
        public static void Transfer(PageId page)
        {
            try
            {
                HttpContext.Current.Server.Transfer(GetUrl(page));
            }
            catch (ThreadAbortException) { }
        }

        /// <summary>
        /// Redirects the user to the specified <paramref name="page"/> and with the specified 
        /// <paramref name="args"/> appended as query string parameters. Example: If <paramref name="page"/> is PageId.album, 
        /// the current page is /dev/gs/gallery.aspx, <paramref name="format"/> is "aid={0}", and <paramref name="args"/>
        /// is "23", this function redirects to /dev/gs/gallery.aspx?g=album&amp;aid=23.
        /// </summary>
        /// <param name="page">A <see cref="PageId"/> enumeration that represents the desired <see cref="Pages.GalleryPage"/>.</param>
        /// <param name="endResponse">When <c>true</c> the redirect occurs immediately. When false, the redirect is delayed until the
        /// page processing is complete.</param>
        /// <param name="format">A format string whose placeholders are replaced by values in <paramref name="args"/>. Do not use a '?'
        /// or '&amp;' at the beginning of the format string. Example: "msg={0}".</param>
        /// <param name="args">The values to be inserted into the <paramref name="format"/> string.</param>
        public static void Redirect(PageId page, bool endResponse, string format, params object[] args)
        {
            HttpContext.Current.Response.Redirect(GetUrl(page, format, args), endResponse);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Retrieves the specified query string parameter value from the query string. Returns int.MinValue if
        /// the parameter is not found, it is not a valid integer, or it is &lt;= 0.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <returns>Returns the value of the specified query string parameter.</returns>
        public static int GetQueryStringParameterInt32(string parameterName)
        {
            string parm = HttpContext.Current.Request.QueryString[parameterName];

            int qsValue;
            if (Int32.TryParse(parm, out qsValue) && (qsValue >= 0))
            {
                return qsValue;
            }
            else
            {
                return Int32.MinValue;
            }
        }

        /// <summary>
        /// Retrieves the specified query string parameter value from the query string. If no URI is specified, the current 
        /// request URL is used. Returns int.MinValue if the parameter is not found, it is not a valid integer, or it is &lt;= 0.
        /// </summary>
        /// <param name="uri">The URI containing the query string parameter to retrieve.</param>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <returns>Returns the value of the specified query string parameter.</returns>
        public static int GetQueryStringParameterInt32(Uri uri, string parameterName)
        {
            string parm = null;
            if (uri == null)
            {
                parm = HttpContext.Current.Request.QueryString[parameterName];
            }
            else
            {
                string qs = uri.Query.TrimStart(new char[] { '?' });
                foreach (string nameValuePair in qs.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] nameValue = nameValuePair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (nameValue.Length > 1)
                    {
                        if (String.Equals(nameValue[0], parameterName))
                        {
                            parm = nameValue[1];
                            break;
                        }
                    }
                }
            }

            if ((String.IsNullOrEmpty(parm)) || (!HelperFunctions.IsInt32(parm) || (Convert.ToInt32(parm, CultureInfo.InvariantCulture) <= 0)))
            {
                return Int32.MinValue;
            }
            else
            {
                return Convert.ToInt32(parm, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Retrieves the specified query string parameter value from the query string. Returns string.Empty 
        /// if the parameter is not found.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <returns>Returns the value of the specified query string parameter.</returns>
        /// <remarks>Do not call UrlDecode on the string, as it appears that .NET already does this.</remarks>
        public static string GetQueryStringParameterString(string parameterName)
        {
            return HttpContext.Current.Request.QueryString[parameterName] ?? String.Empty;
        }

        /// <summary>
        /// Retrieves the specified query string parameter values from the query string as an array. When the query
        /// string value contains the <paramref name="delimiter" />, the value is split into an array of items.
        /// Returns null if the parameter is not found. Any leading or trailing apostrophes, quotation marks, or 
        /// spaces are removed. Example: If <paramref name="parameterName" />="tag", 
        /// <paramref name="delimiter" />="," and the query string is "tag=misty morning,fox&amp;people=Toby", this method
        /// returns a string array { "misty morning", "fox" }.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <param name="delimiter">The delimiter to separate the query string value by. Default value is '+'.
        /// To specify '+' delimiter in the query string, it must be encoded as '%2B'.</param>
        /// <returns>Returns a string[] representing the value(s) of the specified query string parameter.</returns>
        /// <remarks>Do not call UrlDecode on the string, as it appears that .NET already does this.</remarks>
        public static string[] GetQueryStringParameterStrings(string parameterName, char delimiter = '+')
        {
            return ToArray(HttpContext.Current.Request.QueryString[parameterName], delimiter);
        }

        /// <summary>
        /// Splits the <paramref name="value" /> into an array based on the <paramref name="delimiter" />.
        /// Any leading or trailing apostrophes, quotation marks, or spaces are removed.
        /// </summary>
        /// <param name="value">The value to convert to an array.</param>
        /// <param name="delimiter">The delimiter to separate the <paramref name="value" /> by. Default value is '+'.
        /// </param>
        /// <returns>System.String[].</returns>
        public static string[] ToArray(string value, char delimiter = '+')
        {
            return (value == null ?
              null :
              value
                .Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim(new[] { '"', '\'', ' ' })).ToArray());
        }

        /// <summary>
        /// Retrieves the specified query string parameter value from the specified <paramref name="uri"/>. Returns 
        /// string.Empty if the parameter is not found.
        /// </summary>
        /// <param name="uri">The URI to search.</param>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <returns>Returns the value of the specified query string parameter found in the <paramref name="uri"/>.</returns>
        public static string GetQueryStringParameterString(Uri uri, string parameterName)
        {
            string parm = null;
            if (uri == null)
            {
                parm = HttpContext.Current.Request.QueryString[parameterName];
            }
            else
            {
                string qs = uri.Query.TrimStart(new char[] { '?' });
                foreach (string nameValuePair in qs.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] nameValue = nameValuePair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (nameValue.Length > 1)
                    {
                        if (String.Equals(nameValue[0], parameterName))
                        {
                            parm = nameValue[1];
                            break;
                        }
                    }
                }
            }

            if (parm == null)
            {
                return String.Empty;
            }
            else
            {
                return parm;
            }
        }

        /// <summary>
        /// Retrieves the specified query string parameter value from the query string. The values "true" and "1"
        /// are returned as true; any other value is returned as false. It is not case sensitive. The bool is not
        /// set if the parameter is not present in the query string (i.e. the HasValue property is false).
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter for which to retrieve it's value.</param>
        /// <returns>Returns the value of the specified query string parameter.</returns>
        public static bool? GetQueryStringParameterBoolean(string parameterName)
        {
            bool? parmValue = null;

            object parm = HttpContext.Current.Request.QueryString[parameterName];

            if (parm != null)
            {
                if ((parm.ToString().Equals("1", StringComparison.Ordinal)) || (parm.ToString().Equals("TRUE", StringComparison.OrdinalIgnoreCase)))
                {
                    parmValue = true;
                }
                else
                {
                    parmValue = false;
                }
            }

            return parmValue;
        }

        /// <summary>
        /// Adds the query string parameter to the <paramref name="uri" />.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="queryStringParameterNameValue">The query string parameter name value. Example: "moid=5"</param>
        /// <returns>An instance of <see cref="Uri" />.</returns>
        public static Uri AddQueryStringParameter(Uri uri, string queryStringParameterNameValue)
        {
            return new Uri(AddQueryStringParameter(uri.ToString(), queryStringParameterNameValue));
        }

        /// <summary>
        /// Append the string to the url as a query string parameter. If the <paramref name="url" /> already contains the
        /// specified query string parameter, it is replaced with the new one.
        /// Example:
        /// Url = "www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3"
        /// QueryStringParameterNameValue = "moid=27"
        /// Return value: www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27
        /// </summary>
        /// <param name="url">The Url to which the query string parameter should be added
        /// (e.g. www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3).</param>
        /// <param name="queryStringParameterNameValue">The query string parameter and value to add to the Url
        /// (e.g. "moid=27").</param>
        /// <returns>Returns a new Url containing the specified query string parameter.</returns>
        public static string AddQueryStringParameter(string url, string queryStringParameterNameValue)
        {
            if (String.IsNullOrEmpty(queryStringParameterNameValue))
                return url;

            string parmName = queryStringParameterNameValue.Substring(0, queryStringParameterNameValue.IndexOf("=", StringComparison.Ordinal));

            url = RemoveQueryStringParameter(url, parmName);

            string rv = url;

            if (url.IndexOf("?", StringComparison.Ordinal) < 0)
            {
                rv += "?" + queryStringParameterNameValue;
            }
            else
            {
                rv += "&" + queryStringParameterNameValue;
            }
            return rv;
        }

        /// <overloads>
        /// Remove a query string parameter from an URL.
        /// </overloads>
        /// <summary>
        /// Remove all query string parameters from the url.
        /// Example:
        /// Url = "www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27"
        /// Return value: www.galleryserverpro.com/index.aspx
        /// </summary>
        /// <param name="url">The Url containing the query string parameters to remove
        /// (e.g. www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27).</param>
        /// <returns>Returns a new Url with all query string parameters removed.</returns>
        public static string RemoveQueryStringParameter(string url)
        {
            return RemoveQueryStringParameter(url, String.Empty);
        }

        /// <summary>
        /// Remove the specified query string parameter from the url. Specify <see cref="String.Empty" /> for the
        /// <paramref name="queryStringParameterName" /> parameter to remove the entire set of parameters.
        /// Example:
        /// Url = "www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27"
        /// QueryStringParameterName = "msg"
        /// Return value: www.galleryserverpro.com/index.aspx?aid=5&amp;moid=27
        /// </summary>
        /// <param name="url">The Url containing the query string parameter to remove
        /// (e.g. www.galleryserverpro.com/index.aspx?aid=5&amp;msg=3&amp;moid=27).</param>
        /// <param name="queryStringParameterName">The query string parameter name to remove from the Url
        /// (e.g. "msg"). Specify <see cref="String.Empty" /> to remove the entire set of parameters.</param>
        /// <returns>Returns a new Url with the specified query string parameter removed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url" /> is null.</exception>
        public static string RemoveQueryStringParameter(string url, string queryStringParameterName)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            string newUrl;

            // Get the location of the question mark so we can separate the base url from the query string
            int separator = url.IndexOf("?", StringComparison.Ordinal);

            if (separator < 0)
            {
                // No query string exists on the url. Simply return the original url.
                newUrl = url;
            }
            else
            {
                // We have a query string to remove. Separate the base url from the query string, and process the query string.

                // Get the base url (e.g. "www.galleryserverpro.com/index.aspx")
                newUrl = url.Substring(0, separator);

                if (String.IsNullOrEmpty(queryStringParameterName))
                {
                    return newUrl;
                }

                newUrl += "?";

                string queryString = url.Substring(separator + 1);

                if (queryString.Length > 0)
                {
                    // Url has a query string. Split each name/value pair into a string array, and rebuild the
                    // query string, leaving out the parm passed to the function.
                    string[] queryItems = queryString.Split(new char[] { '&' });

                    for (int i = 0; i < queryItems.Length; i++)
                    {
                        if (!queryItems[i].StartsWith(queryStringParameterName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Query parm doesn't match, so include it as we rebuilt the new query string
                            newUrl += String.Concat(queryItems[i], "&");
                        }
                    }
                }
                // Trim any trailing '&' or '?'.
                newUrl = newUrl.TrimEnd(new char[] { '&', '?' });
            }

            return newUrl;
        }

        /// <summary>
        /// Returns a value indicating whether the specified query string parameter name is part of the query string. 
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter to check for.</param>
        /// <returns>Returns true if the specified query string parameter value is part of the query string; otherwise 
        /// returns false. </returns>
        public static bool IsQueryStringParameterPresent(string parameterName)
        {
            return (HttpContext.Current.Request.QueryString[parameterName] != null);
        }

        /// <summary>
        /// Returns a value indicating whether the specified query string parameter name is part of the query string
        /// of the <paramref name="uri"/>. 
        /// </summary>
        /// <param name="uri">The URI to check for the present of the <paramref name="parameterName">query string parameter name</paramref>.</param>
        /// <param name="parameterName">Name of the query string parameter.</param>
        /// <returns>Returns true if the specified query string parameter value is part of the query string; otherwise 
        /// returns false. </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri" /> is null.</exception>
        public static bool IsQueryStringParameterPresent(Uri uri, string parameterName)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            if (String.IsNullOrEmpty(parameterName))
                return false;

            return (uri.Query.Contains("?" + parameterName + "=") || uri.Query.Contains("&" + parameterName + "="));
        }

        /// <overloads>Remove all HTML tags from the specified string.</overloads>
        /// <summary>
        /// Remove all HTML tags from the specified string.
        /// </summary>
        /// <param name="html">The string containing HTML tags to remove.</param>
        /// <returns>Returns a string with all HTML tags removed.</returns>
        public static string RemoveHtmlTags(string html)
        {
            return RemoveHtmlTags(html, false);
        }

        /// <summary>
        /// Remove all HTML tags from the specified string. If <paramref name="escapeQuotes"/> is true, then all 
        /// apostrophes and quotation marks are replaced with &quot; and &apos; so that the string can be specified in HTML 
        /// attributes such as title tags. If the escapeQuotes parameter is not specified, no replacement is performed.
        /// </summary>
        /// <param name="html">The string containing HTML tags to remove.</param>
        /// <param name="escapeQuotes">When true, all apostrophes and quotation marks are replaced with &quot; and &apos;.</param>
        /// <returns>Returns a string with all HTML tags removed.</returns>
        public static string RemoveHtmlTags(string html, bool escapeQuotes)
        {
            return HtmlValidator.RemoveHtml(html, escapeQuotes);
        }

        /// <summary>
        /// Removes potentially dangerous HTML and Javascript in <paramref name="html"/>.
        /// When the current user is a gallery or site admin, no validation is performed and the 
        /// <paramref name="html" /> is returned without any processing. If the configuration
        /// setting <see cref="IGallerySettings.AllowUserEnteredHtml" /> is true, then the input is cleaned so that all 
        /// HTML tags that are not in a predefined list are HTML-encoded and invalid HTML attributes are deleted. If 
        /// <see cref="IGallerySettings.AllowUserEnteredHtml" /> is false, then all HTML tags are deleted. If the setting 
        /// <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true, then script tags and the text "javascript:"
        /// is allowed. Note that if script is not in the list of valid HTML tags defined in <see cref="IGallerySettings.AllowedHtmlTags" />,
        /// it will be deleted even when <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true. When the setting 
        /// is false, all script tags and instances of the text "javascript:" are deleted.
        /// </summary>
        /// <param name="html">The string containing the HTML tags.</param>
        /// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
        /// <returns>
        /// Returns a string with potentially dangerous HTML tags deleted.
        /// </returns>
        /// <remarks>TODO: Refactor this so that the Clean method knows whether the user is a gallery admin, rendering this
        /// function unnecessary. When this is done, update <see cref="GalleryObject.MetaRegEx" /> so that all meta items are
        /// passed to the Clean method.</remarks>
        public static string CleanHtmlTags(string html, int galleryId)
        {
            if (IsCurrentUserGalleryAdministrator(galleryId))
                return html;
            else
                return HtmlValidator.Clean(html, galleryId);
        }

        /// <summary>
        /// Returns the current version of Gallery Server.
        /// </summary>
        /// <returns>Returns a string representing the version (e.g. "1.0.0").</returns>
        public static string GetGalleryServerVersion()
        {
            string appVersion;
            object version = HttpContext.Current.Application["GalleryServerVersion"];
            if (version != null)
            {
                // Version was found in Application cache. Return.
                appVersion = version.ToString();
            }
            else
            {
                // Version was not found in application cache.
                appVersion = GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(HelperFunctions.GetGalleryServerVersion());

                HttpContext.Current.Application["GalleryServerVersion"] = appVersion;
            }

            return appVersion;
        }

        /// <summary>
        /// Truncate the specified string to the desired length. Any HTML tags that exist in the beginning portion
        /// of the string are preserved as long as no HTML tags exist in the part that is truncated.
        /// </summary>
        /// <param name="text">The string to be truncated. It may contain HTML tags.</param>
        /// <param name="maxLength">The maximum length of the string to be returned. If HTML tags are returned,
        /// their length is not counted - only the length of the "visible" text is counted.</param>
        /// <returns>Returns a string whose length - not counting HTML tags - does not exceed the specified length.</returns>
        public static string TruncateTextForWeb(string text, int maxLength)
        {
            // Example 1: Because no HTML tags are present in the truncated portion of the string, the HTML at the
            // beginning is preserved. (We know we won't be splitting up HTML tags, so we don't mind including the HTML.)
            // text = "Meet my <a href='http://www.cnn.com'>friend</a>. He works at the YMCA."
            // maxLength = 20
            // returns: "Meet my <a href='http://www.cnn.com'>friend</a>. He w"
            //
            // Example 2: The truncated portion has <b> tags, so all HTML is stripped. (This function isn't smart
            // enough to know whether it might be truncating in the middle of a tag, so it takes the safe route.)
            // text = "Meet my <a href='http://www.cnn.com'>friend</a>. He works at the <b>YMCA<b>."
            // maxLength = 20
            // returns: "Meet my friend. He w"
            if (text == null)
                return String.Empty;

            if (text.Length < maxLength)
                return text;

            // Remove all HTML tags from entire string.
            string cleanText = RemoveHtmlTags(text);

            // If the clean text length is less than our maximum, return the raw text.
            if (cleanText.Length <= maxLength)
                return text;

            // Get the text that will be removed.
            string cleanTruncatedPortion = cleanText.Substring(maxLength);

            // If the clean truncated text doesn't match the end of the raw text, the raw text must have HTML tags.
            bool truncatedPortionHasHtml = (!(text.EndsWith(cleanTruncatedPortion, StringComparison.OrdinalIgnoreCase)));

            string truncatedText;
            if (truncatedPortionHasHtml)
            {
                // Since the truncated portion has HTML tags, and we don't want to risk returning malformed HTML,
                // return text without ANY HTML.
                truncatedText = cleanText.Substring(0, maxLength);
            }
            else
            {
                // Since the truncated portion does not have HTML tags, we can safely return the first part of the
                // string, even if it has HTML tags.
                truncatedText = text.Substring(0, text.Length - cleanTruncatedPortion.Length);
            }
            return truncatedText;
        }

        /// <summary>
        /// Generates a pseudo-random 24 character string that can be as an encryption key.
        /// </summary>
        /// <returns>A pseudo-random 24 character string that can be as an encryption key.</returns>
        public static string GenerateNewEncryptionKey()
        {
            const int encryptionKeyLength = 24;
            const int numberOfNonAlphaNumericCharactersInEncryptionKey = 3;
            string encryptionKey = Membership.GeneratePassword(encryptionKeyLength, numberOfNonAlphaNumericCharactersInEncryptionKey);

            // An ampersand (&) is invalid, since it is used as an escape character in XML files. Replace any instances with an 'X'.
            return encryptionKey.Replace("&", "X");
        }

        /// <summary>
        /// HtmlEncodes a string using System.Web.HttpUtility.HtmlEncode().
        /// </summary>
        /// <param name="html">The text to HTML encode.</param>
        /// <returns>Returns <paramref name="html"/> as an HTML-encoded string.</returns>
        public static string HtmlEncode(string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        /// <summary>
        /// HtmlDecodes a string using System.Web.HttpUtility.HtmlDecode().
        /// </summary>
        /// <param name="html">The text to HTML decode.</param>
        /// <returns>Returns <paramref name="html"/> as an HTML-decoded string.</returns>
        public static string HtmlDecode(string html)
        {
            return HttpUtility.HtmlDecode(html);
        }

        /// <overloads>UrlEncodes a string using System.Uri.EscapeDataString().</overloads>
        /// <summary>
        /// UrlEncodes a string using System.Uri.EscapeDataString().
        /// </summary>
        /// <param name="text">The text to URL encode.</param>
        /// <returns>Returns <paramref name="text"/> as an URL-encoded string.</returns>
        public static string UrlEncode(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            return Uri.EscapeDataString(text);
        }

        /// <summary>
        /// Encodes the <paramref name="text" /> so that it can be assigned to a javascript variable.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <returns>Returns <paramref name="text" /> as an encoded string.</returns>
        public static string JsEncode(this string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            return text.Replace("\r\n", @"<br>").Replace(@"\", @"\\").Replace("'", @"\'").Replace(@"""", @"\""").Replace(@"<script>", @"<\script>").Replace(@"</script>", @"<\/script>");
        }

        /// <summary>
        /// UrlEncodes a string using System.Uri.EscapeDataString(), excluding the character specified in <paramref name="charNotToEncode"/>.
        /// This overload is useful for encoding URLs or file paths where the forward or backward slash is not to be encoded.
        /// </summary>
        /// <param name="text">The text to URL encode</param>
        /// <param name="charNotToEncode">The character that, if present in <paramref name="text"/>, is not encoded.</param>
        /// <returns>Returns <paramref name="text"/> as an URL-encoded string.</returns>
        public static string UrlEncode(string text, char charNotToEncode)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            string[] tokens = text.Split(new char[] { charNotToEncode });
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = UrlEncode(tokens[i]);
            }

            return String.Join(charNotToEncode.ToString(), tokens);
        }

        /// <summary>
        /// UrlDecodes a string using System.Uri.UnescapeDataString().
        /// </summary>
        /// <param name="text">The text to URL decode.</param>
        /// <returns>Returns text as an URL-decoded string.</returns>
        public static string UrlDecode(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            // Pre-process for + sign space formatting since System.Uri doesn't handle it
            // plus literals are encoded as %2b normally so this should be safe.
            text = text.Replace("+", " ");
            return Uri.UnescapeDataString(text);
        }

        /// <summary>
        /// Force the current application to recycle by updating the last modified timestamp on web.config.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when the application incorrectly calculates the current application's
        /// web.config file location.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the application does not have write permission to the
        /// current application's web.config file.</exception>
        /// <exception cref="NotSupportedException">Thrown when the path to the web.config file as calculated by the application is
        /// in an invalid format.</exception>
        public static void ForceAppRecycle()
        {
            File.SetLastWriteTime(WebConfigFilePath, DateTime.Now);
        }

        /// <summary>
        /// Excecute a maintenance routine to help ensure data integrity and eliminate unused data. The task is run on a background
        /// thread and this method returns immediately. No action is taken when app is in debug mode (debug="true" in web.config).
        /// Roles are synchronized between the membership system and the GSP roles. 
        /// Also, albums with owners that no longer exist are reset to not have an owner. This method is intended to be called 
        /// periodically; for example, once each time the application starts. Code in the Render method of the base class 
        /// <see cref="Pages.GalleryPage" /> is responsible for knowing when and how to invoke this method.
        /// </summary>
        /// <remarks>The background thread cannot access HttpContext.Current, so this method will probably fail under DotNetNuke.
        /// To fix that, figure out what DNN needs (portal ID?), and pass it in as a parameter.
        /// so that approach was replaced with this one.</remarks>
        public static void PerformMaintenance()
        {
            if (!IsDebugEnabled)
                Task.Factory.StartNew(PerformMaintenanceInternal);
        }

        /// <summary>
        /// Nulls out the cached value of <see cref="Utils.SkinPath" /> so that it is recalculated the next time the property is accessed.
        /// </summary>
        public static void RecalculateSkinPath()
        {
            _skinPath = null;
        }

        /// <summary>
        /// Gets the browser IDs for current request. In many cases this will be equal to HttpContext.Current.Request.Browser.Browsers.
        /// However, Internet Explorer versions 1 through 8 include the ID "ie1to8", which is added by Gallery Server. This allows
        /// the application to treat those versions differently than later versions. When HttpContext.Current is null, this function
        /// returns a one-item array containing "default".
        /// </summary>
        /// <returns>Returns the browser IDs for current request.</returns>
        public static Array GetBrowserIdsForCurrentRequest()
        {
            ArrayList browserIds = HttpContext.Current?.Request.Browser.Browsers ?? new ArrayList(new string[] { "default" });

            AddBrowserIdForInternetExplorer(browserIds);

            AddBrowserIdForChromeAndroid(browserIds);

            return browserIds.ToArray();
        }

        /// <summary>
        /// Determines whether the <paramref name="url" /> is an absolute URL rather than a relative one. An URL is considered absolute if
        /// it starts with "http" or "//".
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>
        /// 	<c>true</c> if the <paramref name="url" /> is absolute; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAbsoluteUrl(string url)
        {
            if (String.IsNullOrEmpty(url))
                return false;

            return (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || url.StartsWith("//", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the database file path from the connection string. Applies only to data providers that specify a file path
        /// in the connection string (SQLite, SQL CE). Returns null if no file path is found.
        /// </summary>
        /// <param name="cnString">The cn string.</param>
        /// <returns>Returns the full file path to the database file, or null if no file path is found.</returns>
        public static string GetDbFilePathFromConnectionString(string cnString)
        {
            // Ex: "data source=|DataDirectory|\GalleryServer_Data.sdf;Password =a@3!7f$dQ;"
            const string dataSourceKeyword = "data source";
            int dataSourceStartPos = cnString.IndexOf(dataSourceKeyword, StringComparison.OrdinalIgnoreCase) + dataSourceKeyword.Length + 1;

            if (dataSourceStartPos < 0)
                return null;

            int dataSourceLength = cnString.IndexOf(";", dataSourceStartPos, StringComparison.Ordinal) - dataSourceKeyword.Length;

            if (dataSourceLength < 0)
                dataSourceLength = cnString.Length - dataSourceStartPos;

            string cnFilePath = cnString.Substring(dataSourceStartPos, dataSourceLength).Replace("|DataDirectory|", "App_Data");

            string filePath = HelperFunctions.IsRelativeFilePath(cnFilePath) ? HttpContext.Current.Request.MapPath(cnFilePath) : cnFilePath;

            if (File.Exists(filePath))
                return filePath;
            else
                return null;
        }

        /// <summary>
        /// Serializes the <paramref name="item" /> as JSON.
        /// </summary>
        /// <param name="item">The object to serialize.</param>
        /// <returns>Returns a string that is a JSON-encoded representation of <paramref name="item" />.</returns>
        /// <remarks>If the results of this function are to be sent to the browser in javascript, the slash (\)
        /// and apostrophe (') characters should be escaped. Do this by adding the following code the return
        /// value: <code>.Replace(@"\", @"\\").Replace("'", @"\'")</code>.</remarks>
        public static string ToJson(this object item)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(item);

            // Old way:
            //DataContractJsonSerializer ser = new DataContractJsonSerializer(item.GetType());
            //MemoryStream ms = new MemoryStream();

            //ser.WriteObject(ms, item);
            //return Encoding.UTF8.GetString(ms.ToArray());
        }

        /// <summary>
        /// Converts the specified <paramref name="json">JSON string</paramref> to the requested object.
        /// </summary>
        /// <typeparam name="T">The type of object to convert the JSON to.</typeparam>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>An instance of T.</returns>
        /// <exception cref="InvalidCastException">Thrown when the JSON string cannot be cast to the 
        /// requested type.</exception>
        /// <exception cref="ArgumentException">Thrown when the string is not valid JSON.</exception>
        public static T FromJson<T>(this string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);

            // Old way:
            //using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            //{
            //	DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            //	return (T)ser.ReadObject(ms);
            //}
        }

        /// <summary>
        /// Adds the <paramref name="results" /> to the current user's session. If an object already exists,
        /// the results are added to the existing collection. No action is taken if the session is unavailable. 
        /// The session object is given the name stored in <see cref="GlobalConstants.SkippedFilesDuringUploadSessionKey" />.
        /// </summary>
        /// <param name="results">The results to store in the user's session.</param>
        public static void AddResultToSession(IEnumerable<ActionResult> results)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return;

            var objResults = HttpContext.Current.Session[GlobalConstants.SkippedFilesDuringUploadSessionKey] as string;

            var uploadResults = (objResults == null ? new List<ActionResult>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<ActionResult>>(objResults));

            lock (uploadResults)
            {
                uploadResults.AddRange(results);
                HttpContext.Current.Session[GlobalConstants.SkippedFilesDuringUploadSessionKey] = Newtonsoft.Json.JsonConvert.SerializeObject(uploadResults);
            }
        }

        /// <summary>
        /// Generates a list of key/value pairs for the specified <paramref name="enumeration" /> where the key  is the
        /// enumeration value and the value is a friendly, human readable description. The value is assigned from a language resource
        /// value if it exists; otherwise, the string representation of the value is returned. The language resource key must
        /// be in this format: "Enum_{EnumTypeName}_{EnumValue}". For example, the expected resource key for the enum value
        /// JQueryTemplateType.Album is "Enum_JQueryTemplateType_Album".
        /// </summary>
        /// <param name="enumeration">An enumeration from which to generate a collection of key/value pairs.</param>
        /// <returns>Returns an enumerable list of key/value pairs.</returns>
        public static IEnumerable<KeyValuePair<string, string>> GetEnumList(Type enumeration)
        {
            Array enumNames = Enum.GetNames(enumeration);
            List<KeyValuePair<string, string>> items = new List<KeyValuePair<string, string>>(enumNames.Length);

            foreach (string enumName in enumNames)
            {
                string resourceKey = String.Concat("Enum_", enumeration.Name, "_", enumName);

                string resDesc = Resources.GalleryServer.ResourceManager.GetString(resourceKey, CultureInfo.CurrentCulture);

                items.Add(new KeyValuePair<string, string>(enumName, resDesc ?? enumName));
            }

            return items;
        }

        ///// <summary>
        ///// Gets a friendly, human readable description of the enumeration <paramref name="value" />. If a language resource
        ///// exists, it is returned; otherwise, the string representation of the value is returned. The language resource key must
        ///// be in this format: "Enum_{EnumTypeName}_{EnumValue}". For example, the expected resource key for the enum value
        ///// JQueryTemplateType.Album is "Enum_JQueryTemplateType_Album".
        ///// </summary>
        ///// <param name="value">An enumeration value.</param>
        ///// <returns>Returns a friendly, human readable description of the enumeration <paramref name="value" />.</returns>
        //public static string GetDescription(this Enum value)
        //{
        //	string resourceKey = String.Concat("Enum_", value.GetType().Name, "_", value.ToString());

        //	return Resources.GalleryServer.ResourceManager.GetString(resourceKey, CultureInfo.CurrentCulture) ?? value.ToString();
        //}

        /// <summary>
        /// Gets a <see cref="StringContent" /> instance with details about the specified <paramref name="ex" />. Returns a generic 
        /// message when debug="false" in web.config; returns the exception message when debug="true".
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>An instance of <see cref="StringContent" />.</returns>
        public static StringContent GetExStringContent(Exception ex)
        {
            var msg = "An error occurred on the server. Check the gallery's event log for details. ";

            if (IsDebugEnabled)
            {
                msg += String.Concat(ex.GetType(), ": ", ex.Message);
            }

            return new StringContent(msg);
        }

        /// <summary>
        /// Returns the username from the parameter, leaving out the domain or computer name if present.
        /// For example, if <paramref name="userName" />="mydomain\Vitali", this function returns "Vitali". If the parameter
        /// does not contain a backward slash ("\"), then <paramref name="userName" /> is returned unmodified. If web.config contains
        /// an application setting SuppressUserNameParsingFromHttpContextIdentity and it is <c>true</c>, then return the parameter
        /// unmodified (thus giving same behavior as 3.2.1 and earlier versions). This setting will be necessary in cases where an
        /// admin has user names containing a backward slash.
        /// 
        /// </summary>
        /// <param name="userName">Name of the user. Examples: "Vitali", "mydomain\Vitali", "pcname\Vitali"</param>
        /// <returns>System.String.</returns>
        public static string ParseUserName(string userName)
        {
            bool dontParse;
            if (Boolean.TryParse(System.Web.Configuration.WebConfigurationManager.AppSettings["SuppressUserNameParsingFromHttpContextIdentity"], out dontParse) && dontParse)
            {
                return userName;
            }

            var idx = userName.IndexOf("\\", StringComparison.Ordinal);
            return (idx >= 0 ? userName.Substring(idx + 1) : userName);
        }

        /// <summary>
        /// Calculates the width and height based on a <paramref name="ratio" /> and the <paramref name="maxLength" /> of one of the sides.
        /// Useful for calculating the size of an empty album thumbnail image.
        /// </summary>
        /// <param name="ratio">The width to height ratio. Example: If the ratio is 1.33, the calculated width will 1.33 times longer than the height.</param>
        /// <param name="maxLength">The length (in pixels) of the longest edge. Example: If <paramref name="maxLength" /> is 115 and the <paramref name="ratio" />
        /// is 1.33, the resulting width will be 115 and the height will be 86.</param>
        /// <returns>An instance of <see cref="System.Drawing.Size" />.</returns>
        public static Size CalculateSize(float ratio, int maxLength)
        {
            if (ratio > 1)
            {
                return new Size(maxLength, Convert.ToInt32((float)maxLength / ratio));
            }
            else
            {
                return new Size(Convert.ToInt32((float)maxLength * ratio), maxLength);
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Calculates the path, relative to the web site root, to the directory containing the Gallery Server user 
        /// controls and other resources. Does not include the default page or the trailing slash. Ex: /dev/gsweb/gsp
        /// </summary>
        /// <returns>Returns the path to the directory containing the Gallery Server user controls and other resources.</returns>
        private static string CalculateGalleryRoot()
        {
            string appPath = AppRoot;
            string galleryPath = GetGalleryResourcesPath().TrimEnd(new char[] { Path.DirectorySeparatorChar, '/' });

            if (!String.IsNullOrEmpty(galleryPath))
            {
                galleryPath = galleryPath.Replace("\\", "/");

                if (!galleryPath.StartsWith("/", StringComparison.Ordinal))
                    galleryPath = String.Concat("/", galleryPath); // Make sure it starts with a '/'

                appPath = String.Concat(appPath, galleryPath.TrimEnd('/'));
            }

            return appPath;
        }

        /// <summary>
        /// Gets the path, relative to the current application, to the directory containing the Gallery Server
        /// resources such as images, user controls, scripts, etc. This value is pulled from the AppSettings value "GalleryResourcesPath"
        /// if present; otherwise it defaults to "gs". Examples: "gs", "GalleryServer\resources"
        /// </summary>
        /// <returns>Returns the path, relative to the current application, to the directory containing the Gallery Server
        /// resources such as images, user controls, scripts, etc.</returns>
        private static String GetGalleryResourcesPath()
        {
            return ConfigurationManager.AppSettings["GalleryResourcesPath"] ?? "gs";
        }

        /// <summary>
        /// When the current browser is Internet Explorer 1 to 8, add a "ie1to8" element to <paramref name="browserIds" />.
        /// </summary>
        /// <param name="browserIds">The browser IDs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="browserIds" /> is null.</exception>
        private static void AddBrowserIdForInternetExplorer(ArrayList browserIds)
        {
            if (browserIds == null)
                throw new ArgumentNullException("browserIds");

            HttpBrowserCapabilities browserCaps = HttpContext.Current?.Request.Browser;

            if ((browserCaps != null) && (browserCaps.Browser != null) && browserCaps.Browser.Equals("IE", StringComparison.OrdinalIgnoreCase))
            {
                const string browserIdForIE1to8 = "ie1to8";
                decimal version;
                if (Decimal.TryParse(browserCaps.Version, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out version) && (version < (decimal)9.0) && (!browserIds.Contains(browserIdForIE1to8)))
                {
                    browserIds.Add(browserIdForIE1to8);
                }
            }
        }

        /// <summary>
        /// When the current browser is Chrome running on Android, add a "chromeandroid" element to <paramref name="browserIds" />.
        /// </summary>
        /// <param name="browserIds">The browser IDs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="browserIds" /> is null.</exception>
        private static void AddBrowserIdForChromeAndroid(ArrayList browserIds)
        {
            if (browserIds == null)
                throw new ArgumentNullException("browserIds");

            HttpBrowserCapabilities browserCaps = HttpContext.Current?.Request.Browser;

            if ((browserCaps != null) && (browserCaps.Browser != null) && browserCaps.Browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase))
            {
                const string browserIdForChromeAndroid = "chromeandroid";
                if (HttpContext.Current.Request.UserAgent != null && HttpContext.Current.Request.UserAgent.Contains("Android"))
                {
                    browserIds.Add(browserIdForChromeAndroid);
                }
            }
        }

        private static void PerformMaintenanceInternal()
        {
            bool mustRunMaintenance = false;

            lock (_sharedLock)
            {
                if (AppSetting.Instance.MaintenanceStatus == MaintenanceStatus.NotStarted)
                {
                    mustRunMaintenance = true;
                    AppSetting.Instance.MaintenanceStatus = MaintenanceStatus.InProgress;
                }
            }

            if (mustRunMaintenance)
            {
                try
                {
                    AppEventController.LogEvent("Maintenance routine has started on a background thread.");

                    HelperFunctions.BeginTransaction();

                    Factory.ValidateGalleries();

                    // Make sure the list of ASP.NET roles is synchronized with the Gallery Server roles.
                    RoleController.ValidateRoles();

                    RoleController.RemoveMissingRolesFromDefaultRolesForUsersSettings();

                    RoleController.ValidateUsersAreInDefaultRolesForUsers();

                    MediaConversionQueue.Instance.DeleteOldQueueItems();

                    DeleteSampleSourceFiles();

                    HelperFunctions.CommitTransaction();

                    AppSetting.Instance.MaintenanceStatus = MaintenanceStatus.Complete;

                    AppEventController.LogEvent("Maintenance routine complete.");
                }
                catch (Exception ex)
                {
                    HelperFunctions.RollbackTransaction();
                    AppEventController.LogError(ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete the sample media asset files in the App_Data directory. These should have been removed when the gallery was first installed, but we check
        /// here just to help keep things nice and clean.
        /// </summary>
        private static void DeleteSampleSourceFiles()
        {
            foreach (var sampleAssetFileName in Constants.SAMPLE_ASSET_FILENAMES)
            {
                var sourceFilePath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.AppDataDirectory, sampleAssetFileName);

                try
                {
                    File.Delete(sourceFilePath);
                }
                catch (Exception ex)
                {
                    // IIS account identity doesn't have permission to delete the file. Tell user to it manually.
                    ex.Data.Add("Info", $"The sample media asset source file is no longer needed, but we could not automatically delete it. To prevent this message from appearing again, delete the file at {sourceFilePath}.");
                    AppEventController.LogError(ex);
                }
            }
        }

        #endregion
    }
}
