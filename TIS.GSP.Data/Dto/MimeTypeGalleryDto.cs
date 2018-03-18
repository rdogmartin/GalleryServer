using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("MimeTypeGallery", Schema = "gsp")]
  public class MimeTypeGalleryDto
  {
    [Key]
    public virtual int MimeTypeGalleryId
    {
      get;
      set;
    }

    [Required]
    public virtual int FKGalleryId
    {
      get;
      set;
    }

    [Required]
    public virtual int FKMimeTypeId
    {
      get;
      set;
    }

    [Required]
    public virtual bool IsEnabled
    {
      get;
      set;
    }

    [ForeignKey("FKGalleryId")]
    public virtual GalleryDto Gallery
    {
      get;
      set;
    }

    [ForeignKey("FKMimeTypeId")]
    public virtual MimeTypeDto MimeType
    {
      get; 
      set;
    }
  }
}
