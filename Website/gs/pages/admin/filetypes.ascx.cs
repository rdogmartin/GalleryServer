using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Events;

namespace GalleryServer.Web.gs.pages.admin
{
  /// <summary>
  /// A page-like user control for managing the file types.
  /// </summary>
  public partial class filetypes : Pages.AdminPage
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
      this.CheckUserSecurity(SecurityActions.AdministerSite);

      ConfigureControlsEveryTime();

      RegisterJavascript();

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }
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

    #region Functions
    private void ConfigureControlsFirstTime()
    {
      BindMimeTypes();

      AdminPageTitle = Resources.GalleryServer.Admin_File_Types_Page_Header;

      if (AppSetting.Instance.License.LicenseType == LicenseLevel.TrialExpired)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Site_Settings_ProductKey_NotEntered_Label,
          Message = Resources.GalleryServer.Admin_Need_Product_Key_Msg2,
          Style = MessageStyle.Info
        };

        OkButtonBottom.Enabled = false;
        OkButtonTop.Enabled = false;
      }
    }

    private void RegisterJavascript()
    {
      // Add link to file to support SlickGrid.
      var url = Utils.IsDebugEnabled ? Utils.GetUrl("/script/grid.js") : Utils.GetUrl("/script/grid.min.js");
      this.Page.Header.Controls.Add(new LiteralControl($"<script type='text/javascript' src='{url}'></script>"));
    }

    private void BindMimeTypes()
    {
      hdnMimeTypes.Value = Factory.LoadMimeTypes(GalleryId)
        .Select(mt => new Entity.MimeType()
        {
          Id = mt.MimeTypeId,
          Enabled = mt.AllowAddToGallery,
          IsDeleted = false,
          Extension = mt.Extension,
          FullType = mt.FullType
        }).ToJson();
    }

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_File_Types_Page_Header;

      // Cancel the AdminPage's configuration of linking the <Enter> key to the 'Save changes' button, since this prevents the user
      // from using the <Enter> key on the OK/Cancel buttons of the 'add mime type' dialog.
      this.Page.Form.DefaultButton = null;
    }

    private void SaveSettings()
    {
      var mimeTypes = hdnMimeTypes.Value.FromJson<Entity.MimeType[]>();

      // Loop through each MIME type entity. For each file extension, get the matching MIME type and add/edit/delete as needed.
      try
      {
        foreach (var mte in mimeTypes)
        {
          try
          {
            if (string.IsNullOrWhiteSpace(mte.Extension) || mte.Extension == ".")
            {
              throw new ArgumentException("No file extension was specified (e.g. .jpg, .mp4).");
            }

            var hasChanges = false;
            var mimeType = Factory.LoadMimeType(GalleryId, mte.Extension);

            if (mimeType == null)
            {
              // User is adding a MIME type.
              var extension = mte.Extension.StartsWith(".") ? mte.Extension : string.Concat(".", mte.Extension);

              mimeType = MimeType.CreateInstance(MimeTypeCategory.NotSet, extension);
              mimeType.MimeTypeId = int.MinValue;
              mimeType.MimeTypeGalleryId = int.MinValue;
              mimeType.GalleryId = GalleryId;
            }

            if (mte.IsDeleted)
            {
              mimeType.Delete();
            }
            else
            {
              if (mimeType.AllowAddToGallery != mte.Enabled)
              {
                mimeType.AllowAddToGallery = mte.Enabled;
                hasChanges = true;
              }

              if (mimeType.FullType != mte.FullType)
              {
                mimeType.FullType = mte.FullType;
                hasChanges = true;
              }

              if (hasChanges)
              {
                mimeType.Save();
              }
            }
          }
          catch (Exception ex)
          {
            ex.Data.Add("FileExtension", mte.Extension);
            ex.Data.Add("MimeType", mte.FullType);

            throw;
          }
        }

        CacheController.RemoveCache(CacheItem.MimeTypes);
        CacheController.RemoveCache(CacheItem.InflatedAlbums); // Old MIME types might be in here, so we need to purge

        BindMimeTypes();

        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
          Message = Resources.GalleryServer.Admin_Save_Success_Text,
          Style = MessageStyle.Success
        };
      }
      catch (Exception ex)
      {
        EventController.RecordError(ex, AppSetting.Instance, GalleryId, Factory.LoadGallerySettings());

        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Error_Hdr,
          Message = ex.Message,
          Style = MessageStyle.Error
        };

      }
    }

    #endregion
  }
}