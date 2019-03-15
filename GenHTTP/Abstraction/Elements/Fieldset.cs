using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// Defines a fieldset.
  /// </summary>
  public class Fieldset : StyledContainerElement {
    private NeutralElement _Legend;

    #region Constructors

    /// <summary>
    /// Create a new field set.
    /// </summary>
    /// <param name="caption">The title of the fieldset</param>
    public Fieldset(string caption) {
      _Legend = new NeutralElement(caption);
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The legend of this set.
    /// </summary>
    public NeutralElement Legend {
      get { return _Legend; }
      set { _Legend = value; }
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
      b.Append("\r\n<fieldset" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">\r\n");
      b.Append("  <legend" + _Legend.ToClassString() + _Legend.ToXHtml(IsXHtml) + ToCss() + ">");
      _Legend.Serialize(b, type);
      b.Append("</legend>\r\n");
      SerializeChildren(b, type);
      b.Append("\r\n</fieldset>\r\n");
    }

    #endregion
    
  }

}
