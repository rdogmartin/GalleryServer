using System;
using System.Web;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Controller
{
  /// <summary>
  /// Contains functionality related to managing the user profile.
  /// </summary>
  public static class ProfileController
  {
    #region Public Methods

    /// <overloads>
    /// Gets the gallery-specific user profile for a user.
    /// </overloads>
    /// <summary>
    /// Gets the gallery-specific user profile for the currently logged on user and specified <paramref name="galleryId"/>.
    /// Guaranteed to not return null (returns an empty object if no profile is found).
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>Gets the profile for the current user and the specified gallery.</returns>
    public static IUserGalleryProfile GetProfileForGallery(int galleryId)
    {
      return GetProfileForGallery(Utils.UserName, galleryId);
    }

    /// <summary>
    /// Gets the gallery-specific user profile for the specified <paramref name="userName"/> and <paramref name="galleryId"/>.
    /// Guaranteed to not return null (returns an empty object if no profile is found).
    /// </summary>
    /// <param name="userName">The account name for the user whose profile settings are to be retrieved. You can specify null or an empty string
    /// for anonymous users.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>Gets the profile for the specified user and gallery.</returns>
    public static IUserGalleryProfile GetProfileForGallery(string userName, int galleryId)
    {
      return GetProfile(userName).GetGalleryProfile(galleryId);
    }

    /// <overloads>
    /// Gets a user's profile. The UserName property will be an empty string 
    /// for anonymous users and the remaining properties will be set to default values.
    /// </overloads>
    /// <summary>
    /// Gets the profile for the current user.
    /// </summary>
    /// <returns>Gets the profile for the current user.</returns>
    public static IUserProfile GetProfile()
    {
      return GetProfile(Utils.UserName);
    }

    /// <summary>
    /// Gets the user profile for the specified <paramref name="userName" />. Guaranteed to not
    /// return null (returns an empty object if no profile is found).
    /// </summary>
    /// <param name="userName">The account name for the user whose profile settings are to be retrieved. You can specify null or an empty string
    /// for anonymous users.</param>
    /// <returns>Gets the profile for the specified user.</returns>
    public static IUserProfile GetProfile(string userName)
    {
      if (String.IsNullOrEmpty(userName))
      {
        // Anonymous user. Get from session. If not found in session, return an empty object.
        return GetProfileFromSession() ?? new UserProfile();
      }
      else
      {
        return Factory.LoadUserProfile(userName);
      }
    }

    /// <summary>
    /// Saves the specified <paramref name="userProfile" />. Anonymous profiles (those with an 
    /// empty string in <see cref="IUserProfile.UserName" />) are saved to session; profiles for 
    /// users with accounts are persisted to the data store. The profile cache is automatically
    /// cleared.
    /// </summary>
    /// <param name="userProfile">The user profile to save.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userProfile" /> is null.</exception>
    public static void SaveProfile(IUserProfile userProfile)
    {
      if (userProfile == null)
        throw new ArgumentNullException("userProfile");

      if (String.IsNullOrEmpty(userProfile.UserName))
        SaveProfileToSession(userProfile);
      else
      {
        Factory.SaveUserProfile(userProfile);
      }
    }

    /// <summary>
    /// Saves the profile settings in <paramref name="user" />. Anonymous profiles (those with an 
    /// empty string in <see cref="IUserProfile.UserName" />) are saved to session; profiles for 
    /// users with accounts are persisted to the data store. The profile cache is automatically
    /// cleared.
    /// </summary>
    /// <param name="user">The user containing the profile data to save.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    /// <exception cref="Events.CustomExceptions.InvalidGalleryException">Thrown when no gallery exists for the <see cref="Entity.User.GalleryId" />
    /// property of <paramref name="user" />, or the gallery ID refers to the template gallery.</exception>
    public static void SaveProfile(Entity.User user)
    {
      if (user == null)
        throw new ArgumentNullException(nameof(user));

      // Check gallery ID. Throws InvalidGalleryException when not valid.
      Factory.LoadGallery(user.GalleryId.GetValueOrDefault());

      var profile = ProfileController.GetProfile(user.UserName);

      var userGalleryProfile = profile.GetGalleryProfile(user.GalleryId.GetValueOrDefault());

      userGalleryProfile.MediaViewSize = user.MediaViewSize;
      userGalleryProfile.SlideShowType = user.SlideShowType;
      userGalleryProfile.SlideShowLoop = user.SlideShowLoop;

      // At this time we only persist the above properties. Feel free to uncomment below if required.
      //if (user.EnableUserAlbum.HasValue)
      //  userGalleryProfile.EnableUserAlbum = user.EnableUserAlbum.Value;

      //userGalleryProfile.UserAlbumId = user.UserAlbumId;

      ProfileController.SaveProfile(profile);
    }

    /// <summary>
    /// Permanently delete the profile records for the specified <paramref name="userName" />.
    /// </summary>
    /// <param name="userName">The user name that uniquely identifies the user.</param>
    public static void DeleteProfileForUser(string userName)
    {
      Factory.DeleteUserProfile(userName);
    }

    /// <summary>
    /// Permanently delete the profile records associated with the specified <paramref name="gallery" />.
    /// </summary>
    /// <param name="gallery">The gallery.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
    public static void DeleteProfileForGallery(Business.Gallery gallery)
    {
      //Factory.GetDataProvider().Profile_DeleteProfilesForGallery(gallery.GalleryId);
      Factory.DeleteProfileForGallery(gallery);
    }

    /// <summary>
    /// Resets the profile settings for the <paramref name="userName" /> to default values. Specifically, we clear the setting for
    /// <see cref="IUserGalleryProfile.MediaViewSize" /> and <see cref="IUserGalleryProfile.SlideShowType" />, causing the user
    /// to inherit the gallery settings (or gallery control settings, if the override is enabled).
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    public static void ResetProfileSettings(string userName)
    {
      var profile = GetProfile(userName);

      // Currently the album and media profiles store rating history. Don't clear these because user could clear and re-rate items
      // repeatedly. Also there's no clear use case for a user wanting to unlink their rating from their account (remember that
      // clearing the profile data doesn't actually remove the rating, which is stored in the Metadata table).
      //profile.AlbumProfiles.Clear();
      //profile.MediaObjectProfiles.Clear();

      foreach (var galleryProfile in profile.GalleryProfiles)
      {
        galleryProfile.MediaViewSize = DisplayObjectType.Unknown;
        galleryProfile.SlideShowType = SlideShowType.NotSet;
        galleryProfile.SlideShowLoop = null;
      }

      SaveProfile(profile);
    }

    #endregion

    #region Private Functions

    /// <summary>
    /// Gets the current user's profile from session. Returns null if no object is found.
    /// </summary>
    /// <returns>Returns an instance of <see cref="IUserProfile" /> or null if no profile
    /// is found in session.</returns>
    /// <remarks>See the remarks for <see cref="SaveProfileToSession" /> for information about why we use
    /// JSON.NET during the deserialization process.</remarks>
    private static IUserProfile GetProfileFromSession()
    {
      IUserProfile pc = null;

      if (HttpContext.Current.Session != null)
      {
        // First see if we already deserialized it earlier in this HTTP call.
        var profile = HttpContext.Current.Items["Profile"];
        if (profile != null)
        {
          return (IUserProfile) profile;
        }

        // Must be first call to this function in this page life cycle or no profile has yet been saved. Look in session.
        var pcString = HttpContext.Current.Session["_Profile"] as string;

        if (!string.IsNullOrWhiteSpace(pcString))
        {
          pc = Newtonsoft.Json.JsonConvert.DeserializeObject<IUserProfile>(pcString,
            new UserProfileConverter(),
            new UserGalleryProfileConverter(),
            new AlbumProfileConverter(),
            new MediaObjectProfileConverter());

          HttpContext.Current.Items["Profile"] = pc;
        }
      }

      return pc;
    }

    /// <summary>
    /// Saves the <paramref name="userProfile" /> to session.
    /// </summary>
    /// <param name="userProfile">The user profile.</param>
    /// <remarks>The built-in serializer used by ASP.NET for storing objects in session is unable to save an
    /// instance of <see cref="IUserProfile" />, so we first use JSON.NET to serialize it to a string, then
    /// persist *that* to session.</remarks>
    private static void SaveProfileToSession(IUserProfile userProfile)
    {
      if (HttpContext.Current.Session != null)
      {
        HttpContext.Current.Session["_Profile"] = Newtonsoft.Json.JsonConvert.SerializeObject(userProfile);
      }
    }

    #endregion
  }
}
