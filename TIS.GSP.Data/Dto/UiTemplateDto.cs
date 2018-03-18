using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business;

namespace GalleryServer.Data
{
	[Table("UiTemplate", Schema = "gsp")]
	[System.Diagnostics.DebuggerDisplay("{TemplateType} ({Name})")]
	public class UiTemplateDto
	{
		[Key]
		public virtual int UiTemplateId
		{
			get;
			set;
		}

    [Required]
		public virtual UiTemplateType TemplateType
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

		// We can't configure a foreign key because it will conflict with the album relationship in table UiTemplateAlbum
		//[ForeignKey("FKGalleryId")]
		//public virtual GalleryDto Gallery
		//{
		//	get;
		//	set;
		//}

    [Required, MaxLength(255)]
    public virtual string Name
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string Description
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

    public virtual ICollection<UiTemplateAlbumDto> TemplateAlbums
		{
			get;
			set;
		}
	}
}
