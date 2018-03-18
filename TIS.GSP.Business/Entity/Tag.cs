using System.Diagnostics;
using Newtonsoft.Json;

namespace GalleryServer.Business.Entity
{
  /// <summary>
  /// A client-optimized object representing a tag or person.
  /// </summary>
  [DebuggerDisplay("Value=\"{Value}\": Count={Count}")]
  public class Tag
  {
    /// <summary>
    /// Gets or sets the value of the tag or person.
    /// </summary>
    /// <value>The value.</value>
    [JsonProperty(PropertyName = "value")]
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the number of times this tag is used, taking the user's security context into account.
    /// </summary>
    /// <value>The count.</value>
    [JsonProperty(PropertyName = "count")]
    public int Count { get; set; }
  }
}