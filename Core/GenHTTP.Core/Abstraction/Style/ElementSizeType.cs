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
  /// Defines the different sizing types.
  /// </summary>
  public enum ElementSizeType {
    /// <summary>
    /// The 'font-size' of the relevant font.
    /// </summary>
    Em,
    /// <summary>
    /// The 'x-height' of the relevant font.
    /// </summary>
    Ex,
    /// <summary>
    /// Pixels, relative to the viewing device.
    /// </summary>
    Px,
    /// <summary>
    /// Inches.
    /// </summary>
    In,
    /// <summary>
    /// Centimeters.
    /// </summary>
    Cm,
    /// <summary>
    /// Millimeters.
    /// </summary>
    Mm,
    /// <summary>
    /// Points.
    /// </summary>
    Pt,
    /// <summary>
    /// Picas.
    /// </summary>
    Pc,
    /// <summary>
    /// Percent. Used for container elements.
    /// </summary>
    Percent,
    /// <summary>
    /// No size.
    /// </summary>
    None
  }

}
