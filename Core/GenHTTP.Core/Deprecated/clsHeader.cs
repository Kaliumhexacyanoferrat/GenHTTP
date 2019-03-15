using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Style;

namespace GenHTTP {
  
  /// <summary>
  /// Represents the header of a (X)HTML page (<see cref="Page"/>).
  /// </summary>
  public class Header {
    private Page _Page;
    private string _Title;
    private string _Description;
    private string _Favicon;
    private List<string> _Keywords;
    private List<string> _CssFiles;
    private List<string> _JsFiles;
    private Dictionary<string, string> _Meta;

    /// <summary>
    /// Create a new header.
    /// </summary>
    /// <param name="page">The page this header relates to</param>
    internal Header(Page page) {
      _Page = page;
      _Keywords = new List<string>();
      _CssFiles = new List<string>();
      _JsFiles = new List<string>();
      _Meta = new Dictionary<string, string>();
    }

    #region get-/setters

    /// <summary>
    /// Add additional, user-defined meta information to this page.
    /// </summary>
    /// <param name="header">The meta header</param>
    /// <param name="value">The content of this field</param>
    public void AddMetaInformation(string header, string value) {
      if (!_Meta.ContainsKey(header)) _Meta.Add(header, value);
    }

    /// <summary>
    /// The title of the web page.
    /// </summary>
    public string Title {
      get {
        return _Title;
      }
      set {
        _Title = (value == null) ? "" : value;
      }
    }

    /// <summary>
    /// The keywords of the web page.
    /// </summary>
    public List<string> Keywords {
      get {
        return _Keywords;
      }
    }

    /// <summary>
    /// The description of the web page.
    /// </summary>
    public string Description {
      get {
        return _Description;
      }
      set {
        _Description = value;
      }
    }

    /// <summary>
    /// A link to the favicon for this page.
    /// </summary>
    public string Favicon {
      get { return _Favicon; }
      set { _Favicon = value; }
    }

    /// <summary>
    /// The CSS files to include.
    /// </summary>
    /// <remarks>
    /// You can add a non existing path (e.g. "./style/design.css") to the list, but you need to parse the
    /// requested url then. 
    /// </remarks>
    public List<string> CssFiles {
      get {
        return _CssFiles;
      }
    }

    /// <summary>
    /// The JS files to include.
    /// </summary>
    public List<string> JsFiles {
      get {
        return _JsFiles;
      }
    }

    #endregion

    /// <summary>
    /// Write the header to a string builder.
    /// </summary>
    /// <param name="builder">The string builder to add the data to</param>
    /// <param name="style">The style used for the serialization</param>
    internal void SerializeContent(StringBuilder builder, IPageStyle style) {
      string nl = Environment.NewLine;
      // write title
      builder.Append("  <title>" + _Title + "</title>" + nl);
      // write encoding information     
      string encoding = Enum.GetName(typeof(HtmlEncoding), _Page.Style.Header.HtmlEncoding).Replace("_", "-");
      if (encoding == "shift-jis") encoding = "shift_jis";
      builder.Append("  <meta http-equiv=\"content-type\" content=\"text/html; charset=" + encoding + "\" />" + nl);
      // css sources
      if (_CssFiles.Count != 0) {
        foreach (string css in _CssFiles) {
          builder.Append("  <link rel=\"stylesheet\" type=\"text/css\" href=\"" + css + "\" />" + nl);
        }
      }
      // js sources
      if (_JsFiles.Count != 0) {
        foreach (string js in _JsFiles) {
          builder.Append("  <script type=\"text/javascript\" src=\"" + js + "\"></script>" + nl);
        }
      }
      // meta information
      if (_Meta.Count != 0) {
        foreach (string key in _Meta.Keys) {
          builder.Append("  <meta http-equiv=\"" + key + "\" content=\"" + _Meta[key] + "\" />" + nl);
        }
      }
      // description
      if (_Description != null) {
        builder.Append("  <meta name=\"description\" content=\"" + _Description + "\" />" + nl);
      }
      // favicon
      if (_Favicon != null && _Favicon != "") {
        builder.Append("  <link rel=\"shortcut icon\" type=\"image/x-icon\" href=\"" + _Favicon + "\" />" + nl);
      }
      // keywords
      if (_Keywords.Count != 0) {
        builder.Append("  <meta name=\"keywords\" content=\"");
        for (int i = 0; i < _Keywords.Count - 1; i++) {
          builder.Append(_Keywords[i] + ", ");
        }
        builder.Append(_Keywords[_Keywords.Count - 1]);
        builder.Append("\" />");
      }
    }

  }

}
