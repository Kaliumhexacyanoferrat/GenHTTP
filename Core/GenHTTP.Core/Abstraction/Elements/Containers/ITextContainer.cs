using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines the methods which every element containing
    /// text elements has to provide.
    /// </summary>
    public interface ITextContainer
    {

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        void Print(string text);

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
        void Print(string text, bool escapeEntities);

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        /// <returns>The newly created object</returns>
        Text AddText(string text);

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
        /// <returns>The newly created object</returns>
        Text AddText(string text, bool escapeEntities);

        /// <summary>
        /// Add an empty text element to the element.
        /// </summary>
        /// <returns>The newly created object</returns>
        Text AddText();

    }

}
