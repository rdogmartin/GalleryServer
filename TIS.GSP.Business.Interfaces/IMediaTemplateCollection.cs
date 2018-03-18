using System;
using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// A collection of <see cref="IMediaTemplate" /> objects.
  /// </summary>
  public interface IMediaTemplateCollection : IEnumerable<IMediaTemplate>
  {
    /// <summary>
    /// Adds the specified media template.
    /// </summary>
    /// <param name="item">The media template to add.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
    void Add(IMediaTemplate item);

    /// <summary>
    /// Adds the media templates to the current collection.
    /// </summary>
    /// <param name="mediaTemplates">The media templates to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaTemplates" /> is null.</exception>
    void AddRange(System.Collections.Generic.IEnumerable<IMediaTemplate> mediaTemplates);

    /// <overloads>
    /// Finds the matching media template in the collection, or null if no match is found.
    /// </overloads>
    /// <summary>
    /// Gets the most specific <see cref="IMediaTemplate" /> item that matches one of the <paramref name="browserIds" />, or 
    /// null if no match is found. This method loops through each of the browser IDs in <paramref name="browserIds" />, 
    /// starting with the most specific item, and looks for a match in the current collection.
    /// </summary>
    /// <param name="browserIds">A <see cref="System.Array"/> of browser ids for the current browser. This is a list of strings,
    /// ordered from most general to most specific, that represent the various categories of browsers the current
    /// browser belongs to. This is typically populated by calling ToArray() on the Request.Browser.Browsers property.
    /// </param>
    /// <returns>The <see cref="IMediaTemplate" /> that most specifically matches one of the <paramref name="browserIds" />; 
    /// otherwise, a null reference.</returns>
    /// <example>During a request where the client is Firefox, the Request.Browser.Browsers property returns an ArrayList with these 
    /// five items: default, mozilla, gecko, mozillarv, and mozillafirefox. This method starts with the most specific item 
    /// (mozillafirefox) and looks in the current collection for an item with this browser ID. If a match is found, that item 
    /// is returned. If no match is found, the next item (mozillarv) is used as the search parameter.  This continues until a match 
    /// is found. If no match is found, a null is returned.
    /// </example>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="browserIds" /> does not have any items.</exception>
    IMediaTemplate Find(Array browserIds);

    /// <summary>
    /// Gets the <see cref="IMediaTemplate" /> item that matches the <paramref name="browserId" />, or null if no match is found.
    /// </summary>
    /// <param name="browserId">The identifier of a browser as specified in the .Net Framework's browser definition file. Typically
    /// this parameter is populated from one of the entries in the Browsers property of the HttpContext.Current.Request.Browser object.</param>
    /// <returns>Returns the <see cref="IMediaTemplate" /> item that matches the <paramref name="browserId" />, or null if no match is found.</returns>
    IMediaTemplate Find(string browserId);

    /// <summary>
    /// Gets one or more media templates in the collection that match the <paramref name="mimeType" />. If no item is found, then
    /// the MIME type that matches the major portion is returned. For example, if the collection does not contain a specific item 
    /// for "image/jpeg", then the MIME type for "image/*" is returned. This method returns multiple items when more than one 
    /// template has been specified for browsers. That is, all returned items will have the same value for 
    /// <see cref="IMediaTemplate.MimeType" /> but the <see cref="IMediaTemplate.BrowserId" /> property will vary. At least one
    /// item in the collection will have the <see cref="IMediaTemplate.BrowserId" /> property set to "default". Guaranteed to not
    /// return null. If no items are found (which shouldn't happen), an empty collection is returned.
    /// </summary>
    /// <param name="mimeType">The MIME type for which to retrieve matching media templates.</param>
    /// <returns>Returns a <see cref="IMediaTemplateCollection" /> containing media templates that match the 
    /// <paramref name="mimeType" />. </returns>
    IMediaTemplateCollection Find(IMimeType mimeType);

    /// <summary>
    /// Creates a deep copy of this instance.
    /// </summary>
    /// <returns>Returns a deep copy of this instance.</returns>
    IMediaTemplateCollection Copy();

    /// <summary>
    /// Creates a new, empty instance of an <see cref="IMediaTemplate" /> object. This method can be used by code that only has a 
    /// reference to the interface layer and therefore cannot create a new instance of an object on its own.
    /// </summary>
    /// <returns>Returns a new, empty instance of an <see cref="IMediaTemplate" /> object.</returns>
    IMediaTemplate CreateEmptyMediaTemplateInstance();

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    /// <value>The count.</value>
    int Count { get; }
  }
}
