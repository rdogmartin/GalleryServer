using System;
using System.Globalization;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Entity;
using TreeView = GalleryServer.Web.Entity.TreeView;

namespace GalleryServer.Web.Controls
{
  /// <summary>
  /// A user control that renders an album treeview.
  /// </summary>
  public partial class albumtreeview : GalleryUserControl
  {
    #region Private Fields

    private Entity.TreeView _tv;
    private IIntegerCollection _checkedAlbumIds;
    private string _securityPermissionParm;
    private IGalleryCollection _galleries;
    private string _treeViewTheme;
    private string _treeDataJson;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets a reference to the <see cref="TreeView" /> instance within this user control.
    /// </summary>
    public Entity.TreeView TreeView
    {
      get
      {
        if (_tv == null)
        {
          _tv = new Entity.TreeView();
        }

        return _tv;
      }
    }

    /// <summary>
    /// Gets or sets the client ID of the HTML DOM node that contains the treeview structure. When not specified, 
    /// the value is auto-generated. Users of this control can access the client-side treeview object with code
    /// like this: <code>var tv = $("#&lt;%= tvUC.TreeViewClientId %&gt;");</code>
    /// </summary>
    public string TreeViewClientId
    {
      get
      {
        object viewStateValue = ViewState["TvClientId"];

        return (viewStateValue != null ? viewStateValue.ToString() : String.Concat(this.GalleryPage.ClientID, "_tvContainer"));
      }
      set
      {
        ViewState["TvClientId"] = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether more than one checkbox can be selected at a time in the treeview.
    /// The default value is false. This property should be set before calling DataBind().
    /// </summary>
    public bool AllowMultiCheck
    {
      get
      {
        object viewStateValue = ViewState["AllowMultiSelect"];
        bool allowMultiSelect;
        if ((viewStateValue != null) && (Boolean.TryParse(viewStateValue.ToString(), out allowMultiSelect)))
          return allowMultiSelect;
        else
          return false;
      }
      set
      {
        ViewState["AllowMultiSelect"] = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a checkbox is to be rendered for each album in the treeview. The default value
    /// is true. This property should be set before calling DataBind().
    /// </summary>
    public bool EnableCheckboxPlugin
    {
      get
      {
        object viewStateValue = ViewState["EnableCheckboxPlugin"];
        bool enableCheckbox;
        if ((viewStateValue != null) && (Boolean.TryParse(viewStateValue.ToString(), out enableCheckbox)))
          return enableCheckbox;
        else
          return true;
      }
      set
      {
        Invalidate();
        ViewState["EnableCheckboxPlugin"] = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the album specified in the <see cref="AlbumId" /> property is to be included in the 
    /// output or whether only the children are to be rendered. This property is ignored when <see cref="AlbumId" /> has not been
    /// assigned a value. For example, when this property is <c>true</c> and <see cref="AlbumId" /> is the root album, the generated 
    /// true includes a node for the root album and its children. If this property is <c>false</c> only a collection of nodes 
    /// representing the root album's children are generated. Defaults to <c>true</c>. Although this property is ignored when 
    /// <see cref="Galleries" /> is assigned instead of <see cref="AlbumId" />, in that situation the top level albums are rendered 
    /// as if this property were set to <c>true</c>.
    /// </summary>

    public bool IncludeAlbum
    {
      get
      {
        object viewStateValue = ViewState["IncludeAlbum"];
        bool includeAlbum;
        if ((viewStateValue != null) && (Boolean.TryParse(viewStateValue.ToString(), out includeAlbum)))
          return includeAlbum;
        else
          return true;
      }
      set
      {
        Invalidate();
        ViewState["IncludeAlbum"] = value;
      }
    }


    /// <summary>
    /// Gets or sets the base relative or absolute URL to invoke when a tree node is clicked. Leave this value as null or set to 
    /// an empty string when no navigation is desired. The album ID of the selected album is passed to the 
    /// URL as the query string parameter "aid". Example: "Gallery.aspx, http://site.com/gallery.aspx"
    /// </summary>
    public string NavigateUrl
    {
      get
      {
        object viewStateValue = ViewState["NavigateUrl"];

        return (viewStateValue != null ? viewStateValue.ToString() : String.Empty);
      }
      set
      {
        Invalidate();
        ViewState["NavigateUrl"] = value;
      }
    }

    /// <summary>
    /// Gets or sets a value to be prepended to the root album titles in the treeview. The default value is <see cref="String.Empty" />.
    /// May contain the placeholder values {GalleryId} and/or {GalleryDescription}. If present, the placeholders are replaced by the 
    /// action values during databinding. This property should be set before calling DataBind(). Example: "Gallery {GalleryDescription}: "
    /// </summary>
    public string RootNodesPrefix
    {
      get
      {
        object viewStateValue = ViewState["RootAlbumPrefix"];

        return (viewStateValue != null ? viewStateValue.ToString() : String.Empty);
      }
      set
      {
        Invalidate();
        ViewState["RootAlbumPrefix"] = value;
      }
    }

    /// <summary>
    /// Gets or sets the galleries to be rendered in the treeview. If not explicitly set, this defaults to the current gallery.
    /// If the <see cref="AlbumId" /> property is assigned, this property is ignored.
    /// </summary>
    /// <value>The galleries to be rendered in the treeview.</value>
    public IGalleryCollection Galleries
    {
      get
      {
        if (_galleries == null)
        {
          _galleries = new GalleryCollection();

          _galleries.Add(Factory.LoadGallery(this.GalleryPage.GalleryId));
        }

        return _galleries;
      }
      set
      {
        Invalidate();
        _galleries = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating the top level album to render. When not specified, the <see cref="Galleries" /> property determines
    /// the root albums to be rendered. This property should be set before calling DataBind().
    /// </summary>
    /// <value>The top level album to render.</value>
    public int AlbumId
    {
      get
      {
        object viewStateValue = ViewState["AlbumId"];
        int rootAlbumId;
        if ((viewStateValue != null) && (Int32.TryParse(viewStateValue.ToString(), out rootAlbumId)))
          return rootAlbumId;
        else
          return int.MinValue;
      }
      set
      {
        Invalidate();
        ViewState["AlbumId"] = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user is required to select an album from the treeview.
    /// Default is false.
    /// </summary>
    public bool RequireAlbumSelection
    {
      get
      {
        object viewStateValue = ViewState["RequireAlbumSelection"];
        bool requireAlbumSelection;
        if ((viewStateValue != null) && (Boolean.TryParse(viewStateValue.ToString(), out requireAlbumSelection)))
          return requireAlbumSelection;
        else
          return true;
      }
      set
      {
        Invalidate();
        ViewState["RequireAlbumSelection"] = value;
      }
    }

    /// <summary>
    /// Gets or sets the security permission the logged on user must have in order for an album to be displayed in the
    /// treeview. It may be a single value or some combination of valid enumeration values.
    /// </summary>
    public SecurityActions RequiredSecurityPermissions
    {
      get
      {
        var viewStateValue = (ViewState["RequiredSecurityPermissions"] ?? String.Empty).ToString();

        SecurityActions secActions;
        
        if (Enum.TryParse<SecurityActions>(viewStateValue, out secActions) && SecurityActionEnumHelper.IsValidSecurityAction(secActions))
        {
          return secActions;
        }
        else
        {
          return SecurityActions.NotSpecified;
        }
      }
      set
      {
        if (!SecurityActionEnumHelper.IsValidSecurityAction(value))
          throw new ArgumentException("Invalid SecurityActions enumeration value.");

        Invalidate();

        ViewState["RequiredSecurityPermissions"] = value;
      }
    }

    /// <summary>
    /// Gets a string representing the RequiredSecurityPermission property that can be used as a querystring parameter.
    /// Ex: "&amp;secaction=3"
    /// </summary>
    private string SecurityPermissionQueryStringParm
    {
      get
      {
        if (String.IsNullOrEmpty(this._securityPermissionParm))
        {
          if (SecurityActionEnumHelper.IsValidSecurityAction(this.RequiredSecurityPermissions))
          {
            this._securityPermissionParm = String.Format(CultureInfo.CurrentCulture, "&secaction={0}", (int)this.RequiredSecurityPermissions);
          }
        }

        return this._securityPermissionParm;
      }
    }

    /// <summary>
    /// Gets a list of the checked treeview nodes in the treeview.
    /// </summary>
    public object[] CheckedNodes
    {
      get
      {
        return null;
      }
    }

    ///// <summary>
    ///// Gets a collection of the "highest" checked nodes.
    ///// </summary>
    //public IIntegerCollection TopLevelCheckedAlbumIds
    //{
    //  get
    //  {
    //    //TreeViewNode[] checkedNodes = tv.CheckedNodes;
    //    IIntegerCollection checkedNodeIds = new IntegerCollection();

    //    //foreach (TreeViewNode node in checkedNodes)
    //    //{
    //    //  if (IsTopLevelCheckedNode(node))
    //    //  {
    //    //    checkedNodeIds.Add(Convert.ToInt32(node.Value, CultureInfo.InvariantCulture));
    //    //  }
    //    //}

    //    return checkedNodeIds;
    //  }
    //}

    /// <summary>
    /// A reference to the collection of album IDs whose associated checkboxes are to be selected, checked, and made visible.
    /// In a postback this property is initially populated with the collection of selected albums made by the user.
    /// </summary>
    public IIntegerCollection SelectedAlbumIds
    {
      get
      {
        if (this._checkedAlbumIds == null)
        {
          this._checkedAlbumIds = new IntegerCollection();

          if (IsPostBack)
          {
            foreach (string albumIdString in hdnCheckedAlbumIds.Value.Split(new char[] { ',' }))
            {
              int albumId;
              if (Int32.TryParse(albumIdString, out albumId))
              {
                this._checkedAlbumIds.Add(albumId);
              }
            }
          }
        }

        return this._checkedAlbumIds;
      }
      set
      {
        Invalidate();
        this._checkedAlbumIds = value;
      }
    }

    /// <summary>
    /// Gets or sets the number of hierarchical levels of albums to include in the tree. Defaults to two. Currently supports values
    /// of one or two.
    /// </summary>
    public int NumberOfLevels
    {
      get
      {
        object viewStateValue = ViewState["NumberOfLevels"];
        int numLevels;
        if ((viewStateValue != null) && (Int32.TryParse(viewStateValue.ToString(), out numLevels)))
          return numLevels;
        else
          return 2;
      }
      set
      {
        Invalidate();
        ViewState["NumberOfLevels"] = value;
      }
    }

    /// <summary>
    /// During a postback, gets a read-only instance of the album selected by the user or, during an initial page load, the album assigned to the first item
    /// in the <see cref="SelectedAlbumIds" /> collection.
    /// </summary>
    /// <value>The selected (checked) album in the treeview.</value>
    /// <exception cref="NotSupportedException">Thrown when <see cref="AllowMultiCheck" /> is <c>true</c>. In multi checkbox
    /// mode, use <see cref="SelectedAlbumIds" /> to access the selected items.</exception>
    public IAlbum SelectedAlbum
    {
      get
      {
        if (AllowMultiCheck)
        {
          throw new NotSupportedException("It is invalid to access the SelectedAlbum property when AllowMultiCheck is true. Use SelectedAlbumIds instead.");
        }

        if (SelectedAlbumIds.Count == 1)
        {
          try
          {
            return AlbumController.LoadAlbumInstance(SelectedAlbumIds[0]);
          }
          catch (InvalidAlbumException)
          {
            return null;
          }
        }
        else
        {
          return null;
        }
      }
    }

    /// <summary>
    /// Gets or sets the theme to use for the treeview. This value is used to generate the CSS class name that
    /// is applied to the HTML DOM element that contains the treeview. For example, if this property is 'gsp', the
    /// containing DOM element will have a class named 'jstree-gsp'. Default value is 'gsp'.
    /// </summary>
    /// <value>The theme to use for the treeview.</value>
    public string TreeViewTheme
    {
      get
      {
        if (String.IsNullOrEmpty(_treeViewTheme))
        {
          _treeViewTheme = "gsp";
        }

        return _treeViewTheme;
      }
      set
      {
        Invalidate();
        _treeViewTheme = value;
      }
    }

    /// <summary>
    /// Gets a JSON-formatted string of data that can be assigned to the data property of a 
    /// jsTree jQuery instance.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown when one or more business rules
    /// are violated.</exception>
    /// <value>
    /// A string formatted as JSON.
    /// </value>
    private string TreeDataJson
    {
      get { return _treeDataJson ?? (_treeDataJson = GetTreeData()); }
    }

    #endregion

    #region Protected Events

    /// <summary>
    /// Handles the Load event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
      //ConfigureControlsEveryTime();

      //string json = LoadTree();
    }

    /// <summary>
    /// Handles the PreRender event of the Page control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void Page_PreRender(object sender, EventArgs e)
    {
      RegisterJavascript();
    }

    #endregion

    #region Public Methods

    ///// <summary>
    ///// Render the treeview to two levels - the root album and its direct children. If the AlbumIdsToCheck property
    ///// has items in its collection, make sure every album in the collection is rendered, no matter how deep in the album heirarchy 
    ///// they may be. If the albumToSelect parameter is specified, then make sure this album is rendered and 
    ///// selected/checked, no matter how deep in the hierarchy it may be.
    ///// </summary>
    //public void BindTreeView()
    //{
    //  //BindTreeView(null);
    //}

    ///// <summary>
    ///// Render the treeview to two levels - the root album and its direct children. If the <see cref="CheckedAlbumIds" /> property
    ///// has items in its collection, make sure every album in the collection is rendered, no matter how deep in the album heirarchy 
    ///// they may be. If the <paramref name="albumToSelect" /> parameter is specified, then make sure this album is rendered and 
    ///// selected/checked, no matter how deep in the hierarchy it may be.
    ///// </summary>
    ///// <param name="albumToSelect">An album to be selected, checked, and made visible. The treeview is automatically expanded as
    ///// needed to ensure this album is visible.</param>
    //public void BindTreeView(IAlbum albumToSelect)
    //{
    //  //this._albumToSelect = albumToSelect;

    //  //DataBindTreeView();

    //  //this._albumToSelect = null;

    //  //this.AlbumIdsToCheck.Clear();
    //}

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets a JSON-formatted string of data that can be assigned to the data property of a 
    /// jsTree jQuery instance.
    /// </summary>
    /// <returns>A string formatted as JSON.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when one or more business rules
    /// are violated.</exception>
    private string GetTreeData()
    {
      #region Validation

      if (!this.AllowMultiCheck && this.SelectedAlbumIds.Count > 1)
      {
        throw new InvalidOperationException("The property AllowMultiCheck must be false when multiple album IDs have been assigned to the property SelectedAlbumIds.");
      }

      if (!SecurityActionEnumHelper.IsValidSecurityAction(this.RequiredSecurityPermissions))
      {
        throw new InvalidOperationException("The property GalleryServer.Web.Controls.albumtreeview.RequiredSecurityPermissions must be assigned before the TreeView can be rendered.");
      }

      #endregion

      TreeViewOptions tvOptions = new TreeViewOptions()
      {
        SelectedAlbumIds = SelectedAlbumIds,
        NumberOfLevels = NumberOfLevels,
        NavigateUrl = NavigateUrl,
        EnableCheckboxPlugin = EnableCheckboxPlugin,
        IncludeAlbum = IncludeAlbum,
        RequiredSecurityPermissions = RequiredSecurityPermissions,
        AlbumId = AlbumId,
        RootNodesPrefix = RootNodesPrefix,
        Galleries = Galleries
      };

      Entity.TreeView tv = AlbumTreeViewBuilder.GetAlbumsAsTreeView(tvOptions);

      //if (_nodesToCheckIds == null)
      //	_nodesToCheckIds = new List<string>(tv.NodesToCheckIdArray.Count);

      //_nodesToCheckIds.AddRange(tv.NodesToCheckIdArray);

      return tv.ToJson();
    }

    private void RegisterJavascript()
    {
      string script = String.Format(CultureInfo.InvariantCulture, @"
(function ($) {{
  $(document).ready(function() {{
    var options = {{
      allowMultiSelect: {1},
      checkedAlbumIdsHiddenFieldClientId: '{2}',
      theme: '{3}',
      requiredSecurityPermissions: {4},
      navigateUrl: '{5}',
      enableCheckboxPlugin: {6}
    }};
    $('#{0}').gsTreeView({7}, options);
  }});
}})(jQuery);
",
 TreeViewClientId, // 0
 AllowMultiCheck.ToString().ToLowerInvariant(), // 1
 hdnCheckedAlbumIds.ClientID, // 2
 TreeViewTheme, // 3
 (int)RequiredSecurityPermissions, // 4
 NavigateUrl, // 5
 EnableCheckboxPlugin.ToString().ToLowerInvariant(), // 6
 TreeDataJson // 7
 );
      this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Concat(this.GalleryPage.ClientID, "_tvScript"), script, true);
    }

    private void Invalidate()
    {
      _treeDataJson = null;
    }

    #endregion
  }
}