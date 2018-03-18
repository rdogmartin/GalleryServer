using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("Gallery", Schema = "gsp")]
	public class GalleryDto
	{
		[Key]
		public virtual int GalleryId
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(1000)]
		public virtual string Description
		{
			get;
			set;
		}

    [Required]
		public virtual bool IsTemplate
    {
      get;
      set;
    }

    [Required]
		public virtual System.DateTime DateAdded
		{
			get;
			set;
		}
	}
}
