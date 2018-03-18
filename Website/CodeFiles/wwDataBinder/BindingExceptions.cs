using System;
using System.Collections.Generic;
using System.Text;

namespace GalleryServer.WebControls
{

	/// <summary>
	/// Exception thrown when a required field is not filled in. Used internally
	/// for catching these errors and rendering the error.
	/// </summary>
	public class RequiredFieldException : ApplicationException
	{
		/// <summary>
		/// Initializes a new RequiredFieldException.
		/// </summary>
		public RequiredFieldException() : base() { }
		/// <summary>
		/// Initializes a new RequiredFieldException.
		/// </summary>
		/// <param name="Message">A message describing this exception.</param>
		public RequiredFieldException(string Message) : base(Message) { }
	}

	/// <summary>
	/// Exception thrown when a BindingError is encountered
	/// </summary>
	public class BindingErrorException : ApplicationException
	{
		/// <summary>
		/// Initializes a new BindingErrorException.
		/// </summary>
		public BindingErrorException() : base() { }

		/// <summary>
		/// Initializes a new BindingErrorException.
		/// </summary>
		/// <param name="Message">A message describing this exception.</param>
		public BindingErrorException(string Message) : base(Message) { }
	}

	/// <summary>
	/// An exception fired if a validation error occurs in DataBinding
	/// </summary>
	public class ValidationErrorException : BindingErrorException
	{
		public ValidationErrorException() : base() { }
		public ValidationErrorException(string Message) : base(Message) { }
	}


}
