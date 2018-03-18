using System;
using System.Windows;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// Provides functionality for creating and saving the files associated with gallery objects.
  /// </summary>
  public interface IDisplayObjectCreator
  {
    /// <summary>
    /// Gets or sets the display object this instance belongs to.
    /// </summary>
    /// <value>The display object this instance belongs to.</value>
    IDisplayObject Parent { get; set; }

    /// <summary>
    /// Generate the file for this display object and save it to the file system. The routine may decide that
    /// a file does not need to be generated, usually because it already exists. However, it will always be
    /// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If 
    /// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is 
    /// persisted to the data store.
    /// </summary>
    void GenerateAndSaveFile();

    /// <summary>
    /// Gets the width and height of the specified <paramref name="displayObject" />. The value is calculated from the
    /// physical file. Returns an empty <see cref="System.Windows.Size" /> instance if the value cannot be computed or
    /// is not applicable to the object (for example, for audio files and external media objects).
    /// </summary>
    /// <param name="displayObject">The display object.</param>
    /// <returns><see cref="System.Windows.Size" />.</returns>
    Size GetSize(IDisplayObject displayObject);
  }
}
