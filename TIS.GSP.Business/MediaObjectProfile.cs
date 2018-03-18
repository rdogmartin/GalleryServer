using GalleryServer.Business.Interfaces;
using Newtonsoft.Json;

namespace GalleryServer.Business
{
	/// <summary>
	/// Defines an object that contains user preferences for a media object.
	/// </summary>
	public class MediaObjectProfile : IMediaObjectProfile
	{
		#region Properties

		/// <summary>
		/// Gets or sets the ID of the media object. Album IDs are not supported.
		/// </summary>
		/// <value>An <see cref="int" />.</value>
		[JsonProperty(PropertyName = "Id")]
		public int MediaObjectId { get; set; }

		/// <summary>
		/// Gets or sets the rating for the album or media object having ID <see cref="MediaObjectId" />.
		/// </summary>
		/// <value>A <see cref="string" />.</value>
		public string Rating { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectProfile" /> class.
		/// </summary>
		public MediaObjectProfile()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaObjectProfile" /> class.
		/// </summary>
		/// <param name="mediayObjectId">The mediay object ID.</param>
		/// <param name="rating">The rating.</param>
		public MediaObjectProfile(int mediayObjectId, string rating)
		{
			MediaObjectId = mediayObjectId;
			Rating = rating;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Perform a deep copy of this item.
		/// </summary>
		/// <returns>An instance of <see cref="IMediaObjectProfile" />.</returns>
		public IMediaObjectProfile Copy()
		{
			return Factory.CreateMediaObjectProfile(MediaObjectId, Rating);
		}

		#endregion
	}
}
