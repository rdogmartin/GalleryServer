using System;
using System.Collections.Generic;
using System.Linq;

namespace GalleryServer.Business.Interfaces
{
  /// <summary>
  /// A collection of <see cref="IUserAccount" /> objects.
  /// </summary>
  public interface IUserAccountCollection : IEnumerable<IUserAccount>
  {
    /// <summary>
    /// Gets a sorted list of user names for accounts in the collection. This is equivalent to iterating through each <see cref="IUserAccount" />
    /// and compiling a string array of the <see cref="IUserAccount.UserName" /> properties.
    /// </summary>
    /// <returns>Returns a string array of user names of accounts in the collection.</returns>
    string[] GetUserNames();

    /// <summary>
    /// Adds the user accounts to the current collection.
    /// </summary>
    /// <param name="userAccounts">The user accounts to add to the current collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userAccounts" /> is null.</exception>
    void AddRange(System.Collections.Generic.IEnumerable<IUserAccount> userAccounts);

    /// <overloads>
    /// Determines whether a user is a member of the collection.
    /// </overloads>
    /// <summary>
    /// Determines whether the <paramref name="item"/> is a member of the collection. An object is considered a member
    /// of the collection if they both have the same <see cref="IUserAccount.UserName"/>. The comparison is case-insensitive.
    /// </summary>
    /// <param name="item">An <see cref="IUserAccount"/> to determine whether it is a member of the current collection.</param>
    /// <returns>
    /// Returns <c>true</c> if <paramref name="item"/> is a member of the current collection; otherwise returns <c>false</c>.
    /// </returns>
    bool Contains(IUserAccount item);

    /// <summary>
    /// Determines whether a user account with the specified <paramref name="userName"/> is a member of the collection.
    /// The comparison is case-insensitive.
    /// </summary>
    /// <param name="userName">The user name that uniquely identifies the user.</param>
    /// <returns>
    /// Returns <c>true</c> if <paramref name="userName"/> is a member of the current collection; otherwise returns <c>false</c>.
    /// </returns>
    bool Contains(string userName);

    /// <summary>
    /// Adds the specified user account.
    /// </summary>
    /// <param name="user">The user account to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    void Add(IUserAccount user);

    /// <summary>
    /// Removes the specified user.
    /// </summary>
    /// <param name="user">The user account to remove.</param>
    /// <returns><c>true</c> if the object was removed successfully; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
    bool Remove(IUserAccount user);

    /// <summary>
    /// Removes all users from the current collection.
    /// </summary>
    void Clear();

    /// <summary>
    /// Find the user account in the collection that matches the specified <paramref name="userName" />. If no matching object is found,
    /// null is returned. The comparison is case-insensitive.
    /// </summary>
    /// <param name="userName">The user name that uniquely identifies the user.</param>
    /// <returns>Returns an <see cref="IUserAccount" />object from the collection that matches the specified <paramref name="userName" />,
    /// or null if no matching object is found.</returns>
    IUserAccount FindByUserName(string userName);

    /// <summary>
    /// Finds the users whose <see cref="IUserAccount.UserName" /> begins with the specified <paramref name="userNameSearchString" />. 
    /// This method can be used to find a set of users that match the first few characters of a string. Returns an empty collection if 
    /// no matches are found. Returns all users if <paramref name="userNameSearchString" /> is null or white space. The match is 
    /// case-insensitive. Example: If <paramref name="userNameSearchString" />="Rob", this method returns users with names like 
    /// "Rob", "Robert", and "robert" but not names such as "Boston Rob".
    /// </summary>
    /// <param name="userNameSearchString">A string to match against the beginning of a <see cref="IUserAccount.UserName" />. Do not
    /// specify a wildcard character. If value is null or an empty string, all users are returned.</param>
    /// <returns>Returns an <see cref="IUserAccountCollection" />object from the collection where the <see cref="IUserAccount.UserName" /> 
    /// begins with the specified <paramref name="userNameSearchString" />, or an empty collection if no matching object is found.</returns>
    IQueryable<IUserAccount> FindAllByUserName(string userNameSearchString);
  }
}
