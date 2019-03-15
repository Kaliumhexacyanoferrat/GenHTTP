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
  /// Describes, which methods a container with form
  /// child elements must implement.
  /// </summary>
  public interface IFormContainer {

    /// <summary>
    /// Add a new form.
    /// </summary>
    /// <param name="action">The URL of the file to invoke on submit</param>
    /// <returns>The created object</returns>
    /// <remarks>
    /// This method will create a form using the HTTP POST method.
    /// </remarks>
    Form AddForm(string action);

    /// <summary>
    /// Add a new form.
    /// </summary>
    /// <param name="action">The URL of the file to invoke on submit</param>
    /// <param name="method">The HTTP method to use</param>
    /// <returns>The created object</returns>
    Form AddForm(string action, FormMethod method);

  }

}
