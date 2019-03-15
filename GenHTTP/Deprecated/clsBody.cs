using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using GenHTTP.Style;

namespace GenHTTP {

  /// <summary>
  /// Represents the body of a HTML page.
  /// </summary>
  public class Body {
    private Page _Page;
    private Color _Color;
    private Position _Margin;
    private Position _Padding;
    private Font _Font;
    private CssClassCollection _Classes;
    private ItemCollection _Children;
    private List<JsFunction> _Functions;
    private string _GenericJs = "";

    #region get-/setters

    /// <summary>
    /// Retrieve the margin of the body element.
    /// </summary>
    public Position Margin {
      get {
        return _Margin;
      }
    }

    /// <summary>
    /// Retrieve the padding of the body element.
    /// </summary>
    public Position Padding {
      get {
        return _Padding;
      }
    }

    /// <summary>
    /// The elements of this web page.
    /// </summary>
    public ItemCollection Children {
      get {
        return _Children;
      }
    }

    /// <summary>
    /// The font used for this web page.
    /// </summary>
    public Font Font {
      get { return _Font; }
    }

    /// <summary>
    /// Retrieve the colors of the body element.
    /// </summary>
    public Color Color {
      get {
        return _Color;
      }
    }

    /// <summary>
    /// Retrieve the CSS classes of this element.
    /// </summary>
    public CssClassCollection Classes {
      get {
        return _Classes;
      }
    }

    /// <summary>
    /// The JavaScript functions used by this web page.
    /// </summary>
    public List<JsFunction> JsFunctions {
      get {
        return _Functions;
      }
    }

    /// <summary>
    /// JavaScript which will be written directly into the body element, without using functions.
    /// </summary>
    public string GenericJs {
      get { return _GenericJs; }
      set { _GenericJs = value; }
    }

    #endregion

    /// <summary>
    /// Create a new body object.
    /// </summary>
    /// <param name="page">The page this element relates to</param>
    internal Body(Page page) {
      _Page = page;
      _Margin = new Position("margin");
      _Padding = new Position("padding");
      _Color = new Color();
      _Classes = new CssClassCollection();
      _Children = new ItemCollection();
      _Functions = new List<JsFunction>();
      _Font = new Font();
    }

    /// <summary>
    /// Generate the page's body.
    /// </summary>
    /// <param name="p">The string builder to add the output data to</param>
    /// <param name="style">The style used for this serialization</param>
    internal void SerializeContent(StringBuilder p, IPageStyle style) {
      string nl = Environment.NewLine;
      string css = _Margin.ToCss() + _Padding.ToCss() + _Color.ToCss() + _Font.ToCss();
      string cls = _Classes.ToCss();
      if (css != "") css = " style=\"" + css + "\"";
      if (cls != "") cls = " class=\"" + cls + "\"";
      p.Append("<body" + css + cls + ">" + nl + nl);
      if (_Functions.Count > 0 && _GenericJs != "") {
        p.Append(nl + "<script type=\"text/javascript\">" + nl);
        foreach (JsFunction func in _Functions) {
          p.Append(nl);
          func.SerializeContent(p);
          p.Append(nl);
        }
        p.Append(nl + _GenericJs + nl);
        p.Append(nl + "</script>" + nl);
      }
      foreach (IItem child in _Children) {
        child.SerializeContent(p, style);
      }
      p.Append(nl + nl);
      p.Append("</body>");
    }

    /// <summary>
    /// Find a child by its ID.
    /// </summary>
    /// <param name="id">The ID of the child to search for</param>
    public IItem this[string id] {
      get {
        return _Children[id];
      }
    }

    /// <summary>
    /// Enumerator to iterate over all children.
    /// </summary>
    /// <returns>The enumerator for this object</returns>
    public IEnumerator<IItem> GetEnumerator() {
      return _Children.GetEnumerator();
    }

    /// <summary>
    /// Include a whole text file into the page.
    /// </summary>
    /// <param name="file">The text file to include</param>
    /// <param name="encoding">The encoding to use</param>
    public void Include(string file, Encoding encoding) {
      Children.InsertFile(file, encoding);
    }

    /// <summary>
    /// Include a whole text file into the page.
    /// </summary>
    /// <param name="file">The text file to include</param>
    public void Include(string file) {
      Children.InsertFile(file);
    }

    /// <summary>
    /// Write a string to the output page (like echo in PHP).
    /// </summary>
    /// <param name="text">The text to write</param>
    public void Print(string text) {
      Children.Insert(text);
    }

    /// <summary>
    /// Write a comment to the output page.
    /// </summary>
    /// <param name="text">The note to write</param>
    public void Comment(string text) {
      Children.InsertComment(text);
    }

  }

}
