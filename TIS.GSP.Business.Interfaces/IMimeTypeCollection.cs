namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IMimeType" /> objects.
	/// </summary>
	public interface IMimeTypeCollection : System.Collections.Generic.ICollection<IMimeType>
	{
		/// <summary>
		/// Adds the specified MIME type.
		/// </summary>
		/// <param name="item">The MIME type to add.</param>
		new void Add(IMimeType item);

		/// <summary>
		/// Find the MIME type in the collection that matches the specified <paramref name="fileExtension" />. If no matching object is found,
		/// null is returned. It is not case sensitive.
		/// </summary>
		/// <param name="fileExtension">A string representing the file's extension, including the period (e.g. ".jpg", ".avi").
		/// It is not case sensitive.</param>
		/// <returns>Returns an <see cref="IMimeType" />object from the collection that matches the specified <paramref name="fileExtension" />,
		/// or null if no matching object is found.</returns>
		IMimeType Find(string fileExtension);
	}
}
