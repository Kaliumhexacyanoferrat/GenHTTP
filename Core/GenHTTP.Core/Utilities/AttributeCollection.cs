using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Utilities {

  /// <summary>
  /// Stores attributes of a <see cref="Setting" /> entry.
  /// </summary>
  public class AttributeCollection {
    /// <summary>
    /// Stores the attributes to handle.
    /// </summary>
    protected Dictionary<string, string> _Attributes;

    /// <summary>
    /// Create a new, empty attribute collection.
    /// </summary>
    internal AttributeCollection() {
      _Attributes = new Dictionary<string, string>();
    }

    /// <summary>
    /// Retrieve the value of an attribute.
    /// </summary>
    /// <param name="attribute">The name of the attribute to recieve</param>
    /// <returns>The value of the attribute</returns>
    public string this[string attribute] {
      get {
        if (_Attributes.ContainsKey(attribute)) return _Attributes[attribute];
        return "";
      }
      set {
        if (_Attributes.ContainsKey(attribute)) _Attributes[attribute] = (value == null) ? "" : value;
        else _Attributes.Add(attribute, (value == null) ? "" : value);
      }
    }

    /// <summary>
    /// Retrieve an enumerator to iterate over the list of attributes.
    /// </summary>
    /// <returns>The requested iterator</returns>
    public IEnumerator<string> GetEnumerator() {
      return _Attributes.Keys.GetEnumerator();
    }

  }

}
