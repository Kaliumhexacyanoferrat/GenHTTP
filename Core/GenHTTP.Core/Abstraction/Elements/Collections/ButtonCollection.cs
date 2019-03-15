using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add buttons to a collection.
    /// </summary>
    public class ButtonCollection : IButtonContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new button collection.
        /// </summary>
        /// <param name="d">The method used to add elements to the underlying container</param>
        public ButtonCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region IButtonContainer Members

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <returns>The created object</returns>
        public Button AddButton()
        {
            Button button = new Button();
            _Delegate(button);
            return button;
        }

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <returns>The created object</returns>
        public Button AddButton(string name)
        {
            Button button = new Button(name);
            _Delegate(button);
            return button;
        }

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="type">The type of the button</param>
        /// <returns>The created object</returns>
        public Button AddButton(string name, ButtonType type)
        {
            Button button = new Button(name, type);
            _Delegate(button);
            return button;
        }

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        /// <returns>The created object</returns>
        public Button AddButton(string name, string value)
        {
            Button button = new Button(name, value);
            _Delegate(button);
            return button;
        }

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        /// <param name="content">The button text</param>
        /// <returns>The created object</returns>
        public Button AddButton(string name, string value, string content)
        {
            Button button = new Button(name, value, content);
            _Delegate(button);
            return button;
        }

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="type">The type of the button</param>
        /// <param name="value">The value of the button</param>
        /// <returns>The created object</returns>
        public Button AddButton(string name, string value, ButtonType type)
        {
            Button button = new Button(name, value, type);
            _Delegate(button);
            return button;
        }

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="type">The type of the button</param>
        /// <param name="value">The value of the button</param>
        /// <param name="content">The button text</param>
        /// <returns>The created object</returns>
        public Button AddButton(string name, string value, string content, ButtonType type)
        {
            Button button = new Button(name, value, content, type);
            _Delegate(button);
            return button;
        }

        #endregion

    }

}
