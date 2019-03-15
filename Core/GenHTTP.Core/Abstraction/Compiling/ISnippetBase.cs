/*

Updated: 2009/10/28

2009/10/28  Andreas Nägeli        Initial version of this file.


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
  /// This interface defines methods for a snippet base.
  /// </summary>
  public interface ISnippetBase {

    /// <summary>
    /// Retrieve parts of the snippet base.
    /// </summary>
    /// <param name="nr">The number of the snippet to retrieve</param>
    /// <returns>The content of the part</returns>
    byte[] this[int nr] { get; }

    /// <summary>
    /// The encoding of the related document.
    /// </summary>
    Encoding Encoding { get; }

    /// <summary>
    /// The type of the related document.
    /// </summary>
    DocumentType Type { get; }

  }

}
