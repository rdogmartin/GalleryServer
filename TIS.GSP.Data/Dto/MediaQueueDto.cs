using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business;

namespace GalleryServer.Data
{
	[Table("MediaQueue", Schema = "gsp")]
	public class MediaQueueDto
	{
		[Key]
		public virtual int MediaQueueId
		{
			get;
			set;
		}

		[Required]
		public virtual int FKMediaObjectId
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

		[Required, MaxLength(256)]
		public virtual string Status
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength]
		public virtual string StatusDetail
		{
			get;
			set;
		}

		[Required]
		public virtual MediaQueueItemConversionType ConversionType { get; set; }

		[Required]
		public virtual MediaAssetRotateFlip RotationAmount { get; set; }

		[Required]
		public virtual System.DateTime DateAdded
		{
			get;
			set;
		}

		public virtual System.DateTime? DateConversionStarted
		{
			get;
			set;
		}

		public virtual System.DateTime? DateConversionCompleted
		{
			get;
			set;
		}
	}
}
