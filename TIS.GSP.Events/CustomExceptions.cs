using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.Properties;

namespace GalleryServer.Events.CustomExceptions
{
	/// <summary>
	///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
	/// </summary>
	[Serializable]
	public class WebException : Exception
	{
		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		public WebException()
			: base(Resources.Web_Ex_Msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public WebException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public WebException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Web namespace.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected WebException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
	/// </summary>
	[Serializable]
	public class BusinessException : Exception
	{
		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		public BusinessException()
			: base(Resources.Business_Ex_Msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public BusinessException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public BusinessException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Business namespace.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected BusinessException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
	/// </summary>
	[Serializable]
	public class DataException : Exception
	{
		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		public DataException()
			: base(Resources.Data_Ex_Msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public DataException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public DataException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   The exception that is thrown when a general error occurs in the GalleryServer.Data namespace.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected DataException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid media object is referenced.
	/// </summary>
	[Serializable]
	public class ApplicationNotInitializedException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate Gallery Server has not been properly intialized.
		/// </summary>
		public ApplicationNotInitializedException()
			: base(Resources.ApplicationNotInitialized_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate Gallery Server has not been properly intialized.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public ApplicationNotInitializedException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate Gallery Server has not been properly intialized.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public ApplicationNotInitializedException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate Gallery Server has not been properly intialized.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected ApplicationNotInitializedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user attempts to perform an action the user does not have authorization to perform.
	/// </summary>
	[Serializable]
	public class GallerySecurityException : Exception
	{
		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		public GallerySecurityException()
			: base(Resources.GallerySecurity_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public GallerySecurityException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public GallerySecurityException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception when a user attempts to perform an action the user does not have authorization to perform.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected GallerySecurityException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid gallery is referenced.
	/// </summary>
	[Serializable]
	public class InvalidGalleryException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		public InvalidGalleryException()
			: base(Resources.InvalidGallery_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidGalleryException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery that is not valid.</param>
		public InvalidGalleryException(int galleryId)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidGallery_Ex_Msg2, galleryId))
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidGalleryException(int galleryId, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidGallery_Ex_Msg2, galleryId), innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidGalleryException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid gallery.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidGalleryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid media object is referenced.
	/// </summary>
	[Serializable]
	public class InvalidMediaObjectException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		public InvalidMediaObjectException()
			: base(Resources.InvalidMediaObject_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidMediaObjectException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="mediaObjectId">The ID of the media object that is not valid.</param>
		public InvalidMediaObjectException(int mediaObjectId)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidMediaObject_Ex_Msg2, mediaObjectId))
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="mediaObjectId">The ID of the media object that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidMediaObjectException(int mediaObjectId, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidMediaObject_Ex_Msg2, mediaObjectId), innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidMediaObjectException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media object.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidMediaObjectException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid album is referenced.
	/// </summary>
	[Serializable]
	public class InvalidAlbumException : Exception
	{
		[NonSerialized] private readonly int _albumId;

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		public InvalidAlbumException()
			: base(Resources.InvalidAlbum_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidAlbumException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="albumId">The ID of the album that is not valid.</param>
		public InvalidAlbumException(int albumId)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidAlbum_Ex_Msg2, albumId))
		{
			_albumId = albumId;
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="albumId">The ID of the album that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidAlbumException(int albumId, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidAlbum_Ex_Msg2, albumId), innerException)
		{
			_albumId = albumId;
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidAlbumException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid album.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidAlbumException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the album ID that is causing the exception.
		/// </summary>
		public int AlbumId
		{
			get { return _albumId; }
		}
	}

	/// <summary>
	///   The exception that is thrown when the user album feature is enabled but the album ID that is specified for
	///   containing the user albums does not exist.
	/// </summary>
	[Serializable]
	public class CannotDeleteAlbumException : Exception
	{
		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		public CannotDeleteAlbumException()
			: base(Resources.CannotDeleteAlbum_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public CannotDeleteAlbumException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="albumId">The ID of the album that cannot be deleted.</param>
		public CannotDeleteAlbumException(int albumId)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.CannotDeleteAlbum_Ex_Msg2, albumId))
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="albumId">The ID of the album that cannot be deleted.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotDeleteAlbumException(int albumId, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.CannotDeleteAlbum_Ex_Msg2, albumId), innerException)
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotDeleteAlbumException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception when an album cannot be deleted.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotDeleteAlbumException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
	///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
	///   not have enough memory to process the image.
	/// </summary>
	[Serializable]
	public class UnsupportedImageTypeException : Exception
	{
		[NonSerialized] private UnsupportedImageTypeExceptionState _state;

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		public UnsupportedImageTypeException()
			: base(Resources.UnsupportedImageType_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public UnsupportedImageTypeException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedImageTypeException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="mediaObject">The media object that contains the unsupported image file.</param>
		public UnsupportedImageTypeException(IGalleryObject mediaObject)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.UnsupportedImageType_Ex_Msg2, ((mediaObject != null) && (mediaObject.Original != null) ? mediaObject.Original.FileName : String.Empty)))
		{
			_state.MediaObject = mediaObject;

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="mediaObject">The media object that contains the unsupported image file.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedImageTypeException(IGalleryObject mediaObject, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.UnsupportedImageType_Ex_Msg2, ((mediaObject != null) && (mediaObject.Original != null) ? mediaObject.Original.FileName : String.Empty)), innerException)
		{
			_state.MediaObject = mediaObject;

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate the .NET Framework is unable to load an image file into the System.Drawing.Bitmap
		///   class. This is probably because it is corrupted, not an image supported by the .NET Framework, or the server does
		///   not have enough memory to process the image.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected UnsupportedImageTypeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the media object that is causing the exception.
		/// </summary>
		public IGalleryObject MediaObject
		{
			get { return _state.MediaObject; }
		}

		/// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct UnsupportedImageTypeExceptionState : ISafeSerializationData
		{
			private IGalleryObject _mediaObject;

			/// <summary>
			///   Gets the media object that is causing the exception.
			/// </summary>
			public IGalleryObject MediaObject
			{
				get { return _mediaObject; }
				set { _mediaObject = value; }
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (UnsupportedImageTypeException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server encounters a file it does not recognize as
	///   a valid media object (e.g. image, video, audio, etc.). This may be because the file is a type that
	///   is disabled, or it may have an unrecognized file extension and the allowUnspecifiedMimeTypes configuration
	///   setting is false.
	/// </summary>
	[Serializable]
	public class UnsupportedMediaObjectTypeException : Exception
	{
		[NonSerialized] private UnsupportedMediaObjectTypeExceptionState _state;

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		public UnsupportedMediaObjectTypeException()
			: base(Resources.UnsupportedMediaObjectType_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedMediaObjectTypeException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="filePath">
		///   The full filepath to the file that is not recognized as a valid media object
		///   (ex: C:\inetpub\wwwroot\gs\mediaobjects\myvacation\utah\bikingslickrock.jpg).
		/// </param>
		public UnsupportedMediaObjectTypeException(string filePath)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.UnsupportedMediaObjectType_Ex_Msg2, Path.GetExtension(filePath)))
		{
			_state.MediaObjectFilePath = filePath;

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="file">The FileInfo object that is not recognized as a valid media object.</param>
		public UnsupportedMediaObjectTypeException(FileSystemInfo file)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.UnsupportedMediaObjectType_Ex_Msg2, (file != null ? Path.GetExtension(file.FullName) : String.Empty)))
		{
			_state.MediaObjectFilePath = (file == null ? Resources.DefaultFilename : file.FullName);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="file">The FileInfo object that is not recognized as a valid media object.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnsupportedMediaObjectTypeException(FileSystemInfo file, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.UnsupportedMediaObjectType_Ex_Msg2, (file != null ? Path.GetExtension(file.FullName) : String.Empty)), innerException)
		{
			_state.MediaObjectFilePath = (file == null ? Resources.DefaultFilename : file.FullName);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate a file that is not recognized as a valid media object supported by
		///   Gallery Server. This may be because the file is a type that is disabled, or it may have an
		///   unrecognized file extension and the allowUnspecifiedMimeTypes configuration setting is false.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected UnsupportedMediaObjectTypeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the filename of the media object that is causing the exception. Example:
		///   C:\mypics\vacation\grandcanyon.jpg, grandcanyon.jpg
		/// </summary>
		public string MediaObjectFilePath
		{
			get { return _state.MediaObjectFilePath; }
		}

		/// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct UnsupportedMediaObjectTypeExceptionState : ISafeSerializationData
		{
			private string _mediaObjectFilePath;

			/// <summary>
			///   Gets the filename of the media object that is causing the exception. Example:
			///   C:\mypics\vacation\grandcanyon.jpg, grandcanyon.jpg
			/// </summary>
			public string MediaObjectFilePath
			{
				get { return _mediaObjectFilePath; }
				set { _mediaObjectFilePath = value; }
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (UnsupportedMediaObjectTypeException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server cannot find a directory.
	/// </summary>
	[Serializable]
	public class InvalidMediaObjectDirectoryException : Exception
	{
		[NonSerialized] private InvalidMediaObjectDirectoryExceptionState _state;

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		public InvalidMediaObjectDirectoryException()
			: base(Resources.InvalidMediaObjectsDirectory_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		/// <param name="mediaObjectPath">The media object directory that is not valid.</param>
		public InvalidMediaObjectDirectoryException(string mediaObjectPath)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidMediaObjectsDirectory_Ex_Msg2, mediaObjectPath))
		{
			_state.MediaObjectPath = (mediaObjectPath ?? Resources.DefaultDirectoryPath);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		/// <param name="mediaObjectPath">The media object directory that is not valid.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidMediaObjectDirectoryException(string mediaObjectPath, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.InvalidMediaObjectsDirectory_Ex_Msg2, mediaObjectPath), innerException)
		{
			_state.MediaObjectPath = (mediaObjectPath ?? Resources.DefaultDirectoryPath);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception to indicate an invalid media objects directory.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidMediaObjectDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the media object directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
		/// </summary>
		public string MediaObjectPath
		{
			get { return _state.MediaObjectPath; }
		}

		/// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct InvalidMediaObjectDirectoryExceptionState : ISafeSerializationData
		{
			private string _mediaObjectPath;

			/// <summary>
			///   Gets the media object directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
			/// </summary>
			public string MediaObjectPath
			{
				get { return _mediaObjectPath; }
				set { _mediaObjectPath = value; }
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (InvalidMediaObjectDirectoryException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server is unable to write to, or delete from, a directory.
	/// </summary>
	[Serializable]
	public class CannotWriteToDirectoryException : Exception
	{
		[NonSerialized] private CannotWriteToDirectoryExceptionState _state;

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		public CannotWriteToDirectoryException()
			: base(Resources.CannotWriteToDirectory_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be written to.</param>
		public CannotWriteToDirectoryException(string directoryPath)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.CannotWriteToDirectory_Ex_Msg2, directoryPath))
		{
			_state.DirectoryPath = (directoryPath ?? Resources.DefaultDirectoryPath);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be written to.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotWriteToDirectoryException(string directoryPath, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.CannotWriteToDirectory_Ex_Msg2, directoryPath), innerException)
		{
			_state.DirectoryPath = (directoryPath ?? Resources.DefaultDirectoryPath);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to write to, or delete from, a directory.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotWriteToDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
		/// </summary>
		public string DirectoryPath
		{
			get { return _state.DirectoryPath; }
		}

		/// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct CannotWriteToDirectoryExceptionState : ISafeSerializationData
		{
			private string _directoryPath;

			/// <summary>
			///   Gets the directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
			/// </summary>
			public string DirectoryPath
			{
				get { return _directoryPath; }
				set { _directoryPath = value; }
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (CannotWriteToDirectoryException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server is unable to read from a directory.
	/// </summary>
	[Serializable]
	public class CannotReadFromDirectoryException : Exception
	{
		[NonSerialized] private CannotReadFromDirectoryExceptionState _state;

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		public CannotReadFromDirectoryException()
			: base(Resources.CannotReadFromDirectory_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be read from.</param>
		public CannotReadFromDirectoryException(string directoryPath)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.CannotReadFromDirectory_Ex_Msg2, directoryPath))
		{
			_state.DirectoryPath = (directoryPath ?? Resources.DefaultDirectoryPath);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		/// <param name="directoryPath">The directory that cannot be read from.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotReadFromDirectoryException(string directoryPath, Exception innerException)
			: base(String.Format(CultureInfo.CurrentCulture, Resources.CannotReadFromDirectory_Ex_Msg2, directoryPath), innerException)
		{
			_state.DirectoryPath = (directoryPath ?? Resources.DefaultDirectoryPath);

			// In response to SerializeObjectState, we need to provide any state to serialize with the exception.  In this 
			// case, since our state is already stored in an ISafeSerializationData implementation, we can just provide that.
			SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState(_state);
		}

		/// <summary>
		///   Throws an exception when Gallery Server is unable to read from a directory.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotReadFromDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///   Gets the directory that cannot be read from. Example: C:\inetput\wwwroot\mediaobjects
		/// </summary>
		public string DirectoryPath
		{
			get { return _state.DirectoryPath; }
		}

		/// <summary>
		///   Stores any custom state for this exception and enables the serialization of this state.
		/// </summary>
		[Serializable]
		private struct CannotReadFromDirectoryExceptionState : ISafeSerializationData
		{
			private string _directoryPath;

			/// <summary>
			///   Gets the directory that cannot be written to. Example: C:\inetput\wwwroot\mediaobjects
			/// </summary>
			public string DirectoryPath
			{
				get { return _directoryPath; }
				set { _directoryPath = value; }
			}

			/// <summary>
			///   Completes the deserialization.
			/// </summary>
			/// <param name="obj">The obj.</param>
			void ISafeSerializationData.CompleteDeserialization(object obj)
			{
				// This method is called when deserialization of the exception is complete.
				// Since the exception simply contains an instance of the exception state object, we can repopulate it 
				// here by just setting its instance field to be equal to this deserialized state instance.
				var exception = (CannotReadFromDirectoryException)obj;
				exception._state = this;
			}
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server encounters a query string parameter it does not recognize.
	/// </summary>
	[Serializable]
	public class UnexpectedQueryStringException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate an unexpected query string value.
		/// </summary>
		public UnexpectedQueryStringException()
			: base(Resources.UnexpectedQueryString_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected query string value.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public UnexpectedQueryStringException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected query string value.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnexpectedQueryStringException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected query string value.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected UnexpectedQueryStringException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when Gallery Server encounters form data in a web page it does not recognize.
	/// </summary>
	[Serializable]
	public class UnexpectedFormValueException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate an unexpected form data in a web page.
		/// </summary>
		public UnexpectedFormValueException()
			: base(Resources.UnexpectedFormData_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected form data in a web page.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public UnexpectedFormValueException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected form data in a web page.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public UnexpectedFormValueException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected form data in a web page.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected UnexpectedFormValueException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid gallery server role is referenced, or one is attempted to be created
	///   with invalid parameters.
	/// </summary>
	[Serializable]
	public class InvalidGalleryServerRoleException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		public InvalidGalleryServerRoleException()
			: base(Resources.InvalidGalleryServerRole_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidGalleryServerRoleException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidGalleryServerRoleException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid gallery server role is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidGalleryServerRoleException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when an invalid user is referenced, or one is attempted to be created
	///   with invalid parameters.
	/// </summary>
	[Serializable]
	public class InvalidUserException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		public InvalidUserException()
			: base(Resources.InvalidUser_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public InvalidUserException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public InvalidUserException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when an invalid user is referenced, or one is attempted to be created
		///   with invalid parameters.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected InvalidUserException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user attempts to begin a synchronization when another one is already
	///   in progress.
	/// </summary>
	[Serializable]
	public class SynchronizationInProgressException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		public SynchronizationInProgressException()
			: base(Resources.SynchronizationInProgress_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public SynchronizationInProgressException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public SynchronizationInProgressException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested synchronization cannot be started because another one is
		///   in progress.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected SynchronizationInProgressException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user requests the cancellation of an in-progress synchronization.
	/// </summary>
	[Serializable]
	public class SynchronizationTerminationRequestedException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
		/// </summary>
		public SynchronizationTerminationRequestedException()
			: base(Resources.SynchronizationTerminationRequested_Ex_Msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public SynchronizationTerminationRequestedException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public SynchronizationTerminationRequestedException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate when a user requests the cancellation of an in-progress synchronization.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected SynchronizationTerminationRequestedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user tries to transfer (either by moving or copying)
	///   an album to one of its own directories, whether direct or nested. For example,
	///   a user cannot copy an album from D:\gs_store\folder1 to D:\gs_store\folder1\folder2.
	/// </summary>
	[Serializable]
	public class CannotTransferAlbumToNestedDirectoryException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		public CannotTransferAlbumToNestedDirectoryException()
			: base(Resources.CannotTransferAlbumToNestedDirectoryException_Ex_msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public CannotTransferAlbumToNestedDirectoryException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotTransferAlbumToNestedDirectoryException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the requested move or copy album command cannot take place because the destination
		///   album is a child album of the album we are copying, or is the same album as the one we are copying.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotTransferAlbumToNestedDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>
	///   The exception that is thrown when a user tries to move a directory but the operating system
	///   won't allow it. This can happen if the user is viewing the contents of the directory in Windows Explorer.
	/// </summary>
	[Serializable]
	public class CannotMoveDirectoryException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		public CannotMoveDirectoryException()
			: base(Resources.CannotMoveDirectoryException_Ex_msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public CannotMoveDirectoryException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotMoveDirectoryException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate the application is unable to move a directory on the hard drive due to
		///   a restriction by the operating system.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotMoveDirectoryException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	///// <summary>
	///// The exception that is thrown when a user attempts to upload a file(s) whose size exceeds
	///// the maximum request length (specified in web.config).
	///// </summary>
	//[Serializable]
	//public class MaxRequestLengthExceededException : Exception
	//{
	//  /// <summary>
	//  /// Throws an exception to indicate the user attempted to upload a file(s) whose size exceeds
	//  /// the maximum request length.
	//  /// </summary>
	//  public MaxRequestLengthExceededException() 
	//    : base("File(s) cannot be uploaded: The maximum upload size has been exceeded.") { }

	//  /// <summary>
	//  /// Throws an exception to indicate the user attempted to upload a file(s) whose size exceeds
	//  /// the maximum request length.
	//  /// </summary>
	//  /// <param name="msg">A message that describes the error.</param>
	//  public MaxRequestLengthExceededException(string msg)
	//    : base(msg) { }

	//  /// <summary>
	//  /// Throws an exception to indicate the user attempted to upload a file(s) whose size exceeds
	//  /// the maximum request length.
	//  /// </summary>
	//  /// <param name="maxRequestLength">The maximum allowed size of an upload request, in bytes.</param>
	//  /// <param name="actualRequestLength">The actual size of the upload request, in bytes.</param>
	//  public MaxRequestLengthExceededException(int maxRequestLength, long actualRequestLength)
	//    : base(String.Format(System.Globalization.CultureInfo.CurrentCulture, "The maximum upload size "
	//    + "has been exceeded. The total size of the upload was {0} KB, which exceeds the limit of "
	//    + "{1} KB specified in web.config (or machine.config, if not "
	//    + "specified in web.config). Either reduce the size of your file(s) or, if you are the "
	//    + "administrator, adjust the value of maxRequestLength in web.config.", actualRequestLength, maxRequestLength))
	//  { }

	//  /// <summary>
	//  /// Throws an exception to indicate the user attempted to upload a file(s) whose size exceeds
	//  /// the maximum request length.
	//  /// </summary>
	//  /// <param name="maxRequestLength">The maximum allowed size of an upload request, in bytes.</param>
	//  /// <param name="actualRequestLength">The actual size of the upload request, in bytes.</param>
	//  /// <param name="innerException">The exception that is the cause of the current exception. If the 
	//  /// innerException parameter is not a null reference, the current exception is raised in a catch
	//  /// block that handles the inner exception.</param>
	//  public MaxRequestLengthExceededException(int maxRequestLength, long actualRequestLength, Exception innerException)
	//    : base(String.Format(System.Globalization.CultureInfo.CurrentCulture, "The maximum upload size "
	//    + "has been exceeded. The total size of the upload was {0} KB, which exceeds the limit of "
	//    + "{1} KB specified in web.config (or machine.config, if not "
	//    + "specified in web.config). Either reduce the size of your file(s) or, if you are the "
	//    + "administrator, adjust the value of maxRequestLength in web.config.", actualRequestLength, maxRequestLength), innerException)
	//  {}

	//  /// <summary>
	//  /// Throws an exception to indicate the user attempted to upload a file(s) whose size exceeds
	//  /// the maximum request length.
	//  /// </summary>
	//  /// <param name="msg">A message that describes the error.</param>
	//  /// <param name="innerException">The exception that is the cause of the current exception. If the 
	//  /// innerException parameter is not a null reference, the current exception is raised in a catch
	//  /// block that handles the inner exception.</param>
	//  public MaxRequestLengthExceededException(string msg, Exception innerException)
	//    : base(msg, innerException) { }

	//  /// <summary>
	//  /// Throws an exception to indicate the user attempted to upload a file(s) whose size exceeds
	//  /// the maximum request length.
	//  /// </summary>
	//  /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object 
	//  /// data about the exception being thrown.</param>
	//  /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual 
	//  /// information about the source or destination.</param>
	//  protected MaxRequestLengthExceededException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
	//    : base(info, context) {}
	//}
}