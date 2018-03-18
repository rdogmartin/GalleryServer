
namespace GalleryServer.Business
{
  /// <summary>
  /// Contains constants used throughout Gallery Server.
  /// </summary>
  public static class GlobalConstants
  {
    /// <summary>
    /// The default name for a user when no actual user account is available. For example, this value is used when remotely
    /// invoking a synchronization.
    /// </summary>
    public const string SystemUserName = "System";
    /// <summary>
    /// The default name for a directory when a valid name cannot be generated from the album title. This occurs
    /// when a user enters an album title consisting entirely of characters that are invalid for a directory
    /// name, such as ?, *, :.
    /// </summary>
    public const string DefaultAlbumDirectoryName = "Album";
    
    /// <summary>
    /// Gets the name of the dictionary key that references the <see cref="Interfaces.IGalleryServerRoleCollection" /> item containing
    /// all roles for the current gallery in the cache item named <see cref="CacheItem.GalleryServerRoles" />. Note that other items 
    /// in the dictionary have keys identified by the username.
    /// </summary>
    public const string GalleryServerRoleAllRolesCacheKey = "AllRoles";

    /// <summary>
    /// Gets the string that is used for the beginning of every role name used for album ownership. The role name has
    /// this format: {RoleNamePrefix} - {AlbumOwnerUserName} - {AlbumTitle} (album {AlbumID}) For example:
    /// "Album Owner - rdmartin - rdmartin's album (album 193)" Current value: "Album Owner"
    /// </summary>
    public const string AlbumOwnerRoleNamePrefix = "Album Owner";

    /// <summary>
    /// Gets the name of the role that defines the permissions to use for album ownership roles.
    /// Current value: _Album Owner Template"
    /// </summary>
    public const string AlbumOwnerRoleTemplateName = "_Album Owner Template";

    /// <summary>
    /// Gets the name of the session variable that stores a List&lt;String&gt; of filenames that were skipped
    /// when the user added one or more files to Gallery Server on the Add objects page.
    /// </summary>
    public const string SkippedFilesDuringUploadSessionKey = "SkippedFiles";

    /// <summary>
    /// Gets the name of the thumbnail file that is created to represent an external media object.
    /// </summary>
    public const string ExternalMediaObjectFilename = "external";

    /// <summary>
    /// Gets the maximum number of skipped objects to display to the user after a synchronization. If the number is too high, 
    /// it can take a long time to transmit the data to the browser, or it it can exceed the maxJsonLength value set in web.config,
    /// which causes a "maximum length exceed" error.
    /// </summary>
    public const int MaxNumberOfSkippedObjectsToDisplayAfterSynch = 500;

    /// <summary>
    /// Gets the maximum number of users to display in a list on the manage users page. When the number of users exceeds
    /// this number, the layout of the page changes to be more efficient with large numbers of users.
    /// </summary>
    public const int MaxNumberOfUsersToDisplayOnManageUsersPage = 1000;

    /// <summary>
    /// Gets the path, relative to the web application root, where files may be temporarily persisted. Ex: "App_Data\\_Temp"
    /// </summary>
    public const string TempUploadDirectory = "App_Data\\_Temp";

    /// <summary>
    /// Gets the path, relative to the web application root, where watermark image files are stored. Ex: "App_Data\\Watermark_Images"
    /// </summary>
    public const string WatermarkDirectory = "App_Data\\Watermark_Images";

    /// <summary>
    /// Gets the path, relative to the web application root, of the application data directory. Ex: "App_Data"
    /// </summary>
    public const string AppDataDirectory = "App_Data";

    /// <summary>
    /// Gets the name of the file that, when present in the App_Data directory, causes the Install Wizard to automatically run.
    /// Ex: "install.txt"
    /// </summary>
    public const string InstallTriggerFileName = "install.txt";

    /// <summary>
    /// Gets the name of the semaphore file that indicates, by its presence, that schema changes are required for 4.0.0. When present, 
    /// the __MigrationHistory.ContextKey and Applications.ApplicationName values are updated during app startup. It is intended
    /// that the gallery deletes this file after the 4.0 schema changes have been applied. However, no harm is caused if the file 
    /// is present after the schema changes are made. Ex: "v4_schema_update_required.txt"
    /// </summary>
    public const string V4SchemaUpdateRequiredFileName = "v4_schema_update_required.txt";

    /// <summary>
    /// Gets the name of the file containing the version key. This is a file distributed with commercial versions that indicates
    /// the version and license type a user has purchased. It is expected that users will place this file in the App_Data directory
    /// after the initial purchase. Upgrade packages should include new version key files automatically. Ex: "version_key.txt"
    /// </summary>
    public const string VersionKeyFileName = "version_key.txt";

    /// <summary>
    /// Gets the URTL to the license server. When app is compiled in DEBUG mode, returns the URL to the dev license server; otherwise
    /// returns the prod license server. Ex: "http://dev.galleryserverpro.com/woocommerce/", "https://galleryserverpro.com/woocommerce/"
    /// </summary>
#if DEBUG
    public const string LicenseServerUrl = "http://dev.galleryserverpro.com/woocommerce/";
#else
    public const string LicenseServerUrl = "https://galleryserverpro.com/woocommerce/";
#endif

    /// <summary>
    /// Gets the instance ID to use when the license activation algorithm was unable to reach the license server. The instance ID is
    /// a string stored in <see cref="AppSetting.InstanceId" /> and is required when deactivating a license.
    /// </summary>
    public const string LicenseActivationFailedInstanceId = "0000000000";

    /// <summary>
    /// Gets the name of the Default membership provider.
    /// </summary>
    public const string DefaultMembershipProviderName = "System.Web.Providers.DefaultMembershipProvider";

    /// <summary>
    /// Gets the name of the Active Directory membership provider.
    /// </summary>
    public const string ActiveDirectoryMembershipProviderName = "System.Web.Security.ActiveDirectoryMembershipProvider";

    /// <summary>
    /// Gets the name of the Active Directory role provider.
    /// </summary>
    public const string ActiveDirectoryRoleProviderName = "GalleryServer.Web.ActiveDirectoryRoleProvider";

    /// <summary>
    /// Gets the number of days Gallery Server is fully functional before it requires a license key to be entered.
    /// Default value = 30.
    /// </summary>
    public const int TrialNumberOfDays = 30;

    /// <summary>
    /// The maximum allowed length for an album directory name.
    /// </summary>
    public const int AlbumDirectoryNameLength = 255;

    /// <summary>
    /// The maximum allowed length for a media object file name.
    /// </summary>
    public const int MediaObjectFileNameLength = 255;

    /// <summary>
    /// The default encryption key as stored in a new installation. It is updated to a new value the first time the application is run.
    /// </summary>
    public const string ENCRYPTION_KEY = "mNU-h7:5f_)3=c%@^}#U9Tn*";
  }
}
