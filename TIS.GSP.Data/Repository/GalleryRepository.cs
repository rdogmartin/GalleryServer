using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the Gallery table.
  /// </summary>
  public class GalleryRepository : Repository<GalleryDb, GalleryDto>
  {
    /// <summary>
    /// Gets the name of the connection string. Example: GalleryDb
    /// </summary>
    /// <value>The name of the connection string.</value>
    public string ConnectionStringName
    {
      get
      {
        var fullName = Context.GetType().ToString();

        return fullName.Substring(fullName.LastIndexOf('.') + 1);
      }
    }
  }
}