using System;
using System.IO;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// Provides functionality for creating and saving the thumbnail image files associated with <see cref="Image" /> gallery objects.
  /// </summary>
  public class ImageOptimizedCreator : DisplayObjectCreator
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageOptimizedCreator"/> class.
    /// </summary>
    /// <param name="galleryObject">The media object.</param>
    public ImageOptimizedCreator(IGalleryObject galleryObject)
    {
      GalleryObject = galleryObject;
    }

    /// <summary>
    /// Generate the file for this display object and save it to the file system. The routine may decide that
    /// a file does not need to be generated, usually because it already exists. However, it will always be
    /// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
    /// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
    /// persisted to the data store.
    /// </summary>
    /// <exception cref="UnsupportedImageTypeException">Thrown when Gallery Server cannot process the image, 
    /// most likely because it is corrupt or an unsupported image type.</exception>
    public override void GenerateAndSaveFile()
    {
      // If necessary, generate and save the optimized version of the original image.
      if (!(IsOptimizedImageRequired()))
      {
        bool rotateFlipIsRequested = (GalleryObject.RotateFlip != MediaAssetRotateFlip.NotSpecified);

        if (rotateFlipIsRequested || ((GalleryObject.IsNew) && (String.IsNullOrEmpty(GalleryObject.Optimized.FileName))))
        {
          // One of the following is true:
          // 1. The original is being rotated or flipped and there isn't a separate optimized image.
          // 2. This is a new object that doesn't need a separate optimized image.
          // In either case, set the optimized properties equal to the original properties.
          GalleryObject.Optimized.FileName = GalleryObject.Original.FileName;
          GalleryObject.Optimized.Width = GalleryObject.Original.Width;
          GalleryObject.Optimized.Height = GalleryObject.Original.Height;
          GalleryObject.Optimized.FileSizeKB = GalleryObject.Original.FileSizeKB;
        }
        return; // No optimized image required.
      }

      IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryObject.GalleryId);

      // Determine file name and path of the optimized image. If a file name has already been previously
      // calculated for this media object, re-use it. Otherwise generate a unique name.
      var newFilename = GalleryObject.Optimized.FileName;
      var newFilePath = GalleryObject.Optimized.FileNamePhysicalPath;

      if (String.IsNullOrEmpty(newFilePath))
      {
        var optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this.GalleryObject.Original.FileInfo.DirectoryName, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
        newFilename = GenerateJpegFilename(optimizedPath, gallerySetting.OptimizedFileNamePrefix);
        newFilePath = Path.Combine(optimizedPath, newFilename);
      }

      try
      {
        bool imageCreated = false;

        var size = System.Windows.Size.Empty;
        if (Array.IndexOf<string>(gallerySetting.ImageMagickFileTypes, Path.GetExtension(GalleryObject.Original.FileName).ToLowerInvariant()) >= 0)
        {
          size = GenerateImageUsingImageMagick(newFilePath, gallerySetting.MaxOptimizedLength, gallerySetting.OptimizedImageJpegQuality);

          imageCreated = !size.IsEmpty;
        }

        if (!imageCreated)
        {
          size = GenerateImageUsingDotNet(newFilePath, gallerySetting.MaxOptimizedLength, gallerySetting.OptimizedImageJpegQuality);
        }

        if (!size.IsEmpty)
        {
          GalleryObject.Optimized.Width = (int)size.Width;
          GalleryObject.Optimized.Height = (int)size.Height;
        }

        GalleryObject.Optimized.FileName = newFilename;
        GalleryObject.Optimized.FileNamePhysicalPath = newFilePath;

        int fileSize = (int)(GalleryObject.Optimized.FileInfo.Length / 1024);

        GalleryObject.Optimized.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
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

    private bool IsOptimizedImageRequired()
    {
      // We must create an optimized image in the following circumstances:
      // 1. The file corresponding to a previously created optimized image file does not exist.
      //    OR
      // 2. The overwrite flag is true.
      //    OR
      // 3. There is a request to rotate the image.
      //    AND
      // 4. The size of width/height dimensions of the original exceed the optimized triggers.
      //    OR
      // 5. The original image is not a JPEG.
      // In other words: image required = ((1 || 2 || 3) && (4 || 5))

      bool optimizedImageMissing = IsOptimizedImageFileMissing(); // Test 1

      bool overwriteFlag = GalleryObject.RegenerateOptimizedOnSave; // Test 2

      bool rotateFlipIsRequested = (GalleryObject.RotateFlip != MediaAssetRotateFlip.NotSpecified); // Test 3

      bool originalExceedsOptimizedDimensionTriggers = false;
      bool isOriginalNonJpegImage = false;
      if (optimizedImageMissing || overwriteFlag || rotateFlipIsRequested)
      {
        // Only need to run test 3 and 4 if test 1 or test 2 is true.
        originalExceedsOptimizedDimensionTriggers = DoesOriginalExceedOptimizedDimensionTriggers(); // Test 4

        isOriginalNonJpegImage = IsOriginalNonJpegImage(); // Test 5
      }

      return ((optimizedImageMissing || overwriteFlag || rotateFlipIsRequested) && (originalExceedsOptimizedDimensionTriggers || isOriginalNonJpegImage));
    }

    private bool IsOriginalNonJpegImage()
    {
      // Return true if the original image is not a JPEG.
      string[] jpegImageTypes = new string[] { ".jpg", ".jpeg" };
      string originalFileExtension = Path.GetExtension(GalleryObject.Original.FileName).ToLowerInvariant();

      bool isOriginalNonJpegImage = Array.IndexOf(jpegImageTypes, originalFileExtension) < 0;

      return isOriginalNonJpegImage;
    }

    private bool DoesOriginalExceedOptimizedDimensionTriggers()
    {
      IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryObject.GalleryId);

      // Test 1: Is the file size of the original greater than OptimizedImageTriggerSizeKB?
      bool isOriginalFileSizeGreaterThanTriggerSize = GalleryObject.Original.FileSizeKB > gallerySetting.OptimizedImageTriggerSizeKb;

      // Test 2: Is the width or length of the original greater than the MaxOptimizedLength?
      bool isOriginalLengthGreaterThanMaxAllowedLength = false;
      int optimizedMaxLength = gallerySetting.MaxOptimizedLength;
      double originalWidth = 0;
      double originalHeight = 0;

      try
      {
        var size = GalleryObject.Original.GetSize();
        originalWidth = size.Width;
        originalHeight = size.Height;
      }
      catch (UnsupportedImageTypeException) { }

      if ((originalWidth > optimizedMaxLength) || (originalHeight > optimizedMaxLength))
      {
        isOriginalLengthGreaterThanMaxAllowedLength = true;
      }

      return (isOriginalFileSizeGreaterThanTriggerSize | isOriginalLengthGreaterThanMaxAllowedLength);
    }

    private bool IsOptimizedImageFileMissing()
    {
      // Does the optimized image file exist? (Maybe it was accidentally deleted or moved by the user,
      // or maybe it's a new object.)
      return !File.Exists(GalleryObject.Optimized.FileNamePhysicalPath);
    }
  }
}
