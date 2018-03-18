using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("MetadataTag", Schema = "gsp")]
	public class MetadataTagDto
	{
		[Key, Column(Order = 0)]
		public virtual int FKMetadataId
		{
			get;
			set;
		}

    [Key, Column(Order = 1), MaxLength(100)]
		public virtual string FKTagName
		{
			get;
			set;
		}

    [ForeignKey("FKMetadataId")]
    public virtual MetadataDto Metadata
    {
      get;
      set;
    }

    [ForeignKey("FKTagName")]
    public virtual TagDto Tag
    {
      get;
      set;
    }

    /// <summary>
    /// The ID of the gallery this item belongs to. This data is actually redundant since it can be calculated from
    /// its relationship to the album or media object; however, we store it here purely for performance reasons,
    /// since getting a list of tags belonging to a gallery is far faster when we can use this property in the WHERE clause.
    /// </summary>
	  public virtual int FKGalleryId
		{
			get;
			set;
		}
  }
}
