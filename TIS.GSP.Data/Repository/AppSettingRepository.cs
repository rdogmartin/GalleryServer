using System;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the AppSetting table.
  /// </summary>
  public class AppSettingRepository : Repository<GalleryDb, AppSettingDto>
  {
    /// <summary>
    /// Persist the current application settings to the data store.
    /// </summary>
    /// <param name="appSetting">An instance of <see cref="IAppSetting"/> to persist to the data store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="appSetting" /> is null.</exception>
    public void Save(IAppSetting appSetting)
    {
      if (appSetting == null)
        throw new ArgumentNullException("appSetting");

      var asType = appSetting.GetType();

      // Specify the list of properties we want to save.
      var propertiesToSave = new[] { "InstallDateEncrypted", "Skin", "MediaObjectDownloadBufferSize", "EncryptMediaObjectUrlOnClient", "EncryptionKey", 
                                     "JQueryScriptPath", "JQueryMigrateScriptPath", "JQueryUiScriptPath", "ImageMagickPath", "MembershipProviderName", 
                                     "RoleProviderName", "LicenseEmail", "LicenseKey", "VersionKey", "InstanceId", "EnableCache", 
                                     "AllowGalleryAdminToManageUsersAndRoles", "AllowGalleryAdminToViewAllUsersAndRoles", "MaxNumberErrorItems", 
                                     "EmailFromName", "EmailFromAddress", "SmtpServer", "SmtpServerPort", "SendEmailUsingSsl", "CustomCss"};

      var boolType = typeof(bool).ToString();
      var intType = typeof(int).ToString();
      var stringType = typeof(string).ToString();

      Context.AppSettings.Load();

      foreach (var prop in asType.GetProperties())
      {
        if ((prop == null) || (prop.PropertyType.FullName == null))
        {
          continue;
        }

        if (Array.IndexOf(propertiesToSave, prop.Name) >= 0)
        {
          // This is one of the properties we want to save.
          string propValue;

          if (prop.PropertyType.FullName.Equals(boolType))
          {
            propValue = Convert.ToBoolean(prop.GetValue(appSetting, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
          }
          else if (prop.PropertyType.FullName.Equals(intType))
          {
            propValue = Convert.ToInt32(prop.GetValue(appSetting, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
          }
          else if (prop.PropertyType.FullName.Equals(stringType))
          {
            propValue = Convert.ToString(prop.GetValue(appSetting, null), CultureInfo.InvariantCulture);
          }
          else
          {
            propValue = prop.GetValue(appSetting, null).ToString();
          }

          // Find the app setting in the DB and update it.
          var appSettingDto = Context.AppSettings.Local.FirstOrDefault(a => a.SettingName == prop.Name);

          if (appSettingDto != null)
          {
            appSettingDto.SettingValue = propValue;
          }
          else
          {
            throw new DataException(String.Format(CultureInfo.CurrentCulture, "Cannot update application setting. No record was found in AppSetting with SettingName='{0}'.", prop.Name));
          }
        }
      }

      Save();
    }
  }
}