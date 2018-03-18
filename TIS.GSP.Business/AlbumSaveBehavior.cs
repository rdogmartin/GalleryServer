using System;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// Provides functionality for persisting an album to the data store and file system.
  /// </summary>
  public class AlbumSaveBehavior : ISaveBehavior
  {
    readonly IAlbum _albumObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumSaveBehavior"/> class.
    /// </summary>
    /// <param name="albumObject">The album object.</param>
    public AlbumSaveBehavior(IAlbum albumObject)
    {
      this._albumObject = albumObject;
    }

    /// <summary>
    /// Persist the object to which this behavior belongs to the data store. Also persist to the file system, if
    /// the object has a representation on disk, such as albums (stored as directories) and media objects (stored
    /// as files). New objects with ID = int.MinValue will have a new <see cref="IGalleryObject.Id"/> assigned
    /// and <see cref="IGalleryObject.IsNew"/> set to false.
    /// All validation should have taken place before calling this method.
    /// </summary>
    public void Save()
    {
      if (this._albumObject.IsVirtualAlbum)
        return; // Don't save virtual albums.

      // Must save to disk first, since the method queries properties that might be updated when it is
      // saved to the data store.
      PersistToFileSystemStore(this._albumObject);

      // Save to the data store.
      //Factory.GetDataProvider().Album_Save(this._albumObject);
      using (var repo = new AlbumRepository())
      {
        repo.Save(this._albumObject);
      }

      if (this._albumObject.GalleryIdHasChanged)
      {
        // Album has been assigned to a new gallery, so we need to iterate through all
        // its children and update those gallery IDs as well.
        AssignNewGalleryId(this._albumObject.GetChildGalleryObjects(GalleryObjectType.Album), this._albumObject.GalleryId);
      }
    }

    /// <summary>
    /// Assigns the specified <paramref name="galleryId" /> to the <paramref name="albums" />,
    /// acting recursively on all child albums.
    /// </summary>
    /// <param name="albums">The albums whose gallery is to be updated.</param>
    /// <param name="galleryId">The new gallery ID.</param>
    private static void AssignNewGalleryId(IEnumerable<IGalleryObject> albums, int galleryId)
    {
      foreach (IAlbum childAlbum in albums)
      {
        childAlbum.GalleryId = galleryId;
        //Factory.GetDataProvider().Album_Save(childAlbum);
        using (var repo = new AlbumRepository())
        {
          repo.Save(childAlbum);
        }

        AssignNewGalleryId(childAlbum.GetChildGalleryObjects(GalleryObjectType.Album), galleryId);
      }
    }

    /// <summary>
    /// Update the directory on disk with the current name and location of the album. A new directory is
    /// created for new albums, and the directory is moved to the location specified by FullPhysicalPath if
    /// that property is different than FullPhysicalPathOnDisk.
    /// </summary>
    /// <param name="album">The album to persist to disk.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
    private static void PersistToFileSystemStore(IAlbum album)
    {
      if (album == null)
        throw new ArgumentNullException("album");

      if (album.IsRootAlbum)
      {
        return; // The directory for the root album is the media objects path, whose existence has already been verified by other code.
      }

      if (album.IsNew)
      {
        System.IO.Directory.CreateDirectory(album.FullPhysicalPath);

        IGallerySettings gallerySetting = Factory.LoadGallerySetting(album.GalleryId);

        // Create directory for thumbnail cache, if needed.
        string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
        if (thumbnailPath != album.FullPhysicalPath)
        {
          System.IO.Directory.CreateDirectory(thumbnailPath);
        }

        // Create directory for optimized image cache, if needed.
        string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
        if (optimizedPath != album.FullPhysicalPath)
        {
          System.IO.Directory.CreateDirectory(optimizedPath);
        }
      }
      else if (album.FullPhysicalPathOnDisk != album.FullPhysicalPath)
      {
        // We need to move the directory to its new location or change its name. Verify that the containing directory doesn't already
        // have a directory with the new name. If it does, alter it slightly to make it unique.
        System.IO.DirectoryInfo di = System.IO.Directory.GetParent(album.FullPhysicalPath);

        IGallerySettings gallerySetting = Factory.LoadGallerySetting(album.GalleryId);

        string newDirName = HelperFunctions.ValidateDirectoryName(di.FullName, album.DirectoryName, gallerySetting.DefaultAlbumDirectoryNameLength);
        if (album.DirectoryName != newDirName)
        {
          album.DirectoryName = newDirName;
        }

        // Now we are guaranteed to have a "safe" directory name, so proceed with the move/rename.
        System.IO.Directory.Move(album.FullPhysicalPathOnDisk, album.FullPhysicalPath);

        // Rename directory for thumbnail cache, if needed.
        string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
        if (thumbnailPath != album.FullPhysicalPath)
        {
          string currentThumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPathOnDisk, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);

          RenameDirectory(currentThumbnailPath, thumbnailPath);
        }

        // Rename directory for optimized image cache, if needed.
        string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
        if (optimizedPath != thumbnailPath && optimizedPath != album.FullPhysicalPath)
        {
          string currentOptimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(album.FullPhysicalPathOnDisk, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);

          RenameDirectory(currentOptimizedPath, optimizedPath);
        }

        album.FullPhysicalPathOnDisk = album.FullPhysicalPath;
      }
    }

    private static void RenameDirectory(string oldDirPath, string newDirPath)
    {
      if (System.IO.Directory.Exists(oldDirPath))
      {
        if (System.IO.Directory.Exists(newDirPath))
        {
          throw new BusinessException($"The cache directory {newDirPath} already exists, so it is not possible to rename the directory to it. You can try to manually delete this directory or perform a synchronization on the parent album, which will remove unused cache directories.");
        }

        System.IO.Directory.Move(oldDirPath, newDirPath);
      }
      else if (!System.IO.Directory.Exists(newDirPath))
      {
        System.IO.Directory.CreateDirectory(newDirPath);
      }
    }
  }
}
