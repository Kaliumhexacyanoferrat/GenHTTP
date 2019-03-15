using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {

  /// <summary>
  /// A checkbox.
  /// </summary>
  public class Checkbox : HtmlElement, IItem {
    private string _ID;
    private string _Caption;
    private bool _Checked;

    /// <summary>
    /// Create a new checkbox.
    /// </summary>
    public Checkbox() {

    }

    /// <summary>
    /// Create a new checkbox.
    /// </summary>
    /// <param name="id">The ID of this control</param>
    public Checkbox(string id) {
      _ID = id;
    }

    /// <summary>
    /// Create a new checkbox.
    /// </summary>
    /// <param name="id">The ID of this control</param>
    /// <param name="caption">The caption of the checkbox</param>
    public Checkbox(string id, string caption) : this(id) {
      _Caption = caption;
    }

    /// <summary>
    /// The type of this control. Should be <see cref="ItemType.Checkbox" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Checkbox; }
    }

    /// <summary>
    /// The ID of this element.
    /// </summary>
    public string ID {
      get {
        return _ID;
      }
      set {
        _ID = value;
      }
    }

    /// <summary>
    /// Specifiy, whether the control is checked or not.
    /// </summary>
    public bool Checked {
      get { return _Checked; }
      set { _Checked = value; }
    }

    /// <summary>
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The StringBuilder to write the data to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      builder.Append("<input type=\"checkbox\" ");
      if (_ID != "") builder.Append("id=\"" + _ID + "\" name=\"" + _ID + "\" ");
      if (Classes.ToCss() != "") builder.Append("class=\"" + Classes.ToCss() + "\" ");
      if (CssString != "") builder.Append("style=\"" + CssString + "\" ");
      if (_Checked) builder.Append("checked=\"checked\" ");
      builder.Append("/>");
      if (_Caption != "") builder.Append(" " + _Caption);
    }

    /// <summary>
    /// Search for an element with the given ID.
    /// </summary>
    /// <param name="id">The ID of the element to find</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return null;
    }

  }

}
