using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// The Image class represents a media object within Gallery Server that is an image.
	/// </summary>
	public class Image : GalleryObject
	{
		#region Private Fields

		#endregion

		#region Properties

		/// <summary>
		/// Gets the gallery object type.
		/// </summary>
		/// <value>
		/// An instance of <see cref="GalleryObjectType" />.
		/// </value>
		public override GalleryObjectType GalleryObjectType
		{
			get { return GalleryObjectType.Image; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of an <see cref="Image"/> object.
		/// </summary>
		/// <param name="imageFile">A <see cref="System.IO.FileInfo"/> object containing the original image for this object.</param>
		/// <param name="parentAlbum">The album that contains this object. This is a required parameter.</param>
		/// <exception cref="InvalidMediaObjectException">Thrown when 
		/// <paramref name="imageFile"/> refers to a file that is not in the same directory as the parent album's directory.</exception>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name="imageFile"/> is specified (not null) and its file extension does not correspond to an image MIME
		/// type, as determined by the MIME type definition in the configuration file.</exception>
		/// <exception cref="UnsupportedImageTypeException">Thrown when the 
		/// .NET Framework is unable to load an image file into the <see cref="System.Drawing.Bitmap"/> class. This is 
		/// probably because it is corrupted, not an image supported by the .NET Framework, or the server does not have 
		/// enough memory to process the image. The file cannot, therefore, be handled using the <see cref="Image"/> 
		/// class; use <see cref="GenericMediaObject"/> instead. This exception is thrown only when <paramref name="imageFile"/>
		/// is specified (non-null).</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
		/// <remarks>This constructor does not verify that <paramref name="imageFile"/> refers to a file type that is enabled in the 
		/// configuration file.</remarks>
		internal Image(System.IO.FileInfo imageFile, IAlbum parentAlbum)
			: this(int.MinValue, parentAlbum, string.Empty,
				int.MinValue, int.MinValue, int.MinValue, string.Empty, int.MinValue, int.MinValue,
				int.MinValue, string.Empty, int.MinValue, int.MinValue, int.MinValue, int.MinValue,
				String.Empty, DateTime.UtcNow, String.Empty, DateTime.MinValue, parentAlbum != null ? parentAlbum.IsPrivate : false, false, imageFile, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of an <see cref="Image" /> object.
		/// </summary>
		/// <param name="id">The <see cref="GalleryObject.Id">ID</see> that uniquely identifies this object. Specify int.MinValue for a new object.</param>
		/// <param name="parentAlbum">The album that contains this object. This is a required parameter.</param>
		/// <param name="thumbnailFilename">The filename of the thumbnail image.</param>
		/// <param name="thumbnailWidth">The width (px) of the thumbnail image.</param>
		/// <param name="thumbnailHeight">The height (px) of the thumbnail image.</param>
		/// <param name="thumbnailSizeKb">The size (KB) of the thumbnail image.</param>
		/// <param name="optimizedFilename">The filename of the optimized image.</param>
		/// <param name="optimizedWidth">The width (px) of the optimized image.</param>
		/// <param name="optimizedHeight">The height (px) of the optimized image.</param>
		/// <param name="optimizedSizeKb">The size (KB) of the optimized image.</param>
		/// <param name="originalFilename">The filename of the original image.</param>
		/// <param name="originalWidth">The width (px) of the original image.</param>
		/// <param name="originalHeight">The height (px) of the original image.</param>
		/// <param name="originalSizeKb">The size (KB) of the original image.</param>
		/// <param name="sequence">An integer that represents the order in which this image should appear when displayed.</param>
		/// <param name="createdByUsername">The user name of the account that originally added this object to the data store.</param>
		/// <param name="dateAdded">The date this image was added to the data store.</param>
		/// <param name="lastModifiedByUsername">The user name of the account that last modified this object.</param>
		/// <param name="dateLastModified">The date this object was last modified.</param>
		/// <param name="isPrivate">Indicates whether this object should be hidden from un-authenticated (anonymous) users.</param>
		/// <param name="isInflated">A bool indicating whether this object is fully inflated.</param>
		/// <param name="imageFile">A <see cref="FileInfo"/> object containing the original image for this object. This is intended to be 
		///   specified when creating a new media object from a file. Specify null when instantiating an object for an existing database
		///   record.</param>
		/// <param name="metadata">A collection of <see cref="Data.MetadataDto" /> instances containing metadata for the
		///   object. Specify null if not available.</param>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// <paramref name="imageFile"/> is specified (not null) and the file it refers to is not in the same directory
		/// as the parent album's directory.</exception>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when
		/// <paramref name="imageFile"/> is specified (not null) and its file extension does not correspond to an image MIME
		/// type, as determined by the MIME type definition in the configuration file.</exception>
		/// <exception cref="UnsupportedImageTypeException">Thrown when the 
		/// .NET Framework is unable to load an image file into the <see cref="System.Drawing.Bitmap"/> class. This is 
		/// probably because it is corrupted, not an image supported by the .NET Framework, or the server does not have 
		/// enough memory to process the image. The file cannot, therefore, be handled using the <see cref="Image"/> 
		/// class; use <see cref="GenericMediaObject"/> instead. This exception is thrown only when <paramref name="imageFile"/>
		/// is specified (non-null).</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
		/// <remarks>This constructor does not verify that <paramref name="imageFile"/> refers to a file type that is enabled in the 
		/// configuration file.</remarks>
		internal Image(int id, IAlbum parentAlbum, string thumbnailFilename, int thumbnailWidth, int thumbnailHeight, int thumbnailSizeKb, string optimizedFilename, int optimizedWidth, int optimizedHeight, int optimizedSizeKb, string originalFilename, int originalWidth, int originalHeight, int originalSizeKb, int sequence, string createdByUsername, DateTime dateAdded, string lastModifiedByUsername, DateTime dateLastModified, bool isPrivate, bool isInflated, FileInfo imageFile, IEnumerable<MetadataDto> metadata)
		{
			if (parentAlbum == null)
				throw new ArgumentNullException("parentAlbum");

			this.Id = id;
			this.Parent = parentAlbum;
			this.GalleryId = this.Parent.GalleryId;
			//this.Title = title;
			this.Sequence = sequence;
			this.CreatedByUserName = createdByUsername;
			this.DateAdded = dateAdded;
			this.LastModifiedByUserName = lastModifiedByUsername;
			this.DateLastModified = dateLastModified;
			this.IsPrivate = isPrivate;
		  this.IsWritable = parentAlbum.IsWritable;

			this.SaveBehavior = Factory.GetMediaObjectSaveBehavior(this);
			this.DeleteBehavior = Factory.GetMediaObjectDeleteBehavior(this);
			this.MetadataReadWriter = Factory.GetMetadataReadWriter(this);

			string parentPhysicalPath = this.Parent.FullPhysicalPathOnDisk;
			IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryId);

			// Thumbnail image
			this.Thumbnail = DisplayObject.CreateInstance(this, thumbnailFilename, thumbnailWidth, thumbnailHeight, DisplayObjectType.Thumbnail, new ImageThumbnailCreator(this));
			this.Thumbnail.FileSizeKB = thumbnailSizeKb;
			if (thumbnailFilename.Length > 0)
			{
				// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified) .
				string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(parentPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
				this.Thumbnail.FileNamePhysicalPath = System.IO.Path.Combine(thumbnailPath, thumbnailFilename);
			}

			// Optimized image
			this.Optimized = DisplayObject.CreateInstance(this, optimizedFilename, optimizedWidth, optimizedHeight, DisplayObjectType.Optimized, new ImageOptimizedCreator(this));
			this.Optimized.FileSizeKB = optimizedSizeKb;
			if (optimizedFilename.Length > 0)
			{
				// Calcululate the full file path to the optimized image. If the optimized filename is equal to the original filename, then no
				// optimized version exists, and we'll just point to the original. If the names are different, then there is a separate optimized
				// image file, and it is stored in either the album's physical path or an alternate location (if optimizedPath config setting is specified).
				string optimizedPath = parentPhysicalPath;

				if (optimizedFilename != originalFilename)
					optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(parentPhysicalPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);

				this.Optimized.FileNamePhysicalPath = System.IO.Path.Combine(optimizedPath, optimizedFilename);
			}

			// Original image
			this.Original = DisplayObject.CreateInstance(this, originalFilename, originalWidth, originalHeight, DisplayObjectType.Original, new ImageOriginalCreator(this));
			this.Original.ExternalHtmlSource = String.Empty;
			this.Original.ExternalType = MimeTypeCategory.NotSet;

			if (imageFile != null)
			{
				this.Original.FileInfo = imageFile; // Will throw InvalidMediaObjectException if the file's directory is not the same as the album's directory.

				if (this.Original.MimeType.TypeCategory != MimeTypeCategory.Image)
				{
					throw new UnsupportedMediaObjectTypeException(this.Original.FileInfo);
				}

				var size = this.Original.GetSize();
				if (!size.IsEmpty)
				{
					this.Original.Width = (int)size.Width;
					this.Original.Height = (int)size.Height;
				}

				int fileSize = (int)(imageFile.Length / 1024);
				this.Original.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.

				// Get metadata from the image file.
				if (IsNew)
				{
					try
					{
						ExtractMetadata();
					}
					catch (OutOfMemoryException ex)
					{
						this.Parent.RemoveGalleryObject(this);
						throw new UnsupportedImageTypeException(this, ex);
					}
				}
			}
			else
			{
				this.Original.FileNamePhysicalPath = System.IO.Path.Combine(parentPhysicalPath, originalFilename);
				this.Original.FileSizeKB = originalSizeKb;
			}

			if (metadata != null)
				AddMeta(GalleryObjectMetadataItemCollection.FromMetaDtos(this, metadata));

			this.IsInflated = isInflated;

			// Setting the previous properties has caused HasChanges = true, but we don't want this while
			// we're instantiating a new object. Reset to false.
			this.HasChanges = false;

			// Set up our event handlers.
			//this.Saving += new EventHandler(Image_Saving); // Don't need
			this.Saved += Image_Saved;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Inflate the current object by loading all properties from the data store. If the object is already inflated 
		/// (<see cref="GalleryObject.IsInflated"/>=true), no action is taken.
		/// </summary>
		public override void Inflate()
		{
			// If this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((!this.IsNew) && (!this.IsInflated))
			{
				Factory.LoadImageInstance(this);

				if ((!this.IsInflated) || (this.HasChanges))
					throw new System.InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.Image_Inflate_Ex_Msg, this.IsInflated, this.HasChanges));
			}
		}

		/// <summary>
		/// This method verifies the optimized image maps to an existing file on disk. If not, set the
		///  <see cref="GalleryObject.RegenerateThumbnailOnSave" />
		/// property to true so that the thumbnail image is created during the <see cref="GalleryObject.Save" /> operation.
		/// </summary>
		protected override void CheckForOptimizedImage()
		{
			if (!System.IO.File.Exists(this.Optimized.FileNamePhysicalPath))
			{
				this.RegenerateOptimizedOnSave = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the specified <paramref name="metaDef" />
		/// applies to the current gallery object.
		/// </summary>
		/// <param name="metaDef">The metadata definition.</param>
		/// <returns><c>true</c> when the specified metadata item should be displayed; otherwise <c>false</c>.</returns>
		public override bool MetadataDefinitionApplies(IMetadataDefinition metaDef)
		{
			switch (metaDef.MetadataItem)
			{
				case MetadataItemName.HtmlSource: return false;
				default:
					return base.MetadataDefinitionApplies(metaDef);
			}
		}

		#endregion

		#region Event Handlers

		void Image_Saved(object sender, EventArgs e)
		{
			// This event is fired when the Save() method is called, after all data is saved.

			#region Assign DisplayObject.MediaObjectId

			// If the MediaObjectId has not yet been assigned, do so now. This will occur after a media object is first
			// saved, since that is when the ID is generated.
			if (this.Thumbnail.MediaObjectId == int.MinValue)
			{
				this.Thumbnail.MediaObjectId = this.Id;
			}

			if (this.Optimized.MediaObjectId == int.MinValue)
			{
				this.Optimized.MediaObjectId = this.Id;
			}

			if (this.Original.MediaObjectId == int.MinValue)
			{
				this.Original.MediaObjectId = this.Id;
			}

			#endregion
		}

		#endregion
	}
}
