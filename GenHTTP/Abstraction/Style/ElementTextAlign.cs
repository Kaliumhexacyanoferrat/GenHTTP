/*

Updated: 2009/10/19

2009/10/19  Andreas Nägeli        Initial version of this file.


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
  /// Describes the alignment of text.
  /// </summary>
  public enum ElementTextAlign {
    /// <summary>
    /// Left.
    /// </summary>
    Left,
    /// <summary>
    /// Center.
    /// </summary>
    Center,
    /// <summary>
    /// Right.
    /// </summary>
    Right,
    /// <summary>
    /// Unspecified.
    /// </summary>
    Unspecified
  }

}
