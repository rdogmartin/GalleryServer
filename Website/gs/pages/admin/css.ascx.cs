using System;
using System.Web.UI.WebControls;
using GalleryServer.Business;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for viewing and editing the cascading style sheets used by the gallery.
  /// </summary>
  public partial class css : Pages.AdminPage
  {
    #region Properties
    
    private static readonly object _sharedLock = new object();

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!UserCanAdministerSite && UserCanAdministerGallery)
      {
        Utils.Redirect(PageId.admin_gallerysettings, "aid={0}", this.GetAlbumId());
      }

      this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

      ConfigureControlsEveryTime();

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }
    }

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
    /// Determines whether the event for the server control is passed up the page's UI server control hierarchy.
    /// </summary>
    /// <param name="source">The source of the event.</param>
    /// <param name="args">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
    /// <returns>
    /// true if the event has been canceled; otherwise, false. The default is false.
    /// </returns>
    protected override bool OnBubbleEvent(object source, EventArgs args)
    {
      //An event from the control has bubbled up.  If it's the Ok button, then run the
      //code to save the data to the database; otherwise ignore.
      Button btn = source as Button;
      if ((btn != null) && (((btn.ID == "btnOkTop") || (btn.ID == "btnOkBottom"))))
      {
        SaveSettings();
      }

      return true;
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Site_Settings_Css_Page_Header;
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Site_Settings_Css_Page_Header;

      txtCss.Text = AppSetting.Instance.CustomCss;
    }

    private void SaveSettings()
    {
      lock (_sharedLock)
      {
        AppSetting.Instance.CustomCss = txtCss.Text;
        AppSetting.Instance.Save();
      }

      ClientMessage = new ClientMessageOptions
      {
        Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
        Message = Resources.GalleryServer.Admin_Save_Success_Text,
        Style = MessageStyle.Success
      };
    }

    #endregion
  }
}