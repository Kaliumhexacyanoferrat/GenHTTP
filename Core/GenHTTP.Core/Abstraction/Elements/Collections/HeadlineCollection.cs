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
  /// Allows you to add headlines to a container.
  /// </summary>
  public class HeadlineCollection : IHeadlineContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new headline collection.
    /// </summary>
    /// <param name="d">The method used to add an element to the underlying container</param>
    public HeadlineCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region IHeadlineContainer Members

    /// <summary>
    /// Add a new headline.
    /// </summary>
    /// <param name="value">The value of the headline</param>
    /// <returns>The created object</returns>
    public Headline AddHeadline(string value) {
      Headline line = new Headline(value);
      _Delegate(line);
      return line;
    }

    /// <summary>
    /// Add a new headline.
    /// </summary>
    /// <param name="value">The value of the headline</param>
    /// <param name="size">The size of the headline (from 1 to 6)</param>
    /// <returns>The created object</returns>
    public Headline AddHeadline(string value, byte size) {
      Headline line = new Headline(value, size);
      _Delegate(line);
      return line;
    }

    #endregion

  }

}
