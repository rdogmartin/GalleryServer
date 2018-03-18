using System;
using System.Collections.Generic;
using System.Globalization;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.NullObjects;
using GalleryServer.Business.Properties;
using GalleryServer.Events;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{

	/// <summary>
	/// Verifies an album can be safely deleted.
	/// </summary>
	public class AlbumDeleteValidator
	{
		private readonly IAlbum _albumToDelete;
		private bool _validationFailure;
		private GalleryObjectDeleteValidationFailureReason _validationFailureReason;

		/// <summary>
		/// Gets a value indicating whether the album can be deleted. Call <see cref="Validate" /> before checking this property.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the album can be deleted; otherwise, <c>false</c>.
		/// </value>
		public bool CanBeDeleted
		{
			get { return !_validationFailure; }
		}

		/// <summary>
		/// Gets the message describing why an album cannot be deleted. Call <see cref="Validate" /> before checking this property.
		/// Returns null before the Validate method is invoked and when <see cref="CanBeDeleted" /> is <c>true</c>.
		/// </summary>
		/// <value>The message.</value>
		public GalleryObjectDeleteValidationFailureReason ValidationFailureReason
		{
			get { return _validationFailureReason; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AlbumDeleteValidator"/> class.
		/// </summary>
		/// <param name="albumToDelete">The album to delete.</param>
		public AlbumDeleteValidator(IAlbum albumToDelete)
		{
			_albumToDelete = albumToDelete;
		}

		/// <summary>
		/// Verifies that the album passed into the constructor can be safely deleted. This method causes the <see cref="CanBeDeleted" />
		/// and <see cref="ValidationFailureReason" /> properties to be set.
		/// </summary>
		public void Validate()
		{
			CheckForUserAlbumConflict();

			CheckForDefaultGalleryObjectConflict();
		}

		/// <summary>
		/// Checks the album to be deleted to see if it is specified as the user album container or if one of its children is the user
		/// album container. If user albums are disabled, no action is taken. If a problem is found, the member variables are updated
		/// with details.
		/// </summary>
		private void CheckForUserAlbumConflict()
		{
			IGallerySettings gallerySetting = Factory.LoadGallerySetting(_albumToDelete.GalleryId);

			if (!gallerySetting.EnableUserAlbum)
				return;

			IGalleryObject userAlbumParent;
			try
			{
				userAlbumParent = Factory.LoadAlbumInstance(gallerySetting.UserAlbumParentAlbumId);
			}
			catch (InvalidAlbumException ex)
			{
				// User album doesn't exist. Record the error and then return because there is no problem with deleting the current album.
				string galleryDescription = Factory.LoadGallery(gallerySetting.GalleryId).Description;
				string msg = String.Format(CultureInfo.CurrentCulture, Resources.Error_User_Album_Parent_Invalid_Ex_Msg, galleryDescription, _albumToDelete.Id);
				EventController.RecordError(new WebException(msg, ex), AppSetting.Instance, _albumToDelete.GalleryId, Factory.LoadGallerySettings());
				return;
			}

			// Test #1: Are we trying to delete the album that is specified as the user album parent album?
			if (userAlbumParent.Id == _albumToDelete.Id)
			{
				_validationFailure = true;
				_validationFailureReason = GalleryObjectDeleteValidationFailureReason.AlbumSpecifiedAsUserAlbumContainer;
				return;
			}

			// Test #2: Does the user album parent album exist somewhere below the album we want to delete?
			IGalleryObject albumParent = userAlbumParent.Parent;
			while (!(albumParent is NullGalleryObject))
			{
				if (albumParent.Id == _albumToDelete.Id)
				{
					_validationFailure = true;
					_validationFailureReason = GalleryObjectDeleteValidationFailureReason.AlbumContainsUserAlbumContainer;
					return;
				}
				albumParent = albumParent.Parent;
			}
		}

		/// <summary>
		/// If a default media object or album is specified, make sure it isn't contained in the album we want to delete.
		/// If a problem is found, the member variables are updated with details.
		/// </summary>
		private void CheckForDefaultGalleryObjectConflict()
		{
			if (_validationFailureReason != GalleryObjectDeleteValidationFailureReason.NotSet)
			{
				return; // We have already identified a validation failure, so just return so we don't overwrite it.
			}

			// Get a list of gallery control settings we need to test.
			List<IGalleryControlSettings> gcsList = new List<IGalleryControlSettings>();
			foreach (IGalleryControlSettings gcs in Factory.LoadGalleryControlSettings())
			{
				if ((gcs.GalleryId == _albumToDelete.GalleryId) && (gcs.AlbumId.HasValue || gcs.MediaObjectId.HasValue))
				{
					gcsList.Add(gcs);
				}
			}

			if (gcsList.Count == 0)
			{
				return; // Return, since there is nothing to validate
			}

			foreach (IGalleryControlSettings gcs in gcsList)
			{
				CheckForDefaultGalleryObjectAlbumConflict(gcs);

				if (_validationFailureReason == GalleryObjectDeleteValidationFailureReason.NotSet)
				{
					// We only bother checking the next item if we haven't yet encountered a validation failure.
					CheckForDefaultGalleryObjectMediaObjectConflict(gcs);
				}
			}
		}

		private void CheckForDefaultGalleryObjectAlbumConflict(IGalleryControlSettings gcs)
		{
			if (!gcs.AlbumId.HasValue)
			{
				return; // No default album is specified, so there is nothing to test.
			}

			// Test #1: If an album is specified as the default gallery object, is it the album we are deleting?
			if (gcs.AlbumId.Value == _albumToDelete.Id)
			{
				_validationFailure = true;
				_validationFailureReason = GalleryObjectDeleteValidationFailureReason.AlbumSpecifiedAsDefaultGalleryObject;
				return;
			}

			// Test #2: If an album is specified as the default gallery object, is it contained within the hierarchy of
			// the album we are deleting?
			IGalleryObject defaultGalleryAlbum = GetDefaultGalleryAlbum(gcs);

			if (defaultGalleryAlbum != null)
			{
				IGalleryObject albumParent = defaultGalleryAlbum.Parent;
				while (!(albumParent is NullGalleryObject))
				{
					if (albumParent.Id == _albumToDelete.Id)
					{
						_validationFailure = true;
						_validationFailureReason = GalleryObjectDeleteValidationFailureReason.AlbumContainsDefaultGalleryObjectAlbum;
						return;
					}
					albumParent = albumParent.Parent;
				}
			}
		}

		private void CheckForDefaultGalleryObjectMediaObjectConflict(IGalleryControlSettings gcs)
		{
			if (!gcs.MediaObjectId.HasValue)
			{
				return; // No default media object is specified, so there is nothing to test.
			}

			// If a media object is specified as the default gallery object, is it contained within the hierarchy of
			// the album we are deleting?
			IGalleryObject defaultGalleryMediaObject = GetDefaultGalleryMediaObject(gcs);

			if (defaultGalleryMediaObject != null)
			{
				IGalleryObject albumParent = defaultGalleryMediaObject.Parent;
				while (!(albumParent is NullGalleryObject))
				{
					if (albumParent.Id == _albumToDelete.Id)
					{
						_validationFailure = true;
						_validationFailureReason = GalleryObjectDeleteValidationFailureReason.AlbumContainsDefaultGalleryObjectMediaObject;
						return;
					}
					albumParent = albumParent.Parent;
				}
			}
		}

		private IGalleryObject GetDefaultGalleryMediaObject(IGalleryControlSettings gcs)
		{
			if (!gcs.MediaObjectId.HasValue)
			{
				return null; // We should never get here because the calling method should have already verified there is a value, but we'll be extra safe.
			}

			IGalleryObject defaultGalleryMediaObject = null;
			try
			{
				defaultGalleryMediaObject = Factory.LoadMediaObjectInstance(gcs.MediaObjectId.Value);
			}
			catch (InvalidMediaObjectException ex)
			{
				// Media object doesn't exist. This won't prevent us from deleting the album but we should note the issue, since
				// it can cause problems to specify a media object that doesn't exist for the default gallery object.
				string galleryDescription = Factory.LoadGallery(_albumToDelete.GalleryId).Description;
				string msg = String.Format(CultureInfo.CurrentCulture, Resources.Error_Default_Gallery_Object_MediaObject_Invalid_Ex_Msg, galleryDescription, _albumToDelete.Id);
				EventController.RecordError(new BusinessException(msg, ex), AppSetting.Instance, _albumToDelete.GalleryId, Factory.LoadGallerySettings());
			}

			return defaultGalleryMediaObject;
		}

		private IGalleryObject GetDefaultGalleryAlbum(IGalleryControlSettings gcs)
		{
			if (!gcs.AlbumId.HasValue)
			{
				return null; // We should never get here because the calling method should have already verified there is a value, but we'll be extra safe.
			}

			IGalleryObject defaultGalleryAlbum = null;
			try
			{
				defaultGalleryAlbum = Factory.LoadAlbumInstance(gcs.AlbumId.Value);
			}
			catch (InvalidAlbumException ex)
			{
				// Album doesn't exist. This won't prevent us from deleting the album but we should note the issue, since
				// it can cause problems to specify an album that doesn't exist for the default gallery object.
				string galleryDescription = Factory.LoadGallery(_albumToDelete.GalleryId).Description;
				string msg = String.Format(CultureInfo.CurrentCulture, Resources.Error_Default_Gallery_Object_Album_Invalid_Ex_Msg, galleryDescription, _albumToDelete.Id);
				EventController.RecordError(new BusinessException(msg, ex), AppSetting.Instance, _albumToDelete.GalleryId, Factory.LoadGallerySettings());
			}

			return defaultGalleryAlbum;
		}
	}
}
