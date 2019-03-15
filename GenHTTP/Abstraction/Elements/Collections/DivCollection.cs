using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows to add div boxes to a container.
  /// </summary>
  public class DivCollection : IDivContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new div collection.
    /// </summary>
    /// <param name="d">The delegate which allows to add elements to the container</param>
    public DivCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region IDivContainer Members

    /// <summary>
    /// Add an empty div to the container.
    /// </summary>
    /// <returns>The created object</returns>
    public Div AddDiv() {
      Div div = new Div();
      _Delegate(div);
      return div;
    }

    /// <summary>
    /// Add an empty div to the container.
    /// </summary>
    /// <param name="id">The ID of the new Div</param>
    /// <returns>The created object</returns>
    public Div AddDiv(string id) {
      Div div = new Div();
      div.ID = id;
      _Delegate(div);
      return div;
    }

    /// <summary>
    /// Add a div to the container.
    /// </summary>
    /// <param name="element">The content of the div box</param>
    /// <returns>The created object</returns>
    public Div AddDiv(Element element) {
      Div div = new Div();
      div.Add(element);
      _Delegate(div);
      return div;
    }

    #endregion

  }

}
