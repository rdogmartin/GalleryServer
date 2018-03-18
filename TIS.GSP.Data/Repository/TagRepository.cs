using System;
using System.Collections.Generic;
using System.Linq;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the Tag table.
  /// </summary>
  public class TagRepository : Repository<GalleryDb, TagDto>
  {
    private static readonly object _sharedLock = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="TagRepository"/> class.
    /// </summary>
    public TagRepository()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagRepository"/> class.
    /// </summary>
    /// <param name="ctx">The data context.</param>
    public TagRepository(GalleryDb ctx)
    {
      Context = ctx;
    }

    /// <summary>
    /// Persists the tag names, if required, to the Tag table. When a tag has previously been
    /// used, no action is taken because the tag already exists in that table. Additional code
    /// in <see cref="MetadataTagRepository.Save(MetadataDto,System.Collections.Generic.IEnumerable{string},int)" /> 
    /// deletes the tag if it is no longer applied to any metadata items.
    /// </summary>
    /// <param name="metaName">A <see cref="Business.Metadata.MetadataItemName"/> indicating the type of tag.
    /// Must be either <see cref="Business.Metadata.MetadataItemName.Tags" /> or <see cref="Business.Metadata.MetadataItemName.People" />.</param>
    /// <param name="tags">The tags to persist.</param>
    internal void Save(Business.Metadata.MetadataItemName metaName, IEnumerable<string> tags)
    {
      if ((metaName != Business.Metadata.MetadataItemName.Tags) && (metaName != Business.Metadata.MetadataItemName.People))
        return;

      // We need a lock to prevent other threads from inserting tags at the same time (which was cause a duplicate error)
      lock (_sharedLock)
      {
        foreach (var tag in tags)
        {
          if (!Context.Tags.Any(t => t.TagName.Equals(tag.Trim(), StringComparison.Ordinal)))
          {
            Context.Tags.Add(new TagDto
            {
              TagName = tag.Trim()
            });
          }
        }

        Save();
      }
    }

    /// <summary>
    /// Deletes the unused tags.
    /// </summary>
    public void DeleteUnusedTags()
    {
      foreach (var tagDto in Where(m => !m.MetadataTags.Any()))
      {
        Delete(tagDto);
      }

      Save();
    }
  }
}