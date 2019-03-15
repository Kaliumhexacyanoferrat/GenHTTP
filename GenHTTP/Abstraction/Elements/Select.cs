using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;
using GenHTTP.Abstraction.Elements.Collections;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// A selection list control.
  /// </summary>
  public class Select : StyledElementWithChildren, IOptionContainer, IOptionGroupContainer {
    private ushort _Size = 1;
    private bool _Multiple;
    private bool _Disabled;
    private ushort _TabIndex;
    private string _OnFocus;
    private string _OnBlur;
    private string _OnChange;

    private OptionCollection _OptionElements;
    private OptionGroupCollection _OptionGroupElements;

    #region Constructors

    /// <summary>
    /// Create a new selection list.
    /// </summary>
    /// <param name="name">The name of the list</param>
    public Select(string name) {
      Name = name;
      _OptionElements = new OptionCollection(new AddElement(Add));
      _OptionGroupElements = new OptionGroupCollection(new AddElement(Add));
    }

    /// <summary>
    /// Create a new selection list.
    /// </summary>
    /// <param name="name">The name of the list</param>
    /// <param name="size">The size of the list</param>
    public Select(string name, ushort size) : this(name) {
      _Size = size;
    }

    /// <summary>
    /// Create a new selection list.
    /// </summary>
    /// <param name="name">The name of the list</param>
    /// <param name="size">The size of the list</param>
    /// <param name="multiple">Specifies, whether multiple entries can be selected from this list</param>
    public Select(string name, ushort size, bool multiple) : this(name) {
      _Size = size;
      _Multiple = multiple;
    }

    #endregion

    #region Element management

    /// <summary>
    /// Add an element to this list.
    /// </summary>
    /// <param name="element">The element to add</param>
    public override void Add(Element element) {
      if (!(element is Option) && !(element is OptionGroup)) throw new ArgumentException("You can only add options and option-groups to this container");
      base.Add(element);
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The size of this list.
    /// </summary>
    public ushort Size {
      get { return _Size; }
      set { _Size = value; }
    }

    /// <summary>
    /// Defines, whether the user can select multiple entries from
    /// the list or not.
    /// </summary>
    public bool Multiple {
      get { return _Multiple; }
      set { _Multiple = value; }
    }

    /// <summary>
    /// Defines, whether this control is disabled or not.
    /// </summary>
    public bool Disabled {
      get { return _Disabled; }
      set { _Disabled = value; }
    }

    /// <summary>
    /// The tab index of this control.
    /// </summary>
    public ushort TabIndex {
      get { return _TabIndex; }
      set { _TabIndex = value; }
    }

    /// <summary>
    /// Code which will be executed if the element gets the focus.
    /// </summary>
    public string OnFocus {
      get { return _OnFocus; }
      set { _OnFocus = value; }
    }

    /// <summary>
    /// Code which will be executed if the element loses the focus.
    /// </summary>
    public string OnBlur {
      get { return _OnBlur; }
      set { _OnBlur = value; }
    }

    /// <summary>
    /// Code which will be exectued if the selected element changes.
    /// </summary>
    public string OnChange {
      get { return _OnChange; }
      set { _OnChange = value; }
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

    #region IOptionGroupContainer Members

    /// <summary>
    /// Add a option group.
    /// </summary>
    /// <param name="label">The label of the group</param>
    /// <returns>The created object</returns>
    public OptionGroup AddOptionGroup(string label) {
      return _OptionGroupElements.AddOptionGroup(label);
    }

    /// <summary>
    /// Add a option group.
    /// </summary>
    /// <param name="label">The label of the group</param>
    /// <param name="isDisabled">Specifies, whether this option group is disabled or not</param>
    /// <returns>The created object</returns>
    public OptionGroup AddOptionGroup(string label, bool isDisabled) {
      return _OptionGroupElements.AddOptionGroup(label, isDisabled);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      RenderingType = type;
      b.Append("\r\n<select" + ToClassString() + ToXHtml(IsXHtml) + ToCss());
      b.Append(" size=\"" + _Size + "\"");
      if (_Multiple) b.Append(" multiple=\"multiple\"");
      if (_Disabled) b.Append(" disabled=\"disabled\"");
      if (_TabIndex > 0) b.Append(" tabindex=\"" + _TabIndex + "\"");
      if (_OnFocus != null && _OnFocus.Length > 0) b.Append(" onfocus=\"" + _OnFocus + "\"");
      if (_OnBlur != null && _OnBlur.Length > 0) b.Append(" onblur=\"" + _OnBlur + "\"");
      if (_OnChange != null && _OnChange.Length > 0) b.Append(" onchange=\"" + _OnChange + "\"");
      b.Append(">\r\n");
      SerializeChildren(b, type);
      b.Append("</select>\r\n");
    }

    #endregion


    
  }

}
