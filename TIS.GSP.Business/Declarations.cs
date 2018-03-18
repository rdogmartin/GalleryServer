using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// Defines a list that uniquely identifies cache items stored in the cache.
  /// </summary>
  public enum CacheItem
  {
    /// <summary>
    /// A System.Collections.Concurrent.ConcurrentDictionary&lt;<see cref="string" />, <see cref="IGalleryServerRoleCollection" />&gt;
    /// stored in cache. The key is a concatenation of the user's session ID and user name. The corresponding value stores the roles that 
    /// user belongs to. The first item in the dictionary will have a key = "AllRoles", and its dictionary entry holds all 
    /// roles used in the current gallery.
    /// </summary>
    GalleryServerRoles,
    /// <summary>
    /// A <see cref="IUserAccountCollection"/> containing a list of all users as reported by the membership provider (Membership.GetAllUsers()).
    /// </summary>
    Users,
    /// <summary>
    /// A System.Collections.Concurrent.ConcurrentDictionary&lt;<see cref="string" />, <see cref="IUserAccountCollection" />&gt;
    /// stored in cache. The key is a concatenation of the user's session ID and user name. The corresponding value stores the users that 
    /// the current user has permission to view.
    /// </summary>
    UsersCurrentUserCanView,
    /// <summary>
    /// An <see cref="IEventCollection" /> stored in cache.
    /// </summary>
    AppEvents,
    /// <summary>
    /// A System.Collections.Concurrent.ConcurrentDictionary&lt;<see cref="string" />, <see cref="IUserProfile" />&gt; 
    /// stored in cache. The key specifies the username of the profile stored in the dictionary entry.
    /// </summary>
    Profiles,
    /// <summary>
    /// An <see cref="IUiTemplateCollection" /> stored in cache.
    /// </summary>
    UiTemplates,
    /// <summary>
    /// An <see cref="IMediaTemplateCollection" /> stored in cache.
    /// </summary>
    MediaTemplates,
    /// <summary>
    /// A System.Collections.Concurrent.ConcurrentDictionary&lt;<see cref="int" />, <see cref="IMimeTypeCollection" />&gt; 
    /// stored in cache. The key specifies the gallery ID of the MIME types stored in the dictionary entry.
    /// </summary>
    MimeTypes,
    /// <summary>
    /// A System.Collections.Concurrent.ConcurrentDictionary&lt;<see cref="int" />, <see cref="CacheItemMedia" />&gt; 
    /// stored in cache. The key specifies the ID of the media asset stored in the dictionary entry.
    /// </summary>
    MediaAssets,
    /// <summary>
    /// A System.Collections.Concurrent.ConcurrentDictionary&lt;<see cref="int" />, <see cref="CacheItemAlbum" />&gt; 
    /// stored in cache. The key specifies the ID of the album asset stored in the dictionary entry.
    /// </summary>
    AlbumAssets,
    /// <summary>
    /// A System.Collections.Concurrent.ConcurrentDictionary&lt;<see cref="int" />, <see cref="IAlbum" />&gt; 
    /// stored in cache. The key specifies the ID of the album stored in the dictionary entry.
    /// </summary>
    InflatedAlbums,
    /// <summary>
    /// A <see cref="System.Collections.Concurrent.ConcurrentDictionary{T, V}" />, where the key is a <see cref="string" /> and the value
    /// is a <see cref="System.Collections.Generic.List{T}" /> of <see cref="Entity.Tag" /> instances. The key is a concatenation of
    /// several properties that define the characteristics of the associated list.
    /// </summary>
    Tags,
    /// <summary>
    /// A <see cref="System.Collections.Concurrent.ConcurrentDictionary{T, V}" />, where the key is a <see cref="string" /> containing 
    /// the user name and the value is an array of <see cref="string" /> instances containing the groups the user belongs to. Used only
    /// by ActiveDirectoryRoleProvider.
    /// </summary>
    ActiveDirectoryUserGroups,
    /// <summary>
    /// A <see cref="System.Collections.Concurrent.ConcurrentDictionary{T, V}" />, where the key is a <see cref="string" /> containing 
    /// the group name and the value is an array of <see cref="string" /> instances containing the users in the group. The first item
    /// in the dictionary will have a key = "AllGroups", and its dictionary entry holds all groups returned by Active Directory, 
    /// filtered by any business logic in the AD role provider. Used only by ActiveDirectoryRoleProvider.
    /// </summary>
    ActiveDirectoryGroupUsers
  }
}
