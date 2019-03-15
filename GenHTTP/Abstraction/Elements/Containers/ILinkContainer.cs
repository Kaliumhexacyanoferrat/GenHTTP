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
  /// Allows you to add links to a container.
  /// </summary>
  public interface ILinkContainer {

    /// <summary>
    /// Add an empty link.
    /// </summary>
    /// <returns>The created object</returns>
    Link AddLink();

    /// <summary>
    /// Add a new link.
    /// </summary>
    /// <param name="url">The URL to link</param>
    /// <returns>The created object</returns>
    Link AddLink(string url);

    /// <summary>
    /// Add a new link.
    /// </summary>
    /// <param name="url">The URL to link</param>
    /// <param name="linkText">The link text</param>
    /// <returns>The created object</returns>
    Link AddLink(string url, string linkText);

    /// <summary>
    /// Add a new link.
    /// </summary>
    /// <param name="url">The URL to link</param>
    /// <param name="element">The link element</param>
    /// <returns>The created object</returns>
    Link AddLink(string url, Element element);

  }

}
