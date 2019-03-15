using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Containers
{

    /// <summary>
    /// Describes, which methods a container with form
    /// child elements must implement.
    /// </summary>
    public interface IFormContainer
    {

        /// <summary>
        /// Add a new form.
        /// </summary>
        /// <param name="action">The URL of the file to invoke on submit</param>
        /// <returns>The created object</returns>
        /// <remarks>
        /// This method will create a form using the HTTP POST method.
        /// </remarks>
        Form AddForm(string action);

        /// <summary>
        /// Add a new form.
        /// </summary>
        /// <param name="action">The URL of the file to invoke on submit</param>
        /// <param name="method">The HTTP method to use</param>
        /// <returns>The created object</returns>
        Form AddForm(string action, FormMethod method);

    }

}
