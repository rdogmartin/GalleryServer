
using System;
using System.Collections.Generic;
using System.Linq;
using GalleryServer.Data;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a media object that is queued for some kind of processing, such as transcoding a video.
  /// </summary>
  public class MediaQueueItem
  {
    #region Properties

    /// <summary>
    /// Gets or sets the media queue ID.
    /// </summary>
    /// <value>The media queue ID.</value>
    public int MediaQueueId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the media object this queue item applies to.
    /// </summary>
    /// <value>The media object ID.</value>
    public int MediaObjectId { get; set; }

    /// <summary>
    /// Specifies the status of the media object in the media object conversion queue.
    /// </summary>
    /// <value>The status.</value>
    public MediaQueueItemStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the status detail.
    /// </summary>
    /// <value>The status detail.</value>
    public string StatusDetail { get; set; }

    /// <summary>
    /// Specifies the type of processing to be executed on a media object in the media object conversion queue.
    /// </summary>
    /// <value>The type of the conversion.</value>
    public MediaQueueItemConversionType ConversionType { get; set; }

    /// <summary>
    /// Gets or sets the amount of rotation or flipping to be applied to the media object.
    /// </summary>
    /// <value>The rotation amount.</value>
    public MediaAssetRotateFlip RotateFlipAmount { get; set; }

    /// <summary>
    /// Gets or sets the date and time this queue item was created.
    /// </summary>
    /// <value>The date added.</value>
    public DateTime DateAdded { get; set; }

    /// <summary>
    /// Gets or sets the date and time processing began on this queue item.
    /// </summary>
    /// <value>The date conversion started.</value>
    public DateTime? DateConversionStarted { get; set; }

    /// <summary>
    /// Gets or sets the date and time processing finished on this queue item.
    /// </summary>
    /// <value>The date conversion completed.</value>
    public DateTime? DateConversionCompleted { get; set; }

    /// <summary>
    /// Gets or sets the name of the file being created during this media queue processing.
    /// </summary>
    public string NewFilename { get; set; }

    #endregion


    #region Methods

    /// <summary>
    /// Saves this current state of this instance to the repository.
    /// </summary>
    public void Save()
    {
      var mediaQueueDto = ToMediaQueueDto(this);

      using (var repo = new MediaQueueRepository())
      {
        repo.Upsert(mediaQueueDto, mq => mq.MediaQueueId == int.MinValue);
        repo.Save();
      }

      MediaQueueId = mediaQueueDto.MediaQueueId;
    }

    /// <summary>
    /// Permanently deletes this queue item from the repository.
    /// </summary>
    public void Delete()
    {
      using (var repo = new MediaQueueRepository())
      {
        var queueDto = repo.Find(MediaQueueId);
        if (queueDto != null)
        {
          repo.Delete(queueDto);
          repo.Save();
        }
      }
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Converts the <paramref name="item" /> to an instance of <see cref="MediaQueueDto" />.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>An instance of <see cref="MediaQueueDto" />.</returns>
    private static MediaQueueDto ToMediaQueueDto(MediaQueueItem item)
    {
      return new MediaQueueDto
               {
                 MediaQueueId = item.MediaQueueId,
                 FKMediaObjectId = item.MediaObjectId,
                 Status = item.Status.ToString(),
                 StatusDetail = item.StatusDetail,
                 ConversionType = item.ConversionType,
                 RotationAmount = item.RotateFlipAmount,
                 DateAdded = item.DateAdded,
                 DateConversionStarted = item.DateConversionStarted,
                 DateConversionCompleted = item.DateConversionCompleted
               };
    }

    /// <summary>
    /// Converts the <paramref name="item" /> to an instance of <see cref="MediaQueueItem" />.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>An instance of <see cref="MediaQueueItem" />.</returns>
    private static MediaQueueItem ToMediaQueueItem(MediaQueueDto item)
    {
      return new MediaQueueItem
               {
                 MediaQueueId = item.MediaQueueId,
                 MediaObjectId = item.FKMediaObjectId,
                 Status = Enum<MediaQueueItemStatus>.Parse(item.Status),
                 StatusDetail = item.StatusDetail,
                 ConversionType = item.ConversionType,
                 RotateFlipAmount = item.RotationAmount,
                 DateAdded = item.DateAdded,
                 DateConversionStarted = item.DateConversionStarted,
                 DateConversionCompleted = item.DateConversionCompleted
               };
    }

    /// <summary>
    /// Converts the <paramref name="mediaQueueDtos" /> to an enumerable collection of <see cref="MediaQueueItem" /> instances.
    /// </summary>
    /// <param name="mediaQueueDtos">The media queue DTO instances.</param>
    /// <returns>IEnumerable{MediaQueueItem}.</returns>
    public static IEnumerable<MediaQueueItem> ToMediaQueueItems(IEnumerable<MediaQueueDto> mediaQueueDtos)
    {
      return mediaQueueDtos.Select(ToMediaQueueItem);
    }

    #endregion
  }
}
