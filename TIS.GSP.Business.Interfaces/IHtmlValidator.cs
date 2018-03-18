using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Provides functionality for validating and cleaning HTML.
	/// </summary>
	public interface IHtmlValidator
	{
		/// <summary>
		/// Gets the list of HTML tags found in the user-entered input that are not allowed. This property is set after
		/// the <see cref="Validate"/> method is invoked. Guaranteed to not be null.
		/// </summary>
		/// <value>The list of HTML tags found in the user-entered input that are not allowed.</value>
		List<String> InvalidHtmlTags { get; }

		/// <summary>
		/// Gets the list of HTML attributes found in the user-entered input that are not allowed. This property is set after
		/// the <see cref="Validate"/> method is invoked. Guaranteed to not be null.
		/// </summary>
		/// <value>The list of HTML attributes found in the user-entered input that are not allowed.</value>
		List<String> InvalidHtmlAttributes { get; }

		/// <summary>
		/// Gets a value indicating whether invalid javascript was detected in the HTML. This property is set after
		/// the <see cref="Validate"/> method is invoked. Returns <c>true</c> only when the configuration setting 
		/// allowUserEnteredJavascript is <c>false</c> and either a script tag or the string "javascript:" is detected.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if invalid javascript is detected; otherwise, <c>false</c>.
		/// </value>
		bool InvalidJavascriptDetected { get; }

		/// <summary>
		/// Evaluates the HTML for invalid tags, attributes, and javascript. After executing this method the <see cref="IsValid"/>
		/// property can be checked. If this property is <c>true</c>, the properties <see cref="InvalidHtmlTags"/>, 
		/// <see cref="InvalidHtmlAttributes"/>, and <see cref="InvalidJavascriptDetected"/> can be inspected for details.
		/// </summary>
		void Validate();

		/// <summary>
		/// Gets a value indicating whether any invalid HTML tags, attributes, or javascript was found in the HTML.
		/// </summary>
		/// <value><c>true</c> if invalid HTML tags, attributes, or javascript was found; otherwise, <c>false</c>.</value>
		bool IsValid { get; }
	}
}
