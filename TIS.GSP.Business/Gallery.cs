using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Business.Properties;
using GalleryServer.Data;
using GalleryServer.Events.CustomExceptions;

namespace GalleryServer.Business
{
    /// <summary>
    /// Represents a gallery within Gallery Server.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Gallery ID = {_id}")]
    public class Gallery : IGallery, IComparable
    {
        #region Private Fields

        private int _id;
        private int _rootAlbumId = int.MinValue;
        private string _description;
        private DateTime _creationDate;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the unique identifier for this gallery.
        /// </summary>
        /// <value>The unique identifier for this gallery.</value>
        public int GalleryId
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
        /// </summary>
        /// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
        public bool IsNew
        {
            get
            {
                return (_id == int.MinValue);
            }
        }

        /// <summary>
        /// Gets or sets the description for this gallery.
        /// </summary>
        /// <value>The description for this gallery.</value>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the date this gallery was created.
        /// </summary>
        /// <value>The date this gallery was created.</value>
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { _creationDate = value; }
        }

        /// <summary>
        /// Gets the ID of the root album of this gallery.
        /// </summary>
        /// <value>The ID of the root album of this gallery</value>
        public int RootAlbumId
        {
            get
            {
                if (_rootAlbumId == int.MinValue)
                {
                    // The root album is the item in the Albums dictionary with the most number of child albums.
                    int maxCount = int.MinValue;
                    foreach (KeyValuePair<int, List<int>> kvp in FlattenedAlbums)
                    {
                        if (kvp.Value.Count > maxCount)
                        {
                            maxCount = kvp.Value.Count;
                            _rootAlbumId = kvp.Key;
                        }
                    }
                }
                return _rootAlbumId;
            }
        }

        /// <summary>
        /// Gets or sets a thread-safe dictionary containing a list of album IDs (key) and the flattened list of
        /// all child album IDs within each album. The list includes the album identified in the key.
        /// </summary>
        /// <value>An instance of Dictionary&lt;int, List&lt;int&gt;&gt;.</value>
        public ConcurrentDictionary<int, List<int>> FlattenedAlbums { get; set; }

        /// <summary>
        /// Gets or sets a thread-safe dictionary containing a list of album IDs (key) and the hierarchical path of the root
        /// album to the album specified in the key, but does not include album identified in the key.
        /// </summary>
        /// <value>An instance of ConcurrentDictionary&lt;int&gt;, List&lt;int&gt;&gt;.</value>
        public ConcurrentDictionary<int, List<int>> AlbumHierarchies { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Gallery"/> class.
        /// </summary>
        public Gallery()
        {
            this._id = int.MinValue;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        /// <returns>Returns a deep copy of this instance.</returns>
        public IGallery Copy()
        {
            IGallery galleryCopy = new Gallery();

            galleryCopy.GalleryId = this.GalleryId;
            galleryCopy.Description = this.Description;
            galleryCopy.CreationDate = this.CreationDate;

            galleryCopy.FlattenedAlbums = new ConcurrentDictionary<int, List<int>>();
            foreach (KeyValuePair<int, List<int>> kvp in this.FlattenedAlbums)
            {
                galleryCopy.FlattenedAlbums.TryAdd(kvp.Key, new List<int>(kvp.Value));
            }

            galleryCopy.AlbumHierarchies = new ConcurrentDictionary<int, List<int>>();
            foreach (KeyValuePair<int, List<int>> kvp in this.AlbumHierarchies)
            {
                galleryCopy.AlbumHierarchies.TryAdd(kvp.Key, new List<int>(kvp.Value));
            }

            return galleryCopy;
        }

        /// <summary>
        /// Persist this gallery instance to the data store.
        /// </summary>
        public void Save()
        {
            bool isNew = IsNew;

            using (var repo = new GalleryRepository())
            {
                if (IsNew)
                {
                    var galleryDto = new GalleryDto { Description = Description, DateAdded = CreationDate };
                    repo.Add(galleryDto);
                    repo.Save();
                    _id = galleryDto.GalleryId;
                }
                else
                {
                    var galleryDto = repo.Find(GalleryId);

                    if (galleryDto != null)
                    {
                        galleryDto.Description = Description;
                        repo.Save();
                    }
                    else
                    {
                        throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Cannot save gallery: No existing gallery with Gallery ID {0} was found in the database.", GalleryId));
                    }
                }
            }

            // For new galleries, configure it and then trigger the created event.
            if (isNew)
            {
                Validate();

                Factory.ClearGalleryCache(); // Needed so LoadGalleries(), called by AddDefaultRolesToRoleAlbumTable(), pulls new gallery from data store

                AddDefaultRolesToRoleAlbumTable();
            }

            Factory.ClearAllCaches();
        }

        /// <summary>
        /// Permanently delete the current gallery from the data store, including all related records. This action cannot
        /// be undone.
        /// </summary>
        public void Delete()
        {
            //Factory.GetDataProvider().Gallery_Delete(this);
            OnBeforeDeleteGallery();

            // Cascade delete relationships should take care of any related records not deleted in OnBeforeDeleteGallery.
            using (var repo = new GalleryRepository())
            {
                var galleryDto = repo.Find(GalleryId);
                if (galleryDto != null)
                {
                    // Delete gallery. Cascade delete rules in DB will delete related records.
                    repo.Delete(galleryDto);
                    repo.Save();
                }
            }

            Factory.ClearAllCaches();
        }

        /// <summary>
        /// Configure the gallery by verifying that a default set of
        /// records exist in the relevant tables (Album, GallerySetting, MimeTypeGallery, Role_Album, UiTemplate,
        /// UiTemplateAlbum). No changes are made to the file system as part of this operation. This method does not overwrite 
        /// existing data, but it does insert missing data. This function can be used during application initialization to validate 
        /// the data integrity for a gallery. For example, if the user has added a record to the MIME types or template gallery 
        /// settings tables, this method will ensure that the new records are associated with this gallery.
        /// </summary>
        public void LoadData()
        {
            // Reset the sync table.
            ConfigureSyncTable();

            AssignAlbumListProperties();
        }

        /// <summary>
        /// Inspect the database for missing records; inserting if necessary.
        /// </summary>
        public void Validate()
        {
            // Step 1: Check for missing gallery settings, copying them from the template settings if necessary.
            var needToClearCache = ConfigureGallerySettingsTable();

            // Step 2: Create a new set of gallery MIME types (do nothing if already present).
            needToClearCache = ConfigureMimeTypeGalleryTable() | needToClearCache;

            // Step 3: Create the root album if necessary.
            var rootAlbumDto = ConfigureAlbumTable();

            // Step 4: For each role with AllowAdministerSite permission, add a corresponding record in gs_Role_Album giving it 
            // access to the root album.
            needToClearCache = ConfigureRoleAlbumTable(rootAlbumDto.AlbumId) | needToClearCache;

            // Step 5: Validate the UI templates.
            needToClearCache = ConfigureUiTemplateTable() | needToClearCache;
            needToClearCache = ConfigureUiTemplateAlbumTable(rootAlbumDto) | needToClearCache;

            if (needToClearCache)
            {
                Factory.ClearAllCaches();
            }
        }

        /// <summary>
        /// Verify there are gallery settings for the current gallery that match every template gallery setting, creating any
        /// if necessary.
        /// </summary>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        private bool ConfigureGallerySettingsTable()
        {
            var foundTmplGallerySettings = false;
            var needToClearCache = false;
            using (var repo = new GallerySettingRepository())
            {
                //repo.Load();
                var gallerySettingNamesInGallery = repo.Where(gs => gs.FKGalleryId == GalleryId).Select(gs => gs.SettingName).ToList();

                // Loop through each template gallery setting.
                foreach (var gsTmpl in repo.Where(g => g.Gallery.IsTemplate))
                {
                    foundTmplGallerySettings = true;
                    //if (!repo.Local.Any(gs => gs.SettingName == gsTmpl.SettingName && gs.FKGalleryId == GalleryId))
                    //if (!repo.Where(gs => gs.SettingName == gsTmpl.SettingName && gs.FKGalleryId == GalleryId).Any())
                    if (!gallerySettingNamesInGallery.Contains(gsTmpl.SettingName))
                    {
                        // This gallery is missing an entry for a gallery setting. Create one by copying it from the template gallery.
                        repo.Add(new GallerySettingDto()
                        {
                            FKGalleryId = GalleryId,
                            SettingName = gsTmpl.SettingName,
                            SettingValue = gsTmpl.SettingValue
                        });

                        needToClearCache = true;
                    }
                }

                repo.Save();
            }

            if (!foundTmplGallerySettings)
            {
                // If there weren't *any* template gallery settings, insert the seed data. Generally this won't be necessary, but it
                // can help recover from certain conditions, such as when a SQL Server connection is accidentally specified without
                // the MultipleActiveResultSets keyword (or it was false). In this situation the galleries are inserted but an error 
                // prevents the remaining data from being inserted. Once the user corrects this and tries again, this code can run to
                // finish inserting the seed data.
                using (var ctx = new GalleryDb())
                {
                    SeedController.InsertSeedData(ctx);
                }
                Factory.ValidateGalleries();
            }

            return needToClearCache;
        }

        /// <summary>
        /// Verify there is a MIME type/gallery mapping for the current gallery for every MIME type, creating any
        /// if necessary.
        /// </summary>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        private bool ConfigureMimeTypeGalleryTable()
        {
            var defaultEnabledExtensions = new List<string> { ".jpg", ".jpeg" };
            var needToClearCache = false;

            using (var repoMt = new MimeTypeRepository())
            {
                using (var repoMtg = new MimeTypeGalleryRepository())
                {
                    // Get MIME types that don't have a match in the MIME Type Gallery table
                    foreach (var mtDto in repoMt.Where(mt => mt.MimeTypeGalleries.All(mtg => mtg.FKGalleryId != GalleryId)))
                    {
                        repoMtg.Add(new MimeTypeGalleryDto()
                        {
                            FKGalleryId = GalleryId,
                            FKMimeTypeId = mtDto.MimeTypeId,
                            IsEnabled = defaultEnabledExtensions.Contains(mtDto.FileExtension)
                        });

                        needToClearCache = true;
                    }

                    repoMtg.Save();
                }
            }

            return needToClearCache;
        }

        /// <summary>
        /// Verify the current gallery has a root album, creating one if necessary. The root album is returned.
        /// </summary>
        /// <returns>An instance of <see cref="AlbumDto" />.</returns>
        private AlbumDto ConfigureAlbumTable()
        {
            using (var repo = new AlbumRepository())
            {
                var rootAlbumDto = repo.Where(a => a.FKGalleryId == GalleryId && a.FKAlbumParentId == null).FirstOrDefault();

                if (rootAlbumDto == null)
                {
                    rootAlbumDto = new AlbumDto
                    {
                        FKGalleryId = GalleryId,
                        FKAlbumParentId = null,
                        DirectoryName = String.Empty,
                        ThumbnailMediaObjectId = 0,
                        SortByMetaName = MetadataItemName.DateAdded,
                        SortAscending = true,
                        Seq = 0,
                        DateAdded = DateTime.UtcNow,
                        CreatedBy = GlobalConstants.SystemUserName,
                        LastModifiedBy = GlobalConstants.SystemUserName,
                        DateLastModified = DateTime.UtcNow,
                        OwnedBy = String.Empty,
                        OwnerRoleName = String.Empty,
                        IsPrivate = false,
                        Metadata = new Collection<MetadataDto>
                        {
                            new MetadataDto {MetaName = MetadataItemName.Caption, Value = Resources.Root_Album_Default_Summary},
                            new MetadataDto {MetaName = MetadataItemName.Title, Value = Resources.Root_Album_Default_Title}
                        }
                    };

                    repo.Add(rootAlbumDto);
                    repo.Save();
                }

                return rootAlbumDto;
            }
        }

        /// <summary>
        /// Verify there is a site admin role/album mapping for the root album in the current gallery, creating one
        /// if necessary.
        /// </summary>
        /// <param name="albumId">The album ID of the root album in the current gallery.</param>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        private static bool ConfigureRoleAlbumTable(int albumId)
        {
            var needToClearCache = false;

            using (var repoR = new RoleRepository())
            {
                using (var repoRa = new RoleAlbumRepository())
                {
                    // Get admin roles that aren't assigned to the album, then assign them
                    foreach (var rDto in repoR.Where(r => r.AllowAdministerSite && r.RoleAlbums.All(ra => ra.FKAlbumId != albumId)))
                    {
                        repoRa.Add(new RoleAlbumDto()
                        {
                            FKRoleName = rDto.RoleName,
                            FKAlbumId = albumId
                        });

                        needToClearCache = true;
                    }

                    repoRa.Save();
                }
            }

            return needToClearCache;
        }

        /// <summary>
        /// Add each role specified in <see cref="Interfaces.IGallerySettings.DefaultRolesForUser" /> to this gallery's root album.
        /// </summary>
        private void AddDefaultRolesToRoleAlbumTable()
        {
            var rootAlbumId = Factory.LoadRootAlbumInstance(GalleryId).Id;

            foreach (var defaultRole in Factory.LoadGallerySetting(GalleryId).DefaultRolesForUser)
            {
                var defaultRole2 = defaultRole;
                using (var repoRole = new RoleRepository())
                {
                    if (!repoRole.Where(r => r.RoleName == defaultRole2).Any())
                    {
                        continue; // We don't have a role with the name specified in the setting. Just skip it for now - RoleController.RemoveMissingRolesFromDefaultRolesForUsersSettings() will fix this when the maintenance routine runs.
                    }
                }

                using (var repoRa = new RoleAlbumRepository())
                {
                    if (!repoRa.Where(r => r.FKRoleName == defaultRole2 && r.FKAlbumId == rootAlbumId).Any())
                    {
                        // Add this role to the root album.
                        repoRa.Add(new RoleAlbumDto()
                        {
                            FKRoleName = defaultRole2,
                            FKAlbumId = rootAlbumId
                        });
                    }

                    repoRa.Save();
                }
            }
        }

        /// <summary>
        /// Verify there are UI templates for the current gallery that match every UI template associated with
        /// the template gallery, creating any if necessary.
        /// </summary>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        private bool ConfigureUiTemplateTable()
        {
            var needToClearCache = false;

            using (var repoUiTmpl = new UiTemplateRepository())
            {
                var ctx = repoUiTmpl.Context;

                //repoUiTmpl.Load();
                var uiTemplatesInGallery = repoUiTmpl.Where(ui => ui.FKGalleryId == GalleryId).Select(ui => ui.TemplateType + '|' + ui.Name).ToList();

                // Get the UI templates belonging to the template gallery. We have to do a join here because the data
                // model doesn't have a relationship. (Doing so would conflict with the relationship between
                // the UITemplateAlbum and Album tables.)
                var tmplForTmplGallery = from uiTmpl in ctx.UiTemplates join g in ctx.Galleries on uiTmpl.FKGalleryId equals g.GalleryId where g.IsTemplate select uiTmpl;

                // For each UI template, make sure one exists in the gallery
                foreach (var uiTmpl in tmplForTmplGallery)
                {
                    //if (!repoUiTmpl.Local.Any(ui => ui.TemplateType == uiTmpl.TemplateType && ui.FKGalleryId == GalleryId && ui.Name == uiTmpl.Name))
                    if (!uiTemplatesInGallery.Contains(uiTmpl.TemplateType + '|' + uiTmpl.Name))
                    {
                        // We need to add a UI template
                        repoUiTmpl.Add(new UiTemplateDto()
                        {
                            TemplateType = uiTmpl.TemplateType,
                            FKGalleryId = GalleryId,
                            Name = uiTmpl.Name,
                            Description = uiTmpl.Description,
                            HtmlTemplate = uiTmpl.HtmlTemplate,
                            ScriptTemplate = uiTmpl.ScriptTemplate
                        });

                        needToClearCache = true;
                    }
                }

                repoUiTmpl.Save();
            }

            return needToClearCache;
        }

        /// <summary>
        /// Verify there is a UI template/album mapping for the root album in the current gallery, creating them
        /// if necessary.
        /// </summary>
        /// <param name="rootAlbum">The root album.</param>
        /// <returns><c>true</c> if data was changed that necessitates reloading data from the data store, <c>false</c> otherwise.</returns>
        private static bool ConfigureUiTemplateAlbumTable(AlbumDto rootAlbum)
        {
            var needToClearCache = false;

            using (var repoUiTmpl = new UiTemplateRepository())
            {
                using (var repoUiTmplA = new UiTemplateAlbumRepository(repoUiTmpl.Context))
                {
                    // Make sure each template category has at least one template assigned to the root album.
                    // We do this with a union of two queries:
                    // 1. For categories where there is at least one album assignment, determine if at least one of
                    //    those assignments is the root album.
                    // 2. Find categories without any albums at all (this is necessary because the SelectMany() in the first
                    //    query won't return any categories that don't have related records in the template/album table).
                    var dtos = repoUiTmpl.Where(t => t.FKGalleryId == rootAlbum.FKGalleryId)
                                         .SelectMany(t => t.TemplateAlbums, (t, tt) => new { t.TemplateType, tt.FKAlbumId })
                                         .GroupBy(t => t.TemplateType)
                                         .Where(t => t.All(ta => ta.FKAlbumId != rootAlbum.AlbumId))
                                         .Select(t => t.Key)
                                         .Union(repoUiTmpl.Where(t => t.FKGalleryId == rootAlbum.FKGalleryId).GroupBy(t => t.TemplateType).Where(t => t.All(t2 => !t2.TemplateAlbums.Any())).Select(t => t.Key))
                                         ;

                    foreach (var dto in dtos)
                    {
                        // We have a template type without a root album. Find the default template and assign that one.
                        var dto1 = dto;
                        repoUiTmplA.Add(new UiTemplateAlbumDto()
                        {
                            FKUiTemplateId = repoUiTmpl.Where(t => t.FKGalleryId == rootAlbum.FKGalleryId && t.TemplateType == dto1 && t.Name.Equals("default", StringComparison.OrdinalIgnoreCase)).First().UiTemplateId,
                            FKAlbumId = rootAlbum.AlbumId
                        });

                        needToClearCache = true;
                    }

                    repoUiTmplA.Save();
                }
            }

            return needToClearCache;
        }

        /// <summary>
        /// Deletes the synchronization record belonging to the current gallery, but only when it's in an invalid state.
        /// When a sync is initiated it will be created.
        /// </summary>
        private void ConfigureSyncTable()
        {
            // It is very important to preserve the sync row if the state is InterruptedByAppRecycle!
            var syncStatesToDelete = new[] { SynchronizationState.SynchronizingFiles, SynchronizationState.PersistingToDataStore };

            using (var repo = new SynchronizeRepository())
            {
                var syncDto = repo.Where(s => s.FKGalleryId == GalleryId && syncStatesToDelete.Contains(s.SynchState)).FirstOrDefault();

                if (syncDto != null)
                {
                    repo.Delete(syncDto);
                    repo.Save();
                }
            }
        }

        /// <summary>
        /// Called before deleting a gallery. This function deletes the albums and any related records that won't be automatically
        /// deleted by the cascade delete relationship on the gallery table.
        /// </summary>
        private void OnBeforeDeleteGallery()
        {
            DeleteRootAlbum();

            DeleteUiTemplates();
        }

        /// <summary>
        /// Deletes the root album for the current gallery and all child items, but leaves the directories and original files on disk.
        /// This function also deletes the metadata for the root album, which will leave it in an invalid state. For this reason, 
        /// call this function *only* when also deleting the gallery the album is in.
        /// </summary>
        private void DeleteRootAlbum()
        {
            // Step 1: Delete the root album contents
            var rootAlbum = Factory.LoadRootAlbumInstance(GalleryId);
            rootAlbum.DeleteFromGallery();

            // Step 2: Delete all metadata associated with the root album of this gallery
            using (var repo = new MetadataRepository())
            {
                foreach (var dto in repo.Where(m => m.FKAlbumId == rootAlbum.Id))
                {
                    repo.Delete(dto);
                }
                repo.Save();
            }
        }

        /// <summary>
        /// Deletes the UI templates associated with the current gallery.
        /// </summary>
        private void DeleteUiTemplates()
        {
            using (var repo = new UiTemplateRepository())
            {
                foreach (var dto in repo.Where(m => m.FKGalleryId == GalleryId))
                {
                    repo.Delete(dto);
                }
                repo.Save();
            }
        }

        /// <summary>
        /// A simple class that holds an album's ID and its parent ID.
        /// </summary>
        [DebuggerDisplay("AlbumId {AlbumId}: AlbumParentId={AlbumParentId}")]
        private class AlbumTuple
        {
            public int AlbumId;
            public int? AlbumParentId;
        }

        /// <summary>
        /// Generate and assign values to the <see cref="IGallery.FlattenedAlbums" /> and <see cref="IGallery.AlbumHierarchies" /> properties.
        /// </summary>
        private void AssignAlbumListProperties()
        {
            var albumRelationships = GetAlbumRelationships();

            FlattenedAlbums = GenerateFlattenedAlbums(albumRelationships);
            AlbumHierarchies = GenerateAlbumHierarchies(albumRelationships);
        }

        /// <summary>
        /// Gets the album ID and its parent album ID for all albums in the current gallery. Guaranteed to not return null.
        /// </summary>
        /// <returns>An array of <see cref="AlbumTuple" /> instances.</returns>
        private AlbumTuple[] GetAlbumRelationships()
        {
            using (var repo = new AlbumRepository())
            {
                return repo.Where(a => a.FKGalleryId == GalleryId).Select(a => new AlbumTuple { AlbumId = a.AlbumId, AlbumParentId = a.FKAlbumParentId }).ToArray();
            }
        }

        /// <summary>
        /// Generates a dictionary of all album IDs in the gallery and, for each one, a list of album IDs from the root album down to its
        /// parent. Note that the album ID identified in the key is not included in the list stored in the Value property.
        /// </summary>
        /// <param name="albumRelationships">The album relationships for the current gallery.</param>
        /// <returns>ConcurrentDictionary&lt;System.Int32, List&lt;System.Int32&gt;&gt;.</returns>
        private static ConcurrentDictionary<int, List<int>> GenerateAlbumHierarchies(AlbumTuple[] albumRelationships)
        {
            var albumHierarchy = new ConcurrentDictionary<int, List<int>>();
            foreach (var album in albumRelationships)
            {
                List<int> hierarchy;
                if (album.AlbumParentId.HasValue)
                {
                    // Notice we call ToList to make a deep copy, since we don't want to change the collection already in the dictionary.
                    hierarchy = albumHierarchy.TryGetValue(album.AlbumParentId.Value, out hierarchy) ? hierarchy.ToList() : new List<int>();

                    hierarchy.Add(album.AlbumParentId.Value);
                }
                else
                {
                    hierarchy = new List<int>();
                }

                albumHierarchy.TryAdd(album.AlbumId, hierarchy);
            }

            return albumHierarchy;
        }

        /// <summary>
        /// Generates a dictionary of all album IDs in the gallery and, for each one, a flattened list of all album IDs contained in it, 
        /// recursively including all child IDs, not just those of its immediate children. The key is the album ID and the value is a list
        /// of all child album IDs contained in the album.
        /// </summary>
        /// <param name="albumRelationships">The album relationships for the current gallery.</param>
        /// <returns>ConcurrentDictionary&lt;System.Int32, List&lt;System.Int32&gt;&gt;.</returns>
        private static ConcurrentDictionary<int, List<int>> GenerateFlattenedAlbums(AlbumTuple[] albumRelationships)
        {
            var flatIds = new ConcurrentDictionary<int, List<int>>();
            var albumLookup = albumRelationships.ToLookup(a => a.AlbumParentId, v => v);

            int? rootAlbumParentId = null;

            // Get a reference to the root album
            AlbumTuple rootAlbum = albumLookup[rootAlbumParentId].FirstOrDefault();

            if (rootAlbum != null)
            {
                // Add the root album to our flat list and set up the child list
                flatIds.TryAdd(rootAlbum.AlbumId, new List<int> { rootAlbum.AlbumId });

                // Now add the children of the root album
                foreach (AlbumTuple albumTuple in albumLookup[rootAlbum.AlbumId])
                {
                    FlattenAlbum(albumTuple, albumLookup, flatIds, new List<int> { rootAlbum.AlbumId });
                }
            }

            return flatIds;
        }

        /// <summary>
        /// Add the <paramref name="album" /> to all albums in <paramref name="flatIds" /> where it is a child. Recursively
        /// process the album's children. The end result is a dictionary of album IDs (key) and the flattened list of all albums 
        /// each album contains (value).
        /// </summary>
        /// <param name="album">The album to flatten. This object is not modified.</param>
        /// <param name="hierarchicalIds">A lookup list where all albums (value) with a particular parent ID (key) can be quickly 
        /// found. This object is not modified.</param>
        /// <param name="flatIds">The flattened list of albums and their child albums. The <paramref name="album" /> and its
        /// children are added to this list.</param>
        /// <param name="currentAlbumFlatIds">The current hierarchy of album IDs we are processing. The function uses this to 
        /// know which items in <paramref name="flatIds" /> to update for each album.</param>
        private static void FlattenAlbum(AlbumTuple album, ILookup<int?, AlbumTuple> hierarchicalIds, ConcurrentDictionary<int, List<int>> flatIds, List<int> currentAlbumFlatIds)
        {
            // First time we get here, ID=2, ParentId=1
            flatIds.TryAdd(album.AlbumId, new List<int> { album.AlbumId });

            // For each album in the current hierarchy, find its match in flatIds and add the album to its list.
            foreach (int currentAlbumFlatId in currentAlbumFlatIds)
            {
                flatIds[currentAlbumFlatId].Add(album.AlbumId);
            }

            // Now add this album to the list so it will get updated when any children are processed.
            currentAlbumFlatIds.Add(album.AlbumId);

            foreach (AlbumTuple albumTuple in hierarchicalIds[album.AlbumId])
            {
                FlattenAlbum(albumTuple, hierarchicalIds, flatIds, new List<int>(currentAlbumFlatIds));
            }
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="obj"/> is not the same type as this instance. </exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            else
            {
                IGallery other = obj as IGallery;
                if (other != null)
                    return this.GalleryId.CompareTo(other.GalleryId);
                else
                    return 1;
            }
        }

        #endregion
    }
}
