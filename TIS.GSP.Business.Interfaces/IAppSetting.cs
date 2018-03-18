using System;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Represents application level settings.
  /// </summary>
  public interface IAppSetting
  {
    /// <summary>
    /// Gets or sets the name of the skin.
    /// </summary>
    /// <value>The name of the skin.</value>
    string Skin { get; set; }

    /// <summary>
    /// Gets or sets the size of each block of bytes when transferring files to streams and vice versa. This property was originally
    /// created to specify the buffer size for downloading a media object to the client, but it is now used for all
    /// file/stream copy operations.
    /// </summary>
    int MediaObjectDownloadBufferSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether security-sensitive portions of the URL to the media object are encrypted when it is sent 
    /// to the client browser. When false, the URL to the media object is sent in plain text, such as 
    /// "handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
    /// These URLs can be seen by viewing the source of the HTML page. From this URL one can determine the album ID 
    /// for this media object is 8, (aid=8), the file path to the media object on the server is 
    /// C:\gs\mypics\birthday.jpeg, and the requested image is a thumbnail (dt=1, where 1 is the value of the 
    /// GalleryServer.Business.DisplayObjectType enumeration for a thumbnail). For enhanced security, this property should
    /// be true, which uses Triple DES encryption to encrypt the the query string.
    /// It is recommended to set this to true except when you are	troubleshooting and it is useful to see the 
    /// filename and path in the HTML source. The Triple DES algorithm uses the secret key specified in the 
    /// <see cref="EncryptionKey" /> property.
    /// </summary>
    bool EncryptMediaObjectUrlOnClient { get; set; }

    /// <summary>
    /// Gets or sets the secret key used for the Triple DES algorithm. Applicable when the property <see cref="EncryptMediaObjectUrlOnClient" /> = true.
    /// The string must be 24 characters in length and be sufficiently strong so that it cannot be easily cracked.
    /// An exception is thrown by the .NET Framework if the key is considered weak. Change this to a value known only
    /// to you to prevent others from being able to decrypt.
    /// </summary>
    string EncryptionKey { get; set; }

    /// <summary>
    /// Gets or sets the absolute or relative path to the jQuery script file as stored in the application settings table.
    /// A relative path must be relative to the root of the web application and start with a tilde ("~"). An absolute path must be a full URI
    /// (e.g. http://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js, //ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js).
    /// It is not valid to specify a UNC path, mapped drive path, or path to the local file system (e.g. "C:\scripts\jquery.js").
    /// Specify an empty string to indicate to GSP that the containing application is responsible for adding the jQuery
    /// reference. In this case, GSP will not attempt to add a jQuery reference.  Guaranteed to not return null.
    /// </summary>
    /// <value>The absolute or relative path to the jQuery script file as stored in the application settings table.</value>
    /// <remarks>The path is returned exactly how it appears in the database.</remarks>
    string JQueryScriptPath { get; set; }
    
    /// <summary>
    /// Gets or sets the absolute or relative path to the jQuery Migrate script file as stored in the application settings table.
    /// The jQuery Migrate Plugin is used to provide backwards compatibility when using jQuery 1.9 and higher.
    /// A relative path must be relative to the root of the web application and start with a tilde ("~"). An absolute path must be a full URI
    /// (e.g. http://code.jquery.com/jquery-migrate-1.2.1.min.js, //code.jquery.com/jquery-migrate-1.0.0.min.js).
    /// It is not valid to specify a UNC path, mapped drive path, or path to the local file system (e.g. "C:\scripts\jquery.js").
    /// Specify an empty string when the migrate plugin should not be used.  Guaranteed to not return null.
    /// </summary>
    /// <value>The absolute or relative path to the jQuery Migrate script file as stored in the application settings table.</value>
    /// <remarks>The path is returned exactly how it appears in the database.</remarks>
    string JQueryMigrateScriptPath { get; set; }

    /// <summary>
    /// Gets or sets the absolute or relative path to the jQuery UI script file as stored in the application settings table.
    /// A relative path must be relative to the root of the web application and start with a tilde ("~"). An absolute path must be a full URI
    /// (e.g. http://ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js.
    /// It is not valid to specify a UNC path, mapped drive path, or path to the local file system (e.g. "C:\scripts\jquery.js").
    /// Specify an empty string to indicate to GSP that the containing application is responsible for adding the jQuery UI
    /// reference. In this case, GSP will not attempt to add a jQuery reference. Guaranteed to not return null.
    /// </summary>
    /// <value>The absolute or relative path to the jQuery UI script file as stored in the application settings table.</value>
    /// <remarks>The path is returned exactly how it appears in the database.</remarks>
    string JQueryUiScriptPath { get; set; }

    /// <summary>
    /// Gets or sets the absolute or relative path to the *directory* containing the ImageMagick convert.exe application. Setting this property
    /// automatically causes <see cref="ImageMagickPathResolved" /> to be recalculated.
    /// A relative path must be relative to the root of the web application and start with a tilde ("~"). An absolute path must be a full 
    /// path starting with a drive letter. When possible, the administrator should specify the path to the full ImageMagick installation, 
    /// as it has more capability than just having convert.exe in the bin directory. Callers should generally use <see cref="ImageMagickPathResolved" />
    /// rather than this property since that one has validation to ensure the application exists.
    /// Examples: "C:\Program Files\ImageMagick-6.9.3-Q16", "\bin"
    /// </summary>
    /// <value>A <see cref="string" /> representing the absolute or relative path to *directory* containing the ImageMagick convert.exe application.</value>
    string ImageMagickPath { get; set; }

    /// <summary>
    /// Gets or sets the absolute path to the ImageMagick convert.exe application. This property is calculated based on <see cref="ImageMagickPath" />.
    /// Will be <see cref="string.Empty" /> when <see cref="ImageMagickPath" /> is invalid, such as when convert.exe has not yet been copied
    /// to the bin directory. Examples: "C:\Program Files\ImageMagick-6.9.3-Q16\convert.exe", "C:\Dev\GS\Dev-Main\Website\bin\convert.exe"
    /// </summary>
    /// <value>A <see cref="string" /> representing the absolute path to the ImageMagick convert.exe application, or <see cref="string.Empty" />
    /// when convert.exe is not present.</value>
    string ImageMagickPathResolved { get; }

    /// <summary>
    /// Gets the data store currently being used for gallery data.
    /// </summary>
    /// <value>An instance of <see cref="ProviderDataStore" />.</value>
    ProviderDataStore ProviderDataStore { get; }

    /// <summary>
    /// Gets or sets the name of the Membership provider for the gallery users. Optional. When not specified, the default provider specified
    /// in web.config is used.
    /// </summary>
    /// <remarks>The name of the Membership provider for the gallery users.</remarks>
    string MembershipProviderName { get; set; }

    /// <summary>
    /// Gets or sets the name of the Role provider for the gallery users. Optional. When not specified, the default provider specified
    /// in web.config is used.
    /// </summary>
    /// <remarks>The name of the Role provider for the gallery users.</remarks>
    string RoleProviderName { get; set; }

    /// <summary>
    /// Gets or sets the license for the current application.
    /// </summary>
    /// <value>The license for the current application.</value>
    ILicense License { get; set; }

    /// <summary>
    /// Gets or sets the license key for this installation of Gallery Server.
    /// </summary>
    string LicenseKey { get; set; }

    /// <summary>
    /// Gets or sets the email used to purchase the license key for this installation of Gallery Server.
    /// </summary>
    string LicenseEmail { get; set; }

    /// <summary>
    /// Gets or sets the version key for this installation of Gallery Server.
    /// </summary>
    string VersionKey { get; set; }

    /// <summary>
    /// Gets or sets the instance ID assigned by the license activation server. Required in order to deactivate a license at a particular installation.
    /// </summary>
    string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to store objects in a cache for quicker retrieval. This significantly improves
    /// performance, but cannot be used in web farms because the cache is local to each server and there is not a cross-server 
    /// mechanism to expire the cache.</summary>
    bool EnableCache { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gallery administrators are allowed to create, edit, and delete users and roles.</summary>
    bool AllowGalleryAdminToManageUsersAndRoles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gallery administrators are allowed to see users and roles that do not have 
    /// access to current gallery.</summary>
    bool AllowGalleryAdminToViewAllUsersAndRoles { get; set; }

    /// <summary>
    /// Indicates the maximum number of error objects to persist to the data store. When the number of errors exceeds this
    /// value, the oldest item is purged to make room for the new item. A value of zero means no limit is enforced.
    /// </summary>
    int MaxNumberErrorItems { get; set; }

    /// <summary>
    /// The name associated with the <see cref="EmailFromAddress" /> email address. Emails sent from Gallery Server 
    /// will appear to be sent from this person.
    /// </summary>
    string EmailFromName { get; set; }

    /// <summary>
    /// The email address associated with <see cref="EmailFromName" />. Emails sent from Gallery Server 
    /// will appear to be sent from this email address.
    /// </summary>
    string EmailFromAddress { get; set; }

    /// <summary>
    /// Specifies the IP address or name of the SMTP server used to send emails. (Examples: 127.0.0.1, 
    /// Godzilla, mail.yourisp.com) This value will override the SMTP server setting that may be in the 
    /// system.net mailSettings section of the web.config file (either explicitly or inherited from a 
    /// parent web.config file). Leave this setting blank to use the value in web.config or if you are 
    /// not using the email functionality.
    /// </summary>
    string SmtpServer { get; set; }

    /// <summary>
    /// Specifies the SMTP server port number used to send emails. This value will override the SMTP 
    /// server port setting that may be in the system.net mailSettings section of the web.config file 
    /// (either explicitly or inherited from a parent web.config file). Leave this setting blank to 
    /// use the value in web.config or if you are not using the email functionality. Defaults to 25 
    /// if not specified here or in web.config.
    /// </summary>
    string SmtpServerPort { get; set; }

    /// <summary>
    /// Specifies whether e-mail functionality uses Secure Sockets Layer (SSL) to encrypt the connection.
    /// </summary>
    bool SendEmailUsingSsl { get; set; }

    /// <summary>
    /// Gets or sets the custom CSS an administrator may have provided.
    /// </summary>
    string CustomCss { get; set; }

    /// <summary>
    /// Gets the physical application path of the currently running application. For web applications this will be equal to
    /// the Request.PhysicalApplicationPath property.
    /// </summary>
    string PhysicalApplicationPath { get; }

    /// <summary>
    /// Gets the trust level of the currently running application. 
    /// </summary>
    ApplicationTrustLevel AppTrustLevel { get; }

    /// <summary>
    /// Gets the name of the currently running application. Default is "Gallery Server".
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// Gets the path, relative to the current application, to the directory containing the Gallery Server
    /// resources such as images, user controls, scripts, etc. When setting the property, the following scrubbing occurs: (a) leading
    /// or trailing slashes are removed, (b) forward slashes ('/') are replaced with path 
    /// separator characters (i.e. the backward slash '\'). Examples: "gs", "GalleryServer\resources"
    /// </summary>
    /// <value>A string.</value>
    string GalleryResourcesPath { get; }

    /// <summary>
    /// Gets the full physical path to the directory where files can be temporarily stored. Example:
    /// "C:\inetpub\wwwroot\galleryserverpro\App_Data\_Temp"
    /// </summary>
    string TempUploadDirectory { get; }

    /// <summary>
    /// Gets or sets the maintenance status. During each application restart a maintenance routine is run that helps
    /// ensure data integrity and eliminate unused data. This property describes the status of the maintenance routine.
    /// </summary>
    /// <value>The maintenance status.</value>
    MaintenanceStatus MaintenanceStatus { get; set; }

    /// <summary>
    /// Gets the UTC date/time this gallery was installed on the web server.
    /// </summary>
    /// <value>The install date.</value>
    DateTime InstallDate { get; }

    /// <summary>
    /// Gets or sets the UTC date/time this gallery was installed on the web server. The value is returned as it is 
    /// stored in the data store. That is, as an encrypted string. It can be decrypted using HelperFunctions.Decrypt().
    /// </summary>
    /// <value>The encrypted install date.</value>
    string InstallDateEncrypted { get; set; }

    /// <summary>
    /// Gets the .NET Framework version the current application is running under. Contains only the major and minor components.
    /// </summary>
    /// <value>The .NET Framework version the current application is running under.</value>
    /// <example>
    /// To verify the current application is running 3.0 or higher, use this:
    /// <code>
    /// if (AppSetting.Instance.DotNetFrameworkVersion > new Version("2.0"))
    /// { /* App is 3.0 or higher */ }
    /// </code>
    /// </example>
    Version DotNetFrameworkVersion { get; }

    /// <summary>
    /// Gets the IIS application pool identity.
    /// </summary>
    /// <value>The application app pool identity.</value>
    string IisAppPoolIdentity { get; }

    /// <summary>
    /// Gets the full file path to the FFmpeg utility. During application initialization the bin directory is inspected for the
    /// presence of ffmpeg.exe. If present, this property is assigned the value of the full path to the utility. If not present,
    /// the property is assigned <see cref="string.Empty" />. FFmpeg is used to extract thumbnails from videos and for video conversion.
    /// Example: C:\inetpub\wwwroot\gallery\bin\ffmpeg.exe
    /// </summary>
    /// <value>
    /// 	Returns the full file path to the FFmpeg utility, or <see cref="string.Empty" /> if the utility is not present.
    /// </value>
    string FFmpegPath { get; }

    /// <summary>
    /// Gets or sets the version of the objects in the database as reported by the database. Ex: "2.4.1"
    /// </summary>
    /// <value>The version of the objects in the database as reported by the database.</value>
    string DataSchemaVersion { get; set; }

    /// <summary>
    /// Gets a value indicating whether the current library has been populated with data from the calling assembly.
    /// This library is initialized by calling <see cref="Initialize" />.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets or sets a value indicating whether an installation is being requested. This value will be <c>true</c> when a text
    /// file named install.txt is detected in the App_Data directory. This property may be set during application initialization 
    /// so that later in the code path, when the gallery ID is available, the objects can be created.
    /// </summary>
    /// <value><c>true</c> if an installation is being requested; otherwise, <c>false</c>.</value>
    bool InstallationRequested { get; set; }

    /// <summary>
    /// Assign various application-wide properties to be used during the lifetime of the application. This method
    /// should be called once when the application first starts.
    /// </summary>
    /// <param name="trustLevel">The trust level of the current application.</param>
    /// <param name="physicalAppPath">The physical path of the currently executing application. For web applications
    /// this will be equal to the Request.PhysicalApplicationPath property.</param>
    /// <param name="appName">The name of the currently running application.</param>
    /// <param name="galleryResourcesPath">The path, relative to the current application, to 
    /// the directory containing the Gallery Server resources such as images, user controls, 
    /// scripts, etc. Examples: "gs", "GalleryServer\resources"</param>
    /// <exception cref="System.InvalidOperationException">Thrown when this method is called more than once during
    /// the application's lifetime.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the trustLevel parameter has the value
    /// ApplicationTrustLevel.None.</exception>
    /// <exception cref="System.ArgumentNullException">Thrown if any parameters are null or empty.</exception>
    void Initialize(ApplicationTrustLevel trustLevel, string physicalAppPath, string appName, string galleryResourcesPath);

    /// <summary>
    /// Persist the specified application settings to the data store.
    /// </summary>
    void Save();

    ///// <summary>
    ///// Persist the specified application settings to the data store. Specify a null value for each parameter whose value is
    ///// not changing.
    ///// </summary>
    ///// <param name="license">A license instance containing the license key for this installation of Gallery Server. The
    ///// license key must be validated before invoking this method.</param>
    ///// <param name="skin">The name of the skin.</param>
    ///// <param name="mediaObjectDownloadBufferSize">The size of each block of bytes when transferring files to streams and vice versa.</param>
    ///// <param name="encryptMediaObjectUrlOnClient">Indicates whether security-sensitive portions of the URL to the media object are
    ///// encrypted when it is sent to the client browser.</param>
    ///// <param name="encryptionKey">The secret key used for the Triple DES algorithm.</param>
    ///// <param name="jQueryScriptPath">The absolute or relative path to the jQuery script file.</param>
    ///// <param name="jQueryMigrateScriptPath">The absolute or relative path to the jQuery Migrate script file.</param>
    ///// <param name="jQueryUiScriptPath">The absolute or relative path to the jQuery UI script file.</param>
    ///// <param name="imageMagickPath">The absolute or relative path to the directory containing the ImageMagick convert.exe application.</param>
    ///// <param name="membershipProviderName">The name of the Membership provider for the gallery users.</param>
    ///// <param name="roleProviderName">The name of the Role provider for the gallery users.</param>
    ///// <param name="enableCache">Indicates whether to store objects in a cache for quicker retrieval.</param>
    ///// <param name="allowGalleryAdminToManageUsersAndRoles">Indicates whether gallery administrators are allowed to create, edit, and delete
    ///// users and roles.</param>
    ///// <param name="allowGalleryAdminViewAllUsersAndRoles">Indicates whether gallery administrators are allowed to see users and roles that
    ///// do not have access to current gallery.</param>
    ///// <param name="maxNumberErrorItems">The maximum number of error objects to persist to the data store.</param>
    ///// <param name="emailFromName">The name associated with the <paramref name="emailFromAddress" /> email address. Emails sent from Gallery Server
    ///// will appear to be sent from this person.</param>
    ///// <param name="emailFromAddress">The email address associated with <paramref name="emailFromName" />. Emails sent from Gallery Server
    ///// will appear to be sent from this email address.</param>
    ///// <param name="smtpServer">Specifies the IP address or name of the SMTP server used to send emails. (Examples: 127.0.0.1,
    ///// Godzilla, mail.yourisp.com)</param>
    ///// <param name="smtpServerPort">Specifies the SMTP server port number used to send emails.</param>
    ///// <param name="sendEmailUsingSsl">Specifies whether e-mail functionality uses Secure Sockets Layer (SSL) to encrypt the connection.</param>
    ///// <param name="customCSS">The custom CSS an administrator wants to apply in addition to the default CSS.</param>
    //void Save(ILicense license, string skin, int? mediaObjectDownloadBufferSize, bool? encryptMediaObjectUrlOnClient, string encryptionKey, string jQueryScriptPath, string jQueryMigrateScriptPath, string jQueryUiScriptPath, string imageMagickPath, string membershipProviderName, string roleProviderName, bool? enableCache, bool? allowGalleryAdminToManageUsersAndRoles, bool? allowGalleryAdminViewAllUsersAndRoles, int? maxNumberErrorItems, string emailFromName, string emailFromAddress, string smtpServer, string smtpServerPort, bool? sendEmailUsingSsl, string customCSS);
  }
}