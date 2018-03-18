using System;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for interacting with the application's event log.
  /// </summary>
  public partial class eventlog : Pages.AdminPage
  {
    #region Protected Events

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
      this.CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

      if (!IsPostBack)
      {
        ValidateErrorLog();

        CacheController.RemoveCache(CacheItem.AppEvents);

        ConfigureControlsFirstPageLoad();

        BindData();
      }

      ConfigureControlsEveryPageLoad();
    }

    /// <summary>
    /// Handles the Click event of the btnClearLog control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void btnClearLog_Click(object sender, EventArgs e)
    {
      // When the user is a sys admin, delete all errors. When user is a gallery admin, just delete errors the user
      // has permission to administer.
      if (UserCanAdministerSite)
      {
        foreach (IGallery gallery in Factory.LoadGalleries())
        {
          EventController.ClearEventLog(gallery.GalleryId);
        }
      }
      else if (UserCanAdministerGallery)
      {
        foreach (IGallery gallery in UserController.GetGalleriesCurrentUserCanAdminister())
        {
          EventController.ClearEventLog(gallery.GalleryId);
        }
      }

      CacheController.RemoveCache(CacheItem.AppEvents);

      BindData();
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsFirstPageLoad()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Site_Settings_Event_Log_Page_Header;
      OkButtonIsVisible = false;
    }

    private void ConfigureControlsEveryPageLoad()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Site_Settings_Event_Log_Page_Header;
    }

    private void BindData()
    {
      rptr.DataSource = GetAppEvents();
      rptr.DataBind();
    }

    /// <summary>
    /// Gets the events the current user has permission to view. Site admins can see all events. Gallery admins can see
    /// events they are the admin for in addition to the system events (those associated with the template gallery because
    /// a more specific gallery ID was unknown or not applicable at the time the event was recorded).
    /// </summary>
    /// <returns>An instance of <see cref="IEventCollection" />.</returns>
    private IEventCollection GetAppEvents()
    {
      var allEvents = Factory.GetAppEvents();

      if (UserCanAdministerSite)
      {
        return allEvents;
      }

      var events = Factory.CreateEventCollection();

      foreach (var gallery in UserController.GetGalleriesCurrentUserCanAdminister())
      {
        events.AddRange(allEvents.FindAllForGallery(gallery.GalleryId));
      }

      // Add system events (these are assigned to the template gallery)
      events.AddRange(allEvents.FindAllForGallery(Factory.GetTemplateGalleryId()));

      return events;
    }

    /// <summary>
    /// Remove events if needed to ensure log does not exceed max log size. Normally the log size is validated each time an event
    /// occurs, but we run it here in case the user just reduced the log size setting.
    /// </summary>
    private static void ValidateErrorLog()
    {
      int numItemsDeleted = EventController.ValidateLogSize(AppSetting.Instance.MaxNumberErrorItems);
    }

    #endregion
  }
}