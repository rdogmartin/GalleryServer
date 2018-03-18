namespace GalleryServer.Web
{
	/// <summary>
	/// Contains constant values used within Gallery Server.
	/// </summary>
	public static class Constants
	{
		public const string APP_NAME = "Gallery Server";
		public static readonly string[] SAMPLE_ASSET_FILENAMES = { "Family portrait.jpg", "Nature path.jpg" }; // The names of sample assets in the App_Data directory
	}

	#region Public Enums

	/// <summary>
	/// Specifies a distinct web page within Gallery Server.
	/// </summary>
	/// <remarks>IMPORTANT: These enumeration values must match the ones defined in the PageId enumeration in gallery.ts</remarks>
	public enum PageId
	{
		none = 0,
		admin_albums,
		admin_backuprestore,
		admin_css,
		admin_eventlog,
		admin_galleries,
		admin_gallerysettings,
		admin_gallerycontrolsettings,
		admin_images,
		admin_manageroles,
		admin_manageusers,
		admin_mediaobjects,
		admin_metadata,
		admin_filetypes,
		admin_mediatemplates,
		admin_sitesettings,
		admin_uitemplates,
		admin_usersettings,
		admin_videoaudioother,
		admin_mediaqueue,
		album,
		//albumtreeview,
		changepassword,
		createaccount,
		error_cannotwritetodirectory,
		error_generic,
		//error_unauthorized, // Removed from use 2009.01.22 (feature # 128)
		//install,
		login,
		mediaobject,
		myaccount,
		recoverpassword,
		task_addobjects,
		//task_assignthumbnail,
		//task_createalbum,
		//task_deletealbum,
		//task_deletehires,
		//task_deleteobjects,
		//task_downloadobjects,
		//task_editcaptions,
		//task_rearrange,
		//task_rotateimage,
		//task_rotateimages,
		task_synchronize,
		//task_transferobject,
		//search,
		//upgrade
	}

	#endregion
}
