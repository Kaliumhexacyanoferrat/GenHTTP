using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP.SessionManagement {
  
  /// <summary>
  /// Represents a user, managed by an <see cref="UserCollection" />.
  /// </summary>
  /// <remarks>
  /// Permission handling: The user will gain all permissions of the groups he is a member of. Permission
  /// gains will overwrite permission denies.
  /// </remarks>
  public class User {
    private Setting _Section;
    private SessionManager _Manager;

    /// <summary>
    /// Create a new user object.
    /// </summary>
    /// <param name="section">The XML section which describes this user</param>
    /// <param name="manager">The <see cref="SessionManager" /> this user depends on</param>
    internal User(Setting section, SessionManager manager) {
      _Section = section;
      _Manager = manager;
    }

    #region get-/setters

    /// <summary>
    /// The name of this user.
    /// </summary>
    /// <remarks>
    /// You can change the name of the user without losing references to existing
    /// sessions. The key used for this link is the user ID.
    /// </remarks>
    public string Name {
      get { return _Section.Attributes["name"]; }
      set { 
        _Section.Attributes["name"] = value;
        if (_Manager.AutoDump) _Manager.Save();
      }
    }

    /// <summary>
    /// The description of this user.
    /// </summary>
    public string Description {
      get { return _Section["description"].Value; }
      set { 
        _Section["description"].Value = value;
        if (_Manager.AutoDump) _Manager.Save();
      }
    }

    /// <summary>
    /// The ID of this user.
    /// </summary>
    public uint ID {
      get { return Convert.ToUInt32(_Section.Attributes["id"]); }
    }

    /// <summary>
    /// The rank of this user.
    /// </summary>
    public byte Rank {
      get { return Convert.ToByte(_Section.Attributes["rank"]); }
      set { 
        _Section.Attributes["rank"] = value.ToString();
        if (_Manager.AutoDump) _Manager.Save();
      }
    }

    /// <summary>
    /// The password hash of this user.
    /// </summary>
    /// <remarks>
    /// The setter of this property will not hash the given value.
    /// </remarks>
    public string PasswordHash {
      get { return _Section["password"].Value; }
      set {
        _Section["password"].Value = value;
        if (_Manager.AutoDump) _Manager.Save();
      }
    }

    /// <summary>
    /// For additional data.
    /// </summary>
    public Setting Tag {
      get { return _Section["tag"]; }
    }

    /// <summary>
    /// The last time the session was checked for this user.
    /// </summary>
    public DateTime LastActivity {
      get { return _Section["last"].ToDateTime(); }
      set { _Section["last"].FromDateTime(value); }
    }

    /// <summary>
    /// The last time the session was checked for this user.
    /// </summary>
    public DateTime RegistrationDate {
      get { return _Section["registration"].ToDateTime(); }
      set { _Section["registration"].FromDateTime(value); }
    }

    #endregion

    /// <summary>
    /// Increase the rank of this user by 1.
    /// </summary>
    public void Promote() {
      if (Rank < 254) {
        Rank++;
        if (_Manager.AutoDump) _Manager.Save();
      }
    }

    /// <summary>
    /// Decrease the rank of this user by 1.
    /// </summary>
    public void Demote() {
      if (Rank > 1) {
        Rank--;
        if (_Manager.AutoDump) _Manager.Save();
      }
    }

    #region group management

    /// <summary>
    /// Check, whether this user is assigned to the given group.
    /// </summary>
    /// <param name="group">The group to check</param>
    /// <returns>true, if the user is a member of the given group</returns>
    public bool IsInGroup(Group group) {
      if (group == null) return false;
      foreach (Setting setting in _Section["groups"].Children) {
        if (setting.Attributes["id"] == group.ID.ToString()) return true;
      }
      return false;
    }

    /// <summary>
    /// Add this user to the given group.
    /// </summary>
    /// <param name="group">The group to add the user to</param>
    public void AddToGroup(Group group) {
      if (group == null) throw new ArgumentNullException();
      Setting setting = new Setting("group");
      setting.Attributes["id"] = group.ID.ToString();
      _Section["groups"].Children.Add(setting);
      if (_Manager.AutoDump) _Manager.Save();
    }

    /// <summary>
    /// Remove this user from the given group.
    /// </summary>
    /// <param name="group">The group to remove the user from</param>
    public void RemoveFromGroup(Group group) {
      if (group == null) throw new ArgumentNullException();
      _Section["groups"].Children.Remove("group", "id", group.ID.ToString());
      if (_Manager.AutoDump) _Manager.Save();
    }

    /// <summary>
    /// Retrieve all groups, this user is a member of.
    /// </summary>
    public Collection<Group> AssignedGroups {
      get {
        List<Group> groups = new List<Group>();
        foreach (Setting entry in _Section["groups"].Children["group", 0]) {
          Group group = _Manager.Groups[Convert.ToUInt32(entry.Attributes["id"])];
          if (group != null) groups.Add(group);
        }
        return new Collection<Group>(groups);
      }
    }

    #endregion

    #region permission handling

    /// <summary>
    /// Check, whether the user has a given permission.
    /// </summary>
    /// <param name="permission">The permission to check for</param>
    /// <returns>true, if the user has the given permission</returns>
    public bool HasPermission(string permission) {
      foreach (Group group in AssignedGroups) if (group.HasPermission(permission)) return true;
      return _Section["permissions"]["permission", "name", permission.ToLower()].ConvertTo<int>(0) == 1;
    }

    /// <summary>
    /// Set a permission for this user.
    /// </summary>
    /// <param name="permission">The permission to set</param>
    /// <param name="allowed">true, if the user should gain this permission. false to deny this permission for this user</param>
    public void SetPermission(string permission, bool allowed) {
      if (allowed) {
        _Section["permissions"]["permission", "name", permission.ToLower()].Value = "1";
      }
      else {
        _Section["permissions"].Children.Remove("permission", "name", permission.ToLower());
      }
      if (_Manager.AutoDump) _Manager.Save();
    }

    /// <summary>
    /// Retrieve a list with all permissions of this user (including the permissions of
    /// the groups the user is a member of).
    /// </summary>
    public Collection<string> Permissions {
      get {
        List<String> permissions = new List<string>();
        foreach (Group group in AssignedGroups) {
          foreach (string perm in group.Permissions) if (!permissions.Contains(perm)) permissions.Add(perm);
        }
        foreach (Setting permission in _Section["permissions"].Children["permission", 0]) permissions.Add(permission.Attributes["name"]);
        return new Collection<string>(permissions);
      }
    }
    
    #endregion


  }

}
