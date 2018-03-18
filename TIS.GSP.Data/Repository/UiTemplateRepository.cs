using System;
using System.Data;
using System.Globalization;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the UiTemplate table.
  /// </summary>
  public class UiTemplateRepository : Repository<GalleryDb, UiTemplateDto>
  {
    /// <summary>
    /// Persist the UI template to the data store.
    /// </summary>
    /// <param name="uiTemplate">An instance of <see cref="IUiTemplate"/> to persist to the data store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="uiTemplate" /> is null.</exception>
    public void Save(IUiTemplate uiTemplate)
    {
      if (uiTemplate == null)
        throw new ArgumentNullException("uiTemplate");

      PersistTemplateToDataStore(uiTemplate);

      var repo = new UiTemplateAlbumRepository(Context); // Don't put in using construct because we don't want our Context disposed
      repo.Save(uiTemplate.UiTemplateId, uiTemplate.RootAlbumIds);
    }

    private void PersistTemplateToDataStore(IUiTemplate uiTemplate)
    {
      if (uiTemplate.IsNew)
      {
        var uiTmplDto = new UiTemplateDto
                              {
                                TemplateType = uiTemplate.TemplateType,
                                FKGalleryId = uiTemplate.GalleryId,
                                Name = uiTemplate.Name,
                                Description = uiTemplate.Description,
                                HtmlTemplate = uiTemplate.HtmlTemplate,
                                ScriptTemplate = uiTemplate.ScriptTemplate
                              };

        Add(uiTmplDto);
        Save();

        // Assign newly created ID.
        uiTemplate.UiTemplateId = uiTmplDto.UiTemplateId;
      }
      else
      {
        var uiTmplDto = Find(uiTemplate.UiTemplateId);

        if (uiTmplDto != null)
        {
          uiTmplDto.TemplateType = uiTemplate.TemplateType;
          uiTmplDto.FKGalleryId = uiTemplate.GalleryId;
          uiTmplDto.Name = uiTemplate.Name;
          uiTmplDto.Description = uiTemplate.Description;
          uiTmplDto.HtmlTemplate = uiTemplate.HtmlTemplate;
          uiTmplDto.ScriptTemplate = uiTemplate.ScriptTemplate;

          Save();
        }
        else
        {
          throw new DataException(String.Format(CultureInfo.CurrentCulture, "Cannot save UI template: No existing template with UI Template ID {0} was found in the database.", uiTemplate.UiTemplateId));
        }
      }
    }
  }
}