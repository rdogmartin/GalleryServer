using System;
using System.Globalization;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages
{
	/// <summary>
	/// A page-like user control that provides password recovery services.
	/// </summary>
	public partial class recoverpassword : Pages.GalleryPage
	{
		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			ConfigureControls();
		}

		private void ConfigureControls()
		{
			txtUserName.Focus();

			string msgTitle = String.Empty;
			string msgDetail = String.Empty;

			if (UserController.RequiresQuestionAndAnswer)
			{
				msgTitle = Resources.GalleryServer.Anon_Pwd_Recovery_Disabled_Header;
				msgDetail = Resources.GalleryServer.Anon_Pwd_Recovery_Disabled_Because_Question_Answer_Is_Enabled_Body;
			}

			if (!UserController.EnablePasswordRetrieval)
			{
				msgTitle = Resources.GalleryServer.Anon_Pwd_Recovery_Disabled_Header;
				msgDetail = Resources.GalleryServer.Anon_Pwd_Recovery_Disabled_Because_Password_Retrieval_Is_Disabled_Body;
			}

			if (!String.IsNullOrEmpty(msgTitle))
			{
				ClientMessage = new ClientMessageOptions
				{
					Title = msgTitle,
					Message = msgDetail,
					Style = MessageStyle.Warning,
					AutoCloseDelay = 0
				};

				pnlPwdRecoverContainer.Visible = false;
			}
		}

		/// <summary>
		/// Handles the Click event of the btnRetrievePassword control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnRetrievePassword_Click(object sender, EventArgs e)
		{
			RecoverPassword();
		}

		private void RecoverPassword()
		{
			var iconStyle = MessageStyle.Warning;
			string msgTitle;
			string msgDetail;

			IUserAccount user = UserController.GetUser(txtUserName.Text, true);

			if (user == null)
			{
				msgTitle = Resources.GalleryServer.Anon_Pwd_Recovery_Cannot_Retrieve_Pwd_Header;
				msgDetail = String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", Resources.GalleryServer.Anon_Pwd_Recovery_GetUser_Throws_Exception_Message);
			}
			else
			{
				if (HelperFunctions.IsValidEmail(user.Email))
				{
					// Send user's password in an email.
					try
					{
						EmailController.SendNotificationEmail(user.UserName, user.Email, Entity.EmailTemplateForm.UserNotificationPasswordRecovery, false);
						
						msgTitle = Resources.GalleryServer.Anon_Pwd_Recovery_Confirmation_Title;
						string url = String.Format(CultureInfo.CurrentCulture, " <a href='{0}'>{1}</a>", Web.Utils.GetUrl(PageId.login), Resources.GalleryServer.Login_Button_Text);
						msgDetail = String.Concat(String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Anon_Pwd_Recovery_Confirmation_Body, user.Email), url);
						iconStyle = MessageStyle.Info;
						pnlPwdRecoverContainer.Visible = false;
					}
					catch (Exception ex)
					{
						string errorMsg;
						if (this.GallerySettings.ShowErrorDetails)
							errorMsg = EventController.GetExceptionDetails(ex);
						else
							errorMsg = Resources.GalleryServer.Error_Generic_Msg;

						msgTitle = Resources.GalleryServer.Anon_Pwd_Recovery_Cannot_Retrieve_Pwd_Header;
						msgDetail = "<p>" + String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Anon_Pwd_Recovery_Cannot_Retrieve_Pwd_Generic_Error, errorMsg) + "</p>";
					}
				}
				else // User is valid but does not have a valid email
				{
					msgTitle = Resources.GalleryServer.Anon_Pwd_Recovery_Cannot_Retrieve_Pwd_Header;
					msgDetail = String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", Resources.GalleryServer.Anon_Pwd_Recovery_Cannot_Retrieve_Pwd_Invalid_Email);
				}
			}

			// Render confirmation message
			ClientMessage = new ClientMessageOptions
			{
				Title = msgTitle,
				Message = msgDetail,
				Style = iconStyle
			};
		}
	}
}