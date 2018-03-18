using Newtonsoft.Json;

namespace GalleryServer.Business.Entity
{
  /// <summary>
  /// An entity class representing the JSON response from the license server.
  /// </summary>
  /// <remarks>
  /// The license server uses the WooCommerce Software Add-On: https://docs.woothemes.com/document/software-add-on/
  /// </remarks>
  public class LicenseServerResult
  {
    /// <summary>
    /// Gets or sets whether an activation request was successful.
    /// </summary>
    [JsonProperty(PropertyName = "activated")]
    public bool Activated { get; set; }

    /// <summary>
    /// Gets or sets whether an deactivation request was successful.
    /// </summary>
    [JsonProperty(PropertyName = "reset")]
    public bool Reset { get; set; }

    /// <summary>
    /// Gets or sets the instance ID associated with an activation.
    /// </summary>
    [JsonProperty(PropertyName = "instance")]
    public string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets a message from the license server.
    /// </summary>
    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the timestamp associated with the license server request. It is unknown what format the timestamp is in.
    /// </summary>
    [JsonProperty(PropertyName = "timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the signature associated with the license server request.
    /// </summary>
    [JsonProperty(PropertyName = "sig")]
     public string Signature { get; set; }

    /// <summary>
    /// Gets or sets the error message associated with the license server request.
    /// </summary>
    [JsonProperty(PropertyName = "error")]
     public string Error { get; set; }

    /// <summary>
    /// Gets or sets the code associated with the license server request.
    /// </summary>
    [JsonProperty(PropertyName = "code")]
     public string Code { get; set; }
 }
}