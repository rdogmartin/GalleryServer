using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
  /// <summary>
  /// An object that specifies options for retrieving gallery objects. Used in conjunction with the
  /// <see cref="TagSearcher" /> class.
  /// </summary>
  public class TagSearchOptions
  {
    /// <summary>
    /// Identifies a particular property in the <see cref="Business.Entity.Tag" /> class.
    /// </summary>
    public enum TagProperty
    {
      /// <summary>
      /// Indicates that no tag property has been specified.
      /// </summary>
      NotSpecified = 0,

      /// <summary>
      /// The <see cref="Business.Entity.Tag.Value" /> property.
      /// </summary>
      Value,

      /// <summary>
      /// The <see cref="Business.Entity.Tag.Count" /> property.
      /// </summary>
      Count,
    }

    /// <summary>
    /// Gets or sets the type of tag search.
    /// </summary>
    public TagSearchType SearchType { get; set; }

    /// <summary>
    /// Gets or sets the gallery ID. Only items in this gallery are returned.
    /// </summary>
    public int GalleryId { get; set; }

    /// <summary>
    /// Gets or sets the search term. Only tags matching or containing this search term are returned.
    /// </summary>
    public string SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the roles the current user belongs to. Required when <see cref="IsUserAuthenticated" />=<c>true</c>; 
    /// otherwise, the value can be left null.
    /// </summary>
    public IGalleryServerRoleCollection Roles { get; set; }

    /// <summary>
    /// Gets or sets whether the current user has been authenticated.
    /// </summary>
    public bool IsUserAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets the number tags to retrieve. Values less than zero are treated the same as zero. Specify 
    /// <see cref="int.MaxValue" /> to return all tags.
    /// </summary>
    public int NumTagsToRetrieve { get; set; }

    /// <summary>
    /// Gets or sets which property of the <see cref="Business.Entity.Tag" /> class to sort by.
    /// </summary>
    public TagProperty SortProperty { get; set; }

    /// <summary>
    /// Gets or sets whether to sort the tags in ascending order. When <c>false</c>, tags are sorted in
    /// descending order on the property specified by <see cref="SortProperty" />.
    /// </summary>
    public bool SortAscending { get; set; }

    /// <summary>
    /// Gets or sets whether the tag tree is shown expanded or collapsed.
    /// </summary>
    public bool TagTreeIsExpanded { get; set; }
  }
}
