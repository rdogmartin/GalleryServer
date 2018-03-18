using System;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Business.NullObjects
{
	/// <summary>
	/// Represents a <see cref="IGalleryObject" /> that is equivalent to null. This class is used instead of null to prevent 
	/// <see cref="NullReferenceException" /> errors if the calling code accesses a property or executes a method.
	/// </summary>
	public class NullGalleryObject : IGalleryObject, IComparable
	{
		private int _id = int.MinValue;

		public int Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}

		public IGalleryObject Parent
		{
			get
			{
				return new NullGalleryObject();
			}
			set
			{
			}
		}

		public string Title
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		public string Caption
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		public int GalleryId
		{
			get { return int.MinValue; }
			set
			{
			}
		}

		public bool GalleryIdHasChanged
		{
			get { return false; }
		}

		public IDisplayObject Thumbnail
		{
			get
			{
				return new NullDisplayObject();
			}
			set
			{
			}
		}

		public IDisplayObject Optimized
		{
			get
			{
				return new NullDisplayObject();
			}
			set
			{
			}
		}

		public IDisplayObject Original
		{
			get
			{
				return new NullDisplayObject();
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the current instance can be modified. Objects that are stored in a cache must
		/// be treated as read-only. Only objects that are instantiated right from the database and not shared across threads
		/// should be updated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can be modified; otherwise, <c>false</c>.
		/// </value>
		/// <exception cref="ArgumentException">Thrown when there is an attempt to assign a new value to this property once it has
		/// been assigned.</exception>
		public bool IsWritable
		{
			get { return false; }
			set { }
		}

		public void AddGalleryObject(IGalleryObject galleryObject)
		{
		}

		public void DoAddGalleryObject(IGalleryObject galleryObject)
		{
		}

		public void RemoveGalleryObject(IGalleryObject galleryObject)
		{
		}

		public IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool sortBySequence)
		{
			return new GalleryObjectCollection();
		}

		public void AddMeta(IGalleryObjectMetadataItemCollection metaItems)
		{
		}

		public void Save()
		{
			if (Saving != null)
			{
				Saving(this, new EventArgs());
			}

			if (Saved != null)
			{
				Saved(this, new EventArgs());
			}
		}

		public void DeleteOriginalFile()
		{
		}

		public void Inflate()
		{
		}

		public bool MetadataDefinitionApplies(IMetadataDefinition metaDef)
		{
			return false;
		}

		public string FullPhysicalPath
		{
			get { return string.Empty; }
		}

		public string FullPhysicalPathOnDisk
		{
			get { return string.Empty; }
			set { }
		}

		public bool HasChanges
		{
			get { return false; }
			set
			{
			}
		}

		public bool IsNew
		{
			get { return false; }
		}

		public void Delete()
		{
		}

		public void DeleteFromGallery()
		{
		}

		public bool IsInflated
		{
			get { return true; }
			set
			{
			}
		}

		public GalleryObjectType GalleryObjectType
		{
			get
			{
				return GalleryObjectType.None;
			}
		}

		public IMimeType MimeType
		{
			get
			{
				return new NullMimeType();
			}
		}

		public int Sequence
		{
			get
			{
				return int.MinValue;
			}
			set
			{
			}
		}

		public bool RegenerateThumbnailOnSave
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool RegenerateOptimizedOnSave
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public event System.EventHandler Saving;

		public event System.EventHandler Saved;

		public DateTime DateAdded
		{
			get
			{
				return DateTime.MinValue;
			}
			set
			{
			}
		}

		public string CreatedByUserName
		{
			get { return string.Empty; }
			set { }
		}

		public string LastModifiedByUserName
		{
			get { return string.Empty; }
			set { }
		}

		public DateTime DateLastModified
		{
			get { return DateTime.MinValue; }
			set { }
		}

		public bool IsPrivate
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public IMetadataReadWriter MetadataReadWriter
		{
			get;
			set;
		}

		public void SetParentToNullObject()
		{
		}

		public IMetadataDefinitionCollection MetaDefinitions
		{
			get
			{
				return new Metadata.MetadataDefinitionCollection();
			}
		}

		public IGalleryObjectMetadataItemCollection MetadataItems
		{
			get
			{
				return new Metadata.GalleryObjectMetadataItemCollection();
			}
		}

		public bool IsMetadataLoaded
		{
			get
			{
				return false;
			}
			set
			{

			}
		}

		public MediaAssetRotateFlip RotateFlip
		{
			get
			{
				return MediaAssetRotateFlip.NotSpecified;
			}
			set
			{
			}
		}

		public IGalleryObject CopyTo(IAlbum destinationAlbum, string userName)
		{
			return new NullGalleryObject();
		}

		/// <summary>
		/// Build the set of metadata for the current gallery object and assign to the <see cref="IGalleryObject.MetadataItems" />
		/// property.
		/// </summary>
		public void ExtractMetadata()
		{
		}

		public void ExtractMetadata(IMetadataDefinition metaDef)
		{
		}

		/// <summary>
		/// Creates a metadata item for the current gallery object. The parameter <paramref name="metaDef" />
		/// contains the template and display name to use. Guaranteed to not return null.
		/// </summary>
		/// <param name="metaDef">The metadata definition.</param>
		/// <returns>An instance of <see cref="IGalleryObjectMetadataItem" />.</returns>
		public IGalleryObjectMetadataItem CreateMetaItem(IMetadataDefinition metaDef)
		{
			return Factory.CreateMetadataItem(int.MinValue, new NullGalleryObject(), null, String.Empty, false, new Metadata.MetadataDefinition(Metadata.MetadataItemName.AudioBitRate, String.Empty, false, false, PropertyEditorMode.NotSet, false, int.MinValue, String.Empty));
		}

		public MediaAssetRotateFlip CalculateNeededRotation()
		{
			return MediaAssetRotateFlip.NotSpecified;
		}

		public Orientation GetOrientation()
		{
			return Orientation.NotInitialized;
		}

		public IGalleryObjectMetadataItemCollection GetMetadata()
		{
			return new Metadata.GalleryObjectMetadataItemCollection();
		}

		public void MoveTo(IAlbum destinationAlbum)
		{
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;
			else
			{
				IGalleryObject other = obj as IGalleryObject;
				if (other != null)
					return this.Sequence.CompareTo(other.Sequence);
				else
					return 1;
			}
		}

		#endregion
	}
}
