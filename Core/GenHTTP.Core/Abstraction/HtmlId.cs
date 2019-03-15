/*

Updated: 2009/10/14

2009/10/14  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Abstraction {
  
  /// <summary>
  /// Represents an identifier in a (X)HTML <see cref="Document" />.
  /// </summary>
  public class HtmlId {
    private static Regex _Pattern;
    private string _Name;

    #region constructors

    /// <summary>
    /// Create a new identifier.
    /// </summary>
    /// <param name="name">The name of the identifier</param>
    public HtmlId(string name) {
      Name = name;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Get or set the name of this identifier.
    /// </summary>
    /// <remarks>
    /// The name of the identifier needs to be unique within the document.
    /// </remarks>
    public string Name {
      get { return _Name; }
      set {
        if (!ValidId(value)) throw new ArgumentException("An HTML ID has to begin with a letter, followed by letters, numbers, hyphens, underscores, colons or periods.", "value");
        _Name = value;
      }
    }

    #endregion

    /// <summary>
    /// Check, whether the given name matches the
    /// ID naming rules or not.
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <returns>true, if the name is valid for an identfier</returns>
    public static bool ValidId(string name) {
      if (_Pattern == null) _Pattern = new Regex(@"[a-zA-Z][a-zA-Z0-9_\:\-\.]*");
      return _Pattern.IsMatch(name);
    }

  }

}
