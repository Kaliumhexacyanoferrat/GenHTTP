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
  /// Defines the methods of a snippet.
  /// </summary>
  public interface ISnippet {

    /// <summary>
    /// The content length of the snippet.
    /// </summary>
    long ContentLength { get; }

    /// <summary>
    /// Generate the output for this snippet.
    /// </summary>
    /// <returns>The content of this snippet</returns>
    byte[] ToByteArray();

    /// <summary>
    /// Generate the ouput for this snippet.
    /// </summary>
    /// <returns>The content of this snippet</returns>
    string ToString();

  }

}
