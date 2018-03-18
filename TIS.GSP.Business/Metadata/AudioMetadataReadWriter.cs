using System;
using System.Globalization;
using System.Text.RegularExpressions;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// Provides functionality for reading and writing metadata to or from an audio file.
	/// </summary>
	public class AudioMetadataReadWriter : MediaObjectMetadataReadWriter
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
		/// Initializes a new instance of the <see cref="AudioMetadataReadWriter" /> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public AudioMetadataReadWriter(IGalleryObject galleryObject)
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

		#endregion

		#region Functions

		private string GetFFmpegOutput()
		{
			return FFmpeg.GetOutput(GalleryObject.Original.FileNamePhysicalPath, GalleryObject.GalleryId);
		}

		#endregion
	}
}