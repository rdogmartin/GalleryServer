using System;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// Provides functionality for extracting metadata from an album.
	/// </summary>
	public class AlbumMetadataReadWriter : GalleryObjectMetadataReadWriter
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AlbumMetadataReadWriter" /> class.
		/// </summary>
		/// <param name="album">The album.</param>
		public AlbumMetadataReadWriter(IGalleryObject album)
			: base(album)
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
				case MetadataItemName.Title: return GetAlbumTitle();
				default:
					return base.GetMetaValue(metaName);
			}
		}

		/// <summary>
		/// Gets the album title, which is defined as the directory name, except for the root album,
		/// in which case we return the title property to preserve the original title. Returns null
		/// for new albums, since in those cases the directory name may not yet be known.
		/// </summary>
		/// <returns>An instance that implements <see cref="IMetaValue" />.</returns>
		private IMetaValue GetAlbumTitle()
		{
			var dirName = GetDirectoryName();

			if (GalleryObject.IsNew)
			{
				// For new albums we may not yet have the directory name, so return null for now.
				// Later code in the Album_Saving event will assign the directory name, which 
				// in turn will update the DirectoryName metadata property.
				return null;
			}
			else
			{
				return new MetaValue((!String.IsNullOrWhiteSpace(dirName) ? dirName : GalleryObject.Title));
			}
		}

		private string GetDirectoryName()
		{
			var album = GalleryObject as IAlbum;

			return (album != null ? album.DirectoryName : null);
		}

		#endregion
	}
}