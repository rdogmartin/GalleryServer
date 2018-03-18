using System;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages
{
	/// <summary>
	/// A page-like user control that allows a user to change their password.
	/// </summary>
	public partial class changepassword : Pages.GalleryPage
	{
		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (this.IsAnonymousUser)
				Utils.Redirect(Web.PageId.album);
			else
				ConfigureControlsFirstTime();

			cp1.Focus();
		}

		private void ConfigureControlsFirstTime()
		{
			cp1.MembershipProvider = UserController.MembershipGsp.Name;
			cp1.CancelDestinationPageUrl = this.PreviousUrl;
			cp1.ContinueDestinationPageUrl = Utils.GetUrl(Web.PageId.myaccount);
		}

		/// <summary>
		/// Handles the SendingMail event of the cp1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Web.UI.WebControls.MailMessageEventArgs"/> instance containing the event data.</param>
		protected void cp1_SendingMail(object sender, System.Web.UI.WebControls.MailMessageEventArgs e)
		{
			// By default the ChangePassword control will use the mail settings in web.config. But since Gallery Server allows setting the 
			// SMTP server and port as a gallery setting, the user might not have configured web.config, resulting in a failed e-mail.
			// To prevent this, we'll send our own e-mail, then cancel the original one.
			EmailController.SendNotificationEmail(UserController.GetUser(), Entity.EmailTemplateForm.UserNotificationPasswordChanged);

			e.Cancel = true;
		}

		/// <summary>
		/// Handles the CancelButtonClick event of the cp1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void cp1_CancelButtonClick(object sender, EventArgs e)
		{
			this.RedirectToPreviousPage();
		}
	}
}