using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
    /// <summary>
    /// A collection of <see cref="IUserAccount" /> objects.
    /// </summary>
    public class UserAccountCollection : IUserAccountCollection
    {
        /// <summary>
        /// Gets the user objects in this collection. The key is the username converted to lowercase invariant. We prefer a dictionary 
        /// over <see cref="ConcurrentBag&lt;IUserAccount&gt;" /> primarily because the dictionary enforces unique keys, while the bag 
        /// allows duplicates.
        /// </summary>
        private ConcurrentDictionary<string, IUserAccount> Items { get; } = new ConcurrentDictionary<string, IUserAccount>();

        /// <summary>
        /// Gets a sorted list of user names for accounts in the collection. This is equivalent to iterating through each <see cref="IUserAccount" />
        /// and compiling a string array of the <see cref="IUserAccount.UserName" /> properties.
        /// </summary>
        /// <returns>Returns a string array of user names of accounts in the collection.</returns>
        public string[] GetUserNames()
        {
            return Items.Values.Select(u => u.UserName).OrderBy(u => u).ToArray();
        }

        /// <summary>
        /// Adds the user accounts to the current collection.
        /// </summary>
        /// <param name="userAccounts">The user accounts to add to the current collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="userAccounts" /> is null.</exception>
        public void AddRange(IEnumerable<IUserAccount> userAccounts)
        {
            if (userAccounts == null)
                throw new ArgumentNullException(nameof(userAccounts));

            foreach (var userAccount in userAccounts)
            {
                this.Add(userAccount);
            }
        }

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
        public bool Contains(IUserAccount item)
        {
            if (item == null)
                return false;

            return Items.ContainsKey(item.UserName.ToLowerInvariant());
        }

        /// <summary>
        /// Determines whether a user account with the specified <paramref name="userName"/> is a member of the collection.
        /// The comparison is case-insensitive.
        /// </summary>
        /// <param name="userName">The user name that uniquely identifies the user.</param>
        /// <returns>
        /// Returns <c>true</c> if <paramref name="userName"/> is a member of the current collection; otherwise returns <c>false</c>.
        /// </returns>
        public bool Contains(string userName)
        {
            return Contains(new UserAccount(userName.ToLowerInvariant()));
        }

        /// <summary>
        /// Adds the specified user account.
        /// </summary>
        /// <param name="user">The user account to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
        public void Add(IUserAccount user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Cannot add null to an existing UserAccountCollection. Items.Count = " + Items.Count);

            Items.TryAdd(user.UserName.ToLowerInvariant(), user);
        }

        /// <summary>
        /// Removes the specified user.
        /// </summary>
        /// <param name="user">The user account to remove.</param>
        /// <returns><c>true</c> if the object was removed successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user" /> is null.</exception>
        public bool Remove(IUserAccount user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IUserAccount removedUser;
            return Items.TryRemove(user.UserName.ToLowerInvariant(), out removedUser);
        }

        /// <summary>
        /// Removes all users from the current collection.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Find the user account in the collection that matches the specified <paramref name="userName" />. If no matching object is found,
        /// null is returned. The comparison is case-insensitive.
        /// </summary>
        /// <param name="userName">The user name that uniquely identifies the user.</param>
        /// <returns>Returns an <see cref="IUserAccount" />object from the collection that matches the specified <paramref name="userName" />,
        /// or null if no matching object is found.</returns>
        public IUserAccount FindByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            IUserAccount user;
            Items.TryGetValue(userName.ToLowerInvariant(), out user);
            return user;
        }

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
        public IQueryable<IUserAccount> FindAllByUserName(string userNameSearchString)
        {
            if (string.IsNullOrWhiteSpace(userNameSearchString))
            {
                return Items.Values.AsQueryable();
            }

            return Items.Values.Where(u => u.UserName.StartsWith(userNameSearchString, StringComparison.OrdinalIgnoreCase)).AsQueryable();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<IUserAccount> GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }
    }
}
