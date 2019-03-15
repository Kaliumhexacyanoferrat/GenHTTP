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

namespace GenHTTP.Abstraction.Compiling {

  /// <summary>
  /// Stores the static content of a template.
  /// </summary>
  public interface ITemplateBase {

    /// <summary>
    /// Retrieve a static part.
    /// </summary>
    /// <param name="nr">The number of the part to retrive</param>
    /// <returns>The requested part</returns>
    byte[] this[int nr] { get; }

    /// <summary>
    /// The encoding used for this template.
    /// </summary>
    Encoding Encoding { get; }

    /// <summary>
    /// The content length of the static content.
    /// </summary>
    long ContentLength { get; }

    /// <summary>
    /// The type of the document.
    /// </summary>
    DocumentType Type { get; }

  }

}
