using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines the methods which a container with input elements
    /// should implement.
    /// </summary>
    public interface IInputContainer
    {

        /// <summary>
        /// Add a new, empty input field.
        /// </summary>
        /// <returns>The created object</returns>
        Input AddInput();

        /// <summary>
        /// Add an input element.
        /// </summary>
        /// <param name="type">The type of the element</param>
        /// <param name="name">The name of the element</param>
        /// <param name="id">The ID of the element</param>
        /// <returns>The created object</returns>
        Input AddInput(InputType type, string name, string id);

        /// <summary>
        /// Add an input element.
        /// </summary>
        /// <param name="type">The type of the element</param>
        /// <param name="name">The name of the element</param>
        /// <param name="id">The ID of the element</param>
        /// <param name="value">The value of the element</param>
        /// <returns>The created object</returns>
        Input AddInput(InputType type, string name, string id, string value);

        /// <summary>
        /// Add a new checkbox.
        /// </summary>
        /// <param name="name">The name of the checkbox</param>
        /// <param name="check">Specify, whether this box should be checked</param>
        /// <returns>The new checkbox</returns>
        Input AddInput(string name, bool check);

    }

}
