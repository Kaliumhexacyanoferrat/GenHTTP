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

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// A textarea.
  /// </summary>
  public class Textarea : StyledElement {
    private string _Value;
    private ushort _Rows;
    private ushort _Cols;
    private bool _Disabled;
    private bool _ReadOnly;
    private string _OnSelect;
    private string _OnChange;
    private ushort _TabIndex;
    private string _OnFocus;
    private string _OnBlur;
    private char _AccessKey;

    #region Constructors

    /// <summary>
    /// Create a new textarea.
    /// </summary>
    /// <param name="name">The name of the textarea</param>
    /// <param name="rows">The row count of the textarea</param>
    /// <param name="cols">The col count of the textarea</param>
    public Textarea(string name, ushort rows, ushort cols) {
      Name = name;
      _Rows = rows;
      _Cols = cols;
    }

    /// <summary>
    /// Create a new textarea.
    /// </summary>
    /// <param name="name">The name of the textarea</param>
    /// <param name="rows">The row count of the textarea</param>
    /// <param name="cols">The col count of the textarea</param>
    /// <param name="value">The value of the textarea</param>
    public Textarea(string name, ushort rows, ushort cols, string value) {
      Name = name;
      _Rows = rows;
      _Cols = cols;
      _Value = value;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The value of the textfield.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// The number of rows.
    /// </summary>
    public ushort Rows {
      get { return _Rows; }
      set { _Rows = value; }
    }

    /// <summary>
    /// The number of columns.
    /// </summary>
    public ushort Cols {
      get { return _Cols; }
      set { _Cols = value; }
    }

    /// <summary>
    /// Define, whether this control is disabled.
    /// </summary>
    public bool Disabled {
      get { return _Disabled; }
      set { _Disabled = value; }
    }

    /// <summary>
    /// Define, whether this control is read-only.
    /// </summary>
    public bool ReadOnly {
      get { return _ReadOnly; }
      set { _ReadOnly = value; }
    }

    /// <summary>
    /// Code which will be executed if the element is selected.
    /// </summary>
    public string OnSelect {
      get { return _OnSelect; }
      set { _OnSelect = value; }
    }

    /// <summary>
    /// Code which will be executed if the value of the element changes.
    /// </summary>
    public string OnChange {
      get { return _OnChange; }
      set { _OnChange = value; }
    }

    /// <summary>
    /// Code which will be executed if the element gets the focus.
    /// </summary>
    public string OnFocus {
      get { return _OnFocus; }
      set { _OnFocus = value; }
    }

    /// <summary>
    /// Code which will be executed, if the element loses the focus.
    /// </summary>
    public string OnBlur {
      get { return _OnBlur; }
      set { _OnBlur = value; }
    }

    /// <summary>
    /// The tabindex of this element.
    /// </summary>
    public ushort TabIndex {
      get { return _TabIndex; }
      set { _TabIndex = value; }
    }

    /// <summary>
    /// The shortcut of this element.
    /// </summary>
    public char AccessKey {
      get { return _AccessKey; }
      set { _AccessKey = value; }
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("<textarea" + ToClassString() + ToXHtml(IsXHtml) + ToCss());
      b.Append(" rows=\"" + _Rows + "\" cols=\"" + _Cols + "\"");
      if (_Disabled) b.Append(" disabled=\"disabled\"");
      if (_ReadOnly) b.Append(" readonly=\"readonly\"");
      if (_TabIndex > 0) b.Append(" tabindex=\"" + _TabIndex + "\"");
      if (_AccessKey != 0) b.Append(" accesskey=\"" + _AccessKey + "\"");
      if (_OnBlur != null && _OnBlur.Length > 0) b.Append(" onblur=\"" + _OnBlur + "\"");
      if (_OnChange != null && _OnChange.Length > 0) b.Append(" onchange=\"" + _OnChange + "\"");
      if (_OnFocus != null && _OnFocus.Length > 0) b.Append(" onfocus=\"" + _OnFocus + "\"");
      if (_OnSelect != null && _OnSelect.Length > 0) b.Append(" onselect=\"" + _OnSelect + "\"");
      b.Append(">");
      if (_Value != null) b.Append(_Value);
      b.Append("</textarea>");
    }

    #endregion

  }

}
