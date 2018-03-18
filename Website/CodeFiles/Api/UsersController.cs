using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using GalleryServer.Business;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
  /// <summary>
  /// Contains methods for Web API access to users.
  /// </summary>
  public class UsersController : ApiController
  {
    /// <summary>
    /// Gets the user with the specified <paramref name="userName" />.
    /// </summary>
    /// <param name="userName">The name of the user to retrieve.</param>
    /// <param name="galleryId">The gallery ID. Required for retrieving the correct user album ID.</param>
    /// <returns>An instance of <see cref="Entity.User" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException"></exception>
    /// <exception cref="HttpResponseMessage"></exception>
    [ActionName("GetByUserName")]
    public Entity.User Get(string userName, int galleryId)
    {
      // GET /api/users/getbyusername?userName=Admin&amp;galleryId=1
      try
      {
        return UserController.GetUserEntity(userName, galleryId);
      }
      catch (GallerySecurityException)
      {
        // This is thrown when the current user does not have view and edit permission to the requested user.
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
      }
      catch (InvalidUserException)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                                          {
                                            Content = new StringContent(String.Format("User '{0}' does not exist", userName)),
                                            ReasonPhrase = "User Not Found"
                                          });
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                          {
                                            Content = Utils.GetExStringContent(ex),
                                            ReasonPhrase = "Server Error"
                                          });
      }
    }

    /// <summary>
    /// Gets a value indicating whether the <paramref name="userName" /> represents an existing user.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <returns><c>true</c> if the user exists, <c>false</c> otherwise</returns>
    [ActionName("Exists")]
    public bool Get(string userName)
    {
      return (UserController.GetUser(userName, false) != null);
    }

    /// <summary>
    /// Persists the <paramref name="user" /> to the data store. The user can be an existing one or a new one to be
    /// created.
    /// </summary>
    /// <param name="user">The role.</param>
    /// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the requested action is not successful.</exception>
    public HttpResponseMessage Post(Entity.User user)
    {
      // POST /api/users/post
      try
      {
        string newPwd = null;

        if (user.IsNew.GetValueOrDefault())
        {
          UserController.CreateUser(user);
        }
        else
        {
          UserController.SaveUser(user, out newPwd);
        }

        var msg = new StringContent(String.Format(CultureInfo.CurrentCulture, "User '{0}' has been saved.{1}",
          Utils.HtmlEncode(user.UserName),
          user.PasswordResetRequested.GetValueOrDefault() ? String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Admin_Manage_Users_New_Pwd_Text, newPwd) : String.Empty
          ));

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = msg };
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        // Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user.
        if (user.IsNew.GetValueOrDefault() && UserController.GetUser(user.UserName, false) != null)
        {
          UserController.DeleteUser(user.UserName);
        }
        
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
          Content = new StringContent(ex.Message),
          ReasonPhrase = "Action Forbidden"
        });
      }
      catch (InvalidUserException ex)
      {
        AppEventController.LogError(ex);

        // Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user.
        if (user.IsNew.GetValueOrDefault() && UserController.GetUser(user.UserName, false) != null)
        {
          UserController.DeleteUser(user.UserName);
        }

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = new StringContent(ex.Message),
          ReasonPhrase = "Invalid User"
        });
      }
      catch (MembershipCreateUserException ex)
      {
        AppEventController.LogError(ex);

        // Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user,
        // but only if the user exists AND the error wasn't 'DuplicateUserName'.
        if (user.IsNew.GetValueOrDefault() && (ex.StatusCode != MembershipCreateStatus.DuplicateUserName) && (UserController.GetUser(user.UserName, false) != null))
        {
          UserController.DeleteUser(user.UserName);
        }

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = new StringContent(UserController.GetAddUserErrorMessage(ex.StatusCode)),
          ReasonPhrase = "Cannot Create User"
        });
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);

        // Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user.
        if (user.IsNew.GetValueOrDefault() && UserController.GetUser(user.UserName, false) != null)
        {
          UserController.DeleteUser(user.UserName);
        }

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
      finally
      {
        CacheController.RemoveCache(CacheItem.GalleryServerRoles);
      }
    }

    /// <summary>
    /// Persist the profile properties of the <paramref name="user" /> to the profile store. The UserName property of 
    /// <paramref name="user" /> must match the currently logged on user's username.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
    [HttpPost]
    [ActionName("CurrentUserProfile")]
    public HttpResponseMessage CurrentUserProfile(Entity.User user)
    {
      // POST /api/users/currentuserprofile
      try
      {
        if ((!string.IsNullOrWhiteSpace(Utils.UserName) || !string.IsNullOrWhiteSpace(user.UserName)) && !Utils.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase))
        {
          throw new GallerySecurityException("Cannot save profile because specified username does not match currently logged on username.");
        }

        ProfileController.SaveProfile(user);

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"Profile updated for user {user.UserName}...") };
      }
      catch (GallerySecurityException ex)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }

    /// <summary>
    /// Resets the profile settings for the <paramref name="userName" /> to default values. The <paramref name="userName" />
    /// must match the currently logged on user's username.
    /// </summary>
    /// <param name="userName">The username.</param>
    /// <returns>An instance of <see cref="ActionResult" />.</returns>
    [HttpPost]
    [ActionName("ClearUserProfile")]
    public ActionResult ClearUserProfile(string userName)
    {
      // POST /api/users/clearuserprofile
      try
      {
        if ((!string.IsNullOrWhiteSpace(Utils.UserName) || !string.IsNullOrWhiteSpace(userName)) && !Utils.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
        {
          throw new GallerySecurityException("Cannot clear profile because specified username does not match currently logged on username.");
        }

        ProfileController.ResetProfileSettings(userName);

        return new ActionResult
        {
          Status = ActionResultStatus.Success.ToString(),
          Title = $"Successfully reset profile settings for {userName}",
          Message = string.Empty
        };
      }
      catch (GallerySecurityException ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }

    /// <summary>
    /// Permanently delete the <paramref name="userName" /> from the data store.
    /// </summary>
    /// <param name="userName">The name of the user to be deleted.</param>
    /// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when the requested action is not successful.</exception>
    [ActionName("DeleteByUserName")]
    [HttpDelete]
    public HttpResponseMessage Delete(string userName)
    {
      // DELETE /api/users
      try
      {
        // Don't need to check security here because we'll do that in RoleController.DeleteGalleryServerProRole.
        UserController.DeleteGalleryServerProUser(userName, true);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
          Content = new StringContent(String.Format(CultureInfo.CurrentCulture, "User '{0}' has been deleted", Utils.HtmlEncode(userName)))
        };
      }
      catch (GallerySecurityException ex)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
          Content = new StringContent(ex.Message),
          ReasonPhrase = "Action Forbidden"
        });
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);

        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
          Content = Utils.GetExStringContent(ex),
          ReasonPhrase = "Server Error"
        });
      }
    }
  }
}