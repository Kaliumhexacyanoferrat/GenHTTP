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
  /// Clears the flow of an element.
  /// </summary>
  public enum ElementClear {
    /// <summary>
    /// Default.
    /// </summary>
    None,
    /// <summary>
    /// Left flow.
    /// </summary>
    Left,
    /// <summary>
    /// Right flow.
    /// </summary>
    Right,
    /// <summary>
    /// Flow.
    /// </summary>
    Both,
    /// <summary>
    /// Not specified.
    /// </summary>
    Unspecified
  }

}
