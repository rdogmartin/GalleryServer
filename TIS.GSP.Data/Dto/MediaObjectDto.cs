using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("MediaObject", Schema = "gsp")]
	public class MediaObjectDto
	{
		[Key]
		public virtual int MediaObjectId
		{
			get;
			set;
		}

		[Required]
		public virtual int FKAlbumId
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

		[Required(AllowEmptyStrings = true), MaxLength(255)]
		public virtual string ThumbnailFilename
		{
			get;
			set;
		}

		[Required]
		public virtual int ThumbnailWidth
		{
			get;
			set;
		}

		[Required]
		public virtual int ThumbnailHeight
		{
			get;
			set;
		}

		[Required]
		public virtual int ThumbnailSizeKB
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(255)]
		public virtual string OptimizedFilename
		{
			get;
			set;
		}

		[Required]
		public virtual int OptimizedWidth
		{
			get;
			set;
		}

		[Required]
		public virtual int OptimizedHeight
		{
			get;
			set;
		}

		[Required]
		public virtual int OptimizedSizeKB
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(255)]
		public virtual string OriginalFilename
		{
			get;
			set;
		}

		[Required]
		public virtual int OriginalWidth
		{
			get;
			set;
		}

		[Required]
		public virtual int OriginalHeight
		{
			get;
			set;
		}

		[Required]
		public virtual int OriginalSizeKB
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength]
		public virtual string ExternalHtmlSource
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(15)]
		public virtual string ExternalType
		{
			get;
			set;
		}

		[Required]
		public virtual int Seq
		{
			get;
			set;
		}

		[Required, MaxLength(256)]
		public virtual string CreatedBy
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

		[Required, MaxLength(256)]
		public virtual string LastModifiedBy
		{
			get;
			set;
		}

		[Required]
		public virtual System.DateTime DateLastModified
		{
			get;
			set;
		}

		[Required]
		public virtual bool IsPrivate
		{
			get;
			set;
		}

		public virtual ICollection<MetadataDto> Metadata
		{
			get;
			set;
		}
	}
}
