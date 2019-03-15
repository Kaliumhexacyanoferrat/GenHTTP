using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows a client to add a table line to a container.
  /// </summary>
  public class TableLineCollection : ITableLineContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new table line collection.
    /// </summary>
    /// <param name="d">The method used to add an element to the container</param>
    public TableLineCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region ITableLineContainer Members

    /// <summary>
    /// Add a new, empty table line.
    /// </summary>
    /// <returns>The created object</returns>
    public TableLine AddTableLine() {
      TableLine line = new TableLine();
      _Delegate(line);
      return line;
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(string[] cells) {
      TableLine line = new TableLine(cells);
      _Delegate(line);
      return line;
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(IEnumerable<string> cells) {
      TableLine line = new TableLine(cells);
      _Delegate(line);
      return line;
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(Element[] cells) {
      TableLine line = new TableLine(cells);
      _Delegate(line);
      return line;
    }

    /// <summary>
    /// Add a table line.
    /// </summary>
    /// <param name="cells">The content of this line</param>
    /// <returns>The created and filled object</returns>
    public TableLine AddTableLine(IEnumerable<Element> cells) {
      TableLine line = new TableLine(cells);
      _Delegate(line);
      return line;
    }

    #endregion

  }

}
