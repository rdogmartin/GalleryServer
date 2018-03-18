using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// Contains security-related functionality.
  /// </summary>
  public static class SecurityManager
  {
    #region Public Static Methods

    /// <summary>
    /// Throws a <see cref="GallerySecurityException" /> if the user belonging to the
    /// specified <paramref name="roles" /> does not have at least one of the requested permissions for the specified album.
    /// </summary>
    /// <param name="securityRequest">Represents the permission or permissions being requested. Multiple actions can be specified by using
    /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />).
    /// If multiple actions are specified, the method is successful when the user has permission for at least one of the actions.</param>
    /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
    /// for anonymous users (i.e. <paramref name="isAuthenticated" />=false). The parameter may be null.</param>
    /// <param name="albumId">The album for which the requested permission applies.</param>
    /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in
    /// this gallery. This parameter is not required <paramref name="securityRequest" /> is SecurityActions.AdministerSite (you can specify
    /// <see cref="int.MinValue" />).</param>
    /// <param name="isAuthenticated">A value indicating whether the current user is logged in. If true, the
    /// <paramref name="roles" /> parameter must be given the names of the roles for the current user. If
    /// <paramref name="isAuthenticated" />=true and the <paramref name="roles" /> parameter
    /// is either null or an empty collection, this method thows a <see cref="GallerySecurityException" /> exception.</param>
    /// <param name="isPrivateAlbum">A value indicating whether the album is hidden from anonymous users. This parameter is ignored for
    /// logged-on users.</param>
    /// <param name="isVirtualAlbum">if set to <c>true</c> the album is a virtual album.</param>
    /// <remarks>
    /// This method handles both anonymous and logged on users. Note that when <paramref name="isAuthenticated" />=true, the <paramref name="isPrivateAlbum" /> parameter is
    /// ignored. When it is false, the <paramref name="roles" /> parameter is ignored.
    /// </remarks>
    /// <exception cref="Events.CustomExceptions.GallerySecurityException">Thrown when user is not authorized.</exception>
    public static void ThrowIfUserNotAuthorized(SecurityActions securityRequest, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isAuthenticated, bool isPrivateAlbum, bool isVirtualAlbum)
    {
      if (!(IsUserAuthorized(securityRequest, roles, albumId, galleryId, isAuthenticated, isPrivateAlbum, SecurityActionsOption.RequireOne, isVirtualAlbum)))
      {
        throw new Events.CustomExceptions.GallerySecurityException(String.Format(CultureInfo.CurrentCulture, "You do not have permission '{0}' for album ID {1}.", securityRequest.ToString(), albumId));
      }
    }

    /// <overloads>
    /// Determine if a user has permission to perform the requested action.
    /// </overloads>
    /// <summary>
    /// Determine whether the user belonging to the specified <paramref name="roles" /> has permission to perform at least one of the specified security 
    /// actions against the specified <paramref name="albumId" />. The user may be anonymous or logged on.
    /// When the the user is logged on (i.e. <paramref name="isAuthenticated"/> = true), this method determines whether the user is authorized by
    /// validating that at least one role has the requested permission to the specified album. When the user is anonymous,
    /// the <paramref name="roles"/> parameter is ignored and instead the <paramref name="isPrivateAlbum"/> parameter is used.
    /// Anonymous users do not have any access to private albums. When the the user is logged on (i.e. <paramref name="isAuthenticated"/> = true),
    /// the <paramref name="roles"/> parameter must contain the roles belonging to the user.
    /// </summary>
    /// <param name="securityRequests">Represents the permission or permissions being requested. Multiple actions can be specified by using
    /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />). 
    /// If multiple actions are specified, the method is successful if the user has permission for at least one of the actions. If you require 
    /// that all actions be satisfied to be successful, call one of the overloads that accept a <see cref="SecurityActionsOption" /> and 
    /// specify <see cref="SecurityActionsOption.RequireAll" />.</param>
    /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
    /// 	for anonymous users (i.e. <paramref name="isAuthenticated"/>=false). The parameter may be null.</param>
    /// <param name="albumId">The album for which the requested permission applies. This parameter does not apply when the requested permission
    /// 	is <see cref="SecurityActions.AdministerSite" />.</param>
    /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in this 
    /// gallery. This parameter is not required <paramref name="securityRequests" /> is SecurityActions.AdministerSite (you can specify 
    /// <see cref="int.MinValue" />).</param>
    /// <param name="isAuthenticated">A value indicating whether the current user is logged on. If true, the
    /// 	<paramref name="roles"/> parameter should contain the names of the roles for the current user. If <paramref name="isAuthenticated"/>=true
    /// 	and the <paramref name="roles"/> parameter is either null or an empty collection, this method returns false.</param>
    /// <param name="isPrivateAlbum">A value indicating whether the album is hidden from anonymous users. This parameter is ignored for
    /// 	logged-on users.</param>
    /// <param name="isVirtualAlbum">if set to <c>true</c> the album is a virtual album.</param>		/// 
    /// <returns>
    /// Returns true if the user has the requested permission; returns false if not.
    /// </returns>
    /// <remarks>This method handles both anonymous and logged on users. Note that when <paramref name="isAuthenticated"/>=true, the
    /// <paramref name="isPrivateAlbum"/> parameter is ignored. When it is false, the <paramref name="roles" /> parameter is ignored.</remarks>
    public static bool IsUserAuthorized(SecurityActions securityRequests, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isAuthenticated, bool isPrivateAlbum, bool isVirtualAlbum)
    {
      return IsUserAuthorized(securityRequests, roles, albumId, galleryId, isAuthenticated, isPrivateAlbum, SecurityActionsOption.RequireOne, isVirtualAlbum);
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
      return IsUserAuthorized(SecurityActions.AdministerSite, roles, int.MinValue, int.MinValue, true, false, false);
    }

    /// <summary>
    /// Determine whether the user belonging to the specified <paramref name="roles" /> is a gallery administrator for the specified 
    /// <paramref name="galleryId" />. The user is considered a gallery administrator if at least one role has Allow Administer Gallery permission.
    /// </summary>
    /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. The parameter may be null.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>
    /// 	<c>true</c> if the user is a gallery administrator; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsUserGalleryAdministrator(IGalleryServerRoleCollection roles, int galleryId)
    {
      return IsUserAuthorized(SecurityActions.AdministerGallery, roles, int.MinValue, galleryId, true, false, false);
    }

    /// <summary>
    /// Determine whether the user belonging to the specified <paramref name="roles" /> has permission to perform all of the specified security
    /// actions against the specified <paramref name="albumId" />. The user may be anonymous or logged on.
    /// When the the user is logged on (i.e. <paramref name="isAuthenticated" /> = true), this method determines whether the user is authorized by
    /// validating that at least one role has the requested permission to the specified album. When the user is anonymous,
    /// the <paramref name="roles" /> parameter is ignored and instead the <paramref name="isPrivateAlbum" /> parameter is used.
    /// Anonymous users do not have any access to private albums. When the the user is logged on (i.e. <paramref name="isAuthenticated" /> = true),
    /// the <paramref name="roles" /> parameter must contain the roles belonging to the user.
    /// </summary>
    /// <param name="securityRequests">Represents the permission or permissions being requested. Multiple actions can be specified by using
    /// a bitwise OR between them (example: <see cref="SecurityActions.AdministerSite" /> | <see cref="SecurityActions.AdministerGallery" />).
    /// If multiple actions are specified, use <paramref name="secActionsOption" /> to specify whether all of the actions must be satisfied
    /// to be successful or only one item must be satisfied.</param>
    /// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs. This parameter is ignored
    /// for anonymous users (i.e. <paramref name="isAuthenticated" />=false). The parameter may be null.</param>
    /// <param name="albumId">The album for which the requested permission applies. This parameter does not apply when the requested permission
    /// is <see cref="SecurityActions.AdministerSite" /> or <see cref="SecurityActions.AdministerGallery" />.</param>
    /// <param name="galleryId">The ID for the gallery the user is requesting permission in. The <paramref name="albumId" /> must exist in this
    /// gallery. This parameter is not required <paramref name="securityRequests" /> is SecurityActions.AdministerSite (you can specify
    /// <see cref="int.MinValue" />).</param>
    /// <param name="isAuthenticated">A value indicating whether the current user is logged on. If true, the
    /// <paramref name="roles" /> parameter should contain the names of the roles for the current user. If <paramref name="isAuthenticated" />=true
    /// and the <paramref name="roles" /> parameter is either null or an empty collection, this method returns false.</param>
    /// <param name="isPrivateAlbum">A value indicating whether the album is hidden from anonymous users. This parameter is ignored for
    /// logged-on users.</param>
    /// <param name="secActionsOption">Specifies whether the user must have permission for all items in <paramref name="securityRequests" />
    /// to be successful or just one. This parameter defaults to SecurityActionsOption.RequireAll when not specified, and is applicable only
    /// when <paramref name="securityRequests" /> contains more than one item.</param>
    /// <param name="isVirtualAlbum">if set to <c>true</c> the album is a virtual album.</param>
    /// <returns>
    /// Returns true if the user has the requested permission; returns false if not.
    /// </returns>
    /// <exception cref="System.ArgumentOutOfRangeException"></exception>
    /// <exception cref="System.ComponentModel.InvalidEnumArgumentException"></exception>
    /// <remarks>
    /// This method handles both anonymous and logged on users. Note that when <paramref name="isAuthenticated" />=true, the
    /// <paramref name="isPrivateAlbum" /> parameter is ignored. When it is false, the <paramref name="roles" /> parameter is ignored.
    /// </remarks>
    public static bool IsUserAuthorized(SecurityActions securityRequests, IGalleryServerRoleCollection roles, int albumId, int galleryId, bool isAuthenticated, bool isPrivateAlbum, SecurityActionsOption secActionsOption, bool isVirtualAlbum)
    {
      #region Validation

      if (isAuthenticated && !isVirtualAlbum && ((roles == null) || (roles.Count == 0)))
        return false;

      var userIsRequestingSysAdminPermission = (securityRequests & SecurityActions.AdministerSite) == SecurityActions.AdministerSite;
      var userIsRequestingGalleryAdminPermission = (securityRequests & SecurityActions.AdministerGallery) == SecurityActions.AdministerGallery;

      if (galleryId == int.MinValue)
      {
        var isMoreThanOnePermissionRequest = !SecurityActionEnumHelper.IsSingleSecurityAction(securityRequests);
        if (isMoreThanOnePermissionRequest || !userIsRequestingSysAdminPermission)
        {
          throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "A valid gallery ID must be specified. Instead, the value was {0}.", galleryId));
        }
      }

      #endregion

      if (isVirtualAlbum && (!userIsRequestingSysAdminPermission && !userIsRequestingGalleryAdminPermission))
      {
        return true; // Virtual albums are always allowed, but only for non-admin requests. This feels hacky and non-intuitive; should try to improve someday
      }

      // Handle anonymous users.
      if (!isAuthenticated)
      {
        return IsAnonymousUserAuthorized(securityRequests, isPrivateAlbum, galleryId, secActionsOption);
      }

      // If we get here we are dealing with an authenticated (logged on) user. Authorization for authenticated users is
      // given if the user is a member of at least one role that provides permission.
      if (SecurityActionEnumHelper.IsSingleSecurityAction(securityRequests))
      {
        // Iterate through each GalleryServerRole. If at least one allows the action, return true. Note that the
        // AdministerSite security action, if granted, applies to all albums and allows all actions (except HideWatermark).
        foreach (IGalleryServerRole role in roles)
        {
          if (IsAuthenticatedUserAuthorized(securityRequests, role, albumId, galleryId))
            return true;
        }
        return false;
      }
      else
      {
        // There are multiple security actions in securityRequest enum. Iterate through each one and determine if the user
        // has permission for it.
        List<bool> authResults = new List<bool>();
        foreach (SecurityActions securityAction in SecurityActionEnumHelper.ParseSecurityAction(securityRequests))
        {
          // Iterate through each role. If at least one role allows the action, permission is granted.
          foreach (IGalleryServerRole role in roles)
          {
            bool authResult = IsAuthenticatedUserAuthorized(securityAction, role, albumId, galleryId);

            authResults.Add(authResult);

            if (authResult)
              break; // We found a role that provides permission, so no need to check the other roles. Just move on to the next security request.
          }
        }

        // Determine the return value based on what the calling method wanted.
        if (secActionsOption == SecurityActionsOption.RequireAll)
        {
          return (authResults.Count > 0 ? authResults.TrueForAll(delegate(bool value) { return value; }) : false);
        }
        else if (secActionsOption == SecurityActionsOption.RequireOne)
        {
          return authResults.Contains(true);
        }
        else
        {
          throw new InvalidEnumArgumentException("secActionsOption", (int)secActionsOption, typeof(SecurityActionsOption));
        }
      }
    }

    /// <summary>
    /// Gets an object describing whether a user having the specified <paramref name="roles" /> has permission to add or edit albums
    /// and media objects in at least one album in the gallery having ID <paramref name="galleryId" />. This method works
    /// by iterating through the roles and looking at the desired permissions, so it is quite efficient.
    /// </summary>
    /// <param name="roles">The roles a user belongs to.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>An instance of <see cref="ComplexUserPermission" />.</returns>
    public static ComplexUserPermission GetUserObjectPermissions(IEnumerable<IGalleryServerRole> roles, int galleryId)
    {
      var userPerm = new ComplexUserPermission(galleryId);

      var gallery = Factory.LoadGallery(galleryId);

      foreach (var role in roles)
      {
        if (role.Galleries.Contains(gallery))
        {
          if (role.AllowAdministerSite)
          {
            userPerm.UserCanAddAlbumToAtLeastOneAlbum = true;
            userPerm.UserCanAddMediaAssetToAtLeastOneAlbum = true;
            userPerm.UserCanEditAtLeastOneAlbum = true;
            userPerm.UserCanEditAtLeastOneMediaAsset = true;
            break;
          }

          if (role.AllowAddMediaObject)
            userPerm.UserCanAddMediaAssetToAtLeastOneAlbum = true;

          if (role.AllowAddChildAlbum)
            userPerm.UserCanAddAlbumToAtLeastOneAlbum = true;

          if (role.AllowEditAlbum)
            userPerm.UserCanEditAtLeastOneAlbum = true;

          if (role.AllowEditMediaObject)
            userPerm.UserCanEditAtLeastOneMediaAsset = true;
        }
      }

      return userPerm;
    }

    #endregion

    #region Private Static Methods

    private static bool IsAnonymousUserAuthorized(SecurityActions securityRequests, bool isPrivateAlbum, int galleryId, SecurityActionsOption secActionsOption)
    {
      // Anonymous user. Return true for viewing-related permission requests on PUBLIC albums; return false for all others.
      IGallerySettings gallerySettings = Factory.LoadGallerySetting(galleryId);

      if (SecurityActionEnumHelper.IsSingleSecurityAction(securityRequests))
      {
        return IsAnonymousUserAuthorizedForSingleSecurityAction(securityRequests, isPrivateAlbum, gallerySettings);
      }
      else
      {
        return IsAnonymousUserAuthorizedForMultipleSecurityActions(securityRequests, isPrivateAlbum, gallerySettings, secActionsOption);
      }
    }

    private static bool IsAnonymousUserAuthorizedForSingleSecurityAction(SecurityActions securityRequests, bool isPrivateAlbum, IGallerySettings gallerySettings)
    {
      return (securityRequests == SecurityActions.ViewAlbumOrMediaObject) && !isPrivateAlbum && gallerySettings.AllowAnonymousBrowsing ||
        (securityRequests == SecurityActions.ViewOriginalMediaObject) && !isPrivateAlbum && gallerySettings.AllowAnonymousBrowsing && gallerySettings.EnableAnonymousOriginalMediaObjectDownload;
    }

    private static bool IsAnonymousUserAuthorizedForMultipleSecurityActions(SecurityActions securityRequests, bool isPrivateAlbum, IGallerySettings gallerySettings, SecurityActionsOption secActionsOption)
    {
      // There are multiple security actions in securityAction enum.  Iterate through each one and determine if the user
      // has permission for it.
      List<bool> authResults = new List<bool>();
      foreach (SecurityActions securityAction in SecurityActionEnumHelper.ParseSecurityAction(securityRequests))
      {
        authResults.Add(IsAnonymousUserAuthorizedForSingleSecurityAction(securityAction, isPrivateAlbum, gallerySettings));
      }

      if (secActionsOption == SecurityActionsOption.RequireAll)
      {
        return (authResults.Count > 0 ? authResults.TrueForAll(delegate(bool value) { return value; }) : false);
      }
      else if (secActionsOption == SecurityActionsOption.RequireOne)
      {
        return authResults.Contains(true);
      }
      else
      {
        throw new InvalidEnumArgumentException("secActionsOption", (int)secActionsOption, typeof(SecurityActionsOption));
      }
    }

    private static bool IsAuthenticatedUserAuthorized(SecurityActions securityRequest, IGalleryServerRole role, int albumId, int galleryId)
    {
      if (role.AllowAdministerSite && (securityRequest != SecurityActions.HideWatermark))
      {
        // Administer permissions imply permissions to carry out all other actions, except for hide watermark, which is more of 
        // a preference assigned to the user.
        return true;
      }

      switch (securityRequest)
      {
        case SecurityActions.AdministerSite: if (role.AllowAdministerSite) return true; break;
        case SecurityActions.AdministerGallery: if (role.AllowAdministerGallery && (role.Galleries.FindById(galleryId) != null)) return true; break;
        case SecurityActions.ViewAlbumOrMediaObject: if (role.AllowViewAlbumOrMediaObject && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.ViewOriginalMediaObject: if (role.AllowViewOriginalImage && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.AddChildAlbum: if (role.AllowAddChildAlbum && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.AddMediaObject: if (role.AllowAddMediaObject && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.DeleteAlbum:
          {
            // It is OK to delete the album if the AllowDeleteChildAlbum permission is true and one of the following is true:
            // 1. The album is the root album and its ID is in the list of targeted albums (Note that we never actually delete the root album.
            //    Instead, we delete all objects within the album. But the idea of deleting the top level album to clear out all objects in the
            //    gallery is useful to the user.)
            // 2. The album is not the root album and its parent album's ID is in the list of targeted albums.
            if (role.AllowDeleteChildAlbum)
            {
              IAlbum album = Factory.LoadAlbumInstance(albumId);
              if (album.IsRootAlbum)
              {
                if (role.AllAlbumIds.Contains(album.Id)) return true; break;
              }
              else
              {
                if (role.AllAlbumIds.Contains(album.Parent.Id)) return true; break;
              }
            }
            break;
          }
        case SecurityActions.DeleteChildAlbum: if (role.AllowDeleteChildAlbum && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.DeleteMediaObject: if (role.AllowDeleteMediaObject && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.EditAlbum: if (role.AllowEditAlbum && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.EditMediaObject: if (role.AllowEditMediaObject && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.HideWatermark: if (role.HideWatermark && role.AllAlbumIds.Contains(albumId)) return true; break;
        case SecurityActions.Synchronize: if (role.AllowSynchronize && role.AllAlbumIds.Contains(albumId)) return true; break;
        default: throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "The IsUserAuthorized function is not designed to handle the {0} SecurityActions. It must be updated by a developer.", securityRequest.ToString()));
      }
      return false;
    }

    #endregion
  }

  /// <summary>
  /// A data entity containing information about a user's permission set in a gallery.
  /// </summary>
  public class ComplexUserPermission
  {
    /// <summary>
    /// Prevents a default instance of the <see cref="ComplexUserPermission"/> class from being created.
    /// </summary>
    private ComplexUserPermission()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComplexUserPermission"/> class.
    /// </summary>
    /// <param name="galleryId">The gallery identifier.</param>
    public ComplexUserPermission(int galleryId)
    {
      GalleryId = galleryId;
    }

    /// <summary>
    /// Gets or sets the ID of the gallery this instance refers to.
    /// </summary>
    public int GalleryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user has permision to add an album to at least one album in the gallery.
    /// </summary>
    public bool UserCanAddAlbumToAtLeastOneAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user has permision to add a media asset to at least one album in the current gallery.
    /// </summary>
    public bool UserCanAddMediaAssetToAtLeastOneAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user can edit at least one album in the gallery.
    /// </summary>
    public bool UserCanEditAtLeastOneAlbum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a user can edit at least one media asset in the gallery.
    /// </summary>
    public bool UserCanEditAtLeastOneMediaAsset { get; set; }
  }
}
