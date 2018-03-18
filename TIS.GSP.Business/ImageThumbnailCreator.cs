using System;
using System.IO;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// Provides functionality for creating and saving the thumbnail image files associated with <see cref="Image" /> gallery objects.
  /// </summary>
  public class ImageThumbnailCreator : DisplayObjectCreator
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageThumbnailCreator"/> class.
    /// </summary>
    /// <param name="imageObject">The image object.</param>
    public ImageThumbnailCreator(Image imageObject)
    {
      this.GalleryObject = imageObject;
    }

    /// <summary>
    /// Generate the file for this display object and save it to the file system. The routine may decide that
    /// a file does not need to be generated, usually because it already exists. However, it will always be
    /// created if the relevant flag is set on the parent IGalleryObject. (Example: If
    /// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will 
    /// always be created.) No data is persisted to the data store.
    /// </summary>
    /// <exception cref="UnsupportedImageTypeException">Thrown when Gallery Server cannot process the image, 
    /// most likely because it is corrupt or an unsupported image type.</exception>
    public override void GenerateAndSaveFile()
    {
      // If necessary, generate and save the thumbnail version of the original image.
      if (!(IsThumbnailImageRequired()))
      {
        return; // No thumbnail image required.
      }

      var gallerySetting = GallerySettings;

      // Determine file name and path of the thumbnail image. If a file name has already been previously
      // calculated for this media object, re-use it. Otherwise generate a unique name.
      var newFilename = GalleryObject.Thumbnail.FileName;
      var newFilePath = GalleryObject.Thumbnail.FileNamePhysicalPath;

      if (String.IsNullOrEmpty(newFilePath))
      {
        var thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this.GalleryObject.Original.FileInfo.DirectoryName, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
        newFilename = GenerateJpegFilename(thumbnailPath, gallerySetting.ThumbnailFileNamePrefix);
        newFilePath = Path.Combine(thumbnailPath, newFilename);
      }

      try
      {
        var imageCreated = false;

        var size = System.Windows.Size.Empty;
        if (Array.IndexOf<string>(gallerySetting.ImageMagickFileTypes, Path.GetExtension(GalleryObject.Original.FileName).ToLowerInvariant()) >= 0)
        {
          size = GenerateImageUsingImageMagick(newFilePath, gallerySetting.MaxThumbnailLength, gallerySetting.ThumbnailImageJpegQuality);

          imageCreated = !size.IsEmpty;
        }

        if (!imageCreated)
        {
          size = GenerateImageUsingDotNet(newFilePath, gallerySetting.MaxThumbnailLength, gallerySetting.ThumbnailImageJpegQuality);
        }

        if (!size.IsEmpty)
        {
          GalleryObject.Thumbnail.Width = (int)size.Width;
          GalleryObject.Thumbnail.Height = (int)size.Height;
        }

        GalleryObject.Thumbnail.FileName = newFilename;
        GalleryObject.Thumbnail.FileNamePhysicalPath = newFilePath;

        int fileSize = (int)(GalleryObject.Thumbnail.FileInfo.Length / 1024);

        GalleryObject.Thumbnail.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
      }
      catch (Exception ex)
      {
        if (!string.IsNullOrEmpty(newFilePath) && !ex.Data.Contains("File path"))
        {
          ex.Data.Add("File path", newFilePath);
        }

        throw;
      }
    }

    private bool IsThumbnailImageRequired()
    {
      // We must create a thumbnail image in the following circumstances:
      // 1. The file corresponding to a previously created thumbnail image file does not exist.
      //    OR
      // 2. The overwrite flag is true.
      //    OR
      // 3. There is a request to rotate the image.

      bool thumbnailImageMissing = IsThumbnailImageFileMissing(); // Test 1

      bool overwriteFlag = GalleryObject.RegenerateThumbnailOnSave; // Test 2

      bool rotateIsRequested = (GalleryObject.RotateFlip != MediaAssetRotateFlip.NotSpecified);

      return (thumbnailImageMissing || overwriteFlag || rotateIsRequested);
    }

    private bool IsThumbnailImageFileMissing()
    {
      // Does the thumbnail image file exist? (Maybe it was accidentally deleted or moved by the user,
      // or maybe it's a new object.)
      bool thumbnailImageExists = false;
      if (File.Exists(GalleryObject.Thumbnail.FileNamePhysicalPath))
      {
        // Thumbnail image file exists.
        thumbnailImageExists = true;
      }

      bool thumbnailImageIsMissing = !thumbnailImageExists;

      return thumbnailImageIsMissing;
    }
  }
}
