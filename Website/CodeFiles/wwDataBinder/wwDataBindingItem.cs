using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GalleryServer.WebControls.Tools;

namespace GalleryServer.WebControls
{
	/// <summary>
	/// An individual binding item. A BindingItem maps a source object - 
	/// a property/field or database field - to a property of a Control object.
	///
	/// The object is a child for the wwDataBinder object which acts as a master
	/// object that performs the actual binding of individual BingingItems.
	/// 
	/// Binding Items can be attached to controls and if the control implements the
	/// IwwDataBinder
	/// </summary>
	//[TypeConverter(typeof(wwDataItemTypeConverter))]
	[ToolboxData("<{0}:wwDataBindingItem runat=server />")]
	[Category("Data")]
	[DefaultEvent("Validate")]
	[Description("An individual databinding item that allows you to bind a source binding source - a database field or Object property typically - to a target control property")]
	[Serializable]
	public class wwDataBindingItem : Control
	{
		/// <summary>
		/// Explicitly set designmode flag - stock doesn't work on Collection child items
		/// </summary>
		protected new bool DesignMode = (HttpContext.Current == null);

		/// <summary>
		/// Default Constructor
		/// </summary>
		public wwDataBindingItem()
		{
		}

		/// <summary>
		/// Overridden constructor to allow DataBinder to be passed
		/// as a reference. Unfortunately ASP.NET doesn't fire this when
		/// creating the DataBinder child items.
		/// </summary>
		/// <param name="Parent"></param>
		public wwDataBindingItem(wwDataBinder Parent)
		{
			this._Binder = Parent;
		}

		/// <summary>
		/// Reference to the DataBinder parent object.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public wwDataBinder Binder
		{
			get { return _Binder; }
			set { _Binder = value; }
		}
		private wwDataBinder _Binder = null;

		/// <summary>
		/// The ID of the control to that is bound.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("The ID of the control to that is bound."), DefaultValue("")]
		[TypeConverter(typeof(ControlIDConverter))]
		[Browsable(true)]
		public string ControlId
		{
			get
			{
				return _ControlId;
			}
			set
			{
				_ControlId = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private string _ControlId = "";

		/// <summary>
		/// An optional instance of the control that can be assigned. Used internally
		/// by the wwDatBindiner to assign the control whenever possible as the instance
		/// is more efficient and reliable than the string name.
		/// </summary>
		[NotifyParentProperty(false)]
		[Description("An instance value for the controls")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public Control ControlInstance
		{
			get
			{
				return _ControlInstance;
			}
			set
			{
				_ControlInstance = value;
			}
		}
		private Control _ControlInstance = null;

		/// <summary>
		/// The binding source object that is the source for databinding.
		/// This is an object of some sort and can be either a real object
		/// or a DataRow/DataTable/DataSet. If a DataTable is used the first row 
		/// is assumed. If a DataSet is used the first table and first row are assumed.
		///
		/// The object reference is always Page relative, so binding doesn't work
		/// against local variables, only against properties of the form. Form
		/// properties that are bound should be marked public or protected, but
		/// not private as Reflection is used to get the values. 
		/// 
		/// This or me is implicit, but can be specified so
		///  "Customer" or "this.Customer" is equivalent. 
		/// </summary>
		/// <example>
		/// // *** Bind a DataRow Item
		/// bi.BindingSource = "Customer.DataRow";
		/// bi.BindingSourceMember = "LastName";
		///
		/// // *** Bind a DataRow within a DataSet  - not recommended though
		/// bi.BindingSource = "this.Customer.Tables["TCustomers"].Rows[0]";
		/// bi.BindingSourceMember = "LastName";
		///
		/// // *** Bind an Object
		/// bi.BindingSource = "InventoryItem.Entity";
		/// bi.BindingSourceMember = "ItemPrice";
		/// 
		/// // *** Bind a form property
		/// bi.BindingSource = "this";   // also "me" 
		/// bi.BindingSourceMember = "CustomerPk";
		/// </example>
		[NotifyParentProperty(true)]
		[Description("The name of the object or DataSet/Table/Row to bind to. Page relative. Example: Customer.DataRow = this.Customer.DataRow"), DefaultValue("")]
		public string BindingSource
		{
			get { return _BindingSource; }
			set
			{
				_BindingSource = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private string _BindingSource = "";


		/// <summary>
		/// An instance of the object that the control is bound to
		/// Optional - can be passed instead of a BindingSource string. Using
		/// a reference is more efficient. Declarative use in the designer
		/// always uses strings, code base assignments should use instances
		/// with BindingSourceObject.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object BindingSourceObject
		{
			get { return _BindingSourceObject; }
			set
			{
				_BindingSourceObject = value;
			}
		}
		private object _BindingSourceObject = null;

		/// <summary>
		/// The property or field on the Binding Source that is
		/// bound. Example: BindingSource: Customer.Entity BindingSourceMember: Company
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("The name of the property or field to bind to. Example: So you can bind to a BindingSource of Customer.DataRow and the BindingSourceMember is Company."), DefaultValue("")]
		public string BindingSourceMember
		{
			get { return _BindingSourceMember; }
			set
			{
				_BindingSourceMember = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private string _BindingSourceMember = "";

		/// <summary>
		/// Property that is bound on the target controlId
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("Property that is bound on the target control"), DefaultValue("Text")]
		public string BindingProperty
		{
			get { return _BindingProperty; }
			set
			{
				_BindingProperty = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private string _BindingProperty = "Text";

		/// <summary>
		/// Format Expression ( {0:c) ) applied to the binding source when it's displayed.
		/// Watch out for two way conversion issues when formatting this way. If you create
		/// expressions and you are also saving make sure the format used can be saved back.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("Format Expression ( {0:c) ) applied to the binding source when it's displayed."), DefaultValue("")]
		public string DisplayFormat
		{
			get { return _DisplayFormat; }
			set
			{
				_DisplayFormat = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private string _DisplayFormat = "";

		/// <summary>
		/// If set requires that the control contains a value, otherwise a validation error is thrown
		/// Useful mostly on TextBox controls only.
		/// </summary>
		[NotifyParentProperty(true)]
		[Description("If set requires that the control contains a value, otherwise a validation error is thrown - recommended only on TextBox controls."), DefaultValue(false)]
		public bool IsRequired
		{
			get { return _IsRequired; }
			set
			{
				_IsRequired = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private bool _IsRequired = false;

		/// <summary>
		/// A descriptive name for the field used for error display
		/// </summary>
		[Description("A descriptive name for the field used for error display"), DefaultValue("")]
		[NotifyParentProperty(true)]
		public string UserFieldName
		{
			get { return _UserFieldName; }
			set
			{
				_UserFieldName = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private string _UserFieldName = "";

		/// <summary>
		/// Determines how binding and validation errors display on the control
		/// </summary>
		[Description("Determines how binding and validation errors display on the control"),
		 DefaultValue(BindingErrorMessageLocations.WarningIconRight)]
		[NotifyParentProperty(true)]
		public BindingErrorMessageLocations ErrorMessageLocation
		{
			get { return _ErrorMessageLocation; }
			set
			{
				_ErrorMessageLocation = value;
				if (this.DesignMode && this.Binder != null)
					this.Binder.NotifyDesigner();
			}
		}
		private BindingErrorMessageLocations _ErrorMessageLocation = BindingErrorMessageLocations.WarningIconRight;

		/// <summary>
		/// Internal property that lets you know if there was binding error
		/// on this control after binding occurred
		/// </summary>
		[NotifyParentProperty(true)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsBindingError
		{
			get { return _IsBindingError; }
			set { _IsBindingError = value; }
		}
		private bool _IsBindingError = false;

		/// <summary>
		/// An error message that gets set if there is a binding error
		/// on the control.
		/// </summary>
		[NotifyParentProperty(true)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BindingErrorMessage
		{
			get { return _BindingErrorMessage; }
			set { _BindingErrorMessage = value; }
		}
		private string _BindingErrorMessage = "";

		/// <summary>
		/// Determines how databinding and unbinding is done on the target control. 
		/// One way only fires DataBind() and ignores Unbind() calls. 
		/// Two-way does both. None effectively turns off binding.
		/// </summary>
		[Description("Determines how databinding and unbinding is done on the target control. One way only fires DataBind() and ignores Unbind() calls. Two-way does both"),
		Browsable(true), DefaultValue(BindingModes.TwoWay)]
		public BindingModes BindingMode
		{
			get { return _BindingMode; }
			set { _BindingMode = value; }
		}
		private BindingModes _BindingMode = BindingModes.TwoWay;

		/// <summary>
		/// Use this event to hook up special validation logic. Called after binding completes. Return false to indicate validation failed
		/// </summary>
		[Browsable(true), Description("Use this event to hook up special validation logic. Called after binding completes. Return false to indicate validation failed")]
		public event delDataBindingItemValidate Validate;

		/// <summary>
		/// Fires the Validation Event
		/// </summary>
		/// <returns></returns>
		public bool OnValidate()
		{
			if (this.Validate != null)
			{
				DataBindingValidationEventArgs Args = new DataBindingValidationEventArgs();
				Args.DataBindingItem = this;

				this.Validate(this, Args);

				if (!Args.IsValid)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Binds a source object and property to a control's property. For example
		/// you can bind a business object to a the text property of a text box, or 
		/// a DataRow field to a text box field. You specify a binding source object 
		/// (Customer.Entity or Customer.DataRow) and property or field(Company, FirstName)
		/// and bind it to the control and the property specified (Text).
		/// </summary>
		public new void DataBind()
		{
			if (BindingMode == BindingModes.None)
				return;

			if (this.Binder != null)
				this.DataBind(this.Binder.Page);

			this.DataBind(this.Page);
		}

		/// <summary>
		/// Binds a source object and property to a control's property. For example
		/// you can bind a business object to a the text property of a text box, or 
		/// a DataRow field to a text box field. You specify a binding source object 
		/// (Customer.Entity or Customer.DataRow) and property or field(Company, FirstName)
		/// and bind it to the control and the property specified (Text).
		/// </summary>
		/// <param name="WebPage">the Base control that binding source objects are attached to</param>
		public void DataBind(Control WebPage)
		{
			if (BindingMode == BindingModes.None)
				return;

			// *** Empty BindingSource - simply skip
			if (this.BindingSourceObject == null &&
					string.IsNullOrEmpty(this.BindingSource) ||
					string.IsNullOrEmpty(this.BindingSourceMember))
				return;

			// *** Retrieve the binding source either by object reference or by name
			string BindingSource = this.BindingSource;
			object BindingSourceObject = this.BindingSourceObject;

			string BindingSourceMember = this.BindingSourceMember;
			string BindingProperty = this.BindingProperty;

			Control ActiveControl = null;
			if (this.ControlInstance != null)
				ActiveControl = this.ControlInstance;
			else
				ActiveControl = wwUtils.FindControlRecursive(WebPage, this.ControlId);

			try
			{
				if (ActiveControl == null)
					throw new ApplicationException();

				// *** Assign so error handler can get a clean control reference
				this.ControlInstance = ActiveControl;

				// *** Retrieve the bindingsource by name - otherwise we use the 
				if (BindingSourceObject == null)
				{
					// *** Get a reference to the actual control source object
					// *** Allow this or me to be bound to the page
					if (BindingSource == "this" || BindingSource.ToLower() == "me")
						BindingSourceObject = WebPage;
					else
						BindingSourceObject = wwUtils.GetPropertyEx(WebPage, BindingSource);
				}

				if (BindingSourceObject == null)
					throw new BindingErrorException("Invalid BindingSource: " +
																					this.BindingSource + "." + this.BindingSourceMember);

				// *** Retrieve the control source value
				object loValue;

				if (BindingSourceObject is System.Data.DataSet)
				{
					string lcTable = BindingSourceMember.Substring(0, BindingSourceMember.IndexOf("."));
					string lcColumn = BindingSourceMember.Substring(BindingSourceMember.IndexOf(".") + 1);
					DataSet Ds = (DataSet)BindingSourceObject;
					loValue = Ds.Tables[lcTable].Rows[0][lcColumn];
				}
				else if (BindingSourceObject is System.Data.DataRow)
				{
					DataRow Dr = (DataRow)BindingSourceObject;
					loValue = Dr[BindingSourceMember];
				}
				else if (BindingSourceObject is System.Data.DataTable)
				{
					DataTable Dt = (DataTable)BindingSourceObject;
					loValue = Dt.Rows[0][BindingSourceMember];
				}
				else if (BindingSourceObject is System.Data.DataView)
				{
					DataView Dv = (DataView)BindingSourceObject;
					loValue = Dv.Table.Rows[0][BindingSourceMember];
				}
				else
				{
					loValue = wwUtils.GetPropertyEx(BindingSourceObject, BindingSourceMember);
				}

				/// *** Figure out the type of the control we're binding to
				object loBindValue = wwUtils.GetProperty(ActiveControl, BindingProperty);
				string lcBindingSourceType = loBindValue.GetType().Name;

				// TODO: Handle DbNull value here...
				if (loValue == null)
					if (lcBindingSourceType == "String")
						wwUtils.SetProperty(ActiveControl, BindingProperty, "");
					else if (lcBindingSourceType == "Boolean")
						wwUtils.SetProperty(ActiveControl, BindingProperty, false);
					else
						wwUtils.SetProperty(ActiveControl, BindingProperty, "");
				else
				{
					if (lcBindingSourceType == "Boolean")
						wwUtils.SetProperty(ActiveControl, BindingProperty, loValue);
					else
					{
						if (string.IsNullOrEmpty(this.DisplayFormat))
							wwUtils.SetProperty(ActiveControl, BindingProperty, loValue.ToString());
						else
							wwUtils.SetProperty(ActiveControl, BindingProperty, String.Format(this.DisplayFormat, loValue));
					}
				}
			}
			catch (Exception ex)
			{
				string lcException = ex.Message;
				throw (new BindingErrorException("Unable to bind " +
						BindingSource + "." +
						BindingSourceMember));
			}
		}

		/// <summary>
		/// Unbinds control properties back into the control source.
		/// 
		/// This method uses reflection to lift the data out of the control, then 
		/// parses the string value back into the type of the data source. If an error 
		/// occurs the exception is not caught internally, but generally the 
		/// FormUnbindData method captures the error and assigns an error message to 
		/// the BindingErrorMessage property of the control.
		/// </summary>
		public void Unbind()
		{
			if (this.BindingMode != BindingModes.TwoWay)
				return;

			if (this.Binder != null)
				this.Unbind(this.Binder.Page);

			this.Unbind(this.Page);
		}

		/// <summary>
		/// Unbinds control properties back into the control source.
		/// 
		/// This method uses reflection to lift the data out of the control, then 
		/// parses the string value back into the type of the data source. If an error 
		/// occurs the exception is not caught internally, but generally the 
		/// FormUnbindData method captures the error and assigns an error message to 
		/// the BindingErrorMessage property of the control.
		/// </summary>
		/// <param name="WebPage">
		/// The base control that binding sources are based on.
		/// </param>
		public void Unbind(Control WebPage)
		{

			// *** Get the Control Instance first so we ALWAYS have a ControlId
			// *** instance reference available
			Control ActiveControl = null;
			if (this.ControlInstance != null)
				ActiveControl = this.ControlInstance;
			else
				ActiveControl = wwUtils.FindControlRecursive(WebPage, this.ControlId);

			if (ActiveControl == null)
				throw new ApplicationException("Invalid Control Id");

			this.ControlInstance = ActiveControl;

			// *** Don't unbind this item unless we're in TwoWay mode
			if (this.BindingMode != BindingModes.TwoWay)
				return;

			// *** Empty BindingSource - simply skip
			if (this.BindingSourceObject == null &&
					string.IsNullOrEmpty(this.BindingSource) ||
					string.IsNullOrEmpty(this.BindingSourceMember))
				return;

			// *** Retrieve the binding source either by object reference or by name
			string BindingSource = this.BindingSource;
			object BindingSourceObject = this.BindingSourceObject;

			string BindingSourceMember = this.BindingSourceMember;
			string BindingProperty = this.BindingProperty;

			if (BindingSourceObject == null)
			{
				if (BindingSource == null || BindingSource.Length == 0 ||
						BindingSourceMember == null || BindingSourceMember.Length == 0)
					return;

				if (BindingSource == "this" || BindingSource.ToLower() == "me")
					BindingSourceObject = WebPage;
				else
					BindingSourceObject = wwUtils.GetPropertyEx(WebPage, BindingSource);
			}

			if (BindingSourceObject == null)
				throw new ApplicationException("Invalid BindingSource");


			// Retrieve the new value from the control
			object ControlValue = wwUtils.GetPropertyEx(ActiveControl, BindingProperty);

			// Check for Required values not being blank
			if (this.IsRequired && (string)ControlValue == "")
				throw new RequiredFieldException();

			// Try to retrieve the type of the BindingSourceMember
			Type typBindingSource = null;
			string BindingSourceType;
			string DataColumn = null;
			string DataTable = null;

			if (BindingSourceObject is System.Data.DataSet)
			{
				// *** Split out the datatable and column names
				int At = BindingSourceMember.IndexOf(".");
				DataTable = BindingSourceMember.Substring(0, At);
				DataColumn = BindingSourceMember.Substring(At + 1);
				DataSet Ds = (DataSet)BindingSourceObject;
				BindingSourceType = Ds.Tables[DataTable].Columns[DataColumn].DataType.Name;
				typBindingSource = Ds.Tables[DataTable].Columns[DataColumn].DataType;
			}
			else if (BindingSourceObject is System.Data.DataRow)
			{
				DataRow Dr = (DataRow)BindingSourceObject;
				BindingSourceType = Dr.Table.Columns[BindingSourceMember].DataType.Name;
				typBindingSource = Dr.Table.Columns[BindingSourceMember].DataType;
			}
			else if (BindingSourceObject is System.Data.DataTable)
			{
				DataTable dt = (DataTable)BindingSourceObject;
				BindingSourceType = dt.Columns[BindingSourceMember].DataType.Name;
				typBindingSource = dt.Columns[BindingSourceMember].DataType;
			}
			else
			{
				// *** It's an object property or field - get it
				MemberInfo[] MInfo = BindingSourceObject.GetType().GetMember(BindingSourceMember, wwUtils.MemberAccess);
				if (MInfo[0].MemberType == MemberTypes.Field)
				{
					FieldInfo Field = (FieldInfo)MInfo[0];
					BindingSourceType = Field.FieldType.Name;
					typBindingSource = Field.FieldType;
				}
				else
				{
					PropertyInfo loField = (PropertyInfo)MInfo[0];
					BindingSourceType = loField.PropertyType.Name;
					typBindingSource = loField.PropertyType;
				}
			}

			// ***  Retrieve the value
			object AssignedValue;

			if (typBindingSource == typeof(string))
				AssignedValue = ControlValue;
			else if (typBindingSource == typeof(Int16))
			{
				Int16 TValue = 0;
				if (!Int16.TryParse((string)ControlValue, NumberStyles.Integer, Thread.CurrentThread.CurrentCulture.NumberFormat, out TValue))
					throw new BindingErrorException("Invalid numeric input");
				else
					AssignedValue = TValue;
			}
			else if (typBindingSource == typeof(Int32))
			{
				Int32 TValue = 0;
				if (!Int32.TryParse((string)ControlValue, NumberStyles.Integer, Thread.CurrentThread.CurrentCulture.NumberFormat, out TValue))
					throw new BindingErrorException("Invalid numeric input");
				else
					AssignedValue = TValue;
			}
			else if (typBindingSource == typeof(Int64))
			{
				Int64 TValue = 0;
				if (!Int64.TryParse((string)ControlValue, NumberStyles.Integer, Thread.CurrentThread.CurrentCulture.NumberFormat, out TValue))
					throw new BindingErrorException("Invalid numeric input");
				else
					AssignedValue = TValue;
			}
			else if (typBindingSource == typeof(byte))
				AssignedValue = Convert.ToByte(ControlValue);
			else if (typBindingSource == typeof(decimal))
				AssignedValue = Decimal.Parse((string)ControlValue, NumberStyles.Any);
			else if (typBindingSource == typeof(float))
				AssignedValue = Single.Parse((string)ControlValue, NumberStyles.Any);
			else if (typBindingSource == typeof(double))
				AssignedValue = Double.Parse((string)ControlValue, NumberStyles.Any);
			else if (typBindingSource == typeof(bool))
			{
				AssignedValue = ControlValue;
			}
			else if (typBindingSource == typeof(DateTime))
			{
				DateTime TValue = DateTime.MinValue;
				if (!DateTime.TryParse((string)ControlValue, Thread.CurrentThread.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out TValue))
					throw new BindingErrorException("Invalid date input");
				else
					AssignedValue = TValue;
				//AssignedValue = Convert.ToDateTime(loValue);
			}
			else if (typBindingSource.IsEnum)
				AssignedValue = Enum.Parse(typBindingSource, (string)ControlValue);
			else  // Not HANDLED!!!
				// *** Use a generic exception - we don't want to display the error
				throw (new Exception("Field Type not Handled by Data unbinding"));

			/// Write the value back to the underlying object/data item
			if (BindingSourceObject is System.Data.DataSet)
			{
				DataSet Ds = (DataSet)BindingSourceObject;
				Ds.Tables[DataTable].Rows[0][DataColumn] = AssignedValue;
			}
			else if (BindingSourceObject is System.Data.DataRow)
			{
				DataRow Dr = (DataRow)BindingSourceObject;
				Dr[BindingSourceMember] = AssignedValue;
			}
			else if (BindingSourceObject is System.Data.DataTable)
			{
				DataTable dt = (DataTable)BindingSourceObject;
				dt.Rows[0][BindingSourceMember] = AssignedValue;
			}
			else if (BindingSourceObject is System.Data.DataView)
			{
				DataView dv = (DataView)BindingSourceObject;
				dv[0][BindingSourceMember] = AssignedValue;
			}
			else
				wwUtils.SetPropertyEx(BindingSourceObject, BindingSourceMember, AssignedValue);

			// *** Clear the error message - no error
			this.BindingErrorMessage = "";
		}

		/// <summary>
		/// Returns a the control bindingsource and binding source member
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.BindingSource))
				return base.ToString();

			return this.BindingSource + "." + this.BindingSourceMember;
		}


		#region Hide Properties for the Designer

		/// <summary>
		/// Gets or sets the value that uniquely identifies this item.
		/// </summary>
		[Browsable(false)]
		public override string ID
		{
			get
			{
				return base.ID;
			}
			set
			{
				base.ID = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this item is visible.
		/// </summary>
		[Browsable(false)]
		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether viewstate is used to persist object state.
		/// </summary>
		[Browsable(false)]
		public override bool EnableViewState
		{
			get
			{
				return base.EnableViewState;
			}
			set
			{
				base.EnableViewState = value;
			}
		}
		#endregion

	}

	/// <summary>
	/// Enumeration for the various binding error message locations possible
	/// that determine where the error messages are rendered in relation to the
	/// control.
	/// </summary>
	public enum BindingErrorMessageLocations
	{
		/// <summary>
		/// Displays an image icon to the right of the control
		/// </summary>
		WarningIconRight,
		/// <summary>
		/// Displays a text ! next to the control 
		/// </summary>
		TextExclamationRight,
		/// <summary>
		/// Displays the error message as text below the control
		/// </summary>
		RedTextBelow,
		/// <summary>
		/// Displays an icon and the text of the message below the control.
		/// </summary>
		RedTextAndIconBelow,
		/// <summary>
		/// Indicates the error icon is not shown next to the control.
		/// </summary>
		None
	}

	/// <summary>
	/// Determines how databinding is performed for the target control. Note that 
	/// if a wwDataBindingItem is  marked for None or OneWay, the control will not 
	/// be unbound or in the case of None bound even when an explicit call to 
	/// DataBind() or Unbind() is made.
	/// </summary>
	public enum BindingModes
	{
		/// <summary>
		/// Databinding occurs for DataBind() and Unbind()
		/// </summary>
		TwoWay,
		/// <summary>
		/// DataBinding occurs for DataBind() only
		/// </summary>
		OneWay,
		/// <summary>
		/// No binding occurs
		/// </summary>
		None
	}


	/// <summary>
	/// Event Args passed to a Validate event of a wwDataBindingItem control.
	/// </summary>
	public class DataBindingValidationEventArgs : EventArgs
	{
		/// <summary>
		/// Instance of the DataBinding Control that fired this Validation event.
		/// </summary>
		public wwDataBindingItem DataBindingItem
		{
			get { return _DataBindingItem; }
			set { _DataBindingItem = value; }
		}
		private wwDataBindingItem _DataBindingItem = null;

		/// <summary>
		/// Out flag that determines whether this control value is valid.
		/// </summary>
		public bool IsValid
		{
			get { return _IsValid; }
			set { _IsValid = value; }
		}
		private bool _IsValid = true;
	}

}