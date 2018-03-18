using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IUiTemplate" /> objects.
	/// </summary>
	public interface IUiTemplateCollection : IEnumerable<IUiTemplate>
	{
		/// <summary>
		/// Adds the specified UI template.
		/// </summary>
		/// <param name="item">The UI template to add.</param>
		void Add(IUiTemplate item);

		/// <summary>
		/// Adds the UI templates to the current collection.
		/// </summary>
		/// <param name="uiTemplates">The UI templates to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="uiTemplates" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<IUiTemplate> uiTemplates);

		/// <summary>
		/// Gets the template with the specified <paramref name="templateType" /> that applies to <paramref name="album" />.
		/// Guaranteed to not return null. If multiple templates apply, the closest one is returned. Example, if there 
		/// are two templates - one for the root album and one for the requested album's parent, the latter is returned. 
		/// If multiple templates are assigned to the same album, the first one is returned (as sorted alphebetically by name).
		/// </summary>
		/// <param name="templateType">Type of the template.</param>
		/// <param name="album">The album for which the relevant template is to be returned.</param>
		/// <returns>Returns an instance of <see cref="IUiTemplate" />.</returns>
		IUiTemplate Get(UiTemplateType templateType, IAlbum album);
	}
}
