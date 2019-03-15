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
  /// Contains table columns.
  /// </summary>
  public class TableColumnCollection : Element {
    private List<TableColumn> _Columns;

    #region Constructor

    /// <summary>
    /// Create a new, empty table column collection.
    /// </summary>
    public TableColumnCollection() {
      _Columns = new List<TableColumn>();
    }

    /// <summary>
    /// Create a new table column collection with one column.
    /// </summary>
    /// <param name="width">The width of the column to add</param>
    public TableColumnCollection(byte width) : this() {
      _Columns.Add(new TableColumn(width));
    }

    /// <summary>
    /// Create a new table collection with some columns.
    /// </summary>
    /// <param name="widths">The widths of the columns to add</param>
    public TableColumnCollection(byte[] widths) : this() {
      foreach (byte width in widths) _Columns.Add(new TableColumn(width));
    }

    /// <summary>
    /// Create a new table collection with some columns.
    /// </summary>
    /// <param name="widths">The widths of the columns to add</param>
    public TableColumnCollection(IEnumerable<byte> widths) : this() {
      foreach (byte width in widths) _Columns.Add(new TableColumn(width));
    }

    #endregion

    #region Element handling

    /// <summary>
    /// Add a table column to the collection.
    /// </summary>
    /// <param name="column">The column to add</param>
    /// <remarks>
    /// Note: you can add a column more than once.
    /// </remarks>
    public void Add(TableColumn column) {
      _Columns.Add(column);
    }

    /// <summary>
    /// Add a column with a specified width.
    /// </summary>
    /// <param name="width">The width of the column</param>
    public void Add(byte width) {
      Add(new TableColumn(width));
    }

    /// <summary>
    /// Add some columns.
    /// </summary>
    /// <param name="widths">The widths of the columns to add</param>
    public void Add(byte[] widths) {
      foreach (byte b in widths) Add(b);
    }

    /// <summary>
    /// Add some columns.
    /// </summary>
    /// <param name="widths">The widths of the columns to add</param>
    public void Add(IEnumerable<byte> widths) {
      foreach (byte b in widths) Add(b);
    }

    /// <summary>
    /// The number of columns in this collection.
    /// </summary>
    public int Count {
      get { return _Columns.Count; }
    }

    /// <summary>
    /// Retrieve an enumerator to iterate over this collection.
    /// </summary>
    /// <returns>An enumerator to iterate over this collection</returns>
    public IEnumerator<TableColumn> GetEnumerator() {
      return _Columns.GetEnumerator();
    }

    /// <summary>
    /// Remove a column from this collection.
    /// </summary>
    /// <param name="col">The column to remove</param>
    public void Remove(TableColumn col) {
      if (_Columns.Contains(col)) _Columns.Remove(col);
    }

    /// <summary>
    /// Remove a column from this collection.
    /// </summary>
    /// <param name="nr">The number of the collection within this collection</param>
    public void Remove(int nr) {
      if (nr >= 0 && nr < Count) _Columns.RemoveAt(nr);
    }

    /// <summary>
    /// Retrieve a table column by its number.
    /// </summary>
    /// <param name="nr"></param>
    /// <returns></returns>
    public TableColumn this[int nr] {
      get {
        if (nr >= 0 && nr < Count) return _Columns[nr];
        return null;
      }
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("  <colgroup" + ToXHtml(IsXHtml) + ">\r\n");
      foreach (TableColumn col in _Columns) col.Serialize(b, type);
      b.Append("  </colgroup>\r\n");
    }

    #endregion

  }

}
