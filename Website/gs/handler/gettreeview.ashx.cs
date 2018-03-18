using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Services;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Entity;

namespace GalleryServer.Web.Handler
{
  /// <summary>
  /// Defines a handler that returns JSON in a format that is consumable by the JsTree jQuery plug-in.
  /// This can be called when a user clicks on a treeview node to dynamically load that node's contents.
  /// JsTree home page: http://www.jstree.com
  /// </summary>
  [WebService(Namespace = "http://tempuri.org/"), WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  public class gettreeview : IHttpHandler
  {
    #region Fields

    private int _albumId;
    private IGalleryCollection _galleries;
    private SecurityActions _securityAction;
    private bool _showCheckbox;
    private string _navigateUrl;
    private int _numberOfLevels;
    private bool _includeAlbum;
    private readonly IntegerCollection _albumIdsToSelect = new IntegerCollection();

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
    /// </summary>
    /// <value></value>
    /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
    /// </returns>
    public bool IsReusable
    {
      get
      {
        return false;
      }
    }

    private string RootNodesPrefix
    {
      get
      {
        if (_galleries?.Count > 1)
        {
          return string.Concat(Resources.GalleryServer.Site_Gallery_Text, " '{GalleryDescription}': ");
        }
        else
        {
          return string.Empty;
        }
      }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
    /// </summary>
    /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
    public void ProcessRequest(HttpContext context)
    {
      try
      {
        if (!GalleryController.IsInitialized)
        {
          GalleryController.InitializeGspApplication();
        }

        if (InitializeVariables(context))
        {
          string tvXml = GenerateTreeviewJson();
          context.Response.ContentType = "application/json";
          context.Response.Cache.SetCacheability(HttpCacheability.NoCache); // Needed for IE 7
          context.Response.Write(tvXml);
        }
        else
          context.Response.End();
      }
      catch (System.Threading.ThreadAbortException)
      {
        throw; // We don't want these to fall into the generic catch because we don't want them logged.
      }
      catch (Exception ex)
      {
        AppEventController.LogError(ex);
        throw;
      }
    }

    #endregion

    #region Private Functions

    private string GenerateTreeviewJson()
    {
      TreeView tv = GenerateTreeview();
      return tv.ToJson();
    }

    private TreeView GenerateTreeview()
    {
      var tvOptions = new Entity.TreeViewOptions()
      {
        AlbumId = _albumId,
        SelectedAlbumIds = _albumIdsToSelect,
        NumberOfLevels = _numberOfLevels,
        EnableCheckboxPlugin = _showCheckbox,
        IncludeAlbum = _includeAlbum,
        NavigateUrl = _navigateUrl,
        RequiredSecurityPermissions = _securityAction,
        RootNodesPrefix = RootNodesPrefix,
        Galleries = (_albumId > 0 ? null : _galleries)
      };

      return AlbumTreeViewBuilder.GetAlbumsAsTreeView(tvOptions);
    }

    /// <summary>
    /// Initialize the class level variables with information from the query string. Returns false if the variables cannot 
    /// be properly initialized.
    /// </summary>
    /// <param name="context">The HttpContext for the current request.</param>
    /// <returns>Returns true if all variables were initialized; returns false if there was a problem and one or more variables
    /// could not be set.</returns>
    private bool InitializeVariables(HttpContext context)
    {
      return ExtractQueryStringParms(context.Request.Url.Query);
    }

    /// <summary>
    /// Extract information from the query string and assign to our class level variables. Return false if something goes wrong
    /// and the variables cannot be set. This will happen when the query string is in an unexpected format.
    /// </summary>
    /// <param name="queryString">The query string for the current request. Can be populated with HttpContext.Request.Url.Query.</param>
    /// <returns>Returns true if all relevant variables were assigned from the query string; returns false if there was a problem.</returns>
    private bool ExtractQueryStringParms(string queryString)
    {
      if (String.IsNullOrEmpty(queryString)) return false;

      if (queryString.StartsWith("?", StringComparison.Ordinal)) queryString = queryString.Remove(0, 1);

      // id=0&gid=all&secaction=6&sc=false&navurl=&levels=2&includealbum=true&idtoselect%5B%5D=220&idtoselect%5B%5D=99
      foreach (string nameValuePair in queryString.Split(new char[] { '&' }))
      {
        string[] nameOrValue = nameValuePair.Split(new char[] { '=' });

        if (nameOrValue.Length < 2)
        {
          return false;
        }

        switch (Utils.UrlDecode(nameOrValue[0]))
        {
          case "id":
            {
              int aid;
              if (Int32.TryParse(nameOrValue[1], out aid))
                _albumId = aid;
              else
                return false;
              break;
            }
          case "gid":
            {
              int gid;
              if (Int32.TryParse(nameOrValue[1], out gid))
              {
                _galleries = new GalleryCollection() { Factory.LoadGallery(gid) };
              }
              else if (Utils.UrlDecode(nameOrValue[1]).Trim() == "all")
              {
                _galleries = UserController.GetGalleriesCurrentUserCanAdminister();
              }
              else
                return false;
              break;
            }
          case "secaction":
            {
              int secActionInt;
              if (Int32.TryParse(nameOrValue[1], out secActionInt))
              {
                if (SecurityActionEnumHelper.IsValidSecurityAction((SecurityActions)secActionInt))
                {
                  _securityAction = (SecurityActions)secActionInt; break;
                }
                else
                  return false;
              }
              else
                return false;
            }
          case "sc":
            {
              bool showCheckbox;
              if (Boolean.TryParse(nameOrValue[1], out showCheckbox))
                _showCheckbox = showCheckbox;
              else
                return false;
              break;
            }
          case "navurl":
            {
              _navigateUrl = Utils.UrlDecode(nameOrValue[1]).Trim();
              break;
            }
          case "levels":
            {
              int numLevels;
              if (Int32.TryParse(nameOrValue[1], out numLevels))
                _numberOfLevels = numLevels;
              else
                return false;
              break;
            }
          case "includealbum":
            {
              bool includeAlbum;
              if (Boolean.TryParse(nameOrValue[1], out includeAlbum))
                _includeAlbum = includeAlbum;
              else
                return false;
              break;
            }
          case "idtoselect":
          case "idtoselect[]":
            {
              int idToSelect;
              if (Int32.TryParse(nameOrValue[1], out idToSelect))
                _albumIdsToSelect.Add(idToSelect);
              else
                return false;
              break;
            }
          default: return false; // Unexpected query string parm. Return false so execution is aborted.
        }
      }

      return true;
    }

    #endregion
  }
}
