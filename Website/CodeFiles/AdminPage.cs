using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Controls.Admin;

namespace GalleryServer.Web.Pages
{
	/// <summary>
	/// The base class that is used for administration pages.
	/// </summary>
	public abstract class AdminPage : Pages.GalleryPage
	{
		#region Private Fields

		private PlaceHolder _phAdminHeader;
		private Controls.Admin.adminheader _adminHeader;
		private PlaceHolder _phAdminFooter;
		private Controls.Admin.adminfooter _adminFooter;
		//private Controls.Admin.adminmenu _adminMenu;
		private IGallerySettings _gallerySettings;
		private IGalleryControlSettings _galleryControlSettings;
		private List<String> _usersWithAdminPermission;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AdminPage"/> class.
		/// </summary>
		protected AdminPage()
		{
			//this.Init += AdminPage_Init;
			this.Load += AdminPage_Load;
			//this.BeforeHeaderControlsAdded += AdminPage_BeforeHeaderControlsAdded;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a writable instance of gallery settings.
		/// </summary>
		/// <value>A writable instance of gallery settings.</value>
		public IGallerySettings GallerySettingsUpdateable
		{
			get
			{
				if (_gallerySettings == null)
				{
					_gallerySettings = Factory.LoadGallerySetting(GalleryId, true);
				}

				return _gallerySettings;
			}
		}

		/// <summary>
		/// Gets a writable instance of gallery settings.
		/// </summary>
		/// <value>A writable instance of gallery settings.</value>
		public IGalleryControlSettings GalleryControlSettingsUpdateable
		{
			get
			{
				if (_galleryControlSettings == null)
				{
					_galleryControlSettings = Factory.LoadGalleryControlSetting(this.GalleryControl.ControlId, true);
				}

				return _galleryControlSettings;
			}
		}

		/// <summary>
		/// Gets or sets the location for the <see cref="adminheader"/> user control. Classes that inherit 
		/// <see cref="Pages.AdminPage"/> should set this property to the <see cref="PlaceHolder"/> that is to contain
		/// the admin header control. If this property is not assigned by the inheriting class, the admin header control
		/// is not added to the page output.
		/// </summary>
		/// <value>The <see cref="adminheader"/> user control.</value>
		public PlaceHolder AdminHeaderPlaceHolder
		{
			get
			{
				return this._phAdminHeader;
			}
			set
			{
				this._phAdminHeader = value;
			}
		}

		/// <summary>
		/// Gets the admin header user control that is rendered near the top of the administration pages. This control contains the 
		/// page title and the top Save/Cancel buttons. (The bottom Save/Cancel buttons are in the <see cref="adminfooter"/> user control.
		/// </summary>
		/// <value>The admin header user control that is rendered near the top of the administration pages.</value>
		public Controls.Admin.adminheader AdminHeader
		{
			get
			{
				return this._adminHeader;
			}
		}

		/// <summary>
		/// Gets or sets the location for the <see cref="adminfooter"/> user control. Classes that inherit 
		/// <see cref="Pages.AdminPage"/> should set this property to the <see cref="PlaceHolder"/> that is to contain
		/// the admin footer control. If this property is not assigned by the inheriting class, the admin footer control
		/// is not added to the page output.
		/// </summary>
		/// <value>The <see cref="adminfooter"/> user control.</value>
		public PlaceHolder AdminFooterPlaceHolder
		{
			get
			{
				return this._phAdminFooter;
			}
			set
			{
				this._phAdminFooter = value;
			}
		}

		/// <summary>
		/// Gets the admin footer user control that is rendered near the bottom of the administration pages. This control contains the 
		/// bottom Save/Cancel buttons. (The top Save/Cancel buttons are in the <see cref="adminheader"/> user control.
		/// </summary>
		/// <value>The admin footer user control that is rendered near the bottom of the administration pages.</value>
		public Controls.Admin.adminfooter AdminFooter
		{
			get
			{
				return this._adminFooter;
			}
		}

		/// <summary>
		/// Gets / sets the page title text (e.g. Site Settings - General).
		/// </summary>
		public string AdminPageTitle
		{
			get
			{
				return this._adminHeader.AdminPageTitle;
			}
			set
			{
				this._adminHeader.AdminPageTitle = value;
			}
		}

		/// <summary>
		/// Gets / sets the text that appears on the top and bottom Ok buttons on the page. This is rendered as the value
		/// attribute of the input HTML tag.
		/// </summary>
		public string OkButtonText
		{
			get
			{
				return this.AdminHeader.OkButtonText;
			}
			set
			{
				this.AdminHeader.OkButtonText = value;
				this.AdminFooter.OkButtonText = value;
			}
		}

		/// <summary>
		/// Gets / sets the ToolTip for the top and bottom Ok buttons on the page. The ToolTip is rendered as 
		/// the title attribute of the input HTML tag.
		/// </summary>
		public string OkButtonToolTip
		{
			get
			{
				return this.AdminHeader.OkButtonToolTip;
			}
			set
			{
				this.AdminHeader.OkButtonToolTip = value;
				this.AdminFooter.OkButtonToolTip = value;
			}
		}

		/// <summary>
		/// Gets / sets the visibility of the top and bottom Ok buttons on the page. When true, the buttons
		/// are visible. When false, they are not visible (not rendered in the page output.)
		/// </summary>
		public bool OkButtonIsVisible
		{
			get
			{
				return this.AdminHeader.OkButtonIsVisible;
			}
			set
			{
				this.AdminHeader.OkButtonIsVisible = value;
				this.AdminFooter.OkButtonIsVisible = value;
			}
		}

		/// <summary>
		/// Gets a reference to the top button that initiates the completion of the task.
		/// </summary>
		public Button OkButtonTop
		{
			get
			{
				return this.AdminHeader.OkButtonTop;
			}
		}

		/// <summary>
		/// Gets a reference to the bottom button that initiates the completion of the task.
		/// </summary>
		public Button OkButtonBottom
		{
			get
			{
				return this.AdminFooter.OkButtonBottom;
			}
		}

		/// <summary>
		/// Gets the list of site administrators and gallery administrators for the current gallery. That is, it 
		/// returns the user names of accounts belonging to roles with AllowAdministerSite or AllowAdministerGallery permission.
		/// </summary>
		/// <value>The list of site and gallery administrators.</value>
		public List<String> UsersWithAdminPermission
		{
			get
			{
				if (this._usersWithAdminPermission == null)
				{
					this._usersWithAdminPermission = new List<string>();

					foreach (var role in RoleController.GetGalleryServerRoles())
					{
						if (role.AllowAdministerSite || role.AllowAdministerGallery && role.Galleries.Any(g => g.GalleryId == GalleryId))
						{
							foreach (var userName in RoleController.GetUsersInRole(role.RoleName).Where(userName => !this._usersWithAdminPermission.Contains(userName)))
							{
								this._usersWithAdminPermission.Add(userName);
							}
						}
					}
				}

				return this._usersWithAdminPermission;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the logged on user can add, edit, or delete users and roles. Returns true when the user is a site administrator
		/// and - for gallery admins - when the application setting <see cref="IAppSetting.AllowGalleryAdminToManageUsersAndRoles" /> is <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the logged on user can add, edit, or delete users and roles; otherwise, <c>false</c>.
		/// </value>
		public bool UserCanEditUsersAndRoles
		{
			get
			{
				return UserCanAdministerSite || (!UserCanAdministerSite && UserCanAdministerGallery && AppSetting.Instance.AllowGalleryAdminToManageUsersAndRoles);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the logged on user can view users and roles who do not have access to the current gallery. Returns true when 
		/// the user is a site administrator and - for gallery admins - when the application setting <see cref="IAppSetting.AllowGalleryAdminToViewAllUsersAndRoles" />
		/// is <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the logged on user can view users and roles who do not have access to the current gallery; otherwise <c>false</c>.
		/// </value>
		public bool UserCanViewUsersAndRolesInOtherGalleries
		{
			get
			{
				return UserCanAdministerSite || (!UserCanAdministerSite && UserCanAdministerGallery && AppSetting.Instance.AllowGalleryAdminToViewAllUsersAndRoles);
			}
		}

		#endregion

		#region Event Handlers

		void AdminPage_Load(object sender, EventArgs e)
		{
			ValidateRequest();

			AddUserControls();

			ConfigureControls();

			AddFooter();
		}

		//protected void AdminPage_BeforeHeaderControlsAdded(object sender, EventArgs e)
		//{
		//	// Add the admin menu to the page. Note that if you use any index other than 0 in the AddAt method, the viewstate
		//	// is not preserved across postbacks. This is the reason why the <see cref="BeforeHeaderControlsAdded"/> event was created in 
		//	// <see cref="GalleryPage"/> and handled here. We need to add the admin menu *before* <see cref="GalleryPage"/> adds the album breadcrumb
		//	// menu and the gallery header.
		//	Controls.Admin.adminmenu adminMenu = (Controls.Admin.adminmenu)LoadControl(Utils.GetUrl("/controls/admin/adminmenu.ascx"));
		//	this._adminMenu = adminMenu;
		//	this.Controls.AddAt(0, adminMenu);
		//	//this.Controls.AddAt(Controls.IndexOf(AlbumMenu) + 1, adminMenu); // Do not use: viewstate is not preserved
		//}

		#endregion

		#region Private Methods

		private void AddUserControls()
		{
			Controls.Admin.adminheader adminHeader = (Controls.Admin.adminheader)LoadControl(Utils.GetUrl("/controls/admin/adminheader.ascx"));
			this._adminHeader = adminHeader;
			if (this.AdminHeaderPlaceHolder != null)
				this.AdminHeaderPlaceHolder.Controls.Add(adminHeader);

			Controls.Admin.adminfooter adminFooter = (Controls.Admin.adminfooter)LoadControl(Utils.GetUrl("/controls/admin/adminfooter.ascx"));
			this._adminFooter = adminFooter;
			if (this.AdminFooterPlaceHolder != null)
				this.AdminFooterPlaceHolder.Controls.Add(adminFooter);
		}

		private void ConfigureControls()
		{
			if ((this.AdminHeaderPlaceHolder != null) && (this.AdminHeader != null) && (this.AdminHeader.OkButtonTop != null))
				this.Page.Form.DefaultButton = this.AdminHeader.OkButtonTop.UniqueID;
		}

		/// <summary>
		/// Verify that the inferred gallery ID is the same as the one specified for this page.
		/// If not, remove the aid parameter and reload page. This helps prevent the situation
		/// where a page is assigned to a particular gallery but the album ID specified in the
		/// URL belongs to another gallery, causing the page to render with settings for the 
		/// other gallery, potentially confusing the admin.
		/// </summary>
		private void ValidateRequest()
		{
			// If the inferred gallery ID is different than the one specified for this page,
			// remove the aid parameter and reload page.
			if (GalleryId != GalleryControl.GalleryId)
			{
				string url = Utils.RemoveQueryStringParameter(Request.Url.PathAndQuery, "aid");

				Utils.Redirect(url);
			}

			// If the album ID in the query string is set to int.MinValue, remove it and reload the page.
			// I don't think there are valid reasons for ever having the aid parameter set to this value,
			// so someday we might want to change the logic so this value never becomes part of the query
			// string in the first place.
			if (Utils.IsQueryStringParameterPresent("aid") && Utils.GetQueryStringParameterInt32("aid") == int.MinValue)
			{
				string url = Utils.RemoveQueryStringParameter(Request.Url.PathAndQuery, "aid");

				Utils.Redirect(url);
			}
		}

		private void AddFooter()
		{
			// Add the GS version info to the end
			if (this.AdminFooterPlaceHolder != null)
			{
				var license = AppSetting.Instance.License.LicenseType;
				if (license == LicenseLevel.NotSet || license == LicenseLevel.Free)
					AdminFooterPlaceHolder.Controls.Add(GsLogo);
				
				AdminFooterPlaceHolder.Controls.Add(new LiteralControl(string.Format("<p class='gsp_textcenter'>Gallery Server {0}</p>", Utils.GetGalleryServerVersion())));
			}
		}

		#endregion
	}
}
