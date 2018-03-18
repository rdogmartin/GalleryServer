using System;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServer.Business;

namespace GalleryServer.Web.Pages
{
  /// <summary>
  /// A page-like user control that renders the left pane, center pane, and right pane.
  /// </summary>
  public partial class media : Pages.GalleryPage
  {
    #region Fields

    private Panel _allPanesContainer;
    private Panel _centerAndRightPanesContainer;
    private Panel _leftPane;
    private Panel _centerPane;
    private Panel _rightPane;

    #endregion

    #region Properties

    private bool LeftPaneVisible { get { return ShouldPageHaveTreeView(); } }
    private bool LeftPaneDocked { get { return false; } }
    private bool CenterPaneVisible { get { return ShowCenterPane; } }
    private bool RightPaneVisible { get { return ShowRightPane; } }
    private bool RightPaneDocked { get { return false; } }

    private Panel AllPanesContainer
    {
      get
      {
        if (_allPanesContainer == null)
        {
          _allPanesContainer = new Panel();
          _allPanesContainer.ID = "media";
          _allPanesContainer.CssClass = "gsp_s_c";
        }

        return _allPanesContainer;
      }
    }

    private Panel LeftPane
    {
      get
      {
        if (_leftPane == null)
        {
          _leftPane = new Panel();
          _leftPane.ClientIDMode = ClientIDMode.Static;
          _leftPane.ID = LeftPaneClientId;
          _leftPane.CssClass = "gsp_tb_s_LeftPane gsp_tb_s_pane";
        }

        return _leftPane;
      }
    }

    private Panel CenterAndRightPanesContainer
    {
      get
      {
        if (_centerAndRightPanesContainer == null)
        {
          _centerAndRightPanesContainer = new Panel();
          _centerAndRightPanesContainer.ID = "mediaCR";
          _centerAndRightPanesContainer.CssClass = "gsp_tb_s_CenterAndRightPane";
        }

        return _centerAndRightPanesContainer;
      }
    }

    private Panel CenterPane
    {
      get
      {
        if (_centerPane == null)
        {
          _centerPane = new Panel();
          _centerPane.CssClass = "gsp_tb_s_CenterPane gsp_tb_s_pane";

          if (PageId == Web.PageId.album)
          {
            _centerPane.Controls.Add(AlbumThumbnails);
          }
          else
          {
            _centerPane.Controls.Add(MediaView);
          }
        }

        return _centerPane;
      }
    }

    private Controls.thumbnailview AlbumThumbnails
    {
      get
      {
        return (Controls.thumbnailview)LoadControl(Utils.GetUrl("/controls/thumbnailview.ascx"));
      }
    }

    private Controls.mediaview MediaView
    {
      get
      {
        return (Controls.mediaview)LoadControl(Utils.GetUrl("/controls/mediaview.ascx"));
      }
    }

    private Panel RightPane
    {
      get
      {
        if (_rightPane == null)
        {
          _rightPane = new Panel();
          _rightPane.ClientIDMode = ClientIDMode.Static;
          _rightPane.ID = RightPaneClientId;
          _rightPane.CssClass = "gsp_tb_s_RightPane gsp_tb_s_pane";
        }

        return _rightPane;
      }
    }

    private string LeftPaneHtmlTmplClientId
    {
      get { return String.Concat(ClientID, "_lpHtmlTmpl"); }
    }

    private string LeftPaneScriptTmplClientId
    {
      get { return String.Concat(ClientID, "_lpScriptTmpl"); }
    }

    private string RightPaneHtmlTmplClientId
    {
      get { return String.Concat(ClientID, "_rpHtmlTmpl"); }
    }

    private string RightPaneScriptTmplClientId
    {
      get { return String.Concat(ClientID, "_rpScriptTmpl"); }
    }


    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="media"/> class.
    /// </summary>
    protected media()
    {
      this.BeforeHeaderControlsAdded += MediaBeforeHeaderControlsAdded;
    }

    #endregion

    #region Events

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      AddPanes();

      RegisterJavaScript();
    }

    private void AddPanes()
    {
      Panel pnl;
      if (LeftPaneVisible)
      {
        pnl = AllPanesContainer;
        pnl.Controls.Add(LeftPane);
        pnl.Controls.Add(CenterAndRightPanesContainer);
      }
      else
      {
        pnl = CenterAndRightPanesContainer;
      }

      if (CenterPaneVisible)
      {
        CenterAndRightPanesContainer.Controls.Add(CenterPane);
      }

      if (RightPaneVisible)
      {
        CenterAndRightPanesContainer.Controls.Add(RightPane);
      }

      this.Controls.Add(pnl);
    }

    private void MediaBeforeHeaderControlsAdded(object sender, EventArgs e)
    {
    }

    #endregion

    #region Functions

    private void RegisterJavaScript()
    {
      // Add left and right pane templates, then invoke their scripts.
      // Note that when the header is visible, we wait for it to finish rendering before running our script.
      // We do this so  that the splitter's height calculations are correct.
      // When the device is a touchscreen, we double the width of the splitter pane (from 6px to 12px).
      // We trigger a javascript event gspPanesRendered so that any dependent code that queries the final width
      // and height of the panes can run (not currently used anywhere but could be useful to UI template editors).
      var script = String.Format(CultureInfo.InvariantCulture, @"
{0}
{1}
<script>
	$().ready(function () {{
		var runPaneScripts = function() {{
			{2}
			{3}
			{4}
			{6}

			$(document.documentElement).trigger('gspPanesRendered.{5}');
		}};

		if (Gs.Vars['{5}'].gsData.Settings.ShowHeader)
			$(document.documentElement).on('gsHeaderLoaded.{5}', runPaneScripts);
		else
			runPaneScripts();
	}});
</script>
",
        GetLeftPaneTemplates(), // 0
        GetRightPaneTemplates(), // 1
        GetLeftPaneScript(), // 2
        GetRightPaneScript(), // 3
        GetCenterPaneScript(), // 4
        GspClientId, // 5
        GetTouchScreenHacks() // 6
        );

      this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.ClientID, "_mediaScript"), script, false);
    }

    /// <summary>
    /// Gets some JavaScript to make touchscreen devices work better.
    /// </summary>
    /// <returns>System.String.</returns>
    private static string GetTouchScreenHacks()
    {
      // Implement these rules if a touchscreen less than 1500px wide is detected:
      // 1. Increase width of splitter bar to 12px.
      // 2. For non-IE browsers:
      //    (a) Remove scrollbars in center pane (necessary because Safari/Chrome has hidden scrollbars on 
      //        small devices that can't be selected, and the Selectable in the center pane prevents scrolling.
      //    (b) Make splitter bars draggable, which activates the Touch Punch hack
      var browserCaps = System.Web.HttpContext.Current.Request.Browser;
      var isIe = (browserCaps != null) && (browserCaps.Browser != null) && browserCaps.Browser.Equals("InternetExplorer", StringComparison.OrdinalIgnoreCase);

      const string nonIeScript = @"
	$('.gsp_tb_s_CenterPane').css('overflow', 'visible');
	$('.splitter-bar').draggable();";

      return String.Format(CultureInfo.InvariantCulture, @"
if (Gs.Utils.isTouchScreen() && Gs.Utils.isWidthLessThan(1500)) {{
	$('.splitter-bar-vertical').width('12');{0}
}}",
         isIe ? String.Empty : nonIeScript);
    }

    private string GetLeftPaneTemplates()
    {
      var uiTemplate = UiTemplates.Get(UiTemplateType.LeftPane, GetAlbum());

      return String.Format(CultureInfo.InvariantCulture, @"
<script id='{0}' type='text/x-jsrender'>
{1}
</script>
<script id='{2}' type='text/x-jsrender'>
{3}
</script>
",
                                    LeftPaneHtmlTmplClientId, // 0
                                    uiTemplate.HtmlTemplate, // 1
                                    LeftPaneScriptTmplClientId, // 2
                                    uiTemplate.ScriptTemplate // 3
);
    }

    private string GetRightPaneTemplates()
    {
      var uiTemplate = UiTemplates.Get(UiTemplateType.RightPane, GetAlbum());

      return String.Format(CultureInfo.InvariantCulture, @"
<script id='{0}' type='text/x-jsrender'>
{1}
</script>
<script id='{2}' type='text/x-jsrender'>
{3}
</script>
",
                                    RightPaneHtmlTmplClientId, // 0
                                    uiTemplate.HtmlTemplate, // 1
                                    RightPaneScriptTmplClientId, // 2
                                    uiTemplate.ScriptTemplate // 3
);
    }

    private string GetLeftPaneScript()
    {
      if (!LeftPaneVisible)
        return String.Empty;

      // Call splitter jQuery plug-in that sets up the split between the left and center panes
      // The splitter is only called when the gallery control's width is greater than 750px, because
      // we don't want it on small media screens (like smart phones)
      return String.Format(CultureInfo.InvariantCulture, @"
$.templates({{{0}: $('#{1}').html() }});
(new Function($('#{2}').render(Gs.Vars['{3}'].gsData)))();
{4}
",
          LeftPaneTmplName, // 0
          LeftPaneHtmlTmplClientId, // 1
          LeftPaneScriptTmplClientId, // 2
          GspClientId, // 3
          GetLeftPaneSplitterScript() // 4
          );
    }

    private string GetLeftPaneSplitterScript()
    {
      if (!CenterPaneVisible)
        return String.Empty;

      // Call splitter jQuery plug-in that sets up the split between the left and center panes
      // The splitter is only called when the gallery control's width is greater than 750px, 
      // because we don't want it on small media screens (like smart phones).
      return String.Format(CultureInfo.InvariantCulture, @"
if ($('#{0}').width() >= 750) {{
	$('#{1}').splitter({{
		type: 'v',
		outline: false,
		minLeft: 100, sizeLeft: {2}, maxLeft: 600,
		dock: 'left',
		dockSpeed: 200,
		anchorToWindow: true,
		accessKey: 'L',
		splitbarClass: 'gsp_vsplitbar',
		cookie: 'gsp_left-pane_{1}',
		cookiePath: '/'
	}});
}}
",
          GspClientId, // 0
          AllPanesContainer.ClientID, // 1
          LeftPaneDocked ? "0" : "true" // 2
          );
    }

    private string GetRightPaneScript()
    {
      if (!RightPaneVisible)
        return String.Empty;

      // Call splitter jQuery plug-in that sets up the split between the center and right panes.
      // The splitter is only called when the gallery control's width is greater than 750px, because
      // we don't want it on small media screens (like smart phones)
      return String.Format(CultureInfo.InvariantCulture, @"
$.templates({{{0}: $('#{1}').html() }});
(new Function($('#{2}').render(Gs.Vars['{3}'].gsData)))();

{4}
",
      RightPaneTmplName, // 0
      RightPaneHtmlTmplClientId, // 1
      RightPaneScriptTmplClientId, // 2
      GspClientId, // 3
      GetRightPaneSplitterScript()
      );
    }

    private string GetCenterPaneScript()
    {
      if ((LeftPaneVisible && RightPaneVisible) || !CenterPaneVisible)
        return String.Empty;

      // When either the left pane or right pane is hidden, we no longer want overflow:auto applied to
      // the center pane.
      return String.Format("$('.gsp_tb_s_CenterPane', $('#{0}')).css('overflow', 'inherit');",
        GspClientId);
    }

    private string GetRightPaneSplitterScript()
    {
      // Call splitter jQuery plug-in that sets up the split between the center and right panes.
      // The splitter is only called when the gallery control's width is greater than 750px, 
      // because we don't want it on small media screens (like smart phones).
      if (!CenterPaneVisible)
        return String.Empty;

      return String.Format(CultureInfo.InvariantCulture, @"
if ($('#{0}').width() >= 750) {{
	$('#{1}').splitter({{
		type: 'v',
		outline: false,
		minRight: 100, sizeRight: {2}, maxRight: 1000,
		dock: 'right',
		dockSpeed: 200{3},
		accessKey: 'R',
		splitbarClass: 'gsp_vsplitbar',
		cookie: 'gsp_right-pane_{1}',
		cookiePath: '/'
	}});
}}
",
      GspClientId, // 0
      CenterAndRightPanesContainer.ClientID, // 1
      RightPaneDocked ? "0" : "true", // 2
      LeftPaneVisible ? String.Empty : ",anchorToWindow: true" // 3
      );

    }

    private bool ShouldPageHaveTreeView()
    {
      // The only pages that should display an album treeview are the album and media object pages.
      switch (PageId)
      {
        case PageId.album:
          return ShowLeftPaneForAlbum;

        case PageId.mediaobject:
          return ShowLeftPaneForMediaObject;

        default:
          return false;
      }
    }

    #endregion
  }
}