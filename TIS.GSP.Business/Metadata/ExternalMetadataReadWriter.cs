using System;
using System.Text.RegularExpressions;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// Provides functionality for reading and writing metadata to or from an external media object.
	/// </summary>
	public class ExternalMetadataReadWriter : MediaObjectMetadataReadWriter
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ExternalMetadataReadWriter" /> class.
		/// </summary>
		/// <param name="galleryObject">The gallery object.</param>
		public ExternalMetadataReadWriter(IGalleryObject galleryObject)
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
				case MetadataItemName.HtmlSource: return GetHtmlContent();
				default:
					return base.GetMetaValue(metaName);
			}
		}

		#endregion

		#region Functions

		private IMetaValue GetHtmlContent()
		{
			return new MetaValue(GalleryObject.Original.ExternalHtmlSource);
		}

		#endregion
	}
}