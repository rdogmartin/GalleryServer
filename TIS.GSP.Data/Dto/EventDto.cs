using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business;

namespace GalleryServer.Data
{
	[Table("Event", Schema = "gsp")]
	public class EventDto
	{
		[Key]
		public virtual int EventId
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

    [ForeignKey("FKGalleryId")]
		public virtual GalleryDto Gallery
		{
			get;
			set;
		}

		[Required]
		public virtual EventType EventType
		{
			get;
			set;
		}

    [Required]
		public virtual System.DateTime TimeStampUtc
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(4000)]
		public virtual string Message
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string EventData
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(1000)]
		public virtual string ExType
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(1000)]
		public virtual string ExSource
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string ExTargetSite
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string ExStackTrace
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(1000)]
		public virtual string InnerExType
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(4000)]
		public virtual string InnerExMessage
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(1000)]
		public virtual string InnerExSource
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string InnerExTargetSite
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string InnerExStackTrace
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string InnerExData
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength(1000)]
		public virtual string Url
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string FormVariables
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string Cookies
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string SessionVariables
		{
			get;
			set;
		}

    [Required(AllowEmptyStrings = true), MaxLength]
		public virtual string ServerVariables
		{
			get;
			set;
		}
	}
}
