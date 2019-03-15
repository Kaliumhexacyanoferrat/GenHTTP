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
  /// Allows you to add option group elements to a container.
  /// </summary>
  public class OptionGroupCollection : IOptionGroupContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new option group collection.
    /// </summary>
    /// <param name="d">The method used to add group elements to the container</param>
    public OptionGroupCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region IOptionGroupContainer Members

    /// <summary>
    /// Add a option group.
    /// </summary>
    /// <param name="label">The label of the group</param>
    /// <returns>The created object</returns>
    public OptionGroup AddOptionGroup(string label) {
      OptionGroup group = new OptionGroup(label);
      _Delegate(group);
      return group;
    }

    /// <summary>
    /// Add a option group.
    /// </summary>
    /// <param name="label">The label of the group</param>
    /// <param name="isDisabled">Specifies, whether this option group is disabled or not</param>
    /// <returns>The created object</returns>
    public OptionGroup AddOptionGroup(string label, bool isDisabled) {
      OptionGroup group = new OptionGroup(label, isDisabled);
      _Delegate(group);
      return group;
    }

    #endregion

  }

}
