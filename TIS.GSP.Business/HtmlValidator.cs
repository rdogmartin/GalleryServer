using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.RegularExpressions;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// Provides functionality for validating and cleaning HTML.
	/// </summary>
	public class HtmlValidator : IHtmlValidator
	{
		#region Private Fields

		private static readonly Regex _jsAttributeRegex = new Regex("javascript:", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		private static readonly Regex _scriptTag = new Regex("<script[\\w\\W]*?</script>", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
		private static readonly TagRegex _startTag = new TagRegex();
		private static readonly EndTagRegex _endTag = new EndTagRegex();
		private static readonly TextRegex _innerContentRegEx = new TextRegex();

		private readonly string _originalHtml;
		private string _dirtyHtml;
		private readonly StringBuilder _cleanHtml;
		private readonly string[] _allowedTags;
		private readonly string[] _allowedAttributes;
		private readonly bool _allowJavascript;
		private bool _invalidJavascriptDetected;
		private readonly List<string> _invalidHtmlTags = new List<string>();
		private readonly List<string> _invalidHtmlAttributes = new List<string>();
		private bool _validateHasExecuted;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the list of HTML tags found in the user-entered input that are not allowed. This property is set after
		/// the <see cref="Validate"/> method is invoked. Guaranteed to not be null.
		/// </summary>
		/// <value>
		/// The list of HTML tags found in the user-entered input that are not allowed.
		/// </value>
		public List<string> InvalidHtmlTags
		{
			get { return this._invalidHtmlTags; }
		}

		/// <summary>
		/// Gets the list of HTML attributes found in the user-entered input that are not allowed. This property is set after
		/// the <see cref="Validate"/> method is invoked. Guaranteed to not be null.
		/// </summary>
		/// <value>
		/// The list of HTML attributes found in the user-entered input that are not allowed.
		/// </value>
		public List<string> InvalidHtmlAttributes
		{
			get { return this._invalidHtmlAttributes; }
		}

		/// <summary>
		/// Gets a value indicating whether invalid javascript was detected in the HTML. This property is set after
		/// the <see cref="Validate"/> method is invoked. Returns <c>true</c> only when the configuration setting
		/// allowUserEnteredJavascript is <c>false</c> and either a script tag or the string "javascript:" is detected.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if invalid javascript is detected; otherwise, <c>false</c>.
		/// </value>
		public bool InvalidJavascriptDetected
		{
			get { return this._invalidJavascriptDetected; }
		}

		/// <summary>
		/// Gets a value indicating whether any invalid HTML tags, attributes, or javascript was found in the HTML.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if invalid HTML tags, attributes, or javascript was found; otherwise, <c>false</c>.
		/// </value>
		public bool IsValid
		{
			get
			{
				if (!this._validateHasExecuted)
					throw new InvalidOperationException("You must call the Validate method before accessing the IsValid property.");

				if (this._allowJavascript)
					return ((this.InvalidHtmlTags.Count == 0) && (this.InvalidHtmlAttributes.Count == 0));
				else
					return ((this.InvalidHtmlTags.Count == 0) && (this.InvalidHtmlAttributes.Count == 0) && !this._invalidJavascriptDetected);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="HtmlValidator"/> class.
		/// </summary>
		/// <param name="html">The text to be cleaned. May be null.</param>
		/// <param name="allowedHtmlTags">The HTML tags that are allowed in <paramref name="html"/>. May be null.</param>
		/// <param name="allowedHtmlAttributes">The HTML attributes that are allowed in <paramref name="html"/>. May be null.</param>
		/// <param name="allowJavascript">If set to <c>true</c> allow script tag and the string "javascript:". Note that
		/// if the script tag is not a member of <paramref name="allowedHtmlTags"/> it will be removed even if this
		/// parameter is <c>true</c>.</param>
		private HtmlValidator(string html, string[] allowedHtmlTags, string[] allowedHtmlAttributes, bool allowJavascript)
		{
			#region Validation

			if (html == null)
				html = String.Empty;

			if (allowedHtmlTags == null)
				allowedHtmlTags = new string[0];

			if (allowedHtmlAttributes == null)
				allowedHtmlAttributes = new string[0];

			#endregion

			this._originalHtml = html;
			this._allowedTags = allowedHtmlTags;
			this._allowedAttributes = allowedHtmlAttributes;
			this._allowJavascript = allowJavascript;

			this._cleanHtml = new StringBuilder(this._originalHtml.Length);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Evaluates the HTML for invalid tags, attributes, and javascript. After executing this method the <see cref="IsValid"/>
		/// property can be checked. If this property is <c>true</c>, the properties <see cref="InvalidHtmlTags"/>,
		/// <see cref="InvalidHtmlAttributes"/>, and <see cref="InvalidJavascriptDetected"/> can be inspected for details.
		/// </summary>
		public void Validate()
		{
			this._invalidHtmlTags.Clear();
			this._invalidHtmlAttributes.Clear();

			Clean();

			_validateHasExecuted = true;
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="HtmlValidator"/> class with the specified parameters.
		/// </summary>
		/// <param name="html">The text to be cleaned.</param>
		/// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
		/// <returns>Returns an instance of <see cref="HtmlValidator"/>.</returns>
		public static IHtmlValidator Create(string html, int galleryId)
		{
			IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

			return new HtmlValidator(html, gallerySetting.AllowedHtmlTags, gallerySetting.AllowedHtmlAttributes, gallerySetting.AllowUserEnteredJavascript);
		}

		/// <summary>
		/// Removes potentially dangerous HTML and Javascript in <paramref name="html"/>. If the configuration
		/// setting <see cref="IGallerySettings.AllowUserEnteredHtml" /> is true, then the input is cleaned so that all 
		/// HTML tags that are not in a predefined list are HTML-encoded and invalid HTML attributes are deleted. If 
		/// <see cref="IGallerySettings.AllowUserEnteredHtml" /> is false, then all HTML tags are deleted. If the setting 
		/// <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true, then script tags and the text "javascript:"
		/// is allowed. Note that if script is not in the list of valid HTML tags defined in <see cref="IGallerySettings.AllowedHtmlTags" />,
		/// it will be deleted even when <see cref="IGallerySettings.AllowUserEnteredJavascript" /> is true. When the setting 
		/// is false, all script tags and instances of the text "javascript:" are deleted.
		/// </summary>
		/// <param name="html">The string containing the HTML tags.</param>
		/// <param name="galleryId">The gallery ID. This is used to look up the appropriate configuration values for the gallery.</param>
		/// <returns>
		/// Returns a string with potentially dangerous HTML tags deleted.
		/// </returns>
		public static string Clean(string html, int galleryId)
		{
			IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

			if (gallerySetting.AllowUserEnteredHtml)
			{
				HtmlValidator scrubber = new HtmlValidator(html, gallerySetting.AllowedHtmlTags, gallerySetting.AllowedHtmlAttributes, gallerySetting.AllowUserEnteredJavascript);
				return scrubber.Clean();
			}
			else
			{
				// HTML not allowed. Pass in empty variables for the valid tags and attributes.
				HtmlValidator scrubber = new HtmlValidator(html, null, null, gallerySetting.AllowUserEnteredJavascript);
				return scrubber.Clean();
			}
		}

		/// <summary>
		/// Remove all HTML tags and javascript from the specified string. If <paramref name="escapeQuotes"/> is <c>true</c>, then all 
		/// apostrophes and quotation marks are replaced with &quot; and &apos; so that the string can be specified in HTML 
		/// attributes such as title tags.
		/// </summary>
		/// <param name="html">The string containing HTML tags to remove.</param>
		/// <param name="escapeQuotes">When true, all apostrophes and quotation marks are replaced with &quot; and &apos;.</param>
		/// <returns>Returns a string with all HTML tags removed, including the brackets.</returns>
		public static string RemoveHtml(string html, bool escapeQuotes)
		{
			HtmlValidator scrubber = new HtmlValidator(html, null, null, false);
			string cleanHtml = scrubber.Clean();

			if (escapeQuotes)
			{
				cleanHtml = cleanHtml.Replace("\"", "&quot;");
				cleanHtml = cleanHtml.Replace("'", "&apos;");
			}

			return cleanHtml;
		}

		#endregion

		#region Private Functions

		/// <summary>
		/// Remove invalid HTML tags, attributes, and javascript from the HTML.
		/// </summary>
		/// <returns>Returns a string consisting of clean HTML.</returns>
		private string Clean()
		{
			int dirtyHtmlIndex = 0;
			bool foundFirstTag = false;

			if (this._allowJavascript)
				this._dirtyHtml = this._originalHtml;
			else
				this._dirtyHtml = DeleteScriptTags(this._originalHtml);

			while (dirtyHtmlIndex < this._dirtyHtml.Length)
			{
				// Look for start tag and process if we find it.
				Match tagMatch = _startTag.Match(this._dirtyHtml, dirtyHtmlIndex);
				if (tagMatch.Success)
				{
					foundFirstTag = true;

					// Increment our index the length of the tag.
					dirtyHtmlIndex = tagMatch.Index + tagMatch.Length;

					// Process the start tag. The method might increment our index if there is content after this tag.
					dirtyHtmlIndex = this.ProcessStartTag(tagMatch, dirtyHtmlIndex);

					continue;
				}

				// Look for end tag and process if we find it.
				tagMatch = _endTag.Match(this._dirtyHtml, dirtyHtmlIndex);
				if (tagMatch.Success)
				{
					// Increment our index the length of the tag.
					dirtyHtmlIndex = tagMatch.Index + tagMatch.Length;

					// Process the end tag. The method might increment our index if their is content after this tag.
					dirtyHtmlIndex = this.ProcessEndTag(tagMatch, dirtyHtmlIndex);

					continue;
				}

				if (!foundFirstTag)
				{
					// We haven't encountered an HTML tag yet, so append the current character.
					this._cleanHtml.Append(this._dirtyHtml.Substring(dirtyHtmlIndex, 1));
				}

				dirtyHtmlIndex++;
			}

			return this._cleanHtml.ToString();
		}

		/// <summary>
		/// Scrub the specified <paramref name="tagMatch"/> of invalid HTML tags, attributes, and javascript. The tag will be
		/// either a starting tag (e.g. &lt;p&gt;) or a single tag (e.g. &lt;br /&gt;).
		/// </summary>
		/// <param name="tagMatch">A <see cref="Match"/> resulting from passing a string containing HTML to an instance of
		/// <see cref="TagRegex"/>.</param>
		/// <param name="index">The position within the original HTML where the <paramref name="tagMatch"/> ends.</param>
		/// <returns>The position within the original HTML after the <paramref name="tagMatch"/> and any text that occurs
		/// after it. It can be used by the calling code for looking for the next match.</returns>
		private int ProcessStartTag(Match tagMatch, int index)
		{
			string tagName = tagMatch.Groups["tagname"].Value.ToLowerInvariant();

			if (Array.IndexOf<string>(this._allowedTags, tagName) >= 0)
			{
				// This tag is valid. Clean the attributes and append to our output.
				_cleanHtml.Append(RemoveInvalidAttributes(tagMatch, this._allowedAttributes, this._invalidHtmlAttributes));
			}
			else
			{
				// Invalid tag. Call RemoveInvalidAttributes so that we can get our list of invalid attributes updated.
				RemoveInvalidAttributes(tagMatch, this._allowedAttributes, this._invalidHtmlAttributes);

				// Add to list of invalid tags if not already there
				if (!this._invalidHtmlTags.Contains(tagName))
					this._invalidHtmlTags.Add(tagName);
			}

			// Add any text between this start tag and the beginning of the next tag.
			Match contentMatch = _innerContentRegEx.Match(_dirtyHtml, index);
			if (contentMatch.Success)
			{
				_cleanHtml.Append(contentMatch.Value);

				// Increment our index so that when we search for the next tag we do it after the content we just found.
				index = contentMatch.Index + contentMatch.Length;
			}

			return index;
		}

		/// <summary>
		/// Scrub the specified <paramref name="tagMatch"/> of invalid HTML tags, attributes, and javascript. The tag will be
		/// an ending tag (e.g. &lt;/p&gt;).
		/// </summary>
		/// <param name="tagMatch">A <see cref="Match"/> resulting from passing a string containing HTML to an instance of
		/// <see cref="TagRegex"/>.</param>
		/// <param name="index">The position within the original HTML where the <paramref name="tagMatch"/> ends.</param>
		/// <returns>The position within the original HTML after the <paramref name="tagMatch"/> and any text that occurs
		/// after it. It can be used by the calling code for looking for the next match.</returns>
		private int ProcessEndTag(Match tagMatch, int index)
		{
			if (Array.IndexOf<string>(this._allowedTags, tagMatch.Groups["tagname"].Value.ToLowerInvariant()) >= 0)
			{
				_cleanHtml.Append(tagMatch.Value);
			}

			// Add any text between this end tag and the beginning of the next tag.
			Match contentMatch = _innerContentRegEx.Match(_dirtyHtml, index);
			if (contentMatch.Success)
			{
				_cleanHtml.Append(contentMatch.Value);

				// Increment our index so that when we search for the next tag we do it after the content we just found.
				index = contentMatch.Index + contentMatch.Length;
			}

			return index;
		}

		/// <summary>
		/// Removes HTML attributes and their values from the HTML string in <paramref name="tagMatch"/> if they do not exist in 
		/// <paramref name="allowedAttributes"/>. Any invalid attributes are added to <paramref name="invalidHtmlAttributes"/>.
		/// </summary>
		/// <param name="tagMatch">A <see cref="Match"/> resulting from passing a string containing HTML to an instance of
		/// <see cref="TagRegex"/>.</param>
		/// <param name="allowedAttributes">The HTML attributes that are allowed to be present in the HTML string in 
		/// <paramref name="tagMatch"/>.</param>
		/// <param name="invalidHtmlAttributes">A running list of invalid HTML attributes. Any attributes found to be invalid
		/// in <paramref name="tagMatch"/> are added to this list.</param>
		/// <returns>Returns the HTML string stored in <paramref name="tagMatch"/> with invalid attributes and their values removed.</returns>
		private static string RemoveInvalidAttributes(Match tagMatch, string[] allowedAttributes, ICollection<string> invalidHtmlAttributes)
		{
			string cleanTag = String.Concat("<", tagMatch.Groups["tagname"].Value.ToLowerInvariant());

			Group grpAttrName = tagMatch.Groups["attrname"];
			Group grpAttrVal = tagMatch.Groups["attrval"];

			CaptureCollection attrNameCaptures = grpAttrName.Captures;
			CaptureCollection attrValCaptures = grpAttrVal.Captures;

			if (attrNameCaptures.Count == attrValCaptures.Count)
			{
				for (int attValuePairIterator = 0; attValuePairIterator < attrNameCaptures.Count; attValuePairIterator++)
				{
					if (Array.IndexOf<string>(allowedAttributes, attrNameCaptures[attValuePairIterator].Value.ToLowerInvariant()) >= 0)
					{
						// Valid attribute. Append attribute/value string to our clean tag.
						cleanTag += GetAttValuePair(tagMatch, attValuePairIterator);
					}
					else
					{
						if (!invalidHtmlAttributes.Contains(attrNameCaptures[attValuePairIterator].Value.ToLowerInvariant()))
							invalidHtmlAttributes.Add(attrNameCaptures[attValuePairIterator].Value.ToLowerInvariant());
					}
				}
			}

			cleanTag += ">";

			return cleanTag;
		}

    /// <summary>
    /// Gets the attribute="value" string in the <paramref name="tagMatch" /> at the specified <paramref name="index" />. If the original
    /// value was not surrounded by quotation marks or apostrophes, add them, selectively choosing the most appropriate one so
    /// as not to interfere with the presence of one or the other in the attribute value. For example, if the attribute value
    /// contains an apostrophe, surround it with quotation marks. Includes a leading space. (Example: " class='boldtext'")
    /// </summary>
    /// <param name="tagMatch">A <see cref="Match"/> resulting from passing a string containing HTML to an instance of
    /// <see cref="TagRegex"/>.</param>
    /// <param name="index">The index of the attribute within the <see cref="CaptureCollection"/> of <paramref name="tagMatch"/>.</param>
    /// <returns>Returns an attribute="value" string with a leading space (Example: " class='boldtext'").</returns>
    private static string GetAttValuePair(Match tagMatch, int index)
		{
			char[] delimiters = new char[] { '\'', '"' };

			Capture attrValCapture = tagMatch.Groups["attrval"].Captures[index];
			int indexOfAttributeStart = attrValCapture.Index - tagMatch.Index;

			// Get the characters at the start and end of the attribute value. Typically this is a quote or apostrophe.
			char beginAttValue = tagMatch.Value.Substring(indexOfAttributeStart - 1, 1)[0];
			char endAttValue = tagMatch.Value.Substring(indexOfAttributeStart + attrValCapture.Length, 1)[0];

			// If one or both characters are not a quote or apostrophe, specify one. If the attribute value contains one, 
			// then choose the other so as not to interfere.
			if (Array.IndexOf(delimiters, beginAttValue) < 0)
				beginAttValue = (attrValCapture.Value.Contains("'") ? '"' : '\'');

			if (Array.IndexOf(delimiters, endAttValue) < 0)
				endAttValue = (attrValCapture.Value.Contains("'") ? '"' : '\'');

			return String.Concat(" ", tagMatch.Groups["attrname"].Captures[index].Value, "=", beginAttValue, attrValCapture.Value, endAttValue);
		}

		///// <summary>
		///// HTML-encodes a string and returns the encoded string.
		///// </summary>
		///// <param name="text">The text string to encode. </param>
		///// <returns>The HTML-encoded text.</returns>
		//private static string HtmlEncode(string text)
		//{
		//  if (text == null)
		//    return null;

		//  StringBuilder sb = new StringBuilder(text.Length);

		//  int len = text.Length;
		//  for (int i = 0; i < len; i++)
		//  {
		//    switch (text[i])
		//    {
		//      case '<':
		//        sb.Append("&lt;");
		//        break;
		//      case '>':
		//        sb.Append("&gt;");
		//        break;
		//      case '"':
		//        sb.Append("&quot;");
		//        break;
		//      case '&':
		//        sb.Append("&amp;");
		//        break;
		//      default:
		//        if (text[i] > 159)
		//        {
		//          // decimal numeric entity
		//          sb.Append("&#");
		//          sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
		//          sb.Append(";");
		//        }
		//        else
		//          sb.Append(text[i]);
		//        break;
		//    }
		//  }
		//  return sb.ToString();
		//}

		/// <summary>
		/// Delete any script tag and its content. Delete any instances of the string "javascript:". If javascript
		/// is detected, the member variable _javascriptDetected is set to <c>true</c>.
		/// </summary>
		/// <param name="html">The string to be cleaned of script tags.</param>
		/// <returns>
		/// Returns <paramref name="html"/> cleaned of script tags.
		/// </returns>
		private string DeleteScriptTags(string html)
		{
			int originalLength = html.Length;

			// Delete any <script> tags
			html = _scriptTag.Replace(html, String.Empty);

			// Delete any instances of the string "javascript:"
			html = _jsAttributeRegex.Replace(html, String.Empty);

			if (html.Length != originalLength)
			{
				this._invalidJavascriptDetected = true;
			}

			return html;
		}

		#endregion
	}
}
