using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("Role", Schema = "gsp")]
	public class RoleDto
	{
		[Key, MaxLength(256)]
		public virtual string RoleName
		{
			get;
			set;
		}

    [Required]
		public virtual bool AllowViewAlbumsAndObjects
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowViewOriginalImage
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowAddChildAlbum
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowAddMediaObject
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowEditAlbum
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowEditMediaObject
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowDeleteChildAlbum
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowDeleteMediaObject
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowSynchronize
		{
			get;
			set;
		}

    [Required]
    public virtual bool HideWatermark
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowAdministerGallery
		{
			get;
			set;
		}

    [Required]
    public virtual bool AllowAdministerSite
		{
			get;
			set;
		}

		public virtual ICollection<RoleAlbumDto> RoleAlbums
		{
			get;
			set;
		}
	}
}
