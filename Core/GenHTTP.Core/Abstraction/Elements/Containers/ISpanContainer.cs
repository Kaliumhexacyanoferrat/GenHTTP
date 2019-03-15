/*

Updated: 2009/10/23

2009/10/23  Andreas Nägeli        Initial version of this file.


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

using GenHTTP.Abstraction.Style;

namespace GenHTTP.Abstraction.Elements.Containers {
  
  /// <summary>
  /// Provides methods which containers should implement if
  /// they contain span elements.
  /// </summary>
  public interface ISpanContainer {

    /// <summary>
    /// Add a new, empty span.
    /// </summary>
    /// <returns>The created object</returns>
    Span AddSpan();

    /// <summary>
    /// Add a new span.
    /// </summary>
    /// <param name="text">The content of the span</param>
    /// <param name="decoration">The text decoration to use</param>
    /// <returns>The created object</returns>
    Span AddSpan(string text, ElementTextDecoration decoration);

    /// <summary>
    /// Add a new span.
    /// </summary>
    /// <param name="text">The content of the span</param>
    /// <param name="fontWeight">The font weight to use</param>
    /// <returns>The created object</returns>
    Span AddSpan(string text, ElementFontWeight fontWeight);

  }

}
