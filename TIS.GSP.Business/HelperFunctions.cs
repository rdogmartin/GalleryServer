using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Provides general helper functions.
    /// </summary>
    public static class HelperFunctions
    {
        #region Private Fields

        private static readonly object _fileLock = new object(); // Used in ValidatePhysicalPathExistsAndIsReadWritable()

        #endregion

        #region Constructors

        #endregion

        #region Extensions

        /// <summary>
        /// Separates the comma-separated string into a collection of individual string values.
        /// Leading and trailing spaces of each item are trimmed.
        /// </summary>
        /// <param name="value">The value. It is expected to be a comma-delimited string 
        /// (e.g. "dog, cat, house").</param>
        /// <returns>Returns a collection of strings, or null when <paramref name="value" /> is null.</returns>
        public static List<string> ToListFromCommaDelimited(this string value)
        {
            if (value == null)
                return null;

            var items = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var rv = new List<string>(items.Length);

            foreach (var item in items)
            {
                rv.Add(item.Trim());
            }

            return rv;
        }

        /// <summary>
        /// Converts the string to an instance of <see cref="DateTime" /> using the specified <paramref name="format" />.
        /// If the value cannot be converted to a <see cref="DateTime" />, the value <see cref="DateTime.MinValue" />
        /// is returned.
        /// </summary>
        /// <param name="value">The value to convert to <see cref="System.DateTime"/>.</param>
        /// <param name="format">The expected format of <paramref name="value" />.</param>
        /// Returns a <see cref="System.DateTime"/> value.
        public static DateTime ToDateTime(this string value, string format)
        {
            return ToDateTime(value, format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert the string to the requested nullable type.
        /// </summary>
        /// <typeparam name="T">The type of object to convert the string to.</typeparam>
        /// <param name="s">The string to convert.</param>
        /// <returns>System.Nullable&lt;T&gt;.</returns>
        public static T? ToNullable<T>(this string s) where T : struct
        {
            var result = new T?();
            try
            {
                if (!string.IsNullOrEmpty(s) && s.Trim().Length > 0 && !s.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    var convertFrom = TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(s);
                    if (convertFrom != null) result = (T)convertFrom;
                }
            }
            catch (ArgumentNullException) { }
            return result;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Returns true if the object is a valid System.Int32 value; otherwise returns false.
        /// </summary>
        /// <param name="value">The parameter to test whether it is a System.Int32.</param>
        /// <returns>Returns true if the object is a valid System.Int32 value; otherwise returns false.</returns>
        public static bool IsInt32(object value)
        {
            if (value == null)
                return false;

            int result;
            return Int32.TryParse(value.ToString(), out result);
        }

        /// <summary>
        /// Returns the first matching set of integers in the <paramref name="input" />, returning <see cref="Int32.MinValue" />
        /// if no matches are found. Example: "Size: 704 px x 480 px" will return 704.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>An instance of <see cref="System.Int32" />.</returns>
        public static int ParseInteger(string input)
        {
            foreach (Match match in Regex.Matches(input, @"\d+"))
            {
                return Int32.Parse(match.Value);
            }

            return Int32.MinValue;
        }

        /// <summary>
        /// Returns true if the object is a valid System.Boolean value; otherwise returns false.
        /// </summary>
        /// <param name="value">The parameter to test whether it is a System.Boolean value.</param>
        /// <returns>Returns true if the object is a valid System.Boolean value; otherwise returns false.
        /// </returns>
        public static bool IsBoolean(object value)
        {
            if (value == null)
                return false;

            //Returns true if value of object is bool; otherwise false
            bool result;
            return Boolean.TryParse(value.ToString(), out result);
        }

        /// <summary>
        /// Returns true if the object is a valid System.Double value; otherwise returns false.
        /// </summary>
        /// <param name="value">The parameter to test whether it is a System.Double.</param>
        /// <returns>Returns true if the object is a valid System.Double value; otherwise returns false.</returns>
        public static bool IsDouble(object value)
        {
            if (value == null)
                return false;

            //Returns true if the value of object is a double; otherwise false.
            //NOT CURRENTLY USED. ONLY HERE BECAUSE IT *MIGHT* BE USEFUL
            Double result;
            return Double.TryParse(value.ToString(), out result);
        }

        /// <summary>
        /// Returns true if the object is a valid System.DateTime object; otherwise returns false.
        /// </summary>
        /// <param name="value">The parameter to test whether it is a System.DateTime object.</param>
        /// <returns>Returns true if the object is a valid System.DateTime object; otherwise returns false.</returns>
        public static bool IsDateTime(object value)
        {
            if (value == null)
                return false;

            // Returns true if the value of the object is a DateTime value; otherwise false
            DateTime result;
            return DateTime.TryParse(value.ToString(), out result);
        }

        /// <summary>
        /// Format the testValue parameter to so it data store-compatible. Specifically, 
        /// int.MinValue is replaced with 0. Use this method when updating or inserting 
        /// records in the database.
        /// </summary>
        /// <param name="testValue">The int to send to the database. int.MinValue is 
        /// replaced with 0.</param>
        /// <returns>Returns the parameter value formatted as a value that can be persisted
        /// to the data store.</returns>
        public static int ToDBValue(int testValue)
        {
            return (testValue == Int32.MinValue ? 0 : testValue);
        }

        /// <overloads>
        /// Convert the specified object to System.DateTime.
        /// </overloads>
        /// <summary>
        /// Convert the specified object to System.DateTime using culture-specific information. Use this object when retrieving
        /// values from a database. If the object is of type System.TypeCode.DBNull, null, an empty string, or cannot be converted,
        /// <see cref="DateTime.MinValue" /> is returned.
        /// </summary>
        /// <param name="value">The object to convert to <see cref="System.DateTime" />. An exception is thrown
        /// if the object cannot be converted.</param>
        /// <returns>Returns a <see cref="System.DateTime" /> value.</returns>
        public static DateTime ToDateTime(object value)
        {
            return ToDateTime(value, NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// Convert the specified object to System.DateTime using the specified <paramref name="formatProvider"/>.
        /// Use this object when retrieving values from a database. If the object is of type System.TypeCode.DBNull, null,
        /// an empty string, or cannot be converted, <see cref="DateTime.MinValue"/> is returned.
        /// </summary>
        /// <param name="value">The object to convert to <see cref="System.DateTime"/>. An exception is thrown
        /// if the object cannot be converted.</param>
        /// <param name="formatProvider">An <see cref="IFormatProvider" /> interface implementation that supplies 
        /// culture-specific formatting information. </param>
        /// <returns>
        /// Returns a <see cref="System.DateTime"/> value.
        /// </returns>
        /// <remarks>This overload was created 2011-01-14. The original version did all conversions using 
        /// <see cref="NumberFormatInfo.CurrentInfo" />, but for 2.4.6 I needed to invoke this from another place
        /// (<see cref="GallerySettings.RetrieveGallerySettingsFromDataStore" />) which required the conversion using 
        /// <see cref="CultureInfo.InvariantCulture" />, so an overload was created so that each caller can invoke it the
        /// way it wants. I wonder whether the original version could have used <see cref="CultureInfo.InvariantCulture" />,
        /// but rather than test it, it is safer to preserve the original behavior. As timer permits, one could see if
        /// this method still works when it is hard-coded to use <see cref="CultureInfo.InvariantCulture" />.</remarks>
        public static DateTime ToDateTime(object value, IFormatProvider formatProvider)
        {
            if (Convert.IsDBNull(value) || (value == null) || String.IsNullOrEmpty(value.ToString()))
            {
                return DateTime.MinValue;
            }
            else
            {
                DateTime result;
                if (DateTime.TryParse(value.ToString(), formatProvider, DateTimeStyles.None, out result))
                    return result;
                else
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Convert the specified object to System.DateTime using the specified <paramref name="format" /> and 
        /// <paramref name="formatProvider"/>. Use this object when retrieving values from a database. If the object is of 
        /// type System.TypeCode.DBNull, null, empty string, or cannot be converted, <see cref="DateTime.MinValue"/> is returned.
        /// </summary>
        /// <param name="value">The object to convert to <see cref="System.DateTime"/>. An exception is thrown
        /// if the object cannot be converted.</param>
        /// <param name="format">The expected format of <paramref name="value" />.</param>
        /// <param name="formatProvider">An <see cref="IFormatProvider"/> interface implementation that supplies
        /// culture-specific formatting information.</param>
        /// <returns>
        /// Returns a <see cref="System.DateTime"/> value.
        /// </returns>
        public static DateTime ToDateTime(object value, string format, IFormatProvider formatProvider)
        {
            if (Convert.IsDBNull(value) || (value == null) || String.IsNullOrEmpty(value.ToString()))
                return DateTime.MinValue;

            DateTime result;
            if (DateTime.TryParseExact(value.ToString(), format, formatProvider, DateTimeStyles.RoundtripKind, out result))
                return result;
            else
                return DateTime.MinValue;
        }

        /// <summary>
        /// Determines whether the specified string is formatted as a valid email address. This is determined by performing 
        /// two tests: (1) Comparing the string to a regular expression. (2) Using the validation built in to the .NET 
        /// constructor for the <see cref="System.Net.Mail.MailAddress"/> class. The method does not determine that the 
        /// email address actually exists.
        /// </summary>
        /// <param name="email">The string to validate as an email address.</param>
        /// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
        /// returns false.</returns>
        public static bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
                return false;

            return (ValidateEmailByRegEx(email) && ValidateEmailByMailAddressCtor(email));
        }

        /// <summary>
        /// Ensure the specified string is a valid name for a directory within the specified path. Invalid
        /// characters are removed and the existing directory is checked to see if it already has a child
        /// directory with the requested name. If it does, the name is slightly altered to make it unique.
        /// The name is shortened if its length exceeds the <paramref name="defaultAlbumDirectoryNameLength" />.
        /// The clean, guaranteed safe directory name is returned. No directory is actually created in the
        /// file system.
        /// </summary>
        /// <param name="dirPath">The path, including the parent directory, in which the specified name
        /// should be checked for validity (e.g. C:\mediaobjects\2006).</param>
        /// <param name="dirName">The directory name to be validated against the directory path. It should
        /// represent a proposed directory name and not an actual directory that already exists in the file
        /// system.</param>
        /// <param name="defaultAlbumDirectoryNameLength">Default length of the album directory name. You can
        /// specify the configuration setting DefaultAlbumDirectoryNameLength for this value.</param>
        /// <returns>
        /// Returns a string that can be safely used as a directory name within the path dirPath.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dirPath" /> or <paramref name="dirName" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="dirPath" /> or <paramref name="dirName" /> is an empty string.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1818:DoNotConcatenateStringsInsideLoops")]
        public static string ValidateDirectoryName(string dirPath, string dirName, int defaultAlbumDirectoryNameLength)
        {
            #region Parameter validaton

            if (dirPath == null)
                throw new ArgumentNullException("dirPath");

            if (dirName == null)
                throw new ArgumentNullException("dirName");

            if (String.IsNullOrEmpty(dirPath) || String.IsNullOrEmpty(dirName))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.HelperFunctions_ValidateDirectoryName_Ex_Msg, dirPath, dirName));
            }

            // Test 1: Remove any characters that are not valid for directory names on the operating system.
            string newDirName = RemoveInvalidDirectoryNameCharacters(dirName);

            // If we end up with an empty string, resort to the default value.
            if (newDirName.Length == 0)
                newDirName = GlobalConstants.DefaultAlbumDirectoryName;

            // Test 2: Verify length is less than our max allowed length.
            int maxLength = defaultAlbumDirectoryNameLength;
            if (newDirName.Length > maxLength)
            {
                newDirName = newDirName.Substring(0, maxLength);
            }

            // Test 3: If the name ends in a period or space, delete it. This is to handle a 8.3 DOS filename compatibility issue where most/all 
            // trailing periods and spaces are stripped from file and folder names by Windows, a holdover from the transition from 8.3 
            // filenames where the dot is not stored but implied. If we did not do this, then Windows would store the directory without
            // the trailing period or space, but Gallery Server would think it was still there. See bug # #90 for more info.
            newDirName = newDirName.TrimEnd(new char[] { '.', ' ' });

            #endregion

            // Test 3: Check to make sure the parent directory (specified in dirPath) doesn't contain a directory with
            // the new directory name (newDirName). If it does, keep altering the name until we come up with a unique one.
            string newSuffix = String.Empty;
            int counter = 1;

            while (Directory.Exists(Path.Combine(dirPath, newDirName)))
            {
                // The parent directory already contains a child directory with our new name. We need to strip off the
                // previous suffix if we added one (e.g. (1), (2), etc), generate a new suffix, and try again.
                if (newSuffix.Length > 0)
                {
                    // Remove the previous suffix we appended. Don't remove anything if this is the first time going
                    // through this loop (indicated by newSuffix.Length = 0).
                    newDirName = newDirName.Remove(newDirName.Length - newSuffix.Length);
                }

                // Generate the new suffix to append to the filename (e.g. "(3)")
                newSuffix = String.Format(CultureInfo.InvariantCulture, "({0})", counter);

                int newTotalLength = newDirName.Length + newSuffix.Length;
                if (newTotalLength > maxLength)
                {
                    // Our new name is going to be longer than our allowed max length. Remove just enough
                    // characters from newDirName so that the new length is equal to the max length.
                    int numCharactersToRemove = newTotalLength - maxLength;
                    newDirName = newDirName.Remove(newDirName.Length - numCharactersToRemove);
                }

                // Append the suffix. Place at the end for a directory.
                newDirName += newSuffix;

                counter++;
            }

            return newDirName;
        }

        /// <summary>
        /// Ensure the specified string is a valid name for a file within the specified path. Invalid 
        /// characters are removed and the existing directory is checked to see if it already has a file
        /// with the requested name. If it does, the name is slightly altered to make it unique.
        /// The clean, guaranteed safe filename is returned. No file is actually created in the file system.
        /// </summary>
        /// <param name="dirPath">The path, including the parent directory, in which the specified name
        /// should be checked for validity (e.g. C:\mediaobjects\2006\).</param>
        /// <param name="fileName">The filename to be validated against the directory path. It should 
        /// represent a proposed filename and not an actual file that already exists in the file system.</param>
        /// <returns>Returns a string that can be safely used as a filename within the path dirPath.</returns>
        public static string ValidateFileName(string dirPath, string fileName)
        {
            #region Parameter validation

            if (dirPath == null)
                throw new ArgumentNullException("dirPath");

            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (String.IsNullOrEmpty(dirPath) || String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.HelperFunctions_ValidateFileName_Ex_Msg1, dirPath, fileName));
            }

            if (!(Path.HasExtension(fileName)))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.HelperFunctions_ValidateFileName_Ex_Msg2, fileName));
            }

            #endregion

            // Test 1: Remove any characters that are not valid for directory names on the operating system.
            string newFilename = RemoveInvalidFileNameCharacters(fileName);

            // It is very unlikely that the above method stripped every character from the filename, because the filenames
            // should always come from existing files that are uploaded or added. But just in case it does, set a default.
            if (newFilename.Length == 0)
                newFilename = "DefaultFilename";

            // Test 2: Verify length is less than our max allowed length.
            const int maxLength = GlobalConstants.MediaObjectFileNameLength;
            if (newFilename.Length > maxLength)
            {
                newFilename = newFilename.Substring(0, maxLength);
            }

            // Test 3: Check to make sure the parent directory (specified in dirPath) doesn't contain a file with
            // the new filename (newFilename). If it does, keep altering the name until we come up with a unique one.
            string newSuffix = String.Empty;
            int counter = 1;

            while (File.Exists(Path.Combine(dirPath, newFilename)))
            {
                // The parent directory already contains a file with our new name. We need to strip off the
                // previous suffix if we added one (e.g. (1), (2), etc), generate a new suffix, and try again.
                if (newSuffix.Length > 0)
                {
                    // Remove the previous suffix we appended. Don't remove anything if this is the first time going
                    // through this loop (indicated by newSuffix.Length = 0).
                    string newFilenameWithoutExtension = Path.GetFileNameWithoutExtension(newFilename); // e.g. if newFilename=puppy(1).jpg, get "puppy(1)"
                    int indexOfSuffixToRemove = newFilenameWithoutExtension.Length - newSuffix.Length;
                    string newFilenameWithoutExtensionAndSuffix = newFilenameWithoutExtension.Remove(indexOfSuffixToRemove); // e.g. "puppy"
                    newFilename = newFilenameWithoutExtensionAndSuffix + Path.GetExtension(newFilename); // e.g. puppy.jpg
                }

                // Generate the new suffix to append to the filename (e.g. "(3)")
                newSuffix = String.Format(CultureInfo.InvariantCulture, "({0})", counter);

                int newTotalLength = newFilename.Length + newSuffix.Length;
                if (newTotalLength > maxLength)
                {
                    // Our new name is going to be longer than our allowed max length. Remove just enough
                    // characters from newFilename so that the new length is equal to the max length.
                    int numCharactersToRemove = newTotalLength - maxLength;
                    newFilename = newFilename.Remove(newFilename.Length - numCharactersToRemove);
                }

                // Insert the suffix just before the ".".
                newFilename = newFilename.Insert(newFilename.LastIndexOf(".", StringComparison.Ordinal), newSuffix);

                counter++;
            }

            return newFilename;
        }

        /// <summary>
        /// Removes all characters from the specified string that are invalid for a directory name
        /// for the operating system. This function uses Path.GetInvalidPathChars() so it may remove 
        /// different characters under different operating systems, depending on the characters returned
        /// from this .NET function.
        /// </summary>
        /// <param name="directoryName">A string representing a proposed directory name
        /// that should have all invalid characters removed.</param>
        /// <returns>Removes a clean version of the directoryName parameter that has all invalid
        /// characters removed.</returns>
        public static string RemoveInvalidDirectoryNameCharacters(string directoryName)
        {
            // Set up our array of invalid characters. Path.GetInvalidPathChars() does not include the wildcard
            // characters *, ?, :, \, and /, so add them manually.
            char[] invalidChars = new char[(Path.GetInvalidPathChars().Length + 5)];
            Path.GetInvalidPathChars().CopyTo(invalidChars, 0);
            invalidChars[invalidChars.Length - 5] = '?';
            invalidChars[invalidChars.Length - 4] = '*';
            invalidChars[invalidChars.Length - 3] = ':';
            invalidChars[invalidChars.Length - 2] = '\\';
            invalidChars[invalidChars.Length - 1] = '/';

            // Strip out invalid characters that make the OS puke
            return Regex.Replace(directoryName, "[" + Regex.Escape(new string(invalidChars)) + "]", String.Empty);
        }

        /// <summary>
        /// Removes all characters from the specified string that are invalid for filenames
        /// for the operating system. This function uses Path.GetInvalidFileNameChars() so it may remove 
        /// different characters under different operating systems, depending on the characters returned
        /// from this .NET function.
        /// </summary>
        /// <param name="fileName">A string representing a proposed filename
        /// that should have all invalid characters removed.</param>
        /// <returns>Removes a clean version of the filename parameter that has all invalid
        /// characters removed.</returns>
        /// <remarks>This function also removes the ampersand (&amp;) because this character cannot be used in an URL (even if we try to encode it).
        /// </remarks>
        public static string RemoveInvalidFileNameCharacters(string fileName)
        {
            // Set up our array of invalid characters. Path.InvalidPathChars does not include the wildcard
            // characters *, ?, and also :, \, /, <, and >, so add them manually.
            char[] invalidChars = new char[(Path.GetInvalidFileNameChars().Length + 6)];
            Path.GetInvalidPathChars().CopyTo(invalidChars, 0);
            invalidChars[invalidChars.Length - 6] = '&';
            invalidChars[invalidChars.Length - 5] = '?';
            invalidChars[invalidChars.Length - 4] = '*';
            invalidChars[invalidChars.Length - 3] = ':';
            invalidChars[invalidChars.Length - 2] = '\\';
            invalidChars[invalidChars.Length - 1] = '/';

            // Strip out invalid characters that make the OS puke
            return Regex.Replace(fileName, "[" + Regex.Escape(new string(invalidChars)) + "]", String.Empty);
        }

        /// <summary>
        /// Parse the specified string and return a valid <see cref="System.Drawing.Color" />. The color may be specified as a 
        /// Hex value (e.g. "#336699", "#369"), an RGB color value (e.g. "(100,100,100)"), or one of the
        /// <see cref="System.Drawing.KnownColor" /> enumeration values ("Crimson", "Maroon"). An <see cref="ArgumentOutOfRangeException" />
        /// is thrown if a color cannot be parsed from the parameter.
        /// </summary>
        /// <param name="colorValue">A string representing the desired color. The color may be specified as a 
        /// Hex value (e.g. "#336699", "#369"), an RGB color value (e.g. "(100,100,100)"), or one of the
        /// <see cref="System.Drawing.KnownColor" /> enumeration values ("Crimson", "Maroon").</param>
        /// <returns>Returns a <see cref="System.Drawing.Color" /> struct that matches the color specified in the parameter.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="colorValue" /> is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="colorValue" /> cannot be converted into a known color.</exception>
        public static Color GetColor(string colorValue)
        {
            if (colorValue == null)
                throw new ArgumentNullException("colorValue");

            // #336699; (100, 100, 100); WhiteSmoke
            const string hexPattern = @"^\#[0-9A-Fa-f]{3}$|^\#[0-9A-Fa-f]{6}$";
            const string rgbPattern = @"^\(\d{1,3},\d{1,3},\d{1,3}\)$";
            const string namePattern = "^[A-Za-z]+$";

            colorValue = colorValue.Replace(" ", String.Empty); // Remove all white space

            Color myColor;

            Regex regExHex = new Regex(hexPattern);
            Regex regExRgb = new Regex(rgbPattern);
            Regex regExName = new Regex(namePattern);

            if (regExHex.IsMatch(colorValue))
            {
                // Color is specified as Hex. Parse.
                // If specified in 4-digit shorthand (e.g. #369), expand to full 7 digits (e.g. #336699).
                if (colorValue.Length == 4)
                {
                    colorValue = colorValue.Insert(1, colorValue.Substring(1, 1));
                    colorValue = colorValue.Insert(3, colorValue.Substring(3, 1));
                    colorValue = colorValue.Insert(5, colorValue.Substring(5, 1));
                }

                myColor = ColorTranslator.FromHtml(colorValue.ToUpper(CultureInfo.InvariantCulture));
            }

            else if (regExRgb.IsMatch(colorValue))
            {
                // Color is specified as RGB. Parse.
                string colorVal = colorValue;

                // Strip the opening and closing parentheses.
                colorVal = colorVal.TrimStart(new char[] { '(' });
                colorVal = colorVal.TrimEnd(new char[] { ')' });

                // First verify each value is a number from 0-255. (The reg ex matched 0-999).
                string[] rgbStringValues = colorVal.Split(new char[] { ',' });

                // Convert to integers
                int[] rgbValues = new int[3];
                for (int i = 0; i < rgbStringValues.Length; i++)
                {
                    rgbValues[i] = Int32.Parse(rgbStringValues[i], CultureInfo.InvariantCulture);

                    if ((rgbValues[i] < 0) || (rgbValues[i] > 255))
                        throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The color {0} does not represent a valid RGB color.", colorValue));
                }

                myColor = Color.FromArgb(rgbValues[0], rgbValues[1], rgbValues[2]);
            }

            else if (regExName.IsMatch(colorValue))
            {
                // Color is specified as a name. Parse.
                myColor = Color.FromName(colorValue);

                if ((myColor.A == 0) && (myColor.R == 0) && (myColor.G == 0) && (myColor.B == 0))
                    throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The color {0} does not represent a color known to the .NET Framework.", colorValue));
            }

            else
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The color {0} does not represent a valid color.", colorValue));
            }

            return myColor;
        }

        /// <summary>
        /// Encrypt the specified string using the System.Security.Cryptography.TripleDESCryptoServiceProvider cryptographic
        /// service provider. The secret key used in the encryption is specified in the encryptionKey configuration setting.
        /// The encrypted string can be decrypted to its original string using the Decrypt function in this class.
        /// </summary>
        /// <param name="plainText">A plain text string to be encrypted. If the value is null or empty, the return value is
        /// equal to String.Empty.</param>
        /// <returns>Returns an encrypted version of the plainText parameter.</returns>
        public static string Encrypt(string plainText)
        {
            return Utils.Encrypt(plainText, AppSetting.Instance.EncryptionKey);
        }

        /// <summary>
        /// Decrypt the specified string using the System.Security.Cryptography.TripleDESCryptoServiceProvider cryptographic
        /// service provider. The secret key used in the decryption is specified in the encryptionKey configuration setting.
        /// </summary>
        /// <param name="encryptedText">A string to be decrypted. The encrypted string should have been encrypted using the
        /// Encrypt function in this class. If the value is null or empty, the return value is equal to String.Empty.</param>
        /// <returns>
        /// Returns the original, unencrypted string contained in the encryptedText parameter.
        /// </returns>
        /// <exception cref="System.FormatException">Thrown when the text cannot be decrypted.</exception>
        public static string Decrypt(string encryptedText)
        {
            return Utils.Decrypt(encryptedText, AppSetting.Instance.EncryptionKey);
        }

        /// <summary>
        /// Determine the type of the gallery object (album, image, video, etc) specified by the ID. The object must exist 
        /// in the data store. If no gallery object is found, or a media object (image, video, etc) is found but 
        /// the file extension does not correspond to a supported MIME type by Gallery Server, 
        /// <see cref="GalleryObjectType.Unknown"/> is returned. If both a media object and an album exist with the 
        /// <paramref name="id"/>, the media object reference is returned.
        /// </summary>
        /// <param name="id">An integer representing a gallery object that exists in the data store (album, video,
        /// image, etc).</param>
        /// <returns>Returns a GalleryObjectType enum indicating the type of gallery object specified by ID.</returns>
        public static GalleryObjectType DetermineGalleryObjectType(int id)
        {
            if (id == Int32.MinValue)
                return GalleryObjectType.Unknown;

            #region Is ID a media object?

            GalleryObjectType goType = DetermineMediaObjectType(id);

            #endregion

            #region Is ID an album?

            if (goType == GalleryObjectType.Unknown)
            {
                // The ID does not represent a known MediaObject. Check to see if it's an album.
                using (var repo = new AlbumRepository())
                {
                    if (repo.Find(id) != null)
                    {
                        // If we get here, we found an album.
                        goType = GalleryObjectType.Album;
                    }
                }
            }

            #endregion

            // If ID is not a media object or album that exists in the data store, return GalleryObjectType.Unknown.
            return goType;
        }

        /// <overloads>Determine the type of the media object (image, video, audio, generic, etc) specified by the parameter(s). 
        /// This method returns <see cref="GalleryObjectType.Unknown" /> if no matching MIME type can be found. Guaranteed to not 
        /// return null.</overloads>
        /// <summary>
        /// Determine the type of the media object (image, video, audio, generic, etc) based on its ID. 
        /// This method returns <see cref="GalleryObjectType.Unknown" /> if no matching MIME type can be found. Guaranteed to not 
        /// return null.
        /// </summary>
        /// <param name="mediaObjectId">An integer representing a media object that exists in the data store. If no 
        /// matching media object is found, an <see cref="InvalidMediaObjectException" /> is thrown. (this will occur when no 
        /// matching record exists in the data store, or the ID actually represents an album ID). If a media object 
        /// is found, but no MIME type is declared in the configuration file that matches the file's extension, 
        /// <see cref="GalleryObjectType.Unknown" /> is returned.</param>
        /// <returns>Returns a <see cref="GalleryObjectType" /> enum indicating the type of media object specified by the 
        /// mediaObjectId parameter. Guaranteed to not return null.</returns>
        /// <remarks>Use this method for existing objects that have previously been added to the data store. </remarks>
        /// <exception cref="InvalidMediaObjectException">Thrown 
        /// when the mediaObjectId parameter does not represent an existing media object in the data store.</exception>
        public static GalleryObjectType DetermineMediaObjectType(int mediaObjectId)
        {
            using (var repo = new MediaObjectRepository())
            {
                return DetermineMediaObjectType(repo.Find(mediaObjectId));
            }
        }

        /// <summary>
        /// Determine the type of the media object (image, video, audio, generic, etc) based on <paramref name="moDto" />. 
        /// This method returns <see cref="GalleryObjectType.Unknown" /> if no matching MIME type can be found. Guaranteed to not 
        /// return null.
        /// </summary>
        /// <param name="moDto">The media asset DTO.</param>
        /// <returns>Returns a <see cref="GalleryObjectType" /> enum indicating the type of media object specified by the 
        /// mediaObjectId parameter. Guaranteed to not return null.</returns>
        /// <remarks>Use this method for existing objects that have previously been added to the data store. </remarks>
        /// <exception cref="InvalidMediaObjectException">Thrown 
        /// when the mediaObjectId parameter does not represent an existing media object in the data store.</exception>
        public static GalleryObjectType DetermineMediaObjectType(MediaObjectDto moDto)
        {
            return DetermineMediaObjectType(moDto.OriginalFilename, moDto.ExternalHtmlSource);
        }

        /// <summary>
        /// Determine the type of the media object (image, video, audio, generic, etc) based on the file's extension. 
        /// This method returns <see cref="GalleryObjectType.Unknown" /> if no matching MIME type can be found. Guaranteed to not 
        /// return null.
        /// </summary>
        /// <param name="fileName">A filename from which to determine its media object type. This is done by comparing
        /// its file extension to the list of extensions known to Gallery Server. If the file extension 
        /// does not correspond to a known MIME type, <see cref="GalleryObjectType.Unknown" /> is returned.</param>
        /// <returns>Returns a <see cref="GalleryObjectType" /> enum indicating the type of media object specified by the 
        /// filename parameter. Guaranteed to not return null.</returns>
        public static GalleryObjectType DetermineMediaObjectType(string fileName)
        {
            return DetermineMediaObjectType(fileName, String.Empty);
        }

        /// <summary>
        /// Determine the type of the media object (image, video, audio, generic, external etc) based on the file's extension or 
        /// whether external HTML exists. This method returns <see cref="GalleryObjectType.Unknown" /> if <paramref name="externalHtmlSource"/> is 
        /// null or empty and no matching MIME type can be found for <paramref name="fileName"/>. Guaranteed to not return null. 
        /// This overload is intended to be invoked when instantiating an existing media object.
        /// </summary>
        /// <param name="fileName">A filename from which to determine its media object type. This is done by comparing
        /// its file extension to the list of extensions known to Gallery Server. If the file extension
        /// does not correspond to a known MIME type, <see cref="GalleryObjectType.Unknown" /> is returned.</param>
        /// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at YouTube.</param>
        /// <returns>
        /// Returns a <see cref="GalleryObjectType" /> enum indicating the type of media object. Guaranteed to not return null.
        /// </returns>
        public static GalleryObjectType DetermineMediaObjectType(string fileName, string externalHtmlSource)
        {
            var goType = GalleryObjectType.Unknown;

            if (!String.IsNullOrEmpty(externalHtmlSource))
            {
                goType = GalleryObjectType.External;
            }
            else
            {
                IMimeType mimeType = Factory.LoadMimeType(fileName);

                if (mimeType != null)
                {
                    switch (mimeType.TypeCategory)
                    {
                        case MimeTypeCategory.Image: goType = GalleryObjectType.Image; break;
                        case MimeTypeCategory.Video: goType = GalleryObjectType.Video; break;
                        case MimeTypeCategory.Audio: goType = GalleryObjectType.Audio; break;
                        case MimeTypeCategory.Other: goType = GalleryObjectType.Generic; break;
                        default: throw new InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "HelperFunctions.DetermineMediaObjectType() encountered a MimeTypeCategory enumeration it does not recognize. The method may need to be updated. (Unrecognized MimeTypeCategory enumeration: MimeTypeCategory.{0})", mimeType.TypeCategory));
                    }
                }
            }

            return goType;
        }

        /// <summary>
        /// Gets a value indicating whether a user can view the specified <paramref name="album" />.
        /// </summary>
        /// <param name="album">The album.</param>
        /// <param name="roles">The roles the user is in.</param>
        /// <param name="isUserAuthenticated">If set to <c>true</c> the user is authenticated.</param>
        /// <returns><c>true</c> if the user can view the album; otherwise, <c>false</c>.</returns>
        public static bool CanUserViewAlbum(IAlbum album, IGalleryServerRoleCollection roles, bool isUserAuthenticated)
        {
            return SecurityManager.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, album.Id, album.GalleryId, isUserAuthenticated, album.IsPrivate, SecurityActionsOption.RequireOne, album.IsVirtualAlbum);
        }

        /// <summary>
        /// Parse the albumPhysicalPath parameter to find the portion that refers to album folders below the root album, then
        /// append this portion to the alternatePhysicalPath parameter and return the computed string. If alternatePhysicalPath is
        /// null or empty, then return albumPhysicalPath. This is useful when mapping an album's physical location
        /// to the physical location within the thumbnail and/or optimized image cache directory. For example, if an album is located
        /// at C:\mypics\album1\album2, the media object root directory is at C:\mypics (specified by the mediaObjectPath configuration
        /// setting), and the thumbnail directory is specified to be C:\thumbnailCache (the thumbnailPath configuration setting),
        /// then return C:\thumbnailCache\album1\album2.
        /// </summary>
        /// <param name="albumPhysicalPath">The full physical path to an existing album. An exception is thrown if the directory is not
        /// a child directory of the root media object directory (GallerySetting.FullMediaObjectPath). Ex: C:\mypics\album1\album2</param>
        /// <param name="alternatePhysicalPath">The full physical path to a directory on the hard drive. This is typically (always?)
        /// the path to either the thumbnail or optimized cache (refer to thumbnailPath and optimized configuration setting). Ex: C:\thumbnailCache
        /// This parameter is optional. If not specified, the method returns the albumPhysicalPath parameter without modification.</param>
        /// <param name="fullMediaObjectPath">The full physical path to the directory containing the media objects in the current
        /// gallery. You can use Factory.LoadGallerySetting(galleryId).FullMediaObjectPath to populate this parameter.
        /// Example: "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"</param>
        /// <returns>
        /// Returns the alternatePhysicalPath parameter with the album directory path appended. Ex: C:\thumbnailCache\album1\album2
        /// If the alternatePhysicalPath parameter is not specified, the method returns the albumPhysicalPath parameter without modification.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="albumPhysicalPath" /> or <paramref name="fullMediaObjectPath" /> is null.</exception>
        public static string MapAlbumDirectoryStructureToAlternateDirectory(string albumPhysicalPath, string alternatePhysicalPath, string fullMediaObjectPath)
        {
            if (albumPhysicalPath == null)
                throw new ArgumentNullException("albumPhysicalPath");

            if (fullMediaObjectPath == null)
                throw new ArgumentNullException("fullMediaObjectPath");

            if (String.IsNullOrEmpty(alternatePhysicalPath))
            {
                return albumPhysicalPath;
            }

            if (!albumPhysicalPath.StartsWith(fullMediaObjectPath, StringComparison.OrdinalIgnoreCase))
                throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Expected this.Parent.FullPhysicalPathOnDisk (\"{0}\") to start with \"{1}\", but it did not.", albumPhysicalPath, fullMediaObjectPath));

            string relativePath = albumPhysicalPath.Remove(0, fullMediaObjectPath.Length).Trim(new char[] { Path.DirectorySeparatorChar });

            return Path.Combine(alternatePhysicalPath, relativePath);
        }

        /// <summary>
        /// Generate a full physical path, such as "C:\inetpub\wwwroot\galleryserverpro\myimages", based on the specified parameters.
        /// If relativeOrFullPath is a relative path, such as "\myimages\", append it to the physicalAppPath and return. If 
        /// relativeOrFullPath is a full path, such as "C:\inetpub\wwwroot\galleryserverpro\myimages", ignore the physicalAppPath
        /// and return the full path. In either case, this procedure guarantees that all directory separator characters are valid
        /// for the current operating system and that there is no directory separator character after the final (innermost) directory.
        /// Does not verify to ensure the directory exists or that it is writable.
        /// </summary>
        /// <param name="physicalAppPath">The physical path of the currently executing application.</param>
        /// <param name="relativeOrFullPath">The relative or full file path. Relative paths should be relative to the root of the
        /// running application so that, when it is combined with physicalAppPath parameter, it creates a valid path.
        /// Examples: "C:\inetpub\wwwroot\galleryserverpro\myimages\", "C:/inetpub/wwwroot/galleryserverpro/myimages",
        /// "\myimages\", "\myimages", "myimages\", "myimages",	"/myimages/", "/myimages"</param>
        /// <returns>Returns a full physical path, without the trailing slash. For example: 
        /// "C:\inetpub\wwwroot\galleryserverpro\myimages"</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="physicalAppPath" /> or <paramref name="relativeOrFullPath" /> is 
        /// null or an empty string.</exception>
        public static string CalculateFullPath(string physicalAppPath, string relativeOrFullPath)
        {
            #region Validation

            if (String.IsNullOrEmpty(relativeOrFullPath))
                throw new ArgumentOutOfRangeException("relativeOrFullPath");

            if (String.IsNullOrEmpty(physicalAppPath))
                throw new ArgumentOutOfRangeException("physicalAppPath");

            #endregion

            string fullPhysicalPath;
            string modifiedMediaObjectPath;
            // Delete any leading or trailing slashes, and ensure all slashes are the backward ones (\).  If the user has entered a UNC drive we only remove
            // the trailing slashes and do not append the application directory
            if (IsUncPath(relativeOrFullPath)) //User has entered a UNC directory
            {
                modifiedMediaObjectPath = relativeOrFullPath.TrimEnd(new char[] { '/', Path.DirectorySeparatorChar }).Replace("/", Path.DirectorySeparatorChar.ToString());
                fullPhysicalPath = modifiedMediaObjectPath;
            }
            else
            {
                modifiedMediaObjectPath = relativeOrFullPath.TrimStart(new char[] { '/', Path.DirectorySeparatorChar });
                modifiedMediaObjectPath = modifiedMediaObjectPath.TrimEnd(new char[] { '/', Path.DirectorySeparatorChar }).Replace("/", Path.DirectorySeparatorChar.ToString());

                // If, after the trimming, we have a volume without a directory (e.g. "C:"), then add a trailing slash (e.g. "C:\").
                // We do this because subsequent code might use our return value as a parameter in Path.Combine, and Path.Combine
                // is not smart enough to add a slash when combining a volume and a path (e.g. "C:" and "mypics").
                if (modifiedMediaObjectPath.EndsWith(Path.VolumeSeparatorChar.ToString(), StringComparison.Ordinal))
                    modifiedMediaObjectPath += Path.DirectorySeparatorChar.ToString();

                if (IsRelativeFilePath(modifiedMediaObjectPath))
                {
                    fullPhysicalPath = Path.Combine(physicalAppPath, modifiedMediaObjectPath);
                }
                else
                {
                    fullPhysicalPath = modifiedMediaObjectPath;
                }
            }

            return fullPhysicalPath;
        }

        private static bool IsUncPath(string relativeOrFullPath)
        {
            return relativeOrFullPath.StartsWith(@"\\", StringComparison.Ordinal);
        }

        /// <summary>
        /// Validates that the specified path exists and that it is writable. If the path does not exist, we attempt to 
        /// create it. Once we know it exists, we write a tiny file to it and then delete it. If that passes, we know we
        /// have sufficient read/write access for Gallery Server to read/write files to the directory.
        /// </summary>
        /// <param name="fullPhysicalPath">The full physical path to test (e.g. "C:\inetpub\wwwroot\galleryserverpro\myimages")</param>
        /// <exception cref="CannotWriteToDirectoryException">
        /// Thrown when Gallery Server is unable to write to, or delete from, the path <paramref name="fullPhysicalPath"/>.</exception>
        public static void ValidatePhysicalPathExistsAndIsReadWritable(string fullPhysicalPath)
        {
            // Create directory if it does not exist.
            try
            {
                if (!Directory.Exists(fullPhysicalPath))
                {
                    Directory.CreateDirectory(fullPhysicalPath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new CannotWriteToDirectoryException(fullPhysicalPath, ex);
            }
            catch (SecurityException ex)
            {
                throw new CannotWriteToDirectoryException(fullPhysicalPath, ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new CannotWriteToDirectoryException(fullPhysicalPath, ex);
            }

            // Verify the directory is writable.
            string testFilePath = String.Empty;
            try
            {
                lock (_fileLock)
                {
                    string uniqueFileName = ValidateFileName(fullPhysicalPath, "_test_file_okay_to_delete.config");
                    testFilePath = Path.Combine(fullPhysicalPath, uniqueFileName);
                    using (FileStream s = File.Create(testFilePath))
                    {
                        s.WriteByte(42);
                    }

                    File.Delete(testFilePath);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(testFilePath))
                    {
                        File.Delete(testFilePath); // Clean up by deleting the file we created
                    }
                }
                catch { }

                throw new CannotWriteToDirectoryException(fullPhysicalPath, ex);
            }
        }

        /// <summary>
        /// Validates that the specified path exists and that it is writable. If the path does not exist, we attempt to 
        /// create it. Once we know it exists, we write a tiny file to it and then delete it. If that passes, we know we
        /// have sufficient read/write access for Gallery Server to read/write files to the directory.
        /// </summary>
        /// <param name="fullPhysicalPath">The full physical path to test (e.g. "C:\inetpub\wwwroot\galleryserverpro\myimages")</param>
        /// <exception cref="CannotWriteToDirectoryException">
        /// Thrown when Gallery Server is unable to read from the path <paramref name="fullPhysicalPath"/>.</exception>
        public static void ValidatePhysicalPathExistsAndIsReadable(string fullPhysicalPath)
        {
            // Verify the directory exists.
            if (!Directory.Exists(fullPhysicalPath))
                throw new DirectoryNotFoundException(String.Format(CultureInfo.InvariantCulture, Resources.DirectoryNotFound_Ex_Msg, fullPhysicalPath));

            // Verify the directory is readable.
            try
            {
                string[] files = Directory.GetFiles(fullPhysicalPath);
            }
            catch (Exception ex)
            {
                throw new CannotReadFromDirectoryException(fullPhysicalPath, ex);
            }
        }

        /// <summary>
        /// Determine whether the specified file can be added to Gallery Server. This is determined by first looking at the
        /// <see cref="IGallerySettings.AllowUnspecifiedMimeTypes" /> configuration setting, and returns true if this setting is 
        /// true. If false, the method looks up the MIME type for this file from the configuration file and returns the value 
        /// of the allowAddToGallery attribute. If there isn't a MIME type entry for this file and 
        /// <see cref="IGallerySettings.AllowUnspecifiedMimeTypes" /> = <c>false</c>, this method returns false.
        /// </summary>
        /// <param name="fileName">A name of a file that includes the extension.</param>
        /// <param name="galleryId">The gallery ID. This value is used to look up the configuration setting 
        /// <see cref="IGallerySettings.AllowUnspecifiedMimeTypes" /></param>
        /// <returns>
        /// Returns true if the file can be added to Gallery Server; otherwise returns false.
        /// </returns>
        public static bool IsFileAuthorizedForAddingToGallery(string fileName, int galleryId)
        {
            if (Factory.LoadGallerySetting(galleryId).AllowUnspecifiedMimeTypes)
                return true;

            IMimeType mimeType = Factory.LoadMimeType(galleryId, fileName);

            if ((mimeType != null) && mimeType.AllowAddToGallery)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Update the audit fields of the gallery object. This should be invoked before saving any gallery object within this
        /// class library. Class libraries that use this library are responsible for updating the audit fields themselves.
        /// The audit fields are: CreatedByUsername, DateAdded, LastModifiedByUsername, DateLastModified
        /// </summary>
        /// <param name="galleryObject">The gallery object whose audit fields are to be updated.</param>
        /// <param name="userName">The user name of the currently logged on user.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> or <paramref name="userName" /> is null.</exception>
        public static void UpdateAuditFields(IGalleryObject galleryObject, string userName)
        {
            if (galleryObject == null)
                throw new ArgumentNullException("galleryObject");

            if (userName == null)
                throw new ArgumentNullException("userName");

            DateTime currentTimestamp = DateTime.UtcNow;

            if (galleryObject.IsNew)
            {
                galleryObject.CreatedByUserName = userName;
                galleryObject.DateAdded = currentTimestamp;
            }

            galleryObject.LastModifiedByUserName = userName;
            galleryObject.DateLastModified = currentTimestamp;
        }

        /// <summary>
        /// Begins a new database transaction. All subsequent database actions occur within the context of this transaction.
        /// Use <see cref="CommitTransaction"/> to commit this transaction or <see cref="RollbackTransaction" /> to abort it. If a transaction
        /// is already in progress, then this method returns without any action, which preserves the original transaction.
        /// <note type="caution">The SQLite data provider supports this method, but the SQL Server data provider does not. The
        /// primary reason for this is the SQL Server provider was written first without transactions in mind, but SQLite
        /// encounters serious performance degradation unless transactions are used, so transaction support was added.</note>
        /// </summary>
        /// <remarks>Transactions are supported only when the client is a web application.This is because the 
        /// transaction is stored in the HTTP context Items property. If the client is not a web application, then 
        /// System.Web.HttpContext.Current is null. When this happens, this method returns without taking any action.</remarks>
        public static void BeginTransaction()
        {
            //Factory.GetDataProvider().BeginTransaction();
        }

        /// <summary>
        /// Commits the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
        /// If there is not an existing transaction, no action is taken. If this method is called when a datareader is open, the
        /// actual commit is delayed until all datareaders are disposed.
        /// <note type="caution">The SQLite data provider supports this method, but the SQL Server data provider does not. The
        /// primary reason for this is the SQL Server provider was written first without transactions in mind, but SQLite
        /// encounters serious performance degradation unless transactions are used, so transaction support was added.</note>
        /// </summary>
        /// <remarks>Transactions are supported only when the client is a web application.This is because the 
        /// transaction is stored in the HTTP context Items property. If the client is not a web application, then 
        /// System.Web.HttpContext.Current is null. When this happens, this method returns without taking any action.</remarks>
        public static void RollbackTransaction()
        {
            //Factory.GetDataProvider().RollbackTransaction();
        }

        /// <summary>
        /// Aborts the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
        /// If there is not an existing transaction, no action is taken.
        /// <note type="caution">The SQLite data provider supports this method, but the SQL Server data provider does not. The
        /// primary reason for this is the SQL Server provider was written first without transactions in mind, but SQLite
        /// encounters serious performance degradation unless transactions are used, so transaction support was added.</note>
        /// </summary>
        /// <remarks>Transactions are supported only when the client is a web application.This is because the 
        /// transaction is stored in the HTTP context Items property. If the client is not a web application, then 
        /// System.Web.HttpContext.Current is null. When this happens, this method returns without taking any action.</remarks>
        public static void CommitTransaction()
        {
            //Factory.GetDataProvider().CommitTransaction();
        }

        ///// <summary>
        ///// Create and return a deep copy of the specified object. The copy is created by serializing the object to memory and
        ///// then deserializing it into a new object. Returns null if the specified parameter is null.
        ///// </summary>
        ///// <typeparam name="T">The type of object for which to make a deep copy.</typeparam>
        ///// <param name="obj">The object for which to make a deep copy. May be null.</param>
        ///// <returns>Returns a deep copy of the specified parameter, or null if the parameter is null.</returns>
        ///// <remarks>This method requires Full Trust.</remarks>
        //public static T CloneObject<T>(T obj)
        //{
        //  // Create a memory stream and a formatter.
        //  using (System.IO.MemoryStream ms = new System.IO.MemoryStream(1000))
        //  {
        //    BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));

        //    // Serialize the object into the stream.
        //    bf.Serialize(ms, obj);

        //    // Position stream pointer back to first byte.
        //    ms.Seek(0, System.IO.SeekOrigin.Begin);

        //    // Deserialize into another object.
        //    return (T) bf.Deserialize(ms);
        //  }
        //}

        /// <summary>
        /// Returns the current version of Gallery Server.
        /// </summary>
        /// <returns>An instance of <see cref="GalleryDataSchemaVersion" /> representing the version (e.g. "1.0.0").</returns>
        public static GalleryDataSchemaVersion GetGalleryServerVersion()
        {
            return GalleryDb.DataSchemaVersion;
        }

        /// <summary>
        /// Determines whether <paramref name="modifiedMediaObjectPath" /> is a relative file path or an absolute one. It is
        /// considered a relative path if <see cref="Path.GetPathRoot" /> returns a null or empty string. 
        /// Examples: "App_Data\GalleryServer_Data.sdf" returns true; "C:\data\GalleryServer_Data.sdf" returns false.
        /// </summary>
        /// <param name="modifiedMediaObjectPath">The modified media object path.</param>
        /// <returns>
        /// 	<c>true</c> if <paramref name="modifiedMediaObjectPath" /> is a relative file path; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRelativeFilePath(string modifiedMediaObjectPath)
        {
            return String.IsNullOrEmpty(Path.GetPathRoot(modifiedMediaObjectPath));
        }

        /// <summary>
        /// Move <paramref name="sourceFilePath" /> to <paramref name="destFilePath" />, first deleting the destination file if
        /// it exists. Includes retry functionality to handle the rare scenario of the destination file not being fully deleted by the 
        /// time it calls <see cref="File.Move" />. If the move fails for whatever reason, the original file, if it had existed at 
        /// <paramref name="destFilePath" />, is left unmodified.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="destFilePath">The destination file path. If it exists, it is deleted.</param>
        /// <exception cref="FileNotFoundException">Thrown when <paramref name="sourceFilePath" /> does not exist.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceFilePath" /> or <paramref name="destFilePath" /> is null.</exception>
        /// <exception cref="IOException">Thrown when <paramref name="destFilePath" /> exists even after multiple attempts to delete it.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sourceFilePath" /> or <paramref name="destFilePath" /> is
        /// a zero-length string, contains only white space, or contains invalid characters as defined in <see cref="Path.InvalidPathChars" />.</exception>
        /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on 
        /// Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="DirectoryNotFoundException">The path specified in <paramref name="sourceFilePath" /> or <paramref name="destFilePath" /> is invalid,
        /// (for example, it is on an unmapped drive).</exception>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="sourceFilePath" /> or <paramref name="destFilePath" /> is in an invalid format.</exception>
        public static void MoveFileSafely(string sourceFilePath, string destFilePath)
        {
            if (sourceFilePath == null)
                throw new ArgumentNullException(nameof(sourceFilePath));

            if (destFilePath == null)
                throw new ArgumentNullException(nameof(destFilePath));

            if (!File.Exists(sourceFilePath))
            {
                try
                {
                    throw new FileNotFoundException("File not found", sourceFilePath);

                }
                catch (FileNotFoundException ex) // Do this so error handler can record sourceFilePath
                {
                    ex.Data.Add("File", sourceFilePath);
                    throw;
                }
            }

            string tmpFilePath = null;

            if (File.Exists(destFilePath))
            {
                // Move file to temp location so we can restore it if we encounter an issue.
                tmpFilePath = Path.Combine(AppSetting.Instance.TempUploadDirectory, string.Concat(Guid.NewGuid().ToString(), ".tmp"));
                try
                {
                    File.Move(destFilePath, tmpFilePath);
                }
                catch (Exception ex)
                {
                    ex.Data.Add("CannotMakeBackupFile", $"This error occurred while trying to move file '{destFilePath}' to '{tmpFilePath}' in preparation for it to be replaced with '{sourceFilePath}'.");
                    Events.EventController.RecordError(ex, AppSetting.Instance);
                    throw;
                }
            }

            // Move file. If IOException happens, wait 0.5 seconds and try again, up to 10 times.
            var counter = 0;
            const int maxTries = 10;
            try
            {
                while (true)
                {
                    try
                    {
                        File.Move(sourceFilePath, destFilePath);

                        // File successfully moved, so we can delete the backup.
                        if (File.Exists(tmpFilePath))
                        {
                            File.Delete(tmpFilePath);
                        }

                        break;
                    }
                    catch (IOException)
                    {
                        counter++;

                        if (counter >= maxTries)
                            throw;

                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(tmpFilePath))
                {
                    // Restore file so that the original is not lost
                    File.Move(tmpFilePath, destFilePath);
                }

                ex.Data.Add("CannotMoveFile", $"This error occurred while trying to move file '{sourceFilePath}' to '{destFilePath}'. We tried {counter} times, with a delay between attempts, but it is still failing.");
                Events.EventController.RecordError(ex, AppSetting.Instance);

                throw;
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Validates that the e-mail address conforms to a regular expression pattern for e-mail addresses.
        /// </summary>
        /// <param name="email">The string to validate as an email address.</param>
        /// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
        /// returns false.</returns>
        private static bool ValidateEmailByRegEx(string email)
        {
            const string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

            return Regex.IsMatch(email, pattern);
        }

        /// <summary>
        /// Uses the validation built in to the .NET constructor for the <see cref="System.Net.Mail.MailAddress"/> class
        /// to determine if the e-mail conforms to the expected format of an e-mail address.
        /// </summary>
        /// <param name="email">The string to validate as an email address.</param>
        /// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
        /// returns false.</returns>
        private static bool ValidateEmailByMailAddressCtor(string email)
        {
            bool passesMailAddressTest = false;
            try
            {
                new MailAddress(email);
                passesMailAddressTest = true;
            }
            catch (FormatException) { }

            return passesMailAddressTest;
        }

        #endregion
    }
}
