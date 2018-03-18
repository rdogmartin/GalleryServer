using System;

namespace GalleryServer.Business.Interfaces
{
	/// <summary>
	/// Represents a user in the current application.
	/// </summary>
	public interface IUserAccount
	{
		/// <summary>
		/// Gets or sets application-specific information for the membership user. 
		/// </summary>
		/// <value>Application-specific information for the membership user.</value>
		string Comment { get; set; }

		/// <summary>
		/// Gets the date and time when the user was added to the membership data store.
		/// </summary>
		/// <value>The date and time when the user was added to the membership data store.</value>
		DateTime CreationDate { get; }

		/// <summary>
		/// Gets or sets the e-mail address for the membership user.
		/// </summary>
		/// <value>The e-mail address for the membership user.</value>
		string Email { get; set; }

		/// <summary>
		/// Gets or sets whether the membership user can be authenticated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if user can be authenticated; otherwise, <c>false</c>.
		/// </value>
		bool IsApproved { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the membership user is locked out and unable to be validated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the membership user is locked out and unable to be validated; otherwise, <c>false</c>.
		/// </value>
		bool IsLockedOut { get; set; }

		/// <summary>
		/// Gets whether the user is currently online.
		/// </summary>
		/// <value><c>true</c> if the user is online; otherwise, <c>false</c>.</value>
		bool IsOnline { get; }

		/// <summary>
		/// Gets or sets the date and time when the membership user was last authenticated or accessed the application.
		/// </summary>
		/// <value>The date and time when the membership user was last authenticated or accessed the application.</value>
		DateTime LastActivityDate { get; set; }

		/// <summary>
		/// Gets the most recent date and time that the membership user was locked out.
		/// </summary>
		/// <value>The most recent date and time that the membership user was locked out.</value>
		DateTime LastLockoutDate { get; }

		/// <summary>
		/// Gets or sets the date and time when the user was last authenticated.
		/// </summary>
		/// <value>The date and time when the user was last authenticated.</value>
		DateTime LastLoginDate { get; set; }

		/// <summary>
		/// Gets the date and time when the membership user's password was last updated.
		/// </summary>
		/// <value>The date and time when the membership user's password was last updated.</value>
		DateTime LastPasswordChangedDate { get; }

		/// <summary>
		/// Gets the password question for the membership user.
		/// </summary>
		/// <value>The password question for the membership user.</value>
		string PasswordQuestion { get; }

		/// <summary>
		/// Gets the name of the membership provider that stores and retrieves user information for the membership user.
		/// </summary>
		/// <value>The name of the membership provider that stores and retrieves user information for the membership user.</value>
		string ProviderName { get; }

		/// <summary>
		/// Gets the user identifier from the membership data source for the user.
		/// </summary>
		/// <value>The user identifier from the membership data source for the user.</value>
		object ProviderUserKey { get; }

		/// <summary>
		/// Gets the logon name of the membership user.
		/// </summary>
		/// <value>The logon name of the membership user.</value>
		string UserName { get; }

		/// <summary>
		/// Copies the current account information to the specified <paramref name="userAccount" />.
		/// </summary>
		/// <param name="userAccount">The user account to populate with information from the current instance.</param>
		void CopyTo(IUserAccount userAccount);

		/// <summary>
		/// Gets a value indicating whether the user has no restrictions on actions.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user is a super user; otherwise, <c>false</c>.
		/// </value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		bool IsSuperUser { get; }

		/// <summary>
		/// Gets or sets the user's first name.
		/// </summary>
		/// <value>The user's first name.</value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the user's last name.
		/// </summary>
		/// <value>The user's last name.</value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		string LastName { get; set; }

		/// <summary>
		/// Gets or sets the user's display name.
		/// </summary>
		/// <value>The user's display name.</value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		string DisplayName { get; set; }
	}
}