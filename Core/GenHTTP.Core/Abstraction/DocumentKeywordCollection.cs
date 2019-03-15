/*

Updated: 2009/10/13

2009/10/13  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction {
  
  /// <summary>
  /// Stores keywords of a <see cref="DocumentHeader"/>.
  /// </summary>
  public class DocumentKeywordCollection {
    private List<string> _Keywords;

    #region Constructors

    /// <summary>
    /// Create a new keyword collection.
    /// </summary>
    internal DocumentKeywordCollection() {
      _Keywords = new List<string>();
    }

    #endregion

    #region Collection functionality

    /// <summary>
    /// Add a keyword to this collection.
    /// </summary>
    /// <param name="keyword">The keyword to add</param>
    public void Add(string keyword) {
      if (keyword == null) throw new ArgumentNullException();
      if (!_Keywords.Contains(keyword)) _Keywords.Add(keyword);
    }

    /// <summary>
    /// Add some keywords to this collection.
    /// </summary>
    /// <param name="keywords">The keywords to add</param>
    public void Add(string[] keywords) {
      if (keywords == null) throw new ArgumentNullException();
      foreach (string keyword in keywords) if (keyword != null) Add(keyword);
    }

    /// <summary>
    /// Remove a keyword from this collection.
    /// </summary>
    /// <param name="keyword">The keyword to remove</param>
    public void Remove(string keyword) {
      if (keyword == null) throw new ArgumentNullException();
      if (_Keywords.Contains(keyword)) _Keywords.Remove(keyword);
    }

    /// <summary>
    /// Retrieve the number of elements in this collection.
    /// </summary>
    public int Count {
      get { return _Keywords.Count; }
    }

    #endregion

    #region Conversion

    /// <summary>
    /// Convert this collection into a <see cref="DocumentMetaInformation"/>
    /// object.
    /// </summary>
    /// <returns>The corresponding meta information object</returns>
    internal DocumentMetaInformation ToMetaInformation() {
      StringBuilder b = new StringBuilder();
      foreach (string keyword in _Keywords) {
        b.Append("," + DocumentEncoding.ConvertString(keyword));
      }
      b.Remove(0, 1);
      return new DocumentMetaInformation("keywords", b.ToString());
    }

    #endregion

  }

}
