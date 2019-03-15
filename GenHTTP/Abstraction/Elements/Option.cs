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
  /// Describes a list entry of a <see cref="Select"/> element.
  /// </summary>
  public class Option : StyledElement {
    private bool _Selected;
    private bool _Disabled;
    private string _Label;
    private string _Value;
    private string _Content;

    #region Constructors

    /// <summary>
    /// Create a new option.
    /// </summary>
    /// <param name="content">The description of the option</param>
    public Option(string content) {
      Content = content;
    }

    /// <summary>
    /// Create a new option.
    /// </summary>
    /// <param name="content">The description of the option</param>
    /// <param name="selected">Specifies, whether this option should be selected</param>
    public Option(string content, bool selected) {
      Content = content;
      _Selected = selected;
    }

    /// <summary>
    /// Create a new option.
    /// </summary>
    /// <param name="content">The description of the option</param>
    /// <param name="value">The value of this entry</param>
    public Option(string content, string value) {
      Content = content;
      _Value = value;
    }

    /// <summary>
    /// Create a new option.
    /// </summary>
    /// <param name="content">The description of the option</param>
    /// <param name="value">The value of this entry</param>
    /// <param name="selected">Specifies, whether this option should be select</param>
    public Option(string content, string value, bool selected) {
      Content = content;
      _Value = value;
      _Selected = selected;
    }

    /// <summary>
    /// Create a new option.
    /// </summary>
    /// <param name="content">The description of the option</param>
    /// <param name="value">The value of this entry</param>
    /// <param name="label">The label of the option</param>
    public Option(string content, string value, string label) {
      Content = content;
      _Value = value;
      _Label = label;
    }

    /// <summary>
    /// Create a new option.
    /// </summary>
    /// <param name="content">The description of the option</param>
    /// <param name="value">The value of this entry</param>
    /// <param name="label">The label of the option</param>
    /// <param name="selected">Specifies, whether this option should be select</param>
    public Option(string content, string value, string label, bool selected) {
      Content = content;
      _Value = value;
      _Label = label;
      _Selected = selected;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Specifies, whether this list entry is selected.
    /// </summary>
    public bool Selected {
      get { return _Selected; }
      set { _Selected = value; }
    }

    /// <summary>
    /// Specifies, whether this list entry is disabled.
    /// </summary>
    public bool Disabled {
      get { return _Disabled; }
      set { _Disabled = value; }
    }

    /// <summary>
    /// The label of this list entry.
    /// </summary>
    public string Label {
      get { return _Label; }
      set { _Label = value; }
    }

    /// <summary>
    /// The value of this list entry.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// The description of this list entry.
    /// </summary>
    public string Content {
      get { return _Content; }
      set {
        if (value == null || value.Length == 0) throw new ArgumentException("The content of a select box cannot be null or empty");
        _Content = value;
      }
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("  <option" + ToClassString() + ToXHtml(IsXHtml) + ToCss());
      if (_Selected) b.Append(" selected=\"selected\"");
      if (_Disabled) b.Append(" disabled=\"disabled\"");
      if (_Label != null && _Label.Length > 0) b.Append(" label=\"" + _Label + "\"");
      if (_Value != null && _Value.Length > 0) b.Append(" value=\"" + _Value + "\"");
      b.Append(">" + _Content + "</option>\r\n");
    }

    #endregion

  }

}
