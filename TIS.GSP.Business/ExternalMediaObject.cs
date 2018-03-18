using System;
using System.Collections.Generic;
using System.Globalization;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
	/// <summary>
	/// The <see cref="ExternalMediaObject" /> class represents a media object within Gallery Server that references an 
	/// externally stored object, such as a video on YouTube or Silverlight.streaming.net.
	/// </summary>
	public class ExternalMediaObject : GalleryObject
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
			get { return GalleryObjectType.External; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of a ExternalMediaObject object.
		/// </summary>
		/// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at 
		/// YouTube or Silverlight.live.com.</param>
		/// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
		/// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
		/// <param name="parentAlbum">The album that contains this object. This is a required parameter.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
		internal ExternalMediaObject(string externalHtmlSource, MimeTypeCategory mimeType, IAlbum parentAlbum)
			: this(int.MinValue, parentAlbum, string.Empty, int.MinValue, int.MinValue, int.MinValue, externalHtmlSource, mimeType, int.MinValue, String.Empty,
						DateTime.UtcNow, String.Empty, DateTime.MinValue, parentAlbum != null ? parentAlbum.IsPrivate : false, false, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of a ExternalMediaObject object.
		/// </summary>
		/// <param name="id">The ID that uniquely identifies this object. Specify int.MinValue for a new object.</param>
		/// <param name="parentAlbum">The album that contains this object. This is a required parameter.</param>
		/// <param name="thumbnailFilename">The filename of the thumbnail image.</param>
		/// <param name="thumbnailWidth">The width (px) of the thumbnail image.</param>
		/// <param name="thumbnailHeight">The height (px) of the thumbnail image.</param>
		/// <param name="thumbnailSizeKb">The size (KB) of the thumbnail image.</param>
		/// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at
		///   YouTube or Silverlight.live.com.</param>
		/// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of
		///   the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
		/// <param name="sequence">An integer that represents the order in which this image should appear when displayed.</param>
		/// <param name="createdByUsername">The user name of the account that originally added this object to the data store.</param>
		/// <param name="dateAdded">The date this image was added to the data store.</param>
		/// <param name="lastModifiedByUsername">The user name of the account that last modified this object.</param>
		/// <param name="dateLastModified">The date this object was last modified.</param>
		/// <param name="isPrivate">Indicates whether this object should be hidden from un-authenticated (anonymous) users.</param>
		/// <param name="isInflated">A bool indicating whether this object is fully inflated.</param>
		/// <param name="metadata">A collection of <see cref="Data.MetadataDto" /> instances containing metadata for the
		///   object. Specify null if not available.</param>
		/// <exception cref="InvalidMediaObjectException">Thrown when
		/// the file parameter is specified (not null) and the file it refers to is not in the same directory
		/// as the parent album's directory.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="parentAlbum" /> is null.</exception>
		internal ExternalMediaObject(int id, IAlbum parentAlbum, string thumbnailFilename, int thumbnailWidth, int thumbnailHeight, int thumbnailSizeKb, string externalHtmlSource, MimeTypeCategory mimeType, int sequence, string createdByUsername, DateTime dateAdded, string lastModifiedByUsername, DateTime dateLastModified, bool isPrivate, bool isInflated, IEnumerable<MetadataDto> metadata)
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
			this.Thumbnail = DisplayObject.CreateInstance(this, thumbnailFilename, thumbnailWidth, thumbnailHeight, DisplayObjectType.Thumbnail, new ExternalThumbnailCreator(this));
			this.Thumbnail.FileSizeKB = thumbnailSizeKb;
			if (thumbnailFilename.Length > 0)
			{
				// The thumbnail is stored in either the album's physical path or an alternate location (if thumbnailPath config setting is specified).
				string thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(parentPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
				this.Thumbnail.FileNamePhysicalPath = System.IO.Path.Combine(thumbnailPath, thumbnailFilename);
			}

			// ExternalMediaObject instances do not have an optimized version.
			this.Optimized = new NullObjects.NullDisplayObject();

			// Original file
			this.Original = DisplayObject.CreateInstance(this, DisplayObjectType.External, mimeType);
			this.Original.ExternalHtmlSource = externalHtmlSource;

			switch (mimeType)
			{
				case MimeTypeCategory.Audio:
					this.Original.Width = gallerySetting.DefaultAudioPlayerWidth;
					this.Original.Height = gallerySetting.DefaultAudioPlayerHeight;
					break;
				case MimeTypeCategory.Video:
					this.Original.Width = gallerySetting.DefaultVideoPlayerWidth;
					this.Original.Height = gallerySetting.DefaultVideoPlayerHeight;
					break;
				case MimeTypeCategory.Image:
				case MimeTypeCategory.Other:
					this.Original.Width = gallerySetting.DefaultGenericObjectWidth;
					this.Original.Height = gallerySetting.DefaultGenericObjectHeight;
					break;
			}

			if (IsNew)
			{
				ExtractMetadata();
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
		/// (<see cref="IGalleryObject.IsInflated"/>=true), no action is taken.
		/// </summary>
		public override void Inflate()
		{
			// If this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((!this.IsNew) && (!this.IsInflated))
			{
				Factory.LoadExternalMediaObjectInstance(this);

				if ((!this.IsInflated) || (this.HasChanges))
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.GenericMediaObject_Inflate_Ex_Msg, this.IsInflated, this.HasChanges));
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
