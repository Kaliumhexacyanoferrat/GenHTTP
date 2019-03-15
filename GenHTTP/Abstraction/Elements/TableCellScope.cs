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

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// The scope of table header cells.
  /// </summary>
  public enum TableCellScope {
    /// <summary>
    /// The TD provides header information for the rest of the row.
    /// </summary>
    Row,
    /// <summary>
    /// The TD provides header information for the rest of the column.
    /// </summary>
    Col,
    /// <summary>
    /// The TD gives header information for the rest of the row group.
    /// </summary>
    RowGroup,
    /// <summary>
    /// The TD gives header information for the rest of the column group.
    /// </summary>
    ColGroup,
    /// <summary>
    /// Not specified.
    /// </summary>
    Unspecified
  }

}
