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
  /// Specifies the style of a font.
  /// </summary>
  public enum ElementFontStyle {
    /// <summary>
    /// Normal font style.
    /// </summary>
    Normal,
    /// <summary>
    /// Italic font style.
    /// </summary>
    Italic,
    /// <summary>
    /// Oblique font style.
    /// </summary>
    Oblique,
    /// <summary>
    /// Not specified.
    /// </summary>
    Unspecified
  }

}
