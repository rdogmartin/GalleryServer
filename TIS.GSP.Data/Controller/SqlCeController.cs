using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Reflection;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with a SQL Server CE database.
  /// </summary>
  public class SqlCeController: Interfaces.IDbController
  {
    #region Fields

    private Assembly _assembly;
    private Type _sqlCeType;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the connection string to the gallery data.
    /// </summary>
    /// <value>The connection string.</value>
    private string ConnectionString { get; set; }

    /// <summary>
    /// Gets a reference to the SQL Server CE assembly (System.Data.SqlServerCe.dll). Hard coded to the 4.0.0.0 version.
    /// </summary>
    /// <value>An instance of <see cref="Assembly" />.</value>
    /// <remarks>Microsoft does not recommend loading weakly named assemblies, so that's we we are specifying the long form here. 
    /// See Best Practices for Assembly Loading (http://msdn.microsoft.com/en-us/library/dd153782.aspx) for more info.</remarks>
    private Assembly SqlCeAssembly
    {
      get { return _assembly ?? (_assembly = Assembly.Load("System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")); }
    }

    /// <summary>
    /// Gets a reference to the type System.Data.SqlServerCe.SqlCeEngine.
    /// </summary>
    /// <value>A reference to the type System.Data.SqlServerCe.SqlCeEngine.</value>
    private Type SqlCeEngineType
    {
      get { return _sqlCeType ?? (_sqlCeType = SqlCeAssembly.GetType("System.Data.SqlServerCe.SqlCeEngine")); }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCeController"/> class.
    /// </summary>
    public SqlCeController()
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
      return new SqlCeConnection(ConnectionString);
    }

    /// <summary>
    /// Reclaims wasted space in the database and recalculates identity column values.
    /// </summary>
    public void Compact()
    {
      using (var engine = new SqlCeEngine(ConnectionString))
      {
        engine.Compact(null);
      }

      // Same thing using Reflection:
      //object sqlCeEngine = null;
      //try
      //{
      //	sqlCeEngine = SqlCeEngineType.InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] { ConnectionString });

      //	SqlCeEngineType.InvokeMember("Compact", BindingFlags.InvokeMethod, null, sqlCeEngine, new object[] { null });
      //}
      //catch (TargetInvocationException ex)
      //{
      //	AppEventController.LogError(ex.InnerException ?? ex);
      //	throw;
      //}
      //finally
      //{
      //	if (sqlCeEngine != null)
      //		SqlCeEngineType.InvokeMember("Dispose", BindingFlags.InvokeMethod, null, sqlCeEngine, null);
      //}
    }

    /// <summary>
    /// Recalculates the checksums for each page in the database and compares the new checksums to the expected values. Also verifies
    /// that each index entry exists in the table and that each table entry exists in the index.
    /// </summary>
    /// <returns>
    /// 	<c>True</c> if there is no database corruption; otherwise, <c>false</c>.
    /// </returns>
    public bool Verify()
    {
      using (var engine = new SqlCeEngine(ConnectionString))
      {
        return engine.Verify(VerifyOption.Enhanced);
      }

      // Same thing using Reflection:
      //object sqlCeEngine = null;
      //try
      //{
      //	sqlCeEngine = SqlCeEngineType.InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] { ConnectionString });

      //	var verifyOptionEnumType = SqlCeAssembly.GetType("System.Data.SqlServerCe.VerifyOption");

      //	var verifyOptionEnhanced = verifyOptionEnumType.GetField("Enhanced").GetValue(verifyOptionEnumType);

      //	var rv = SqlCeEngineType.InvokeMember("Verify", BindingFlags.InvokeMethod, null, sqlCeEngine, new[] { verifyOptionEnhanced });

      //	return Convert.ToBoolean(rv);
      //}
      //catch (TargetInvocationException ex)
      //{
      //	AppEventController.LogError(ex.InnerException ?? ex);
      //	throw;
      //}
      //finally
      //{
      //	if (sqlCeEngine != null)
      //		SqlCeEngineType.InvokeMember("Dispose", BindingFlags.InvokeMethod, null, sqlCeEngine, null);
      //}
    }

    /// <summary>
    /// Repairs a corrupted database. Call this method when <see cref="Verify"/> returns false.
    /// </summary>
    public void Repair()
    {
      using (var engine = new SqlCeEngine(ConnectionString))
      {
        engine.Repair(null, RepairOption.RecoverAllPossibleRows);
      }

      // Same thing using Reflection:
      //object sqlCeEngine = null;
      //try
      //{
      //	sqlCeEngine = SqlCeEngineType.InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[] { ConnectionString });

      //	var repairOptionEnumType = SqlCeAssembly.GetType("System.Data.SqlServerCe.RepairOption");

      //	var repairOptionRecoverAll = repairOptionEnumType.GetField("RecoverAllPossibleRows").GetValue(repairOptionEnumType);

      //	SqlCeEngineType.InvokeMember("Repair", BindingFlags.InvokeMethod, null, sqlCeEngine, new[] { null, repairOptionRecoverAll });
      //}
      //catch (TargetInvocationException ex)
      //{
      //	AppEventController.LogError(ex.InnerException ?? ex);
      //	throw;
      //}
      //finally
      //{
      //	if (sqlCeEngine != null)
      //		SqlCeEngineType.InvokeMember("Dispose", BindingFlags.InvokeMethod, null, sqlCeEngine, null);
      //}
    }

    /// <summary>
    /// Deletes all records from the SQL CE Event table using the SQL 'DELETE FROM Event'. This is more efficient
    /// than using EF to clear the table and is also the only way to clear the table when this SQL CE bug
    /// (http://connect.microsoft.com/SQLServer/feedback/details/606152) prevents the code from retrieving EventDto
    /// records.
    /// </summary>
    public void ClearEventLog()
    {
      using (var cn = new SqlCeConnection(ConnectionString))
      {
        using (var cmd = cn.CreateCommand())
        {
          cmd.CommandText = String.Format("DELETE FROM {0}", Utils.GetSqlName("Event", Business.ProviderDataStore.SqlCe));
          cn.Open();
          cmd.ExecuteNonQuery();
        }
      }
    }

    #endregion

    #region Functions

    #endregion
  }
}