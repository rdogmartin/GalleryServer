using System;
using System.Collections.Generic;
using System.Text;

namespace GalleryServer.WebControls
{
	/// <summary>
	/// This class provides a holding container for BindingErrors. BindingErrors 
	/// occur during binding and unbinding of controls and any errors are stored in
	///  this collection. This class is used extensively for checking for 
	/// validation errors and then displaying them with the ToString() or ToHtml() 
	/// methods.
	/// </summary>
	public class BindingErrors : List<BindingError>
	{

		/// <summary>
		/// Formats all the BindingErrors into a rich list of error messages. The error
		///  messages are marked up with links to the appropriate controls. Format of 
		/// the list is a &lt;ul&gt; style list ready to display in an HTML page.
		/// </summary>
		/// <returns>an Html string of the errors</returns>
		public string ToHtml()
		{
			if (this.Count < 1)
				return "";

			StringBuilder sb = new StringBuilder("");
			sb.Append("\r\n<ul>");
			foreach (BindingError Error in this)
			{
				sb.Append("<li style='margin-left:0px;'>");
				if (Error.ClientID != null && Error.ClientID != "")
					sb.Append("<a href='javascript:;' onclick=\"var T = document.getElementById('" + Error.ClientID + "'); if(T == null) { return }; T.style.borderWidth='2px';T.style.borderColor='Red';try {T.focus();} catch(e) {;} " +
										@"window.setTimeout('T=document.getElementById(\'" + Error.ClientID + @"\');T.style.borderWidth=\'\';T.style.borderColor=\'\';',3000);" + "\"" +
										">" + Error.Message + "</a>\r\n</li>");
				else
					sb.Append(Error.Message + "\r\n");
			}
			sb.Append("</ul>");
			return sb.ToString();
		}


		/// <summary>
		/// Formats an Binding Errors collection as a string with carriage returns
		/// </summary>
		/// <param name="Errors"></param>
		/// <returns></returns>
		public override string ToString()
		{
			// *** Optional Error Parsing
			if (this.Count > 0)
			{
				StringBuilder sb = new StringBuilder("");
				foreach (BindingError Error in this)
				{
					sb.Append(Error.Message + " ");
				}
				return sb.ToString();
			}

			return "";
		}
	}


	/// <summary>
	/// Error object used to return error information during databinding.
	/// </summary>
	public class BindingError
	{
		/// <summary>
		/// The ClientID of the control the error occurred on. This value is used to 
		/// provide the hot linking to the control.
		/// </summary>
		public string ClientID
		{
			get { return this._ClientID; }
			set { this._ClientID = value; }
		}
		string _ClientID = "";

		/// <summary>
		/// The error message that is displayed for the Binding error.
		/// </summary>
		public string Message
		{
			get { return this._Message; }
			set { this._Message = value; }
		}
		string _Message = "";

		/// <summary>
		/// The raw Exception error message. Not used at the moment.
		/// </summary>
		public string ErrorMessage
		{
			get { return this._cErrorMessage; }
			set { this._cErrorMessage = value; }
		}
		string _cErrorMessage;


		//public string Id 
		//{
		//    get { return this._Id; }
		//    set { this._Id = value; }
		//}
		//string _Id = "";





		///// <summary>
		///// The code that caused the error
		///// </summary>
		//public string Source
		//{
		//    get { return this._cSource; }
		//    set { this._cSource = value; }
		//}
		//string _cSource;

		///// <summary>
		///// The call stack that led up to the error
		///// </summary>
		//public string StackTrace
		//{
		//    get { return this._StackTrace; }
		//    set { this._StackTrace = value; }
		//}
		//string _StackTrace;

		/// <summary>
		/// Initializes a new instance of the <see cref="BindingError"/> class.
		/// </summary>
		public BindingError()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BindingError"/> class.
		/// </summary>
		/// <param name="errorMessage">The error message.</param>
		public BindingError(string errorMessage)
		{
			this.Message = errorMessage;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BindingError"/> class.
		/// </summary>
		/// <param name="errorMessage">The error message.</param>
		/// <param name="clientId">The client ID.</param>
		public BindingError(string errorMessage, string clientId)
		{
			this.Message = errorMessage;
			//this.Id = "txt" + ClientID;

			if (clientId == null)
				clientId = "";

			this.ClientID = clientId;
		}
	}



}
