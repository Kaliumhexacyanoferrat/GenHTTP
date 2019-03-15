/*

Updated: 2009/10/20

2009/10/20  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Style;
using GenHTTP.Abstraction.Elements.Containers;
using GenHTTP.Abstraction.Elements.Collections;
using GenHTTP.Abstraction.Compiling;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// An element which can be used as a container (e.g. div, td).
  /// </summary>
  /// <remarks>
  /// This class does not check, whether it's allowed to add a specific
  /// item to the container (e.g. adding a div to a span).
  /// </remarks>
  public abstract class StyledContainerElement : StyledElementWithChildren, ITextContainer, ILinkContainer, 
    IDivContainer, ITableContainer, IImageContainer, IMapContainer, IHeadlineContainer, IFormContainer,
    IInputContainer, ISelectContainer, ITextareaContainer, IFieldsetContainer, IButtonContainer, ISpanContainer {
    private TextCollection _TextElements;
    private LinkCollection _LinkElements;
    private DivCollection _DivElements;
    private TableCollection _TableElements;
    private ImageCollection _ImageElements;
    private MapCollection _MapElements;
    private HeadlineCollection _HeadlineElements;
    private FormCollection _FormElements;
    private InputCollection _InputElements;
    private SelectCollection _SelectElements;
    private TextareaCollection _TextareaElements;
    private FieldsetCollection _FieldsetElements;
    private ButtonCollection _ButtonElements;
    private SpanCollection _SpanElements;

    #region Constructors

    /// <summary>
    /// Create a new, styled container element with children.
    /// </summary>
    public StyledContainerElement() {
      _TextElements = new TextCollection(new AddElement(Add));
      _LinkElements = new LinkCollection(new AddElement(Add));
      _DivElements = new DivCollection(new AddElement(Add));
      _TableElements = new TableCollection(new AddElement(Add));
      _ImageElements = new ImageCollection(new AddElement(Add));
      _MapElements = new MapCollection(new AddElement(Add));
      _HeadlineElements = new HeadlineCollection(new AddElement(Add));
      _FormElements = new FormCollection(new AddElement(Add));
      _InputElements = new InputCollection(new AddElement(Add));
      _SelectElements = new SelectCollection(new AddElement(Add));
      _TextareaElements = new TextareaCollection(new AddElement(Add));
      _FieldsetElements = new FieldsetCollection(new AddElement(Add));
      _ButtonElements = new ButtonCollection(new AddElement(Add));
      _SpanElements = new SpanCollection(new AddElement(Add));
    }

    #endregion

    #region ITextContainer Members

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    public void Print(string text) {
      _TextElements.Print(text);
    }

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
    public void Print(string text, bool escapeEntities) {
      _TextElements.Print(text, escapeEntities);
    }

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <returns>The newly created object</returns>
    public Text AddText(string text) {
      return _TextElements.AddText(text);
    }

    /// <summary>
    /// Print some text to the element.
    /// </summary>
    /// <param name="text">The text to print</param>
    /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
    /// <returns>The newly created object</returns>
    public Text AddText(string text, bool escapeEntities) {
      return _TextElements.AddText(text, escapeEntities);
    }

    /// <summary>
    /// Add an empty text element to the element.
    /// </summary>
    /// <returns>The newly created object</returns>
    public Text AddText() {
      return _TextElements.AddText();
    }

    #endregion

    #region ILinkContainer Members

    /// <summary>
    /// Add an empty link.
    /// </summary>
    /// <returns>The created object</returns>
    public Link AddLink() {
      return _LinkElements.AddLink();
    }

    /// <summary>
    /// Add a new link.
    /// </summary>
    /// <param name="url">The URL to link</param>
    /// <returns>The created object</returns>
    public Link AddLink(string url) {
      return _LinkElements.AddLink(url);
    }

    /// <summary>
    /// Add a new link.
    /// </summary>
    /// <param name="url">The URL to link</param>
    /// <param name="linkText">The link text</param>
    /// <returns>The created object</returns>
    public Link AddLink(string url, string linkText) {
      return _LinkElements.AddLink(url, linkText);
    }

    /// <summary>
    /// Add a new link.
    /// </summary>
    /// <param name="url">The URL to link</param>
    /// <param name="element">The link element</param>
    /// <returns>The created object</returns>
    public Link AddLink(string url, Element element) {
      return _LinkElements.AddLink(url, element);
    }

    #endregion

    #region IDivContainer Members

    /// <summary>
    /// Add an empty div to the container.
    /// </summary>
    /// <returns>The created object</returns>
    public Div AddDiv() {
      return _DivElements.AddDiv();
    }

    /// <summary>
    /// Add an empty div to the container.
    /// </summary>
    /// <param name="id">The ID of the new Div</param>
    /// <returns>The created object</returns>
    public Div AddDiv(string id) {
      return _DivElements.AddDiv(id);
    }

    /// <summary>
    /// Add a div to the container.
    /// </summary>
    /// <param name="element">The content of the div box</param>
    /// <returns>The created object</returns>
    public Div AddDiv(Element element) {
      return _DivElements.AddDiv(element);
    }

    #endregion

    #region ITableContainer Members

    /// <summary>
    /// Add a new table.
    /// </summary>
    /// <returns>The created table</returns>
    public Table AddTable() {
      return _TableElements.AddTable();
    }

    #endregion

    #region IImageContainer Members

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription) {
      return _ImageElements.AddImage(source, alternativeDescription);
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, string longDescription) {
      return _ImageElements.AddImage(source, alternativeDescription, longDescription);
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <param name="mapSource">The map to use for this image</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, string longDescription, string mapSource) {
      return _ImageElements.AddImage(source, alternativeDescription, longDescription, mapSource);
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="isMap">Specifies, whether this image should be used as a map</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, bool isMap) {
      return _ImageElements.AddImage(source, alternativeDescription, isMap);
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <param name="isMap">Specifies, whether this image should be used as a map</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, string longDescription, bool isMap) {
      return _ImageElements.AddImage(source, alternativeDescription, longDescription, isMap);
    }

    #endregion

    #region IMapContainer Members

    /// <summary>
    /// Add a new map.
    /// </summary>
    /// <param name="name">The name of the map</param>
    /// <returns>The created object</returns>
    public Map AddMap(string name) {
      return _MapElements.AddMap(name);
    }

    #endregion

    #region IHeadlineContainer Members

    /// <summary>
    /// Add a new headline.
    /// </summary>
    /// <param name="value">The value of the headline</param>
    /// <returns>The created object</returns>
    public Headline AddHeadline(string value) {
      return _HeadlineElements.AddHeadline(value);
    }

    /// <summary>
    /// Add a new headline.
    /// </summary>
    /// <param name="value">The value of the headline</param>
    /// <param name="size">The size of the headline (from 1 to 6)</param>
    /// <returns>The created object</returns>
    public Headline AddHeadline(string value, byte size) {
      return _HeadlineElements.AddHeadline(value, size);
    }

    #endregion

    #region IFormContainer Members

    /// <summary>
    /// Add a new form.
    /// </summary>
    /// <param name="action">The URL of the file to invoke on submit</param>
    /// <returns>The created object</returns>
    /// <remarks>
    /// This method will create a form using the HTTP POST method.
    /// </remarks>
    public Form AddForm(string action) {
      return _FormElements.AddForm(action);
    }

    /// <summary>
    /// Add a new form.
    /// </summary>
    /// <param name="action">The URL of the file to invoke on submit</param>
    /// <param name="method">The HTTP method to use</param>
    /// <returns>The created object</returns>
    public Form AddForm(string action, FormMethod method) {
      return _FormElements.AddForm(action, method);
    }

    #endregion

    #region IInputContainer Members

    /// <summary>
    /// Add a new, empty input field.
    /// </summary>
    /// <returns>The created object</returns>
    public Input AddInput() {
      return _InputElements.AddInput();
    }

    /// <summary>
    /// Add an input element.
    /// </summary>
    /// <param name="type">The type of the element</param>
    /// <param name="name">The name of the element</param>
    /// <param name="id">The ID of the element</param>
    /// <returns>The created object</returns>
    public Input AddInput(InputType type, string name, string id) {
      return _InputElements.AddInput(type, name, id);
    }

    /// <summary>
    /// Add an input element.
    /// </summary>
    /// <param name="type">The type of the element</param>
    /// <param name="name">The name of the element</param>
    /// <param name="id">The ID of the element</param>
    /// <param name="value">The value of the element</param>
    /// <returns>The created object</returns>
    public Input AddInput(InputType type, string name, string id, string value) {
      return _InputElements.AddInput(type, name, id, value);
    }

    /// <summary>
    /// Add a new checkbox.
    /// </summary>
    /// <param name="name">The name of the checkbox</param>
    /// <param name="check">Specify, whether this box should be checked</param>
    /// <returns>The new checkbox</returns>
    public Input AddInput(string name, bool check) {
      return _InputElements.AddInput(name, check);
    }

    #endregion

    #region ISelectContainer Members

    /// <summary>
    /// Add a new selection list.
    /// </summary>
    /// <param name="name">The name of the list</param>
    /// <returns>The created object</returns>
    public Select AddSelect(string name) {
      return _SelectElements.AddSelect(name);
    }

    /// <summary>
    /// Add a new selection list.
    /// </summary>
    /// <param name="name">The name of the list</param>
    /// <param name="size">The size of the list</param>
    /// <returns>The created object</returns>
    public Select AddSelect(string name, ushort size) {
      return _SelectElements.AddSelect(name, size);
    }

    /// <summary>
    /// Add a new selection list.
    /// </summary>
    /// <param name="name">The name of the list</param>
    /// <param name="size">The size of the list</param>
    /// <param name="multiple">Specifies, whether the user can select multiple entries</param>
    /// <returns>The created object</returns>
    public Select AddSelect(string name, ushort size, bool multiple) {
      return _SelectElements.AddSelect(name, size, multiple);
    }

    #endregion

    #region ITextareaContainer Members

    /// <summary>
    /// Add a new textarea.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="rows">The row count of the field</param>
    /// <param name="cols">The column count of the field</param>
    /// <returns>The created object</returns>
    public Textarea AddTextarea(string name, ushort rows, ushort cols) {
      return _TextareaElements.AddTextarea(name, rows, cols);
    }

    /// <summary>
    /// Add a new textarea.
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="rows">The row count of the field</param>
    /// <param name="cols">The column count of the field</param>
    /// <param name="value">The value of the textarea</param>
    /// <returns>The created object</returns>
    public Textarea AddTextarea(string name, ushort rows, ushort cols, string value) {
      return _TextareaElements.AddTextarea(name, rows, cols, value);
    }

    #endregion

    #region IFieldsetContainer Members

    /// <summary>
    /// Add a new fieldset.
    /// </summary>
    /// <param name="caption">The caption of the set</param>
    /// <returns>The created object</returns>
    public Fieldset AddFieldset(string caption) {
      return _FieldsetElements.AddFieldset(caption);
    }

    #endregion

    #region IButtonContainer Members

    /// <summary>
    /// Add a new, empty button.
    /// </summary>
    /// <returns>The created object</returns>
    public Button AddButton() {
      return _ButtonElements.AddButton();
    }

    /// <summary>
    /// Add a new, empty button.
    /// </summary>
    /// <param name="name">The name of the button</param>
    /// <returns>The created object</returns>
    public Button AddButton(string name) {
      return _ButtonElements.AddButton(name);
    }

    /// <summary>
    /// Add a new, empty button.
    /// </summary>
    /// <param name="name">The name of the button</param>
    /// <param name="type">The type of the button</param>
    /// <returns>The created object</returns>
    public Button AddButton(string name, ButtonType type) {
      return _ButtonElements.AddButton(name, type);
    }

    /// <summary>
    /// Add a new, empty button.
    /// </summary>
    /// <param name="name">The name of the button</param>
    /// <param name="value">The value of the button</param>
    /// <returns>The created object</returns>
    public Button AddButton(string name, string value) {
      return _ButtonElements.AddButton(name, value);
    }

    /// <summary>
    /// Add a new, empty button.
    /// </summary>
    /// <param name="name">The name of the button</param>
    /// <param name="value">The value of the button</param>
    /// <param name="content">The button text</param>
    /// <returns>The created object</returns>
    public Button AddButton(string name, string value, string content) {
      return _ButtonElements.AddButton(name, value, content);
    }

    /// <summary>
    /// Add a new, empty button.
    /// </summary>
    /// <param name="name">The name of the button</param>
    /// <param name="type">The type of the button</param>
    /// <param name="value">The value of the button</param>
    /// <returns>The created object</returns>
    public Button AddButton(string name, string value, ButtonType type) {
      return _ButtonElements.AddButton(name, value, type);
    }

    /// <summary>
    /// Add a new, empty button.
    /// </summary>
    /// <param name="name">The name of the button</param>
    /// <param name="type">The type of the button</param>
    /// <param name="value">The value of the button</param>
    /// <param name="content">The button text</param>
    /// <returns>The created object</returns>
    public Button AddButton(string name, string value, string content, ButtonType type) {
      return _ButtonElements.AddButton(name, value, content, type);
    }

    #endregion

    #region ISpanContainer Members

    /// <summary>
    /// Add a new, empty span.
    /// </summary>
    /// <returns>The created object</returns>
    public Span AddSpan() {
      return _SpanElements.AddSpan();
    }

    /// <summary>
    /// Add a new span.
    /// </summary>
    /// <param name="text">The content of the span</param>
    /// <param name="decoration">The text decoration to use</param>
    /// <returns>The created object</returns>
    public Span AddSpan(string text, ElementTextDecoration decoration) {
      return _SpanElements.AddSpan(text, decoration);
    }

    /// <summary>
    /// Add a new span.
    /// </summary>
    /// <param name="text">The content of the span</param>
    /// <param name="fontWeight">The font weight to use</param>
    /// <returns>The created object</returns>
    public Span AddSpan(string text, ElementFontWeight fontWeight) {
      return _SpanElements.AddSpan(text, fontWeight);
    }

    #endregion

    #region Placeholder support

    /// <summary>
    /// Add a placeholder to this container.
    /// </summary>
    /// <param name="type">The type of the placeholder</param>
    /// <param name="name">The name of the placeholder</param>
    public void AddPlaceholder(Type type, string name) {
      AddText(new Placeholder(type, name).ToString());
    }

    #endregion

  }

}
