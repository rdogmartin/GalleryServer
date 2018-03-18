namespace GalleryServer.Business
{
	/// <summary>
	/// Contains settings and data for a media file to be added to the gallery. This class may be used in 
	/// javascript.
	/// </summary>
	public class AddMediaObjectSettings
	{
		/// <summary>
		/// The name to use when persisting the media file to the destination directory.
		/// </summary>
		public string FileName;

		/// <summary>
		/// The name of the file as it exists in the temporary upload directory on the server.
		/// </summary>
		public string FileNameOnServer;

		/// <summary>
		/// The ID of the album the media object is a member of.
		/// </summary>
		public int AlbumId;

		/// <summary>
		/// Indicates whether to discard the original file after creating a web-optimized version. When a
		/// web-optimized version is not created (e.g. for PDF files or videos when FFmpeg is not available),
		/// this setting is ignored.
		/// </summary>
		public bool DiscardOriginalFile;

		/// <summary>
		/// Indicates whether to extract the contents of a ZIP archive or treat it as its own media object.
		/// When <c>true</c>, the contents are extracted and the original ZIP archive is discarded. When
		/// <c>false</c>, no contents are extracted and the file is treated as a regular media object.
		/// </summary>
		public bool ExtractZipFile;

		/// <summary>
		/// The user name for the current user.
		/// </summary>
		public string CurrentUserName;
	}
}