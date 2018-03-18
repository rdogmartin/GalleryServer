using System;
using System.Globalization;
using System.Web.UI;
using GalleryServer.Business;

namespace GalleryServer.Web.gs.pages.admin
{
  /// <summary>
  /// A page-like user control for viewing and interacting with the media queue.
  /// </summary>
  public partial class mediaqueue : Pages.AdminPage
  {
    #region Properties

    protected string LoadingLbl => Resources.GalleryServer.Site_Loading;

    /// <summary>
    /// Gets the name of the cookie that stores the index of the currently selected tab.
    /// </summary>
    /// <value>A string.</value>
    protected string SelectedTabCookieName => string.Concat(cid, "_mqtb_cookie");

    protected string SiteOptionsTooltip => Resources.GalleryServer.Site_Options_Tooltip;

    protected string CurrentItemDetailsLbl => Resources.GalleryServer.Admin_MediaQueue_Current_Item_Details_Lbl;

    protected string CancelLbl => Resources.GalleryServer.Default_Task_Cancel_Button_Text;

    protected string StatusLbl => Resources.GalleryServer.Admin_MediaQueue_Status_Lbl;

    protected string AlbumLbl => Resources.GalleryServer.Admin_MediaQueue_Album_Lbl;

    protected string AssetLbl => Resources.GalleryServer.Admin_MediaQueue_Asset_Lbl;

    protected string ActionLbl => Resources.GalleryServer.Admin_MediaQueue_Action_Lbl;

    protected string AddedLbl => Resources.GalleryServer.Admin_MediaQueue_Added_Lbl;

    protected string StartedLbl => Resources.GalleryServer.Admin_MediaQueue_Started_Lbl;

    protected string DurationLbl => Resources.GalleryServer.Admin_MediaQueue_Duration_Lbl;

    protected string WtgRemoveTt => Resources.GalleryServer.Admin_MediaQueue_Wtg_Remove_Tt;

    protected string CmpRemoveTt => Resources.GalleryServer.Admin_MediaQueue_Cmp_Remove_Tt;

    /// <summary>
    /// Gets the text 'FFMPEG OUTPUT'
    /// </summary>
    protected string DetailLbl => Resources.GalleryServer.Admin_MediaQueue_Detail_Lbl;

    protected string DetailTt => Resources.GalleryServer.Admin_MediaQueue_Detail_Tt;

    protected string ToLbl => Resources.GalleryServer.Admin_MediaQueue_Cmp_Dur_To;

    #endregion

    #region Event handlers

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
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      this.CheckUserSecurity(SecurityActions.AdministerSite);

      ConfigureControlsEveryTime();

      RegisterJavascript();

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }

    }

    #endregion

    #region Functions

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_MediaQueue_Page_Header;
    }

    private void RegisterJavascript()
    {
      var url = Utils.IsDebugEnabled ? ResolveClientUrl("~/Scripts/jquery.signalR-2.2.2.js") : ResolveClientUrl("~/Scripts/jquery.signalR-2.2.2.min.js");
      var link = $"<script src='{url}'></script>\n";
      this.Page.Header.Controls.Add(new LiteralControl(link));

      link = $"<script src='{ResolveClientUrl("~/gs/signalr/hubs")}'></script>\n";
      this.Page.Header.Controls.Add(new LiteralControl(link));
    }

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_MediaQueue_Page_Header;

      OkButtonIsVisible = false;

      if (string.IsNullOrWhiteSpace(AppSetting.Instance.FFmpegPath))
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_MediaQueue_Ffmpeg_Not_Installed_Hdr,
          Message = Resources.GalleryServer.Admin_MediaQueue_Ffmpeg_Not_Installed_Bdy,
          Style = MessageStyle.Info
        };
      }

    }

      #endregion
  }
}