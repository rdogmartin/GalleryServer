using System;
using System.Data;
using System.Globalization;
using System.Linq;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the MediaTemplate table.
  /// </summary>
  public class MediaTemplateRepository : Repository<GalleryDb, MediaTemplateDto>
  {
    /// <summary>
    /// Fill the <paramref name="emptyCollection"/> with all the media templates in the current application. The return value is the same reference
    /// as the parameter.
    /// </summary>
    /// <param name="emptyCollection">An empty <see cref="IMediaTemplateCollection"/> object to populate with the list of media templates in the current
    /// application. This parameter is required because the library that implements this interface does not have
    /// the ability to directly instantiate any object that implements <see cref="IMediaTemplateCollection"/>.</param>
    /// <returns>
    /// Returns an <see cref="IMediaTemplateCollection"/> representing the media templates in the current application. The returned object is the
    /// same object in memory as the <paramref name="emptyCollection"/> parameter.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="emptyCollection" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="emptyCollection" /> is not empty.</exception>
    public IMediaTemplateCollection GetMediaTemplates(IMediaTemplateCollection emptyCollection)
    {
      if (emptyCollection == null)
        throw new ArgumentNullException(nameof(emptyCollection));

      if (emptyCollection.Count > 0)
      {
        throw new ArgumentException($"The emptyCollection parameter must be empty. Instead, it had {emptyCollection.Count} elements.", nameof(emptyCollection));
      }

      foreach (MediaTemplateDto btDto in Context.MediaTemplates.OrderBy(i => i.MimeType))
      {
        var bt = emptyCollection.CreateEmptyMediaTemplateInstance();
        bt.MediaTemplateId = btDto.MediaTemplateId;
        bt.MimeType = btDto.MimeType.Trim();
        bt.BrowserId = btDto.BrowserId.Trim();
        bt.HtmlTemplate = btDto.HtmlTemplate.Trim();
        bt.ScriptTemplate = btDto.ScriptTemplate.Trim();

        emptyCollection.Add(bt);
      }

      return emptyCollection;
    }

    /// <summary>
    /// Persist the media template to the data store.
    /// </summary>
    /// <param name="mediaTemplate">An instance of <see cref="IMediaTemplate"/> to persist to the data store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaTemplate" /> is null.</exception>
    public void Save(IMediaTemplate mediaTemplate)
    {
      if (mediaTemplate == null)
        throw new ArgumentNullException("mediaTemplate");

      PersistTemplateToDataStore(mediaTemplate);
    }

    private void PersistTemplateToDataStore(IMediaTemplate mediaTemplate)
    {
      if (mediaTemplate.IsNew)
      {
        var uiTmplDto = new MediaTemplateDto
        {
          MimeType = mediaTemplate.MimeType,
          BrowserId = mediaTemplate.BrowserId,
          HtmlTemplate = mediaTemplate.HtmlTemplate,
          ScriptTemplate = mediaTemplate.ScriptTemplate
        };

        Add(uiTmplDto);
        Save();

        // Assign newly created ID.
        mediaTemplate.MediaTemplateId = uiTmplDto.MediaTemplateId;
      }
      else
      {
        var mediaTmplDto = Find(mediaTemplate.MediaTemplateId);

        if (mediaTmplDto != null)
        {
          mediaTmplDto.MimeType = mediaTemplate.MimeType;
          mediaTmplDto.BrowserId = mediaTemplate.BrowserId;
          mediaTmplDto.HtmlTemplate = mediaTemplate.HtmlTemplate;
          mediaTmplDto.ScriptTemplate = mediaTemplate.ScriptTemplate;

          Save();
        }
        else
        {
          throw new DataException(String.Format(CultureInfo.CurrentCulture, "Cannot save media template: No existing template with Media Template ID {0} was found in the database.", mediaTemplate.MediaTemplateId));
        }
      }
    }
  }
}