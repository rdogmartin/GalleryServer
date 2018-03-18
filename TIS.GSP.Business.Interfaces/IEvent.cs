using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents an application event or error that occurred during the execution of Gallery Server code.
	/// </summary>
	public interface IEvent
	{
		/// <summary>
		/// Gets or sets a value that uniquely identifies an application event.
		/// </summary>
		/// <value>A value that uniquely identifies an application event.</value>
		int EventId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the type of the event.
		/// </summary>
		/// <value>The type of the event.</value>
		EventType EventType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the gallery that is the source of this event.
		/// </summary>
		/// <value>The ID of the gallery that is the source of this event</value>
		int GalleryId
		{
			get;
		}

		/// <summary>
		/// Gets the UTC date and time the event occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The date and time the event occurred.</value>
		System.DateTime TimestampUtc
		{
			get;
		}

		/// <summary>
		/// Gets the message associated with the event. Guaranteed to not be null.
		/// </summary>
		/// <value>The message associated with the event.</value>
		string Message
		{
			get;
		}

		/// <summary>
		/// Gets the data associated with the event. When <see cref="EventType" /> is <see cref="Business.EventType.Error" />,
		/// any items in the exception data are added. Guaranteed to not be null.
		/// </summary>
		/// <value>The data associate with the exception.</value>
		List<KeyValuePair<string, string>> EventData
		{
			get;
		}

		/// <summary>
		/// Gets the type of the exception. Contains a value only when <see cref="EventType" /> 
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The type of the exception.</value>
		string ExType
		{
			get;
		}

		/// <summary>
		/// Gets the source of the exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The source of the exception.</value>
		string ExSource
		{
			get;
		}

		/// <summary>
		/// Gets the target site of the exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The target site of the exception.</value>
		string ExTargetSite
		{
			get;
		}

		/// <summary>
		/// Gets the stack trace of the exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The stack trace of the exception.</value>
		string ExStackTrace
		{
			get;
		}

		/// <summary>
		/// Gets the type of the inner exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The type of the inner exception.</value>
		string InnerExType
		{
			get;
		}

		/// <summary>
		/// Gets the message of the inner exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The message of the inner exception.</value>
		string InnerExMessage
		{
			get;
		}

		/// <summary>
		/// Gets the source of the inner exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The source of the inner exception.</value>
		string InnerExSource
		{
			get;
		}

		/// <summary>
		/// Gets the target site of the inner exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The target site of the inner exception.</value>
		string InnerExTargetSite
		{
			get;
		}

		/// <summary>
		/// Gets the stack trace of the inner exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
		/// </summary>
		/// <value>The stack trace of the inner exception.</value>
		string InnerExStackTrace
		{
			get;
		}

		/// <summary>
		/// Gets the data associated with the inner exception. Contains a value only when <see cref="EventType" />
		/// is <see cref="Business.EventType.Error" />. This is extracted from <see cref="System.Exception.Data"/>.
		/// Guaranteed to not be null.
		/// </summary>
		/// <value>The data associate with the inner exception.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> InnerExData
		{
			get;
		}

		/// <summary>
		/// Gets the URL of the page where the event occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The URL of the page where the event occurred.</value>
		string Url
		{
			get;
		}

		/// <summary>
		/// Gets the HTTP user agent where the event occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The HTTP user agent where the event occurred.</value>
		string HttpUserAgent
		{
			get;
		}

		/// <summary>
		/// Gets the form variables from the web page where the event occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The form variables from the web page where the event occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> FormVariables
		{
			get;
		}

		/// <summary>
		/// Gets the cookies from the web page where the event occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The cookies from the web page where the event occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> Cookies
		{
			get;
		}

		/// <summary>
		/// Gets the session variables from the web page where the event occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The session variables from the web page where the event occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> SessionVariables
		{
			get;
		}

		/// <summary>
		/// Gets the server variables from the web page where the event occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The server variables from the web page where the event occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> ServerVariables
		{
			get;
		}

		/// <summary>
		/// Formats the name of the specified <paramref name="item"/> into an HTML paragraph tag. Example: If 
		/// <paramref name="item"/> = <see cref="EventItem.ExStackTrace" />, the text "Stack Trace" is returned as the content of the tag.
		/// </summary>
		/// <param name="item">The enum value to be used as the content of the paragraph element. It is HTML encoded.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		string ToHtmlName(EventItem item);

		/// <summary>
		/// Formats the value of the specified <paramref name="item"/> into an HTML paragraph tag. Example: If 
		/// <paramref name="item"/> = <see cref="EventItem.ExStackTrace" />, the action stack trace data associated with the current event 
		/// is returned as the content of the tag. If present, line breaks (\r\n) are converted to &lt;br /&gt; tags.
		/// </summary>
		/// <param name="item">The enum value indicating the event item to be used as the content of the paragraph element.
		/// The text is HTML encoded.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		string ToHtmlValue(EventItem item);

		/// <summary>
		/// Generate HTML containing detailed information about the application event. Does not include the outer html
		/// and body tag. The HTML may contain references to CSS classes for formatting purposes, so be sure to include
		/// these CSS definitions in the containing web page.
		/// </summary>
		/// <returns>Returns an HTML formatted string containing detailed information about the exception.</returns>
		string ToHtml();

		/// <summary>
		/// Generate a complete HTML page containing detailed information about the application event. Includes the outer html
		/// and body tag, including definitions for the CSS classes that are referenced within the body. Does not depend
		/// on external style sheets or other resources. This method can be used to generate the body of an HTML e-mail.
		/// </summary>
		/// <returns>Returns an HTML formatted string containing detailed information about the exception.</returns>
		string ToHtmlPage();
	}
}
