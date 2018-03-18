using System;
using System.Collections.Generic;
using System.Linq;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the MetadataTag table.
  /// </summary>
  public class MetadataTagRepository : Repository<GalleryDb, MetadataTagDto>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataTagRepository"/> class.
    /// </summary>
    public MetadataTagRepository()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataTagRepository"/> class.
    /// </summary>
    /// <param name="ctx">The data context.</param>
    public MetadataTagRepository(GalleryDb ctx)
    {
      Context = ctx;
    }

    /// <summary>
    /// Persists the tag/metadata relationships to the data store. If any relationships are
    /// deleted, an additional step ensures the deletion of the tag in the Tag table if that was
    /// the last reference to it.
    /// </summary>
    /// <param name="mDto">An instance of <see cref="MetadataDto"/>.</param>
    /// <param name="tags">The tags to persist.</param>
    /// <param name="galleryId"></param>
    internal void Save(MetadataDto mDto, IEnumerable<string> tags, int galleryId)
    {
      if ((mDto.MetaName != Business.Metadata.MetadataItemName.Tags) && (mDto.MetaName != Business.Metadata.MetadataItemName.People))
        return;

      // Step 1: Create a copy of the tags to save. We'll be removing items as we process them.
      var tagsToPersist = new List<string>(tags);

      // Step 2: Get a list of all meta/tag relationships in the data store for this metadata item.
      var metaTagRelationshipsToDelete = new List<MetadataTagDto>();
      foreach (var metaTag in (Context.MetadataTags.Where(mt => mt.FKMetadataId == mDto.MetadataId)))
      {
        // Step 3: Iterate through each item stored in the data store. If it is in our list of 
        // tags to save, then we don't need to insert it, so remove it from the 'persist' 
        // list (see step 5 why). If not, add it to a list of relationships to be deleted.
        if (tagsToPersist.Contains(metaTag.FKTagName))
        {
          tagsToPersist.Remove(metaTag.FKTagName);
        }
        else
        {
          metaTagRelationshipsToDelete.Add(metaTag);
        }
      }

      // Step 4: Delete the records we accumulated in our list.
      foreach (MetadataTagDto metaTagDto in metaTagRelationshipsToDelete)
      {
        Context.MetadataTags.Remove(metaTagDto);
      }

      // Step 5: Any items still left in the tagsToPersist list must be new ones. Add them.
      foreach (string tag in tagsToPersist)
      {
        Context.MetadataTags.Add(new MetadataTagDto { FKMetadataId = mDto.MetadataId, FKTagName = tag, FKGalleryId = galleryId});
      }

      Save();

      // Step 6: If a tag is no longer being used in a relationship, delete it from the tag table.
      // This requires that the previous changes are already persisted to the data store.
      foreach (MetadataTagDto metaTagDto in metaTagRelationshipsToDelete)
      {
        if (!Context.MetadataTags.Any(mt => mt.FKTagName.Equals(metaTagDto.FKTagName, StringComparison.OrdinalIgnoreCase)))
        {
          var tag = Context.Tags.Find(metaTagDto.FKTagName);
          if (tag != null)
          {
            Context.Tags.Remove(tag);
          }
        }
      }
    }
  }
}