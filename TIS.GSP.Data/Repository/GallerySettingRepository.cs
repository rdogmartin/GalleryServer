using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the GallerySetting table.
  /// </summary>
  public class GallerySettingRepository : Repository<GalleryDb, GallerySettingDto>
  {
    /// <summary>
    /// Saves the specified gallery settings.
    /// </summary>
    /// <param name="gallerySettings">The gallery settings.</param>
    /// <exception cref="System.ArgumentNullException">gallerySettings</exception>
    public void Save(IGallerySettings gallerySettings)
    {
      if (gallerySettings == null)
        throw new ArgumentNullException("gallerySettings");

      Type gsType = gallerySettings.GetType();
      string boolType = typeof(bool).ToString();
      string intType = typeof(int).ToString();
      string stringType = typeof(string).ToString();
      string stringArrayType = typeof(string[]).ToString();
      string floatType = typeof(float).ToString();
      string dateTimeType = typeof(DateTime).ToString();
      string usersType = typeof(IUserAccountCollection).ToString();
      string metadataDefType = typeof(IMetadataDefinitionCollection).ToString();
      string encoderSettingsType = typeof(IMediaEncoderSettingsCollection).ToString();

      Context.GallerySettings.Load();

      foreach (PropertyInfo prop in gsType.GetProperties())
      {
        if ((prop == null) || (prop.PropertyType.FullName == null))
        {
          continue;
        }

        string propValue;

        if (prop.PropertyType.FullName.Equals(boolType))
        {
          propValue = Convert.ToBoolean(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }
        else if (prop.PropertyType.FullName.Equals(intType))
        {
          propValue = Convert.ToInt32(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }
        else if (prop.PropertyType.FullName.Equals(stringType))
        {
          propValue = Convert.ToString(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture);
        }
        else if (prop.PropertyType.FullName.Equals(stringArrayType))
        {
          propValue = String.Join(",", (string[])prop.GetValue(gallerySettings, null));
        }
        else if (prop.PropertyType.FullName.Equals(floatType))
        {
          propValue = Convert.ToSingle(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }
        else if (prop.PropertyType.FullName.Equals(dateTimeType))
        {
          propValue = Convert.ToDateTime(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString("O", CultureInfo.InvariantCulture);
        }
        else if (prop.PropertyType.FullName.Equals(usersType))
        {
          propValue = String.Join(",", ((IUserAccountCollection)prop.GetValue(gallerySettings, null)).GetUserNames());
        }
        else if (prop.PropertyType.FullName.Equals(metadataDefType))
        {
          propValue = ((IMetadataDefinitionCollection)prop.GetValue(gallerySettings, null)).Serialize();
        }
        else if (prop.PropertyType.FullName.Equals(encoderSettingsType))
        {
          propValue = ((IMediaEncoderSettingsCollection)prop.GetValue(gallerySettings, null)).Serialize();
        }
        else
        {
          propValue = prop.GetValue(gallerySettings, null).ToString();
        }

        // Find the gallery setting in the DB and update it.
        PropertyInfo propLocal = prop;
        var gallerySettingDto = (from i in Context.GallerySettings.Local where i.FKGalleryId == gallerySettings.GalleryId && i.SettingName == propLocal.Name select i).FirstOrDefault();

        if (gallerySettingDto != null)
        {
          gallerySettingDto.SettingValue = propValue;
        }
      }

      if (gallerySettings.MediaObjectPathIsReadOnly)
      {
        // This section resolves bug#599: Error creating gallery when current gallery has read-only media objects path
        // When user saves a read only gallery, we update the template gallery to have the same settings. When a new gallery is subsequently 
        // created, the default values are likely to be ones that work and not generate an error message. Without this step, a new gallery
        // will be created with a path of gs\mediaobjects and MediaObjectPathIsReadOnly=false, which will likely fail when the code
        // checks whether the IIS app pool can write to the directory.
        Where(gs => gs.Gallery.IsTemplate && gs.SettingName == "MediaObjectPathIsReadOnly").First().SettingValue = gallerySettings.MediaObjectPathIsReadOnly.ToString(CultureInfo.InvariantCulture);
        Where(gs => gs.Gallery.IsTemplate && gs.SettingName == "MediaObjectPath").First().SettingValue = gallerySettings.MediaObjectPath;
        Where(gs => gs.Gallery.IsTemplate && gs.SettingName == "SynchAlbumTitleAndDirectoryName").First().SettingValue = gallerySettings.SynchAlbumTitleAndDirectoryName.ToString(CultureInfo.InvariantCulture);
        Where(gs => gs.Gallery.IsTemplate && gs.SettingName == "ThumbnailPath").First().SettingValue = gallerySettings.ThumbnailPath;
        Where(gs => gs.Gallery.IsTemplate && gs.SettingName == "OptimizedPath").First().SettingValue = gallerySettings.OptimizedPath;
      }

      Context.SaveChanges();
    }
  }
}

