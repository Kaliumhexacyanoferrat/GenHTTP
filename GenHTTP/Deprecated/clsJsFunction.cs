using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// Represents a JavaScript function.
  /// </summary>
  /// <remarks>
  /// There is no abstraction of JavaScript code. It will be stored as plain text in the
  /// body attribute.
  /// </remarks>
  public class JsFunction {
    private List<string> _Parameters;
    private string _Body = "";
    private string _Name;

    /// <summary>
    /// Create a new JavaScript function.
    /// </summary>
    /// <param name="name">The name of the function</param>
    public JsFunction(string name) {
      _Name = name;
      _Parameters = new List<string>();
    }

    /// <summary>
    /// Create a new JavaScript function.
    /// </summary>
    /// <param name="name">The name of the function</param>
    /// <param name="parameters">A list with all parameter names</param>
    public JsFunction(string name, string[] parameters) : this(name) {
      foreach (string param in parameters) {
        _Parameters.Add(param);
      }
    }

    /// <summary>
    /// Create a new JavaScript function.
    /// </summary>
    /// <param name="name">The name of this function</param>
    /// <param name="parameters">The names of the parameters</param>
    /// <param name="body">The JavaScript code of this function</param>
    public JsFunction(string name, string[] parameters, string body) : this(name, parameters) {
      _Body = body;
    }

    #region get-/setters

    /// <summary>
    /// The name of this function.
    /// </summary>
    public string Name {
      get { return _Name; }
    }

    /// <summary>
    /// The parameter list.
    /// </summary>
    public List<string> Parameters {
      get { return _Parameters; }
    }

    /// <summary>
    /// The code of the function.
    /// </summary>
    public string Body {
      get { return _Body; }
      set { _Body = value; }
    }

    #endregion

    /// <summary>
    /// Serialize this element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to serialize to</param>
    public void SerializeContent(StringBuilder builder) {
      string nl = Environment.NewLine;
      builder.Append("function " + _Name + "(");
      int i = 0;
      foreach (string param in _Parameters) {
        i++;
        if (i == _Parameters.Count) {
          builder.Append(param);
        }
        else {
          builder.Append(param + ", ");
        }
      }
      builder.Append(") {" + nl);
      string[] lines = _Body.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
      foreach (string line in lines) {
        builder.Append("  " + line + nl);
      }
      builder.Append("}");
    }

  }

}
