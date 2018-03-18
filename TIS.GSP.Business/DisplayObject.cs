using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// Represents a human viewable representation of a gallery object. Examples include the thumbnail, optimized, or full-size version
	/// of an image, the video of a video file, and the content of a document.
	/// </summary>
	public class DisplayObject : IDisplayObject
	{
		#region Private Fields

		private IGalleryObject _parent;
		private int _mediaObjectId;
		private int _width;
		private int _height;
		private string _filename;
		private string _filenamePhysicalPath = string.Empty;
		private int _fileSizeKB;
		private System.IO.FileInfo _fileInfo;
		private IMimeType _mimeType;
		private DisplayObjectType _displayType;
		private IDisplayObjectCreator _displayObjectCreator;
		private string _externalHtmlSource;
		private MimeTypeCategory _externalType;
		private string _tempFilePath;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayObject"/> class.
		/// </summary>
		/// <param name="width">The width of this object, in pixels.</param>
		/// <param name="height">The height of this object, in pixels.</param>
		/// <param name="filename">The name of the file representing this object. Example: sonorandesert.jpg</param>
		/// <param name="parent">The media object to which this display object applies.</param>
		/// <param name="displayType">The type of the display object.</param>
		/// <param name="displayObjectCreator">The object responsible for generating the file this display object points to.</param>
		private DisplayObject(int width, int height, string filename, IGalleryObject parent, DisplayObjectType displayType, IDisplayObjectCreator displayObjectCreator)
		{
			this._width = width;
			this._height = height;
			this._filename = filename;

			if (!String.IsNullOrEmpty(filename))
			{
				this._mimeType = Factory.LoadMimeType(parent.GalleryId, this._filename);
			}

			if (this._mimeType == null)
			{
				this._mimeType = new NullObjects.NullMimeType();
			}

			this._parent = parent;
			this._displayType = displayType;
			this._displayObjectCreator = displayObjectCreator;
			this._displayObjectCreator.Parent = this;

			if (this._parent is IAlbum)
			{
				this._mediaObjectId = int.MinValue;
			}
			else
			{
				this._mediaObjectId = parent.Id;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayObject"/> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <param name="displayType">The display type.</param>
		/// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
		/// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
		private DisplayObject(IGalleryObject parent, DisplayObjectType displayType, MimeTypeCategory mimeType)
		{
			if (displayType != DisplayObjectType.External)
				throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "This overload of the DisplayObject constructor can only be called when the displayType parameter is DisplayObjectType.External. Instead, it was {0}.", displayType.ToString()));

			this._width = int.MinValue;
			this._height = int.MinValue;
			this._filename = String.Empty;
			this._mimeType = Business.MimeType.CreateInstance(mimeType, string.Empty);
			this._externalType = this._mimeType.TypeCategory;
			this._parent = parent;
			this._displayType = displayType;
			this._displayObjectCreator = new NullObjects.NullDisplayObjectCreator { Parent = this };

			if (this._parent is IAlbum)
			{
				this._mediaObjectId = int.MinValue;
			}
			else
			{
				this._mediaObjectId = parent.Id;
			}

		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Create a new display object instance with the specified properties. No data is retrieved from the
		/// data store. A lazy load is used to inflate the object when necessary
		/// </summary>
		/// <param name="parent">The media object to which this display object applies.</param>
		/// <returns>Returns an instance representing a new display object with default properties.</returns>
		public static IDisplayObject CreateInstance(IGalleryObject parent)
		{
			return CreateInstance(parent, string.Empty, int.MinValue, int.MinValue, DisplayObjectType.Unknown, new NullObjects.NullDisplayObjectCreator());
		}

		/// <summary>
		/// Create a new display object instance with the specified properties. No data is retrieved from the
		/// data store. A lazy load is used to inflate the object when necessary
		/// </summary>
		/// <param name="parent">The media object to which this display object applies. This will typically be
		/// an Album object.</param>
		/// <param name="sourceMediaObjectId">The ID of the media object to use as the source for setting this 
		/// object's properties.</param>
		/// <param name="displayType">The display object type of the source media object to use to set this object's
		/// properties. For example, if displayType=Thumbnail, then use the properties of the source media
		/// object's thumbnail object to assign to this display object's properties.</param>
		/// <returns>Create a new display object instance with the specified properties.</returns>
		/// <remarks>This overload of CreateInstance() is typically used when instantiating albums.</remarks>
		public static IDisplayObject CreateInstance(IGalleryObject parent, int sourceMediaObjectId, DisplayObjectType displayType)
		{
			IDisplayObject newDisObject = CreateInstance(parent, string.Empty, int.MinValue, int.MinValue, displayType, new NullObjects.NullDisplayObjectCreator());

			newDisObject.MediaObjectId = sourceMediaObjectId;

			return newDisObject;
		}

		/// <summary>
		/// Create a new display object instance with the specified properties. No data is retrieved from the
		/// data store. A lazy load is used to inflate the object when necessary
		/// </summary>
		/// <param name="parent">The media object to which this display object applies.</param>
		/// <param name="fileName">The name of the file representing this object. Example: sonorandesert.jpg</param>
		/// <param name="width">The width of this object, in pixels.</param>
		/// <param name="height">The height of this object, in pixels.</param>
		/// <param name="displayType">The type of the display object.</param>
		/// <param name="displayObjectCreator">The object responsible for generating the file this display object points to.</param>
		/// <returns>Create a new display object instance with the specified properties.</returns>
		public static IDisplayObject CreateInstance(IGalleryObject parent, string fileName, int width, int height, DisplayObjectType displayType, IDisplayObjectCreator displayObjectCreator)
		{
			return new DisplayObject(width, height, fileName, parent, displayType, displayObjectCreator);
		}

		/// <summary>
		/// Create a new display object instance with the specified properties. No data is retrieved from the
		/// data store. A lazy load is used to inflate the object when necessary
		/// </summary>
		/// <param name="parent">The media object to which this display object applies.</param>
		/// <param name="displayType">The type of the display object.</param>
		/// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
		/// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
		/// <returns>Create a new display object instance with the specified properties.</returns>
		public static IDisplayObject CreateInstance(IGalleryObject parent, DisplayObjectType displayType, MimeTypeCategory mimeType)
		{
			if (displayType != DisplayObjectType.External)
				throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "This overload of DisplayObject.CreateInstance can only be called when the displayType parameter is DisplayObjectType.External. Instead, it was {0}.", displayType.ToString()));

			return new DisplayObject(parent, displayType, mimeType);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the gallery object this display object applies to.
		/// </summary>
		/// <value>The gallery object this display object applies to.</value>
		public IGalleryObject Parent
		{
			get
			{
				return this._parent;
			}
			set
			{
				this._parent = value;
			}
		}

		/// <summary>
		/// Gets or sets the width of this object, in pixels.
		/// </summary>
		/// <value>The width of this object, in pixels.</value>
		public int Width
		{
			get
			{
				VerifyObjectIsInflated();

				return this._width;
			}
			set
			{
				this._parent.HasChanges = (this._width == value ? this._parent.HasChanges : true);
				this._width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height of this object, in pixels.
		/// </summary>
		/// <value>The height of this object, in pixels.</value>
		public int Height
		{
			get
			{
				VerifyObjectIsInflated();

				return this._height;
			}
			set
			{
				this._parent.HasChanges = (this._height == value ? this._parent.HasChanges : true);
				this._height = value;
			}
		}

		/// <summary>
		/// Gets or sets the file representing this display object. Accessing this property causes the file to be
		/// generated if it does not exist (thumbnail images only; also, for Image instances, will generate the optimized image).
		/// Returns null for external objects (<see cref="ExternalType" /> = MimeTypeCategory.External).
		/// </summary>
		/// <value>The file representing this display object, or null when this instance represents and external object
		/// (<see cref="ExternalType" /> = MimeTypeCategory.External).</value>
		/// <exception cref="InvalidMediaObjectException">Thrown if the file 
		/// is located in a different directory than the directory of this object's containing album.</exception>
		public System.IO.FileInfo FileInfo
		{
			get
			{
				if ((this._fileInfo == null) && (this._displayType != DisplayObjectType.External))
				{
					if ((String.IsNullOrEmpty(this.FileNamePhysicalPath)) || (!System.IO.File.Exists(this.FileNamePhysicalPath)))
					{
						this.GenerateAndSaveFile();
						System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(this.FileNamePhysicalPath), "DisplayObject.FilenamePhysicalPath should not be empty after executing GenerateAndSaveFile().");
					}

					this._fileInfo = new System.IO.FileInfo(this.FileNamePhysicalPath);
				}

				return this._fileInfo;
			}
			set
			{
				#region Validation

				// Validate: Make sure the file is in the same directory as the album. Thumbnail and optimized files may be in a separate directory
				// as specified in the configuration file.
				if (value != null)
				{
					IAlbum parentAlbum = this.Parent.Parent as IAlbum;

					if (parentAlbum != null)
					{
						string albumOriginalPath = parentAlbum.FullPhysicalPathOnDisk;

						IGallerySettings gallerySetting = Factory.LoadGallerySetting(this.Parent.GalleryId);

						string albumOptimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumOriginalPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
						string albumThumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(albumOriginalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);

						string newDirPath = value.Directory.FullName;

						if (!((String.Equals(newDirPath, albumOriginalPath, StringComparison.OrdinalIgnoreCase))
							|| (String.Equals(newDirPath, albumOptimizedPath, StringComparison.OrdinalIgnoreCase))
							|| (String.Equals(newDirPath, albumThumbnailPath, StringComparison.OrdinalIgnoreCase))))
						{
							throw new InvalidMediaObjectException(String.Format(CultureInfo.CurrentCulture, Resources.DisplayObject_FileInfo_Ex_Msg, value.Name, parentAlbum.Id, albumOriginalPath, albumOptimizedPath, albumThumbnailPath));
						}
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}

				#endregion

				this._fileInfo = value;
				this.FileName = value.Name;
				this.FileNamePhysicalPath = value.FullName;

				this._mimeType = Factory.LoadMimeType(this.Parent.GalleryId, value.Name);

				if (this._mimeType == null)
				{
					this._mimeType = new NullObjects.NullMimeType();
				}

			}
		}

		/// <summary>
		/// Gets or sets the name of the file representing this object. Example: sonorandesert.jpg
		/// </summary>
		/// <value>The name of the file representing this object.</value>
		public string FileName
		{
			get
			{
				return this._filename;
			}
			set
			{
				this._parent.HasChanges = (this._filename == value ? this._parent.HasChanges : true);
				this._filename = (value == null ? string.Empty : value);

				if (!String.IsNullOrEmpty(value))
					this._mimeType = Factory.LoadMimeType(this.Parent.GalleryId, this._filename);
			}
		}

		/// <summary>
		/// Gets or sets the physical path to this object, including the object's name. Example:
		/// C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets\sonorandesert.jpg
		/// </summary>
		/// <value>The physical path to this object, including the object's nam.</value>
		public string FileNamePhysicalPath
		{
			get
			{
				if (String.IsNullOrEmpty(this._filenamePhysicalPath))
				{
					VerifyObjectIsInflated();
				}

				return this._filenamePhysicalPath;
			}
			set
			{
				this._filenamePhysicalPath = value;
			}
		}

		/// <summary>
		/// Gets or sets the physical path to a temporary version of this object. This property can be used as a holding area for
		/// an intermediate file that is created while processing the object, such as when ImageMagick is used to create a JPEG
		/// version of an object that is subsequently used by both the thumbnail and optimized image generators.
		/// Example: C:\Inetpub\wwwroot\galleryserverpro\App_Data\_Temp\sonorandesert.jpg
		/// </summary>
		/// <value>The physical path to a temporary version of this object.</value>
		public string TempFilePath
		{
			get
			{
				return _tempFilePath;
			}
			set
			{
				_tempFilePath = value;
			}
		}

		/// <summary>
		/// Gets or sets the size of the file, in KB, for this display object.
		/// </summary>
		/// <value>The size of the file, in KB, for this display object.</value>
		public int FileSizeKB
		{
			get
			{
				return this._fileSizeKB;
			}
			set
			{
				this._parent.HasChanges = (this._fileSizeKB == value ? this._parent.HasChanges : true);
				this._fileSizeKB = value;
			}
		}

		/// <summary>
		/// Gets the MIME type for this display object. The MIME type is determined from the extension of the <see cref="FileName"/> property. 
		/// Returns a <see cref="NullObjects.NullMimeType" /> object if the <see cref="FileName"/> property has not been set or a 
		/// MIME type cannot be determined from the file's extension.
		/// </summary>
		/// <value>The MIME type for this display object.</value>
		public IMimeType MimeType
		{
			get
			{
				return this._mimeType;
			}
		}

		/// <summary>
		/// Gets or sets the ID of the media object that contains the file specified in this object. For albums, it refers to the 
		/// media object used to represent the thumbnail image. For all other objects, it refers to this object's parent ID.
		/// </summary>
		/// <value>
		/// The ID of the media object that contains the file specified in this object.
		/// </value>
		public int MediaObjectId
		{
			get
			{
				return this._mediaObjectId;
			}
			set
			{
				this._mediaObjectId = value;
			}
		}

		/// <summary>
		/// Gets or sets the type of the display object.
		/// </summary>
		/// <value>The type of the display object.</value>
		public DisplayObjectType DisplayType
		{
			get
			{
				return this._displayType;
			}
			set
			{
				this._displayType = value;
			}
		}

		/// <summary>
		/// Gets or sets the object responsible for generating the file this display object points to.
		/// </summary>
		/// <value>
		/// The object responsible for generating the file this display object points to.
		/// </value>
		public IDisplayObjectCreator DisplayObjectCreator
		{
			get
			{
				return this._displayObjectCreator;
			}
			set
			{
				this._displayObjectCreator = value;
			}
		}

		/// <summary>
		/// Gets or sets the HTML that defines an externally stored media object, such as videos hosted at YouTube. For local
		/// media objects, this property is an empty string.
		/// </summary>
		/// <example> 
		/// For example, for a YouTube video it may look like this:
		/// <code>
		/// <![CDATA[
		///		<object width="425" height="344">
		///			<param name="movie" value="http://www.youtube.com/v/0tNzoCw9xms&hl=en"></param>
		///			<param name="allowFullScreen" value="true"></param>
		///			<embed src="http://www.youtube.com/v/0tNzoCw9xms&hl=en" type="application/x-shockwave-flash" allowfullscreen="true" width="425" height="344"></embed>
		///		</object>]]> 
		/// </code>
		/// </example> 
		/// <value>The HTML that defines an externally stored media object, such as YouTube or Silverlight.net.</value>
		public string ExternalHtmlSource
		{
			get
			{
				return this._externalHtmlSource;
			}
			set
			{
				this._externalHtmlSource = value;
			}
		}

		/// <summary>
		/// Gets or sets the MIME type category for an externally stored media object, such as videos hosted at YouTube or Silverlight.live.com.
		/// This property is not relevant for locally stored media objects.
		/// </summary>
		/// <value>The MIME type category for an externally stored media object.</value>
		public MimeTypeCategory ExternalType
		{
			get
			{
				return this._externalType;
			}
			set
			{
				this._externalType = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. No data is persisted to the data
		/// store.
		/// </summary>
		public void GenerateAndSaveFile()
		{
			this._displayObjectCreator.GenerateAndSaveFile();
		}

		/// <summary>
		/// Gets the width and height of this display object. The value is calculated from the physical file. Returns an empty
		/// <see cref="System.Windows.Size" /> instance if the value cannot be computed or is not applicable to the object
		/// (for example, for audio files and external media objects).
		/// </summary>
		/// <returns><see cref="System.Windows.Size" />.</returns>
		public Size GetSize()
		{
			return DisplayObjectCreator.GetSize(this);
		}

		#endregion

		#region Public Override Methods

		/// <summary>
		/// Serves as a hash function for a particular type. The hash code is based on the <see cref="FileNamePhysicalPath" /> property.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return this.FileNamePhysicalPath.GetHashCode();
		}

		#endregion

		#region Private Methods

		private void VerifyObjectIsInflated()
		{
			if (this._parent.IsNew)
			{
				return; // Don't inflate for new objects - there's nothing to get from the data store.
			}

			if (!this._parent.IsInflated)
			{
				this._parent.Inflate();

				System.Diagnostics.Debug.Assert(this._mediaObjectId > int.MinValue, "Inflating the parent of this DisplayObject should cause _mediaObjectId to be populated, but it did not.");
			}
		}

		#endregion
	}
}
