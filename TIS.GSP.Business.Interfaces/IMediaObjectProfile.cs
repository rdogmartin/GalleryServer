namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Defines an object that contains user preferences for a media object.
	/// </summary>
	public interface IMediaObjectProfile
	{
		/// <summary>
		/// Gets or sets the ID of the media object. Album IDs are not supported.
		/// </summary>
		/// <value>An <see cref="int" />.</value>
		int MediaObjectId { get; set; }

		/// <summary>
		/// Gets or sets the rating for the album or media object having ID <see cref="MediaObjectId" />.
		/// </summary>
		/// <value>A <see cref="string" />.</value>
		string Rating { get; set; }

		/// <summary>
		/// Perform a deep copy of this item.
		/// </summary>
		/// <returns>An instance of <see cref="IMediaObjectProfile" />.</returns>
		IMediaObjectProfile Copy();
	}
}