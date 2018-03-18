using System;
using System.Globalization;
using System.IO;
using System.Web.Security;
using System.Web.UI;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages
{
  /// <summary>
  /// A page-like user control that allows a user to create a new account.
  /// </summary>
  public partial class createaccount : Pages.GalleryPage
  {

    #region Private Fields

    private bool? _enableUserAlbum;
    private bool? _enableEmailVerification;
    private bool? _requireAdminApproval;
    private bool? _useEmailForAccountName;

    private enum PageMode
    {
      Unknown = 0,

      /// <summary>
      /// Specifies the current page is intended to create a site administrator.
      /// </summary>
      CreateAdmin,

      /// <summary>
      /// Specifies the current page is intended to create a normal user.
      /// </summary>
      Normal
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating the page mode.
    /// </summary>
    private static PageMode ViewMode
    {
      get
      {
        return (Utils.InstallRequested ? PageMode.CreateAdmin : PageMode.Normal);
      }
    }

    /// <summary>
    /// Gets a value indicating whether user albums are enabled.
    /// </summary>
    /// <value><c>true</c> if user albums are enabled; otherwise, <c>false</c>.</value>
    public bool EnableUserAlbum
    {
      get
      {
        if (!this._enableUserAlbum.HasValue)
          this._enableUserAlbum = GallerySettings.EnableUserAlbum;

        return this._enableUserAlbum.Value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether email verification is required.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if email verification is required; otherwise, <c>false</c>.
    /// </value>
    public bool EnableEmailVerification
    {
      get
      {
        if (!this._enableEmailVerification.HasValue)
          this._enableEmailVerification = GallerySettings.RequireEmailValidationForSelfRegisteredUser;

        return this._enableEmailVerification.Value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether admin approval for new accounts is required.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if admin approval for new accounts is required; otherwise, <c>false</c>.
    /// </value>
    public bool RequireAdminApproval
    {
      get
      {
        if (!this._requireAdminApproval.HasValue)
          this._requireAdminApproval = GallerySettings.RequireApprovalForSelfRegisteredUser;

        return this._requireAdminApproval.Value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the username must consist of an e-mail address.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if the username must consist of an e-mail address; otherwise, <c>false</c>.
    /// </value>
    public bool UseEmailForAccountName
    {
      get
      {
        if (!this._useEmailForAccountName.HasValue)
          this._useEmailForAccountName = GallerySettings.UseEmailForAccountName;

        return this._useEmailForAccountName.Value;
      }
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
      if (Utils.IsQueryStringParameterPresent("verify"))
        ValidateUser();

      if (!IsAnonymousUser)
        Utils.Redirect(PageId.album);

      if (ViewMode == PageMode.Normal && !GallerySettings.EnableSelfRegistration)
        Utils.Redirect(PageId.album);

      ConfigureControls();

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }
    }

    /// <summary>
    /// Handles the Click event of the btnCreateAccount control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void btnCreateAccount_Click(object sender, EventArgs e)
    {
      if (ValidateUserNameConformsToRequirements())
      {
        CreateAccount();

        if (ViewMode == PageMode.CreateAdmin)
        {
          UpdateRootAlbumTitleAfterAdminCreation();
          DeleteInstallFile();
        }
      }
    }

    #endregion

    #region Functions

    private void ConfigureControls()
    {
      if (UseEmailForAccountName)
      {
        trEmail.Visible = false;
        l2.Text = Resources.GalleryServer.CreateAccount_Email_Header_Text;
      }

      if (this.EnableEmailVerification)
      {
        lblEmailReqd.Visible = true;
        //rfvEmail.Enabled = true;
      }
    }

    private void ConfigureControlsFirstTime()
    {
      if (ViewMode == PageMode.CreateAdmin)
      {
        litHeader.Text = Resources.GalleryServer.CreateAccount_HeaderAdmin;

        AddEula();
      }
    }

    private void AddEula()
    {
      var html = String.Format(CultureInfo.InvariantCulture, @"<section class='gsp_ca_eula_dg' runat='server'>
  <section class='gsp_ca_eula_logo_ctr gsp_roundedtop10'>
    <img src='{0}' alt='Gallery Server logo' />
  </section>
  <section class='gsp_ca_eula'>
    <p class='gsp_ca_eula_hdr'>To continue, you must accept the End User License Agreement.</p>
    <iframe frameborder='0' scrolling='auto' src='{1}/pages/eula-gpl.htm' style='width: 100%; height: 475px;'></iframe>
  </section>
</section>
", 
        Utils.GetSkinnedUrl("/images/gs-logo.png"),
        Utils.GalleryRoot);

      var script = String.Format(CultureInfo.InvariantCulture, @"
<script>
(function ($) {{
  $(document).ready(function () {{
    var $createUserPanel = $('.gsp_createuser', $('#{0}')).hide();

    $('.gsp_ca_eula_dg', $('#{0}')).dialog({{
      appendTo: $('#{0}'),
      autoOpen: true,
      draggable: false,
      resizable: false,
      width: 650,
      height: 725,
      modal: false,
      hide: 'fade',
      classes: {{ 'ui-dialog': 'gsp_ca_eula_dg_ctr' }},
      buttons: [{{
        text: 'Accept',
        click: function() {{
          $(this).dialog( 'close' );
        }}
      }}],
      close: function() {{
        $createUserPanel.fadeIn().find('#{1}').focus();
      }}
    }});
  }});
}})(jQuery);
</script>
",
        GspClientId, 
        txtNewUserUserName.ClientID);

      phEula.Controls.Add(new LiteralControl(html));
      this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.ClientID, "_createAccountEulaScript"), script, false);
    }

    /// <summary>
    /// Updates the root album title so that it no longer contains the message about creating an admin account.
    /// </summary>
    private void UpdateRootAlbumTitleAfterAdminCreation()
    {
      var rootAlbum = Factory.LoadRootAlbumInstance(GalleryId, true);
      var updateableRootAlbum = Factory.LoadAlbumInstance(new AlbumLoadOptions(rootAlbum.Id) { IsWritable = true });

      updateableRootAlbum.Caption = Resources.GalleryServer.Site_Welcome_Msg;
      GalleryObjectController.SaveGalleryObject(updateableRootAlbum);
    }

    private void DeleteInstallFile()
    {
      try
      {
        File.Delete(Utils.InstallFilePath);
      }
      catch (UnauthorizedAccessException ex)
      {
        // IIS account identity doesn't have permission to delete install.txt. Tell user to it manually.
        ClientMessage.Message += String.Format("<p>ATTENTION: You must manually delete the file <b>{0}</b> before continuing. (The following error occurred: {1})</p>", Utils.InstallFilePath, ex.Message);
        ClientMessage.Style = MessageStyle.Warning;
      }
    }

    private bool ValidateUserNameConformsToRequirements()
    {
      var userName = txtNewUserUserName.Text.Trim();

      if (UseEmailForAccountName && (!HelperFunctions.IsValidEmail(userName)))
      {
        // App is configured to use an e-mail address as the account name, but the name is not a valid e-mail.
        ClientMessage = new ClientMessageOptions
                          {
                            Title = Resources.GalleryServer.Validation_Summary_Text,
                            Message = Resources.GalleryServer.CreateAccount_Verification_Username_Not_Valid_Email_Text,
                            Style = MessageStyle.Error
                          };

        return false;
      }

      return true;
    }

    private void CreateAccount()
    {
      try
      {
        IUserAccount user = this.AddUser();

        ReportSuccess(user);
      }
      catch (MembershipCreateUserException ex)
      {
        // Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user
        // and - if it exists - the user album, but only if the user exists AND the error wasn't 'DuplicateUserName'.
        if ((ex.StatusCode != MembershipCreateStatus.DuplicateUserName) && (UserController.GetUser(this.txtNewUserUserName.Text.Trim(), false) != null))
        {
          DeleteUserAlbum();

          UserController.DeleteUser(this.txtNewUserEmail.Text);
        }

        this.DisplayErrorMessage(Resources.GalleryServer.Admin_Manage_Users_Cannot_Create_User_Msg, UserController.GetAddUserErrorMessage(ex.StatusCode));

        LogError(ex);
      }
      catch (Exception ex)
      {
        // Just in case we created the user and the exception occured at a later step, like adding the roles, delete the user
        // and - if it exists - the user album.
        DeleteUserAlbum();

        if (UserController.GetUser(this.txtNewUserUserName.Text.Trim(), false) != null)
        {
          UserController.DeleteUser(this.txtNewUserUserName.Text.Trim());
        }

        this.DisplayErrorMessage(Resources.GalleryServer.Admin_Manage_Users_Cannot_Create_User_Msg, ex.Message);

        LogError(ex);
      }
    }

    private void DeleteUserAlbum()
    {
      if (String.IsNullOrEmpty(this.txtNewUserUserName.Text))
        return;

      if (GallerySettings.EnableUserAlbum)
      {
        IAlbum album = null;

        try
        {
          IUserGalleryProfile profile = ProfileController.GetProfileForGallery(this.txtNewUserUserName.Text.Trim(), GalleryId);

          if (profile != null)
          {
            album = AlbumController.LoadAlbumInstance(profile.UserAlbumId);
          }
        }
        catch (InvalidAlbumException)
        {
          return;
        }

        if (album != null)
        {
          AlbumController.DeleteAlbum(album);
        }
      }
    }

    private void ReportSuccess(IUserAccount user)
    {
      string title = Resources.GalleryServer.CreateAccount_Success_Header_Text;

      string detailPendingNotification = String.Concat("<p>", Resources.GalleryServer.CreateAccount_Success_Detail1_Text, "</p>");
      detailPendingNotification += String.Concat(@"<p>", String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.CreateAccount_Success_Pending_Notification_Detail2_Text, user.Email), "</p>");
      detailPendingNotification += String.Concat(@"<p>", Resources.GalleryServer.CreateAccount_Success_Pending_Notification_Detail3_Text, "</p>");

      string detailPendingApproval = String.Concat("<p>", Resources.GalleryServer.CreateAccount_Success_Detail1_Text, "</p>");
      detailPendingApproval += String.Concat(@"<p>", String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.CreateAccount_Success_Pending_Approval_Detail2_Text), "</p>");
      detailPendingApproval += String.Concat(@"<p>", Resources.GalleryServer.CreateAccount_Success_Pending_Approval_Detail3_Text, "</p>");

      string detailActivated = String.Format(CultureInfo.InvariantCulture, @"<p>{0}</p><p><a href=""{1}"">{2}</a></p>",
                                             Resources.GalleryServer.CreateAccount_Success_Detail1_Text,
                                             Utils.GetCurrentPageUrl(),
                                             Resources.GalleryServer.CreateAccount_Gallery_Link_Text);

      if (EnableEmailVerification)
      {
        DisplaySuccessMessage(title, detailPendingNotification);
      }
      else if (RequireAdminApproval)
      {
        DisplaySuccessMessage(title, detailPendingApproval);
      }
      else
      {
        UserController.LogOnUser(user.UserName, GalleryId);

        if (EnableUserAlbum && (UserController.GetUserAlbumId(user.UserName, GalleryId) > int.MinValue))
        {
          detailActivated += String.Format(CultureInfo.InvariantCulture, @"<p><a href=""{0}"">{1}</a></p>",
                                           Utils.GetUrl(PageId.album, "aid={0}", UserController.GetUserAlbumId(user.UserName, GalleryId)),
                                           Resources.GalleryServer.CreateAccount_User_Album_Link_Text);
        }

        DisplaySuccessMessage(title, detailActivated);
      }

      pnlCreateUser.Visible = false;
    }

    private IUserAccount AddUser()
    {
      string newUserName = txtNewUserUserName.Text.Trim();
      string newUserPassword1 = txtNewUserPassword1.Text;
      string newUserPassword2 = txtNewUserPassword2.Text;

      if (newUserPassword1 != newUserPassword2)
        throw new WebException(Resources.GalleryServer.Admin_CreateAccount_Passwords_Not_Matching_Error);

      return UserController.CreateUser(newUserName, newUserPassword1, txtNewUserEmail.Text, GetRolesForNewUser(), true, GetGalleryId());
    }

    private int GetGalleryId()
    {
      return (ViewMode == PageMode.CreateAdmin ? GalleryController.GetTemplateGalleryId() : GalleryId);
    }

    private string[] GetRolesForNewUser()
    {
      return (ViewMode == PageMode.CreateAdmin ? new[] { RoleController.ValidateSysAdminRole(), RoleController.CreateAuthUsersRole() } : GallerySettings.DefaultRolesForUser);
    }

    private void DisplayErrorMessage(string title, string detail)
    {
      DisplayMessage(title, detail, MessageStyle.Error);
    }

    private void DisplaySuccessMessage(string title, string detail)
    {
      DisplayMessage(title, detail, MessageStyle.Success);
    }

    private void DisplayMessage(string title, string detail, MessageStyle iconStyle)
    {
      ClientMessage = new ClientMessageOptions
                        {
                          Title = title,
                          Message = detail,
                          Style = iconStyle,
                          AutoCloseDelay = 0
                        };
    }

    /// <summary>
    /// Update the user account to indicate the e-mail address has been validated. If admin approval is required, send an e-mail
    /// to the administrators. If not required, activate the account. Display results to user.
    /// </summary>
    private void ValidateUser()
    {
      if (!IsAnonymousUser)
      {
        // Log off user and reload page. This prevents the redirection that would occur after 
        // exiting the function. We need to reload page because some page state may have already 
        // been decided based on the logged on user at this point, so we just need to start over.
        UserController.LogOffUser();
        Utils.Redirect(Utils.GetCurrentPageUrl(true));
      }

      pnlCreateUser.Visible = false;

      try
      {
        string userName = HelperFunctions.Decrypt(Utils.GetQueryStringParameterString("verify"));

        UserController.UserEmailValidatedAfterCreation(userName, GalleryId);

        string title = Resources.GalleryServer.CreateAccount_Verification_Success_Header_Text;

        string detail = GetEmailValidatedUserMessageDetail(userName);

        DisplaySuccessMessage(title, detail);
      }
      catch (Exception ex)
      {
        LogError(ex);

        string failDetailText = String.Format(CultureInfo.CurrentCulture, "<p>{0}</p>", Resources.GalleryServer.CreateAccount_Verification_Fail_Detail_Text);

        DisplayErrorMessage(Resources.GalleryServer.CreateAccount_Verification_Fail_Header_Text, failDetailText);
      }
    }

    /// <summary>
    /// Gets the message to display to the user after she validated the account by clicking on the link in the verification
    /// e-mail.
    /// </summary>
    /// <param name="userName">The username whose account has been validated.</param>
    /// <returns>Returns an HTML-formatted string to display to the user.</returns>
    private string GetEmailValidatedUserMessageDetail(string userName)
    {
      if (GallerySettings.RequireApprovalForSelfRegisteredUser)
      {
        return String.Format(CultureInfo.InvariantCulture, @"<p>{0}</p>", Resources.GalleryServer.CreateAccount_Verification_Success_Needs_Admin_Approval_Detail_Text);
      }

      string detail = String.Format(CultureInfo.InvariantCulture, @"<p>{0}</p><p><a href=""{1}"">{2}</a></p>",
                                    Resources.GalleryServer.CreateAccount_Verification_Success_Detail_Text,
                                    Utils.GetCurrentPageUrl(),
                                    Resources.GalleryServer.CreateAccount_Gallery_Link_Text);

      if (GallerySettings.EnableUserAlbum && (UserController.GetUserAlbumId(userName, GalleryId) > int.MinValue))
      {
        detail += String.Format(CultureInfo.InvariantCulture, @"<p><a href=""{0}"">{1}</a></p>",
                                Utils.GetUrl(PageId.album, "aid={0}", UserController.GetUserAlbumId(userName, GalleryId)),
                                Resources.GalleryServer.CreateAccount_User_Album_Link_Text);
      }

      return detail;
    }

    #endregion
  }
}