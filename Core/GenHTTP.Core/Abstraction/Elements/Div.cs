/*

Updated: 2009/10/21

2009/10/21  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// A container box.
  /// </summary>
  public class Div : StyledContainerElement {

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("\r\n<div" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">\r\n");
      SerializeChildren(b, type);
      b.Append("\r\n</div>\r\n");
    }

    #endregion

  }

}
