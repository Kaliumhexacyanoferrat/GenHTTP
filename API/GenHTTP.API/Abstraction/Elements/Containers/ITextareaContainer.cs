using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Containers
{

    /// <summary>
    /// Defines methods which should be implemented by a container
    /// with textarea elements.
    /// </summary>
    public interface ITextareaContainer
    {

        /// <summary>
        /// Add a new textarea.
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="rows">The row count of the field</param>
        /// <param name="cols">The column count of the field</param>
        /// <returns>The created object</returns>
        Textarea AddTextarea(string name, ushort rows, ushort cols);

        /// <summary>
        /// Add a new textarea.
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="rows">The row count of the field</param>
        /// <param name="cols">The column count of the field</param>
        /// <param name="value">The value of the textarea</param>
        /// <returns>The created object</returns>
        Textarea AddTextarea(string name, ushort rows, ushort cols, string value);

    }

}
