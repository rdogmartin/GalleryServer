using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Web.Security;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Controller
{
  /// <summary>
  /// Contains functionality related to user management.
  /// </summary>
  public static class UserController
  {
    #region Private Fields

    private static MembershipProvider _membershipProvider;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the Membership provider used by Gallery Server.
    /// </summary>
    /// <value>The Membership provider used by Gallery Server.</value>
    internal static MembershipProvider MembershipGsp
    {
      get
      {
        if (_membershipProvider == null)
        {
          _membershipProvider = GetMembershipProvider();
        }

        return _membershipProvider;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the membership provider is configured to require the user to answer a password 
    /// question for password reset and retrieval. 
    /// </summary>
    /// <value>
    /// 	<c>true</c> if a password answer is required for password reset and retrieval; otherwise, <c>false</c>. The default is true.
    /// </value>
    public static bool RequiresQuestionAndAnswer
    {
      get
      {
        return MembershipGsp.RequiresQuestionAndAnswer;
      }
    }

    /// <summary>
    /// Indicates whether the membership provider is configured to allow users to reset their passwords. 
    /// </summary>
    /// <value><c>true</c> if the membership provider supports password reset; otherwise, <c>false</c>. The default is true.</value>
    public static bool EnablePasswordReset
    {
      get
      {
        return MembershipGsp.EnablePasswordReset;
      }
    }

    /// <summary>
    /// Indicates whether the membership provider is configured to allow users to retrieve their passwords. 
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the membership provider is configured to support password retrieval; otherwise, <c>false</c>. The default is false.
    /// </value>
    public static bool EnablePasswordRetrieval
    {
      get
      {
        return MembershipGsp.EnablePasswordRetrieval;
      }
    }

    /// <summary>
    /// Gets the minimum length required for a password. 
    /// </summary>
    /// <value>The minimum length required for a password. </value>
    public static int MinRequiredPasswordLength
    {
      get
      {
        return MembershipGsp.MinRequiredPasswordLength;
      }
    }

    /// <summary>
    /// Gets the minimum number of non alphanumeric characters that must be present in a password. 
    /// </summary>
    /// <value>The minimum number of non alphanumeric characters that must be present in a password.</value>
    public static int MinRequiredNonAlphanumericCharacters
    {
      get
      {
        return MembershipGsp.MinRequiredNonAlphanumericCharacters;
      }
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Gets an unsorted collection of all the users in the database. The users may be returned from a cache.
    /// </summary>
    /// <returns>Returns a collection of all the users in the database.</returns>
    public static IUserAccountCollection GetAllUsers()
    {
      IUserAccountCollection usersCache = CacheController.GetUsersCache();

      if (usersCache == null)
      {
        usersCache = new UserAccountCollection();

        int totalRecords;
        foreach (MembershipUser user in MembershipGsp.GetAllUsers(0, 0x7fffffff, out totalRecords))
        {
          usersCache.Add(ToUserAccount(user));
        }

        CacheController.SetCache(CacheItem.Users, usersCache);
      }

      return usersCache;
    }

    /// <summary>
    /// Populates the properties of <paramref name="userToLoad" /> with information about the user. Requires that the
    /// <see cref="IUserAccount.UserName" /> property of the <paramref name="userToLoad" /> parameter be assigned a value.
    /// If no user with the specified username exists, no action is taken.
    /// </summary>
    /// <param name="userToLoad">The user account whose properties should be populated.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userToLoad" /> is null.</exception>
    public static void LoadUser(IUserAccount userToLoad)
    {
      if (userToLoad == null)
        throw new ArgumentNullException("userToLoad");

      if (String.IsNullOrEmpty(userToLoad.UserName))
      {
        throw new ArgumentException("The UserName property of the userToLoad parameter must have a valid value. Instead, it was null or empty.");
      }

      IUserAccount user = GetUser(userToLoad.UserName, false);

      if (user != null)
      {
        user.CopyTo(userToLoad);
      }
    }

    /// <overloads>
    /// Gets information from the data source for a user.
    /// </overloads>
    /// <summary>
    /// Gets information from the data source for the current logged-on membership user.
    /// </summary>
    /// <returns>A <see cref="IUserAccount"/> representing the current logged-on membership user.</returns>
    public static IUserAccount GetUser()
    {
      return String.IsNullOrEmpty(Utils.UserName) ? null : ToUserAccount(MembershipGsp.GetUser(Utils.UserName, false));
    }

    /// <summary>
    /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user. 
    /// Returns null if no matching user is found.
    /// </summary>
    /// <param name="userName">The name of the user to get information for.</param>
    /// <param name="userIsOnline"><c>true</c> to update the last-activity date/time stamp for the user; <c>false</c> to return user 
    /// information without updating the last-activity date/time stamp for the user.</param>
    /// <returns>A <see cref="IUserAccount"/> object populated with the specified user's information from the data source.</returns>
    public static IUserAccount GetUser(string userName, bool userIsOnline)
    {
      return ToUserAccount(MembershipGsp.GetUser(userName, userIsOnline));
    }

    /// <summary>
    /// Gets an unsorted collection of users the current user has permission to view. Users who have administer site permission can view all users,
    /// as can gallery administrators when the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles"/> is true. When
    /// the setting is false, gallery admins can only view users in galleries they have gallery admin permission in. Note that
    /// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
    /// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
    /// This overload is slower than <see cref="GetUsersCurrentUserCanView(bool, bool)"/>, so use that one when possible.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>
    /// Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.
    /// </returns>
    /// <overloads>
    /// Gets a collection of users the current user has permission to view.
    /// </overloads>
    public static IUserAccountCollection GetUsersCurrentUserCanView(int galleryId)
    {
      return GetUsersCurrentUserCanView(Utils.IsCurrentUserSiteAdministrator(), Utils.IsCurrentUserGalleryAdministrator(galleryId));
    }

    /// <summary>
    /// Gets an unsorted collection of users the current user has permission to view. Users who have administer site permission can view all users,
    /// as can gallery administrators when the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles" /> is true. When 
    /// the setting is false, gallery admins can only view users in galleries they have gallery admin permission in. Note that
    /// a user may be able to view a user but not update it. This can happen when the user belongs to roles that are associated with
    /// galleries the current user is not an admin for. The users may be returned from a cache. Guaranteed to not return null.
    /// This overload is faster than <see cref="GetUsersCurrentUserCanView(int)" />, so use this one when possible.
    /// </summary>
    /// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
    /// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
    /// <returns>
    /// Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.
    /// </returns>
    public static IUserAccountCollection GetUsersCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
    {
      if (userIsSiteAdmin)
      {
        return UserController.GetAllUsers();
      }
      else if (userIsGalleryAdmin)
      {
        // See if we have a list in the cache. If not, generate it and add to cache.
        var usersCache = CacheController.GetUsersCurrentUserCanViewCache();

        IUserAccountCollection users;
        string cacheKeyName = String.Empty;

        if (System.Web.HttpContext.Current.Session != null)
        {
          cacheKeyName = GetCacheKeyNameForUsersCurrentUserCanView(Utils.UserName);

          if ((usersCache != null) && (usersCache.TryGetValue(cacheKeyName, out users)))
          {
            return users;
          }
        }

        // Nothing in the cache. Calculate it - this is processor intensive when there are many users and/or roles.
        users = DetermineUsersCurrentUserCanView(userIsSiteAdmin, userIsGalleryAdmin);

        // Add to the cache before returning.
        if (usersCache == null)
        {
          usersCache = new ConcurrentDictionary<string, IUserAccountCollection>();
        }

        // Add to the cache, but only if we have access to the session ID.
        if (System.Web.HttpContext.Current.Session != null)
        {
          lock (usersCache) { 
          usersCache.AddOrUpdate(cacheKeyName, users, (key, existingUsers) =>
          {
            existingUsers.Clear();
            existingUsers.AddRange(users);
            return existingUsers;
          });
}

          CacheController.SetCache(CacheItem.UsersCurrentUserCanView, usersCache);
        }

        return users;
      }

      return new UserAccountCollection();
    }

    /// <summary>
    /// Gets a data entity containing information about the specified <paramref name="userName" /> or the current user
    /// if <paramref name="userName" /> is null or empty. A <see cref="GallerySecurityException" /> is thrown if the 
    /// current user does not have view and edit permission to the requested user. The instance can be JSON-parsed and sent to the
    /// browser.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="galleryId">The gallery ID. Optional parameter - But note that when not specified, the <see cref="User.UserAlbumId" />
    /// property is assigned to zero, regardless of its actual value.</param>
    /// <returns>Returns <see cref="Entity.User" /> object containing information about the current user.</returns>
    /// <exception cref="GallerySecurityException">Thrown when the current user does not have permission to view and edit the user.</exception>
    /// <exception cref="InvalidUserException">Thrown when the requested user does not exist.</exception>
    /// <exception cref="InvalidGalleryException">Thrown when the gallery ID does not represent an existing gallery.</exception>
    public static User GetUserEntity(string userName, int galleryId)
    {
      Factory.LoadGallery(galleryId); // Throws ex if gallery ID is not valid

      if (String.IsNullOrWhiteSpace(userName))
        return new Entity.User() { IsNew = true, GalleryId = galleryId, Roles = RoleController.GetDefaultRolesForUser() };

      var user = GetUser(userName, false);

      if (user == null)
      {
        if (Utils.IsCurrentUserSiteAdministrator() || Utils.IsCurrentUserGalleryAdministrator(galleryId))
          throw new InvalidUserException(String.Format("User '{0}' does not exist", userName));
        else
          throw new GallerySecurityException("Insufficient permission to view the user."); // Throw to avoid giving non-admin clues about existence of user
      }
      else if (!UserCanViewAndEditUser(user))
        throw new GallerySecurityException("Insufficient permission to view user.");

      var userPerms = SecurityManager.GetUserObjectPermissions(RoleController.GetGalleryServerRolesForUser(), galleryId);

      return new Entity.User()
        {
          UserName = user.UserName,
          Comment = user.Comment,
          Email = user.Email,
          IsApproved = user.IsApproved,
          IsAuthenticated = Utils.IsAuthenticated,
          CanAddAlbumToAtLeastOneAlbum = userPerms.UserCanAddAlbumToAtLeastOneAlbum,
          CanAddMediaToAtLeastOneAlbum = userPerms.UserCanAddMediaAssetToAtLeastOneAlbum,
          CanEditAtLeastOneAlbum = userPerms.UserCanEditAtLeastOneAlbum,
          CanEditAtLeastOneMediaAsset = userPerms.UserCanEditAtLeastOneMediaAsset,
          EnableUserAlbum = ProfileController.GetProfile(user.UserName).GetGalleryProfile(galleryId).EnableUserAlbum,
          UserAlbumId = Math.Max((galleryId > int.MinValue ? GetUserAlbumId(user.UserName, galleryId) : 0), 0), // Returns 0 for no user album
          GalleryId = galleryId,
          CreationDate = user.CreationDate,
          IsLockedOut = user.IsLockedOut,
          LastActivityDate = user.LastActivityDate,
          LastLoginDate = user.LastLoginDate,
          LastPasswordChangedDate = user.LastPasswordChangedDate,
          Roles = RoleController.GetGalleryServerRolesForUser(userName).Select(r => r.RoleName).ToArray(),
          Password = null,
          PasswordResetRequested = null,
          PasswordChangeRequested = null,
          NotifyUserOnPasswordChange = null
        };
    }

    /// <summary>
    /// Gets the password for the specified user name from the data source. 
    /// </summary>
    /// <param name="userName">The user to retrieve the password for. </param>
    /// <returns>The password for the specified user name.</returns>
    public static String GetPassword(string userName)
    {
      return MembershipGsp.GetPassword(userName, null);
    }

    /// <summary>
    /// Resets a user's password to a new, automatically generated password. This method does not authorize the request; it is
    /// assumed the caller has already done this.
    /// </summary>
    /// <param name="userName">The user to reset the password for. </param>
    /// <returns>The new password for the specified user.</returns>
    public static string ResetPassword(string userName)
    {
      return ResetPasswordInternal(userName);
    }

    /// <summary>
    /// Processes a request to update the password for a membership user.
    /// </summary>
    /// <param name="userName">The user to update the password for.</param>
    /// <param name="oldPassword">The current password for the specified user.</param>
    /// <param name="newPassword">The new password for the specified user.</param>
    /// <returns><c>true</c> if the password was updated successfully; otherwise, <c>false</c>.</returns>
    public static bool ChangePassword(string userName, string oldPassword, string newPassword)
    {
      return MembershipGsp.ChangePassword(userName, oldPassword, newPassword);
    }

    /// <summary>
    /// Clears a lock so that the membership user can be validated.
    /// </summary>
    /// <param name="userName">The membership user whose lock status you want to clear.</param>
    /// <returns><c>true</c> if the membership user was successfully unlocked; otherwise, <c>false</c>.</returns>
    public static bool UnlockUser(string userName)
    {
      return MembershipGsp.UnlockUser(userName);
    }

    /// <overloads>
    /// Persist the user to the data store.
    /// </overloads>
    /// <summary>
    /// Persist the <paramref name="user" /> to the data store. If a password reset is being requested, the new password is 
    /// assigned to <paramref name="newPassword" />.
    /// </summary>
    /// <param name="user">The user to save.</param>
    /// <param name="newPassword">The value of the newly reset password. Assigned only when <see cref="Entity.User.PasswordResetRequested" />
    /// is <c>true</c>; will be null in all other cases.</param>
    /// <exception cref="System.ArgumentNullException">user</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">user;The GalleryId property of the user parameter was null.</exception>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    /// <exception cref="InvalidUserException">Thrown when the e-mail address is not valid.</exception>
    public static void SaveUser(Entity.User user, out string newPassword)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      if (!user.GalleryId.HasValue)
        throw new ArgumentOutOfRangeException("user", "The GalleryId property of the user parameter was null.");

      if (user.Roles == null)
        throw new ArgumentOutOfRangeException("user", "The Roles property of the user parameter was null.");

      var userAccount = ToUserAccount(user);

      if (userAccount == null)
        throw new GallerySecurityException();

      SaveUser(userAccount, user.Roles);

      SaveProfileProperties(user);

      HandlePasswordUpdateRequest(user, out newPassword);
    }

    /// <summary>
    /// Persist the <paramref name="user" /> to the data store.
    /// </summary>
    /// <param name="user">The user to save.</param>
    public static void SaveUser(IUserAccount user)
    {
      UserController.UpdateUser(user);

      CacheController.ReplaceUserInCache(user);
    }

    /// <summary>
    /// Persist the <paramref name="user"/> to the data store, including associating the specified roles with the user. The user is
    /// automatically removed from any other roles they may be a member of. Prior to saving, validation is performed and a 
    /// <see cref="GallerySecurityException"/> is thrown if a business rule would be violated.
    /// </summary>
    /// <param name="user">The user to save.</param>
    /// <param name="roles">The roles to associate with the user.</param>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    /// <exception cref="InvalidUserException">Thrown when the e-mail address is not valid.</exception>
    public static void SaveUser(IUserAccount user, string[] roles)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      if (roles == null)
        throw new ArgumentNullException("roles");

      ValidateSaveUser(user, roles);

      UserController.UpdateUser(user);

      CacheController.ReplaceUserInCache(user);

      var currentRolesForUser = RoleController.GetRolesForUser(user.UserName);

      // Enforce the rule that any default roles settings in any gallery are applied to this user, ensuring the user isn't inadvertently removed from any.
      var rolesToAssignToUser = roles.Union(RoleController.GetDefaultRolesForUser()).ToArray();

      var rolesToAdd = rolesToAssignToUser.Where(r => !currentRolesForUser.Contains(r)).ToArray();
      var rolesToRemove = currentRolesForUser.Where(r => !rolesToAssignToUser.Contains(r)).ToArray();

      RoleController.AddUserToRoles(user.UserName, rolesToAdd);
      RoleController.RemoveUserFromRoles(user.UserName, rolesToRemove);

      var addingOrDeletingRoles = ((rolesToAdd.Length > 0) || (rolesToRemove.Length > 0));

      if (addingOrDeletingRoles)
      {
        RoleController.RemoveRolesForUserFromCache(user.UserName);
        CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
      }
    }

    /// <summary>
    /// Removes a user from the membership data source.
    /// </summary>
    /// <param name="userName">The name of the user to delete.</param>
    /// <returns><c>true</c> if the user was successfully deleted; otherwise, <c>false</c>.</returns>
    public static bool DeleteUser(string userName)
    {
      var deleteResult = MembershipGsp.DeleteUser(userName, true);

      CacheController.RemoveUserFromCache(userName);
      RoleController.RemoveRolesForUserFromCache(userName);

      return deleteResult;
    }

    /// <summary>
    /// Contains functionality that must execute after a user has logged on. Specifically, roles are cleared from the cache,
    /// validation ensures the user has the default roles specified for the gallery, and if user albums are enabled, the user's 
    /// personal album is validated. Developers integrating Gallery Server into their applications should call this method 
    /// after they have authenticated a user. User must be logged on by the time this method is called. For example, one can 
    /// call this method in the LoggedIn event of the ASP.NET Login control.
    /// </summary>
    /// <param name="galleryId">The gallery ID for the gallery the user has logged into. This value is required.</param>
    /// <param name="userName">Name of the user that has logged on.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId"/> is <see cref="Int32.MinValue" />.</exception>
    public static void UserLoggedOn(string userName, int galleryId)
    {
      RoleController.RemoveRolesForUserFromCache(userName);

      // Store the user name and the fact that user is authenticated. Ideally we would not do this and just use
      // User.Identity.Name and User.Identity.IsAuthenticated, but those won't be assigned by ASP.NET until the 
      // next page load.
      Utils.IsAuthenticated = true;
      Utils.UserName = userName;

      ValidateUserAlbum(userName, galleryId);

      ValidateUserIsInDefaultRoles(userName);
    }

    /// <summary>
    /// Contains functionality that must execute after a user has logged off. Specifically, roles are cleared from the cache.
    /// Developers integrating Gallery Server into their applications should call this method after a user has signed out. 
    /// User must be already be logged off by the time this method is called. For example, one can call this method in the 
    /// LoggedOut event of the ASP.NET LoginStatus control.
    /// </summary>
    public static void UserLoggedOff()
    {
      RoleController.RemoveRolesForUserFromCache(Utils.UserName);

      // Clear the user name and the fact that user is not authenticated. Ideally we would not do this and just use
      // User.Identity.Name and User.Identity.IsAuthenticated, but those won't be assigned by ASP.NET until the 
      // next page load.
      Utils.IsAuthenticated = false;
      Utils.UserName = String.Empty;
    }

    /// <overloads>
    /// Create a new user in the Membership data store.
    /// </overloads>
    /// <summary>
    /// Creates the new user having the properties specified in <paramref name="user" />. Note that only the username, email,
    /// password, and roles are persisted. To save other properties, call <see cref="SaveUser(Entity.User, out string)" /> after 
    /// executing this function. If default roles roles have been specified for any non-template gallery, they are assigned
    /// to the user even if they are not included in the <see cref="Entity.User.Roles" /> property of <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>An instance of <see cref="IUserAccount" />.</returns>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
    /// <exception cref="MembershipCreateUserException">Thrown when an error occurs during account creation. Check the StatusCode
    /// property for a MembershipCreateStatus value.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Entity.User.GalleryId" /> property of the <paramref name="user" />
    ///  parameter is null.</exception>
    public static IUserAccount CreateUser(Entity.User user)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      if (!user.GalleryId.HasValue)
        throw new ArgumentOutOfRangeException("user", "The GalleryId property of the user parameter was null.");

      return CreateUser(user.UserName, user.Password, user.Email, user.Roles, false, user.GalleryId.Value);
    }

    /// <summary>
    /// Creates a new account in the membership system with the specified <paramref name="userName"/>, <paramref name="password"/>,
    /// <paramref name="email"/>, and belonging to the specified <paramref name="roles"/>. If required, it sends a verification
    /// e-mail to the user, sends an e-mail notification to admins, and creates a user album. The account will be disabled when
    /// <paramref name="isSelfRegistration"/> is <c>true</c> and either the system option 
    /// <see cref="IGallerySettings.RequireEmailValidationForSelfRegisteredUser" /> or 
    /// <see cref="IGallerySettings.RequireApprovalForSelfRegisteredUser" /> is enabled.
    /// </summary>
    /// <param name="userName">Account name of the user. Cannot be null or empty.</param>
    /// <param name="password">The password for the user. Cannot be null or empty.</param>
    /// <param name="email">The email associated with the user. Required when <paramref name="isSelfRegistration"/> is true
    /// and email verification is enabled.</param>
    /// <param name="roles">The names of the roles to assign to the user. The roles must already exist. If null or empty, no
    /// roles are assigned to the user. If default roles roles have been specified for any non-template gallery, they are assigned
    /// to the user regardless of the values specified here.</param>
    /// <param name="isSelfRegistration">Indicates when the user is creating his or her own account. Set to false when an
    /// administrator creates an account.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>Returns the newly created user.</returns>
    /// <exception cref="MembershipCreateUserException">Thrown when an error occurs during account creation. Check the StatusCode
    /// property for a MembershipCreateStatus value.</exception>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userName" /> or <paramref name="password" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userName" /> or <paramref name="password" /> is an empty string.</exception>
    public static IUserAccount CreateUser(string userName, string password, string email, string[] roles, bool isSelfRegistration, int galleryId)
    {
      #region Validation

      if (userName == null)
        throw new ArgumentNullException("userName");

      if (password == null)
        throw new ArgumentNullException("password");

      if (String.IsNullOrEmpty(userName))
        throw new ArgumentException("The parameter cannot be an empty string.", "userName");

      if (String.IsNullOrEmpty(password))
        throw new ArgumentException("The parameter cannot be an empty string.", "password");

      if ((String.IsNullOrEmpty(email)) && (HelperFunctions.IsValidEmail(userName)))
      {
        // No email address was specified, but the user name happens to be in the form of an email address,
        // so let's set the email property to the user name.
        email = userName;
      }

      #endregion

      IGallerySettings gallerySettings = Factory.LoadGallerySetting(galleryId);

      // Step 1: Create the user. Any number of exceptions may occur; we'll let the caller deal with them.
      IUserAccount user = CreateUser(userName, password, email);

      // Step 2: If this is a self-registered account and email verification is enabled or admin approval is required,
      // disable it. It will be approved when the user validates the email or the admin gives approval.
      if (isSelfRegistration)
      {
        if (gallerySettings.RequireEmailValidationForSelfRegisteredUser || gallerySettings.RequireApprovalForSelfRegisteredUser)
        {
          user.IsApproved = false;
          UpdateUser(user);
        }
      }

      // Step 3: Verify no business rules are being violated by the logged-on user creating an account. We skip this verification
      // for self registrations, because there isn't a logged-on user.
      if (!isSelfRegistration)
      {
        ValidateSaveUser(user, roles);
      }

      // Step 4: Ensure this user is being added to the default roles for all galleries.
      var rolesForUser = roles.Union(RoleController.GetDefaultRolesForUser()).ToArray();

      // Step 5: Add user to roles.
      RoleController.AddUserToRoles(userName, rolesForUser);

      // Step 6: Notify admins that an account was created.
      NotifyAdminsOfNewlyCreatedAccount(user, isSelfRegistration, false, galleryId);

      // Step 7: Send user a welcome message or a verification link.
      if (HelperFunctions.IsValidEmail(user.Email))
      {
        NotifyUserOfNewlyCreatedAccount(user, galleryId);
      }
      else if (isSelfRegistration && gallerySettings.RequireEmailValidationForSelfRegisteredUser)
      {
        // Invalid email, but we need one to send the email verification. Throw error.
        throw new MembershipCreateUserException(MembershipCreateStatus.InvalidEmail);
      }

      CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
      CacheController.RemoveCache(CacheItem.Users);
      
      return user;
    }

    /// <summary>
    /// Delete the user from the membership system. In addition, remove the user from any roles. If a role is an ownership role,
    /// then delete it if the user is the only member. Remove the user from ownership of any albums, and delete the user's
    /// personal album, if user albums are enabled.
    /// </summary>
    /// <param name="userName">Name of the user to be deleted.</param>
    /// <param name="preventDeletingLoggedOnUser">If set to <c>true</c>, throw a <see cref="WebException"/> if attempting
    /// to delete the currently logged on user.</param>
    /// <exception cref="WebException">Thrown when the user cannot be deleted because doing so violates one of the business rules.</exception>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be deleted because doing so violates one of the business rules.</exception>
    public static void DeleteGalleryServerProUser(string userName, bool preventDeletingLoggedOnUser)
    {
      if (String.IsNullOrEmpty(userName))
        return;

      ValidateDeleteUser(userName, preventDeletingLoggedOnUser, true);

      foreach (IGallery gallery in Factory.LoadGalleries())
      {
        DeleteUserAlbum(userName, gallery.GalleryId);
      }

      UpdateRolesAndOwnershipBeforeDeletingUser(userName);

      ProfileController.DeleteProfileForUser(userName);

      DeleteUser(userName);
    }

    /// <overloads>
    /// Gets the personal album for a user.
    /// </overloads>
    /// <summary>
    /// Gets the album for the current user's personal album and <paramref name="galleryId" /> (that is, get the 
    /// album that was created when the user's account was created). The album is created if it does not exist. 
    /// If user albums are disabled or the user has disabled their own album, this function returns null. It also 
    /// returns null if the UserAlbumId property is not found in the profile (this should not typically occur).
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>Returns the album for the current user's personal album.</returns>
    public static IAlbum GetUserAlbum(int galleryId)
    {
      return GetUserAlbum(Utils.UserName, galleryId);
    }

    /// <summary>
    /// Gets the personal album for the specified <paramref name="userName"/> and <paramref name="galleryId" /> 
    /// (that is, get the album that was created when the user's account was created). The album is created if it 
    /// does not exist. If user albums are disabled or the user has disabled their own album, this function returns 
    /// null. It also returns null if the UserAlbumId property is not found in the profile (this should not typically occur).
    /// </summary>
    /// <param name="userName">The account name for the user.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>
    /// Returns the personal album for the specified <paramref name="userName"/>.
    /// </returns>
    public static IAlbum GetUserAlbum(string userName, int galleryId)
    {
      return ValidateUserAlbum(userName, galleryId);
    }

    /// <summary>
    /// Gets the ID of the album for the specified user's personal album (that is, this is the album that was created when the
    /// user's account was created). If user albums are disabled or the UserAlbumId property is not found in the profile,
    /// this function returns int.MinValue. This function executes faster than <see cref="GetUserAlbum(int)"/> and 
    /// <see cref="GetUserAlbum(string, int)"/> but it does not validate that the album exists.
    /// </summary>
    /// <param name="userName">The account name for the user.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>
    /// Returns the ID of the album for the current user's personal album.
    /// </returns>
    public static int GetUserAlbumId(string userName, int galleryId)
    {
      int albumId = Int32.MinValue;

      if (!Factory.LoadGallerySetting(galleryId).EnableUserAlbum)
        return albumId;

      int tmpAlbumId = ProfileController.GetProfileForGallery(userName, galleryId).UserAlbumId;
      albumId = (tmpAlbumId > 0 ? tmpAlbumId : albumId);

      return albumId;
    }

    /// <summary>
    /// Verifies the user album for the specified <paramref name="userName">user</paramref> exists if it is supposed to exist
    /// (creating it if necessary), or does not exist if not (that is, deleting it if necessary). Returns a reference to the user
    /// album if a user album exists or has just been created; otherwise returns null. Also returns null if user albums are
    /// disabled at the application level or <see cref="IGallerySettings.UserAlbumParentAlbumId" /> does not match an existing album.
    /// A user album is created if user albums are enabled but none for the user exists. If user albums are enabled at the
    /// application level but the user has disabled them in his profile, the album is deleted if it exists.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <param name="galleryId">The gallery ID for the gallery where the user album is to be validated. This value is required.</param>
    /// <returns>
    /// Returns a reference to the user album for the specified <paramref name="userName">user</paramref>, or null
    /// if user albums are disabled or <see cref="IGallerySettings.UserAlbumParentAlbumId" /> does not match an existing album.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId"/> is <see cref="Int32.MinValue" />.</exception>
    public static IAlbum ValidateUserAlbum(string userName, int galleryId)
    {
      if (String.IsNullOrEmpty(userName))
        throw new ArgumentException("Parameter cannot be null or an empty string.", "userName");

      if (!Factory.LoadGallerySetting(galleryId).EnableUserAlbum)
        return null;

      if (galleryId == Int32.MinValue)
      {
        // If we get here then user albums are enabled but an invalid gallery ID has been passed. This function can't do 
        // its job without the ID, so throw an error.
        throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "A valid gallery ID must be passed to the UserController.ValidateUserAlbum function when user albums are enabled. Instead, the value {0} was passed for the gallery ID.", galleryId));
      }

      bool userAlbumExists = false;
      bool userAlbumShouldExist = ProfileController.GetProfileForGallery(userName, galleryId).EnableUserAlbum;

      IAlbum album = null;

      int albumId = GetUserAlbumId(userName, galleryId);

      if (albumId > Int32.MinValue)
      {
        try
        {
          // Try loading the album.
          album = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(albumId) { IsWritable = true });

          userAlbumExists = true;
        }
        catch (InvalidAlbumException) { }
      }

      // Delete or create if necessary. Deleting should only be needed if 
      if (userAlbumExists && !userAlbumShouldExist)
      {
        try
        {
          AlbumController.DeleteAlbum(album);
        }
        catch (Exception ex)
        {
          // Log any errors that happen but don't let them bubble up.
          AppEventController.LogError(ex, galleryId);
        }
        finally
        {
          album = null;
        }
      }
      else if (!userAlbumExists && userAlbumShouldExist)
      {
        album = AlbumController.CreateUserAlbum(userName, galleryId);
      }

      return album;
    }

    /// <summary>
    /// Verifies the default roles specified in <see cref="IGallerySettings.DefaultRolesForUser" /> for all non-template galleries
    /// are assigned to <paramref name="userName" />. This is necessary in a few cases: (1) A user has been added to AD and is now logging in.
    /// (2) An admin updated the default roles setting and a user is logging in before the change has been propagated to all users.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    private static void ValidateUserIsInDefaultRoles(string userName)
    {
      var needToReloadRoles = false;
      var usersRoles = RoleController.GetGalleryServerRolesForUser(userName).Select(r => r.RoleName);

      foreach (var role in RoleController.GetDefaultRolesForUser().Where(r => !usersRoles.Contains(r)).Where(RoleController.RoleExists))
      {
        RoleController.AddUserToRole(userName, role);
        needToReloadRoles = true;
      }

      if (needToReloadRoles)
      {
        RoleController.RemoveRolesForUserFromCache(userName);
        CacheController.RemoveCache(CacheItem.UsersCurrentUserCanView);
      }
    }

    /// <summary>
    /// Activates the account for the specified <paramref name="userName"/> and automatically logs on the user. If the
    /// admin approval system setting is enabled (RequireApprovalForSelfRegisteredUser=<c>true</c>), then record the
    /// validation in the user's comment field but do not activate the account. Instead, send the administrator(s) an
    /// e-mail notifying them of a pending account. This method is typically called after a user clicks the confirmation
    /// link in the verification e-mail after creating a new account.
    /// </summary>
    /// <param name="userName">Name of the user who has just validated his or her e-mail address.</param>
    /// <param name="galleryId">The gallery ID for the gallery where the user is being activated. This value is required.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId"/> is <see cref="Int32.MinValue" />.</exception>
    public static void UserEmailValidatedAfterCreation(string userName, int galleryId)
    {
      IUserAccount user = GetUser(userName, true);

      NotifyAdminsOfNewlyCreatedAccount(user, true, true, galleryId);

      if (!Factory.LoadGallerySetting(galleryId).RequireApprovalForSelfRegisteredUser)
      {
        user.IsApproved = true;

        LogOffUser();
        LogOnUser(userName, galleryId);
      }

      user.Comment = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.CreateAccount_Verification_Comment_Text, user.Email, DateTime.UtcNow);

      UpdateUser(user);

      CacheController.ReplaceUserInCache(user);
    }

    /// <summary>
    /// Logs off the current user.
    /// </summary>
    public static void LogOffUser()
    {
      FormsAuthentication.SignOut();

      UserLoggedOff();
    }

    /// <overloads>
    /// Sets an authentication cookie for the specified user so that the user is considered logged on by the application. This
    /// function does not authenticate the user; the calling function must perform that function or otherwise guarantee that it
    /// is appropriate to log on the user.
    /// </overloads>
    /// <summary>
    /// Logs on the specified <paramref name="userName"/>.
    /// </summary>
    /// <param name="userName">The username for the user to log on.</param>
    public static void LogOnUser(string userName)
    {
      foreach (IGallery gallery in Factory.LoadGalleries())
      {
        LogOnUser(userName, gallery.GalleryId);
      }
    }

    /// <summary>
    /// Sets an authentication cookie for the specified <paramref name="userName"/> so that the user is considered logged on by
    /// the application. This function does not authenticate the user; the calling function must perform that function or 
    /// otherwise guarantee that it is appropriate to log on the user.
    /// </summary>
    /// <param name="userName">The username for the user to log on.</param>
    /// <param name="galleryId">The gallery ID for the gallery where the user album is to be validated. This value is required.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId"/> is <see cref="Int32.MinValue" />.</exception>
    public static void LogOnUser(string userName, int galleryId)
    {
      FormsAuthentication.SetAuthCookie(userName, false);

      UserLoggedOn(userName, galleryId);
    }

    /// <summary>
    /// Gets the error message associated with the <see cref="MembershipCreateUserException" /> exception that can occur when
    /// adding a user.
    /// </summary>
    /// <param name="status">A <see cref="MembershipCreateStatus" />. This can be populated from the 
    /// <see cref="MembershipCreateUserException.StatusCode" /> property of the exception.</param>
    /// <returns>Returns an error message.</returns>
    public static string GetAddUserErrorMessage(MembershipCreateStatus status)
    {
      switch (status)
      {
        case MembershipCreateStatus.DuplicateUserName:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_DuplicateUserName;

        case MembershipCreateStatus.DuplicateEmail:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_DuplicateEmail;

        case MembershipCreateStatus.InvalidPassword:
          return String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_InvalidPassword, MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);

        case MembershipCreateStatus.InvalidEmail:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_InvalidEmail;

        case MembershipCreateStatus.InvalidAnswer:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_InvalidAnswer;

        case MembershipCreateStatus.InvalidQuestion:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_InvalidQuestion;

        case MembershipCreateStatus.InvalidUserName:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_InvalidUserName;

        case MembershipCreateStatus.ProviderError:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_ProviderError;

        case MembershipCreateStatus.UserRejected:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_UserRejected;

        default:
          return Resources.GalleryServer.Admin_Manage_Users_Create_User_Error_Generic;
      }
    }

    /// <summary>
    /// Get a list of galleries the current user can administer. Site administrators can view all galleries, while gallery
    /// administrators may have access to zero or more galleries.
    /// </summary>
    /// <returns>Returns an <see cref="IGalleryCollection" /> containing the galleries the current user can administer.</returns>
    public static IGalleryCollection GetGalleriesCurrentUserCanAdminister()
    {
      return GetGalleriesUserCanAdminister(Utils.UserName);
    }

    /// <summary>
    /// Get a list of galleries the specified <paramref name="userName"/> can administer. Site administrators can view all
    /// galleries, while gallery administrators may have access to zero or more galleries.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <returns>
    /// Returns an <see cref="IGalleryCollection"/> containing the galleries the current user can administer.
    /// </returns>
    public static IGalleryCollection GetGalleriesUserCanAdminister(string userName)
    {
      IGalleryCollection adminGalleries = new GalleryCollection();
      foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(userName))
      {
        if (role.AllowAdministerSite)
        {
          return Factory.LoadGalleries();
        }
        else if (role.AllowAdministerGallery)
        {
          foreach (IGallery gallery in role.Galleries)
          {
            if (!adminGalleries.Contains(gallery))
            {
              adminGalleries.Add(gallery);
            }
          }
        }
      }

      return adminGalleries;
    }

    /// <summary>
    /// Gets a collection of all the galleries the specified <paramref name="userName" /> has access to.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <returns>Returns an <see cref="IGalleryCollection" /> of all the galleries the specified <paramref name="userName" /> has access to.</returns>
    public static IGalleryCollection GetGalleriesForUser(string userName)
    {
      IGalleryCollection galleries = new GalleryCollection();

      foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(userName))
      {
        foreach (IGallery gallery in role.Galleries)
        {
          if (!galleries.Contains(gallery))
          {
            galleries.Add(gallery);
          }
        }
      }

      return galleries;
    }

    /// <summary>
    /// Validates the logged on user has permission to save the specified <paramref name="userToSave"/> and to add/remove the user 
    /// to/from the specified <paramref name="roles"/>. Throw a <see cref="GallerySecurityException"/> if user is not authorized.
    /// This method assumes the logged on user is a site administrator or gallery administrator but does not verify it.
    /// </summary>
    /// <param name="userToSave">The user to save. The only property that must be specified is <see cref="IUserAccount.UserName" />.</param>
    /// <param name="roles">The roles to be associated with the user.</param>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
    public static void ValidateLoggedOnUserHasPermissionToSaveUser(IUserAccount userToSave, string[] roles)
    {
      if (roles == null)
        throw new ArgumentNullException("roles");

      var rolesForUser = RoleController.GetRolesForUser(userToSave.UserName);
      var rolesToAdd = roles.Where(r => !rolesForUser.Contains(r)).ToArray();
      var rolesToRemove = rolesForUser.Where(r => !roles.Contains(r)).ToArray();

      // Enforces the following rules:
      // 1. A user with site administration permission has no restrictions. Subsequent rules do not apply.
      // 2. Gallery admin is not allowed to add admin site permission to any user or update any user that has site admin permission.
      // 3. Gallery admin cannot add or remove a user to/from a role associated with other galleries, UNLESS he is also a gallery admin
      //    to those galleries.
      // 4. NOT ENFORCED: If user to be updated is a member of roles that apply to other galleries, Gallery admin must be a gallery admin 
      //    in every one of those galleries. Not enforced because this is considered acceptable behavior.

      if (Utils.IsCurrentUserSiteAdministrator())
        return;

      VerifyGalleryAdminIsNotUpdatingUserWithAdminSitePermission(userToSave, rolesToAdd);

      VerifyGalleryAdminCanAddOrRemoveRolesForUser(rolesToAdd, rolesToRemove);

      #region RULE 4 (Not enforced)
      // RULE 4: Gallery admin can update user only when he is a gallery admin in every gallery the user to be updated is a member of.

      //// Step 1: Get a list of galleries the user to be updated is associated with.
      //IGalleryCollection userGalleries = new GalleryCollection();
      //foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(userToSave.UserName))
      //{
      //  foreach (IGallery gallery in role.Galleries)
      //  {
      //    if (!userGalleries.Contains(gallery))
      //    {
      //      userGalleries.Add(gallery);
      //    }
      //  }
      //}

      //// Step 2: Validate that the current user is a gallery admin for every gallery the user to be updated is a member of.
      //foreach (IGallery userGallery in userGalleries)
      //{
      //  if (!adminGalleries.Contains(userGallery))
      //  {
      //    throw new GallerySecurityException("You are attempting to save changes to a user that affects multiple galleries, including at least one gallery you do not have permission to administer. To edit this user, you must be a gallery administrator in every gallery this user is a member of.");
      //  }
      //}
      #endregion
    }

    /// <summary>
    /// Automatically logs on the user specified in the query string parameter 'user' and then reloads the page with the 
    /// parameter removed. If the parameter does not exist or the user is already logged on, do nothing. Additionally, if the 
    /// specified user is not specified in the web.config application setting named GalleryServerAutoLogonUsers, no action is taken.
    /// Note: All requests come through here, even those for static resources like CSS and js files.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public static void AutoLogonUser(System.Web.HttpContext context)
    {
      string username;
      if (!ValidateAutoLogonUserRequest(context, out username))
        return;

      // If we get here, either no one is logged in or the currently logged on user is different than the requested one.
      // Log out the current user, then log in new user.
      FormsAuthentication.SignOut();
      FormsAuthentication.SetAuthCookie(username, false);

      var newUser = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(username, "Forms"), RoleController.GetRolesForUser(username));
      context.User = newUser;

      // Reload the current page with the 'user' query string parameter removed.
      context.Response.Redirect(Utils.RemoveQueryStringParameter(Utils.GetCurrentPageUrl(true), "user"), true);
    }

    #endregion

    #region Private Static Methods

    /// <summary>
    /// Adds a new user with the specified e-mail address to the data store.
    /// </summary>
    /// <param name="userName">The user name for the new user.</param>
    /// <param name="password">The password for the new user.</param>
    /// <param name="email">The email for the new user.</param>
    /// <returns>Returns a new user with the specified e-mail address to the data store.</returns>
    private static IUserAccount CreateUser(string userName, string password, string email)
    {
      // This function is a re-implementation of the System.Web.Security.Membership.CreateUser method. We can't call it directly
      // because it uses the default provider, and we might be using a named provider.
      MembershipCreateStatus status;
      MembershipUser user = MembershipGsp.CreateUser(userName, password, email, null, null, true, null, out status);
      if (user == null)
      {
        throw new MembershipCreateUserException(status);
      }

      return ToUserAccount(user);
    }

    /// <summary>
    /// Gets the Membership provider used by Gallery Server.
    /// </summary>
    /// <returns>The Membership provider used by Gallery Server.</returns>
    private static MembershipProvider GetMembershipProvider()
    {
      if (String.IsNullOrEmpty(AppSetting.Instance.MembershipProviderName))
      {
        return Membership.Provider;
      }
      else
      {
        return Membership.Providers[AppSetting.Instance.MembershipProviderName];
      }
    }

    /// <summary>
    /// Send an e-mail to the users that are subscribed to new account notifications. These are specified in the
    /// <see cref="IGallerySettings.UsersToNotifyWhenAccountIsCreated" /> configuration setting. If 
    /// <see cref="IGallerySettings.RequireEmailValidationForSelfRegisteredUser" /> is enabled, do not send an e-mail at this time. 
    /// Instead, it is sent when the user clicks the confirmation link in the e-mail.
    /// </summary>
    /// <param name="user">An instance of <see cref="IUserAccount"/> that represents the newly created account.</param>
    /// <param name="isSelfRegistration">Indicates when the user is creating his or her own account. Set to false when an
    /// administrator creates an account.</param>
    /// <param name="isEmailVerified">If set to <c>true</c> the e-mail has been verified to be a valid, active e-mail address.</param>
    /// <param name="galleryId">The gallery ID storing the e-mail configuration information and the list of users to notify.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    private static void NotifyAdminsOfNewlyCreatedAccount(IUserAccount user, bool isSelfRegistration, bool isEmailVerified, int galleryId)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      IGallerySettings gallerySettings = Factory.LoadGallerySetting(galleryId);

      if (isSelfRegistration && !isEmailVerified && gallerySettings.RequireEmailValidationForSelfRegisteredUser)
      {
        return;
      }

      EmailTemplate emailTemplate;
      if (isSelfRegistration && gallerySettings.RequireApprovalForSelfRegisteredUser)
      {
        emailTemplate = EmailController.GetEmailTemplate(EmailTemplateForm.AdminNotificationAccountCreatedRequiresApproval, user);
      }
      else
      {
        emailTemplate = EmailController.GetEmailTemplate(EmailTemplateForm.AdminNotificationAccountCreated, user);
      }

      foreach (IUserAccount userToNotify in gallerySettings.UsersToNotifyWhenAccountIsCreated)
      {
        if (!String.IsNullOrEmpty(userToNotify.Email))
        {
          MailAddress admin = new MailAddress(userToNotify.Email, userToNotify.UserName);
          try
          {
            EmailController.SendEmail(admin, emailTemplate.Subject, emailTemplate.Body, galleryId);
          }
          catch (WebException ex)
          {
            AppEventController.LogError(ex);
          }
          catch (SmtpException ex)
          {
            AppEventController.LogError(ex);
          }
        }
      }
    }

    /// <summary>
    /// Send an e-mail to the user associated with the new account. This will be a verification e-mail if e-mail verification
    /// is enabled; otherwise it is a welcome message. The calling method should ensure that the <paramref name="user"/>
    /// has a valid e-mail configured before invoking this function.
    /// </summary>
    /// <param name="user">An instance of <see cref="IUserAccount"/> that represents the newly created account.</param>
    /// <param name="galleryId">The gallery ID. This specifies which gallery to use to look up configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    private static void NotifyUserOfNewlyCreatedAccount(IUserAccount user, int galleryId)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

      bool enableEmailVerification = gallerySetting.RequireEmailValidationForSelfRegisteredUser;
      bool requireAdminApproval = gallerySetting.RequireApprovalForSelfRegisteredUser;

      if (enableEmailVerification)
      {
        EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreatedNeedsVerification);
      }
      else if (requireAdminApproval)
      {
        EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreatedNeedsApproval);
      }
      else
      {
        EmailController.SendNotificationEmail(user, EmailTemplateForm.UserNotificationAccountCreated);
      }
    }

    /// <summary>
    /// Throws an exception if the user cannot be deleted, such as when trying to delete his or her own account, or when deleting
    /// the only account with admin permission.
    /// </summary>
    /// <param name="userName">Name of the user to delete.</param>
    /// <param name="preventDeletingLoggedOnUser">If set to <c>true</c>, throw a <see cref="GallerySecurityException"/> if attempting
    /// to delete the currently logged on user.</param>
    /// <param name="preventDeletingLastAdminAccount">If set to <c>true</c> throw a <see cref="GallerySecurityException"/> if attempting
    /// to delete the last user with <see cref="SecurityActions.AdministerSite" /> permission. When false, do not perform this check. It does not matter
    /// whether the user to delete is actually an administrator.</param>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be deleted because doing so violates one of the business rules.</exception>
    private static void ValidateDeleteUser(string userName, bool preventDeletingLoggedOnUser, bool preventDeletingLastAdminAccount)
    {
      if (preventDeletingLoggedOnUser)
      {
        // Don't let user delete their own account.
        if (userName.Equals(Utils.UserName, StringComparison.OrdinalIgnoreCase))
        {
          throw new GallerySecurityException(Resources.GalleryServer.Admin_Manage_Users_Cannot_Delete_User_Msg);
        }
      }

      if (preventDeletingLastAdminAccount)
      {
        if (!DoesAtLeastOneOtherSiteAdminExist(userName))
        {
          if (!DoesAtLeastOneOtherGalleryAdminExist(userName))
          {
            throw new GallerySecurityException("You are attempting to delete the only user with permission to administer a gallery or site. If you want to delete this account, first assign another account to a role with administrative permission.");
          }
        }
      }

      // User can delete account only if he is a site admin or a gallery admin in every gallery this user can access.
      IGalleryCollection adminGalleries = GetGalleriesCurrentUserCanAdminister();

      if (adminGalleries.Count > 0) // Only continue when user is an admin for at least one gallery. This allows regular users to delete their own account.
      {
        foreach (IGallery gallery in GetGalleriesForUser(userName))
        {
          if (!adminGalleries.Contains(gallery))
          {
            throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "The user '{0}' has access to a gallery (Gallery ID = {1}) that you are not an administrator for. To delete a user, one of the following must be true: (1) you are a site administrator, or (2) you are a gallery administrator in every gallery the user has access to.", userName, gallery.GalleryId));
          }
        }
      }
    }

    /// <summary>
    /// If user is a gallery admin, verify at least one other user is a gallery admin for each gallery. If user is not a gallery 
    /// admin for any gallery, return <c>true</c> without actually verifying that each that each gallery has an admin, since it
    /// is reasonable to assume it does (and even if it didn't, that shouldn't prevent us from deleting this user).
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <returns><c>true</c> if at least one user besides <paramref name="userName" /> is a gallery admin for each gallery;
    /// otherwise <c>false</c>.</returns>
    private static bool DoesAtLeastOneOtherGalleryAdminExist(string userName)
    {
      bool atLeastOneOtherAdminExists = false;

      IGalleryCollection galleriesUserCanAdminister = UserController.GetGalleriesUserCanAdminister(userName);

      if (galleriesUserCanAdminister.Count == 0)
      {
        // User is not a gallery administrator, so we don't have to make sure there is another gallery administrator.
        // Besides, we can assume there is another one anyway.
        return true;
      }

      foreach (IGallery gallery in galleriesUserCanAdminister)
      {
        // Get all the roles that have gallery admin permission to this gallery
        foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForGallery(gallery).GetRolesWithGalleryAdminPermission())
        {
          // Make sure at least one user besides the user specified in userName is in these roles.
          foreach (string userNameInRole in RoleController.GetUsersInRole(role.RoleName))
          {
            if (!userNameInRole.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
              atLeastOneOtherAdminExists = true;
              break;
            }
          }

          if (atLeastOneOtherAdminExists)
            break;
        }

        if (atLeastOneOtherAdminExists)
          break;
      }

      return atLeastOneOtherAdminExists;
    }

    /// <summary>
    /// Determine if at least one other user beside <paramref name="userName" /> is a site administrator.
    /// </summary>
    /// <param name="userName">A user name.</param>
    /// <returns><c>true</c> if at least one other user beside <paramref name="userName" /> is a site administrator; otherwise <c>false</c>.</returns>
    private static bool DoesAtLeastOneOtherSiteAdminExist(string userName)
    {
      bool atLeastOneOtherAdminExists = false;

      foreach (IGalleryServerRole role in RoleController.GetGalleryServerRoles())
      {
        if (!role.AllowAdministerSite)
          continue;

        foreach (string userInAdminRole in RoleController.GetUsersInRole(role.RoleName))
        {
          if (!userInAdminRole.Equals(userName, StringComparison.OrdinalIgnoreCase))
          {
            atLeastOneOtherAdminExists = true;
            break;
          }
        }
      }
      return atLeastOneOtherAdminExists;
    }

    private static void DeleteUserAlbum(string userName, int galleryId)
    {
      IAlbum album = GetUserAlbum(userName, galleryId);

      if (album != null)
        AlbumController.DeleteAlbum(album);
    }

    /// <summary>
    /// Remove the user from any roles. If a role is an ownership role, then delete it if the user is the only member.
    /// Remove the user from ownership of any albums.
    /// </summary>
    /// <param name="userName">Name of the user to be deleted.</param>
    /// <remarks>The user will be specified as an owner only for those albums that belong in ownership roles, so
    /// to find all albums the user owns, we need only to loop through the user's roles and inspect the ones
    /// where the names begin with the album owner role name prefix variable.</remarks>
    private static void UpdateRolesAndOwnershipBeforeDeletingUser(string userName)
    {
      List<string> rolesToDelete = new List<string>();

      string[] userRoles = RoleController.GetRolesForUser(userName);
      foreach (string roleName in userRoles)
      {
        if (RoleController.IsRoleAnAlbumOwnerRole(roleName))
        {
          if (RoleController.GetUsersInRole(roleName).Length <= 1)
          {
            // The user we are deleting is the only user in the owner role. Mark for deletion.
            rolesToDelete.Add(roleName);
          }
        }
      }

      if (userRoles.Length > 0)
      {
        foreach (string role in userRoles)
        {
          RoleController.RemoveUserFromRole(userName, role);
        }
      }

      foreach (string roleName in rolesToDelete)
      {
        RoleController.DeleteGalleryServerProRole(roleName);
      }
    }

    /// <summary>
    /// Updates the properties of the <see cref="IUserAccount" /> corresponding to the specified entity with the properties
    /// of the entity. The changes are not persisted to the data store. Returns null if no existing user has a username
    /// matching <see cref="Entity.User.UserName" />.
    /// </summary>
    /// <param name="userEntity">The user entity.</param>
    /// <returns>An instance of <see cref="IUserAccount" />, or null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userEntity" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more properties of <paramref name="userEntity" />
    /// has an unexpected value.</exception>
    private static IUserAccount ToUserAccount(Entity.User userEntity)
    {
      if (userEntity == null)
        throw new ArgumentNullException("userEntity");

      if (!userEntity.IsApproved.HasValue)
        throw new ArgumentOutOfRangeException("userEntity", "The IsApproved property of the userEntity parameter was null.");

      if (!userEntity.IsLockedOut.HasValue)
        throw new ArgumentOutOfRangeException("userEntity", "The IsLockedOut property of the userEntity parameter was null.");

      var user = GetUser(userEntity.UserName, false);

      if (user == null)
        return null;

      user.Comment = userEntity.Comment;
      user.Email = userEntity.Email;
      user.IsApproved = userEntity.IsApproved.Value;
      user.IsLockedOut = userEntity.IsLockedOut.Value;

      if (MembershipGsp.GetType().ToString() == GlobalConstants.ActiveDirectoryMembershipProviderName)
      {
        // The AD provider will throw an ArgumentException during the UpdateUser method if the comment is empty,
        // so add a single space if necessary.
        if (String.IsNullOrEmpty(user.Comment))
          user.Comment = " ";
      }

      return user;
    }

    private static IUserAccount ToUserAccount(MembershipUser u)
    {
      if (u == null)
        return null;

      if (MembershipGsp.GetType().ToString() == GlobalConstants.ActiveDirectoryMembershipProviderName)
      {
        // The AD provider does not support a few properties so substitute default values for them.
        return new UserAccount(u.Comment, u.CreationDate, u.Email, u.IsApproved, u.IsLockedOut, false,
                               DateTime.MinValue, u.LastLockoutDate, DateTime.MinValue, u.LastPasswordChangedDate,
                               u.PasswordQuestion, u.ProviderName, u.ProviderUserKey, u.UserName, false, String.Empty, String.Empty, String.Empty);
      }
      else
      {
        return new UserAccount(u.Comment, u.CreationDate, u.Email, u.IsApproved, u.IsLockedOut, u.IsOnline,
                               u.LastActivityDate, u.LastLockoutDate, u.LastLoginDate, u.LastPasswordChangedDate,
                               u.PasswordQuestion, u.ProviderName, u.ProviderUserKey, u.UserName, false, String.Empty, String.Empty, String.Empty);
      }
    }

    private static void UpdateMembershipUser(MembershipUser userInDb, IUserAccount source)
    {
      if (userInDb == null)
        throw new ArgumentNullException("userToUpdate");

      if (source == null)
        throw new ArgumentNullException("source");

      userInDb.Comment = source.Comment;
      userInDb.Email = source.Email;
      userInDb.IsApproved = source.IsApproved;
    }

    //private static MembershipUser ToMembershipUser(IUserAccount u)
    //{
    //	if (String.IsNullOrEmpty(u.UserName))
    //	{
    //		throw new ArgumentException("IUserAccount.UserName cannot be empty.");
    //	}

    //	MembershipUser user = MembershipGsp.GetUser(u.UserName, false);

    //	if (user != null)
    //	{
    //		user.Comment = u.Comment;
    //		user.Email = u.Email;
    //		user.IsApproved = u.IsApproved;
    //	}

    //	return user;
    //}

    /// <summary>
    /// Updates information about a user in the data source, including unlocking the user if requested. No action is taken if
    /// an existing user in the data store is not found.
    /// </summary>
    /// <param name="userToSave">A <see cref="IUserAccount"/> object that represents the user to update and the updated information for the user.</param>
    private static void UpdateUser(IUserAccount userToSave)
    {
      var userInDb = MembershipGsp.GetUser(userToSave.UserName, false);

      if (userInDb == null)
        return;

      if (HasUserBeenModified(userToSave, userInDb))
      {
        bool userIsBeingApproved = !userInDb.IsApproved && userToSave.IsApproved;

        if (userInDb.IsLockedOut && !userToSave.IsLockedOut)
        {
          // A request is being made to unlock the user.
          UnlockUser(userToSave.UserName);
        }

        UpdateMembershipUser(userInDb, userToSave);

        MembershipGsp.UpdateUser(userInDb);

        if (userIsBeingApproved)
        {
          // Administrator is approving user. Send notification e-mail to user.
          EmailController.SendNotificationEmail(userToSave, EmailTemplateForm.UserNotificationAccountCreatedApprovalGiven);
        }
      }
    }

    /// <summary>
    /// Make sure the logged-on person has authority to save the user info and that h/she isn't doing anything stupid,
    /// like removing admin permission from his or her own account. Throws a <see cref="GallerySecurityException"/> when
    /// the action is not allowed.
    /// </summary>
    /// <param name="userToSave">The user to save.</param>
    /// <param name="roles">The roles to associate with the user.</param>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
    /// <exception cref="InvalidUserException">Thrown when the e-mail address is not valid.</exception>
    private static void ValidateSaveUser(IUserAccount userToSave, string[] roles)
    {
      if (AppSetting.Instance.InstallationRequested && (GalleryController.GetAdminUserFromInstallTextFile().UserName == userToSave.UserName))
      {
        // We are creating the user specified in install.txt. Don't continue validation because it will fail 
        // if no one is logged in to the gallery or the logged on user doesn't have permission to create/edit a user.
        // This is not a security vulnerability because if the user has the ability to write a file to App_Data
        // the server is already compromised.
        return;
      }

      if (!UserCanViewAndEditUser(userToSave))
      {
        throw new GallerySecurityException("You must be a gallery or site administrator to save changes to this user.");
      }

      if (userToSave.UserName.Equals(Utils.UserName, StringComparison.OrdinalIgnoreCase))
      {
        ValidateUserCanSaveOwnAccount(userToSave, roles);
      }

      ValidateLoggedOnUserHasPermissionToSaveUser(userToSave, roles);

      ValidateEmail(userToSave);
    }

    /// <summary>
    /// Gets a value indicating whether the <paramref name="userToSave" /> is different than the data store's version of 
    /// the user passed in via <paramref name="userInDb" />.
    /// </summary>
    /// <param name="userToSave">The user to persist to the membership provider.</param>
    /// <param name="userInDb">The membership user as it exists in the data store.</param>
    /// <returns>A bool indicating whether the <paramref name="userToSave" /> is different than the one stored in the
    /// membership provider.</returns>
    private static bool HasUserBeenModified(IUserAccount userToSave, MembershipUser userInDb)
    {
      if (userToSave == null)
        throw new ArgumentNullException("userToSave");

      if (userInDb == null)
        throw new ArgumentNullException("userInDb");

      bool commentEqual = ((String.IsNullOrWhiteSpace(userToSave.Comment) && String.IsNullOrWhiteSpace(userInDb.Comment)) || userToSave.Comment == userInDb.Comment);
      bool emailEqual = ((String.IsNullOrWhiteSpace(userToSave.Email) && String.IsNullOrWhiteSpace(userInDb.Email)) || userToSave.Email == userInDb.Email);
      bool isApprovedEqual = (userToSave.IsApproved == userInDb.IsApproved);
      bool isLockEqual = (userToSave.IsLockedOut == userInDb.IsLockedOut);

      return (!(commentEqual && emailEqual && isApprovedEqual && isLockEqual));
    }

    /// <summary>
    /// Validates the user can save his own account. Throws a <see cref="GallerySecurityException" /> when the action is not allowed.
    /// </summary>
    /// <param name="userToSave">The user to save.</param>
    /// <param name="roles">The roles to associate with the user.</param>
    /// <exception cref="GallerySecurityException">Thrown when the user cannot be saved because doing so would violate a business rule.</exception>
    private static void ValidateUserCanSaveOwnAccount(IUserAccount userToSave, IEnumerable<string> roles)
    {
      // This function should be called only when the logged on person is updating their own account. They are not allowed to 
      // revoke approval and they must remain in at least one role that has Administer Site or Administer Gallery permission.
      if (!userToSave.IsApproved)
      {
        throw new GallerySecurityException(Resources.GalleryServer.Admin_Manage_Users_Cannot_Revoke_Approval_Msg);
      }

      if (!RoleController.GetGalleryServerRoles(roles).Any(role => role.AllowAdministerSite || role.AllowAdministerGallery))
      {
        throw new GallerySecurityException(Resources.GalleryServer.Admin_Manage_Users_Cannot_Save_User_Msg);
      }
    }

    /// <summary>
    /// Verifies that the specified <paramref name="userToSave" /> is not a site administrator or is being added to a site administrator
    /// role. Calling methods should invoke this function ONLY when the current user is a gallery administrator.
    /// </summary>
    /// <param name="userToSave">The user to save. The only property that must be specified is <see cref="IUserAccount.UserName" />.</param>
    /// <param name="rolesToAdd">The roles to be associated with the user. Must not be null. The roles should not already be assigned to the
    /// user, although no harm is done if they are.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userToSave" /> or <paramref name="rolesToAdd" /> is null.</exception>
    private static void VerifyGalleryAdminIsNotUpdatingUserWithAdminSitePermission(IUserAccount userToSave, IEnumerable<string> rolesToAdd)
    {
      if (userToSave == null)
        throw new ArgumentNullException("userToSave");

      if (rolesToAdd == null)
        throw new ArgumentNullException("rolesToAdd");

      IGalleryServerRoleCollection rolesAssignedOrBeingAssignedToUser = RoleController.GetGalleryServerRolesForUser(userToSave.UserName).Copy();

      foreach (string roleToAdd in rolesToAdd)
      {
        if (rolesAssignedOrBeingAssignedToUser.GetRole(roleToAdd) == null)
        {
          IGalleryServerRole role = Factory.LoadGalleryServerRole(roleToAdd);

          if (role != null)
          {
            rolesAssignedOrBeingAssignedToUser.Add(role);
          }
        }
      }

      foreach (IGalleryServerRole role in rolesAssignedOrBeingAssignedToUser)
      {
        if (role.AllowAdministerSite)
        {
          throw new GallerySecurityException("You must be a site administrator to add a user to a role with Administer site permission or update an existing user who has Administer site permission. Sadly, you are just a gallery administrator.");
        }
      }
    }

    /// <summary>
    /// Verifies the current user can add or remove the specified roles to or from a user. Specifically, the user must be a gallery
    /// administrator in every gallery each role is associated with. Calling methods should invoke this function ONLY when the current 
    /// user is a gallery administrator.
    /// </summary>
    /// <param name="rolesToAdd">The roles to be associated with the user. Must not be null. The roles should not already be assigned to the
    /// user, although no harm is done if they are.</param>
    /// <param name="rolesToRemove">The roles to remove from user.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rolesToAdd" /> or <paramref name="rolesToRemove" /> is null.</exception>
    private static void VerifyGalleryAdminCanAddOrRemoveRolesForUser(IEnumerable<string> rolesToAdd, IEnumerable<string> rolesToRemove)
    {
      if (rolesToAdd == null)
        throw new ArgumentNullException("rolesToAdd");

      if (rolesToRemove == null)
        throw new ArgumentNullException("rolesToRemove");

      IGalleryCollection adminGalleries = UserController.GetGalleriesCurrentUserCanAdminister();

      List<string> rolesBeingAddedOrRemoved = new List<string>(rolesToAdd);
      rolesBeingAddedOrRemoved.AddRange(rolesToRemove);

      foreach (string roleName in rolesBeingAddedOrRemoved)
      {
        // Gallery admin cannot add or remove a user to/from a role associated with other galleries, UNLESS he is also a gallery admin
        // to those galleries.
        IGalleryServerRole roleToAddOrRemove = Factory.LoadGalleryServerRole(roleName);

        if (roleToAddOrRemove != null)
        {
          foreach (IGallery gallery in roleToAddOrRemove.Galleries)
          {
            if (!adminGalleries.Contains(gallery))
            {
              throw new GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "You are attempting to save changes to a user that will affect multiple galleries, including at least one gallery you do not have permission to administer. Specifically, the role '{0}' applies to gallery {1}, which you are not an administrator for.", roleToAddOrRemove.RoleName, gallery.GalleryId));
            }
          }
        }
      }
    }

    /// <summary>
    /// Gets an unsorted list of users the currently logged on user can view.
    /// </summary>
    /// <param name="userIsSiteAdmin">If set to <c>true</c>, the currently logged on user is a site administrator.</param>
    /// <param name="userIsGalleryAdmin">If set to <c>true</c>, the currently logged on user is a gallery administrator for the current gallery.</param>
    /// <returns>Returns an <see cref="IUserAccountCollection"/> containing a list of roles the user has permission to view.</returns>
    private static IUserAccountCollection DetermineUsersCurrentUserCanView(bool userIsSiteAdmin, bool userIsGalleryAdmin)
    {
      if (userIsSiteAdmin || (userIsGalleryAdmin && AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles))
      {
        return UserController.GetAllUsers();
      }

      // Filter the accounts so that only users in galleries where
      // the current user is a gallery admin are shown.
      IGalleryCollection adminGalleries = UserController.GetGalleriesCurrentUserCanAdminister();

      IUserAccountCollection users = new UserAccountCollection();

      foreach (IUserAccount user in UserController.GetAllUsers())
      {
        foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser(user.UserName))
        {
          bool userHasBeenAdded = false;
          foreach (IGallery gallery in role.Galleries)
          {
            if (adminGalleries.Contains(gallery))
            {
              // User belongs to a gallery that the current user is a gallery admin for. Include the account.
              users.Add(user);
              userHasBeenAdded = true;
              break;
            }
          }
          if (userHasBeenAdded) break;
        }
      }
      return users;
    }

    private static string GetCacheKeyNameForUsersCurrentUserCanView(string userName)
    {
      return String.Concat(System.Web.HttpContext.Current.Session.SessionID, "_", userName, "_Users");
    }

    /// <summary>
    /// Determines whether the user has permission to view and edit the specified user. Determines this by checking
    /// whether the logged on user is a site administrator, the same as the user being viewed, or a gallery 
    /// administrator for at least one gallery associated with the user, or a gallery admin for ANY gallery and the 
    /// option AllowGalleryAdminToViewAllUsersAndRoles is enabled. NOTE: This function assumes the current
    /// user is a site or gallery admin, so be sure this rule is enforced at some point before persisting to
    /// the data store.
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <returns><c>true</c> if the user has permission to view and edit the specified user; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    private static bool UserCanViewAndEditUser(IUserAccount user)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      if (Utils.IsCurrentUserSiteAdministrator())
      {
        return true;
      }

      if (Utils.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase))
      {
        return true; // User can edit their own account
      }

      // Return true if any of the galleries the current user can administer is also one of the galleries the specified
      // user is associated with.
      var userIsInGalleryCurrentUserHasAdminRightsFor = RoleController.GetGalleryServerRolesForUser(Utils.UserName)
        .Any(r => r.Galleries.Any(GalleryController.GetGalleriesCurrentUserCanAdminister().Contains));

      return userIsInGalleryCurrentUserHasAdminRightsFor || (AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles && GalleryController.GetGalleriesCurrentUserCanAdminister().Any());
    }

    private static void SaveProfileProperties(Entity.User user)
    {
      if (!user.GalleryId.HasValue || user.GalleryId == int.MinValue)
        return;

      var gallerySetting = Factory.LoadGallerySetting(user.GalleryId.Value);

      if (!gallerySetting.EnableUserAlbum)
        return; // User albums are disabled system-wide, so there is nothing to save.

      // Get reference to user's album. We need to do this *before* saving the profile, because if the admin disabled the user album,
      // this method will return null after saving the profile.
      var album = UserController.GetUserAlbum(user.UserName, user.GalleryId.Value);

      var userProfile = ProfileController.GetProfile(user.UserName);
      var profile = userProfile.GetGalleryProfile(user.GalleryId.Value);

      profile.EnableUserAlbum = user.EnableUserAlbum.GetValueOrDefault();

      if (!profile.EnableUserAlbum)
      {
        profile.UserAlbumId = 0;
      }

      ProfileController.SaveProfile(userProfile);

      if (!profile.EnableUserAlbum)
      {
        AlbumController.DeleteAlbum(album);
      }
    }

    /// <summary>
    /// Check the <paramref name="user" /> for requests to reset or change the password and execute if found. An email notification
    /// is sent if requested.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="newPassword">The new password.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the GalleryId property of the user parameter is null.</exception>
    private static void HandlePasswordUpdateRequest(Entity.User user, out string newPassword)
    {
      if (!user.GalleryId.HasValue)
        throw new ArgumentOutOfRangeException("user", "The GalleryId property of the user parameter was null.");

      var pwdChange = user.PasswordChangeRequested.GetValueOrDefault();
      var pwdReset = user.PasswordResetRequested.GetValueOrDefault();

      if (pwdChange)
      {
        ChangePassword(user.UserName, GetPassword(user.UserName), user.Password);
      }

      newPassword = (pwdReset ? ResetPassword(user.UserName) : null);

      if ((pwdChange || pwdReset) && user.NotifyUserOnPasswordChange.GetValueOrDefault())
      {
        EmailController.SendNotificationEmail(user.UserName, user.Email, EmailTemplateForm.UserNotificationPasswordChangedByAdmin, false);
      }
    }

    /// <summary>
    /// Resets a user's password to a new, automatically generated password.
    /// </summary>
    /// <param name="userName">The user to reset the password for.</param>
    /// <param name="userNameIsHtmlEncoded">if set to <c>true</c>, the user name is HTML encoded.</param>
    /// <returns>The new password for the specified user.</returns>
    private static string ResetPasswordInternal(string userName, bool userNameIsHtmlEncoded = false)
    {
      try
      {
        return MembershipGsp.ResetPassword(userName, null);
      }
      catch (NullReferenceException)
      {
        // In some cases DNN stores a HTML-encoded version of the username, so if we get a NullReferenceException, try again with an HTML-encoded
        // version. This isn't really needed in the stand-alone version but it doesn't hurt and helps keep the code the same.
        if (!userNameIsHtmlEncoded)
        {
          return ResetPasswordInternal(Utils.HtmlEncode(userName), true);
        }
        else
          throw;
      }
    }

    /// <summary>
    /// Verifies that the e-mail address for the <paramref name="user" /> conforms to the expected format. No action is
    /// taken if <see cref="Entity.User.Email" /> is null or empty.
    /// </summary>
    /// <param name="user">The user to validate.</param>
    /// <exception cref="InvalidUserException">Thrown when the e-mail address is not valid.</exception>
    private static void ValidateEmail(IUserAccount user)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      if (!String.IsNullOrEmpty(user.Email) && !HelperFunctions.IsValidEmail(user.Email))
      {
        throw new InvalidUserException("E-mail is not valid.");
      }
    }

    /// <summary>
    /// Checks the URL for a user login request. Returns <c>true</c> if one is present, the requested user is not
    /// currently logged in, the user exists in the membership database, and the user is listed as an authorized
    /// auto-logon account in web.config. The requested user is assigned to the <paramref name="username" /> parameter.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="username">The username associated with the 'user' query string parameter in the URL. Assigned
    /// to an empty string if query string parameter is not present.</param>
    /// <returns><c>true</c> if the user is authorized to be auto-logged on, <c>false</c> otherwise.</returns>
    private static bool ValidateAutoLogonUserRequest(System.Web.HttpContext context, out string username)
    {
      username = Utils.GetQueryStringParameterString("user");

      if (String.IsNullOrWhiteSpace(username))
        return false;

      var user = context.User;

      if (user == null || !Utils.ParseUserName(user.Identity.Name).Equals(username, StringComparison.InvariantCultureIgnoreCase))
      {
        var userAccount = UserController.GetUser(username, false);

        if (userAccount != null)
        {
          // Found requested user account.
          username = userAccount.UserName; // Update username to fix any case differences (e.g. 'joe' in query string vs 'Joe' as account name)
          
          return ValidateAutoLogonUser(username);
        }
      }

      return false;
    }

    /// <summary>
    /// Verifies that the <paramref name="username" /> is listed as one of the authorized auto-logon accounts in web.config.
    /// </summary>
    /// <param name="username">The user name.</param>
    /// <returns><c>true</c> if the user is listed in web.config, <c>false</c> otherwise.</returns>
    private static bool ValidateAutoLogonUser(string username)
    {
      // web.config setting contains comma-separated list of allowed user names. If it contains * (wildcard), then all users are 
      // allowed except for those with a preceding hyphen.
      // Ex: "Demo,Viewer" Only user Demo and Viewer allowed to auto-login
      // Ex: "*,-Admin" All users allowed except for Admin
      var autoLoginUsersStr = System.Web.Configuration.WebConfigurationManager.AppSettings["GalleryServerAutoLogonUsers"];

      if (String.IsNullOrWhiteSpace(autoLoginUsersStr))
        return false;

      var autoLoginUsers = autoLoginUsersStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();

      var allowedUsers = new List<string>();
      var disallowedUsers = new List<string>();
      var allUsersWildcardFound = false;

      foreach (var autoLoginUser in autoLoginUsers)
      {
        if (autoLoginUser.StartsWith("-"))
        {
          disallowedUsers.Add(autoLoginUser.Substring(1));
        }
        else if (autoLoginUser == "*")
        {
          allUsersWildcardFound = true;
        }
        else
        {
          allowedUsers.Add(autoLoginUser);
        }
      }

      // If wildcard present, allow any user unless it is disallowed.
      if (allUsersWildcardFound)
      {
        return !disallowedUsers.Contains(username);
      }

      // If no wildcard, allow user only if specified
      return allowedUsers.Contains(username);
    }

    #endregion
  }
}
