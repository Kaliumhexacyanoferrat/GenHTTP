/*

Updated: 2009/10/19

2009/10/19  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.Style {
  
  /// <summary>
  /// Specifies the size of an element.
  /// </summary>
  public class ElementSize {
    private ushort _Size;
    private ElementSizeType _Type = ElementSizeType.Px;

    #region Constructors

    /// <summary>
    /// Create a new object of this class.
    /// </summary>
    public ElementSize() {

    }

    /// <summary>
    /// Create a new object with a given size.
    /// </summary>
    /// <param name="size">The element's size</param>
    public ElementSize(ushort size) {
      _Size = size;
    }

    /// <summary>
    /// Create a new object with a given size and a given type.
    /// </summary>
    /// <param name="size">The size of the element</param>
    /// <param name="type">The type of the element's size</param>
    public ElementSize(ushort size, ElementSizeType type) : this(size) {
      _Type = type;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The size of the element.
    /// </summary>
    public ushort Size {
      get { return _Size; }
      set { _Size = value; }
    }

    /// <summary>
    /// The type of the element's size.
    /// </summary>
    public ElementSizeType Type {
      get { return _Type; }
      set { _Type = value; }
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <returns>The serialized content of this class</returns>
    public string ToCss() {
      if (_Size > 0) return _Size + GetSizeType(_Type);
      return "";
    }

    #endregion

    #region Enum conversion

    /// <summary>
    /// Retrieve the string representation of a element size type.
    /// </summary>
    /// <param name="type">The type to convert</param>
    /// <returns>The converted type</returns>
    public static string GetSizeType(ElementSizeType type) {
      if (type == ElementSizeType.None) return "";
      if (type == ElementSizeType.Percent) return "%";
      string ret = type.ToString();
      return ret.ToLower().Substring(0, 1) + ret.Substring(1);
    }

    #endregion

  }

}
