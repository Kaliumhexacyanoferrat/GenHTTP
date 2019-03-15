using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Elements.Containers;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add input elements to a container.
    /// </summary>
    public class InputCollection : IInputContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new input collection.
        /// </summary>
        /// <param name="d">The method used to add elements to the underlying container</param>
        public InputCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region IInputContainer Members

        /// <summary>
        /// Add a new, empty input field.
        /// </summary>
        /// <returns>The created object</returns>
        public Input AddInput()
        {
            Input input = new Input();
            _Delegate(input);
            return input;
        }

        /// <summary>
        /// Add an input element.
        /// </summary>
        /// <param name="type">The type of the element</param>
        /// <param name="name">The name of the element</param>
        /// <param name="id">The ID of the element</param>
        /// <returns>The created object</returns>
        public Input AddInput(InputType type, string name, string id)
        {
            Input input = new Input(type, name, id);
            _Delegate(input);
            return input;
        }

        /// <summary>
        /// Add an input element.
        /// </summary>
        /// <param name="type">The type of the element</param>
        /// <param name="name">The name of the element</param>
        /// <param name="id">The ID of the element</param>
        /// <param name="value">The value of the element</param>
        /// <returns>The created object</returns>
        public Input AddInput(InputType type, string name, string id, string value)
        {
            Input input = new Input(type, name, id, value);
            _Delegate(input);
            return input;
        }

        /// <summary>
        /// Add a new checkbox.
        /// </summary>
        /// <param name="name">The name of the checkbox</param>
        /// <param name="check">Specify, whether this box should be checked</param>
        /// <returns>The new checkbox</returns>
        public Input AddInput(string name, bool check)
        {
            Input input = new Input(name, check);
            _Delegate(input);
            return input;
        }

        #endregion

    }

}
