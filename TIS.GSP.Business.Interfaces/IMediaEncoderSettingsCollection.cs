using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IMediaEncoderSettings" /> objects.
	/// </summary>
	public interface IMediaEncoderSettingsCollection : IEnumerable<IMediaEncoderSettings>
	{
		/// <summary>
		/// Adds the media encoder settings to the current collection.
		/// </summary>
		/// <param name="mediaEncoderSettings">The media encoder settings to add to the current collection.</param>
		void AddRange(System.Collections.Generic.IEnumerable<IMediaEncoderSettings> mediaEncoderSettings);

		/// <summary>
		/// Adds the specified gallery control settings.
		/// </summary>
		/// <param name="item">The gallery control settings to add.</param>
		void Add(IMediaEncoderSettings item);

		/// <summary>
		/// Verifies the items in the collection contain valid data.
		/// </summary>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when one of the items references 
		/// a file type not recognized by the application.</exception>
		void Validate();

		/// <summary>
		/// Generates as string representation of the items in the collection. Use this to convert the collection 
		/// to a form that can be stored in the gallery settings table.
		/// Example: Ex: ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}~~.avi||.flv||-i {SourceFilePath} {DestinationFilePath}"
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		/// <remarks>Each triple-pipe-delimited string represents an <see cref="IMediaEncoderSettings" /> in the collection.
		/// Each of these, in turn, is double-pipe-delimited to separate the properties of the instance 
		/// (e.g. ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}"). The order of the items in the 
		/// return value maps to the <see cref="IMediaEncoderSettings.Sequence" />.</remarks>
		string Serialize();

    /// <summary>
    /// Remove the items in the collection.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    /// <value>The count.</value>
    int Count { get; }
  }
}
