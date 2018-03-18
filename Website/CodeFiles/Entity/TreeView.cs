using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using Newtonsoft.Json;

namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// Contains functionality for representing a tree. Can be JSON-serialized and then assigned to an instance of a 
  /// jQuery jsTree object.
  /// </summary>
  public class TreeView
  {
    #region Properties

    /// <summary>
    /// Gets or sets the nodes contained in the treeview. The nodes are a hierarchical, meaning if there are ten nodes
    /// in the treeview but only one at the root level, there will be one item in this collection. To access the remaining
    /// nodes, use the <see cref="TreeNode.Nodes" /> property of each <see cref="TreeNode" /> item. Guaranteed to
    /// not be null.
    /// </summary>
    /// <value>The nodes in the treeview.</value>
    public TreeNodeCollection Nodes { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether checkbox functionality is desired. The default value
    /// is <c>false</c>. When <c>false</c>, the property <see cref="TreeNode.ShowCheckBox" /> is ignored.
    /// </summary>
    /// <value><c>true</c> if checkbox functionality is desired; otherwise, <c>false</c>.</value>
    public bool EnableCheckBoxPlugin { get; set; }

    ///// <summary>
    ///// Gets a list of the client IDs of the nodes that are to be selected when the treeview is rendered. Guaranteed to
    ///// not be null.
    ///// </summary>
    //public List<String> NodesToCheckIdArray { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeView"/> class. The <see cref="Nodes" /> 
    /// property is initialized to an empty collection.
    /// </summary>
    public TreeView()
    {
      Nodes = new TreeNodeCollection(null);
      //NodesToCheckIdArray = new List<string>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Finds the node in the treeview having the specified data ID. The function searches recursively. Returns null
    /// if no matching item is found.
    /// </summary>
    /// <param name="id">The ID to search for.</param>
    /// <returns>Returns a <see name="TreeNode" /> if a match is found; otherwise returns null.</returns>
    public TreeNode FindNodeByDataId(string id)
    {
      return FindNodeByDataId(Nodes, id);
    }

    /// <summary>
    /// Serializes the current instance to JSON. The resulting string can be used as the data source for the jQuery 
    /// treeview widget.
    /// </summary>
    /// <returns>Returns the current instance as a JSON string.</returns>
    public string ToJson()
    {
      var nodes = Nodes.Select(n => n.NodeInternal).ToList();

      var json = JsonConvert.SerializeObject(nodes);

      // Tweak JSON so that jstree treats nodes with empty children as a lazy-loaded node. See remarks
      // for JsTreeNode.Nodes for more info.
      return json.Replace("\"children\":[]", "\"children\":true");

      // Old way:
      //var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(List<JsTreeNode>));
      //MemoryStream ms = new MemoryStream();

      //ser.WriteObject(ms, nodes);
      //return Encoding.UTF8.GetString(ms.ToArray());
    }

    #endregion

    #region Private Functions

    /// <summary>
    /// Finds the node in the <paramref name="nodes" /> collection having the specified data ID. The
    /// function searches recursively. Returns null if no matching item is found.
    /// </summary>
    /// <param name="nodes">The nodes to search.</param>
    /// <param name="id">The ID to search for.</param>
    /// <returns>Returns a <see name="TreeNode" /> if a match is found; otherwise returns null.</returns>
    private TreeNode FindNodeByDataId(IEnumerable<TreeNode> nodes, string id)
    {
      foreach (TreeNode node in nodes)
      {
        if (node.DataId.Equals(id, StringComparison.Ordinal))
        {
          return node;
        }
        else
        {
          TreeNode matchingNode = FindNodeByDataId(node.Nodes, id);

          if (matchingNode != null)
            return matchingNode;
        }
      }

      return null;
    }

    #endregion
  }

  /// <summary>
  /// Represents a node in a <see cref="TreeView" />.
  /// </summary>
  public class TreeNode
  {
    #region Private Fields

    private TreeView _treeView;
    private readonly JsTreeNode _node;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the treeview containing the current node.
    /// </summary>
    /// <value>The tree view.</value>
    public TreeView TreeView
    {
      get { return _treeView; }
      set { _treeView = value; }
    }

    /// <summary>
    /// Gets or sets the ID of the current node. This value is assigned to the id attribute of the li element generated in the HTML.
    /// </summary>
    /// <value>The ID of the current node.</value>
    public string Id
    {
      get { return _node.ListItemAttributes.Id; }
      set { _node.ListItemAttributes.Id = value; }
    }

    /// <summary>
    /// Gets or sets a piece of data for the current node. This value is assigned to the data-id attribute of the li 
    /// element generated in the HTML. For albums and media objects, specify the <see cref="IGalleryObject.Id" /> value.
    /// </summary>
    /// <value>A string representing a piece of data for the current node.</value>
    public string DataId
    {
      get { return _node.ListItemAttributes.DataId; }
      set { _node.ListItemAttributes.DataId = value; }
    }

    /// <summary>
    /// Gets or sets the text for the node.
    /// </summary>
    /// <value>The text.</value>
    public string Text
    {
      get { return _node.Text; }
      set { _node.Text = value; }
    }

    /// <summary>
    /// Gets or sets the tool tip for the node. This value is assigned to the title attribute of the hyperlink element
    /// generated in the HTML.
    /// </summary>
    /// <value>The tool tip.</value>
    public string ToolTip
    {
      get { return _node.HyperlinkAttributes.ToolTip; }
      set { _node.HyperlinkAttributes.ToolTip = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the current node should display a checkbox.
    /// </summary>
    /// <value><c>true</c> if a check box is to be rendered; otherwise, <c>false</c>.</value>
    public bool ShowCheckBox
    {
      get { return (!_node.ListItemAttributes.CssClasses.Contains("jstree-checkbox-hidden")); }
      set { _node.ListItemAttributes.AddCssClass((value ? String.Empty : "jstree-checkbox-hidden")); }
    }

    /// <summary>
    /// Gets the class attribute of the DOM element. When multiple classes exist, it returns a space-separated string.
    /// </summary>
    /// <value>A string.</value>
    /// <exception cref="NotImplementedException">Thrown when attempting to set this property. Instead, use the 
    /// <see cref="AddCssClass" /> or <see cref="RemoveCssClass" /> methods.</exception>
    public string CssClasses
    {
      get { return _node.ListItemAttributes.CssClasses; }
      private set { throw new NotImplementedException("Setter not implemented for property 'CssClasses' in class GalleryServer.Web.Entity.TreeNode. Use method AddCssClass or RemoveCssClass instead."); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this node has child objects that can be accessed in the data store.
    /// When this value is <c>true</c> and the generated HTML does not contain any child nodes, an AJAX callback
    /// is performed to retrieve the nodes. When <c>false</c>, the expand/collapse icon is not rendered and no
    /// callback is performed.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this node has child objects in the data store; otherwise, <c>false</c>.
    /// </value>
    public bool HasChildren
    {
      get { return _node.HasChildren; }
      set { _node.HasChildren = value; }
    }

    /// <summary>
    /// Gets or sets the base relative or absolute URL to invoke when a tree node is clicked. The album ID of the selected album
    /// is passed to the URL as the query string parameter "aid". Leave this value as null or set to 
    /// an empty string when no navigation is desired. Example: "Gallery.aspx, http://site.com/gallery.aspx"
    /// </summary>
    /// <value>The URL the user is to be sent to when the node is clicked.</value>
    public string NavigateUrl
    {
      get { return _node.HyperlinkAttributes.NavigationUrl; }
      set { _node.HyperlinkAttributes.NavigationUrl = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TreeNode"/> is expanded.
    /// </summary>
    /// <value><c>true</c> if expanded; otherwise, <c>false</c>.</value>
    public bool Expanded
    {
      get { return _node.State.IsExpanded; }
      set { _node.State.IsExpanded = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TreeNode"/> is selected.
    /// </summary>
    /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
    public bool Selected
    {
      get { return _node.State.IsSelected; }
      set { _node.State.IsSelected = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TreeNode"/> is selectable.
    /// </summary>
    /// <value><c>true</c> if selectable; otherwise, <c>false</c>.</value>
    public bool Selectable
    {
      get { return !_node.State.IsDisabled; }
      set { _node.State.IsDisabled = !value; }
    }

    /// <summary>
    /// Gets or sets the nodes contained within the current node. The nodes are a hierarchical. Guaranteed to
    /// not be null.
    /// </summary>
    /// <value>The nodes in the treeview.</value>
    public TreeNodeCollection Nodes { get; private set; }

    /// <summary>
    /// Gets a reference to the internal JsTreeNode object. This object is designed to that it can be JSON-serialized
    /// to a form expected by the jsTree jquery widget.
    /// </summary>
    /// <value>The internal JsTreeNode object.</value>
    public JsTreeNode NodeInternal
    {
      get { return _node; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeNode"/> class. The <see cref="Nodes" /> property is initialized 
    /// to an empty collection.
    /// </summary>
    public TreeNode()
    {
      _node = new JsTreeNode();
      Nodes = new TreeNodeCollection(this);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds the <paramref name="node" /> to the collection of <see cref="JsTreeNode" /> instances.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddInternalNode(JsTreeNode node)
    {
      _node.AddNode(node);
    }

    /// <summary>
    /// Adds the <paramref name="cssClass" /> to the class attribute of the current tree node. If the class
    /// is already specified, no action is taken.
    /// </summary>
    /// <param name="cssClass">The CSS class.</param>
    public void AddCssClass(string cssClass)
    {
      _node.ListItemAttributes.AddCssClass(cssClass);
    }

    /// <summary>
    /// Removes the <paramref name="cssClass" /> from the class attribute of the current tree node.
    /// </summary>
    /// <param name="cssClass">The CSS class.</param>
    public void RemoveCssClass(string cssClass)
    {
      _node.ListItemAttributes.RemoveCssClass(cssClass);
    }

    #endregion
  }

  /// <summary>
  /// Represents settings for manipulating the display of a <see cref="TreeView" />.
  /// </summary>
  public class TreeViewOptions
  {
    private int _numberOfLevels = 1;

    /// <summary>
    /// A reference to the collection of album IDs whose associated checkboxes are to be selected, checked, and made visible.
    /// </summary>
    /// <value>A collection of integers.</value>
    public IIntegerCollection SelectedAlbumIds { get; set; }

    /// <summary>
    /// Gets or sets the number of hierarchical levels of albums to include in the tree. Defaults to one. Currently supports values
    /// of one or two.
    /// </summary>
    public int NumberOfLevels
    {
      get
      {
        return _numberOfLevels;
      }
      set
      {
        if (value < 1 || value > 2)
        {
          throw new NotSupportedException($"The TreeViewOptions.NumberOfLevels must be a value of one or two. Instead, {value} was passed.");
        }

        _numberOfLevels = value;
      }
    }

    /// <summary>
    /// Gets or sets the base relative or absolute URL to invoke when a tree node is clicked. Leave this value as null or set to 
    /// an empty string when no navigation is desired. The album ID of the selected album is passed to 
    /// the URL as the query string parameter "aid". Example: "Gallery.aspx, http://site.com/gallery.aspx"
    /// </summary>
    /// <value>A string representing a relative or absolute URL.</value>
    public String NavigateUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether checkbox functionality is desired. The default value
    /// is <c>false</c>. When <c>false</c>, the property <see cref="TreeNode.ShowCheckBox" /> is ignored.
    /// </summary>
    /// <value><c>true</c> if checkbox functionality is desired; otherwise, <c>false</c>.</value>
    public bool EnableCheckboxPlugin { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the album specified in the <see cref="AlbumId" /> property is to be included in the 
    /// output or whether only the children are to be rendered. This property is ignored when <see cref="AlbumId" /> has not been
    /// assigned a value. For example, when this property is <c>true</c> and <see cref="AlbumId" /> is the root album, the generated 
    /// true includes a node for the root album and its children. If this property is <c>false</c> only a collection of nodes 
    /// representing the root album's children are generated. Defaults to <c>false</c>. Although this property is ignored when 
    /// <see cref="Galleries" /> is assigned instead of <see cref="AlbumId" />, in that situation the top level albums are rendered 
    /// as if this property were set to <c>true</c>.
    /// </summary>
    public bool IncludeAlbum { get; set; }

    /// <summary>
    /// Gets or sets the security permission the logged on user must have in order for an album to be displayed in the
    /// treeview. It may be a single value or some combination of valid enumeration values.
    /// </summary>
    public SecurityActions RequiredSecurityPermissions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the top level album to render. When not specified, the <see cref="Galleries" /> property determines
    /// the root albums to be rendered.
    /// </summary>
    /// <value>The top level album to render.</value>
    public int AlbumId { get; set; }

    /// <summary>
    /// Gets or sets a value to be prepended to the root album titles in the treeview. The default value is <see cref="String.Empty" />.
    /// May contain the placeholder values {GalleryId} and/or {GalleryDescription}. If present, the placeholders are replaced by the 
    /// action values during databinding. Example: "Gallery {GalleryDescription}: "
    /// </summary>
    public string RootNodesPrefix { get; set; }

    /// <summary>
    /// Gets or sets the galleries to be rendered in the treeview. If not explicitly set, this defaults to the current gallery.
    /// If the <see cref="AlbumId" /> property is assigned, this property is ignored.
    /// </summary>
    /// <value>The galleries to be rendered in the treeview.</value>
    public IGalleryCollection Galleries { get; set; }
  }

  /// <summary>
  /// Represents a node that can be JSON-serialized to a form expected by the jsTree jquery widget.
  /// </summary>
  [DebuggerDisplay("Text: {Text} ({Nodes.Count} child nodes)")]
  public class JsTreeNode
  {
    private List<JsTreeNode> _nodes;

    /// <summary>
    /// Gets or sets the text of the node.
    /// </summary>
    /// <value>A string.</value>
    [JsonProperty(PropertyName = "text", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the path to an image to use for the node icon, or the class to be applied to the &lt;ins&gt; 
    /// DOM element. When the value contains a slash (/) it is treated as a file and used as a background image.
    /// Any other string will be assigned to the class attribute of the &lt;i&gt; element that is used to
    /// represent the icon.
    /// </summary>
    /// <value>A string.</value>
    [JsonProperty(PropertyName = "icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Icon { get; set; }

    /// <summary>
    /// Gets or sets the state of a tree node.
    /// </summary>
    /// <value>An instance of <see cref="JsNodeState" />.</value>
    [JsonProperty(PropertyName = "state", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public JsNodeState State { get; set; }

    /// <summary>
    /// Gets or sets the HTML attributes to be rendered on a node's &lt;a&gt; element.
    /// </summary>
    /// <value>The hyperlink attributes.</value>
    [JsonProperty(PropertyName = "a_attr", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public JsNodeAttributes HyperlinkAttributes { get; set; }

    /// <summary>
    /// Gets or sets the HTML attributes to be rendered on a node's &lt;li&gt; element.
    /// </summary>
    /// <value>The list item attributes.</value>
    [JsonProperty(PropertyName = "li_attr", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public JsNodeAttributes ListItemAttributes { get; set; }

    /// <summary>
    /// Gets the child nodes for this instance. This property is intended for Json.NET serialization.
    /// If a developer wishes to access the nodes, read the remarks carefully to prevent unintended side effects of interacting
    /// with this property. Consider adding a GetNodes() method to retrieve the _nodes variable instead of using this property.
    /// </summary>
    /// <remarks>jsTree requires "children":true to configure a node for lazy loading its children. To accomplish this, we carefully
    /// ensure this property has one of thee values:
    /// null - Indicates this node has no children
    /// Count=0 - Indicates this node has children that should lazy load via ajax.
    /// Count>0 - Indicates this node has children explicitly defined.
    /// This usage works as expected for the 1st &amp; 3rd scenario, but when Count=0, JSON.NET serializes it as "children":[]. 
    /// To fix this, we have a Replace function in <see cref="TreeView.ToJson" /> that converts "children":[] to "children":true.
    /// Various attempts at creating a custom JSON serialize were tried, but they failed primarily due to the recursive
    /// nature of this property. </remarks>
    [JsonProperty(PropertyName = "children", DefaultValueHandling = DefaultValueHandling.Ignore)]
    private List<JsTreeNode> Nodes
    {
      get { return _nodes; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this node has child objects that can be accessed in the data store.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this node has child objects in the data store; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>Read the notes for the <see cref="Nodes" /> property for more details.</remarks>
    [JsonIgnore]
    public bool HasChildren
    {
      get { return (_nodes != null); }
      set
      {
        if (value == false)
        {
          _nodes = null;
        }
        else if (_nodes == null)
        {
          _nodes = new List<JsTreeNode>();
        }
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsTreeNode"/> class.
    /// </summary>
    public JsTreeNode()
    {
      State = new JsNodeState();
      HyperlinkAttributes = new JsNodeAttributes();
      ListItemAttributes = new JsNodeAttributes();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsTreeNode"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    public JsTreeNode(string text)
      : this()
    {
      Text = text;
    }

    /// <summary>
    /// Adds the <paramref name="node" /> to the list of nodes that are children of this instance.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="node" /> is null.</exception>
    public void AddNode(JsTreeNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      if (_nodes == null)
      {
        _nodes = new List<JsTreeNode>();
      }

      _nodes.Add(node);
    }
  }

  /// <summary>
  /// Represents the state of a tree node.
  /// </summary>
  public class JsNodeState
  {
    #region Properties

    /// <summary>
    /// Gets or sets whether the node is in an expanded state.
    /// </summary>
    /// <value>A boolean.</value>
    [JsonProperty(PropertyName = "opened", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets or sets whether the node is disabled.
    /// </summary>
    /// <value>A boolean.</value>
    [JsonProperty(PropertyName = "disabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets or sets whether the node is selected.
    /// </summary>
    /// <value>A boolean.</value>
    [JsonProperty(PropertyName = "selected", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsSelected { get; set; }

    #endregion
  }

  /// <summary>
  /// Represents HTML attributes to be rendered on a DOM element corresponding a jsTree treenode in HTML.
  /// </summary>
  //[DataContract]
  public class JsNodeAttributes
  {
    #region Private Fields

    private readonly List<String> _cssClasses;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the ID attribute of the DOM element.
    /// </summary>
    /// <value>A string.</value>
    [JsonProperty(PropertyName = "id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the href attribute of the DOM element.
    /// </summary>
    /// <value>A string.</value>
    [JsonProperty(PropertyName = "href", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string NavigationUrl { get; set; }

    /// <summary>
    /// Gets or sets the title attribute of the DOM element.
    /// </summary>
    /// <value>A string.</value>
    [JsonProperty(PropertyName = "title", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string ToolTip { get; set; }

    /// <summary>
    /// Gets or sets the data-id attribute of the DOM element.
    /// </summary>
    /// <value>A string.</value>
    [JsonProperty(PropertyName = "data-id", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string DataId { get; set; }

    /// <summary>
    /// Gets the class attribute of the DOM element. When multiple classes exist, it returns a space-separated string.
    /// </summary>
    /// <value>A string.</value>
    /// <exception cref="NotImplementedException">Thrown when attempting to set this property. Instead, use the 
    /// <see cref="AddCssClass" /> or <see cref="RemoveCssClass" /> methods.</exception>
    [JsonProperty(PropertyName = "class", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string CssClasses
    {
      get { return String.Join(" ", _cssClasses); }
      private set { throw new NotImplementedException("Setter not implemented for property 'CssClasses' in class GalleryServer.Web.Entity.Attributes. Use method AddCssClass or RemoveCssClass instead."); }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="JsNodeAttributes"/> class.
    /// </summary>
    public JsNodeAttributes()
    {
      _cssClasses = new List<string>();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds the <paramref name="cssClass" /> to the class attribute of the current tree node. If the class
    /// is already specified, no action is taken.
    /// </summary>
    /// <param name="cssClass">The CSS class.</param>
    public void AddCssClass(string cssClass)
    {
      if (!String.IsNullOrWhiteSpace(cssClass) && !_cssClasses.Contains(cssClass))
      {
        _cssClasses.Add(cssClass);
      }
    }

    /// <summary>
    /// Removes the <paramref name="cssClass" /> from the class attribute of the current tree node.
    /// </summary>
    /// <param name="cssClass">The CSS class.</param>
    public void RemoveCssClass(string cssClass)
    {
      if (!String.IsNullOrWhiteSpace(cssClass) && _cssClasses.Contains(cssClass))
      {
        _cssClasses.Remove(cssClass);
      }
    }

    #endregion
  }

  /// <summary>
  /// A collection of <see cref="TreeNode" /> instances.
  /// </summary>
  public class TreeNodeCollection : Collection<TreeNode>
  {
    /// <summary>
    /// Gets or sets a reference to the current collection's parent tree node.
    /// </summary>
    /// <value>The parent.</value>
    private TreeNode Parent { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeNodeCollection"/> class.
    /// </summary>
    private TreeNodeCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeNodeCollection" /> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    public TreeNodeCollection(TreeNode parent)
      : base(new List<TreeNode>())
    {
      this.Parent = parent;
    }

    /// <summary>
    /// Adds the specified tree node.
    /// </summary>
    /// <param name="item">The tree node to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
    public new void Add(TreeNode item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Cannot add null to an existing TreeNodeCollection. Items.Count = " + Items.Count);

      if (Parent != null)
      {
        Parent.AddInternalNode(item.NodeInternal);
        Parent.HasChildren = true;
      }

      base.Add(item);
    }
  }
}