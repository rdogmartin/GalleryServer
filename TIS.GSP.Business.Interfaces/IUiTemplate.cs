namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a UI template within Gallery Server. A UI template is a pattern for a chunk of HTML to
	/// be rendered in the browser.
	/// </summary>
	public interface IUiTemplate
	{
		/// <summary>
		/// Gets or sets the ID for the UI template.
		/// </summary>
		/// <value>The UI templateID.</value>
		int UiTemplateId
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
		/// Gets or sets the type of the template.
		/// </summary>
		/// <value>The type of the template.</value>
		UiTemplateType TemplateType
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the ID of the gallery this template is associated with.
		/// </summary>
		/// <value>The ID of the gallery this template is associated with.</value>
		int GalleryId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the template.
		/// </summary>
		/// <value>The name.</value>
		string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a description of the template.
		/// </summary>
		/// <value>The description.</value>
		string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the IDs of the albums to which the template applies.
		/// </summary>
		/// <value>The IDs of the albums to which the template applies.</value>
		IIntegerCollection RootAlbumIds
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the template for rendering the HTML. String must be compatible with the
		/// jsRender syntax.
		/// </summary>
		/// <value>The template data.</value>
		string HtmlTemplate
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the template for rendering the JavaScript. String must be compatible with the
		/// jsRender syntax.
		/// </summary>
		/// <value>The javascript data.</value>
		string ScriptTemplate
		{
			get;
			set;
		}

		/// <summary>
		/// Creates a deep copy of this instance. It is not persisted to the data store.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		IUiTemplate Copy();

		/// <summary>
		/// Persist this UI template object to the data store.
		/// </summary>
		void Save();

		/// <summary>
		/// Permanently delete the current UI template from the data store. This action cannot be undone.
		/// </summary>
		void Delete();
	}
}
