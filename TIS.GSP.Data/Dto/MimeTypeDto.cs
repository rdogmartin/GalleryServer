using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
  [Table("MimeType", Schema = "gsp")]
  public class MimeTypeDto
  {
    [Key]
    public virtual int MimeTypeId
    {
      get;
      set;
    }

    [Required, MaxLength(30)]
    public virtual string FileExtension
    {
      get;
      set;
    }

    [Required, MaxLength(200)]
    public virtual string MimeTypeValue
    {
      get;
      set;
    }

    [Required(AllowEmptyStrings = true), MaxLength(200)]
    public virtual string BrowserMimeTypeValue
    {
      get;
      set;
    }

    public virtual ICollection<MimeTypeGalleryDto> MimeTypeGalleries
    {
      get;
      set;
    }
  }
}
