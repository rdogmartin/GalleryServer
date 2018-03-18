using System;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;

namespace GalleryServer.Business
{
	/// <summary>
	/// Contains data and behavior for managing a UI template.
	/// </summary>
	public class UiTemplate : IUiTemplate
	{
    /// <summary>
    /// Initializes a new instance of the <see cref="UiTemplate"/> class.
    /// </summary>
    public UiTemplate()
		{
			this.RootAlbumIds = new IntegerCollection();
		}

		/// <summary>
		/// Gets or sets the ID for the UI template.
		/// </summary>
		/// <value>The UI templateID.</value>
		public int UiTemplateId { get; set; }

		/// <summary>
		/// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		public bool IsNew
		{
			get { return (UiTemplateId == int.MinValue); }
		}

		/// <summary>
		/// Gets or sets the type of the template.
		/// </summary>
		/// <value>The type of the template.</value>
		public UiTemplateType TemplateType { get; set; }

		/// <summary>
		/// Gets or sets the ID of the gallery this template is associated with.
		/// </summary>
		/// <value>The ID of the gallery this template is associated with.</value>
		public int GalleryId { get; set; }

		/// <summary>
		/// Gets or sets the name of the template.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a description of the template.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the IDs of the albums to which the template applies.
		/// </summary>
		/// <value>The IDs of the albums to which the template applies.</value>
		public IIntegerCollection RootAlbumIds { get; set; }

		/// <summary>
		/// Gets or sets the template for rendering the HTML. String must be compatible with the
		/// jsRender syntax.
		/// </summary>
		/// <value>The template data.</value>
		public string HtmlTemplate { get; set; }

		/// <summary>
		/// Gets or sets the template for rendering the JavaScript. String must be compatible with the
		/// jsRender syntax.
		/// </summary>
		/// <value>The javascript data.</value>
		public string ScriptTemplate { get; set; }

		/// <summary>
		/// Creates a deep copy of this instance. It is not persisted to the data store.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		public IUiTemplate Copy()
		{
			IUiTemplate tmplCopy = new UiTemplate();

			tmplCopy.UiTemplateId = int.MinValue;
			tmplCopy.TemplateType = this.TemplateType;
			tmplCopy.GalleryId = this.GalleryId;
			tmplCopy.Name = this.Name;
			tmplCopy.Description = this.Description;
			tmplCopy.RootAlbumIds = new IntegerCollection(this.RootAlbumIds);
			tmplCopy.HtmlTemplate = this.HtmlTemplate;
			tmplCopy.ScriptTemplate = this.ScriptTemplate;

			return tmplCopy;
		}

		/// <summary>
		/// Persist this UI template object to the data store.
		/// </summary>
		public void Save()
		{
		  using (var repo = new UiTemplateRepository())
		  {
		    repo.Save(this);
		  }

      CacheController.RemoveCache(CacheItem.UiTemplates);
		}

		/// <summary>
		/// Permanently delete the current UI template from the data store. This action cannot be undone.
		/// </summary>
		public void Delete()
		{
		  using (var repo = new UiTemplateRepository())
		  {
        var uiTmplDto = repo.Find(UiTemplateId);

        if (uiTmplDto != null)
        {
          repo.Delete(uiTmplDto);
          repo.Save();
        }
		  }

      CacheController.RemoveCache(CacheItem.UiTemplates);
		}

		/// <summary>
		/// Gets a collection of all UI templates from the data store. Returns an empty collection if no
		/// items exist.
		/// </summary>
		/// <returns>Returns a collection of all UI templates from the data store.</returns>
		public static IUiTemplateCollection GetUiTemplates()
		{
			IUiTemplateCollection tmpl = new UiTemplateCollection();

		  using (var repo = new UiTemplateRepository())
		  {
        foreach (var jDto in repo.GetAll(j => j.TemplateAlbums))
		    {
		      IUiTemplate t = new UiTemplate
		                            {
		                              UiTemplateId = jDto.UiTemplateId,
		                              TemplateType = jDto.TemplateType,
																	GalleryId = jDto.FKGalleryId,
		                              Name = jDto.Name,
		                              Description = jDto.Description,
		                              HtmlTemplate = jDto.HtmlTemplate,
		                              ScriptTemplate = jDto.ScriptTemplate
		                            };

		      t.RootAlbumIds.AddRange(from r in jDto.TemplateAlbums select r.FKAlbumId);

		      tmpl.Add(t);
		    }
		  }

		  return tmpl;
		}
	}
}
