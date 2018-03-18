using System.Web;

namespace GalleryServer.Web.Entity
{
	/// <summary>
	/// A simple object that contains synchronization options.
	/// </summary>
	public class SyncOptions
	{
		public string SyncId { get; set; }
		public string UserName { get; set; }
		public int AlbumIdToSynchronize { get; set; }
		public bool IsRecursive { get; set; }
		public bool RebuildThumbnails { get; set; }
		public bool RebuildOptimized { get; set; }
		public SyncInitiator SyncInitiator { get; set; }
	}

  /// <summary>
  /// An enumeration that stores values for possible objects that can initiate a synchronization.
  /// </summary>
  public enum SyncInitiator
  {
    /// <summary>
    /// 
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// 
    /// </summary>
    LoggedOnGalleryUser = 1,
    /// <summary>
    /// 
    /// </summary>
    AutoSync = 2,
    /// <summary>
    /// 
    /// </summary>
    RemoteApp = 3
  }
}