using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// Provides functionality for reading and writing metadata to or from a generic gallery object.
	/// </summary>
	public class GenericMetadataReadWriter : MediaObjectMetadataReadWriter
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericMetadataReadWriter" /> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public GenericMetadataReadWriter(IGalleryObject galleryObject)
			: base(galleryObject)
		{
		}

		#endregion

		#region Methods

		// NOTE: To perform metadata extraction that applies only to generic media objects,
		// uncomment the following and write the desired code.

		///// <summary>
		///// Gets the metadata value for the specified <paramref name="metaName" />.
		///// </summary>
		///// <param name="metaName">Name of the metadata item to retrieve.</param>
		///// <returns>Returns a string.</returns>
		//public override string GetMetaValue(MetadataItemName metaName)
		//{
		//	return base.GetMetaValue(metaName);
		//}

		#endregion
	}
}