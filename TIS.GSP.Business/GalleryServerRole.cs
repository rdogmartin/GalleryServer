using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Represents a role that encapsulates a set of permissions for one or more albums in Gallery Server. Each user
    /// is assigned to zero or more roles.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Role name = {_roleName}")]
    public class GalleryServerRole : IGalleryServerRole, IComparable
    {
        #region Private Fields

        private string _roleName;
        private bool _allowViewAlbumOrMediaObject;
        private bool _allowViewOriginalImage;
        private bool _allowAddMediaObject;
        private bool _allowAddChildAlbum;
        private bool _allowEditMediaObject;
        private bool _allowEditAlbum;
        private bool _allowDeleteMediaObject;
        private bool _allowDeleteChildAlbum;
        private bool _allowSynchronize;
        private bool _allowAdministerSite;
        private bool _allowAdministerGallery;
        private bool _hideWatermark;

        private readonly IGalleryCollection _galleries;
        private readonly IIntegerCollection _rootAlbumIds;
        private readonly IIntegerCollection _allAlbumIds;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a string that uniquely identifies the role.
        /// </summary>
        /// <value>The name of the role.</value>
        public string RoleName
        {
            get { return _roleName; }
            set { _roleName = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to view albums and media objects.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to view albums and media objects; otherwise, <c>false</c>.
        /// </value>
        public bool AllowViewAlbumOrMediaObject
        {
            get { return _allowViewAlbumOrMediaObject; }
            set { _allowViewAlbumOrMediaObject = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to view the original,
        /// high resolution version of an image. This setting applies only to images. It has no effect if there are no
        /// high resolution images in the album or albums to which this role applies.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to view the original,
        /// high resolution version of an image; otherwise, <c>false</c>.
        /// </value>
        public bool AllowViewOriginalImage
        {
            get { return _allowViewOriginalImage; }
            set { _allowViewOriginalImage = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to add media objects to an album.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to add media objects to an album; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAddMediaObject
        {
            get { return _allowAddMediaObject; }
            set { _allowAddMediaObject = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to create child albums.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to create child albums; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAddChildAlbum
        {
            get { return _allowAddChildAlbum; }
            set { _allowAddChildAlbum = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to edit a media object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to edit a media object; otherwise, <c>false</c>.
        /// </value>
        public bool AllowEditMediaObject
        {
            get { return _allowEditMediaObject; }
            set { _allowEditMediaObject = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to edit an album.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to edit an album; otherwise, <c>false</c>.
        /// </value>
        public bool AllowEditAlbum
        {
            get { return _allowEditAlbum; }
            set { _allowEditAlbum = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to delete media objects within an album.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to delete media objects within an album; otherwise, <c>false</c>.
        /// </value>
        public bool AllowDeleteMediaObject
        {
            get { return _allowDeleteMediaObject; }
            set { _allowDeleteMediaObject = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to delete child albums.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to delete child albums; otherwise, <c>false</c>.
        /// </value>
        public bool AllowDeleteChildAlbum
        {
            get { return _allowDeleteChildAlbum; }
            set { _allowDeleteChildAlbum = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has permission to synchronize an album.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has permission to synchronize an album; otherwise, <c>false</c>.
        /// </value>
        public bool AllowSynchronize
        {
            get { return _allowSynchronize; }
            set { _allowSynchronize = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user has administrative permission for all albums in the gallery
        /// associated with this role. This permission automatically applies to all albums in the gallery; it cannot be
        /// selectively applied.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user has administrative permission for all albums in the gallery associated with
        /// this role; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAdministerGallery
        {
            get { return _allowAdministerGallery; }
            set { _allowAdministerGallery = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user has administrative permission for all albums. This permission
        /// automatically applies to all albums; it cannot be selectively applied.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user has administrative permission for all albums; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAdministerSite
        {
            get { return _allowAdministerSite; }
            set { _allowAdministerSite = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user assigned to this role has a watermark applied to images.
        /// This setting has no effect if watermarks are not used. A true value means the user does not see the watermark;
        /// a false value means the watermark is applied.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the user assigned to this role has a watermark applied to images; otherwise, <c>false</c>.
        /// </value>
        public bool HideWatermark
        {
            get { return _hideWatermark; }
            set { _hideWatermark = value; }
        }

        /// <summary>
        /// Gets the list of all galleries to which this role applies. This property is dynamically populated based on the
        /// albums in the <see cref="RootAlbumIds"/> property. Calling the Save() method automatically reloads this
        /// property from the data store.
        /// </summary>
        /// <value>The list of all galleries to which this role applies.</value>
        public IGalleryCollection Galleries
        {
            get { return _galleries; }
        }

        /// <summary>
        /// Gets the list of all top-level album IDs for which this role applies. Does not include any descendants of the album.
        /// Note that adding or removing items to this list does not cause <see cref="AllAlbumIds" /> to be cleared out.
        /// </summary>
        /// <value>The list of all top-level album IDs for which this role applies.</value>
        public IIntegerCollection RootAlbumIds
        {
            get { return _rootAlbumIds; }
        }

        /// <summary>
        /// Gets the list of all album IDs for which this role applies. Includes all descendants of all applicable albums.
        /// </summary>
        /// <value>The list of all album IDs for which this role applies.</value>
        /// <exception cref="BusinessException">Thrown when <see cref="RootAlbumIds"/> has more than one item but the internal
        /// field for this property (_allAlbumIds) is empty.</exception>
        public IIntegerCollection AllAlbumIds
        {
            get
            {
                if ((_allAlbumIds.Count == 0) && (_rootAlbumIds.Count > 0))
                {
                    var galleries = Factory.LoadGalleries();

                    Inflate(galleries);

                    if (_allAlbumIds.Count == 0)
                    {
                        string msg = $"Invalid state of GalleryServerRole instance: The AllAlbumIds property has a count of zero but the RootAlbumIds has a count of {_rootAlbumIds.Count}. This occurred despite called the Inflate method.";
                        foreach (var gallery in galleries)
                        {
                            msg += $" Gallery {gallery.GalleryId} FlattenedAlbums Count: {gallery.FlattenedAlbums.Count}; AlbumHierarchies Count: {gallery.AlbumHierarchies.Count}.";
                        }

                        throw new BusinessException(msg);
                    }
                }

                return _allAlbumIds;
            }
            //set { _allAlbumIds = value; }
        }

        #endregion

        #region Constructors

        private GalleryServerRole() { } // Hide default constructor

        /// <summary>
        /// Create a GalleryServerRole instance corresponding to the specified parameters. Throws an exception if a role with the
        /// specified name already exists in the data store.
        /// </summary>
        /// <param name="roleName">A string that uniquely identifies the role.</param>
        /// <param name="allowViewAlbumOrMediaObject">A value indicating whether the user assigned to this role has permission to view albums
        /// and media objects.</param>
        /// <param name="allowViewOriginalImage">A value indicating whether the user assigned to this role has permission to view the original,
        /// high resolution version of an image. This setting applies only to images. It has no effect if there are no
        /// high resolution images in the album or albums to which this role applies.</param>
        /// <param name="allowAddMediaObject">A value indicating whether the user assigned to this role has permission to add media objects to an album.</param>
        /// <param name="allowAddChildAlbum">A value indicating whether the user assigned to this role has permission to create child albums.</param>
        /// <param name="allowEditMediaObject">A value indicating whether the user assigned to this role has permission to edit a media object.</param>
        /// <param name="allowEditAlbum">A value indicating whether the user assigned to this role has permission to edit an album.</param>
        /// <param name="allowDeleteMediaObject">A value indicating whether the user assigned to this role has permission to delete media objects within an album.</param>
        /// <param name="allowDeleteChildAlbum">A value indicating whether the user assigned to this role has permission to delete child albums.</param>
        /// <param name="allowSynchronize">A value indicating whether the user assigned to this role has permission to synchronize an album.</param>
        /// <param name="allowAdministerSite">A value indicating whether the user has administrative permission for all albums. This permission
        /// automatically applies to all albums across all galleries; it cannot be selectively applied.</param>
        /// <param name="allowAdministerGallery">A value indicating whether the user has administrative permission for all albums. This permission
        /// automatically applies to all albums in a particular gallery; it cannot be selectively applied.</param>
        /// <param name="hideWatermark">A value indicating whether the user assigned to this role has a watermark applied to images.
        /// This setting has no effect if watermarks are not used. A true value means the user does not see the watermark;
        /// a false value means the watermark is applied.</param>
        /// <returns>Returns a GalleryServerRole instance corresponding to the specified parameters.</returns>
        internal GalleryServerRole(string roleName, bool allowViewAlbumOrMediaObject, bool allowViewOriginalImage, bool allowAddMediaObject, bool allowAddChildAlbum, bool allowEditMediaObject, bool allowEditAlbum, bool allowDeleteMediaObject, bool allowDeleteChildAlbum, bool allowSynchronize, bool allowAdministerSite, bool allowAdministerGallery, bool hideWatermark)
        {
            this._roleName = roleName;
            this._allowViewAlbumOrMediaObject = allowViewAlbumOrMediaObject;
            this._allowViewOriginalImage = allowViewOriginalImage;
            this._allowAddMediaObject = allowAddMediaObject;
            this._allowAddChildAlbum = allowAddChildAlbum;
            this._allowEditMediaObject = allowEditMediaObject;
            this._allowEditAlbum = allowEditAlbum;
            this._allowDeleteMediaObject = allowDeleteMediaObject;
            this._allowDeleteChildAlbum = allowDeleteChildAlbum;
            this._allowSynchronize = allowSynchronize;
            this._allowAdministerSite = allowAdministerSite;
            this._allowAdministerGallery = allowAdministerGallery;
            this._hideWatermark = hideWatermark;

            this._galleries = new GalleryCollection();

            this._rootAlbumIds = new IntegerCollection();
            this._rootAlbumIds.Cleared += _rootAlbumIds_Cleared;

            this._allAlbumIds = new IntegerCollection();
        }

        #endregion

        #region Event Handlers

        void _rootAlbumIds_Cleared(object sender, EventArgs e)
        {
            // We need to smoke the all albums list whenever the list of root albums has been cleared.
            if (this._allAlbumIds != null)
                this._allAlbumIds.Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Populate the <see cref="AllAlbumIds"/> and <see cref="Galleries"/> properties based on the contents of
        /// <see cref="RootAlbumIds"/> and the flattened list of album IDs in <paramref name="galleries"/>.
        /// </summary>
        /// <param name="galleries">A list of all galleries in the current application. The <see cref="IGallery.FlattenedAlbums"/>
        /// property is used as a source for populating the <see cref="AllAlbumIds"/> and <see cref="Galleries"/> properties
        /// of the current instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="galleries" /> is null.</exception>
        public void Inflate(IGalleryCollection galleries)
        {
            if (galleries == null)
                throw new ArgumentNullException("galleries");

            // For each root album, get the list of flattened album IDs from the gallery (we don't know which gallery, so
            // iterate through them until you find the right one).
            foreach (int albumId in this.RootAlbumIds)
            {
                foreach (IGallery gallery in galleries)
                {
                    List<int> albumIds;
                    if (gallery.FlattenedAlbums.TryGetValue(albumId, out albumIds))
                    {
                        this.AddToAllAlbumIds(albumIds);

                        // If we haven't yet added this gallery, do so now.
                        if (!this.Galleries.Contains(gallery))
                        {
                            this.Galleries.Add(gallery);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Add the specified album to the list of all album IDs. This is used by data and business layer code to
        /// populate the list when it is instantiated or saved.
        /// </summary>
        /// <param name="albumId">The ID that uniquely identifies the album to add to the list.</param>
        public void AddToAllAlbumIds(int albumId)
        {
            this._allAlbumIds.Add(albumId);
        }

        /// <summary>
        /// Add the specified albums to the list of all album IDs. This is used by data and business layer code to
        /// populate the list when it is instantiated or saved.
        /// </summary>
        /// <param name="albumIds">The IDs that uniquely identify the albums to add to the list.</param>
        public void AddToAllAlbumIds(IEnumerable<int> albumIds)
        {
            this._allAlbumIds.AddRange(albumIds);
        }

        /// <summary>
        /// Clears the list of album IDs stored in the <see cref="AllAlbumIds"/> property.
        /// </summary>
        public void ClearAllAlbumIds()
        {
            this._allAlbumIds.Clear();
        }

        /// <summary>
        /// Persist this gallery server role to the data store. The list of top-level albums this role applies to, which is stored
        /// in the <see cref="RootAlbumIds"/> property, is also saved. If <see cref="RootAlbumIds"/> was modified, the caller must
        /// repopulate the <see cref="AllAlbumIds"/> and <see cref="Galleries"/> properties.
        /// </summary>
        public void Save()
        {
            //Factory.GetDataProvider().Role_Save(this);
            using (var repo = new RoleRepository())
            {
                repo.Save(this);
            }
        }

        /// <summary>
        /// Permanently delete this gallery server role from the data store, including the list of role/album relationships
        /// associated with this role.
        /// </summary>
        /// <remarks>This procedure only deletes it from the custom gallery server tables,
        /// not the ASP.NET role membership table(s). The web application code that invokes this procedure also
        /// uses the standard ASP.NET technique to delete the role from the membership table(s).</remarks>
        public void Delete()
        {
            //Factory.GetDataProvider().Role_Delete(this);
            using (var repo = new RoleRepository())
            {
                var roleDto = repo.Find(RoleName);
                if (roleDto != null)
                {
                    repo.Delete(roleDto);
                    repo.Save();
                }
            }
        }

        /// <summary>
        /// Creates a deep copy of this instance, including the Galleries, RootAlbumIds and AllAlbumIds properties.
        /// </summary>
        /// <returns>Returns a deep copy of this instance.</returns>
        public IGalleryServerRole Copy()
        {
            IGalleryServerRole role = Factory.CreateGalleryServerRoleInstance(String.Empty, AllowViewAlbumOrMediaObject, AllowViewOriginalImage,
                                                                              AllowAddMediaObject, AllowAddChildAlbum, AllowEditMediaObject, AllowEditAlbum,
                                                                              AllowDeleteMediaObject, AllowDeleteChildAlbum, AllowSynchronize,
                                                                              AllowAdministerSite, AllowAdministerGallery, HideWatermark);
            role.RoleName = RoleName;

            foreach (IGallery gallery in Galleries)
            {
                role.Galleries.Add(gallery.Copy());
            }

            role.AllAlbumIds.AddRange(AllAlbumIds);
            role.RootAlbumIds.AddRange(RootAlbumIds);

            return role;
        }

        /// <summary>
        /// Verify the role conforms to business rules. Specifically, if the role has administrative permissions
        /// (AllowAdministerSite = true or AllowAdministerGallery = true):
        /// 1. Make sure the role permissions - except HideWatermark - are set to true.
        /// 2. Make sure the root album IDs are a list containing the root album ID for each affected gallery.
        /// If anything needs updating, update the object and persist the changes to the data store. This helps keep the data store
        /// valid in cases where the user is directly editing the tables (for example, adding/deleting records from the gs_Role_Album table).
        /// </summary>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        public bool ValidateIntegrity()
        {
            if (AllowAdministerSite || AllowAdministerGallery)
            {
                return ValidateAdminRoleIntegrity();
            }

            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Verify the administrative role contains a list of the root albums for every affected gallery. This corrects potential data integrity
        /// situations, such as when a developer modifies the RoleAlbum table to give a site administrator access to a child album in a
        /// gallery. Since site admins, by definition, have permission to ALL albums in ALL galleries, we want to make sure the list of albums
        /// we are storing reflect this. Any problems with integrity are automatically corrected and persisted to the data store.
        /// </summary>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        private bool ValidateAdminRoleIntegrity()
        {
            // Test 1: Make sure all role permissions - except HideWatermark - are set to true.
            bool hasChanges = ValidateRoleAdminPermissions();

            // Test 2: Since admins always have complete access to all albums in a gallery (and site admins have access to all albums
            // in every gallery), admin roles should be assigned the root album for each relevant gallery. We verify this by getting the 
            // root album ID for each relevant gallery and then comparing them to the ones assigned to the role. If they are different, 
            // we update them and save.

            // Step 1: Get the list of root album IDs relevant to the role.
            List<int> rootAlbumIds = GetRootAlbumIdsForRole();

            // Step 2: Determine if the list of root album IDs is different than the list assigned to the role.
            bool rootAlbumsCountIsDifferent = (rootAlbumIds.Count != RootAlbumIds.Count);
            bool roleHasMissingAlbumId = false;

            foreach (int albumId in rootAlbumIds)
            {
                if (!RootAlbumIds.Contains(albumId))
                {
                    roleHasMissingAlbumId = true;
                    break;
                }
            }

            if (rootAlbumsCountIsDifferent || roleHasMissingAlbumId)
            {
                // Step 3: When the list is different, update the list assigned to the role.
                RootAlbumIds.Clear();
                RootAlbumIds.AddRange(rootAlbumIds);
                hasChanges = true;
            }

            // Step 4: Save changes if needed.
            if (hasChanges)
            {
                Save();
            }

            return hasChanges;
        }

        /// <summary>
        /// Verifies that admin roles have all applicable permissions, returning a value indicating whether any properties were updated. 
        /// Specifically, admin roles should have most sub permissions, such as adding and editing media objects. Does not modify the 
        /// "hide watermark" permission. The changes are made to the object but not persisted to the data store.
        /// </summary>
        /// <returns><c>true</c> if one or more properties were updated; otherwise <c>false</c>.</returns>
        private bool ValidateRoleAdminPermissions()
        {
            bool hasChanges = false;

            if (AllowAdministerSite || AllowAdministerGallery)
            {
                if (!AllowAddChildAlbum)
                {
                    AllowAddChildAlbum = true;
                    hasChanges = true;
                }
                if (!AllowAddMediaObject)
                {
                    AllowAddMediaObject = true;
                    hasChanges = true;
                }
                if (!AllowDeleteChildAlbum)
                {
                    AllowDeleteChildAlbum = true;
                    hasChanges = true;
                }
                if (!AllowDeleteMediaObject)
                {
                    AllowDeleteMediaObject = true;
                    hasChanges = true;
                }
                if (!AllowEditAlbum)
                {
                    AllowEditAlbum = true;
                    hasChanges = true;
                }
                if (!AllowEditMediaObject)
                {
                    AllowEditMediaObject = true;
                    hasChanges = true;
                }
                if (!AllowSynchronize)
                {
                    AllowSynchronize = true;
                    hasChanges = true;
                }
                if (!AllowViewAlbumOrMediaObject)
                {
                    AllowViewAlbumOrMediaObject = true;
                    hasChanges = true;
                }
                if (!AllowViewOriginalImage)
                {
                    AllowViewOriginalImage = true;
                    hasChanges = true;
                }
            }

            if (AllowAdministerSite)
            {
                // Site admins are also gallery admins.
                if (!AllowAdministerGallery)
                {
                    AllowAdministerGallery = true;
                    hasChanges = true;
                }
            }

            return hasChanges;
        }

        /// <summary>
        /// Gets a list of album IDs at the top of each gallery associated with the role. Returns values only when the
        /// role has a permission that affects an entire gallery (example: site admin or gallery admin). The IDs can be used to validate
        /// that the list of album IDs assigned to the role are stored in the most efficient manner.
        /// </summary>
        /// <returns>Returns a list of album IDs at the top of each gallery associated with the role.</returns>
        private List<int> GetRootAlbumIdsForRole()
        {
            List<int> rootAlbumIds = new List<int>(1);

            if (AllowAdministerSite)
            {
                // Site admins have permission to every gallery, so get root album ID of every gallery.
                foreach (IGallery gallery in Factory.LoadGalleries())
                {
                    rootAlbumIds.Add(gallery.RootAlbumId);
                }
            }
            else if (AllowAdministerGallery)
            {
                // Loop through each album ID associated with this role. Add the root album for each to our list, but don't duplicate any.
                foreach (int topAlbumId in RootAlbumIds)
                {
                    foreach (IGallery gallery in Factory.LoadGalleries())
                    {
                        if (gallery.FlattenedAlbums.ContainsKey(topAlbumId) && (!rootAlbumIds.Contains(gallery.RootAlbumId)))
                        {
                            rootAlbumIds.Add(gallery.RootAlbumId);
                            break;
                        }
                    }
                }
            }

            return rootAlbumIds;
        }


        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
        /// Less than zero: This instance is less than <paramref name="obj"/>. Zero: This instance is equal to <paramref name="obj"/>. 
        /// Greater than zero: This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="obj"/> is not the same type as this instance. </exception>
        public int CompareTo(object obj)
        {
            // Sort by role name.
            if (obj == null)
                return 1;
            else
            {
                GalleryServerRole other = (GalleryServerRole)obj;
                return String.Compare(this.RoleName, other.RoleName, StringComparison.CurrentCulture);
            }
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="GalleryServerRole"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return (this.RoleName.GetHashCode() ^ this.AllAlbumIds.GetHashCode()
                    ^ this.AllowAddChildAlbum.GetHashCode() ^ this.AllowAddMediaObject.GetHashCode()
                    ^ this.AllowAdministerSite.GetHashCode() ^ this.AllowDeleteChildAlbum.GetHashCode()
                    ^ this.AllowDeleteMediaObject.GetHashCode() ^ this.AllowEditAlbum.GetHashCode()
                    ^ this.AllowEditMediaObject.GetHashCode() ^ this.AllowSynchronize.GetHashCode()
                    ^ this.AllowViewAlbumOrMediaObject.GetHashCode() ^ this.AllowViewOriginalImage.GetHashCode()
                    ^ this.HideWatermark.GetHashCode() ^ this.AllowAdministerGallery.GetHashCode());
        }

        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IGalleryServerRole role = obj as IGalleryServerRole;
            if (role == null)
            {
                return false;
            }

            return (this.RoleName.Equals(role.RoleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines whether the specified <paramref name="role" /> is equal to this instance.
        /// </summary>
        /// <param name="role">The role to compare to this instance.</param>
        /// <returns><c>true</c> if the specified <paramref name="role" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(IGalleryServerRole role)
        {
            if (role == null)
            {
                return false;
            }

            return (this.RoleName.Equals(role.RoleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
