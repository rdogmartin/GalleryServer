using System;
using System.Collections;
using System.Globalization;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  ///   A page-like user control for administering galleries.
  /// </summary>
  public partial class galleries : AdminPage
  {
    #region Properties

    #endregion

    #region Protected Events

    /// <summary>
    ///   Handles the Init event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.EventArgs" /> instance containing the event data.
    /// </param>
    protected void Page_Init(object sender, EventArgs e)
    {
      AdminHeaderPlaceHolder = phAdminHeader;
      AdminFooterPlaceHolder = phAdminFooter;
    }

    /// <summary>
    ///   Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.EventArgs" /> instance containing the event data.
    /// </param>
    protected void Page_Load(object sender, EventArgs e)
    {
      CheckUserSecurity(SecurityActions.AdministerSite | SecurityActions.AdministerGallery);

      ConfigureControlsEveryTime();

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }
    }

    /// <summary>
    ///   Handles the RowDataBound event of the gvGalleries control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs" /> instance containing the event data.
    /// </param>
    protected void gvGalleries_RowDataBound(object sender, GridViewRowEventArgs e)
    {
      if (e.Row.RowType == DataControlRowType.DataRow)
      {
        if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
        {
          gvGalleriesRowDataBoundEditMode(e); // Current row is in edit mode
        }
        else if ((e.Row.RowState & DataControlRowState.Normal) == DataControlRowState.Normal)
        {
          gvGalleriesRowDataBoundViewMode(e); // Current row is in view mode
        }
      }
    }

    private void gvGalleriesRowDataBoundEditMode(GridViewRowEventArgs e)
    {
      DataBindMediaPathInGalleriesGridRow(e);
    }

    private void DataBindMediaPathInGalleriesGridRow(GridViewRowEventArgs e)
    {
      var lblMediaPath = (Label)e.Row.FindControl("lblMediaPath");

      if (lblMediaPath == null)
      {
        throw new WebException("Cannot find a Label with ID='lblMediaPath' in the current row of the GridView 'gvGalleries'.");
      }

      var gallery = (IGallery)e.Row.DataItem;
      IGallerySettings gallerySettings = Factory.LoadGallerySetting(gallery.GalleryId);

      lblMediaPath.Text = gallerySettings.MediaObjectPath;
    }

    private void gvGalleriesRowDataBoundViewMode(GridViewRowEventArgs e)
    {
      DataBindViewButtonInGalleriesGridRow(e);

      DataBindEditButtonInGalleriesGridRow(e);

      DataBindDeleteButtonInGalleriesGridRow(e);

      DataBindMediaPathInGalleriesGridRow(e);
    }

    private void DataBindViewButtonInGalleriesGridRow(GridViewRowEventArgs e)
    {
      // reference the View LinkButton
      var hlViewGallery = (HyperLink)e.Row.FindControl("hlViewGallery");

      if (hlViewGallery == null)
      {
        throw new WebException("Cannot find a LinkButton with ID='lbViewGallery' in the current row of the GridView 'gvGalleries'.");
      }

      // Get information about the product bound to the row
      var gallery = (IGallery)e.Row.DataItem;

      if (gallery.GalleryId == GalleryId)
      {
        hlViewGallery.NavigateUrl = Utils.GetCurrentPageUrl();
        hlViewGallery.Visible = true;
      }
    }

    private void DataBindEditButtonInGalleriesGridRow(GridViewRowEventArgs e)
    {
      // reference the View LinkButton
      var lbEditGallery = (LinkButton)e.Row.FindControl("lbEditGallery");

      if (lbEditGallery == null)
      {
        throw new WebException("Cannot find a LinkButton with ID='lbEditGallery' in the current row of the GridView 'gvGalleries'.");
      }
    }

    private void DataBindDeleteButtonInGalleriesGridRow(GridViewRowEventArgs e)
    {
      // reference the Delete LinkButton
      var lbDeleteGallery = (LinkButton)e.Row.FindControl("lbDeleteGallery");

      if (lbDeleteGallery == null)
      {
        throw new WebException("Cannot find a LinkButton with ID='lbDeleteGallery' in the current row of the GridView 'gvGalleries'.");
      }

      // Get information about the product bound to the row
      var gallery = (IGallery)e.Row.DataItem;

      if (gallery.GalleryId != GalleryId)
      {
        string msg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Settings_Delete_Gallery_Confirm_Text, gallery.Description.JsEncode());
        lbDeleteGallery.OnClientClick = String.Format(CultureInfo.InvariantCulture, "return confirm('{0}');", msg);
      }
    }

    /// <summary>
    ///   Handles the RowCommand event of the gvGalleries control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Web.UI.WebControls.GridViewCommandEventArgs" /> instance containing the event data.
    /// </param>
    protected void gvGalleries_RowCommand(object sender, GridViewCommandEventArgs e)
    {
      switch (e.CommandName)
      {
        case "Insert":
          {
            var txtInsert = gvGalleries.FooterRow.FindControl("txtDescriptionInsert") as TextBox;

            if (txtInsert == null)
            {
              throw new WebException("Could not find a TextBox named 'txtDescriptionInsert' in the GridView's row.");
            }

            if (String.IsNullOrEmpty(txtInsert.Text))
            {
              HandleGalleryEditFailure(Resources.GalleryServer.Admin_Gallery_Settings_Gallery_Description_Required);
            }
            else
            {
              IGallery newGallery = CreateGallery(txtInsert.Text);

              SetMediaObjectPathForNewGallery(newGallery);

              VerifyUserHasGalleryAdminPermission(newGallery);

              HandleGalleryEditSuccess(Resources.GalleryServer.Admin_Gallery_Settings_Gallery_Created_Success_Text);
            }

            break;
          }
      }

      gvGalleries.DataBind();
    }

    /// <summary>
    ///   Handles the Updating event of the odsGalleries control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Web.UI.WebControls.ObjectDataSourceMethodEventArgs" /> instance containing the event data.
    /// </param>
    protected void odsGalleries_Updating(object sender, ObjectDataSourceMethodEventArgs e)
    {
      foreach (DictionaryEntry entry in e.InputParameters)
      {
        var gallery = entry.Value as IGallery;
        if ((gallery != null) && String.IsNullOrEmpty(gallery.Description))
        {
          e.Cancel = true;

          ClientMessage = new ClientMessageOptions
                            {
                              Title = Resources.GalleryServer.Validation_Summary_Text,
                              Message = Resources.GalleryServer.Admin_Gallery_Settings_Gallery_Description_Required,
                              Style = MessageStyle.Error
                            };
        }
      }
    }

    /// <summary>
    ///   Handles the Updated event of the odsGalleries control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Web.UI.WebControls.ObjectDataSourceStatusEventArgs" /> instance containing the event data.
    /// </param>
    protected void odsGalleries_Updated(object sender, ObjectDataSourceStatusEventArgs e)
    {
      HandleGalleryEditSuccess(Resources.GalleryServer.Admin_Save_Success_Text);
    }

    /// <summary>
    ///   Handles the Deleting event of the odsGalleries control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Web.UI.WebControls.ObjectDataSourceMethodEventArgs" /> instance containing the event data.
    /// </param>
    protected void odsGalleries_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
    {
      // Make sure user isn't trying to delete the current gallery. Javascript on the client should have prevented us from getting to this point, but 
      // we check again as an extra safety measure.
      foreach (DictionaryEntry entry in e.InputParameters)
      {
        var rowGallery = entry.Value as IGallery;

        if (rowGallery == null)
          return;

        IGallery gallery;
        try
        {
          gallery = Factory.LoadGallery(rowGallery.GalleryId);
        }
        catch (InvalidGalleryException) { return; }

        if (gallery.GalleryId == GalleryId)
        {
          e.Cancel = true;

          ClientMessage = new ClientMessageOptions
                            {
                              Title = Resources.GalleryServer.Validation_Summary_Text,
                              Message = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Settings_Cannot_Delete_Current_Gallery_Text, gallery.Description.JsEncode()),
                              Style = MessageStyle.Error
                            };
        }
      }
    }

    /// <summary>
    ///   Handles the Deleted event of the odsGalleries control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Web.UI.WebControls.ObjectDataSourceStatusEventArgs" /> instance containing the event data.
    /// </param>
    protected void odsGalleries_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
    {
      HandleGalleryEditSuccess(Resources.GalleryServer.Admin_Gallery_Settings_Gallery_Deleted_Success_Text);
    }

    /// <summary>
    ///   Handles the Click event of the lbChangeGallery control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.EventArgs" /> instance containing the event data.
    /// </param>
    protected void lbChangeGallery_Click(object sender, EventArgs e)
    {
      DetectAndSaveChangedGallery();
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsFirstTime()
    {
      AdminPageTitle = Resources.GalleryServer.Admin_Gallery_Manager_Page_Header;

      OkButtonIsVisible = false;

      DataBindGalleriesComboBox();

      CheckForMessages();
    }

    private void DataBindGalleriesComboBox()
    {
      ddlCurrentGallery.Items.Clear();

      foreach (IGallery gallery in UserController.GetGalleriesCurrentUserCanAdminister())
      {
        var li = new ListItem(Utils.RemoveHtmlTags(gallery.Description), gallery.GalleryId.ToString(CultureInfo.InvariantCulture));

        if (gallery.GalleryId == GalleryId)
        {
          li.Selected = true;
        }

        ddlCurrentGallery.Items.Add(li);
      }
    }

    private void ConfigureControlsEveryTime()
    {
      PageTitle = Resources.GalleryServer.Admin_Gallery_Manager_Page_Header;
    }

    private void DetectAndSaveChangedGallery()
    {
      int galleryId = Convert.ToInt32(ddlCurrentGallery.SelectedValue, CultureInfo.InvariantCulture);

      if (GalleryId != galleryId)
      {
        // User wants to change the current gallery. First verify gallery exists and the user has permission, then update.
        IGallery gallery;
        try
        {
          gallery = Factory.LoadGallery(galleryId);
        }
        catch (InvalidGalleryException)
        {
          // Not a valid gallery. Set message and return.
          ClientMessage = new ClientMessageOptions
                            {
                              Title = Resources.GalleryServer.Validation_Summary_Text,
                              Message = "Invalid gallery.",
                              Style = MessageStyle.Error
                            };

          return;
        }

        if (UserController.GetGalleriesCurrentUserCanAdminister().Contains(gallery))
        {
          GalleryControlSettingsUpdateable.GalleryId = galleryId;
          GalleryControlSettingsUpdateable.AlbumId = null;
          GalleryControlSettingsUpdateable.MediaObjectId = null;
          GalleryControlSettingsUpdateable.Save();

          Factory.ClearGalleryControlSettingsCache();

          // Since we are changing galleries, we need to perform a redirect to get rid of the album ID from the old gallery that
          // is sitting in the query string.
          const MessageType msg = MessageType.GallerySuccessfullyChanged;

          Utils.Redirect(PageId.admin_galleries, "msg={0}", ((int)msg).ToString(CultureInfo.InvariantCulture));
        }
        else
        {
          // User does not have permission to change to this gallery. Set message and return.
          ClientMessage = new ClientMessageOptions
                            {
                              Title = Resources.GalleryServer.Validation_Summary_Text,
                              Message = "Invalid gallery.",
                              Style = MessageStyle.Error
                            };
        }
      }
      else
      {
        ClientMessage = new ClientMessageOptions
                          {
                            Title = Resources.GalleryServer.Validation_Summary_Text,
                            Message = Resources.GalleryServer.Admin_Gallery_Settings_Different_Gallery_Not_Selected_Text,
                            Style = MessageStyle.Info
                          };
      }
    }

    private static IGallery CreateGallery(string description)
    {
      IGallery gallery = Factory.CreateGalleryInstance();
      gallery.CreationDate = DateTime.UtcNow;
      gallery.Description = description;
      gallery.Save();

      return gallery;
    }

    private void VerifyUserHasGalleryAdminPermission(IGallery newGallery)
    {
      // If the current user is only a gallery admin, she won't have access to the new gallery, so we need to add the
      // new gallery to the gallery admin role she is in.
      if (!UserCanAdministerSite && UserCanAdministerGallery)
      {
        foreach (IGalleryServerRole role in RoleController.GetGalleryServerRolesForUser())
        {
          if (role.AllowAdministerGallery)
          {
            IAlbum rootAlbum = Factory.LoadRootAlbumInstance(newGallery.GalleryId);

            if (!role.RootAlbumIds.Contains(rootAlbum.Id))
            {
              role.RootAlbumIds.Add(rootAlbum.Id);
              role.Save();
            }
            break;
          }
        }
      }
    }

    private void HandleGalleryEditSuccess(string msg)
    {
      Factory.ClearGalleryCache();

      DataBindGalleriesComboBox();

      ClientMessage = new ClientMessageOptions
                        {
                          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
                          Message = msg,
                          Style = MessageStyle.Success
                        };
    }

    private void HandleGalleryEditFailure(string msg)
    {
      Factory.ClearGalleryCache();

      DataBindGalleriesComboBox();

      ClientMessage = new ClientMessageOptions
                        {
                          Title = Resources.GalleryServer.Validation_Summary_Text,
                          Message = msg,
                          Style = MessageStyle.Error
                        };
    }

    /// <summary>
    ///   Determine if there are any messages we need to display to the user.
    /// </summary>
    private void CheckForMessages()
    {
      if (ClientMessage != null && ClientMessage.MessageId == MessageType.GallerySuccessfullyChanged)
      {
        ClientMessage = new ClientMessageOptions
                          {
                            Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
                            Message = Resources.GalleryServer.Admin_Gallery_Settings_Gallery_Changed_Text,
                            Style = MessageStyle.Success
                          };
      }
    }

    /// <summary>
    ///   Sets the media object path for the new gallery to the path of the current gallery. The change is persisted to the data store.
    /// </summary>
    /// <param name="gallery">The gallery.</param>
    private void SetMediaObjectPathForNewGallery(IGallery gallery)
    {
      IGallerySettings gallerySettings = Factory.LoadGallerySetting(gallery.GalleryId, true);

      gallerySettings.MediaObjectPath = GallerySettings.MediaObjectPath;
      gallerySettings.ThumbnailPath = GallerySettings.ThumbnailPath;
      gallerySettings.OptimizedPath = GallerySettings.OptimizedPath;

      gallerySettings.Save();
    }

    #endregion
  }
}