using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// The type of an HTML item.
  /// </summary>
  public enum ItemType {
    /// <summary>
    /// A span element, used to format text.
    /// </summary>
    Span,
    /// <summary>
    /// A div box, used to format and align elements.
    /// </summary>
    Div,
    /// <summary>
    /// A reference to an image.
    /// </summary>
    Image,
    /// <summary>
    /// An user-visible button.
    /// </summary>
    Button,
    /// <summary>
    /// A text field which can contain a lot of text.
    /// </summary>
    Textarea,
    /// <summary>
    /// An option control.
    /// </summary>
    Option,
    /// <summary>
    /// A select control.
    /// </summary>
    Select,
    /// <summary>
    /// A text field.
    /// </summary>
    Textfield,
    /// <summary>
    /// A table.
    /// </summary>
    Table,
    /// <summary>
    /// A simple line.
    /// </summary>
    Line,
    /// <summary>
    /// A headline.
    /// </summary>
    Headline,
    /// <summary>
    /// A link, pointing on an URL.
    /// </summary>
    Link,
    /// <summary>
    /// A checkbox.
    /// </summary>
    Checkbox,
    /// <summary>
    /// Plain text.
    /// </summary>
    Text,
    /// <summary>
    /// A line in a table.
    /// </summary>
    Tr,
    /// <summary>
    /// A cell in a line of a table.
    /// </summary>
    Td,
    /// <summary>
    /// A form, which can contain user input controls.
    /// </summary>
    Form,
    /// <summary>
    /// A hidden field.
    /// </summary>
    Hidden,
    /// <summary>
    /// A comment.
    /// </summary>
    Comment
  }

  /// <summary>
  /// A HTML page consists of HTML elements which are described by this interface. Every HTML
  /// item needs to implement this interface.
  /// </summary>
  public interface IItem {

    /// <summary>
    /// The type of the element.
    /// </summary>
    ItemType Type { get; }

    /// <summary>
    /// The HTML ID of the element.
    /// </summary>
    string ID { get; set; }

    /// <summary>
    /// Write the HTML element to a <see cref="StringBuilder" />.
    /// </summary>
    /// <param name="builder">The builder to serialize to</param>
    /// <param name="style">The style used for this serialization</param>
    void SerializeContent(StringBuilder builder, IPageStyle style);

    /// <summary>
    /// Search for an element with a specified ID. Check all sub items, too.
    /// </summary>
    /// <param name="id">The ID to search for</param>
    /// <returns>The found element or null, if it could not be found.</returns>
    IItem GetElementByID(string id);

  }

}
