using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
  /// <summary>
  /// A collection of <see cref="IUiTemplate" /> objects.
  /// </summary>
  public class UiTemplateCollection : IUiTemplateCollection
  {
    private readonly ConcurrentBag<IUiTemplate> _items = new ConcurrentBag<IUiTemplate>();

    /// <overloads>
    /// Initializes a new instance of the <see cref="IUiTemplateCollection"/> class.
    /// </overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="IUiTemplateCollection"/> class.
    /// </summary>
    public UiTemplateCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IUiTemplateCollection" /> class with the
    /// contents of <paramref name="items" />.
    /// </summary>
    /// <param name="items">The items.</param>
    public UiTemplateCollection(IEnumerable<IUiTemplate> items)
    {
      AddRange(items);
    }

    /// <summary>
    /// Adds the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <exception cref="System.ArgumentNullException">Cannot add null to an existing GallerySettingsCollection. Items.Count =  + _items.Count</exception>
    public void Add(IUiTemplate item)
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item), "Cannot add null to an existing UiTemplateCollection. Items.Count = " + _items.Count);

      _items.Add(item);
    }

    /// <summary>
    /// Adds the UI templates to the current collection.
    /// </summary>
    /// <param name="uiTemplates">The UI templates to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uiTemplates"/> is null.</exception>
    public void AddRange(IEnumerable<IUiTemplate> uiTemplates)
    {
      if (uiTemplates == null)
        throw new ArgumentNullException(nameof(uiTemplates));

      foreach (var uiTemplate in uiTemplates)
      {
        _items.Add(uiTemplate);
      }
    }

    /// <summary>
    /// Gets the template with the specified <paramref name="templateType"/> that applies to <paramref name="album"/>.
    /// Guaranteed to not return null. If multiple templates apply, the closest one is returned. Example, if there
    /// are two templates - one for the root album and one for the requested album's parent, the latter is returned.
    /// If multiple templates are assigned to the same album, the first one is returned (as sorted alphabetically by name).
    /// </summary>
    /// <param name="templateType">Type of the template.</param>
    /// <param name="album">The album for which the relevant template is to be returned.</param>
    /// <returns>
    /// Returns an instance of <see cref="IUiTemplate"/>.
    /// </returns>
    /// <exception cref="BusinessException">Thrown when no relevant template is found.</exception>
    public IUiTemplate Get(UiTemplateType templateType, IAlbum album)
    {
      // Perf improvement: If there is only one template for the requested type, then return that one.
      var tmplItems = (from t in _items where t.TemplateType == templateType select t).ToList();
      if (tmplItems.Count == 1)
      {
        return tmplItems.First();
      }

      IGalleryObject curAlbum = album;

      if (album.IsVirtualAlbum)
      {
        // We want the template for the root album.
        curAlbum = Factory.LoadRootAlbumInstance(album.GalleryId);
      }

      IUiTemplate template;
      do
      {
        template = (from t in tmplItems where t.RootAlbumIds.Contains(curAlbum.Id) select t).FirstOrDefault();
        if (template != null)
          break;

        curAlbum = (IGalleryObject)curAlbum.Parent;
      } while (!(curAlbum is NullObjects.NullGalleryObject));

      if (template == null)
        throw new BusinessException(string.Format("Missing UI template: No template was found in the data store with type '{0}' that applies to album ID {1}. There must be at least one record for this type with the name 'Default' and assigned to the root album. Try recycling the IIS app pool - data validation during app startup may be able to fix this.", templateType, album.Id));
      
      return template;
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns><see cref="System.Collections.IEnumerator" />.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns><see cref="System.Collections.Generic.IEnumerator&lt;IUiTemplate&gt;" />.</returns>
    public IEnumerator<IUiTemplate> GetEnumerator()
    {
      return _items.GetEnumerator();
    }
  }
}
