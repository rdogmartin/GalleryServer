using System.Collections.Generic;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents an object that stores information about a backup file. Backup files in Gallery Server are XML-based and
	/// contain the data that is stored in the database. They do not contain the actual media files nor do they contain 
	/// configuration data from the web.config file.
	/// </summary>
	public interface IBackupFile
	{
		/// <summary>
		/// Gets or sets the full file path to the backup file. Example: "D:\mybackups\GalleryServerBackup_2008-06-22_141336.xml".
		/// </summary>
		/// <value>The full file path to the backup file.</value>
		string FilePath { get; set; }

		/// <summary>
		/// Gets or sets the schema version for the data that is in the backup file. The schema version typically matches the
		/// release version of Gallery Server. However, if a new release does not contain any changes to the database structure,
		/// the schema version remains the same as the previous version. The version is stored in the database within the
		/// AppSetting table.
		/// </summary>
		/// <value>The schema version for the data that is in the backup file.</value>
		GalleryDataSchemaVersion SchemaVersion { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the backup file conforms to the expected XML schema and whether it can be imported
		/// by the current version of Gallery Server.
		/// </summary>
		/// <value><c>true</c> if the backup file is valid; otherwise, <c>false</c>.</value>
		bool IsValid { get; set; }

		/// <summary>
		/// Gets a dictionary containing the list of tables in the backup file and the corresponding number of records in each table.
		/// </summary>
		/// <value>The dictionary containing the list of tables in the backup file and the corresponding number of records in each table.</value>
		Dictionary<string, int> DataTableRecordCount { get; }

		/// <summary>
		/// Gets or sets a value indicating whether membership data should be included during import or export operations.
		/// </summary>
		/// <value><c>true</c> if membership data is to be included; otherwise, <c>false</c>.</value>
		bool IncludeMembershipData { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether gallery data should be included during import or export operations.
		/// </summary>
		/// <value><c>true</c> if gallery data is to be included; otherwise, <c>false</c>.</value>
		bool IncludeGalleryData { get; set; }

		/// <summary>
		/// Gets the data store currently being used for gallery data.
		/// </summary>
		/// <value>An instance of <see cref="ProviderDataStore" />.</value>
		ProviderDataStore GalleryDataStore { get; }

		/// <summary>
		/// Gets the connection string for the data store containing the gallery data.
		/// </summary>
		/// <value>The connection string.</value>
		string ConnectionString { get; }

		/// <summary>
		/// Gets a collection of names of membership tables whose data is to be imported or exported into or from a data store.
		/// They are returned in the order in which they must be populated during a restoration. Reverse the collection if 
		/// you wish to delete the table contents.
		/// </summary>
		/// <value>The membership table names.</value>
		string[] MembershipTables { get; }

		/// <summary>
		/// Gets a collection of names of gallery tables whose data is to be imported or exported into or from a data store.
		/// They are returned in the order in which they must be populated during a restoration. Reverse the collection if 
		/// you wish to delete the table contents.
		/// </summary>
		/// <value>The gallery table names.</value>
		string[] GalleryTableNames { get; }

		/// <summary>
		/// Validates that the current backup file is valid and populates the remaining properties with information about the file.
		/// </summary>
		void Validate();

		/// <summary>
		/// Exports the Gallery Server data in the current database to an XML-formatted string. Does not export the actual media files;
		/// they must be copied manually with a utility such as Windows Explorer. This method does not make any changes to the database tables
		/// or the files in the media objects directory.
		/// </summary>
		/// <returns>
		/// Returns an XML-formatted string containing the gallery data.
		/// </returns>
		string Create();

		/// <summary>
		/// Imports the Gallery Server data into the current database, overwriting any existing data. Does not import the actual media
		/// files; they must be imported manually with a utility such as Windows Explorer. This method makes changes only to the database tables;
		/// no files in the media objects directory are affected. If both <see cref="IncludeMembershipData" /> and <see cref="IncludeGalleryData"/>
		/// are false, then no action is taken.
		/// </summary>
		void Import();
	}
}
