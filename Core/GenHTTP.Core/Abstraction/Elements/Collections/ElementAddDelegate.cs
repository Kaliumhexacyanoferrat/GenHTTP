/*

Updated: 2009/10/20

2009/10/20  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// The signature of the method used to add an element
  /// to a <see cref="StyledContainerElement"/>.
  /// </summary>
  /// <param name="e">The element to add</param>
  public delegate void AddElement(Element e);

}
