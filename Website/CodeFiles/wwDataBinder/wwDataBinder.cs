//#define USE_WWBUSINESS 

/*
 **************************************************************
 * wwDataBinder
 **************************************************************
 *  Author: Rick Strahl 
 *          (c) West Wind Technologies
 *          http://www.west-wind.com/
 * 
 * License: Free - provided as is, no warranties
 * Created: 01/11/2006 
 **************************************************************   
 */

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using GalleryServer.WebControls.Tools;

namespace GalleryServer.WebControls
{

	/// <summary>
	/// The wwDataBinder class provides two-way, simple databinding a single 
	/// datasource value and single control property. It can bind object properties
	///  and fields and database values (DataRow fields) to a control property such
	///  as the Text, Checked or SelectedValue properties. In a nutshell the 
	/// controls acts as a connector between a datasource and the control and 
	/// provides explicit databinding for the control.
	/// 
	/// The control supports two-way binding. Control can be bound to the 
	/// datasource values and can be unbound by taking control values and storing 
	/// them back into the datasource. The process is performed explicitly by 
	/// calling the DataBind() and Unbind() methods of the control. Controls 
	/// attached to the databinder can also be bound individually.
	/// 
	/// The control also provides a BindErrors collection which captures any 
	/// binding errors and allows friendly display of these binding errors using 
	/// the ToHtml() method. BindingErrors can be manually added and so application
	///  level errors can be handled the same way as binding errors. It's also 
	/// possible to pull in ASP.NET Validator control errors.
	/// 
	/// Simple validation is available with IsRequired for each DataBinding item. 
	/// Additional validation can be performed server side by implementing the 
	/// ValidateControl event which allows you to write application level 
	/// validation code.
	/// 
	/// This control is implemented as an Extender control that extends any Control
	///  based class. This means you can databind to ANY control class and its 
	/// properties with this component.
	/// </summary>
	//[NonVisualControl, Designer(typeof(wwDataBinderDesigner))]
	[ProvideProperty("DataBindingItem", typeof(Control))]
	[ParseChildren(true, "DataBindingItems")]
	[PersistChildren(false)]
	[DefaultProperty("DataBindingItems")]
	[DefaultEvent("ValidateControl")]
	public class wwDataBinder : Control, IExtenderProvider  //System.Web.UI.WebControls.WebControl
	{
		public wwDataBinder()
		{
			this._DataBindingItems = new wwDataBindingItemCollection(this);
		}

		public new bool DesignMode = (HttpContext.Current == null);

		/// <summary>
		/// A collection of all the DataBindingItems that are to be bound. Each 
		/// &lt;&lt;%= TopicLink([wwDataBindingItem],[_1UL03RIKQ]) %&gt;&gt; contains 
		/// the information needed to bind and unbind a DataSource to a Control 
		/// property.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public wwDataBindingItemCollection DataBindingItems
		{
			get
			{
				return _DataBindingItems;
			}
		}
		wwDataBindingItemCollection _DataBindingItems = null;


		/////// <summary>
		/////// Collection of all the preserved properties that are to
		/////// be preserved/restored. Collection hold, ControlId, Property
		/////// </summary>
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		//[PersistenceMode(PersistenceMode.InnerProperty)]
		//public List<wwDataBindingItem> DataBindingItems
		//{
		//    get
		//    {
		//        return _DataBindingItems;
		//    }
		//}
		//List<wwDataBindingItem> _DataBindingItems = new List<wwDataBindingItem>();

		/// <summary>
		/// A collection of binding errors that is filled after binding or unbinding
		/// if errors occur during binding and unbinding.
		/// </summary>
		[Browsable(false)]
		public BindingErrors BindingErrors
		{
			get { return _BindingErrors; }
			set { _BindingErrors = value; }
		}
		private BindingErrors _BindingErrors = new BindingErrors();

		/// <summary>
		/// Determines whether binding errors are display on controls.
		/// </summary>
		[Description("Determines whether binding errors are displayed on controls. The display mode is determined for each binding individually.")]
		[DefaultValue(true)]
		public bool ShowBindingErrorsOnControls
		{
			get { return _ShowBindingErrorsOnControls; }
			set { _ShowBindingErrorsOnControls = value; }
		}
		private bool _ShowBindingErrorsOnControls = true;


		/// <summary>
		/// Optional Url to the Warning and Info Icons.
		/// Note: Used only if the control uses images.
		/// </summary>
		[Description("Optional Image Url for the Error Icon. Used only if the control uses image icons."),
		Editor("System.Web.UI.Design.ImageUrlEditor", typeof(UITypeEditor)),
		DefaultValue("WebResource")]
		public string ErrorIconUrl
		{
			get { return _ErrorIconUrl; }
			set { _ErrorIconUrl = value; }
		}
		private string _ErrorIconUrl = "WebResource";


		/// <summary>
		/// Determines whether the control uses client script to inject error 
		/// notification icons/messages into the page. Setting this flag to true causes
		///  JavaScript to be added to the page to create the messages. If false, the 
		/// DataBinder uses Controls.Add to add controls to the Page or other 
		/// Containers.
		/// 
		/// JavaScript injection is preferrable as it works reliable under all 
		/// environments except when JavaScript is off. Controls.Add() can have 
		/// problems if &lt;% %&gt; &lt;%= %&gt; script is used in a container that has
		///  an error and needs to add a control.
		/// </summary>
		[Description("Uses Client Script code to inject Validation Error messages into the document. More reliable than Controls.Add() due to markup restrictions"),
		DefaultValue(true)]
		public bool UseClientScriptHtmlInjection
		{
			get { return _UseClientScriptHtmlInjection; }
			set { _UseClientScriptHtmlInjection = value; }
		}
		private bool _UseClientScriptHtmlInjection = true;

		bool _ClientScriptInjectionScriptAdded = false;

		/// <summary>
		/// Automatically imports all controls on the form that implement the IwwDataBinder interface and adds them to the DataBinder
		/// </summary>
		[Description("Automatically imports all controls on the form that implement the IwwDataBinder interface and adds them to the DataBinder"),
		 Browsable(true), DefaultValue(false)]
		public bool AutoLoadDataBoundControls
		{
			get { return _AutoLoadDataBoundControls; }
			set { _AutoLoadDataBoundControls = value; }
		}
		private bool _AutoLoadDataBoundControls = false;

		/// <summary>
		/// Flag that determines whether controls where auto-loaded from the page.
		/// </summary>
		private bool _AutoLoadedDataBoundControls = false;

		/// <summary>
		/// Determines whether this control works as an Extender object to other controls on the form.
		/// In some situations it might be useful to disable the extender functionality such
		/// as when all databinding is driven through code or when using the IwwDataBinder
		/// interface with custom designed controls that have their own DataBinder objects.
		/// </summary>
		[Browsable(true), Description("Determines whether this control works as an Extender object to other controls on the form"), DefaultValue(true)]
		public bool IsExtender
		{
			get { return _IsExtender; }
			set { _IsExtender = value; }
		}
		private bool _IsExtender = true;


		/// <summary>
		/// Event that can be hooked to validate each control after it's been unbound. 
		/// Allows for doing application level validation of the data once it's been 
		/// returned.
		/// 
		/// This method receives a wwDataBindingItem parameter which includes a 
		/// reference to both the control and the DataSource object where you can check
		///  values. Return false from the event method to indicate that validation 
		/// failed which causes a new BindingError to be created and added to the 
		/// BindingErrors collection.
		/// </summary>
		/// <example>
		/// &lt;&lt;code lang=&quot;C#&quot;&gt;&gt;protected bool 
		/// DataBinder_ValidateControl(GalleryServer.WebControls.wwDataBindingItem Item)
		/// {
		///     if (Item.ControlInstance == this.txtCategoryId)
		///     {
		///         DropDownList List = Item.ControlInstance as DropDownList;
		///         if (List.SelectedItem.Text == &quot;Dairy Products&quot;)
		///         {
		///             Item.BindingErrorMessage = &quot;Dairy Properties not allowed 
		/// (ValidateControl)&quot;;
		///             return false;
		///         }
		///     }
		/// 
		///     return true;
		/// }&lt;&lt;/code&gt;&gt;
		/// </example>
		[Description("Fired after a control has been unbound. Gets passed an instance of the DataBinding item. Event must check for the control to check.")]
		public event delItemResultNotification ValidateControl;

		/// <summary>
		/// Fired just before the control is bound. You can return false from the 
		/// handler to cause the control to not be bound
		/// </summary>
		[Description("Fires immediately before a control is bound. Fires for all controls and is passed a DataBindingItem.")]
		public event delItemResultNotification BeforeBindControl;

		/// <summary>
		/// Fires immediately after the control has been bound. You can check for
		/// errors or perform additional validation.
		/// </summary>
		[Description("Fires immediately after a control has been bound. Gets passed a DataBinding Item instance. Fires for all bound controls.")]
		public event delItemNotification AfterBindControl;

		/// <summary>
		/// Fires immediately before unbinding of a control takes place.
		/// You can return false to abort DataUnbinding.wl
		/// </summary
		[Description("Fires immediately before a control is unbound. Gets passed a DataBinding Item instance. Fires for all bound controls.")]
		public event delItemResultNotification BeforeUnbindControl;

		/// <summary>
		/// Fires immediately after binding is complete. You can check for errors 
		/// and take additional action. 
		/// </summary>
		[Description("Fires immediately after a control has been unbound. Gets passed a DataBinding Item instance. Fires for all bound controls.")]
		public event delItemNotification AfterUnbindControl;


		/// <summary>
		/// Binds the controls that are attached to this DataBinder.
		/// </summary>
		/// <returns>true if there no errors. False otherwise.</returns>
		public new bool DataBind()
		{
			// Change by Roger Martin 2008.11.18: Originally this method was a single line:
			// return this.DataBind(this.Page);
			// However, when the BindingSource was a property of a user control rather than a page, the DataBind was unable to find
			// it. Therefore, I changed is so that, if this instance is on a user control, it assumes the BindingSource is a property
			// of the user control; otherwise it assumes it is a property of the page.
			if (this.Parent is UserControl)
				return this.DataBind(this.Parent);
			else
				return this.DataBind(this.Page);
		}


		/// <summary>
		/// Binds data of the specified controls into the specified bindingsource
		/// </summary>
		/// <param name="Container">The top level container that is bound</param>
		public bool DataBind(Control Container)
		{
			if (this.AutoLoadDataBoundControls)
				this.LoadFromControls(Container);

			bool ResultFlag = true;

			// *** Run through each item and bind it
			foreach (wwDataBindingItem Item in this.DataBindingItems)
			{
				try
				{
					if (this.BeforeBindControl != null)
					{
						if (!this.BeforeBindControl(Item))
							continue;
					}

					// *** Here's where all the work happens
					Item.DataBind(Container);
				}
				// *** Binding errors fire into here
				catch (Exception ex)
				{
					this.HandleUnbindingError(Item, ex);
				}

				if (this.AfterBindControl != null)
					this.AfterBindControl(Item);
			}

			return ResultFlag;

		}

		/// <summary>
		/// Unbind the controls back into their underlying binding sources. Returns true on success
		/// false on failure. On failure the BindingErrors collection will be set
		/// </summary>
		/// <returns>True if there are no errors. False if unbinding failed. Check the BindingErrors for errors.</returns>
		public bool Unbind()
		{
			return this.Unbind(this.Page);
		}

		/// <summary>
		/// Unbind the controls back into their binding source. Returns true on success
		/// false on failure. On failure the BindingErrors collection will be set
		/// </summary>
		/// <param name="Container">The top level container Control that is bound.</param>
		/// <returns>True if there are no errors. False if unbinding failed. Check the BindingErrors for errors.</returns>
		public bool Unbind(Control Container)
		{
			if (this.AutoLoadDataBoundControls)
				this.LoadFromControls(Container);

			bool ResultFlag = true;

			// *** Loop through all bound items and unbind them
			foreach (wwDataBindingItem Item in this.DataBindingItems)
			{
				try
				{
					if (this.BeforeUnbindControl != null)
					{
						if (!this.BeforeUnbindControl(Item))
							continue;
					}

					// *** here's where all the work happens!
					Item.Unbind(Container);

					// *** Run any validation logic - DataBinder Global first
					if (!OnValidateControl(Item))
						this.HandleUnbindingError(Item, new ValidationErrorException(Item.BindingErrorMessage));

					// *** Run control specific validation
					if (!Item.OnValidate())
						this.HandleUnbindingError(Item, new ValidationErrorException(Item.BindingErrorMessage));
				}
				// *** Handles any unbinding errors
				catch (Exception ex)
				{
					this.HandleUnbindingError(Item, ex);
					ResultFlag = false;
				}

				// *** Notify that we're done unbinding
				if (this.AfterUnbindControl != null)
					this.AfterUnbindControl(Item);
			}

			// *** Add existing validators to the BindingErrors
			foreach (IValidator Validator in this.Page.Validators)
			{
				if (Validator.IsValid)
					continue;

				string ClientId = null;

				BaseValidator BValidator = Validator as BaseValidator;
				if (BValidator != null)
				{
					Control Ctl = wwUtils.FindControlRecursive(this.Page, BValidator.ControlToValidate);
					if (Ctl != null)
						ClientId = Ctl.ClientID;
				}
				this.BindingErrors.Add(new BindingError(Validator.ErrorMessage, ClientId));
			}

			return ResultFlag;
		}

		/// <summary>
		/// Manages errors that occur during unbinding. Sets BindingErrors collection and
		/// and writes out validation error display to the page if specified
		/// </summary>
		/// <param name="Item"></param>
		/// <param name="ex"></param>
		private void HandleUnbindingError(wwDataBindingItem Item, Exception ex)
		{
			Item.IsBindingError = true;

			// *** Display Error info by setting BindingErrorMessage property
			try
			{
				string ErrorMessage = null;

				// *** Must check that the control exists - if invalid ID was
				// *** passed there may not be an instance!
				if (Item.ControlInstance == null)
					ErrorMessage = "Invalid Control: " + Item.ControlId;
				else
				{
					string DerivedUserFieldName = this.DeriveUserFieldName(Item);
					if (ex is RequiredFieldException)
					{
						ErrorMessage = DerivedUserFieldName + " can't be left empty";
					}
					else if (ex is ValidationErrorException)
					{
						/// *** Binding Error Message will be set
						ErrorMessage = ex.Message;
					}
					// *** Explicit error message returned
					else if (ex is BindingErrorException)
					{
						ErrorMessage = ex.Message + " for " + DerivedUserFieldName;
					}
					else
					{
						if (string.IsNullOrEmpty(Item.BindingErrorMessage))
							ErrorMessage = DerivedUserFieldName + ": " + ex.Message; // Change to this 2009.01.26 to yield a more informative message
							//ErrorMessage = "Invalid format for " + DerivedUserFieldName;
						else
							// *** Control has a pre-assigned error message
							ErrorMessage = Item.BindingErrorMessage;
					}
				}
				this.AddBindingError(ErrorMessage, Item);
			}
			catch (Exception)
			{
				this.AddBindingError("Binding Error", Item);
			}
		}

		/// <summary>
		/// Adds a binding to the control. This method is a simple
		/// way to establish a binding.
		/// 
		/// Returns the Item so you can customize properties further
		/// </summary>
		/// <param name="ControlToBind"></param>
		/// <param name="ControlPropertyToBind"></param>
		/// <param name="SourceObjectToBindTo"></param>
		/// <param name="SourceMemberToBindTo"></param>
		public wwDataBindingItem AddBinding(Control ControlToBind, string ControlPropertyToBind,
											object SourceObjectToBindTo, string SourceMemberToBindTo)
		{
			wwDataBindingItem Item = new wwDataBindingItem(this);

			Item.ControlInstance = ControlToBind;
			Item.ControlId = ControlToBind.ID;
			Item.BindingSourceObject = SourceObjectToBindTo;
			Item.BindingSourceMember = SourceMemberToBindTo;

			this.DataBindingItems.Add(Item);

			return Item;
		}

		/// <summary>
		/// Adds a binding to the control. This method is a simple
		/// way to establish a binding.
		/// 
		/// Returns the Item so you can customize properties further
		/// </summary>
		/// <param name="ControlToBind"></param>
		/// <param name="ControlPropertyToBind"></param>
		/// <param name="SourceObjectNameToBindTo"></param>
		/// <param name="SourceMemberToBindTo"></param>
		/// <returns></returns>
		public wwDataBindingItem AddBinding(Control ControlToBind, string ControlPropertyToBind,
											string SourceObjectNameToBindTo, string SourceMemberToBindTo)
		{
			wwDataBindingItem Item = new wwDataBindingItem(this);

			Item.ControlInstance = ControlToBind;
			Item.ControlId = ControlToBind.ID;
			Item.Page = this.Page;
			Item.BindingSource = SourceObjectNameToBindTo;
			Item.BindingSourceMember = SourceMemberToBindTo;

			this.DataBindingItems.Add(Item);

			return Item;
		}

		/// <summary>
		/// This method only adds a data binding item, but doesn't bind it
		/// to anything. This can be useful for only displaying errors
		/// </summary>
		/// <param name="ControlToBind"></param>
		/// <returns></returns>
		public wwDataBindingItem AddBinding(Control ControlToBind)
		{
			wwDataBindingItem Item = new wwDataBindingItem(this);

			Item.ControlInstance = ControlToBind;
			Item.ControlId = ControlToBind.ID;
			Item.Page = this.Page;

			this.DataBindingItems.Add(Item);

			return Item;
		}

		/// <summary>
		/// Adds a binding error message to a specific control attached to this binder
		/// BindingErrors collection.
		/// </summary>
		/// <param name="ControlName">Form relative Name (ID) of the control to set the error on</param>
		/// <param name="ErrorMessage">The Error Message to set it to.</param>
		/// <returns>true if the control was found. False if not found, but message is still assigned</returns>
		public bool AddBindingError(string ErrorMessage, string ControlName)
		{
			wwDataBindingItem DataBindingItem = null;

			foreach (wwDataBindingItem Ctl in this.DataBindingItems)
			{
				if (Ctl.ControlId == ControlName)
				{
					DataBindingItem = Ctl;
					break;
				}
			}

			if (DataBindingItem == null)
			{
				this.BindingErrors.Add(new BindingError(ErrorMessage));
				return false;
			}

			return this.AddBindingError(ErrorMessage, DataBindingItem);
		}

		/// <summary>
		/// Adds a binding error to the collection of binding errors.
		/// </summary>
		/// <param name="ErrorMessage"></param>
		/// <param name="control"></param>
		/// <returns>false if the control was not able to get a control reference to attach hotlinks and an icon. Error message always gets added</returns>
		public bool AddBindingError(string ErrorMessage, Control Control)
		{
			wwDataBindingItem DataBindingItem = null;

			if (Control == null)
			{
				this.BindingErrors.Add(new BindingError(ErrorMessage));
				return false;
			}


			foreach (wwDataBindingItem Ctl in this.DataBindingItems)
			{
				if (Ctl.ControlId == Control.ID)
				{
					Ctl.ControlInstance = Control;
					DataBindingItem = Ctl;
					break;
				}
			}

			// *** No associated control found - just add the error message
			if (DataBindingItem == null)
			{
				this.BindingErrors.Add(new BindingError(ErrorMessage, Control.ClientID));
				return false;
			}

			return this.AddBindingError(ErrorMessage, DataBindingItem);
		}

		/// <summary>
		/// Adds a binding error for DataBindingItem control. This is the most efficient
		/// way to add a BindingError. The other overloads call into this method after
		/// looking up the Control in the DataBinder.
		/// </summary>
		/// <param name="ErrorMessage"></param>
		/// <param name="BindingItem"></param>
		/// <returns></returns>
		public bool AddBindingError(string ErrorMessage, wwDataBindingItem BindingItem)
		{

			// *** Associated control found - add icon and link id
			if (BindingItem.ControlInstance != null)
				this.BindingErrors.Add(new BindingError(ErrorMessage, BindingItem.ControlInstance.ClientID));
			else
			{
				// *** Just set the error message
				this.BindingErrors.Add(new BindingError(ErrorMessage));
				return false;
			}

			BindingItem.BindingErrorMessage = ErrorMessage;

			// *** Insert the error text/icon as a literal
			if (this.ShowBindingErrorsOnControls && BindingItem.ControlInstance != null)
			{
				// *** Retrieve the Html Markup for the error
				// *** NOTE: If script code injection is enabled this is done with client
				// ***       script code to avoid Controls.Add() functionality which may not
				// ***       always work reliably if <%= %> tags are in document. Script HTML injection
				// ***       is the preferred behavior as it should work on any page. If script is used
				// ***       the message returned is blank and the startup script is embedded instead
				string HtmlMarkup = this.GetBindingErrorMessageHtml(BindingItem);

				if (!string.IsNullOrEmpty(HtmlMarkup))
				{
					LiteralControl Literal = new LiteralControl(HtmlMarkup);
					Control Parent = BindingItem.ControlInstance.Parent;

					int CtlIdx = Parent.Controls.IndexOf(BindingItem.ControlInstance);
					try
					{
						// *** Can't add controls to the Control collection if <%= %> tags are on the page
						Parent.Controls.AddAt(CtlIdx + 1, Literal);
					}
					catch { ; }
				}
			}

			return true;
		}

#if USE_WWBUSINESS
		/// <summary>
		/// Takes a collection of ValidationErrors and assigns it to the
		/// matching controls. These controls must match in signature as follows:
		/// Must have the same name as the field and a 3 letter prefix. For example,
		/// 
		/// txtCompany - matches company field
		/// cmbCountry - matches the Country field
		/// </summary>
		/// <returns></returns>
		public void AddValidationErrorsToBindingErrors(Westwind.BusinessObjects.ValidationErrorCollection Errors) 
		{
			foreach (Westwind.BusinessObjects.ValidationError Error in Errors) 
			{
                Control ctl = wwWebUtils.FindControlRecursive(this.Page.Form,Error.ControlID);
                this.AddBindingError(Error.Message,ctl);        
			}
		}
#endif


		/// <summary>
		/// Picks up all controls on the form that implement the IwwDataBinder interface
		/// and adds them to the DataBindingItems Collection
		/// </summary>
		/// <param name="Container"></param>
		/// <returns></returns>
		public void LoadFromControls(Control Container)
		{
			// *** Only allow loading of controls implicitly once
			if (this._AutoLoadedDataBoundControls)
				return;
			this._AutoLoadedDataBoundControls = true;

			LoadDataBoundControls(Container);
		}

		/// <summary>
		/// Loop through all of the contained controls of the form and
		/// check for all that implement IwwDataBinder. If found
		/// add the BindingItem to this Databinder
		/// </summary>
		/// <param name="Container"></param>
		private void LoadDataBoundControls(Control Container)
		{
			foreach (Control Ctl in Container.Controls)
			{
				// ** Recursively call down into any containers
				if (Ctl.Controls.Count > 0)
					this.LoadDataBoundControls(Ctl);

				if (Ctl is IwwDataBinder)
					this.DataBindingItems.Add(((IwwDataBinder)Ctl).BindingItem);
			}
		}

		/// <summary>
		/// Returns a UserField name. Returns UserFieldname if set, or if not
		/// attempts to derive the name based on the field.
		/// </summary>
		/// <param name="Control"></param>
		/// <returns></returns>
		protected string DeriveUserFieldName(wwDataBindingItem Item)
		{
			if (!string.IsNullOrEmpty(Item.UserFieldName))
				return Item.UserFieldName;

			string ControlID = Item.ControlInstance.ID;

			// *** Try to get a name by stripping of control prefixes
			string ControlName = Regex.Replace(Item.ControlInstance.ID, "^txt|^chk|^lst|^rad|", "", RegexOptions.IgnoreCase);
			if (ControlName != ControlID)
				return ControlName;

			// *** Nope - use the default ID
			return ControlID;
		}


		/// <summary>
		/// Creates the text for binding error messages based on the 
		/// BindingErrorMessage property of a data bound control.
		/// 
		/// If set the control calls this method render the error message. Called by 
		/// the various controls to generate the error HTML based on the <see>Enum 
		/// ErrorMessageLocations</see>.
		/// 
		/// If UseClientScriptHtmlInjection is set the error message is injected
		/// purely through a client script JavaScript function which avoids problems
		/// with Controls.Add() when script tags are present in the container.
		/// </summary>
		/// <param name="Item">
		/// Instance of the control that has an error.
		/// </param>
		/// <returns>String</returns>
		internal string GetBindingErrorMessageHtml(wwDataBindingItem Item)
		{
			string Image = null;
			if (string.IsNullOrEmpty(this.ErrorIconUrl) || this.ErrorIconUrl == "WebResource")
				Image = Web.Utils.GetSkinnedUrl("images/error-s.png");
			else
				Image = this.ResolveUrl(this.ErrorIconUrl);

			string Message = "";

			if (Item.ErrorMessageLocation == BindingErrorMessageLocations.WarningIconRight)
				Message = String.Format(CultureInfo.CurrentCulture, "&nbsp;<img src=\"{0}\" alt=\"{1}\" />", Image, Item.BindingErrorMessage);
			else if (Item.ErrorMessageLocation == BindingErrorMessageLocations.RedTextBelow)
				Message = "<br /><span style=\"color:red;\"><smaller>" + Item.BindingErrorMessage + "</smaller></span>";
			else if (Item.ErrorMessageLocation == BindingErrorMessageLocations.RedTextAndIconBelow)
				Message = String.Format(CultureInfo.CurrentCulture, "<br /><img src=\"{0}\"> <span style=\"color:red;\" /><smaller>{1}</smaller></span>", Image, Item.BindingErrorMessage);
			else if (Item.ErrorMessageLocation == BindingErrorMessageLocations.None)
				Message = "";
			else
				Message = "<span style='color:red;font-weight:bold;'> * </span>";

			// *** Fix up message so ' are allowed
			Message = Message.Replace("'", @"\'");


			// *** Use Client Side JavaScript to inject the message rather than adding a control
			if (this.UseClientScriptHtmlInjection && Item.ControlInstance != null)
			{
				if (!this._ClientScriptInjectionScriptAdded)
					this.AddScriptForAddHtmlAfterControl();

				this.Page.ClientScript.RegisterStartupScript(this.GetType(), Item.ControlId,
						String.Format(CultureInfo.CurrentCulture, "AddHtmlAfterControl('{0}','{1}');\r\n", Item.ControlInstance.ClientID, Message), true);

				// *** Message is handled in script so nothing else to write
				Message = "";
			}


			// *** Message will be embedded with a Literal Control
			return Message;
		}

		/// <summary>
		/// This method adds the static script to handle injecting the warning icon/messages 
		/// into the page as literal strings.
		/// </summary>
		private void AddScriptForAddHtmlAfterControl()
		{
			this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "AddHtmlAfterControl",
	 @"function AddHtmlAfterControl(ControlId,HtmlMarkup)
{
var Ctl = document.getElementById(ControlId);
if (Ctl == null)
 return;
 
var Insert = document.createElement('span');
Insert.innerHTML = HtmlMarkup;

var Sibling = Ctl.nextSibling;
if (Sibling != null)
 Ctl.parentNode.insertBefore(Insert,Sibling);
else
 Ctl.parentNode.appendChild(Insert);
}", true);

		}

		/// <summary>
		/// Fires the ValidateControlEvent
		/// </summary>
		/// <param name="Item"></param>
		/// <returns>false - Validation for control failed and a BindingError is added, true - Validation succeeded</returns>
		public bool OnValidateControl(wwDataBindingItem Item)
		{
			if (this.ValidateControl != null && !this.ValidateControl(Item))
				return false;

			return true;
		}

		#region IExtenderProvider Members

		/// <summary>
		/// Determines whether a control can be extended. Basically
		/// we allow ANYTHING to be extended so all controls except
		/// the databinder itself are extendable.
		/// 
		/// Optionally the control can be set up to not act as 
		/// an extender in which case the IsExtender property 
		/// can be set to false
		/// </summary>
		/// <param name="extendee"></param>
		/// <returns></returns>
		public bool CanExtend(object extendee)
		{
			if (!this.IsExtender)
				return false;

			// *** Don't extend ourself <g>
			if (extendee is wwDataBinder)
				return false;

			if (extendee is Control)
				return true;

			return false;
		}

		/// <summary>
		/// Returns a specific DataBinding Item for a given control.
		/// Always returns an item even if the Control is not found.
		/// If you need to check whether this is a valid item check
		/// the BindingSource property for being blank.
		/// 
		/// Extender Property Get method
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public wwDataBindingItem GetDataBindingItem(Control control)
		{

			foreach (wwDataBindingItem Item in this.DataBindingItems)
			{
				if (Item.ControlId == control.ID)
				{
					// *** Ensure the binder is set on the item
					Item.Binder = this;
					return Item;
				}
			}

			wwDataBindingItem NewItem = new wwDataBindingItem(this);
			NewItem.ControlId = control.ID;
			NewItem.ControlInstance = control;

			this.DataBindingItems.Add(NewItem);

			return NewItem;
		}

		/// <summary>
		/// Return a specific databinding item for a give control id.
		/// Note unlike the ControlInstance version return null if the
		/// ControlId isn't found. 
		/// </summary>
		/// <param name="ControlId"></param>
		/// <returns></returns>
		public wwDataBindingItem GetDataBindingItem(string ControlId)
		{
			for (int i = 0; i < this.DataBindingItems.Count; i++)
			{
				if (this.DataBindingItems[i].ControlId == ControlId)
					return this.DataBindingItems[i];
			}

			return null;
		}

		/// <summary>
		/// This is never fired in ASP.NET runtime code
		/// </summary>
		/// <param name="extendee"></param>
		/// <param name="Item"></param>
		//public void SetDataBindingItem(object extendee, object Item)
		//{
		//   wwDataBindingItem BindingItem = Item as wwDataBindingItem;


		//    Control Ctl = extendee as Control;

		//    HttpContext.Current.Response.Write("SetDataBindingItem fired " + BindingItem.ControlId);
		//}

		/// <summary>
		/// this method is used to ensure that designer is notified
		/// every time there is a change in the sub-ordinate validators
		/// </summary>
		internal void NotifyDesigner()
		{
			if (this.DesignMode)
			{
				IDesignerHost Host = this.Site.Container as IDesignerHost;
				ControlDesigner Designer = Host.GetDesigner(this) as ControlDesigner;
				PropertyDescriptor Descriptor = null;
				try
				{
					Descriptor = TypeDescriptor.GetProperties(this)["DataBindingItems"];
				}
				catch
				{
					return;
				}

				ComponentChangedEventArgs ccea = new ComponentChangedEventArgs(
										this,
										Descriptor,
										null,
										this.DataBindingItems);
				Designer.OnComponentChanged(this, ccea);
			}
		}


		#endregion
	}

	public delegate bool delItemResultNotification(wwDataBindingItem Item);

	public delegate void delItemNotification(wwDataBindingItem Item);

	public delegate void delDataBindingItemValidate(object sender, DataBindingValidationEventArgs e);


	///// <summary>
	///// Control designer used so we get a grey button display instead of the 
	///// default label display for the control.
	///// </summary>
	//[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	//public class wwDataBinderDesigner : ControlDesigner
	//{
	//  public override string GetDesignTimeHtml()
	//  {
	//    return base.CreatePlaceHolderDesignTimeHtml("Control Extender");
	//  }
	//}
}
