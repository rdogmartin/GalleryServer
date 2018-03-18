using System;
using System.Web.UI.WebControls;

namespace GalleryServer.Web.Controls.Admin
{
	/// <summary>
	/// Specifies the footer content at the end of each admin page.
	/// </summary>
	public partial class adminfooter : GalleryUserControl
	{
		#region Public Properties

		/// <summary>
		/// Gets / sets the text that appears on the top and bottom Ok buttons on the page. This is rendered as the value
		/// attribute of the input HTML tag.
		/// </summary>
		public string OkButtonText
		{
			get
			{
				return btnOkBottom.Text;
			}
			set
			{
				// Ensure value is less than 25 characters.
				string btnText = String.IsNullOrEmpty(value) ? String.Empty : value.PadRight(25).Substring(0, 25).Trim();
				btnOkBottom.Text = btnText;
			}
		}

		/// <summary>
		/// Gets / sets the ToolTip for the top and bottom Ok buttons on the page. The ToolTip is rendered as 
		/// the title attribute of the input HTML tag.
		/// </summary>
		public string OkButtonToolTip
		{
			get
			{
				return btnOkBottom.ToolTip;
			}
			set
			{
				// Ensure value is less than 250 characters.
				string tooltipText = String.IsNullOrEmpty(value) ? String.Empty : value.PadRight(250).Substring(0, 250).Trim();
				btnOkBottom.ToolTip = tooltipText;
			}
		}

		/// <summary>
		/// Gets / sets the visibility of the top and bottom Ok buttons on the page. When true, the buttons
		/// are visible. When false, they are not visible (not rendered in the page output.)
		/// </summary>
		public bool OkButtonIsVisible
		{
			get
			{
				return btnOkBottom.Visible;
			}
			set
			{
				btnOkBottom.Visible = value;
			}
		}

		/// <summary>
		/// Gets a reference to the bottom button that initiates the completion of the task.
		/// </summary>
		public Button OkButtonBottom
		{
			get
			{
				return btnOkBottom;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		#endregion
	}
}