
namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// Represents a particular e-mail template form.
	/// </summary>
	public enum EmailTemplateForm
	{
		AdminNotificationAccountCreated,
		AdminNotificationAccountCreatedRequiresApproval,
		UserNotificationAccountCreated,
		UserNotificationAccountCreatedApprovalGiven,
		UserNotificationAccountCreatedNeedsApproval,
		UserNotificationAccountCreatedNeedsVerification,
		UserNotificationPasswordChanged,
		UserNotificationPasswordChangedByAdmin,
		UserNotificationPasswordRecovery
	}

	/// <summary>
	/// Specifies a template that can be used to create an e-mail message.
	/// </summary>
	public class EmailTemplate
	{
		/// <summary>
		/// The e-mail subject.
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// The e-mail body.
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// A value that identifies the type of e-mail template.
		/// </summary>
		public EmailTemplateForm EmailTemplateId { get; set; }
	}
}
