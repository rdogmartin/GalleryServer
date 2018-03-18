using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents the settings used to control the encoding of one media type to another. For example, an
	/// instance might store the FFmpeg command line arguments to use when converting .AVI files to .MP4.
	/// </summary>
	public interface IMediaEncoderSettings : IComparable<IMediaEncoderSettings>
	{
		/// <summary>
		/// Gets or sets the file extension of the media file used as the source for an encoding. 
		/// Example: .avi, .dv
		/// </summary>
		/// <value>A string.</value>
		string SourceFileExtension { get; set; }

		/// <summary>
		/// Gets or sets the file extension of the media file created as a result of the encoding. 
		/// Example: .mp4, .flv
		/// </summary>
		/// <value>A string.</value>
		string DestinationFileExtension { get; set; }

		/// <summary>
		/// Gets or sets the arguments to pass to the encoder utility. May contain the following 
		/// replacement tokens: {SourceFilePath}, {DestinationFilePath}, {GalleryResourcesPath},
		/// {BinPath}, {AspectRatio}, {Width}, {Height}
		/// </summary>
		/// <value>A string.</value>
		string EncoderArguments { get; set; }

		/// <summary>
		/// Gets or sets the order of this item in relation to other items.
		/// </summary>
		/// <value>The order this item in relation to other items.</value>
		int Sequence { get; set; }

		/// <summary>
		/// Verifies the item contains valid data.
		/// </summary>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when the instance references
		/// a file type not recognized by the application.</exception>
		void Validate();
	}
}