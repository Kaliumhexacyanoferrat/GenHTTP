using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// Represents a comment.
  /// </summary>
  public class Comment : IItem {
    private string _Value;

    /// <summary>
    /// Create a new comment.
    /// </summary>
    /// <param name="value">The value of the comment</param>
    public Comment(string value) {
      _Value = value;
    }

    /// <summary>
    /// The comment.
    /// </summary>
    public string Value {
      get { return _Value; }
      set { _Value = value; }
    }

    /// <summary>
    /// The type of this element. Should be <see cref="ItemType.Comment" />.
    /// </summary>
    public ItemType Type {
      get { return ItemType.Comment; }
    }

    /// <summary>
    /// This element does not supports IDs.
    /// </summary>
    public string ID {
      get { return null; }
      set { }
    }

    /// <summary>
    /// Serialize this element to an <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The string builder to write to</param>
    /// <param name="style">The style used for the serialization</param>
    public void SerializeContent(StringBuilder builder, IPageStyle style) {
      builder.Append(Environment.NewLine + "<!-- " + _Value + " -->" + Environment.NewLine);
    }

    /// <summary>
    /// This element does not support IDs or children.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <returns>Always null</returns>
    public IItem GetElementByID(string id) {
      return null;
    }

  }

}
