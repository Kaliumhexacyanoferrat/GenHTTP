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

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows you to add table cells to a container.
  /// </summary>
  public class TableCellCollection : ITableCellContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new table cell collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the underlying collection</param>
    public TableCellCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region ITableCellContainer Members

    /// <summary>
    /// Add a new, empty table cell.
    /// </summary>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell() {
      TableCell cell = new TableCell();
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="isHead">Specifiy, whether this cell is a header cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(bool isHead) {
      TableCell cell = new TableCell(isHead);
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(string content) {
      TableCell cell = new TableCell(content);
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(byte colspan) {
      TableCell cell = new TableCell(colspan);
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(string content, byte colspan) {
      TableCell cell = new TableCell(content, colspan);
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="content">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <param name="rowspan">The rowspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(string content, byte colspan, byte rowspan) {
      TableCell cell = new TableCell(content, colspan, rowspan);
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(Element element) {
      TableCell cell = new TableCell(element);
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(Element element, byte colspan) {
      TableCell cell = new TableCell(element, colspan);
      _Delegate(cell);
      return cell;
    }

    /// <summary>
    /// Add a new table cell.
    /// </summary>
    /// <param name="element">The content of the new cell</param>
    /// <param name="colspan">The colspan of the new cell</param>
    /// <param name="rowspan">The rowspan of the new cell</param>
    /// <returns>The created table cell</returns>
    public TableCell AddTableCell(Element element, byte colspan, byte rowspan) {
      TableCell cell = new TableCell(element, colspan, rowspan);
      _Delegate(cell);
      return cell;
    }

    #endregion

  }

}
