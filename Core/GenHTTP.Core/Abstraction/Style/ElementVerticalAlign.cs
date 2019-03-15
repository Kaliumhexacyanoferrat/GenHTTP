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
  /// Sepcifies the vertical text alignment.
  /// </summary>
  public enum ElementVerticalAlign {
    /// <summary>
    /// Aligns the element as it was subscript.
    /// </summary>
    Sub,
    /// <summary>
    /// Aligns the element as it was superscript.
    /// </summary>
    Super,
    /// <summary>
    /// The top of the element is aligned with the top of the tallest element on the line.
    /// </summary>
    Top,
    /// <summary>
    /// The top of the element is aligned with the top of the parent element's font.
    /// </summary>
    TextTop,
    /// <summary>
    /// The element is placed in the middle of the parent element.
    /// </summary>
    Middle,
    /// <summary>
    /// The bottom of the element is aligned with the lowest element on the line.
    /// </summary>
    Bottom,
    /// <summary>
    /// The bottom of the element is aligned with the bottom of the parent element's font.
    /// </summary>
    TextBottom,
    /// <summary>
    /// Not specified.
    /// </summary>
    Unspecified
  }

}
