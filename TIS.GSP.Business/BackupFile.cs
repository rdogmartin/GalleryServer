using System;
using System.Collections.Generic;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;

namespace GalleryServer.Business
{
	/// <summary>
	/// Represents an object that stores information about a backup file. Backup files in Gallery Server are XML-based and
	/// contain the data that is stored in the database. They do not contain the actual media files nor do they contain 
	/// configuration data from the web.config file.
	/// </summary>
	public class BackupFile : IBackupFile
	{
		#region Private Fields

		private readonly Dictionary<string, int> _dataTableRecordCount = new Dictionary<string, int>();

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the full file path to the backup file. Example: "D:\mybackups\GalleryServerBackup_2008-06-22_141336.xml".
		/// </summary>
		/// <value>The full file path to the backup file.</value>
		public string FilePath { get; set; }

		/// <summary>
		/// Gets or sets the schema version for the data that is in the backup file. The schema version typically matches the
		/// release version of Gallery Server. However, if a new release does not contain any changes to the database structure,
		/// the schema version remains the same as the previous version. The version is stored in the database within the
		/// AppSetting table.
		/// </summary>
		/// <value>The schema version for the data that is in the backup file.</value>
		public GalleryDataSchemaVersion SchemaVersion { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the backup file conforms to the expected XML schema and whether it can be imported
		/// by the current version of Gallery Server.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the backup file is valid; otherwise, <c>false</c>.
		/// </value>
		public bool IsValid { get; set; }

		/// <summary>
		/// Gets a dictionary containing the list of tables in the backup file and the corresponding number of records in each table.
		/// </summary>
		/// <value>
		/// The dictionary containing the list of tables in the backup file and the corresponding number of records in each table.
		/// </value>
		public Dictionary<string, int> DataTableRecordCount
		{
			get { return _dataTableRecordCount; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether membership data should be included during import or export operations.
		/// </summary>
		/// <value><c>true</c> if membership data is to be included; otherwise, <c>false</c>.</value>
		public bool IncludeMembershipData { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether gallery data should be included during import or export operations.
		/// </summary>
		/// <value><c>true</c> if gallery data is to be included; otherwise, <c>false</c>.</value>
		public bool IncludeGalleryData { get; set; }

		/// <summary>
		/// Gets the data store currently being used for gallery data.
		/// </summary>
		/// <value>An instance of <see cref="ProviderDataStore" />.</value>
		public ProviderDataStore GalleryDataStore
		{
			get
			{
				return AppSetting.Instance.ProviderDataStore;
			}
		}

		/// <summary>
		/// Gets the connection string for the data store containing the gallery data.
		/// </summary>
		/// <value>The connection string.</value>
		public string ConnectionString
		{
			get { return Factory.GetConnectionStringSettings().ConnectionString; }
		}

		/// <summary>
		/// Gets a collection of names of membership tables whose data is to be imported or exported into or from a data store.
		/// They are returned in the order in which they must be populated during a restoration. Reverse the collection if 
		/// you wish to delete the table contents.
		/// </summary>
		/// <value>The membership table names.</value>
		public string[] MembershipTables
		{
			get
			{
				return new[]
					       {
						       "Applications", "Users", "Memberships", "Roles", "UsersInRoles", "Profiles"
					       };
			}
		}

		/// <summary>
		/// Gets a collection of names of gallery tables whose data is to be imported or exported into or from a data store.
		/// They are returned in the order in which they must be populated during a restoration. Reverse the collection if 
		/// you wish to delete the table contents.
		/// </summary>
		/// <value>The gallery table names.</value>
		public string[] GalleryTableNames
		{
			get
			{
				// Ignore these tables: Synchronize, Event, MediaQueue
				return new[]
					       {
						       "Gallery", "MimeType", "MimeTypeGallery", "GallerySetting", "Album", "Role", "RoleAlbum",
						       "UiTemplate", "UiTemplateAlbum", "MediaObject", "Metadata", "Tag", "MetadataTag",
						       "UserGalleryProfile", "AppSetting", "GalleryControlSetting", "MediaTemplate"
					       };
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BackupFile"/> class.
		/// </summary>
		public BackupFile()
			: this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BackupFile"/> class.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		public BackupFile(string filePath)
		{
			FilePath = filePath;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Validates that the current backup file is valid and populates the remaining properties with information about the file.
		/// </summary>
		public void Validate()
		{
			BackupFileController.ValidateFile(this);
		}

		/// <summary>
		/// Exports the Gallery Server data in the current database to an XML-formatted string. Does not export the actual media files;
		/// they must be copied manually with a utility such as Windows Explorer. This method does not make any changes to the database tables
		/// or the files in the media objects directory.
		/// </summary>
		/// <returns>Returns an XML-formatted string containing the gallery data.</returns>
		public string Create()
		{
			return BackupFileController.ExportToFile(this);
		}

		/// <summary>
		/// Imports the Gallery Server data into the current database, overwriting any existing data. Does not import the actual media
		/// files; they must be imported manually with a utility such as Windows Explorer. This method makes changes only to the database tables;
		/// no files in the media objects directory are affected. If both <see cref="IncludeMembershipData" /> and <see cref="IncludeGalleryData" />
		/// are false, then no action is taken.
		/// </summary>
		public void Import()
		{
			BackupFileController.ImportFromFile(this);
		}

		#endregion

		#region Functions

		#endregion
	}
}
