using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
	/// <summary>
	/// Contains methods for Web API access to metadata.
	/// </summary>
	public class MetaController : ApiController
	{
		#region Methods

		/// <summary>
		/// Gets a list of tags the current user can view. Guaranteed to not return null.
		/// </summary>
		/// <param name="q">The search term. Only tags that begin with this string are returned.
		/// Specify null or an empty string to return all tags.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
		/// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
		/// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or
		/// "value" to sort by tag name. When not specified, defaults to "notspecified".</param>
		/// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
		/// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
		/// <returns>IEnumerable{Tag}.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
		[ActionName("Tags")]
		public IEnumerable<Business.Entity.Tag> GetTags(string q, int galleryId, int top = int.MaxValue, string sortBy = "notspecified", bool sortAscending = false)
		{
			try
			{
				TagSearchOptions.TagProperty sortProperty;
				if (!Enum.TryParse(sortBy, true, out sortProperty))
				{
					sortProperty = TagSearchOptions.TagProperty.NotSpecified;
				}

				return MetadataController.GetTags(TagSearchType.TagsUserCanView, q, galleryId, top, sortProperty, sortAscending);
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}

		/// <summary>
		/// Gets a JSON string representing the tags used in the specified gallery. The JSON can be used as the
		/// data source for the jsTree jQuery widget. Only tags the current user has permission to view are
		/// included. The tag tree has a root node containing a single level of tags. Throws an exception when
		/// the application is not running an Enterprise License.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
		/// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
		/// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or
		/// "value" to sort by tag name. When not specified, defaults to "count".</param>
		/// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
		/// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
		/// <param name="expanded">if set to <c>true</c> the tree is configured to display in an expanded form.</param>
		/// <returns>System.String.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
		public string GetTagTreeAsJson(int galleryId, int top = int.MaxValue, string sortBy = "count", bool sortAscending = false, bool expanded = false)
		{
			try
			{
				//ValidateEnterpriseLicense();

				TagSearchOptions.TagProperty sortProperty;
				if (!Enum.TryParse(sortBy, true, out sortProperty))
				{
					sortProperty = TagSearchOptions.TagProperty.NotSpecified;
				}

				return MetadataController.GetTagTreeAsJson(TagSearchType.TagsUserCanView, galleryId, top, sortProperty, sortAscending, expanded);
			}
			catch (GallerySecurityException)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}	

		/// <summary>
		/// Gets a JSON string representing the tags used in the specified gallery. The JSON can be used as the 
		/// data source for the jsTree jQuery widget. Only tags the current user has permission to view are
		/// included. The tag tree has a root node containing a single level of tags. Throws an exception when
		/// the application is not running an Enterprise License.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
		/// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
		/// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or 
		/// "value" to sort by tag name. When not specified, defaults to "count".</param>
		/// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
		/// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
		/// <param name="expanded">if set to <c>true</c> the tree is configured to display in an expanded form.</param>
		/// <returns>System.String.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
		public string GetPeopleTreeAsJson(int galleryId, int top = int.MaxValue, string sortBy = "count", bool sortAscending = false, bool expanded = false)
		{
			try
			{
				TagSearchOptions.TagProperty sortProperty;
				if (!Enum.TryParse(sortBy, true, out sortProperty))
				{
					sortProperty = TagSearchOptions.TagProperty.NotSpecified;
				}

				return MetadataController.GetTagTreeAsJson(TagSearchType.PeopleUserCanView, galleryId, top, sortProperty, sortAscending, expanded);
			}
			catch (GallerySecurityException)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}

		/// <summary>
		/// Gets a list of people the current user can view. Guaranteed to not return null.
		/// </summary>
		/// <param name="q">The search term. Only tags that begin with this string are returned.
		/// Specify null or an empty string to return all tags.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
		/// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
		/// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or
		/// "value" to sort by tag name. When not specified, defaults to "notspecified".</param>
		/// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
		/// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
		/// <returns>IEnumerable{Tag}.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
		[ActionName("People")]
		public IEnumerable<Business.Entity.Tag> GetPeople(string q, int galleryId, int top = int.MaxValue, string sortBy = "notspecified", bool sortAscending = false)
		{
			try
			{
				TagSearchOptions.TagProperty sortProperty;
				if (!Enum.TryParse(sortBy, true, out sortProperty))
				{
					sortProperty = TagSearchOptions.TagProperty.NotSpecified;
				}

				return MetadataController.GetTags(TagSearchType.PeopleUserCanView, q, galleryId, top, sortProperty, sortAscending);
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}

		/// <summary>
		/// Persists the metadata item to the data store. The current implementation requires that
		/// an existing item exist in the data store and only stores the contents of the
		/// <see cref="Entity.MetaItem.Value" /> property.
		/// </summary>
		/// <param name="metaItem">An instance of <see cref="Entity.MetaItem" /> to persist to the data
		/// store.</param>
		/// <returns>Entity.MetaItem.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when an album or media object associated
		/// with the meta item doesn't exist or an error occurs.</exception>
		public Entity.MetaItem PutMetaItem(Entity.MetaItem metaItem)
		{
			try
			{
				return MetadataController.Save(metaItem);
			}
			catch (InvalidAlbumException)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
																					{
																						Content = new StringContent(String.Format("Could not find album with ID {0}", metaItem.MediaId)),
																						ReasonPhrase = "Album Not Found"
																					});
			}
			catch (InvalidMediaObjectException)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
																					{
																						Content = new StringContent(String.Format("One of the following errors occurred: (1) Could not find meta item with ID {0} (2) Could not find media object with ID {1} ", metaItem.Id, metaItem.MediaId)),
																						ReasonPhrase = "Media Object/Metadata Item Not Found"
																					});
			}
			catch (GallerySecurityException)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}

    /// <summary>
    /// Writes database metadata having ID <paramref name="metaNameId" /> to the media file for all writable assets in the gallery having ID 
    /// <paramref name="galleryId" />. The action is executed asynchronously and returns immediately.
    /// </summary>
    /// <param name="metaNameId">ID of the meta item. This must match the enumeration value of <see cref="MetadataItemName" />.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
    [HttpPost]
		[ActionName("WriteMetaItem")]
		public void WriteItemForGallery(int metaNameId, int galleryId)
		{
			try
			{
				if (Utils.IsCurrentUserGalleryAdministrator(galleryId))
				{
					var metaName = (MetadataItemName) metaNameId;
					if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(metaName))
					{
						MetadataController.WriteItemForGalleryAsync(metaName, galleryId);
					}
				}
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}

		/// <summary>
		/// Rebuilds the meta name having ID <paramref name="metaNameId" /> for all items in the gallery having ID 
		/// <paramref name="galleryId" />. The action is executed asynchronously and returns immediately.
		/// </summary>
		/// <param name="metaNameId">ID of the meta item. This must match the enumeration value of <see cref="MetadataItemName" />.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when an error occurs.</exception>
		[HttpPost]
		[ActionName("RebuildMetaItem")]
		public void RebuildItemForGallery(int metaNameId, int galleryId)
		{
			try
			{
				if (Utils.IsCurrentUserGalleryAdministrator(galleryId))
				{
					var metaName = (MetadataItemName) metaNameId;
					if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(metaName))
					{
						MetadataController.RebuildItemForGalleryAsync(metaName, galleryId);
					}
				}
			}
			catch (Exception ex)
			{
				AppEventController.LogError(ex);

				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = Utils.GetExStringContent(ex),
					ReasonPhrase = "Server Error"
				});
			}
		}

		#endregion

		#region Functions

		// WARNING: Given the current API, there is no way to verify the user has permission to 
		// view the specified meta ID, so we'll comment out this method to ensure it isn't used.
		///// <summary>
		///// Gets the meta item with the specified <paramref name="id" />.
		///// Example: api/meta/4/
		///// </summary>
		///// <param name="id">The value that uniquely identifies the metadata item.</param>
		///// <returns>An instance of <see cref="Entity.MetaItem" />.</returns>
		///// <exception cref="System.Web.Http.HttpResponseException"></exception>
		//public Entity.MetaItem Get(int id)
		//{
		//	try
		//	{
		//		return MetadataController.Get(id);
		//	}
		//	catch (InvalidMediaObjectException)
		//	{
		//		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
		//		{
		//			Content = new StringContent(String.Format("Could not find meta item with ID = {0}", id)),
		//			ReasonPhrase = "Media Object Not Found"
		//		});
		//	}
		//	catch (GallerySecurityException)
		//	{
		//		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
		//	}
		//}

		#endregion
	}
}