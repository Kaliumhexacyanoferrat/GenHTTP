using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Controls {
  
  /// <summary>
  /// Methods used by (X)HTML forms.
  /// </summary>
  public enum HtmlMethod {
    /// <summary>
    /// GET method, used to recieve data from the server.
    /// </summary>
    Get,
    /// <summary>
    /// POST method, used to transmit data to the server.
    /// </summary>
    Post
  }

  /// <summary>
  /// The form element.
  /// </summary>
  public class Form : HtmlElement, IItem {
    private string _ID;
    private HtmlMethod _Method = HtmlMethod.Post;
    private string _File;
    private ItemCollection _Children;

    /// <summary>
    /// Create a new form, which will call the given file on submit.
    /// </summary>
    /// <param name="file">The file to call on submit</param>
    public Form(string file) {
      _File = file;
      _Children = new ItemCollection();
    }

    /// <summary>
    /// Create a new form, using a given method.
    /// </summary>
    /// <param name="method">The method to use</param>
    /// <param name="file">The file to call on submit</param>
    public Form(HtmlMethod method, string file)
      : this(file) {
      _Method = method;
    }
    
    /// <summary>
    /// The type of this element.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Form;  }
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
    /// The children of this element.
    /// </summary>
    public ItemCollection Children {
      get { return _Children; }
    }

    #region Children handling

    /// <summary>
    /// Retrieve an enumerator to iterate over the children of this element.
    /// </summary>
    /// <returns>The requested enumerator</returns>
    public IEnumerator<IItem> GetEnumerator() {
      return _Children.GetEnumerator();
    }

    /// <summary>
    /// Retrieve a child element by its ID.
    /// </summary>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem this[string id] {
      get { return _Children[id]; }
    }

    /// <summary>
    /// Retrieve a child element by its position in the item collection.
    /// </summary>
    /// <param name="pos">The position of the item to retrieve</param>
    /// <returns>The requested element</returns>
    public IItem this[int pos] {
      get { return _Children[pos]; }
    }

    #endregion

    /// <summary>
    /// Write the (X)HTML output to a StringBuilder.
    /// </summary>
    /// <param name="builder">The StringBuilder to append to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      string nl = Environment.NewLine;
      string method = (_Method == HtmlMethod.Get) ? "get" : "post";
      string id = (_ID != null) ? " id=\"" + _ID + "\"" : "";
      builder.Append(nl + "<form" + id + " action=\"" + _File + "\" method=\"" + _Method + "\">" + nl + nl);
      foreach (IItem child in _Children) {
        child.SerializeContent(builder, style);
      }
      builder.Append(nl + "</form>" + nl + nl);
    }

    /// <summary>
    /// Search for a element recursively
    /// </summary>
    /// <param name="id">The id of the element to search for</param>
    /// <returns>The item or null, if it could not be found</returns>
    public IItem GetElementByID(string id) {
      if (_ID == id) return this;
      return _Children[id];
    }

  }

}
