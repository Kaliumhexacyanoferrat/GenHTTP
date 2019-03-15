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

namespace GenHTTP.Abstraction.Elements.Containers {
  
  /// <summary>
  /// Defines the methods which every element containing
  /// text elements has to provide.
  /// </summary>
  public interface ITextContainer {

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    void Print(string text);

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
    void Print(string text, bool escapeEntities);

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <returns>The newly created object</returns>
    Text AddText(string text);

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
    /// <returns>The newly created object</returns>
    Text AddText(string text, bool escapeEntities);

    /// <summary>
    /// Add an empty text element to the element.
    /// </summary>
    /// <returns>The newly created object</returns>
    Text AddText();

  }

}
