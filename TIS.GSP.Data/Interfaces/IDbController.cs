using System.Data;

namespace GalleryServer.Data.Interfaces
{
  /// <summary>
  /// Defines methods and properties for interacting with a database.
  /// </summary>
  public interface IDbController
  {
    /// <summary>
    /// Gets an unopened connection to the database.
    /// </summary>
    /// <returns>An instance of <see cref="IDbConnection" />.</returns>
    IDbConnection GetDbConnection();
  }
}
