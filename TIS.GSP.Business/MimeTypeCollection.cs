using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// Represents a set of MIME types.
	/// </summary>
	public class MimeTypeCollection : Collection<IMimeType>, IMimeTypeCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeTypeCollection"/> class.
		/// </summary>
		public MimeTypeCollection()
			: base(new List<IMimeType>())
		{
		}

		/// <summary>
		/// Adds the specified MIME type.
		/// </summary>
		/// <param name="item">The MIME type to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IMimeType item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing MimeTypeCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		/// <summary>
		/// Find the MIME type in the collection that matches the specified <paramref name="fileExtension" />. If no matching object is found,
		/// null is returned. It is not case sensitive.
		/// </summary>
		/// <param name="fileExtension">A string representing the file's extension, including the period (e.g. ".jpg", ".avi").
		/// It is not case sensitive.</param>
		/// <returns>Returns an <see cref="IMimeType" />object from the collection that matches the specified <paramref name="fileExtension" />,
		/// or null if no matching object is found.</returns>
		public IMimeType Find(string fileExtension)
		{
			List<IMimeType> mimeTypes = (List<IMimeType>)Items;

			return mimeTypes.Find(delegate(IMimeType gallery)
			{
				return (gallery.Extension.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
			});
		}
	}
}
