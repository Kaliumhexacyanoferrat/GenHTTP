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
  /// Defines, what methods a container for table cells
  /// must provide.
  /// </summary>
  public interface ITableCellContainer {

    /// <summary>
    /// Add a new, empty table cell.
    /// </summary>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell();

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="isHead">Specifiy, whether this cell is a header cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(bool isHead);

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(string content);

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(byte colspan);

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(string content, byte colspan);

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <param name="rowspan">The rowspan of the new cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(string content, byte colspan, byte rowspan);

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(Element element);

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(Element element, byte colspan);

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <param name="rowspan">The rowspan of the new cell</param>
    /// <returns>The created table cell</returns>
    TableCell AddTableCell(Element element, byte colspan, byte rowspan);

  }

}
