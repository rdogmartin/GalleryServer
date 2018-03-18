namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a media template within Gallery Server. A media template describes the HTML and javascript that is used
	/// to render a media object in a particular browser.
	/// </summary>
	public interface IMediaTemplate
	{
		/// <summary>
		/// Gets or sets the value that uniquely identifies this media template.
		/// </summary>
		/// <value>The media template ID.</value>
		int MediaTemplateId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		bool IsNew { get; }

		/// <summary>
		/// Gets or sets the identifier of a browser as specified in the .Net Framework's browser definition file. Every MIME type must
		/// have one media template where <see cref="BrowserId" /> = "default". Additional <see cref="IMediaTemplate" /> objects
		/// may represent a more specific browser or browser family, such as Internet Explorer (<see cref="BrowserId" /> = "ie").
		/// </summary>
		/// <value>The identifier of a browser as specified in the .Net Framework's browser definition file.</value>
		string BrowserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the HTML template to use to render a media object in a web browser.
		/// </summary>
		/// <value>The HTML template to use to render a media object in a web browser.</value>
		string HtmlTemplate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the javascript template to use when rendering a media object in a web browser.
		/// </summary>
		/// <value>The javascript template to use when rendering a media object in a web browser.</value>
		string ScriptTemplate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the MIME type this media template applies to. Examples: image/*, video/*, video/quicktime, application/pdf.
		/// Notice that an asterisk (*) can be used to represent all subtypes within a type (e.g. "video/*" matches all videos).
		/// </summary>
		/// <value>The MIME type this media template applies to.</value>
		string MimeType
		{
			get;
			set;
		}

		/// <summary>
		/// Creates a deep copy of this instance.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		IMediaTemplate Copy();

		/// <summary>
		/// Persist this template object to the data store.
		/// </summary>
		void Save();

		/// <summary>
		/// Permanently delete the current template from the data store. This action cannot be undone.
		/// </summary>
		void Delete();
	}
}
