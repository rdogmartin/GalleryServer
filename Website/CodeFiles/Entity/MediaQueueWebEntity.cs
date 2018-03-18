using System.Diagnostics;

namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// A client-optimized object that contains information about tje media queue. This class is used to pass information between the browser and the web server.
  /// </summary>
  [DebuggerDisplay("Status={QueueStatusText}")]
  public class MediaQueueWebEntity
  {
    /// <summary>
    /// Gets or sets the queue status. This is the integer representation of the <see cref="Business.MediaQueueStatus" /> enumeration value.
    /// </summary>
    public int QueueStatus { get; set; }

    /// <summary>
    /// Gets or sets the queue status. This is the text representation of the <see cref="Business.MediaQueueStatus" /> enumeration value.
    /// </summary>
    public string QueueStatusText { get; set; }
  }
}