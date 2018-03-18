using System;
using System.Globalization;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Controls
{
	/// <summary>
	/// A user control that wraps the header controls that appear at the top of the gallery (gallery title, logon controls, search icon, etc.)
	/// </summary>
	public partial class galleryheader : GalleryUserControl
	{
		#region Properties

		/// <summary>
		/// Gets a reference to the Login control on the page. 
		/// </summary>
		/// <value>The Login control on the page.</value>
		private Login Login1
		{
			get
			{
				return (Login)this.GalleryPage.FindControlRecursive(lv, "Login1");
			}
		}

		/// <summary>
		/// Gets the client ID for the DOM element that contains the HTML jsRender template for the 
		/// header. Ex: "gsp_g_gHdrScriptTmpl"
		/// </summary>
		private string HeaderHtmlTmplClientId
		{
			get { return String.Concat(this.GalleryPage.GspClientId, "_gHdrHtmlTmpl"); }
		}

		private string HeaderScriptTmplClientId
		{
			get { return String.Concat(this.GalleryPage.GspClientId, "_gHdrScriptTmpl"); }
		}

		#endregion

		#region Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			ConfigureControls();

			RegisterStartupScript();
		}

		/// <summary>
		/// Handles the LoginError event of the Login control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Login1_LoginError(object sender, EventArgs e)
		{
			// The user has entered an invalid user name and/or error. Redirect to login page and append message.
			Utils.Redirect(PageId.login, "msg={0}&ReturnUrl={1}", ((int)MessageType.UserNameOrPasswordIncorrect).ToString(CultureInfo.InvariantCulture), Utils.UrlEncode(Utils.GetCurrentPageUrl(true)));
		}

		/// <summary>
		/// Handles the LoggedIn event of the Login control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Login1_LoggedIn(object sender, EventArgs e)
		{
			// Get the user. This will ensure we get the username with the correct case, regardless of how the user logged on (Admin vs. admin, etc).
			IUserAccount user = UserController.GetUser(Login1.UserName, false);

			UserController.UserLoggedOn(user.UserName, this.GalleryPage.GalleryId);

			if (this.GalleryPage.GallerySettings.EnableUserAlbum && this.GalleryPage.GallerySettings.RedirectToUserAlbumAfterLogin)
			{
				Utils.Redirect(Utils.GetUrl(PageId.album, "aid={0}", UserController.GetUserAlbumId(user.UserName, this.GalleryPage.GalleryId)));
			}

			ReloadPage();
		}

		#endregion

		#region Functions

		private void ConfigureControls()
		{
			ConfigureLoginDialog();
		}

		private void ConfigureLoginDialog()
		{
			if (this.GalleryPage.IsAnonymousUser && Login1 != null)
			{
				// NOTE: We have to add the '&& Login1 != null' above to prevent a null reference exception in the specific scenario where
				// the method GalleryPage.DetectPreviousInstallation() has triggered a redirect. When that happens, this function still
				// runs but the Login1 object will be null.
				Login1.MembershipProvider = Controller.UserController.MembershipGsp.Name;
				Login1.PasswordRecoveryUrl = Utils.GetUrl(Web.PageId.recoverpassword);

				if (this.GalleryPage.GallerySettings.EnableSelfRegistration)
				{
					Login1.CreateUserText = Resources.GalleryServer.Login_Create_Account_Text;
					Login1.CreateUserUrl = Utils.GetUrl(Web.PageId.createaccount);
				}
			}
		}

		private void RegisterStartupScript()
		{
			var uiTemplate = this.GalleryPage.UiTemplates.Get(UiTemplateType.Header, this.GalleryPage.GetAlbum());

			// Define 3 script tags. The first two hold the HTML and javascript jsRender templates.
			// The last contains start script that does 2 things:
			// 1. Compile the jsRender template and run the javascript generated in the template
			// 2. Generate the JavaScript from the template and add to the page
			string script = String.Format(CultureInfo.InvariantCulture, @"
<script id='{0}' type='text/x-jsrender'>
{1}
</script>
<script id='{2}' type='text/x-jsrender'>
{3}
</script>
<script>
(function ($) {{
	$(document).ready(function () {{
		$.templates({{{5}: $('#{0}').html() }});
		(new Function($('#{2}').render(Gs.Vars['{4}'].gsData)))();
	}});
}})(jQuery);
</script>
",
																		HeaderHtmlTmplClientId, // 0
																		uiTemplate.HtmlTemplate, // 1
																		HeaderScriptTmplClientId, // 2
																		uiTemplate.ScriptTemplate, // 3
																		GalleryPage.GspClientId, // 4
																		GalleryPage.HeaderTmplName // 5
																		);

			this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.GalleryPage.ClientID, "_gHdrTmplScript"), script, false);
		}

		private void ReloadPage()
		{
			// If currently looking at a media object or album, update query string to point to current media object or
			// album page (if album paging is enabled) and redirect. Otherwise just navigate to current album.
			PageId pageId = this.GalleryPage.PageId;
			if ((pageId == PageId.album) || (pageId == PageId.mediaobject))
			{
				string url = Request.Url.PathAndQuery;

				url = Utils.RemoveQueryStringParameter(url, "msg"); // Remove any messages

				if (this.GalleryPage.GetMediaObjectId() > int.MinValue)
				{
					url = Utils.RemoveQueryStringParameter(url, "moid");
					url = Utils.AddQueryStringParameter(url, String.Concat("moid=", this.GalleryPage.GetMediaObjectId()));
				}

				int page = Utils.GetQueryStringParameterInt32(this.GalleryPage.PreviousUri, "page");
				if (page > int.MinValue)
				{
					url = Utils.RemoveQueryStringParameter(url, "page");
					url = Utils.AddQueryStringParameter(url, String.Concat("page=", page));
				}

				Utils.Redirect(url);
			}
			else
			{
				Utils.Redirect(PageId.album, "aid={0}", this.GalleryPage.GetAlbumId());
			}
		}

		#endregion
	}
}