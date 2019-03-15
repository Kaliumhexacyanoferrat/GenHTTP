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
  /// Defines the methods which a container with input elements
  /// should implement.
  /// </summary>
  public interface IInputContainer {

    /// <summary>
    /// Add a new, empty input field.
    /// </summary>
    /// <returns>The created object</returns>
    Input AddInput();

    /// <summary>
    /// Add an input element.
    /// </summary>
    /// <param name="type">The type of the element</param>
    /// <param name="name">The name of the element</param>
    /// <param name="id">The ID of the element</param>
    /// <returns>The created object</returns>
    Input AddInput(InputType type, string name, string id);

    /// <summary>
    /// Add an input element.
    /// </summary>
    /// <param name="type">The type of the element</param>
    /// <param name="name">The name of the element</param>
    /// <param name="id">The ID of the element</param>
    /// <param name="value">The value of the element</param>
    /// <returns>The created object</returns>
    Input AddInput(InputType type, string name, string id, string value);

    /// <summary>
    /// Add a new checkbox.
    /// </summary>
    /// <param name="name">The name of the checkbox</param>
    /// <param name="check">Specify, whether this box should be checked</param>
    /// <returns>The new checkbox</returns>
    Input AddInput(string name, bool check);

  }

}
