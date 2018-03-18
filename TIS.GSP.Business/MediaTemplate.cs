using System.Diagnostics;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a media template within Gallery Server. A media template describes the HTML and javascript that is used
  /// to render a media object in a particular browser.
  /// </summary>
  [DebuggerDisplay("{MimeType}, Browser ID = {BrowserId})")]
  public class MediaTemplate : IMediaTemplate
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTemplate"/> class.
    /// </summary>
    internal MediaTemplate()
    {
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the value that uniquely identifies this media template.
    /// </summary>
    /// <value>The media template ID.</value>
    public int MediaTemplateId { get; set; }

    /// <summary>
    /// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
    /// </summary>
    /// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
    public bool IsNew
    {
      get { return (MediaTemplateId == int.MinValue); }
    }

    /// <summary>
    /// Gets or sets the identifier of a browser as specified in the .Net Framework's browser definition file. Every MIME type must
    /// have one media template where <see cref="IMediaTemplate.BrowserId" /> = "default". Additional <see cref="IMediaTemplate" /> objects
    /// may represent a more specific browser or browser family, such as Internet Explorer (<see cref="IMediaTemplate.BrowserId" /> = "ie").
    /// </summary>
    /// <value>The identifier of a browser as specified in the .Net Framework's browser definition file.</value>
    public string BrowserId { get; set; }

    /// <summary>
    /// Gets or sets the HTML template to use to render a media object in a web browser.
    /// </summary>
    /// <value>The HTML template to use to render a media object in a web browser.</value>
    public string HtmlTemplate { get; set; }

    /// <summary>
    /// Gets or sets the javascript template to use when rendering a media object in a web browser.
    /// </summary>
    /// <value>The javascript template to use when rendering a media object in a web browser.</value>
    public string ScriptTemplate { get; set; }

    /// <summary>
    /// Gets or sets the MIME type this media template applies to. Examples: image/*, video/*, video/quicktime, application/pdf.
    /// Notice that an asterisk (*) can be used to represent all subtypes within a type (e.g. "video/*" matches all videos).
    /// </summary>
    /// <value>The MIME type this media template applies to.</value>
    public string MimeType { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates a deep copy of this instance.
    /// </summary>
    /// <returns>Returns a deep copy of this instance.</returns>
    public IMediaTemplate Copy()
    {
      IMediaTemplate bp = new MediaTemplate();

      bp.MediaTemplateId = int.MinValue;
      bp.MimeType = this.MimeType;
      bp.BrowserId = this.BrowserId;
      bp.HtmlTemplate = this.HtmlTemplate;
      bp.ScriptTemplate = this.ScriptTemplate;

      return bp;
    }

    /// <summary>
    /// Persist this template object to the data store.
    /// </summary>
    public void Save()
    {
      using (var repo = new MediaTemplateRepository())
      {
        repo.Save(this);
      }

      CacheController.RemoveMediaTemplatesFromCache();
    }

    /// <summary>
    /// Permanently delete the current template from the data store. This action cannot be undone.
    /// </summary>
    public void Delete()
    {
      using (var repo = new MediaTemplateRepository())
      {
        var tmplDto = repo.Find(MediaTemplateId);

        if (tmplDto != null)
        {
          repo.Delete(tmplDto);
          repo.Save();
        }
      }

      CacheController.RemoveMediaTemplatesFromCache();
    }

    #endregion
  }
}
