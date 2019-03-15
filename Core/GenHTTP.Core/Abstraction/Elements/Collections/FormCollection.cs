using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows you to add form elements to a container.
  /// </summary>
  public class FormCollection : IFormContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new form collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the underlying container</param>
    public FormCollection(AddElement d) {
      _Delegate = d;
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
      Form form = new Form(action);
      _Delegate(form);
      return form;
    }

    /// <summary>
    /// Add a new form.
    /// </summary>
    /// <param name="action">The URL of the file to invoke on submit</param>
    /// <param name="method">The HTTP method to use</param>
    /// <returns>The created object</returns>
    public Form AddForm(string action, FormMethod method) {
      Form form = new Form(action, method);
      _Delegate(form);
      return form;
    }

    #endregion

  }

}
