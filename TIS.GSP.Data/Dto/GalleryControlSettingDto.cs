using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("GalleryControlSetting", Schema = "gsp")]
	public class GalleryControlSettingDto
	{
		[Key]
		public virtual int GalleryControlSettingId
		{
			get;
			set;
		}

    [Required, MaxLength(350)]
		public virtual string ControlId
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
