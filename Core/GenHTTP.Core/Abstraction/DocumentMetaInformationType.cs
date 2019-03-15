/*

Updated: 2009/10/14

2009/10/14  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction {
  
  /// <summary>
  /// Defines the available types of meta information.
  /// </summary>
  /// <remarks>
  /// The W3C standards allow the 'name' and the 
  /// 'http-equiv' attribute in a 'meta' tag at the same
  /// time. The GenHTTP object framework does not provide
  /// this feature.
  /// </remarks>
  public enum DocumentMetaInformationType {
    /// <summary>
    /// Meta tag with the 'name' attribute.
    /// </summary>
    Normal,
    /// <summary>
    /// Meta tag with the 'http-equiv' attribute.
    /// </summary>
    HttpEquivalent
  }

}
