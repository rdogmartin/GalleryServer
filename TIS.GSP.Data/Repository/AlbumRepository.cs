using System;
using System.Collections.Generic;
using System.Linq;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the Album table.
  /// </summary>
  public class AlbumRepository : Repository<GalleryDb, AlbumDto>
	{

		/// <summary>
		/// Persist the specified album to the data store. Return the ID of the album.
		/// </summary>
		/// <param name="album">An instance of <see cref="IAlbum"/> to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the album. If this is a new album and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public void Save(IAlbum album)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			if (album.IsNew)
			{
				var aDto = new AlbumDto
										 {
											 FKGalleryId = album.GalleryId,
											 FKAlbumParentId = (album.Parent.Id > 0 ? album.Parent.Id : new int?()),
											 DirectoryName = album.DirectoryName,
											 ThumbnailMediaObjectId = album.Thumbnail.MediaObjectId,
											 SortByMetaName = album.SortByMetaName,
											 SortAscending = album.SortAscending,
											 Seq = album.Sequence,
											 CreatedBy = album.CreatedByUserName,
											 DateAdded = album.DateAdded,
											 LastModifiedBy = album.LastModifiedByUserName,
											 DateLastModified = album.DateLastModified,
											 OwnedBy = album.OwnerUserName,
											 OwnerRoleName = album.OwnerRoleName,
											 IsPrivate = album.IsPrivate
										 };

				Add(aDto);
				Save();

				if (album.Id != aDto.AlbumId)
					album.Id = aDto.AlbumId;

				// Insert metadata items, if any, into metadata table.
				var repo = new MetadataRepository(Context); // Don't put in using construct because we don't want our Context disposed
				repo.Save(album.MetadataItems);
			}
			else
			{
				AlbumDto aDto = Find(album.Id);

				if (aDto != null)
				{
					aDto.FKGalleryId = album.GalleryId;
					aDto.FKAlbumParentId = (album.Parent.Id > 0 ? album.Parent.Id : new int?());
					aDto.DirectoryName = album.DirectoryName;
					aDto.ThumbnailMediaObjectId = album.ThumbnailMediaObjectId;
					aDto.SortByMetaName = album.SortByMetaName;
					aDto.SortAscending = album.SortAscending;
					aDto.Seq = album.Sequence;
					aDto.LastModifiedBy = album.LastModifiedByUserName;
					aDto.DateLastModified = album.DateLastModified;
					aDto.OwnedBy = album.OwnerUserName;
					aDto.OwnerRoleName = album.OwnerRoleName;
					aDto.IsPrivate = album.IsPrivate;

					Save();

					// Update metadata items, if necessary, in metadata table.
					var repo = new MetadataRepository(Context); // Don't put in using construct because we don't want our Context disposed
					repo.Save(album.MetadataItems);
				}
			}
		}

		/// <summary>
		/// Permanently delete the specified album from the data store, including any
		/// child albums, media objects, and metadata. This action cannot be undone.
		/// </summary>
		/// <param name="album">The <see cref="IAlbum"/> to delete from the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public void Delete(IAlbum album)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			// Get a list of this album and all its child albums. Then delete.
			var albumIds = new List<int> { album.Id };
			albumIds.AddRange(GetChildAlbumIds(album));

			var repo = new MetadataRepository(Context);
			foreach (var aDto in Where(a => albumIds.Contains(a.AlbumId), m => m.Metadata))
			{
				// We must explicitly delete the album metadata because we don't have a cascade
				// delete relation between them.
				foreach (var mDto in aDto.Metadata.ToList())
				{
					repo.Delete(mDto);
				}

				// The media objects belonging to this album will be automatically deleted through
				// the cascade delete defined in the database.
				Delete(aDto);
			}

			Save();

			var tagRepo = new TagRepository(Context);
			tagRepo.DeleteUnusedTags();
		}

		/// <summary>
		/// Gets the IDs of the child albums of the specified <paramref name="album" />, acting recursively.
		/// </summary>
		/// <param name="album">The album.</param>
		/// <returns>Returns an enumerable list of album ID values.</returns>
		private static IEnumerable<int> GetChildAlbumIds(IAlbum album)
		{
			var albumIds = new List<int>();

			foreach (IGalleryObject childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
			{
				albumIds.Add(childAlbum.Id);
				albumIds.AddRange(GetChildAlbumIds((IAlbum)childAlbum));
			}

			return albumIds;
		}
	}
}