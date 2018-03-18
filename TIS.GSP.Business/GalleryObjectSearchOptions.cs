using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// An object that specifies options for retrieving gallery objects. Used in conjunction with the
	/// <see cref="GalleryObjectSearcher" /> class.
	/// </summary>
	public class GalleryObjectSearchOptions
	{
		/// <summary>
		/// Indicates the type of search being performed.
		/// </summary>
		public GalleryObjectSearchType SearchType;

		/// <summary>
		/// Specifies the tags to search for. Applies only when <see cref="SearchType" /> is
		/// <see cref="GalleryObjectSearchType.SearchByTag" /> or <see cref="GalleryObjectSearchType.SearchByPeople" />.
		/// </summary>
		public string[] Tags;
		
		/// <summary>
		/// Specifies the text to search for. Applies only when <see cref="SearchType" /> is
		/// <see cref="GalleryObjectSearchType.SearchByKeyword" />, <see cref="GalleryObjectSearchType.SearchByTitleOrCaption" />,
		/// or <see cref="GalleryObjectSearchType.SearchByRating" />.
		/// </summary>
		public string[] SearchTerms;

		/// <summary>
		/// The gallery ID. Only items in this gallery are returned.
		/// </summary>
		public int GalleryId;

		/// <summary>
		/// The roles the current user belongs to. Required when <see cref="IsUserAuthenticated" />=<c>true</c>; 
		/// otherwise, the value can be left null.
		/// </summary>
		public IGalleryServerRoleCollection Roles;

		/// <summary>
		/// Indicates whether the current user has been authenticated.
		/// </summary>
		public bool IsUserAuthenticated;

		/// <summary>
		/// The maximum number of results to return. When zero, all matching results are returned.
		/// </summary>
		public int MaxNumberResults;

		/// <summary>
		/// A filter specifying the type of gallery objects to return. Required. Specify
		/// <see cref="GalleryObjectType.All" /> to return all matching items.
		/// </summary>
		public GalleryObjectType Filter;
	}
}
