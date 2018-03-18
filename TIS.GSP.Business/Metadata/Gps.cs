using System;
using System.Globalization;
using System.Windows.Media.Imaging;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{

	/// <summary>
	/// Represents a geographical location on earth.
	/// </summary>
	public class GpsLocation
	{
		private string _version;
		private double? _altitude;
		private GpsDistance _latitude;
		private GpsDistance _longitude;
		private GpsDistance _destLatitude;
		private GpsDistance _destLongitude;

		/// <summary>
		/// The version of the GPS information. Example: "2.2.0.0"
		/// </summary>
		/// <value>The version of the GPS information.</value>
		public string Version
		{
			get { return _version; }
			set { _version = value; }
		}

		/// <summary>
		/// The altitude, in meters, of the media object. Will be negative for values below sea level.
		/// </summary>
		/// <value>The altitude.</value>
		public double? Altitude
		{
			get { return _altitude; }
			set { _altitude = value; }
		}

		/// <summary>
		/// Gets or sets the latitude portion of the current instance.
		/// </summary>
		/// <value>The latitude.</value>
		public GpsDistance Latitude
		{
			get { return _latitude; }
			set { _latitude = value; }
		}

		/// <summary>
		/// Gets or sets the longitude portion of the current instance.
		/// </summary>
		/// <value>The longitude.</value>
		public GpsDistance Longitude
		{
			get { return _longitude; }
			set { _longitude = value; }
		}

		/// <summary>
		/// Gets or sets the destination latitude portion of the current instance.
		/// </summary>
		/// <value>The latitude.</value>
		public GpsDistance DestLatitude
		{
			get { return _destLatitude; }
			set { _destLatitude = value; }
		}

		/// <summary>
		/// Gets or sets the destination longitude portion of the current instance.
		/// </summary>
		/// <value>The longitude.</value>
		public GpsDistance DestLongitude
		{
			get { return _destLongitude; }
			set { _destLongitude = value; }
		}

		/// <summary>
		/// Generates a decimal-based version of the GPS coordinates. Ex: "46.5925° N 88.9882° W"
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public string ToLatitudeLongitudeDecimalString()
		{
			return String.Concat(Latitude.ToDoubleString(), " ", Longitude.ToDoubleString());
		}

		/// <summary>
		/// Expresses the value of the GPS coordinates in terms of degrees, minutes, and seconds. Ex: "46°32'15.24" N 88°53'25.82" W"
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public string ToLatitudeLongitudeDegreeMinuteSecondString()
		{
			return String.Concat(Latitude.ToDegreeMinuteSecondString(), " ", Longitude.ToDegreeMinuteSecondString());
		}

		/// <summary>
		/// Generates a decimal-based version of the destination GPS coordinates. Ex: "46.5925° N 88.9882° W"
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public string ToDestLatitudeLongitudeDecimalString()
		{
			return String.Concat(DestLatitude.ToDoubleString(), " ", DestLongitude.ToDoubleString());
		}

		/// <summary>
		/// Expresses the value of the destination GPS coordinates in terms of degrees, minutes, and seconds. Ex: "46°32'15.24" N 88°53'25.82" W"
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public string ToDestLatitudeLongitudeDegreeMinuteSecondString()
		{
			return String.Concat(DestLatitude.ToDegreeMinuteSecondString(), " ", DestLongitude.ToDegreeMinuteSecondString());
		}

		/// <summary>
		/// Parses the GPS data from the specified <paramref name="bmpMetadata" /> and returns the data in an instance of <see cref="GpsLocation" />.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <returns>An instance of <see cref="GpsLocation" />.</returns>
		public static GpsLocation Parse(IWpfMetadata bmpMetadata)
		{
			GpsLocation gps = new GpsLocation();

			gps.Version = GetVersion(bmpMetadata);
			gps.Latitude = GetLatitude(bmpMetadata);
			gps.Longitude = GetLongitude(bmpMetadata);
			gps.Altitude = GetAltitude(bmpMetadata);
			gps.DestLatitude = GetDestLatitude(bmpMetadata);
			gps.DestLongitude = GetDestLongitude(bmpMetadata);

			//// Combine date portion of gpsDate or gpsDate2 with time portion of gpsTime2
			//object gpsDate = bmpMetadata.GetQuery("System.GPS.Date"); // System.Runtime.InteropServices.ComTypes.FILETIME
			//object gpsDate2 = bmpMetadata.GetQuery("/app1/ifd/gps/{ushort=29}"); // 2010:08:08

			//DateTime gpsDate3 = Convert((System.Runtime.InteropServices.ComTypes.FILETIME)gpsDate);

			//ulong[] gpsTime2 = bmpMetadata.GetQuery("/app1/ifd/gps/{ushort=7}") as ulong[]; // ulong[3]
			//double hh = SplitLongAndDivide(gpsTime2[0]);
			//double mm = SplitLongAndDivide(gpsTime2[1]);
			//double ss = SplitLongAndDivide(gpsTime2[2]);

			//object satellites = bmpMetadata.GetQuery("System.GPS.Satellites"); //"05"
			//object satellites2 = bmpMetadata.GetQuery("/app1/ifd/gps/{ushort=8}"); //"05"

			//double longitude = ConvertCoordinate(longitudeArray);

			return gps;
		}

		private static double? GetAltitude(IWpfMetadata bmpMetadata)
		{
			object altObj = GetQuery(bmpMetadata, "System.GPS.Altitude");

			if (altObj == null)
			{
				altObj = GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=6}");

				if (altObj != null)
				{
					altObj = ConvertCoordinate(new[] { (ulong)altObj })[0];
				}
			}

			if (altObj == null)
			{
				return null;
			}

			return (IsBelowSeaLevel(bmpMetadata) ? (double)altObj * (-1) : (double)altObj);
		}

		/// <summary>
		/// Determines whether the GPS altitude is above or below sea level. Returns <c>false</c> if the metadata is not present.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <returns>
		/// 	<c>true</c> if the GPS position is below sea level; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsBelowSeaLevel(IWpfMetadata bmpMetadata)
		{
			object directionObj = GetQuery(bmpMetadata, "System.GPS.AltitudeRef") ?? GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=5}");

			bool isBelowSeaLevel = false;
			if (directionObj != null)
			{
				try
				{
					isBelowSeaLevel = (Convert.ToByte(directionObj, CultureInfo.InvariantCulture) == 1); // 0 = above sea level; 1 = below sea level
				}
				catch (InvalidCastException) { }
				catch (OverflowException) { }
			}

			return isBelowSeaLevel;
		}

		/// <summary>
		/// Gets the latitude GPS data from <paramref name="bmpMetadata" />.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <returns>An instance of <see cref="GpsDistance" />.</returns>
		private static GpsDistance GetLatitude(IWpfMetadata bmpMetadata)
		{
			string direction = GetQuery(bmpMetadata, "System.GPS.LatitudeRef") as string ?? GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=1}") as string;

			double[] latitude = GetQuery(bmpMetadata, "System.GPS.Latitude") as double[] ?? ConvertCoordinate(GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=2}") as ulong[]);

			if (!String.IsNullOrEmpty(direction) && (latitude != null))
			{
				return new GpsDistance(direction, latitude[0], latitude[1], latitude[2]);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the longitude GPS data from <paramref name="bmpMetadata" />.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <returns>An instance of <see cref="GpsDistance" />.</returns>
		private static GpsDistance GetLongitude(IWpfMetadata bmpMetadata)
		{
			string direction = GetQuery(bmpMetadata, "System.GPS.LongitudeRef") as string ?? GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=3}") as string;

			double[] longitude = GetQuery(bmpMetadata, "System.GPS.Longitude") as double[] ?? ConvertCoordinate(GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=4}") as ulong[]);

			if (!String.IsNullOrEmpty(direction) && (longitude != null))
			{
				return new GpsDistance(direction, longitude[0], longitude[1], longitude[2]);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the destination latitude GPS data from <paramref name="bmpMetadata" />.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <returns>An instance of <see cref="GpsDistance" />.</returns>
		private static GpsDistance GetDestLatitude(IWpfMetadata bmpMetadata)
		{
			string direction = GetQuery(bmpMetadata, "System.GPS.DestLatitudeRef") as string ?? GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=19}") as string;

			double[] latitude = GetQuery(bmpMetadata, "System.GPS.DestLatitude") as double[] ?? ConvertCoordinate(GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=20}") as ulong[]);

			if (!String.IsNullOrEmpty(direction) && (latitude != null))
			{
				return new GpsDistance(direction, latitude[0], latitude[1], latitude[2]);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the destination longitude GPS data from <paramref name="bmpMetadata" />.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <returns>An instance of <see cref="GpsDistance" />.</returns>
		private static GpsDistance GetDestLongitude(IWpfMetadata bmpMetadata)
		{
			string direction = GetQuery(bmpMetadata, "System.GPS.DestLongitudeRef") as string ?? GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=21}") as string;

			double[] longitude = GetQuery(bmpMetadata, "System.GPS.DestLongitude") as double[] ?? ConvertCoordinate(GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=22}") as ulong[]);

			if (!String.IsNullOrEmpty(direction) && (longitude != null))
			{
				return new GpsDistance(direction, longitude[0], longitude[1], longitude[2]);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the version of the GPS information. Example: "2.2.0.0"
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <returns>An instance of <see cref="System.String" />.</returns>
		private static string GetVersion(IWpfMetadata bmpMetadata)
		{
			string version = String.Empty;
			byte[] versionTokens = GetQuery(bmpMetadata, "System.GPS.VersionID") as byte[] ?? GetQuery(bmpMetadata, @"/app1/ifd/gps/{ushort=0}") as byte[];

			if (versionTokens == null) return version;

			foreach (byte versionToken in versionTokens)
			{
				version += versionToken + ".";
			}

			return version.TrimEnd(new[] { '.' });
		}

		/// <summary>
		/// Invokes the <see cref="BitmapMetadata.GetQuery" /> method on <paramref name="bmpMetadata" />, passing in the specified
		/// <paramref name="query" />. Any <see cref="NotSupportedException" /> exceptions are silently swallowed.
		/// </summary>
		/// <param name="bmpMetadata">An object containing the metadata.</param>
		/// <param name="query">The query to execute against <paramref name="bmpMetadata" />.</param>
		/// <returns></returns>
		private static object GetQuery(IWpfMetadata bmpMetadata, string query)
		{
			try
			{
				return bmpMetadata.GetQuery(query);
			}
			catch (NotSupportedException) { return null; }
			catch (System.Runtime.InteropServices.COMException) { return null; }
		}

		/// <summary>
		/// Convert the unsigned long values into an equivalent array of <see cref="System.Double" /> values.
		/// </summary>
		/// <param name="values">The values to convert.</param>
		/// <returns>Returns an array of <see cref="System.Double" /> values.</returns>
		private static double[] ConvertCoordinate(ulong[] values)
		{
			if (values == null) return null;

			double[] convertedValues = new double[values.Length];

			for (int index = 0; index < values.Length; index++)
			{
				convertedValues[index] = SplitLongAndDivide(values[index]);
			}

			return convertedValues;
		}

		/// <summary>
		/// Convert the <paramref name="number" /> into a <see cref="System.Double" />.
		/// </summary>
		/// <param name="number">The number to convert.</param>
		/// <returns>Returns a <see cref="System.Double" />.</returns>
		private static double SplitLongAndDivide(ulong number)
		{
			byte[] bytes = BitConverter.GetBytes(number);
			double dbl1 = BitConverter.ToInt32(bytes, 0);
			double dbl2 = BitConverter.ToInt32(bytes, 4);

			if (Math.Abs(dbl2 - 0) < .0001) return 0;

			return (dbl1 / dbl2);
		}
	}

	/// <summary>
	/// Represents a measure of angular distance. Can be used to store the latitude or longitude component of GPS coordinates.
	/// </summary>
	public class GpsDistance
	{
		readonly string[] _directionValues = new[] { "N", "S", "W", "E" };
		readonly string[] _negativeDirectionValues = new[] { "S", "W" };

		private readonly string _direction; // "N", "S", "W", "E"
		private readonly double _degrees;
		private readonly double _minutes;
		private readonly double _seconds;

		/// <summary>
		/// Gets the bearing of this instance. Returns "N", "S", "W", "E".
		/// </summary>
		/// <value>A <see cref="System.String"/>.</value>
		public string Direction
		{
			get { return _direction; }
		}

		/// <summary>
		/// Gets the degrees component of the current instance.
		/// </summary>
		/// <value>The degrees.</value>
		public double Degrees
		{
			get { return _degrees; }
		}

		/// <summary>
		/// Gets the minutes component of the current instance.
		/// </summary>
		/// <value>The minutes.</value>
		public double Minutes
		{
			get { return _minutes; }
		}

		/// <summary>
		/// Gets the seconds component of the current instance.
		/// </summary>
		/// <value>The seconds.</value>
		public double Seconds
		{
			get { return _seconds; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GpsDistance"/> class.
		/// </summary>
		/// <param name="direction">The bearing of the direction. Specify "N", "S", "W", or "E".</param>
		/// <param name="degrees">The degrees.</param>
		/// <param name="minutes">The minutes.</param>
		/// <param name="seconds">The seconds.</param>
		public GpsDistance(string direction, double degrees, double minutes, double seconds)
		{
			if (Array.IndexOf(_directionValues, direction) >= 0)
			{
				_direction = direction;
			}

			_degrees = (float)degrees;
			_minutes = (float)minutes;
			_seconds = (float)seconds;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="GpsDistance"/> to <see cref="System.Double"/>.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator double(GpsDistance obj)
		{
			if (obj == null)
				return 0;

			return obj.ToDouble();
		}

		/// <summary>
		/// Generates an integer representation of the current instance. Will be negative for values west of the Prime Meridian
		/// and south of the equator. Ex: "46.5925", "-88.9882"
		/// </summary>
		/// <returns>A <see cref="System.Double"/> that represents this instance.</returns>
		public double ToDouble()
		{
			double distance = Degrees + Minutes / 60.0d + Seconds / 3600.0d;

			if (Array.IndexOf(_negativeDirectionValues, Direction) >= 0)
			{
				distance = distance * -1;
			}

			return distance;
		}

		/// <summary>
		/// Generates a decimal representation of the current instance, including the north/south/east/west indicator.
		/// Ex: "46.5925° N", "88.9882° W"
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public string ToDoubleString()
		{
			return String.Format(CultureInfo.InvariantCulture, "{0:F6}° {1}", Math.Abs(ToDouble()), Direction);
		}

		/// <summary>
		/// Generates a string containing the degrees, minutes, and seconds of the current instance. Includes the north/south/east/west indicator.
		/// Ex: "46°32'15.24" N"
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public string ToDegreeMinuteSecondString()
		{
			return String.Format(CultureInfo.InvariantCulture, "{0:F0}°{1:F0}'{2:F2}\" {3}", (float)Degrees, (float)Minutes, (float)Seconds, Direction);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance. Internally, this function calls <see cref="ToDegreeMinuteSecondString" />.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents this instance.</returns>
		public override string ToString()
		{
			return ToDegreeMinuteSecondString();
		}
	}
}
