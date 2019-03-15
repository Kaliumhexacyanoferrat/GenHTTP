/*

Updated: 2009/10/23

2009/10/23  Andreas Nägeli        Initial version of this file.


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
  /// Allows you to add a fieldset to a container.
  /// </summary>
  public class FieldsetCollection : IFieldsetContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new fieldset collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the underlying container</param>
    public FieldsetCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region IFieldsetContainer Members

    /// <summary>
    /// Add a new fieldset.
    /// </summary>
    /// <param name="caption">The caption of the set</param>
    /// <returns>The created object</returns>
    public Fieldset AddFieldset(string caption) {
      Fieldset set = new Fieldset(caption);
      _Delegate(set);
      return set;
    }

    #endregion

  }

}
