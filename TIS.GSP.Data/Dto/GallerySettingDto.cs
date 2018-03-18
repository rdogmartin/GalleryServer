using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("GallerySetting", Schema = "gsp")]
	public class GallerySettingDto
	{
		[Key]
		public virtual int GallerySettingId
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

    [ForeignKey("FKGalleryId")]
		public virtual GalleryDto Gallery
    {
      get;
      set;
    }

    [Required, MaxLength(200)]
		public virtual string SettingName
    {
      get;
      set;
    }

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string SettingValue
    {
      get;
      set;
    }
  }
}
