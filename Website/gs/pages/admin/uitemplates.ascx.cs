using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Pages.Admin
{
  /// <summary>
  /// A page-like user control for administering settings for the gallery template functionality.
  /// </summary>
  public partial class uitemplates : Pages.AdminPage
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

    private IUiTemplate CurrentUiTemplate
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the name of the cookie that stores the index of the currently selected tab.
    /// </summary>
    /// <value>A string.</value>
    protected string SelectedTabCookieName { get { return String.Concat(cid, "_ui_tmpl_cookie"); } }

    #endregion

    #region Event Handlers

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
        ConfigureControlsFirstTime();
      }

      ConfigureControlsEveryTime();
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
    /// Handles the SelectedIndexChanged event of the ddlGalleryItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void ddlGalleryItem_SelectedIndexChanged(object sender, EventArgs e)
    {
      BindTemplateNameDropDownList();

      CurrentUiTemplate = GetSelectedJQueryTemplate();

      BindUiTemplate();
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the ddlTemplateName control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void ddlTemplateName_SelectedIndexChanged(object sender, EventArgs e)
    {
      CurrentUiTemplate = GetSelectedJQueryTemplate();
      BindUiTemplate();
    }

    /// <summary>
    /// Handles the Click event of the lbCreate control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void lbCreate_Click(object sender, EventArgs e)
    {
      ViewMode = PageMode.Insert;

      IUiTemplate tmplCopy = CurrentUiTemplate.Copy();

      tmplCopy.Name = GenerateUniqueTemplateName();

      CurrentUiTemplate = tmplCopy;

      ddlTemplateName.Items.Add(new ListItem(tmplCopy.Name, int.MinValue.ToString()));
      ddlTemplateName.SelectedIndex = ddlTemplateName.Items.Count - 1;

      btnDelete.Enabled = false;

      BindUiTemplate();
    }

    /// <summary>
    /// Handles the Click event of the btnSave control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnSave_Click(object sender, EventArgs e)
    {
      CurrentUiTemplate = GetSelectedJQueryTemplate();

      string invalidReason;
      if (ValidateUiTemplateBeforeSave(out invalidReason))
      {
        UnbindJQueryTemplate();
        CurrentUiTemplate.Save();

        var deactivatedTmpl = DeactivateSiblingTemplatesOnSave();

        BindTemplateNameDropDownList();
        ddlTemplateName.SelectedIndex = ddlTemplateName.Items.IndexOf(ddlTemplateName.Items.FindByValue(CurrentUiTemplate.UiTemplateId.ToString(CultureInfo.InvariantCulture)));
        ViewMode = PageMode.Edit;

        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
          Message = Resources.GalleryServer.Admin_Save_Success_Text,
          Style = MessageStyle.Success
        };

        if (deactivatedTmpl)
        {
          ClientMessage.Message += " This template is now active for all albums in the gallery.";
        }

      }
      else
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Validation_Summary_Text,
          Message = invalidReason,
          Style = MessageStyle.Error
        };
      }
    }

    /// <summary>
    /// If the template being saved applies to all albums, then deactivate any sibling templates that apply to all albums.
    /// This saves the admin from having to bounce back to the original template, deactivate it, then return to the new
    /// template. It also reduces confusion because it prevents multiple templates from being assigned to the root album
    /// at the same time.
    /// </summary>
    /// <returns><c>true</c> if at least one template was deactivated, <c>false</c> otherwise.</returns>
    private bool DeactivateSiblingTemplatesOnSave()
    {
      var deactivatedTmpl = false;
      var rootAlbumId = Factory.LoadRootAlbumInstance(GalleryId).Id;

      if (CurrentUiTemplate.RootAlbumIds.Contains(rootAlbumId))
      {
        // UI template is assigned to root album. If any other templates are also assigned to the root album, de-select it.
        foreach (var uiTemplate in UiTemplates.Where(t => t.TemplateType == CurrentUiTemplate.TemplateType && t.UiTemplateId != CurrentUiTemplate.UiTemplateId && t.RootAlbumIds.Contains(rootAlbumId)))
        {
          uiTemplate.RootAlbumIds.Remove(rootAlbumId);
          uiTemplate.Save();
          deactivatedTmpl = true;
        }
      }

      return deactivatedTmpl;
    }

    /// <summary>
    /// Handles the Click event of the btnCancel control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnCancel_Click(object sender, EventArgs e)
    {
      if (ViewMode == PageMode.Insert)
        BindTemplateNameDropDownList();

      CurrentUiTemplate = GetSelectedJQueryTemplate();
      BindUiTemplate();
      ViewMode = PageMode.Edit;
    }

    /// <summary>
    /// Handles the Click event of the btnDelete control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void btnDelete_Click(object sender, EventArgs e)
    {
      CurrentUiTemplate = GetSelectedJQueryTemplate();

      string invalidReason;
      if (ValidateUiTemplateBeforeDelete(out invalidReason))
      {
        CurrentUiTemplate.Delete();
        BindTemplateNameDropDownList();
        CurrentUiTemplate = GetSelectedJQueryTemplate();
        BindUiTemplate();
        ViewMode = PageMode.Edit;

        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Save_Success_Hdr,
          Message = Resources.GalleryServer.Admin_Templates_Deleted_Msg,
          Style = MessageStyle.Success
        };
      }
      else
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Validation_Summary_Text,
          Message = invalidReason,
          Style = MessageStyle.Error
        };
      }
    }

    #endregion

    #region Private Methods

    private void ConfigureControlsEveryTime()
    {
      this.PageTitle = Resources.GalleryServer.Admin_UiTemplates_Page_Header;
      lblGalleryDescription.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServer.Admin_Gallery_Description_Label, Utils.GetCurrentPageUrl(), Utils.HtmlEncode(Factory.LoadGallery(GalleryId).Description));

      this.tvUC.RequiredSecurityPermissions = SecurityActions.AdministerSite | SecurityActions.AdministerGallery;

      btnDelete.Enabled = AppSetting.Instance.License.LicenseType >= LicenseLevel.Enterprise;

      CurrentUiTemplate = GetSelectedJQueryTemplate();
    }

    private void ConfigureControlsFirstTime()
    {
      ViewMode = PageMode.Edit;
      OkButtonIsVisible = false;

      BindDropDownLists();

      CurrentUiTemplate = GetSelectedJQueryTemplate();

      BindUiTemplate();

      AdminPageTitle = Resources.GalleryServer.Admin_UiTemplates_Page_Header;

      if (AppSetting.Instance.License.LicenseType == LicenseLevel.TrialExpired)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = Resources.GalleryServer.Admin_Site_Settings_ProductKey_NotEntered_Label,
          Message = Resources.GalleryServer.Admin_Need_Product_Key_Msg2,
          Style = MessageStyle.Info
        };

        btnSave.Enabled = btnCancel.Enabled = btnDelete.Enabled = false;
      }
      else if (AppSetting.Instance.License.LicenseType == LicenseLevel.Trial)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = "Gallery Server Enterprise required for some features",
          Message = "<p>During the trial period, the UI Template editor is fully functional.</p><p>When the trial is over, you can continue switching templates by enabling/disabling albums on the Target Albums tab. But you will need Gallery Server Enterprise or higher to edit the HTML or JavaScript on this page.</p>",
          Style = MessageStyle.Info
        };

        btnDelete.Enabled = false;
      }
      else if (AppSetting.Instance.License.LicenseType < LicenseLevel.Enterprise)
      {
        ClientMessage = new ClientMessageOptions
        {
          Title = "Gallery Server Enterprise required for some features",
          Message = "<p>You must have Gallery Server Enterprise or higher to edit the HTML or JavaScript on this page. To unlock this feature, enter a qualifying license key.</p><p>You can, however, switch templates by enabling or disabling albums on the Target Albums tab.</p>",
          Style = MessageStyle.Info
        };

        btnDelete.Enabled = false;
      }
    }

    private void BindDropDownLists()
    {
      BindGalleryItemDropDownList();

      BindTemplateNameDropDownList();
    }

    private void BindGalleryItemDropDownList()
    {
      ddlGalleryItem.DataTextField = "Value";
      ddlGalleryItem.DataValueField = "Key";

      var kvps = Utils.GetEnumList(typeof(UiTemplateType)).AsQueryable();

      ddlGalleryItem.DataSource = kvps.Where(k => k.Key != UiTemplateType.NotSpecified.ToString());
      ddlGalleryItem.DataBind();
    }

    private void BindTemplateNameDropDownList()
    {
      var templateType = Enum<UiTemplateType>.Parse(ddlGalleryItem.SelectedValue);

      const string activeTmplSuffix = " <active>";

      var tmplNames = (from a in GalleryController.GetUiTemplates()
                       where a.TemplateType == templateType && a.GalleryId == GalleryId
                       select new KeyValuePair<int, string>(a.UiTemplateId, a.RootAlbumIds.Any() ? a.Name + activeTmplSuffix : a.Name));

      if (!tmplNames.Any())
      {
        throw new WebException($"Could not find any UI templates in the data store having TemplateType='{templateType}'");
      }

      var selectedIndex = 0;

      if (tmplNames.Any(kvp => !kvp.Value.EndsWith(activeTmplSuffix)))
      {
        // There is at least one tmpl that is active (as there always should be). Grab the index of the first one so we can select it in the dropdown
        selectedIndex = tmplNames.Select((v, i) => new { kvp = v, index = i }).First(item => item.kvp.Value.EndsWith(activeTmplSuffix)).index;
      }

      ddlTemplateName.DataTextField = "Value";
      ddlTemplateName.DataValueField = "Key";
      ddlTemplateName.DataSource = tmplNames;
      ddlTemplateName.DataBind();
      ddlTemplateName.SelectedIndex = selectedIndex;
    }

    /// <summary>
    /// Gets the template that matches the selected item in the dropdown list.
    /// </summary>
    /// <returns>An instance of <see cref="IUiTemplate" />.</returns>
    private IUiTemplate GetSelectedJQueryTemplate()
    {
      if (ddlTemplateName.SelectedValue != int.MinValue.ToString(CultureInfo.InvariantCulture))
      {
        if (ddlTemplateName.SelectedItem.Text.Contains("(3.2.1 version"))
        {
          ClientMessage = new ClientMessageOptions
          {
            Title = "Archived UI Template",
            Message = "<p>You selected an inactive, archived copy of the UI template from 3.2.1. Since any changes you made in the old version were not migrated to the version you're running now, we saved a copy in case you wanted to refer to it. It may be helpful if you want to re-apply your customizations.</p><p>You cannot activate or modify this template. If you finished your migration to the current version or never edited the original UI template, we recommend you delete it.</p>",
            Style = MessageStyle.Info
          };

          btnSave.Enabled = false;
        }
        else
        {
          btnSave.Enabled = true;
        }

        return (from t in UiTemplates
                where t.UiTemplateId == Convert.ToInt32(ddlTemplateName.SelectedValue, CultureInfo.InvariantCulture)
                select t).FirstOrDefault();
      }
      else
      {
        return new UiTemplate
        {
          UiTemplateId = int.MinValue,
          TemplateType = Enum<UiTemplateType>.Parse(ddlGalleryItem.SelectedValue),
          GalleryId = GalleryId,
          Name = ddlTemplateName.SelectedItem.Text,
          RootAlbumIds = new IntegerCollection(),
          HtmlTemplate = String.Empty,
          ScriptTemplate = String.Empty
        };
      }
    }

    /// <summary>
    /// Copies the data from <see cref="CurrentUiTemplate" /> to the relevant web form controls.
    /// </summary>
    private void BindUiTemplate()
    {
      txtTemplateName.Text = CurrentUiTemplate.Name;
      txtTemplate.Text = CurrentUiTemplate.HtmlTemplate;
      txtScript.Text = CurrentUiTemplate.ScriptTemplate;
      tvUC.SelectedAlbumIds = CurrentUiTemplate.RootAlbumIds;
    }

    /// <summary>
    /// Copies the data from the web form to the relevant properties of the <see cref="CurrentUiTemplate" /> property.
    /// </summary>
    private void UnbindJQueryTemplate()
    {
      CurrentUiTemplate.Name = txtTemplateName.Text.Trim();
      CurrentUiTemplate.HtmlTemplate = txtTemplate.Text.Trim();
      CurrentUiTemplate.ScriptTemplate = txtScript.Text.Trim();
      CurrentUiTemplate.Description = String.Empty;
      CurrentUiTemplate.RootAlbumIds = tvUC.SelectedAlbumIds;
    }

    private string GenerateUniqueTemplateName()
    {
      List<string> tmplNames = new List<string>(ddlTemplateName.Items.Count);
      foreach (ListItem item in ddlTemplateName.Items)
      {
        tmplNames.Add(item.Text.Replace(" <active>", string.Empty));
      }

      string proposedName = CurrentUiTemplate.Name.Contains("(copy") ? CurrentUiTemplate.Name : $"{CurrentUiTemplate.Name} (copy)";

      int counter = 1;

      while (tmplNames.Contains(proposedName))
      {
        // There is already a template with our proposed name. We need to strip off the
        // previous suffix and try again.
        proposedName = proposedName.Remove(proposedName.LastIndexOf(" (copy", StringComparison.Ordinal));

        // Generate the new suffix to append to the filename (e.g. "(3)")
        proposedName = $"{proposedName} (copy {counter})";

        counter++;
      }

      return proposedName;
    }

    /// <summary>
    /// Verify the UI template can be saved.
    /// </summary>
    /// <param name="invalidReason">A message describing why the validation failed. Set to <see cref="String.Empty" /> when
    /// validation succeeds.</param>
    /// <returns><c>true</c> if business rules for saving are satisfied, <c>false</c> otherwise</returns>
    private bool ValidateUiTemplateBeforeSave(out string invalidReason)
    {
      // TEST 1: Cannot save changes to the default template.
      if (CurrentUiTemplate.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
      {
        var htmlHasChanged = !CurrentUiTemplate.HtmlTemplate.Equals(txtTemplate.Text, StringComparison.Ordinal);
        var scriptHasChanged = !CurrentUiTemplate.ScriptTemplate.Equals(txtScript.Text, StringComparison.Ordinal);

        if (htmlHasChanged || scriptHasChanged)
        {
          invalidReason = Resources.GalleryServer.Admin_Templates_Cannot_Modify_Default_Tmpl_Msg;
          return false;
        }
      }

      // TEST 2: This must be trial version or Enterprise or higher if saving changes to the HTML and/or JavaScript
      if (AppSetting.Instance.License.LicenseType < LicenseLevel.Enterprise)
      {
        var htmlHasChanged = !CurrentUiTemplate.HtmlTemplate.Equals(txtTemplate.Text, StringComparison.Ordinal);
        var scriptHasChanged = !CurrentUiTemplate.ScriptTemplate.Equals(txtScript.Text, StringComparison.Ordinal);

        if (htmlHasChanged || scriptHasChanged)
        {
          invalidReason = Resources.GalleryServer.Admin_Templates_Cannot_Save_Tmpl_Insufficient_License_Msg;
          return false;
        }
      }

      // TEST 3: Verify no other template has the same name in this category.
      var tmpl = (from t in UiTemplates
                  where t.TemplateType == CurrentUiTemplate.TemplateType &&
                  t.GalleryId == GalleryId &&
                  t.Name == txtTemplateName.Text &&
                  t.UiTemplateId != CurrentUiTemplate.UiTemplateId
                  select t).FirstOrDefault();

      if (tmpl != null)
      {
        invalidReason = Resources.GalleryServer.Admin_Templates_Cannot_Save_Duplicate_Name_Msg;
        return false;
      }

      // TEST 4: Verify user isn't removing the last template from the root album.
      var rootAlbumId = Factory.LoadRootAlbumInstance(GalleryId).Id;

      var curTmplNotAssignedToRootAlbum = !tvUC.SelectedAlbumIds.Contains(rootAlbumId); // Need to use tvUC.SelectedAlbumIds instead of CurrentUiTemplate.RootAlbumIds because CurrentUiTemplate has not yet been unbound
      var noOtherTmplAssignedToRootAlbum = !UiTemplates.Any(t => t.TemplateType == CurrentUiTemplate.TemplateType && t.UiTemplateId != CurrentUiTemplate.UiTemplateId && t.RootAlbumIds.Contains(rootAlbumId));

      if (curTmplNotAssignedToRootAlbum && noOtherTmplAssignedToRootAlbum)
      {
        invalidReason = Resources.GalleryServer.Admin_Templates_Cannot_Save_No_Tmpl_Msg;
        return false;
      }

      // TEST 5: The default template cannot be renamed to something else.
      if (CurrentUiTemplate.Name.Equals("Default", StringComparison.OrdinalIgnoreCase) && !txtTemplateName.Text.Equals("Default", StringComparison.OrdinalIgnoreCase))
      {
        invalidReason = Resources.GalleryServer.Admin_Templates_Cannot_Save_No_Default_Tmpl_Msg;
        return false;
      }

      // All the tests pass, so return true.
      invalidReason = String.Empty;
      return true;
    }

    /// <summary>
    /// Verify the UI template can be deleted.
    /// </summary>
    /// <param name="invalidReason">A message describing why the validation failed. Set to <see cref="String.Empty" /> when
    /// validation succeeds.</param>
    /// <returns><c>true</c> if business rules for deleting are satisfied, <c>false</c> otherwise</returns>
    private bool ValidateUiTemplateBeforeDelete(out string invalidReason)
    {
      // TEST 1: Cannot delete the default template.
      if (CurrentUiTemplate.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
      {
        invalidReason = Resources.GalleryServer.Admin_Templates_Cannot_Modify_Default_Tmpl_Msg;
        return false;
      }

      // TEST 2: Cannot delete a template if it leaves one ore more albums without a template
      var rootAlbumId = Factory.LoadRootAlbumInstance(GalleryId).Id;
      var noOtherTmplAssignedToRootAlbum = !UiTemplates.Any(t => t.TemplateType == CurrentUiTemplate.TemplateType && t.UiTemplateId != CurrentUiTemplate.UiTemplateId && t.RootAlbumIds.Contains(rootAlbumId));

      if (noOtherTmplAssignedToRootAlbum)
      {
        invalidReason = Resources.GalleryServer.Admin_Templates_Cannot_Delete_No_Tmpl_Msg;
        return false;
      }

      // All the tests pass, so return true.
      invalidReason = String.Empty;
      return true;
    }

    #endregion
  }
}