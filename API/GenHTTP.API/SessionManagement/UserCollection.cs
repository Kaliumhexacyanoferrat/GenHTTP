using GenHTTP.Api.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.SessionManagement
{

    /// <summary>
    /// Manages users for the <see cref="SessionManager" />.
    /// </summary>
    public class UserCollection
    {
        private SessionManager _Manager;
        private Dictionary<string, User> _Users;

        /// <summary>
        /// Create a new user collection.
        /// </summary>
        /// <param name="manager">The <see cref="SessionManager" /> the new object will depend on</param>
        internal UserCollection(SessionManager manager)
        {
            _Manager = manager;
            _Users = new Dictionary<string, User>();
            foreach (Setting user in _Manager.DataSource["users"].Children["user", 0]) _Users.Add(user.Attributes["name"], new User(user, _Manager));
        }

        /// <summary>
        /// Retrieve the enumerator for the user list.
        /// </summary>
        /// <returns>An enumerator to iterate over all users</returns>
        public IEnumerator<User> GetEnumerator()
        {
            return _Users.Values.GetEnumerator();
        }

        /// <summary>
        /// Check, whether a user with the given name does exist or not.
        /// </summary>
        /// <param name="userName">The user name to check for existence</param>
        /// <returns>true, if the user exists</returns>
        /// <remarks>
        /// This method is case-sensitive.
        /// </remarks>
        public bool Exists(string userName)
        {
            return _Users.ContainsKey(userName);
        }

        /// <summary>
        /// Remove an user from this collection.
        /// </summary>
        /// <param name="user">The user to remove</param>
        public void Remove(User user)
        {
            _Users.Remove(user.Name);
            _Manager.DataSource["users"].Children.Remove("user", "id", user.ID.ToString());
            // remove linked sessions
            try { _Manager.Sessions.Remove(_Manager.Sessions[user.ID]); }
            catch { }
            if (_Manager.AutoDump) _Manager.Save();
        }

        /// <summary>
        /// Add an user to this collection.
        /// </summary>
        /// <param name="userName">The name of the user to add</param>
        /// <returns>The newly created user object</returns>
        /// <exception cref="System.Exception">Will be thrown if there is already an user with the given name</exception>
        public User Add(string userName)
        {
            if (Exists(userName)) throw new Exception("There is already an user named '" + userName + "'");
            // retrieve the next, valid ID
            uint nextID = 1;
            if (_Users.Count > 0) nextID = _Users.Values.Max((User u) => u.ID) + 1;
            // add the setting
            Setting setting = new Setting("user");
            setting.Attributes["name"] = userName;
            setting.Attributes["id"] = nextID.ToString();
            setting.Attributes["rank"] = "0";
            _Manager.DataSource["users"].Children.Add(setting);
            // generate user object
            User user = new User(setting, _Manager);
            // set last activity
            user.LastActivity = DateTime.Now;
            user.RegistrationDate = DateTime.Now;
            // add the user to the collection
            _Users.Add(userName, user);
            if (_Manager.AutoDump) _Manager.Save();
            return user;
        }

        /// <summary>
        /// Retrieve an user by its name.
        /// </summary>
        /// <param name="userName">The name of the user to lookup</param>
        /// <returns>The requested user or null, if it could not be found</returns>
        public User this[string userName]
        {
            get
            {
                if (Exists(userName)) return _Users[userName];
                return null;
            }
        }

        /// <summary>
        /// Retrieve an user by its ID.
        /// </summary>
        /// <param name="userID">The ID of the user to lookup</param>
        /// <returns>The requested user or null, if it could not be found</returns>
        public User this[uint userID]
        {
            get
            {
                foreach (User user in _Users.Values) if (user.ID == userID) return user;
                return null;
            }
        }

        /// <summary>
        /// The number of users in this collection.
        /// </summary>
        public int Count
        {
            get { return _Users.Count; }
        }

        /// <summary>
        /// Set or get the default user.
        /// </summary>
        /// <remarks>
        /// The <see cref="SessionManager.CheckSession" /> method will return this user in the
        /// <see cref="AuthorizationInfo" /> object if no valid session could be found for the
        /// given <see cref="HttpRequest" />.
        /// </remarks>
        public User DefaultUser
        {
            get
            {
                if (_Manager.DataSource["users"].Attributes["default"] == "") return null;
                return this[Convert.ToUInt32(_Manager.DataSource["users"].Attributes["default"])];
            }
            set { _Manager.DataSource["users"].Attributes["default"] = value.ID.ToString(); }
        }

    }

}
