using System;
using System.Web.UI.WebControls;

namespace GalleryServer.Web.Controls.Task
{
	/// <summary>
	/// Specifies the footer content at the end of each task page.
	/// 
	/// </summary>
	public partial class taskfooter : GalleryUserControl
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
		/// Gets / sets the text that appears on the top and bottom Cancel buttons on the page. This is rendered as the value
		/// attribute of the input HTML tag.
		/// </summary>
		public string CancelButtonText
		{
			// This is the text that appears on the top and bottom Cancel buttons.
			get
			{
				return btnCancelBottom.Text;
			}
			set
			{
				// Ensure value is less than 25 characters.
				string btnText = String.IsNullOrEmpty(value) ? String.Empty : value.PadRight(25).Substring(0, 25).Trim();
				btnCancelBottom.Text = btnText;
			}
		}

		/// <summary>
		/// Gets / sets the ToolTip for the top and bottom Cancel buttons on the page. The ToolTip is rendered as 
		/// the title attribute of the HTML tag.
		/// </summary>
		public string CancelButtonToolTip
		{
			get
			{
				return btnCancelBottom.ToolTip;
			}
			set
			{
				// Ensure value is less than 250 characters.
				string tooltipText = String.IsNullOrEmpty(value) ? String.Empty : value.PadRight(250).Substring(0, 250).Trim();
				btnCancelBottom.ToolTip = tooltipText;
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
		/// Gets / sets the visibility of the top and bottom Cancel buttons on the page. When true, the buttons
		/// are visible. When false, they are not visible (not rendered in the page output.)
		/// </summary>
		public bool CancelButtonIsVisible
		{
			get
			{
				return btnCancelBottom.Visible;
			}
			set
			{
				btnCancelBottom.Visible = value;
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

		/// <summary>
		/// Gets a reference to the bottom cancel button.
		/// </summary>
		public Button CancelButtonBottom
		{
			get
			{
				return btnCancelBottom;
			}
		}

		#endregion

		#region Event Handlers

		//protected void Page_Load(object sender, EventArgs e)
		//{

		//}

		/// <summary>
		/// Handles the Click event of the btnCancel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnCancel_Click(object sender, EventArgs e)
		{
			this.GalleryPage.RedirectToPreviousPage();
		}

		#endregion

	}
}