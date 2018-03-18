using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("Tag", Schema = "gsp")]
	[System.Diagnostics.DebuggerDisplay("{TagName} ({MetaName})")]
	public class TagDto
	{
		[Key, MaxLength(100)]
		public virtual string TagName
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
