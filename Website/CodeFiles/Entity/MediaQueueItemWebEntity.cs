using System;
using System.Diagnostics;
using GalleryServer.Business;

namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// A client-optimized object that contains information about a media queue item. This class is used to pass information between the browser and the web server.
  /// </summary>
  [DebuggerDisplay("ID {MediaQueueId}: MO ID={MediaObjectId}; Status={Status}")]
  public class MediaQueueItemWebEntity
  {
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
    public MediaQueueItemStatus StatusInt { get; set; }

    /// <summary>
    /// Specifies the status of the media object in the media object conversion queue.
    /// </summary>
    /// <value>The status.</value>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
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
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public MediaQueueItemConversionType ConversionType { get; set; }

    /// <summary>
    /// Gets or sets the amount of rotation to be applied to the media object.
    /// </summary>
    /// <value>The rotation amount.</value>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public MediaAssetRotateFlip RotationAmount { get; set; }

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
    /// Gets or sets the number of milliseconds a media asset has been processing or, if it is completed, the duration of the encoding.
    /// Will be zero for items that have not yet started processing.
    /// </summary>
    public double DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the original filename for this media asset.
    /// </summary>
    public string OriginalFilename { get; set; }

    /// <summary>
    /// Gets or sets the name of the file being created during this media queue processing.
    /// </summary>
    public string NewFilename { get; set; }

    /// <summary>
    /// Gets or sets an URL to the gallery object without the host name. The url can be assigned to the src attribute of an img tag.
    /// Ex: "/gs/handler/getmedia.ashx?moid=34&amp;dt=1&amp;g=1"
    /// </summary>
    public string ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the ID of the album containing this media queue item.
    /// </summary>
    public string MediaObjectTitle { get; set; }

    /// <summary>
    /// Gets or sets the ID of the album containing this media queue item.
    /// </summary>
   public int AlbumId { get; set; }

    /// <summary>
    /// Gets or sets the title of the album containing this media queue item.
    /// </summary>
    public string AlbumTitle { get; set; }
  }
}