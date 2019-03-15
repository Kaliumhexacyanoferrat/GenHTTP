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

using GenHTTP.Abstraction.Elements.Collections;
using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// Contains some options of a <see cref="Select"/> element.
  /// </summary>
  public class OptionGroup : StyledElementWithChildren, IOptionContainer {
    private bool _Disabled;
    private string _Label;
    private OptionCollection _OptionElements;

    #region Constructors

    /// <summary>
    /// Create a new option group.
    /// </summary>
    /// <param name="label">The label of the group</param>
    public OptionGroup(string label) {
      Label = label;
      _OptionElements = new OptionCollection(new AddElement(Add));
    }

    /// <summary>
    /// Create a new option group.
    /// </summary>
    /// <param name="label">The label of the group</param>
    /// <param name="disabled">Specifies, whether this group is disabled or not</param>
    public OptionGroup(string label, bool disabled) : this(label) {
      _Disabled = disabled;
    }

    #endregion

    #region Element management

    /// <summary>
    /// Add an element to this list.
    /// </summary>
    /// <param name="element">The element to add</param>
    public override void Add(Element element) {
      if (!(element is Option)) throw new ArgumentException("You can only add options and option-groups to this container");
      base.Add(element);
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Specifies, whether this option group is disabled or not.
    /// </summary>
    public bool Disabled {
      get { return _Disabled; }
      set { _Disabled = value; }
    }

    /// <summary>
    /// The label of the option group.
    /// </summary>
    public string Label {
      get { return _Label; }
      set {
        if (value == null || value.Length == 0) throw new ArgumentException("The label of an option group cannot be null or empty");
        _Label = value;
      }
    }

    #endregion

    #region IOptionContainer Members

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content) {
      return _OptionElements.AddOption(content);
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="selected">Specifies, whether the list entry should be selected</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, bool selected) {
      return _OptionElements.AddOption(content, selected);
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value) {
      return _OptionElements.AddOption(content, value);
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <param name="selected">Specifies, whether the list entry should be selected</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value, bool selected) {
      return _OptionElements.AddOption(content, value, selected);
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <param name="label">The label of the element</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value, string label) {
      return _OptionElements.AddOption(content, value, label);
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <param name="label">The label of the element</param>
    /// <param name="selected">Specifies, whether the list entry should be selected</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value, string label, bool selected) {
      return _OptionElements.AddOption(content, value, label, selected);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append(" <optgroup" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + " label=\"" + _Label + "\"");
      if (_Disabled) b.Append(" disabled=\"disabled\"");
      b.Append(">\r\n");
      SerializeChildren(b, type);
      b.Append(" </optgroup>\r\n");
    }

    #endregion

  }

}
