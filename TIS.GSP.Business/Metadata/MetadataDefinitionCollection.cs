using System;
using System.Collections.ObjectModel;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{
	/// <summary>
	/// A collection of <see cref="IMetadataDefinition" /> objects.
	/// </summary>
	public class MetadataDefinitionCollection : KeyedCollection<MetadataItemName, IMetadataDefinition>, IMetadataDefinitionCollection
	{
		/// <summary>
		/// Adds the specified metadata definition.
		/// </summary>
		/// <param name="item">The metadata definition to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IMetadataDefinition item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing MetadataDefinitionCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		/// <summary>
		/// Find the metadata definition in the collection that matches the specified <paramref name="metadataItemName" />. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="metadataItemName">The metadata item to find.</param>
		/// <returns>Returns an <see cref="IMetadataDefinition" />object from the collection that matches the specified <paramref name="metadataItemName" />,
		/// or null if no matching object is found.</returns>
		public IMetadataDefinition Find(MetadataItemName metadataItemName)
		{
			return base[metadataItemName];
		}

		/// <summary>
		/// Verify that an item exists in this collection for every enumeration value of 
		/// <see cref="MetadataItemName" />. If an item is missing, one is added with default values.
		/// This should be called after the collection is populated from the gallery settings. Doing this 
		/// validation guarantees that later calls to <see cref="IMetadataDefinitionCollection.Find" /> will 
		/// never fail and helps to automatically add items for newly added 
		/// <see cref="MetadataItemName" /> values. 
		/// </summary>
		public void Validate()
		{
			foreach (MetadataItemName item in Enum.GetValues(typeof(MetadataItemName)))
			{
				if (!base.Contains(item) && item != MetadataItemName.NotSpecified)
				{
					base.Add(new MetadataDefinition(item, item.ToString(), false, false, PropertyEditorMode.PlainTextEditor, false, int.MaxValue, String.Empty));
				}
			}

			foreach (var metaDef in this.Items)
			{
				if (!metaDef.IsPersistable)
				{
					// When the application determines a meta item cannot be written to the original file, update PersistToFile
					// accordingly. This ensures data integrity and shouldn't typically be needed, except perhaps when the user
					// edits the meta definitions directly in the SQL table.
					metaDef.PersistToFile = false;
				}
			}

			////Remove after metadata defs are all created as desired
			//foreach (IMetadataDefinition metadataDef in base.Items)
			//{
			//	if (String.IsNullOrEmpty(metadataDef.DisplayName))
			//	{
			//		metadataDef.DisplayName = metadataDef.MetadataItem.ToString();
			//		metadataDef.DefaultValue = String.Concat("{", metadataDef.MetadataItem.ToString(), "}");
			//	}
			//}
		}

		/// <summary>
		/// Generates as string representation of the items in the collection.
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		public string Serialize()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(base.Items);
		}

		/// <summary>
		/// When implemented in a derived class, extracts the key from the specified element.
		/// </summary>
		/// <returns>
		/// The key for the specified element.
		/// </returns>
		/// <param name="item">The element from which to extract the key.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		protected override MetadataItemName GetKeyForItem(IMetadataDefinition item)
		{
			if (item == null)
				throw new ArgumentNullException("item"); 
			
			return item.MetadataItem;
		}
	}
}
