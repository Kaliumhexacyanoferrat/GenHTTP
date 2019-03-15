using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP.Localization {
  
  /// <summary>
  /// Represents a localization for a specific language.
  /// </summary>
  public class Localization {
    private Setting _Source;

    /// <summary>
    /// Create a new localization object.
    /// </summary>
    /// <param name="source">The source section to read</param>
    internal Localization(Setting source) {
      _Source = source;
    }

    #region get-/setters

    /// <summary>
    /// Get or set the name of the language of this localization.
    /// </summary>
    public string Name {
      get {
        return _Source.Attributes["language"].ToLower();
      }
      set {
        if (value == null) throw new ArgumentNullException();
        if (value == "") throw new ArgumentException();
        _Source.Attributes["language"] = value.ToLower();
      }
    }

    /// <summary>
    /// Retrieve a children of the localization node.
    /// </summary>
    /// <param name="name">The name of the child</param>
    /// <returns>The requested child</returns>
    /// <example>
    /// If you want to access the string "mypage.welcome" you can
    /// access this value via this code:
    /// 
    /// <code>
    /// Localization loc;
    /// Console.WriteLine(loc["mypage"]["welcome"].Value);
    /// </code>
    /// </example>
    public Setting this[string name] {
      get { return _Source[name]; }
    }

    /// <summary>
    /// The node of the tree used to store localization data.
    /// </summary>
    public Setting Node {
      get { return _Source; }
    }

    #endregion
   
  }

}
