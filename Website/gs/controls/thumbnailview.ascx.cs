using System;
using System.Globalization;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Controls
{
	/// <summary>
	/// A user control that provides a thumbnail view of gallery objects.
	/// </summary>
	public partial class thumbnailview : GalleryUserControl
	{
		#region Private Fields

		#endregion

		#region Properties

		private string ThumbnailHtmlTmplClientId
		{
			get { return String.Concat(this.GalleryPage.ClientID, "_thmbHtmlTmpl"); }
		}

		private string ThumbnailScriptTmplClientId
		{
			get { return String.Concat(this.GalleryPage.ClientID, "_thmbScriptTmpl"); }
		}

		#endregion

		#region Protected Events

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//this.GalleryPage.SetThumbnailCssStyle(this.GalleryPage.GetAlbum().GetChildGalleryObjects(true, this.GalleryPage.IsAnonymousUser));

			RegisterJavascript();
		}

		#endregion

		#region Protected Methods

		#endregion

		#region Private Methods

		private void RegisterJavascript()
		{
			RegisterStartupScript();
		}

		private void RegisterStartupScript()
		{
			var uiTemplate = this.GalleryPage.UiTemplates.Get(UiTemplateType.Album, this.GalleryPage.GetAlbum());

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
																		ThumbnailHtmlTmplClientId, // 0
																		uiTemplate.HtmlTemplate, // 1
																		ThumbnailScriptTmplClientId, // 2
																		uiTemplate.ScriptTemplate, // 3
																		GalleryPage.GspClientId, // 4
																		GalleryPage.ThumbnailTmplName // 5
																		);

			this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.GalleryPage.ClientID, "_gThmbTmplScript"), script, false);
		}

		#endregion
	}
}