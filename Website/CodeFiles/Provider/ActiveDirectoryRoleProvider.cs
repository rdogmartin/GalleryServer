using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.DirectoryServices;
using System.Web.Hosting;
using System.Web.Security;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using GalleryServer.Business;

namespace GalleryServer.Web
{
  /// <summary>
  /// Provides a Role implementation that uses Active Directory groups as the roles. Implements read-only access to groups. Any method
  /// that alters a role throws a <see cref="NotSupportedException" />.
  /// </summary>
  /// <seealso cref="System.Web.Security.RoleProvider" />
  public sealed class ActiveDirectoryRoleProvider : RoleProvider
  {
    #region Private Fields

    /// <summary>
    /// Indicates whether this provider uses the white list or black list approach.
    /// </summary>
    private enum GroupMode
    {
      Unknown = 0,
      WhiteList,
      BlackList
    }

    private const int MAX_APPLICATION_NAME_LENGTH = 256;
    private string _connectionStringName;
    private string _domain; // Simplified domain name (e.g. "TIS")
    private string _domainDN; // Full domain (e.g. "TIS=ad,DC=techinfosystems,DC=com")
    private GroupMode _groupMode; // Indicates whether provider uses white list or black list approach
    private readonly List<string> _whiteListGroups = new List<string>(); // Groups to recognize (requires groupMode=WhiteList)
    private readonly List<string> _blackListGroups = new List<string>(); // Groups to ignore (requires groupMode=BlackList)
    private int _cacheTimeoutInMinutes; // Length of time, in minutes, to store data returned from AD in memory. Zero indicates no caching.
    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the name of the application to store and retrieve role information for.
    /// </summary>
    /// <value>The name of the application.</value>
    public override string ApplicationName { get; set; }

    #endregion

    #region Member Functions

    /// <summary>
    /// Initializes the provider.
    /// </summary>
    /// <param name="name">The friendly name of the provider.</param>
    /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// The name of the provider is null.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">
    /// The name of the provider has a length of zero.
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
    /// </exception>
    public override void Initialize(string name, NameValueCollection config)
    {
      // Initialize values from web.config.
      if (config == null)
      {
        throw new ArgumentNullException(nameof(config));
      }

      if (string.IsNullOrWhiteSpace(name))
      {
        name = "ActiveDirectoryRoleProvider";
      }

      if (string.IsNullOrWhiteSpace(config["description"]))
      {
        config.Remove("description");
        config.Add("description", "Active Directory Role provider");
      }

      // Initialize the abstract base class.
      base.Initialize(name, config);

      // Retrieve Active Directory Connection String from config
      var adCnStringName = config["connectionStringName"];

      if (string.IsNullOrWhiteSpace(adCnStringName))
      {
        throw new ProviderException("The attribute 'connectionStringName' is missing or empty.");
      }

      var connectionStringSettings = ConfigurationManager.ConnectionStrings[adCnStringName];

      if (string.IsNullOrWhiteSpace(connectionStringSettings?.ConnectionString))
      {
        throw new ProviderException($"ActiveDirectoryRoleProvider configuration issue: The connection string named '{adCnStringName}' was not found in the connection strings section of web.config or it contained an empty string. Check the web configuration file (web.config).");
      }

      _connectionStringName = connectionStringSettings.ConnectionString;

      // Extract the domain portion from the connection string.
      var idx = _connectionStringName.IndexOf("DC=", StringComparison.Ordinal);
      if (idx >= 0)
      {
        _domainDN = _connectionStringName.Substring(idx);

        // Now find the domain name. We assume it's the value of the first 'DC=' in the connection string.
        const string key = "DC=";

        idx = _domainDN.IndexOf(",", StringComparison.Ordinal);
        _domain = (idx > 0 ? _domainDN.Substring(key.Length, idx - key.Length) : _domainDN.Substring(key.Length));
      }
      else
      {
        throw new ProviderException("The connection string specified in 'connectionStringName' does not appear to be a valid LDAP connection string. Missing 'DC='");
      }

      // Retrieve Application Name
      var appName = config["applicationName"];

      if (string.IsNullOrWhiteSpace(appName))
      {
        appName = GetDefaultAppName();
      }

      if (appName.Length > MAX_APPLICATION_NAME_LENGTH)
      {
        throw new ProviderException($"ActiveDirectoryRoleProvider error: applicationName must be less than or equal to {MAX_APPLICATION_NAME_LENGTH} characters.");
      }

      ApplicationName = appName;

      // Retrieve group mode
      var groupModeStr = config["groupMode"];
      if (string.IsNullOrWhiteSpace(groupModeStr))
      {
        throw new ProviderException("The attribute 'groupMode' is missing or empty.");
      }

      GroupMode groupMode;
      if (Enum.TryParse(groupModeStr, true, out groupMode) && groupMode > GroupMode.Unknown)
      {
        _groupMode = groupMode;
      }
      else
      {
        throw new ProviderException("The attribute 'groupMode' must be set to 'WhiteListGroups' or 'BlackListGroups'.");
      }

      switch (_groupMode)
      {
        case GroupMode.WhiteList:
          // When in WhiteList mode, white listed groups are recognized as roles; all others are ignored.
          if (!string.IsNullOrWhiteSpace(config["whiteListGroups"]))
          {
            foreach (var group in config["whiteListGroups"].Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
              _whiteListGroups.Add(group.Trim());
            }
          }

          if (GetCurrentRoleProvider() == name)
          {
            DeleteGalleryServerRolesNotIn(_whiteListGroups);
          }

          break;
        case GroupMode.BlackList:
          // When in BlackList mode, all groups are recognized except for black listed ones.
          if (!string.IsNullOrWhiteSpace(config["blackListGroups"]))
          {
            foreach (var group in config["blackListGroups"].Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
              _blackListGroups.Add(group.Trim());
            }
          }

          if (GetCurrentRoleProvider() == name)
          {
            DeleteGalleryServerRolesIn(_blackListGroups);
          }

          break;
        default:
          throw new ProviderException($"Unexpected group mode '{_groupMode}'.");
      }

      // Extract cache length. Any integer is valid. Values less than one effectively disable caching.
      var cacheTime = config["cacheTimeInMinutes"];
      if (string.IsNullOrWhiteSpace(cacheTime))
      {
        throw new ProviderException("ActiveDirectoryRoleProvider error: The attribute 'cacheTimeInMinutes' is missing or empty.");
      }
      try
      {
        _cacheTimeoutInMinutes = Convert.ToInt32(cacheTime);
      }
      catch
      {
        throw new ProviderException("ActiveDirectoryRoleProvider error: The attribute 'cacheTimeInMinutes' is not a valid integer value.");
      }
    }

    /// <summary>
    /// Retrieve listing of all roles to which a specified user belongs.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns>String array of roles</returns>
    public override string[] GetRolesForUser(string userName)
    {
      var userGroupsCache = CacheController.GetActiveDirectoryUserGroupsCache();
      string[] userGroups;

      if (userGroupsCache == null)
      {
        // App just started or timeout purged cache. Create cache object and add to cache.
        userGroupsCache = new ConcurrentDictionary<string, string[]>();

        if (_cacheTimeoutInMinutes > 0)
        {
          CacheController.SetCache(CacheItem.ActiveDirectoryUserGroups, userGroupsCache, new DateTimeOffset(DateTime.UtcNow.AddMinutes(_cacheTimeoutInMinutes)));
        }
      }
      else if (userGroupsCache.TryGetValue(userName, out userGroups))
      {
        // Found user in cache. Return.
        return userGroups;
      }

      // User not in cache. Get user's groups from AD.
      var userGroupsList = new List<string>();
      using (var context = new PrincipalContext(ContextType.Domain, _domain, _domainDN))
      {
        try
        {
          var p = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName);

          if (p != null)
          {
            var groups = p.GetAuthorizationGroups();

            //foreach (var group in groups)
            // Don't user foreach because we need to swallow NoMatchingPrincipalException, which can't be done with a foreach
            using (var e = groups.GetEnumerator())
            {
              while (e.MoveNext())
              {
                string accountName = null;
                try
                {
                  accountName = e.Current.SamAccountName;
                }
                catch (NoMatchingPrincipalException) { } // Encountered with a customer. See https://social.msdn.microsoft.com/Forums/vstudio/en-US/9dd81553-3539-4281-addd-3eb75e6e4d5d/

                switch (_groupMode)
                {
                  case GroupMode.WhiteList:
                    if (_whiteListGroups.Contains(accountName))
                    {
                      userGroupsList.Add(accountName);
                    }
                    break;
                  case GroupMode.BlackList:
                    if (!_blackListGroups.Contains(accountName))
                    {
                      userGroupsList.Add(accountName);
                    }
                    break;
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          throw new ProviderException("Unable to query Active Directory.", ex);
        }
      }

      userGroups = userGroupsList.ToArray();
      userGroupsCache.TryAdd(userName, userGroups);

      return userGroups;
    }

    /// <summary>
    /// Gets the users in role.
    /// </summary>
    /// <param name="roleName">Name of the role. Case insensitive.</param>
    /// <returns>Returns the users in role.</returns>
    public override string[] GetUsersInRole(string roleName)
    {
      if (!RoleExists(roleName))
      {
        throw new ProviderException($"The role '{roleName}' was not found.");
      }

      var groupUsersCache = CacheController.GetActiveDirectoryGroupUsersCache();
      string[] groupUsers;

      if (groupUsersCache == null)
      {
        // App just started or timeout purged cache. Create cache object and add to cache.
        groupUsersCache = new ConcurrentDictionary<string, string[]>();

        if (_cacheTimeoutInMinutes > 0)
        {
          CacheController.SetCache(CacheItem.ActiveDirectoryGroupUsers, groupUsersCache, new DateTimeOffset(DateTime.UtcNow.AddMinutes(_cacheTimeoutInMinutes)));
        }
      }
      else if (groupUsersCache.TryGetValue(roleName, out groupUsers))
      {
        // Found group in cache. Return.
        return groupUsers;
      }

      // Group not in cache. Get group's users from AD.
      var groupUsersList = new List<string>();

      // Get list of users from membership provider. We only return a user if it's also in the membership user list. We need this because p.GetMembers()
      // returns all users in the role, regardless of the organization unit/container. Since UserController.GetAllUsers() filters by this info, we can use
      // that user list to guarantee that users returned here are also filtered by that info.
      var allUsersFromMembershipProvider = Controller.UserController.GetAllUsers();

      using (var context = new PrincipalContext(ContextType.Domain, _domain, _domainDN))
      {
        try
        {
          var p = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, roleName);

          if (p != null)
          {
            var usersInRole = p.GetMembers(true);

            foreach (var user in usersInRole)
            {
              if (allUsersFromMembershipProvider.Any(u => u.UserName.Equals(user.SamAccountName, StringComparison.OrdinalIgnoreCase)))
              {
                groupUsersList.Add(user.SamAccountName);
              }
            }
          }
        }
        catch (Exception ex)
        {
          throw new ProviderException("Unable to query Active Directory.", ex);
        }
      }

      groupUsers = groupUsersList.ToArray();
      groupUsersCache.TryAdd(roleName, groupUsers);

      return groupUsers;
    }

    /// <summary>
    /// Determine if a specified user is in a specified role.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="roleName"></param>
    /// <returns>Boolean indicating membership</returns>
    public override bool IsUserInRole(string userName, string roleName)
    {
      return GetUsersInRole(roleName).Any(u => u.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Retrieve listing of all roles.
    /// </summary>
    /// <returns>String array of roles</returns>
    public override string[] GetAllRoles()
    {
      var groupUsersCache = CacheController.GetActiveDirectoryGroupUsersCache();
      string[] groups;

      if (groupUsersCache == null)
      {
        // App just started or timeout purged cache. Create cache object and add to cache.
        groupUsersCache = new ConcurrentDictionary<string, string[]>();

        if (_cacheTimeoutInMinutes > 0)
        {
          CacheController.SetCache(CacheItem.ActiveDirectoryGroupUsers, groupUsersCache, new DateTimeOffset(DateTime.UtcNow.AddMinutes(_cacheTimeoutInMinutes)));
        }
      }
      else if (groupUsersCache.TryGetValue("AllGroups", out groups))
      {
        // Found group in cache. Return.
        return groups;
      }

      // "AllGroups" not in cache. Get groups from AD.
      var groupsList = new List<string>();
      var roles = SearchActiveDirectory(_connectionStringName, "(&(objectCategory=group)(|(groupType=-2147483646)(groupType=-2147483644)(groupType=-2147483640)))", "samAccountName");
      foreach (var role in roles)
      {
        switch (_groupMode)
        {
          case GroupMode.WhiteList:
            if (_whiteListGroups.Contains(role))
            {
              groupsList.Add(role);
            }
            break;
          case GroupMode.BlackList:
            if (!_blackListGroups.Contains(role))
            {
              groupsList.Add(role);
            }
            break;
        }
      }

      groups = groupsList.ToArray();
      groupUsersCache.TryAdd("AllGroups", groups);

      return groups;
    }

    /// <summary>
    /// Determine if given role exists
    /// </summary>
    /// <param name="roleName">Role to check</param>
    /// <returns>Boolean indicating existence of role</returns>
    public override bool RoleExists(string roleName)
    {
      return GetAllRoles().Any(r => r == roleName);
    }

    /// <summary>
    /// Gets an array of user names in a role where the user name contains the specified user name to match.
    /// </summary>
    /// <param name="roleName">The role to search in.</param>
    /// <param name="userNameToMatch">The user name to search for.</param>
    /// <returns>
    /// A string array containing the names of all the users where the user name matches <paramref name="userNameToMatch"/> and the user is a member of the specified role.
    /// </returns>
    public override string[] FindUsersInRole(string roleName, string userNameToMatch)
    {
      if (!RoleExists(roleName))
      {
        throw new ProviderException($"The role '{roleName}' was not found.");
      }

      return GetUsersInRole(roleName).Where(u => u.IndexOf(userNameToMatch, StringComparison.OrdinalIgnoreCase) != -1).ToArray();
    }

    #endregion

    #region NonSupported Base Class Functions

    /// <summary>
    /// AddUsersToRoles not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory. 
    /// </summary>
    public override void AddUsersToRoles(string[] userNames, string[] roleNames)
    {
      throw new NotSupportedException("Unable to add users to roles.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory.");
    }

    /// <summary>
    /// CreateRole not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory. 
    /// </summary>
    public override void CreateRole(string roleName)
    {
      throw new NotSupportedException($"Unable to create role '{roleName}'.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory.");
    }

    /// <summary>
    /// DeleteRole not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory. 
    /// </summary>
    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
    {
      throw new NotSupportedException($"Unable to delete role '{roleName}'.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory.");
    }

    /// <summary>
    /// RemoveUsersFromRoles not supported.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory. 
    /// </summary>
    public override void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
    {
      throw new NotSupportedException("Unable to remove users from roles.  For security and management purposes, ActiveDirectoryRoleProvider only supports read operations against Active Directory.");
    }

    #endregion

    #region Functions

    /// <summary>
    /// Performs an extremely constrained query against Active Directory.  Requests only a single value from
    /// AD based upon the filtering parameter to minimize performance hit from large queries.
    /// </summary>
    /// <param name="connectionString">Active Directory Connection String</param>
    /// <param name="filter">LDAP format search filter</param>
    /// <param name="field">AD field to return</param>
    /// <returns>String array containing values specified by 'field' parameter</returns>
    private static IEnumerable<string> SearchActiveDirectory(string connectionString, string filter, string field)
    {
      var results = new List<string>();
      var searcher = new DirectorySearcher
      {
        SearchRoot = new DirectoryEntry(connectionString),
        Filter = filter,
        PageSize = 500
      };

      searcher.PropertiesToLoad.Clear();
      searcher.PropertiesToLoad.Add(field);

      try
      {
        using (var searchResults = searcher.FindAll())
        {
          foreach (SearchResult result in searchResults)
          {
            for (var c = 0; c < result.Properties[field].Count; c++)
            {
              results.Add(result.Properties[field][c].ToString());
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw new ProviderException("Unable to query Active Directory.", ex);
      }

      return results.ToArray();
    }

    /// <summary>
    /// Retrieve the current app name if none has been specified in config. As implemented by MS, lifted from SqlRoleProvider.
    /// </summary>
    /// <returns>String containing the current app name.</returns>
    private static string GetDefaultAppName()
    {
      try
      {
        var appName = HostingEnvironment.ApplicationVirtualPath;

        if (string.IsNullOrEmpty(appName))
        {
          appName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
          var indexOfDot = appName.IndexOf('.');
          if (indexOfDot != -1)
          {
            appName = appName.Remove(indexOfDot);
          }
        }
        return string.IsNullOrEmpty(appName) ? "/" : appName;
      }
      catch
      {
        return "/";
      }
    }

    /// <summary>
    /// Delete roles in gsp.Role table that are not specified in <paramref name="roleNames" />. This preserves data integrity when an
    /// admin changes role providers or switch between <see cref="GroupMode.WhiteList" /> and <see cref="GroupMode.BlackList" />.
    /// </summary>
    /// <param name="roleNames">The role names.</param>
    private static void DeleteGalleryServerRolesNotIn(ICollection<string> roleNames)
    {
      var needToPurgeCache = false;
      foreach (var galleryServerRole in Factory.LoadGalleryServerRoles())
      {
        if (!roleNames.Contains(galleryServerRole.RoleName))
        {
          galleryServerRole.Delete();
          needToPurgeCache = true;
        }
      }

      if (needToPurgeCache)
      {
        CacheController.RemoveCache(CacheItem.GalleryServerRoles);
      }
    }

    /// <summary>
    /// Delete roles in gsp.Role table that are specified in <paramref name="roleNames" />. This preserves data integrity when an
    /// admin changes role providers or switch between <see cref="GroupMode.WhiteList" /> and <see cref="GroupMode.BlackList" />.
    /// </summary>
    /// <param name="roleNames">The role names.</param>
    private static void DeleteGalleryServerRolesIn(ICollection<string> roleNames)
    {
      var needToPurgeCache = false;
      foreach (var galleryServerRole in Factory.LoadGalleryServerRoles())
      {
        if (roleNames.Contains(galleryServerRole.RoleName))
        {
          galleryServerRole.Delete();
          needToPurgeCache = true;
        }
      }

      if (needToPurgeCache)
      {
        CacheController.RemoveCache(CacheItem.GalleryServerRoles);
      }
    }

    /// <summary>
    /// Gets the name of the current role provider as specified in the defaultProvider attribute of the roleManager section in
    /// web.config. We can't get it from Roles.Provider because that will cause a stack overflow. Returns null if there are any
    /// issues encountered while trying to retrieve the value.
    /// </summary>
    /// <returns>System.String.</returns>
    private static string GetCurrentRoleProvider()
    {
      try
      {
        var xmlDoc = new System.Xml.XmlDocument();
        xmlDoc.Load(Utils.WebConfigFilePath);
        var node = xmlDoc.CreateNavigator().SelectSingleNode("/configuration/system.web/roleManager");
        return node?.GetAttribute("defaultProvider", string.Empty);
      }
      catch(Exception ex)
      {
        try
        {
          Controller.AppEventController.LogError(ex);
        }
        catch {}

        return null;
      }
    }

    #endregion
  }
}
