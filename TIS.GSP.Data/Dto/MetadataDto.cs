using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Data
{
	[Table("Metadata", Schema = "gsp")]
	[System.Diagnostics.DebuggerDisplay("Meta {MetaName} = {Value}")]
	public class MetadataDto
	{
		[Key]
		public virtual int MetadataId
		{
			get;
			set;
		}

		[Required]
		public virtual MetadataItemName MetaName
		{
			get;
			set;
		}

		public virtual int? FKMediaObjectId
		{
			get;
			set;
		}

		public virtual int? FKAlbumId
		{
			get;
			set;
		}

		[ForeignKey("FKMediaObjectId")]
		public virtual MediaObjectDto MediaObject
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

		[MaxLength]
		public virtual string RawValue
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength]
		public virtual string Value
		{
			get;
			set;
		}

		public virtual ICollection<MetadataTagDto> MetadataTags
		{
			get;
			set;
		}
	}
}
