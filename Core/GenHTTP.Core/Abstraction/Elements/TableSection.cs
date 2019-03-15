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
using GenHTTP.Abstraction.Elements.Collections;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// Defines a table section.
  /// </summary>
  public class TableSection : StyledElementWithChildren, ITableLineContainer {
    private TableLineCollection _Lines;
    private TableSectionType _Type;

    #region Constructors

    /// <summary>
    /// Create a new table section.
    /// </summary>
    /// <param name="type">The type of the section</param>
    public TableSection(TableSectionType type) {
      _Type = type;
      _Lines = new TableLineCollection(Add);
    }

    #endregion

    #region Element management

    /// <summary>
    /// Add an child element to the table section.
    /// </summary>
    /// <param name="element">A table line</param>
    public override void Add(Element element) {
      if (!(element is TableLine)) throw new ArgumentException("A table section can only contain table lines", "element");
      _Children.Add(element);
    }

    #endregion

    #region ITableLineContainer Members

    /// <summary>
    /// Add a new, empty table line.
    /// </summary>
    /// <returns>The created object</returns>
    public TableLine AddTableLine() {
      return _Lines.AddTableLine();
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(string[] cells) {
      return _Lines.AddTableLine(cells);
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(IEnumerable<string> cells) {
      return _Lines.AddTableLine(cells);
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(Element[] cells) {
      return _Lines.AddTableLine(cells);
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(IEnumerable<Element> cells) {
      return _Lines.AddTableLine(cells);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      string tag = "t" + _Type.ToString().ToLower();
      b.Append(" <" + tag + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">\r\n");
      SerializeChildren(b, type);
      b.Append(" </" + tag + ">\r\n");
    }

    #endregion

  }

}
