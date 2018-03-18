using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;
using Newtonsoft.Json;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a profile for a user in the current application.
  /// </summary>
  [Serializable]
  public class UserProfile : IUserProfile, IComparable
  {
    #region Private Fields

    private const string ProfileNameEnableUserAlbum = "EnableUserAlbum";
    private const string ProfileNameUserAlbumId = "UserAlbumId";
    private const string ProfileNameAlbumProfiles = "AlbumProfiles";
    private const string ProfileNameMediaObjectProfiles = "MediaObjectProfiles";
    private const string ProfileNameMediaViewSize = "MediaViewSize";
    private const string ProfileNameSlideShowType = "SlideShowType";
    private const string ProfileNameSlideShowLoop = "SlideShowLoop";
    private readonly IUserGalleryProfileCollection _galleryProfiles;
    private readonly IAlbumProfileCollection _albumProfiles;
    private readonly IMediaObjectProfileCollection _mediaObjectProfiles;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the account name of the user these profile settings belong to.
    /// </summary>
    /// <value>The account name of the user.</value>
    public string UserName { get; set; }

    /// <summary>
    /// Gets a collection of album preferences for this user. Guaranteed to not return null.
    /// </summary>
    /// <value>An instance of <see cref="IAlbumProfileCollection" />.</value>
    public IAlbumProfileCollection AlbumProfiles
    {
      get { return _albumProfiles; }
    }

    /// <summary>
    /// Gets a collection of media object preferences for this user. Guaranteed to not return null.
    /// </summary>
    /// <value>An instance of <see cref="IMediaObjectProfileCollection" />.</value>
    public IMediaObjectProfileCollection MediaObjectProfiles
    {
      get { return _mediaObjectProfiles; }
    }

    /// <summary>
    /// Gets the collection of gallery profiles for the user. A gallery profile is a set of properties for a user that 
    /// are specific to a particular gallery. Guaranteed to not return null.
    /// </summary>
    /// <value>The gallery profiles.</value>
    public IUserGalleryProfileCollection GalleryProfiles
    {
      get { return _galleryProfiles; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfile" /> class.
    /// </summary>
    public UserProfile()
    {
      _galleryProfiles = new UserGalleryProfileCollection();
      _albumProfiles = new AlbumProfileCollection();
      _mediaObjectProfiles = new MediaObjectProfileCollection();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves the profile for the specified <paramref name="userName" />. Guaranteed to not return null.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <returns>An instance of <see cref="IUserProfile" />.</returns>
    public static IUserProfile RetrieveFromDataStore(string userName)
    {
      IUserProfile profile = new UserProfile();
      profile.UserName = userName;

      IUserGalleryProfile gs = null;
      int prevGalleryId = int.MinValue;

      using (var repo = new ProfileRepository())
      {
        foreach (var profileDto in (repo.Where(p => p.UserName == userName, p => p.Gallery).OrderBy(p => p.FKGalleryId)))
        {
          // Loop through each user profile setting and assign to the relevant property. When we encounter a record with a new gallery ID, 
          // automatically create a new UserGalleryProfile instance and start populating that one. When we are done with the loop we will
          // have created one UserGalleryProfile instance for each gallery the user has a profile for.

          #region Check for application-wide profile setting

          if (profileDto.Gallery.IsTemplate)
          {
            // Profile items associated with the template gallery are application-wide and map to properties
            // on the UserProfile object.
            switch (profileDto.SettingName.Trim())
            {
              case ProfileNameEnableUserAlbum:
              case ProfileNameUserAlbumId:
                throw new DataException(String.Format("It is invalid for the profile setting '{0}' to be associated with a template gallery (Gallery ID {1}).", profileDto.SettingName, profileDto.FKGalleryId));

              case ProfileNameAlbumProfiles:
                var albumProfiles = JsonConvert.DeserializeObject<List<AlbumProfile>>(profileDto.SettingValue.Trim());

                if (albumProfiles != null)
                {
                  profile.AlbumProfiles.AddRange(albumProfiles);
                }

                break;

              case ProfileNameMediaObjectProfiles:
                var moProfiles = JsonConvert.DeserializeObject<List<MediaObjectProfile>>(profileDto.SettingValue.Trim());

                if (moProfiles != null)
                {
                  profile.MediaObjectProfiles.AddRange(moProfiles);
                }

                break;
            }

            continue;
          }

          #endregion

          #region Check for new gallery

          int currGalleryId = profileDto.FKGalleryId;

          if ((gs == null) || (!currGalleryId.Equals(prevGalleryId)))
          {
            // We have encountered settings for a new user gallery profile. Create a new object and add it to our collection.
            gs = profile.GalleryProfiles.CreateNewUserGalleryProfile(currGalleryId);

            profile.GalleryProfiles.Add(gs);

            prevGalleryId = currGalleryId;
          }

          #endregion

          #region Assign property

          // For each setting in the data store, find the matching property and assign the value to it.
          switch (profileDto.SettingName.Trim())
          {
            case ProfileNameEnableUserAlbum:
              gs.EnableUserAlbum = Convert.ToBoolean(profileDto.SettingValue.Trim(), CultureInfo.InvariantCulture);
              break;

            case ProfileNameUserAlbumId:
              gs.UserAlbumId = Convert.ToInt32(profileDto.SettingValue.Trim(), CultureInfo.InvariantCulture);
              break;

            case ProfileNameMediaViewSize:
              gs.MediaViewSize = DisplayObjectTypeEnumHelper.ParseDisplayObjectType(profileDto.SettingValue);
              break;

            case ProfileNameSlideShowType:
              gs.SlideShowType = SlideShowTypeEnumHelper.ParseSlideShowType(profileDto.SettingValue);
              break;

            case ProfileNameSlideShowLoop:
              gs.SlideShowLoop = profileDto.SettingValue.Trim().ToNullable<bool>();
              break;

            case ProfileNameAlbumProfiles:
            case ProfileNameMediaObjectProfiles:
              throw new DataException(String.Format("It is invalid for the profile setting '{0}' to be associated with a non-template gallery (Gallery ID {1}).", profileDto.SettingName, profileDto.FKGalleryId));
          }

          #endregion
        }
      }

      return profile;
    }


    /// <summary>
    /// Persist the specified <paramref name="profile"/> to the data store.
    /// </summary>
    /// <param name="profile">The profile to persist to the data store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="profile" /> is null.</exception>
    public static void Save(IUserProfile profile)
    {
      if (profile == null)
        throw new ArgumentNullException("profile");

      using (var repo = new ProfileRepository())
      {
        // AlbumProfiles
        var pDto = (repo.Where(p => p.UserName == profile.UserName && p.SettingName == ProfileNameAlbumProfiles)).FirstOrDefault();

        var templateGalleryId = GetTemplateGalleryId();

        if (pDto == null)
        {
          pDto = new UserGalleryProfileDto
          {
            UserName = profile.UserName,
            FKGalleryId = templateGalleryId,
            SettingName = ProfileNameAlbumProfiles,
            SettingValue = profile.AlbumProfiles.Serialize()
          };

          repo.Add(pDto);
        }
        else
        {
          pDto.SettingValue = profile.AlbumProfiles.Serialize();
        }

        // Media Object Profiles
        pDto = (repo.Where(p => p.UserName == profile.UserName && p.SettingName == ProfileNameMediaObjectProfiles)).FirstOrDefault();

        if (pDto == null)
        {
          pDto = new UserGalleryProfileDto
          {
            UserName = profile.UserName,
            FKGalleryId = templateGalleryId,
            SettingName = ProfileNameMediaObjectProfiles,
            SettingValue = profile.MediaObjectProfiles.Serialize()
          };

          repo.Add(pDto);
        }
        else
        {
          pDto.SettingValue = profile.MediaObjectProfiles.Serialize();
        }

        // User Gallery Profiles
        foreach (IUserGalleryProfile userGalleryProfile in profile.GalleryProfiles)
        {
          IUserGalleryProfile ugp = userGalleryProfile;

          // EnableUserAlbum
          pDto = (repo.Where(p => p.UserName == profile.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == ProfileNameEnableUserAlbum)).FirstOrDefault();

          if (pDto == null)
          {
            pDto = new UserGalleryProfileDto
                     {
                       UserName = profile.UserName,
                       FKGalleryId = ugp.GalleryId,
                       SettingName = ProfileNameEnableUserAlbum,
                       SettingValue = ugp.EnableUserAlbum.ToString(CultureInfo.InvariantCulture)
                     };

            repo.Add(pDto);
          }
          else
          {
            pDto.SettingValue = ugp.EnableUserAlbum.ToString(CultureInfo.InvariantCulture);
          }

          // UserAlbumId
          pDto = (repo.Where(p => p.UserName == profile.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == ProfileNameUserAlbumId)).FirstOrDefault();

          if (pDto == null)
          {
            pDto = new UserGalleryProfileDto
                     {
                       UserName = profile.UserName,
                       FKGalleryId = ugp.GalleryId,
                       SettingName = ProfileNameUserAlbumId,
                       SettingValue = ugp.UserAlbumId.ToString(CultureInfo.InvariantCulture)
                     };

            repo.Add(pDto);
          }
          else
          {
            pDto.SettingValue = ugp.UserAlbumId.ToString(CultureInfo.InvariantCulture);
          }

          // MediaViewSize
          pDto = (repo.Where(p => p.UserName == profile.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == ProfileNameMediaViewSize)).FirstOrDefault();

          if (pDto == null)
          {
            pDto = new UserGalleryProfileDto
                     {
                       UserName = profile.UserName,
                       FKGalleryId = ugp.GalleryId,
                       SettingName = ProfileNameMediaViewSize,
                       SettingValue = ugp.MediaViewSize.ToString()
                     };

            repo.Add(pDto);
          }
          else
          {
            pDto.SettingValue = ugp.MediaViewSize.ToString();
          }

          // SlideShowType
          pDto = (repo.Where(p => p.UserName == profile.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == ProfileNameSlideShowType)).FirstOrDefault();

          if (pDto == null)
          {
            pDto = new UserGalleryProfileDto
                     {
                       UserName = profile.UserName,
                       FKGalleryId = ugp.GalleryId,
                       SettingName = ProfileNameSlideShowType,
                       SettingValue = ugp.SlideShowType.ToString()
                     };

            repo.Add(pDto);
          }
          else
          {
            pDto.SettingValue = ugp.SlideShowType.ToString();
          }

          // SlideShowLoop
          pDto = (repo.Where(p => p.UserName == profile.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == ProfileNameSlideShowLoop)).FirstOrDefault();

          if (pDto == null)
          {
            pDto = new UserGalleryProfileDto
            {
              UserName = profile.UserName,
              FKGalleryId = ugp.GalleryId,
              SettingName = ProfileNameSlideShowLoop,
              SettingValue = ugp.SlideShowLoop?.ToString(CultureInfo.InvariantCulture) ?? "null"
            };

            repo.Add(pDto);
          }
          else
          {
            pDto.SettingValue = ugp.SlideShowLoop?.ToString(CultureInfo.InvariantCulture) ?? "null";
          }

        }

        repo.Save();
      }
    }

    /// <summary>
    /// Gets the ID of the template gallery.
    /// </summary>
    /// <returns>System.Int32.</returns>
    private static int GetTemplateGalleryId()
    {
      using (var repo = new GalleryRepository())
      {
        return repo.Where(g => g.IsTemplate).Single().GalleryId;
      }
    }

    /// <summary>
    /// Gets the gallery profile for the specified <paramref name="galleryId" />. Guaranteed to not return null.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>A <see cref="IUserGalleryProfile" /> containing profile information.</returns>
    public IUserGalleryProfile GetGalleryProfile(int galleryId)
    {
      IUserGalleryProfile profile = GalleryProfiles.FindByGalleryId(galleryId);

      if (profile == null)
      {
        profile = CreateDefaultProfile(galleryId);

        GalleryProfiles.Add(profile);
      }

      return profile;
    }

    /// <summary>
    /// Creates a new instance containing a deep copy of the items it contains.
    /// </summary>
    /// <returns>Returns a new instance containing a deep copy of the items it contains.</returns>
    public IUserProfile Copy()
    {
      IUserProfile copy = new UserProfile();

      copy.UserName = UserName;
      copy.GalleryProfiles.AddRange(GalleryProfiles.Copy());
      copy.AlbumProfiles.AddRange(AlbumProfiles.Copy());
      copy.MediaObjectProfiles.AddRange(MediaObjectProfiles.Copy());

      return copy;
    }

    #endregion

    #region Private Functions

    private IUserGalleryProfile CreateDefaultProfile(int galleryId)
    {
      IUserGalleryProfile profile = new UserGalleryProfile(galleryId);
      profile.UserAlbumId = 0; // Redundant since this is the default value, but this is for clarity to programmer
      profile.EnableUserAlbum = Factory.LoadGallerySetting(galleryId).EnableUserAlbumDefaultForUser;

      return profile;
    }

    #endregion

    #region IComparable

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
    /// </returns>
    /// <exception cref="T:System.ArgumentException">
    /// 	<paramref name="obj"/> is not the same type as this instance. </exception>
    public int CompareTo(object obj)
    {
      if (obj == null)
        return 1;
      else
      {
        IUserProfile other = obj as IUserProfile;
        if (other != null)
          return String.Compare(this.UserName, other.UserName, StringComparison.Ordinal);
        else
          return 1;
      }
    }

    #endregion
  }

  /// <summary>
  /// Provides functionality that assists JSON.NET during deserialization.
  /// </summary>
  public class UserProfileConverter : Newtonsoft.Json.Converters.CustomCreationConverter<IUserProfile>
  {
    /// <summary>
    /// Creates the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>An instance of <see cref="IUserProfile" />.</returns>
    public override IUserProfile Create(Type objectType)
    {
      return new UserProfile();
    }
  }

  /// <summary>
  /// Provides functionality that assists JSON.NET during deserialization.
  /// </summary>
  public class UserGalleryProfileConverter : Newtonsoft.Json.Converters.CustomCreationConverter<IUserGalleryProfile>
  {
    /// <summary>
    /// Creates the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>An instance of <see cref="IUserGalleryProfile" />.</returns>
    public override IUserGalleryProfile Create(Type objectType)
    {
      return new UserGalleryProfile();
    }
  }

  /// <summary>
  /// Provides functionality that assists JSON.NET during deserialization.
  /// </summary>
  public class AlbumProfileConverter : Newtonsoft.Json.Converters.CustomCreationConverter<IAlbumProfile>
  {
    /// <summary>
    /// Creates the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>An instance of <see cref="IAlbumProfile" />.</returns>
    public override IAlbumProfile Create(Type objectType)
    {
      return new AlbumProfile();
    }
  }

  /// <summary>
  /// Provides functionality that assists JSON.NET during deserialization.
  /// </summary>
  public class MediaObjectProfileConverter : Newtonsoft.Json.Converters.CustomCreationConverter<IMediaObjectProfile>
  {
    /// <summary>
    /// Creates the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>An instance of <see cref="IAlbumProfile" />.</returns>
    public override IMediaObjectProfile Create(Type objectType)
    {
      return new  MediaObjectProfile();
    }
  }
}
