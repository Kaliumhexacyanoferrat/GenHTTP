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
  /// Specifies the font weight.
  /// </summary>
  public enum ElementFontWeight {
    /// <summary>
    /// Defines normal characters.
    /// </summary>
    Normal,
    /// <summary>
    /// Defines thick characters.
    /// </summary>
    Bold,
    /// <summary>
    /// Defines thicker characters.
    /// </summary>
    Bolder,
    /// <summary>
    /// Defines lighter characters.
    /// </summary>
    Lighter,
    /// <summary>
    /// Not specified.
    /// </summary>
    Unspecified
  }

}
