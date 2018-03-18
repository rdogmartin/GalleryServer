using System;
using System.Globalization;
using GalleryServer.Business;

namespace GalleryServer.Web.Pages.Task
{
  /// <summary>
  /// A page-like user control that handles the Synchronize task.
  /// </summary>
  public partial class synchronize : Pages.TaskPage
  {
    #region Properties

    /// <summary>
    /// Gets the synchronize complete js render template.
    /// </summary>
    /// <value>The synchronize complete js render template.</value>
    protected string SyncCompleteJsRenderTemplate
    {
      get
      {
        string taskSynchProgressSkippedObjectsMaxExceededMsg = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Task_Synch_Progress_Skipped_Objects_Max_Exceeded_Msg, GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch);

        return String.Format(CultureInfo.InvariantCulture, @"
  <p>{0}</p>
  {{{{if SkippedFiles.length > 0}}}}
    {{{{if SkippedFiles.length >= {1}}}}}
      <p class='gsp_msgwarning_o'>{2}</p>
    {{{{else}}}}
      <p class='gsp_msgwarning_o'>{3}</p>
    {{{{/if}}}}
    <ul class='gsp_sync_sts_sf_ctr gsp_fs'>
    {{{{for SkippedFiles}}}}
      {{{{if #index < {1}}}}}
        <li><span class='gsp_sync_sts_sf'>{{{{>Key}}}}:</span>&nbsp;<span class='gsp_sync_sts_sf_v'>{{{{>Value}}}}</span></li>
      {{{{/if}}}}
    {{{{/for}}}}
    </ul>
    <p class='gsp_msgfriendly_o gsp_fs'>{4}</p>
  {{{{/if}}}}
",
 Resources.GalleryServer.Task_Synch_Progress_Successful, // 0
 GlobalConstants.MaxNumberOfSkippedObjectsToDisplayAfterSynch, // 1
 taskSynchProgressSkippedObjectsMaxExceededMsg, // 2
 Resources.GalleryServer.Task_Synch_Progress_Skipped_Objects_Msg1, // 3
 Resources.GalleryServer.Task_Synch_Progress_Skipped_Objects_Msg2 // 4
 );
      }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the Init event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Init(object sender, EventArgs e)
    {
      this.TaskHeaderPlaceHolder = phTaskHeader;
      this.TaskFooterPlaceHolder = phTaskFooter;
    }

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      this.CheckUserSecurity(SecurityActions.Synchronize);

      ConfigureControls();
    }

    #endregion

    #region Private Methods

    private void ConfigureControls()
    {
      this.TaskHeaderText = Resources.GalleryServer.Task_Synch_Header_Text;
      this.TaskBodyText = String.Empty;
      this.OkButtonText = Resources.GalleryServer.Task_Synch_Ok_Button_Text;
      this.OkButtonToolTip = Resources.GalleryServer.Task_Synch_Ok_Button_Tooltip;

      this.PageTitle = Resources.GalleryServer.Task_Synch_Page_Title;

      lblAlbumTitle.Text = this.GetAlbum().Title;
    }

    #endregion

    #region Private Static Methods


    #endregion
  }
}