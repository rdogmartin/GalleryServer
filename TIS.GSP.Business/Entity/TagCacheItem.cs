using System.Diagnostics;

namespace GalleryServer.Business.Entity
{
  /// <summary>
  /// An entity representing a tag or person.
  /// </summary>
  [DebuggerDisplay("Value=\"{Value}\": CountAll={CountAll}; PrivateCount={CountPrivate}")]
  public class TagCacheItem
  {
    /// <summary>
    /// Gets or sets the value of the tag or person.
    /// </summary>
    /// <value>The value.</value>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the total number of times this tag is used in the gallery, regardless of the user's security context.
    /// </summary>
    /// <value>The count.</value>
    public int CountAll { get; set; }

    /// <summary>
    /// Gets or sets the number of times this tag is used in private albums in the gallery. This will always be equal to or 
    /// less than <see cref="CountAll" />.
    /// </summary>
    /// <value>The count that applies to private albums.</value>
    public int CountPrivate { get; set; }
  }
}