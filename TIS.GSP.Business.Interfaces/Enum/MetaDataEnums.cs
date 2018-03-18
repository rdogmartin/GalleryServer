using System;
using System.ComponentModel;

namespace GalleryServer.Business.Metadata
{
  /// <summary>
  /// Contains functionality to support the various enumerations related to metadata.
  /// </summary>
  public static class MetadataEnumHelper
  {
    /// <summary>
    /// Determines if the specified <see cref="FlashMode" /> is one of the defined enumerations. This method is more efficient than using
    /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
    /// </summary>
    /// <param name="flashMode">An instance of <see cref="FlashMode" /> to test.</param>
    /// <returns>Returns true if <paramref name="flashMode"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
    public static bool IsValidFlashMode(FlashMode flashMode)
    {
      switch (flashMode)
      {
        case FlashMode.FlashDidNotFire:
        case FlashMode.FlashFired:
        case FlashMode.StrobeReturnLightNotDetected:
        case FlashMode.StrobeReturnLightDetected:
        case FlashMode.FlashFiredCompulsoryFlashMode:
        case FlashMode.FlashFiredCompulsoryFlashModeReturnLightNotDetected:
        case FlashMode.FlashFiredCompulsoryFlashModeReturnLightDetected:
        case FlashMode.FlashDidNotFireCompulsoryFlashMode:
        case FlashMode.FlashDidNotFireAutoMode:
        case FlashMode.FlashFiredAutoMode:
        case FlashMode.FlashFiredAutoModeReturnLightNotDetected:
        case FlashMode.FlashFiredAutoModeReturnLightDetected:
        case FlashMode.NoFlashFunction:
        case FlashMode.FlashFiredRedEyeReductionMode:
        case FlashMode.FlashFiredRedEyeReductionModeReturnLightNotDetected:
        case FlashMode.FlashFiredRedEyeReductionModeReturnLightDetected:
        case FlashMode.FlashFiredCompulsoryFlashModeRedEyeReductionMode:
        case FlashMode.FlashFiredCompulsoryFlashModeRedEyeReductionModeReturnLightNotDetected:
        case FlashMode.FlashFiredCompulsoryFlashModeRedEyeReductionModeReturnLightDetected:
        case FlashMode.FlashFiredAutoModeRedEyeReductionMode:
        case FlashMode.FlashFiredAutoModeReturnLightNotDetectedRedEyeReductionMode:
        case FlashMode.FlashFiredAutoModeReturnLightDetectedRedEyeReductionMode:
          break;

        default:
          return false;
      }
      return true;
    }

    /// <summary>
    /// Determines if the specified <see cref="MeteringMode" /> is one of the defined enumerations. This method is more efficient than using
    /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
    /// </summary>
    /// <param name="meteringMode">An instance of <see cref="MeteringMode" /> to test.</param>
    /// <returns>Returns true if <paramref name="meteringMode"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
    public static bool IsValidMeteringMode(MeteringMode meteringMode)
    {
      switch (meteringMode)
      {
        case MeteringMode.Average:
        case MeteringMode.CenterWeightedAverage:
        case MeteringMode.MultiSpot:
        case MeteringMode.Other:
        case MeteringMode.Partial:
        case MeteringMode.Pattern:
        case MeteringMode.Spot:
        case MeteringMode.Unknown:
          break;

        default:
          return false;
      }
      return true;
    }

    /// <summary>
    /// Determines if the specified <see cref="LightSource" /> is one of the defined enumerations. This method is more efficient than using
    /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
    /// </summary>
    /// <param name="lightSource">An instance of <see cref="LightSource" /> to test.</param>
    /// <returns>Returns true if <paramref name="lightSource"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
    public static bool IsValidLightSource(LightSource lightSource)
    {
      switch (lightSource)
      {
        case LightSource.D55:
        case LightSource.D65:
        case LightSource.D75:
        case LightSource.Daylight:
        case LightSource.Flash:
        case LightSource.Fluorescent:
        case LightSource.Other:
        case LightSource.StandardLightA:
        case LightSource.StandardLightB:
        case LightSource.StandardLightC:
        case LightSource.Tungsten:
        case LightSource.Unknown:
          break;

        default:
          return false;
      }
      return true;
    }

    /// <summary>
    /// Determines if the specified <see cref="ResolutionUnit" /> is one of the defined enumerations. This method is more efficient than using
    /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
    /// </summary>
    /// <param name="resUnit">An instance of <see cref="ResolutionUnit" /> to test.</param>
    /// <returns>Returns true if <paramref name="resUnit"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
    public static bool IsValidResolutionUnit(ResolutionUnit resUnit)
    {
      switch (resUnit)
      {
        case ResolutionUnit.dpcm:
        case ResolutionUnit.dpi:
          break;

        default:
          return false;
      }
      return true;
    }

    /// <summary>
    /// Determines if the specified <see cref="ExposureProgram" /> is one of the defined enumerations. This method is more efficient than using
    /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
    /// </summary>
    /// <param name="expProgram">An instance of <see cref="ExposureProgram" /> to test.</param>
    /// <returns>Returns true if <paramref name="expProgram"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
    public static bool IsValidExposureProgram(ExposureProgram expProgram)
    {
      switch (expProgram)
      {
        case ExposureProgram.Action:
        case ExposureProgram.Aperture:
        case ExposureProgram.Creative:
        case ExposureProgram.Landscape:
        case ExposureProgram.Manual:
        case ExposureProgram.Normal:
        case ExposureProgram.Portrait:
        case ExposureProgram.Reserved:
        case ExposureProgram.Shutter:
        case ExposureProgram.Undefined:
          break;

        default:
          return false;
      }
      return true;
    }

    /// <summary>
    /// Determines if the specified <see cref="Orientation" /> is one of the defined enumerations. This method is more efficient than using
    /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
    /// </summary>
    /// <param name="orientation">An instance of <see cref="Orientation" /> to test.</param>
    /// <returns>Returns true if <paramref name="orientation"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
    public static bool IsValidOrientation(Orientation orientation)
    {
      switch (orientation)
      {
        case Orientation.NotInitialized:
        case Orientation.None:
        case Orientation.Normal:
        case Orientation.Mirrored:
        case Orientation.Rotated180:
        case Orientation.Flipped:
        case Orientation.FlippedAndRotated90:
        case Orientation.Rotated270:
        case Orientation.FlippedAndRotated270:
        case Orientation.Rotated90:
          break;

        default:
          return false;
      }
      return true;
    }
  }

  /// <summary>
  /// Specifies the unit of measure used for the horizontal resolution and the vertical resolution.
  /// </summary>
  public enum ResolutionUnit : ushort
  {
    ///<summary>Dots Per Inch</summary>
    dpi = 2,
    ///<summary>Centimeters Per Inch</summary>
    dpcm = 3
  }

  /// <summary>
  /// Specifies the image orientation viewed in terms of rows and columns.
  /// </summary>
  /// <remarks>
  /// For an explanation of these values, see http://www.impulseadventure.com/photo/exif-orientation.html
  /// </remarks>
  public enum Orientation : ushort
  {
    ///<summary>Indicates that no orientation has yet been assigned.</summary>
    NotInitialized = 0,
    ///<summary>Indicates that no orientation value exists for an object.</summary>
    [Description("None")]
    None = 65535,
    ///<summary>Indicates the item is right side up in a normal orientation.</summary>
    [Description("Normal")]
    Normal = 1,
    ///<summary>Indicates the item is mirrored horizontally.</summary>
    [Description("Mirrored")]
    Mirrored = 2,
    ///<summary>Indicates the item is upside down.</summary>
    [Description("Rotated 180°")]
    Rotated180 = 3,
    ///<summary>Indicates the item is mirrored vertically.</summary>
    [Description("Flipped")]
    Flipped = 4,
    ///<summary>Indicates the item is mirrored vertically and rotated 90 degrees clockwise.</summary>
    [Description("Flipped and rotated 90° CW")]
    FlippedAndRotated90 = 5,
    ///<summary>Indicates the item is rotated 90 degrees counter clockwise.</summary>
    [Description("Rotated 90° CCW")]
    Rotated270 = 6,
    ///<summary>Indicates the item is mirrored vertically and rotated 90 degrees counter clockwise .</summary>
    [Description("Flipped and rotated 90° CCW")]
    FlippedAndRotated270 = 7,
    ///<summary>Indicates the item is rotated 90 degrees clockwise.</summary>
    [Description("Rotated 90° CW")]
    Rotated90 = 8
  }

  /// <summary>
  /// Specifies the class of the program used by the camera to set exposure when the picture is taken.
  /// </summary>
  public enum ExposureProgram : ushort
  {
    ///<summary>not defined</summary>
    Undefined = 0,
    ///<summary>manual</summary>
    Manual = 1,
    ///<summary>normal program</summary>
    Normal = 2,
    ///<summary>aperture priority</summary>
    Aperture = 3,
    ///<summary>shutter priority</summary>
    Shutter = 4,
    ///<summary>creative program (biased toward depth of field)</summary>
    Creative = 5,
    ///<summary>action program (biased toward fast shutter speed)</summary>
    Action = 6,
    ///<summary>portrait mode (for close-up photos with the background out of focus)</summary>
    Portrait = 7,
    ///<summary>landscape mode (for landscape photos with the background in focus)</summary>
    Landscape = 8,
    ///<summary>9 to 255 - reserved</summary>
    Reserved = 9
  }

  /// <summary>
  /// Specifies the metering mode.
  /// </summary>
  public enum MeteringMode : ushort
  {
    ///<summary>Unknown</summary>
    Unknown = 0,
    ///<summary>Average</summary>
    Average = 1,
    ///<summary>Center weighted average</summary>
    CenterWeightedAverage = 2,
    ///<summary>Spot</summary>
    Spot = 3,
    ///<summary>Multi Spot</summary>
    MultiSpot = 4,
    ///<summary>Pattern</summary>
    Pattern = 5,
    ///<summary>Partial</summary>
    Partial = 6,
    ///<summary>Other</summary>
    Other = 255
  }

  /// <summary>
  /// Specifies the flash mode.
  /// </summary>
  public enum FlashMode : ushort
  {
    /// <summary>Specifies that the flash did not fire.</summary>
    [Description("Flash did not fire")]
    FlashDidNotFire = 0x0000,
    /// <summary>Specifies that the flash fired.</summary>
    [Description("Flash fired")]
    FlashFired = 0x0001,
    /// <summary>Specifies that strobe return light not detected.</summary>
    [Description("Strobe return light not detected")]
    StrobeReturnLightNotDetected = 0x0005,
    /// <summary>Specifies that strobe return light was detected.</summary>
    [Description("Strobe return light detected")]
    StrobeReturnLightDetected = 0x0007,
    /// <summary>Specifies that flash fired, compulsory flash mode.</summary>
    [Description("Flash fired, compulsory flash mode")]
    FlashFiredCompulsoryFlashMode = 0x0009,
    /// <summary>Specifies that flash fired, compulsory flash mode, return light not detected.</summary>
    [Description("Flash fired, compulsory flash mode, return light not detected")]
    FlashFiredCompulsoryFlashModeReturnLightNotDetected = 0x000D,
    /// <summary>Specifies that flash fired, compulsory flash mode, return light detected.</summary>
    [Description("Flash fired, compulsory flash mode, return light detected")]
    FlashFiredCompulsoryFlashModeReturnLightDetected = 0x000F,
    /// <summary>Specifies that flash did not fire, compulsory flash mode.</summary>
    [Description("Flash did not fire, compulsory flash mode")]
    FlashDidNotFireCompulsoryFlashMode = 0x0010,
    /// <summary>Specifies that flash did not fire, auto mode.</summary>
    [Description("Flash did not fire, auto mode")]
    FlashDidNotFireAutoMode = 0x0018,
    /// <summary>Specifies that flash fired, auto mode.</summary>
    [Description("Flash fired, auto mode")]
    FlashFiredAutoMode = 0x0019,
    /// <summary>Specifies that flash fired, auto mode, return light not detected.</summary>
    [Description("Flash fired, auto mode, return light not detected")]
    FlashFiredAutoModeReturnLightNotDetected = 0x001D,
    /// <summary>Specifies that flash fired, auto mode, return light detected.</summary>
    [Description("Flash fired, auto mode, return light detected")]
    FlashFiredAutoModeReturnLightDetected = 0x001F,
    /// <summary>Specifies that no flash function was detected.</summary>
    [Description("No flash function")]
    NoFlashFunction = 0x0020,
    /// <summary>Specifies that flash fired, red-eye reduction mode.</summary>
    [Description("Flash fired, red-eye reduction mode")]
    FlashFiredRedEyeReductionMode = 0x0041,
    /// <summary>Specifies that flash fired, red-eye reduction mode, return light not detected.</summary>
    [Description("Flash fired, red-eye reduction mode, return light not detected")]
    FlashFiredRedEyeReductionModeReturnLightNotDetected = 0x0045,
    /// <summary>Specifies that flash fired, red-eye reduction mode, return light detected.</summary>
    [Description("Flash fired, red-eye reduction mode, return light detected")]
    FlashFiredRedEyeReductionModeReturnLightDetected = 0x0047,
    /// <summary>Specifies that flash fired, compulsory flash mode, red-eye reduction mode.</summary>
    [Description("Flash fired, compulsory flash mode, red-eye reduction mode")]
    FlashFiredCompulsoryFlashModeRedEyeReductionMode = 0x0049,
    /// <summary>Specifies that flash fired, compulsory flash mode, red-eye reduction mode, return light not detected.</summary>
    [Description("Flash fired, compulsory flash mode, red-eye reduction mode, return light not detected")]
    FlashFiredCompulsoryFlashModeRedEyeReductionModeReturnLightNotDetected = 0x004D,
    /// <summary>Specifies that flash fired, compulsory flash mode, red-eye reduction mode, return light detected.</summary>
    [Description("Flash fired, compulsory flash mode, red-eye reduction mode, return light detected")]
    FlashFiredCompulsoryFlashModeRedEyeReductionModeReturnLightDetected = 0x004F,
    /// <summary>Specifies that flash fired, auto mode, red-eye reduction mode.</summary>
    [Description("Flash fired, auto mode, red-eye reduction mode")]
    FlashFiredAutoModeRedEyeReductionMode = 0x0059,
    /// <summary>Specifies that flash fired, auto mode, return light not detected, red-eye reduction mode.</summary>
    [Description("Flash fired, auto mode, return light not detected, red-eye reduction mode")]
    FlashFiredAutoModeReturnLightNotDetectedRedEyeReductionMode = 0x005D,
    /// <summary>Specifies that flash fired, auto mode, return light detected, red-eye reduction mode.</summary>
    [Description("Flash fired, auto mode, return light detected, red-eye reduction mode")]
    FlashFiredAutoModeReturnLightDetectedRedEyeReductionMode = 0x005F,
  }

  /// <summary>
  /// Specifies possible light sources (white balance).
  /// </summary>
  public enum LightSource : ushort
  {
    /// <summary>Unknown light source.</summary>
    Unknown = 0,
    /// <summary>Daylight light source.</summary>
    Daylight = 1,
    /// <summary>Fluorescent light source.</summary>
    Fluorescent = 2,
    /// <summary>Tungsten light source.</summary>
    Tungsten = 3,
    /// <summary>Flash light source.</summary>
    Flash = 4,
    /// <summary>Fine Weather light source.</summary>
    [Description("Fine Weather")]
    FineWeather = 9,
    /// <summary>Cloudy Weather light source.</summary>
    [Description("Cloudy Weather")]
    CloudyWeather = 10,
    /// <summary>Shade light source.</summary>
    Shade = 11,
    /// <summary>Daylight Fluorescent light source.</summary>
    [Description("Daylight Fluorescent")]
    DaylightWluorescent = 12,
    /// <summary>Day White Fluorescent light source.</summary>
    [Description("Day White Fluorescent")]
    DayWhiteFluorescent = 13,
    /// <summary>Cool White Fluorescent light source.</summary>
    [Description("Cool White Fluorescent")]
    CoolWhiteFluorescent = 14,
    /// <summary>White Fluorescent" light source.</summary>
    [Description("White Fluorescent")]
    WhiteFluorescent = 15,
    /// <summary>Standard Light A light source.</summary>
    [Description("Standard Light A")]
    StandardLightA = 17,
    /// <summary>Standard Light B light source.</summary>
    [Description("Standard Light B")]
    StandardLightB = 18,
    /// <summary>Standard Light C light source.</summary>
    [Description("Standard Light C")]
    StandardLightC = 19,
    /// <summary>D55 light source.</summary>
    D55 = 20,
    /// <summary>D65 light source.</summary>
    D65 = 21,
    /// <summary>D75 light source.</summary>
    D75 = 22,
    /// <summary>D50 light source.</summary>
    D50 = 23,
    /// <summary>ISO Studio Tungsten light source.</summary>
    [Description("ISO Studio Tungsten")]
    ISOStudioTungsten = 24,
    /// <summary>Other light source.</summary>
    Other = 255,
  }

  ///<summary>
  /// Specifies the data type of the values stored in the value data member of that same <see cref="System.Drawing.Imaging.PropertyItem" /> object.
  ///</summary>
  public enum PropertyTagType : short
  {
    ///<summary>Specifies that the format is 4 bits per pixel, indexed.</summary>
    PixelFormat4bppIndexed = 0,
    ///<summary>Specifies that the value data member is an array of bytes.</summary>
    Byte = 1,
    ///<summary>Specifies that the value data member is a null-terminated ASCII string. If you set the type data member of a <see cref="System.Drawing.Imaging.PropertyItem" /> object to PropertyTagType.ASCII, you should set the length data member to the length of the string including the NULL terminator. For example, the string HELLO would have a length of 6.</summary>
    ASCII = 2,
    ///<summary>Specifies that the value data member is an array of unsigned short (16-bit) integers.</summary>
    UnsignedShort = 3,
    ///<summary>Specifies that the value data member is an array of unsigned long (32-bit) integers.</summary>
    UnsignedInt = 4,
    ///<summary>Specifies that the value data member is an array of pairs of unsigned long (32-bit) integers. Each pair represents a fraction; the first integer is the numerator and the second integer is the denominator.</summary>
    UnsignedFraction = 5,
    ///<summary>Specifies that the value data member is an array of bytes that can hold values of any data type.</summary>
    Undefined = 6,
    ///<summary>Specifies that the value data member is an array of signed long (32-bit) integers.</summary>
    Int = 7,
    ///<summary>Specifies that the value data member is an array of pairs of signed long (32-bit) integers. Each pair represents a fraction; the first integer is the numerator and the second integer is the denominator.</summary>
    Fraction = 10
  }

  /// <summary>
  /// Specifies the data type of the meta data value after it has been extracted from the <see cref="System.Drawing.Imaging.PropertyItem" /> object and formatted
  /// into a user-friendly value.
  /// </summary>
  public enum ExtractedValueType
  {
    ///<summary>Specifies that the value is not defined.</summary>
    NotDefined = 0,
    ///<summary>Specifies that the value is a System.Byte[].</summary>
    ByteArray,
    ///<summary>Specifies that the value is a System.Int64.</summary>
    Int64,
    ///<summary>Specifies that the value is a System.Int64[].</summary>
    Int64Array,
    ///<summary>Specifies that the value is a System.String.</summary>
    String,
    ///<summary>Specifies that the value is an instance of GalleryServer.Business.Fraction.</summary>
    Fraction,
    ///<summary>Specifies that the value is an instance of GalleryServer.Business.Fraction[].</summary>
    FractionArray
  }

  /// <summary>
  /// Specifies a particular property in the Windows Property System.
  /// See https://msdn.microsoft.com/en-us/library/windows/desktop/dd561977(v=vs.85).aspx
  /// </summary>
  public enum SystemMetaProperty
  {
    ///<summary>Specifies System.Photo.ExposureProgram</summary>
    System_Photo_ExposureProgram,
    ///<summary>Specifies System.Photo.ExposureTimeNumerator</summary>
    System_Photo_ExposureTimeNumerator,
    ///<summary>Specifies System.Photo.ExposureTimeDenominator</summary>
    System_Photo_ExposureTimeDenominator,
    ///<summary>Specifies System.Photo.ExposureBias</summary>
    System_Photo_ExposureBias,
    ///<summary>Specifies System.Photo.Orientation</summary>
    System_Photo_Orientation,
    ///<summary>Specifies System.Photo.Flash</summary>
    System_Photo_Flash,
    ///<summary>Specifies System.Photo.FNumber</summary>
    System_Photo_FNumber,
    ///<summary>Specifies System.Photo.FocalLength</summary>
    System_Photo_FocalLength,
    ///<summary>Specifies System.Photo.ISOSpeed</summary>
    System_Photo_ISOSpeed,
    ///<summary>Specifies System.Photo.Aperture</summary>
    System_Photo_Aperture,
    ///<summary>Specifies System.Photo.LightSource</summary>
    System_Photo_LightSource,
    ///<summary>Specifies System.Photo.MeteringMode</summary>
    System_Photo_MeteringMode,
    ///<summary>Specifies System.Photo.PeopleNames</summary>
    System_Photo_PeopleNames,
  }

  /// <summary>
  /// Specifies the name of the formatted metadata item associated with a media object. The data for each item may
  /// originate from one of these sources: (1) An EXIF, IPTC, or XMP value as determined by
  /// System.Windows.Media.Imaging.BitmapMetadata, (2) A GDI+ image property tag (The GDI+ property tags are defined
  /// in the <see cref="RawMetadataItemName" /> enumeration.), (3) A file property (these are grouped at the end of
  /// this enumeration). This enumeration differs from <see cref="RawMetadataItemName" /> in that these items are
  /// formatted to a user-friendly value (e.g. "1/500 sec") and may include more than just GDI+ property tags (such
  /// as the file properties).
  /// </summary>
  /// <remarks>
  /// Note to developer: If you edit these items, be sure to update
  /// <see cref="MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName" /> to match your changes.
  /// </remarks>
  public enum MetadataItemName
  {
    /// <summary>Specifies that no particular enumeration has been selected.</summary>
    NotSpecified = -2147483648,
    /// <summary>The bit rate of the audio portion of the media object.</summary>
    AudioBitRate = 0,
    /// <summary>Data about the audio portion of the media object.</summary>
    AudioFormat = 1,
    /// <summary>Specifies the name of the person who created the image. Derived from BitmapMetadata.Author.
    /// If that property is empty, it is derived from Artist.</summary>
    Author = 2,
    /// <summary>The overall bitrate of the media object.</summary>
    BitRate = 3,
    /// <summary>Specifies the model name or model number of the equipment used to record the image.
    /// Derived from BitmapMetadata.CameraModel.</summary>
    CameraModel = 4,
    /// <summary>Comment tag. Derived from BitmapMetadata.Comment. If that property is empty, it is derived from
    /// ExifUserComment.</summary>
    Comment = 5,
    /// <summary>Color space specifier. Derived from ExifColorSpace.</summary>
    ColorRepresentation = 6,
    /// <summary>Specifies the copyright information. Derived from BitmapMetadata.Copyright.</summary>
    Copyright = 7,
    /// <summary>Date and time when the original image data was generated. Derived from BitmapMetadata.DateTaken.</summary>
    DatePictureTaken = 8,
    /// <summary>Specifies the title of the image. Derived from ImageDescription.</summary>
    Description = 9,
    /// <summary>Specifies the width and height of the image in pixels. Derived from ImageWidth and
    /// ImageHeight for uncompressed images such as TIFF, and from ExifPixXDim and ExifPixYDim for compressed
    /// images such as JPEG.</summary>
    Dimensions = 10,
    /// <summary>Specifies the duration of the media object. Applies only to media objects that span time, such as audio
    /// or video.</summary>
    Duration = 11,
    /// <summary>Specifies the manufacturer of the equipment used to record the image.
    /// Derived from EquipMake.</summary>
    EquipmentManufacturer = 12,
    /// <summary>Exposure bias. The unit is the APEX value. Ordinarily it is given in the range -99.99 
    /// to 99.99. Derived from ExifExposureBias.</summary>
    ExposureCompensation = 13,
    /// <summary>Class of the program used by the camera to set exposure when the picture is taken.
    /// Derived from ExifExposureProg.</summary>
    ExposureProgram = 14,
    /// <summary>Exposure time, measured in seconds. Derived from ExifExposureTime.</summary>
    ExposureTime = 15,
    /// <summary>Flash status. This tag is recorded when an image is taken using a strobe light (flash).
    /// Derived from ExifFlash.</summary>
    FlashMode = 16,
    /// <summary>F number. Derived from ExifFNumber.</summary>
    FNumber = 17,
    /// <summary>Actual focal length, in millimeters, of the lens. Conversion is not made to the focal length
    ///of a 35 millimeter film camera. Derived from ExifFocalLength.</summary>
    FocalLength = 18,
    /// <summary>Number of pixels rows. For uncompressed images such as TIFF, this value is derived from 
    /// ImageHeight. For compressed images such as JPEG, this value is derived from ExifPixYDim.  For videos, this
    /// is parsed from the output of the FFmpeg utility.</summary>
    Height = 19,
    /// <summary>Number of pixels per unit in the image width (x) direction. The value is derived from 
    /// XResolution and the unit of measure (inch, centimeter, etc.) is derived from ResolutionUnit.</summary>
    HorizontalResolution = 20,
    /// <summary>ISO speed and ISO latitude of the camera or input device as specified in ISO 12232.
    /// Derived from ExifISOSpeed.</summary>
    IsoSpeed = 21,
    /// <summary>A comma-separated list of tag names associated with the object. Derived from BitmapMetadata.Keywords.</summary>
    Tags = 22,
    /// <summary>Lens aperture. The unit is the APEX value. The value is the same as the F-Number if present;
    /// otherwise it is calculated from ExifAperture using this formula: Math.Pow(Sqrt(2), ExifAperture).</summary>
    LensAperture = 23,
    /// <summary>Type of light source. Derived from ExifLightSource.</summary>
    LightSource = 24,
    /// <summary>Metering mode. Derived from ExifMeteringMode.</summary>
    MeteringMode = 25,
    /// <summary>Rating. The value is an integer from 0-5. Derived from BitmapMetadata.Rating.</summary>
    Rating = 26,
    /// <summary>Distance to the subject, measured in meters. Derived from ExifSubjectDist.</summary>
    SubjectDistance = 27,
    /// <summary>The subject pertaining to the image. Derived from BitmapMetadata.Subject.</summary>
    Subject = 28,
    /// <summary>Specifies the title of a media object. For images, it derives from BitmapMetadata.Title.</summary>
    Title = 29,
    /// <summary>Number of pixels per unit in the image height (y) direction. The value is derived from 
    /// YResolution and the unit of measure (inch, centimeter, etc.) is derived from ResolutionUnit.</summary>
    VerticalResolution = 30,
    /// <summary>The bit rate of the video portion of the media object.</summary>
    VideoBitRate = 31,
    /// <summary>Data about the video portion of the media object.</summary>
    VideoFormat = 32,
    /// <summary>Number of pixels per row. For uncompressed images such as TIFF, this value is derived from 
    /// ImageWidth. For compressed images such as JPEG, this value is derived from ExifPixXDim. For videos, this
    /// is parsed from the output of the FFmpeg utility.</summary>
    Width = 33,
    /// <summary>
    /// The name of the file associated with the media object.
    /// </summary>
    FileName = 34,
    /// <summary>
    /// The name of the file associated with the media object, excluding the file extension.
    /// </summary>
    FileNameWithoutExtension = 35,
    /// <summary>
    /// The size, in KB, of the file associated with the media object.
    /// </summary>
    FileSizeKb = 36,
    /// <summary>
    /// The file creation timestamp of the file associated with the media object, in local time.
    /// </summary>
    DateFileCreated = 37,
    /// <summary>
    /// The file creation timestamp of the file associated with the media object, in UTC time.
    /// </summary>
    DateFileCreatedUtc = 38,
    /// <summary>
    /// The date last modified timestamp of the file associated with the media object, in local time.
    /// </summary>
    DateFileLastModified = 39,
    /// <summary>
    /// The date last modified timestamp of the file associated with the media object, in UTC time.
    /// </summary>
    DateFileLastModifiedUtc = 40,
    /// <summary>
    /// The caption for the object.
    /// </summary>
    Caption = 41,
    /// <summary>
    /// A comma-separated list of person names associated with the object.
    /// </summary>
    People = 42,
    /// <summary>
    /// The orientation of an image or video.
    /// </summary>
    Orientation = 43,
    /// <summary>
    /// The latitude and longitude of the location of the media object. Example: "27.1234 N 15.5678 W"
    /// </summary>
    GpsLocation = 101,
    /// <summary>
    /// The latitude and longitude of the location of the media object, enclosed in a hyperlink to a mapping service.
    /// Example: "27.1234 N 15.5678 W"
    /// </summary>
    GpsLocationWithMapLink = 102,
    /// <summary>
    /// The latitude of the location of the media object. Value will be negative for latitudes south of the equator.
    /// Example: "27.1234"
    /// </summary>
    GpsLatitude = 103,
    /// <summary>
    /// The longitude of the location of the media object. Value will be negative for longitudes west of the Prime Meridian. 
    /// Example: "27.1234"
    /// </summary>
    GpsLongitude = 104,
    /// <summary>
    /// The latitude and longitude of the destination location of the media object. Example: "27.1234 N 15.5678 W"
    /// </summary>
    GpsDestLocation = 105,
    /// <summary>
    /// The latitude and longitude of the destination location of the media object, enclosed in a hyperlink to a mapping service.
    /// Example: "27.1234 N 15.5678 W"
    /// </summary>
    GpsDestLocationWithMapLink = 106,
    /// <summary>
    /// The latitude of the destination location of the media object. Value will be negative for latitudes south of the equator. 
    /// Example: "27.1234"
    /// </summary>
    GpsDestLatitude = 107,
    /// <summary>
    /// The longitude of the destination location of the media object. Value will be negative for longitudes west of the Prime Meridian. 
    /// Example: "27.1234"
    /// </summary>
    GpsDestLongitude = 108,
    /// <summary>
    /// The altitude, in meters, of the media object.
    /// </summary>
    GpsAltitude = 109,
    /// <summary>
    /// The version of the GPS information. Example: "2.2.0.0"
    /// </summary>
    GpsVersion = 110,
    /// <summary>
    /// The timestamp for when the item was added to the gallery.
    /// </summary>
    DateAdded = 111,
    /// <summary>
    /// The HTML fragment / embed code that defines the content of an external media object.
    /// Note that the application is hard-coded to add an item of this type ONLY when the media
    /// object has the type ExternalMediaobject. 
    /// </summary>
    HtmlSource = 112,
    /// <summary>
    /// The number of ratings that have been applied to an album or media object.
    /// </summary>
    RatingCount = 113,
    /// <summary>
    /// The IPTC by-line.
    /// </summary>
    IptcByline = 1001,
    /// <summary>
    /// The IPTC by-line title.
    /// </summary>
    IptcBylineTitle = 1002,
    /// <summary>
    /// The IPTC caption.
    /// </summary>
    IptcCaption = 1003,
    /// <summary>
    /// The IPTC city.
    /// </summary>
    IptcCity = 1004,
    /// <summary>
    /// The IPTC copyright notice.
    /// </summary>
    IptcCopyrightNotice = 1005,
    /// <summary>
    /// The IPTC country name.
    /// </summary>
    IptcCountryPrimaryLocationName = 1006,
    /// <summary>
    /// The IPTC credit.
    /// </summary>
    IptcCredit = 1007,
    /// <summary>
    /// The IPTC date created value.
    /// </summary>
    IptcDateCreated = 1008,
    /// <summary>
    /// The IPTC headline.
    /// </summary>
    IptcHeadline = 1009,
    /// <summary>
    /// The IPTC keywords.
    /// </summary>
    IptcKeywords = 1010,
    /// <summary>
    /// The IPTC object name.
    /// </summary>
    IptcObjectName = 1011,
    /// <summary>
    /// The IPTC original transmission reference.
    /// </summary>
    IptcOriginalTransmissionReference = 1012,
    /// <summary>
    /// The IPTC province/state.
    /// </summary>
    IptcProvinceState = 1013,
    /// <summary>
    /// The IPTC record version.
    /// </summary>
    IptcRecordVersion = 1014,
    /// <summary>
    /// The IPTC source.
    /// </summary>
    IptcSource = 1015,
    /// <summary>
    /// The IPTC special instructions.
    /// </summary>
    IptcSpecialInstructions = 1016,
    /// <summary>
    /// The IPTC sub-location.
    /// </summary>
    IptcSublocation = 1017,
    /// <summary>
    /// The IPTC writer/editor.
    /// </summary>
    IptcWriterEditor = 1018,
    /// <summary>A custom metadata item.</summary>
    Custom1 = 2000,
    /// <summary>A custom metadata item.</summary>
    Custom2 = 2001,
    /// <summary>A custom metadata item.</summary>
    Custom3 = 2002,
    /// <summary>A custom metadata item.</summary>
    Custom4 = 2003,
    /// <summary>A custom metadata item.</summary>
    Custom5 = 2004,
    /// <summary>A custom metadata item.</summary>
    Custom6 = 2005,
    /// <summary>A custom metadata item.</summary>
    Custom7 = 2006,
    /// <summary>A custom metadata item.</summary>
    Custom8 = 2007,
    /// <summary>A custom metadata item.</summary>
    Custom9 = 2008,
    /// <summary>A custom metadata item.</summary>
    Custom10 = 2009,
    /// <summary>A custom metadata item.</summary>
    Custom11 = 2010,
    /// <summary>A custom metadata item.</summary>
    Custom12 = 2011,
    /// <summary>A custom metadata item.</summary>
    Custom13 = 2012,
    /// <summary>A custom metadata item.</summary>
    Custom14 = 2013,
    /// <summary>A custom metadata item.</summary>
    Custom15 = 2014,
    /// <summary>A custom metadata item.</summary>
    Custom16 = 2015,
    /// <summary>A custom metadata item.</summary>
    Custom17 = 2016,
    /// <summary>A custom metadata item.</summary>
    Custom18 = 2017,
    /// <summary>A custom metadata item.</summary>
    Custom19 = 2018,
    /// <summary>A custom metadata item.</summary>
    Custom20 = 2019
  }

  /// <summary>
  /// Contains functionality to support the <see cref="MetadataItemName" /> enumeration.
  /// </summary>
  public static class MetadataItemNameEnumHelper
  {
    /// <summary>
    /// Determines if the <paramref name="item" /> parameter is one of the defined enumerations. This method is more efficient than using
    /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
    /// </summary>
    /// <param name="item">An instance of <see cref="MetadataItemName" /> to test.</param>
    /// <returns>Returns true if <paramref name="item" /> is one of the defined items in the enumeration; otherwise returns false.</returns>
    public static bool IsValidFormattedMetadataItemName(MetadataItemName item)
    {
      switch (item)
      {
        // Most frequently specified items are at the top
        case MetadataItemName.Title:

        case MetadataItemName.AudioBitRate:
        case MetadataItemName.AudioFormat:
        case MetadataItemName.Author:
        case MetadataItemName.BitRate:
        case MetadataItemName.CameraModel:
        case MetadataItemName.Comment:
        case MetadataItemName.ColorRepresentation:
        case MetadataItemName.Copyright:
        case MetadataItemName.DatePictureTaken:
        case MetadataItemName.Description:
        case MetadataItemName.Dimensions:
        case MetadataItemName.Duration:
        case MetadataItemName.EquipmentManufacturer:
        case MetadataItemName.ExposureCompensation:
        case MetadataItemName.ExposureProgram:
        case MetadataItemName.ExposureTime:
        case MetadataItemName.FlashMode:
        case MetadataItemName.FNumber:
        case MetadataItemName.FocalLength:
        case MetadataItemName.Height:
        case MetadataItemName.HorizontalResolution:
        case MetadataItemName.IsoSpeed:
        case MetadataItemName.Tags:
        case MetadataItemName.LensAperture:
        case MetadataItemName.LightSource:
        case MetadataItemName.MeteringMode:
        case MetadataItemName.Rating:
        case MetadataItemName.SubjectDistance:
        case MetadataItemName.Subject:
        case MetadataItemName.VerticalResolution:
        case MetadataItemName.VideoBitRate:
        case MetadataItemName.VideoFormat:
        case MetadataItemName.Width:
        case MetadataItemName.FileName:
        case MetadataItemName.FileNameWithoutExtension:
        case MetadataItemName.FileSizeKb:
        case MetadataItemName.DateFileCreated:
        case MetadataItemName.DateFileCreatedUtc:
        case MetadataItemName.DateFileLastModified:
        case MetadataItemName.DateFileLastModifiedUtc:
        case MetadataItemName.Caption:
        case MetadataItemName.People:
        case MetadataItemName.Orientation:

        case MetadataItemName.GpsLocation:
        case MetadataItemName.GpsLocationWithMapLink:
        case MetadataItemName.GpsLatitude:
        case MetadataItemName.GpsLongitude:
        case MetadataItemName.GpsDestLocation:
        case MetadataItemName.GpsDestLocationWithMapLink:
        case MetadataItemName.GpsDestLatitude:
        case MetadataItemName.GpsDestLongitude:
        case MetadataItemName.GpsAltitude:
        case MetadataItemName.GpsVersion:

        case MetadataItemName.DateAdded:
        case MetadataItemName.HtmlSource:
        case MetadataItemName.RatingCount:
        //case MetadataItemName.AlbumTitle:
        //case MetadataItemName.AlbumCaption:

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
        case MetadataItemName.Custom1:
        case MetadataItemName.Custom2:
        case MetadataItemName.Custom3:
        case MetadataItemName.Custom4:
        case MetadataItemName.Custom5:
        case MetadataItemName.Custom6:
        case MetadataItemName.Custom7:
        case MetadataItemName.Custom8:
        case MetadataItemName.Custom9:
        case MetadataItemName.Custom10:
        case MetadataItemName.Custom11:
        case MetadataItemName.Custom12:
        case MetadataItemName.Custom13:
        case MetadataItemName.Custom14:
        case MetadataItemName.Custom15:
        case MetadataItemName.Custom16:
        case MetadataItemName.Custom17:
        case MetadataItemName.Custom18:
        case MetadataItemName.Custom19:
        case MetadataItemName.Custom20:
          break;

        default:
          return false;
      }
      return true;
    }
  }

  /// <summary>
  /// Specifies the Microsoft Windows GDI+ image property tags.
  /// </summary>	
  /// <remarks>The enumeration members and their comments taken from MSDN documentation at
  /// <a href="http://msdn.microsoft.com/en-us/library/ms534417(VS.85).aspx" /></remarks>
  public enum RawMetadataItemName : int
  {
    ///<summary>Null-terminated character string that specifies the name of the person who created the image. (0x013B, 315)</summary>
    Artist = 315,
    ///<summary>Number of bits per color component. See also SamplesPerPixel.</summary>
    BitsPerSample = 258,
    ///<summary>Height of the dithering or halftoning matrix.</summary>
    CellHeight = 265,
    ///<summary>Width of the dithering or halftoning matrix.</summary>
    CellWidth = 264,
    ///<summary>Chrominance table. The luminance table and the chrominance table are used to control JPEG quality. 
    ///A valid luminance or chrominance table has 64 entries of type PropertyTagTypeShort. If an image has 
    ///either a luminance table or a chrominance table, then it must have both tables.</summary>
    ChrominanceTable = 20625,
    ///<summary>Color palette (lookup table) for a palette-indexed image.</summary>
    ColorMap = 320,
    ///<summary>Table of values that specify color transfer functions.</summary>
    ColorTransferFunction = 20506,
    ///<summary>Compression scheme used for the image data.</summary>
    Compression = 259,
    ///<summary>Null-terminated character string that contains copyright information.</summary>
    Copyright = 33432,
    ///<summary>Date and time the image was created.</summary>
    DateTime = 306,
    ///<summary>Null-terminated character string that specifies the name of the document from which the image 
    ///was scanned.</summary>
    DocumentName = 269,
    ///<summary>Color component values that correspond to a 0 percent dot and a 100 percent dot.</summary>
    DotRange = 336,
    ///<summary>Null-terminated character string that specifies the manufacturer of the equipment used to 
    ///record the image.</summary>
    EquipMake = 271,
    ///<summary>Null-terminated character string that specifies the model name or model number of the 
    ///equipment used to record the image.</summary>
    EquipModel = 272,
    ///<summary>Lens aperture. The unit is the APEX value.</summary>
    ExifAperture = 37378,
    ///<summary>Brightness value. The unit is the APEX value. Ordinarily it is given in the range of 
    ///-99.99 to 99.99.</summary>
    ExifBrightness = 37379,
    ///<summary>The color filter array (CFA) geometric pattern of the image sensor when a one-chip color area sensor 
    ///is used. It does not apply to all sensing methods.</summary>
    ExifCfaPattern = 41730,
    ///<summary>Color space specifier. Normally sRGB (=1) is used to define the color space based on the PC monitor 
    ///conditions and environment. If a color space other than sRGB is used, Uncalibrated (=65535) is set. Image 
    ///data recorded as Uncalibrated can be treated as sRGB when it is converted to FlashPix.</summary>
    ExifColorSpace = 40961,
    ///<summary>Information specific to compressed data. The compression mode used for a compressed image is
    ///indicated in unit BPP.</summary>
    ExifCompBPP = 37122,
    ///<summary>Information specific to compressed data. The channels of each component are arranged in order from 
    ///the first component to the fourth. For uncompressed data, the data arrangement is given in the 
    ///PropertyTagPhotometricInterp tag. However, because PropertyTagPhotometricInterp can only express the 
    ///order of Y, Cb, and Cr, this tag is provided for cases when compressed data uses components other than Y, 
    ///Cb, and Cr and to support other sequences.</summary>
    ExifCompConfig = 37121,
    ///<summary>Date and time when the image was stored as digital data. If, for example, an image was captured 
    ///by DSC and at the same time the file was recorded, then DateTimeOriginal and DateTimeDigitized will have 
    ///the same contents. The format is YYYY:MM:DD HH:MM:SS with time shown in 24-hour format and the date and 
    ///time separated by one blank character (0x2000). The character string length is 20 bytes including the 
    ///NULL terminator. When the field is empty, it is treated as unknown.</summary>
    ExifDTDigitized = 36868,
    ///<summary>Null-terminated character string that specifies a fraction of a second for the 
    ///PropertyTagExifDTDigitized tag.</summary>
    ExifDTDigSS = 37522,
    ///<summary>Date and time when the original image data was generated. For a DSC, the date and time when the 
    ///picture was taken. The format is YYYY:MM:DD HH:MM:SS with time shown in 24-hour format and the date and
    ///time separated by one blank character (0x2000). The character string length is 20 bytes including the 
    ///NULL terminator. When the field is empty, it is treated as unknown.</summary>
    ExifDTOrig = 36867,
    ///<summary>Null-terminated character string that specifies a fraction of a second for the 
    ///PropertyTagExifDTOrig tag.</summary>
    ExifDTOrigSS = 37521,
    ///<summary>Null-terminated character string that specifies a fraction of a second for the PropertyTagDateTime tag.</summary>
    ExifDTSubsec = 37520,
    ///<summary>Exposure bias. The unit is the APEX value. Ordinarily it is given in the range -99.99 to 99.99.</summary>
    ExifExposureBias = 37380,
    ///<summary>Exposure index selected on the camera or input device at the time the image was captured.</summary>
    ExifExposureIndex = 41493,
    ///<summary>Class of the program used by the camera to set exposure when the picture is taken. The value
    ///is an integer with these values: 0 - not defined; 1 - manual; 2 - normal program; 3 - aperture priority;
    ///4 - shutter priority; 5 - creative program (biased toward depth of field); 6 - action program (biased
    ///toward fast shutter speed); 7 - portrait mode (for close-up photos with the background out of focus);
    ///8 - landscape mode (for landscape photos with the background in focus); 9 to 255 - reserved</summary>
    ExifExposureProg = 34850,
    ///<summary>Exposure time, measured in seconds.</summary>
    ExifExposureTime = 33434,
    ///<summary>The image source. If a DSC recorded the image, the value of this tag is 3.</summary>
    ExifFileSource = 41728,
    ///<summary>Flash status. This tag is recorded when an image is taken using a strobe light (flash). 
    ///Bit 0 indicates the flash firing status (0b - flash did not fire 1b - flash fired), and bits 1 and 2 
    ///indicate the flash return status (00b - no strobe return detection function 01b - reserved 10b - strobe 
    ///return light not detected 11b - strobe return light detected). Resulting flash tag values: 0x0000 - flash 
    ///did not fire; 0x0001 - flash fired; 0x0005 - strobe return light not detected</summary>
    ExifFlash = 37385,
    ///<summary>Strobe energy, in Beam Candle Power Seconds (BCPS), at the time the image was captured.</summary>
    ExifFlashEnergy = 41483,
    ///<summary>F number.</summary>
    ExifFNumber = 33437,
    ///<summary>Actual focal length, in millimeters, of the lens. Conversion is not made to the focal length
    ///of a 35 millimeter film camera.</summary>
    ExifFocalLength = 37386,
    ///<summary>Unit of measure for PropertyTagExifFocalXRes and PropertyTagExifFocalYRes.</summary>
    ExifFocalResUnit = 41488,
    ///<summary>Number of pixels in the image width (x) direction per unit on the camera focal plane. The unit is 
    ///specified in PropertyTagExifFocalResUnit.</summary>
    ExifFocalXRes = 41486,
    ///<summary>Number of pixels in the image height (y) direction per unit on the camera focal plane. The unit is
    ///specified in PropertyTagExifFocalResUnit.</summary>
    ExifFocalYRes = 41487,
    ///<summary>FlashPix format version supported by an FPXR file. If the FPXR function supports FlashPix format 
    ///version 1.0, this is indicated similarly to PropertyTagExifVer by recording 0100 as a 4-byte ASCII string. 
    ///Because the type is PropertyTagTypeUndefined, there is no NULL terminator.</summary>
    ExifFPXVer = 40960,
    ///<summary>Private tag used by GDI+. Not for public use. GDI+ uses this tag to locate Exif-specific 
    ///information.</summary>
    ExifIFD = 34665,
    ///<summary>Offset to a block of property items that contain interoperability information.</summary>
    ExifInterop = 40965,
    ///<summary>ISO speed and ISO latitude of the camera or input device as specified in ISO 12232.</summary>
    ExifISOSpeed = 34855,
    ///<summary>Type of light source. This is an integer with these values: 0 - unknown; 1 - Daylight;
    ///2 - Flourescent; 3 - Tungsten; 17 - Standard Light A; 18 - Standard Light B; 19 - Standard Light C;
    ///20 - D55; 21 - D65; 22 - D75; 23 to 254 - reserved; 255 - other</summary>
    ExifLightSource = 37384,
    ///<summary>Note tag. A tag used by manufacturers of EXIF writers to record information. The contents are 
    ///up to the manufacturer.</summary>
    ExifMakerNote = 37500,
    ///<summary>Smallest F number of the lens. The unit is the APEX value. Ordinarily it is given in the range 
    ///of 00.00 to 99.99, but it is not limited to this range.</summary>
    ExifMaxAperture = 37381,
    ///<summary>Metering mode. This is an integer with these values: 0 - unknown; 1 - Average; 2 - 
    ///CenterWeightedAverage; 3 - Spot; 4 - MultiSpot; 5 - Pattern; 6 - Partial; 7 to 254 - reserved;
    ///255 - other</summary>
    ExifMeteringMode = 37383,
    ///<summary>Optoelectronic conversion function (OECF) specified in ISO 14524. The OECF is the relationship 
    ///between the camera optical input and the image values.</summary>
    ExifOECF = 34856,
    ///<summary>Information specific to compressed data. When a compressed file is recorded, the valid width of the 
    ///meaningful image must be recorded in this tag, whether or not there is padding data or a restart marker. 
    ///This tag should not exist in an uncompressed file.</summary>
    ExifPixXDim = 40962,
    ///<summary>Information specific to compressed data. When a compressed file is recorded, the valid height of the 
    ///meaningful image must be recorded in this tag whether or not there is padding data or a restart marker. 
    ///This tag should not exist in an uncompressed file. Because data padding is unnecessary in the vertical 
    ///direction, the number of lines recorded in this valid image height tag will be the same as that recorded 
    ///in the SOF.</summary>
    ExifPixYDim = 40963,
    ///<summary>The name of an audio file related to the image data. The only relational information recorded is 
    ///the EXIF audio file name and extension (an ASCII string that consists of 8 characters plus a period (.), 
    ///plus 3 characters). The path is not recorded. When you use this tag, audio files must be recorded in 
    ///conformance with the EXIF audio format. Writers can also store audio data within APP2 as FlashPix extension 
    ///stream data.</summary>
    ExifRelatedWav = 40964,
    ///<summary>The type of scene. If a DSC recorded the image, the value of this tag must be set to 1, indicating 
    ///that the image was directly photographed.</summary>
    ExifSceneType = 41729,
    ///<summary>Image sensor type on the camera or input device. This is an integer with these values:
    ///1 - not defined; 2 - one-chip color area sensor; 3 - two-chip color area sensor; 4 - three-chip color area 
    ///sensor; 5 - color sequential area sensor; 7 - trilinear sensor; 8 - color sequential linear sensor;
    ///Other - reserved</summary>
    ExifSensingMethod = 41495,
    ///<summary>Shutter speed. The unit is the Additive System of Photographic Exposure (APEX) value.</summary>
    ExifShutterSpeed = 37377,
    ///<summary>Camera or input device spatial frequency table and SFR values in the image width, image height, and 
    ///diagonal direction, as specified in ISO 12233.</summary>
    ExifSpatialFR = 41484,
    ///<summary>Null-terminated character string that specifies the spectral sensitivity of each channel of the 
    ///camera used. The string is compatible with the standard developed by the ASTM Technical Committee.</summary>
    ExifSpectralSense = 34852,
    ///<summary>Distance to the subject, measured in meters.</summary>
    ExifSubjectDist = 37382,
    ///<summary>Location of the main subject in the scene. The value of this tag represents the pixel at the center 
    ///of the main subject relative to the left edge. The first value indicates the column number, and the second 
    ///value indicates the row number.</summary>
    ExifSubjectLoc = 41492,
    ///<summary>Comment tag. A tag used by EXIF users to write keywords or comments about the image besides those 
    ///in PropertyTagImageDescription and without the character-code limitations of the 
    ///PropertyTagImageDescription tag.</summary>
    ExifUserComment = 37510,
    ///<summary>Version of the EXIF standard supported. Nonexistence of this field is taken to mean nonconformance 
    ///to the standard. Conformance to the standard is indicated by recording 0210 as a 4-byte ASCII string. 
    ///Because the type is PropertyTagTypeUndefined, there is no NULL terminator.</summary>
    ExifVer = 36864,
    ///<summary>Number of extra color components. For example, one extra component might hold an alpha value.</summary>
    ExtraSamples = 338,
    ///<summary>Logical order of bits in a byte.</summary>
    FillOrder = 266,
    ///<summary>Time delay, in hundredths of a second, between two frames in an animated GIF image.</summary>
    FrameDelay = 20736,
    ///<summary>For each string of contiguous unused bytes, the number of bytes in that string.</summary>
    FreeByteCounts = 289,
    ///<summary>For each string of contiguous unused bytes, the byte offset of that string.</summary>
    FreeOffset = 288,
    ///<summary>Gamma value attached to the image. The gamma value is stored as a rational number (pair of long) 
    ///with a numerator of 100000. For example, a gamma value of 2.2 is stored as the pair (100000, 45455).</summary>
    Gamma = 769,
    ///<summary>Color palette for an indexed bitmap in a GIF image.</summary>
    GlobalPalette = 20738,
    ///<summary>Altitude, in meters, based on the reference altitude specified by PropertyTagGpsAltitudeRef.</summary>
    GpsAltitude = 6,
    ///<summary>Reference altitude, in meters.</summary>
    GpsAltitudeRef = 5,
    ///<summary>Bearing to the destination point. The range of values is from 0.00 to 359.99.</summary>
    GpsDestBear = 24,
    ///<summary>Null-terminated character string that specifies the reference used for giving the bearing to the 
    ///destination point. T specifies true direction, and M specifies magnetic direction.</summary>
    GpsDestBearRef = 23,
    ///<summary>Distance to the destination point.</summary>
    GpsDestDist = 26,
    ///<summary>Null-terminated character string that specifies the unit used to express the distance to the 
    ///destination point. K, M, and N represent kilometers, miles, and knots respectively.</summary>
    GpsDestDistRef = 25,
    ///<summary>Latitude of the destination point. The latitude is expressed as three rational values giving the 
    ///degrees, minutes, and seconds respectively. When degrees, minutes, and seconds are expressed, the format 
    ///is dd/1, mm/1, ss/1. When degrees and minutes are used and, for example, fractions of minutes are given 
    ///up to two decimal places, the format is dd/1, mmmm/100, 0/1.</summary>
    GpsDestLat = 20,
    ///<summary>Null-terminated character string that specifies whether the latitude of the destination point 
    ///is north or south latitude. N specifies north latitude, and S specifies south latitude.</summary>
    GpsDestLatRef = 19,
    ///<summary>Longitude of the destination point. The longitude is expressed as three rational values giving 
    ///the degrees, minutes, and seconds respectively. When degrees, minutes, and seconds are expressed, the 
    ///format is ddd/1, mm/1, ss/1. When degrees and minutes are used and, for example, fractions of minutes 
    ///are given up to two decimal places, the format is ddd/1, mmmm/100, 0/1.</summary>
    GpsDestLong = 22,
    ///<summary>Null-terminated character string that specifies whether the longitude of the destination point is 
    ///east or west longitude. E specifies east longitude, and W specifies west longitude.</summary>
    GpsDestLongRef = 21,
    ///<summary>GPS DOP (data degree of precision). An HDOP value is written during 2-D measurement, and a 
    ///PDOP value is written during 3-D measurement.</summary>
    GpsGpsDop = 11,
    ///<summary>Null-terminated character string that specifies the GPS measurement mode. 2 specifies 2-D 
    ///measurement, and 3 specifies 3-D measurement.</summary>
    GpsGpsMeasureMode = 10,
    ///<summary>Null-terminated character string that specifies the GPS satellites used for measurements. This tag 
    ///can be used to specify the ID number, angle of elevation, azimuth, SNR, and other information about each 
    ///satellite. The format is not specified. If the GPS receiver is incapable of taking measurements, the value 
    ///of the tag must be set to NULL.</summary>
    GpsGpsSatellites = 8,
    ///<summary>Null-terminated character string that specifies the status of the GPS receiver when the image is 
    ///recorded. A means measurement is in progress, and V means the measurement is Interoperability.</summary>
    GpsGpsStatus = 9,
    ///<summary>Time as coordinated universal time (UTC). The value is expressed as three rational numbers that 
    ///give the hour, minute, and second.</summary>
    GpsGpsTime = 7,
    ///<summary>Offset to a block of GPS property items. Property items whose tags have the prefix PropertyTagGps 
    ///are stored in the GPS block. The GPS property items are defined in the EXIF specification. GDI+ uses this 
    ///tag to locate GPS information, but GDI+ does not expose this tag for public use.</summary>
    GpsIFD = 34853,
    ///<summary>Direction of the image when it was captured. The range of values is from 0.00 to 359.99.</summary>
    GpsImgDir = 17,
    ///<summary>Null-terminated character string that specifies the reference for the direction of the image when 
    ///it is captured. T specifies true direction, and M specifies magnetic direction.</summary>
    GpsImgDirRef = 16,
    ///<summary>Latitude. Latitude is expressed as three rational values giving the degrees, minutes, and seconds 
    ///respectively. When degrees, minutes, and seconds are expressed, the format is dd/1, mm/1, ss/1. When 
    ///degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, 
    ///the format is dd/1, mmmm/100, 0/1.</summary>
    GpsLatitude = 2,
    ///<summary>Null-terminated character string that specifies whether the latitude is north or south. 
    ///N specifies north latitude, and S specifies south latitude.</summary>
    GpsLatitudeRef = 1,
    ///<summary>Longitude. Longitude is expressed as three rational values giving the degrees, minutes, and seconds
    ///respectively. When degrees, minutes and seconds are expressed, the format is ddd/1, mm/1, ss/1. When 
    ///degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, 
    ///the format is ddd/1, mmmm/100, 0/1.</summary>
    GpsLongitude = 4,
    ///<summary>Null-terminated character string that specifies whether the longitude is east or west longitude. 
    ///E specifies east longitude, and W specifies west longitude.</summary>
    GpsLongitudeRef = 3,
    ///<summary>Null-terminated character string that specifies geodetic survey data used by the GPS receiver. 
    ///If the survey data is restricted to Japan, the value of this tag is TOKYO or WGS-84.</summary>
    GpsMapDatum = 18,
    ///<summary>Speed of the GPS receiver movement.</summary>
    GpsSpeed = 13,
    ///<summary>Null-terminated character string that specifies the unit used to express the GPS receiver speed 
    ///of movement. K, M, and N represent kilometers per hour, miles per hour, and knots respectively.</summary>
    GpsSpeedRef = 12,
    ///<summary>Direction of GPS receiver movement. The range of values is from 0.00 to 359.99.</summary>
    GpsTrack = 15,
    ///<summary>Null-terminated character string that specifies the reference for giving the direction of GPS 
    /// receiver movement. T specifies true direction, and M specifies magnetic direction.</summary>
    GpsTrackRef = 14,
    ///<summary>Version of the Global Positioning Systems (GPS) IFD, given as 2.0.0.0. This tag is mandatory 
    ///when the PropertyTagGpsIFD tag is present. When the version is 2.0.0.0, the tag value is 0x02000000.</summary>
    GpsVer = 0,
    ///<summary>For each possible pixel value in a grayscale image, the optical density of that pixel value.</summary>
    GrayResponseCurve = 291,
    ///<summary>Precision of the number specified by PropertyTagGrayResponseCurve. 1 specifies tenths, 
    ///2 specifies hundredths, 3 specifies thousandths, and so on.</summary>
    GrayResponseUnit = 290,
    ///<summary>Block of information about grids and guides.</summary>
    GridSize = 20497,
    ///<summary>Angle for screen.</summary>
    HalftoneDegree = 20492,
    ///<summary>Information used by the halftone function</summary>
    HalftoneHints = 321,
    ///<summary>Ink's screen frequency, in lines per inch.</summary>
    HalftoneLPI = 20490,
    ///<summary>Units for the screen frequency.</summary>
    HalftoneLPIUnit = 20491,
    ///<summary>Miscellaneous halftone information.</summary>
    HalftoneMisc = 20494,
    ///<summary>Boolean value that specifies whether to use the printer's default screens.</summary>
    HalftoneScreen = 20495,
    ///<summary>Shape of the halftone dots.</summary>
    HalftoneShape = 20493,
    ///<summary>Null-terminated character string that specifies the computer and/or operating system 
    ///used to create the image.</summary>
    HostComputer = 316,
    ///<summary>ICC profile embedded in the image.</summary>
    ICCProfile = 34675,
    ///<summary>Null-terminated character string that identifies an ICC profile. </summary>
    ICCProfileDescriptor = 770,
    ///<summary>Null-terminated character string that specifies the title of the image.</summary>
    ImageDescription = 270,
    ///<summary>Number of pixel rows.</summary>
    ImageHeight = 257,
    ///<summary>Null-terminated character string that specifies the title of the image.</summary>
    ImageTitle = 800,
    ///<summary>Number of pixels per row.</summary>
    ImageWidth = 256,
    ///<summary>Index of the background color in the palette of a GIF image.</summary>
    IndexBackground = 20739,
    ///<summary>Index of the transparent color in the palette of a GIF image.</summary>
    IndexTransparent = 20740,
    ///<summary>Sequence of concatenated, null-terminated, character strings that specify the names of the 
    ///inks used in a separated image.</summary>
    InkNames = 333,
    ///<summary>Set of inks used in a separated image.</summary>
    InkSet = 332,
    ///<summary>For each color component, the offset to the AC Huffman table for that component. See also 
    ///PropertyTagSamplesPerPixel.</summary>
    JPEGACTables = 521,
    ///<summary>For each color component, the offset to the DC Huffman table (or lossless Huffman table) for 
    ///that component. See also PropertyTagSamplesPerPixel.</summary>
    JPEGDCTables = 520,
    ///<summary>Offset to the start of a JPEG bitstream.</summary>
    JPEGInterFormat = 513,
    ///<summary>Length, in bytes, of the JPEG bitstream.</summary>
    JPEGInterLength = 514,
    ///<summary>For each color component, a lossless predictor-selection value for that component. 
    ///See also PropertyTagSamplesPerPixel.</summary>
    JPEGLosslessPredictors = 517,
    ///<summary>For each color component, a point transformation value for that component. See also 
    ///PropertyTagSamplesPerPixel.</summary>
    JPEGPointTransforms = 518,
    ///<summary>JPEG compression process.</summary>
    JPEGProc = 512,
    ///<summary>For each color component, the offset to the quantization table for that component. 
    ///See also PropertyTagSamplesPerPixel.</summary>
    JPEGQTables = 519,
    ///<summary>Private tag used by the Adobe Photoshop format. Not for public use.</summary>
    JPEGQuality = 20496,
    ///<summary>Length of the restart interval.</summary>
    JPEGRestartInterval = 515,
    ///<summary>For an animated GIF image, the number of times to display the animation. A value of 0 specifies 
    ///that the animation should be displayed infinitely.</summary>
    LoopCount = 20737,
    ///<summary>Luminance table. The luminance table and the chrominance table are used to control JPEG quality. 
    ///A valid luminance or chrominance table has 64 entries of type PropertyTagTypeShort. If an image has 
    ///either a luminance table or a chrominance table, then it must have both tables.</summary>
    LuminanceTable = 20624,
    ///<summary>For each color component, the maximum value assigned to that component. See also 
    ///PropertyTagSamplesPerPixel.</summary>
    MaxSampleValue = 281,
    ///<summary>For each color component, the minimum value assigned to that component. See also 
    ///PropertyTagSamplesPerPixel.</summary>
    MinSampleValue = 280,
    ///<summary>Type of data in a subfile.</summary>
    NewSubfileType = 254,
    ///<summary>Number of inks.</summary>
    NumberOfInks = 334,
    ///<summary>Image orientation viewed in terms of rows and columns. The value is a System.UShort, with these
    ///values: 1 - The 0th row is at the top of the visual image, and the 0th column is the visual left side. 
    ///2 - The 0th row is at the visual top of the image, and the 0th column is the visual right side. 
    ///3 - The 0th row is at the visual bottom of the image, and the 0th column is the visual right side. 
    ///4 - The 0th row is at the visual bottom of the image, and the 0th column is the visual right side. 
    ///5 - The 0th row is the visual left side of the image, and the 0th column is the visual top. 
    ///6 - The 0th row is the visual right side of the image, and the 0th column is the visual top. 
    ///7 - The 0th row is the visual right side of the image, and the 0th column is the visual bottom. 
    ///8 - The 0th row is the visual left side of the image, and the 0th column is the visual bottom. </summary>
    Orientation = 274,
    ///<summary>Null-terminated character string that specifies the name of the page from which the image was scanned.</summary>
    PageName = 285,
    ///<summary>Page number of the page from which the image was scanned.</summary>
    PageNumber = 297,
    ///<summary>Palette histogram.</summary>
    PaletteHistogram = 20755,
    ///<summary>How pixel data will be interpreted.</summary>
    PhotometricInterp = 262,
    ///<summary>Pixels per unit in the x direction.</summary>
    PixelPerUnitX = 20753,
    ///<summary>Pixels per unit in the y direction.</summary>
    PixelPerUnitY = 20754,
    ///<summary>Unit for PropertyTagPixelPerUnitX and PropertyTagPixelPerUnitY.</summary>
    PixelUnit = 20752,
    ///<summary>Whether pixel components are recorded in chunky or planar format.</summary>
    PlanarConfig = 284,
    ///<summary>Type of prediction scheme that was applied to the image data before the encoding scheme was applied.</summary>
    Predictor = 317,
    ///<summary>For each of the three primary colors in the image, the chromaticity of that color.</summary>
    PrimaryChromaticities = 319,
    ///<summary>Sequence of one-byte Boolean values that specify printing options.</summary>
    PrintFlags = 20485,
    ///<summary>Print flags bleed width.</summary>
    PrintFlagsBleedWidth = 20488,
    ///<summary>Print flags bleed width scale.</summary>
    PrintFlagsBleedWidthScale = 20489,
    ///<summary>Print flags center crop marks.</summary>
    PrintFlagsCrop = 20487,
    ///<summary>Print flags version.</summary>
    PrintFlagsVersion = 20486,
    ///<summary>Reference black point value and reference white point value.</summary>
    REFBlackWhite = 532,
    ///<summary>Unit of measure for the horizontal resolution and the vertical resolution. 2 = inch, 3 = centimeter</summary>
    ResolutionUnit = 296,
    ///<summary>Units in which to display the image width.</summary>
    ResolutionXLengthUnit = 20483,
    ///<summary>Units in which to display horizontal resolution.</summary>
    ResolutionXUnit = 20481,
    ///<summary>Units in which to display the image height.</summary>
    ResolutionYLengthUnit = 20484,
    ///<summary>Units in which to display vertical resolution.</summary>
    ResolutionYUnit = 20482,
    ///<summary>Number of rows per strip. See also PropertyTagStripBytesCount and PropertyTagStripOffsets.</summary>
    RowsPerStrip = 278,
    ///<summary>For each color component, the numerical format (unsigned, signed, floating point) of that
    ///component. See also PropertyTagSamplesPerPixel.</summary>
    SampleFormat = 339,
    ///<summary>Number of color components per pixel.</summary>
    SamplesPerPixel = 277,
    ///<summary>For each color component, the maximum value of that component. See also PropertyTagSamplesPerPixel.</summary>
    SMaxSampleValue = 341,
    ///<summary>For each color component, the minimum value of that component. See also PropertyTagSamplesPerPixel.</summary>
    SMinSampleValue = 340,
    ///<summary>Null-terminated character string that specifies the name and version of the software 
    ///or firmware of the device used to generate the image.</summary>
    SoftwareUsed = 305,
    ///<summary>How the image should be displayed as defined by the International Color Consortium (ICC). If a 
    ///GDI+ Image object is constructed with the useEmbeddedColorManagement parameter set to TRUE, then GDI+ 
    ///renders the image according to the specified rendering intent. The intent can be set to perceptual, 
    ///relative colorimetric, saturation, or absolute colorimetric. Perceptual intent (0), which is suitable for 
    ///photographs, gives good adaptation to the display device gamut at the expense of colorimetric accuracy. 
    ///Relative colorimetric intent (1) is suitable for images (for example, logos) that require color appearance 
    ///matching that is relative to the display device white point. Saturation intent (2), which is suitable for 
    ///charts and graphs, preserves saturation at the expense of hue and lightness. Absolute colorimetric intent (3)
    ///is suitable for proofs (previews of images destined for a different display device) that require 
    ///preservation of absolute colorimetry.</summary>
    SRGBRenderingIntent = 771,
    ///<summary>For each strip, the total number of bytes in that strip.</summary>
    StripBytesCount = 279,
    ///<summary>For each strip, the byte offset of that strip. See also PropertyTagRowsPerStrip
    ///and PropertyTagStripBytesCount.</summary>
    StripOffsets = 273,
    ///<summary>Type of data in a subfile.</summary>
    SubfileType = 255,
    ///<summary>Set of flags that relate to T4 encoding.</summary>
    T4Option = 292,
    ///<summary>Set of flags that relate to T6 encoding.</summary>
    T6Option = 293,
    ///<summary>Null-terminated character string that describes the intended printing environment.</summary>
    TargetPrinter = 337,
    ///<summary>Technique used to convert from gray pixels to black and white pixels.</summary>
    ThreshHolding = 263,
    ///<summary>Null-terminated character string that specifies the name of the person who created the thumbnail image.</summary>
    ThumbnailArtist = 20532,
    ///<summary>Number of bits per color component in the thumbnail image. See also 
    ///PropertyTagThumbnailSamplesPerPixel.</summary>
    ThumbnailBitsPerSample = 20514,
    ///<summary>Bits per pixel (BPP) for the thumbnail image.</summary>
    ThumbnailColorDepth = 20501,
    ///<summary>Compressed size, in bytes, of the thumbnail image.</summary>
    ThumbnailCompressedSize = 20505,
    ///<summary>Compression scheme used for thumbnail image data.</summary>
    ThumbnailCompression = 20515,
    ///<summary>Null-terminated character string that contains copyright information for the thumbnail image.</summary>
    ThumbnailCopyRight = 20539,
    ///<summary>Raw thumbnail bits in JPEG or RGB format. Depends on PropertyTagThumbnailFormat.</summary>
    ThumbnailData = 20507,
    ///<summary>Date and time the thumbnail image was created. See also PropertyTagDateTime.</summary>
    ThumbnailDateTime = 20531,
    ///<summary>Null-terminated character string that specifies the manufacturer of the equipment used to 
    ///record the thumbnail image.</summary>
    ThumbnailEquipMake = 20518,
    ///<summary>Null-terminated character string that specifies the model name or model number of the
    ///equipment used to record the thumbnail image.</summary>
    ThumbnailEquipModel = 20519,
    ///<summary>Format of the thumbnail image.</summary>
    ThumbnailFormat = 20498,
    ///<summary>Height, in pixels, of the thumbnail image.</summary>
    ThumbnailHeight = 20500,
    ///<summary>Null-terminated character string that specifies the title of the image.</summary>
    ThumbnailImageDescription = 20517,
    ///<summary>Number of pixel rows in the thumbnail image.</summary>
    ThumbnailImageHeight = 20513,
    ///<summary>Number of pixels per row in the thumbnail image.</summary>
    ThumbnailImageWidth = 20512,
    ///<summary>Thumbnail image orientation in terms of rows and columns. See also PropertyTagOrientation.</summary>
    ThumbnailOrientation = 20521,
    ///<summary>How thumbnail pixel data will be interpreted.</summary>
    ThumbnailPhotometricInterp = 20516,
    ///<summary>Whether pixel components in the thumbnail image are recorded in chunky or planar format. 
    ///See also PropertyTagPlanarConfig.</summary>
    ThumbnailPlanarConfig = 20527,
    ///<summary>Number of color planes for the thumbnail image.</summary>
    ThumbnailPlanes = 20502,
    ///<summary>For each of the three primary colors in the thumbnail image, the chromaticity of that color. 
    ///See also PropertyTagPrimaryChromaticities.</summary>
    ThumbnailPrimaryChromaticities = 20534,
    ///<summary>Byte offset between rows of pixel data.</summary>
    ThumbnailRawBytes = 20503,
    ///<summary>Reference black point value and reference white point value for the thumbnail image. See also 
    ///PropertyTagREFBlackWhite.</summary>
    ThumbnailRefBlackWhite = 20538,
    ///<summary>Unit of measure for the horizontal resolution and the vertical resolution of the thumbnail 
    ///image. See also PropertyTagResolutionUnit.</summary>
    ThumbnailResolutionUnit = 20528,
    ///<summary>Thumbnail resolution in the width direction. The resolution unit is
    ///given in PropertyTagThumbnailResolutionUnit</summary>
    ThumbnailResolutionX = 20525,
    ///<summary>Thumbnail resolution in the height direction. The resolution unit is 
    ///given in PropertyTagThumbnailResolutionUnit</summary>
    ThumbnailResolutionY = 20526,
    ///<summary>Number of rows per strip in the thumbnail image. See also 
    ///PropertyTagThumbnailStripBytesCount and PropertyTagThumbnailStripOffsets.</summary>
    ThumbnailRowsPerStrip = 20523,
    ///<summary>Number of color components per pixel in the thumbnail image.</summary>
    ThumbnailSamplesPerPixel = 20522,
    ///<summary>Total size, in bytes, of the thumbnail image.</summary>
    ThumbnailSize = 20504,
    ///<summary>Null-terminated character string that specifies the name and version of the 
    ///software or firmware of the device used to generate the thumbnail image.</summary>
    ThumbnailSoftwareUsed = 20530,
    ///<summary>For each thumbnail image strip, the total number of bytes in that strip.</summary>
    ThumbnailStripBytesCount = 20524,
    ///<summary>For each strip in the thumbnail image, the byte offset of that strip. See also 
    ///PropertyTagThumbnailRowsPerStrip and PropertyTagThumbnailStripBytesCount.</summary>
    ThumbnailStripOffsets = 20520,
    ///<summary>Tables that specify transfer functions for the thumbnail image. See also 
    ///PropertyTagTransferFunction.</summary>
    ThumbnailTransferFunction = 20529,
    ///<summary>Chromaticity of the white point of the thumbnail image. See also PropertyTagWhitePoint.</summary>
    ThumbnailWhitePoint = 20533,
    ///<summary>Width, in pixels, of the thumbnail image.</summary>
    ThumbnailWidth = 20499,
    ///<summary>Coefficients for transformation from RGB to YCbCr data for the thumbnail image. See also 
    ///PropertyTagYCbCrCoefficients.</summary>
    ThumbnailYCbCrCoefficients = 20535,
    ///<summary>Position of chrominance components in relation to the luminance component for the thumbnail image. 
    ///See also PropertyTagYCbCrPositioning.</summary>
    ThumbnailYCbCrPositioning = 20537,
    ///<summary>Sampling ratio of chrominance components in relation to the luminance component for the 
    ///thumbnail image. See also PropertyTagYCbCrSubsampling.</summary>
    ThumbnailYCbCrSubsampling = 20536,
    ///<summary>For each tile, the number of bytes in that tile.</summary>
    TileByteCounts = 325,
    ///<summary>Number of pixel rows in each tile.</summary>
    TileLength = 323,
    ///<summary>For each tile, the byte offset of that tile.</summary>
    TileOffset = 324,
    ///<summary>Number of pixel columns in each tile.</summary>
    TileWidth = 322,
    ///<summary>Tables that specify transfer functions for the image.</summary>
    TransferFunction = 301,
    ///<summary>Table of values that extends the range of the transfer function.</summary>
    TransferRange = 342,
    ///<summary>Chromaticity of the white point of the image.</summary>
    WhitePoint = 318,
    ///<summary>Offset from the left side of the page to the left side of the image. The unit of measure
    ///is specified by PropertyTagResolutionUnit.</summary>
    XPosition = 286,
    ///<summary>Number of pixels per unit in the image width (x) direction. The unit is specified by 
    ///PropertyTagResolutionUnit.</summary>
    XResolution = 282,
    ///<summary>Coefficients for transformation from RGB to YCbCr image data. </summary>
    YCbCrCoefficients = 529,
    ///<summary>Position of chrominance components in relation to the luminance component.</summary>
    YCbCrPositioning = 531,
    ///<summary>Sampling ratio of chrominance components in relation to the luminance component.</summary>
    YCbCrSubsampling = 530,
    ///<summary>Offset from the top of the page to the top of the image. The unit of measure is 
    ///specified by PropertyTagResolutionUnit.</summary>
    YPosition = 287,
    ///<summary>Number of pixels per unit in the image height (y) direction. The unit is specified by 
    ///PropertyTagResolutionUnit.</summary>
    YResolution = 283
  }
}
