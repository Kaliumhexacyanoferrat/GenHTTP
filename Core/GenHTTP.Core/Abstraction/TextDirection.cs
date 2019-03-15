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

namespace GenHTTP.Abstraction {
  
  /// <summary>
  /// Direction for weak/neutral text.
  /// </summary>
  public enum TextDirection {
    /// <summary>
    /// Write the text from the right to the left.
    /// </summary>
    RightToLeft,
    /// <summary>
    /// Write the text from the left to the right.
    /// </summary>
    LeftToRight,
    /// <summary>
    /// Do not specify this value for the element
    /// or <see cref="Document" />.
    /// </summary>
    Unspecified
  }

}
