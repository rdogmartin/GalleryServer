using System;
using System.Globalization;
using GalleryServer.Business;

namespace GalleryServer.Web.Controls
{
	/// <summary>
	/// A user control that renders a particular media object.
	/// </summary>
	public partial class mediaview : GalleryUserControl
	{
		#region Properties

		private string MediaHtmlTmplClientId
		{
			get { return String.Concat(GalleryPage.ClientID, "_mediaHtmlTmpl"); }
		}

		private string MediaScriptTmplClientId
		{
			get { return String.Concat(GalleryPage.ClientID, "_mediaScriptTmpl"); }
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
			if (!IsPostBack)
			{
				if (GalleryPage.GetMediaObjectId() == int.MinValue)
				{
					Utils.Redirect(Utils.AddQueryStringParameter(Utils.GetCurrentPageUrl(), "msg=" + (int)MessageType.MediaObjectDoesNotExist));
				}

				RegisterJavascript();
			}
		}

		#endregion

		#region Functions

		private void RegisterJavascript()
		{
			RegisterStartupScript();
		}

		private void RegisterStartupScript()
		{
			var uiTemplate = GalleryPage.UiTemplates.Get(UiTemplateType.MediaObject, GalleryPage.GetAlbum());

			// Define 3 script tags. The first two hold the HTML and javascript jsRender templates.
			// The last contains start script that does 2 things:
			// 1. Compile the jsRender template and run the javascript generated in the template
			// 2. Generate the JavaScript from the template and add to the page
			string script = String.Format(CultureInfo.InvariantCulture, @"
<script id='{0}' type='text/x-jsrender'>
{1}
</script>
<script id='{2}' type='text/x-jsrender'>
{3}
</script>
<script>
(function ($) {{
	$(document).ready(function () {{
		$.templates({{{5}: $('#{0}').html() }});
		(new Function($('#{2}').render(Gs.Vars['{4}'].gsData)))();
	}});
}})(jQuery);
</script>
",
																		MediaHtmlTmplClientId, // 0
																		uiTemplate.HtmlTemplate, // 1
																		MediaScriptTmplClientId, // 2
																		uiTemplate.ScriptTemplate, // 3
																		GalleryPage.GspClientId, // 4
																		GalleryPage.MediaTmplName // 5
																		);

			Page.ClientScript.RegisterStartupScript(GetType(), String.Concat(GalleryPage.ClientID, "_mViewTmplScript"), script, false);
		}

		#endregion
	}
}