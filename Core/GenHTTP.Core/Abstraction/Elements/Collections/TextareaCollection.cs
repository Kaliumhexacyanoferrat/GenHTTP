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
  /// Allows you to add textareas to a container.
  /// </summary>
  public class TextareaCollection : ITextareaContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new textarea collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the underlying collection</param>
    public TextareaCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region ITextareaContainer Members

    /// <summary>
    /// Add a new textarea.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="rows">The row count of the field</param>
    /// <param name="cols">The column count of the field</param>
    /// <returns>The created object</returns>
    public Textarea AddTextarea(string name, ushort rows, ushort cols) {
      Textarea area = new Textarea(name, rows, cols);
      _Delegate(area);
      return area;
    }

    /// <summary>
    /// Add a new textarea.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="rows">The row count of the field</param>
    /// <param name="cols">The column count of the field</param>
    /// <param name="value">The value of the textarea</param>
    /// <returns>The created object</returns>
    public Textarea AddTextarea(string name, ushort rows, ushort cols, string value) {
      Textarea area = new Textarea(name, rows, cols, value);
      _Delegate(area);
      return area;
    }

    #endregion

  }

}
