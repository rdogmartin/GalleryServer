using System;
using GalleryServer.Data;
using Newtonsoft.Json;

namespace GalleryServer.Business.Entity
{
  /// <summary>
  /// Represents a version key. A version key is an encrypted string included with commercial versions of Gallery Server. It is
  /// delivered as a text file named version_key.txt that is copied to the App_Data directory and subsequently copied to the
  /// <see cref="AppSetting.VersionKey" /> property. The encrypted string can be decoded into a <see cref="ProductId" /> and 
  /// <see cref="Version" />.
  /// </summary>
  public class VersionKey
  {
    /// <summary>
    /// Gets or sets the license purchased by the user.
    /// </summary>
    public LicenseLevel ProductId { get; set; }

    /// <summary>
    /// Gets or sets the application version the license applies to. Example: "4.0.1"
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Creates an instance of <see cref="VersionKey" /> from <paramref name="versionKeyString" />.
    /// </summary>
    /// <param name="versionKeyString">The version key string. Ex: S9Z/5Qxd710GVVjgsQiN4/c1FXU4s2Gf8u/VX3eU3eXe8uWBtac5UQ==</param>
    /// <returns>VersionKey.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="versionKeyString" /> cannot be decrypted to a JSON string.</exception>
    /// <exception cref="ArgumentException">Thrown when the string is not valid JSON.</exception>
    /// <exception cref="InvalidCastException">Thrown when <paramref name="versionKeyString" /> cannot be converted from a JSON string to an
    ///  instance of <see cref="VersionKey" />.</exception>
    public static VersionKey ToVersionKey(string versionKeyString)
    {
      string versionKeyJson;
      try
      {
        versionKeyJson = Utils.Decrypt(versionKeyString, GlobalConstants.ENCRYPTION_KEY);
      }
      catch (FormatException ex)
      {
        ex.Data.Add("EncryptedVersionKey", versionKeyString);

        throw;
      }
      try
      {
        return JsonConvert.DeserializeObject<VersionKey>(versionKeyJson);
      }
      catch (ArgumentException ex)
      {
        ex.Data.Add("VersionKeyJson", versionKeyJson);

        throw;
      }
      catch (InvalidCastException ex)
      {
        ex.Data.Add("VersionKeyJson", versionKeyJson);

        throw;
      }
    }
  }
}