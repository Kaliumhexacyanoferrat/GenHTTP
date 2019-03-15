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

using GenHTTP.Abstraction.Style;
using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows you to add span elements to a container.
  /// </summary>
  public class SpanCollection : ISpanContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new span collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the underlying container</param>
    public SpanCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region ISpanContainer Members

    /// <summary>
    /// Add a new, empty span.
    /// </summary>
    /// <returns>The created object</returns>
    public Span AddSpan() {
      Span span = new Span();
      _Delegate(span);
      return span;
    }

    /// <summary>
    /// Add a new span.
    /// </summary>
    /// <param name="text">The content of the span</param>
    /// <param name="decoration">The text decoration to use</param>
    /// <returns>The created object</returns>
    public Span AddSpan(string text, ElementTextDecoration decoration) {
      Span span = new Span(text, decoration);
      _Delegate(span);
      return span;
    }

    /// <summary>
    /// Add a new span.
    /// </summary>
    /// <param name="text">The content of the span</param>
    /// <param name="fontWeight">The font weight to use</param>
    /// <returns>The created object</returns>
    public Span AddSpan(string text, ElementFontWeight fontWeight) {
      Span span = new Span(text, fontWeight);
      _Delegate(span);
      return span;
    }

    #endregion

  }

}
