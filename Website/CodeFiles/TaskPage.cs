
using System;
using System.Web.UI.WebControls;
using GalleryServer.Web.Controls.Task;

namespace GalleryServer.Web.Pages
{
	/// <summary>
	/// The base class user control used in Gallery Server to represent a task page, such as adding objects and
	/// deleting albums.
	/// </summary>
	public class TaskPage : Pages.GalleryPage
	{
		#region Private Fields

		private PlaceHolder _phTaskHeader;
		private Controls.Task.taskheader _taskHeader;
		private PlaceHolder _phTaskFooter;
		private Controls.Task.taskfooter _taskFooter;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TaskPage"/> class.
		/// </summary>
		public TaskPage()
		{
			this.Load += TaskPage_Load;
			//this.Init += TaskPage_Init;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the location for the <see cref="taskheader"/> user control. Classes that inherit 
		/// <see cref="Pages.TaskPage"/> should set this property to the <see cref="PlaceHolder"/> that is to contain
		/// the task header control. If this property is not assigned by the inheriting class, the task header control
		/// is not added to the page output.
		/// </summary>
		/// <value>The <see cref="taskheader"/> user control.</value>
		public PlaceHolder TaskHeaderPlaceHolder
		{
			get
			{
				return this._phTaskHeader;
			}
			set
			{
				this._phTaskHeader = value;
			}
		}

		/// <summary>
		/// Gets the task header user control that is rendered near the top of the task page. This control contains the 
		/// page title and the top Save/Cancel buttons. (The bottom Save/Cancel buttons are in the <see cref="taskfooter"/> user control.
		/// </summary>
		/// <value>The task header user control that is rendered near the top of the task page.</value>
		public Controls.Task.taskheader TaskHeader
		{
			get
			{
				return this._taskHeader;
			}
			set
			{
				this._taskHeader = value;
			}
		}

		/// <summary>
		/// Gets or sets the location for the <see cref="taskfooter"/> user control. Classes that inherit 
		/// <see cref="Pages.TaskPage"/> should set this property to the <see cref="PlaceHolder"/> that is to contain
		/// the task footer control. If this property is not assigned by the inheriting class, the task footer control
		/// is not added to the page output.
		/// </summary>
		/// <value>The <see cref="taskfooter"/> user control.</value>
		public PlaceHolder TaskFooterPlaceHolder
		{
			get
			{
				return this._phTaskFooter;
			}
			set
			{
				this._phTaskFooter = value;
			}
		}

		/// <summary>
		/// Gets the task footer user control that is rendered near the top of the task page. This control contains the 
		/// page title and the top Save/Cancel buttons. (The bottom Save/Cancel buttons are in the <see cref="taskfooter"/> user control.
		/// </summary>
		/// <value>The task footer user control that is rendered near the top of the task page.</value>
		public Controls.Task.taskfooter TaskFooter
		{
			get
			{
				return this._taskFooter;
			}
			set
			{
				this._taskFooter = value;
			}
		}

		/// <summary>
		/// Gets / sets the task header text (e.g. Task: Delete an album).
		/// </summary>
		public string TaskHeaderText
		{
			get
			{
				return this.TaskHeader.TaskHeaderText;
			}
			set
			{
				this.TaskHeader.TaskHeaderText = value;
			}
		}

		/// <summary>
		/// Gets / sets the task body text (e.g. Confirm the deletion of this album by clicking 'Delete album'.).
		/// </summary>
		public string TaskBodyText
		{
			get
			{
				return this.TaskHeader.TaskBodyText;
			}
			set
			{
				this.TaskHeader.TaskBodyText = value;
			}
		}

		/// <summary>
		/// Gets / sets the text that appears on the top and bottom Ok buttons on the page. This is rendered as the value
		/// attribute of the input HTML tag.
		/// </summary>
		public string OkButtonText
		{
			get
			{
				return this.TaskHeader.OkButtonText;
			}
			set
			{
				this.TaskHeader.OkButtonText = value;
				this.TaskFooter.OkButtonText = value;
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
				return this.TaskHeader.OkButtonToolTip;
			}
			set
			{
				this.TaskHeader.OkButtonToolTip = value;
				this.TaskFooter.OkButtonToolTip = value;
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
				return this.TaskHeader.CancelButtonText;
			}
			set
			{
				this.TaskHeader.CancelButtonText = value;
				this.TaskFooter.CancelButtonText = value;
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
				return this.TaskHeader.CancelButtonToolTip;
			}
			set
			{
				this.TaskHeader.CancelButtonToolTip = value;
				this.TaskFooter.CancelButtonToolTip = value;
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
				return this.TaskHeader.OkButtonIsVisible;
			}
			set
			{
				this.TaskHeader.OkButtonIsVisible = value;
				this.TaskFooter.OkButtonIsVisible = value;
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
				return this.TaskHeader.CancelButtonIsVisible;
			}
			set
			{
				this.TaskHeader.CancelButtonIsVisible = value;
				this.TaskFooter.CancelButtonIsVisible = value;
			}
		}

		/// <summary>
		/// Gets a reference to the top button that initiates the completion of the task.
		/// </summary>
		public Button OkButtonTop
		{
			get
			{
				return this.TaskHeader.OkButtonTop;
			}
		}

		/// <summary>
		/// Gets a reference to the bottom button that initiates the completion of the task.
		/// </summary>
		public Button OkButtonBottom
		{
			get
			{
				return this.TaskFooter.OkButtonBottom;
			}
		}

		/// <summary>
		/// Gets a reference to the top cancel button.
		/// </summary>
		public Button CancelButtonTop
		{
			get
			{
				return this.TaskHeader.CancelButtonTop;
			}
		}

		/// <summary>
		/// Gets a reference to the bottom cancel button.
		/// </summary>
		public Button CancelButtonBottom
		{
			get
			{
				return this.TaskFooter.CancelButtonBottom;
			}
		}

		#endregion

		#region Event Handlers

		void TaskPage_Load(object sender, EventArgs e)
		{
			Controls.Task.taskheader taskHeader = (Controls.Task.taskheader)LoadControl(Utils.GetUrl("/controls/task/taskheader.ascx"));
			this.TaskHeader = taskHeader;
			if (this.TaskHeaderPlaceHolder != null)
				this.TaskHeaderPlaceHolder.Controls.Add(taskHeader);

			Controls.Task.taskfooter taskFooter = (Controls.Task.taskfooter)LoadControl(Utils.GetUrl("/controls/task/taskfooter.ascx"));
			this.TaskFooter = taskFooter;
			if (this.TaskFooterPlaceHolder != null)
				this.TaskFooterPlaceHolder.Controls.Add(taskFooter);
			
			ConfigureControls();
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			if ((this.TaskHeader != null) && (this.TaskHeader.OkButtonTop != null))
				this.Page.Form.DefaultButton = this.TaskHeader.OkButtonTop.UniqueID;
		}

		#endregion
	}
}
