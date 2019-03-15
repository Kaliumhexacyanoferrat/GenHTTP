/*

Updated: 2009/10/15

2009/10/15  Andreas Nägeli        Initial version of this file.


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
  /// Allows you to store meta information following the
  /// DublinCore (DC) convention.
  /// </summary>
  public class DublinElement {
    private string _Name;
    private Dictionary<string, string> _Values;

    #region Constructors

    /// <summary>
    /// Create a new DC element with a given name.
    /// </summary>
    /// <param name="name">The name of the element</param>
    internal DublinElement(string name) {
      _Name = name;
      _Values = new Dictionary<string, string>();
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Get or set the value of the element.
    /// </summary>
    public string Value {
      get {
        if (_Values.ContainsKey("")) return _Values[""];
        return null;
      }
      set {
        if (!_Values.ContainsKey("")) _Values.Add("", value);
        else _Values[""] = value;
      }
    }

    /// <summary>
    /// Set the value of this element for a given language.
    /// </summary>
    /// <param name="language">The language of the entry</param>
    /// <param name="value">The value of the entry</param>
    public void Set(LanguageInfo language, string value) {
      if (language == null || value == null) throw new ArgumentNullException();
      if (_Values.ContainsKey(language.LanguageString)) _Values[language.LanguageString] = value;
      else _Values.Add(language.LanguageString, value);
    }

    /// <summary>
    /// Retrieve the value of this element for a given language.
    /// </summary>
    /// <param name="language">The language of the entry</param>
    /// <returns>The value of the entry</returns>
    public string Get(LanguageInfo language) {
      if (language == null) throw new ArgumentNullException();
      if (!_Values.ContainsKey(language.LanguageString)) return null;
      return _Values[language.LanguageString];
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize the information of this class to a document.
    /// </summary>
    /// <param name="doc">The related document</param>
    internal void Serialize(Document doc) {
      foreach (string lang in _Values.Keys) {
        DocumentMetaInformation info = new DocumentMetaInformation(_Name, _Values[lang]);
        if (lang != "") info.Internationalization.Language = new LanguageInfo(lang);
        doc.Header.AdditionalMetaInformation.Add(info);
      }
    }

    #endregion

  }

}
