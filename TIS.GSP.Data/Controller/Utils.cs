using System;
using System.Linq;
using GalleryServer.Business;

namespace GalleryServer.Data
{
  /// <summary>
  /// General purpose utilities to assist the data layer.
  /// </summary>
  public static class Utils
  {
    /// <summary>
    /// Gets the schema-qualified name for the specified <paramref name="rawName" />. For SQL Server, the name is prefixed with the schema "gsp.".
    /// For SQL CE, the value is returned without any changes. An exception is thrown for all other data providers.
    /// </summary>
    /// <param name="rawName">Name of the table.</param>
    /// <param name="galleryDataStore">The data provider for the gallery data.</param>
    /// <param name="schema">The schema. Defaults to "gsp" if not specified.</param>
    /// <returns>Returns the schema qualified name for <paramref name="rawName" />.</returns>
    /// <exception cref="System.ComponentModel.InvalidEnumArgumentException"></exception>
    public static string GetSqlName(string rawName, ProviderDataStore galleryDataStore, string schema = "gsp")
    {
      switch (galleryDataStore)
      {
        case ProviderDataStore.SqlServer:
          return String.Concat(schema, ".", rawName);
        case ProviderDataStore.SqlCe:
          return rawName;
        default:
          throw new System.ComponentModel.InvalidEnumArgumentException(string.Format("This function is not designed to handle the enum value {0}.", galleryDataStore));
      }
    }


    /// <summary>
    /// Gets the connection string settings for the connection string associated with the gallery data.
    /// </summary>
    /// <returns>An instance of <see cref="System.Configuration.ConnectionStringSettings" />.</returns>
    public static System.Configuration.ConnectionStringSettings GetConnectionStringSettings()
    {
      return System.Configuration.ConfigurationManager.ConnectionStrings.Cast<System.Configuration.ConnectionStringSettings>().First(cnStringObj => cnStringObj.Name == GetConnectionStringName());
    }

    /// <summary>
    /// Gets the name of the connection string for the gallery data.
    /// </summary>
    /// <returns>System.String.</returns>
    private static string GetConnectionStringName()
    {
      using (var repo = new GalleryRepository())
      {
        return repo.ConnectionStringName;
      }
    }


    /// <summary>
    /// Encrypt the specified string using the <see cref="System.Security.Cryptography.TripleDESCryptoServiceProvider" /> cryptographic
    /// service provider. It is expected that <see cref="Business.Interfaces.IAppSetting.EncryptionKey" /> is used for the encryption key.
    /// The encrypted string can be decrypted to its original string using the <see cref="Decrypt" /> function in this class.
    /// </summary>
    /// <param name="plainText">A plain text string to be encrypted. If the value is null or empty, the return value is
    /// equal to String.Empty.</param>
    /// <param name="encryptionKey">The encryption key.</param>
    /// <returns>Returns an encrypted version of the <paramref name="plainText" /> parameter.</returns>
    public static string Encrypt(string plainText, string encryptionKey)
    {
      if (String.IsNullOrEmpty(plainText))
        return String.Empty;

      // This method (and the Decrypt method) inspired from Code Project.
      // http://www.codeproject.com/useritems/Cryptography.asp
      byte[] stringToEncryptArray = System.Text.Encoding.UTF8.GetBytes(plainText);

      using (var tdes = new System.Security.Cryptography.TripleDESCryptoServiceProvider())
      {
        // Set the secret key for the tripleDES algorithm
        tdes.Key = System.Text.Encoding.UTF8.GetBytes(encryptionKey);

        // Mode of operation. there are other 4 modes. We choose ECB (Electronic code Book)
        tdes.Mode = System.Security.Cryptography.CipherMode.ECB;

        //padding mode(if any extra byte added)
        tdes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

        // Transform the specified region of bytes array to resultArray
        var cTransform = tdes.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(stringToEncryptArray, 0, stringToEncryptArray.Length);

        // Release resources held by TripleDes Encryptor
        tdes.Clear();

        // Return the encrypted data into unreadable string format
        return Convert.ToBase64String(resultArray);
      }
    }

    /// <summary>
    /// Decrypt the specified string using the <see cref="System.Security.Cryptography.TripleDESCryptoServiceProvider" /> cryptographic
    /// service provider. It is expected that <see cref="Business.Interfaces.IAppSetting.EncryptionKey" /> is used for the encryption key.
    /// </summary>
    /// <param name="encryptedText">A string to be decrypted. The encrypted string should have been encrypted using the
    /// <see cref="Encrypt" /> function in this class. If the value is null or empty, the return value is equal to String.Empty.</param>
    /// <param name="encryptionKey">The encryption key.</param>
    /// <returns>Returns the original, unencrypted string contained in the <paramref name="encryptedText" /> parameter.</returns>
    /// <exception cref="System.FormatException">Thrown when the text cannot be decrypted.</exception>
    public static string Decrypt(string encryptedText, string encryptionKey)
    {
      if (String.IsNullOrEmpty(encryptedText))
        return String.Empty;

      // Get the byte code of the string
      byte[] toEncryptArray = Convert.FromBase64String(encryptedText);

      using (var tdes = new System.Security.Cryptography.TripleDESCryptoServiceProvider())
      {
        // Set the secret key for the tripleDES algorithm.
        tdes.Key = System.Text.Encoding.UTF8.GetBytes(encryptionKey);

        // Mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
        tdes.Mode = System.Security.Cryptography.CipherMode.ECB;

        // Padding mode(if any extra byte added)
        tdes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

        var cTransform = tdes.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        // Release resources held by TripleDes Encryptor                
        tdes.Clear();

        // Return the Clear decrypted TEXT
        return System.Text.Encoding.UTF8.GetString(resultArray);
      }
    }
  }
}