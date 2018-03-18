using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("AppSetting", Schema = "gsp")]
	public class AppSettingDto
	{
		[Key]
		public virtual int AppSettingId
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
