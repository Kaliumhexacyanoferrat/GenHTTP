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
  /// A template, used to seperate static from dynamic content.
  /// </summary>
  public interface ITemplate {

    /// <summary>
    /// Retrieve the whole content of the template.
    /// </summary>
    /// <returns></returns>
    byte[] ToByteArray();

    /// <summary>
    /// Retrieve the template base of this template.
    /// </summary>
    ITemplateBase Base { get; }

  }

}
