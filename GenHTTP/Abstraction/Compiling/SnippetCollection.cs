/*

Updated: 2009/10/28

2009/10/28  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.Compiling {
  
  /// <summary>
  /// Stores snippets for a project.
  /// </summary>
  public class SnippetCollection {
    private Dictionary<string, ISnippetBase> _Snippets;

    #region Constructors

    internal SnippetCollection() {
      _Snippets = new Dictionary<string, ISnippetBase>();
    }

    #endregion

    #region Collection handling

    /// <summary>
    /// Add a snippet base to this collection.
    /// </summary>
    /// <param name="name">The name of the snippet</param>
    /// <param name="snippet">The snippet to add</param>
    public void Add(string name, ISnippetBase snippet) {
      if (_Snippets.ContainsKey(name)) _Snippets[name] = snippet;
      else _Snippets.Add(name, snippet);
    }

    /// <summary>
    /// Remove a snippet base from this collection.
    /// </summary>
    /// <param name="name">The name of the snippet to remove</param>
    public void Remove(string name) {
      if (_Snippets.ContainsKey(name)) _Snippets.Remove(name);
    }

    /// <summary>
    /// The number of snippets in this collection.
    /// </summary>
    public int Count {
      get { return _Snippets.Count; }
    }

    /// <summary>
    /// Retrieve an enumerator to iterate over all snippets.
    /// </summary>
    /// <returns>The enumerator for this collection</returns>
    public IEnumerator<string> GetEnumerator() {
      return _Snippets.Keys.GetEnumerator();
    }

    /// <summary>
    /// Retrieve a snippet base.
    /// </summary>
    /// <param name="name">The name of the snippet to retrieve</param>
    /// <returns>The requested snippet base</returns>
    public ISnippetBase this[string name] {
      get {
        if (_Snippets.ContainsKey(name)) return _Snippets[name];
        return null;
      }
    }

    #endregion

  }

}
