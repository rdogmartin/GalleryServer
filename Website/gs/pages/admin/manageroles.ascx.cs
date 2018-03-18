using System;
using System.Collections.Generic;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Pages.Admin
{
	/// <summary>
	/// A page-like user control for administering roles.
	/// </summary>
	public partial class manageroles : Pages.AdminPage
	{
		#region Public Properties

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
			// Test #1: User must be a site or gallery admin.
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

		protected string GetPermissionTreeviewData()
		{
			TreeView tv = new TreeView();

			TreeNode nodeSiteAdmin = new TreeNode();
			nodeSiteAdmin.DataId = "AdministerSite";
			nodeSiteAdmin.Id = String.Concat(GspClientId, "_tv_", nodeSiteAdmin.DataId);
			nodeSiteAdmin.Text = Resources.GalleryServer.Admin_Manage_Roles_SiteAdmin_Text;
			nodeSiteAdmin.Expanded = true;

			TreeNode nodeGalleryAdmin = new TreeNode();
			nodeGalleryAdmin.DataId = "AdministerGallery";
			nodeGalleryAdmin.Id = String.Concat(GspClientId, "_tv_", nodeGalleryAdmin.DataId);
			nodeGalleryAdmin.Text = Resources.GalleryServer.Admin_Manage_Roles_GalleryAdmin_Text;
			nodeGalleryAdmin.Expanded = true;

			string[] perms = new[] { "ViewAlbumOrMediaObject", "ViewOriginalMediaObject", "AddChildAlbum", "AddMediaObject", "EditAlbum", "EditMediaObject", "DeleteChildAlbum", "DeleteMediaObject", "Synchronize" };
			const string resourcePrefix = "Admin_Manage_Roles_";
			const string headerSuffix = "_Text";

			foreach (string perm in perms)
			{
				TreeNode n = new TreeNode();
				n.DataId = perm;
				n.Id = String.Concat(GspClientId, "_tv_", n.DataId);
				n.Text = Resources.GalleryServer.ResourceManager.GetString(String.Concat(resourcePrefix, perm, headerSuffix));
				nodeGalleryAdmin.Nodes.Add(n);
			}

			TreeNode nodeWtrmk = new TreeNode();
			nodeWtrmk.DataId = "HideWatermark";
			nodeWtrmk.Id = String.Concat(GspClientId, "_tv_", nodeWtrmk.DataId);
			nodeWtrmk.Text = Resources.GalleryServer.Admin_Manage_Roles_HideWatermark_Text;

			nodeSiteAdmin.Nodes.Add(nodeGalleryAdmin);
			tv.Nodes.Add(nodeSiteAdmin);
			tv.Nodes.Add(nodeWtrmk);

			return tv.ToJson();
		}

		protected string GetRoleNames()
		{
			List<Role> roles = new List<Role>();
			var gspRoles = GetRolesCurrentUserCanView();
			const int maxNRolesPerformanceLimit = 30; // We don't include a list of members when the roles count exceeds this value because it is processor intensive
			bool includeMembers = (gspRoles.Count <= maxNRolesPerformanceLimit);

			roles.Add(new Role { Name = Resources.GalleryServer.Admin_Manage_Roles_Create_Role_Lbl, IsNew = true });

			foreach (IGalleryServerRole r in gspRoles)
			{
				Role role = new Role() { Name = r.RoleName, IsOwner = RoleController.IsRoleAnAlbumOwnerRole(r.RoleName) || RoleController.IsRoleAnAlbumOwnerTemplateRole(r.RoleName) };
				if (includeMembers)
				{
					role.Members = RoleController.GetUsersInRole(r.RoleName);
				}

				roles.Add(role);
			}

			return roles.ToArray().ToJson().JsEncode();
		}

		#endregion

		#region Private Methods

		private void ConfigureControlsFirstTime()
		{
			AdminPageTitle = Resources.GalleryServer.Admin_Manage_Roles_Page_Header;
		}

		private void ConfigureControlsEveryTime()
		{
			OkButtonIsVisible = false;

			if (AppSetting.Instance.License.LicenseType == LicenseLevel.TrialExpired)
			{
				ClientMessage = new ClientMessageOptions { Title = Resources.GalleryServer.Admin_Site_Settings_ProductKey_NotEntered_Label, Message = Resources.GalleryServer.Admin_Need_Product_Key_Msg2, Style = MessageStyle.Info };

				OkButtonBottom.Enabled = false;
				OkButtonTop.Enabled = false;
			}

			this.PageTitle = Resources.GalleryServer.Admin_Manage_Roles_Page_Header;
		}

		#endregion
	}
}