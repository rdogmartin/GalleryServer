using System;
using System.Globalization;
using System.Text.RegularExpressions;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// Provides functionality for reading and writing metadata to or from a video file.
	/// </summary>
	public class VideoMetadataReadWriter : MediaObjectMetadataReadWriter
	{
		#region Fields

		private string _ffmpegOutput;

		#endregion

		#region Properties

		private string FfmpegOutput
		{
			get { return _ffmpegOutput ?? (_ffmpegOutput = GetFFmpegOutput()); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="VideoMetadataReadWriter" /> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public VideoMetadataReadWriter(IGalleryObject galleryObject)
			: base(galleryObject)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the metadata value for the specified <paramref name="metaName" />.
		/// </summary>
		/// <param name="metaName">Name of the metadata item to retrieve.</param>
		/// <returns>An instance that implements <see cref="IMetaValue" />.</returns>
		public override IMetaValue GetMetaValue(MetadataItemName metaName)
		{
			switch (metaName)
			{
				case MetadataItemName.Duration: return GetDuration();
				case MetadataItemName.BitRate: return GetBitRate();
				case MetadataItemName.AudioFormat: return GetAudioFormat();
				case MetadataItemName.VideoFormat: return GetVideoFormat();
				case MetadataItemName.Width: return GetWidth();
				case MetadataItemName.Height: return GetHeight();
				case MetadataItemName.Orientation: return GetRotation();
				default:
					return base.GetMetaValue(metaName);
			}
		}

		#endregion

		#region Metadata Functions

		private IMetaValue GetDuration()
		{
			Regex re = new Regex("[D|d]uration:.((\\d|:|\\.)*)");
			Match m = re.Match(FfmpegOutput);

			return (m.Success ? new MetaValue(m.Groups[1].Value.Trim(), m.Groups[1].Value.Trim()) : null);
		}

		private IMetaValue GetBitRate()
		{
			Regex re = new Regex("[B|b]itrate:.((\\d|:)*)");
			var m = re.Match(FfmpegOutput);
			double kb;
			if (m.Success && Double.TryParse(m.Groups[1].Value, out kb))
			{
				//TODO: Parse bitrate units instead of assuming they are kb/s
				// Line we are parsing looks like this: Duration: 00:00:25.27, start: 0.000000, bitrate: 932 kb/s
				return new MetaValue(String.Concat(kb, " kb/s"), kb.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				return null;
			}
		}

		private IMetaValue GetAudioFormat()
		{
			Regex re = new Regex("[A|a]udio:.*");
			var m = re.Match(FfmpegOutput);
			return (m.Success ? new MetaValue(m.Value.Trim(), m.Value.Trim()) : null);
		}

		private IMetaValue GetVideoFormat()
		{
			Regex re = new Regex("[V|v]ideo:.*");
			var m = re.Match(FfmpegOutput);
			return (m.Success ? new MetaValue(m.Value.Trim(), m.Value.Trim()) : null);
		}

		private IMetaValue GetWidth()
		{
			var width = FFmpeg.ParseSourceVideoWidth(FfmpegOutput);

			return (width > int.MinValue ? new MetaValue(String.Concat(width, " ", Resources.Metadata_Width_Units), width.ToString(CultureInfo.InvariantCulture)) : null);
		}

		private IMetaValue GetHeight()
		{
			var height = FFmpeg.ParseSourceVideoHeight(FfmpegOutput);

			return (height > int.MinValue ? new MetaValue(String.Concat(height, " ", Resources.Metadata_Height_Units), height.ToString(CultureInfo.InvariantCulture)) : null);
		}

		private IMetaValue GetRotation()
		{
			var orientation = FFmpeg.ParseOrientation(FfmpegOutput);

			if (MetadataEnumHelper.IsValidOrientation(orientation) && (orientation != Orientation.None))
			{
				return new MetaValue(orientation.GetDescription(), ((ushort)orientation).ToString(CultureInfo.InvariantCulture));
			}
			return null;
		}

		#endregion

		#region Functions

		private string GetFFmpegOutput()
		{
			return FFmpeg.GetOutput(GalleryObject.Original.FileNamePhysicalPath, GalleryObject.GalleryId);
		}

		#endregion
	}
}