using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Style {

  /// <summary>
  /// This class can be used by HTML elements which can be positioned.
  /// </summary>
  public class Position {
    private string _Value = "";
    private string _Top = "";
    private string _Bottom = "";
    private string _Left = "";
    private string _Right = "";

    private string _Entity;

    /// <summary>
    /// The entity this position relates to (e.g. padding or margin).
    /// </summary>
    /// <param name="entity">The name of the entity this position relates to</param>
    internal Position(string entity) {
      _Entity = entity;
    }

    #region get-/setters

    /// <summary>
    /// The value of the position entity (e.g. "padding: 200px").
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// The distance from the top border.
    /// </summary>
    public string Top {
      get { return _Top; }
      set { _Top = value; }
    }

    /// <summary>
    /// The distance from the bottom border.
    /// </summary>
    public string Bottom {
      get { return _Bottom; }
      set { _Bottom = value; }
    }

    /// <summary>
    /// The distance from the left border.
    /// </summary>
    public string Left {
      get { return _Left; }
      set { _Left = value; }
    }

    /// <summary>
    /// The distance from the right border.
    /// </summary>
    public string Right {
      get { return _Right; }
      set { _Right = value; }
    }

    #endregion

    /// <summary>
    /// Create a CSS string from the given values.
    /// </summary>
    /// <returns>The CSS format string representing the position</returns>
    public string ToCss() {
      string retval = "";
      if (_Value != null && _Value.Length > 0) retval += _Entity + ": " + _Value + "; ";
      if (_Top != null && _Top.Length > 0) retval += _Entity + "-top: " + _Top + "; ";
      if (_Bottom != null && _Bottom.Length > 0) retval += _Entity + "-bottom: " + _Bottom + "; ";
      if (_Left != null && _Left.Length > 0) retval += _Entity + "-left: " + _Left + "; ";
      if (_Right != null && _Right.Length > 0) retval += _Entity + "-right: " + _Right + "; ";
      return retval;
    }

  }

}
