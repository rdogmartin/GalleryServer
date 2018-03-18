using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// A collection of <see cref="IMediaEncoderSettings" /> objects.
  /// </summary>
  public class MediaEncoderSettingsCollection : IMediaEncoderSettingsCollection
  {
    // The items in the collection. We use a List<T> because it preserves the order. We'd use a ConcurrentList if
    // it existed, but it doesn't and, at any rate, there appears to be a very low to zero risk of multi-threading
    // issues given that this collection is never modified on a property shared across threads.
    private List<IMediaEncoderSettings> Items { get; } = new List<IMediaEncoderSettings>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaEncoderSettingsCollection"/> class.
    /// </summary>
    public MediaEncoderSettingsCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaEncoderSettingsCollection"/> class.
    /// </summary>
    /// <param name="encoderSettings">The encoder settings.</param>
    public MediaEncoderSettingsCollection(IEnumerable<IMediaEncoderSettings> encoderSettings)
    {
      AddRange(encoderSettings);
    }

    /// <summary>
    /// Adds the media encoder settings to the current collection.
    /// </summary>
    /// <param name="mediaEncoderSettings">The media encoder settings to add to the current collection.</param>
    public void AddRange(IEnumerable<IMediaEncoderSettings> mediaEncoderSettings)
    {
      if (mediaEncoderSettings == null)
        throw new ArgumentNullException(nameof(mediaEncoderSettings));

      foreach (var mediaEncoderSetting in mediaEncoderSettings)
      {
        Add(mediaEncoderSetting);
      }
    }

    /// <summary>
    /// Adds the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Add(IMediaEncoderSettings item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item), "Cannot add null to an existing MediaEncoderSettingsCollection. Items.Count = " + Items.Count);

      Items.Add(item);
    }

    /// <summary>
    /// Verifies the items in the collection contain valid data.
    /// </summary>
    /// <exception cref="UnsupportedMediaObjectTypeException">Thrown when one of the items references
    /// a file type not recognized by the application.</exception>
    public void Validate()
    {
      foreach (var setting in Items)
      {
        setting.Validate();
      }
    }

    /// <summary>
    /// Generates as string representation of the items in the collection. Use this to convert the collection
    /// to a form that can be stored in the gallery settings table.
    /// Example: Ex: ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}~~.avi||.flv||-i {SourceFilePath} {DestinationFilePath}"
    /// </summary>
    /// <returns>
    /// Returns a string representation of the items in the collection.
    /// </returns>
    /// <remarks>Each triple-pipe-delimited string represents an <see cref="IMediaEncoderSettings"/> in the collection.
    /// Each of these, in turn, is double-pipe-delimited to separate the properties of the instance
    /// (e.g. ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}"). The order of the items in the
    /// return value maps to the <see cref="IMediaEncoderSettings.Sequence"/>.</remarks>
    public string Serialize()
    {
      var sb = new StringBuilder();

      // Now that it is sorted, we can iterate in increasing sequence. Validate as we go along to ensure each 
      // sequence is equal to or higher than the one before.
      var lastSeq = 0;
      foreach (var encoderSetting in Items.OrderBy(mes => mes.Sequence))
      {
        if (encoderSetting.Sequence < lastSeq)
        {
          throw new BusinessException("Cannot serialize MediaEncoderSettingsCollection because the underlying collection is not in ascending sequence.");
        }

        sb.AppendFormat(CultureInfo.InvariantCulture, "{0}||{1}||{2}~~", encoderSetting.SourceFileExtension, encoderSetting.DestinationFileExtension, encoderSetting.EncoderArguments);

        lastSeq = encoderSetting.Sequence;
      }

      if (sb.Length >= 2)
      {
        sb.Remove(sb.Length - 2, 2); // Remove the final ~~
      }

      return sb.ToString();
    }

    /// <summary>
    /// Remove the items in the collection.
    /// </summary>
    public void Clear()
    {
      Items.Clear();
    }

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    /// <value>The count.</value>
    public int Count => Items.Count;

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns><see cref="System.Collections.IEnumerator" />.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns><see cref="System.Collections.Generic.IEnumerator&lt;IMediaEncoderSettings&gt;" />.</returns>
    public IEnumerator<IMediaEncoderSettings> GetEnumerator()
    {
      return Items.GetEnumerator();
    }
  }
}
