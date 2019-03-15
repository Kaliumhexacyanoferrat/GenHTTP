using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// The methods of this interface should be implemented
    /// by containers which contain button elements.
    /// </summary>
    public interface IButtonContainer
    {

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <returns>The created object</returns>
        Button AddButton();

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <returns>The created object</returns>
        Button AddButton(string name);

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="type">The type of the button</param>
        /// <returns>The created object</returns>
        Button AddButton(string name, ButtonType type);

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        /// <returns>The created object</returns>
        Button AddButton(string name, string value);

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        /// <param name="content">The button text</param>
        /// <returns>The created object</returns>
        Button AddButton(string name, string value, string content);

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="type">The type of the button</param>
        /// <param name="value">The value of the button</param>
        /// <returns>The created object</returns>
        Button AddButton(string name, string value, ButtonType type);

        /// <summary>
        /// Add a new, empty button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="type">The type of the button</param>
        /// <param name="value">The value of the button</param>
        /// <param name="content">The button text</param>
        /// <returns>The created object</returns>
        Button AddButton(string name, string value, string content, ButtonType type);

    }

}
