using System;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages
{
	/// <summary>
	/// A page-like user control that provides login capability.
	/// </summary>
	public partial class login : Pages.GalleryPage
	{
		#region Private Fields


		#endregion

		#region Public Properties

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				if (!this.IsAnonymousUser)
					Utils.Redirect(Web.PageId.album);

				ConfigureControlsFirstTime();
			}

			ConfigureControlsEveryTime();
		}

		/// <summary>
		/// Handles the LoggedIn event of the Login1 control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Login1_LoggedIn(object sender, EventArgs e)
		{
			// Get the user. This will ensure we get the username with the correct case, regardless of how the user logged on (Admin vs. admin, etc).
			IUserAccount user = UserController.GetUser(Login1.UserName, false);

			UserController.UserLoggedOn(user.UserName, GalleryId);

			if (GallerySettings.EnableUserAlbum && GallerySettings.RedirectToUserAlbumAfterLogin)
			{
				Utils.Redirect(Utils.GetUrl(PageId.album, "aid={0}", UserController.GetUserAlbumId(user.UserName, GalleryId)));
			}

			// Reload page.
			if (String.IsNullOrEmpty(Utils.GetQueryStringParameterString("ReturnUrl")))
				Utils.Redirect(Web.PageId.album);
			else
				Response.Redirect(Utils.GetQueryStringParameterString("ReturnUrl"));
			//Response.Redirect(GetRedirectUrlAfterLoginOrLogout());

			// Note: For reasons I don't quite understand we cannot use the following pattern that is used elsewhere. It has 
			// something to do with the fact that we are in a postback. When we try to use it, the page finishes the postback
			// without doing the redirect. That would be OK except we need to clear out any msg parameters in
			// the query string. The easiest way to do that is with a redirect.
			//Response.Redirect(GetRedirectUrlAfterLoginOrLogout(), false);
			//System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
		}

    /// <summary>
    /// Handles the LoginError event of the Login1 control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void Login1_LoginError(object sender, EventArgs e)
		{
			if (ClientMessage != null)
			{
				ClientMessage.Title = Resources.GalleryServer.Login_Failure_Text;
				ClientMessage.AutoCloseDelay = 4000;
			}
		}

		#endregion


		#region Private Methods

		private void ConfigureControlsEveryTime()
		{
			// Don't need the login link if we are already on the login page.
			this.ShowLogin = false;

			// Hide the search button if anonymous browsing is disabled, since the user can't search if they are not logged in.
			if (!this.AllowAnonymousBrowsing)
				this.ShowSearch = false;
			
			Login1.Focus();
		}

		private void ConfigureControlsFirstTime()
		{
			if (ClientMessage != null && ClientMessage.MessageId == MessageType.UserNameOrPasswordIncorrect)
			{
				ClientMessage.Title = Resources.GalleryServer.Login_Failure_Text;
			}

			Login1.MembershipProvider = Controller.UserController.MembershipGsp.Name;
			Login1.PasswordRecoveryUrl = Utils.GetUrl(Web.PageId.recoverpassword);

			// Don't show login at top right. We don't need it, since we are already showing login controls, and it doesn' work
			// right anyway, because the 
			//this.GalleryControl.ShowLogin = false;

			if (GallerySettings.EnableSelfRegistration)
			{
				Login1.CreateUserText = Resources.GalleryServer.Login_Create_Account_Text;
				Login1.CreateUserUrl = Utils.GetUrl(Web.PageId.createaccount);
			}
		}

		#endregion
	}
}