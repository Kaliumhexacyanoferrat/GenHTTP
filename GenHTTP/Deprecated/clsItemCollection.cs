using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using GenHTTP.Controls;
using GenHTTP.Style;

namespace GenHTTP {

  /// <summary>
  /// A simple class to store a list of child elements. Provides some syntactic
  /// sugar to increase page generation comfort.
  /// </summary>
  public class ItemCollection {
    private List<IItem> _Children;

    /// <summary>
    /// Create a new child collection.
    /// </summary>
    internal ItemCollection() {
      _Children = new List<IItem>(3);
    }

    /// <summary>
    /// Retrieve the enumerator of the list.
    /// </summary>
    /// <returns>The enumerator of the list</returns>
    public IEnumerator<IItem> GetEnumerator() {
      return _Children.GetEnumerator();
    }

    /// <summary>
    /// Retrieve a child by its position in the list.
    /// </summary>
    /// <param name="pos">The position of the item to retrieve</param>
    /// <returns>The requested item</returns>
    public IItem this[int pos] {
      get {
        return _Children[pos]; 
      }
    }

    /// <summary>
    /// Retrieve a child by its ID. This function will search recursively in the
    /// child list.
    /// </summary>
    /// <remarks>
    /// Replaced GetElementByID() with this function in 1.06.
    /// </remarks>
    /// <param name="id">The ID of the element to search for</param>
    /// <returns>The requested element or null, if it could not be found</returns>
    public IItem this[string id] {
      get {
        if (id == null) throw new ArgumentNullException();
        foreach (IItem item in _Children) {
          IItem ret = item.GetElementByID(id);
          if (ret != null) return ret;
        }
        return null;
      }
    }

    /// <summary>
    /// Add an item to the list and move it to the last position.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>The added item</returns>
    public IItem Insert(IItem item) {
      if (item == null) throw new ArgumentNullException();
      if (item == this) throw new ArgumentException("Stack overflow detected: can't add an object to its own children list");
      _Children.Add(item);
      return item;
    }

    /// <summary>
    /// Add an item at the specified position.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="index">The index which defines where to insert the element</param>
    /// <returns>The added item</returns>
    public IItem InsertAt(IItem item, int index) {
      if (item == null) throw new ArgumentNullException();
      if (item == this) throw new ArgumentException("Stack overflow detected: can't add an object to its own children list");
      _Children.Insert(index, item);
      return item;
    }

    /// <summary>
    /// Insert an item before the element with the given ID.
    /// </summary>
    /// <remarks>
    /// This function will not search recursively for elements with the given
    /// ID.
    /// </remarks>
    /// <param name="item">The item to insert</param>
    /// <param name="id">The ID of the element to search</param>
    /// <returns>The added item</returns>
    public IItem InsertBefore(IItem item, string id) {
      if (item == null || id == null) throw new ArgumentNullException();
      int index = 0;
      bool found = false;
      for (int i = 0; i < _Children.Count; i++) {
        if (_Children[i].ID == id) {
          index = i - 1;
          found = true;
          break;
        }
      }
      if (!found) throw new ItemNotFoundException(id);
      if (index < 0) index = 0;
      return InsertAt(item, index);
    }

    /// <summary>
    /// Insert an item after the element with the given ID.
    /// </summary>
    /// <remarks>
    /// This function will not search recursively for elements with the given ID.
    /// </remarks>
    /// <param name="item">The item to insert</param>
    /// <param name="id">The ID of the element you want the item to get inserted after</param>
    /// <returns>The added item</returns>
    public IItem InsertAfter(IItem item, string id) {
      if (item == null || id == null) throw new ArgumentNullException();
      int index = 0;
      bool found = false;
      for (int i = 0; i < _Children.Count; i++) {
        if (_Children[i].ID == id) {
          index = i;
          found = true;
          break;
        }
      }
      if (!found) throw new ItemNotFoundException(id);
      return InsertAt(item, index);
    }

    /// <summary>
    /// Append a string to the item.
    /// </summary>
    /// <param name="text">The text to append</param>
    /// <returns>The appended item</returns>
    public IItem Insert(string text) {
      return Insert(new Text(text));
    }

    /// <summary>
    /// Insert a single space.
    /// </summary>
    /// <returns>The created text object</returns>
    public Text InsertSpace() {
      return (Text)Insert(HtmlConstants.Space);
    }

    /// <summary>
    /// Insert some spaces.
    /// </summary>
    /// <param name="count">The number of spaces to add</param>
    /// <returns>The created text object</returns>
    public Text InsertSpaces(byte count) {
      string text = "";
      for (int i = 0; i < count; i++) text += HtmlConstants.Space;
      return (Text)Insert(text);
    }

    /// <summary>
    /// Insert a new line.
    /// </summary>
    /// <returns>The created text object</returns>
    public Text InsertNewLine() {
      return (Text)Insert(HtmlConstants.NewLine);
    }

    /// <summary>
    /// Insert some new lines.
    /// </summary>
    /// <param name="count">The number of new lines to add</param>
    /// <returns>The created text object</returns>
    public Text InsertNewLines(byte count) {
      string text = "";
      for (int i = 0; i < count; i++) text += HtmlConstants.NewLine;
      return (Text)Insert(text);
    }

    /// <summary>
    /// Append the whole content of a file to the item collection.
    /// </summary>
    /// <param name="file">The file to add</param>
    /// <param name="encoding">The encoding to open the file with</param>
    public void InsertFile(string file, Encoding encoding) {
      if (file == null) throw new ArgumentNullException();
      if (!File.Exists(file)) throw new FileNotFoundException();
      StreamReader r = new StreamReader(file, encoding);
      Insert(r.ReadToEnd());
      r.Close();
    }

    /// <summary>
    /// Append the whole content of a file to the item collection.
    /// </summary>
    /// <param name="file">The file to add</param>
    public void InsertFile(string file) {
      InsertFile(file, Encoding.UTF8);
    }

    /// <summary>
    /// Delete all children from the list.
    /// </summary>
    public void Clear() {
      _Children.Clear();
    }

    #region short syntax for adding elements

    /// <summary>
    /// Add a link.
    /// </summary>
    /// <param name="url">The url of the link</param>
    /// <returns>The created item</returns>
    public Link InsertLink(string url) {
      if (url == null) throw new ArgumentNullException();
      Link link = new Link(url);
      Insert(link);
      return link;
    }

    /// <summary>
    /// Add a link.
    /// </summary>
    /// <param name="url">The url of the link</param>
    /// <param name="text">The text which should be displayed</param>
    /// <returns>The created item</returns>
    public Link InsertLink(string url, string text) {
      if (url == null || text == null) throw new ArgumentNullException();
      Link link = InsertLink(url);
      link.Children.Insert(new Text(text));
      return link;
    }

    /// <summary>
    /// Insert an image.
    /// </summary>
    /// <param name="url">The URL of the image</param>
    /// <param name="alternative">The alternative text of the image</param>
    /// <returns>The created image object</returns>
    public Img InsertImage(string url, string alternative) {
      if (url == null || alternative == null) throw new ArgumentNullException();
      Img img = new Img(url, alternative);
      Insert(img);
      return img;
    }

    /// <summary>
    /// Insert a line.
    /// </summary>
    /// <returns>The created line object</returns>
    public Line InsertLine() {
      Line line = new Line();
      Insert(line);
      return line;
    }

    /// <summary>
    /// Insert a span.
    /// </summary>
    /// <returns>The created span object</returns>
    public Span InsertSpan() {
      Span span = new Span();
      Insert(span);
      return span;
    }

    /// <summary>
    /// Insert a span with a given ID.
    /// </summary>
    /// <param name="id">The ID of the span to add</param>
    /// <returns>The created span object</returns>
    public Span InsertSpan(string id) {
      Span span = new Span(id);
      Insert(span);
      return span;
    }

    /// <summary>
    /// Insert a new table line.
    /// </summary>
    /// <returns>The created line (tr) object</returns>
    public Tr InsertTableLine() {
      Tr tr = new Tr();
      Insert(tr);
      return tr;
    }

    /// <summary>
    /// Insert a new table cell.
    /// </summary>
    /// <returns>The created cell (td) object</returns>
    public Td InsertTableCell() {
      Td td = new Td();
      Insert(td);
      return td;
    }

    /// <summary>
    /// Insert a new table cell.
    /// </summary>
    /// <param name="text">The text to write into the cell</param>
    /// <returns>The created cell (td) object</returns>
    public Td InsertTableCell(string text) {
      if (text == null) throw new ArgumentNullException();
      Td cell = InsertTableCell();
      cell.Children.Insert(text);
      return cell;
    }

    /// <summary>
    /// Insert a table line with some table cells. For every string in the text array, a new
    /// cell will be appended.
    /// </summary>
    /// <param name="text">The content of the cells</param>
    /// <returns>The created table line (tr)</returns>
    public Tr InsertTableLine(string[] text) {
      if (text == null) throw new ArgumentNullException();
      Tr tr = InsertTableLine();
      foreach (string cell in text) {
        Td td = new Td();
        td.Children.Insert(cell);
        tr.Children.Insert(td);
      }
      return tr;
    }

    /// <summary>
    /// Insert a check box.
    /// </summary>
    /// <param name="id">The ID of the new element</param>
    /// <returns>The created checkbox</returns>
    public Checkbox InsertCheckbox(string id) {
      if (id == null) throw new ArgumentNullException();
      Checkbox box = new Checkbox(id);
      Insert(box);
      return box;
    }

    /// <summary>
    /// Insert a new, hidden form field.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="value">The value of the field</param>
    /// <returns>The newly created hidden form field</returns>
    public Hidden InsertHidden(string name, string value) {
      if (name == null || value == null) throw new ArgumentNullException();
      Hidden hidden = new Hidden(name, value);
      Insert(hidden);
      return hidden;
    }

    /// <summary>
    /// Insert a new submit button for a form.
    /// </summary>
    /// <param name="value">The caption of the button</param>
    /// <returns>The newly created button</returns>
    public SubmitButton InsertSubmitButton(string value) {
      if (value == null) throw new ArgumentNullException();
      SubmitButton button = new SubmitButton(value);
      Insert(button);
      return button;
    }

    /// <summary>
    /// Add a new table.
    /// </summary>
    /// <returns>The newly created table</returns>
    public Table InsertTable() {
      Table table = new Table();
      Insert(table);
      return table;
    }

    /// <summary>
    /// Add a new table with a given ID.
    /// </summary>
    /// <param name="id">The ID of the new table</param>
    /// <returns>The newly created table</returns>
    public Table InsertTable(string id) {
      if (id == null) throw new ArgumentNullException();
      Table table = InsertTable();
      table.ID = id;
      return table;
    }

    /// <summary>
    /// Insert a new select control.
    /// </summary>
    /// <returns>The newly created select control</returns>
    public Select InsertSelect() {
      Select select = new Select();
      Insert(select);
      return select;
    }

    /// <summary>
    /// Insert a new select control with the given ID.
    /// </summary>
    /// <param name="id">The ID of the new control</param>
    /// <returns>The newly created select control</returns>
    public Select InsertSelect(string id) {
      if (id == null) throw new ArgumentNullException();
      Select select = InsertSelect();
      select.ID = id;
      return select;
    }

    /// <summary>
    /// Insert a new text field.
    /// </summary>
    /// <param name="id">The ID of the text field to create</param>
    /// <param name="value">The value of the text field</param>
    /// <returns>The newly created text field</returns>
    public Textfield InsertTextfield(string id, string value) {
      if (id == null || value == null) throw new ArgumentNullException();
      Textfield field = InsertTextfield(id);
      field.Value = value;
      return field;
    }

    /// <summary>
    /// Insert a new text field.
    /// </summary>
    /// <param name="id">The ID of the text field to create</param>
    /// <returns>The newly created text field</returns>
    public Textfield InsertTextfield(string id) {
      if (id == null) throw new ArgumentNullException();
      Textfield field = new Textfield(id);
      Insert(field);
      return field;
    }

    /// <summary>
    /// Insert a comment.
    /// </summary>
    /// <param name="comment">The note to write</param>
    /// <returns>The newly created comment</returns>
    public Comment InsertComment(string comment) {
      if (comment == null) throw new ArgumentNullException();
      Comment com = new Comment(comment);
      Insert(com);
      return com;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// Retrieve the number of children in this collection.
    /// </summary>
    public int Count {
      get { return _Children.Count; }
    }

    #endregion

  }

  /// <summary>
  /// This exception will be thrown whenever you want to access an item with
  /// an ID, but the item could not be found.
  /// </summary>
  public class ItemNotFoundException : Exception {
    private string _ID;

    /// <summary>
    /// Create a new exception of this type.
    /// </summary>
    /// <param name="id">The ID of the item which could not be found</param>
    public ItemNotFoundException(string id) {
      _ID = id;
    }

    /// <summary>
    /// The message of this exception.
    /// </summary>
    public override string Message {
      get { return "The item with the ID '" + _ID + "' could not be found"; }
    }

  }

}
