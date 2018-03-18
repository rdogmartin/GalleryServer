using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
	[Table("Synchronize", Schema = "gsp")]
	public class SynchronizeDto
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public virtual int FKGalleryId
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(46)]
		public virtual string SynchId
		{
			get;
			set;
		}

    [Required]
		public virtual SynchronizationState SynchState
		{
			get;
			set;
		}

    [Required]
		public virtual int TotalFiles
		{
			get;
			set;
		}

    [Required]
		public virtual int CurrentFileIndex
		{
			get;
			set;
		}
	}
}
