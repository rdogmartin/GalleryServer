using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Web.Controller;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering users.
  /// </summary>
  public partial class manageusers : Pages.AdminPage
  {
    #region Properties

    protected string ShowAlbumOwnerRolesLabel
    {
      get { return Resources.GalleryServer.Admin_User_Settings_Show_Album_Owner_Roles_Lbl; }
    }

    protected string QuestionAnswerEnabledMsg
    {
      get { return Resources.GalleryServer.Admin_Manage_Users_Question_Answer_Enabled_Msg.JsEncode(); }
    }

    protected string PwdResetDisabledMsg
    {
      get { return Resources.GalleryServer.Admin_Manage_Users_Pwd_Rest_Disabled_Msg.JsEncode(); }
    }

    protected string PwdRetrievalDisabledMsg
    {
      get { return Resources.GalleryServer.Admin_Manage_Users_Pwd_Retrieval_Disabled_Msg.JsEncode(); }
    }

    protected int MaxNumberOfUsersToRender
    {
      get { return GlobalConstants.MaxNumberOfUsersToDisplayOnManageUsersPage; }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Handles the Init event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Init(object sender, EventArgs e)
    {
      this.AdminHeaderPlaceHolder = phAdminHeader;
      this.AdminFooterPlaceHolder = phAdminFooter;
    }

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      // User must be a site or gallery admin.
      this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

      if (!UserCanAdministerSite && UserCanAdministerGallery && !AppSetting.Instance.AllowGalleryAdminToManageUsersAndRoles)
      {
        // If we get here, user is a gallery admin but site admin has disabled ability for them to manage users and roles.
        Utils.Redirect(PageId.album, "aid={0}", this.GetAlbumId());
      }

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }

      ConfigureControlsEveryTime();
    }

    /// <summary>
    /// Gets a JSON string containing a list of user names.
    /// </summary>
    /// <returns>System.String.</returns>
    protected string GetUserNames()
    {
      //var users = new[] { Resources.GalleryServer.Admin_Manage_Users_Add_User_Link_Text }
      //	.Concat(GetUsersCurrentUserCanView().Select(u => u.UserName)).ToArray();

      var users = GetUsersCurrentUserCanView().GetUserNames();

      return users.ToJson().JsEncode();
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Manage_Users_Page_Header;
    }

    private void ConfigureControlsEveryTime()
    {
      OkButtonIsVisible = false;

      this.PageTitle = Resources.GalleryServer.Admin_Manage_Users_Page_Header;

      GenerateRolesList();

      CheckForLockedUsers();
    }

    private void CheckForLockedUsers()
    {
      if (ClientMessage != null)
        return; // A message has already been specified, so don't overwrite it

      if (UserController.MembershipGsp.GetType().ToString() != GlobalConstants.DefaultMembershipProviderName)
        return; // Only look for locked users when using default membership provider. Too much uncertainty and possible perf issues with other providers.

      var lockedUserNames = String.Join(", ", GetUsersCurrentUserCanView().Where(u => u.IsLockedOut).Select(u => u.UserName).OrderBy(u => u));

      if (!String.IsNullOrWhiteSpace(lockedUserNames))
      {
        ClientMessage = new ClientMessageOptions
                          {
                            Title = Resources.GalleryServer.Admin_Manage_Users_Locked_User_Hdr,
                            Message = String.Concat(Resources.GalleryServer.Admin_Manage_Users_Locked_User_Dtl, lockedUserNames),
                            Style = MessageStyle.Info,
                            AutoCloseDelay = 0
                          };
      }
    }

    private void GenerateRolesList()
    {
      // Bind the list of roles to a hidden dropdown list. This will be used by javascript when building the roles multi-select.
      var roleListItems = new List<ListItem>();
      var defaultRoleListItems = new List<ListItem>();
      foreach (var role in GetRolesCurrentUserCanView())
      {
        roleListItems.Add(new ListItem(RoleController.ParseRoleNameFromGspRoleName(role.RoleName), role.RoleName)); // Don't need to HTML encode

        if (RoleController.IsRoleAnAlbumOwnerRole(role.RoleName) || RoleController.IsRoleAnAlbumOwnerTemplateRole(role.RoleName))
        {
          roleListItems[roleListItems.Count - 1].Attributes["class"] = "gsp_j_albumownerrole";
        }

        // Pre-select any roles that are specified in the DefaultRolesForUser setting. The admin may choose to de-select if desired,
        // but the default roles are still added by validation when saving the user.
        if (Array.IndexOf(GallerySettings.DefaultRolesForUser, role.RoleName) >= 0)
        {
          defaultRoleListItems.Add(roleListItems[roleListItems.Count - 1]);

          roleListItems[roleListItems.Count - 1].Selected = true;
          roleListItems[roleListItems.Count - 1].Attributes["disabled"] = "disabled";
        }
      }

      // Sort by putting default roles first, then the rest (I know it's ugly, but it works).
      var sortedList = roleListItems.OrderByDescending(r => Enumerable.Reverse(defaultRoleListItems).ToList().IndexOf(r));

      lbRoles.Items.AddRange(sortedList.ToArray());
    }

    #endregion
  }
}