using System;
using System.Web.Security;
using System.Globalization;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using GalleryServer.WebControls;

namespace GalleryServer.Web.Pages
{
	/// <summary>
	/// A page-like user control that lets a user manage their personal account settings.
	/// </summary>
	public partial class myaccount : Pages.GalleryPage
	{
		#region Private Fields

		private IUserAccount _user;
		private IUserProfile _currentProfile;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the current user.
		/// </summary>
		/// <value>The current user.</value>
		public IUserAccount CurrentUser
		{
			get
			{
				if (this._user == null)
					_user = UserController.GetUser();

				return this._user;
			}
		}

		/// <summary>
		/// Gets the current profile.
		/// </summary>
		/// <value>The current profile.</value>
		protected IUserProfile CurrentProfile
		{
			get
			{
				if (this._currentProfile == null)
				{
					this._currentProfile = ProfileController.GetProfile().Copy();
				}

				return this._currentProfile;
			}
		}

		/// <summary>
		/// Gets the current gallery profile.
		/// </summary>
		/// <value>The current gallery profile.</value>
		public IUserGalleryProfile CurrentGalleryProfile
		{
			get
			{
				return CurrentProfile.GetGalleryProfile(GalleryId);
			}
		}

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			if (this.IsAnonymousUser)
				Utils.Redirect(Web.PageId.album);

			if (!GallerySettings.AllowManageOwnAccount)
				Utils.Redirect(Web.PageId.album);

			if (!IsPostBack)
				ConfigureControlsFirstTime();
		}

		/// <summary>
		/// Handles the OnAfterBindControl event of the wwDataBinder control.
		/// </summary>
		/// <param name="item">The wwDataBindingItem item.</param>
		protected void wwDataBinder_AfterBindControl(wwDataBindingItem item)
		{
			// HTML encode the data
			if (item.ControlId == lblUserName.ID)
			{
				lblUserName.Text = Utils.HtmlEncode(lblUserName.Text);
			}
		}

		/// <summary>
		/// Handles the Click event of the btnSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnSave_Click(object sender, EventArgs e)
		{
			SaveSettings();
		}

		/// <summary>
		/// Handles the Click event of the btnCancel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnCancel_Click(object sender, EventArgs e)
		{
			RedirectToPreviousPage();
		}

		/// <summary>
		/// Handles the Click event of the lbDeleteAccount control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void lbDeleteAccount_Click(object sender, EventArgs e)
		{
			ProcessAccountDeletion();
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstTime()
		{
			hlChangePwd.NavigateUrl = Web.Utils.GetUrl(Web.PageId.changepassword);

			lbDeleteAccount.OnClientClick = String.Format(CultureInfo.CurrentCulture, "return confirm('{0}')", Resources.GalleryServer.MyAccount_Delete_Account_Confirmation);

			if (GallerySettings.EnableUserAlbum)
			{
				litDeleteAccountWarning.Text = Resources.GalleryServer.MyAccount_Delete_Account_With_User_Albums_Warning;
			}
			else
			{
				litDeleteAccountWarning.Text = Resources.GalleryServer.MyAccount_Delete_Account_Warning;
				pnlUserAlbum.Visible = false;
			}

			if (GallerySettings.AllowDeleteOwnAccount)
			{
				pnlDeleteAccount.Visible = true;
			}

			CheckForMessages();

			this.wwDataBinder.DataBind();
		}

		private void SaveSettings()
		{
			this.wwDataBinder.Unbind(this);

			if (wwDataBinder.BindingErrors.Count > 0)
			{
				ClientMessage = new ClientMessageOptions
				{
					Title = Resources.GalleryServer.Validation_Summary_Text,
					Message = wwDataBinder.BindingErrors.ToString(),
					Style = MessageStyle.Error
				};

				return;
			}

			UserController.SaveUser(this.CurrentUser);

			bool originalEnableUserAlbumSetting = ProfileController.GetProfileForGallery(GalleryId).EnableUserAlbum;

			SaveProfile(this.CurrentProfile);

			SaveSettingsCompleted(originalEnableUserAlbumSetting);
		}

		private void SaveSettingsCompleted(bool originalEnableUserAlbumSetting)
		{
			bool newEnableUserAlbumSetting = ProfileController.GetProfileForGallery(GalleryId).EnableUserAlbum;
			
			if (originalEnableUserAlbumSetting != newEnableUserAlbumSetting)
			{
				// Since we changed a setting that affect how and which controls are rendered to the page, let us redirect to the current page and
				// show the save success message. If we simply show a message without redirecting, two things happen: (1) the user doesn't see the effect
				// of their change until the next page load, (2) there is the potential for a viewstate validation error.
				const MessageType msg = MessageType.SettingsSuccessfullyChanged;

				Utils.Redirect(PageId.myaccount, "msg={0}", ((int)msg).ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				ClientMessage = new ClientMessageOptions
				{
					Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
					Message = Resources.GalleryServer.Admin_Save_Success_Text,
					Style = MessageStyle.Success
				};
			}
		}

		private void SaveProfile(IUserProfile userProfile)
		{
			// Get reference to user's album. We need to do this *before* saving the profile, because if the user disabled their user album,
			// this method will return null after saving the profile.
			IAlbum album = UserController.GetUserAlbum(GalleryId);

			IUserGalleryProfile profile = userProfile.GetGalleryProfile(GalleryId);

			if (!profile.EnableUserAlbum)
			{
				AlbumController.DeleteAlbum(album);
			}

			if (!profile.EnableUserAlbum)
			{
				profile.UserAlbumId = 0;
			}

			ProfileController.SaveProfile(userProfile);
		}

		private void ProcessAccountDeletion()
		{
			try
			{
				UserController.DeleteGalleryServerProUser(this.CurrentUser.UserName, false);
			}
			catch (WebException ex)
			{
				int errorId = LogError(ex);

				ClientMessage = new ClientMessageOptions
				{
					Title = Resources.GalleryServer.Validation_Summary_Text,
					Message = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.MyAccount_Delete_Account_Err_Msg, errorId, ex.GetType()),
					Style = MessageStyle.Error
				};

				return;
			}
			catch (GallerySecurityException ex)
			{
				int errorId = LogError(ex);

				ClientMessage = new ClientMessageOptions
				{
					Title = Resources.GalleryServer.Validation_Summary_Text,
					Message = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.MyAccount_Delete_Account_Err_Msg, errorId, ex.GetType()),
					Style = MessageStyle.Error
				};

				return;
			}

			UserController.LogOffUser();

			Utils.Redirect(PageId.album);
		}

		private void CheckForMessages()
		{
			if (ClientMessage != null && ClientMessage.MessageId == MessageType.SettingsSuccessfullyChanged)
			{
				ClientMessage.Title = Resources.GalleryServer.Admin_Save_Success_Hdr;
				ClientMessage.Message = Resources.GalleryServer.Admin_Save_Success_Text;
			}
		}

		#endregion
	}
}