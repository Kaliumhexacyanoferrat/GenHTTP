/*

Updated: 2009/10/21

2009/10/21  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.Style {
  
  /// <summary>
  /// Specifies the decoration of text.
  /// </summary>
  public enum ElementTextDecoration {
    /// <summary>
    /// No text decoration.
    /// </summary>
    None,
    /// <summary>
    /// Underlined text.
    /// </summary>
    Underline,
    /// <summary>
    /// Overlined text.
    /// </summary>
    Overline,
    /// <summary>
    /// Lined trough text.
    /// </summary>
    LineThrough,
    /// <summary>
    /// Blinking text.
    /// </summary>
    Blink,
    /// <summary>
    /// Not specified.
    /// </summary>
    Unspecified
  }

}
