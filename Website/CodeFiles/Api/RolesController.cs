using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GalleryServer.Business;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Api
{
	/// <summary>
	/// Contains methods for Web API access to roles.
	/// </summary>
	public class RolesController : ApiController
	{
		/// <summary>
		/// Gets the role with the specified <paramref name="roleName" />.
		/// Example: GET /api/roles/getbyrolename?roleName=System%20Administrator
		/// </summary>
		/// <param name="roleName">The name of the role to retrieve.</param>
		/// <returns>An instance of <see cref="Entity.Role" />.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException"></exception>
		[ActionName("GetByRoleName")]
		public Entity.Role Get(string roleName)
		{
			try
			{
				return RoleController.GetRoleEntity(roleName);
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
		/// Persists the <paramref name="role" /> to the data store. The role can be an existing one or a new one to be
		/// created.
		/// </summary>
		/// <param name="role">The role.</param>
		/// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when the requested action is not successful.</exception>
		public HttpResponseMessage Post(Entity.Role role)
		{
			// POST /api/roles
			try
			{
				// Don't need to check security here because we'll do that in RoleController.Save.
				RoleController.Save(role);
				
				return new HttpResponseMessage(HttpStatusCode.OK)
					       {
						       Content = new StringContent(String.Format(CultureInfo.CurrentCulture, "Role '{0}' has been saved", Utils.HtmlEncode(role.Name)))
					       };
			}
			catch (InvalidGalleryServerRoleException ex)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
				{
					Content = new StringContent(ex.Message),
					ReasonPhrase = "Action Forbidden"
				});
			}
			catch (GallerySecurityException ex)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
				{
					Content = new StringContent(ex.Message),
					ReasonPhrase = "Action Forbidden"
				});
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
			finally
			{
				CacheController.RemoveCache(CacheItem.GalleryServerRoles);
			}
		}

		/// <summary>
		/// Permanently delete the <paramref name="roleName" /> from the data store.
		/// </summary>
		/// <param name="roleName">The name of the role to be deleted.</param>
		/// <returns>An instance of <see cref="HttpResponseMessage" />.</returns>
		/// <exception cref="System.Web.Http.HttpResponseException">Thrown when the requested action is not successful.</exception>
		[ActionName("DeleteByRoleName")]
		[HttpDelete]
		public HttpResponseMessage Delete(string roleName)
		{
			// DELETE /api/roles
			try
			{
				// Don't need to check security here because we'll do that in RoleController.DeleteGalleryServerProRole.
				RoleController.DeleteGalleryServerProRole(roleName);

				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(String.Format(CultureInfo.CurrentCulture, "Role '{0}' has been deleted", Utils.HtmlEncode(roleName)))
				};
			}
			catch (GallerySecurityException ex)
			{
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
				{
					Content = new StringContent(ex.Message),
					ReasonPhrase = "Action Forbidden"
				});
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
			finally
			{
				CacheController.RemoveCache(CacheItem.GalleryServerRoles);
			}
		}
	}
}