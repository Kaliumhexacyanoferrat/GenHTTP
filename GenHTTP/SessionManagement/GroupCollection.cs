using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP.SessionManagement {
  
  /// <summary>
  /// Manages groups of a <see cref="SessionManagement" /> class.
  /// </summary>
  public class GroupCollection {
    private SessionManager _Manager;
    private Dictionary<string, Group> _Groups;

    /// <summary>
    /// Create a new group collection.
    /// </summary>
    /// <param name="manager">The session manager this object should depend on</param>
    internal GroupCollection(SessionManager manager) {
      _Manager = manager;
      _Groups = new Dictionary<string, Group>();
      // read groups
      foreach (Setting group in _Manager.DataSource["groups"].Children["group", 0]) _Groups.Add(group.Attributes["name"], new Group(group, _Manager));
    }

    /// <summary>
    /// Retrieve an enumerator to iterate over all available groups.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Group> GetEnumerator() {
      return _Groups.Values.GetEnumerator();
    }

    /// <summary>
    /// Check, whether a group with the given name does exist or not.
    /// </summary>
    /// <param name="groupName">The name of the group to check for existence</param>
    /// <returns>true, if the given group does exist</returns>
    public bool Exists(string groupName) {
      return _Groups.ContainsKey(groupName);
    }

    /// <summary>
    /// Remove a existing group.
    /// </summary>
    /// <param name="group">The group to remove</param>
    public void Remove(Group group) {
      _Groups.Remove(group.Name);
      _Manager.DataSource["groups"].Children.Remove("group", "id", group.ID.ToString());
      foreach (User user in _Manager.Users) if (user.IsInGroup(group)) user.RemoveFromGroup(group);
      if (_Manager.AutoDump) _Manager.Save();
    }

    /// <summary>
    /// Add a group to this collection.
    /// </summary>
    /// <param name="name">The name of the group to add</param>
    /// <returns>The newly created group</returns>
    public Group Add(string name) {
      if (Exists(name)) throw new Exception("There is already a group named '" + name + "'");
      // calculate the ID for this group
      uint nextID = 1;
      if (_Groups.Count > 0) nextID = _Groups.Values.Max((Group g) => g.ID) + 1;
      // add the setting
      Setting setting = new Setting("group");
      setting.Attributes["id"] = nextID.ToString();
      setting.Attributes["name"] = name;
      _Manager.DataSource["groups"].Children.Add(setting);
      // generate the managed group object
      Group group = new Group(setting, _Manager);
      _Groups.Add(name, group);
      // dump if required
      if (_Manager.AutoDump) _Manager.Save();
      return group;
    }

    /// <summary>
    /// Retrieve a group object by the name of the group.
    /// </summary>
    /// <param name="groupName">The name of the group to retrieve</param>
    /// <returns>The requested group or null, if it does not exist</returns>
    public Group this[string groupName] {
      get { return (_Groups.ContainsKey(groupName)) ? _Groups[groupName] : null; }
    }

    /// <summary>
    /// Retrieve a group object by the ID of the group.
    /// </summary>
    /// <param name="groupID">The ID of the group to search for</param>
    /// <returns>The requested group or null, if it does not exist</returns>
    public Group this[uint groupID] {
      get {
        foreach (Group group in _Groups.Values) if (group.ID == groupID) return group;
        return null;
      }
    }

  }

}
