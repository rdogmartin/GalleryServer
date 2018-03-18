using System;
using System.Drawing.Imaging;
using System.Globalization;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// Contains functionality for interacting with a <see cref="PropertyItem" /> object.
	/// </summary>
	public class MetadataItem
	{
		#region Private Fields

		private PropertyItem _propItem;
		private object _propertyItemValue;
		private ExtractedValueType _extractedValueType;

		private const int NUM_BITS_PER_BYTE = 8;
		private const int NUM_BYTES_PER_32_BIT_INT = 4;
		private const int NUM_BYTES_PER_16_BIT_INT = 2;

		private static System.Text.UTF8Encoding _utf8Encoding = new System.Text.UTF8Encoding();
		private static System.Text.UnicodeEncoding _unicodeEncoding = new System.Text.UnicodeEncoding();

		#endregion

		#region Constructors

		/// <summary>
		/// Private constructor so class can't be instantiated with default constructor from the outside.
		/// </summary>
		private MetadataItem() { }

		/// <summary>
		/// Instantiate a new instance of the <see cref="MetadataItem" /> class with the specified <paramref name="propItem"/>.
		/// </summary>
		/// <param name="propItem">A <see cref="System.Drawing.Imaging.PropertyItem" /> object for which to retrieve information.</param>
		public MetadataItem(PropertyItem propItem)
		{
			this._propItem = propItem;

			ExtractPropertyItemValue();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the <see cref="RawMetadataItemName"/> of the current instance. This value corresponds to the Id property
		/// of the <see cref="System.Drawing.Imaging.PropertyItem"/> instance specified in the constructor.
		/// </summary>
		/// <value>The name of the raw metadata item.</value>
		public RawMetadataItemName RawMetadataItemName
		{
			get
			{
				return (RawMetadataItemName)this._propItem.Id;
			}
		}

		/// <summary>
		/// Gets an enumeration specifying the data type of the values stored in the current <see cref="PropertyItem"/> property.
		/// Note that these are not ordinary .NET types.
		/// </summary>
		/// <value>The enum value specifying the data type of the values stored in the current <see cref="PropertyItem"/> property.</value>
		public PropertyTagType PropertyTagType
		{
			get
			{
				return (PropertyTagType)this._propItem.Type;
			}
		}

		/// <summary>
		/// Gets an enumeration specifying the data type of the value stored in the <see cref="Value" /> property.
		/// </summary>
		/// <value>The enum value specifying the data type of the value stored in the <see cref="Value" /> property.</value>
		public ExtractedValueType ExtractedValueType
		{
			get { return this._extractedValueType; }
			set { this._extractedValueType = value; }
		}

		/// <summary>
		/// Gets the <see cref="System.Drawing.Imaging.PropertyItem" /> for this metadata item.
		/// </summary>
		public PropertyItem PropertyItem
		{
			get
			{
				return this._propItem;
			}
		}

		/// <summary>
		/// Gets the value of the current <see cref="PropertyItem"/>. It is converted from the original byte array to
		/// to the appropriate user-friendly .NET type. The type of the value is stored in the <see cref="ExtractedValueType" /> property.
		/// </summary>
		public Object Value
		{
			get
			{
				return this._propertyItemValue;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Extracts the value of the current PropertyItem and stores it in the private field _propertyItemValue. 
		/// It is converted to the appropriate user-friendly .NET type based on the PropertyTagType enumeration and whether
		/// the value contains a single item or an array. The type of the converted value is stored in the ExtractedValueType property.
		/// </summary>
		/// <remarks>
		/// PropertyTagType.Byte and ASCII are converted to a string (ExtractedValueType.String). PropertyTagType.UnsignedShort, UnsignedInt, 
		/// and Int are converted to System.Int64 or System.Int64[] (ExtractedValueType.Int64 or ExtractedValueType.Int64Array).
		/// PropertyTagType.Fraction and UnsignedFraction are converted to the Fraction class or an array of Fraction objects 
		/// (ExtractedValueType.Fraction or ExtractedValueType.FractionArray). PropertyTagType.Undefined and any other 
		/// PropertyTagType enumerations are returned as System.Byte[] (ExtractedValueType.ByteArray). After this function runs, 
		/// the ExtractedValueType property is guaranteed to be set a value other than ExtractedValueType.NotDefined. The field 
		/// _propertyItemValue is guaranteed to be a type that matches the ExtractedValueType enumeration, which means it will be
		/// one of these types: String, Int64, Int64[], Byte[], Fraction, or Fraction[].
		/// </remarks>
		private void ExtractPropertyItemValue()
		{
			object propertyItemValue = String.Empty;
			ExtractedValueType formattedValueType = ExtractedValueType.NotDefined;

			switch (this.PropertyTagType)
			{
				case PropertyTagType.Byte: { propertyItemValue = ExtractPropertyValueByte(ref formattedValueType); break; }
				case PropertyTagType.ASCII: { propertyItemValue = ExtractPropertyValueString(ref formattedValueType); break; }
				case PropertyTagType.UnsignedShort: { propertyItemValue = ExtractPropertyValueUnsignedShort(ref formattedValueType); break; }
				case PropertyTagType.UnsignedInt: { propertyItemValue = ExtractPropertyValueUnsignedInt(ref formattedValueType); break; }
				case PropertyTagType.Int: { propertyItemValue = ExtractPropertyValueInt(ref formattedValueType); break; }
				case PropertyTagType.UnsignedFraction: { propertyItemValue = ExtractPropertyValueUnsignedFraction(ref formattedValueType); break; }
				case PropertyTagType.Fraction: { propertyItemValue = ExtractPropertyValueSignedFraction(ref formattedValueType); break; }
				case PropertyTagType.Undefined:
				default: { propertyItemValue = ExtractPropertyValueUndefined(ref formattedValueType); break; }
			}

			this._extractedValueType = formattedValueType;
			this._propertyItemValue = propertyItemValue;

			if (this._extractedValueType == ExtractedValueType.NotDefined)
			{
				throw new BusinessException("The function GalleryServer.Business.Metadata.MetadataItem.ExtractPropertyItemValue() must assign a value other than NotDefined to the field _extractedValueType.");
			}
		}

		private byte[] ExtractPropertyValueUndefined(ref ExtractedValueType formattedValueType)
		{
			formattedValueType = ExtractedValueType.ByteArray;

			if (this._propItem.Value == null)
				return new byte[] { 0 };

			return this._propItem.Value;
		}

		private object ExtractPropertyValueSignedFraction(ref ExtractedValueType formattedValueType)
		{
			Fraction[] resultSFraction = new Fraction[this._propItem.Len / NUM_BITS_PER_BYTE];
			int sNominator;
			int sDenominator;
			for (int i = 0; i < resultSFraction.Length; i++)
			{
				try
				{
					sNominator = BitConverter.ToInt32(this._propItem.Value, i * NUM_BITS_PER_BYTE);
					sDenominator = BitConverter.ToInt32(this._propItem.Value, (i * NUM_BITS_PER_BYTE) + NUM_BYTES_PER_32_BIT_INT);
				}
				catch (ArgumentNullException)
				{
					formattedValueType = ExtractedValueType.Fraction;
					return new Fraction(0, 1);
				}
				catch (ArgumentOutOfRangeException)
				{
					formattedValueType = ExtractedValueType.Fraction;
					return new Fraction(0, 1);
				}

				resultSFraction[i] = new Fraction(sNominator, sDenominator);
			}

			if (resultSFraction.Length == 1)
			{
				formattedValueType = ExtractedValueType.Fraction;
				return resultSFraction[0];
			}
			else
			{
				formattedValueType = ExtractedValueType.FractionArray;

				// Comment out, since it causes a huge performance hit when synchronizing files that contain this type of metadata item.
				// Perhaps find some way to raise the visibility of this by noting it once per application run.
				//string msg = String.Format(CultureInfo.CurrentCulture, "Discovered an Exif metadata item named {0} that is of type GalleryServer.Business.Fraction[] rather than the usual GalleryServer.Business.Fraction. Gallery Server cannot process this metadata and will discard it. One may want to modify Gallery Server to handle this data type.", this.RawMetadataItemName);
				//ErrorHandler.CustomExceptions.BusinessException ex = new ErrorHandler.CustomExceptions.BusinessException(msg);
				//ErrorHandler.Error.Record(ex, int.MinValue, Factory.LoadGallerySettings(), AppSetting.Instance);
				return null;
			}
		}

		private object ExtractPropertyValueUnsignedFraction(ref ExtractedValueType formattedValueType)
		{
			Fraction[] resultFraction = new Fraction[this._propItem.Len / NUM_BITS_PER_BYTE];
			uint uNominator;
			uint uDenominator;
			for (int i = 0; i < resultFraction.Length; i++)
			{
				uNominator = 1;
				try
				{
					uNominator = BitConverter.ToUInt32(this._propItem.Value, i * NUM_BITS_PER_BYTE);
					uDenominator = BitConverter.ToUInt32(this._propItem.Value, (i * NUM_BITS_PER_BYTE) + NUM_BYTES_PER_32_BIT_INT);
				}
				catch (ArgumentNullException)
				{
					formattedValueType = ExtractedValueType.Fraction;
					return new Fraction(0, 1);
				}
				catch (ArgumentOutOfRangeException)
				{
					formattedValueType = ExtractedValueType.Fraction;
					return new Fraction(0, 1);
				}

				resultFraction[i] = new Fraction(uNominator, uDenominator);
			}

			if (resultFraction.Length == 1)
			{
				formattedValueType = ExtractedValueType.Fraction;
				return resultFraction[0];
			}
			else
			{
				formattedValueType = ExtractedValueType.FractionArray;

				// Comment out, since it causes a huge performance hit when synchronizing files that contain this type of metadata item.
				// Perhaps find some way to raise the visibility of this by noting it once per application run.
				//string msg = String.Format(CultureInfo.CurrentCulture, "Discovered an Exif metadata item named {0} that is of type GalleryServer.Business.Fraction[] rather than the usual GalleryServer.Business.Fraction. Gallery Server cannot process this metadata and will discard it. One may want to modify Gallery Server to handle this data type.", this.RawMetadataItemName);
				//ErrorHandler.CustomExceptions.BusinessException ex = new ErrorHandler.CustomExceptions.BusinessException(msg);
				//ErrorHandler.Error.Record(ex, int.MinValue, Factory.LoadGallerySettings(), AppSetting.Instance);
				return null;
			}
		}

		private object ExtractPropertyValueInt(ref ExtractedValueType formattedValueType)
		{
			System.Int64[] resultInt64 = new System.Int64[this._propItem.Len / NUM_BYTES_PER_32_BIT_INT];
			for (int i = 0; i < resultInt64.Length; i++)
			{
				try
				{
					resultInt64[i] = Convert.ToInt64(BitConverter.ToInt32(this._propItem.Value, i * NUM_BYTES_PER_32_BIT_INT));
				}
				catch (ArgumentNullException)
				{
					formattedValueType = ExtractedValueType.Int64;
					return Convert.ToInt64(0);
				}
				catch (ArgumentOutOfRangeException)
				{
					formattedValueType = ExtractedValueType.Int64;
					return Convert.ToInt64(0);
				}
			}

			if (resultInt64.Length == 1)
			{
				formattedValueType = ExtractedValueType.Int64;
				return resultInt64[0];
			}
			else
			{
				formattedValueType = ExtractedValueType.Int64Array;

				// Comment out, since it causes a huge performance hit when synchronizing files that contain this type of metadata item.
				// Perhaps find some way to raise the visibility of this by noting it once per application run.
				//string msg = String.Format(CultureInfo.CurrentCulture, "Discovered an Exif metadata item named {0} that is of type System.Int64[] rather than the usual System.Int64. Gallery Server cannot process this metadata and will discard it. One may want to modify Gallery Server to handle this data type.", this.RawMetadataItemName);
				//ErrorHandler.CustomExceptions.BusinessException ex = new ErrorHandler.CustomExceptions.BusinessException(msg);
				//ErrorHandler.Error.Record(ex, int.MinValue, Factory.LoadGallerySettings(), AppSetting.Instance);
				return null;
			}
		}

		private object ExtractPropertyValueUnsignedInt(ref ExtractedValueType formattedValueType)
		{
			System.Int64[] resultInt64 = new System.Int64[this._propItem.Len / NUM_BYTES_PER_32_BIT_INT];
			for (int i = 0; i < resultInt64.Length; i++)
			{
				try
				{
					resultInt64[i] = Convert.ToInt64(BitConverter.ToUInt32(this._propItem.Value, i * NUM_BYTES_PER_32_BIT_INT));
				}
				catch (ArgumentNullException)
				{
					formattedValueType = ExtractedValueType.Int64;
					return Convert.ToInt64(0);
				}
				catch (ArgumentOutOfRangeException)
				{
					formattedValueType = ExtractedValueType.Int64;
					return Convert.ToInt64(0);
				}
			}

			if (resultInt64.Length == 1)
			{
				formattedValueType = ExtractedValueType.Int64;
				return resultInt64[0];
			}
			else
			{
				formattedValueType = ExtractedValueType.Int64Array;

				// Comment out, since it causes a huge performance hit when synchronizing files that contain this type of metadata item.
				// Perhaps find some way to raise the visibility of this by noting it once per application run.
				//string msg = String.Format(CultureInfo.CurrentCulture, "Discovered an Exif metadata item named {0} that is of type System.UInt32[] rather than the usual System.UInt32. Gallery Server cannot process this metadata and will discard it. One may want to modify Gallery Server to handle this data type.", this.RawMetadataItemName);
				//ErrorHandler.CustomExceptions.BusinessException ex = new ErrorHandler.CustomExceptions.BusinessException(msg);
				//ErrorHandler.Error.Record(ex, int.MinValue, Factory.LoadGallerySettings(), AppSetting.Instance);
				return null;
			}
		}

		private object ExtractPropertyValueUnsignedShort(ref ExtractedValueType formattedValueType)
		{
			System.Int64[] resultInt64 = new System.Int64[this._propItem.Len / NUM_BYTES_PER_16_BIT_INT];
			for (int i = 0; i < resultInt64.Length; i++)
			{
				try
				{
					resultInt64[i] = Convert.ToInt64(BitConverter.ToUInt16(this._propItem.Value, i * NUM_BYTES_PER_16_BIT_INT));
				}
				catch (ArgumentNullException)
				{
					formattedValueType = ExtractedValueType.Int64;
					return Convert.ToInt64(0);
				}
				catch (ArgumentOutOfRangeException)
				{
					formattedValueType = ExtractedValueType.Int64;
					return Convert.ToInt64(0);
				}
			}

			if (resultInt64.Length == 1)
			{
				formattedValueType = ExtractedValueType.Int64;
				return resultInt64[0];
			}
			else
			{
				formattedValueType = ExtractedValueType.Int64Array;

				// Comment out, since it causes a huge performance hit when synchronizing files that contain this type of metadata item.
				// Perhaps find some way to raise the visibility of this by noting it once per application run.
				//string msg = String.Format(CultureInfo.CurrentCulture, "Discovered an Exif metadata item named {0} that is of type System.UInt16[] rather than the usual System.UInt16. Gallery Server cannot process this metadata and will discard it. One may want to modify Gallery Server to handle this data type.", this.RawMetadataItemName);
				//ErrorHandler.CustomExceptions.BusinessException ex = new ErrorHandler.CustomExceptions.BusinessException(msg);
				//ErrorHandler.Error.Record(ex, int.MinValue, Factory.LoadGallerySettings(), AppSetting.Instance);
				return null;
			}
		}

		private string ExtractPropertyValueString(ref ExtractedValueType formattedValueType)
		{
			formattedValueType = ExtractedValueType.String;

			if (this._propItem.Value == null)
				return String.Empty;

			// Do not use ASCII decoding because it can't handle UTF8-encoded data, which is fairly common.
			// See http://stackoverflow.com/questions/19284205/safe-to-use-utf8-decoding-for-exif-property-marked-as-ascii
			return _utf8Encoding.GetString(this._propItem.Value);
		}

		private string ExtractPropertyValueByte(ref ExtractedValueType formattedValueType)
		{
			formattedValueType = ExtractedValueType.String;

			if (this._propItem.Value == null)
				return String.Empty;

			if (this._propItem.Value.Length == 1)
			{
				return this._propItem.Value[0].ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				return _unicodeEncoding.GetString(this._propItem.Value);
			}
		}

		#endregion
	}
}
