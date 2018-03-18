using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A simple object that contains synchronization status information. This class is used to pass information between the browser and the web server
	/// via AJAX callbacks during a synchronization.
	/// </summary>
	public class SynchStatusWebEntity
	{
		/// <summary>
		/// A GUID that uniquely identifies the current synchronization.
		/// </summary>
		public string SynchId { get; set; }
		/// <summary>
		/// The status of the current synchronization. This is the text representation of the <see cref="SynchronizationState" /> enumeration.
		/// </summary>
		public string Status { get; set; }
		/// <summary>
		/// A user-friendly version of the status.
		/// </summary>
		public string StatusForUI { get; set; }
		/// <summary>
		/// The total number of files in the directory or directories that are being processed in the current synchronization.
		///  </summary>
		public int TotalFileCount { get; set; }
		/// <summary>
		/// The one-based index value of the current file being processed.
		/// </summary>
		public int CurrentFileIndex { get; set; }
		/// <summary>
		/// The path, including the file name, to the current file being processed. The path is relative to the media object
		/// directory. For example, if the media objects directory is C:\mypics\ and the file currently being processed is
		/// at C:\mypics\vacations\india\buddha.jpg, this property is vacations\india\buddha.jpg.
		/// </summary>
		public string CurrentFile { get; set; }
		/// <summary>
		/// The percent complete of the current synchronization.
		/// </summary>
		public int PercentComplete { get; set; }
    /// <summary>
    /// The rate of the current synchronization (e.g. "28.1 objects/sec").
    /// </summary>
    public string SyncRate { get; set; }
    /// <summary>
		/// A list of all files that were encountered during the synchronization but were not added. The key contains
		/// the name of the file; the value contains the reason why the object was skipped. Guaranteed to not be null.
		/// </summary>
		public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>> SkippedFiles { get; set; }
	}
}
