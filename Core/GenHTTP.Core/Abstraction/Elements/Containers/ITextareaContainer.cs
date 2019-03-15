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

namespace GenHTTP.Abstraction.Elements.Containers {
  
  /// <summary>
  /// Defines methods which should be implemented by a container
  /// with textarea elements.
  /// </summary>
  public interface ITextareaContainer {

    /// <summary>
    /// Add a new textarea.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="rows">The row count of the field</param>
    /// <param name="cols">The column count of the field</param>
    /// <returns>The created object</returns>
    Textarea AddTextarea(string name, ushort rows, ushort cols);

    /// <summary>
    /// Add a new textarea.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="rows">The row count of the field</param>
    /// <param name="cols">The column count of the field</param>
    /// <param name="value">The value of the textarea</param>
    /// <returns>The created object</returns>
    Textarea AddTextarea(string name, ushort rows, ushort cols, string value);

  }

}
