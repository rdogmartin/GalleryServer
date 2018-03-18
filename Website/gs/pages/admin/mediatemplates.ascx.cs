using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering media templates.
  /// </summary>
  public partial class mediatemplates : Pages.AdminPage
  {
    #region Private Fields / Enums

    private enum PageMode
    {
      Unknown = 0,
      Edit,
      Insert
    }

    #endregion

    #region Properties

    private PageMode ViewMode
    {
      get
      {
        object viewStateValue = ViewState["ViewMode"];

        return (viewStateValue != null ? (PageMode)viewStateValue : PageMode.Unknown);
      }
      set
      {
        ViewState["ViewMode"] = value;
      }
    }

    private IMediaTemplate CurrentMediaTemplate
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the name of the cookie that stores the index of the currently selected tab.
    /// </summary>
    /// <value>A string.</value>
    protected string SelectedTabCookieName { get { return String.Concat(cid, "_media_tmpl_cookie"); } }

    #endregion

    #region Event Handlers

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
      if (!UserCanAdministerSite && UserCanAdministerGallery)
      {
        Utils.Redirect(PageId.admin_gallerysettings, "aid={0}", this.GetAlbumId());
      }

      this.CheckUserSecurity(SecurityActions.AdministerSite);

      if (!IsPostBack)
      {
        ConfigureControlsFirstTime();
      }

      ConfigureControlsEveryTime();

    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the ddlMimeType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void ddlMimeType_SelectedIndexChanged(object sender, EventArgs e)
    {
      BindBrowserIdDropDownList();

      CurrentMediaTemplate = GetSelectedMediaTemplate();

      BindMediaTemplate();
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the ddlBrowserId control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void ddlBrowserId_SelectedIndexChanged(object sender, EventArgs e)
    {
      CurrentMediaTemplate = GetSelectedMediaTemplate();
      BindMediaTemplate();
    }

    /// <summary>
    /// Handles the Click event of the lbCreate control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void lbCreate_Click(object sender, EventArgs e)
    {
      ViewMode = PageMode.Insert;

      var tmplCopy = CurrentMediaTemplate.Copy();

      tmplCopy.BrowserId = GenerateUniqueTemplateName();

      CurrentMediaTemplate = tmplCopy;

      ddlBrowserId.Items.Add(new ListItem(tmplCopy.BrowserId, int.MinValue.ToString(CultureInfo.InvariantCulture)));
      ddlBrowserId.SelectedIndex = ddlBrowserId.Items.Count - 1;

      btnDelete.Enabled = !CurrentMediaTemplate.IsNew;

      BindMediaTemplate();
    }

    /// <summary>
    /// Handles the Click event of the btnSave control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnSave_Click(object sender, EventArgs e)
    {
      CurrentMediaTemplate = GetSelectedMediaTemplate();

      if (ValidateTemplateBeforeSave())
      {
        UnbindMediaTemplate();
        CurrentMediaTemplate.Save();

        if (ddlMimeType.SelectedValue != CurrentMediaTemplate.MimeType)
        {
          // The user changed the MIME type, so rebind the MIME type dropdown.
          BindMimeTypeDropDownList();
          ddlMimeType.SelectedIndex = ddlMimeType.Items.IndexOf(ddlMimeType.Items.FindByValue(CurrentMediaTemplate.MimeType.ToString(CultureInfo.InvariantCulture)));
        }

        BindBrowserIdDropDownList();
        ddlBrowserId.SelectedIndex = ddlBrowserId.Items.IndexOf(ddlBrowserId.Items.FindByValue(CurrentMediaTemplate.MediaTemplateId.ToString(CultureInfo.InvariantCulture)));
        ViewMode = PageMode.Edit;

        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
          Message = Resources.GalleryServer.Admin_Save_Success_Text,
          Style = MessageStyle.Success
        };
      }

      btnDelete.Enabled = !CurrentMediaTemplate.IsNew;
    }

    /// <summary>
    /// Handles the Click event of the btnCancel control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnCancel_Click(object sender, EventArgs e)
    {
      if (ViewMode == PageMode.Insert)
        BindBrowserIdDropDownList();

      CurrentMediaTemplate = GetSelectedMediaTemplate();
      BindMediaTemplate();
      ViewMode = PageMode.Edit;
    }

    /// <summary>
    /// Handles the Click event of the btnDelete control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnDelete_Click(object sender, EventArgs e)
    {
      CurrentMediaTemplate = GetSelectedMediaTemplate();

      if (ValidateDeleteTemplate())
      {
        CurrentMediaTemplate.Delete();
        BindBrowserIdDropDownList();

        if (ddlBrowserId.Items.Count == 0)
        {
          // User deleted last template for a MIME type. Re-bind the MIME types dropdown.
          BindMimeTypeDropDownList();
          BindBrowserIdDropDownList();
          CurrentMediaTemplate = LoadDefaultMediaTemplate();
        }
        else
        {
          CurrentMediaTemplate = GetSelectedMediaTemplate();
        }

        BindMediaTemplate();
        ViewMode = PageMode.Edit;

        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
          Message = Resources.GalleryServer.Admin_Templates_Deleted_Msg,
          Style = MessageStyle.Success
        };
      }
    }

    private bool ValidateDeleteTemplate()
    {
      var isDefault = CurrentMediaTemplate.BrowserId.Equals("default", StringComparison.OrdinalIgnoreCase);
      var nonDefaultTmplExist = Factory.LoadMediaTemplates().Any(mt => mt.MimeType == CurrentMediaTemplate.MimeType && mt.BrowserId != "default");

      if (isDefault && nonDefaultTmplExist)
      {
        ClientMessage = new ClientMessageOptions
                          {
                            Title = Resources.GalleryServer.Validation_Summary_Text,
                            Message = Resources.GalleryServer.Admin_Media_Templates_Cannot_Delete_Default_Tmpl_Msg,
                            Style = MessageStyle.Error
                          };

        return false;
      }

      return true;
    }

    #endregion

    #region Functions

    private void ConfigureControlsFirstTime()
    {
      ViewMode = PageMode.Edit;
      OkButtonIsVisible = false;

      BindDropDownLists();

      CurrentMediaTemplate = LoadDefaultMediaTemplate();

      BindMediaTemplate();

      AdminPageTitle = Resources.GalleryServer.Admin_Media_Templates_Page_Header;
    }

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_Media_Templates_Page_Header;

      CurrentMediaTemplate = GetSelectedMediaTemplate();
    }

    #endregion

    private void BindDropDownLists()
    {
      BindMimeTypeDropDownList();

      BindBrowserIdDropDownList();
    }

    private void BindMimeTypeDropDownList()
    {
      var mimeTypes = Factory.LoadMediaTemplates().Select(mt => mt.MimeType).Distinct().ToArray();

      Array.Sort(mimeTypes);
      ddlMimeType.DataSource = mimeTypes;
      ddlMimeType.DataBind();
    }

    private void BindBrowserIdDropDownList()
    {
      var browserIds = Factory.LoadMediaTemplates()
        .Where(mt => mt.MimeType == ddlMimeType.SelectedValue)
        .Select(mt => new KeyValuePair<int, string>(mt.MediaTemplateId, mt.BrowserId));

      ddlBrowserId.DataTextField = "Value";
      ddlBrowserId.DataValueField = "Key";
      ddlBrowserId.DataSource = browserIds;
      ddlBrowserId.DataBind();
    }

    /// <summary>
    /// Gets the template that matches the selected item in the dropdown list.
    /// </summary>
    /// <returns>An instance of <see cref="IMediaTemplate" />.</returns>
    private IMediaTemplate GetSelectedMediaTemplate()
    {
      if (!String.IsNullOrWhiteSpace(ddlBrowserId.SelectedValue) && ddlBrowserId.SelectedValue != int.MinValue.ToString(CultureInfo.InvariantCulture))
      {
        return Factory.LoadMediaTemplates().FirstOrDefault(mt => mt.MediaTemplateId == Convert.ToInt32(ddlBrowserId.SelectedValue, CultureInfo.InvariantCulture));
      }
      else
      {
        return CreateEmptyMediaTemplate(ddlMimeType.SelectedValue.Trim());
      }
    }

    private static IMediaTemplate CreateEmptyMediaTemplate(string mimeType)
    {
      var mt = Factory.CreateEmptyMediaTemplate();

      mt.MediaTemplateId = int.MinValue;
      mt.MimeType = mimeType;
      mt.BrowserId = "default";
      mt.HtmlTemplate = String.Empty;
      mt.ScriptTemplate = String.Empty;

      return mt;
    }

    /// <summary>
    /// Gets the default UI template for the album. Guaranteed to not return null.
    /// </summary>
    /// <returns>Returns an instance of <see cref="IUiTemplate" />.</returns>
    /// <exception cref="WebException">Thrown when no UI template exists in the data store having 
    /// TemplateType='Album' and TemplateName='Default'</exception>
    private IMediaTemplate LoadDefaultMediaTemplate()
    {
      var tmpl = Factory.LoadMediaTemplates().FirstOrDefault(mt => mt.MimeType == ddlMimeType.SelectedValue);

      if (tmpl == null)
        return CreateEmptyMediaTemplate(ddlMimeType.SelectedValue.Trim());

      return tmpl;
    }

    /// <summary>
    /// Copies the data from <see cref="CurrentMediaTemplate" /> to the relevant web form controls.
    /// </summary>
    private void BindMediaTemplate()
    {
      txtMimeType.Text = CurrentMediaTemplate.MimeType;
      txtBrowserId.Text = CurrentMediaTemplate.BrowserId;
      txtHtml.Text = CurrentMediaTemplate.HtmlTemplate;
      txtScript.Text = CurrentMediaTemplate.ScriptTemplate;

      txtBrowserId.Enabled = !txtBrowserId.Text.Equals("default", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Copies the data from the web form to the relevant properties of the <see cref="CurrentMediaTemplate" /> property.
    /// </summary>
    private void UnbindMediaTemplate()
    {
      CurrentMediaTemplate.MimeType = txtMimeType.Text.Trim();
      CurrentMediaTemplate.BrowserId = txtBrowserId.Text.Trim();
      CurrentMediaTemplate.HtmlTemplate = txtHtml.Text.Trim();
      CurrentMediaTemplate.ScriptTemplate = txtScript.Text.Trim();
    }

    private string GenerateUniqueTemplateName()
    {
      var tmplNames = from ListItem item in ddlBrowserId.Items select item.Text;

      string proposedName = String.Concat(CurrentMediaTemplate.BrowserId, " (copy)");
      int counter = 1;

      while (tmplNames.Contains(proposedName))
      {
        // There is already a template with our proposed name. We need to strip off the
        // previous suffix and try again.
        proposedName = proposedName.Remove(proposedName.LastIndexOf(" (copy", StringComparison.Ordinal));

        // Generate the new suffix to append to the filename (e.g. "(3)")
        proposedName = String.Concat(proposedName, " (copy ", counter, ")");

        counter++;
      }

      return proposedName;
    }

    /// <summary>
    /// Returns <c>true</c> when the media template can be saved; otherwise <c>false</c>. When it fails validation,
    /// a message to the user is automatically generated. Enforeces these rules:
    /// 1. Cannot save a template that has the same MIME type and browser ID as another one.
    /// 2. When creating a template, the MIME type must exist in the gsp_MimeType table.
    /// 3. The first template for a new MIME type must have a browser ID "default".
    /// </summary>
    private bool ValidateTemplateBeforeSave()
    {
      var mimeType = txtMimeType.Text.Trim();
      var browserId = txtBrowserId.Text.Trim();

      // TEST 1: Cannot save a template that has the same MIME type and browser ID as another one.
      if (Factory.LoadMediaTemplates().Any(mt =>
                                            mt.MediaTemplateId != CurrentMediaTemplate.MediaTemplateId &&
                                            mt.MimeType == mimeType &&
                                            mt.BrowserId == browserId))
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Validation_Summary_Text,
          Message = Resources.GalleryServer.Admin_Templates_Cannot_Save_Duplicate_Name_Msg,
          Style = MessageStyle.Error
        };

        return false;
      }

      var firstTmplForMimeType = Factory.LoadMediaTemplates().All(mt => mt.MimeType != mimeType);

      if (!firstTmplForMimeType)
      {
        return true;
      }

      // TEST 2. When creating a template, the MIME type must exist in the gsp_MimeType table.
      if (!DoesMatchingMimeTypeExist(mimeType))
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Validation_Summary_Text,
          Message = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Media_Templates_Cannot_Save_No_Matching_MimeType_Msg, mimeType),
          Style = MessageStyle.Error
        };

        return false;
      }

      // TEST 3. The first template for a new MIME type must have a browser ID "default".
      var isNonDefaultTmpl = !browserId.Equals("default", StringComparison.OrdinalIgnoreCase);
      if (isNonDefaultTmpl)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Validation_Summary_Text,
          Message = Resources.GalleryServer.Admin_Media_Templates_Cannot_Save_No_Default_Tmpl_Msg,
          Style = MessageStyle.Error
        };

        return false;
      }

      return true;
    }

    private static bool DoesMatchingMimeTypeExist(string mimeType)
    {
      // If the MIME type ends with a wildcard (*), just verify the major type; otherwise check the whole thing.
      if (mimeType.Last() == '*')
      {
        var majorType = mimeType.Substring(0, mimeType.IndexOf("/", StringComparison.Ordinal));
        return Factory.LoadMimeTypes().Any(mt => mt.MajorType == majorType);
      }
      else
      {
        return Factory.LoadMimeTypes().Any(mt => mt.FullType == mimeType);
      }
    }
  }
}