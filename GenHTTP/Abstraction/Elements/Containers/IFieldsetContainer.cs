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
  /// This interface should be implemented by container
  /// with fieldset elements.
  /// </summary>
  public interface IFieldsetContainer {

    /// <summary>
    /// Add a new fieldset.
    /// </summary>
    /// <param name="caption">The caption of the set</param>
    /// <returns>The created object</returns>
    Fieldset AddFieldset(string caption);

  }

}
