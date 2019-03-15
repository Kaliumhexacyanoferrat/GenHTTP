/*

Updated: 2009/10/23

2009/10/23  Andreas Nägeli        Initial version of this file.


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

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows you to add options to a container.
  /// </summary>
  public class OptionCollection : IOptionContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new option collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the underlying container</param>
    public OptionCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region IOptionContainer Members

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content) {
      Option opt = new Option(content);
      _Delegate(opt);
      return opt;
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="selected">Specifies, whether the list entry should be selected</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, bool selected) {
      Option opt = new Option(content, selected);
      _Delegate(opt);
      return opt;
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value) {
      Option opt = new Option(content, value);
      _Delegate(opt);
      return opt;
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <param name="selected">Specifies, whether the list entry should be selected</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value, bool selected) {
      Option opt = new Option(content, selected);
      _Delegate(opt);
      return opt;
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <param name="label">The label of the element</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value, string label) {
      Option opt = new Option(content, value, label);
      _Delegate(opt);
      return opt;
    }

    /// <summary>
    /// Add an option to this element.
    /// </summary>
    /// <param name="content">The description of the element</param>
    /// <param name="value">The value of the element</param>
    /// <param name="label">The label of the element</param>
    /// <param name="selected">Specifies, whether the list entry should be selected</param>
    /// <returns>The created object</returns>
    public Option AddOption(string content, string value, string label, bool selected) {
      Option opt = new Option(content, value, label, selected);
      _Delegate(opt);
      return opt;
    }

    #endregion

  }

}
