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

namespace GenHTTP.Abstraction.Elements.Containers {
  
  /// <summary>
  /// Defines, how the user can add a 'div' element
  /// to a container.
  /// </summary>
  public interface IDivContainer {

    /// <summary>
    /// Add an empty div to the container.
    /// </summary>
    /// <returns>The created object</returns>
    Div AddDiv();

    /// <summary>
    /// Add an empty div to the container.
    /// </summary>
    /// <param name="id">The ID of the new Div</param>
    /// <returns>The created object</returns>
    Div AddDiv(string id);

    /// <summary>
    /// Add a div to the container.
    /// </summary>
    /// <param name="element">The content of the div box</param>
    /// <returns>The created object</returns>
    Div AddDiv(Element element);

  }

}
