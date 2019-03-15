/*

Updated: 2009/10/20

2009/10/20  Andreas Nägeli        Initial version of this file.


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
  /// Specifies the type of a border.
  /// </summary>
  public enum ElementBorderType {
    /// <summary>
    /// In border conflict resolution for table elements.
    /// </summary>
    Hidden,
    /// <summary>
    /// Specifies a dotted border.
    /// </summary>
    Dotted,
    /// <summary>
    /// Specifies a dashed border.
    /// </summary>
    Dashed,
    /// <summary>
    /// Specifies a solid border.
    /// </summary>
    Solid,
    /// <summary>
    /// Specifies a double border.
    /// </summary>
    Double,
    /// <summary>
    /// Specifies a 3D grooved border.
    /// </summary>
    Groove,
    /// <summary>
    /// Specifies a 3D ridged border.
    /// </summary>
    Ridge,
    /// <summary>
    /// Specifies a 3D inset border.
    /// </summary>
    Inset,
    /// <summary>
    /// Specifies a 3D outset border.
    /// </summary>
    Outset
  }

}
