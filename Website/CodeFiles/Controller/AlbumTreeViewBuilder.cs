using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.NullObjects;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Controller
{
    /// <summary>
    /// Contains functionality for creating <see cref="TreeView" /> instances.
    /// </summary>
    public class AlbumTreeViewBuilder
    {
        #region Fields

        private IGalleryServerRoleCollection _roles;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the options that specified the <see cref="TreeView" /> look and behavior.
        /// </summary>
        private TreeViewOptions Options { get; }

        /// <summary>
        /// Gets the tree this instance is constructing.
        /// </summary>
        private TreeView Tree { get; }

        /// <summary>
        /// Gets the roles belonging to the current user.
        /// </summary>
        private IGalleryServerRoleCollection Roles => _roles ?? (_roles = RoleController.GetGalleryServerRolesForUser());

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumTreeViewBuilder"/> class.
        /// </summary>
        /// <param name="tvOptions">The treeview options.</param>
        private AlbumTreeViewBuilder(TreeViewOptions tvOptions)
        {
            Options = tvOptions;
            Tree = new TreeView();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates a <see cref="TreeView" /> instance corresponding to the settings specified in <paramref name="tvOptions" />.
        /// </summary>
        /// <param name="tvOptions">The treeview options.</param>
        /// <returns>An instance of <see cref="TreeView" />.</returns>
        public static TreeView GetAlbumsAsTreeView(TreeViewOptions tvOptions)
        {
            return new AlbumTreeViewBuilder(tvOptions).Generate();
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Render the treeview with the first two levels of albums that are viewable to the logged on user.
        /// </summary>
        private TreeView Generate()
        {
            Tree.EnableCheckBoxPlugin = Options.EnableCheckboxPlugin;

            foreach (IAlbum rootAlbum in GetTopAlbums())
            {
                if (!Utils.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, rootAlbum.Id, rootAlbum.GalleryId, rootAlbum.IsPrivate, rootAlbum.IsVirtualAlbum))
                {
                    continue;
                }

                // Add root node.
                TreeNode rootNode = new TreeNode();

                string albumTitle = GetTopAlbumTitle(rootAlbum);
                rootNode.Text = albumTitle;
                rootNode.ToolTip = albumTitle;
                rootNode.Id = String.Concat("tv_", rootAlbum.Id.ToString(CultureInfo.InvariantCulture));
                rootNode.DataId = rootAlbum.Id.ToString(CultureInfo.InvariantCulture);
                rootNode.Expanded = (Options.NumberOfLevels > 1);

                if (rootAlbum.Parent is NullGalleryObject)
                {
                    rootNode.AddCssClass("jstree-root-node");
                }

                if (!String.IsNullOrEmpty(Options.NavigateUrl))
                {
                    var url = rootAlbum.IsVirtualAlbum ? Options.NavigateUrl : Utils.AddQueryStringParameter(Options.NavigateUrl, String.Concat("aid=", rootAlbum.Id.ToString(CultureInfo.InvariantCulture)));
                    rootNode.NavigateUrl = url;
                }

                // If it has a nav URL, it's always selectable & won't have a checkbox. If not, then it's selectable if the user has permission
                if (string.IsNullOrEmpty(rootNode.NavigateUrl))
                {
                    rootNode.Selectable = !rootAlbum.IsVirtualAlbum && Utils.IsUserAuthorized(Options.RequiredSecurityPermissions, RoleController.GetGalleryServerRolesForUser(), rootAlbum.Id, rootAlbum.GalleryId, rootAlbum.IsPrivate, SecurityActionsOption.RequireOne, rootAlbum.IsVirtualAlbum);

                    if (Options.EnableCheckboxPlugin)
                    {
                        rootNode.ShowCheckBox = rootNode.Selectable;
                    }
                }
                else
                {
                    rootNode.Selectable = true;
                }

                // Select and check this node if needed.
                if (Options.SelectedAlbumIds.Contains(rootAlbum.Id))
                {
                    rootNode.Selected = true;
                }

                Tree.Nodes.Add(rootNode);

                // Add the first level of albums below the root album.
                //var childAlbums = rootAlbum.GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated);
                if (rootAlbum.Parent is NullGalleryObject && Options.NumberOfLevels == 1)
                {
                    // We get here when building the gallery tree in a multi-gallery scenario. For perf reasons we don't want to call GetChildGalleryObjects.
                    rootNode.HasChildren = true;
                }
                else if (Options.NumberOfLevels == 1)
                {
                    rootNode.HasChildren = rootAlbum.GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated).Any();
                }
                else
                {
                    BindAlbumToTreeview(rootAlbum.GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated).ToSortedList(), rootNode, false);
                }

                // Only display the root node if it is selectable or we added any children to it; otherwise, remove it.
                //if (!rootNode.Selectable && rootNode.Nodes.Count == 0)
                //{
                //  Tree.Nodes.Remove(rootNode);
                //}
            }

            // Make sure all specified albums are visible and checked.
            try
            {
                foreach (var albumId in Options.SelectedAlbumIds.Where(id => id > int.MinValue))
                {
                    var album = AlbumController.LoadAlbumInstance(albumId);

                    if (Utils.IsUserAuthorized(Options.RequiredSecurityPermissions, Roles, album.Id, album.GalleryId, album.IsPrivate, SecurityActionsOption.RequireOne, album.IsVirtualAlbum))
                    {
                        BindSpecificAlbumToTreeview(album);
                    }
                }
            }
            catch (InvalidAlbumException ex)
            {
                // One of the albums we want to select doesn't exist. Log the event but otherwise continue on gracefully.
                if (!ex.Data.Contains("Tree_SelectedAlbum_Info"))
                {
                    ex.Data.Add("Tree_SelectedAlbum_Info", $"Album {ex.AlbumId} was one of the SelectedAlbumIds of the TreeViewOptions object. It may have been deleted by another user just before this code ran.");
                }

                AppEventController.LogError(ex);
            }

            return Tree;
        }

        /// <summary>
        /// Add the collection of albums to the specified treeview node.
        /// </summary>
        /// <param name="albums">The collection of albums to add the the treeview node.</param>
        /// <param name="parentNode">The treeview node that will receive child nodes representing the specified albums.</param>
        /// <param name="expandNode">Specifies whether the nodes should be expanded.</param>
        private void BindAlbumToTreeview(IEnumerable<IGalleryObject> albums, TreeNode parentNode, bool expandNode)
        {
            foreach (IAlbum album in albums)
            {
                TreeNode node = new TreeNode();
                string albumTitle = Utils.RemoveHtmlTags(album.Title);
                node.Text = albumTitle;
                node.ToolTip = albumTitle;
                node.Id = String.Concat("tv_", album.Id.ToString(CultureInfo.InvariantCulture));
                node.DataId = album.Id.ToString(CultureInfo.InvariantCulture);
                node.Expanded = expandNode;

                if (!String.IsNullOrEmpty(Options.NavigateUrl))
                {
                    node.NavigateUrl = Utils.AddQueryStringParameter(Options.NavigateUrl, String.Concat("aid=", album.Id.ToString(CultureInfo.InvariantCulture)));
                }

                // If it has a nav URL, it's always selectable & won't have a checkbox. If not, then it's selectable if the user has permission
                if (string.IsNullOrEmpty(node.NavigateUrl))
                {
                    node.Selectable = !album.IsVirtualAlbum && Utils.IsUserAuthorized(Options.RequiredSecurityPermissions, RoleController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, album.IsPrivate, SecurityActionsOption.RequireOne, album.IsVirtualAlbum);

                    if (Options.EnableCheckboxPlugin && !parentNode.ShowCheckBox)
                    {
                        node.ShowCheckBox = node.Selectable;
                    }
                }
                else
                {
                    node.Selectable = true;
                }

                if (album.GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated).Any())
                {
                    node.HasChildren = true;
                }

                // Select and check this node if needed.
                if (Options.SelectedAlbumIds.Contains(album.Id))
                {
                    node.Expanded = true;
                    node.Selected = true;
                    // Expand the child of the selected album.
                    BindAlbumToTreeview(album.GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated).ToSortedList(), node, false);
                }

                parentNode.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Bind the specified album to the treeview. This method assumes the treeview has at least the root node already
        /// built. The specified album can be at any level in the hierarchy. Nodes between the album and the existing top node
        /// are automatically created so that the full node path to the album is shown.
        /// </summary>
        /// <param name="album">An album to be added to the treeview.</param>
        private void BindSpecificAlbumToTreeview(IAlbum album)
        {
            if (Tree.FindNodeByDataId(album.Id.ToString(CultureInfo.InvariantCulture)) == null)
            {
                // Get a stack of albums that go from the current album to the top level album.
                // Once the stack is built we'll then add these albums to the treeview so that the full heirarchy
                // to the current album is shown.
                TreeNode existingParentNode;
                Stack<IAlbum> albumParents = GetAlbumsBetweenTopLevelNodeAndAlbum(album, out existingParentNode);

                if (existingParentNode == null)
                    return;

                BindSpecificAlbumToTreeview(existingParentNode, albumParents);
            }
        }

        /// <summary>
        /// Bind the hierarchical list of albums to the specified treeview node.
        /// </summary>
        /// <param name="existingParentNode">The treeview node to add the first album in the stack to.</param>
        /// <param name="albumParents">A list of albums where the first album should be a child of the specified treeview
        /// node, and each subsequent album is a child of the previous album.</param>
        private void BindSpecificAlbumToTreeview(TreeNode existingParentNode, Stack<IAlbum> albumParents)
        {
            // Assumption: The first album in the stack is a child of the existingParentNode node.
            existingParentNode.Expanded = true;

            // For each album in the heirarchy of albums to the current album, add the album and all its siblings to the 
            // treeview.
            foreach (IAlbum album in albumParents)
            {
                if (existingParentNode.Nodes.Count == 0)
                {
                    // Add all the album's siblings to the treeview.
                    var childAlbums = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(Convert.ToInt32(existingParentNode.DataId, CultureInfo.InvariantCulture)) { InflateChildObjects = true }).GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated).ToSortedList();
                    BindAlbumToTreeview(childAlbums, existingParentNode, false);
                }

                // Now find the album in the siblings we just added that matches the current album in the stack.
                // Set that album as the new parent and expand it.
                TreeNode nodeInAlbumHeirarchy = null;
                foreach (TreeNode node in existingParentNode.Nodes)
                {
                    if (node.DataId.Equals(album.Id.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal))
                    {
                        nodeInAlbumHeirarchy = node;
                        nodeInAlbumHeirarchy.Expanded = true;
                        break;
                    }
                }

                if (nodeInAlbumHeirarchy == null)
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Album ID {0} is not a child of the treeview node representing album ID {1}.", album.Id, Convert.ToInt32(existingParentNode.DataId, CultureInfo.InvariantCulture)));

                existingParentNode = nodeInAlbumHeirarchy;
            }
            existingParentNode.Expanded = false;
        }

        /// <summary>
        /// Retrieve a list of albums that are in the heirarchical path between the specified album and a node in the treeview.
        /// The node that is discovered as the ancestor of the album is assigned to the existingParentNode parameter.
        /// </summary>
        /// <param name="album">An album. This method navigates the ancestors of this album until it finds a matching node in the treeview.</param>
        /// <param name="existingParentNode">The existing node in the treeview that is an ancestor of the specified album is assigned to
        /// this parameter.</param>
        /// <returns>Returns a list of albums where the first album (the one returned by calling Pop) is a child of the album 
        /// represented by the existingParentNode treeview node, and each subsequent album is a child of the previous album.
        /// The final album is the same album specified in the album parameter.</returns>
        private Stack<IAlbum> GetAlbumsBetweenTopLevelNodeAndAlbum(IAlbum album, out TreeNode existingParentNode)
        {
            if (Tree.Nodes.Count == 0)
                throw new ArgumentException("The treeview must have at least one top-level node before calling the function GetAlbumsBetweenTopLevelNodeAndAlbum().");

            Stack<IAlbum> albumParents = new Stack<IAlbum>();
            albumParents.Push(album);

            IAlbum parentAlbum = (IAlbum)album.Parent;

            albumParents.Push(parentAlbum);

            // Navigate up from the specified album until we find an album that exists in the treeview. Remember,
            // the treeview has been built with the root node and the first level of albums, so eventually we
            // should find an album. If not, just return without showing the current album.
            while ((existingParentNode = Tree.FindNodeByDataId(parentAlbum.Id.ToString(CultureInfo.InvariantCulture))) == null)
            {
                parentAlbum = parentAlbum.Parent as IAlbum;

                if (parentAlbum == null)
                    break;

                albumParents.Push(parentAlbum);
            }

            // Since we found a node in the treeview we don't need to add the most recent item in the stack. Pop it off.
            albumParents.Pop();

            return albumParents;
        }

        /// <summary>
        /// Gets a list of top-level albums to display in the treeview. For a new tree these are the top-level nodes in the tree. For a node expansion
        /// callback event, these are the set of albums below the expanded node. If the <see cref="TreeViewOptions.AlbumId" /> property is assigned, 
        /// that album is returned and the <see cref="TreeViewOptions.Galleries" /> property is ignored.
        /// </summary>
        /// <returns>Returns a list of top-level albums to display in the treeview.</returns>
        private IEnumerable<IAlbum> GetTopAlbums()
        {
            List<IAlbum> rootAlbums = new List<IAlbum>(1);

            if (Options.AlbumId > 0)
            {
                var album = AlbumController.LoadAlbumInstance(new AlbumLoadOptions(Options.AlbumId) { InflateChildObjects = true });
                if (Options.IncludeAlbum)
                {
                    rootAlbums.Add(album);
                }
                else
                {
                    rootAlbums.AddRange(album.GetChildGalleryObjects(GalleryObjectType.Album, !Utils.IsAuthenticated).ToSortedList().Cast<IAlbum>());
                }
            }
            else if (Options.Galleries != null)
            {
                foreach (IGallery gallery in Options.Galleries)
                {
                    var rootAlbum = Factory.LoadRootAlbum(gallery.GalleryId, Roles, Utils.IsAuthenticated);

                    if (rootAlbum != null)
                        rootAlbums.Add(rootAlbum);
                }
            }

            return rootAlbums;
        }

        private string GetTopAlbumTitle(IAlbum rootAlbum)
        {
            IGallery gallery = Factory.LoadGallery(rootAlbum.GalleryId);
            string rootAlbumPrefix = Options.RootNodesPrefix.Replace("{GalleryId}", gallery.GalleryId.ToString(CultureInfo.InvariantCulture)).Replace("{GalleryDescription}", gallery.Description);
            return Utils.RemoveHtmlTags(String.Concat(rootAlbumPrefix, rootAlbum.Title));
        }

        #endregion

    }
}