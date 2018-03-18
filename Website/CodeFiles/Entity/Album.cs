using System;
using GalleryServer.Business;

namespace GalleryServer.Web.Entity
{
  /// <summary>
  /// A simple object that contains album information. This class is used to pass information between the browser and the web server
  /// via AJAX callbacks.
  /// </summary>
  public class Album
  {
    /// <summary>
    /// The album ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The ID of the album containing this album.
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// The ID of the gallery to which the album belongs.
    /// </summary>
    public int GalleryId { get; set; }

    /// <summary>
    /// The album title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The album caption.
    /// </summary>
    public string Caption { get; set; }

    /// <summary>
    /// The album owner. Populated only when the
    /// user is a gallery administrator or higher.
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// A comma-delimited list of owners the current album inherits from parent albums. Populated only when the
    /// user is a gallery administrator or higher.
    /// </summary>
    public string InheritedOwners { get; set; }

    /// <summary>
    /// Indicates whether this album is hidden from anonymous users.
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Gets or sets the type of the virtual album.  Maps to the <see cref="VirtualAlbumType" /> enumeration.
    /// </summary>
    public VirtualAlbumType VirtualType { get; set; }

    /// <summary>
    /// Gets or sets the RSS URL for the album. Will be null when an RSS URL is not valid (eg. for virtual root
    /// albums or when not running an Enterprise license.)
    /// </summary>
    public string RssUrl { get; set; }

    /// <summary>
    /// Gets the ID of the metadata item name the album is sorted by. Maps to <see cref="Business.Metadata.MetadataItemName" />.
    /// </summary>
    public Business.Metadata.MetadataItemName SortById { get; set; }
    
    /// <summary>
    /// Indicates whether the album is sorted in ascending (<c>true</c>) or descending (<c>false</c>) order.
    /// </summary>
    public bool SortUp { get; set; }

    /// <summary>
    /// Gets the number of gallery objects in the album (includes albums and media objects).
    /// </summary>
    public int NumGalleryItems { get; set; }

    /// <summary>
    /// Gets the number of child albums in the album.
    /// </summary>
    public int NumAlbums { get; set; }

    /// <summary>
    /// Gets the number of media objects in the album (excludes albums).
    /// </summary>
    public int NumMediaItems { get; set; }

    /// <summary>
    /// Gets a summarized view of all items in this album. Includes both albums and media objects. Populated only when viewing a set of album thumbnails.
    /// </summary>
    public GalleryItem[] GalleryItems { get; set; }

    /// <summary>
    /// Gets the media objects in the album (excludes albums). Populated only when viewing a single media asset.
    /// </summary>
    public MediaItem[] MediaItems { get; set; }

    /// <summary>
    /// Gets the permissions the current user has for the album.
    /// </summary>
    public Permissions Permissions { get; set; }

    /// <summary>
    /// Gets or sets the metadata available for this album.
    /// </summary>
    /// <value>The metadata.</value>
    public MetaItem[] MetaItems { get; set; }

    /// <summary>
    /// Gets or sets the HTML for the album link and all it's parents the user has permission to see.
    /// </summary>
    public string BreadCrumbLinks { get; set; }
  }
}

