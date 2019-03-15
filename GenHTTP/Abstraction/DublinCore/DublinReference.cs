/*

Updated: 2009/10/16

2009/10/16  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.DublinCore {
  
  /// <summary>
  /// Allows you to reference other sources within
  /// a Dublin Core meta tag.
  /// </summary>
  public class DublinReference {
    private string _Value;
    private bool _IsUri = true;
    private string _Name;

    #region Constructors

    /// <summary>
    /// Create a new reference (as an URL).
    /// </summary>
    /// <param name="name">The name of the resource</param>
    internal DublinReference(string name) {
      _Name = name;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The identifier of the source.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// Define, whether the identifier is an URL.
    /// </summary>
    public bool IsUri {
      get { return _IsUri; }
      set { _IsUri = value; }
    }

    /// <summary>
    /// The name of the ressource.
    /// </summary>
    public string Name {
      get { return _Name; }
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this reference to the given document.
    /// </summary>
    /// <param name="doc">The document to write to</param>
    internal void Serialize(Document doc) {
      if (_Value == null || _Value == "") return;
      if (_IsUri) {
        doc.Header.AdditionalMetaInformation.Add(new DocumentMetaInformation(_Name, "DCTERMS.URI", _Value));
      }
      else {
        doc.Header.AdditionalMetaInformation.Add(_Name, _Value);
      }
    }

    #endregion

  }

}
