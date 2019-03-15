using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {
  
  /// <summary>
  /// Stores css class names for a (X)HTML element.
  /// </summary>
  public class CssClassCollection {
    private List<string> _Classes;

    /// <summary>
    /// Create a new CssClassCollection.
    /// </summary>
    internal CssClassCollection() {
      _Classes = new List<string>();
    }

    /// <summary>
    /// Add a class.
    /// </summary>
    /// <param name="cls">The name of the class to add</param>
    public void AddClass(string cls) {
      if (_Classes.Contains(cls)) return;
      _Classes.Add(cls);
    }

    /// <summary>
    /// Remove a class.
    /// </summary>
    /// <param name="cls">The name of the class to remove</param>
    public void RemoveClass(string cls) {
      if (_Classes.Contains(cls)) _Classes.Remove(cls);
    }

    /// <summary>
    /// Retrieve all available CSS classes in this collection.
    /// </summary>
    public Collection<string> Classes {
      get {
        return new Collection<string>(_Classes);
      }
    }

    /// <summary>
    /// Check whether this collection contains a class or not.
    /// </summary>
    /// <param name="cls">The name of the class to check for existence</param>
    /// <returns>True if the collection contains the given class</returns>
    public bool ContainsClass(string cls) {
      return _Classes.Contains(cls);
    }

    internal string ToCss() {
      string retval = "";
      for (int i = 0; i < _Classes.Count - 1; i++) {
        retval += _Classes[i] + " ";
      }
      if (_Classes.Count > 0) {
        retval += _Classes[_Classes.Count - 1];
      }
      return retval;
    }

  }

}
