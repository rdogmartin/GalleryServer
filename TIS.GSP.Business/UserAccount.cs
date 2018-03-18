using System;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
	/// <summary>
	/// Represents a user in the current application.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("UserName = {_userName}")]
	public class UserAccount : IUserAccount, IComparable
	{
		#region Private Fields

		private string _comment;
		private DateTime _creationDate;
		private string _email;
		private bool _isApproved;
		private bool _isLockedOut;
		private bool _isOnline;
		private DateTime _lastActivityDate;
		private DateTime _lastLockoutDate;
		private DateTime _lastLoginDate;
		private DateTime _lastPasswordChangedDate;
		private string _passwordQuestion;
		private string _providerName;
		private object _providerUserKey;
		private string _userName;
		private readonly bool _isSuperUser;
		private string _firstName;
		private string _lastName;
		private string _displayName;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAccount"/> class with the specified <paramref name="userName" />.
		/// All other properties are left at default values.
		/// </summary>
		/// <param name="userName">The logon name of the membership user.</param>
		public UserAccount(string userName)
		{
			_userName = userName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAccount"/> class.
		/// </summary>
		/// <param name="comment">Application-specific information for the membership user.</param>
		/// <param name="creationDate">The date and time when the user was added to the membership data store.</param>
		/// <param name="email">The e-mail address for the membership user.</param>
		/// <param name="isApproved">Indicates whether the membership user can be authenticated.</param>
		/// <param name="isLockedOut">Indicates whether the membership user is locked out.</param>
		/// <param name="isOnline">Indicates whether the membership user is online.</param>
		/// <param name="lastActivityDate">The date and time when the membership user was last authenticated or accessed the application.</param>
		/// <param name="lastLockoutDate">The most recent date and time that the membership user was locked out.</param>
		/// <param name="lastLoginDate">The date and time when the user was last authenticated.</param>
		/// <param name="lastPasswordChangedDate">The date and time when the membership user's password was last updated.</param>
		/// <param name="passwordQuestion">The password question for the membership user.</param>
		/// <param name="providerName">The name of the membership provider that stores and retrieves user information for the membership user.</param>
		/// <param name="providerUserKey">The user identifier from the membership data source for the user.</param>
		/// <param name="userName">The logon name of the membership user.</param>
		/// <param name="isSuperUser">Indicates whether the user has no restrictions on actions. DotNetNuke only. Specify <c>false</c> for 
		/// non-DotNetNuke versions.</param>
		/// <param name="firstName">The first name. DotNetNuke only. Specify <see cref="String.Empty" /> for non-DotNetNuke versions.</param>
		/// <param name="lastName">The last name. DotNetNuke only. Specify <see cref="String.Empty" /> for non-DotNetNuke versions.</param>
		/// <param name="displayName">The display name. DotNetNuke only. Specify <see cref="String.Empty" /> for non-DotNetNuke versions.</param>
		public UserAccount(string comment, DateTime creationDate, string email, bool isApproved, bool isLockedOut, bool isOnline, DateTime lastActivityDate, DateTime lastLockoutDate, DateTime lastLoginDate, DateTime lastPasswordChangedDate, string passwordQuestion, string providerName, object providerUserKey, string userName, bool isSuperUser, string firstName, string lastName, string displayName)
		{
			_comment = comment;
			_creationDate = creationDate;
			_email = email;
			_isApproved = isApproved;
			_isLockedOut = isLockedOut;
			_isOnline = isOnline;
			_lastActivityDate = lastActivityDate;
			_lastLockoutDate = lastLockoutDate;
			_lastLoginDate = lastLoginDate;
			_lastPasswordChangedDate = lastPasswordChangedDate;
			_passwordQuestion = passwordQuestion;
			_providerName = providerName;
			_providerUserKey = providerUserKey;
			_userName = userName;
			_isSuperUser = isSuperUser;
			_firstName = firstName;
			_lastName = lastName;
			_displayName = displayName;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets application-specific information for the membership user. 
		/// </summary>
		/// <value>Application-specific information for the membership user.</value>
		public string Comment
		{
			get { return this._comment; }
			set { this._comment = value; }
		}

		/// <summary>
		/// Gets the date and time when the user was added to the membership data store.
		/// </summary>
		/// <value>The date and time when the user was added to the membership data store.</value>
		public DateTime CreationDate
		{
			get { return this._creationDate; }
		}

		/// <summary>
		/// Gets or sets the e-mail address for the membership user.
		/// </summary>
		/// <value>The e-mail address for the membership user.</value>
		public string Email
		{
			get { return this._email; }
			set { this._email = value; }
		}

		/// <summary>
		/// Gets or sets whether the membership user can be authenticated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if user can be authenticated; otherwise, <c>false</c>.
		/// </value>
		public bool IsApproved
		{
			get { return this._isApproved; }
			set { this._isApproved = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the membership user is locked out and unable to be validated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the membership user is locked out and unable to be validated; otherwise, <c>false</c>.
		/// </value>
		public bool IsLockedOut
		{
			get { return this._isLockedOut; }
			set { this._isLockedOut = value; }
		}

		/// <summary>
		/// Gets whether the user is currently online.
		/// </summary>
		/// <value><c>true</c> if the user is online; otherwise, <c>false</c>.</value>
		public bool IsOnline
		{
			get { return this._isOnline; }
		}

		/// <summary>
		/// Gets or sets the date and time when the membership user was last authenticated or accessed the application.
		/// </summary>
		/// <value>The date and time when the membership user was last authenticated or accessed the application.</value>
		public DateTime LastActivityDate
		{
			get { return this._lastActivityDate; }
			set { this._lastActivityDate = value; }
		}

		/// <summary>
		/// Gets the most recent date and time that the membership user was locked out.
		/// </summary>
		/// <value>The most recent date and time that the membership user was locked out.</value>
		public DateTime LastLockoutDate
		{
			get { return this._lastLockoutDate; }
		}

		/// <summary>
		/// Gets or sets the date and time when the user was last authenticated.
		/// </summary>
		/// <value>The date and time when the user was last authenticated.</value>
		public DateTime LastLoginDate
		{
			get { return this._lastLoginDate; }
			set { this._lastLoginDate = value; }
		}

		/// <summary>
		/// Gets the date and time when the membership user's password was last updated.
		/// </summary>
		/// <value>The date and time when the membership user's password was last updated.</value>
		public DateTime LastPasswordChangedDate
		{
			get { return this._lastPasswordChangedDate; }
		}

		/// <summary>
		/// Gets the password question for the membership user.
		/// </summary>
		/// <value>The password question for the membership user.</value>
		public string PasswordQuestion
		{
			get { return this._passwordQuestion; }
		}

		/// <summary>
		/// Gets the name of the membership provider that stores and retrieves user information for the membership user.
		/// </summary>
		/// <value>The name of the membership provider that stores and retrieves user information for the membership user.</value>
		public string ProviderName
		{
			get { return this._providerName; }
		}

		/// <summary>
		/// Gets the user identifier from the membership data source for the user.
		/// </summary>
		/// <value>The user identifier from the membership data source for the user.</value>
		public object ProviderUserKey
		{
			get { return this._providerUserKey; }
		}

		/// <summary>
		/// Gets the logon name of the membership user.
		/// </summary>
		/// <value>The logon name of the membership user.</value>
		public string UserName
		{
			get { return this._userName; }
		}

		/// <summary>
		/// Gets a value indicating whether the user has no restrictions on actions.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the user is a super user; otherwise, <c>false</c>.
		/// </value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		public bool IsSuperUser
		{
			get { return _isSuperUser; }
		}

		/// <summary>
		/// NOT IMPLEMENTED: Gets or sets the user's first name.
		/// </summary>
		/// <value>The user's first name.</value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		public string FirstName
		{
			get { return _firstName; }
			set { _firstName = value; }
		}

		/// <summary>
		/// NOT IMPLEMENTED: Gets or sets the user's last name.
		/// </summary>
		/// <value>The user's last name.</value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		public string LastName
		{
			get { return _lastName; }
			set { _lastName = value; }
		}

		/// <summary>
		/// NOT IMPLEMENTED: Gets or sets the user's display name.
		/// </summary>
		/// <value>The user's display name.</value>
		[Obsolete("Not implemented in current version of Gallery Server, but may be implemented in versions that derive from this code, such as the DotNetNuke module.", true)]
		public string DisplayName
		{
			get { return _displayName; }
			set { _displayName = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Copies the current account information to the specified <paramref name="userAccount" />. The <paramref name="userAccount" />
		/// must be able to be cast to an instance of <see cref="UserAccount" />. If not, an <see cref="ArgumentNullException" />
		/// is thrown.
		/// </summary>
		/// <param name="userAccount">The user account to populate with information from the current instance.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userAccount" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userAccount" /> cannot be cast to an instance of 
		/// <see cref="UserAccount" />.</exception>
		public void CopyTo(IUserAccount userAccount)
		{
			if (userAccount == null)
				throw new ArgumentNullException("userAccount");

			try
			{
				CopyToInstance(userAccount as UserAccount);
			}
			catch (ArgumentNullException)
			{
				throw new ArgumentOutOfRangeException("userAccount", "The parameter 'userAccount' cannot be cast to an instance of UserAccount.");
			}
		}

		#endregion

		#region Private Functions

		/// <summary>
		/// Copies the current account information to the specified <paramref name="userAccount" />.
		/// </summary>
		/// <param name="userAccount">The user account to populate with information from the current instance.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="userAccount" /> is null.</exception>
		private void CopyToInstance(UserAccount userAccount)
		{
			if (userAccount == null)
				throw new ArgumentNullException("userAccount");

			userAccount._comment = this.Comment;
			userAccount._creationDate = this.CreationDate;
			userAccount._email = this.Email;
			userAccount._isApproved = this.IsApproved;
			userAccount._isLockedOut = this.IsLockedOut;
			userAccount._isOnline = this.IsOnline;
			userAccount._lastActivityDate = this.LastActivityDate;
			userAccount._lastLockoutDate = this.LastLockoutDate;
			userAccount._lastLoginDate = this.LastLoginDate;
			userAccount._lastPasswordChangedDate = this.LastPasswordChangedDate;
			userAccount._passwordQuestion = this.PasswordQuestion;
			userAccount._providerName = this.ProviderName;
			userAccount._providerUserKey = this.ProviderUserKey;
			userAccount._userName = this.UserName;
		}

		#endregion

		#region IComparable

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
				IUserAccount other = obj as IUserAccount;
				if (other != null)
					return String.Compare(this.UserName, other.UserName, StringComparison.Ordinal);
				else
					return 1;
			}
		}

		#endregion
	}
}
