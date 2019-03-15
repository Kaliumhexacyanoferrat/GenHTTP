/*

Updated: 2009/10/22

2009/10/22  Andreas Nägeli        Initial version of this file.


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
  /// This interface describes, which methods a container for
  /// table lines must provide.
  /// </summary>
  public interface ITableLineContainer {

    /// <summary>
    /// Add a new, empty table line.
    /// </summary>
    /// <returns>The created object</returns>
    TableLine AddTableLine();

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    TableLine AddTableLine(string[] cells);

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    TableLine AddTableLine(IEnumerable<string> cells);

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    TableLine AddTableLine(Element[] cells);

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    TableLine AddTableLine(IEnumerable<Element> cells);

  }

}
