using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Provides methods to add table sections to a container.
  /// </summary>
  public class TableSectionCollection : ITableSectionContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new table section collection.
    /// </summary>
    /// <param name="d">The method used to add elemenets to the underlying container</param>
    public TableSectionCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region ITableSectionContainer Members

    /// <summary>
    /// Add a new table section.
    /// </summary>
    /// <param name="type">The type of the table section</param>
    /// <returns>The created object</returns>
    public TableSection AddTableSection(TableSectionType type) {
      TableSection sect = new TableSection(type);
      _Delegate(sect);
      return sect;
    }

    #endregion

  }

}
