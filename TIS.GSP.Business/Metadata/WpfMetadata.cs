using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
  /// <summary>
  /// Contains functionality for interacting with a file's metadata through the WPF classes.
  /// Essentially it is a wrapper for the <see cref="BitmapMetadata" /> class.
  /// </summary>
  internal class WpfMetadata : IWpfMetadata
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="WpfMetadata" /> class.
    /// </summary>
    /// <param name="galleryObject">An object containing the metadata.</param>
    public WpfMetadata(IGalleryObject galleryObject)
    {
      GalleryObject = galleryObject;

      if (MetaExtractionMethod == MetadataExtractionMethod.BitmapDecoder || MetaExtractionMethod == MetadataExtractionMethod.Both)
      {
        Metadata = GetBitmapMetadataUsingBitmapDecoderTechnique();
      }

      ExtractMetadata();
    }

    #endregion

    #region Properties / Enums

    /// <summary>
    /// Defines a technique for extracting metadata from an image file.
    /// </summary>
    private enum MetadataExtractionMethod
    {
      BitmapDecoder,
      BitmapFrame,
      Both
    }

    /// <summary>
    /// Gets the technique we use to extract the metadata. Versions earlier than 4.0 used the BitmapDecoder method, but when adding
    /// support for extracting meta from NEF files in the 4.0 release, I discovered a BitmapFrame technique that is faster, provides 
    /// access to the NEF data (which BitmapDecoder did not), and appears to extract all the same data as BitmapDecoder. However,
    /// rather than replace the old technique with the new one, I added this switch to revert back to the old one if desired or even 
    /// to use both. This allows for easier investigation into user reports should they discover a problem with the new technique.
    /// </summary>
    /// <value>The meta extraction method.</value>
    private static MetadataExtractionMethod MetaExtractionMethod => MetadataExtractionMethod.Both;

    /// <summary>
    /// Gets the metadata for the <see cref="GalleryObject" />. This property is initialized in the constructor and lasts for the lifetime of
    /// the instance. Note that it will be null when <see cref="MetaExtractionMethod" /> is <see cref="MetadataExtractionMethod.BitmapDecoder" />
    /// or when an unexpected error occurs while retrieving it.
    /// </summary>
    /// <value>An instance of <see cref="BitmapMetadata" /> or null.</value>
    private BitmapMetadata Metadata { get; }

    /// <summary>
    /// Gets the gallery object containing the metadata to retrieve.
    /// </summary>
    /// <value>An instance of <see cref="IGalleryObject" />.</value>
    private IGalleryObject GalleryObject { get; }

    /// <summary>
    /// Gets a value that indicates the date that the image was taken.
    /// </summary>
    /// <value>A string.</value>
    public string DateTaken
    {
      get; private set;
    }

    /// <summary>
    /// Gets a value that indicates the title of an image file.
    /// </summary>
    /// <value>A string.</value>
    public string Title
    {
      get; private set;
    }

    /// <summary>
    /// Gets a value that indicates the author of an image.
    /// </summary>
    /// <value>A collection.</value>
    public ReadOnlyCollection<string> Author
    {
      get; private set;
    }

    /// <summary>
    /// Gets a value that identifies the camera model that was used to capture the image.
    /// </summary>
    /// <value>A string.</value>
    public string CameraModel
    {
      get; private set;
    }

    /// <summary>
    /// Gets a value that identifies the camera manufacturer that is associated with an image.
    /// </summary>
    /// <value>A string.</value>
    public string CameraManufacturer
    {
      get; private set;
    }

    /// <summary>
    /// Gets a collection of keywords that describe the image.
    /// </summary>
    /// <value>A collection.</value>
    public ReadOnlyCollection<string> Keywords
    {
      get; private set;
    }

    /// <summary>
    /// Gets a value that identifies the image rating.
    /// </summary>
    /// <value>An integer.</value>
    public int Rating
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets a value that identifies a comment that is associated with an image.
    /// </summary>
    /// <value>A string.</value>
    public string Comment
    {
      get; private set;
    }

    /// <summary>
    /// Gets a value that identifies copyright information that is associated with an image.
    /// </summary>
    /// <value>A string.</value>
    public string Copyright
    {
      get; private set;
    }

    /// <summary>
    /// Gets a value that indicates the subject matter of an image.
    /// </summary>
    /// <value>A string.</value>
    public string Subject
    {
      get; private set;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Provides access to a metadata query reader that can extract metadata from a bitmap image file.
    /// </summary>
    /// <param name="query">Identifies the string that is being queried in the current object.</param>
    /// <returns>The metadata at the specified query location.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    public object GetQuery(string query)
    {
      object queryResult = null;

      if (Metadata != null)
      {
        queryResult = Metadata.GetQuery(query);
      }

      if (queryResult == null && (MetaExtractionMethod == MetadataExtractionMethod.BitmapFrame || MetaExtractionMethod == MetadataExtractionMethod.Both))
      {
        using (var fs = GetImageFileStream())
        {
          return (fs != null ? GetBitmapMetadataUsingBitmapFrameTechnique(fs)?.GetQuery(query) : null);
        }
      }

      return queryResult;
    }

    #endregion

    #region Functions

    /// <summary>
    /// Extracts the metadata in the <see cref="GalleryObject" /> and store in member properties.
    /// </summary>
    private void ExtractMetadata()
    {
      switch (MetaExtractionMethod)
      {
        case MetadataExtractionMethod.BitmapDecoder:
          try
          {
            Author = Metadata?.Author;
            CameraManufacturer = Metadata?.CameraManufacturer;
            CameraModel = Metadata?.CameraModel;
            Comment = Metadata?.Comment;
            Copyright = Metadata?.Copyright;
            DateTaken = Metadata?.DateTaken;
            Keywords = Metadata?.Keywords;
            Rating = Metadata?.Rating ?? 0;
            Subject = Metadata?.Subject;
            Title = Metadata?.Title;
          }
          catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
          catch (ArgumentException) { }
          catch (InvalidOperationException) { }

          break;

          case MetadataExtractionMethod.BitmapFrame: // When it's BitmapFrame, the Metadata property will be null so the following code correctly uses the BitmapFrame technique
          case MetadataExtractionMethod.Both:

            using (var fs = GetImageFileStream())
            {
              var bmpMetadataAlt = GetBitmapMetadataUsingBitmapFrameTechnique(fs);

              try
              {
                Author = Metadata?.Author ?? bmpMetadataAlt?.Author;
                CameraManufacturer = Metadata?.CameraManufacturer ?? bmpMetadataAlt?.CameraManufacturer;
                CameraModel = Metadata?.CameraModel ?? bmpMetadataAlt?.CameraModel;
                Comment = Metadata?.Comment ?? bmpMetadataAlt?.Comment;
                Copyright = Metadata?.Copyright ?? bmpMetadataAlt?.Copyright;
                DateTaken = Metadata?.DateTaken ?? bmpMetadataAlt?.DateTaken;
                Keywords = Metadata?.Keywords ?? bmpMetadataAlt?.Keywords;
                Rating = Metadata?.Rating ?? bmpMetadataAlt?.Rating ?? 0;
                Subject = Metadata?.Subject ?? bmpMetadataAlt?.Subject;
                Title = Metadata?.Title ?? bmpMetadataAlt?.Title;
              }
              catch (NotSupportedException) { } // Some image types, such as png, throw a NotSupportedException. Let's swallow them and move on.
              catch (ArgumentException) { }
              catch (InvalidOperationException) { }
            }

          break;
      }
    }

    /// <summary>
    /// Gets an instance of <see cref="BitmapMetadata" /> for the current <see cref="GalleryObject" /> using a technique that uses a
    /// <see cref="BitmapDecoder" />. This method allows us to get a reference that lasts the lifetime of the instance without locking
    /// the image file or hanging on to unnecessary memory. It's disadvantage is that it is unable to access metadata in some file
    /// types such as NEF. Returns null if something unexpected happens (most common exceptions are silently swallowed).
    /// </summary>
    /// <returns>An instance of <see cref="BitmapMetadata" /> or null.</returns>
    /// <remarks>This function is no longer used beginning with Gallery Server 4.0, but remains so that one can easily switch back to it if 
    /// desired. <see cref="MetaExtractionMethod"/></remarks>
    private BitmapMetadata GetBitmapMetadataUsingBitmapDecoderTechnique()
    {
      BitmapDecoder fileBitmapDecoder = GetBitmapDecoderReader();

      if ((fileBitmapDecoder == null) || (fileBitmapDecoder.Frames.Count == 0))
        return null;

      BitmapFrame fileFirstFrame = fileBitmapDecoder.Frames[0];

      BitmapDecoder firstFrameBitmapDecoder = fileFirstFrame?.Decoder;

      if (firstFrameBitmapDecoder == null || (firstFrameBitmapDecoder.Frames.Count == 0))
        return null;

      BitmapFrame firstFrameInDecoderInFirstFrameOfFile = firstFrameBitmapDecoder.Frames[0];

      // The Metadata property is of type ImageMetadata, so we must cast it to BitmapMetadata.
      return firstFrameInDecoderInFirstFrameOfFile.Metadata as BitmapMetadata;
    }

    /// <summary>
    /// Gets an instance of <see cref="BitmapDecoder" /> for the current <see cref="GalleryObject" />.
    /// </summary>
    /// <returns>An instance of <see cref="BitmapDecoder" />.</returns>
    /// <remarks>This function is no longer used beginning with Gallery Server 4.0, but remains so that one can easily switch back to it if 
    /// desired. <see cref="MetaExtractionMethod"/></remarks>
    private BitmapDecoder GetBitmapDecoderReader()
    {
      var imageFilePath = GalleryObject.Original.FileNamePhysicalPath;
      BitmapDecoder fileBitmapDecoder = null;

      // Do not use the BitmapCacheOption.Default or None option, as it will hold a lock on the file until garbage collection. I discovered
      // this problem and it has been submitted to MS as a bug. See thread in the managed newsgroup:
      // http://www.microsoft.com/communities/newsgroups/en-us/default.aspx?dg=microsoft.public.dotnet.framework&tid=b694ada2-10c4-4999-81f8-97295eb024a9&cat=en_US_a4ab6128-1a11-4169-8005-1d640f3bd725&lang=en&cr=US&sloc=en-us&m=1&p=1
      // Also do not use BitmapCacheOption.OnLoad as suggested in the thread, as it causes the memory to not be released until 
      // eventually IIS crashes when you do things like synchronize 100 images.
      // BitmapCacheOption.OnDemand seems to be the only option that doesn't lock the file or crash IIS.
      // Update 2007-07-29: OnDemand seems to also lock the file. There is no good solution! Acckkk
      // Update 2007-08-04: After installing VS 2008 beta 2, which also installs .NET 2.0 SP1, I discovered that OnLoad no longer crashes IIS.
      // Update 2008-05-19: The Create method doesn't release the file lock when an exception occurs, such as when the file is a WMF. See:
      // http://www.microsoft.com/communities/newsgroups/en-us/default.aspx?dg=microsoft.public.dotnet.framework&tid=fe3fb82f-0191-40a3-b789-0602cc4445d3&cat=&lang=&cr=&sloc=&p=1
      // Bug submission: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=344914
      // The workaround is to use a different overload of Create that takes a FileStream.

      try
      {
        using (Stream stream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          try
          {
            fileBitmapDecoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            // DO NOT USE: fileBitmapDecoder = BitmapDecoder.Create(new Uri(imageFilePath, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
          }
          catch (NotSupportedException) { } // Thrown by some file types such as wmf
          catch (InvalidOperationException) { } // Reported by some users
          catch (ArgumentException) { } // Reported by some users
          catch (FileFormatException) { } // Reported by some users
          catch (IOException) { } // Reported by some users
          catch (OverflowException) { } // Reported by some users
          catch (OutOfMemoryException)
          {
            // The garbage collector will automatically run to try to clean up memory, so let's wait for it to finish and 
            // try again. If it still doesn't work because the image is just too large and the system doesn't have enough
            // memory, just give up.
            GC.WaitForPendingFinalizers();
            try
            {
              fileBitmapDecoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            catch (NotSupportedException) { }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (OutOfMemoryException) { }
          }
          catch (Exception ex)
          {
            if (!ex.Data.Contains("Note"))
              ex.Data.Add("Note", "This error was silently handled by the application and did not cause user disruption.");

            if (!ex.Data.Contains("Image File path"))
              ex.Data.Add("Image File path", imageFilePath);

            Events.EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
          }
        }
      }
      catch (FileNotFoundException) { } // Return null if file not found
      catch (DirectoryNotFoundException) { } // Return null if directory not found
      catch (IOException) { } // Return null if IO problem occurs

      return fileBitmapDecoder;
    }

    /// <summary>
    /// Gets a read-only <see cref="FileStream" /> for the current <see cref="GalleryObject" />. The caller must dispose the return
    /// value. Returns null if the file or directory does not exist or another unexpected <see cref="IOException" /> occurs.
    /// </summary>
    /// <returns>An instance of <see cref="FileStream" /> or null if an unexpected error occurs.</returns>
    private FileStream GetImageFileStream()
    {
      FileStream fs = null;
      try
      {
        fs = new FileStream(this.GalleryObject.Original.FileNamePhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
      }
      catch (FileNotFoundException) { } // Return null if file not found
      catch (DirectoryNotFoundException) { } // Return null if directory not found
      catch (IOException) { } // Return null if IO problem occurs

      return fs;
    }

    /// <summary>
    /// Gets an instance of <see cref="BitmapMetadata" /> from the specified <paramref name="stream" /> using a technique that uses the
    /// <see cref="BitmapFrame" /> class. This technique allows one to access metadata that can't be retrieved through 
    /// <see cref="GetBitmapMetadataUsingBitmapDecoderTechnique" />, such as NEF files. Returns null if something unexpected happens 
    /// (most common exceptions are silently swallowed).
    /// </summary>
    /// <param name="stream">A read-only stream corresponding to the original image file of the <see cref="GalleryObject" />.</param>
    /// <returns>An instance of <see cref="BitmapMetadata" /> or null.</returns>
    private BitmapMetadata GetBitmapMetadataUsingBitmapFrameTechnique(Stream stream)
    {
      BitmapMetadata bmpMetadata = null;
      try
      {
        // Tests showed that using BitmapCacheOption.OnLoad resulted in awful performance. BitmapCacheOption.Default took about 20% longer
        bmpMetadata = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None).Metadata as BitmapMetadata;
      }
      catch (NotSupportedException) { } // Thrown by some file types such as wmf
      catch (InvalidOperationException) { } // Reported by some users
      catch (ArgumentException) { } // Reported by some users
      catch (FileFormatException) { } // Reported by some users
      catch (IOException) { } // Reported by some users
      catch (OverflowException) { } // Reported by some users
      catch (OutOfMemoryException)
      {
        // The garbage collector will automatically run to try to clean up memory, so let's wait for it to finish and 
        // try again. If it still doesn't work because the image is just too large and the system doesn't have enough
        // memory, just give up.
        GC.WaitForPendingFinalizers();
        try
        {
          bmpMetadata = BitmapFrame.Create(stream).Metadata as BitmapMetadata;
        }
        catch (NotSupportedException) { }
        catch (InvalidOperationException) { }
        catch (ArgumentException) { }
        catch (OutOfMemoryException) { }
      }
      catch (Exception ex)
      {
        if (!ex.Data.Contains("Note"))
          ex.Data.Add("Note", "This error was silently handled by the application and did not cause user disruption.");

        if (!ex.Data.Contains("Image File path"))
          ex.Data.Add("Image File path", GalleryObject.Original.FileNamePhysicalPath);

        Events.EventController.RecordError(ex, AppSetting.Instance, GalleryObject.GalleryId, Factory.LoadGallerySettings());
      }

      return bmpMetadata;
    }

    #endregion

  }
}