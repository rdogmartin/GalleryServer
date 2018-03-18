using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("UiTemplateAlbum", Schema = "gsp")]
	[System.Diagnostics.DebuggerDisplay("Template ID {FKUiTemplateId}; Album ID ({FKAlbumId})")]
	public class UiTemplateAlbumDto
	{
		[Key, Column(Order = 0)]
		public virtual int FKUiTemplateId
		{
			get;
			set;
		}

		[Key, Column(Order = 1)]
		public virtual int FKAlbumId
		{
			get;
			set;
		}

		[ForeignKey("FKUiTemplateId")]
		public virtual UiTemplateDto UiTemplate
		{
			get;
			set;
		}

    [ForeignKey("FKAlbumId")]
		public virtual AlbumDto Album
		{
			get;
			set;
		}
	}
}
