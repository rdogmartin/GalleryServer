using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// A client-optimized object that stores application-level properties for the gallery.
  /// </summary>
  public class App
  {
    /// <summary>
    /// Gets the path, relative to the current application, to the directory containing the Gallery Server
    /// resources such as images, user controls, scripts, etc. Examples: "gs", "GalleryServer\resources"
    /// </summary>
    /// <value>
    /// The gallery resources path.
    /// </value>
    public string GalleryResourcesPath { get; set; }

    /// <summary>
    /// Gets the name of the current skin. Examples: "light", "dark"
    /// </summary>
    /// <value>The skin name.</value>
    public string Skin { get; set; }

    /// <summary>
    /// Gets the path, relative to the current application, to the directory containing the Gallery Server
    /// skin resources for the currently selected skin. Examples: "gs/skins/simple-grey", "/dev/gallery/gsp/skins/simple-grey"
    /// </summary>
    /// <value>The skin path.</value>
    public string SkinPath { get; set; }

    /// <summary>
    /// Gets the URL, relative to the website root and without any query string parameters, 
    /// to the current page. Example: "/dev/gs/gallery.aspx"
    /// </summary>
    /// <value>
    /// The current page URL.
    /// </value>
    public string CurrentPageUrl { get; set; }

    /// <summary>
    /// Get the URI scheme, DNS host name or IP address, and port number for the current application. 
    /// Examples: http://www.site.com, http://localhost, http://127.0.0.1, http://godzilla
    /// </summary>
    /// <value>The URL to the current web host.</value>
    public string HostUrl { get; set; }

    /// <summary>
    /// Gets the URL to the current web application. Does not include the containing page or the trailing slash. 
    ///  Example: If the gallery is installed in a virtual directory 'gallery' on domain 'www.site.com', this 
    /// returns 'http://www.site.com/gallery'.
    /// </summary>
    /// <value>The URL to the current web application.</value>
    public string AppUrl { get; set; }

    /// <summary>
    /// Gets the URL to the list of recently added media objects. Requires trial mode or a Home &amp; Nonprofit edition or higher license;
    /// otherwise it will be null. Ex: http://site.com/gallery/default.aspx?latest=50
    /// </summary>
    /// <value>The URL to the list of recently added media objects.</value>
    public string LatestUrl { get; set; }

    /// <summary>
    /// Gets the URL to the list of top rated media objects. Requires trial mode or a Home &amp; Nonprofit edition or higher license;
    /// otherwise it will be null. Ex: http://site.com/gallery/default.aspx?latest=50
    /// </summary>
    /// <value>The URL to the list of top rated media objects.</value>
    public string TopRatedUrl { get; set; }

    /// <summary>
    /// Gets a value indicating whether gallery administrators are allowed to create, edit, and delete users and roles.
    /// </summary>
    public bool AllowGalleryAdminToManageUsersAndRoles { get; set; }

    /// <summary>
    /// Gets the license applied to the current application.
    /// </summary>
    public Business.LicenseLevel License { get; set; }

    /// <summary>
    /// Gets a value indicating whether the app is in debug mode. That is, it returns <c>true</c> when 
    /// debug = "true" in web.config and returns <c>false</c> when debug = "false".
    /// </summary>
    public bool IsDebugEnabled { get; set; }
  }
}