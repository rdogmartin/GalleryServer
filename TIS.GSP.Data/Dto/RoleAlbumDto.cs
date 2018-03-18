using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("RoleAlbum", Schema = "gsp")]
	public class RoleAlbumDto
	{
		[Key, Column(Order = 0), MaxLength(256)]
		public virtual string FKRoleName
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

		[ForeignKey("FKRoleName")]
		public virtual RoleDto Role
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
