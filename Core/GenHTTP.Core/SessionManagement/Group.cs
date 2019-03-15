using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP.SessionManagement
{

    /// <summary>
    /// A group with specific permissions.
    /// </summary>
    public class Group
    {
        private Setting _Section;
        private SessionManager _Manager;

        /// <summary>
        /// Create a new group object.
        /// </summary>
        /// <param name="section">The XML section to handle</param>
        /// <param name="manager">The <see cref="SessionManager" /> this group is a part of</param>
        internal Group(Setting section, SessionManager manager)
        {
            _Section = section;
            _Manager = manager;
        }

        #region get-/setters

        /// <summary>
        /// Set or get the description of this group.
        /// </summary>
        public string Description
        {
            get { return _Section["description"].Value; }
            set
            {
                _Section["description"].Value = value;
                if (_Manager.AutoDump) _Manager.Save();
            }
        }

        /// <summary>
        /// Set or get the name of this group.
        /// </summary>
        public string Name
        {
            get { return _Section.Attributes["name"]; }
            set
            {
                _Section.Attributes["name"] = value;
                if (_Manager.AutoDump) _Manager.Save();
            }
        }

        /// <summary>
        /// Get the ID of this group.
        /// </summary>
        public uint ID
        {
            get { return Convert.ToUInt32(_Section.Attributes["id"]); }
        }

        /// <summary>
        /// Retrieve all users which are assigned to this group.
        /// </summary>
        public Collection<User> AssignedUsers
        {
            get
            {
                List<User> users = new List<User>();
                foreach (User user in _Manager.Users)
                {
                    if (user.IsInGroup(this)) users.Add(user);
                }
                return new Collection<User>(users);
            }
        }

        /// <summary>
        /// Some additional information.
        /// </summary>
        public Setting Tag
        {
            get { return _Section["tag"]; }
        }

        #endregion

        #region permission handling

        /// <summary>
        /// Check, whether this group is allowed to perform the given action.
        /// </summary>
        /// <param name="permission">The permission to check for</param>
        /// <returns>true, if the group is allowed to perform the given action</returns>
        public bool HasPermission(string permission)
        {
            return _Section["permissions"]["permission", "name", permission.ToLower()].ConvertTo<int>(0) == 1;
        }

        /// <summary>
        /// Set the permission for a given action.
        /// </summary>
        /// <param name="permission">The permission to set</param>
        /// <param name="allowed">true, if the group should gain the permission to execute the given action</param>
        public void SetPermission(string permission, bool allowed)
        {
            if (allowed)
            {
                _Section["permissions"]["permission", "name", permission.ToLower()].Value = "1";
            }
            else
            {
                _Section["permissions"].Children.Remove("permission", "name", permission.ToLower());
            }
            if (_Manager.AutoDump) _Manager.Save();
        }

        /// <summary>
        /// Retrieve all permissions of this group.
        /// </summary>
        public Collection<string> Permissions
        {
            get
            {
                List<string> permissions = new List<string>();
                foreach (Setting permission in _Section["permissions"].Children["permission", 0]) permissions.Add(permission.Attributes["name"]);
                return new Collection<string>(permissions);
            }
        }

        #endregion

    }

}
