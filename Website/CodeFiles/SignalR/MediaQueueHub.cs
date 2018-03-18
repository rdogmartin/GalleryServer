using System.Collections.Generic;
using GalleryServer.Business;

namespace GalleryServer.Web.SignalR
{
  /// <summary>
  /// Provides media queue-related methods for communicating with SignalR connections.
  /// </summary>
  public class MediaQueueHub : Microsoft.AspNet.SignalR.Hub
	{
    private readonly MediaQueueSignalR _mediaQueueSignalR;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaQueueHub"/> class.
    /// </summary>
    public MediaQueueHub() : this(MediaQueueSignalR.Instance) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaQueueHub"/> class.
    /// </summary>
    /// <param name="mediaQueueSignalR">The singleton class that provides functionality for bridging the state of media 
    /// encoding and the transient nature of SignalR hub instances..</param>
    public MediaQueueHub(MediaQueueSignalR mediaQueueSignalR)
    {
      _mediaQueueSignalR = mediaQueueSignalR;
    }

    /// <summary>
    /// Gets the current media queue. Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="Entity.MediaQueueWebEntity" />.</returns>
    public Entity.MediaQueueWebEntity GetMediaQueue()
    {
      return _mediaQueueSignalR.GetMediaQueue();
    }

    /// <summary>
    /// Gets the current media queue item. If no item is currently being processed, an instance with default properties
    /// is returned. Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="Entity.MediaQueueItemWebEntity" />.</returns>
    public Entity.MediaQueueItemWebEntity GetCurrentMediaQueueItem()
    {
      return _mediaQueueSignalR.GetCurrentMediaQueueItem();
    }

    /// <summary>
    /// Gets all media queue items having a status of <see cref="MediaQueueItemStatus.Waiting" />. Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="IEnumerable&lt;MediaQueueItemWebEntity&gt;" />.</returns>
    public IEnumerable<Entity.MediaQueueItemWebEntity> GetWaitingMediaQueueItems()
    {
      return _mediaQueueSignalR.GetWaitingMediaQueueItems();
    }

    /// <summary>
    /// Gets all media queue items NOT having a status of <see cref="MediaQueueItemStatus.Waiting" /> or <see cref="MediaQueueItemStatus.Processing" />.
    /// Guaranteed to not return null.
    /// </summary>
    /// <returns><see cref="IEnumerable&lt;MediaQueueItemWebEntity&gt;" />.</returns>
    public IEnumerable<Entity.MediaQueueItemWebEntity> GetCompleteMediaQueueItems()
    {
      return _mediaQueueSignalR.GetCompleteMediaQueueItems();
    }

    ///// <summary>
    ///// Gets all media queue items.
    ///// </summary>
    ///// <returns><see cref="IEnumerable&lt;MediaQueueItemWebEntity&gt;" />.</returns>
    //public IEnumerable<Entity.MediaQueueItemWebEntity> GetAllMediaQueueItems()
    //{
    //  return _mediaQueueSignalR.GetAllMediaQueueItems();
    //}
  }
}