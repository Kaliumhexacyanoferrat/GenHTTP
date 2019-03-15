/*

Updated: 2009/10/22

2009/10/22  Andreas Nägeli        Initial version of this file.


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
  /// Provides methods which must be implemented by container 
  /// with headline elements.
  /// </summary>
  public interface IHeadlineContainer {

    /// <summary>
    /// Add a new headline.
    /// </summary>
    /// <param name="value">The value of the headline</param>
    /// <returns>The created object</returns>
    Headline AddHeadline(string value);

    /// <summary>
    /// Add a new headline.
    /// </summary>
    /// <param name="value">The value of the headline</param>
    /// <param name="size">The size of the headline (from 1 to 6)</param>
    /// <returns>The created object</returns>
    Headline AddHeadline(string value, byte size);

  }

}
