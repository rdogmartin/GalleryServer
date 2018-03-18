using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;
using GalleryServer.Events;

namespace GalleryServer.Business.Metadata
{
    /// <summary>
    /// Provides functionality for reading and writing metadata to or from a gallery object.
    /// </summary>
    /// <remarks>Documentation for WPF meta properties (e.g. "System.Photo.ExposureTime"): https://msdn.microsoft.com/en-us/library/windows/desktop/dd561977(v=vs.85).aspx</remarks>
    public class ImageMetadataReadWriter : MediaObjectMetadataReadWriter
    {
        #region Fields

        private enum MetaPersistAction
        {
            Delete = 1,
            Save = 2
        }

        private const uint MetadataPaddingInBytes = 2048;

        private PropertyItem[] _propertyItems;
        private IWpfMetadata _wpfMetadata;
        private int _width, _height;
        private Dictionary<RawMetadataItemName, MetadataItem> _rawMetadata;
        private GpsLocation _gpsLocation;
        private static Dictionary<MetadataItemName, string> _iptcQueryParameters;
        private static Dictionary<SystemMetaProperty, string> _wpfMetadataQueryStrings;
        private static readonly object _sharedLock = new Object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the property items associated with the image file. Guaranteed to not return null.
        /// </summary>
        /// <value>An array of <see cref="PropertyItem" /> instances.</value>
        private IEnumerable<PropertyItem> PropertyItems
        {
            get { return _propertyItems ?? (_propertyItems = GetImagePropertyItems(GalleryObject.Original.FileNamePhysicalPath)); }
        }

        /// <summary>
        /// Gets the raw metadata associated with the current image file. Guaranteed to not return null.
        /// </summary>
        /// <value>The raw metadata associated with the current image file.</value>
        private Dictionary<RawMetadataItemName, MetadataItem> RawMetadata
        {
            get { return _rawMetadata ?? (_rawMetadata = GetRawMetadataDictionary()); }
        }

        /// <summary>
        /// Gets an object that can extract metadata from a media file using the .NET WPF classes.
        /// Guaranteed to not return null.
        /// </summary>
        /// <value>An instance of <see cref="WpfMetadataReader" /> when possible; otherwise 
        /// <see cref="NullObjects.NullWpfMetadata" />.</value>
        private IWpfMetadata WpfMetadataReader
        {
            get { return _wpfMetadata ?? (_wpfMetadata = GetBitmapMetadataReader()); }
        }

        /// <summary>
        /// Gets an object that can retrieve GPS-related data from a media file.
        /// </summary>
        /// <value>An instance of <see cref="GpsLocation" />.</value>
        private GpsLocation GpsLocation
        {
            get { return _gpsLocation ?? (_gpsLocation = GpsLocation.Parse(WpfMetadataReader)); }
        }

        /// <summary>
        /// Gets the query format string to be used for extracting IPTC data from a media file.
        /// Example: "/app13/irb/8bimiptc/iptc/{{str={0}}}"
        /// </summary>
        /// <value>A string.</value>
        private static string IptcQueryFormatString
        {
            get { return "/app13/irb/8bimiptc/iptc/{{str={0}}}"; }
        }

        /// <summary>
        /// Gets a collection of query parameters for interacting with IPTC data in a media file.
        /// The key identifies the metadata item. The value is the query identifier that is combined
        /// with the <see cref="IptcQueryFormatString" /> to create a query string that can be
        /// passed to the <see cref="BitmapMetadata.GetQuery" /> method.
        /// </summary>
        /// <value>A Dictionary object.</value>
        private static Dictionary<MetadataItemName, string> IptcQueryParameters
        {
            get
            {
                if (_iptcQueryParameters == null)
                {
                    lock (_sharedLock)
                    {
                        if (_iptcQueryParameters == null)
                        {
                            var tmp = new Dictionary<MetadataItemName, string>();

                            tmp.Add(MetadataItemName.IptcByline, "By-Line");
                            tmp.Add(MetadataItemName.IptcBylineTitle, "By-line Title");
                            tmp.Add(MetadataItemName.IptcCaption, "Caption");
                            tmp.Add(MetadataItemName.IptcCity, "City");
                            tmp.Add(MetadataItemName.IptcCopyrightNotice, "Copyright Notice");
                            tmp.Add(MetadataItemName.IptcCountryPrimaryLocationName, "Country/Primary Location Name");
                            tmp.Add(MetadataItemName.IptcCredit, "Credit");
                            tmp.Add(MetadataItemName.IptcDateCreated, "Date Created");
                            tmp.Add(MetadataItemName.IptcHeadline, "Headline");
                            tmp.Add(MetadataItemName.IptcKeywords, "Keywords");
                            tmp.Add(MetadataItemName.IptcObjectName, "Object Name");
                            tmp.Add(MetadataItemName.IptcOriginalTransmissionReference, "Original Transmission Reference");
                            tmp.Add(MetadataItemName.IptcProvinceState, "Province/State");
                            tmp.Add(MetadataItemName.IptcRecordVersion, "Record Version");
                            tmp.Add(MetadataItemName.IptcSource, "Source");
                            tmp.Add(MetadataItemName.IptcSpecialInstructions, "Special Instructions");
                            tmp.Add(MetadataItemName.IptcSublocation, "Sub-location");
                            tmp.Add(MetadataItemName.IptcWriterEditor, "Writer/Editor");

                            Thread.MemoryBarrier();

                            _iptcQueryParameters = tmp;
                        }
                    }
                }

                return _iptcQueryParameters;
            }
        }

        /// <summary>
        /// Gets a collection of query strings that can be passed to <see cref="BitmapMetadata.GetQuery" /> The key identifies the 
        /// metadata item. The value is the string that can be passed to the <see cref="BitmapMetadata.GetQuery" /> method.
        /// </summary>
        /// <value>A Dictionary object.</value>
        private static Dictionary<SystemMetaProperty, string> WpfMetadataQueryStrings
        {
            get
            {
                if (_wpfMetadataQueryStrings == null)
                {
                    lock (_sharedLock)
                    {
                        if (_wpfMetadataQueryStrings == null)
                        {
                            var tmp = new Dictionary<SystemMetaProperty, string>
              {
                {SystemMetaProperty.System_Photo_ExposureProgram, "System.Photo.ExposureProgram"},
                {SystemMetaProperty.System_Photo_ExposureTimeNumerator, "System.Photo.ExposureTimeNumerator"},
                {SystemMetaProperty.System_Photo_ExposureTimeDenominator, "System.Photo.ExposureTimeDenominator"},
                {SystemMetaProperty.System_Photo_ExposureBias, "System.Photo.ExposureBias"},
                {SystemMetaProperty.System_Photo_Orientation, "System.Photo.Orientation"},
                {SystemMetaProperty.System_Photo_Flash, "System.Photo.Flash"},
                {SystemMetaProperty.System_Photo_FNumber, "System.Photo.FNumber"},
                {SystemMetaProperty.System_Photo_FocalLength, "System.Photo.FocalLength"},
                {SystemMetaProperty.System_Photo_ISOSpeed, "System.Photo.ISOSpeed"},
                {SystemMetaProperty.System_Photo_Aperture, "System.Photo.Aperture"},
                {SystemMetaProperty.System_Photo_LightSource, "System.Photo.LightSource"},
                {SystemMetaProperty.System_Photo_MeteringMode, "System.Photo.MeteringMode"},
                {SystemMetaProperty.System_Photo_PeopleNames, "System.Photo.PeopleNames"},
              };

                            Thread.MemoryBarrier();

                            _wpfMetadataQueryStrings = tmp;
                        }
                    }
                }

                return _wpfMetadataQueryStrings;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaObjectMetadataReadWriter" /> class.
        /// </summary>
        /// <param name="mediaObject">The media object.</param>
        public ImageMetadataReadWriter(IGalleryObject mediaObject)
          : base(mediaObject)
        {
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Extract the property items from the image at <paramref name="imageFilePath" />. The path must refer to an image that can be 
        /// passed to a <see cref="System.Drawing.Bitmap" /> constructor (when running in medium trust) or 
        /// <see cref="System.Drawing.Image.FromStream(Stream, bool, bool)" /> (when running in full trust). It will throw an exception 
        /// if it cannot. Guaranteed to not return null.
        /// </summary>
        /// <param name="imageFilePath">The full path to the image file. Ex: "C:\Dev\GS\Dev-Main\Website\gs\mediaobjects\Uploads\Desert.jpg"</param>
        /// <returns>An array of <see cref="PropertyItem" /> instances.</returns>
        public static PropertyItem[] GetImagePropertyItems(string imageFilePath)
        {
            if (string.IsNullOrWhiteSpace(imageFilePath))
                return new PropertyItem[0];

            if (AppSetting.Instance.AppTrustLevel == ApplicationTrustLevel.Full)
            {
                return GetPropertyItemsUsingFullTrustTechnique(imageFilePath);
            }
            else
            {
                return GetPropertyItemsUsingLimitedTrustTechnique(imageFilePath);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the metadata value for the specified <paramref name="metaName" />. May return null.
        /// </summary>
        /// <param name="metaName">Name of the metadata item to retrieve.</param>
        /// <returns>An instance that implements <see cref="IMetaValue" />.</returns>
        public override IMetaValue GetMetaValue(MetadataItemName metaName)
        {
            try
            {
                switch (metaName)
                {
                    case MetadataItemName.Title: return GetTitle();
                    case MetadataItemName.DatePictureTaken: return GetDatePictureTaken();
                    case MetadataItemName.Author: return GetAuthor();
                    case MetadataItemName.CameraModel: return GetCameraModel();
                    case MetadataItemName.EquipmentManufacturer: return GetCameraManufacturer();
                    case MetadataItemName.Tags: return GetKeywords();
                    case MetadataItemName.Rating: return GetRating();
                    case MetadataItemName.Comment: return GetComment();
                    case MetadataItemName.Copyright: return GetCopyright();
                    case MetadataItemName.Subject: return GetSubject();
                    case MetadataItemName.ColorRepresentation: return GetColorRepresentation();
                    case MetadataItemName.Description: return GetDescription();
                    case MetadataItemName.Dimensions: return GetDimensions();
                    case MetadataItemName.ExposureCompensation: return GetExposureCompensation();
                    case MetadataItemName.ExposureProgram: return GetExposureProgram();
                    case MetadataItemName.People: return GetPeople();
                    case MetadataItemName.Orientation: return GetOrientation();
                    case MetadataItemName.ExposureTime: return GetExposureTime();
                    case MetadataItemName.FlashMode: return GetFlashMode();
                    case MetadataItemName.FNumber: return GetFNumber();
                    case MetadataItemName.FocalLength: return GetFocalLength();
                    case MetadataItemName.Height: return GetHeight();
                    case MetadataItemName.HorizontalResolution: return GetHorizontalResolution();
                    case MetadataItemName.IsoSpeed: return GetIsoSpeed();
                    case MetadataItemName.LensAperture: return GetLensAperture();
                    case MetadataItemName.LightSource: return GetLightSource();
                    case MetadataItemName.MeteringMode: return GetMeteringMode();
                    case MetadataItemName.SubjectDistance: return GetSubjectDistance();
                    case MetadataItemName.VerticalResolution: return GetVerticalResolution();
                    case MetadataItemName.Width: return GetWidth();

                    case MetadataItemName.GpsVersion:
                    case MetadataItemName.GpsLocation:
                    case MetadataItemName.GpsLatitude:
                    case MetadataItemName.GpsLongitude:
                    case MetadataItemName.GpsAltitude:
                    case MetadataItemName.GpsDestLocation:
                    case MetadataItemName.GpsDestLatitude:
                    //case MetadataItemName.GpsLocationWithMapLink: // Built from template, nothing to extract
                    //case MetadataItemName.GpsDestLocationWithMapLink: // Built from template, nothing to extract
                    case MetadataItemName.GpsDestLongitude: return GetGpsValue(metaName);

                    case MetadataItemName.IptcByline:
                    case MetadataItemName.IptcBylineTitle:
                    case MetadataItemName.IptcCaption:
                    case MetadataItemName.IptcCity:
                    case MetadataItemName.IptcCopyrightNotice:
                    case MetadataItemName.IptcCountryPrimaryLocationName:
                    case MetadataItemName.IptcCredit:
                    case MetadataItemName.IptcDateCreated:
                    case MetadataItemName.IptcHeadline:
                    case MetadataItemName.IptcKeywords:
                    case MetadataItemName.IptcObjectName:
                    case MetadataItemName.IptcOriginalTransmissionReference:
                    case MetadataItemName.IptcProvinceState:
                    case MetadataItemName.IptcRecordVersion:
                    case MetadataItemName.IptcSource:
                    case MetadataItemName.IptcSpecialInstructions:
                    case MetadataItemName.IptcSublocation:
                    case MetadataItemName.IptcWriterEditor: return GetIptcValue(metaName);

                    default:
                        return base.GetMetaValue(metaName);
                }
            }
            catch (NotSupportedException)
            {
                // We may get here when we try to retrieve a property through WpfMetadataReader.GetQuery()
                return null;
            }
            catch (Exception ex)
            {
                // Record the file path and meta name, then re-throw.
                if (!ex.Data.Contains("Filepath"))
                {
                    ex.Data.Add("Filepath", GalleryObject.Original.FileNamePhysicalPath);
                }
                if (!ex.Data.Contains("Metaname"))
                {
                    ex.Data.Add("Metaname", metaName);
                }
                throw;
            }
        }

        /// <summary>
        /// Persists the meta value identified by <paramref name="metaName" /> to the media file. It is expected the meta item
        /// exists in <see cref="IGalleryObject.MetadataItems" />. No action is taken if <see cref="IGalleryObjectMetadataItem.PersistToFile" />
        /// is <c>false</c>.
        /// </summary>
        /// <param name="metaName">Name of the meta item to persist.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void SaveMetaValue(MetadataItemName metaName)
        {
            PersistMetaValue(metaName, MetaPersistAction.Save);
        }

        /// <summary>
        /// Permanently removes the meta value from the media file. The item is also removed from
        /// <see cref="IGalleryObject.MetadataItems" />. No action is taken if <see cref="IGalleryObjectMetadataItem.PersistToFile" />
        /// is <c>false</c>.
        /// </summary>
        /// <param name="metaName">Name of the meta item to delete.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void DeleteMetaValue(MetadataItemName metaName)
        {
            PersistMetaValue(metaName, MetaPersistAction.Delete);
        }

        #endregion

        #region Functions

        private IMetaValue GetTitle()
        {
            // Look in three places for title:
            // 1. The Title property in the WPF BitmapMetadata class.
            // 2. The ImageTitle property of the GDI+ property tags.
            // 3. The filename.
            var wpfTitle = GetWpfTitle();

            if (wpfTitle != null)
                return wpfTitle;

            var title = GetStringMetadataItem(RawMetadataItemName.ImageTitle);

            return !String.IsNullOrWhiteSpace(title) ? new MetaValue(title, title) : new MetaValue(GalleryObject.Original.FileName);
        }

        private IMetaValue GetWpfTitle()
        {
            string wpfValue = null;

            try
            {
                wpfValue = WpfMetadataReader.Title;
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return (!String.IsNullOrWhiteSpace(wpfValue) ? new MetaValue(wpfValue.Trim(), wpfValue) : null);
        }

        private IMetaValue GetDatePictureTaken()
        {
            return GetDatePictureTakenWpf() ?? GetDatePictureTakenGdi();
        }

        private IMetaValue GetAuthor()
        {
            try
            {
                var author = ConvertToDelimitedString(WpfMetadataReader.Author);

                return new MetaValue(author, author);
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return null;
        }

        private IMetaValue GetCameraModel()
        {
            var wpfCameraModel = GetWpfCameraModel();

            if (wpfCameraModel != null)
                return wpfCameraModel;

            var cameraModel = GetStringMetadataItem(RawMetadataItemName.EquipModel);

            return !String.IsNullOrWhiteSpace(cameraModel) ? new MetaValue(cameraModel, cameraModel) : null;
        }

        private IMetaValue GetWpfCameraModel()
        {
            string wpfValue = null;

            try
            {
                wpfValue = WpfMetadataReader.CameraModel;
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return (wpfValue != null ? new MetaValue(wpfValue.Trim(), wpfValue) : null);
        }

        private IMetaValue GetCameraManufacturer()
        {
            var wpfCameraManufacturer = GetWpfCameraManufacturer();

            if (wpfCameraManufacturer != null)
                return wpfCameraManufacturer;

            var cameraMfg = GetStringMetadataItem(RawMetadataItemName.EquipMake);

            return !String.IsNullOrWhiteSpace(cameraMfg) ? new MetaValue(cameraMfg, cameraMfg) : null;
        }

        private IMetaValue GetWpfCameraManufacturer()
        {
            string wpfValue = null;

            try
            {
                wpfValue = WpfMetadataReader.CameraManufacturer;
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return (wpfValue != null ? new MetaValue(wpfValue.Trim(), wpfValue) : null);
        }

        private IMetaValue GetKeywords()
        {
            try
            {
                const int maxTagLength = 100; // Due to database constraints, tags cannot be longer than 100 characters
                var keywords = ConvertToDelimitedString(WpfMetadataReader.Keywords, maxTagLength);

                return new MetaValue(keywords, keywords);
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return null;
        }

        private IMetaValue GetRating()
        {
            try
            {
                var rating = WpfMetadataReader.Rating;
                return (rating > 0 ? new MetaValue(rating.ToString(CultureInfo.InvariantCulture), rating.ToString(CultureInfo.InvariantCulture)) : null);
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return null;
        }

        private IMetaValue GetComment()
        {
            var wpfComment = GetWpfComment();

            if (wpfComment != null)
                return wpfComment;

            var comment = GetStringMetadataItem(RawMetadataItemName.ExifUserComment);

            return !String.IsNullOrWhiteSpace(comment) ? new MetaValue(comment, comment) : null;
        }

        private IMetaValue GetWpfComment()
        {
            string wpfValue = null;

            try
            {
                wpfValue = WpfMetadataReader.Comment;
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return (wpfValue != null ? new MetaValue(wpfValue.Trim(), wpfValue) : null);
        }

        private IMetaValue GetCopyright()
        {
            var wpfCopyright = GetWpfCopyright();

            if (wpfCopyright != null)
                return wpfCopyright;

            var copyright = GetStringMetadataItem(RawMetadataItemName.Copyright);

            return !String.IsNullOrWhiteSpace(copyright) ? new MetaValue(copyright, copyright) : null;
        }

        private IMetaValue GetWpfCopyright()
        {
            string wpfValue = null;

            try
            {
                wpfValue = WpfMetadataReader.Copyright;
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return (wpfValue != null ? new MetaValue(wpfValue.Trim(), wpfValue) : null);
        }

        private IMetaValue GetSubject()
        {
            var wpfSubject = GetWpfSubject();

            if (wpfSubject != null)
                return wpfSubject;
            else
                return null;
        }

        private IMetaValue GetWpfSubject()
        {
            string wpfValue = null;

            try
            {
                wpfValue = WpfMetadataReader.Subject;
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return (wpfValue != null ? new MetaValue(wpfValue.Trim(), wpfValue) : null);
        }

        private IMetaValue GetColorRepresentation()
        {
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifColorSpace, out rawMdi))
            {
                string value = rawMdi.Value.ToString().Trim();

                if (value == "1")
                    return new MetaValue(Resources.Metadata_ColorRepresentation_sRGB, value);
                else
                    return new MetaValue(Resources.Metadata_ColorRepresentation_Uncalibrated, value);
            }

            return null;
        }

        private IMetaValue GetDescription()
        {
            var desc = GetStringMetadataItem(RawMetadataItemName.ImageDescription);

            return (desc != null ? new MetaValue(desc, desc) : null);
        }

        private IMetaValue GetDimensions()
        {
            int width = GetWidthAsInt();
            int height = GetHeightAsInt();

            if ((width > 0) && (height > 0))
            {
                return new MetaValue(String.Concat(width, " x ", height));
            }

            return null;
        }

        private IMetaValue GetExposureCompensation()
        {
            double? expComp = null;
            MetadataItem rawMdi;

            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifExposureBias, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    expComp = ((Fraction)rawMdi.Value).ToSingle();
                }
            }

            if (!expComp.HasValue)
            {
                // It's not in our raw metadata. Try to use WPF.
                expComp = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_ExposureBias]) as double?;
            }

            if (expComp.HasValue)
            {
                return new MetaValue(
                  String.Concat(expComp.Value.ToString("##0.# ", CultureInfo.InvariantCulture), Resources.Metadata_ExposureCompensation_Suffix),
                  expComp.Value.ToString(CultureInfo.InvariantCulture));

            }

            return null;
        }

        private IMetaValue GetExposureProgram()
        {
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifExposureProg, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
                {
                    var expProgram = (ExposureProgram)(Int64)rawMdi.Value;
                    if (MetadataEnumHelper.IsValidExposureProgram(expProgram))
                    {
                        return new MetaValue(expProgram.ToString(), ((ushort)expProgram).ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            // It's not in our raw metadata. Try to use WPF.
            var expProgramWpf = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_ExposureProgram]) as ushort?;

            if (expProgramWpf.HasValue && MetadataEnumHelper.IsValidExposureProgram((ExposureProgram)expProgramWpf))
            {
                return new MetaValue(((ExposureProgram)expProgramWpf).ToString(), expProgramWpf.Value.ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetPeople()
        {
            var peopleArray = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_PeopleNames]) as string[];

            if (peopleArray != null)
            {
                var people = string.Join(",", peopleArray);

                return new MetaValue(people, people);
            }

            return null;
        }

        private IMetaValue GetOrientation()
        {
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.Orientation, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
                {
                    var orientation = (Orientation)(Int64)rawMdi.Value;
                    if (MetadataEnumHelper.IsValidOrientation(orientation))
                    {
                        return new MetaValue(orientation.GetDescription(), ((ushort)orientation).ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            // It's not in our raw metadata. Try to use WPF.
            var orientationWpf = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_Orientation]) as ushort?;

            if (orientationWpf.HasValue && MetadataEnumHelper.IsValidOrientation((Orientation)orientationWpf))
            {
                return new MetaValue(((Orientation)orientationWpf).ToString(), orientationWpf.Value.ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetExposureTime()
        {
            string exposureTime;
            MetadataItem rawMdi;
            const Single numSeconds = 1; // If the exposure time is less than this # of seconds, format as fraction (1/350 sec.); otherwise convert to Single (2.35 sec.)
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifExposureTime, out rawMdi))
            {
                if ((rawMdi.ExtractedValueType == ExtractedValueType.Fraction) && ((Fraction)rawMdi.Value).ToSingle() > numSeconds)
                {
                    exposureTime = Math.Round(((Fraction)rawMdi.Value).ToSingle(), 2).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    exposureTime = rawMdi.Value.ToString();
                }

                return new MetaValue(String.Concat(exposureTime, " ", Resources.Metadata_ExposureTime_Units), exposureTime);
            }

            // It's not in our raw metadata. Try to use WPF.
            var expTimeNum = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_ExposureTimeNumerator]) as uint?;
            var expTimeDen = (expTimeNum.HasValue ? WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_ExposureTimeDenominator]) as uint? : new uint?());

            if (expTimeNum.HasValue && expTimeDen.HasValue)
            {
                exposureTime = new Fraction(expTimeNum.Value, expTimeDen.Value).ToString();

                return new MetaValue(String.Concat(exposureTime, " ", Resources.Metadata_ExposureTime_Units), exposureTime);
            }

            return null;
        }

        private IMetaValue GetFlashMode()
        {
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifFlash, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
                {
                    var flashMode = (FlashMode)(Int64)rawMdi.Value;
                    if (MetadataEnumHelper.IsValidFlashMode(flashMode))
                    {
                        return new MetaValue(flashMode.GetDescription(), ((ushort)flashMode).ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            // It's not in our raw metadata. Try to use WPF.
            var flashModeWpf = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_Flash]) as ushort?;

            if (flashModeWpf.HasValue && MetadataEnumHelper.IsValidFlashMode((FlashMode)flashModeWpf.Value))
            {
                return new MetaValue(((FlashMode)flashModeWpf.Value).GetDescription(), ((ushort)flashModeWpf).ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetFNumber()
        {
            double? fstop = null;
            MetadataItem rawMdi;

            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifFNumber, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    fstop = ((Fraction)rawMdi.Value).ToSingle();
                }
            }

            if (!fstop.HasValue)
            {
                // It's not in our raw metadata. Try to use WPF.
                fstop = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_FNumber]) as double?;
            }

            if (fstop.HasValue)
            {
                return new MetaValue(fstop.Value.ToString("f/##0.#", CultureInfo.InvariantCulture), fstop.Value.ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetFocalLength()
        {
            double? focalLength = null;
            MetadataItem rawMdi;

            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifFocalLength, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    focalLength = ((Fraction)rawMdi.Value).ToSingle();
                }
            }

            if (!focalLength.HasValue)
            {
                // It's not in our raw metadata. Try to use WPF.
                focalLength = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_FocalLength]) as double?;
            }

            if (focalLength.HasValue)
            {
                return new MetaValue(String.Concat(Math.Round(focalLength.Value), " ", Resources.Metadata_FocalLength_Units), focalLength.Value.ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetHeight()
        {
            int height = GetHeightAsInt();

            return (height > 0 ? new MetaValue(String.Concat(height, " ", Resources.Metadata_Height_Units), height.ToString(CultureInfo.InvariantCulture)) : null);
        }

        private IMetaValue GetHorizontalResolution()
        {
            MetadataItem rawMdi;
            string resolutionUnit = String.Empty;

            if (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionXUnit, out rawMdi))
            {
                resolutionUnit = rawMdi.Value.ToString();
            }

            if ((String.IsNullOrWhiteSpace(resolutionUnit)) && (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionUnit, out rawMdi)))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
                {
                    ResolutionUnit resUnit = (ResolutionUnit)(Int64)rawMdi.Value;
                    if (MetadataEnumHelper.IsValidResolutionUnit(resUnit))
                    {
                        resolutionUnit = resUnit.ToString();
                    }
                }
            }

            if (RawMetadata.TryGetValue(RawMetadataItemName.XResolution, out rawMdi))
            {
                string xResolution;
                if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    xResolution = Math.Round(((Fraction)rawMdi.Value).ToSingle(), 2).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    xResolution = rawMdi.Value.ToString();
                }

                return new MetaValue(String.Concat(xResolution, " ", resolutionUnit), xResolution);
            }

            return null;
        }

        private IMetaValue GetIsoSpeed()
        {
            var iso = GetStringMetadataItem(RawMetadataItemName.ExifISOSpeed);

            if (!String.IsNullOrEmpty(iso))
            {
                return new MetaValue(iso, iso);
            }

            // It's not in our raw metadata. Try to use WPF.
            var isoSpeed = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_ISOSpeed]) as ushort?;

            if (isoSpeed.HasValue)
            {
                return new MetaValue(String.Concat(Resources.Metadata_ISO_Prefix, isoSpeed.Value), isoSpeed.Value.ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetLensAperture()
        {
            // The aperture is the same as the F-Number if present; otherwise it is calculated from ExifAperture.
            var mi = GetFNumber();
            if (mi != null)
            {
                return mi;
            }

            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifAperture, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    var apertureRaw = ((Fraction)rawMdi.Value).ToSingle();
                    var exifFNumber = (float)Math.Round(Math.Pow(Math.Sqrt(2), apertureRaw), 1);
                    var aperture = exifFNumber.ToString("f/##0.#", CultureInfo.InvariantCulture);

                    return new MetaValue(aperture, apertureRaw.ToString(CultureInfo.InvariantCulture));
                }
            }

            // It's not in our raw metadata. Try to use WPF.
            var apertureRawWpf = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_Aperture]) as double?;

            if (apertureRawWpf.HasValue)
            {
                var exifFNumberWpf = (float)Math.Round(Math.Pow(Math.Sqrt(2), apertureRawWpf.Value), 1);
                var apertureWpf = exifFNumberWpf.ToString("f/##0.#", CultureInfo.InvariantCulture);

                return new MetaValue(apertureWpf, apertureRawWpf.Value.ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetLightSource()
        {
            LightSource lightSource = LightSource.Unknown;
            var foundExifValue = false;
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifLightSource, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
                {
                    lightSource = (LightSource)(Int64)rawMdi.Value;
                    foundExifValue = true;
                }
            }

            if (!foundExifValue)
            {
                // It's not in our raw metadata. Try to use WPF.
                var lightSourceWpf = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_LightSource]) as ushort?;

                if (lightSourceWpf.HasValue)
                {
                    lightSource = (LightSource)lightSourceWpf.Value;
                }
            }

            if (MetadataEnumHelper.IsValidLightSource(lightSource))
            {
                // Don't bother with it if it is "Unknown"
                if (lightSource != LightSource.Unknown)
                {
                    return new MetaValue(lightSource.GetDescription(), ((ushort)lightSource).ToString(CultureInfo.InvariantCulture));
                }
            }

            return null;
        }

        private IMetaValue GetMeteringMode()
        {
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifMeteringMode, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
                {
                    var meterMode = (MeteringMode)(Int64)rawMdi.Value;
                    if (MetadataEnumHelper.IsValidMeteringMode(meterMode))
                    {
                        return new MetaValue(meterMode.ToString(), ((ushort)meterMode).ToString(CultureInfo.InvariantCulture));
                    }
                }
            }

            // It's not in our raw metadata. Try to use WPF.
            var meterModeWpf = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_MeteringMode]) as ushort?;

            if (meterModeWpf.HasValue && MetadataEnumHelper.IsValidMeteringMode((MeteringMode)meterModeWpf.Value))
            {
                return new MetaValue((((MeteringMode)meterModeWpf.Value).ToString()), meterModeWpf.Value.ToString(CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetSubjectDistance()
        {
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifSubjectDist, out rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    double distance = ((Fraction)rawMdi.Value).ToSingle();

                    if (distance > 1)
                    {
                        distance = Math.Round(distance, 1);
                    }

                    return new MetaValue(String.Concat(distance.ToString("0.### ", CultureInfo.InvariantCulture), Resources.Metadata_SubjectDistance_Units), distance.ToString("0.### ", CultureInfo.InvariantCulture));
                }
                else
                {
                    string value = rawMdi.Value.ToString().Trim().TrimEnd(new[] { '\0' });

                    if (!String.IsNullOrWhiteSpace(value))
                    {
                        return new MetaValue(String.Format(CultureInfo.CurrentCulture, String.Concat("{0} ", Resources.Metadata_SubjectDistance_Units), value), value);
                    }
                }
            }

            // It's not in our raw metadata. Try to use WPF.
            var distanceWpf = WpfMetadataReader.GetQuery(WpfMetadataQueryStrings[SystemMetaProperty.System_Photo_FocalLength]) as double?;

            if (distanceWpf.HasValue)
            {
                if (distanceWpf.Value > 1)
                {
                    distanceWpf = Math.Round(distanceWpf.Value, 1);
                }

                return new MetaValue(String.Concat(distanceWpf.Value.ToString("0.### ", CultureInfo.InvariantCulture), Resources.Metadata_SubjectDistance_Units), distanceWpf.Value.ToString("0.### ", CultureInfo.InvariantCulture));
            }

            return null;
        }

        private IMetaValue GetVerticalResolution()
        {
            MetadataItem rawMdi;
            string resolutionUnit = String.Empty;

            if (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionYUnit, out rawMdi))
            {
                resolutionUnit = rawMdi.Value.ToString();
            }

            if ((String.IsNullOrWhiteSpace(resolutionUnit)) && (RawMetadata.TryGetValue(RawMetadataItemName.ResolutionUnit, out rawMdi)))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.Int64)
                {
                    ResolutionUnit resUnit = (ResolutionUnit)(Int64)rawMdi.Value;
                    if (MetadataEnumHelper.IsValidResolutionUnit(resUnit))
                    {
                        resolutionUnit = resUnit.ToString();
                    }
                }
            }

            if (RawMetadata.TryGetValue(RawMetadataItemName.YResolution, out rawMdi))
            {
                string yResolution;
                if (rawMdi.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    yResolution = Math.Round(((Fraction)rawMdi.Value).ToSingle(), 2).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    yResolution = rawMdi.Value.ToString();
                }

                return new MetaValue(String.Concat(yResolution, " ", resolutionUnit), yResolution);
            }

            return null;
        }

        private IMetaValue GetWidth()
        {
            int width = GetWidthAsInt();

            return (width > 0 ? new MetaValue(String.Concat(width, " ", Resources.Metadata_Width_Units), width.ToString(CultureInfo.InvariantCulture)) : null);
        }

        private IMetaValue GetGpsValue(MetadataItemName metaName)
        {
            switch (metaName)
            {
                case MetadataItemName.GpsVersion:
                    return (!String.IsNullOrWhiteSpace(GpsLocation.Version) ? new MetaValue(GpsLocation.Version, GpsLocation.Version) : null);

                case MetadataItemName.GpsLocation:
                    if ((GpsLocation.Latitude != null) && (GpsLocation.Longitude != null))
                    {
                        var loc = GpsLocation.ToLatitudeLongitudeDecimalString();
                        return new MetaValue(loc, loc);
                    }
                    else
                        return null;

                case MetadataItemName.GpsLatitude:
                    if ((GpsLocation.Latitude != null) && (GpsLocation.Longitude != null))
                    {
                        var lat = GpsLocation.Latitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture);
                        return new MetaValue(lat, lat);
                    }
                    else
                        return null;

                case MetadataItemName.GpsLongitude:
                    if ((GpsLocation.Latitude != null) && (GpsLocation.Longitude != null))
                    {
                        var longitude = GpsLocation.Longitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture);
                        return new MetaValue(longitude, longitude);
                    }
                    else
                        return null;

                case MetadataItemName.GpsAltitude:
                    if (GpsLocation.Altitude.HasValue)
                    {
                        var altitude = GpsLocation.Altitude.Value.ToString("N0", CultureInfo.CurrentCulture);
                        return new MetaValue(String.Concat(altitude, " ", Resources.Metadata_meters), altitude);
                    }
                    else
                        return null;

                case MetadataItemName.GpsDestLocation:
                    if ((GpsLocation.DestLatitude != null) && (GpsLocation.DestLongitude != null))
                    {
                        var loc = GpsLocation.ToDestLatitudeLongitudeDecimalString();
                        return new MetaValue(loc, loc);
                    }
                    else
                        return null;

                case MetadataItemName.GpsDestLatitude:
                    if ((GpsLocation.DestLatitude != null) && (GpsLocation.DestLongitude != null))
                    {
                        var lat = GpsLocation.DestLatitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture);
                        return new MetaValue(lat, lat);
                    }
                    else
                        return null;

                case MetadataItemName.GpsDestLongitude:
                    if ((GpsLocation.DestLatitude != null) && (GpsLocation.DestLongitude != null))
                    {
                        var longitude = GpsLocation.DestLongitude.ToDouble().ToString("F6", CultureInfo.InvariantCulture);
                        return new MetaValue(longitude, longitude);
                    }
                    else
                        return null;

                default:
                    throw new ArgumentException(string.Format("The function GetGpsValue() expects a GPS-related parameter; instead the value {0} was passed.", metaName), "metaName");
            }
        }

        private IMetaValue GetIptcValue(MetadataItemName metaName)
        {
            string iptcValue = null;
            try
            {
                // May come back as a string or a string array (IPTC keywords that are stored with * delimiters come back as string arrays)
                var iptcValueObj = WpfMetadataReader.GetQuery(string.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, IptcQueryParameters[metaName]));

                var iptcValueStr = iptcValueObj as string;
                if (iptcValueStr != null)
                {
                    iptcValue = iptcValueStr;
                }
                else if (iptcValueObj is string[])
                {
                    const int maxTagLength = 100; // Due to database constraints, tags cannot be longer than 100 characters
                    iptcValue = ConvertToDelimitedString((string[])iptcValueObj, maxTagLength);
                }
            }
            catch (ArgumentNullException)
            {
                // Some images throw this exception. When this happens, just exit.
                return null;
            }
            catch (ArgumentException)
            {
                // Some images throw this exception. When this happens, just exit.
                return null;
            }
            catch (InvalidOperationException)
            {
                // Some images throw this exception. When this happens, just exit.
                return null;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Some images throw this exception (NEF). When this happens, just exit.
                return null;
            }

            if (String.IsNullOrWhiteSpace(iptcValue))
                return null;

            var formattedIptcValue = iptcValue;

            // For dates, format to a specific pattern.
            if (metaName == MetadataItemName.IptcDateCreated)
            {
                var dateTaken = TryParseDate(iptcValue);

                if (dateTaken.Year > DateTime.MinValue.Year)
                    formattedIptcValue = dateTaken.ToString(DateTimeFormatString, CultureInfo.InvariantCulture);
            }

            return new MetaValue(formattedIptcValue, iptcValue);
        }

        /// <summary>
        /// Gets the meta value to write to the file, formatting it if necessary. For example, the IPTC date created value should
        /// be stored in the format yyyMMdd.
        /// </summary>
        /// <param name="metaItem">The meta item to be persisted to the file.</param>
        /// <returns>System.String.</returns>
        private static string GetMetaValueForFile(IGalleryObjectMetadataItem metaItem)
        {
            switch (metaItem.MetadataItemName)
            {
                case MetadataItemName.DatePictureTaken:
                    var dateTaken1 = TryParseDate(metaItem.Value);
                    return (dateTaken1.Year > DateTime.MinValue.Year ? dateTaken1.ToString("O", CultureInfo.InvariantCulture) : metaItem.Value);

                case MetadataItemName.IptcDateCreated:
                    var dateTaken2 = TryParseDate(metaItem.Value);
                    return (dateTaken2.Year > DateTime.MinValue.Year ? ToIptcDate(dateTaken2) : metaItem.Value);

                default:
                    return metaItem.Value;
            }
        }

        private static string ToIptcDate(DateTime dte)
        {
            return dte.ToString("yyyMMdd");
        }

        private static string ToIptcTime(DateTime dte)
        {
            return dte.ToString("HHmmss");
        }

        /// <summary>
        /// Fill the class-level _rawMetadata dictionary with MetadataItem objects created from the
        /// PropertyItems property of the image. Skip any items that are not defined in the 
        /// RawMetadataItemName enumeration. Guaranteed to not return null.
        /// </summary>
        private Dictionary<RawMetadataItemName, MetadataItem> GetRawMetadataDictionary()
        {
            var rawMetadata = new Dictionary<RawMetadataItemName, MetadataItem>();

            foreach (var itemIterator in PropertyItems)
            {
                var metadataName = (RawMetadataItemName)itemIterator.Id;
                if (Enum.IsDefined(typeof(RawMetadataItemName), metadataName))
                {
                    if (!rawMetadata.ContainsKey(metadataName))
                    {
                        var metadataItem = new MetadataItem(itemIterator);
                        if (metadataItem.Value != null)
                            rawMetadata.Add(metadataName, metadataItem);
                    }
                }
            }

            return rawMetadata;
        }

        private static PropertyItem[] GetPropertyItemsUsingFullTrustTechnique(string imageFilePath)
        {
            // This technique is fast but requires full trust. Can only be called when app is running under full trust.
            if (AppSetting.Instance.AppTrustLevel != ApplicationTrustLevel.Full)
                throw new InvalidOperationException("The method MediaObjectMetadataExtractor.GetPropertyItemsUsingFullTrustTechnique can only be called when the application is running under full trust. The application should have already checked for this before calling this method. The developer needs to modify the source code to fix this.");

            using (Stream stream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream, true, false))
                    {
                        try
                        {
                            return image.PropertyItems;
                        }
                        catch (NotImplementedException)
                        {
                            // Some images, such as wmf, throw this exception. We'll make a note of it and set our field to an empty array.
                            //if (!ex.Data.Contains("Metadata Extraction Error"))
                            //{
                            //	ex.Data.Add("Metadata Extraction Error", String.Format(CultureInfo.CurrentCulture, "Cannot extract metadata from file \"{0}\".", imageFilePath));
                            //}

                            //LogError(ex, GalleryObject.GalleryId);
                            return new PropertyItem[0];
                        }
                    }
                }
                catch (ArgumentException)
                {
                    //if (!ex.Data.Contains("Metadata Extraction Error"))
                    //{
                    //	ex.Data.Add("Metadata Extraction Error", String.Format(CultureInfo.CurrentCulture, "Cannot extract metadata from file \"{0}\".", imageFilePath));
                    //}

                    //LogError(ex, GalleryObject.GalleryId);
                    return new PropertyItem[0];
                }
            }
        }

        private static PropertyItem[] GetPropertyItemsUsingLimitedTrustTechnique(string imageFilePath)
        {
            // This technique is not as fast as the one in the method GetPropertyItemsUsingFullTrustTechnique() but in works in limited
            // trust environments.
            try
            {
                using (System.Drawing.Image image = new System.Drawing.Bitmap(imageFilePath))
                {
                    try
                    {
                        return image.PropertyItems;
                    }
                    catch (NotImplementedException)
                    {
                        // Some images, such as wmf, throw this exception.
                        return new PropertyItem[0];
                    }
                }
            }
            catch (ArgumentException)
            {
                return new PropertyItem[0];
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                // Some images throw this. Here is one: "D:\Media samples\user submitted objects\6198 Windpendel Windrad.tif"
                return new PropertyItem[0];
            }

        }

        /// <summary>
        /// Get a reference to the <see cref="BitmapMetadata" /> object for this image file that contains 
        /// the metadata such as title, keywords, etc. Guaranteed to not return null. Returns an instance 
        /// of <see cref="NullObjects.NullWpfMetadata" /> if an actual <see cref="BitmapMetadata" /> object 
        /// is not available.
        /// </summary>
        /// <returns> Returns a reference to an <see cref="IWpfMetadata" /> instance.</returns>
        private IWpfMetadata GetBitmapMetadataReader()
        {
            if ((AppSetting.Instance.AppTrustLevel < ApplicationTrustLevel.Full)
              || (!Factory.LoadGallerySetting(GalleryObject.GalleryId).ExtractMetadataUsingWpf)
              || string.IsNullOrWhiteSpace(GalleryObject.Original.FileNamePhysicalPath))
            {
                return new NullObjects.NullWpfMetadata();
            }

            return new WpfMetadata(GalleryObject);
        }

        /// <summary>
        /// Converts the <paramref name="stringCollection" /> to a comma-delimited string, ensuring that each comma-separated value
        /// is never longer than <paramref name="maxLengthOfEachItem" /> characters. Duplicate items are removed.
        /// </summary>
        /// <param name="stringCollection">The string collection.</param>
        /// <param name="maxLengthOfEachItem">The maximum length of each item. When omitted, defaults to <see cref="int.MaxValue" /></param>
        /// <returns>System.String.</returns>
        private static string ConvertToDelimitedString(IEnumerable<string> stringCollection, int maxLengthOfEachItem = int.MaxValue)
        {
            if (stringCollection == null)
                return null;

            // If any of the entries is itself a comma-separated list, parse them. Remove any duplicates.
            var strings = new List<string>();

            foreach (var s in stringCollection.Where(s => s != null))
            {
                strings.AddRange(s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s1 => new String(s1.Trim().Take(maxLengthOfEachItem).ToArray()).Trim()));
            }

            return string.Join(", ", strings.Distinct());
        }

        //private static void LogError(Exception ex, int galleryId)
        //{
        //  EventController.RecordError(ex, AppSetting.Instance, galleryId, Factory.LoadGallerySettings());
        //  CacheController.PurgeCache();
        //}

        private string GetStringMetadataItem(RawMetadataItemName sourceRawMetadataName, string formatString = "{0}")
        {
            MetadataItem rawMdi;
            string rawValue = null;

            if (RawMetadata.TryGetValue(sourceRawMetadataName, out rawMdi))
            {
                var unformattedValue = rawMdi.Value.ToString().Trim().TrimEnd(new[] { '\0' });

                rawValue = String.Format(CultureInfo.CurrentCulture, formatString, unformattedValue);
            }

            return rawValue;
        }

        /// <summary>
        /// Try to convert <paramref name="dteRaw" /> to a valid <see cref="DateTime" /> object. If it cannot be converted, return
        /// <see cref="DateTime.MinValue" />.
        /// </summary>
        /// <param name="dteRaw">The string containing the date/time to convert.</param>
        /// <returns>Returns a <see cref="DateTime" /> instance.</returns>
        /// <remarks>The IPTC specs do not define an exact format for the ITPC Date Created field, so it is unclear how to reliably parse
        /// it. However, an analysis of sample photos, including those provided by IPTC (http://www.iptc.org), show that the format
        /// yyyyMMdd is consistently used, so we'll try that if the more generic parsing doesn't work.</remarks>
        private static DateTime TryParseDate(string dteRaw)
        {
            DateTime result;
            if (DateTime.TryParse(dteRaw, out result))
            {
                return result;
            }
            else if (DateTime.TryParseExact(dteRaw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Convert an EXIF-formatted timestamp to the .NET DateTime type. Returns DateTime.MinValue when the date cannot be parsed.
        /// </summary>
        /// <param name="exifDateTime">An EXIF-formatted timestamp. The format is YYYY:MM:DD HH:MM:SS with time shown 
        /// in 24-hour format and the date and time separated by one blank character (0x2000). The character 
        /// string length is 20 bytes including the NULL terminator.</param>
        /// <returns>Returns the EXIF-formatted timestamp as a .NET DateTime type.</returns>
        private static DateTime ConvertExifDateTimeToDateTime(string exifDateTime)
        {
            DateTime convertedDateTimeValue = DateTime.MinValue;
            const int minCharsReqdToSpecifyDate = 10; // Need at least 10 characters to specify a date (e.g. 2010:10:15)

            if (String.IsNullOrWhiteSpace(exifDateTime) || (exifDateTime.Trim().Length < minCharsReqdToSpecifyDate))
                return convertedDateTimeValue; // No date/time is present; just return

            exifDateTime = exifDateTime.Trim();

            string[] ymdhms = exifDateTime.Split(new[] { ' ', ':' });

            // Default to lowest possible year, first month and first day
            int year = DateTime.MinValue.Year, month = 1, day = 1, hour = 0, minute = 0, second = 0;

            if (ymdhms.Length >= 2)
            {
                Int32.TryParse(ymdhms[0], out year);
                Int32.TryParse(ymdhms[1], out month);
                Int32.TryParse(ymdhms[2], out day);
            }

            if (ymdhms.Length >= 6)
            {
                // The hour, minute and second will default to 0 if it can't be parsed, which is good.
                Int32.TryParse(ymdhms[3], out hour);
                Int32.TryParse(ymdhms[4], out minute);
                Int32.TryParse(ymdhms[5], out second);
            }
            if (year > DateTime.MinValue.Year)
            {
                try
                {
                    convertedDateTimeValue = new DateTime(year, month, day, hour, minute, second);
                }
                catch (ArgumentOutOfRangeException) { }
                catch (ArgumentException) { }
            }

            return convertedDateTimeValue;
        }

        private IMetaValue GetDatePictureTakenWpf()
        {
            try
            {
                var dateTakenRaw = WpfMetadataReader.DateTaken;

                if (!String.IsNullOrWhiteSpace(dateTakenRaw))
                {
                    var dateTaken = TryParseDate(dateTakenRaw);
                    if (dateTaken.Year > DateTime.MinValue.Year)
                    {
                        return new MetaValue(dateTaken.ToString(DateTimeFormatString, CultureInfo.InvariantCulture), dateTaken.ToString("O", CultureInfo.InvariantCulture));
                    }
                    else
                        return new MetaValue(dateTakenRaw, dateTakenRaw); // We can't parse it so just return it as is
                }
            }
            catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }

            return null;
        }

        private IMetaValue GetDatePictureTakenGdi()
        {
            MetadataItem rawMdi;
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifDTOrig, out rawMdi))
            {
                var convertedDateTimeValue = ConvertExifDateTimeToDateTime(rawMdi.Value.ToString());
                if (convertedDateTimeValue > DateTime.MinValue)
                {
                    return new MetaValue(convertedDateTimeValue.ToString(DateTimeFormatString, CultureInfo.InvariantCulture), convertedDateTimeValue.ToString("O", CultureInfo.InvariantCulture));
                }
                else if (!String.IsNullOrWhiteSpace(rawMdi.Value.ToString()))
                {
                    return new MetaValue(rawMdi.Value.ToString(), rawMdi.Value.ToString());
                }
            }

            return null;
        }

        /// <summary>
        /// Get the height of the media object. Extracted from RawMetadataItemName.ExifPixXDim for compressed images and
        /// from RawMetadataItemName.ImageHeight for uncompressed images. The value is stored in a private class level variable
        /// for quicker subsequent access.
        /// </summary>
        /// <returns>Returns the height of the media object.</returns>
        private int GetWidthAsInt()
        {
            if (_width > 0)
                return _width;

            MetadataItem rawMdi;
            int width = int.MinValue;
            bool foundWidth = false;

            // Compressed images store their width in ExifPixXDim. Uncompressed images store their width in ImageWidth.
            // First look in ExifPixXDim since most images are likely to be compressed ones. If we don't find that one,
            // look for ImageWidth. If we don't find that one either (which should be unlikely to ever happen), then just give 
            // up and return null.
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifPixXDim, out rawMdi))
            {
                foundWidth = Int32.TryParse(rawMdi.Value.ToString(), out width);
            }

            if ((!foundWidth) && (RawMetadata.TryGetValue(RawMetadataItemName.ImageWidth, out rawMdi)))
            {
                foundWidth = Int32.TryParse(rawMdi.Value.ToString(), out width);
            }

            if (!foundWidth)
            {
                width = this.GalleryObject.Original.Width;
                foundWidth = (width > 0);
            }

            if (foundWidth)
                _width = width;

            return width;
        }

        /// <summary>
        /// Get the width of the media object. Extracted from RawMetadataItemName.ExifPixYDim for compressed images and
        /// from RawMetadataItemName.ImageWidth for uncompressed images. The value is stored in a private class level variable
        /// for quicker subsequent access.
        /// </summary>
        /// <returns>Returns the width of the media object.</returns>
        private int GetHeightAsInt()
        {
            if (_height > 0)
                return _height;

            MetadataItem rawMdi;
            int height = int.MinValue;
            bool foundHeight = false;

            // Compressed images store their width in ExifPixXDim. Uncompressed images store their width in ImageWidth.
            // First look in ExifPixXDim since most images are likely to be compressed ones. If we don't find that one,
            // look for ImageWidth. If we don't find that one either (which should be unlikely to ever happen), then just give 
            // up and return null.
            if (RawMetadata.TryGetValue(RawMetadataItemName.ExifPixYDim, out rawMdi))
            {
                foundHeight = Int32.TryParse(rawMdi.Value.ToString(), out height);
            }

            if ((!foundHeight) && (RawMetadata.TryGetValue(RawMetadataItemName.ImageHeight, out rawMdi)))
            {
                foundHeight = Int32.TryParse(rawMdi.Value.ToString(), out height);
            }

            if (!foundHeight)
            {
                height = this.GalleryObject.Original.Height;
                foundHeight = (height > 0);
            }

            if (foundHeight)
                _height = height;

            return height;
        }

        /// <summary>
        /// Persists the meta value to the original media file. No action is taken if <see cref="IGalleryObjectMetadataItem.PersistToFile" /> is <c>false</c>
        /// or if the application is running at less than full trust.
        /// </summary>
        /// <param name="metaName">Name of the meta.</param>
        /// <param name="persistAction">The persist action.</param>
        private void PersistMetaValue(MetadataItemName metaName, MetaPersistAction persistAction)
        {
            if (AppSetting.Instance.AppTrustLevel < ApplicationTrustLevel.Full)
                return;

            // Adapted from: https://code.google.com/p/flickrmetasync/source/browse/trunk/FlickrMetadataSync/Picture.cs?spec=svn29&r=29
            IGalleryObjectMetadataItem metaItem;
            if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem) && !metaItem.PersistToFile)
            {
                return; // Do nothing when meta item has PersistToFile set to false
            }

            lock (_sharedLock)
            {
                var inPlaceUpdateSuccessful = false;
                var filePath = GalleryObject.Original.FileNamePhysicalPath;

                try
                {
                    using (Stream savedFile = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        InPlaceBitmapMetadataWriter bitmapMetadata = null;
                        try
                        {
                            var output = BitmapDecoder.Create(savedFile, BitmapCreateOptions.None, BitmapCacheOption.Default);
                            bitmapMetadata = output.Frames[0].CreateInPlaceBitmapMetadataWriter();
                        }
                        catch (NotSupportedException) { }
                        catch (InvalidOperationException) { }
                        catch (ArgumentException) { }
                        catch (FileFormatException) { }
                        catch (IOException) { }
                        catch (OverflowException) { }

                        if (bitmapMetadata != null)
                        {
                            SetMetadata(bitmapMetadata, metaName, persistAction);

                            // Saving might fail if there isn't enough metadata padding to hold the new info.
                            inPlaceUpdateSuccessful = bitmapMetadata.TrySave();
                        }
                    }
                }
                catch (DirectoryNotFoundException) { }
                catch (IOException) { }
                catch (System.Runtime.InteropServices.COMException) { }
                catch (Exception ex)
                {
                    // Log the error. This should be a rare event. If it's common and unavoidable for certain files, we may need to add a catch clause.
                    if (!ex.Data.Contains("Meta Save Error"))
                    {
                        ex.Data.Add("Meta Save Error", String.Format("An unexpected error occurred while trying to write the meta property '{0}' to the original file for media object ID {1}. It was, however, persisted to the database and is still available for viewing in the gallery. If this error occurs frequently, please report it to Gallery Server.", metaName, GalleryObject.Id));
                    }

                    EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
                }

                // If the in-place save wasn't successful, try to save another way.
                if (!inPlaceUpdateSuccessful)
                {
                    var tmpFilePath = Path.Combine(AppSetting.Instance.TempUploadDirectory, String.Concat(Guid.NewGuid().ToString(), ".tmp"));

                    if (TryAlternateMethodsOfPersistingMetadata(filePath, tmpFilePath, metaName, persistAction))
                    {
                        HelperFunctions.MoveFileSafely(tmpFilePath, filePath);
                    }
                    else if (File.Exists(tmpFilePath))
                    {
                        File.Delete(tmpFilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Write or delete the meta property <paramref name="metaName" /> to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist.</param>
        /// <param name="persistAction">The persist action.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="metaName" /> has a value this function was not designed to handle.</exception>
        private void SetMetadata(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            switch (metaName)
            {
                case MetadataItemName.Orientation:
                    SetOrientationMetadata(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.Title:
                    SetTitle(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.Caption:
                    SetCaption(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.DatePictureTaken:
                    SetDatePictureTaken(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.Author:
                    SetAuthor(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.Tags:
                    SetTagsMetadata(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.Rating:
                    SetRating(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.Copyright:
                case MetadataItemName.EquipmentManufacturer:
                case MetadataItemName.CameraModel:
                case MetadataItemName.Subject:
                    SetMetaString(bitmapMetadata, metaName, persistAction);
                    break;

                case MetadataItemName.IptcByline:
                case MetadataItemName.IptcBylineTitle:
                case MetadataItemName.IptcCaption:
                case MetadataItemName.IptcCity:
                case MetadataItemName.IptcCopyrightNotice:
                case MetadataItemName.IptcCountryPrimaryLocationName:
                case MetadataItemName.IptcCredit:
                case MetadataItemName.IptcDateCreated:
                case MetadataItemName.IptcHeadline:
                case MetadataItemName.IptcKeywords:
                case MetadataItemName.IptcObjectName:
                case MetadataItemName.IptcOriginalTransmissionReference:
                case MetadataItemName.IptcProvinceState:
                case MetadataItemName.IptcRecordVersion:
                case MetadataItemName.IptcSource:
                case MetadataItemName.IptcSpecialInstructions:
                case MetadataItemName.IptcSublocation:
                case MetadataItemName.IptcWriterEditor:
                    SetIptcValue(bitmapMetadata, metaName, persistAction);
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", metaName));
            }
        }

        /// <summary>
        /// Write or delete the orientation meta value to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be <see cref="MetadataItemName.Orientation" />.</param>
        /// <param name="persistAction">The persist action.</param>
        private void SetOrientationMetadata(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            const string orientationMetaPath = "/app1/ifd/{ushort=274}";

            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.RemoveQuery(orientationMetaPath);
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem orientationMeta;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out orientationMeta))
                    {
                        ushort orientationRaw;
                        if (UInt16.TryParse(orientationMeta.RawValue, out orientationRaw) && MetadataEnumHelper.IsValidOrientation((Orientation)orientationRaw))
                        {
                            bitmapMetadata.SetQuery(orientationMetaPath, orientationRaw);
                        }
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Write or delete the title meta value to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be <see cref="MetadataItemName.Title" />.</param>
        /// <param name="persistAction">The persist action.</param>
        private void SetTitle(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.Title = null;
                    bitmapMetadata.RemoveQuery(IptcQueryParameters[MetadataItemName.IptcHeadline]);
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem metaItem;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem))
                    {
                        // Assigning the Title property pushes it into the EXIF (ImageDescription & XPTitle) and XMP (Title & Description) fields; the SetQuery takes care of IPTC.
                        bitmapMetadata.Title = metaItem.Value;
                        bitmapMetadata.SetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, IptcQueryParameters[MetadataItemName.IptcHeadline]), metaItem.Value);
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Write or delete the caption meta value to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be <see cref="MetadataItemName.Caption" />.</param>
        /// <param name="persistAction">The persist action.</param>
        private void SetCaption(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            const string xmpMetaPath = "/xmp/xmp:Description";

            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.Comment = null;
                    bitmapMetadata.RemoveQuery(xmpMetaPath);
                    bitmapMetadata.RemoveQuery(IptcQueryParameters[MetadataItemName.IptcCaption]);
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem metaItem;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem))
                    {
                        // Assigning the Comment property pushes it into the EXIF (XPComment) and XMP (Description) fields; the SetQuery takes care of IPTC.
                        bitmapMetadata.Comment = metaItem.Value;
                        bitmapMetadata.SetQuery(xmpMetaPath, metaItem.Value);
                        bitmapMetadata.SetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, IptcQueryParameters[MetadataItemName.IptcCaption]), metaItem.Value);
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Write or delete the date photo taken meta value to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be <see cref="MetadataItemName.DatePictureTaken" />.</param>
        /// <param name="persistAction">The persist action.</param>
        private void SetDatePictureTaken(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.DateTaken = null;
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem metaItem;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem))
                    {
                        // Assigning the DateTaken property pushes it into the EXIF (DateTimeOriginal & CreateDate), XMP (CreateDate) fields,
                        // and sometimes the IPTC DateCreated field (see below for more info).
                        var dateTaken = GetMetaValueForFile(metaItem);
                        if (!string.IsNullOrWhiteSpace(dateTaken))
                        {
                            // Only assign when we have a valid date or else .NET throws an exception
                            bitmapMetadata.DateTaken = dateTaken;
                        }

                        var iptc = bitmapMetadata.GetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, IptcQueryParameters[MetadataItemName.IptcDateCreated])) as string;
                        if (iptc == null)
                        {
                            // When ITPC DateCreated doesn't exist, write it (and TimeCreated). Don't do anything when it *does* exist, because in that case 
                            // setting the DateTaken property above writes to these IPTC properties. (Specifically, it appears to write them if *any* IPTC
                            // properties exist.)
                            var dte = TryParseDate(metaItem.Value);
                            if (dte.Year > DateTime.MinValue.Year)
                            {
                                bitmapMetadata.SetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, IptcQueryParameters[MetadataItemName.IptcDateCreated]), ToIptcDate(dte));
                                bitmapMetadata.SetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, "Time Created"), ToIptcTime(dte));
                            }
                        }
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Write or delete the author meta value to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be <see cref="MetadataItemName.Author" />.</param>
        /// <param name="persistAction">The persist action.</param>
        private void SetAuthor(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.Author = null;
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem metaItem;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem))
                    {
                        // Assigning the Author property pushes it into the EXIF (Artist, XPAuthor) and XMP (Creator) fields.
                        bitmapMetadata.Author = new System.Collections.ObjectModel.ReadOnlyCollection<string>(metaItem.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Write or delete the meta value specified by <paramref name="metaName" /> to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be one of these values: <see cref="MetadataItemName.EquipmentManufacturer" />,
        /// <see cref="MetadataItemName.CameraModel" />, <see cref="MetadataItemName.Copyright" />, <see cref="MetadataItemName.Subject" />.</param>
        /// <param name="persistAction">The persist action.</param>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="metaName" /> or <paramref name="persistAction" /> specifies a 
        /// value not supported in the function.</exception>
        private void SetMetaString(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            var supportedProps = new[] { MetadataItemName.EquipmentManufacturer, MetadataItemName.CameraModel, MetadataItemName.Copyright, MetadataItemName.Subject };

            if (Array.IndexOf(supportedProps, metaName) < 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function does not support writing the meta property '{0}' to a file.", metaName), "metaName");
            }

            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    switch (metaName)
                    {
                        case MetadataItemName.EquipmentManufacturer:
                            bitmapMetadata.CameraManufacturer = null;
                            break;

                        case MetadataItemName.CameraModel:
                            bitmapMetadata.CameraModel = null;
                            break;

                        case MetadataItemName.Copyright:
                            bitmapMetadata.Copyright = null;
                            break;

                        case MetadataItemName.Subject:
                            bitmapMetadata.Subject = null;
                            break;
                    }
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem metaItem;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem))
                    {
                        switch (metaName)
                        {
                            case MetadataItemName.EquipmentManufacturer:
                                bitmapMetadata.CameraManufacturer = metaItem.Value; // EXIF (Make); no XMP or IPTC
                                break;

                            case MetadataItemName.CameraModel:
                                bitmapMetadata.CameraModel = metaItem.Value; // EXIF (Model); no XMP or IPTC
                                break;

                            case MetadataItemName.Copyright:
                                bitmapMetadata.Copyright = metaItem.Value; // EXIF (Copyright) and XMP (Rights); no IPTC
                                break;

                            case MetadataItemName.Subject:
                                bitmapMetadata.Subject = metaItem.Value; // EXIF (XPSubject); no XMP or IPTC
                                break;
                        }
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Update <see cref="BitmapMetadata.Rating" /> with the value of the rating meta value in <see cref="GalleryObject" />.
        /// Note that the rating is the averaged rating by all users, not the value selected by the user that triggered the
        /// current operation. Also note that because the rating property is an integer, the actual rating is rounded to the
        /// nearest integer (e.g. "1.5" is stored as "2" in the file).
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be <see cref="MetadataItemName.Rating" />.</param>
        /// <param name="persistAction">The persist action.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="bitmapMetadata" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="metaName" /> or
        /// <paramref name="persistAction" /> contain an invalid value.</exception>
        private void SetRating(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            if (bitmapMetadata == null)
            {
                throw new ArgumentNullException("bitmapMetadata");
            }

            if (metaName != MetadataItemName.Rating)
            {
                throw new ArgumentException(String.Format("The metaName parameter must be {0}. Instead, it was {1}.", MetadataItemName.Rating, metaName));
            }

            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.Rating = 0;
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem ratingMeta;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out ratingMeta))
                    {
                        // Assigning the Rating property pushes it into the EXIF (Rating) and XMP (Rating) fields; there is no IPTC equivalent.
                        decimal decRating;
                        if (decimal.TryParse(ratingMeta.Value, out decRating))
                        {
                            bitmapMetadata.Rating = (int)Math.Round(decRating, 0, MidpointRounding.AwayFromZero);
                        }
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Write or delete the tags meta value to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be <see cref="MetadataItemName.Tags" />.</param>
        /// <param name="persistAction">The persist action.</param>
        private void SetTagsMetadata(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.Keywords = new System.Collections.ObjectModel.ReadOnlyCollection<string>(new List<string>());
                    bitmapMetadata.RemoveQuery(IptcQueryParameters[MetadataItemName.IptcKeywords]);
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem metaItem;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem))
                    {
                        // Assigning the Keywords property pushes it into the EXIF and XMP fields; the SetQuery takes care of IPTC.
                        var tagArray = metaItem.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tagArray.Length == 0)
                        {
                            if (bitmapMetadata.Keywords != null)
                            {
                                // Only update when we need to clear the tags. If we set this to an empty collection when Keywords is null,
                                // the save will fail with "This codec does not support the specified property."
                                bitmapMetadata.Keywords = new System.Collections.ObjectModel.ReadOnlyCollection<string>(new string[0]);
                            }
                        }
                        else
                        {
                            bitmapMetadata.Keywords = new System.Collections.ObjectModel.ReadOnlyCollection<string>(tagArray);
                        }

                        bitmapMetadata.SetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, IptcQueryParameters[MetadataItemName.IptcKeywords]), metaItem.Value);
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Write or delete the IPTC meta value to the <paramref name="bitmapMetadata" />.
        /// </summary>
        /// <param name="bitmapMetadata">An instance of <see cref="BitmapMetadata" /> corresponding to the <see cref="GalleryObject" />.</param>
        /// <param name="metaName">Meta name to persist. Must be one of the IPTC settings defined in <see cref="IptcQueryParameters" />.</param>
        /// <param name="persistAction">The persist action.</param>
        private void SetIptcValue(BitmapMetadata bitmapMetadata, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            switch (persistAction)
            {
                case MetaPersistAction.Delete:
                    bitmapMetadata.RemoveQuery(IptcQueryParameters[metaName]);
                    break;

                case MetaPersistAction.Save:
                    IGalleryObjectMetadataItem metaItem;
                    if (GalleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem))
                    {
                        bitmapMetadata.SetQuery(String.Format(CultureInfo.InvariantCulture, IptcQueryFormatString, IptcQueryParameters[metaName]), GetMetaValueForFile(metaItem));
                    }
                    break;

                default:
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "This function is not designed to handle the enumeration value {0}. The function must be updated.", persistAction));
            }
        }

        /// <summary>
        /// Try several methods for persisting metadata, returning <c>true</c> if successful. When successful, <paramref name="tmpFilePath" />
        /// is a copy of <paramref name="originalFilePath" /> with the metadata persisted. The calling function should replace the original
        /// file with this new one. The file must be JPG or JPEG; if not, no action is taken. Any exceptions that occur are 
        /// logged and swallowed.
        /// </summary>
        /// <param name="originalFilePath">The path to the original file.</param>
        /// <param name="tmpFilePath">The path for a temporary file. The file will be created and does not need to exist prior to calling this function.</param>
        /// <param name="metaName">Name of the meta property to persist to the file.</param>
        /// <param name="persistAction">The persist action.</param>
        /// <returns><c>true</c> if the meta property was successfully persisted to the file, <c>false</c> otherwise.</returns>
        private bool TryAlternateMethodsOfPersistingMetadata(string originalFilePath, string tmpFilePath, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            if (!IsOriginalJpegImage())
            {
                return false; // Technique below requires JPG images, so just return if we don't have a JPG or JPEG file.
            }

            // First try to create a cloned copy of the original file's metadata, then add some padding and write the new property
            // to the file at tmpFilePath.
            try
            {
                using (Stream originalFile = new FileStream(originalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapDecoder bmpDecoderOriginal;
                    try
                    {
                        const BitmapCreateOptions createOptions = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;
                        bmpDecoderOriginal = BitmapDecoder.Create(originalFile, createOptions, BitmapCacheOption.None);
                    }
                    catch (NotSupportedException) { return false; }
                    catch (InvalidOperationException) { return false; }
                    catch (ArgumentException) { return false; }
                    catch (FileFormatException) { return false; }
                    catch (IOException) { return false; }
                    catch (OverflowException) { return false; }

                    var encoder = GetEncoder();

                    if (encoder == null)
                    {
                        return false;
                    }

                    if (bmpDecoderOriginal.Frames.Count > 0 && bmpDecoderOriginal.Frames[0] != null && bmpDecoderOriginal.Frames[0].Metadata != null)
                    {
                        // Attempt #1: Clone the original meta, add some padding, and create a new temp file.
                        var bitmapMetadata = bmpDecoderOriginal.Frames[0].Metadata.Clone() as BitmapMetadata;

                        if (bitmapMetadata != null)
                        {
                            bitmapMetadata.SetQuery("/app1/ifd/PaddingSchema:Padding", MetadataPaddingInBytes);
                            bitmapMetadata.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", MetadataPaddingInBytes);
                            bitmapMetadata.SetQuery("/xmp/PaddingSchema:Padding", MetadataPaddingInBytes);

                            SetMetadata(bitmapMetadata, metaName, persistAction);

                            encoder.Frames.Add(BitmapFrame.Create(bmpDecoderOriginal.Frames[0], bmpDecoderOriginal.Frames[0].Thumbnail, bitmapMetadata, bmpDecoderOriginal.Frames[0].ColorContexts));
                        }
                    }

                    try
                    {
                        using (Stream tmpFileStream = File.Open(tmpFilePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            encoder.Save(tmpFileStream);

                            return true;
                        }
                    }
                    catch (NotSupportedException)
                    {
                        return TryAlternateMethod2OfPersistingMetadata(bmpDecoderOriginal, tmpFilePath, metaName, persistAction);
                    }
                    catch (ArgumentException)
                    {
                        return TryAlternateMethod2OfPersistingMetadata(bmpDecoderOriginal, tmpFilePath, metaName, persistAction);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Meta Save Error"))
                {
                    ex.Data.Add("Meta Save Error", String.Format("Unable to persist the meta property '{0}' to the original file for media object ID {1}. It was, however, persisted to the database and is still available for viewing in the gallery.", metaName, GalleryObject.Id));
                }

                EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());

                return false;
            }
        }

        /// <summary>
        /// Gets the encoder that matches the file extension. Currently returns the <see cref="JpegBitmapEncoder" /> for JPG and JPEG
        /// files; returns null for all others.
        /// </summary>
        /// <returns>An instance of <see cref="BitmapEncoder" /> or null.</returns>
        private BitmapEncoder GetEncoder()
        {
            switch (Path.GetExtension(GalleryObject.Original.FileName).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return new JpegBitmapEncoder
                    {
                        QualityLevel = Factory.LoadGallerySetting(GalleryObject.GalleryId).OriginalImageJpegQuality
                    };

                //case ".tif":
                //case ".tiff":
                //	return new TiffBitmapEncoder(); // This supports some meta writing but messes up colors for some TIFF images

                //case ".png":
                //	return new PngBitmapEncoder(); // Doesn't support meta writing

                //case ".gif":
                //	return new GifBitmapEncoder(); // Doesn't support meta writing

                default:
                    return null;
            }
        }

        /// <summary>
        /// Write the meta property identified by <paramref name="metaName" /> using a technique that copies the original file,
        /// then generates another new file using a clone of the copied file's metadata with additional padding added. Returns
        /// <c>true</c> if successful; otherwise <c>false</c>. The file must be JPG or JPEG. Any exceptions that occur are 
        /// logged and swallowed.
        /// </summary>
        /// <param name="bmpDecoderOriginal">A <see cref="BitmapDecoder" /> referencing the original JPG or JPEG file.</param>
        /// <param name="tmpFilePath">The path for a temporary file. The file will be created and does not need to exist prior to calling this function.</param>
        /// <param name="metaName">Name of the meta property to persist to the file.</param>
        /// <param name="persistAction">The persist action.</param>
        /// <returns><c>true</c> if the meta property was successfully persisted to the file, <c>false</c> otherwise.</returns>
        private bool TryAlternateMethod2OfPersistingMetadata(BitmapDecoder bmpDecoderOriginal, string tmpFilePath, MetadataItemName metaName, MetaPersistAction persistAction)
        {
            // Attempt to write the meta property with this process:
            // 1. Create a copy of the original file and its metadata. Give it a unique name.
            // 2. Open this file, clone its metadata, add some padding, then write the new meta property.
            // 3. Generate a new file at tmpFilePath from the above file.
            if (bmpDecoderOriginal == null || bmpDecoderOriginal.Frames.Count <= 0 || bmpDecoderOriginal.Frames[0] == null)
            {
                return false;
            }

            var tmpFilePath2 = String.Concat(tmpFilePath, "2");

            try
            {
                var encoder = GetEncoder();

                // Create a new file using the original metadata.
                encoder.Frames.Add(BitmapFrame.Create(bmpDecoderOriginal.Frames[0], bmpDecoderOriginal.Frames[0].Thumbnail, (BitmapMetadata)bmpDecoderOriginal.Frames[0].Metadata, bmpDecoderOriginal.Frames[0].ColorContexts));

                using (Stream tmpFileStream = File.Open(tmpFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    encoder.Save(tmpFileStream);
                }

                // Rename the newly created file, write the meta property to a clone of it's metadata, then write to a new file at tmpFilePath.
                File.Move(tmpFilePath, tmpFilePath2);

                using (Stream newOutputFile = new FileStream(tmpFilePath2, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    const BitmapCreateOptions createOptions = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;
                    var bmpDecoder = BitmapDecoder.Create(newOutputFile, createOptions, BitmapCacheOption.None);

                    var encoder2 = GetEncoder();

                    if (bmpDecoder.Frames[0] != null && bmpDecoder.Frames[0].Metadata != null)
                    {
                        var bitmapMetadata = bmpDecoder.Frames[0].Metadata.Clone() as BitmapMetadata;

                        if (bitmapMetadata != null)
                        {
                            bitmapMetadata.SetQuery("/app1/ifd/PaddingSchema:Padding", MetadataPaddingInBytes);
                            bitmapMetadata.SetQuery("/app1/ifd/exif/PaddingSchema:Padding", MetadataPaddingInBytes);
                            bitmapMetadata.SetQuery("/xmp/PaddingSchema:Padding", MetadataPaddingInBytes);

                            SetMetadata(bitmapMetadata, metaName, persistAction);

                            encoder2.Frames.Add(BitmapFrame.Create(bmpDecoder.Frames[0], bmpDecoder.Frames[0].Thumbnail, bitmapMetadata, bmpDecoder.Frames[0].ColorContexts));
                        }
                    }

                    using (Stream outputFile = File.Open(tmpFilePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        encoder2.Save(outputFile);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ex.Data.Contains("Meta Save Error"))
                {
                    ex.Data.Add("Meta Save Error", String.Format("Unable to persist the meta property '{0}' to the original file for media object ID {1}. It was, however, persisted to the database and is still available for viewing in the gallery.", metaName, GalleryObject.Id));
                }

                EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());

                return false;
            }
            finally
            {
                try
                {
                    File.Delete(tmpFilePath2);
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Return <c>true</c> if the original image has a JPG or JPEG file extension; otherwise <c>false</c>.
        /// </summary>
        /// <returns><c>true</c> if  the original image has a JPG or JPEG file extension; otherwise <c>false</c>.</returns>
        private bool IsOriginalJpegImage()
        {
            // Return true if the original image is not a JPEG.
            var jpegImageTypes = new[] { ".jpg", ".jpeg" };
            var originalFileExtension = Path.GetExtension(GalleryObject.Original.FileName).ToLowerInvariant();

            return Array.IndexOf(jpegImageTypes, originalFileExtension) >= 0;
        }

        #endregion
    }
}