/*

Updated: 2009/10/30

2009/10/30  Andreas Nägeli        Initial version of this file.


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
using System.Text.RegularExpressions;

using GenHTTP.Abstraction.Elements;

namespace GenHTTP.Content {
  
  /// <summary>
  /// A parameter given to a script.
  /// </summary>
  public class Parameter {
    private string _Name;
    private FormMethod _Method;
    private string _Value;
    private bool _Required;
    private Regex _Pattern;

    #region Constructors

    /// <summary>
    /// Create a new parameter.
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="method">The type of the parameter</param>
    /// <param name="required">Defines, whether the parameter is required or not</param>
    /// <param name="pattern">The pattern this parameter needs to follow</param>
    public Parameter(string name, FormMethod method, bool required, string pattern) {
      _Name = name;
      _Method = method;
      _Required = required;
      if (pattern != null) _Pattern = new Regex(pattern);
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The name of the parameter
    /// </summary>
    public string Name {
      get { return _Name; }
    }

    /// <summary>
    /// The type of the parameter.
    /// </summary>
    public FormMethod Method {
      get { return _Method; }
    }

    /// <summary>
    /// Defines, whether the parameter is required or not.
    /// </summary>
    public bool Required {
      get { return _Required; }
      set { _Required = value; }
    }

    /// <summary>
    /// The value of the parameter.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// Convert the value of the parameter.
    /// </summary>
    /// <typeparam name="T">The type to convert to</typeparam>
    /// <param name="val">The default value if the conversion fails</param>
    /// <returns>The converted value of the parameter</returns>
    public T ConvertTo<T>(T val) {
      try {
        return (T)Convert.ChangeType(_Value, typeof(T));
      }
      catch {
        return val;
      }
    }

    /// <summary>
    /// Specifies, whether the given pattern matches the value
    /// of the parameter or not.
    /// </summary>
    public bool PatternMissmatch {
      get {
        if (_Pattern != null) return !_Pattern.IsMatch(_Value);
        return false;
      }
    }

    #endregion

  }

}
