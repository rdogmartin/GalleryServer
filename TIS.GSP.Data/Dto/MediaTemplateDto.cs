using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("MediaTemplate", Schema = "gsp")]
	public class MediaTemplateDto
	{
		[Key]
		public virtual int MediaTemplateId
		{
			get;
			set;
		}

    [Required, MaxLength(200)]
    public virtual string MimeType
		{
			get;
			set;
		}

    [Required, MaxLength(50)]
    public virtual string BrowserId
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string HtmlTemplate
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string ScriptTemplate
		{
			get;
			set;
		}
	}
}
