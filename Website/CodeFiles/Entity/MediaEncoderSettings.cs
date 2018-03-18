namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A client-optimized object that represents media encoder settings.
	/// </summary>
	public class MediaEncoderSettings
	{
		/// <summary>
		/// Gets or sets the file extension of the media file used as the source for an encoding. 
		/// Example: .avi, .dv
		/// </summary>
		/// <value>A string.</value>
		public string SourceFileExtension { get; set; }

		/// <summary>
		/// Gets or sets the file extension of the media file created as a result of the encoding. 
		/// Example: .mp4, .flv
		/// </summary>
		/// <value>A string.</value>
		public string DestinationFileExtension { get; set; }

		/// <summary>
		/// Gets or sets the arguments to pass to the encoder utility. May contain the following 
		/// replacement tokens: {SourceFilePath}, {DestinationFilePath}, {GalleryResourcesPath},
		/// {BinPath}, {AspectRatio}, {Width}, {Height}
		/// </summary>
		/// <value>A string.</value>
		public string EncoderArguments { get; set; }
	}

	/// <summary>
	/// A client-optimized object that represents a file extension.
	/// </summary>
	public class FileExtension
	{
		/// <summary>
		/// Gets or sets the text representation of a file extension (e.g. ".jpg", "All video").
		/// </summary>
		/// <value>The text.</value>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets the file extension (e.g. ".jpg", "*video").
		/// </summary>
		/// <value>The value.</value>
		public string Value { get; set; }
	}
}