using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines methods, which should be implemented by
    /// containers with option elements.
    /// </summary>
    public interface IOptionContainer
    {

        /// <summary>
        /// Add an option to this element.
        /// </summary>
        /// <param name="content">The description of the element</param>
        /// <returns>The created object</returns>
        Option AddOption(string content);

        /// <summary>
        /// Add an option to this element.
        /// </summary>
        /// <param name="content">The description of the element</param>
        /// <param name="selected">Specifies, whether the list entry should be selected</param>
        /// <returns>The created object</returns>
        Option AddOption(string content, bool selected);

        /// <summary>
        /// Add an option to this element.
        /// </summary>
        /// <param name="content">The description of the element</param>
        /// <param name="value">The value of the element</param>
        /// <returns>The created object</returns>
        Option AddOption(string content, string value);

        /// <summary>
        /// Add an option to this element.
        /// </summary>
        /// <param name="content">The description of the element</param>
        /// <param name="value">The value of the element</param>
        /// <param name="selected">Specifies, whether the list entry should be selected</param>
        /// <returns>The created object</returns>
        Option AddOption(string content, string value, bool selected);

        /// <summary>
        /// Add an option to this element.
        /// </summary>
        /// <param name="content">The description of the element</param>
        /// <param name="value">The value of the element</param>
        /// <param name="label">The label of the element</param>
        /// <returns>The created object</returns>
        Option AddOption(string content, string value, string label);

        /// <summary>
        /// Add an option to this element.
        /// </summary>
        /// <param name="content">The description of the element</param>
        /// <param name="value">The value of the element</param>
        /// <param name="label">The label of the element</param>
        /// <param name="selected">Specifies, whether the list entry should be selected</param>
        /// <returns>The created object</returns>
        Option AddOption(string content, string value, string label, bool selected);

    }

}
