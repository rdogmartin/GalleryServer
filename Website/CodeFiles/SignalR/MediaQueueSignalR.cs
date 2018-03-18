using System;
using System.Collections.Generic;
using System.Linq;
using GalleryServer.Business;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Entity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace GalleryServer.Web.SignalR
{
  /// <summary>
  /// A singleton that provides functionality for bridging the state of media encoding and the transient nature of
  /// SignalR hub instances.
  /// </summary>
  public class MediaQueueSignalR
  {
    #region Fields

    private static readonly Lazy<MediaQueueSignalR> _instance = new Lazy<MediaQueueSignalR>(() => new MediaQueueSignalR(GlobalHost.ConnectionManager.GetHubContext<MediaQueueHub>().Clients));

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaQueueSignalR"/> class.
    /// </summary>
    /// <param name="clients">An object representing all information about a SignalR connection for an <see cref="IHub" />.</param>
    private MediaQueueSignalR(IHubConnectionContext<dynamic> clients)
    {
      Clients = clients;

      // Subscribe to events thrown from MediaConversionQueue.
      MediaConversionQueue.MediaQueueStatusChanged += MediaQueueStatusChanged;
      MediaConversionQueue.MediaQueueItemAdded += MediaQueueItemAdded;
      MediaConversionQueue.MediaQueueItemStarted += MediaQueueItemStarted;
      MediaConversionQueue.ActiveMediaQueueItemUpdated += ActiveMediaQueueItemUpdated;
      MediaConversionQueue.MediaQueueItemCompleted += MediaQueueItemCompleted;
      MediaConversionQueue.MediaQueueItemDeleted += MediaQueueItemDeleted;
      MediaConversionQueue.MediaQueueItemStatusDetailAppended += MediaQueueItemStatusDetailAdded;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets an object representing all information about a SignalR connection for an <see cref="IHub" />.
    /// </summary>
    private IHubConnectionContext<dynamic> Clients { get; }

    /// <summary>
    /// Gets a reference to the <see cref="MediaQueueSignalR" /> singleton for this app domain.
    /// </summary>
    public static MediaQueueSignalR Instance
    {
      get { return _instance.Value; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the current media queue. Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="MediaQueueWebEntity" />.</returns>
    public MediaQueueWebEntity GetMediaQueue()
    {
      try
      {
        return ToMediaQueueWebEntity(MediaConversionQueue.Instance.Status);
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);
        throw;
      }
    }

    /// <summary>
    /// Gets the current media queue item. If no item is currently being processed, an instance with default properties
    /// is returned. Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="MediaQueueItemWebEntity" />.</returns>
    public MediaQueueItemWebEntity GetCurrentMediaQueueItem()
    {
      try
      {
        return ToMediaQueueItem(MediaConversionQueue.Instance.GetCurrentMediaQueueItem());
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);
        throw;
      }
    }

    /// <summary>
    /// Gets all media queue items having a status of <see cref="MediaQueueItemStatus.Waiting" />. Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="IEnumerable&lt;MediaQueueItemWebEntity&gt;" />.</returns>
    public IEnumerable<MediaQueueItemWebEntity> GetWaitingMediaQueueItems()
    {
      try
      {
        return MediaConversionQueue.Instance.MediaQueueItems
          .Where(mq => mq.Status == MediaQueueItemStatus.Waiting)
          .OrderBy(mq => mq.DateAdded)
          .Select(ToMediaQueueItem);
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);
        throw;
      }
    }

    /// <summary>
    /// Gets all media queue items NOT having a status of <see cref="MediaQueueItemStatus.Waiting" /> or <see cref="MediaQueueItemStatus.Processing" />.
    /// Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="IEnumerable&lt;MediaQueueItemWebEntity&gt;" />.</returns>
    public IEnumerable<MediaQueueItemWebEntity> GetCompleteMediaQueueItems()
    {
      try
      {
        var incompleteStatus = new[] { MediaQueueItemStatus.Waiting, MediaQueueItemStatus.Processing };

        return MediaConversionQueue.Instance.MediaQueueItems
          .OrderByDescending(mq => mq.DateConversionCompleted)
          .Where(mq => !incompleteStatus.Contains(mq.Status))
          .Select(ToMediaQueueItem);
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);
        throw;
      }
    }

    #endregion


    #region Functions

    /// <summary>
    /// Handle the event that fires when the media queue status changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MediaConversionQueueEventArgs"/> instance containing the event data.</param>
    private void MediaQueueStatusChanged(object sender, MediaConversionQueueEventArgs e)
    {
      Clients.All.mediaQueueStatusChanged(ToMediaQueueWebEntity(e.QueueStatus));
    }

    /// <summary>
    /// Handle the event that fires when a media queue item has been added to the queue. Notify any connected SignalR clients.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MediaConversionQueueEventArgs"/> instance containing the event data.</param>
    private void MediaQueueItemAdded(object sender, MediaConversionQueueEventArgs e)
    {
      Clients.All.mediaQueueItemAdded(ToMediaQueueItem(e.MediaQueueItem));
    }

    /// <summary>
    /// Handle the event that fires when a media queue item has begun processing. Notify any connected SignalR clients.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MediaConversionQueueEventArgs"/> instance containing the event data.</param>
    private void MediaQueueItemStarted(object sender, MediaConversionQueueEventArgs e)
    {
      Clients.All.mediaQueueItemStarted(ToMediaQueueItem(e.MediaQueueItem));
    }

    /// <summary>
    /// Handle the event that fires when the currently processing media queue item is updated. Notify any connected SignalR clients.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MediaConversionQueueEventArgs"/> instance containing the event data.</param>
    private void ActiveMediaQueueItemUpdated(object sender, MediaConversionQueueEventArgs e)
    {
      Clients.All.activeMediaQueueItemUpdated(ToMediaQueueItem(e.MediaQueueItem));
    }

    /// <summary>
    /// Handle the event that fires when there is new information added to the <see cref="MediaQueueItem.StatusDetail" />
    /// property. Notify any connected SignalR clients.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MediaConversionQueueEventArgs"/> instance containing the event data.</param>
    private void MediaQueueItemStatusDetailAdded(object sender, MediaConversionQueueEventArgs e)
    {
      Clients.All.addToMediaQueueItemStatusDetail(e.StatusDetailAppended);
    }

    /// <summary>
    /// Handle the event that fires when a media queue item has finished processing, either successfully or unsuccessfully.
    /// Notify any connected SignalR clients.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MediaConversionQueueEventArgs"/> instance containing the event data.</param>
    private void MediaQueueItemCompleted(object sender, MediaConversionQueueEventArgs e)
    {
      Clients.All.mediaQueueItemCompleted(ToMediaQueueItem(e.MediaQueueItem));
    }

    /// <summary>
    /// Handle the event that fires when a media queue item has been deleted.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="MediaConversionQueueEventArgs"/> instance containing the event data.</param>
    private void MediaQueueItemDeleted(object sender, MediaConversionQueueEventArgs e)
    {
      Clients.All.mediaQueueItemDeleted(e.MediaQueueItem.MediaQueueId);
    }

    /// <summary>
    /// Convert the <paramref name="queueStatus" /> to an instance of <see cref="MediaQueueWebEntity" />. Guaranteed to not be null.
    /// </summary>
    /// <param name="queueStatus">The queue status.</param>
    /// <returns>An instance of <see cref="MediaQueueWebEntity" />.</returns>
    private static MediaQueueWebEntity ToMediaQueueWebEntity(MediaQueueStatus queueStatus)
    {
      return new MediaQueueWebEntity()
      {
        QueueStatus = (int)queueStatus,
        QueueStatusText = queueStatus.ToString()
      };
    }

    /// <summary>
    /// Convert <paramref name="mqItem" /> to an instance of <see cref="MediaQueueItemWebEntity" />. Returns null if <paramref name="mqItem" /> is null.
    /// </summary>
    /// <param name="mqItem">The media queue item. May be null.</param>
    /// <returns><see cref="MediaQueueItemWebEntity" /> or null.</returns>
    private static MediaQueueItemWebEntity ToMediaQueueItem(MediaQueueItem mqItem)
    {
      if (mqItem == null)
      {
        return null;
      }

      var mo = Factory.LoadMediaObjectInstance(mqItem.MediaObjectId);

      var moBuilderOptions = MediaObjectHtmlBuilder.GetMediaObjectHtmlBuilderOptions(mo);
      moBuilderOptions.DisplayType = DisplayObjectType.Thumbnail;

      var moBuilder = new MediaObjectHtmlBuilder(moBuilderOptions);

      return new MediaQueueItemWebEntity()
      {
        MediaQueueId = mqItem.MediaQueueId,
        MediaObjectId = mqItem.MediaObjectId,
        StatusInt = mqItem.Status,
        Status = mqItem.Status,
        ConversionType = mqItem.ConversionType,
        DateAdded = mqItem.DateAdded,
        DateConversionStarted = mqItem.DateConversionStarted,
        DateConversionCompleted = mqItem.DateConversionCompleted,
        DurationMs = GetDurationMs(mqItem),
        StatusDetail = mqItem.StatusDetail,
        OriginalFilename = mo.Original.FileName,
        NewFilename = mqItem.NewFilename,
        ThumbnailUrl = GetThumbnailUrl(moBuilder),
        MediaObjectTitle = mo.Title,
        AlbumId = mo.Parent.Id,
        AlbumTitle = mo.Parent.Title
      };
    }

    /// <summary>
    /// Calculates the URL to the thumbnail image of a media asset, with the host name excluded.
    /// Ex: "/gs/handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
    /// </summary>
    /// <param name="moBuilder">An instance of <see cref="MediaObjectHtmlBuilder" />.</param>
    /// <returns>An instance of <see cref="System.String" />.</returns>
    private static string GetThumbnailUrl(MediaObjectHtmlBuilder moBuilder)
    {
      // We remove the host URL (e.g. "http://localhost/dev/gs") to avoid sending the wrong host name to a user. For example, if two users are on the
      // media queue page but on different hosts (e.g. localhost and rdog), the GetMediaObjectUrl() function below will return the same host URL for
      // both users, potentially resulting in a broken link for one of them. This is because Utils.GetHostUrl() requires an HTTP context and when one 
      // is not present (such as with SignalR), it falls back to the last known host URL (which may be for a different user).
      return moBuilder.GetMediaObjectUrl().Replace(Utils.GetHostUrl(), string.Empty);
    }

    /// <summary>
    /// Calculates the number of milliseconds a media asset spent being processed. If it is currently being processed, calculated
    /// the elapsed number of milliseconds. Returns zero for items that have not started processing.
    /// </summary>
    /// <param name="mqItem">The media queue item.</param>
    /// <returns>An instance of <see cref="System.Double" />.</returns>
    private static double GetDurationMs(MediaQueueItem mqItem)
    {
      if (!mqItem.DateConversionStarted.HasValue)
      {
        return 0;
      }

      if (!mqItem.DateConversionCompleted.HasValue)
      {
        return (DateTime.UtcNow - mqItem.DateConversionStarted.Value).TotalMilliseconds;
      }

      return (mqItem.DateConversionCompleted.Value - mqItem.DateConversionStarted.Value).TotalMilliseconds;
    }

    #endregion
  }
}