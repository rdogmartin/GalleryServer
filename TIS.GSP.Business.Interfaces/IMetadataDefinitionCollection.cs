using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IMetadataDefinition" /> objects.
	/// </summary>
	public interface IMetadataDefinitionCollection : System.Collections.Generic.ICollection<IMetadataDefinition>
	{
		/// <summary>
		/// Adds the specified metadata definition.
		/// </summary>
		/// <param name="item">The metadata definition to add.</param>
		new void Add(IMetadataDefinition item);

		/// <summary>
		/// Find the metadata definition in the collection that matches the specified <paramref name="metadataItemName" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="metadataItemName">The metadata item to find.</param>
		/// <returns>Returns an <see cref="IMetadataDefinition" />object from the collection that matches the specified <paramref name="metadataItemName" />,
		/// or null if no matching object is found.</returns>
		IMetadataDefinition Find(MetadataItemName metadataItemName);

		/// <summary>
		/// Verify that there exists an item in this collection for every enumeration value of <see cref="MetadataItemName" />.
		/// This should be called after the collection is filled. Doing this validation guarantees that later calls to <see cref="Find" />
		/// will never fail.
		/// </summary>
		void Validate();

		/// <summary>
		/// Generates as string representation of the items in the collection.
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		string Serialize();
	}
}
