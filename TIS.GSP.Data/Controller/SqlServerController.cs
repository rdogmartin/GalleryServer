using System.Data;
using System.Data.SqlClient;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with a SQL Server database.
  /// </summary>
  public class SqlServerController: Interfaces.IDbController
  {
    #region Properties

    /// <summary>
    /// Gets or sets the connection string to the gallery data.
    /// </summary>
    /// <value>The connection string.</value>
    private string ConnectionString { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCeController"/> class.
    /// </summary>
    public SqlServerController()
    {
      ConnectionString = Utils.GetConnectionStringSettings().ConnectionString;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets an unopened connection to the database.
    /// </summary>
    /// <returns>An instance of <see cref="IDbConnection" />.</returns>
    public IDbConnection GetDbConnection()
    {
      return new SqlConnection(ConnectionString);
    }

    #endregion
  }
}