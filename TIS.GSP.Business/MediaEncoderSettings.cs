using System;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// Represents the settings used to control the encoding of one media type to another. For example, an
	/// instance might store the FFmpeg command line arguments to use when converting .AVI files to .MP4.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{_sourceFileExtension} => {_destinationFileExtension}, Seq={_sequence}, Args={_encoderArguments}")]
	public class MediaEncoderSettings : IMediaEncoderSettings
	{
		private string _sourceFileExtension;
		private string _destinationFileExtension;
		private string _encoderArguments;
		private int _sequence;

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaEncoderSettings"/> class.
		/// </summary>
		/// <param name="sourceFileExtension">The source file extension.</param>
		/// <param name="destinationFileExtension">The destination file extension.</param>
		/// <param name="encoderArguments">The encoder arguments.</param>
		/// <param name="sequence">The sequence.</param>
		public MediaEncoderSettings(string sourceFileExtension, string destinationFileExtension, string encoderArguments, int sequence)
		{
			_sourceFileExtension = sourceFileExtension;
			_destinationFileExtension = destinationFileExtension;
			_encoderArguments = encoderArguments;
			_sequence = sequence;
		}

		/// <summary>
		/// Gets or sets the file extension of the media file used as the source for an encoding. 
		/// Example: .avi, .dv
		/// </summary>
		/// <value>A string.</value>
		public string SourceFileExtension
		{
			get { return _sourceFileExtension; }
			set { _sourceFileExtension = value; }
		}

		/// <summary>
		/// Gets or sets the file extension of the media file created as a result of the encoding. 
		/// Example: .mp4, .flv
		/// </summary>
		/// <value>A string.</value>
		public string DestinationFileExtension
		{
			get { return _destinationFileExtension; }
			set { _destinationFileExtension = value; }
		}

		/// <summary>
		/// Gets or sets the arguments to pass to the encoder utility. May contain the following
		/// replacement tokens: {SourceFilePath}, {DestinationFilePath}, {GalleryResourcesPath},
		/// {BinPath}, {AspectRatio}, {Width}, {Height}
		/// </summary>
		/// <value>A string.</value>
		public string EncoderArguments
		{
			get { return _encoderArguments; }
			set { _encoderArguments = value; }
		}

		/// <summary>
		/// Gets or sets the order of this item in relation to other items.
		/// </summary>
		/// <value>The order this item in relation to other items.</value>
		public int Sequence
		{
			get { return _sequence; }
			set { _sequence = value; }
		}

		/// <summary>
		/// Verifies the item contains valid data.
		/// </summary>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when the instance references
		/// a file type not recognized by the application.</exception>
		public void Validate()
		{
			if ((ShouldValidate(SourceFileExtension)) && Factory.LoadMimeType(SourceFileExtension) == null)
			{
				throw new UnsupportedMediaObjectTypeException(string.Format("The media encoder setting references a file extension ({0}) not recognized by the application.", SourceFileExtension));
			}

			if ((ShouldValidate(DestinationFileExtension)) && Factory.LoadMimeType(DestinationFileExtension) == null)
			{
				throw new UnsupportedMediaObjectTypeException(string.Format("The media encoder setting references a file extension ({0}) not recognized by the application.", DestinationFileExtension));
			}
		}

		/// <summary>
		/// Returns a value indicating whether the <paramref name="fileExtension" /> should be 
		/// validated.
		/// </summary>
		/// <param name="fileExtension">The file extension (e.g. ".avi").</param>
		/// <returns><c>true</c> if the value should be validated; otherwise <c>false</c>.</returns>
		private bool ShouldValidate(string fileExtension)
		{
			string[] extensionsNotNeedingValidation = new[] {"*audio", "*video"};
			return Array.IndexOf(extensionsNotNeedingValidation, fileExtension) < 0;
		}

		#region IComparable

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(IMediaEncoderSettings other)
		{
			if (other == null)
				return 1;
			else
			{
				return Sequence.CompareTo(other.Sequence);
			}
		}

		#endregion
	}
}